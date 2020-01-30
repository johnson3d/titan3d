using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Bricks.Animation.Binding;
using EngineNS.IO;

namespace EngineNS.Bricks.Animation.AnimElement
{
    public class CGfxSkeletonAnimationElement : CGfxAnimationElement
    {
        Dictionary<uint, CGfxBoneAnimationElement> mBoneAnimationElements = new Dictionary<uint, CGfxBoneAnimationElement>();
        public Dictionary<uint, CGfxBoneAnimationElement> BoneAnimationElements { get => mBoneAnimationElements; }
        public CGfxSkeletonAnimationElement() : base("GfxSkeletonAnimationElement")
        {
            ElementType = AnimationElementType.Skeleton;
        }
        public CGfxSkeletonAnimationElement(NativePointer nativePointer) : base(nativePointer)
        {
            ElementType = AnimationElementType.Skeleton;
        }
        uint mKeyCount = 0;
        public override uint GetKeyCount()
        {
            return mKeyCount;
        }
        public void AddElement(CGfxBoneAnimationElement element)
        {
            mBoneAnimationElements.Add(element.Desc.NameHash, element);
            if (mKeyCount < element.GetKeyCount())
            {
                mKeyCount = element.GetKeyCount();
            }
        }
        public override void Evaluate(float curveT, Binding.AnimationElementBinding bindingElement)
        {
            var binding = bindingElement as SkeletonAnimationBinding;
            if (binding == null)
                return;
            for (int i = 0; i < binding.BoneBindings.Count; ++i)
            {
                var eleHash = binding.BoneBindings[i].AnimationElementHash;
                AnimElement.CGfxBoneAnimationElement element = null;
                if (mBoneAnimationElements.TryGetValue(eleHash, out element))
                {
                    var boneBind = binding.BoneBindings[i];
                    element.Evaluate(curveT, boneBind);
                }
            }
        }

        public override void SyncNative()
        {
            var count = SDK_GfxSkeletonAnimationElement_GetElementCount(CoreObject);
            for (uint i = 0; i < count; ++i)
            {
                var element = new CGfxBoneAnimationElement(SDK_GfxSkeletonAnimationElement_GetElement(CoreObject, i));
                AddElement(element);
            }
        }

        #region SaveLoad
        public override async Task<bool> Load(CRenderContext rc, XndNode node)
        {
            await CEngine.Instance.EventPoster.Post(async () =>
            {
                SyncLoad(rc, node);
                await Thread.AsyncDummyClass.DummyFunc();
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }
        public override bool SyncLoad(CRenderContext rc, XndNode node)
        {
            var att = node.FindAttrib("AnimationElement");
            if (att != null)
            {
                att.BeginRead();
                att.ReadMetaObject(this);
                att.EndRead();
            }
            var elementNode = node.FindNode("Elements");
            if (elementNode == null)
                return true;
            var elements = elementNode.GetNodes();
            for (int i = 0; i < elements.Count; ++i)
            {
                var boneAnim = new CGfxBoneAnimationElement();
                if (boneAnim.SyncLoad(rc, elements[i]))
                {
                    AddElement(boneAnim);
                }
            }
            return true;
        }
        public override void Save(XndNode node)
        {
            var att = node.AddAttrib("AnimationElement");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
            var elementsNode = node.AddNode("Elements", 0, 0);
            using (var it = mBoneAnimationElements.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var boneAnim = it.Current.Value;
                    var boneAnimNode = elementsNode.AddNode(boneAnim.Desc.Name, 0, 0);
                    boneAnim.Save(boneAnimNode);
                }
            }
        }
        #endregion
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxSkeletonAnimationElement_GetElementCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxAnimationElement.NativePointer SDK_GfxSkeletonAnimationElement_GetElement(NativePointer self, uint index);
        #endregion

    }
}
