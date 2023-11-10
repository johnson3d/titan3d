using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.Rtti;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

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
        public UTypeDesc TypeBeDesigned { get; set; }
        /// <summary>
        /// 用来设计该类的类型
        /// </summary>
        public UTypeDesc TypeForDesign { get; set; }
    }

    public static class TtDesignableClassTypes
    {
        static List<FDesignTypePair> mDesignTypePairs = new List<FDesignTypePair>();
        public static List<FDesignTypePair> DesignTypePairs
        {
            get
            {
                Rtti.UTypeDescManager.Instance.OnTypeChanged += Instance_OnTypeChanged;
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
        static bool GetTypesContainsAttribute<T>(ref List<UTypeDesc> types) where T : Attribute
        {
            try
            {
                foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
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
            List<UTypeDesc> typeDescs = new List<UTypeDesc>();
            if(GetTypesContainsAttribute<DesignableAttribute>(ref typeDescs))
            {
                foreach(var type in typeDescs)
                {
                    var att = type.GetCustomAttribute<DesignableAttribute>(false);
                    FDesignTypePair pair = default;
                    pair.ShowClassName = att.ShowName;
                    pair.TypeForDesign = type;
                    pair.TypeBeDesigned = UTypeDesc.TypeOf(att.TypeBeDesigned);
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
        public FDesignMacrossEditorRenderingContext()
        {

        }
       
    }

}
