using System.ComponentModel.DataAnnotations;

namespace Contact_Management_system.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [Required, StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;
        public string FullName => FirstName + " " + LastName;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public int ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;

    }
}
