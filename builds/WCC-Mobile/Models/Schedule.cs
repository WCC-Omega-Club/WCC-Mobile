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
using SQLite;
using SQLiteNetExtensions;
using System.ComponentModel;
using SQLiteNetExtensions.Attributes;

namespace WCCMobile.Models
{
    [Table("Schedules")]
    public class Schedule : INotifyPropertyChanged, IModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged(nameof(id));
            }
        }
        private int id;
        [NotNull]
        public DayOfWeek Day
        {
            get { return day; }
            set
            {
                day = value;
                OnPropertyChanged(nameof(day));
            }
        }
        private DayOfWeek day;

        [OneToMany(CascadeOperations = CascadeOperation.CascadeDelete)]
        public List<Times> Times { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeDelete)]
        public List<Course> Courses { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this,
              new PropertyChangedEventArgs(propertyName));
        }
    }
}