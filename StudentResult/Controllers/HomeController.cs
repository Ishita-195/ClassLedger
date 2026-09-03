using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using StudentResult.Models;

namespace StudentResult.Controllers
{
    public class HomeController : Controller
    {
        private readonly ModelContext _context;

        public HomeController(ModelContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────
        // AUTH
        // ─────────────────────────────────────────

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult doLogin(StudentResultVM model)
        {
            var input = model.SrUsersObj;
            var user = _context.SrUsers.SingleOrDefault(u =>
                u.Useremail == input.Useremail && u.Userpassword == input.Userpassword);

            if (user != null)
            {
                HttpContext.Session.SetString("UserID",   user.Userid.ToString());
                HttpContext.Session.SetString("UserName", user.Username);
                HttpContext.Session.SetString("UserRole", user.Userrole);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Message = "Invalid email or password!";
            return View("Login", model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ─────────────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────────────

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            var role = HttpContext.Session.GetString("UserRole");

            StudentResultVM obj = new StudentResultVM();
            obj.TotalStudents  = _context.SrStudents.Count();
            obj.TotalClasses   = _context.SrClasses.Count();
            obj.TotalSubjects  = _context.SrSubjects.Count();
            obj.TotalPresent   = _context.SrAttendance.Count(a => a.Status == "Present");
            obj.TotalAbsent    = _context.SrAttendance.Count(a => a.Status == "Absent");
            obj.AllStudents    = _context.SrStudents.OrderBy(s => s.Studentname).ToList();
            obj.AllClasses     = _context.SrClasses.ToList();

            ViewBag.Role = role;
            return View(obj);
        }

        // ─────────────────────────────────────────
        // STUDENTS
        // ─────────────────────────────────────────

        public IActionResult Students()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            StudentResultVM obj = new StudentResultVM();
            obj.AllStudents = _context.SrStudents.OrderBy(s => s.Studentname).ToList();
            obj.AllClasses  = _context.SrClasses.ToList();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStudent(StudentResultVM model)
        {
            var s = model.SrStudentsObj;
            var maxId = _context.SrStudents.Select(x => x.Studentid).DefaultIfEmpty(0).Max();
            s.Studentid = maxId + 1;
            _context.Add(s);
            _context.SaveChanges();
            return RedirectToAction("Students");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.SrStudents.Find(id);
            if (student != null)
            {
                _context.SrStudents.Remove(student);
                _context.SaveChanges();
            }
            return RedirectToAction("Students");
        }

        // ─────────────────────────────────────────
        // MARKS / RESULTS  (master → detail)
        // ─────────────────────────────────────────

        // Master: pick a student.
        public IActionResult Marks()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            var obj = new StudentResultVM
            {
                AllStudents = _context.SrStudents.OrderBy(s => s.Studentname).ToList(),
                AllClasses  = _context.SrClasses.ToList()
            };
            return View(obj);
        }

        // Detail: one student's marks across all subjects, with statistics.
        public IActionResult StudentMarks(int id)
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            var student = _context.SrStudents.Find(id);
            if (student == null)
                return RedirectToAction("Marks");

            var subjects = _context.SrSubjects.OrderBy(s => s.Subjectid).ToList();
            var marks = _context.SrMarks.Where(m => m.Studentid == id).ToList();

            var vm = new StudentMarksVM
            {
                Student  = student,
                Class    = _context.SrClasses.FirstOrDefault(c => c.Classid == student.Classid),
                Subjects = subjects
            };

            foreach (var sub in subjects)
            {
                var entry = marks.Where(m => m.Subjectid == sub.Subjectid)
                                 .OrderByDescending(m => m.Markid)
                                 .FirstOrDefault();
                var row = new SubjectMarkRow { Subject = sub.Subjectname };
                if (entry != null && entry.Totalmarks > 0)
                {
                    row.Recorded   = true;
                    row.Markid     = entry.Markid;
                    row.Scored     = entry.Marksscored;
                    row.Total      = entry.Totalmarks;
                    row.Percentage = Math.Round((decimal)(entry.Marksscored / entry.Totalmarks * 100), 1);
                    row.Grade      = Grade(row.Percentage);
                }
                vm.Rows.Add(row);
            }

            var recorded = vm.Rows.Where(r => r.Recorded).ToList();
            vm.SubjectsRecorded   = recorded.Count;
            vm.TotalScored        = recorded.Sum(r => r.Scored ?? 0);
            vm.TotalMax           = recorded.Sum(r => r.Total ?? 0);
            vm.OverallPercentage  = vm.TotalMax > 0 ? Math.Round(vm.TotalScored / vm.TotalMax * 100, 1) : 0;
            vm.AveragePercentage  = recorded.Count > 0 ? Math.Round(recorded.Average(r => r.Percentage), 1) : 0;
            vm.SubjectsPassed     = recorded.Count(r => r.Percentage >= 40);
            vm.OverallGrade       = Grade(vm.OverallPercentage);
            (vm.Performance, vm.PerformanceColor) = PerformanceLabel(vm.OverallPercentage);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMarks(StudentResultVM model)
        {
            var m = model.SrMarksObj;
            var maxId = _context.SrMarks.Select(x => x.Markid).DefaultIfEmpty(0).Max();
            m.Markid   = maxId + 1;
            m.Examdate = DateTime.Now;
            _context.Add(m);
            _context.SaveChanges();
            return RedirectToAction("StudentMarks", new { id = m.Studentid });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMark(int id)
        {
            var mark = _context.SrMarks.Find(id);
            int? sid = mark?.Studentid;
            if (mark != null)
            {
                _context.SrMarks.Remove(mark);
                _context.SaveChanges();
            }
            return sid != null
                ? RedirectToAction("StudentMarks", new { id = sid })
                : RedirectToAction("Marks");
        }

        // ─────────────────────────────────────────
        // ATTENDANCE  (master → detail)
        // ─────────────────────────────────────────

        // Master: pick a student. Students are redirected to their own record.
        public IActionResult Attendance()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            if (HttpContext.Session.GetString("UserRole") == "Student")
            {
                var uid = Convert.ToDecimal(HttpContext.Session.GetString("UserID"));
                var me = _context.SrStudents.FirstOrDefault(s => s.Userid == uid);
                if (me != null)
                    return RedirectToAction("StudentAttendance", new { id = me.Studentid });
            }

            var obj = new StudentResultVM
            {
                AllStudents = _context.SrStudents.OrderBy(s => s.Studentname).ToList(),
                AllClasses  = _context.SrClasses.ToList()
            };
            return View(obj);
        }

        // Detail: one student's attendance for a selected month, with stats.
        public IActionResult StudentAttendance(int id, int? year, int? month)
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            var student = _context.SrStudents.Find(id);
            if (student == null)
                return RedirectToAction("Attendance");

            var records = _context.SrAttendance
                .Where(a => a.Studentid == id && a.Attenddate != null)
                .ToList();

            var vm = new StudentAttendanceVM
            {
                Student = student,
                Class   = _context.SrClasses.FirstOrDefault(c => c.Classid == student.Classid)
            };

            var months = records
                .Select(a => new { a.Attenddate.Value.Year, a.Attenddate.Value.Month })
                .Distinct()
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .ToList();

            int selYear, selMonth;
            if (year.HasValue && month.HasValue) { selYear = year.Value; selMonth = month.Value; }
            else if (months.Any()) { selYear = months.First().Year; selMonth = months.First().Month; }
            else { selYear = DateTime.Now.Year; selMonth = DateTime.Now.Month; }

            vm.Year = selYear;
            vm.Month = selMonth;
            vm.MonthName = new DateTime(selYear, selMonth, 1).ToString("MMMM yyyy");

            foreach (var mo in months)
            {
                vm.AvailableMonths.Add(new MonthOption
                {
                    Year = mo.Year,
                    Month = mo.Month,
                    Label = new DateTime(mo.Year, mo.Month, 1).ToString("MMMM yyyy"),
                    Selected = mo.Year == selYear && mo.Month == selMonth
                });
            }

            var monthRecords = records
                .Where(a => a.Attenddate.Value.Year == selYear && a.Attenddate.Value.Month == selMonth)
                .ToList();

            vm.Present   = monthRecords.Count(a => a.Status == "Present");
            vm.Absent    = monthRecords.Count(a => a.Status == "Absent");
            vm.Leave     = monthRecords.Count(a => a.Status == "Leave");
            vm.TotalDays = monthRecords.Count;
            vm.HasData   = vm.TotalDays > 0;
            vm.Percentage = vm.TotalDays > 0 ? Math.Round((double)vm.Present / vm.TotalDays * 100, 1) : 0;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAttendance(StudentResultVM model)
        {
            var a = model.SrAttendanceObj;
            var maxId = _context.SrAttendance.Select(x => x.Attendanceid).DefaultIfEmpty(0).Max();
            a.Attendanceid = maxId + 1;
            if (a.Attenddate == null) a.Attenddate = DateTime.Now;
            _context.Add(a);
            _context.SaveChanges();
            return RedirectToAction("StudentAttendance", new { id = a.Studentid });
        }

        // ── grading helpers ─────────────────────────
        private static string Grade(decimal pct) =>
            pct >= 90 ? "A+" :
            pct >= 75 ? "A"  :
            pct >= 60 ? "B"  :
            pct >= 45 ? "C"  :
            pct >= 40 ? "D"  : "F";

        private static (string Label, string Color) PerformanceLabel(decimal pct) =>
            pct >= 75 ? ("Excellent", "success") :
            pct >= 60 ? ("Good", "primary") :
            pct >= 40 ? ("Average", "warning") :
                        ("Needs Improvement", "danger");

        // ─────────────────────────────────────────
        // REPORT CARD  (student-facing)
        // ─────────────────────────────────────────

        public IActionResult ReportCard()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            var userId = Convert.ToDecimal(HttpContext.Session.GetString("UserID"));
            var student = _context.SrStudents.SingleOrDefault(s => s.Userid == userId);

            StudentResultVM obj = new StudentResultVM();
            if (student != null)
            {
                obj.SrStudentsObj = student;
                obj.AllMarks      = _context.SrMarks.Where(m => m.Studentid == student.Studentid).ToList();
                obj.AllSubjects   = _context.SrSubjects.ToList();
                obj.AllAttendance = _context.SrAttendance.Where(a => a.Studentid == student.Studentid).ToList();
            }
            return View(obj);
        }

        // ─────────────────────────────────────────
        // CLASSES & SUBJECTS  (Admin)
        // ─────────────────────────────────────────

        public IActionResult Classes()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            StudentResultVM obj = new StudentResultVM();
            obj.AllClasses  = _context.SrClasses.ToList();
            obj.AllSubjects = _context.SrSubjects.ToList();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClass(StudentResultVM model)
        {
            var c = model.SrClassesObj;
            var maxId = _context.SrClasses.Select(x => x.Classid).DefaultIfEmpty(0).Max();
            c.Classid = maxId + 1;
            _context.Add(c);
            _context.SaveChanges();
            return RedirectToAction("Classes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSubject(StudentResultVM model)
        {
            var s = model.SrSubjectsObj;
            var maxId = _context.SrSubjects.Select(x => x.Subjectid).DefaultIfEmpty(0).Max();
            s.Subjectid = maxId + 1;
            _context.Add(s);
            _context.SaveChanges();
            return RedirectToAction("Classes");
        }

        // ─────────────────────────────────────────
        // ERROR
        // ─────────────────────────────────────────

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
