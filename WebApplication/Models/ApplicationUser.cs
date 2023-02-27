using Microsoft.AspNetCore.Identity;

namespace SeniorProject.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string FirstName { get; set; }   
        public string LastName { get; set; }   
        public int faculty_id { get; set; }   
        public int branch_id { get; set; }   
        public int prefix_id { get; set; }   
    }
}
