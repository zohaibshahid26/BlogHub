using Microsoft.AspNetCore.Identity;
namespace Domain.Entities
{
    public class MyUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public int? ImageId { get; set; }
        public Image? Image { get; set; }
    }
}
