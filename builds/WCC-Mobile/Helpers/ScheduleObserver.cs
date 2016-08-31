using Android.Gms.Maps.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WCCMobile.Models;

namespace WCCMobile
{
  
        public class ScheduleObserver : IObservable<Schedule[]>
        {
            

            TimeSpan freshnessTimeout;
            string savedData;
            
            List<ScheduleSubscriber> subscribers = new List<ScheduleSubscriber>();

            public static readonly ScheduleObserver Instance = new ScheduleObserver();

            public ScheduleObserver() : this(TimeSpan.FromMinutes(5))
            {

            }

            public ScheduleObserver(TimeSpan freshnessTimeout)
            {
                this.freshnessTimeout = freshnessTimeout;
                //client.Timeout = TimeSpan.FromSeconds(30);
            }

            public DateTime LastUpdateTime
            {
                get;
                private set;
            }

            public Schedule[] LastScheduleItems
            {
                get;
                private set;
            }

            public static Schedule[] GetEventsAround(Schedule[] schedule, LatLng location, double minDistance = 100, int maxItems = 4)
            {
                var dic = new SortedDictionary<double, Schedule>();
                foreach (Schedule s in schedule)
                {
                    var d = LocationUtils.Distance(location, Course.GetLatLng(s.Course.Building));
                    if (d < minDistance)
                        dic.Add(d, s);
                }
                return dic.Select(ds => ds.Value).Take(maxItems).ToArray();
            }

            
            public bool HasCachedData
            {
                get
                {
                    return savedData != null && DateTime.Now < (LastUpdateTime + freshnessTimeout);
                }
            }

        public object LastScheduleItem { get; internal set; }

        int attempt = 0;
            public async Task<Schedule[]> GetEvents(bool forceRefresh = false, Action<string> dataCacher = null)
            {
                string data = null;

                if (forceRefresh)
                    attempt = 0;

                if (HasCachedData && !forceRefresh)
                    data = savedData;
                else
                {
                    attempt++;
                    while (data == null)
                    {
                        try
                        {
                            attempt = 0;
                        }
                        catch (Exception e)
                        {
                            
                            if (attempt >= 3)
                            {
                                attempt = 0;
                                return new Schedule[] { };
                            }
                        }
                        if (data == null)
                            await Task.Delay(500);
                    }
                }

                if (dataCacher != null)
                    dataCacher(data);


            //var eventss =  

            //LastUpdateTime = FromUnixTime(stationRoot.Timestamp);
            //LastScheduleItems = stations;

            /*if (subscribers.Any())
                foreach (var sub in subscribers)
                    sub.Observer.OnNext(stations);
            return stations;*/
            return new Schedule[] { };
            }

            DateTime FromUnixTime(long secs)
            {
                return (new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc) + TimeSpan.FromSeconds(secs / 1000.0)).ToLocalTime();
            }

            public IDisposable Subscribe(IObserver<Schedule[]> observer)
            {
                var sub = new ScheduleSubscriber(subscribers.Remove, observer);
                subscribers.Add(sub);
                return sub;
            }

            class ScheduleSubscriber : IDisposable
            {
                
                Func<ScheduleSubscriber, bool> unsubscribe;

                public ScheduleSubscriber(Func<ScheduleSubscriber, bool> unsubscribe, IObserver<Schedule[]> observer)
                {
                    Observer = observer;
                    this.unsubscribe = unsubscribe;
                }

                public IObserver<Schedule[]> Observer
                {
                    get;
                    set;
                }

                public void Dispose()
                {
                    unsubscribe(this);
                }
            }
        }
}