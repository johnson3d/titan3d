using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Particle.Simple
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FSimpleParticle
    {
        public FParticleBase BaseData;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FSimpleParticleSystem
    {
        public FParticleSystemBase BaseData;
    }
    public class USimpleEmitter : Bricks.Particle.UEmitter<FSimpleParticle, FSimpleParticleSystem>
    {
        public override unsafe void InitEmitter(RHI.CRenderContext rc, Graphics.Mesh.UMesh mesh, uint maxParticle)
        {
            SystemData.BaseData.Flags = 0;
            base.InitEmitter(rc, mesh, maxParticle);
        }
        public override void DoUpdateSystem()
        {
            if (SystemData.BaseData.Flags == 0)
            {
                mCoreObject.Spawn(512, SetParticleFlags(EParticleFlags.EmitShape, 0), 3.0f);
                SystemData.BaseData.Flags = 1;
            }
        }
        public unsafe override void OnInitParticle(FSimpleParticle* pParticles, ref FSimpleParticle particle)
        {
            if (HasFlags(in particle.BaseData, EParticleFlags.EmitIndex) != 0)
            {
                var particleIndex = GetParticleData(particle.BaseData.Flags);
                particle.BaseData.Location = pParticles[particleIndex].BaseData.Location;
                particle.BaseData.Location.Y += 2.0f;
                //particle.mLocation.Z = RandomUnit() * 10.0f;
            }
            particle.BaseData.Life += RandomUnit() * 0.5f;
            
            particle.BaseData.Scale = 0.5f - RandomUnit() * 0.2f;
        }
        public override unsafe void OnDeadParticle(uint index, ref FSimpleParticle particle)
        {
            if (HasFlags(in particle.BaseData, EParticleFlags.EmitShape) != 0)
            {
                uint shapeIndex = GetParticleData(particle.BaseData.Flags);
                if (shapeIndex == 0)
                    mCoreObject.Spawn(1, SetParticleFlags(EParticleFlags.EmitShape, 1), 5.0f);
                else
                    mCoreObject.Spawn(1, SetParticleFlags(EParticleFlags.EmitShape, 0), 3.0f);
            }

            //mCoreObject.Spawn(1, SetParticleFlags(EParticleFlags.EmitIndex, index), 3.0f);
        }
        public override string GetHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.FileManager.ReadAllText($"{RName.GetRName("UTest\\Particles\\USimpleEmitter\\Emitter.compute").Address}");
            codeBuilder.AddLine(code, ref sourceCode);

            codeBuilder.AddLine("\nvoid DoParticleEmitShape(uint3 id, inout FParticle cur, uint shapeIndex)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                int index = 0;
                codeBuilder.AddLine("switch(shapeIndex)", ref sourceCode);
                codeBuilder.PushSegment(ref sourceCode);
                {
                    foreach (var i in EmitterShapes)
                    {
                        codeBuilder.AddLine($"case {index}:", ref sourceCode);
                        codeBuilder.PushSegment(ref sourceCode);
                        {
                            codeBuilder.AddLine($"{i.Name}_UpdateLocation(id, EmitShape{index}, cur);", ref sourceCode);
                        }
                        codeBuilder.PopSegment(ref sourceCode);
                        codeBuilder.AddLine($"break;", ref sourceCode);
                        index++;
                    }
                }
                codeBuilder.PopSegment(ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);

            return sourceCode;
        }
    }

    public class USimpleMdfQueue : UParticleMdfQueue<FSimpleParticle, FSimpleParticleSystem>
    {
        
    }
    public class USimpleNebulaNode : UNebulaNode<FSimpleParticle, FSimpleParticleSystem, USimpleEmitter, USimpleMdfQueue>
    {
        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var dd = CoreDefine.RoundUpPow2(512, 32);
            var nebulaData = data as UNebulaNodeData;
            nebulaData.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(USimpleMdfQueue));

            await base.InitializeNode(world, data, bvType, placementType);

            var Emitter = NebulaParticle.AddEmitter(typeof(USimpleEmitter), "emitter0") as USimpleEmitter;
            Emitter.IsGpuDriven = true;
            Emitter.InitEmitter(UEngine.Instance.GfxDevice.RenderContext, this.Mesh, 1024);
            var sphereShape = new Bricks.Particle.UShapeSphere();
            sphereShape.Radius = 10.0f;
            sphereShape.Thinness = 0.1f;
            var boxShape = new Bricks.Particle.UShapeBox();
            boxShape.Thinness = 0.2f;
            Emitter.EmitterShapes.Add(sphereShape);
            Emitter.EmitterShapes.Add(boxShape);
            var ef1 = new UAcceleratedEffector();
            ef1.Acceleration = new Vector3(0, -0.1f, 0);
            Emitter.AddEffector("default", ef1);            
            Emitter.SetCurrentQueue("default");

            var nblMdf = this.Mesh.MdfQueue as USimpleMdfQueue;
            nblMdf.Emitter = Emitter;

            UEngine.Instance.NebulaTemplateManager.UpdateShaders(NebulaParticle);

            return true;
        }
    }
}
