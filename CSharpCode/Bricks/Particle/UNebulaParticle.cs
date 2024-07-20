using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Rtti.Meta]
    public class TtNebulaParticleAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtNebulaParticle.AssetExt;
        }

        public override string GetAssetTypeName()
        {
            return "Nebula";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            //return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
            return new TtNebulaParticle();
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "nebula", null);
        //}
    }
    [TtNebulaParticle.UNebulaParticleImport]
    [IO.AssetCreateMenu(MenuName = "FX/NubulaParticle")]
    [EngineNS.Editor.UAssetEditor(EditorType = typeof(Editor.TtParticleEditor))]
    public partial class TtNebulaParticle : IO.IAsset, IDisposable
    {
        public const string AssetExt = ".nebula";
        public class UNebulaParticleImportAttribute : IO.CommonCreateAttribute
        {
            protected override bool CheckAsset()
            {
                var material = mAsset as TtNebulaParticle;
                if (material == null)
                    return false;

                return true;
            }
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtNebulaParticleAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }
            UEngine.Instance.SourceControlModule.AddFile(name.Address);

            IO.TtFileManager.SaveObjectToXml(name.Address, ParticleGraph);
            UEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        public static async Thread.Async.TtTask<TtNebulaParticle> LoadAsset(RName name, bool bForEditor)
        {
            TtNebulaParticle result;
            
            result = new TtNebulaParticle();
            result.AssetName = name;
            result.ParticleGraph = new Editor.TtParticleGraph();
            if (IO.TtFileManager.LoadXmlToObject(name.Address, result.ParticleGraph) == false)
            {
                result = new TtNebulaParticle();
                result.AssetName = name;
                var Emitter = result.AddEmitter(typeof(Simple.TtSimpleEmitter), "emitter0") as Simple.TtSimpleEmitter;
                Emitter.IsGpuDriven = true;

                await Emitter.InitEmitter(RName.GetRName("utest/mesh/unit_sphere.ums"), 1024);

                var sphereShape = new Bricks.Particle.TtShapeSphere();
                sphereShape.Radius = 10.0f;
                sphereShape.Thinness = 0.1f;
                var boxShape = new Bricks.Particle.TtShapeBox();
                boxShape.Thinness = 0.2f;
                Emitter.EmitterShapes.Add(sphereShape);
                Emitter.EmitterShapes.Add(boxShape);
                var ef1 = new TtAcceleratedEffector();
                ef1.Acceleration = new Vector3(0, -0.1f, 0);
                Emitter.AddEffector("default", ef1);
                Emitter.SetCurrentQueue("default");

                await UEngine.Instance.NebulaTemplateManager.UpdateShaders(result);
                return result;
                //return null;
            }

            await result.CreateEmitters(bForEditor);

            return result;
        }

        internal async Thread.Async.TtTask CreateEmitters(bool bForEditor)
        {
            Emitter.Clear();
            foreach (var i in ParticleGraph.Nodes)
            {
                var emtNode = i as Editor.TtEmitterNode;
                if (emtNode != null)
                {
                    var emt = this.AddEmitter(emtNode.CreateEmitterType().SystemType, emtNode.EmitterName);
                    emt.IsGpuDriven = emtNode.IsGpuDriven;
                    await emt.InitEmitter(emtNode.MeshName, emtNode.MaxParticle);

                    var shapeNode = ParticleGraph.FindOutLinkerSingle(emtNode.Shapes).InNode as Editor.TtEmitShapeNode;
                    while (shapeNode != null)
                    {
                        var shape = shapeNode.CreateShape();
                        emt.EmitterShapes.Add(shape);
                        if (bForEditor)
                            shapeNode.EditingObject = shape;
                        shapeNode = ParticleGraph.FindOutLinkerSingle(shapeNode.Right)?.InNode as Editor.TtEmitShapeNode;
                    }

                    var effectorQueueNode = ParticleGraph.FindOutLinkerSingle(emtNode.Effectors).InNode as Editor.TtEffectorQueueNode;
                    if (effectorQueueNode != null)
                    {
                        var effectorNode = ParticleGraph.FindOutLinkerSingle(effectorQueueNode.Right).InNode as Editor.TtEffectorNode;
                        while (effectorNode != null)
                        {
                            var effector = effectorNode.CreateEffector();
                            if (bForEditor)
                                effectorNode.EditingObject = effector;
                            emt.AddEffector(effectorQueueNode.QueueName, effector);
                            effectorNode = ParticleGraph.FindOutLinkerSingle(effectorNode.Right)?.InNode as Editor.TtEffectorNode;
                        }
                    }

                    if (emtNode.DefaultCurrentQueue != null)
                        emt.SetCurrentQueue(emtNode.DefaultCurrentQueue);
                }
            }
            await UEngine.Instance.NebulaTemplateManager.UpdateShaders(this);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        public void Dispose()
        {
            foreach(var i in Emitter.Values)
            {
                i.Dispose();
            }
            Emitter.Clear();
        }
        public Editor.TtParticleGraph ParticleGraph { get; set; }
        public Dictionary<string, TtEmitter> Emitter { get; } = new Dictionary<string, TtEmitter>();
        [Rtti.Meta]
        public TtEmitter AddEmitter(System.Type type, string name)
        {
            var emitter = Rtti.UTypeDescManager.CreateInstance(type) as TtEmitter;
            Emitter.Add(name, emitter);
            return emitter;
        }
        public void RemoveEmitter(string name)
        {
            Emitter.Remove(name);
        }
        public void Update(UParticleGraphNode particleSystem, float elpased)
        {
            //var cmdlist = particleSystem.BasePass.DrawCmdList;
            //cmdlist.BeginCommand();
            foreach (var i in Emitter.Values)
            {
                i.Update(particleSystem, elpased);                
            }
            //cmdlist.EndCommand();
            //policy.CommitCommandList(cmdlist);
        }

        public async Thread.Async.TtTask<TtNebulaParticle> CloneNebula()
        {
            var result = new TtNebulaParticle();
            result.ParticleGraph = ParticleGraph;
            result.AssetName = AssetName;
            foreach (var i in Emitter)
            {
                result.Emitter.Add(i.Key, i.Value.CloneEmitter());
            }
            await UEngine.Instance.NebulaTemplateManager.UpdateShaders(result);
            return result;
            //return this;
        }
    }
}
