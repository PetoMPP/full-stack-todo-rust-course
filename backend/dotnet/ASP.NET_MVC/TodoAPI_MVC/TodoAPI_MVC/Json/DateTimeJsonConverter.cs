using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoAPI_MVC.Json
{
    public class DateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType == JsonTokenType.String)
                    return DateTime.Parse(reader.GetString()!);

                if (reader.TokenType == JsonTokenType.Null)
                    return null;
            }
            catch (Exception error)
            {
                throw new JsonTokenException(
                    reader.TokenType, error, JsonTokenType.String, JsonTokenType.Null);
            }

            throw new JsonTokenException(
                reader.TokenType, null, JsonTokenType.String, JsonTokenType.Null);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value is DateTime dateTime)
                writer.WriteStringValue(dateTime);
            else
                writer.WriteNullValue();
        }
    }
}