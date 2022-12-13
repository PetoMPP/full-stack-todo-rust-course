using TodoAPI_MVC.Database;
using TodoAPI_MVC.Database.Service;

namespace TodoAPI_MVC_Tests.Database
{
    public class DbConstraintTests
    {
        private record struct Test(int Number, string Text);
        private record struct TestNullable(int? Number, string? Text);

        [Test]
        public void ShouldBuildProperSqlString_FromNormalTypes()
        {
            var dbService = new DbService();
            var number = 69;
            var text = "Just bee yourself!";
            var test = new Test(number, text);
            var constraint = new DbConstraint(
                dbService,
                (Test t) =>
                t.Number == number &&
                t.Text != "text" ||
                t.Text == test.Text &&
                t.Number >= Math.Max(number, 85));

            var expected =
                "number = 69 AND " +
                "text != 'text' OR " +
                "text = 'Just bee yourself!' AND " +
                "number >= 85";

            constraint.ToSqlString().Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldBuildProperSqlString_FromNullableTypes()
        {
            var dbService = new DbService();
            var number = 69;
            var test = new TestNullable(number, null);
            var constraint = new DbConstraint(
                dbService,
                (TestNullable t) =>
                t.Number == number &&
                t.Text != null ||
                t.Text == test.Text &&
                t.Number <= Math.Max(number, 85));

            var expected =
                "number = 69 AND " +
                "text is not null OR " +
                "text is null AND " +
                "number <= 85";

            constraint.ToSqlString().Should().BeEquivalentTo(expected);
        }
    }
}
