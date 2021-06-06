using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public class UDirectionLight
    {
        public Vector3 mDirection;
        public float mSunLightLeak;
        public Vector3 mSunLightColor;
        public float mSunLightIntensity;
        public Vector3 mSkyLightColor;
        public Vector3 mGroundLightColor;
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        public Vector3 SunLightColor
        {
            get => mSunLightColor;
            set
            {
                mSunLightColor = value;
            }
        }
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        public Vector3 SkyLightColor
        {
            get => mSkyLightColor;
            set
            {
                mSkyLightColor = value;
            }
        }
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        public Vector3 GroundLightColor
        {
            get => mGroundLightColor;
            set
            {
                mGroundLightColor = value;
            }
        }
        public UDirectionLight()
        {
            mSunLightColor = new Vector3(1, 1, 1);
            mDirection = new Vector3(1, 1, 1);
            mSunLightLeak = 0.05f;
            mDirection.Normalize();
            mSkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            mGroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}
