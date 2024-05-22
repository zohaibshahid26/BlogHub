using Microsoft.AspNetCore.Identity;

namespace BlogHub.Helper
{
    public class MyUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
