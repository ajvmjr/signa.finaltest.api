using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Signa.TemplateCore.Api.ConfigurationsHelper
{
    // TODO: incluir em Signa.Library.Api
    internal class DecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal) || objectType == typeof(decimal?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            decimal? d = default(decimal?);
            string df = default(string);

            if (value != null)
            {
                d = value as decimal?;

                if (d.HasValue) // If value was a decimal?, then this is possible
                {
                    d = new decimal?(new decimal(decimal.ToDouble(d.Value))); // The ToDouble-conversion removes all unnessecary precision
                    df = string.Format("{0:N}", d);
                }
            }
            JToken.FromObject(df).WriteTo(writer);
        }
    }
}
