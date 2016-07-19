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

namespace WCCMobile.Models
{
    public class TimeBlock
    {
        private char _day;
        private DateTime _start;
        private DateTime _end;
        private TimeSpan _length;

        public char Day
        {
            get
            {
                return _day;
            }
            set
            {
                _day = value;
            }
        }

        public DateTime Start
        {
            get
            {
                return _start;
            }
            set
            {
                _start = value;
            }
        }

        public DateTime End
        {
            get
            {
                return _end;
            }
            set
            {
                _end = value;
            }
        }

        public bool ConflictsWith(TimeBlock a)
        {
            if (_day != a.Day)
            {
                return false;
            }
            else
            {
                int comp = _start.CompareTo(a.Start);
                int comp2 = _start.CompareTo(a.End);
                if (comp != comp2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public TimeBlock(char day, String start, String end)
        {
            _day = day;
           // _start =// start;
           // _end =// MainWindow.StringToTime(end);
            long difference = _end.Ticks - _start.Ticks;
            _length = TimeSpan.FromTicks(difference);
        }

        public override string ToString()
        {
            return _start + " - " + _end;
        }
    }
}