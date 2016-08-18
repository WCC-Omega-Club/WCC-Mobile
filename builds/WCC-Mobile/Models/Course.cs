using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.ComponentModel;

namespace WCCMobile.Models
{
   [Table("Courses")]
   public class Course: INotifyPropertyChanged, IModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get { return id; }
            set
            {   id = value;
                OnPropertyChanged(nameof(id));
            }
        }
        private int id;
        [NotNull]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged(nameof(name));
            }
        }
        private string name;
        [NotNull]
        public string Major
        {
            get { return major; }
            set
            {
                major = value;
                OnPropertyChanged(nameof(major));
            }
        }
        private string major;
        [NotNull]
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged(nameof(description));
            }
        }
        private string description;
        [NotNull]
        public string Building
        {
            get { return building; }
            set
            {
                building = value;
                OnPropertyChanged(nameof(building));
            }
        }
        private string building;


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

        [ManyToOne]
        public Schedule schedule { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this,
              new PropertyChangedEventArgs(propertyName));
        }
    }
  

    
}