using EngineNS.Graphics.Pipeline;
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
        public TtCamera ShadowCamera = new TtCamera();
        public unsafe Hash160 UpdateShadowHash(List<GamePlay.Scene.TtNode> nodes, ref DBoundingBox aabb)
        {
            nodes.Sort();
            aabb.InitEmptyBox();
            using (var ar = IO.TtMemWriter.CreateInstance())
            {
                foreach(var i in nodes)
                {
                    var nh = i.GetHashCode();
                    ar.WritePtr(&nh, sizeof(int));
                    aabb.Merge(in i.AbsAABB);
                }
                return Hash160.CreateHash160(ar.Ptr, (uint)ar.GetPosition());
            }   
        }
        public bool CheckDirty(ref DBoundingBox aabb)
        {
            var newHash = UpdateShadowHash(ShadowNodes, ref aabb);
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
            for (int z = 0; z < SideZ; z++)
            {
                for (int x = 0; x < SideX; x++)
                {
                    int index = Tiles[z, x];
                    if (index == -1)
                        continue;
                    context.TilePool[index].ShadowNodes.Clear();
                }
            }
        }
        public void UpdateShadowMap(TtAdvanceShadow context)
        {
            for (int z = 0; z < SideZ; z++)
            {
                for (int x = 0; x < SideX; x++)
                {
                    int index = Tiles[z, x];
                    if (index == -1)
                        continue;
                    var aabb = new DBoundingBox();
                    if (context.TilePool[index].CheckDirty(ref aabb))
                    {
                        //foreach Tiles => Draw ShadowMap
                        DrawShadowMap(context, context.TilePool[index], z, x, in aabb);
                    }
                }
            }
        }
        protected void DrawShadowMap(TtAdvanceShadow context, TtShadowMapTile tile, int z, int x, in DBoundingBox aabb)
        {
            tile.ShadowCamera.DoOrthoProjectionForShadow(TileSize, TileSize, 1, 1000, 0, 0);
            var fx = (float)x * TileSize;
            var fz = (float)z * TileSize;
            var center = new DVector3(fx * 0.5f, 0, fz * 0.5f);
            var FrustumSphereDiameter = aabb.GetSize().Length();
            tile.ShadowCamera.LookAtLH(center - context.LightDir * FrustumSphereDiameter, center, in Vector3.UnitY);
        }
    }
    public class TtAdvanceShadow
    {
        public DVector3 StartOffset = DVector3.Zero;
        public TtShadowMapTile[] TilePool;
        public DVector3 LightDir;
        public int AllocTile()
        {
            return -1;
        }
        public void FreeTile(int index)
        {

        }
        public float TextureTileSize = 128.0f;
        public TtClipmapLayer[] Layers;
        public bool Initialize(int clipLevel, float coverSize, int poolsize, float texTileSize = 128.0f)
        {
            TextureTileSize = texTileSize;
            Layers = new TtClipmapLayer[clipLevel];
            int side = 1;
            for (int i = 0; i < clipLevel; i++)
            {
                Layers[i] = new TtClipmapLayer();
                Layers[i].Level = i;
                Layers[i].TileSize = coverSize;
                Layers[i].Tiles = new int[side, side];
                for (int z = 0; z < side; z++)
                {
                    for (int x = 0; x < side; x++)
                    {
                        Layers[i].Tiles[z, x] = -1;
                    }
                }
                side *= 2;
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
            var coverBox = new DBoundingBox();
            //project node.AbsAABB as 2d box
            coverBox.Minimum = node.AbsAABB.Minimum - StartOffset;
            coverBox.Maximum = node.AbsAABB.Maximum - StartOffset;

            for (int i = 0; i < Layers.Length; i++)
            {
                //test cover
                int sx, ex;
                int sz, ez;
                sx = (int)(coverBox.Minimum.X / Layers[i].TileSize);
                ex = (int)(coverBox.Maximum.X / Layers[i].TileSize);
                sz = (int)(coverBox.Minimum.Z / Layers[i].TileSize);
                ez = (int)(coverBox.Maximum.Z / Layers[i].TileSize);

                sx = Math.Max(sx, 0);
                ex = Math.Min(ex, Layers[i].SideX - 1);
                sz = Math.Max(sz, 0);
                ez = Math.Min(ez, Layers[i].SideZ - 1);

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
