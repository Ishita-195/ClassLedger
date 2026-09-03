using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace StudentResult.Models
{
    public partial class ModelContext : DbContext
    {
        public ModelContext() { }

        public ModelContext(DbContextOptions<ModelContext> options) : base(options) { }

        public virtual DbSet<SrUsers>      SrUsers      { get; set; }
        public virtual DbSet<SrClasses>    SrClasses    { get; set; }
        public virtual DbSet<SrSubjects>   SrSubjects   { get; set; }
        public virtual DbSet<SrStudents>   SrStudents   { get; set; }
        public virtual DbSet<SrMarks>      SrMarks      { get; set; }
        public virtual DbSet<SrAttendance> SrAttendance { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // SQLite: a single self-contained file, no DB server required.
                // Path is overridable via DB_PATH (used by the container host);
                // defaults to a file next to the app.
                var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
                if (string.IsNullOrWhiteSpace(dbPath))
                    dbPath = "studentresult.db";
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SrUsers>(entity =>
            {
                entity.HasKey(e => e.Userid);
                entity.ToTable("SR_USERS");
                entity.Property(e => e.Userid).HasColumnName("USERID").ValueGeneratedNever();
                entity.Property(e => e.Username).HasColumnName("USERNAME").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Useremail).HasColumnName("USEREMAIL").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Userpassword).HasColumnName("USERPASSWORD").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Userrole).HasColumnName("USERROLE").HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<SrClasses>(entity =>
            {
                entity.HasKey(e => e.Classid);
                entity.ToTable("SR_CLASSES");
                entity.Property(e => e.Classid).HasColumnName("CLASSID").ValueGeneratedNever();
                entity.Property(e => e.Classname).HasColumnName("CLASSNAME").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Section).HasColumnName("SECTION").HasMaxLength(10);
            });

            modelBuilder.Entity<SrSubjects>(entity =>
            {
                entity.HasKey(e => e.Subjectid);
                entity.ToTable("SR_SUBJECTS");
                entity.Property(e => e.Subjectid).HasColumnName("SUBJECTID").ValueGeneratedNever();
                entity.Property(e => e.Subjectname).HasColumnName("SUBJECTNAME").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Classid).HasColumnName("CLASSID");
            });

            modelBuilder.Entity<SrStudents>(entity =>
            {
                entity.HasKey(e => e.Studentid);
                entity.ToTable("SR_STUDENTS");
                entity.Property(e => e.Studentid).HasColumnName("STUDENTID").ValueGeneratedNever();
                entity.Property(e => e.Studentname).HasColumnName("STUDENTNAME").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Rollno).HasColumnName("ROLLNO").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Classid).HasColumnName("CLASSID");
                entity.Property(e => e.Gender).HasColumnName("GENDER").HasMaxLength(10);
                entity.Property(e => e.Parentname).HasColumnName("PARENTNAME").HasMaxLength(100);
                entity.Property(e => e.Contactno).HasColumnName("CONTACTNO").HasMaxLength(20);
                entity.Property(e => e.Userid).HasColumnName("USERID");
            });

            modelBuilder.Entity<SrMarks>(entity =>
            {
                entity.HasKey(e => e.Markid);
                entity.ToTable("SR_MARKS");
                entity.Property(e => e.Markid).HasColumnName("MARKID").ValueGeneratedNever();
                entity.Property(e => e.Studentid).HasColumnName("STUDENTID");
                entity.Property(e => e.Subjectid).HasColumnName("SUBJECTID");
                entity.Property(e => e.Examtype).HasColumnName("EXAMTYPE").HasMaxLength(30);
                entity.Property(e => e.Marksscored).HasColumnName("MARKSSCORED");
                entity.Property(e => e.Totalmarks).HasColumnName("TOTALMARKS");
                entity.Property(e => e.Examdate).HasColumnName("EXAMDATE");
            });

            modelBuilder.Entity<SrAttendance>(entity =>
            {
                entity.HasKey(e => e.Attendanceid);
                entity.ToTable("SR_ATTENDANCE");
                entity.Property(e => e.Attendanceid).HasColumnName("ATTENDANCEID").ValueGeneratedNever();
                entity.Property(e => e.Studentid).HasColumnName("STUDENTID");
                entity.Property(e => e.Classid).HasColumnName("CLASSID");
                entity.Property(e => e.Attenddate).HasColumnName("ATTENDDATE");
                entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(10);
            });
        }

        // ─────────────────────────────────────────────────────────────
        // First-run setup: create the SQLite schema and seed the sample
        // data from schema.sql. Safe to call on every startup — it only
        // seeds when the tables are empty.
        // ─────────────────────────────────────────────────────────────
        public void EnsureSeeded()
        {
            Database.EnsureCreated();

            if (SrUsers.Any())
                return;

            SrUsers.AddRange(
                new SrUsers { Userid = 1, Username = "Admin",       Useremail = "admin@school.com",  Userpassword = "admin123",   Userrole = "Admin" },
                new SrUsers { Userid = 2, Username = "Mr. Sharma",  Useremail = "sharma@school.com", Userpassword = "teacher123", Userrole = "Teacher" },
                new SrUsers { Userid = 3, Username = "Ravi Kumar",  Useremail = "ravi@student.com",  Userpassword = "ravi123",    Userrole = "Student" },
                new SrUsers { Userid = 4, Username = "Priya Singh", Useremail = "priya@student.com", Userpassword = "priya123",   Userrole = "Student" }
            );

            SrClasses.AddRange(
                new SrClasses { Classid = 1, Classname = "10th", Section = "A" },
                new SrClasses { Classid = 2, Classname = "10th", Section = "B" },
                new SrClasses { Classid = 3, Classname = "9th",  Section = "A" }
            );

            SrSubjects.AddRange(
                new SrSubjects { Subjectid = 1, Subjectname = "Mathematics", Classid = 1 },
                new SrSubjects { Subjectid = 2, Subjectname = "Science",     Classid = 1 },
                new SrSubjects { Subjectid = 3, Subjectname = "English",     Classid = 1 },
                new SrSubjects { Subjectid = 4, Subjectname = "Mathematics", Classid = 3 }
            );

            SrStudents.AddRange(
                new SrStudents { Studentid = 1, Studentname = "Ravi Kumar",  Rollno = "R001", Classid = 1, Gender = "Male",   Parentname = "Suresh Kumar", Contactno = "9876543210", Userid = 3 },
                new SrStudents { Studentid = 2, Studentname = "Priya Singh", Rollno = "R002", Classid = 1, Gender = "Female", Parentname = "Ramesh Singh", Contactno = "9876543211", Userid = 4 }
            );

            SrMarks.AddRange(
                new SrMarks { Markid = 1, Studentid = 1, Subjectid = 1, Examtype = "Mid Term", Marksscored = 78, Totalmarks = 100, Examdate = new DateTime(2024, 10, 10) },
                new SrMarks { Markid = 2, Studentid = 1, Subjectid = 2, Examtype = "Mid Term", Marksscored = 85, Totalmarks = 100, Examdate = new DateTime(2024, 10, 11) },
                new SrMarks { Markid = 3, Studentid = 1, Subjectid = 3, Examtype = "Mid Term", Marksscored = 70, Totalmarks = 100, Examdate = new DateTime(2024, 10, 12) },
                new SrMarks { Markid = 4, Studentid = 2, Subjectid = 1, Examtype = "Mid Term", Marksscored = 90, Totalmarks = 100, Examdate = new DateTime(2024, 10, 10) },
                new SrMarks { Markid = 5, Studentid = 2, Subjectid = 2, Examtype = "Mid Term", Marksscored = 88, Totalmarks = 100, Examdate = new DateTime(2024, 10, 11) }
            );

            SrAttendance.AddRange(
                new SrAttendance { Attendanceid = 1, Studentid = 1, Classid = 1, Attenddate = new DateTime(2024, 11, 1), Status = "Present" },
                new SrAttendance { Attendanceid = 2, Studentid = 1, Classid = 1, Attenddate = new DateTime(2024, 11, 2), Status = "Absent" },
                new SrAttendance { Attendanceid = 3, Studentid = 2, Classid = 1, Attenddate = new DateTime(2024, 11, 1), Status = "Present" },
                new SrAttendance { Attendanceid = 4, Studentid = 2, Classid = 1, Attenddate = new DateTime(2024, 11, 2), Status = "Present" }
            );

            SaveChanges();
        }
    }
}
