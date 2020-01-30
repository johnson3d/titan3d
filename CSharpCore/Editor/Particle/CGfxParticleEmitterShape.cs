using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle.EmitShape
{
    public partial class CGfxParticleEmitterShape
    {
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        //[Editor.DisplayParamName("是否显示辅助模型（bool）")]
        public virtual RName UseMeshName
        {
            get;
            set;
        }

        public virtual void BuildMatrix(ref Matrix mat)
        {

        }
    }

    partial class CGfxParticleEmitterShapeSphere
    {
        public override RName UseMeshName
        {
            get;
            set;
        } = RName.GetRName("ParticleResource/models/sphere.gms");

        public override void BuildMatrix(ref Matrix mat)
        {
            float scale = Radius * 2.0f;
            mat.M11 *= scale;
            mat.M12 *= scale;
            mat.M13 *= scale;
            mat.M21 *= scale;
            mat.M22 *= scale;
            mat.M23 *= scale;
            mat.M31 *= scale;
            mat.M32 *= scale;
            mat.M33 *= scale;
        }
    }

    partial class CGfxParticleEmitterShapeBox
    {
        public override RName UseMeshName
        {
            get;
            set;
        } = RName.GetRName("ParticleResource/models/box.gms");

        public override void BuildMatrix(ref Matrix mat)
        {

            float scalex = SizeX;
            float scaley = SizeY;
            float scalez = SizeZ;

            mat.M11 *= scalex;
            mat.M12 *= scalex;
            mat.M13 *= scalex;
            mat.M21 *= scaley;
            mat.M22 *= scaley;
            mat.M23 *= scaley;
            mat.M31 *= scalez;
            mat.M32 *= scalez;
            mat.M33 *= scalez;
        }
    }

    partial class CGfxParticleEmitterShapeCone
    {
        public override RName UseMeshName
        {
            get;
            set;
        } = RName.GetRName("ParticleResource/models/sphere.gms");
    }

    //partial class CGfxParticleEmitterShapeMesh
    //{
    //    public RName UseMeshName
    //    {
    //        get;
    //        set;
    //    }
    //}
}