using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Authentication.Handlers;

namespace TodoAPI_MVC_Tests.Authentication.Handlers
{
    public class AccessHandlerTests : AccessHandler
    {
        public AccessHandlerTests() : base(Mock.Of<ILogger<AccessHandler>>())
        {
        }

        [Test]
        public async Task HandleRequirement_ShouldSucceed_OnValidAccess()
        {
            var access = EndpointAccess.TasksOwned;
            var requirement = new AccessRequirement(access);
            var userClaims = new List<Claim>() { new Claim("Access", $"{(int)access}")};
            var context = TestsHelper.GetAuthorizationHandlerContext(
                requirement, userClaims, true);

            await HandleRequirementAsync(context, requirement);

            context.HasSucceeded.Should().BeTrue();
        }

        [Test]
        public async Task HandleRequirement_ShouldFailAndCreateErrorHeader_OnAccessMissingAndHttpContextAvailable()
        {
            var access = EndpointAccess.TasksOwned;
            var requirement = new AccessRequirement(access);
            var httpContext = new DefaultHttpContext();
            var context = TestsHelper.GetAuthorizationHandlerContext(
                requirement, isAuthenticated: true, resource: httpContext);

            await HandleRequirementAsync(context, requirement);

            context.HasSucceeded.Should().BeFalse();
            context.HasFailed.Should().BeTrue();
            context.FailureReasons.Should().Contain(r => r.Message == AccessMissingMessage);
            httpContext = (DefaultHttpContext)context.Resource!;
            httpContext.Response.Headers
                .Should().Contain(kv => kv.Value == new StringValues(AccessMissingMessage));
        }

        [Test]
        public async Task HandleRequirement_ShouldFail_OnAccessUnmet()
        {
            var userAccess = EndpointAccess.TasksOwned;
            var userClaims = new List<Claim>() { new Claim("Access", $"{(int)userAccess}")};
            var requiredAccess = EndpointAccess.TasksAll;
            var requirement = new AccessRequirement(requiredAccess);
            var context = TestsHelper.GetAuthorizationHandlerContext(
                requirement, userClaims, false);

            await HandleRequirementAsync(context, requirement);

            context.HasSucceeded.Should().BeFalse();
            context.HasFailed.Should().BeTrue();
            context.FailureReasons.Should().Contain(r => r.Message == AccessInvalidMessage);
        }

        [Test]
        public async Task HandleRequirement_ShouldFail_OnAccessUnparsable()
        {
            var access = EndpointAccess.TasksOwned;
            var requirement = new AccessRequirement(access);
            var userClaims = new List<Claim>() { new Claim("Access", $"{access}") };
            var context = TestsHelper.GetAuthorizationHandlerContext(
                requirement, userClaims, true);

            await HandleRequirementAsync(context, requirement);

            context.HasSucceeded.Should().BeFalse();
            context.HasFailed.Should().BeTrue();
            context.FailureReasons.Should().Contain(r => r.Message == AccessUnparsableMessage);
        }
    }
}
