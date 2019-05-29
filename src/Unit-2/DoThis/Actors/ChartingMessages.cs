using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartApp.Actors
{
    #region Reporting
    /// <summary>
    /// Signal used to indicate that it's time to sample all counters
    /// </summary>
    public class GatherMetrics { }

    /// <summary>
    /// Metric data at the time of sample
    /// </summary>
    public class Metric
    {
        public Metric(string series, float counterValue)
        {
            Series = series;
            CounterValue = counterValue;
        }

        public string Series { get; private set; }
        public float CounterValue { get; private set; }
    }
    #endregion

    #region Performance Counter Management
    /// <summary>
    /// All types of counters supported by this example
    /// </summary>
    public enum CounterType
    {
        Cpu,
        Memory,
        Disk
    }

    /// <summary>
    /// Enables a counter and begins publishing values to <see cref="Subscriber"/>.
    /// </summary>
    public class SubscribeCounter
    {
        public SubscribeCounter(CounterType counterType, IActorRef subscriber)
        {
            CounterType = counterType;
            Subscriber = subscriber;
        }

        public CounterType CounterType { get; private set; }
        public IActorRef Subscriber { get; private set; }
    }

    /// <summary>
    /// Unsubscribes <see cref="Subscriber"/> from receiving updates 
    /// for a given counter
    /// </summary>
    public class UnsubscribeCounter
    {
        public UnsubscribeCounter(CounterType counterType, IActorRef subscriber)
        {
            CounterType = counterType;
            Subscriber = subscriber;
        }

        public CounterType CounterType { get; private set; }
        public IActorRef Subscriber { get; private set; }
    }
    #endregion
}
