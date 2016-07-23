using System;
using System.Collections.Generic;
using System.Linq;
using Java.IO;
using Java.Util;
using System.Text.RegularExpressions;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

/// <summary>
/// This Class is the CourseActivity. It handles managing the courses for a Student.
/// <br/>Files related to the CourseActivity:
/// <br/>Resources/
/// <br/>layout/CourseLayout.axml - The Layout that the CourseActivty will use.
/// <br/>values/
/// <br/>days.xml - Contains a list of Days.
/// <br/>locations.xml - Contains a list of Locations.
/// <br/>meridian.xml - Contains AM and PM as a list.
/// <br/>
/// <br/>     Assets/BasicInfoFiles/Courses.txt - the Courses that will be parse into Dictionaries and Lists.
/// </summary>
namespace WCCMobile
{
    [Activity(Label = "CourseActivity", ParentActivity = typeof(WCCMobile.MainActivity))]
    public class CourseActivity : Activity, AdapterView.IOnItemSelectedListener, View.IOnClickListener
    {
        static CourseActivity singleton = null;
        public static CourseActivity singleR
        {
            get { return singleton; }
        }
        static ActivityAttribute attr = null;
        static ActivityAttribute ATTR
        {
            get { return attr != null ? attr : attr = new ActivityAttribute(); }
        }
        Dictionary<string, List<string>> majorNames; //Dictionary of the Names, Keys are the major, Values are a list of Names that the major holds (Placement order matches majorIDs)
        Dictionary<string, List<string>> majorIDs; //Dictionary of the IDs, Keys are the major, Values are a list of IDs that the major holds (Placement order matches majorNames)
        List<string> majors; //A list of all the available Majors.

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
            toast = Toast.MakeText(this, "", ToastLength.Short);

            CourseRepository.LoadRepository();

            BufferedReader reader = null;
            #region IO init
            try
            {
                //Reads the Courses.txt from the Course Parser and sets up the Dictionaries and Lists prior to the values.
                reader = new BufferedReader(
                    new InputStreamReader(MainActivity.singleR.Assets.Open("BasicInfoFiles/Courses.txt")));

                string lastMajor = null;
                string major;
                string id;
                string name;
                string line = null;
                StringTokenizer lineTkns = null;
                bool firstLine = true;
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
            courseSpinner.OnItemSelectedListener = this;

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
            #endregion
            //ActionBar.SetIcon(ImageAdapter.Label);
            // ActionBar.SetIcon(Android.Resource.Color.Transparent);
            ATTR.MainLauncher = true;
        }

        /// <summary>
        /// Handles what occurs when an Item is Selected, e.g. a Spinner
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="view"></param>
        /// <param name="position"></param>
        /// <param name="id"></param>
        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            if (parent.Id.Equals(courseSpinner.Id))
            {
                string currentMajor = (string)parent.GetItemAtPosition(position);
                if (previousMajor != currentMajor)
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

        /// <summary>
        /// Sets up the id Spinner for the selected major
        /// </summary>
        /// <param name="major"></param>
        void setIDSpinner(string major)
        {
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

        /// <summary>
        /// Checks the validity of a course.
        /// </summary>
        /// <returns>Returns true if a Course can be saved without an issue, else return false.</returns>
        public bool IsValidCourse()
        {
            Spinner courseSpinner = FindViewById<Spinner>(Resource.Id.courseSpinner);
            if (((string)courseSpinner.GetItemAtPosition(courseSpinner.LastVisiblePosition)).Equals("None"))
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return false;
            }
            EditText room = FindViewById<EditText>(Resource.Id.roomText);
            if (room.Text.Length == 0)
            {
                toast.SetText("Enter a Room Number"); //Error Message
                toast.Show();
                return false;
            }
            return IsValidDay();
        }

        /// <summary>
        /// Handles saving the current course that the manager has setup.
        /// </summary>
        public void SaveCourse()
        {
            if (!IsValidCourse()) return;

            string room = FindViewById<EditText>(Resource.Id.roomText).Text;
            Spinner bldgSpinner = FindViewById<Spinner>(Resource.Id.bldgSpinner);
            string bldg = (string)bldgSpinner.GetItemAtPosition(bldgSpinner.LastVisiblePosition);
            string courseName = FindViewById<TextView>(Resource.Id.courseName).Text;
            Course course;
            if (CourseRepository.Contains(currentCourse.GetHashCode()))
            {
                course = CourseRepository.GetCourse(currentCourse.GetHashCode());
                course.Update(courseName, room, bldg);
                toast.SetText("Course: " + course.NAME + " has been updated");
            }
            else
            {
                Spinner courseSpinner = FindViewById<Spinner>(Resource.Id.courseSpinner);
                string major = (string)courseSpinner.GetItemAtPosition(courseSpinner.LastVisiblePosition);
                Spinner idSpinner = FindViewById<Spinner>(Resource.Id.idSpinner);
                string courseID = (string)idSpinner.GetItemAtPosition(idSpinner.LastVisiblePosition);

                course = new Course(major, courseID, courseName, room, bldg);
                CourseRepository.InsertCourse(course);
                toast.SetText("Course: " + course.NAME + " has been saved");

            }

            EditText startTime = FindViewById<EditText>(Resource.Id.startTime);
            EditText endTime = FindViewById<EditText>(Resource.Id.endTime);
            Spinner startMerSpinner = FindViewById<Spinner>(Resource.Id.merSpinner1);
            Spinner endMerSpinner = FindViewById<Spinner>(Resource.Id.merSpinner2);
            Spinner daySpinner = FindViewById<Spinner>(Resource.Id.daySpinner);
            string day = (string)daySpinner.GetItemAtPosition(daySpinner.LastVisiblePosition);
            string startMer = (string)startMerSpinner.GetItemAtPosition(startMerSpinner.LastVisiblePosition);
            string endMer = (string)endMerSpinner.GetItemAtPosition(endMerSpinner.LastVisiblePosition);
            if (course.ContainsDay(day))
            {
                course.GetDay(day).UpdateTime(startTime.Text, startMer, endTime.Text, endMer);
            }
            else
            {
                course.InsertDay(new Course.CourseDay(day, startTime.Text, startMer, endTime.Text, endMer));
            }

            CourseRepository.SaveRepository();

            toast.Show();
        }

        /// <summary>
        /// Handles setting up the alert for deleting a Course
        /// </summary>
        public void OpenCourseAlert()
        {
            if (CourseRepository.GetCourseList().Count == 0)
            {
                toast.SetText("Cannot Find a Course"); //Error Message
                toast.Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Open a Course");
            alert.SetItems(CourseRepository.GetCourseIDArray(), OpenCourseAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        /// <summary>
        /// The Handler for when a Course is chosen to be opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OpenCourseAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            currentCourse = CourseRepository.GetCourseIDArray()[e.Which];
            Course course = CourseRepository.GetCourse(currentCourse.GetHashCode());
            courseSpinner.SetSelection(majors.IndexOf(course.MAJOR));

            idSpinnerIndex = majorIDs[course.MAJOR].IndexOf(course.ID);

            Spinner bldgSpinner = FindViewById<Spinner>(Resource.Id.bldgSpinner);
            ArrayAdapter adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.locations_array, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            bldgSpinner.SetSelection(adapter.GetPosition(course.LOCATION));
            courseName.Text = course.NAME;

            OpenDay(course.DAY_OF_WEEK.Keys.ToArray()[0]);

            FindViewById<EditText>(Resource.Id.roomText).Text = course.ROOM;
        }

        /// <summary>
        /// Handles setting up the alert for deleting a Course
        /// </summary>
        public void DeleteCourseAlert()
        {
            if (CourseRepository.GetCourseList().Count == 0)
            {
                toast.SetText("Cannot Find a Course"); //Error Message
                toast.Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete a Course");
            alert.SetItems(CourseRepository.GetCourseIDArray(), deleteCourseAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        /// <summary>
        /// The Handler for when a Course is chosen to be deleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void deleteCourseAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            int id = CourseRepository.GetCourseIDArray()[e.Which].GetHashCode();
            string name = CourseRepository.GetCourse(id).NAME;

            CourseRepository.DeleteCourse(CourseRepository.GetCourseIDArray()[e.Which].GetHashCode());
            CourseRepository.SaveRepository();
            toast.SetText("Course: " + name + " has been deleted");
            toast.Show();
        }

        //Handles saving the current day in the Course Manager to the Course.
        public void SaveDay()
        {
            if (currentCourse == null)
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return;
            }

            if (!IsValidDay()) return;

            if (!CourseRepository.Contains(currentCourse.GetHashCode()))
            {
                toast.SetText("The Course must be Saved"); //Error Message
                toast.Show();
                return;
            }

            EditText startTime = FindViewById<EditText>(Resource.Id.startTime);
            EditText endTime = FindViewById<EditText>(Resource.Id.endTime);
            Spinner startMerSpinner = FindViewById<Spinner>(Resource.Id.merSpinner1);
            Spinner endMerSpinner = FindViewById<Spinner>(Resource.Id.merSpinner2);
            Spinner daySpinner = FindViewById<Spinner>(Resource.Id.daySpinner);
            string day = (string)daySpinner.GetItemAtPosition(daySpinner.LastVisiblePosition);
            string startMer = (string)startMerSpinner.GetItemAtPosition(startMerSpinner.LastVisiblePosition);
            string endMer = (string)endMerSpinner.GetItemAtPosition(endMerSpinner.LastVisiblePosition);

            Course course = CourseRepository.GetCourse(currentCourse.GetHashCode());
            if (course.ContainsDay(day))
            {
                course.GetDay(day).UpdateTime(startTime.Text, startMer, endTime.Text, endMer);
                TOAST.SetText(day + " has been updated for the Course: " + course.NAME);
                
            }
            else
            {
                course.InsertDay(new Course.CourseDay(day, startTime.Text, startMer, endTime.Text, endMer));
                TOAST.SetText(day + " has been saved to the Course: " + course.NAME);
            }

            CourseRepository.SaveRepository();
            TOAST.Show();
        }

        /// <summary>
        /// Opens up the day
        /// </summary>
        /// <param name="day"></param>
        public void OpenDay(string day)
        {
            Course.CourseDay courseDay = CourseRepository.GetCourse(currentCourse.GetHashCode()).GetDay(day);
            ArrayAdapter adapter = ArrayAdapter.CreateFromResource(CourseActivity.singleR, Resource.Array.days_array, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            Spinner daySpinner = FindViewById<Spinner>(Resource.Id.daySpinner);
            daySpinner.SetSelection(adapter.GetPosition(day));

            //24 hr to 12 hr conversion
            int startHRs = int.Parse(courseDay.START_TIME.ToString(@"hh"));
            int startMins = int.Parse(courseDay.START_TIME.ToString(@"mm"));
            int endHRs = int.Parse(courseDay.END_TIME.ToString(@"hh"));
            int endMins = int.Parse(courseDay.END_TIME.ToString(@"mm"));
            Spinner merSpinner1 = FindViewById<Spinner>(Resource.Id.merSpinner1);
            Spinner merSpinner2 = FindViewById<Spinner>(Resource.Id.merSpinner2);
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

            FindViewById<EditText>(Resource.Id.startTime).Text = startHRs + ":" + (startMins.ToString().Length == 1 ? "0" + startMins.ToString() : startMins.ToString());
            FindViewById<EditText>(Resource.Id.endTime).Text = endHRs + ":" + (endMins.ToString().Length == 1 ? "0" + endMins.ToString() : endMins.ToString());
        }

        /// <summary>
        /// Handles setting up the alert for opening a day
        /// </summary>
        public void OpenDayAlert()
        {
            if (currentCourse == null)
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return;
            }

            if (!CourseRepository.Contains(currentCourse.GetHashCode()))
            {
                toast.SetText("The Course must be Saved"); //Error Message
                toast.Show();
                return;
            }

            Course course = CourseRepository.GetCourse(currentCourse.GetHashCode());
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Open a Day");
            alert.SetItems(course.DAY_OF_WEEK.Keys.ToArray(), OpenDayAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        /// <summary>
        /// The Handler for when a day is chosen to be opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenDayAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            string day = CourseRepository.GetCourse(currentCourse.GetHashCode()).DAY_OF_WEEK.Keys.ToArray()[e.Which];

            OpenDay(day);

            toast.SetText(day + " has been opened");
            toast.Show();
        }

        /// <summary>
        /// Handles setting up the alert for deleting a day.
        /// </summary>
        public void DeleteDayAlert()
        {
            if (currentCourse == null)
            {
                toast.SetText("Choose a Course"); //Error Message
                toast.Show();
                return;
            }

            if (!CourseRepository.Contains(currentCourse.GetHashCode()))
            {
                toast.SetText("The Course must be saved"); //Error Message
                toast.Show();
                return;
            }

            Course course = CourseRepository.GetCourse(currentCourse.GetHashCode());

            if (course.DAY_OF_WEEK.Count<=1)
            {
                toast.SetText("Each Course must have atleast 1 Day"); //Error Message
                toast.Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete a Day");
            alert.SetItems(course.DAY_OF_WEEK.Keys.ToArray(), deleteDayAlertHandler);
            alert.Show();
            alert.Dispose();
        }

        /// <summary>
        /// The Handler for when a day is chosen to be deleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void deleteDayAlertHandler(object sender, DialogClickEventArgs e)
        {
            AlertDialog objAlertDialog = sender as AlertDialog;
            Course course = CourseRepository.GetCourse(currentCourse.GetHashCode());
            string day = course.DAY_OF_WEEK.Keys.ToArray()[e.Which];
            course.DeleteDay(day);
            toast.SetText(day + " has been deleted");
            
            CourseRepository.SaveRepository();

            toast.Show();
        }
        
        /// <summary>
        /// Handles what occurs when a button is Clicked
        /// </summary>
        /// <param name="v"></param>
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
                    OpenCourseAlert();
                }
                else if(v.Id.Equals(delCourseBtn.Id))
                {
                    DeleteCourseAlert();
                }
                else if(v.Id.Equals(saveDayBtn.Id))
                {
                    SaveDay();
                }
                else if(v.Id.Equals(openDayBtn.Id))
                {
                    OpenDayAlert();
                }
                else if(v.Id.Equals(delDayBtn.Id))
                {
                    DeleteDayAlert();
                }
                
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            inClickEvent = false;
        }

        /// <summary>
        /// Checks the validity of the current day and time.
        /// </summary>
        /// <returns>Returns true if the time is valid, else returns false.</returns>
        public static bool IsValidDay()
        {
            EditText startTime = CourseActivity.singleR.FindViewById<EditText>(Resource.Id.startTime);
            EditText endTime = CourseActivity.singleR.FindViewById<EditText>(Resource.Id.endTime);
            string timeRegex = "^(?:[01]?[0-9]|1[0-2]):[0-5][0-9]$";
            if (Regex.IsMatch(startTime.Text, timeRegex) && Regex.IsMatch(endTime.Text, timeRegex))
            {
                return true;
            }
            CourseActivity.singleR.TOAST.SetText("The Time must be valid."); //Error Message
            CourseActivity.singleR.TOAST.Show();
            return false;
        }
    }
}