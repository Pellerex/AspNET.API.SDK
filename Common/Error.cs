using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Common
{
    public class Error
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int StatusCode { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }
}