using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TodoAPI_MVC.Json;

namespace TodoAPI_MVC_Tests.Json
{
    public class DateTimeJsonConverterTests
    {
        [TestCase("\"2020-01-19 01:43:28.552 +06:00\"")]
        [TestCase("null")]
        public void ShouldConvertValues(string input)
        {
            var factory = new DateTimeJsonConverterFactory();
            var converter = (JsonConverter<DateTime?>)factory
                .CreateConverter(typeof(DateTime?), new JsonSerializerOptions())!;

            var utf8JsonReader = new Utf8JsonReader(
                new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(input)));

            var convertAction = (Utf8JsonReader reader) => converter.Read(
                ref reader, typeof(DateTime?), new JsonSerializerOptions());

            utf8JsonReader.Read();

            convertAction(utf8JsonReader);
        }
    }
}
