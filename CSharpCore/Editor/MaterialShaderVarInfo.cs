using System;
using System.ComponentModel;

namespace EngineNS.Editor
{
    /// <summary>
    /// 材质shader的信息
    /// </summary>
//    public class MaterialShaderVarInfo : INotifyPropertyChanged
//    {
//        #region INotifyPropertyChangedMembers
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void OnPropertyChanged(string propertyName)
//        {
//            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
//        }
//        #endregion
//        public delegate bool Delegate_OnReName(MaterialShaderVarInfo info, string oldName, string newName);
//        public Delegate_OnReName OnReName;
               
//        bool m_bIsDirty = false;
//        public bool IsDirty
//        {
//            get { return m_bIsDirty; }
//            set { m_bIsDirty = value; }
//        }
       
//        public string EditorType = "";

//        string mVarName;
//        public string VarName
//        {
//            get { return mVarName; }
//            set
//            {
//                if (mVarName != value)
//                {
//                    mVarName = value;
////                    NickName = mVarName.Remove(0, ValueNamePreString.Length);
//                    IsDirty = true;
//                }
//            }
//        }

//        UInt32 mElementIndex;
//        public UInt32 ElementIndex
//        {
//            get { return mElementIndex; }
//            set
//            {
//                mElementIndex = value;
//                OnPropertyChanged("ElementIndex");
//            }
//        }

//        //string mNickName = "";
//        //public string NickName
//        //{
//        //    get { return mNickName; }
//        //    set
//        //    {
//        //        mNickName = value;
//        //        OnPropertyChanged("NickName");
//        //    }
//        //}

//        string m_VarType;
//        public string VarType
//        {
//            get { return m_VarType; }
//            set
//            {
//                m_VarType = value;
//                IsDirty = true;
//            }
//        }
//        string m_VarValue;
//        public string VarValue
//        {
//            get { return m_VarValue; }
//            set
//            {
//                m_VarValue = value;
//                IsDirty = true;
//            }
//        }
//        bool mDefaultBakeTexture;
//        public bool DefaultBakeTexture
//        {
//            get { return mDefaultBakeTexture; }
//            set
//            {
//                mDefaultBakeTexture = value;
//                OnPropertyChanged("DefaultBakeTexture");
//                IsDirty = true;
//            }
//        }

//        public void Copy(MaterialShaderVarInfo info)
//        {
//            VarName = info.VarName;
//            VarType = info.VarType;
//            VarValue = info.VarValue;
//            EditorType = info.EditorType;
//            DefaultBakeTexture = info.DefaultBakeTexture;
//        }
//        public void Save(EngineNS.IO.XmlNode node)
//        {
//            node.AddAttrib("EditorType", EditorType);
//            node.AddAttrib("Type", VarType);
//            node.AddAttrib("Value", VarValue);
//            node.AddAttrib("BakeTexture", DefaultBakeTexture?"true":"false");

//        }
//        public void Load(EngineNS.IO.XmlNode node)
//        {
//            var vAttr = node.FindAttrib("EditorType");
//            if (vAttr != null)
//                EditorType = vAttr.Value;
//            vAttr = node.FindAttrib("Type");
//            if (vAttr != null)
//                VarType = vAttr.Value;
//            vAttr = node.FindAttrib("Value");
//            if (vAttr != null)
//                VarValue = vAttr.Value;
//            vAttr = node.FindAttrib("BakeTexture");
//            if (vAttr != null)
//                DefaultBakeTexture = (vAttr.Value == "true") ? true : false;
//            VarName = node.Name;
//        }
//        public static string NewValueString(string varType)
//        {
//            switch (varType)
//            {
//                case "texture":
//                    return EngineNS.CEngine.Instance.GameEditorInstance.Desc.DefaultTextureName;

//                case "float":
//                case "float1":
//                    return "0";

//                case "float2":
//                    return "0,0";

//                case "float3":
//                    return "0,0,0";

//                case "float4":
//                    return "0,0,0,0";
//            }

//            return "";
//        }
//        public bool Rename(string newName)
//        {
//            if (OnReName != null)
//            {
//                if (OnReName(this, mVarName, newName) == false)
//                    return false;
//            }

//            mVarName = newName;

//            return true;
//        }

//    }
}
