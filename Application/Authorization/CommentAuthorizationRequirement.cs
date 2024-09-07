using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization
{
    public class CommentAuthorizationRequirement : IAuthorizationRequirement
    {

        public CommentAuthorizationRequirement(string operationName)
        {
            OperationName = operationName;
        }
        public string OperationName { get; }
    }
}
