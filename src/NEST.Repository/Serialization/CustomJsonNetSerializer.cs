using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Repository.Serialization
{
    public class CustomJsonNetSerializer : ConnectionSettingsAwareSerializerBase
    {
        public CustomJsonNetSerializer(IElasticsearchSerializer builtinSerializer, IConnectionSettingsValues connectionSettings)
            : base(builtinSerializer, connectionSettings) { }

        //protected override IEnumerable<JsonConverter> CreateJsonConverters() =>
        //    Enumerable.Empty<JsonConverter>();

        protected override JsonSerializerSettings CreateJsonSerializerSettings() =>
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };

        //protected override void ModifyContractResolver(ConnectionSettingsAwareContractResolver resolver) =>
        //    resolver.NamingStrategy = new SnakeCaseNamingStrategy();
    }
}
