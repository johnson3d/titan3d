using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay
{
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.UDirectionLight@EngineCore" })]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtDirectionLight
        : IO.BaseSerializer
    {
        public Graphics.Pipeline.Shader.FDirLight mDirLight;
        [Rtti.Meta("")]
        [Category("Option")]
        public Vector3 Direction
        {
            get => mDirLight.Direction;
            set => mDirLight.Direction = value;
        }

        [EGui.Controls.PropertyGrid.TtColor3PickerEditor]
        [Rtti.Meta("")]
        [Category("Option")]
        public Vector3 SunLightColor
        {
            get => mDirLight.SunLightColor;
            set
            {
                mDirLight.SunLightColor = value;
            }
        }
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor]
        [Rtti.Meta("")]
        [Category("Option")]
        public Vector3 SkyLightColor
        {
            get => mDirLight.SkyLightColor;
            set
            {
                mDirLight.SkyLightColor = value;
            }
        }
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor]
        [Rtti.Meta("")]
        [Category("Option")]
        public Vector3 GroundLightColor
        {
            get => mDirLight.GroundLightColor;
            set
            {
                mDirLight.GroundLightColor = value;
            }
        }
        [Rtti.Meta("")]
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

            /*
             Sky 永远比 Ground 亮，通常亮 3~5 倍
             Sky 偏冷色（蓝），Ground 偏暖色（黄/棕/绿）——这就是为什么阴影区域通常偏蓝，因为天光主导
             Ground 的色调取决于地表材质：草地偏绿、沙地偏黄、雪地偏白
             Ground 的亮度通常是 Sky 的 15%~25%（除了雪地等高反射率地面）
            */
            mDirLight.SkyLightColor = new Vector3(0.6f, 0.75f, 1.0f);//偏蓝，天空散射的瑞利散射色
            mDirLight.GroundLightColor = new Vector3(0.15f, 0.13f, 0.08f);//偏暖黄褐，地面反弹光（草地/泥土）
        }
    }
}
