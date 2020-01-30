using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;
using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.Animation.Notify;

namespace EngineNS.Bricks.Animation.Skeleton
{
    [Rtti.MetaClass]
    public class NotifyDesc : IO.Serializer.Serializer
    {
        [MetaData]
        public string Name { get; set; }
        RName mNotifyType;
        [MetaData]
        public RName NotifyType
        {
            get=> mNotifyType;
            set
            {
                mNotifyType = value;
                mNotifyGetter = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<CGfxNotify>(value);
                if (mNotifyGetter != null)
                {
                    mType = mNotifyGetter.Get(false).GetType();
                }
                else
                {
                    mType = typeof(CGfxNotify);
                }
            }
            
        }
        protected EngineNS.Macross.MacrossGetter<CGfxNotify> mNotifyGetter;
        Type mType = null;
        public Type Type
        {
            get
            {
                return mType;
            }
        }
    }
    [Rtti.MetaClass]
    public class CGfxSkeleton : AuxIOCoreObject<CGfxSkeleton.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CGfxSkeleton()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxSkeleton");
        }

        public CGfxSkeleton(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
            SyncNative();
        }
        #region Bones
        int mRootIndex = -1;
        List<CGfxBone> mBones = new List<CGfxBone>();
        [Rtti.MetaData]
        public List<Skeleton.CGfxBone> Bones
        {
            get { return mBones; }
            protected set
            {
                mBones = value;
                for (int i = 0; i < mBones.Count; ++i)
                {
                    if (mBonesDic.ContainsKey(mBones[i].BoneDesc.NameHash))
                    {
                        continue;
                    }
                    mBones[i].IndexInTable = (ushort)i;
                    mBonesDic.Add(mBones[i].BoneDesc.NameHash, mBones[i]);
                    SDK_GfxSkeleton_AddBone(CoreObject, mBones[i].CoreObject);
                }
            }
        }
        Dictionary<uint, CGfxBone> mBonesDic = new Dictionary<uint, CGfxBone>();

        //public void InitDefaultPose()
        //{
        //    //SDK_GfxAnimationPose_InitDefaultPose(CoreObject);
        //    for (int i = 0; i < mBones.Count; ++i)
        //    {
        //        var bone = mBones[i];
        //        CGfxBone parentBone = null;
        //        parentBone = FindBone(bone.BoneDesc.ParentHash);
        //        if (parentBone != null)
        //        {
        //            var bindMat = bone.BoneDesc.BindMatrix;
        //            var parentMatrix = parentBone.BoneDesc.BindMatrix;
        //            parentMatrix.Inverse();
        //            var parentSpaecMat = bindMat * parentMatrix;
        //            CGfxBoneTransform temp = CGfxBoneTransform.Identify;
        //            parentSpaecMat.Decompose(out temp.Scale, out temp.Rotation, out temp.Position);
        //            bone.BoneDesc.ParentSpaceBindTransform = temp;
        //            bone.ParentSpaceTransform = temp;
        //        }
        //        else
        //        {
        //            bone.BoneDesc.ParentSpaceBindTransform = bone.BoneDesc.BindTransform;
        //            bone.ParentSpaceTransform = bone.BoneDesc.BindTransform;
        //        }
        //        bone.CharacterSpaceTransform = bone.BoneDesc.BindTransform;
        //    }
        //} 
        public CGfxBone Root
        {
            get
            {
                if (mRootIndex >= 0)
                    return GetBone((uint)mRootIndex);
                return null;
            }
        }
        public bool SetRoot(string name)
        {
            var bone = FindBone(name);
            if (bone == null)
                return false;
            mRootIndex = bone.IndexInTable;
            return true;
        }

        public CGfxBone FindBone(string name)
        {
            var hash = UniHash.APHash(name);
            return FindBone(hash);
        }

        public CGfxBone FindBone(uint nameHash)
        {
            if (mBonesDic.ContainsKey(nameHash))
                return mBonesDic[nameHash];
            return null;
        }
        public int BoneNumber
        {
            get => mBones.Count;
        }

        public CGfxBone GetBone(uint index)
        {
            if (index >= mBones.Count)
                return null;
            return mBones[(int)index];
        }

        int AddBone(CGfxBone bone)
        {
            if (mBonesDic.ContainsKey(bone.BoneDesc.NameHash))
            {
                return -1;
            }
            bone.IndexInTable = (UInt16)mBones.Count;
            mBones.Add(bone);
            mBonesDic.Add(bone.BoneDesc.NameHash, bone);
            SDK_GfxSkeleton_AddBone(CoreObject, bone.CoreObject);
            return mBones.Count - 1;
        }
        public int AddBoneWithoutNative(CGfxBone bone)
        {
            if (mBonesDic.ContainsKey(bone.BoneDesc.NameHash))
            {
                return -1;
            }
            bone.IndexInTable = (UInt16)mBones.Count;
            mBones.Add(bone);
            mBonesDic.Add(bone.BoneDesc.NameHash, bone);
            //SDK_GfxSkeleton_AddBone(CoreObject, bone.CoreObject);
            return mBones.Count - 1;
        }
        bool RemoveBone(CGfxBone bone)
        {
            if (mBonesDic.ContainsKey(bone.BoneDesc.NameHash))
            {
                mBones.Remove(bone);
                mBonesDic.Remove(bone.BoneDesc.NameHash);
                return SDK_GfxSkeleton_RemoveBone(CoreObject, bone.BoneDesc.NameHash);
            }
            return false;
        }
        public bool DeleteBone(CGfxBone bone)
        {
            return RemoveBone(bone);
        }
        public CGfxBone NewBone(CGfxBoneDesc desc)
        {
            CGfxBone bone = new CGfxBone(desc);
            AddBone(bone);
            return bone;
        }
        public void GenerateHierarchy()
        {
            for (int i = 0; i < mBones.Count; ++i)
            {
                mBones[i].ClearChildren();
            }
            //Build mFullBones BoneTree
            for (int i = 0; i < mBones.Count; ++i)
            {
                var bone = mBones[i];
                if (bone.BoneDesc.Parent == "")
                {
                    if (mRootIndex == i)
                        continue;
                    if (mRootIndex == -1)
                    {
                        mRootIndex = i;
                        SDK_GfxSkeleton_SetRootByIndex(CoreObject, (uint)i);
                    }
                    else
                    {
                        var parnet = Root;
                        if (parnet != null)
                            parnet.AddChild((UInt16)i);
                    }
                }
                else
                {
                    var parent = FindBone(bone.BoneDesc.ParentHash);
                    if (parent != null)
                        parent.AddChild((UInt16)i);
                    //var grantParent = FindBone(bone.BoneDesc.GrantParentHash);
                    //if (grantParent != null)
                    //    grantParent.AddGrantChild((UInt16)i);
                }
            }
        }




        /// <summary>
        /// 直接操作c++ 对象，会造成C# 与 C++ 的不同步
        /// </summary>
        #region Native Call
        public void GenerateHierarchyNative()
        {
            SDK_GfxSkeleton_GenerateHierarchy(mCoreObject);
        }
        public CGfxBone RootNative
        {
            get
            {
                var ptr = SDK_GfxSkeleton_GetRoot(CoreObject);
                if (ptr.Pointer == IntPtr.Zero)
                    return null;
                return new CGfxBone(ptr);
            }
        }
        public bool SetRootNative(string name)
        {
            return (bool)SDK_GfxSkeleton_SetRoot(CoreObject, name);
        }
        public UInt32 BoneNumberNative
        {
            get
            {
                return SDK_GfxSkeleton_GetBoneNumber(CoreObject);
            }
        }
        public CGfxBone FindBoneNative(string name)
        {
            var ptr = SDK_GfxSkeleton_FindBone(CoreObject, name);
            if (ptr.Pointer == IntPtr.Zero)
                return null;
            return new CGfxBone(ptr);
        }
        public CGfxBone FindBoneNative(uint nameHash)
        {
            var ptr = SDK_GfxSkeleton_FindBoneByNameHash(CoreObject, nameHash);
            if (ptr.Pointer == IntPtr.Zero)
                return null;
            return new CGfxBone(ptr);
        }
        public CGfxBone GetBoneNative(UInt32 index)
        {
            var ptr = SDK_GfxSkeleton_GetBone(CoreObject, index);
            if (ptr.Pointer == IntPtr.Zero)
                return null;
            return new CGfxBone(ptr);
        }
        public CGfxBone NewBoneNative(CGfxBoneDesc desc)
        {
            var result = new CGfxBone(SDK_GfxSkeleton_NewBone(CoreObject, desc.CoreObject));
            result.Core_Release();
            return result;
        }

        #endregion


        #endregion Bones

        public CGfxSkeletonPose CreateSkeletonPose()
        {
            CGfxSkeletonPose pose = new CGfxSkeletonPose();
            pose.SetReferenceSkeleton(this);
            for(int i = 0;i<mBones.Count;++i)
            {
                CGfxBonePose bonePose = new CGfxBonePose();
                var bone = mBones[i];
                CGfxBone parentBone = null;
                parentBone = FindBone(bone.BoneDesc.ParentHash);
                if (parentBone != null)
                {
                    var bindMat = bone.BoneDesc.BindMatrix;
                    var parentMatrix = parentBone.BoneDesc.BindMatrix;
                    parentMatrix.Inverse();
                    var parentSpaecMat = bindMat * parentMatrix;
                    CGfxBoneTransform temp = CGfxBoneTransform.Identify;
                    parentSpaecMat.Decompose(out temp.Scale, out temp.Rotation, out temp.Position);
                    bonePose.Transform = temp;
                }
                else
                {
                    bonePose.Transform = bone.BoneDesc.BindTransform;
                }
                bonePose.SetReferenceBone( mBones[i]);
                pose.Add(bonePose);
            }
            return pose;
        }
        private RName mName;
        public RName Name { get => mName; }
        int HashCode = -1;
        public override int GetHashCode()
        {
            if (HashCode != -1)
                return HashCode;
            string hash = "";
            for(int i = 0;i<Bones.Count;++i)
            {
                hash += Bones[i].BoneDesc.Name;
            }
            HashCode = (int)EngineNS.UniHash.APHash(hash);
            return HashCode;
        }
        List<NotifyDesc> mNotifies = new List<NotifyDesc>();
        [Rtti.MetaData]
        public List<NotifyDesc> Notifies
        {
            get => mNotifies;
            set => mNotifies = value;
        }
        public bool AddNotify(string notify, RName type)
        {
           var exist = mNotifies.Find((NotifyDesc desc) => 
            {
                if (desc.Name == notify)
                    return true;
                return false;
            });
            if (exist == null)
            {
                var decs = new NotifyDesc();
                decs.Name = notify;
                decs.NotifyType = type;
                mNotifies.Add(decs);
                return true;
            }
            return false;
        }
        public void Save()
        {
            Save(mName.Address);
        }
        public void Save(string absFileName)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            var node = xnd.Node;
            //SDK_GfxSkeleton_Save2Xnd(CoreObject, node.CoreObject);
            var att = node.AddAttrib("Skeleton");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
            IO.XndHolder.SaveXND(absFileName, xnd);
        }
        public void Save(XndNode node)
        {
            var att = node.AddAttrib("Skeleton");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
        }
        public void SaveAs(RName name)
        {
            if (mName == null)
                mName = name;
            var xnd = IO.XndHolder.NewXNDHolder();
            var node = xnd.Node;
            //SDK_GfxSkeleton_Save2Xnd(CoreObject, node.CoreObject);
            var att = node.AddAttrib("Skeleton");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
            IO.XndHolder.SaveXND(name.Address, xnd);
        }

        public bool Load(RName name, bool firstLoad = true)
        {
            mName = name;
            using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
            {
                if (xnd == null)
                    return false;
                var att = xnd.Node.FindAttrib("Skeleton");
                if (att != null)
                {
                    att.BeginRead();
                    att.ReadMetaObject(this);
                    att.EndRead();
                }

                //if (SDK_GfxSkeleton_LoadXnd(CoreObject, xnd.Node.CoreObject))
                //    return true;
                //else
                //    return false;
                GenerateHierarchy();
                return true;
            }
        }
        public bool Load(XndNode node, bool firstLoad = true)
        {
            var att = node.FindAttrib("Skeleton");
            if (att != null)
            {
                att.BeginRead();
                att.ReadMetaObject(this);
                att.EndRead();
            }

            //if (SDK_GfxSkeleton_LoadXnd(CoreObject, xnd.Node.CoreObject))
            //    return true;
            //else
            //    return false;
            GenerateHierarchy();
            return true;
        }
        //Only For Import
        internal void SyncNative()
        {
            for (uint i = 0; i < BoneNumberNative; ++i)
            {
                AddBoneWithoutNative(GetBoneNative(i));
            }
        }

        public CGfxSkeleton Clone()
        {
            var clone = new CGfxSkeleton();
            for (int i = 0; i < mBones.Count; ++i)
            {
                var bone = mBones[i];
                var copyBone = new CGfxBone(bone.BoneDesc);
                copyBone.IndexInTable = bone.IndexInTable;
                for (uint j = 0; j < bone.ChildNumber; ++j)
                {
                    copyBone.AddChild(bone.GetChild(j));
                }
                clone.AddBone(copyBone);
            }
            clone.GenerateHierarchy();

            clone.Notifies = Notifies;
            clone.mName = mName;
            //var cloneSke = new CGfxSkeleton(SDK_GfxSkeleton_CloneSkeleton(CoreObject));
            //cloneSke.Core_Release();
            return clone;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxSkeleton_GetBoneNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_GfxSkeleton_CloneSkeleton(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeleton_GetRoot(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSkeleton_SetRoot(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeleton_SetRootByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxSkeleton_ExtractRootMotion(NativePointer self, vBOOL OnlyPosition);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxSkeleton_ExtractRootMotionPosition(NativePointer self, vBOOL ingoreY);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeleton_FindBone(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeleton_FindBoneByNameHash(NativePointer self, uint nameHash);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeleton_GetBone(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeleton_NewBone(NativePointer self, CGfxBoneDesc.NativePointer pBone);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxSkeleton_AddBone(NativePointer self, CGfxBone.NativePointer pBone);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSkeleton_RemoveBone(NativePointer self, uint pBone);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeleton_GenerateHierarchy(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_GfxSkeleton_Clone(NativePointer self);

        #endregion
    }

    public class CGfxSkeletonAssetManager
    {
        Dictionary<string, CGfxSkeleton> Skeletons
        {
            get;
        } = new Dictionary<string, CGfxSkeleton>();

        ~CGfxSkeletonAssetManager()
        {
            Skeletons.Clear();
        }
        public void Remove(RName name)
        {
            var key = name.PureName();
            lock (Skeletons)
            {
                if (Skeletons.ContainsKey(key))
                {
                    Skeletons.Remove(key);
                }
            }
        }

        public void ReFresh()
        {
            var fileList = CEngine.Instance.FileManager.GetFiles(CEngine.Instance.FileManager.ProjectContent, "*" + CEngineDesc.SkeletonExtension, System.IO.SearchOption.AllDirectories);
            for (int i = 0; i < fileList.Count; ++i)
            {
                var name = RName.EditorOnly_GetRNameFromAbsFile(fileList[i]);
                if (name.IsExtension(CEngineDesc.SkeletonExtension) == false)
                    continue;
                CGfxSkeleton skeleton;
                if (false == Skeletons.TryGetValue(name.PureName(), out skeleton))
                {
                    skeleton = new CGfxSkeleton();
                    if (skeleton.Load(name, false) == true)
                    {
                        if (!Skeletons.ContainsKey(name.PureName()))
                            Skeletons.Add(name.PureName(), skeleton);
                        else
                            Skeletons[name.PureName()] = skeleton;
                    }
                }
                // GetSkeleton(EngineNS.CEngine.Instance.RenderContext, name, false);
            }
        }
        public CGfxSkeleton GetSkeleton(CRenderContext rc, RName name, bool firstLoad = true)
        {
            if (name.GetExtension() == "")
            {
                name = RName.GetRName(name.Name + CEngineDesc.SkeletonExtension);
            }
            if (name.IsExtension(CEngineDesc.SkeletonExtension) == false)
                return null;
            CGfxSkeleton skeleton;
            //先看是否缓存
            if (false == Skeletons.TryGetValue(name.PureName(), out skeleton))
            {
                //再看文件是否存在，存在则直接加载
                if (CEngine.Instance.FileManager.FileExists(name.Address))
                {
                    skeleton = new CGfxSkeleton();
                    if (skeleton.Load(name, firstLoad) == false)
                        return null;

                    Skeletons.Add(name.PureName(), skeleton);
                }
                else
                {
                    //扫描游戏目录，是否存在资源
                    ReFresh();
                    if (false == Skeletons.TryGetValue(name.PureName(), out skeleton))
                    {
                        skeleton = new CGfxSkeleton();
                        if (skeleton.Load(name, firstLoad) == false)
                            return null;

                        Skeletons.Add(name.PureName(), skeleton);
                    }
                }
            }
            return skeleton;
        }
        public CGfxSkeleton CreateSkeletonAsset(CRenderContext rc, RName name, Graphics.Mesh.CGfxSkinModifier skinModifier)
        {
            var newAsset = skinModifier.SkinSkeleton.Clone();
            CGfxSkeleton oldAsset;
            //假定了所用的骨骼资产都已经加载了，如果无法保证就要用其他机制
            if (false == Skeletons.TryGetValue(name.PureName(), out oldAsset))
            {
                newAsset.SaveAs(name);
                Skeletons.Add(name.PureName(), newAsset);
                return newAsset;
            }
            else
            {
                //新旧资产互相比对融合出一个新的。
                //新增骨骼不可改变原有骨骼已有的父子级关系
                oldAsset = Skeletons[name.PureName()];
                for (uint i = 0; i < oldAsset.BoneNumber; ++i)
                {
                    newAsset.NewBone(oldAsset.GetBone(i).BoneDesc);
                }


                var mergeedAsset = newAsset;
                mergeedAsset.SaveAs(name);
                Skeletons[name.PureName()] = mergeedAsset;
                return mergeedAsset;
            }

        }
        public CGfxSkeleton CreateSkeletonAsset(CRenderContext rc, RName name, CGfxSkeleton skeleton)
        {
            var newAsset = skeleton.Clone();
            CGfxSkeleton oldAsset;
            //假定了所用的骨骼资产都已经加载了，如果无法保证就要用其他机制
            if (false == Skeletons.TryGetValue(name.PureName(), out oldAsset))
            {
                newAsset.SaveAs(name);
                Skeletons.Add(name.PureName(), newAsset);
                return newAsset;
            }
            else
            {
                //新旧资产互相比对融合出一个新的。
                //新增骨骼不可改变原有骨骼已有的父子级关系
                oldAsset = Skeletons[name.PureName()];
                for (uint i = 0; i < oldAsset.BoneNumber; ++i)
                {
                    newAsset.NewBone(oldAsset.GetBone(i).BoneDesc);
                }


                var mergeedAsset = newAsset;
                mergeedAsset.SaveAs(name);
                Skeletons[name.PureName()] = mergeedAsset;
                return mergeedAsset;
            }

        }
    }
}
