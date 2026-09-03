using System.Collections.Generic;

namespace StudentResult.Models
{
    // ─────────────────────────────────────────────────────────────
    // View models backing the student-centric Marks & Attendance
    // detail pages (master → detail flow).
    // ─────────────────────────────────────────────────────────────

    public class SubjectMarkRow
    {
        public string Subject { get; set; }
        public bool Recorded { get; set; }
        public decimal? Scored { get; set; }
        public decimal? Total { get; set; }
        public decimal Percentage { get; set; }
        public string Grade { get; set; }
        public int? Markid { get; set; }
    }

    public class StudentMarksVM
    {
        public SrStudents Student { get; set; }
        public SrClasses Class { get; set; }
        public List<SrSubjects> Subjects { get; set; } = new List<SrSubjects>();
        public List<SubjectMarkRow> Rows { get; set; } = new List<SubjectMarkRow>();

        public decimal TotalScored { get; set; }
        public decimal TotalMax { get; set; }
        public decimal OverallPercentage { get; set; }
        public decimal AveragePercentage { get; set; }
        public int SubjectsRecorded { get; set; }
        public int SubjectsPassed { get; set; }
        public string OverallGrade { get; set; }
        public string Performance { get; set; }      // Excellent / Good / Average / Needs Improvement
        public string PerformanceColor { get; set; } // bootstrap contextual suffix
    }

    public class MonthOption
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; }
        public bool Selected { get; set; }
    }

    public class StudentAttendanceVM
    {
        public SrStudents Student { get; set; }
        public SrClasses Class { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }

        public int Present { get; set; }
        public int Absent { get; set; }
        public int Leave { get; set; }
        public int TotalDays { get; set; }
        public double Percentage { get; set; }

        public bool HasData { get; set; }
        public List<MonthOption> AvailableMonths { get; set; } = new List<MonthOption>();
    }
}
