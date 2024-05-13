using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlogHub.Models;
using BlogHub.Helper;  // Ensure you include your model namespaces

namespace BlogHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<MyUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Custom DbSets
        public DbSet<Post> Posts { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensuring the CategoryName is used properly as a foreign key
         
            modelBuilder.Entity<Post>().
                Property(p => p.PostId).
                ValueGeneratedOnAdd();

            modelBuilder.Entity<Comment>()
               .HasOne(c => c.Post)
               .WithMany(p => p.Comments)
               .HasForeignKey(c => c.PostId)
               .HasPrincipalKey(p => p.PostId)
               .OnDelete(DeleteBehavior.ClientCascade);
        }

    }
}
