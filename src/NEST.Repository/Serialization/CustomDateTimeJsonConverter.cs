using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Repository.Serialization
{
    public class CustomDateTimeJsonConverter : IsoDateTimeConverter
    {
        //public override bool CanConvert(Type objectType) => objectType == typeof(DateTime) || objectType == typeof(DateTime?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null: return null;
                case JsonToken.Date:
                    var dateTime = ((DateTime)reader.Value).ToUniversalTime();
                    if (dateTime == DateTime.MinValue)
                    {
                        return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local);
                    }
                    else if (dateTime == DateTime.MaxValue)
                    {
                        return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Local);
                    }
                    else
                    {
                        return dateTime.ToLocalTime();
                    }
            }
            throw new JsonSerializationException($"Cannot convert token of type {reader.TokenType} to {objectType}.");
        }

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    if (value == null)
        //        writer.WriteNull();
        //    else
        //    {
        //        var dateTime = (DateTime)value;
        //        writer.WriteValue(dateTime.ToUniversalTime().ToString());
        //    }
        //}
    }
}
