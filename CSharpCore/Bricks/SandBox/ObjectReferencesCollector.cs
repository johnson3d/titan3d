using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.SandBox
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NotProcessObjectReferenceAttribute : Attribute
    {

    }

    public class ObjectReferencesCollector
    {
        public delegate Task FOnVisitProperty(System.Reflection.PropertyInfo prop, object value);
        public delegate Task FOnVisitMember(System.Reflection.FieldInfo member, object value);
        public delegate Task FOnVisitContainer(object value);
        public HashSet<object> VisitedObjects = new HashSet<object>();
        public bool HasVisited(object obj)
        {
            if (obj == null)
                return true;
            if (obj.GetType().FullName == "System.RuntimeType")
                return true;
            if (obj.GetType() == typeof(EngineNS.CConstantBuffer))
                return true;
            if (VisitedObjects.Contains(obj))
                return true;
            VisitedObjects.Add(obj);
            return false;
        }
        public class CollectProcessData
        {
            public bool IgnoreValueType = true;
            public Func<object, bool> ObjectValidAction;
        }
        public async Task CollectReferences(object obj, FOnVisitProperty visitProp, FOnVisitMember visitMember, FOnVisitContainer visitContainer, CollectProcessData data)
        {
            if (obj == null)
                return;
            var type = obj.GetType();
            var props = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach(var i in props)
            {
                if (i.GetIndexParameters().Length != 0)
                    continue;
                if (data.IgnoreValueType && i.PropertyType.IsValueType)
                    continue;
                if (i.GetCustomAttributes(typeof(NotProcessObjectReferenceAttribute), false).Length > 0)
                    continue;
                object cld = null;
                try
                {
                    cld = i.GetValue(obj, null);
                }
                catch
                {
                    cld = null;
                }
                if (data.ObjectValidAction?.Invoke(cld) == false)
                    continue;
                if (HasVisited(cld))
                    continue;
                if (visitProp!=null)
                {
                    await visitProp(i, cld);
                }
                var enumerable = cld as System.Collections.IEnumerable;
                if(enumerable != null)
                {
                    // 可枚举对象
                    if (visitContainer!=null)
                    {
                        var lst = cld as System.Collections.IList;
                        if (lst != null)
                        {
                            for (int j = 0; j < lst.Count; j++)
                            {
                                if (data.IgnoreValueType && lst[j].GetType().IsValueType)
                                    continue;
                                if (data.ObjectValidAction?.Invoke(lst[j]) == false)
                                    continue;
                                if (HasVisited(lst[j]))
                                    continue;
                                await visitContainer(lst[j]);
                                await CollectReferences(lst[j], visitProp, visitMember, visitContainer, data);
                            }
                        }
                        var dict = cld as System.Collections.IDictionary;
                        if (dict != null)
                        {
                            var j = dict.GetEnumerator();
                            while (j.MoveNext())
                            {
                                if (j.Key != null)
                                {
                                    if (data.IgnoreValueType == false || j.Key.GetType().IsValueType == false)
                                    {
                                        if (data.ObjectValidAction?.Invoke(j.Key) == false)
                                            continue;
                                        if (HasVisited(j.Key))
                                            continue;
                                        await visitContainer(j.Key);
                                        await CollectReferences(j.Key, visitProp, visitMember, visitContainer, data);
                                    }
                                }

                                if (j.Value != null)
                                {
                                    if (data.IgnoreValueType == false || j.Value.GetType().IsValueType == false)
                                    {
                                        if (data.ObjectValidAction?.Invoke(j.Value) == false)
                                            continue;
                                        if (HasVisited(j.Value))
                                            continue;
                                        await visitContainer(j.Value);
                                        await CollectReferences(j.Value, visitProp, visitMember, visitContainer, data);
                                    }
                                }
                            }
                        }
                    } 
                }
                else
                    await CollectReferences(cld, visitProp, visitMember, visitContainer, data);
                
            }
            var members = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var i in members)
            {
                if (data.IgnoreValueType && i.FieldType.IsValueType)
                    continue;
                if (i.GetCustomAttributes(typeof(NotProcessObjectReferenceAttribute), false).Length > 0)
                    continue;
                var cld = i.GetValue(obj);
                if (data.ObjectValidAction?.Invoke(cld) == false)
                    continue;
                if (HasVisited(cld))
                    continue;
                if (visitMember != null)
                {
                    await visitMember(i, cld);
                }
                var enumerable = cld as System.Collections.IEnumerable;
                if(enumerable != null)
                {
                    // 可枚举对象
                    if (visitContainer != null)
                    {
                        var lst = cld as System.Collections.IList;
                        if (lst != null)
                        {
                            for (int j = 0; j < lst.Count; j++)
                            {
                                if (data.IgnoreValueType && lst[j].GetType().IsValueType)
                                    continue;
                                if (data.ObjectValidAction?.Invoke(lst[j]) == false)
                                    continue;
                                if (HasVisited(lst[j]))
                                    continue;
                                await visitContainer(lst[j]);
                                await CollectReferences(lst[j], visitProp, visitMember, visitContainer, data);
                            }
                        }
                        var dict = cld as System.Collections.IDictionary;
                        if (dict != null)
                        {
                            var j = dict.GetEnumerator();
                            while (j.MoveNext())
                            {
                                if (j.Key != null)
                                {
                                    if (data.IgnoreValueType == false || j.Key.GetType().IsValueType == false)
                                    {
                                        if (data.ObjectValidAction?.Invoke(j.Key) == false)
                                            continue;
                                        if (HasVisited(j.Key))
                                            continue;
                                        await visitContainer(j.Key);
                                        await CollectReferences(j.Key, visitProp, visitMember, visitContainer, data);
                                    }
                                }

                                if (j.Value != null)
                                {
                                    if (data.IgnoreValueType == false || j.Value.GetType().IsValueType == false)
                                    {
                                        if (data.ObjectValidAction?.Invoke(j.Value) == false)
                                            continue;
                                        if (HasVisited(j.Value))
                                            continue;
                                        await visitContainer(j.Value);
                                        await CollectReferences(j.Value, visitProp, visitMember, visitContainer, data);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    await CollectReferences(cld, visitProp, visitMember, visitContainer, data);
            }
        }
    }
}
