using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;


namespace EngineNS.Bricks.HollowMaker
{
    [Rtti.MetaClassAttribute]
    public class Agent
    {
        public float AgentGridSize
        {
            get;
            set;
        } = 2.0f;

        [Rtti.MetaClassAttribute]
        public class GeoBox : IO.Serializer.Serializer
        {
            public enum BoxFace : int
            {
                StartIndex = 0,
                Front = 0,
                Back = 1,
                Left = 2,
                Right = 3,
                Top = 4,
                Bottom = 5,
                Number = 6,
            }

            //在格子中位置
            [Rtti.MetaData]
            public int X;

            [Rtti.MetaData]
            public int Y;

            [Rtti.MetaData]
            public int Z;

            public bool Walk;
            public bool CanWalk;

            [Rtti.MetaData]
            public BoundingBox Box;

            //[Rtti.MetaData]
            //public BoundingBox OrigBox;

            [Rtti.MetaData]
            public byte FaceType
            {
                get;
                set;
            }

            //[Rtti.MetaData] 手动保存
            public int[] Neighbors = new int[6] { -1, -1, -1, -1, -1, -1};
            
            [Rtti.MetaData]
            public int Index;

            public GeoBox[] LinkedBox = new GeoBox[6];
        }

        public GeoBox[,,] GeoBoxs;
        public List<BoundingBox> AgentComponenetBox;
        public Vector3 CurrentPosition = new Vector3();
        public Quaternion CurrentRotation = new Quaternion();
        public Vector3 CurrentScale = new Vector3();

        //物理反馈的数据不精确 待优化
        private bool CheckBoxInPhy(GamePlay.GWorld world, ref BoundingBox box, ref Vector3 pos, ref Quaternion rotation)//float dis, float offset
        {
            GamePlay.SceneGraph.GSceneGraph DefaultScene = world.DefaultScene;
            Bricks.PhysicsCore.CPhyScene PhyScene = DefaultScene.PhyScene;
            var shape = CEngine.Instance.PhyContext.CreateShapeBox(PhysicsCore.CPhyMaterial.DefaultPhyMaterial, box.GetSize().X, box.GetSize().Y, box.GetSize().Z);
            GamePlay.SceneGraph.VHitResult result = new GamePlay.SceneGraph.VHitResult();

            Matrix mat1 = Matrix.Transformation(Vector3.UnitXYZ, Quaternion.Identity, box.GetCenter());
            Matrix mat2 = Matrix.Transformation(Vector3.UnitXYZ, rotation, pos);
            Matrix mat3 = mat1 * mat2;

            Vector3 scale;
            Vector3 position;
            Quaternion rot;
            mat3.Decompose(out scale, out rot, out position);
            return PhyScene.Overlap(shape, position, rot, ref result);
        }

        public void BuildGeoBoxs(Vector3 pos, Vector3 scale, Quaternion rotation)
        {
            int sizex = (int)(scale.X / AgentGridSize);
            int sizez = (int)(scale.Z / AgentGridSize);

            int sizey = (int)(scale.Y / AgentGridSize);
            float startx = scale.X * -0.5f;
            float starty = scale.Y * -0.5f;
            float startz = scale.Z * -0.5f;

            CurrentScale = scale;
            CurrentPosition = pos;
            CurrentRotation = rotation;

            Matrix mat = Matrix.Transformation(Vector3.UnitXYZ, rotation, pos);
            GeoBoxs = new GeoBox[sizex, sizey, sizez];
            //Vector4 outv4 = new Vector4(); 
            for (int i = 0; i < sizex; i++)
            {
                for (int j = 0; j < sizey; j++)
                {
                    for (int k = 0; k < sizez; k++)
                    {
                        GeoBoxs[i, j, k] = new GeoBox();
                        //Vector3 min = new Vector3(startx + i * AgentGridSize, starty + j * AgentGridSize, startz + k * AgentGridSize)
                        GeoBoxs[i, j, k].Box.Minimum = new Vector3(startx + i * AgentGridSize, starty + j * AgentGridSize, startz + k * AgentGridSize);
                        GeoBoxs[i, j, k].Box.Maximum = new Vector3(startx + (i + 1) * AgentGridSize, starty + (j + 1) * AgentGridSize, startz + (k + 1) * AgentGridSize);
                        //Vector3.Transform(ref GeoBoxs[i, j, k].OrigBox.Minimum, ref mat, out outv4);
                        //GeoBoxs[i, j, k].Box.Minimum = new Vector3(outv4.X, outv4.Y, outv4.Z);
                        //Vector3.Transform(ref GeoBoxs[i, j, k].OrigBox.Maximum, ref mat, out outv4);
                        //GeoBoxs[i, j, k].Box.Maximum = new Vector3(outv4.X, outv4.Y, outv4.Z);
                        GeoBoxs[i, j, k].FaceType = 0;
                        GeoBoxs[i, j, k].Index = -1;

                        GeoBoxs[i, j, k].X = i;
                        GeoBoxs[i, j, k].Y = j;
                        GeoBoxs[i, j, k].Z = k;
                    }
                }
            }
            BuildBoxLinker();
        }

        private void BuildBoxLinker()
        {
            for (int x = 0; x < GeoBoxs.GetLength(0); x++)
            {
                for (int y = 0; y < GeoBoxs.GetLength(1); y++)
                {
                    for (int z = 0; z < GeoBoxs.GetLength(2); z++)
                    {
                        var curBox = GeoBoxs[x, y, z];
                        curBox.CanWalk = true;//初始化为都能走
                        curBox.Walk = false;//初始化为都没走过

                        if (z - 1>=0)
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Front] = GeoBoxs[x, y, z - 1];
                        if (z + 1 <= GeoBoxs.GetLength(2) - 1)
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Back] = GeoBoxs[x, y, z + 1];
                        if (x - 1 >= 0)
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Left] = GeoBoxs[x - 1, y, z];
                        if (x + 1 <= GeoBoxs.GetLength(0) - 1)
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Right] = GeoBoxs[x + 1, y, z];
                        if (y + 1 <= GeoBoxs.GetLength(1) - 1)
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Top] = GeoBoxs[x, y + 1, z];
                        if (y - 1 >= 0)
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Bottom] = GeoBoxs[x, y - 1, z];
                    }
                }
            }
        }

        public void BuildWalkables2(GamePlay.GWorld world, Stack<GeoBox> stack)
        {
            while(stack.Count>0)
            {
                GeoBox curBox = stack.Pop();
                foreach (var i in curBox.LinkedBox)
                {
                    if (i == null)
                        continue;//边缘
                    if (i.Walk)
                        continue;//已经走过了
                    if (i.CanWalk == false)
                        continue;//一个阻塞了
                    if (CheckBoxInPhy(world, ref i.Box, ref CurrentPosition, ref CurrentRotation))
                    {
                        i.CanWalk = false;
                    }
                    else
                    {
                        i.Walk = true;
                        i.CanWalk = true;
                        stack.Push(i);
                    }
                }
            }
        }

        public void BuildWalkables(GamePlay.GWorld world, GeoBox curBox, int curDepth, ref int maxDepth)
        {//这里要递归改回溯算法，否则堆栈吃不消
            if (curDepth > maxDepth)
                maxDepth = curDepth;
            curBox.Walk = true;
            curBox.CanWalk = true;
            foreach(var i in curBox.LinkedBox)
            {
                if (i == null)
                    continue;//边缘
                if (i.Walk)
                    continue;//已经走过了
                if (i.CanWalk == false)
                    continue;//一个阻塞了
                if (CheckBoxInPhy(world, ref i.Box, ref CurrentPosition, ref CurrentRotation))
                {
                    i.CanWalk = false;
                }
                else
                {
                    BuildWalkables(world, i, curDepth + 1, ref maxDepth);
                }
            }
        }
        public void BuildGeomScene(GeomScene geomscene)
        {
            for (int x = 0; x < GeoBoxs.GetLength(0); x++)
            {
                for (int y = 0; y < GeoBoxs.GetLength(1); y++)
                {
                    for (int z = 0; z < GeoBoxs.GetLength(2); z++)
                    {
                        var curBox = GeoBoxs[x, y, z];
                        if(curBox.CanWalk && curBox.Walk)
                        {
                            curBox.Index = geomscene.AgentData.Count;
                            geomscene.AgentData.Add(curBox);
                        }
                    }
                }
            }
            for (int x = 0; x < GeoBoxs.GetLength(0); x++)
            {
                for (int y = 0; y < GeoBoxs.GetLength(1); y++)
                {
                    for (int z = 0; z < GeoBoxs.GetLength(2); z++)
                    {
                        var curBox = GeoBoxs[x, y, z];
                        if (curBox.LinkedBox[(int)GeoBox.BoxFace.Front] != null &&
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Front].CanWalk)
                        {
                            curBox.Neighbors[(int)GeoBox.BoxFace.Front] = curBox.LinkedBox[(int)GeoBox.BoxFace.Front].Index;
                        }
                        if (curBox.LinkedBox[(int)GeoBox.BoxFace.Back] != null &&
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Back].CanWalk)
                        {
                            curBox.Neighbors[(int)GeoBox.BoxFace.Back] = curBox.LinkedBox[(int)GeoBox.BoxFace.Back].Index;
                        }
                        if (curBox.LinkedBox[(int)GeoBox.BoxFace.Left] != null &&
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Left].CanWalk)
                        {
                            curBox.Neighbors[(int)GeoBox.BoxFace.Left] = curBox.LinkedBox[(int)GeoBox.BoxFace.Left].Index;
                        }
                        if (curBox.LinkedBox[(int)GeoBox.BoxFace.Right] != null &&
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Right].CanWalk)
                        {
                            curBox.Neighbors[(int)GeoBox.BoxFace.Right] = curBox.LinkedBox[(int)GeoBox.BoxFace.Right].Index;
                        }
                        if (curBox.LinkedBox[(int)GeoBox.BoxFace.Top] != null &&
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Top].CanWalk)
                        {
                            curBox.Neighbors[(int)GeoBox.BoxFace.Top] = curBox.LinkedBox[(int)GeoBox.BoxFace.Top].Index;
                        }
                        if (curBox.LinkedBox[(int)GeoBox.BoxFace.Bottom] != null &&
                            curBox.LinkedBox[(int)GeoBox.BoxFace.Bottom].CanWalk)
                        {
                            curBox.Neighbors[(int)GeoBox.BoxFace.Bottom] = curBox.LinkedBox[(int)GeoBox.BoxFace.Bottom].Index;
                        }
                    }
                }
            }
        }
    }
}
