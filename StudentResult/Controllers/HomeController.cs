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
        // MARKS / RESULTS
        // ─────────────────────────────────────────

        public IActionResult Marks()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            StudentResultVM obj = new StudentResultVM();
            obj.AllMarks    = _context.SrMarks.OrderByDescending(m => m.Markid).ToList();
            obj.AllStudents = _context.SrStudents.ToList();
            obj.AllSubjects = _context.SrSubjects.ToList();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMarks(StudentResultVM model)
        {
            var m = model.SrMarksObj;
            var maxId = _context.SrMarks.Select(x => x.Markid).DefaultIfEmpty(0).Max();
            m.Markid    = maxId + 1;
            m.Examdate  = DateTime.Now;
            _context.Add(m);
            _context.SaveChanges();
            return RedirectToAction("Marks");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMark(int id)
        {
            var mark = _context.SrMarks.Find(id);
            if (mark != null)
            {
                _context.SrMarks.Remove(mark);
                _context.SaveChanges();
            }
            return RedirectToAction("Marks");
        }

        // ─────────────────────────────────────────
        // ATTENDANCE
        // ─────────────────────────────────────────

        public IActionResult Attendance()
        {
            if (HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login");

            StudentResultVM obj = new StudentResultVM();
            obj.AllAttendance = _context.SrAttendance.OrderByDescending(a => a.Attenddate).ToList();
            obj.AllStudents   = _context.SrStudents.ToList();
            obj.AllClasses    = _context.SrClasses.ToList();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAttendance(StudentResultVM model)
        {
            var a = model.SrAttendanceObj;
            var maxId = _context.SrAttendance.Select(x => x.Attendanceid).DefaultIfEmpty(0).Max();
            a.Attendanceid = maxId + 1;
            a.Attenddate   = DateTime.Now;
            _context.Add(a);
            _context.SaveChanges();
            return RedirectToAction("Attendance");
        }

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
