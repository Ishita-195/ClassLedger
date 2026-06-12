using System.Collections.Generic;

namespace StudentResult.Models
{
    public class StudentResultVM
    {
        // Single object references
        public SrUsers SrUsersObj { get; set; }
        public SrStudents SrStudentsObj { get; set; }
        public SrClasses SrClassesObj { get; set; }
        public SrSubjects SrSubjectsObj { get; set; }
        public SrMarks SrMarksObj { get; set; }
        public SrAttendance SrAttendanceObj { get; set; }

        // List references
        public List<SrStudents> AllStudents { get; set; }
        public List<SrClasses> AllClasses { get; set; }
        public List<SrSubjects> AllSubjects { get; set; }
        public List<SrMarks> AllMarks { get; set; }
        public List<SrAttendance> AllAttendance { get; set; }
        public List<SrUsers> AllUsers { get; set; }

        // Dashboard summary counts
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
    }
}
