using BlogHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace BlogHub.Authorization
{
    public class CommentAuthorizationHandler : AuthorizationHandler<CommentAuthorizationRequirement, Comment>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CommentAuthorizationRequirement requirement, Comment resource)
        {
            if (requirement.OperationName == "Edit")
            {
                if (context.User.Identity?.Name == resource.User?.UserName)
                {
                    context.Succeed(requirement);
                }
            }
            if (requirement.OperationName == "Delete")
            {
                if (context.User.Identity?.Name == resource.User?.UserName || context.User.Identity?.Name == resource.Post!.User?.UserName)
                {
                    context.Succeed(requirement);

                }
            }

            return Task.CompletedTask;
        }
    }
}
