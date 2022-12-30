using TodoAPI_MVC.Atributtes;
using TodoAPI_MVC.Database.Service;

namespace TodoAPI_MVC_Tests.Attributes
{
    public class DbValueConverterAttributeTests
    {
        private abstract class TestConverter : DbValueConverter
        {
        }

        [Test]
        public void Ctor_ShouldAcceptConverterType_WhenInhereitsFromDbValueConverter()
        {
            var testType = typeof(TestConverter);

            var attribute = new DbValueConverterAttribute(testType);

            attribute.ConverterType.Should().Be(typeof(TestConverter));
        }

        [Test]
        public void Ctor_ShouldThrowOnConverterType_WhenDoesntInhereitsFromDbValueConverter()
        {
            var testType = typeof(object);
            
            var ctorAction = () => new DbValueConverterAttribute(testType);

            ctorAction.Should().ThrowExactly<ArgumentException>();
        }
    }
}
