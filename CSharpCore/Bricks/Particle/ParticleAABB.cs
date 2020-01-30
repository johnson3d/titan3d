using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class ParticleAABB
    {
        public BoundingBox Box;
        float mL = 1f;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float L
        {
            get
            {
                return mL;
            }
            set
            {
                mL = value;
                ResetBoundBox();
            }
        }

        float mW = 1f;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float W
        {
            get
            {
                return mW;
            }
            set
            {
                mW = value;
                ResetBoundBox();
            }
        }

        float mH = 1f;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float H
        {
            get
            {
                return mH;
            }
            set
            {
                mH = value;
                ResetBoundBox();
            }
        }

        Vector3 mCenter = Vector3.Zero;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Center
        {
            get
            {
                return mCenter;
            }
            set
            {
                mCenter = value;
                ResetBoundBox();
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float X
        {
            get
            {
                return mCenter.X;
            }
            set
            {
                mCenter.X = value;
                ResetBoundBox();
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Y
        {
            get
            {
                return mCenter.Y;
            }
            set
            {
                mCenter.Y = value;
                ResetBoundBox();
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Z
        {
            get
            {
                return mCenter.Z;
            }
            set
            {
                mCenter.Z = value;
                ResetBoundBox();
            }
        }

        public void ResetBoundBox()
        {
            Box.Minimum.X = mCenter.X - mL * 0.5f;
            Box.Minimum.Z = mCenter.Z - mW * 0.5f;
            Box.Minimum.Y = mCenter.Y - mH * 0.5f;

            Box.Maximum.X = mCenter.X + mL * 0.5f;
            Box.Maximum.Z = mCenter.Z + mW * 0.5f;
            Box.Maximum.Y = mCenter.Y + mH * 0.5f;
        }
    }
}