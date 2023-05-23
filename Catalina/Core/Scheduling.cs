﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Catalina.Common;
using Catalina.Extensions;
using System.Threading.Tasks;

namespace Catalina.Core;
public static class EventScheduler
{
#pragma warning disable IDE0044 // Add readonly modifier
    private static List<IEvent> _events = new List<IEvent>();
#pragma warning restore IDE0044 // Add readonly modifier

    public static void Setup(ServiceProvider services)
    {
        var events = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .SelectMany(y => y.GetMethods())
            .Where(m => m.GetCustomAttribute<Invoke>() is not null && m.IsPublic && m.IsStatic);
        var repeatingEvents = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .SelectMany(y => y.GetMethods())
            .Where(m => m.GetCustomAttribute<InvokeRepeating>() is not null && m.IsPublic && m.IsStatic);

        foreach (var @event in repeatingEvents)
        {
            var attribute = @event.GetCustomAttribute<InvokeRepeating>();
            var repeatingEvent = new RepeatingEvent
            {
                Method = @event,
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
            string fullMethodName = $"{repeatingEvent.Method.DeclaringType.Namespace}" +
                $".{repeatingEvent.Method.DeclaringType.Name}" +
                $".{repeatingEvent.Method.Name}";

            services.GetRequiredService<Logger>(
                ).Debug($"{fullMethodName} scheduled for " +
                $"{repeatingEvent.NextExecution.ToLocalTime():HH:mm:ss.f}" +
                $", repeat interval {repeatingEvent.Interval:c}");

            _events.Add(repeatingEvent);
        }
        foreach (var @event in events)
        {
            var attribute = @event.GetCustomAttribute<Invoke>();
            var scheduledEvent = new Event
            {
                Method = @event,
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
            string fullMethodName = $"{scheduledEvent.Method.DeclaringType.Namespace}" +
                $".{scheduledEvent.Method.DeclaringType.Name}" +
                $".{scheduledEvent.Method.Name}";
            services.GetRequiredService<Logger>(
                ).Debug($"{fullMethodName} scheduled for " +
                $"{scheduledEvent.NextExecution.ToLocalTime():HH:mm:ss.f}" +
                $", not scheduled to repeat");
            _events.Add(scheduledEvent);
        }

        Start(services);
    }
    private static void Start(ServiceProvider services)
        {
        new Task(async () =>
        {
            var utcNow = DateTime.UtcNow;
            var nearestMinute = DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(1));
            services.GetRequiredService<Logger>()
            .Debug($"Scheduler sleeping until {nearestMinute.ToLocalTime():HH:mm:ss.f}");
            await Task.Delay(nearestMinute - utcNow);
            Tick(services);
            while (true)
            {
                Tick(services);
                if (!_events.Where(ev => ev is RepeatingEvent).Any()) return;
                var nextExecution = _events.Min(e => e.NextExecution);
                services.GetRequiredService<Logger>()
                .Debug($"Next scheduler tick at {nextExecution.ToLocalTime():HH:mm:ss.f}");
                await Task.Delay(
                    (nextExecution - DateTime.UtcNow) > TimeSpan.FromSeconds(1) 
                    ? (nextExecution - DateTime.UtcNow) 
                    : TimeSpan.FromMinutes(1)
                    );
            }
        }).Start();
    }

    public static void AddEvent(IEvent @event) 
    {
        if (_events.Any(e => e.Method == @event.Method))
        {
            _events.Add(@event);
        }
        else throw new Exceptions
                .DuplicateEntryException("the action provided is already scheduled.");
    }

    public static void RemoveEvent(IEvent @event)
    {
        if (_events.Any(e => e.Method == @event.Method))
        {
            _events.Remove(_events.First(e => e.Method == @event.Method));
        }
        else throw new Exceptions
                .InvalidArgumentException("the action provided does not exist.");
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
    public MethodInfo Method { get; set; }
    public DateTime NextExecution { get; set; }
}
public struct RepeatingEvent : IEvent
{
    public TimeSpan Interval;
    public MethodInfo Method { get; set; }
    public DateTime NextExecution { get; set; }

    public RepeatingEvent(TimeSpan delay, TimeSpan interval, MethodInfo method)
    {
        Interval = interval; Method = method;
        NextExecution = (DateTime.UtcNow + interval + delay);
    }
    public RepeatingEvent(DateTime executionTime, TimeSpan interval, MethodInfo method)
    {
        Interval = interval; Method = method;
        NextExecution = executionTime;
    }
    public RepeatingEvent(TimeSpan interval, MethodInfo method)
    {
        Interval = interval; Method = method;
        NextExecution = DateTime.UtcNow + TimeSpan.FromMinutes(5) + interval;
    }
}
public struct Event : IEvent
{
    public DateTime NextExecution { get; set; }
    public MethodInfo Method { get; set; }

    public Event(TimeSpan delay, MethodInfo method)
    {
        Method = method;
        NextExecution = (DateTime.UtcNow + delay);
    }
    public Event(DateTime executionTime, MethodInfo method)
    {
        Method = method;
        NextExecution = executionTime;
    }
    public Event(MethodInfo method)
    {
        Method = method;
        NextExecution = DateTime.UtcNow + TimeSpan.FromMinutes(5);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class InvokeRepeating : Attribute
{
    public InvokeRepeating(Timings interval, Timings delay)
    {
        Interval = TimeSpan.FromSeconds((double) interval);
        Delay = TimeSpan.FromSeconds((double) delay);
        AlignTo = 0;
    }
    public InvokeRepeating(Timings interval, AlignTo alignTo)
    {
        Interval = TimeSpan.FromSeconds((double) interval);
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
    public Invoke(Timings delay)
    {
        Delay = TimeSpan.FromSeconds((double) delay);
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

public enum Timings : ulong
{
    OneMinute = 60,

    FiveMinutes = OneMinute * 5,
    TenMinutes = OneMinute * 10,
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
    OneWeek = OneDay * 7,

    TwoWeeks = OneWeek * 2
}

public enum AlignTo : ulong
{
    Disabled = 0,

    OneMinute = 60,

    FiveMinutes = OneMinute * 5,
    TenMinutes = OneMinute * 10,
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
    OneWeek = OneDay * 7,

    TwoWeeks = OneWeek * 2
}