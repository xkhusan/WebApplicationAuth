using System.ComponentModel.DataAnnotations;

namespace WebApplicationAuth.Api.ViewModels
{
    public class LoginUserVM
    {
        [Required]
        public required string EmailAddress { get; set; }
        [Required]
        public required string PassWord { get; set; }
    }
}
