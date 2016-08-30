using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace WCCMobile.Adapters
{
    using Android.Util;
    using Models;
    using System.Reflection;

    public class ScheduleViewHolder : BasePopulateViewHolder<Schedule>
    {
        [InjectView(Resource.Id.roomName)]
        TextView roomName;
        [InjectView(Resource.Id.courseName)]
        TextView courseName;
        [InjectView(Resource.Id.professor)]
        TextView professor;
        [InjectView(Resource.Id.timeInterval)]
        TextView timeInterval;
        [InjectView(Resource.Id.calenderButton)]
        ImageButton calenderButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleViewHolder"/> class.
        /// </summary>
        /// <param name="itemView">The item view.</param>
        public ScheduleViewHolder(View itemView) : base(itemView)
        {
            Log.Debug("ScheduleViewHolder", MethodBase.GetCurrentMethod().Name + $" with args: View: {itemView})");

            Cheeseknife.Inject(this, itemView);
        }
        /// <summary>
        /// Populates overriding children of <see cref="T:WCCMobile.Adapters.BasePopulateViewHolder`1" /> with <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data.</param>
        public override void PopulateFrom(Schedule data)
        {
            Log.Debug("ScheduleViewHolder", MethodBase.GetCurrentMethod().Name + $" with args: Schedule.Course.Name: {data.Course.Name})");

            roomName.Text = data.Course.Room;
            courseName.Text = data.Course.Name;
            professor.Text = data.Course.Professor;
            timeInterval.Text = data.Times.StartTime.ToString() + " - " + data.Times.EndTime.ToString();
        }
    }

}