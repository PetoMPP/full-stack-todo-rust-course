using System.Linq.Expressions;
using System.Reflection;
using TodoAPI_MVC.Atributtes;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Database.Service;

namespace TodoAPI_MVC_Tests.Database
{
    public class DbConstraintTests
    {
        private delegate int TestHandler();
        private event TestHandler? TestEvent;

        private enum Testum
        {
            Yes,
            Maybe
        };

        private record struct Test(int Number, string Text, Testum Testum);
        private record struct TestIgnored([property: DbIgnore] int Number, string Text, Testum Testum);
        private record struct TestNullable(int? Number, string? Text, Testum? Testum);

        [Test]
        public void Ctor_ShouldBuildProperSqlString_FromNormalTypes()
        {
            var dbService = new DbService();
            var number = 69;
            var text = "Just bee yourself!";
            var testum = Testum.Yes;
            var test = new Test(number, text, testum);
            var constraint = new DbConstraint(
                dbService,
                (Test t) =>
                t.Number <= test.Number &&
                t.Text != "text" ||
                t.Testum > testum &&
                t.Number >= Math.Max(number, 85));

            var expected =
                "number <= 69 AND " +
                "text != 'text' OR " +
                "testum > 0 AND " +
                "number >= 85";

            constraint.ToSqlString().Should().Be(expected);
        }

        [Test]
        public void Ctor_ShouldBuildProperSqlString_FromNullableTypes()
        {
            var dbService = new DbService();
            var number = 69;
            var test = new TestNullable(number, null, null);
            var constraint = new DbConstraint(
                dbService,
                (TestNullable t) =>
                t.Number == GetNumber(number) &&
                t.Text != null ||
                t.Testum < Testum.Maybe &&
                t.Number == null);
            var expected =
                "number = 69 AND " +
                "text is not null OR " +
                "testum < 1 AND " +
                "number is null";

            string actual = constraint;

            actual.Should().Be(expected);
        }

        [Test]
        public void Ctor_ShouldThrowInvalidOperationException_OnInvalidExpressionBodyType()
        {
            var dbService = new DbService();

            var ctorAction = () => new DbConstraint(dbService, (Test t) => t.Number);

            ctorAction.Should().ThrowExactly<InvalidOperationException>();
        }

        [Test]
        public void Ctor_ShouldThrowNotSupportedException_OnInvalidOperator()
        {
            var dbService = new DbService();

            var ctorAction = () => new DbConstraint(dbService, (Test t) => t.Number | 0);

            ctorAction.Should().ThrowExactly<NotSupportedException>();
        }

        [Test]
        public void Ctor_ShouldThrowNotSupportedException_OnInvalidLeftExpression()
        {
            var dbService = new DbService();

            var ctorAction = () => new DbConstraint(dbService, (Test t) => true && t.Number == 0);

            ctorAction.Should().ThrowExactly<NotSupportedException>();
        }

        [Test]
        public void Ctor_ShouldThrowInvalidOperationException_OnDbIgnoreAttribute()
        {
            var dbService = new DbService();

            var ctorAction = () => new DbConstraint(dbService, (TestIgnored t) => t.Number == 0);

            ctorAction.Should().ThrowExactly<InvalidOperationException>();
        }

        private static int GetNumber(int number)
        {
            return number;
        }
    }
}
