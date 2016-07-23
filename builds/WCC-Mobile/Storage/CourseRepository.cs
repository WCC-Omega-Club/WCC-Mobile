using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WCCMobile
{
    /// <summary>
    /// Course Repository is a static class that handles all the course information.
    /// <br/>The id parameter that should be passed for the static methods should be the hashcode of the string "[major]:[id]", e.g. "ACC:117".GetHashCode()
    /// <br/>The Course's UID field should be used with the id parameter.
    /// </summary>
    public static class CourseRepository
    {
        [JsonProperty("Courses")]
        static Dictionary<int,Course> courses = new Dictionary<int,Course>();
        /// <summary>
        /// The Dictionary of Courses.
        /// </summary>
        static Dictionary<int, Course> COURSES
        {
            get { return courses; }
        }

        /// <summary>
        /// Loads the Courses.
        /// </summary>
        public static void LoadRepository()
        {

            if (System.IO.File.Exists(@"/sdcard/Courses.json"))
            {
                try
                {
                    courses = JsonConvert.DeserializeObject<Dictionary<int,Course>>(System.IO.File.ReadAllText(@"/sdcard/Courses.json"));
                }
                catch(Exception)
                {

                }
            }
        }

        /// <summary>
        /// Saves the File for the Courses.
        /// </summary>
        public static void SaveRepository()
        {
            System.IO.File.WriteAllText(@"/sdcard/Courses.json",JsonConvert.SerializeObject(courses));
        }

        /// <summary>
        /// Deletes the Course based on the id.
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteCourse(int id)
        {
            courses.Remove(id);
        }

        /// <summary>
        /// Retreives the Course based on the id
        /// </summary>
        /// <param name="id">The id that will be checked</param>
        /// <returns></returns>
        public static Course GetCourse(int id)
        {
            return courses[id];
        }

        /// <summary>
        /// </summary>
        /// <returns>A List of Courses</returns>
        public static List<Course> GetCourseList()
        {
            if (courses.Count == 0)
                return null;
            List<Course> courseList = new List<Course>();
            foreach(Course course in courses.Values)
            {
                courseList.Add(course);
            }
            return courseList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string array of the Course's ID</returns>
        public static string[] GetCourseIDArray()
        {
            if (courses.Count == 0)
                return null;
            string[] ids = new string[courses.Count];
            int i = 0;
            foreach(Course course in courses.Values)
            {
                ids[i] = course.MAJOR + ":" + course.ID;
                i++;
            }
            return ids;
        }

        /// <summary>
        /// Checks if the Course exists in the Dictionary based on the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true if the Course exists, else false</returns>
        public static bool Contains(int id)
        {
            return courses.ContainsKey(id);
        }

        /// <summary>
        /// Inserts the Course into the Dictionary.
        /// </summary>
        /// <param name="c"></param>
        public static void InsertCourse(Course c)
        {
            courses.Add(c.UID,c);
        }
    }
}