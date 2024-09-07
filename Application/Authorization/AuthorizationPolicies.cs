using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Application.Authorization
{
    public static class AuthorizationPolicies
    {
        public static void AddCustomAuthorizationPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Email, "admin@bloghub.com"));
            options.AddPolicy("EditPostPolicy", policy => policy.Requirements.Add(new PostAuthorizationRequirement("Edit")));
            options.AddPolicy("DeletePostPolicy", policy => policy.Requirements.Add(new PostAuthorizationRequirement("Delete")));
            options.AddPolicy("EditCommentPolicy", policy => policy.Requirements.Add(new CommentAuthorizationRequirement("Edit")));
            options.AddPolicy("DeleteCommentPolicy", policy => policy.Requirements.Add(new CommentAuthorizationRequirement("Delete")));
        }
    }
}
