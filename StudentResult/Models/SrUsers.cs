namespace StudentResult.Models
{
    public partial class SrUsers
    {
        public decimal Userid { get; set; }
        public string Username { get; set; }
        public string Useremail { get; set; }
        public string Userpassword { get; set; }
        public string Userrole { get; set; }  // "Admin", "Teacher", "Student"
    }
}
