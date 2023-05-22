using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Catalina.Common;
using Catalina.Extensions;

namespace Catalina.Core;

public struct Event
{
    public TimeSpan Interval;
    public DateTime NextExecution;
    public Action Action;

    public Event(TimeSpan delay, TimeSpan interval, Action action)
    {
        Interval = interval; Action = action;
        NextExecution = (DateTime.UtcNow + interval + delay);
    }
    public Event(DateTime executionTime, TimeSpan interval, Action action)
    {
        Interval = interval; Action = action;
        NextExecution = executionTime;
    }
    public Event(TimeSpan interval, Action action)
    {
        Interval = interval; Action = action;
        NextExecution = DateTime.UtcNow + TimeSpan.FromMinutes(5) + interval;
    }
}
public static class EventScheduler
{
#pragma warning disable IDE0044 // Add readonly modifier
    private static List<Event> _events = new List<Event>();
#pragma warning restore IDE0044 // Add readonly modifier

    public static void Start(ServiceProvider services)
    {
        var methods = Assembly.GetExecutingAssembly().DefinedTypes
            .SelectMany(cl => cl.GetMethods(BindingFlags.Public | (BindingFlags.Public & BindingFlags.Static)))
            .Where(m => m.GetCustomAttribute<ScheduledInvoke>() is not null);

        foreach (var method in methods)
        {
            var scheduledInvoke = method.GetCustomAttribute<ScheduledInvoke>();
            if (scheduledInvoke.HourAlign)
            {
                var nextHour = DateTime.UtcNow.RoundUp(TimeSpan.FromHours(1));
                AddEvent(new Event(
                    action: method.CreateDelegate<Action>(),
                    interval: scheduledInvoke.Interval,
                    executionTime: nextHour

                    ));
            }
            else
            {
                AddEvent(new Event(
                    action: method.CreateDelegate<Action>(),
                    interval: scheduledInvoke.Interval,
                    delay: scheduledInvoke.Delay
                    ));
            }
        }

        new Thread(() =>
        {
            var utcNow = DateTime.UtcNow;
            var nearestMinute = DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(1));
            Thread.Sleep(nearestMinute - utcNow);
            Tick(services);
            {
                Tick(services);
                Thread.Sleep(_events.Min(e => e.Interval));
            }
        }).Start();
    }

    public static void AddEvent(Event @event) 
    {
        if (!_events.Any(e => e.Action == @event.Action))
        {
            _events.Add(@event);
        }
        else throw new Exceptions.DuplicateEntryException("the action provided is already scheduled.");
    }

    public static void RemoveEvent(Event @event)
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
                }
                finally
                {
                    e.NextExecution = DateTime.UtcNow + e.Interval;
                }
            }
        });
    }

}
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ScheduledInvoke : Attribute
{
    public ScheduledInvoke(int interval, int delay)
    {
        Interval = TimeSpan.FromSeconds(interval);
        Delay = TimeSpan.FromSeconds(delay);
        HourAlign = false;
    }
    public ScheduledInvoke(int interval, bool hourAlign)
    {
        Interval = TimeSpan.FromSeconds(interval);
        Delay = TimeSpan.FromMinutes(5);
        HourAlign = hourAlign;
    }

    public bool HourAlign;
    public TimeSpan Interval;
    public TimeSpan Delay;
}