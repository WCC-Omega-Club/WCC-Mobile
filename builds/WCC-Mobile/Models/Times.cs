using System;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.ComponentModel;

namespace WCCMobile.Models
{
    [Table("Times")]
    public class Times : INotifyPropertyChanged, IModel
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

        [ForeignKey(typeof(Schedule))]
        public int ScheduleId
        {
            get { return scheduleId; }
            set
            {
                scheduleId = value;
                OnPropertyChanged(nameof(scheduleId));
            }
        }
        private int scheduleId;

        [OneToOne]
        public Schedule schedule { get; set; }
        [NotNull]
        public TimeSpan StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                OnPropertyChanged(nameof(startTime));
            }
        }
        private TimeSpan startTime;
        [NotNull]
        public TimeSpan EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                OnPropertyChanged(nameof(endTime));
            }
        }
        private TimeSpan endTime;


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this,
              new PropertyChangedEventArgs(propertyName));
        }

    }
}