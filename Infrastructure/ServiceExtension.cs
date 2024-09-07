using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)
            );

            services.AddDefaultIdentity<MyUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddSingleton(environment);
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            return services;
        }
        public static IServiceCollection AddInfrastructureApi(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)
            );

            services.AddIdentityApiEndpoints<MyUser>().
                AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddSingleton(environment);
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            return services;

        }
    }
}
