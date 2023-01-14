using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoAPI_MVC.Controllers;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC_Tests.Controllers
{
    public class TasksControllerTests
    {
        private static readonly TodoTask TodoTask = new()
        {
            Id = 1,
            Title = "NO_TITLE",
            CreatedAt = DateTime.ParseExact("10.10.2010", "dd.MM.yyyy", null),
            UserId = 1
        };

        [Test]
        public async Task Create_ShouldSucceed_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.Create(
                TodoTask, false, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeEquivalentTo(TodoTask);
        }

        [Test]
        public async Task Create_ShouldFail_OnInvalidUser()
        {
            var controller = GetController(
                taskDataMock: GetTaskDataMock(StatusCode.Error));

            var actual = (ObjectResult)await controller.Create(
                TodoTask, false, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeEquivalentTo(TestHelper.Error);
        }

        [Test]
        public async Task Get_ShouldSucceed_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.Get(1, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeEquivalentTo(TodoTask);
        }

        [Test]
        public async Task Get_ShouldFail_OnInvalidDatabaseResult()
        {
            var controller = GetController(
                taskDataMock: GetTaskDataMock((StatusCode)9999, true));

            var actual = (ObjectResult)await controller.Get(1, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(500, 599);
            actual.Value.Should().Be("");
        }

        [Test]
        public async Task GetAllOwned_ShouldSucceed_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.GetAllOwned(CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeEquivalentTo(new[] { TodoTask });
        }

        [Test]
        public async Task GetAllOwned_ShouldFail_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.GetAllOwned(CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeEquivalentTo(new[] { TodoTask });
        }

        [Test]
        public async Task Update_ShouldSucceed_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.Update(1, TodoTask, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeEquivalentTo(TodoTask);
        }

        [Test]
        public async Task ToggleCompleted_ShouldSucceed_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.ToggleCompleted(1, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeEquivalentTo(TodoTask);
        }

        [Test]
        public async Task Delete_ShouldSucceed_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (StatusCodeResult)await controller.Delete(1, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
        }

        [Test]
        public async Task Delete_ShouldFail_OnInvalidUser()
        {
            var controller = GetController(
                taskDataMock: GetTaskDataMock(StatusCode.Error));

            var actual = (ObjectResult)await controller.Delete(1, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeEquivalentTo(TestHelper.Error);
        }

        [Test]
        public async Task Delete_ShouldFail_OnInvalidDatabaseResult()
        {
            var controller = GetController(
                taskDataMock: GetTaskDataMock((StatusCode)9999, true),
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.Delete(1, CancellationToken.None);

            actual.StatusCode.Should().BeInRange(500, 599);
            actual.Value.Should().BeEquivalentTo("");
        }

        [Test]
        public async Task GetAll_ShouldSucceed_OnValidUser()
        {
            var controller = GetController(
                claims: new[] { new Claim("Id", $"{TodoTask.Id}") });

            var actual = (ObjectResult)await controller.GetAll(CancellationToken.None);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeEquivalentTo(new[] { TodoTask });
        }

        private static TasksController GetController(
            Mock<ITaskData>? taskDataMock = null,
            Mock<UserManager<User>>? userManagerMock = null,
            Mock<SignInManager<User>>? signInManagerMock = null,
            IEnumerable<Claim>? claims = null)
        {
            taskDataMock ??= GetTaskDataMock();
            userManagerMock ??= TestHelper.GetUserManagerMock();
            signInManagerMock ??= TestHelper.GetSignInManagerMock(userManagerMock.Object);
            return new TasksController(
                            taskDataMock.Object,
                            userManagerMock.Object,
                            signInManagerMock.Object)
            {
                ControllerContext = TestHelper.GetControllerContext(claims)
            };
        }

        private static Mock<ITaskData> GetTaskDataMock(
            StatusCode returnedStatus = StatusCode.Ok,
            bool isErrorNull = false)
        {
            var mock = new Mock<ITaskData>();
            mock.Setup(m => m.CreateAsync(
                    It.IsAny<TodoTask>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(returnedStatus, isErrorNull, TodoTask));

            mock.Setup(m => m.GetAsync(
                    It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(returnedStatus, isErrorNull, TodoTask));

            mock.Setup(m => m.GetAllOwnedAsync(
                    It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(returnedStatus, isErrorNull, new[] { TodoTask }));

            mock.Setup(m => m.UpdateAsync(
                    It.IsAny<int>(), It.IsAny<TodoTask>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(returnedStatus, isErrorNull, TodoTask));

            mock.Setup(m => m.ToggleCompletedAsync(
                    It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(returnedStatus, isErrorNull, TodoTask));

            mock.Setup(m => m.DeleteAsync(
                    It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(returnedStatus, isErrorNull));

            mock.Setup(m => m.GetAllAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(returnedStatus, isErrorNull, new[] { TodoTask }));

            return mock;
        }
    }
}
