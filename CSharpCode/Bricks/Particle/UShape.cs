using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public abstract class UShape
    {
        public virtual string Name { get; }
        public virtual void SetCBuffer(uint index, NxRHI.UCbView cbuffer)
        {

        }
        public unsafe abstract void UpdateLocation(IParticleEmitter emitter, FParticleBase* particle);
        public abstract UShape CloneShape();
    }
    public class UShapeBox : UShape
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FShapeBox
        {
            public Vector3 Center;
            public float Thinness;//[0,1]

            public Vector3 HalfExtent;
            public uint FEmitShapeBox_Pad0;
        };
        FShapeBox mShapeBox;
        #region Property
        public override string Name 
        { 
            get
            {
                return "UShapeBox";
            }
        }
        public Vector3 Center
        {
            get => mShapeBox.Center;
            set => mShapeBox.Center = value;
        }
        public Vector3 HalfExtent
        {
            get => mShapeBox.HalfExtent;
            set => mShapeBox.HalfExtent = value;
        }
        public float Thinness
        {
            get => mShapeBox.Thinness;
            set => mShapeBox.Thinness = value;
        }
        #endregion
        public UShapeBox()
        {
            mShapeBox.HalfExtent = Vector3.One;
            mShapeBox.Thinness = 1.0f;
        }
        public unsafe override void UpdateLocation(IParticleEmitter emitter, FParticleBase* particle)
        {
            var offset = new Vector3();
            if (Thinness >= 1.0f)
            {
                offset.X = emitter.RandomSignedUnit() * HalfExtent.X;
                offset.Y = emitter.RandomSignedUnit() * HalfExtent.Y;
                offset.Z = emitter.RandomSignedUnit() * HalfExtent.Z;
            }
            else
            {
                switch (emitter.RandomNext() % 6)
                {
                    case 0://x
                        {
                            offset.X = HalfExtent.X - (Thinness * emitter.RandomUnit()) * HalfExtent.X;
                            offset.Y = emitter.RandomSignedUnit() * HalfExtent.Y;
                            offset.Z = emitter.RandomSignedUnit() * HalfExtent.Z;
                        }
                        break;
                    case 1://-x
                        {
                            offset.X = -HalfExtent.X + (Thinness * emitter.RandomUnit()) * HalfExtent.X;
                            offset.Y = emitter.RandomSignedUnit() * HalfExtent.Y;
                            offset.Z = emitter.RandomSignedUnit() * HalfExtent.Z;
                        }
                        break;
                    case 2://y
                        {
                            offset.Y = HalfExtent.Y - (Thinness * emitter.RandomUnit()) * HalfExtent.Y;
                            offset.X = emitter.RandomSignedUnit() * HalfExtent.X;
                            offset.Z = emitter.RandomSignedUnit() * HalfExtent.Z;
                        }
                        break;
                    case 3://-y
                        {
                            offset.X = -HalfExtent.Y + (Thinness * emitter.RandomUnit()) * HalfExtent.Y;
                            offset.X = emitter.RandomSignedUnit() * HalfExtent.X;
                            offset.Z = emitter.RandomSignedUnit() * HalfExtent.Z;
                        }
                        break;
                    case 4://z
                        {
                            offset.Z = HalfExtent.Z - (Thinness * emitter.RandomUnit()) * HalfExtent.Z;
                            offset.X = emitter.RandomSignedUnit() * HalfExtent.X;
                            offset.Y = emitter.RandomSignedUnit() * HalfExtent.Y;
                        }
                        break;
                    case 5://-z
                        {
                            offset.Z = -HalfExtent.Z + (Thinness * emitter.RandomUnit()) * HalfExtent.Z;
                            offset.X = emitter.RandomSignedUnit() * HalfExtent.X;
                            offset.Y = emitter.RandomSignedUnit() * HalfExtent.Y;
                        }
                        break;
                }
            }
            particle->Location = Center + offset;
        }
        public override void SetCBuffer(uint index, NxRHI.UCbView CBuffer)
        {
            CBuffer.SetValue($"EmitShape{index}", in mShapeBox);
        }
        public override UShape CloneShape()
        {
            var result = new UShapeBox();
            result.mShapeBox = mShapeBox;
            return result;
        }
    }
    public class UShapeSphere : UShape
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FShapeSphere
        {
            public Vector3 Center;
            public float Radius;

            public Vector3 FEmitShapeSphere_Pad0;
            public float Thinness;//[0,1]
        };
        public override string Name
        {
            get
            {
                return "UShapeSphere";
            }
        }
        FShapeSphere mShapeSphere;
        public Vector3 Center
        {
            get => mShapeSphere.Center;
            set => mShapeSphere.Center = value;
        }
        public float Radius
        {
            get => mShapeSphere.Radius;
            set => mShapeSphere.Radius = value;
        }
        public float Thinness
        {
            get => mShapeSphere.Thinness;
            set => mShapeSphere.Thinness = value;
        }
        public UShapeSphere()
        {
            mShapeSphere.Radius = 1.0f;
        }
        public override unsafe void UpdateLocation(IParticleEmitter emitter, FParticleBase* particle)
        {
            Vector3 offset;            
            offset = emitter.RandomVector() * (Radius - Radius * (Thinness * emitter.RandomUnit()));
            particle->Location = Center + offset;
        }
        public override void SetCBuffer(uint index, NxRHI.UCbView CBuffer)
        {
            CBuffer.SetValue($"EmitShape{index}", in mShapeSphere);
        }
        public override UShape CloneShape()
        {
            var result = new UShapeSphere();
            result.mShapeSphere = mShapeSphere;
            return result;
        }
    }
}
