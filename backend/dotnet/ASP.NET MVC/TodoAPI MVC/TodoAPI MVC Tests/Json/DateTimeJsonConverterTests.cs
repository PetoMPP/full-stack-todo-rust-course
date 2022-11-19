using FluentAssertions;
using System.Text;
using System.Text.Json;
using TodoAPI_MVC.Json;

namespace TodoAPI_MVC_Tests.Json
{
    public class DateTimeJsonConverterTests
    {
        [TestCase("\"2020-01-19 01:43:28.552 +06:00\"")]
        [TestCase("null")]
        public void should_convert(string input)
        {
            var converter = new DateTimeJsonConverter();
            var utf8JsonReader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(input)));
            var convertAction = (Utf8JsonReader reader) => converter.Read(ref reader, typeof(DateTime?), new JsonSerializerOptions());
            
            utf8JsonReader.Read();

            convertAction(utf8JsonReader);
        }
    }
}
