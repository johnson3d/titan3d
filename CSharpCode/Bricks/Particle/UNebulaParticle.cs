using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Rtti.Meta("")]
    public class TtNebulaParticleAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtNebulaParticle.AssetExt;
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.NebulaBoderColor;
        }
        public override string GetAssetTypeName()
        {
            return "Nebula";
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.NebulaTemplateManager.GetParticle(GetAssetName());
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
    [TtNebulaParticle.TtNebulaParticleImport]
    [IO.AssetCreateMenu(MenuName = "FX/NebulaParticle")]
    [EngineNS.Editor.UAssetEditor(EditorType = typeof(Editor.TtParticleEditor))]
    public partial class TtNebulaParticle : IO.BaseSerializer, IO.IAsset, IDisposable
    {
        public const string AssetExt = ".nebula";
        public string TypeExt { get => AssetExt; }
        public class TtNebulaParticleImportAttribute : IO.CommonCreateAttribute
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
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            if (ParticleGraph == null)
                return;
            ameta.RefAssetRNames.Clear();
            foreach (var i in ParticleGraph.Nodes)
            {
                var emtNode = i as Editor.TtEmitterNode;
                if (emtNode != null)
                {
                    ameta.AddReferenceAsset(emtNode.MeshName);
                }
            }
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            name.AMeta.AddAssetFile(name.Address);
            
            IO.TtFileManager.SaveObjectToXml(name.Address, this);
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        public static async Thread.Async.TtTask<TtNebulaParticle> LoadAsset(RName name, bool bForEditor)
        {
            TtNebulaParticle result;
            
            result = new TtNebulaParticle();
            result.AssetName = name;
            result.ParticleGraph = new Editor.TtParticleGraph();
            if (IO.TtFileManager.LoadXmlToObject(name.Address, result) == false)
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
                ef1.AccelerationMin = new Vector3(0, -0.1f, 0);
                ef1.AccelerationRange = new Vector3(0, 0.1f, 0);
                Emitter.AddEffector("default", ef1);
                Emitter.SetCurrentQueue("default");

                await TtEngine.Instance.NebulaTemplateManager.UpdateShaders(result);
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
                if (emtNode != null && emtNode.Enable)
                {
                    var emt = this.AddEmitter(emtNode.CreateEmitterType().SystemType, emtNode.EmitterName);
                    emt.IsGpuDriven = emtNode.IsGpuDriven;
                    await emt.InitEmitter(emtNode.MeshName, emtNode.MaxParticle);
                    emtNode.InitEmitter(emt);
                    if (bForEditor)
                        emtNode.EditingObject = emt;

                    var shapeNode = ParticleGraph.FindOutLinkerSingle(emtNode.Shapes).InNode as Editor.TtEmitShapeNode;
                    while (shapeNode != null)
                    {
                        var shape = shapeNode.CreateShape();
                        emt.EmitterShapes.Add(shape);
                        if (bForEditor)
                            shapeNode.EditingObject = shape;
                        shapeNode = ParticleGraph.FindOutLinkerSingle(shapeNode.Right)?.InNode as Editor.TtEmitShapeNode;
                    }

                    var effetorLinker = ParticleGraph.FindOutLinkerSingle(emtNode.Effectors);
                    if(effetorLinker != null)
                    {
                        var effectorQueueNode = effetorLinker.InNode as Editor.TtEffectorQueueNode;
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
                    }

                    if (emtNode.DefaultCurrentQueue != null)
                        emt.SetCurrentQueue(emtNode.DefaultCurrentQueue);
                }
            }
            await TtEngine.Instance.NebulaTemplateManager.UpdateShaders(this);

            mMcObject?.Get()?.OnCreated(this);
        }
        [Rtti.Meta("")]
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
        [Rtti.Meta("")]
        public Editor.TtParticleGraph ParticleGraph { get; set; }
        public Dictionary<string, TtEmitter> Emitter { get; } = new Dictionary<string, TtEmitter>();
        [Rtti.Meta("")]
        public int ParticleNumOfTask { get; set; } = 50;
        [Rtti.Meta("")]
        public TtEmitter AddEmitter(System.Type type, string name)
        {
            var emitter = Rtti.TtTypeDescManager.CreateInstance(type) as TtEmitter;
            Emitter[name] = emitter;
            return emitter;
        }
        public void RemoveEmitter(string name)
        {
            Emitter.Remove(name);
        }
        public void Update(Graphics.Pipeline.TtRenderPolicy policy, UParticleGraphNode particleSystem, float elpased, Vector3 Location)
        {
            mMcObject?.Get()?.OnUpdate(this, particleSystem, elpased);

            //var cmdlist = particleSystem.BasePass.DrawCmdList;
            //cmdlist.BeginCommand();
            foreach (var i in Emitter.Values)
            {
                mMcObject?.Get()?.OnUpdateEmitter(this, i, particleSystem, elpased);
                i.Update(this, policy, particleSystem, elpased, Location);                
            }
            //cmdlist.EndCommand();
            //policy.CommitCommandList(cmdlist);
        }
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = CodeBuilder.TtMacross.AssetExt, MacrossType = typeof(TtNebulaMacross))]
        public RName McName
        {
            get
            {
                if (mMcObject == null)
                    return null;
                return mMcObject.Name;
            }
            set
            {
                if (value == null)
                {
                    mMcObject = null;
                    return;
                }
                if (mMcObject == null)
                {
                    mMcObject = Macross.TtMacrossGetter<TtNebulaMacross>.NewInstance();
                }
                mMcObject.Name = value;
            }
        }
        Macross.TtMacrossGetter<TtNebulaMacross> mMcObject;
        public async Thread.Async.TtTask<TtNebulaParticle> CloneNebula()
        {
            var result = new TtNebulaParticle();
            result.ParticleGraph = ParticleGraph;
            result.AssetName = AssetName;
            foreach (var i in Emitter)
            {
                result.Emitter.Add(i.Key, i.Value.CloneEmitter());
            }
            await TtEngine.Instance.NebulaTemplateManager.UpdateShaders(result);
            return result;
            //return this;
        }
    }

    [Macross.TtMacross]
    public partial class TtNebulaMacross
    {
        [Rtti.Meta("")]
        public virtual void OnCreated(TtNebulaParticle nebula)
        {

        }
        [Rtti.Meta("")]
        public virtual void OnUpdate(TtNebulaParticle nebula, UParticleGraphNode particleSystem, float elpased)
        {

        }
        [Rtti.Meta("")]
        public virtual void OnUpdateEmitter(TtNebulaParticle nebula, TtEmitter emitter, UParticleGraphNode particleSystem, float elpased)
        {

        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.Particle
{
	partial class TtNebulaParticle
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_AddEmitter_3553406330 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtNebulaParticle->TtEmitter AddEmitter(System.Type type, string name)");
		public unsafe TtEmitter macross_AddEmitter (string nodeName, System.Type type, string name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":type", type);
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			var _return_value = AddEmitter(type, name);
			macross_break_AddEmitter_3553406330.TryBreak();
			return _return_value;
		}
	}
}


namespace EngineNS.Bricks.Particle
{
	partial class TtNebulaMacross
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnCreated_2571610209 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtNebulaMacross->void OnCreated(TtNebulaParticle nebula)");
		public unsafe void macross_OnCreated (string nodeName, TtNebulaParticle nebula) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":nebula", nebula);
				}
			}
			OnCreated(nebula);
			macross_break_OnCreated_2571610209.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnUpdate_2992855503 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtNebulaMacross->void OnUpdate(TtNebulaParticle nebula, UParticleGraphNode particleSystem, float elpased)");
		public unsafe void macross_OnUpdate (string nodeName, TtNebulaParticle nebula, UParticleGraphNode particleSystem, float elpased) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":nebula", nebula);
					stackframe.SetWatchVariable(nodeName + ":particleSystem", particleSystem);
					stackframe.SetWatchVariable(nodeName + ":elpased", elpased);
				}
			}
			OnUpdate(nebula, particleSystem, elpased);
			macross_break_OnUpdate_2992855503.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnUpdateEmitter_525844223 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtNebulaMacross->void OnUpdateEmitter(TtNebulaParticle nebula, TtEmitter emitter, UParticleGraphNode particleSystem, float elpased)");
		public unsafe void macross_OnUpdateEmitter (string nodeName, TtNebulaParticle nebula, TtEmitter emitter, UParticleGraphNode particleSystem, float elpased) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":nebula", nebula);
					stackframe.SetWatchVariable(nodeName + ":emitter", emitter);
					stackframe.SetWatchVariable(nodeName + ":particleSystem", particleSystem);
					stackframe.SetWatchVariable(nodeName + ":elpased", elpased);
				}
			}
			OnUpdateEmitter(nebula, emitter, particleSystem, elpased);
			macross_break_OnUpdateEmitter_525844223.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross