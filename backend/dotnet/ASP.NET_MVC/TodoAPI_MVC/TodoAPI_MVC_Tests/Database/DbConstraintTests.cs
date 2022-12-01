using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC_Tests.Database
{
    public class DbConstraintTests
    {
        [Test]
        public void ShouldBuildProperSqlString()
        {
            var taskTitle = "AAAAAAAAAAAAAAAA!!!!!!!!";
            var testTask = new TodoTask
            {
                Id = 5,
                Title = "Tit",
                Priority = null,
                CreatedAt = DateTime.Now,
                CompletedAt = null,
                Description = null,
                UserId = 1
            };

            var builder = new DbConstraint(
                (TodoTask t) => t.Id == testTask.Id && t.UserId == testTask.UserId || t.Title == taskTitle);
            var expected = $"Id = 5 AND user_id = 1 OR title = '{taskTitle}'";

            builder.ToSqlString().Should().BeEquivalentTo(expected);
        }
    }
}
