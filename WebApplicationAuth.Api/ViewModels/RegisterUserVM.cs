using System.ComponentModel.DataAnnotations;

namespace WebApplicationAuth.Api.ViewModels
{
    public class RegisterUserVM
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = string.Empty;
        [Required]
        public required string EmailAddress { get; set; }
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string PassWord { get; set; }
    }
}
