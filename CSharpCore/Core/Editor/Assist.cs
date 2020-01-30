using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public interface IMacrossDebugValueShow
    {
        string GetShowValue();
    }

    public interface ICustomPropertyDescriptor
    {
        object GetValue(object ins);
        void SetValue(object ins, object value);
    }

    public class PropertyChangedUtility
    {
        public static void PropertyChangedProcess<T>(T obj, string propertyName, System.ComponentModel.PropertyChangedEventHandler propertyChanged)
        {
#if PWindow
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Game)
                return;
            PropertyChangedProcessImpl<T>(obj, propertyName, propertyChanged);
#endif
        }
        private static void PropertyChangedProcessImpl<T>(T obj, string propertyName, System.ComponentModel.PropertyChangedEventHandler propertyChanged)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(()=>
            {
                propertyChanged?.Invoke(obj, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }
        
        public static void CollectionChangedProcess<T>(T obj, System.Collections.Specialized.NotifyCollectionChangedEventHandler collectionChanged, System.Collections.Specialized.NotifyCollectionChangedEventArgs arg)
        {
#if PWindow
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Game)
                return;
            CollectionChangedProcessImpl<T>(obj, collectionChanged, arg);
#endif
        }
        private static void CollectionChangedProcessImpl<T>(T obj, System.Collections.Specialized.NotifyCollectionChangedEventHandler collectionChanged, System.Collections.Specialized.NotifyCollectionChangedEventArgs arg)
        {
#if PWindow
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Game)
                return;
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                collectionChanged?.Invoke(obj, arg);
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
#endif
        }
    }

    public class Assist
    {
        public static string GetValuedGUIDString(Guid guid)
        {
            string retString = guid.ToString();
            retString = retString.Replace("-", "_");

            return retString;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void OutPut(Profiler.ELogTag tag,string categroy,string info)
        {
            EngineNS.Profiler.Log.WriteLine(tag, categroy, info);
        }
    }
}
