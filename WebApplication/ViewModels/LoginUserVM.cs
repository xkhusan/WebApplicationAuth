using System.ComponentModel.DataAnnotations;

namespace WebApplication.ViewModels
{
    public class LoginUserVM
    {
        [Required]
        public string EmailAddress { get; set; } = null!;

        [Required]
        public string PassWord { get; set; } = null!;
    }
}
