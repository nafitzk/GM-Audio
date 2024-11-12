using Gma;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core
{
    public class GmaReferenceResolver : ReferenceResolver
    {
        public override void AddReference(string referenceId, object value)
        {
            throw new NotImplementedException();
        }

        public override string GetReference(object value, out bool alreadyExists)
        {
            throw new NotImplementedException();
        }

        public override object ResolveReference(string referenceId)
        {
            throw new NotImplementedException();
        }
    }

    //public class JsonAudioConverter : JsonConverter
    //{

    //    public override Type? Type => throw new NotImplementedException();

    //    public override bool CanConvert(Type typeToConvert)
    //    {
    //        return typeof(GmaBaseAudio).IsAssignableFrom(typeToConvert);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        JObject
    //    }
}
