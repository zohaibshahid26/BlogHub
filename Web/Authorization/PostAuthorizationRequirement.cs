using Microsoft.AspNetCore.Authorization;

namespace Web.Authorization
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
