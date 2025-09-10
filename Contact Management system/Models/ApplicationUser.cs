using System.ComponentModel.DataAnnotations;

namespace Contact_Management_system.Models
{
    public class ApplicationUser
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public int Id { get; set; }
        [Required, StringLength(50, MinimumLength = 2)]
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public HashSet<Contact> Contacts { get; set; } = new HashSet<Contact>();

    }
}
