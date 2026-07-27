namespace WebApplicationAuth.Api.ViewModels
{
    public class AuthResultVM
    {
        public required string AccessToken { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
