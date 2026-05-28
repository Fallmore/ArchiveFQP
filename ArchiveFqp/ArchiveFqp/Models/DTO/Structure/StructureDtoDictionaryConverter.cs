using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ArchiveFqp.Models.DTO.Structure
{
    public class StructureDtoDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<StructureDto, List<StructureDto>>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<StructureDto, List<StructureDto>>();
            var obj = JObject.Load(reader);

            foreach (var property in obj.Properties())
            {
                // Десериализуем ключ из строки
                var structureDto = JsonConvert.DeserializeObject<StructureDto>(property.Name);
                var value = property.Value.ToObject<List<StructureDto>>(serializer);

                if (structureDto != null)
                    result[structureDto] = value ?? [];
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null) return;

            var dictionary = (Dictionary<StructureDto, List<StructureDto>>)value;
            var obj = new JObject();

            foreach (var kvp in dictionary)
            {
                // Сериализуем ключ в строку
                var keyJson = JObject.FromObject(kvp.Key, serializer);
                var keyString = keyJson.ToString(Formatting.None);
                obj.Add(keyString, JToken.FromObject(kvp.Value, serializer));
            }

            obj.WriteTo(writer);
        }
    }
}
