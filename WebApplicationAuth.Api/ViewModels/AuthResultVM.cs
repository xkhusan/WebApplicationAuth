namespace WebApplicationAuth.Api.ViewModels
{
    public class AuthResultVM
    {
        public required string Token { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
