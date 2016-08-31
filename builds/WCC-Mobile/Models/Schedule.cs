using System;
using System.ComponentModel;
using SQLite;

namespace WCCMobile.Models
{
    [Table("Schedules")]
    public class Schedule : INotifyPropertyChanged
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
        [NotNull, Indexed]
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

       
        public int CourseId { get; set; }
        
        public int TimesId { get; set; }
        [Ignore]
        public Times Times { get; set; }
        [Ignore]
        public Course Course { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this,
              new PropertyChangedEventArgs(propertyName));
        }

        public Schedule(int id, DayOfWeek day, Course course, Times times)
        {
            this.id = id;
            this.day = day;
            this.Course = course;
            this.Times = times;
        }
    }
}