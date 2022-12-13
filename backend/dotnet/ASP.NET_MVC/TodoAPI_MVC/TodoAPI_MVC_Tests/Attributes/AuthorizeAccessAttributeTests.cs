using TodoAPI_MVC.Atributtes;
using TodoAPI_MVC.Authentication;

namespace TodoAPI_MVC_Tests.Attributes
{
    public class AuthorizeAccessAttributeTests
    {
        [Test]
        public void Ctor_ShouldAssignCorrectPolicy_OnDefinedAccess()
        {
            var access = EndpointAccess.TasksOwned;

            var attribute = new AuthorizeAccessAttribute(access);

            attribute.Policy.Should().Be($"{access}");
        }

        [Test]
        public void Ctor_ShouldThrowArgumentException_OnUndefinedAccess()
        {
            var access = (EndpointAccess)999;

            var ctorAction = () => new AuthorizeAccessAttribute(access);

            ctorAction.Should().ThrowExactly<ArgumentException>();
        }
    }
}
