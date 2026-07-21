using Microsoft.AspNetCore.Identity;

namespace WebApplicationAuth.Api.DataBase.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string? FirstName { get; set; } = null!;
        public required string? LastName { get; set; } = string.Empty;
        public string? Custom { get; set; } = string.Empty;
    }
}
