using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    public class Editor_NoDefaultObjectAttribute : Attribute
    {
    }
    public abstract class Editor_BaseAttribute : Attribute
    {
        public abstract object[] GetConstructParams();
    }
    public abstract class Editor_CustomEditorAttribute : Editor_BaseAttribute
    {
        public override abstract object[] GetConstructParams();
    }
    public class Editor_UseCustomEditorAttribute : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_MultipleOfTwoAttribute : Attribute { }
    public sealed class Editor_HexAttribute : Attribute { }
    public sealed class UIEditor_BindingPropertyAttribute : Attribute
    {
        Type[] mAvailableTypes;
        public Type[] AvailableTypes
        {
            get { return mAvailableTypes; }
        }

        public UIEditor_BindingPropertyAttribute(Type[] availableTypes = null)
        {
            mAvailableTypes = availableTypes;
        }
    }
    public sealed class Editor_LinkSystemCustomClassPropertyEnableShowAttribute : Attribute { }
    public sealed class Editor_PropertyGridSortIndex : Attribute
    {
        public int Index = 0;
        public Editor_PropertyGridSortIndex(int idx)
        {
            Index = idx;
        }
    }
    public sealed class Editor_ColorPicker : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_Color4Picker : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    [Description("自带auto属性的标志,如属性Width，他的自动属性为Width_Auto")]
    /// <summary>
    /// 自带auto属性的标志,如属性Width，他的自动属性为Width_Auto
    /// </summary>
    public sealed class UIEditor_PropertysWithAutoSet : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class OpenFolderEditorAttribute : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }

    [Description("打开文件对话框Attribute")]
    public sealed class OpenFileEditorAttribute : Editor_CustomEditorAttribute
    {
        string mConsExt = "";
        public List<string> ExtNames = new List<string>();
        /// <summary>
        /// 打开文件对话框Attribute
        /// </summary>
        /// <param name="ext">文件扩展名，不带.</param>
        public OpenFileEditorAttribute(string ext)
        {
            mConsExt = ext;
            if (!string.IsNullOrEmpty(ext))
            {
                var splits = ext.Split(',');
                foreach (var split in splits)
                {
                    ExtNames.Add(split);
                }
            }
        }
        public override object[] GetConstructParams()
        {
            return new object[] { mConsExt };
        }
    }
    public sealed class Editor_ValueWithRange : Editor_CustomEditorAttribute
    {
        public double maxValue = 100;
        public double minValue = 0;
        public Editor_ValueWithRange(double min, double max)
        {
            minValue = min;
            maxValue = max;
        }
        public override object[] GetConstructParams()
        {
            return new object[] { minValue, maxValue };
        }
    }
    public sealed class Editor_VectorEditor : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_Angle360Setter : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_Angle180Setter : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_HotKeySetter : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_FlagsEnumSetter : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_PropertyGridDataTemplateAttribute : Editor_CustomEditorAttribute
    {
        string mDataTemplateType = "";
        public string DataTemplateType
        {
            get { return mDataTemplateType; }
        }

        public object[] Args { get; set; }

        public Editor_PropertyGridDataTemplateAttribute(string dataTemplateType, object[] args = null)
        {
            mDataTemplateType = dataTemplateType;
            Args = args;
        }

        public override object[] GetConstructParams()
        {
            return new object[] { mDataTemplateType, Args };
        }
    }

    public sealed class Editor_ShowInPropertyGridAttribute : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    public sealed class Editor_ExpandedInPropertyGridAttribute : Editor_BaseAttribute
    {
        public bool IsExpanded;
        public override object[] GetConstructParams()
        {
            return new object[] { IsExpanded };
        }

        public Editor_ExpandedInPropertyGridAttribute(bool isExpanded)
        {
            IsExpanded = isExpanded;
        }
    }
    // 在PropertyGrid中不显示父级属性
    [Description("在PropertyGrid中不显示父级属性")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Editor_DoNotShowBaseClassProperties : System.Attribute
    {

    }
    public sealed class Editor_SocketSelectAttribute : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return new object[] { };
        }
    }
    public sealed class Editor_LAGraphBonePoseSelectAttribute : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return new object[] { };
        }
    }
    public sealed class Editor_ClassPropertySelectAttributeAttribute : Editor_CustomEditorAttribute
    {
        public Type ClassType;
        public Type[] FilterTypes;
        public Editor_ClassPropertySelectAttributeAttribute(Type classType, Type[] filterTypes)
        {
            ClassType = classType;
            FilterTypes = filterTypes;
        }

        public override object[] GetConstructParams()
        {
            return new object[] { ClassType, FilterTypes };
        }
    }
    public interface Editor_RNameTypeObjectBind
    {
        void invoke(object param);
    }
    public sealed class Editor_RNameTypeAttribute : Editor_CustomEditorAttribute
    {
        public const string Mesh = "Mesh";
        public const string MeshSource = "MeshSource";
        public const string MeshCluster = "MeshCluster";
        public const string AnimationClip = "AnimationClip";
        public const string AnimationBlendSpace1D = "AnimationBlendSpace1D";
        public const string AnimationBlendSpace = "AnimationBlendSpace";
        public const string AnimationAdditiveBlendSpace1D = "AnimationAdditiveBlendSpace1D";
        public const string AnimationAdditiveBlendSpace = "AnimationAdditiveBlendSpace";
        public const string AnimationMacross = "AnimationMacross";
        public const string ComponentMacross = "ComponentMacross";
        public const string Skeleton = "Skeleton";
        public const string Material = "Material";
        public const string MaterialInstance = "MaterialInstance";
        public const string ShaderCode = "ShaderCode";
        public const string ShadingEnv = "ShadingEnv";
        public const string Texture = "Texture";
        public const string Describe = "Describe";
        public const string VertexCloud = "VertexCloud";
        public const string Macross = "Macross";
        public const string McLogicAnim = "McLogicAnim";
        public const string McLogicFSM = "McLogicStateMachine";
        public const string McBehaviorTree = "McBehaviorTree";
        public const string MacrossEnum = "Macross_Enum";
        public const string Excel = "Xls";
        public const string Particle = "Particle";
        public const string Scene = "Scene";
        public const string PhyGeom = "PhyGeom";
        public const string PhyHeightFieldGeom = "PhyHeightFieldGeom";
        public const string PhyTriangleMeshGeom = "PhyTriangleMeshGeom";
        public const string PhyConvexGeom = "PhyConvexGeom";
        public const string PhyMaterial = "PhyMaterial";
        public const string Prefab = "Prefab";
        public const string UVAnim = "UVAnim";
        public const string UI = "UI";
        public const string Font = "Font";

        public string RNameType = "";
        public bool ShowActiveBtn = false;
        public Editor_RNameTypeAttribute(string rNameType, bool showactive = false)
        {
            RNameType = rNameType;
            ShowActiveBtn = showactive;
        }

        public override object[] GetConstructParams()
        {
            return new object[] { RNameType, ShowActiveBtn };
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Editor_PackDataAttribute : System.Attribute
    {
        public Editor_PackDataAttribute()
        {
        }
    }

    public class Editor_RNameMacrossType : Editor_CustomEditorAttribute
    {
        public Type MacrossBaseType;
        public Editor_RNameMacrossType(Type macrossBaseType)
        {
            MacrossBaseType = macrossBaseType;
        }
        public override object[] GetConstructParams()
        {
            return new object[] { MacrossBaseType };
        }
    }

    public class Editor_RNameMExcelType : Editor_CustomEditorAttribute
    {
        public Type MacrossBaseType;
        public Editor_RNameMExcelType(Type macrossBaseType)
        {
            MacrossBaseType = macrossBaseType;
        }
        public override object[] GetConstructParams()
        {
            return new object[] { MacrossBaseType };
        }
    }

    public class Editor_PropertyGridUIProvider
    {
        public virtual bool UseCustomCtrl { get => true; }
        public virtual string GetName(object arg)
        {
            return arg.ToString();
        }
        public virtual Type GetUIType(object arg)
        {
            return typeof(int);
        }
        public virtual bool IsReadOnly(object arg)
        {
            return false;
        }
        public virtual void SetValue(object arg, object val) { }
        public virtual object GetValue(object arg) { return null; }
    }
    public sealed class Editor_PropertyGridUIShowProviderAttribute : Editor_BaseAttribute
    {
        public Type PropertyGridUIProviderType;

        public override object[] GetConstructParams()
        {
            return new Type[] { PropertyGridUIProviderType };
        }

        public Editor_PropertyGridUIShowProviderAttribute(Type type)
        {
            PropertyGridUIProviderType = type;
        }
    }

    public sealed class Editor_EnumerableAttribute : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams() { return null; }
    }

    public sealed class Editor_InputWithErrorCheckAttribute : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams() { return null; }
    }

    #region NotifyValueChanged

    public class NotifyMemberValueChangedArg
    {
        public object OldValue;
        public object NewValue;
        public object ValueHostObject;
        public ICustomPropertyDescriptor Property;
    }
    public interface INotifyMemberValueChangedReceiver
    {
        void OnMemberValueChanged(string path, NotifyMemberValueChangedArg arg);
    }
    public sealed class Editor_NotifyMemberValueChangedAttribute : Editor_BaseAttribute
    {
        public override object[] GetConstructParams() { return null; }
    }

    #endregion

    #region Macross
    [Description("允许在逻辑图中使用的成员(函数、属性等)")]
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Enum)]
    public sealed class MacrossMemberAttribute : System.Attribute
    {
        [EngineNS.IO.Serializer.EnumSizeAttribute(typeof(EngineNS.IO.Serializer.UInt64Enum))]
        public enum enMacrossType : UInt64
        {
            Unknow = 0,
            Callable = 1 << 0,        // 可被Macross调用
            Overrideable = 1 << 1,        // 可被Macross重写
            ReadOnly = 1 << 2,        // 对Macross只读
            Unsafe = 1 << 3,        // unsafe函数

            NotShowInBreak = 1 << 4,        // debug断点时显示值
            IgnoreCopy = 1 << 5,
            PropReadOnly = Callable | ReadOnly,
        }
        public enMacrossType MacrossType
        {
            get;
            private set;
        }

        /// <summary>
        /// 节点列表中显示的路径
        /// </summary>
        public string Path
        {
            get;
            private set;
        }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get;
            private set;
        }
        /// <summary>
        /// 安全等级
        /// </summary>
        public byte SecurityLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// 允许在逻辑图中使用的成员(函数、属性等)
        /// </summary>
        /// <param name="path">在节点列表中显示的路径(aa.bb.cc)</param>
        /// <param name="description">描述</param>
        /// <param name="securityLevel">安全级别，根据客户权限对能否调用进行筛选</param>
        public MacrossMemberAttribute(enMacrossType macrossType, string path, string description, byte securityLevel = 0)
        {
            MacrossType = macrossType;
            Path = path.Replace(",", ".");
            Description = description;
            SecurityLevel = securityLevel;
        }

        public MacrossMemberAttribute(enMacrossType macrossType, string description = "", byte securityLevel = 0)
        {
            MacrossType = macrossType;
            Path = "";
            Description = description;
            SecurityLevel = securityLevel;
        }

        public bool HasType(enMacrossType type)
        {
            return (MacrossType & type) == type;
        }
        public static bool HasType(enMacrossType cur, enMacrossType tag)
        {
            return (cur & tag) == tag;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public sealed class Editor_MacrossClassIconAttribute : Editor_BaseAttribute
    {
        public string IconRNameStr;
        public EngineNS.RName.enRNameType IconRNameType = RName.enRNameType.Game;
        public override object[] GetConstructParams() { return new object[] { IconRNameStr, IconRNameType }; }
        public Editor_MacrossClassIconAttribute(string iconRNameStr, EngineNS.RName.enRNameType iconRNameType)
        {
            IconRNameStr = iconRNameStr;
            IconRNameType = iconRNameType;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public sealed class Editor_ComponentClassIconAttribute : Editor_BaseAttribute
    {
        public string IconRNameStr;
        public EngineNS.RName.enRNameType IconRNameType = RName.enRNameType.Game;
        public override object[] GetConstructParams() { return new object[] { IconRNameStr, IconRNameType }; }
        public Editor_ComponentClassIconAttribute(string iconRNameStr, EngineNS.RName.enRNameType iconRNameType)
        {
            IconRNameStr = iconRNameStr;
            IconRNameType = iconRNameType;
        }
    }

    // 标识类型是否为Macross可用的类型
    [Description("标识类型是否为Macross可用的类型")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
    public sealed class Editor_MacrossClassAttribute : Editor_BaseAttribute
    {
        public override object[] GetConstructParams() { return new object[] { CSType, MacrossType }; }
        [Flags]
        public enum enMacrossType : UInt64
        {
            None = 0,
            Useable = 1 << 0,               // 可被Macross使用
            Inheritable = 1 << 1,           // 可被Macross类继承
            Createable = 1 << 2,            // 可被Macross创建
            Declareable = 1 << 3,           // 可被Macross声明
            MacrossGetter = 1 << 4,         // 可以定义MacrossGetter<T>成员变量

            AllFeatures = Useable | Inheritable | Createable | Declareable | MacrossGetter,
        }
        public enMacrossType MacrossType { get; set; }
        public EngineNS.ECSType CSType { get; set; }
        public Editor_MacrossClassAttribute(EngineNS.ECSType csType, enMacrossType macrossType)
        {
            MacrossType = macrossType;
            CSType = csType;
        }

        public bool HasType(enMacrossType type)
        {
            return (MacrossType & type) == type;
        }
    }
    // Macross类型的显示名称
    public sealed class Editor_ClassDisplayNameAttribute : Editor_BaseAttribute
    {
        public override object[] GetConstructParams()
        {
            return new object[] { DisplayName };
        }
        public string[] DisplayName;
        public Editor_ClassDisplayNameAttribute(params string[] displayName)
        {
            DisplayName = displayName;
        }
    }

    // 标识类型是否为Macross可用的类型
    [Description("标识类型是否为Macross Enum可用的类型")]
    [AttributeUsage(AttributeTargets.Enum)]
    public class Editor_MacrossEnumAttribute : System.Attribute
    {
        public Editor_MacrossEnumAttribute()
        {
        }
    }

    [Description("用于在重载函数时设置函数参数为不同的类型（类型间必须可转换）")]
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class Editor_MacrossMethodParamTypeAttribute : Editor_BaseAttribute
    {
        public Type ParamType;
        public Editor_MacrossMethodParamTypeAttribute(Type paramType)
        {
            ParamType = paramType;
        }

        public override object[] GetConstructParams()
        {
            return new object[] { ParamType };
        }
    }
    //[Editor_MacrossClass(ECSType.Client, Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor_MacrossClassAttribute.enMacrossType.Useable)]
    //public class Macross_Test
    //{
    //    public string ProStr
    //    {
    //        get;
    //        set;
    //    }
    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable)]
    //    public int ProInt
    //    {
    //        get;
    //        set;
    //    }
    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable)]
    //    public int ProIntGet
    //    {
    //        get;
    //    }

    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable)]
    //    public virtual int Function(ref int a, out float b)
    //    {
    //        b = 0;
    //        return 0;
    //    }
    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable | MacrossMemberAttribute.enMacrossType.Overrideable)]
    //    protected virtual string FunctionOverride(ref UInt16 a, out Int32 b)
    //    {
    //        //bool debug = false;
    //        //if(debug)
    //        //{
    //        //    EngineNS.CEngine.Instance.EventPoster.Post(() =>
    //        //    {

    //        //    }, Thread.Async.EAsyncTarget.MacrossDebug).Wait();
    //        //}
    //        //else
    //        //{

    //        //}

    //        b = 0;
    //        return "";
    //    }
    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable | MacrossMemberAttribute.enMacrossType.Overrideable)]
    //    protected virtual void FunctionEvent(ref int a, out int b)
    //    {
    //        b = 0;
    //    }
    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable | MacrossMemberAttribute.enMacrossType.Overrideable)]
    //    protected virtual void FunctionEvent2()
    //    {
    //    }

    //    public enum enMT
    //    {
    //        A,
    //        B,
    //        C,
    //        D,
    //    }


    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable)]
    //    public void FunctionCall(int a, List<int> list, List<enMT> mt, List<string> strList)
    //    {

    //    }

    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable)]
    //    public void FunctionRName([EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource)]
    //                              EngineNS.RName rname)
    //    {

    //    }

    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable | MacrossMemberAttribute.enMacrossType.Overrideable)]
    //    public virtual async Task<bool> AsyncTask()
    //    {
    //        await EngineNS.Thread.AsyncDummyClass.DummyFunc();
    //        return true;
    //    }
    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable | MacrossMemberAttribute.enMacrossType.Overrideable | MacrossMemberAttribute.enMacrossType.Unsafe)]
    //    public virtual void UnsafeFunction()
    //    {

    //    }

    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable | MacrossMemberAttribute.enMacrossType.Overrideable)]
    //    public virtual async Task AsyncTaskWithoutReturn()
    //    {
    //        await EngineNS.Thread.AsyncDummyClass.DummyFunc();
    //    }

    //    [MacrossMemberAttribute(MacrossMemberAttribute.enMacrossType.Callable | MacrossMemberAttribute.enMacrossType.Overrideable)]
    //    public virtual void DelegateTest(Action<int, bool> act1, Action<int> act2)
    //    {

    //    }
    //}

    #endregion

    public sealed class Editor_MenuMethod : Editor_CustomEditorAttribute
    {
        public string[] MenuNames;
        public override object[] GetConstructParams() { return new object[] { MenuNames }; }
        public Editor_MenuMethod(params string[] menuNames)
        {
            MenuNames = menuNames;
        }
    }

    public sealed class Editor_PlantAbleActor : Editor_CustomEditorAttribute
    {
        public string Category;
        public string Name;
        public override object[] GetConstructParams() { return new object[] { Category, Name }; }
        public Editor_PlantAbleActor(string category, string name)
        {
            Category = category;
            Name = name;
        }
    }

    public sealed class Editor_Guid : Editor_CustomEditorAttribute
    {
        public string Guid { get; }
        public override object[] GetConstructParams() { return new object[] { Guid }; }
        public Editor_Guid(string guid)
        {
            Guid = guid;
        }
    }

    public sealed class Editor_ListCustomAddRemoveActionAttribute : Editor_CustomEditorAttribute
    {
        public abstract class AddRemoveActionProviderBase
        {
            public abstract object[] Add();
            public abstract object[] Insert();
            public abstract bool Remove(object item);
        }
        public Type ProviderType;
        public override object[] GetConstructParams()
        {
            return new object[] { ProviderType };
        }
        public Editor_ListCustomAddRemoveActionAttribute(Type providerType)
        {
            ProviderType = providerType;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MacrossClassKeyAttribute : System.Attribute
    {
        public MacrossClassKeyAttribute()
        {

        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class DisplayParamNameAttribute : DisplayNameAttribute
    {
        public DisplayParamNameAttribute(string name) : base(name)
        {

        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class MacrossPanelPathAttribute : Attribute
    {
        public string Path;
        public MacrossPanelPathAttribute(string path)
        {
            Path = path;
        }
    }
    public sealed class Editor_ShowOnlyInnerProperties : Editor_CustomEditorAttribute
    {
        public override object[] GetConstructParams()
        {
            return null;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class Editor_DisplayNameInEnumerable : Editor_CustomEditorAttribute
    {
        public string DisplayName = "";
        public Editor_DisplayNameInEnumerable(string displayName)
        {
            DisplayName = displayName;
        }
        public override object[] GetConstructParams()
        {
            return new object[] { DisplayName };
        }
    }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class Editor_TypeChangeWithParamAttribute : Editor_CustomEditorAttribute
    {
        public int Index = -1;
        public Editor_TypeChangeWithParamAttribute(int index)
        {
            Index = index;
        }
        public override object[] GetConstructParams()
        {
            return new object[] { Index };
        }
    }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class Editor_TypeFilterAttribute : Editor_CustomEditorAttribute
    {
        public Type BaseType;
        [Flags]
        public enum enTypeFilter : UInt16
        {
            Unknow = 0,
            Class = 1 << 0,
            Struct = 1 << 1,
            Primitive = 1 << 2,
            Enum = 1 << 3,
            All = UInt16.MaxValue,
        }
        public enTypeFilter Filter = enTypeFilter.All;
        public Editor_MacrossClassAttribute.enMacrossType MacrossType = Editor_MacrossClassAttribute.enMacrossType.None;
        public static bool Contains(enTypeFilter filter, enTypeFilter tagFilter)
        {
            return ((filter & tagFilter) == tagFilter);
        }

        public Editor_TypeFilterAttribute(enTypeFilter filter)
        {
            BaseType = typeof(object);
            Filter = filter;
        }
        public Editor_TypeFilterAttribute(Type baseType, enTypeFilter filter)
        {
            BaseType = baseType;
            Filter = filter;
        }
        public Editor_TypeFilterAttribute(Type baseType)
        {
            BaseType = baseType;
        }

        public Editor_TypeFilterAttribute(enTypeFilter filter, Editor_MacrossClassAttribute.enMacrossType macrossType)
        {
            BaseType = typeof(object);
            Filter = filter;
            MacrossType = macrossType;
        }
        public Editor_TypeFilterAttribute(Type baseType, enTypeFilter filter, Editor_MacrossClassAttribute.enMacrossType macrossType)
        {
            BaseType = baseType;
            Filter = filter;
            MacrossType = macrossType;
        }
        public Editor_TypeFilterAttribute(Type baseType, Editor_MacrossClassAttribute.enMacrossType macrossType)
        {
            BaseType = baseType;
            MacrossType = macrossType;
        }
        public override object[] GetConstructParams()
        {
            return new object[] { BaseType, Filter, MacrossType };
        }
    }
    public sealed class Editor_NoCategoryAttribute : Editor_BaseAttribute
    {
        public override object[] GetConstructParams()
        {
            return new object[0];
        }

    }
}