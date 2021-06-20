using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta]
    public class UAnimationClipAMeta : IO.IAssetMeta
    {
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            base.OnDraw(ref cmdlist, ref sz, ContentBrowser);
        }
    }

    [Rtti.Meta]
    [UAnimationClip.Import]
    [IO.AssetCreateMenu(MenuName = "Animation")]
    public partial class UAnimationClip : IO.BaseSerializer, IAnimationAsset
    {
        //DynamicInitialize
        Dictionary<Animatable.IPropertySetter, Guid> PropertySetFuncMapping { get; set; } = new Dictionary<Animatable.IPropertySetter, Guid>();
        #region IAnimationAsset
        public const string AssetExt = ".animclip";
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }

        public IAssetMeta CreateAMeta()
        {
            var result = new UAnimationClipAMeta();
            return result;
        }

        public IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }

        public void SaveAssetTo(RName name)
        {
            throw new NotImplementedException();
        }
        #endregion IAnimationAsset

        #region AnimationChunk
        UAnimationChunk AnimationChunk = null;
        public Base.AnimHierarchy AnimatedHierarchy
        {
            get
            {
                return AnimationChunk.AnimatedHierarchy;
            }
        }
        public Dictionary<Guid, Curve.ICurve> AnimCurvesList
        {
            get
            {
                return AnimationChunk.AnimCurvesList;
            }
        }
        #endregion

        #region ImprotAttribute
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                mFileDialog.Dispose();
            }
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = ImGui.ImGuiFileDialog.CreateInstance();
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                mDir = dir;
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe void OnDraw(EGui.Controls.ContentBrowser ContentBrowser)
            {
                FBXCreateCreateDraw(ContentBrowser);
            }

            //for just create a clip as a property animation not from fbx 
            public unsafe void SimpleCreateDraw(EGui.Controls.ContentBrowser ContentBrowser)
            {

            }
            private unsafe bool SimpleImport()
            {
                return false;
            }

            public unsafe partial void FBXCreateCreateDraw(EGui.Controls.ContentBrowser ContentBrowser);
        }
        #endregion

        public void Update(float time)
        {
            //loop repead pingpong
            //time = ......
            ///

            //normal do not call evaluate in update
            Evaluate(time);
        }
        public void Evaluate(float time)
        {
            var it = PropertySetFuncMapping.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Key.SetValue(AnimCurvesList[it.Current.Value], time);
            }

        }
        public void Binding(Animatable.IAnimatable animatable)
        {
            var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
            if (objType == AnimatedHierarchy.Value.ClassType)
            {
                var properties = objType.SystemType.GetProperties();
                for (int i = 0; i < properties.Length; ++i)
                {
                    var property = properties[i];
                    if (property.GetType() != typeof(Animatable.IAnimatable))
                    {
                        bool hasAnimatablePropertyAttribute = false;
                        var atts = property.GetCustomAttributes(false);
                        for (int j = 0; j < atts.Length; ++j)
                        {
                            if (atts[j].GetType() == typeof(Animatable.AnimatablePropertyAttribute))
                            {
                                hasAnimatablePropertyAttribute = true;
                            }
                        }
                        if (hasAnimatablePropertyAttribute)
                        {
                            for (int j = 0; j < AnimatedHierarchy.Value.Properties.Count; ++j)
                            {
                                var hierarchyNodePropertyDesc = AnimatedHierarchy.Value.Properties[j];
                                if (property.Name == hierarchyNodePropertyDesc.Name.GetString()
                                    && property.PropertyType == hierarchyNodePropertyDesc.ClassType.SystemType)
                                {
                                    Animatable.AnimatablePropertyDesc desc = new Animatable.AnimatablePropertyDesc();
                                    desc.ClassType = objType;
                                    desc.PropertyType = hierarchyNodePropertyDesc.ClassType;
                                    var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
                                    func.SetAnimatableObject(animatable);
                                    PropertySetFuncMapping.Add(func, hierarchyNodePropertyDesc.CurveId);
                                }
                            }
                        }

                    }
                    else
                    {
                        var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
                        for (int j = 0; j < AnimatedHierarchy.Children.Count; ++j)
                        {
                            if (AnimatedHierarchy.Children[j].Value.Name.GetString() == property.Name)
                            {
                                BindeRecursion(childAnimatable, AnimatedHierarchy.Children[i]);
                            }
                        }
                    }
                }
            }
        }
        void BindeRecursion(Animatable.IAnimatable animatable, Base.AnimHierarchy instanceHierarchy)
        {
            var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
            if (objType == AnimatedHierarchy.Value.ClassType)
            {
                var properties = objType.SystemType.GetProperties();
                for (int i = 0; i < properties.Length; ++i)
                {
                    var property = properties[i];
                    if (property.GetType() != typeof(Animatable.IAnimatable))
                    {
                        for (int j = 0; j < AnimatedHierarchy.Value.Properties.Count; ++j)
                        {
                            var animatedPropertyDesc = AnimatedHierarchy.Value.Properties[j];
                            if (property.Name == animatedPropertyDesc.Name.GetString())
                            {
                                var atts = property.GetCustomAttributes(false);
                                for (int k = 0; k < atts.Length; ++k)
                                {
                                    var attType = Rtti.UTypeDesc.TypeOfFullName(atts[k].GetType().FullName);
                                    if (attType == AnimatedHierarchy.Value.ClassType)
                                    {
                                        Animatable.AnimatablePropertyDesc desc = new Animatable.AnimatablePropertyDesc();
                                        desc.ClassType = objType;
                                        desc.PropertyType = attType;
                                        var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
                                        func.SetAnimatableObject(animatable);
                                        PropertySetFuncMapping.Add(func, animatedPropertyDesc.CurveId);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
                        for (int j = 0; j < instanceHierarchy.Children.Count; ++j)
                        {
                            if (instanceHierarchy.Children[j].Value.Name.GetString() == property.Name)
                            {
                                BindeRecursion(childAnimatable, instanceHierarchy.Children[i]);
                            }
                        }
                    }
                }
            }
        }

    }
}
