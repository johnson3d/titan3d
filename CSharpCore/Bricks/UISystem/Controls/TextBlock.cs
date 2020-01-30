using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Graphics.View;

namespace EngineNS.UISystem.Controls
{
    [Rtti.MetaClass]
    public class TextBlockInitializer : UIElementInitializer
    {
        [Rtti.MetaData]
        public string Text { get; set; } = "Text";
        [Rtti.MetaData]
        public UInt32 TextID { get; set; } = UInt32.MaxValue;
        [Rtti.MetaData]
        public int FontSize { get; set; } = 24;
        [EngineNS.Editor.Editor_PackData]
        [Rtti.MetaData]
        public RName Font { get; set; } = RName.GetRName("Font/msyh.ttf");// CEngine.Instance.Desc.DefaultFont;
        [EngineNS.Editor.Editor_PackData]
        [Rtti.MetaData]
        public RName FontMaterial { get; set; } = RName.GetRName("ui/mi_ui_defaultfont.instmtl", RName.enRNameType.Engine);// CEngine.Instance.Desc.DefaultFontInstmtl;
        [Rtti.MetaData]
        public string TextureShaderName { get; set; } = "txDiffuse";
        [Rtti.MetaData]
        public Color4 TextColor { get; set; } = new Color4(1, 1, 1, 1);
        [Rtti.MetaData]
        public float Opacity { get; set; } = 1.0f;
        [Rtti.MetaData]
        public bool WidthToContent { get; set; } = false;
        [Rtti.MetaData]
        public bool HeightToContent { get; set; } = false;

        public enum enWrap : SByte
        {
            None,
            Char,
            Word,
        }
        [Rtti.MetaData]
        [EngineNS.IO.Serializer.EnumSize(typeof(EngineNS.IO.Serializer.SByteEnum))]
        public enWrap WrapMode { get; set; } = enWrap.None;

        public enum enSizeToContentType : SByte
        {
            None,
            Width,
            Height,
            WidthAndHeight,
        }
        [Rtti.MetaData]
        [EngineNS.IO.Serializer.EnumSize(typeof(EngineNS.IO.Serializer.SByteEnum))]
        public enSizeToContentType SizeToContentType { get; set; } = enSizeToContentType.None;
    }
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor_UIControlInit(typeof(TextBlockInitializer))]
    [Editor_UIControl("通用.文本框", "用于显示文字的控件", "TextBlock.png")]
    public class TextBlock : UIElement
    {
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public string Text
        {
            get => ((TextBlockInitializer)mInitializer).Text;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.Text == value)
                    return;
                init.Text = value;
                if (mTextFont != null)
                {
                    mTextOrigionDrawPixelSize = mTextFont.MeasureString(Text);
                    CalculateWrap();
                    if(SizeToContentType != TextBlockInitializer.enSizeToContentType.None)
                        UpdateLayout();
                }
                OnPropertyChanged("Text");
            }
        }

        [Description("用于设置多国化时文本的ID")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public UInt32 TextID
        {
            get => ((TextBlockInitializer)mInitializer).TextID;
            set
            {
                ((TextBlockInitializer)mInitializer).TextID = value;
                OnPropertyChanged("TextID");
            }
        }

        [Category("字体"), DisplayName("字体尺寸")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int FontSize
        {
            get => ((TextBlockInitializer)mInitializer).FontSize;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.FontSize == value)
                    return;
                init.FontSize = value;
                mTextFont = CEngine.Instance.FontManager.GetFont(Font, value, mCachTextureSize, mTextureSize);
                mTextOrigionDrawPixelSize = mTextFont.MeasureString(Text);
                FontPixelSize = mTextFont.FontSize;
                mResetContent = true;
                CalculateWrap();
                if (SizeToContentType != TextBlockInitializer.enSizeToContentType.None)
                    UpdateLayout();
                OnPropertyChanged("FontSize");
            }
        }

        [Category("字体"), DisplayName("字体")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Font)]
        [EngineNS.Editor.Editor_PackData]
        public RName Font
        {
            get => ((TextBlockInitializer)mInitializer).Font;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.Font == value)
                    return;
                init.Font = value;
                mTextFont = CEngine.Instance.FontManager.GetFont(value, FontSize, mCachTextureSize, mTextureSize);
                mTextOrigionDrawPixelSize = mTextFont.MeasureString(Text);
                FontPixelSize = mTextFont.FontSize;
                mResetContent = true;
                OnPropertyChanged("Font");
            }
        }

        [Category("Shader"), DisplayName("文字材质")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.Editor_PackData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance)]
        public RName FontMaterial
        {
            get => ((TextBlockInitializer)mInitializer).FontMaterial;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.FontMaterial == value)
                    return;
                init.FontMaterial = value;
                var noUse = UpdateMaterial();
            }
        }

        [Category("Shader"), DisplayName("贴图Shader参数名称")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.Editor_PackData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public string TextureShaderName
        {
            get => ((TextBlockInitializer)mInitializer).TextureShaderName;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.TextureShaderName == value)
                    return;
                init.TextureShaderName = value;
                var noUse = UpdateMaterial();
            }
        }

        [EngineNS.Editor.Editor_UseCustomEditor]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Color4 TextColor
        {
            get => ((TextBlockInitializer)mInitializer).TextColor;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.TextColor == value)
                    return;
                init.TextColor = value;
                if (mTextMesh != null)
                    mTextMesh.TextColor = value;
            }
        }

        [EngineNS.Editor.Editor_ValueWithRange(0.0f, 1.0f)]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Opacity
        {
            get => ((TextBlockInitializer)mInitializer).Opacity;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.Opacity == value)
                    return;
                init.Opacity = value;
                if (mTextMesh != null)
                    mTextMesh.TextOpacity = value;
            }
        }

        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public TextBlockInitializer.enWrap WrapMode
        {
            get => ((TextBlockInitializer)mInitializer).WrapMode;
            set
            {
                var init = (TextBlockInitializer)mInitializer;
                if (init.WrapMode == value)
                    return;
                init.WrapMode = value;
                CalculateWrap();
                if (SizeToContentType != TextBlockInitializer.enSizeToContentType.None)
                    UpdateLayout();
            }
        }

        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public TextBlockInitializer.enSizeToContentType SizeToContentType
        {
            get => ((TextBlockInitializer)mInitializer).SizeToContentType;
            set
            {
                ((TextBlockInitializer)mInitializer).SizeToContentType = value;
                UpdateLayout();
            }
        }

        readonly int mCachTextureSize = 1024;
        readonly int mTextureSize = 128;
        Bricks.FreeTypeFont.CFTFont mTextFont;
        Bricks.FreeTypeFont.CFontMesh mTextMesh;

        bool mResetContent = true;
        Size mTextOrigionDrawPixelSize = new Size(100, 100);
        int FontPixelSize;

        async Task UpdateMaterial()
        {
            if (mTextMesh != null)
            {
                var rc = CEngine.Instance.RenderContext;
                var tm = mTextMesh;
                var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, FontMaterial);
                if (mtl == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Font", $"Font material {FontMaterial} load failed");
                }
                else
                {
                    await tm.SetMaterial(rc, mtl, TextureShaderName);
                }
            }
        }

        public override async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            if (!await base.Initialize(rc, init))
                return false;

            mTextFont = CEngine.Instance.FontManager.GetFont(Font, FontSize, mCachTextureSize, mTextureSize);
            mTextMesh = new Bricks.FreeTypeFont.CFontMesh();
            await UpdateMaterial();
            mTextMesh.TextColor = TextColor;
            mTextMesh.DrawText(rc, mTextFont, " ", true);
            mTextOrigionDrawPixelSize = mTextFont.MeasureString(Text);
            FontPixelSize = mTextFont.FontSize;
            CalculateWrap();
            if (SizeToContentType != TextBlockInitializer.enSizeToContentType.None)
                UpdateLayout();

            return true;
        }
        int mTextTotalHeight;
        float mCurrentTextLayoutWidth;
        void CalculateWrap()
        {
            unsafe
            {
                switch (WrapMode)
                {
                    case TextBlockInitializer.enWrap.None:
                        mTextArray.Clear();
                        mTextArray.Add(Text);
                        mTextTotalHeight = mTextOrigionDrawPixelSize.Height;// mTextFont.FontSize;
                        break;
                    case TextBlockInitializer.enWrap.Char:
                        {
                            if(mTextOrigionDrawPixelSize.Width > mInitializer.DesignRect.Width)
                            {
                                mTextArray.Clear();
                                int arraySize = System.Math.Min(Text.Length, 10000);
                                var idxArray = stackalloc int[arraySize];
                                var linesCount = mTextFont.CalculateWrap(Text, idxArray, arraySize, (int)(mInitializer.DesignRect.Width), out mTextTotalHeight);
                                for (int i = 0; i < linesCount; i++)
                                {
                                    var len = 0;
                                    if (i == 0)
                                    {
                                        len = idxArray[i];
                                        mTextArray.Add(Text.Substring(0, len));
                                    }
                                    else if (i == (linesCount - 1))
                                    {
                                        len = Text.Length - idxArray[i - 1];
                                        mTextArray.Add(Text.Substring(idxArray[i - 1], len));
                                    }
                                    else
                                    {
                                        len = idxArray[i] - idxArray[i - 1];
                                        mTextArray.Add(Text.Substring(idxArray[i - 1], len));
                                    }
                                }
                            }
                            else
                            {
                                mTextArray.Clear();
                                mTextArray.Add(Text);
                                mTextTotalHeight = mTextOrigionDrawPixelSize.Height;
                            }
                        }
                        break;
                    case TextBlockInitializer.enWrap.Word:
                        {
                            if(mTextOrigionDrawPixelSize.Width > mInitializer.DesignRect.Width)
                            {
                                mTextArray.Clear();
                                int arraySize = System.Math.Min(Text.Length, 10000);
                                var idxArray = stackalloc int[arraySize];
                                var linesCount = mTextFont.CalculateWrapWithWord(Text, idxArray, arraySize, (int)(mInitializer.DesignRect.Width), out mTextTotalHeight);
                                for (int i = 0; i < linesCount; i++)
                                {
                                    var len = 0;
                                    if (i == 0)
                                    {
                                        len = idxArray[i];
                                        mTextArray.Add(Text.Substring(0, len));
                                    }
                                    else if (i == (linesCount - 1))
                                    {
                                        len = Text.Length - idxArray[i - 1];
                                        mTextArray.Add(Text.Substring(idxArray[i - 1], len));
                                    }
                                    else
                                    {
                                        len = idxArray[i] - idxArray[i - 1];
                                        mTextArray.Add(Text.Substring(idxArray[i - 1], len));
                                    }
                                }
                            }
                            else
                            {
                                mTextArray.Clear();
                                mTextArray.Add(Text);
                                mTextTotalHeight = mTextOrigionDrawPixelSize.Height;
                            }

                        }
                        break;
                }
                var rc = CEngine.Instance.RenderContext;
                for (int i = 0; i < mTextArray.Count; i++)
                {
                    mTextMesh.DrawText(rc, 0, i * FontPixelSize, mTextFont, mTextArray[i], mResetContent);

                    if (mResetContent)
                        mResetContent = false;
                }
            }
            mCurrentTextLayoutWidth = DesignRect.Width;
            mResetContent = true;
        }
        List<string> mTextArray = new List<string>(20);
        public override bool Commit(CCommandList cmd, ref Matrix parentTransformMatrix, float dpiScale)
        {
            if (IsRenderable() == false)
                return false;

            if (mTextMesh == null || mTextFont == null || mTextArray == null || mTextArray.Count == 0)
                return false;

            var rect = DesignRect;

            mTextMesh.RenderMatrix = Matrix.Scaling(dpiScale, dpiScale, 1) *
                                     Matrix.Translate(rect.Left * dpiScale, rect.Top * dpiScale, 0.0f) *
                                     parentTransformMatrix;

            for (int i = 0; i < mTextArray.Count; i++)
            {
                mTextMesh.BuildMesh(cmd);
            }
            return true;
        }
        public override bool Draw(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {
            if (IsRenderable() == false)
                return false;

            if (mTextMesh == null || mTextFont == null)
                return false;

            for(int i=0; i<mTextMesh.PassNum; i++)
            {
                var pass = mTextMesh.GetPass(i);
                if (pass == null)
                    continue;

                pass.ViewPort = view.Viewport;
                pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                pass.ShadingEnv.BindResources(mTextMesh.Mesh, pass);

                cmd.PushPass(pass);
            }

            return true;
        }

        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            switch(SizeToContentType)
            {
                case TextBlockInitializer.enSizeToContentType.None:
                    return availableSize;
                case TextBlockInitializer.enSizeToContentType.Width:
                    {
                        SizeF retSize = availableSize;
                        retSize.Width = mTextOrigionDrawPixelSize.Width;
                        return retSize;
                    }
                case TextBlockInitializer.enSizeToContentType.Height:
                    {
                        SizeF retSize = availableSize;
                        retSize.Height = mTextTotalHeight;
                        return retSize;
                    }
                case TextBlockInitializer.enSizeToContentType.WidthAndHeight:
                    {
                        SizeF retSize = new SizeF();
                        retSize.Width = mTextOrigionDrawPixelSize.Width;
                        retSize.Height = mTextTotalHeight;
                        return retSize;
                    }
            }
            return availableSize;
        }
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            base.ArrangeOverride(ref arrangeSize);
            var clipRect = DesignClipRect;
            mTextMesh.SetClip((int)clipRect.Left, (int)clipRect.Top, (int)clipRect.Right, (int)clipRect.Bottom);

            if (mInitializer.DesignRect.Width != mCurrentTextLayoutWidth)
            {
                switch(SizeToContentType)
                {
                    case TextBlockInitializer.enSizeToContentType.None:
                    case TextBlockInitializer.enSizeToContentType.Height:
                        CalculateWrap();
                        break;
                }
            }
        }
    }
}
