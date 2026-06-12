using System;

namespace StudentResult.Models
{
    public partial class SrAttendance
    {
        public int Attendanceid { get; set; }
        public int? Studentid { get; set; }
        public int? Classid { get; set; }
        public DateTime? Attenddate { get; set; }
        public string Status { get; set; }   // "Present", "Absent", "Leave"
    }
}
