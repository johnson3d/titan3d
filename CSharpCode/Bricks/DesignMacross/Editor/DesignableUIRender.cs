using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Editor
{
    public struct FDesignTypePair
    {
        /// <summary>
        /// DesignableClass显示的类名 
        /// </summary>
        public string ShowClassName { get; set; }
        /// <summary>
        /// 可编辑类的类型或者函数?
        /// </summary>
        public TtTypeDesc TypeBeDesigned { get; set; }
        /// <summary>
        /// 用来设计该类的类型
        /// </summary>
        public TtTypeDesc TypeForDesign { get; set; }
    }

    public static class TtDesignableClassTypes
    {
        static List<FDesignTypePair> mDesignTypePairs = new List<FDesignTypePair>();
        public static List<FDesignTypePair> DesignTypePairs
        {
            get
            {
                Rtti.TtTypeDescManager.Instance.OnTypeChanged += Instance_OnTypeChanged;
                if (mDesignTypePairs.Count == 0)
                {
                    CollectDesignableEditingTypes();
                }
                return mDesignTypePairs;
            }
        }

        private static void Instance_OnTypeChanged()
        {
            CollectDesignableEditingTypes();
        }
        static bool GetTypesContainsAttribute<T>(ref List<TtTypeDesc> types) where T : Attribute
        {
            try
            {
                foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
                {
                    foreach (var type in service.Types.Values)
                    {
                        if (type.IsValueType)
                            continue;
                        if (type.IsSealed)
                            continue;

                        var att = type.GetCustomAttribute<T>(false);
                        if (att != null)
                        {
                            types.Add(type);
                        }
                    }
                }
                return types.Count > 0;
            }
            catch (Exception)
            {
                
            }
            return false;
        }
        static void CollectDesignableEditingTypes()
        {
            List<TtTypeDesc> typeDescs = new List<TtTypeDesc>();
            if(GetTypesContainsAttribute<DesignableAttribute>(ref typeDescs))
            {
                foreach(var type in typeDescs)
                {
                    var att = type.GetCustomAttribute<DesignableAttribute>(false);
                    FDesignTypePair pair = default;
                    pair.ShowClassName = att.ShowName;
                    pair.TypeForDesign = type;
                    pair.TypeBeDesigned = TtTypeDesc.TypeOf(att.TypeBeDesigned);
                    mDesignTypePairs.Add(pair);
                }
                
            }
          
        }
    }

    public struct FDesignMacrossEditorRenderingContext
    {
        public TtCommandHistory CommandHistory { get; set; } = null;
        public TtEditorInteroperation EditorInteroperation { get; set; } = new TtEditorInteroperation();
        public TtGraphElementStyleCollection GraphElementStyleManager { get; set; } = null;
        public Dictionary<Guid, IGraphElement> DescriptionsElement { get; set; } = null;
        public TtClassDescription DesignedClassDescription { get; set; } = null;
        public FDesignMacrossEditorRenderingContext()
        {

        }
       
    }

}
