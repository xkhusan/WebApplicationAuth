using System.ComponentModel.DataAnnotations;

namespace WebApplication.ViewModels
{
    public class RegisterUserVM
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        [Required]
        public string EmailAddress { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string PassWord { get; set; } = null!;
    }
}
