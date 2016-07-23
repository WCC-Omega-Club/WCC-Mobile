using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WCCMobile
{
    /// <summary>
    /// This class represents a Course.
    /// </summary>
    public class Course
    {
        [JsonProperty("DayOfWeek")]
        Dictionary<string, CourseDay> DayOfWeek;
        /// <summary>
        /// Dictionary of the days for the Course.  The Keys represents a the Day of the Week, Values are a CourseDay.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, CourseDay> DAY_OF_WEEK
        {
            get { return DayOfWeek; }
        }

        [JsonProperty("Unique ID")]
        int uid;
        /// <summary>
        /// The Unique Identifier for the Course. This is represented by the int value produced from "[major]:[id]".GetHashCode()
        /// </summary>
        [JsonIgnore]
        public int UID
        {
            get { return uid; }
        }

        [JsonProperty("Name")]
        string name;
        [JsonIgnore]
        public string NAME
        {
            get { return name; }
        }

        [JsonProperty("ID")]
        string id;
        /// <summary>
        /// The Course ID of the Course, e.g. 100, 101, 201
        /// </summary>
        [JsonIgnore]
        public string ID
        {
            get { return id; }
        }

        [JsonProperty("Major")]
        string major;
        [JsonIgnore]
        public string MAJOR
        {
            get { return major; }
        }

        [JsonProperty("Room")]
        string room;
        [JsonIgnore]
        public string ROOM
        {
            get { return room; }
        }

        [JsonProperty("Location")]
        string location;
        [JsonIgnore]
        public string LOCATION
        {
            get { return location; }
        }

        /// <summary>
        /// Default Constructor for Json Deserialization.
        /// </summary>
        public Course()
        {

        }
        /// <summary>
        /// Constructor for creating a Course
        /// </summary>
        /// <param name="major">The Major that the Course is from, e.g. COMSC, MATH, PHYSC</param>
        /// <param name="id">The Course ID that the Course should be, e.g. 100, 101, 201</param>
        /// <param name="name">The name of the Course</param>
        /// <param name="room">The room that the Course is in</param>
        /// <param name="location">The building location that the Course is in</param>
        public Course(string major, string id, string name, string room, string location)
        {
            this.major = major;
            this.id = id;
            uid = (major + ":" + id).GetHashCode();
            Update(name, room, location);
            DayOfWeek = new Dictionary<string, CourseDay>();
        }

        /// <summary>
        /// Converts the string of a day to fit the proper format that the DAY_OF_WEEK Dictionary use.
        /// <br/>The format has the first letter capitalized and every other letter lowercased.
        /// </summary>
        /// <param name="day">The day that will be converted</param>
        /// <returns>The properly formatted string for the day</returns>
        private static string ConvertDay(string day)
        {
            day.ToLower();
            day.Replace(day[0],day[0].ToString().ToUpper()[0]);
            return day;
        }

        /// <summary>
        /// Checks if the DAY_OF_WEEK Dictionary contains that specified day.
        /// </summary>
        /// <param name="day">The day that will be checked</param>
        /// <returns>true if the day exists in the Dictionary, else false</returns>
        public bool ContainsDay(string day)
        {
            
            return DayOfWeek.ContainsKey(ConvertDay(day));
        }

        /// <summary>
        /// Returns the CourseDay from the DAY_OF_WEEK Dictionary
        /// </summary>
        /// <param name="day">The day that will be returned.</param>
        /// <returns></returns>
        public CourseDay GetDay(string day)
        {
            return DayOfWeek[ConvertDay(day)];
        }

        /// <summary>
        /// Inserts a day into the DAY_OF_WEEK Dictionary.
        /// </summary>
        /// <param name="courseDay">The CourseDay that will be inserted.</param>
        public void InsertDay(CourseDay courseDay)
        {
            DayOfWeek.Add(courseDay.DAY, courseDay);
        }

        /// <summary>
        /// Deletes a day from the DAY_OF_WEEK Dictionary.
        /// </summary>
        /// <param name="day">The day to delete.</param>
        public void DeleteDay(string day)
        {
            DayOfWeek.Remove(ConvertDay(day));
        }

        /// <summary>
        /// Updates the name, room, and location of the Course
        /// </summary>
        /// <param name="name">The new name for the Course</param>
        /// <param name="room">The new room for the Course</param>
        /// <param name="location">The new location for the Course</param>
        public void Update(string name, string room, string location)
        {
            this.name = name;
            this.room = room;
            this.location = location;
        }

        public override bool Equals(object obj)
        {
            return (obj is Course) ? this == (Course)obj : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The Unique Identifier</returns>
        public override int GetHashCode()
        {
            return uid;
        }


        /// <summary>
        /// This Class represents a Day of the Week and Time Schedule for a Course
        /// </summary>
        public class CourseDay
        {
            [JsonProperty("Day")]
            string day;
            [JsonIgnore]
            public string DAY
            {
                get { return day; }
            }

            [JsonProperty("StartTime")]
            TimeSpan startTime;
            [JsonIgnore]
            public TimeSpan START_TIME
            {
                get { return startTime; }
            }

            [JsonProperty("EndTime")]
            TimeSpan endTime;
            [JsonIgnore]
            public TimeSpan END_TIME
            {
                get { return endTime; }
            }

            /// <summary>
            /// Default Constructor for Json Deserialization.
            /// </summary>
            public CourseDay()
            {

            }

            /// <summary>
            /// Constructor for creating a CourseDay
            /// </summary>
            /// <param name="day">One of the days of the week that this will represent.</param>
            /// <param name="startTimeStamp">The starting time.</param>
            /// <param name="startMer">The meridian for the starting time.</param>
            /// <param name="endTimeStamp">The ending time.</param>
            /// <param name="endMer">The meridian for the ending time.</param>
            public CourseDay(string day, string startTimeStamp, string startMer, string endTimeStamp, string endMer)
            {
                this.day = ConvertDay(day);
                UpdateTime(startTimeStamp, startMer, endTimeStamp, endMer);
            }

            /// <summary>
            /// Updates the time.
            /// </summary>
            /// <param name="startTimeStamp">The starting time.</param>
            /// <param name="startMer">The meridian for the starting time.</param>
            /// <param name="endTimeStamp">The ending time.</param>
            /// <param name="endMer">The meridian for the ending time.</param>
            public void UpdateTime(string startTimeStamp, string startMer, string endTimeStamp, string endMer)
            {
                //12 hr to 24 hr conversion
                int hours = int.Parse(startTimeStamp.Substring(0, startTimeStamp.IndexOf(":")));
                int minutes = int.Parse(startTimeStamp.Substring(startTimeStamp.IndexOf(":") + 1));
                if (startMer == "PM")
                    hours = (hours + 12) % 24;
                startTime = new TimeSpan(hours, minutes, 0);
                System.Console.WriteLine(startTime);
                hours = int.Parse(endTimeStamp.Substring(0, endTimeStamp.IndexOf(":")));
                minutes = int.Parse(endTimeStamp.Substring(endTimeStamp.IndexOf(":") + 1));
                if (endMer == "PM")
                    hours = (hours + 12) % 24;
                endTime = new TimeSpan(hours, minutes, 0);
                //
            }
        }
    }
}