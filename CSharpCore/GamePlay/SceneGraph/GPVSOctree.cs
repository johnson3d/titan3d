using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.SceneGraph
{
    [Rtti.MetaClass]
    public class OctreeDesc : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        [DisplayName("最大深度")]
        public int MaxDepth
        {
            get;
            set;
        } = 10;

        [Rtti.MetaData]
        [DisplayName("最大对象数量")]
        public int MaxCellsCount
        {
            get;
            set;
        } = 10;

        [Rtti.MetaData]
        [DisplayName("最小边长")]
        public float MinSide
        {
            get;
            set;
        } = 5.0f;
    }
    [Rtti.MetaClass]
    public class GPVSOctreeNode : IO.Serializer.Serializer
    {
        BoundingBox mOrigionAABB;

        [Rtti.MetaData]
        public BoundingBox OrigionAABB
        {
            get => mOrigionAABB;
            set
            {
                mOrigionAABB = value;
            }
        }
        BoundingBox mExtendAABB;
        [Rtti.MetaData]
        public BoundingBox ExtendAABB
        {
            get => mExtendAABB;
            set
            {
                mExtendAABB = value;
            }
        }

        public Byte Depth
        {
            get;
            private set;
        }
        public void SetDepth(Byte depth)
        {
            Depth = depth;
        }

        public GPVSOctreeNode Parent
        {
            get;
            set;
        }
        public List<GPVSOctreeNode> Children
        {
            get;
        } = new List<GPVSOctreeNode>();

        public List<GPvsCell> mPVSCells = new List<GPvsCell>();

        public async Task<bool> BuildTree(List<GPvsCell> cells, OctreeDesc desc)
        {
            mPVSCells.Clear();
            if(cells.Count <= desc.MaxCellsCount || Depth >= desc.MaxDepth)
            {
                // 不能再拆分
                for(int i=0; i<cells.Count; i++)
                {
                    var cell = cells[i];
                    mPVSCells.Add(cell);
                }
            }
            else
            {
                var size = mOrigionAABB.Maximum - mOrigionAABB.Minimum;
                if(System.Math.Max(System.Math.Max(size.X, size.Y), size.Z) < desc.MinSide)
                {
                    // 不能再拆分
                    for(int i=0; i<cells.Count; i++)
                    {
                        var cell = cells[i];
                        mPVSCells.Add(cell);
                    }
                }
                else
                {
                    Children.Clear();
                    // 拆分并计算归属
                    var corners = mOrigionAABB.GetCorners();
                    var center = mOrigionAABB.GetCenter();
                    for(int i=0; i<8; i++)
                    {
                        var childNode = new GPVSOctreeNode();
                        childNode.Parent = this;
                        childNode.SetDepth((byte)(Depth + 1));
                        var min = new Vector3();
                        min.X = System.Math.Min(corners[i].X, center.X);
                        min.Y = System.Math.Min(corners[i].Y, center.Y);
                        min.Z = System.Math.Min(corners[i].Z, center.Z);
                        var max = new Vector3();
                        max.X = System.Math.Max(corners[i].X, center.X);
                        max.Y = System.Math.Max(corners[i].Y, center.Y);
                        max.Z = System.Math.Max(corners[i].Z, center.Z);
                        childNode.OrigionAABB = new BoundingBox(min, max);

                        var childCells = new List<GPvsCell>();
                        foreach(var cell in cells)
                        { 
                            var cellAABB = cell.BoundVolume;
                            var cellCenter = cellAABB.GetCenter();
                            cellCenter = Vector3.TransformCoordinate(cellCenter, cell.HostSet.WorldMatrix);
                            if(childNode.OrigionAABB.Contains(ref cellCenter) != ContainmentType.Disjoint)
                            {
                                childCells.Add(cell);
                            }
                        }
                        // 内部对象为0则不加这个子节点
                        if(childCells.Count != 0)
                        {
                            await childNode.BuildTree(childCells, desc);
                            Children.Add(childNode);
                        }
                        else if(childCells.Count == cells.Count)
                        {
                            // 子与父对象相同，不再进行拆分
                            Children.Clear();
                            foreach(var cell in cells)
                            {
                                mPVSCells.Add(cell);
                            }
                            break;
                        }
                        else
                        {
                            foreach(var cell in childCells)
                            {
                                cells.Remove(cell);
                            }
                        }
                    }
                }
            }
            // 根据cell和子节点计算扩展包围盒
            mExtendAABB = mOrigionAABB;
            foreach(var cell in mPVSCells)
            {
                var corners = cell.BoundVolume.GetCorners();
                foreach(var cor in corners)
                {
                    var tempPos = Vector3.TransformCoordinate(cor, cell.HostSet.WorldMatrix);
                    mExtendAABB.Merge(ref tempPos);
                }
            }
            foreach(var child in Children)
            {
                var extAABB = child.ExtendAABB;
                mExtendAABB.Merge2(ref extAABB, ref mExtendAABB);
            }

            return true;
        }

        public bool CheckVisible(CCommandList cmd, GSceneGraph scene, Graphics.CGfxCamera camera, CheckVisibleParam param)
        {
            var camPos = camera.CullingFrustum.TipPos;
            if (ExtendAABB.Contains(ref camPos) == ContainmentType.Disjoint)
                return false;

            for (int i=0; i<Children.Count; i++)
            {
                if (Children[i].CheckVisible(cmd, scene, camera, param))
                    return true;
            }
            bool pvsChecked = false;
            Vector3 tempPos;
            for(int i=0; i<mPVSCells.Count; i++)
            {
                var cell = mPVSCells[i];
                Vector3.TransformCoordinate(ref camPos, ref cell.HostSet.WorldMatrixInv, out tempPos);
                if (cell.BoundVolume.Contains(ref tempPos) == ContainmentType.Disjoint)
                    continue;

                cell.OnCheckVisible(cmd, scene, camera, param);
                pvsChecked = true;
            }
            return pvsChecked;
        }

        public async Task<bool> Save2Xnd(IO.XndNode node, GSceneGraph scene)
        {
            var headAtt = node.AddAttrib("Head");
            headAtt.BeginWrite();
            headAtt.WriteMetaObject(this);
            headAtt.EndWrite();

            var cellAtt = node.AddAttrib("Cell");
            cellAtt.BeginWrite();
            cellAtt.Write((int)(mPVSCells.Count));
            foreach(var cell in mPVSCells)
            {
                var idx = scene.PvsSets.IndexOf(cell.HostSet);
                if (idx < 0)
                    throw new InvalidOperationException("PVSCell所属set没有找到");
                cellAtt.Write(idx);
                cellAtt.WriteMetaObject(cell);
            }
            cellAtt.EndWrite();

            var childNode = node.AddNode("Children", 0, 0);
            foreach(var child in Children)
            {
                var cNode = childNode.AddNode("child", 0, 0);
                await child.Save2Xnd(cNode, scene);
            }

            return true;
        }

        public async Task<bool> LoadXnd(IO.XndNode node, GSceneGraph scene)
        {
            var headAtt = node.FindAttrib("Head");
            if(headAtt != null)
            {
                headAtt.BeginRead();
                headAtt.ReadMetaObject(this);
                headAtt.EndRead();
            }

            var cellAtt = node.FindAttrib("Cell");
            if(cellAtt != null)
            {
                cellAtt.BeginRead();
                int cellCount;
                cellAtt.Read(out cellCount);
                mPVSCells = new List<GPvsCell>(cellCount);
                for(int i=0; i<cellCount; i++)
                {
                    int idx;
                    cellAtt.Read(out idx);
                    var cell = new GPvsCell();
                    cellAtt.ReadMetaObject(cell);
                    // 这里应该都能对应的上，不做越界判断
                    cell.HostSet = scene.PvsSets[idx];
                    mPVSCells.Add(cell);
                }
                cellAtt.EndRead();
            }

            var childNode = node.FindNode("Children");
            Children.Clear();
            if (childNode != null)
            {
                var nodes = childNode.GetNodes();
                for(int i=0; i<nodes.Count; i++)
                {
                    var cNode = nodes[i];
                    var child = new GPVSOctreeNode();
                    await child.LoadXnd(cNode, scene);
                    Children.Add(child);
                }
            }

            return true;
        }
    }

    [Rtti.MetaClass]
    public class GPVSOctree : IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public GPVSOctreeNode Root
        {
            get;
            private set;
        }

        OctreeDesc mDesc = new OctreeDesc();

        public async Task<bool> BuildTree(List<GPvsSet> pvsSets)
        {
            Root = new GPVSOctreeNode();
            var aabb = BoundingBox.EmptyBox();
            foreach(var set in pvsSets)
            {
                foreach(var cell in set.PvsCells)
                {
                    var corners = cell.BoundVolume.GetCorners();
                    for(int i=0; i<corners.Length; i++)
                    {
                        var tempPos = Vector3.TransformCoordinate(corners[i], set.WorldMatrix);
                        aabb.Merge(ref tempPos);
                    }
                }
            }
            Root.OrigionAABB = aabb;
            Root.SetDepth(0);

            var allCells = new List<EngineNS.GamePlay.SceneGraph.GPvsCell>(pvsSets.Count * 20);
            foreach(var set in pvsSets)
            {
                allCells.AddRange(set.PvsCells);
            }
            return await Root.BuildTree(allCells, mDesc);
        }
        public static Profiler.TimeScope ScopeCheckVisible = Profiler.TimeScopeManager.GetTimeScope(typeof(GPVSOctree), nameof(CheckVisible));
        public bool CheckVisible(CCommandList cmd, GSceneGraph scene, Graphics.CGfxCamera camera, CheckVisibleParam param)
        {
            if (Root == null)
                return false;
            ScopeCheckVisible.Begin();
            var result = Root.CheckVisible(cmd, scene, camera, param);
            ScopeCheckVisible.End();
            return result;
        }

        public async Task<bool> Save2Xnd(EngineNS.IO.XndNode node, GSceneGraph scene)
        {
            var att = node.AddAttrib("Head");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();

            if(Root != null)
            {
                var childNode = node.AddNode("Root", 0, 0);
                return await Root.Save2Xnd(childNode, scene);
            }
            return false;
        }

        public async Task<bool> LoadXnd(EngineNS.IO.XndNode node, GSceneGraph scene)
        {
            var att = node.FindAttrib("Head");
            if(att != null)
            {
                att.BeginRead();
                att.ReadMetaObject(this);
                att.EndRead();
            }

            var rootNode = node.FindNode("Root");
            if(rootNode != null)
            {
                Root = new GPVSOctreeNode();
                return await Root.LoadXnd(rootNode, scene);
            }

            return false;
        }
    }
}
