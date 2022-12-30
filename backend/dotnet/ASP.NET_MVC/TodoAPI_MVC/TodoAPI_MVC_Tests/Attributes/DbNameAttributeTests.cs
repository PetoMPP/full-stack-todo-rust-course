using TodoAPI_MVC.Atributtes;

namespace TodoAPI_MVC_Tests.Attributes
{
    internal class DbNameAttributeTests
    {
        [Test]
        public void Ctor_ShouldAssignCorrectPropertyName_OnValidColumnName()
        {
            var columnName = "propName";

            var attribute = new DbNameAttribute(columnName);

            attribute.ColumnName.Should().Be(columnName);
        }

        [Test]
        public void Ctor_ShouldThrowArgumentException_OnInvalidColumnName()
        {
            var columnName = "";

            var ctorAction = () => new DbNameAttribute(columnName);

            ctorAction.Should().ThrowExactly<ArgumentException>();
        }
    }
}
