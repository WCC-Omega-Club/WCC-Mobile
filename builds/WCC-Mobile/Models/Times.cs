using SQLite;
using System;
using System.ComponentModel;


namespace WCCMobile.Models
{
    [Table("Times")]
    public class Times : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int TimesId
        {
            get { return timesId; }
            set
            {
                timesId = value;
                OnPropertyChanged(nameof(timesId));
            }
        }
        private int timesId;

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
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Times(int timesId, TimeSpan startTime, TimeSpan endTime)
        {
            this.timesId = timesId;
            this.startTime = startTime;
            this.endTime = endTime;
        }

    }
}