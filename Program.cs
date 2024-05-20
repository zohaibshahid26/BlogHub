using BlogHub.Data;
using BlogHub.Repository;
using BlogHub.Helper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BlogHub.Authorization;
using BlogHub.Models;
using BlogHub.UnitofWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<MyUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddSingleton<IAuthorizationHandler, PostAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentAuthorizationHandler>();
builder.Services.AddSingleton(builder.Environment);


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Email, "admin@bloghub.com"));
    options.AddPolicy("EditPostPolicy", policy => policy.Requirements.Add(new PostAuthorizationRequirement("Edit")));
    options.AddPolicy("DeletePostPolicy", policy => policy.Requirements.Add(new PostAuthorizationRequirement("Delete")));
    options.AddPolicy("EditCommentPolicy", policy => policy.Requirements.Add(new CommentAuthorizationRequirement("Edit")));
    options.AddPolicy("DeleteCommentPolicy", policy => policy.Requirements.Add(new CommentAuthorizationRequirement("Delete")));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
