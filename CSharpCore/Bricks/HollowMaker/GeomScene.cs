using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.HollowMaker
{
    public class GeomScene
    {
        // Temp.. 方便运算
        public List<Agent.GeoBox> AgentData = new List<Agent.GeoBox>();
        public class AgentBoxs
        {
            public List<Agent.GeoBox> AgentData = new List<Agent.GeoBox>();
            //public Vector3 Postion = new Vector3();
            //public Quaternion Rotation = new Quaternion();
            public Matrix Mat = new Matrix();
            public Vector3 BVSize;
        }

        public List<AgentBoxs> AgentDatas = new List<AgentBoxs>();

        byte face = (byte)(EngineNS.Graphics.Mesh.CGfxMeshCooker.EBoxFace.Front |
            EngineNS.Graphics.Mesh.CGfxMeshCooker.EBoxFace.Back | EngineNS.Graphics.Mesh.CGfxMeshCooker.EBoxFace.Bottom |
            EngineNS.Graphics.Mesh.CGfxMeshCooker.EBoxFace.Left | EngineNS.Graphics.Mesh.CGfxMeshCooker.EBoxFace.Right | EngineNS.Graphics.Mesh.CGfxMeshCooker.EBoxFace.Top);
        public static string ActorName = "Lineshelper";
        public async System.Threading.Tasks.Task CreateRenderInfos(GamePlay.GWorld world)
        {
            //EngineNS.Bricks.GraphDrawer.GraphLines
            AgentBoxs ab = AgentDatas[0];
            Agent.GeoBox[] ap = ab.AgentData.ToArray();
            GamePlay.Actor.GActor actor = new GamePlay.Actor.GActor();
            //创建包围盒的每个点
            for (int i = 0; i < ap.Length; i ++)
            {

                EngineNS.Bricks.GraphDrawer.GraphLines graph = new EngineNS.Bricks.GraphDrawer.GraphLines();
                var boxgen = new EngineNS.Bricks.GraphDrawer.McBoxGen();
                bool allface = ap[i].FaceType == face;
                boxgen.Interval =   allface ? 0.05f : 0.1f;
                boxgen.Segement = allface ? 1.0f : 0.2f;

                Vector4 outv4;
                Vector3.Transform(ref ap[i].Box.Maximum, ref ab.Mat, out outv4);
                ap[i].Box.Maximum = new Vector3(outv4.X, outv4.Y, outv4.Z);
                Vector3.Transform(ref ap[i].Box.Minimum, ref ab.Mat, out outv4);
                ap[i].Box.Minimum = new Vector3(outv4.X, outv4.Y, outv4.Z);
                boxgen.SetBoundBox(ap[i].Box);

                graph.LinesGen = boxgen;
                var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
                CEngine.Instance.RenderContext,
                //蓝线就是六个面都通了
                RName.GetRName(allface ? "editor/icon/icon_3D/material/physical_model.instmtl" : "editor/volume/mi_volume_octree.instmtl" ));//rotator

                var init = await graph.Init(mtl, 0.0f);

                graph.GraphActor.Placement.Location = Vector3.Zero;
                graph.GraphActor.SpecialName = ActorName;
                //world.AddActor(graph.GraphActor);
                //world.DefaultScene.AddActor(graph.GraphActor);
                actor.Children.Add(graph.GraphActor);

            }

            actor.SpecialName = "NavLineDebuger";
            world.AddActor(actor);
            world.DefaultScene.AddActor(actor);
        }

        public void SaveXND(string name)
        {
            if (AgentDatas == null || AgentDatas.Count == 0)
                return;
  
            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.AddAttrib("GeomSceneData");
            attr.BeginWrite();
            attr.Write(AgentDatas.Count);
            foreach (AgentBoxs data in AgentDatas)
            {
                attr.Write(data.BVSize);
                attr.Write(data.Mat);
                attr.Write(data.AgentData.Count);
                foreach (var geobox in data.AgentData)
                {
                    attr.WriteMetaObject(geobox);
                    for (Agent.GeoBox.BoxFace i = Agent.GeoBox.BoxFace.StartIndex; i < Agent.GeoBox.BoxFace.Number; i++)
                    {
                        int index = geobox.Neighbors[(int)i];
                        attr.Write(index);
                    }   
                }
            }
            attr.EndWrite();

            EngineNS.IO.XndHolder.SaveXND(name, xnd);
            xnd.Node.TryReleaseHolder();
        }

        public static async System.Threading.Tasks.Task<GeomScene> CreateGeomScene(string name)
        {
            var xnd = await EngineNS.IO.XndHolder.LoadXND(name);
            if (xnd == null)
                return new GeomScene();

 
            var attr = xnd.Node.FindAttrib("GeomSceneData");
            if (attr == null)
                return new GeomScene();

            GeomScene scenegeo = new GeomScene();
            scenegeo.AgentDatas = new List<AgentBoxs>();
            attr.BeginRead();
            int count = 0;
            attr.Read(out count);
            for (int i = 0; i < count; i++)
            {
                AgentBoxs aboxs = new AgentBoxs();
                int datacount = 0;
                attr.Read(out aboxs.BVSize);
                attr.Read(out aboxs.Mat);
                attr.Read(out datacount);
                //List<Agent.GeoBox> data = new List<Agent.GeoBox>();
                for (int k = 0; k < datacount; k++)
                {
                    Agent.GeoBox geobox = new Agent.GeoBox();
                    attr.ReadMetaObject(geobox);

                    for (int j = 0; j < 6; j++)
                    {
                        int index;
                        attr.Read(out index);
                        geobox.Neighbors[j] = index;
                    }

                    aboxs.AgentData.Add(geobox);
                }
                scenegeo.AgentDatas.Add(aboxs);
            }
            attr.EndRead();
            xnd.Node.TryReleaseHolder();
            return scenegeo;
        }
    }
}
