﻿using BlogHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace BlogHub.Authorization
{
    public class PostAuthorizationHandler : AuthorizationHandler<PostAuthorizationRequirement, Post>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PostAuthorizationRequirement requirement, Post resource)
        {
            if (requirement.OperationName == "Edit" || requirement.OperationName == "Delete")
            {
                if (context.User.Identity?.Name == resource.User.UserName)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
