using Microsoft.AspNetCore.Authorization;

namespace BlogHub.Authorization
{
    public class PostAuthorizationRequirement : IAuthorizationRequirement
    {
       
        public PostAuthorizationRequirement(string operationName)
        {
            OperationName = operationName;
        }
        public string OperationName { get; }
    }
}
