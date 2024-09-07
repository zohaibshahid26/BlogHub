using Application.Authorization;
using Ganss.Xss;
using Infrastructure;
using Web.Hubs;
using Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("ClientId not found");
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("ClientSecret not found");
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services
       .AddInfrastructure(builder.Configuration, builder.Environment)
       .AddApplication();


builder.Services.AddSingleton<HtmlSanitizer>();
builder.Services.AddAuthorization(options =>
{
   options.AddCustomAuthorizationPolicies();
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