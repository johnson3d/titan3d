using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AdvanceShadow
{
    public struct FGpuTile
    {
        public ushort TextureIndex;
        public ushort ArrayIndex;
        public Matrix ShadowMatrix;
    }
    public class TtShadowMapTile
    {
        public FGpuTile GpuTile;
        public List<GamePlay.Scene.TtNode> ShadowNodes;
        public Hash160 ShadowHash;
        public unsafe Hash160 UpdateShadowHash(List<GamePlay.Scene.TtNode> nodes)
        {
            nodes.Sort();
            using (var ar = IO.UMemWriter.CreateInstance())
            {
                foreach(var i in nodes)
                {
                    var nh = i.GetHashCode();
                    ar.WritePtr(&nh, sizeof(int));
                }
                return Hash160.CreateHash160(ar.Ptr, (uint)ar.GetPosition());
            }   
        }
        public bool CheckDirty()
        {
            var newHash = UpdateShadowHash(ShadowNodes);
            if (newHash != ShadowHash)
            {
                ShadowHash = newHash;
                return true;
            }
            return false;
        }
        public void PushShadowNode(GamePlay.Scene.TtNode node)
        {
            if (ShadowNodes.Contains(node) == false)
            {
                ShadowNodes.Add(node);
            }
        }
    }
    public class TtClipmapLayer
    {
        public int[,] Tiles;
        public int SideX
        {
            get=>Tiles.GetLength(1);
        }
        public int SideZ
        {
            get => Tiles.GetLength(0);
        }
        public int Level;
        public float TileSize;
        public int StartX;
        public int EndX;
        public int StartZ;
        public int EndZ;
        public void SetValidSize(float sz)
        {
            int side = (int)(sz / TileSize);
            StartX = (SideX - side) / 2;
            StartZ = (SideZ - side) / 2;
            EndX = StartX + side;
            EndZ = StartZ + side;
        }
        public int GetTile(TtAdvanceShadow context, float z, float x)
        {
            int iz = (int)(z / TileSize);
            int ix = (int)(x / TileSize);
            return GetTileI(context, iz, ix);
        }
        public int GetTileI(TtAdvanceShadow context, int iz, int ix)
        {
            if (Tiles[iz, ix] == -1)
            {
                Tiles[iz, ix] = context.AllocTile();
            }
            return Tiles[iz, ix];
        }
        public TtShadowMapTile TryGetTileObject(TtAdvanceShadow context, int iz, int ix)
        {
            if (ix < StartX || ix > EndX || iz < StartZ || iz > EndZ)
                return null;
            return context.TilePool[GetTileI(context, iz, ix)];
        }
        public void ResetShadowNodes(TtAdvanceShadow context)
        {
            foreach (var i in Tiles)
            {
                if (i == -1)
                    continue;
                context.TilePool[i].ShadowNodes.Clear();
            }
        }
        public void UpdateShadowMap(TtAdvanceShadow context)
        {
            foreach (var i in Tiles)
            {
                if (i == -1)
                    continue;
                if (context.TilePool[i].CheckDirty())
                {
                    //foreach Tiles => Draw ShadowMap
                }
            }
        }
    }
    public class TtAdvanceShadow
    {
        public TtShadowMapTile[] TilePool;
        public int AllocTile()
        {
            return -1;
        }
        public void FreeTile(int index)
        {

        }
        public TtClipmapLayer[] Layers;
        public bool Initialize(int clipLevel, float coverSize, int poolsize)
        {
            Layers = new TtClipmapLayer[clipLevel];
            for (int i = 0; i < clipLevel; i++)
            {
                Layers[i] = new TtClipmapLayer();
                Layers[i].Level = i;
                Layers[i].TileSize = coverSize;
                Layers[i].Tiles = new int[i,i];
                for (int z = 0; z < i; z++)
                {
                    for (int x = 0; x < i; x++)
                    {
                        Layers[i].Tiles[z, x] = -1;
                    }
                }
                coverSize /= 2;
            }
            TilePool = new TtShadowMapTile[poolsize];
            return true;
        }
        public TtShadowMapTile GetMipTile(float z, float x, int level)
        {
            return TilePool[Layers[level].GetTile(this, z, x)];
        }
        public void PushShadowNode(GamePlay.Scene.TtNode node)
        {
            //project node.AbsAABB as 2d box
            BoundingBox coverBox = new BoundingBox();
            for (int i = 0; i < Layers.Length; i++)
            {
                //test cover
                int sx, ex;
                int sz, ez;
                sx = (int)(coverBox.Minimum.X / Layers[i].TileSize);
                ex = (int)(coverBox.Maximum.X / Layers[i].TileSize);
                sz = (int)(coverBox.Minimum.Z / Layers[i].TileSize);
                ez = (int)(coverBox.Maximum.Z / Layers[i].TileSize);

                for (int z = sz; z < ez; z++)
                {
                    for (int x = sz; x < ex; x++)
                    {
                        var tile = Layers[i].TryGetTileObject(this, z, x);
                        if (tile != null)
                        {
                            tile.PushShadowNode(node);
                        }
                    }
                }
            }
        }
        public void UpdateShadowMap(List<GamePlay.Scene.TtNode> nodes)
        {
            foreach (var i in Layers)
            {
                i.ResetShadowNodes(this);
            }
            foreach (var i in nodes)
            {
                PushShadowNode(i);
            }
            foreach (var i in Layers)
            {
                i.UpdateShadowMap(this);
            }
        }
    }
}
