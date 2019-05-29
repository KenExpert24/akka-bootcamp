using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;

namespace ChartApp.Actors
{
    public class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        #region Message Types
        /// <summary>
        /// Subscribe the <see cref="ChartingActor"/> to 
        /// updates for <see cref="Counter"/>.
        /// </summary>
        public class Watch
        {
            public Watch(CounterType counterType)
            {
                CounterType = counterType;
            }

            public CounterType CounterType { get; private set; }
        }

        /// <summary>
        /// Unsubscribe the <see cref="ChartingActor"/> to 
        /// updates for <see cref="Counter"/>
        /// </summary>
        public class Unwatch
        {
            public Unwatch(CounterType counterType)
            {
                CounterType = counterType;
            }

            public CounterType CounterType { get; }
        }
        #endregion

        /// <summary>
        /// Methods for generating new instances of all <see cref="PerformanceCounter"/>s
        /// we want to monitor
        /// </summary>
        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators = new Dictionary<CounterType, Func<PerformanceCounter>>()
        {
            { CounterType.Cpu, () => new PerformanceCounter("Processor", "% Processor Time", "_Total", true) },
            { CounterType.Memory, () => new PerformanceCounter("Memory", "% Committed Bytes In Use", true) },
            { CounterType.Disk, () => new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true) }
        };

        /// <summary>
        /// Methods for creating new <see cref="Series"/> with distinct colors and names
        /// corresponding to each <see cref="PerformanceCounter"/>
        /// </summary>
        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries = new Dictionary<CounterType, Func<Series>>()
        {
            { CounterType.Cpu, () => new Series(CounterType.Cpu.ToString()) { ChartType = SeriesChartType.SplineArea, Color = Color.DarkGreen } },
            { CounterType.Memory, () => new Series(CounterType.Memory.ToString()) { ChartType = SeriesChartType.FastLine, Color = Color.MediumBlue } },
            { CounterType.Disk, () => new Series(CounterType.Disk.ToString()) { ChartType = SeriesChartType.SplineArea, Color = Color.DarkRed } }
        };

        private Dictionary<CounterType, IActorRef> _counterActors;
        private IActorRef _chartingActor;

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor) : this(chartingActor, new Dictionary<CounterType, IActorRef>())
        {

        }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> counterActors)
        {
            _chartingActor = chartingActor;
            _counterActors = counterActors;

            Receive<Watch>(watch =>
            {
                if (!_counterActors.ContainsKey(watch.CounterType))
                {
                    // create a child actor to monitor this counter if
                    // one doesn't exist already
                    var counterActor = Context.ActorOf(Props.Create(() => new PerformanceCounterActor(watch.CounterType.ToString(), CounterGenerators[watch.CounterType])));

                    // add this counter actor to our index
                    _counterActors[watch.CounterType] = counterActor;
                }

                // register this series with the ChartingActor
                _chartingActor.Tell(new ChartingActor.AddSeries(CounterSeries[watch.CounterType]()));

                // tell the counter actor to begin publishing its
                // statistics to the _chartingActor
                _counterActors[watch.CounterType].Tell(new SubscribeCounter(watch.CounterType, _chartingActor));
            });

            Receive<Unwatch>(unwatch =>
            {
                if (!_counterActors.ContainsKey(unwatch.CounterType))
                {
                    return; // noop
                }

                // unsubscribe the ChartingActor from receiving any more updates
                _counterActors[unwatch.CounterType].Tell(new UnsubscribeCounter(unwatch.CounterType, _chartingActor));

                // remove this series from the ChartingActor
                _chartingActor.Tell(new ChartingActor.RemoveSeries(unwatch.CounterType.ToString()));
            });
        }
    }
}
