using Microsoft.AspNetCore.Identity;

namespace PowerFIt.Models
{
    public class Customer : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public DateTime RegOn { get; set; }
        public ICollection<Order> Orders { get; set; } // 1:M
    }
}
