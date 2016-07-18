using System;
using Android.Content;
using System.Collections.Generic;
using System.Linq;

namespace WCCMobile
{
    public class ScheduleManager
    {

        private readonly string ScheduleKey = "schedule_key";
        public object LastSchedule { get; internal set; }
        public static event EventHandler ScheduleChanged;
       // public static Func<object, object, object> ScheduleChanged { get; internal set; }
        private static ScheduleManager instance;
        Context context;
        ISharedPreferences prefs;
        public ScheduleManager(Context context)
        {
            this.context = context;

        }
        ISharedPreferences Preferences
        {
            get
            {
                return prefs ?? (prefs = context.GetSharedPreferences("preferences", FileCreationMode.Private));
            }
        }
        public static ScheduleManager Obtain(Context context)
        {
            return instance ?? (instance = new ScheduleManager(context));
        }

        internal void RemoveFromSchedule(int currentShownID)
        {
            throw new NotImplementedException();
        }

        public HashSet<int> GetScheduleItemIds()
        {
            var favs = Preferences.GetString(ScheduleKey, string.Empty);
            var result = new HashSet<int>(favs.Split(',')
                                           .Where(f => !string.IsNullOrEmpty(f))
                                           .Select(f => int.Parse(f)));
            LastSchedule = result;
            return result;
        }

        internal void AddToSchedule(int currentShownID)
        {
            throw new NotImplementedException();
        }

        internal object GetScheduleItemIDs()
        {
            throw new NotImplementedException();
        }
    }
}