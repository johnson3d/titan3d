using EngineNS.NxRHI;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace EngineNS.IO
{
    public class TtJsonOptions
    {
        public List<string> SaveProperties = null;
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
                            //if (typeInfo.Kind != JsonTypeInfoKind.Object)
                            //    return;
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
        public static void PartialUpdate<T>(string json, T target, TtJsonOptions options = null)
        {
            //options ??= new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true,
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //};

            //var sourceProperties = JsonSerializer.Deserialize<JsonElement>(json);
            //UpdateProperties<T>(sourceProperties, target, target.GetType(), options);

            using JsonDocument doc = JsonDocument.Parse(json);
            LoadProperties(doc.RootElement, target, options, true);
        }

        //private static void UpdateProperties<T>(JsonElement source, T target, Type targetType, JsonSerializerOptions options)
        //{
        //    foreach (var property in source.EnumerateObject())
        //    {
        //        var targetProperty = targetType.GetProperty(property.Name,
        //            System.Reflection.BindingFlags.IgnoreCase |
        //            System.Reflection.BindingFlags.Public |
        //            System.Reflection.BindingFlags.Instance);

        //        if (targetProperty != null && targetProperty.CanWrite)
        //        {
        //            if (property.Value.ValueKind == JsonValueKind.Object)
        //            {
        //                // 嵌套对象递归处理
        //                var nestedTarget = targetProperty.GetValue(target);
        //                if (nestedTarget != null)
        //                {
        //                    UpdateProperties(property.Value, nestedTarget, targetProperty.PropertyType, options);
        //                }
        //            }
        //            else
        //            {
        //                // 简单属性直接更新
        //                var value = JsonSerializer.Deserialize(property.Value.GetRawText(), targetProperty.PropertyType, options);
        //                targetProperty.SetValue(target, value);
        //            }
        //        }
        //    }
        //}

        public static void LoadProperties(JsonElement source, object target, TtJsonOptions options, bool checkMeta)
        {
            var targetType = target.GetType();
            foreach (var property in source.EnumerateObject())
            {
                var targetProperty = targetType.GetProperty(property.Name,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);
                if (targetProperty == null || targetProperty.CanWrite == false)
                    continue;
                if (checkMeta && targetProperty.GetCustomAttribute<Rtti.MetaAttribute>(false) == null)
                    continue;
                
                var jsonElement = property.Value;
                switch (jsonElement.ValueKind)
                {
                    case JsonValueKind.Undefined:
                        break;
                    case JsonValueKind.Object:
                        {
                            if (targetProperty.PropertyType.IsValueType)
                            {
                                var child = Rtti.TtTypeDescManager.CreateInstance(targetProperty.PropertyType);
                                LoadProperties(jsonElement, child, options, checkMeta);
                                targetProperty.SetValue(target, child);
                            }
                            else
                            {
                                var child = targetProperty.GetValue(target);
                                if (child == null)
                                {
                                    child = Rtti.TtTypeDescManager.CreateInstance(targetProperty.PropertyType);
                                    targetProperty.SetValue(target, child);
                                }
                                LoadProperties(jsonElement, child, options, checkMeta);
                            }   
                        }
                        break;
                    case JsonValueKind.Array:
                        {
                            if (targetProperty.PropertyType.GetInterface("IList") == null)
                                continue;

                            var child = targetProperty.GetValue(target);
                            if (child == null)
                            {
                                child = Rtti.TtTypeDescManager.CreateInstance(targetProperty.PropertyType);
                                targetProperty.SetValue(target, child);
                            }
                            else
                            {
                                var lst1 = child as System.Collections.IList;
                                lst1.Clear();
                            }
                            var lst = child as System.Collections.IList;
                            foreach (var i in jsonElement.EnumerateArray())
                            {
                                var eType = targetProperty.PropertyType.GetGenericArguments()[0];
                                if (i.ValueKind == JsonValueKind.Array)
                                {
                                    System.Diagnostics.Debug.Assert(false);
                                }
                                else if (i.ValueKind != JsonValueKind.Object)
                                {
                                    var val = Support.TConvert.ToObject(eType, i.ToString());
                                    lst.Add(val);
                                    continue;
                                }
                                else
                                {
                                    var e = Rtti.TtTypeDescManager.CreateInstance(eType);
                                    LoadProperties(i, e, options, checkMeta);
                                    lst.Add(e);
                                }
                            }
                        }
                        break;
                    case JsonValueKind.String:
                    case JsonValueKind.Number:
                        {
                            var v = Support.TConvert.ToObject(targetProperty.PropertyType, jsonElement.ToString());
                            targetProperty.SetValue(target, v);// jsonElement.GetRawText()
                        }
                        break;
                    case JsonValueKind.True:
                        targetProperty.SetValue(target, true);
                        break;
                    case JsonValueKind.False:
                        targetProperty.SetValue(target, false);
                        break;
                    case JsonValueKind.Null:
                        targetProperty.SetValue(target, null);
                        break;
                }
            }
        }
        public static void SaveProperties(JsonObject node, object target, TtJsonOptions options)
        {
            var targetType = target.GetType();
            var props = targetType.GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanRead == false)
                    continue;
                if (prop.GetCustomAttribute<Rtti.MetaAttribute>(false) == null)
                    continue;

                if (options!=null && options.SaveProperties!=null)
                {
                    if (options.SaveProperties.Contains(prop.Name) == false)
                        continue;
                }

                var obj = prop.GetValue(target);

                if (obj == null)
                {
                    node.Add(prop.Name, JsonValue.Create((JsonElement?)null));
                    continue;
                }

                var type = obj != null ? obj.GetType() : prop.PropertyType;

                if (IsSimpleType(type))
                {
                    if (type == typeof(RName))
                    {
                        node.Add(prop.Name, JsonValue.Create(obj.ToString()));
                    }
                    else if (type.IsEnum)
                    {
                        node.Add(prop.Name, JsonValue.Create(obj.ToString()));
                    }
                    else
                    {
                        node.Add(prop.Name, JsonValue.Create(obj));
                    }
                }
                else if (type.GetInterface("IList") != null)
                {
                    var jsonArray = new JsonArray();
                    var lst = obj as System.Collections.IList;
                    foreach (var item in lst)
                    {
                        if (IsSimpleType(item.GetType()))
                        {
                            jsonArray.Add(JsonValue.Create(item));
                        }
                        else
                        {
                            var child = new JsonObject();
                            SaveProperties(child, item, options);
                            jsonArray.Add(child);
                        }
                    }
                    node.Add(prop.Name, jsonArray);
                }
                else if(type.IsClass)
                {
                    var child = new JsonObject();
                    SaveProperties(child, obj, options);
                    node.Add(prop.Name, child);
                }
            }
        }
        public static bool IsSimpleType(Type type)
        {
            return Support.TConvert.IsPrimitiveType(type);
        }
    }
}
