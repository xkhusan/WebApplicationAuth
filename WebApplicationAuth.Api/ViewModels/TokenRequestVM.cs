using System.ComponentModel.DataAnnotations;

namespace WebApplicationAuth.Api.ViewModels
{
    public class TokenRequestVM
    {
        [Required]
        public string AccessToken { get; set; } = null!;
        [Required]
        public string? RefreshToken { get; set; }
    }
}
