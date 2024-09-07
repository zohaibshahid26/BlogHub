using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization
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
