using EngineNS.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class TtDynConfigData
    {
        public Dictionary<string, object> ConfigDatas { get; } = new Dictionary<string, object>();
        public void SetConfig(string key, object value, bool bSave = true)
        {
            ConfigDatas[key] = value;
            if (bSave)
            {
                SaveConfigData();
            }
        }
        public object GetConfig(string key)
        {
            if (ConfigDatas.TryGetValue(key, out object value))
                return value;
            return null;
        }
        public T TryGetConfig<T>(string key)
        {
            var obj = GetConfig(key);
            if (obj == null)
            {
                return default(T);
            }
            return (T)obj;
        }
        public void SaveConfigData()
        {
            var file = TtEngine.Instance.FileManager.GetRoot(TtFileManager.ERootDir.Cache) + "DynConfigData.dcd";
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var i in ConfigDatas)
            {
                stringBuilder.Append(i.Key);
                stringBuilder.Append($":{Rtti.TtTypeDesc.TypeOf(i.Value.GetType()).ToString()}");
                stringBuilder.Append("=");
                stringBuilder.Append(i.Value.ToString());
                stringBuilder.Append("\n");
            }
            TtFileManager.WriteAllText(file, stringBuilder.ToString());
        }
        public void LoadConfigData()
        {
            ConfigDatas.Clear();
            var file = TtEngine.Instance.FileManager.GetRoot(TtFileManager.ERootDir.Cache) + "DynConfigData.dcd";
            var textAll = TtFileManager.ReadAllText(file);
            if (textAll == null)
                return;
            var lines = textAll.Split("\n");
            foreach (var l in lines)
            {
                if (string.IsNullOrEmpty(l))
                    continue;
                var text = l;
                var pos = text.IndexOf(":");
                var key = text.Substring(0, pos);
                text = text.Substring(pos + 1);
                pos = text.IndexOf("=");
                var typeStr = text.Substring(0, pos);
                var valueStr = text.Substring(pos + 1);

                ConfigDatas[key] = TConvert.ToObject(Rtti.TtTypeDesc.TypeOf(typeStr), valueStr);
            }
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public IO.TtDynConfigData DynConfigData { get; } = new IO.TtDynConfigData();
    }
}
