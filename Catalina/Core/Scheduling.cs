using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Catalina.Common;
using Catalina.Extensions;
using NodaTime;

namespace Catalina.Core;
public static class EventScheduler
{
#pragma warning disable IDE0044 // Add readonly modifier
    private static List<IEvent> _events = new List<IEvent>();
#pragma warning restore IDE0044 // Add readonly modifier

    public static void Start(ServiceProvider services)
    {
        var events = Assembly.GetExecutingAssembly().DefinedTypes
            .SelectMany(cl => cl.GetMethods(BindingFlags.Public | (BindingFlags.Public & BindingFlags.Static)))
            .Where(m => m.GetCustomAttribute<Invoke>() is not null);

        var repeatingEvents = Assembly.GetExecutingAssembly().DefinedTypes
            .SelectMany(cl => cl.GetMethods(BindingFlags.Public | (BindingFlags.Public & BindingFlags.Static)))
            .Where(m => m.GetCustomAttribute<InvokeRepeating>() is not null);
        


        foreach (var @event in repeatingEvents)
        {
            var attribute = @event.GetCustomAttribute<InvokeRepeating>();
            var repeatingEvent = new RepeatingEvent
            {
                Action = @event.CreateDelegate<Action>(),
                Interval = attribute.Interval,
            };
            if (attribute.AlignTo > AlignTo.Disabled)
            {
                var nextExecution = DateTime.UtcNow.RoundUp(TimeSpan.FromSeconds((ulong) attribute.AlignTo));
                repeatingEvent.NextExecution = nextExecution;
            }
            else
            {
                repeatingEvent.NextExecution = DateTime.UtcNow + attribute.Interval + attribute.Delay;
            }
            _events.Add(repeatingEvent);
        }
        foreach (var @event in events)
        {
            var attribute = @event.GetCustomAttribute<Invoke>();
            var scheduledEvent = new Event
            {
                Action = @event.CreateDelegate<Action>(),
            };
            if (attribute.AlignTo > AlignTo.Disabled)
            {
                var nextExecution = DateTime.UtcNow.RoundUp(TimeSpan.FromSeconds((ulong) (attribute.AlignTo)));
                scheduledEvent.NextExecution = nextExecution;
            }
            else
            {
                scheduledEvent.NextExecution = DateTime.UtcNow + attribute.Delay;
            }
            _events.Add(scheduledEvent);
        }

        new Thread(() =>
        {
            var utcNow = DateTime.UtcNow;
            var nearestMinute = DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(1));
            Thread.Sleep(nearestMinute - utcNow);
            Tick(services);
            while (true)
            {
                Tick(services);
                if (!_events.Where(ev => ev is RepeatingEvent).Any()) return;
                Thread.Sleep(_events.Where(ev => ev is RepeatingEvent).Select(e => (RepeatingEvent) e).Min(e => e.Interval));
            }
        }).Start();
    }

    public static void AddEvent(IEvent @event) 
    {
        if (_events.Any(e => e.Action == @event.Action))
        {
            _events.Add(@event);
        }
        else throw new Exceptions.DuplicateEntryException("the action provided is already scheduled.");
    }

    public static void RemoveEvent(IEvent @event)
    {
        if (_events.Any(e => e.Action == @event.Action))
        {
            _events.Remove(_events.First(e => e.Action == @event.Action));
        }
        else throw new Exceptions.InvalidArgumentException("the action provided does not exist.");
    }

    private static void Tick(ServiceProvider services)
    {
        _events.ForEach(e =>
        {
            if (DateTime.UtcNow >= e.NextExecution)
            {
                try
                {
                    e.Action.BeginInvoke(null, null);
                }
                catch (Exception ex)
                {
                    services.GetRequiredService<Logger>().Error(ex, ex.Message);
                    e.NextExecution = DateTime.UtcNow + TimeSpan.FromHours(1);
                }
                finally
                {
                    if (e is RepeatingEvent @event) e.NextExecution = DateTime.UtcNow + @event.Interval;
                    else { RemoveEvent(e); }
                }
            }
        });
    }

}

public interface IEvent 
{
    public Action Action { get; set; }
    public DateTime NextExecution { get; set; }
}
public struct RepeatingEvent : IEvent
{
    public TimeSpan Interval;
    public Action Action { get; set; }
    public DateTime NextExecution { get; set; }

    public RepeatingEvent(TimeSpan delay, TimeSpan interval, Action action)
    {
        Interval = interval; Action = action;
        NextExecution = (DateTime.UtcNow + interval + delay);
    }
    public RepeatingEvent(DateTime executionTime, TimeSpan interval, Action action)
    {
        Interval = interval; Action = action;
        NextExecution = executionTime;
    }
    public RepeatingEvent(TimeSpan interval, Action action)
    {
        Interval = interval; Action = action;
        NextExecution = DateTime.UtcNow + TimeSpan.FromMinutes(5) + interval;
    }
}
public struct Event : IEvent
{
    public DateTime NextExecution { get; set; }
    public Action Action { get; set; }

    public Event(TimeSpan delay, Action action)
    {
        Action = action;
        NextExecution = (DateTime.UtcNow + delay);
    }
    public Event(DateTime executionTime, Action action)
    {
        Action = action;
        NextExecution = executionTime;
    }
    public Event(Action action)
    {
        Action = action;
        NextExecution = DateTime.UtcNow + TimeSpan.FromMinutes(5);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class InvokeRepeating : Attribute
{
    public InvokeRepeating(int interval, int delay)
    {
        Interval = TimeSpan.FromSeconds(interval);
        Delay = TimeSpan.FromSeconds(delay);
        AlignTo = 0;
    }
    public InvokeRepeating(int interval, AlignTo alignTo)
    {
        Interval = TimeSpan.FromSeconds(interval);
        Delay = TimeSpan.FromMinutes(5);
        AlignTo = alignTo;
    }

    public AlignTo AlignTo;
    public TimeSpan Interval; 
    public TimeSpan Delay;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class Invoke : Attribute
{
    public Invoke(int delay)
    {
        Delay = TimeSpan.FromSeconds(delay);
        AlignTo = 0;
    }
    public Invoke(AlignTo alignTo)
    {
        Delay = TimeSpan.FromMinutes(5);
        AlignTo = alignTo;
    }

    public AlignTo AlignTo;
    public TimeSpan Delay;
}

public enum AlignTo : ulong
{
    Disabled = 0,

    OneMinute = 60,

    FifteenMinutes = OneMinute * 15,
    ThirtyMinutes = OneMinute * 30,

    OneHour = OneMinute * 60,

    TwoHours = OneHour * 2,
    FourHours = OneHour * 4,
    SixHours = OneHour * 6,
    EightHours = OneHour * 8,
    TenHours = OneHour * 10,
    TwelveHours = OneHour * 12,

    OneDay = OneHour * 24,

    TwoDays = OneDay * 2,
    OneWeek = OneDay * 7 
}