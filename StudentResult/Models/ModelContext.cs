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
                // TODO: Replace with your Oracle connection string
                // Format: User Id=<user>;Password=<pass>;Data Source=<host>:<port>/<service>
                optionsBuilder.UseOracle("User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=localhost:1521/XE");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SrUsers>(entity =>
            {
                entity.HasKey(e => e.Userid);
                entity.ToTable("SR_USERS", "SR");
                entity.Property(e => e.Userid).HasColumnName("USERID");
                entity.Property(e => e.Username).HasColumnName("USERNAME").HasColumnType("varchar2").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Useremail).HasColumnName("USEREMAIL").HasColumnType("varchar2").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Userpassword).HasColumnName("USERPASSWORD").HasColumnType("varchar2").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Userrole).HasColumnName("USERROLE").HasColumnType("varchar2").HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<SrClasses>(entity =>
            {
                entity.HasKey(e => e.Classid);
                entity.ToTable("SR_CLASSES", "SR");
                entity.Property(e => e.Classid).HasColumnName("CLASSID");
                entity.Property(e => e.Classname).HasColumnName("CLASSNAME").HasColumnType("varchar2").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Section).HasColumnName("SECTION").HasColumnType("varchar2").HasMaxLength(10);
            });

            modelBuilder.Entity<SrSubjects>(entity =>
            {
                entity.HasKey(e => e.Subjectid);
                entity.ToTable("SR_SUBJECTS", "SR");
                entity.Property(e => e.Subjectid).HasColumnName("SUBJECTID");
                entity.Property(e => e.Subjectname).HasColumnName("SUBJECTNAME").HasColumnType("varchar2").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Classid).HasColumnName("CLASSID");
            });

            modelBuilder.Entity<SrStudents>(entity =>
            {
                entity.HasKey(e => e.Studentid);
                entity.ToTable("SR_STUDENTS", "SR");
                entity.Property(e => e.Studentid).HasColumnName("STUDENTID");
                entity.Property(e => e.Studentname).HasColumnName("STUDENTNAME").HasColumnType("varchar2").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Rollno).HasColumnName("ROLLNO").HasColumnType("varchar2").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Classid).HasColumnName("CLASSID");
                entity.Property(e => e.Gender).HasColumnName("GENDER").HasColumnType("varchar2").HasMaxLength(10);
                entity.Property(e => e.Parentname).HasColumnName("PARENTNAME").HasColumnType("varchar2").HasMaxLength(100);
                entity.Property(e => e.Contactno).HasColumnName("CONTACTNO").HasColumnType("varchar2").HasMaxLength(20);
                entity.Property(e => e.Userid).HasColumnName("USERID");
            });

            modelBuilder.Entity<SrMarks>(entity =>
            {
                entity.HasKey(e => e.Markid);
                entity.ToTable("SR_MARKS", "SR");
                entity.Property(e => e.Markid).HasColumnName("MARKID");
                entity.Property(e => e.Studentid).HasColumnName("STUDENTID");
                entity.Property(e => e.Subjectid).HasColumnName("SUBJECTID");
                entity.Property(e => e.Examtype).HasColumnName("EXAMTYPE").HasColumnType("varchar2").HasMaxLength(30);
                entity.Property(e => e.Marksscored).HasColumnName("MARKSSCORED").HasColumnType("NUMBER(5,2)");
                entity.Property(e => e.Totalmarks).HasColumnName("TOTALMARKS").HasColumnType("NUMBER(5,2)");
                entity.Property(e => e.Examdate).HasColumnName("EXAMDATE").HasColumnType("date");
            });

            modelBuilder.Entity<SrAttendance>(entity =>
            {
                entity.HasKey(e => e.Attendanceid);
                entity.ToTable("SR_ATTENDANCE", "SR");
                entity.Property(e => e.Attendanceid).HasColumnName("ATTENDANCEID");
                entity.Property(e => e.Studentid).HasColumnName("STUDENTID");
                entity.Property(e => e.Classid).HasColumnName("CLASSID");
                entity.Property(e => e.Attenddate).HasColumnName("ATTENDDATE").HasColumnType("date");
                entity.Property(e => e.Status).HasColumnName("STATUS").HasColumnType("varchar2").HasMaxLength(10);
            });
        }
    }
}
