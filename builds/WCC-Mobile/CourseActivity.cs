using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Java.IO;
using Java.Util;
using System.Text.RegularExpressions;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
/*
 * This Class is the CourseActivity. It handles managing the courses for a Student.
 * Files related to the CourseActivity:
 *      Resources/
 *          layout/CourseLayout.axml - The Layout that the CourseActivty will use.
 *          values/
 *              days.xml - Contains a list of Days.
 *              locations.xml - Contains a list of Locations.
 *              meridian.xml - Contains AM and PM as a list.
 *              
 *      Assets/BasicInfoFiles/Courses.txt - the Courses that will be parse into Dictionaries and Lists.
 */
namespace WCCMobile
{
    [Activity(Label = "CourseActivity", MainLauncher = false, ParentActivity = typeof(WCCMobile.MainActivity))]
    public class CourseActivity : Activity, AdapterView.IOnItemSelectedListener, View.IOnClickListener
    {
        static CourseActivity singleton = null;
        public static CourseActivity singleR
        {
            get { return singleton; }
        }
        Dictionary<string, List<string>> majorNames; //Dictionary of the Names, Keys are the major, Values are a list of Names that the major holds (Placement order matches majorIDs)
        Dictionary<string, List<string>> majorIDs; //Dictionary of the IDs, Keys are the major, Values are a list of IDs that the major holds (Placement order matches majorNames)
        List<string> majors; //A list of all the available Majors.
        Dictionary<string,Course> courses; //Dictionary of the Courses. Keys are represented as [Major]:[ID] (Colon included), Values are the courses.
        bool inClickEvent; //Button Click Spam Prevention
        string currentCourse; //The current course that the Course Manager is on. Represented as [Major]:[ID] (Colon included), and used to retrieve a Course from the courses Dictionary.
        string previousMajor; //The previous major that the Course Manager had.

        Button saveCourseBtn;
        Button openCourseBtn;
        Button delCourseBtn;
        Button saveDayBtn;
        Button openDayBtn;
        Button delDayBtn;

        Spinner courseSpinner;
        Spinner idSpinner;
        int idSpinnerIndex;
        TextView courseName;
        Toast toast;
        
        public Toast TOAST
        {
            get { return toast; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            singleton = this;
            SetContentView(Resource.Layout.CourseLayout);

            majorNames = new Dictionary<string, List<string>>();
            majorIDs = new Dictionary<string, List<string>>();
            majors = new List<string>();
            courses = new Dictionary<string,Course>();
            toast = Toast.MakeText(this,"",ToastLength.Short);

            OpenCoursesFile();
            
            BufferedReader reader = null;
            try
            {
                //Reads the Courses.txt from the Course Parser and sets up the Dictionaries and Lists prior to the values.
                reader = new BufferedReader(
                    new InputStreamReader(MainActivity.singleR.Assets.Open("BasicInfoFiles/Courses.txt")));

                string lastMajor=null;
                string major;
                string id;
                string name;
                string line = null;
                StringTokenizer lineTkns = null;
                bool firstLine=true;
                while ((line = reader.ReadLine()) != null)
                {
                    lineTkns = new StringTokenizer(line, "::");
                    major = lineTkns.NextToken();
                    if (firstLine)
                    {
                        major = major.Remove(0, 1);
                        firstLine = false;
                    }
                    id = lineTkns.NextToken();
                    name = lineTkns.NextToken();
                    
                    if (!major.Equals(lastMajor))
                    {
                        lastMajor = major;
                        if (!majors.Contains(major))
                        {
                            majors.Add(major);
                            majorIDs.Add(major, new List<string>());
                            majorNames.Add(major, new List<string>());
                        }
                    }
                    majorIDs[major].Add(id);
                    majorNames[major].Add(name);
                }

                majors.Sort();
                majors.Insert(0, "None");
            }
            catch (Java.IO.IOException)
            {
                //log the exception
            }
            finally
            {
                if (reader != null)
                {
                    try
                    {
                        reader.Close();
                    }
                    catch (Java.IO.IOException)
                    {
                        //log the exception
                    }
                }
            }

            saveCourseBtn = FindViewById<Button>(Resource.Id.saveCourse);
            saveCourseBtn.SetOnClickListener(this);
            openCourseBtn = FindViewById<Button>(Resource.Id.openCourse);
            openCourseBtn.SetOnClickListener(this);
            delCourseBtn = FindViewById<Button>(Resource.Id.delCourse);
            delCourseBtn.SetOnClickListener(this);

            saveDayBtn = FindViewById<Button>(Resource.Id.saveDay);
            saveDayBtn.SetOnClickListener(this);
            openDayBtn = FindViewById<Button>(Resource.Id.openDay);
            openDayBtn.SetOnClickListener(this);
            delDayBtn = FindViewById<Button>(Resource.Id.delDay);
            delDayBtn.SetOnClickListener(this);

            // Create your application here
            courseName = FindViewById<TextView>(Resource.Id.courseName);

            idSpinner = FindViewById<Spinner>(Resource.Id.idSpinner);
            courseSpinner = FindViewById<Spinner>(Resource.Id.courseSpinner);
            ArrayAdapter<String> courseAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, majors);
            courseAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            courseSpinner.Adapter = courseAdapter;
            courseSpinner.OnItemSelectedListener=this;

            Spinner daySpinner = FindViewById<Spinner>(Resource.Id.daySpinner);
            var dayAdapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.days_array, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            dayAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            daySpinner.Adapter = dayAdapter;

            Spinner bldgSpinner = FindViewById<Spinner>(Resource.Id.bldgSpinner);
            var bldgAdapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.locations_array, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            bldgAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            bldgSpinner.Adapter = bldgAdapter;

            var merAdapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.meridian_array, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            merAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            Spinner merSpinner1 = FindViewById<Spinner>(Resource.Id.merSpinner1);
            merSpinner1.Adapter = merAdapter;

            Spinner merSpinner2 = FindViewById<Spinner>(Resource.Id.merSpinner2);
            merSpinner2.Adapter = merAdapter;
        }

        

        /*
         * Opens the File for the Courses
         * Change this code to a better method if there is. This was placed just to have a working save/load method
         * If this code is changed also change the saveCourses for Compatibility
         */
        public void OpenCoursesFile()
        {
            
            if (System.IO.File.Exists("/sdcard/WCC Courses.bin"))
            {
                using (Stream stream = System.IO.File.Open("/sdcard/WCC Courses.bin", FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    courses = (Dictionary<string, Course>)bformatter.Deserialize(stream);
                }
            }
        }

        /*
        * Saves the File for the Courses
        * Change this code to a better method if there is. This was placed just to have a working save/load method
        * If this code is changed also change the openCourses for Compatibility
        */
        public void SaveCoursesFile()
        {
            using (Stream stream = System.IO.File.Open("/sdcard/WCC Courses.bin", FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, courses);
            }
        }


        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            if (parent.Id.Equals(courseSpinner.Id))
            {
                string currentMajor = (string)parent.GetItemAtPosition(position);
                if (previousMajor!=currentMajor)
                {
                    //Course Spinner Item Handler
                    previousMajor = currentMajor;
                    if (currentMajor != "None")
                    {
                        setIDSpinner(currentMajor);
                    }
                    else
                    {
                        //When "None" is selected.
                        idSpinner.Adapter = null;
                        idSpinner.Enabled = false;
                        currentCourse = null;
                        courseName.Text = "None";
                    }
                }
            }
            else if (parent.Id.Equals(idSpinner.Id))
            {
                //ID Spinner Item Handler
                string currentMajor = (string)courseSpinner.GetItemAtPosition(courseSpinner.LastVisiblePosition);
                string currentID = (string)parent.GetItemAtPosition(position);
                courseName.Text = majorNames[currentMajor][majorIDs[currentMajor].IndexOf(currentID)]; //Changes the Course Display Name
                currentCourse = currentMajor + ":" + currentID; //Sets the current course
            }
        }

        void setIDSpinner(string major)
        {
            //Sets up the id Spinner for the selected major
            ArrayAdapter<string> idAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, majorIDs[major]);
            idAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            idSpinner.OnItemSelectedListener = this;
            idSpinner.Adapter = idAdapter;
            idSpinner.Enabled = true;
            idSpinner.SetSelection(idSpinnerIndex);
            idSpinnerIndex = 0;
        }

        public void OnNothingSelected(AdapterView parent)
        {
        }

        
        /*
         * Checks the validity of a course.
         * Returns true if a Course can be saved without an issue
         * Else return false.
         */
        public Boolean IsValidCourse()
        {
            Spinner courseSpinner = FindViewById<Spinner>(Resource.Id.courseSpinner);
            if (((string)courseSpinner.GetItemAtPosition(courseSpinner.LastVisiblePosition)).Equals("None"))
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return false;
            }
            EditText room = FindViewById<EditText>(Resource.Id.roomText);
            if(room.Text.Length==0)
            {
                toast.SetText("Enter a Room Number"); //Error Message
                toast.Show();
                return false;
            }
            return Course.IsValidDay();
        }

        //Handles saving the current course that the manager has setup.
        public void SaveCourse()
        {
            if (!IsValidCourse()) return;

            string room = FindViewById<EditText>(Resource.Id.roomText).Text;
            Spinner bldgSpinner = FindViewById<Spinner>(Resource.Id.bldgSpinner);
            string bldg = (string)bldgSpinner.GetItemAtPosition(bldgSpinner.LastVisiblePosition);
            string courseName = FindViewById<TextView>(Resource.Id.courseName).Text;
            Course course;
            if (courses.ContainsKey(currentCourse))
            {
                
                course = courses[currentCourse];
                course.Update(courseName, room, bldg);
                toast.SetText("Course: " + courses[currentCourse].NAME + " has been updated");
                
            }
            else
            {
                Spinner courseSpinner = FindViewById<Spinner>(Resource.Id.courseSpinner);
                string major = (string)courseSpinner.GetItemAtPosition(courseSpinner.LastVisiblePosition);
                Spinner idSpinner = FindViewById<Spinner>(Resource.Id.idSpinner);
                string courseID = (string)idSpinner.GetItemAtPosition(idSpinner.LastVisiblePosition);
                course = new Course(major, courseID, courseName, room, bldg);
                courses.Add(major + ":" + courseID, course);
                
                toast.SetText("Course: " + courses[currentCourse].NAME + " has been saved");
                
            }

            course.saveDay();
            toast.Show();

            SaveCoursesFile();
        }

        public void OpenCourse()
        {
            if(courses.Count==0)
            {
                toast.SetText("Cannot Find a Course"); //Error Message
                toast.Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Open a Course");
            alert.SetItems(courses.Keys.ToArray(), openCourseAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        void openCourseAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            currentCourse = courses.Keys.ToArray()[e.Which];
            Course course = courses[currentCourse];
            courseSpinner.SetSelection(majors.IndexOf(course.MAJOR));

            idSpinnerIndex = majorIDs[course.MAJOR].IndexOf(course.ID);

            Spinner bldgSpinner = FindViewById<Spinner>(Resource.Id.bldgSpinner);
            ArrayAdapter adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.locations_array, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            bldgSpinner.SetSelection(adapter.GetPosition(course.LOCATION));
            courseName.Text = course.NAME;
            course.openDay(course.DAYS.Keys.ToArray()[0]);
            FindViewById<EditText>(Resource.Id.roomText).Text = course.ROOM;
        }

        public void DeleteCourse()
        {
            if (courses.Count == 0)
            {
                toast.SetText("Cannot Find a Course"); //Error Message
                toast.Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete a Course");
            alert.SetItems(courses.Keys.ToArray(), deleteCourseAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        void deleteCourseAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            string course = courses.Keys.ToArray()[e.Which];
            string name = courses[course].NAME;
            
            courses.Remove(courses.Keys.ToArray()[e.Which]);
            SaveCoursesFile();
            toast.SetText("Course: " + name + " has been deleted");
            toast.Show();
        }

        public void SaveDay()
        {
            if (currentCourse == null)
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return;
            }

            if (!Course.IsValidDay()) return;

            if(!courses.ContainsKey(currentCourse))
            {
                toast.SetText("The Course must be Saved"); //Error Message
                toast.Show();
                return;
            }
            courses[currentCourse].saveDay();
            SaveCoursesFile();
        }

        public void OpenDay()
        {
            if (currentCourse == null)
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return;
            }

            if (!courses.ContainsKey(currentCourse))
            {
                toast.SetText("The Course must be Saved"); //Error Message
                toast.Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Open a Day");
            alert.SetItems(courses[currentCourse].DAYS.Keys.ToArray(), openDayAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        public void openDayAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            string day = courses[currentCourse].DAYS.Keys.ToArray()[e.Which];
            courses[currentCourse].openDay(day);
            toast.SetText(day + " has been opened");
            toast.Show();
        }

        public void DeleteDay()
        {
            if (currentCourse == null)
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return;
            }

            if (!courses.ContainsKey(currentCourse))
            {
                toast.SetText("The Course must be saved"); //Error Message
                toast.Show();
                return;
            }

            if(courses[currentCourse].DAYS.Count<=1)
            {
                toast.SetText("Each Course must have atleast 1 Day"); //Error Message
                toast.Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete a Day");
            alert.SetItems(courses[currentCourse].DAYS.Keys.ToArray(), deleteDayAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        public void deleteDayAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            string day = courses[currentCourse].DAYS.Keys.ToArray()[e.Which];
            courses[currentCourse].DAYS.Remove(day);
            toast.SetText(day + " has been deleted");
            toast.Show();
            SaveCoursesFile();
        }
        
        public void OnClick(View v)
        {
            if (inClickEvent) return;
            inClickEvent = true;
            try
            {
                if (v.Id.Equals(saveCourseBtn.Id))
                {
                    SaveCourse();
                }
                else if (v.Id.Equals(openCourseBtn.Id))
                {
                    OpenCourse();
                }
                else if(v.Id.Equals(delCourseBtn.Id))
                {
                    DeleteCourse();
                }
                else if(v.Id.Equals(saveDayBtn.Id))
                {
                    SaveDay();
                }
                else if(v.Id.Equals(openDayBtn.Id))
                {
                    OpenDay();
                }
                else if(v.Id.Equals(delDayBtn.Id))
                {
                    DeleteDay();
                }
                
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            inClickEvent = false;
        }
    }

    /*
     * This Class represents a Course
     */
    [Serializable]
    public class Course
    {
        Dictionary<string, CourseDay> days; //Dictionary of the days for the Course.  The Keys represents a the Day of the Week, Values are a CourseDay.
        public Dictionary<string, CourseDay> DAYS
        {
            get { return days; }
        }

        string name;
        public string NAME
        {
            get { return name; }
        }
        string id;
        public string ID
        {
            get { return id; }
        }
        string major;
        public string MAJOR
        {
            get { return major; }
        }
        string room;
        public string ROOM
        {
            get { return room; }
        }
        string location;
        public string LOCATION
        {
            get { return location; }
        }
        
        public Course(string major,string id, string name,string room,string location)
        {
            this.major = major;
            this.id = id;
            Update(name,room,location);
            days = new Dictionary<string, CourseDay>();
        }

        public void Update(string name,string room,string location)
        {
            this.name = name;
            this.room = room;
            this.location = location;
        }

        /*
         * Checks the validity of the current day and time.
         * Returns true if the time is valid
         * Else returns false.
         */
        public static Boolean IsValidDay()
        {
            EditText startTime = CourseActivity.singleR.FindViewById<EditText>(Resource.Id.startTime);
            EditText endTime = CourseActivity.singleR.FindViewById<EditText>(Resource.Id.endTime);
            string timeRegex = "^(?:[01]?[0-9]|1[0-2]):[0-5][0-9]$";
            if(Regex.IsMatch(startTime.Text,timeRegex)&&Regex.IsMatch(endTime.Text,timeRegex))
            {
                return true;
            }
            CourseActivity.singleR.TOAST.SetText("The Time must be valid."); //Error Message
            CourseActivity.singleR.TOAST.Show();
            return false;
        }

        //Handles saving the current day in the Course Manager to the Course.
        public void saveDay()
        {
            EditText startTime = CourseActivity.singleR.FindViewById<EditText>(Resource.Id.startTime);
            EditText endTime = CourseActivity.singleR.FindViewById<EditText>(Resource.Id.endTime);
            Spinner startMerSpinner = CourseActivity.singleR.FindViewById<Spinner>(Resource.Id.merSpinner1);
            Spinner endMerSpinner = CourseActivity.singleR.FindViewById<Spinner>(Resource.Id.merSpinner2);
            Spinner daySpinner = CourseActivity.singleR.FindViewById<Spinner>(Resource.Id.daySpinner);
            string day = (string)daySpinner.GetItemAtPosition(daySpinner.LastVisiblePosition);
            string startMer = (string)startMerSpinner.GetItemAtPosition(startMerSpinner.LastVisiblePosition);
            string endMer = (string)endMerSpinner.GetItemAtPosition(endMerSpinner.LastVisiblePosition);
            if (days.ContainsKey(day))
            {
                days[day].Update(day, startTime.Text, startMer, endTime.Text, endMer);
                CourseActivity.singleR.TOAST.SetText(day + " has been updated for the Course: " + name);
                CourseActivity.singleR.TOAST.Show();
            }
            else
            {
                days.Add(day, new CourseDay(day, startTime.Text, startMer, endTime.Text, endMer));
                CourseActivity.singleR.TOAST.SetText(day + " has been saved to the Course: " + name);
                CourseActivity.singleR.TOAST.Show();
            }
        }

        public void openDay(string day)
        {
            CourseDay courseDay = days[day];
            ArrayAdapter adapter = ArrayAdapter.CreateFromResource(CourseActivity.singleR, Resource.Array.days_array,Android.Resource.Layout.SimpleSpinnerDropDownItem);
            Spinner daySpinner = CourseActivity.singleR.FindViewById<Spinner>(Resource.Id.daySpinner);
            daySpinner.SetSelection(adapter.GetPosition(day));

            //24 hr to 12 hr conversion
            int startHRs = int.Parse(courseDay.START_TIME.ToString(@"hh"));
            int startMins = int.Parse(courseDay.START_TIME.ToString(@"mm"));
            int endHRs = int.Parse(courseDay.END_TIME.ToString(@"hh"));
            int endMins = int.Parse(courseDay.END_TIME.ToString(@"mm"));
            Spinner merSpinner1 = CourseActivity.singleR.FindViewById<Spinner>(Resource.Id.merSpinner1);
            Spinner merSpinner2 = CourseActivity.singleR.FindViewById<Spinner>(Resource.Id.merSpinner2);
            if (startHRs == 0 || startHRs > 12)
            {
                merSpinner1.SetSelection(1);
                if (startHRs == 0)
                    startHRs = 12;
                else
                    startHRs -= 12;
            }
            else
                merSpinner1.SetSelection(0);
            if (endHRs == 0 || endHRs > 12)
            {
                merSpinner2.SetSelection(1);
                if (endHRs == 0)
                    endHRs = 12;
                else
                    endHRs -= 12;
            }
            else
                merSpinner2.SetSelection(0);
            //

            CourseActivity.singleR.FindViewById<EditText>(Resource.Id.startTime).Text = startHRs+":"+(startMins.ToString().Length==1 ? "0"+startMins.ToString() : startMins.ToString());
            CourseActivity.singleR.FindViewById<EditText>(Resource.Id.endTime).Text = endHRs + ":" + (endMins.ToString().Length == 1 ? "0" + endMins.ToString() : endMins.ToString());
        }

        /*
         * This Class represents a Day of the Week and Time Schedule for a course
         */
        [Serializable]
        public class CourseDay
        {
            string day;
            public string DAY
            {
                get { return day; }
            }
            TimeSpan startTime;
            public TimeSpan START_TIME
            {
                get { return startTime; }
            }
            TimeSpan endTime;
            public TimeSpan END_TIME
            {
                get { return endTime; }
            }
            public CourseDay(string day, string startTimeStamp, string startMer,string endTimeStamp, string endMer)
            {
                Update(day, startTimeStamp, startMer, endTimeStamp, endMer);
            }
            public void Update(string day, string startTimeStamp, string startMer, string endTimeStamp, string endMer)
            {
                this.day = day;
                //12 hr to 24 hr conversion
                int hours = int.Parse(startTimeStamp.Substring(0, startTimeStamp.IndexOf(":")));
                int minutes = int.Parse(startTimeStamp.Substring(startTimeStamp.IndexOf(":") + 1));
                if (startMer == "PM")
                    hours = (hours + 12) % 24;
                startTime = new TimeSpan(hours, minutes, 0);
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