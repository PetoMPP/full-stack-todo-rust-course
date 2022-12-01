using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoAPI_MVC.Json
{
    public class DateTimeJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var type = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            return type == typeof(DateTime);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (Nullable.GetUnderlyingType(typeToConvert) is null)
                return new DateTimeConverterInner(options);

            return new DateTimeNullableConverterInner(options);
        }

        private class DateTimeNullableConverterInner : JsonConverter<DateTime?>
        {
            public DateTimeNullableConverterInner(JsonSerializerOptions? _ = null)
            {
            }

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
                    writer.WriteStringValue(dateTime.ToString(Consts.DateFormat));
                else
                    writer.WriteNullValue();
            }
        }

        private class DateTimeConverterInner : JsonConverter<DateTime>
        {
            public DateTimeConverterInner(JsonSerializerOptions? _ = null)
            {
            }

            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    if (reader.TokenType == JsonTokenType.String)
                        return DateTime.Parse(reader.GetString()!);
                }
                catch (Exception error)
                {
                    throw new JsonTokenException(
                        reader.TokenType, error, JsonTokenType.String);
                }

                throw new JsonTokenException(
                    reader.TokenType, null, JsonTokenType.String);
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Consts.DateFormat));
            }
        }
    }
}
