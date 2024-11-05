using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.UDirectionLight@EngineCore" })]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtDirectionLight
        : IO.BaseSerializer
    {
        public Graphics.Pipeline.Shader.FDirLight mDirLight;
        [Rtti.Meta]
        [Category("Option")]
        public Vector3 Direction
        {
            get => mDirLight.Direction;
            set => mDirLight.Direction = value;
        }

        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        [Rtti.Meta]
        [Category("Option")]
        public Vector3 SunLightColor
        {
            get => mDirLight.SunLightColor;
            set
            {
                mDirLight.SunLightColor = value;
            }
        }
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        [Rtti.Meta]
        [Category("Option")]
        public Vector3 SkyLightColor
        {
            get => mDirLight.SkyLightColor;
            set
            {
                mDirLight.SkyLightColor = value;
            }
        }
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        [Rtti.Meta]
        [Category("Option")]
        public Vector3 GroundLightColor
        {
            get => mDirLight.GroundLightColor;
            set
            {
                mDirLight.GroundLightColor = value;
            }
        }
        [Rtti.Meta]
        [Category("Option")]
        public float SunLightIntensity
        {
            get => mDirLight.SunLightIntensity;
            set
            {
                mDirLight.SunLightIntensity = value;
            }
        }

        public TtDirectionLight()
        {
            mDirLight.Direction = new Vector3(1, -1, 1);
            mDirLight.Direction.Normalize();
            mDirLight.SunLightLeak = 0.05f;

            mDirLight.SunLightColor = new Vector3(1, 1, 1);
            mDirLight.SunLightIntensity = 2.5f;

            mDirLight.SkyLightColor = new Vector3(0.5f, 0.5f, 0.5f);
            mDirLight.GroundLightColor = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }
}
