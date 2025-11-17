using EngineNS.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class TtDynConfigData
    {
        public Dictionary<string, object> ConfigDatas { get; } = new Dictionary<string, object>();
        public List<string> CommentLines { get; } = new List<string>();
        public void SetConfig(string key, object value, bool bSave = true)
        {
            ConfigDatas[key] = value;
            if (bSave)
            {
                SaveConfigData();
            }
        }
        private object GetConfig(string key)
        {
            if (ConfigDatas.TryGetValue(key, out object value))
                return value;
            return null;
        }
        public bool TryGetConfig<T>(string key, out T result)
        {
            var obj = GetConfig(key);
            if (obj == null)
            {
                result = default(T);
                return false;
            }
            if (obj.GetType() == typeof(string))
            {
                if (typeof(T) == typeof(string))
                {
                    result = (T)obj;
                }
                else
                {
                    result = (T)TConvert.ToObject(typeof(T), obj);
                }
            }
            else
            {
                result = (T)obj;
            }
            return true;
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
            foreach(var i in CommentLines)
            {
                stringBuilder.Append(i);
            }
            TtFileManager.WriteAllText(file, stringBuilder.ToString());
        }
        public void LoadConfigData(string file, bool bAsText = false)
        {
            ConfigDatas.Clear();
            CommentLines.Clear();
            //var file = TtEngine.Instance.FileManager.GetRoot(TtFileManager.ERootDir.Cache) + "DynConfigData.dcd";
            var textAll = TtFileManager.ReadAllText(file);
            if (textAll == null)
                return;
            var lines = textAll.Split("\n");
            foreach (var l in lines)
            {
                if (string.IsNullOrEmpty(l))
                    continue;
                if (l.StartsWith("##"))
                {
                    CommentLines.Add(l);
                    continue;
                }

                var text = l;
                var pos = text.IndexOf("=");
                if (pos < 0)
                {
                    continue;
                }
                var valueStr = text.Substring(pos + 1);
                var key = text.Substring(0, pos);
                string typeStr = null;
                pos = key.IndexOf(":");
                if (pos >= 0)
                {
                    var t = key;
                    key = t.Substring(0, pos);
                    typeStr = t.Substring(pos + 1);
                }

                if (bAsText == false && typeStr != null)
                    ConfigDatas[key] = TConvert.ToObject(Rtti.TtTypeDesc.TypeOf(typeStr), valueStr);
                else
                    ConfigDatas[key] = valueStr;
            }
        }
    }

    public class TtConfigAttribute : Attribute
    {
        public string Path;
    }
    public interface IConfig
    {

    }
    public class TtConfigManager
    {
        Dictionary<Rtti.TtTypeDesc, IConfig> mConfigs = new Dictionary<Rtti.TtTypeDesc, IConfig>();
        public void Initialize()
        {
            string dir = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Game, IO.TtFileManager.ESystemDir.Config);
            var patch_dir = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.Config);
            Rtti.TtTypeDescManager.Instance.InterateTypes((type) =>
            {
                if (type.HasInterface("IConfig"))
                {
                    var attr = type.GetCustomAttribute<TtConfigAttribute>(false);
                    if (attr==null)
                        return;
                    var file = TtFileManager.CombinePath(dir, attr.Path);
                    var jsCode = IO.TtFileManager.ReadAllText(file);
                    if (jsCode!=null)
                    {
                        var cfg = IO.TtFileManager.LoadObjectFromJson(type.SystemType, jsCode) as IConfig;
                        file = TtFileManager.CombinePath(patch_dir, attr.Path);
                        jsCode = IO.TtFileManager.ReadAllText(file);
                        if (jsCode != null)
                        {
                            TtAdvancedJsonPartialUpdater.PartialUpdate(jsCode, cfg, TtJsonOptions.Options);
                        }
                        mConfigs[type] = cfg;
                    }
                    else
                    {
                        mConfigs[type] = Rtti.TtTypeDescManager.CreateInstance(type) as IConfig;
                        var text = IO.TtFileManager.SaveObjectToJson(mConfigs[type]);
                        IO.TtFileManager.WriteAllText(file, text);
                    }
                }
            });
        }
        public T GetConfig<T>() where T : class, IConfig
        {
            var type = Rtti.TtTypeDescGetter<T>.TypeDesc;
            if (mConfigs.TryGetValue(type, out IConfig cfg))
            {
                return cfg as T;
            }
            return null;
        }
        public void SaveConfig<T>() where T : class, IConfig
        {
            var type = Rtti.TtTypeDescGetter<T>.TypeDesc;
            if (mConfigs.TryGetValue(type, out IConfig cfg))
            {
                string dir = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Game, IO.TtFileManager.ESystemDir.Config);
                var attr = type.GetCustomAttribute<TtConfigAttribute>(false);
                if (attr==null)
                    return;
                var file = TtFileManager.CombinePath(dir, attr.Path);
                var text = IO.TtFileManager.SaveObjectToJson(cfg);
                IO.TtFileManager.WriteAllText(file, text);
            }
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public IO.TtDynConfigData DynConfigData { get; } = new IO.TtDynConfigData();
        public IO.TtConfigManager ConfigManager { get; } = new IO.TtConfigManager();
    }
}
