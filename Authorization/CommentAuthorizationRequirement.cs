using Microsoft.AspNetCore.Authorization;

namespace BlogHub.Authorization
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
