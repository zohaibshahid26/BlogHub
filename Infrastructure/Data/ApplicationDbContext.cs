using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<MyUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            try
            {
                var databaseCreator = Database.GetService<IRelationalDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreator != null)
                {
                    if (!databaseCreator.CanConnect())
                    {
                        databaseCreator.Create();
                    }
                    if (!databaseCreator.HasTables())
                    {
                        databaseCreator.CreateTables();
                    }

                }

            }
            catch ( Exception ex)
            {
                throw new Exception("Error in creating database", ex);
            }
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Post>().
                Property(p => p.PostId).
                ValueGeneratedOnAdd();

            modelBuilder.Entity<Comment>()
               .HasOne(c => c.Post)
               .WithMany(p => p.Comments)
               .HasForeignKey(c => c.PostId)
               .HasPrincipalKey(p => p.PostId)
               .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .HasPrincipalKey(p => p.PostId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Image)
                .WithOne()
                .HasForeignKey<Post>(p => p.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.TagName)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();

            modelBuilder.Entity<MyUser>()
                .HasOne(u => u.Image)
                .WithOne()
                .HasForeignKey<MyUser>(u => u.ImageId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
