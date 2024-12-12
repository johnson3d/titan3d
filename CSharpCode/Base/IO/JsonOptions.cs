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
}
