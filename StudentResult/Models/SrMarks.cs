using System;

namespace StudentResult.Models
{
    public partial class SrMarks
    {
        public int Markid { get; set; }
        public int? Studentid { get; set; }
        public int? Subjectid { get; set; }
        public string Examtype { get; set; }      // "Unit Test", "Mid Term", "Final"
        public decimal? Marksscored { get; set; }
        public decimal? Totalmarks { get; set; }
        public DateTime? Examdate { get; set; }
    }
}
