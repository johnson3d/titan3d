using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace AutoBuildingEditor
{
    [EngineNS.Rtti.MetaClassAttribute]
    public class PublisherConfig : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public string Version
        {
            get;
            set;
        } = "1.0.0";

        [EngineNS.Rtti.MetaData]
        public bool IsUpdateSVN
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsAndroid
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsIOS
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsPC
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsCopyRInfo
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsBuild
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsReBuild
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsDebug
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public bool IsRelease
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public string OutDir
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public string ADBAddress
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public List<string> SelectScenes
        {
            get;
            set;
        } = new List<string>();

        [EngineNS.Rtti.MetaData]
        public List<string> SelectUIs
        {
            get;
            set;
        } = new List<string>();

        [EngineNS.Rtti.MetaData]
        public List<string> SelectGames
        {
            get;
            set;
        } = new List<string>();

        [EngineNS.Rtti.MetaData]
        public List<string> SelectExcels
        {
            get;
            set;
        } = new List<string>();

        //public void Save()
        //{
        //    var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
        //    Type type = GetType();
        //    PropertyInfo[] properties = type.GetProperties();
        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        EngineNS.Rtti.MetaDataAttribute att = properties[i].GetCustomAttribute<EngineNS.Rtti.MetaDataAttribute>();
        //        if (att != null)
        //        {
        //            var attrib = xnd.Node.AddAttrib(properties[i].Name);
        //            attrib.BeginWrite();
        //            attrib.Write(properties[i].GetValue(this));
        //            attrib.EndWrite();
        //        }
        //    }


        //    var attr = xnd.Node.AddAttrib("SelectScenes");

        //    foreach (var i in TreeViewItemsNodes)
        //    {
        //        SceneData sd = i as SceneData;
        //        if (sd.IsChecked == true)
        //        {
        //            PConfig.SelectScenes.Add(sd.SceneNameToLower);
        //        }
        //    }

        //    attr.Write(PConfig.SelectScenes.Count);
        //    for (int i = 0; i < PConfig.SelectScenes.Count; i++)
        //    {
        //        attr.Write(PConfig.SelectScenes[i]);
        //    }

        //    attr.EndWrite();

        //    EngineNS.IO.XndHolder.SaveXND("PublisherConfig.cfg", xnd);
        //    xnd.Node.TryReleaseHolder();

        //    var test = LoadConfigFromFile();
        //}
    }
}
