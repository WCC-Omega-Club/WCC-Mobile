using System.Collections.Generic;

namespace WCCMobile
{
    public interface ICourseRepository
    {
        List<Course> GetCourseList(ref List<string> errors);

        Course GetCourse(int id, ref List<string> errors);

        void InsertCourse(Course c, ref List<string> errors);

        void UpdateCourse(Course c, ref List<string> errors);

        void DeleteCourse(int id, ref List<string> errors);

        void UpdatePreReq(int id, string prereq, ref List<string> errors);
    }
}