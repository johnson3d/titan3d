using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Rtti.Meta]
    public class UNebulaParticleAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UNebulaParticle.AssetExt;
        }

        public override string GetAssetTypeName()
        {
            return "Nebula";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            //return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
            return new UNebulaParticle();
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
    [UNebulaParticle.UNebulaParticleImport]
    [IO.AssetCreateMenu(MenuName = "FX/NubulaParticle")]
    [EngineNS.Editor.UAssetEditor(EditorType = typeof(Editor.UParticleEditor))]
    public class UNebulaParticle : IO.IAsset, IDisposable
    {
        public const string AssetExt = ".nebula";
        public class UNebulaParticleImportAttribute : IO.CommonCreateAttribute
        {
            protected override bool CheckAsset()
            {
                var material = mAsset as UNebulaParticle;
                if (material == null)
                    return false;

                return true;
            }
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UNebulaParticleAMeta();
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
        public Dictionary<string, IParticleEmitter> Emitter { get; } = new Dictionary<string, IParticleEmitter>();
        public IParticleEmitter AddEmitter(System.Type type, string name)
        {
            var emitter = Rtti.UTypeDescManager.CreateInstance(type) as IParticleEmitter;
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
            //UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
        }

        public async Thread.Async.TtTask<UNebulaParticle> CloneNebula()
        {
            var result = new UNebulaParticle();
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
