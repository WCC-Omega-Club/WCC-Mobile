using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WCCMobile.Models;

namespace WCCMobile
{
  
        public class ScheduleObserver : IObservable<ScheduleItem[]>
        {
            const string ProntoApiEndpoint = "http://bikenowapp.azurewebsites.net/cron/latest_stations.json";

            public static readonly Func<ScheduleItem, bool> AvailableBikeStationPredicate = s => s.BikeCount > 1 && s.EmptySlotCount > 1;

           // HttpClient client = new HttpClient();
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

            public ScheduleItem[] LastScheduleItems
            {
                get;
                private set;
            }

            public static ScheduleItem[] GetStationsAround(ScheduleItem[] stations, Location location, double minDistance = 100, int maxItems = 4)
            {
                var dic = new SortedDictionary<double, ScheduleItem>();
                foreach (var s in stations)
                {
                    var d = LocationUtils.Distance(location, s.Location);
                    if (d < minDistance)
                        dic.Add(d, s);
                }
                return dic.Select(ds => ds.Value).Take(maxItems).ToArray();
            }

            /*public ScheduleItem[] GetClosestStationTo(ScheduleItem[] events, params Location[] locations)
            {
                return GetClosestStationTo(events, null, locations);
            }

            public ScheduleItem[] GetClosestStationTo(ScheduleItem[] buildings, Func<Location, bool> filter, params Location[] locations)
            {
                var distanceToGeoPoints = new SortedDictionary<double, Location>[locations.Length];
                var ss = filter == null ? buildings : buildings.Where(filter);
                foreach (var station in ss)
                {
                    for (int i = 0; i < locations.Length; i++)
                    {
                        if (distanceToGeoPoints[i] == null)
                            distanceToGeoPoints[i] = new SortedDictionary<double, Location>();
                        distanceToGeoPoints[i].Add(LocationUtils.Distance(locations[i], station.Location), station);
                    }
                }

                return distanceToGeoPoints.Select(ds => ds.First().Value).ToArray();
            }*/

            public bool HasCachedData
            {
                get
                {
                    return savedData != null && DateTime.Now < (LastUpdateTime + freshnessTimeout);
                }
            }

        public object LastScheduleItem { get; internal set; }

        int attempt = 0;
            public async Task<ScheduleItem[]> GetStations(bool forceRefresh = false, Action<string> dataCacher = null)
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
                            //data = await client.GetStringAsync(ProntoApiEndpoint).ConfigureAwait(false);
                            attempt = 0;
                        }
                        catch (Exception e)
                        {
                            //Android.Util.Log.Error("ProntoDownloader", e.ToString());
                            //Xamarin.Insights.Report(e);
                            if (attempt >= 3)
                            {
                                attempt = 0;
                                return new ScheduleItem[] { };
                            }
                        }
                        if (data == null)
                            await Task.Delay(500);
                    }
                }

                if (dataCacher != null)
                    dataCacher(data);

                var stationRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<ScheduleRootObject>(data);

                var stations = stationRoot.Schedule.ToArray();

                LastUpdateTime = FromUnixTime(stationRoot.Timestamp);
                LastScheduleItems = stations;

                if (subscribers.Any())
                    foreach (var sub in subscribers)
                        sub.Observer.OnNext(stations);
                return stations;
            }

            DateTime FromUnixTime(long secs)
            {
                return (new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc) + TimeSpan.FromSeconds(secs / 1000.0)).ToLocalTime();
            }

            public IDisposable Subscribe(IObserver<ScheduleItem[]> observer)
            {
                var sub = new ScheduleSubscriber(subscribers.Remove, observer);
                subscribers.Add(sub);
                return sub;
            }

            class ScheduleSubscriber : IDisposable
            {
                Func<ScheduleSubscriber, bool> unsubscribe;

                public ScheduleSubscriber(Func<ScheduleSubscriber, bool> unsubscribe, IObserver<ScheduleItem[]> observer)
                {
                    Observer = observer;
                    this.unsubscribe = unsubscribe;
                }

                public IObserver<ScheduleItem[]> Observer
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