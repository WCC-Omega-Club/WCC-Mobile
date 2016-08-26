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

using System.Collections.ObjectModel;
using WCCMobile.Models;
using WCCMobile;
using System.IO;

namespace WCCMobile.ORM
{
    

    public class ScheduleRepository
    {
        
        private SQLiteConnection database;
        private static object collisionLock = new object();
        private List<Schedule> schedules;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleRepository"/> class.
        /// </summary>
        public ScheduleRepository()
        {
            string dbName = "Schedule.db3";
            string path = Path.Combine(System.Environment.
              GetFolderPath(System.Environment.
              SpecialFolder.Personal), dbName);
            database = new SQLiteConnection(path);
            database.CreateTable<Course>();
            database.CreateTable<Schedule>();
            database.CreateTable<Times>();
            this.schedules = new List<Schedule>(database.Table<Schedule>());
            if (!database.Table<Course>().Any())
            {
                AddNewSchedule();
            }
        }

        
        public void AddNewSchedule()
        {
            this.schedules.
              Add(new Schedule
              {
                  Id = 1,
                  Course = new Course
                  {
                      Id = 1,
                      Name = "English 102",
                      Building = "TECH",
                      Description = "Learn real good english!"
                  },
                  Times = new Times
                  {   Id = 1,
                      StartTime = new TimeSpan(9, 30, 0),
                      EndTime = new TimeSpan(10, 20, 0)
                  }

              });
        }


        public List<Schedule> GetFilteredSchedule(string name)
        {
            lock (collisionLock)
            {
                var query = from schedule in database.Table<Schedule>()
                            where schedule.Course.Name == name
                            select schedule;
                return query.ToList();
            }
        }

        public List<Schedule> GetCurrentSchedule()
        {
            lock (collisionLock)
            {
                return database.Query<Schedule>(
                    "SELECT * FROM Schedules WHERE Day = " + DateTime.Now.DayOfWeek);
            }
        }

        public List<Schedule> GetFilteredSchedule()
        {
            lock (collisionLock)
            {
                return database.Query<Schedule>(
                  "SELECT * FROM Schedules WHERE Name = 'English 102'");
            }
        }

        public Schedule GetSchedule(int id)
        {
            lock (collisionLock)
            {
                return database.Table<Schedule>().
                  FirstOrDefault(Schedule => Schedule.Id == id);
            }
        }
        /// <summary>
        ///  Insert or Update a single instance of <see cref="Schedule"/> object depending on the existance of a customer class ID
        /// </summary>
        /// <param name="schedule">The schedule instance.</param>
        /// <returns></returns>
        public int SaveSchedule(Schedule schedule)
        {
            lock (collisionLock)
            {
                if (schedule.Id != 0)
                {
                    database.Update(schedule);
                    return schedule.Id;
                }
                else
                {
                    database.Insert(schedule);
                    return schedule.Id;
                }
            }
        }
        /// <summary>
        /// Saves all schedules.
        /// </summary>
        public void SaveAllSchedules()
        {
            lock (collisionLock)
            {
                foreach (Schedule schedule in schedules)
                {
                    if (schedule.Id != 0)
                    {
                        database.Update(schedule);
                    }
                    else
                    {
                        database.Insert(schedule);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the course.
        /// </summary>
        /// <param name="schedule">The course.</param>
        /// <returns></returns>
        public int DeleteSchedule(Schedule schedule)
        {
            int id = schedule.Id;
            if (id != 0)
            {
                lock (collisionLock)
                {
                    database.Delete<Schedule>(id);
                }
            }
            this.schedules.Remove(schedule);
            return id;
        }

        /// <summary>
        /// Deletes all courses.
        /// </summary>
        public void DeleteAllSchedules()
        {
            lock (collisionLock)
            {
                database.DropTable<Schedule>();
                database.DropTable<Course>();
                database.DropTable<Times>();
                database.CreateTable<Schedule>();
                database.CreateTable<Course>();
                database.CreateTable<Times>();
            }
            
            this.schedules = null;
            this.schedules = new List<Schedule>(database.Table<Schedule>());
        }
    }
}