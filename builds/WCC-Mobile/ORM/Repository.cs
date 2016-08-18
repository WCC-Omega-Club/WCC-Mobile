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

namespace WCCMobile.ORM
{
    

    public class ScheduleRepository
    {
        
        private SQLiteConnection database;
        private static object collisionLock = new object();
        private IDatabaseConnection dbconnection;
        private List<Course> courses;
        private List<Schedule> schedules;
        private List<Times> times;
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleRepository"/> class.
        /// </summary>
        public ScheduleRepository()
        {
            database = dbconnection.DbConnection();
            database.CreateTable<Course>();
            database.CreateTable<Schedule>();
            database.CreateTable<Times>();
            this.courses = new List<Course>(database.Table<Course>());
            this.schedules = new List<Schedule>(database.Table<Schedule>());
            this.times = new List<Times>(database.Table<Times>());
            if (!database.Table<Course>().Any())
            {
                AddNewCourse();
            }
        }


        public void AddNewCourse()
        {
            this.courses.
              Add(new Course
              {
                  Name = "English 102",
                  Building = "TECH",
                  Description = "Learn real good english!"
              });
        }


        public IEnumerable<Course> GetFilteredCourses(string name)
        {
            lock (collisionLock)
            {
                var query = from course in database.Table<Course>()
                            where course.Name == name
                            select course;
                return query.AsEnumerable();
            }
        }

        public IEnumerable<Schedule> GetCurrentSchedule()
        {
            lock (collisionLock)
            {
                return database.Query<Schedule>(
                    "SELECT * FROM Schedules WHERE Day = " + DateTime.Now.DayOfWeek);
            }
        }

        public IEnumerable<Course> GetFilteredCourses()
        {
            lock (collisionLock)
            {
                return database.Query<Course>(
                  "SELECT * FROM Courses WHERE Name = 'English 102'").AsEnumerable();
            }
        }

        public Course GetCourse(int id)
        {
            lock (collisionLock)
            {
                return database.Table<Course>().
                  FirstOrDefault(Course => Course.Id == id);
            }
        }
        /// <summary>
        ///  Insert or Update a single instance of <see cref="Course"/> object depending on the existance of a customer class ID
        /// </summary>
        /// <param name="course">The course instance.</param>
        /// <returns></returns>
        public int SaveCourse(Course course)
        {
            lock (collisionLock)
            {
                if (course.Id != 0)
                {
                    database.Update(course);
                    return course.Id;
                }
                else
                {
                    database.Insert(course);
                    return course.Id;
                }
            }
        }
        /// <summary>
        /// Saves all courses.
        /// </summary>
        public void SaveAllCourses()
        {
            lock (collisionLock)
            {
                foreach (Course course in this.courses)
                {
                    if (course.Id != 0)
                    {
                        database.Update(course);
                    }
                    else
                    {
                        database.Insert(course);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the course.
        /// </summary>
        /// <param name="course">The course.</param>
        /// <returns></returns>
        public int DeleteCourse(Course course)
        {
            int id = course.Id;
            if (id != 0)
            {
                lock (collisionLock)
                {
                    database.Delete<Course>(id);
                }
            }
            this.courses.Remove(course);
            return id;
        }

        /// <summary>
        /// Deletes all courses.
        /// </summary>
        public void DeleteAllCourses()
        {
            lock (collisionLock)
            {
                database.DropTable<Course>();
                database.CreateTable<Course>();
            }
            this.courses = null;
            this.courses = new List<Course>(database.Table<Course>());
        }
    }
}