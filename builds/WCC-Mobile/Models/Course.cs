using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.ComponentModel;
using Android.Gms.Maps.Model;

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
        [NotNull]
        public string Room
        {
            get { return room; }
            set
            {
                room = value;
                OnPropertyChanged(nameof(room));
            }
        }
        private string room;
        [NotNull]
        public string Professor
        {
            get { return professor; }
            set
            {
                professor = value;
                OnPropertyChanged(nameof(professor));
            }
        }
        private string professor;

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

        /// <summary>
        /// Gets the <see cref="LatLng"/> associated with <paramref name="Building"/>.
        /// </summary>
        /// <param name="Building">The building.</param>
        /// <returns></returns>
        public static LatLng GetLatLng(string Building)
        {
            switch (Building)
            {
                case "Gateway Center":
                    return new LatLng(41.068822, -73.787990);
                case "Technologies Building":
                    return new LatLng(41.067605, -73.788020);
                case "Student Center":
                    return new LatLng(41.065970, -73.788933);
                case "Bookstore - FSA":
                    return new LatLng(41.065339, -73.789212);
                case "Physical Education Building":
                    return new LatLng(41.065269, -73.786338);
                case "Classroom Building":
                    return new LatLng(41.066733, -73.790463);
                case "Harold L. Drummer Library":
                    return new LatLng(41.067380, -73.789406);
                case "Hartford Hall":
                    return new LatLng(41.068585, -73.790559);
                case "Health Science Building":
                    return new LatLng(41.068856, -73.791943);
                case "Administration Building":
                    return new LatLng(41.068407, -73.789432);
                case "Academic Arts Building":
                    return new LatLng(41.067477, -73.791481);
                default:
                    return null;
            }
        }
        /// <summary>
        /// Gets the <see cref="LatLng"/> associated with <paramref name="Building"/>.
        /// </summary>
        /// <param name="Building">The building.</param>
        /// <returns></returns>
        public static Location GetLocation(string Building)
        {
            switch (Building)
            {
                case "Gateway Center":
                    return new Location { Lat = 41.068822, Lon = -73.787990 };
                case "Technologies Building":
                    return new Location { Lat = 41.067605, Lon = -73.788020 };
                case "Student Center":
                    return new Location { Lat = 41.065970, Lon = -73.788933 };
                case "Bookstore - FSA":
                    return new Location { Lat = 41.065339, Lon = -73.789212 };
                case "Physical Education Building":
                    return new Location { Lat = 41.065269, Lon = -73.786338 };
                case "Classroom Building":
                    return new Location { Lat = 41.066733, Lon = -73.790463 };
                case "Harold L. Drummer Library":
                    return new Location { Lat = 41.067380, Lon = -73.789406 };
                case "Hartford Hall":
                    return new Location { Lat = 41.068585, Lon = -73.790559 };
                case "Health Science Building":
                    return new Location { Lat = 41.068856, Lon = -73.791943 };
                case "Administration Building":
                    return new Location { Lat = 41.068407, Lon = -73.789432 };
                case "Academic Arts Building":
                    return new Location { Lat = 41.067477, Lon = -73.791481 };
                default:
                    return new Location { Lat = 0, Lon = 0 };
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this,
              new PropertyChangedEventArgs(propertyName));
        }
    }
  

    
}