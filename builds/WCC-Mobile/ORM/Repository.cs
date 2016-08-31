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
    
    /*
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
            schedules = new List<Schedule>();
        }

        public void CreateDatabase()
        {
            try
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
                if (!database.Table<Schedule>().Any())
                {
                    AddNewSchedule();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AddNewSchedule()
        {
            
            this.schedules.
              Add(new Schedule
              {
                  Id = 1,
                  CourseId = 1,
                  TimesId = 1

              });
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
                  FirstOrDefault(Schedule => Schedule.TimesId == id);
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
                if (schedule.TimesId != 0 && schedule.CourseId != 0 && schedule.TimesId != 0)
                {
                    database.Update(schedule);
                    database.Update(schedule.Course);//TODO: fix multple update calls which table with foreign keys
                    database.Update(schedule.Times);
                    return schedule.TimesId;
                }
                else
                {
                    database.Insert(schedule);
                    database.Insert(schedule.Course);//TODO: fix multple update calls which table with foreign keys
                    database.Insert(schedule.Times);
                    return schedule.TimesId;
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
                    if (schedule.TimesId != 0 && schedule.CourseId != 0 && schedule.TimesId != 0)
                    {
                        database.Update(schedule);
                        database.Update(schedule.Course);//TODO: fix multple update calls which table with foreign keys
                        database.Update(schedule.Times);
                    }
                    else
                    {
                        database.Insert(schedule);
                        database.Insert(schedule.Course);//TODO: fix multple update calls which table with foreign keys
                        database.Insert(schedule.Times);
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
            int id = schedule.TimesId;
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
    }*/
}