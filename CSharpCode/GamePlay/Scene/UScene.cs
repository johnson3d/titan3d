using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public class USceneData : UNodeData
    {
        
    }
    public class UScene : UNode
    {
        public UScene(UNodeData data)
            : base(data, EBoundVolumeType.Box, typeof(UPlacement))
        {
            
        }
        int PrevAllocId = 0;
        private UNode[] ContainNodes = new UNode[UInt16.MaxValue];
        public void AllocId(UNode node)
        {
            if (node is ULightWeightNodeBase)
                return;
            for (int i = PrevAllocId; i < ContainNodes.Length; i++)
            {
                if (ContainNodes[i] == null)
                {
                    ContainNodes[i] = node;
                    node.Id = (UInt32)i;
                    node.ParentScene = this;
                    PrevAllocId = i;
                    return;
                }
            }
            for (int i = 0; i < ContainNodes.Length; i++)
            {
                if(ContainNodes[i]==null)
                {
                    ContainNodes[i] = node;
                    node.Id = (UInt32)i;
                    node.ParentScene = this;
                    PrevAllocId = i;
                    return;
                }
            }
            System.Diagnostics.Debug.Assert(false);
        }
        public void FreeId(UNode node)
        {
            if (node is ULightWeightNodeBase)
                return;
            if (node.Id >= UInt16.MaxValue)
                return;
            ContainNodes[node.Id] = null;
            node.Id = UInt32.MaxValue;
        }
        public UNode NewNode(string nodeType, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ntype = Rtti.UTypeDesc.TypeOf(nodeType);
            return NewNode(ntype.SystemType, data, bvType, placementType);
        }
        public UNode NewNode(Type nodeType, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var args = new object[] { data, bvType, placementType };
            var node = Rtti.UTypeDescManager.CreateInstance(nodeType, args) as UNode;
            if (node != null)
            {
                AllocId(node);
            }
            return node;
        }
        public USceneData SceneData 
        { 
            get
            {
                return NodeData as USceneData;
            }
        }
        public void SaveScene(RName name)
        {
            var typeStr = Rtti.UTypeDesc.TypeStr(GetType());
            var xndHolder = new EngineNS.IO.CXndHolder(typeStr, 1, 0);
            var xnd = xndHolder;
            var node = xndHolder.RootNode;
            if (SceneData != null)
            {
                using (var dataAttr = xnd.NewAttribute("SceneDesc", 1, 0))
                {
                    node.AddAttribute(dataAttr);
                    var ar = dataAttr.GetWriter((ulong)SceneData.GetStructSize() * 2);
                    ar.Write(SceneData);
                    dataAttr.ReleaseWriter(ref ar);
                }
            }

            SaveXndNode(this, xnd.mCoreObject, node.mCoreObject);

            xndHolder.SaveXnd(name.Address);
        }
        public unsafe bool LoadScene(RName name)
        {
            var xnd = IO.CXndHolder.LoadXnd(name.Address);
            var tmp = xnd.RootNode.mCoreObject.FindFirstAttribute("SceneDesc");
            if (tmp.NativePointer != IntPtr.Zero)
            {
                var descAttr = new XndAttribute(xnd.RootNode.mCoreObject.FindFirstAttribute("SceneDesc"));
                var ar = descAttr.GetReader(this);
                IO.ISerializer desc;
                ar.Read(out desc, this);

                NodeData = desc as USceneData;
            }

            return LoadXndNode(this, xnd.RootNode.mCoreObject);
        }
    }
}
