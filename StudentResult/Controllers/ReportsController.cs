using System;
using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;   // GetDbConnection()
using StudentResult.Models;

namespace StudentResult.Controllers
{
    // ─────────────────────────────────────────────────────────────
    // Analytics dashboard — ADMIN ONLY.
    //
    // Every chart is backed by an aggregate SQL query (GROUP BY) run
    // straight against the Oracle schema, so the heavy lifting happens
    // in the database and only small result sets travel to the app.
    // Tables live in the "SR" schema (see ModelContext mappings).
    // ─────────────────────────────────────────────────────────────
    public class ReportsController : Controller
    {
        private readonly ModelContext _context;

        public ReportsController(ModelContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Same session-based, role-based auth used across the app.
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login", "Home");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Dashboard", "Home");

            var vm = new ReportsDashboardVM();

            var conn = _context.Database.GetDbConnection();
            var wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                LoadAttendanceDistribution(conn, vm);
                LoadCoursePassFail(conn, vm);
                LoadGradeDistribution(conn, vm);
                LoadAttendanceVsResult(conn, vm);
            }
            finally
            {
                if (wasClosed) conn.Close();
            }

            ViewBag.Role = HttpContext.Session.GetString("UserRole");
            return View(vm);
        }

        // ── Chart 1: attendance-% distribution across students ──────
        // Per student: present / total → %, bucketed into bands, then
        // counted. Aggregation is fully server-side.
        private void LoadAttendanceDistribution(DbConnection conn, ReportsDashboardVM vm)
        {
            const string sql = @"
                SELECT bucket, COUNT(*) AS student_count
                FROM (
                    SELECT CASE
                             WHEN pct >= 90 THEN '90-100%'
                             WHEN pct >= 75 THEN '75-89%'
                             WHEN pct >= 60 THEN '60-74%'
                             WHEN pct >= 40 THEN '40-59%'
                             ELSE '0-39%'
                           END AS bucket,
                           pct
                    FROM (
                        SELECT STUDENTID,
                               100.0 * SUM(CASE WHEN STATUS = 'Present' THEN 1 ELSE 0 END) / COUNT(*) AS pct
                        FROM SR_ATTENDANCE
                        GROUP BY STUDENTID
                    ) per_student
                ) banded
                GROUP BY bucket
                ORDER BY MIN(pct)";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        vm.AttendanceDistribution.Add(new AttendanceDistributionVM
                        {
                            Bucket = r.GetString(0),
                            StudentCount = Convert.ToInt32(r.GetValue(1))
                        });
                    }
                }
            }
        }

        // ── Chart 2: pass / fail counts per course ──────────────────
        // A mark passes when scored >= 40% of total. Counts aggregated
        // per subject in a single grouped query.
        private void LoadCoursePassFail(DbConnection conn, ReportsDashboardVM vm)
        {
            const string sql = @"
                SELECT su.SUBJECTNAME,
                       SUM(CASE WHEN m.MARKSSCORED >= 0.4 * m.TOTALMARKS THEN 1 ELSE 0 END) AS pass_count,
                       SUM(CASE WHEN m.MARKSSCORED <  0.4 * m.TOTALMARKS THEN 1 ELSE 0 END) AS fail_count
                FROM SR_MARKS m
                JOIN SR_SUBJECTS su ON su.SUBJECTID = m.SUBJECTID
                WHERE m.TOTALMARKS > 0
                GROUP BY su.SUBJECTID, su.SUBJECTNAME
                ORDER BY su.SUBJECTNAME";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        vm.CoursePassFail.Add(new CoursePassFailVM
                        {
                            CourseName = r.GetString(0),
                            PassCount = Convert.ToInt32(r.GetValue(1)),
                            FailCount = Convert.ToInt32(r.GetValue(2))
                        });
                    }
                }
            }
        }

        // ── Chart 3: grade / marks distribution histogram ───────────
        // Each mark's percentage is bucketed into a grade band and the
        // bands are counted server-side.
        private void LoadGradeDistribution(DbConnection conn, ReportsDashboardVM vm)
        {
            const string sql = @"
                SELECT grade, COUNT(*) AS cnt
                FROM (
                    SELECT CASE
                             WHEN pct >= 90 THEN 'A (90-100)'
                             WHEN pct >= 75 THEN 'B (75-89)'
                             WHEN pct >= 60 THEN 'C (60-74)'
                             WHEN pct >= 40 THEN 'D (40-59)'
                             ELSE 'F (0-39)'
                           END AS grade,
                           pct
                    FROM (
                        SELECT (MARKSSCORED * 100.0 / TOTALMARKS) AS pct
                        FROM SR_MARKS
                        WHERE TOTALMARKS > 0
                    ) per_mark
                ) banded
                GROUP BY grade
                ORDER BY MIN(pct)";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        vm.GradeDistribution.Add(new GradeDistributionVM
                        {
                            Grade = r.GetString(0),
                            Count = Convert.ToInt32(r.GetValue(1))
                        });
                    }
                }
            }
        }

        // ── Chart 4: avg attendance vs avg result (scatter) ─────────
        // Two grouped sub-queries (attendance %, result %) joined per
        // student to expose any correlation.
        private void LoadAttendanceVsResult(DbConnection conn, ReportsDashboardVM vm)
        {
            const string sql = @"
                SELECT s.STUDENTNAME, att.avg_att, res.avg_res
                FROM SR_STUDENTS s
                JOIN (
                    SELECT STUDENTID,
                           100.0 * SUM(CASE WHEN STATUS = 'Present' THEN 1 ELSE 0 END) / COUNT(*) AS avg_att
                    FROM SR_ATTENDANCE
                    GROUP BY STUDENTID
                ) att ON att.STUDENTID = s.STUDENTID
                JOIN (
                    SELECT STUDENTID,
                           AVG(MARKSSCORED * 100.0 / TOTALMARKS) AS avg_res
                    FROM SR_MARKS
                    WHERE TOTALMARKS > 0
                    GROUP BY STUDENTID
                ) res ON res.STUDENTID = s.STUDENTID
                ORDER BY s.STUDENTNAME";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        vm.AttendanceVsResult.Add(new AttendanceVsResultVM
                        {
                            StudentName = r.GetString(0),
                            AvgAttendance = Math.Round(Convert.ToDouble(r.GetValue(1)), 1),
                            AvgResult = Math.Round(Convert.ToDouble(r.GetValue(2)), 1)
                        });
                    }
                }
            }
        }
    }
}
