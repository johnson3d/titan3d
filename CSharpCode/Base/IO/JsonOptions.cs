using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Reflection;
using System.Globalization;
using EngineNS.NxRHI;

namespace EngineNS.IO
{
    public class TtJsonOptions
    {
        public class CustomRNameConverter : JsonConverter<RName>
        {
            public override RName Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                return RName.ParseFrom(reader.GetString());
            }   

            public override void Write(
                Utf8JsonWriter writer,
                RName rn,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(rn.ToString());
            }   
        }

        public class CustomRhiTypeConverter : JsonConverter<ERhiType> 
        {
            public override ERhiType Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                object rhi;
                if(Enum.TryParse(typeof(ERhiType), reader.GetString(), out rhi))
                {
                    return (ERhiType)rhi;
                }
                return ERhiType.RHI_VirtualDevice;
            }

            public override void Write(
                Utf8JsonWriter writer,
                ERhiType rn,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(rn.ToString());
            }
        }

        public class CustomPlatformTypeConverter : JsonConverter<EPlatformType>
        {
            public override EPlatformType Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                object rhi;
                if (Enum.TryParse(typeof(EPlatformType), reader.GetString(), out rhi))
                {
                    return (EPlatformType)rhi;
                }
                return EPlatformType.PLTF_Windows;
            }

            public override void Write(
                Utf8JsonWriter writer,
                EPlatformType rn,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(rn.ToString());
            }
        }

        public static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                    {
                        static typeInfo =>
                        {
                            if (typeInfo.Kind != JsonTypeInfoKind.Object)
                                return;

                            foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
                            {
                                var prop = typeInfo.Type.GetProperty(propertyInfo.Name);
                                if(prop==null || prop.GetCustomAttribute<Rtti.MetaAttribute>(false)==null)
                                {
                                    propertyInfo.IsRequired = false;
                                }
                                else
                                {
                                    if(prop.PropertyType == typeof(RName))
                                    {

                                    }
                                }
                            }
                        }
                    }
            },
            Converters = { 
                new CustomRNameConverter(), 
                new CustomRhiTypeConverter(),
                new CustomPlatformTypeConverter()
            },
        };
    }

    public static class TtAdvancedJsonPartialUpdater
    {
        public static void PartialUpdate<T>(string json, T target, JsonSerializerOptions options = null)
        {
            options ??= new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var sourceProperties = JsonSerializer.Deserialize<JsonElement>(json);
            UpdateProperties(sourceProperties, target, target.GetType(), options);
        }

        private static void UpdateProperties<T>(JsonElement source, T target, Type targetType, JsonSerializerOptions options)
        {
            foreach (var property in source.EnumerateObject())
            {
                var targetProperty = targetType.GetProperty(property.Name,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (targetProperty != null && targetProperty.CanWrite)
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        // 嵌套对象递归处理
                        var nestedTarget = targetProperty.GetValue(target);
                        if (nestedTarget != null)
                        {
                            UpdateProperties(property.Value, nestedTarget, targetProperty.PropertyType, options);
                        }
                    }
                    else
                    {
                        // 简单属性直接更新
                        var value = JsonSerializer.Deserialize(property.Value.GetRawText(), targetProperty.PropertyType, options);
                        targetProperty.SetValue(target, value);
                    }
                }
            }
        }
    }
}
