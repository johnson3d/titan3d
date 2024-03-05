using EngineNS.Bricks.Font;
using EngineNS.Canvas;
using EngineNS.Support;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    [Editor_UIControl("Controls.Text", "Text control, for display string", "")]
    public partial class TtText : TtUIElement
    {
        [Flags]
        public enum ETextFlag : UInt16
        {
            // ETextTrimming
            None                = 0,
            CharacterEllipsis   = 1 << 0,
            WordEllipsis        = 1 << 1,

            // ETextWrapping
            WrapWithOverflow    = 1 << 2,
            NoWrap              = 1 << 3,
            Wrap                = 1 << 4,

            // ETextDirection
            LeftToRight         = 1 << 5,
            RightToLeft         = 1 << 6,
            TopToBottom           = 1 << 7,
            BottomToTop           = 1 << 8,
        }
        [Rtti.Meta, Browsable(false)]
        internal ETextFlag TextFlag
        {
            get;
            set;
        } = ETextFlag.None;
        bool ReadFlag(ETextFlag flag)
        {
            return (TextFlag & flag) != 0;
        }
        void WriteFlag(ETextFlag flag, bool value)
        {
            if (value)
                TextFlag |= flag;
            else
                TextFlag &= ~flag;
        }

        TtFontSDF mFontAsset;
        TtFontSDF FontAsset
        {
            get
            {
                if(mFontAsset == null || mFontDirty)
                {
                    mFontAsset = UEngine.Instance.FontModule.FontManager.GetFontSDF(mFont, mFontSize, mTextureSize.X, mTextureSize.Y);
                    mFontDirty = false;
                }
                return mFontAsset;
            }
        }
        bool mFontDirty = true;
        RName mFont = RName.GetRName("fonts/simli.fontsdf", RName.ERNameType.Engine);
        [Rtti.Meta, BindProperty]
        [DisplayName("Font"), Category("Text")]
        [RName.PGRName(FilterExts = TtFontManager.FontSDFAssetExt + "," + TtFontManager.FontAssetExt)]
        public RName Font
        {
            get => mFont;
            set
            {
                OnValueChange(value, mFont);
                mFont = value;
                mFontDirty = true;
                MeshDirty = true;
            }
        }

        int mFontSize = 64;
        [Rtti.Meta, BindProperty]
        [Category("Text")]
        public int FontSize
        {
            get => mFontSize;
            set
            {
                OnValueChange(value, mFontSize);
                mFontSize = value;
                MeshDirty = true;
            }
        }

        string mText;
        [Rtti.Meta, BindProperty]
        [Category("Text")]
        public string Text
        {
            get => mText;
            set
            {
                OnValueChange(value, mText);
                mText = value;
                MeshDirty = true;
            }
        }

        public enum ETextTrimming
        {
            None = ETextFlag.None,
            CharacterEllipsis = ETextFlag.CharacterEllipsis,
            WordEllipsis = ETextFlag.WordEllipsis,
        }
        [BindProperty]
        [Category("Text")]
        public ETextTrimming TextTrimming
        {
            get
            {
                return (ETextTrimming)(TextFlag & (ETextFlag.CharacterEllipsis | ETextFlag.WordEllipsis));
            }
            set
            {
                OnValueChange(value, TextTrimming);
                WriteFlag(ETextFlag.CharacterEllipsis | ETextFlag.WordEllipsis, false);
                WriteFlag((ETextFlag)value, true);
                MeshDirty = true;
            }
        }

        public enum ETextWrapping
        {
            WrapWithOverflow = ETextFlag.WrapWithOverflow,
            NoWrap = ETextFlag.NoWrap,
            Wrap = ETextFlag.Wrap,
        }
        [BindProperty]
        [Category("Text")]
        public ETextWrapping TextWrapping
        {
            get
            {
                return (ETextWrapping)(TextFlag & (ETextFlag.WrapWithOverflow | ETextFlag.NoWrap | ETextFlag.Wrap));
            }
            set
            {
                OnValueChange(value, TextWrapping);
                WriteFlag(ETextFlag.WrapWithOverflow | ETextFlag.NoWrap | ETextFlag.Wrap, false);
                WriteFlag((ETextFlag)value, true);
                //MeshDirty = true;
                InvalidateMeasure();
            }
        }

        float mLineSpacingScale = 1.0f;
        [Rtti.Meta, BindProperty]
        [Category("Text")]
        public float LineSpacingScale
        {
            get => mLineSpacingScale;
            set
            {
                OnValueChange(value, mLineSpacingScale);
                mLineSpacingScale = value;
                MeshDirty = true;
            }
        }
        int mLineCount = 1;
        struct stTextLineData
        {
            public string Text;
            public RectangleF Rect;
            public int StartIndex;
            public int EndIndex;
        }
        List<stTextLineData> mTextInLines = new List<stTextLineData>();
        List<string> mTextSplitWithLanguages = new List<string>();

        public enum ETextDirection
        {
            LeftToRight = ETextFlag.LeftToRight,
            RightToLeft = ETextFlag.RightToLeft,
            TopToBottom = ETextFlag.TopToBottom,
            BottomToTop = ETextFlag.BottomToTop,
        }
        public ETextDirection FlowDirection
        {
            get
            {
                return (ETextDirection)(TextFlag & (ETextFlag.LeftToRight | ETextFlag.LeftToRight | ETextFlag.LeftToRight | ETextFlag.BottomToTop));
            }
            set
            {
                OnValueChange(value, FlowDirection);
                WriteFlag(ETextFlag.LeftToRight | ETextFlag.LeftToRight | ETextFlag.LeftToRight | ETextFlag.BottomToTop, false);
                WriteFlag((ETextFlag)value, true);
                MeshDirty = true;
            }
        }

        Vector2i mTextureSize = new Vector2i(1024, 1024);

        void CalculateTextWrapping(float sizeX, float sizeY)
        {
            var textSize = FontAsset.GetTextSize(mText);

        }

        public TtText()
        {
            TextTrimming = ETextTrimming.None;
            TextWrapping = ETextWrapping.NoWrap;
            FlowDirection = ETextDirection.LeftToRight;
        }

        public override bool IsReadyToDraw()
        {
            return true;
        }
        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            if (string.IsNullOrEmpty(mText))
                return;

            var delta = mFontSize * 1.0f / FontAsset.FontSize;
            var mat = Matrix.Scaling(delta);
            batch.Middleground.PushTransformIndex(TransformIndex);
            batch.Middleground.PushFont(FontAsset);
            batch.Middleground.PushMatrix(mat);
            batch.Middleground.AddText(mText, mCurFinalRect.Left, mCurFinalRect.Top, Color4f.FromABGR(Color.LightPink));
            batch.Middleground.PopMatrix();
            batch.Middleground.PopFont();
            batch.Middleground.PopTransformIndex();
        }

        bool CanBreakLine(char chr)
        {
            return false;
        }
        int GetLastWordSplitPosition(string text, int lastIndex)
        {
            return -1;
        }

        void CalculateLines(in UNativeArray<FTWord> words, in SizeF availableSize)
        {
            mTextInLines.Clear();
            var scaleDelta = mFontSize * 1.0f / FontAsset.FontSize;
            switch(FlowDirection)
            {
                case ETextDirection.LeftToRight:
                    {
                        var line = new stTextLineData()
                        {
                            StartIndex = 0,
                            EndIndex = 0,
                        };
                        RectangleF lastRect = RectangleF.Empty;
                        for (int i=0; i<words.Count; i++)
                        {
                            line.Rect.Width += words[i].Advance.X * scaleDelta;
                            var height = (words[i].TexY + words[i].PixelHeight) * scaleDelta;
                            if (height > line.Rect.Height)
                                line.Rect.Height = height;
                            if(line.Rect.Width >= availableSize.Width)
                            {
                                switch(TextWrapping)
                                {
                                    case ETextWrapping.NoWrap:
                                        break;
                                    case ETextWrapping.Wrap:
                                        line.EndIndex = i - 1;
                                        line.Text = mText.Substring(line.StartIndex, i - line.StartIndex);
                                        mTextInLines.Add(line);
                                        line = new stTextLineData()
                                        {
                                            StartIndex = i,
                                            Rect = new RectangleF(0, line.Rect.Height, 0, 0),
                                        };
                                        break;
                                    case ETextWrapping.WrapWithOverflow:
                                        break;
                                }
                            }
                        }
                        mTextInLines.Add(line);
                    }
                    break;
                case ETextDirection.RightToLeft:
                case ETextDirection.TopToBottom:
                case ETextDirection.BottomToTop:
                    // todo
                    break;
            }
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            
            var words = new UNativeArray<FTWord>();
            FontAsset.GetWords(ref words, mText);
            CalculateLines(words, availableSize);
            switch(FlowDirection)
            {
                case ETextDirection.LeftToRight:
                    {
                        //if (textSize.X > availableSize.Width)
                        //{
                        //    switch(TextWrapping)
                        //    {
                        //        case ETextWrapping.NoWrap:
                        //            {
                        //                switch(TextTrimming)
                        //                {
                        //                    case ETextTrimming.CharacterEllipsis:
                        //                        break;
                        //                    case ETextTrimming.WordEllipsis:
                        //                        break;
                        //                    case ETextTrimming.None:
                        //                        mTextInLines.Add(mText);
                        //                        mLineCount = 1;
                        //                        break;
                        //                }
                        //            }
                        //            break;
                        //        case ETextWrapping.Wrap:
                        //            break;
                        //        case ETextWrapping.WrapWithOverflow:
                        //            break;
                        //    }
                        //}
                        //else
                        //{
                        //    mTextInLines.Add(mText);
                        //    mLineCount = 1;
                        //}
                    }
                    break;
                case ETextDirection.RightToLeft:
                case ETextDirection.TopToBottom:
                case ETextDirection.BottomToTop:
                    break;
            }
            return base.MeasureOverride(availableSize);
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            base.ArrangeOverride(arrangeSize);
        }
    }
}
