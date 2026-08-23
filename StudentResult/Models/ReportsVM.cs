using System.Collections.Generic;

namespace StudentResult.Models
{
    // ─────────────────────────────────────────────────────────────
    // ViewModels backing the Admin analytics dashboard.
    // One row-type per chart, aggregated by ReportsDashboardVM.
    // Every list is populated by a GROUP BY query in ReportsController
    // (no in-memory LINQ over full tables).
    // ─────────────────────────────────────────────────────────────

    /// <summary>One bucket of the attendance-percentage distribution across students.</summary>
    public class AttendanceDistributionVM
    {
        public string Bucket { get; set; }       // e.g. "75-89%"
        public int StudentCount { get; set; }     // students whose attendance % falls in this bucket
    }

    /// <summary>Pass / fail tally for a single course (subject).</summary>
    public class CoursePassFailVM
    {
        public string CourseName { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
    }

    /// <summary>One grade band of the marks-distribution histogram.</summary>
    public class GradeDistributionVM
    {
        public string Grade { get; set; }         // e.g. "A (90-100)"
        public int Count { get; set; }            // number of mark entries in this band
    }

    /// <summary>One student's average attendance vs average result (scatter point).</summary>
    public class AttendanceVsResultVM
    {
        public string StudentName { get; set; }
        public double AvgAttendance { get; set; } // percentage
        public double AvgResult { get; set; }     // percentage
    }

    /// <summary>Container passed to the Reports/Index view.</summary>
    public class ReportsDashboardVM
    {
        public List<AttendanceDistributionVM> AttendanceDistribution { get; set; } = new List<AttendanceDistributionVM>();
        public List<CoursePassFailVM> CoursePassFail { get; set; } = new List<CoursePassFailVM>();
        public List<GradeDistributionVM> GradeDistribution { get; set; } = new List<GradeDistributionVM>();
        public List<AttendanceVsResultVM> AttendanceVsResult { get; set; } = new List<AttendanceVsResultVM>();
    }
}
