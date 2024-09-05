using Web.Authorization;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Ganss.Xss;
using Infrastructure;
using Application.Services;
using Infrastructure.UnitOfWork;
using Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("ClientId not found");
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("ClientSecret not found");
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddInfrastructure(builder.Configuration,builder.Environment);
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();

builder.Services.AddSingleton<IAuthorizationHandler, PostAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentAuthorizationHandler>();
builder.Services.AddSingleton<HtmlSanitizer>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Email, "admin@bloghub.com"));
    options.AddPolicy("EditPostPolicy", policy => policy.Requirements.Add(new PostAuthorizationRequirement("Edit")));
    options.AddPolicy("DeletePostPolicy", policy => policy.Requirements.Add(new PostAuthorizationRequirement("Delete")));
    options.AddPolicy("EditCommentPolicy", policy => policy.Requirements.Add(new CommentAuthorizationRequirement("Edit")));
    options.AddPolicy("DeleteCommentPolicy", policy => policy.Requirements.Add(new CommentAuthorizationRequirement("Delete")));
});

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ChatHub>("/ChatHub");
app.Run();