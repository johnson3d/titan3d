using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial class CEngineDesc
    {
        public static string UIExtension
        {
            get;
            set;
        } = MacrossExtension;// ".ui";
        public static string UVAnimExtension
        {
            get;
            set;
        } = ".uvanim";

        [Editor.Editor_PackData]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.MaterialInstance)]
        public static RName DefaultUIMaterialInstance
        {
            get;
            set;
        } = RName.GetRName("ui/mi_ui_default.instmtl", RName.enRNameType.Engine);
        [Editor.Editor_PackData]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public static RName DefaultUITextureName
        {
            get;
            set;
        } = RName.GetRName("Texture/uvchecker.txpic");
        [Editor.Editor_PackData]
        public static RName DefaultUIUVAnim
        {
            get;
            set;
        } = RName.GetRName("ui/uv_ui_default.uvanim", RName.enRNameType.Engine);
        [Editor.Editor_PackData()]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.MaterialInstance)]
        public RName DefaultFontInstmtl
        {
            get;
            set;
        } = RName.GetRName("Material/font.instmtl");
        [Editor.Editor_PackData()]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Font)]
        public RName DefaultFont
        {
            get;
            set;
        } = RName.GetRName("Font/msyh.ttf");
        [Editor.Editor_PackData()]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName DefaultEnvMap
        {
            get;
            set;
        } = RName.GetRName("EngineAsset/Texture/default_envmap.txpic");
        [Editor.Editor_PackData()]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName DefaultEyeEnvMap
        {
            get;
            set;
        } = RName.GetRName("Texture/eyeenvmap0.txpic");
        [Editor.Editor_PackData()]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName DefaultVignette
        {
            get;
            set;
        } = RName.GetRName("EngineAsset/Texture/default_vignette.txpic"); 
    }
}
