using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC_Tests.Database
{
    public class DefaultsTests
    {
        [Test]
        public void DefaultAdmin_ReturnsValidUserDto()
        {
            var defaults = new Defaults(Mock.Of<IVariables>());

            var actual = defaults.DefaultAdmin;

            actual.Should().NotBeNull();
        }

        [Test]
        public void DefaultTasks_ReturnsValidTaskCollection()
        {
            var defaults = new Defaults(Mock.Of<IVariables>());

            var actual = defaults.DefaultTasks;

            actual.Should().NotBeEmpty();
        }
    }
}
