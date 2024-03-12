using EngineNS.Bricks.Font;
using EngineNS.Canvas;
using EngineNS.Localization;
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
            Center              = 1 << 7,
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

        Vector2i mTextureSize = new Vector2i(1024, 1024);

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
                //MeshDirty = true;
                UpdateLayout();
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
                UpdateLayout();
            }
        }

        string mText = "Text";
        [Rtti.Meta, BindProperty]
        [Category("Text")]
        public string Text
        {
            get => mText;
            set
            {
                OnValueChange(value, mText);
                mText = value;
                UpdateLayout();
            }
        }

        EngineNS.Color mColor = EngineNS.Color.White;
        [Rtti.Meta, BindProperty, Category("Text")]
        [EGui.Controls.PropertyGrid.Color4PickerEditor()]
        public EngineNS.Color Color
        {
            get => mColor;
            set
            {
                OnValueChange(value, mColor);
                mColor = value;
                //MeshDirty = true;
                UpdateLayout();
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
                //MeshDirty = true;
                UpdateLayout();
            }
        }

        public enum ETextWrapping
        {
            NoWrap = ETextFlag.NoWrap,                      // 不折行
            Wrap = ETextFlag.Wrap,                          // 如果放得下按词折行，放不下按字折行
            WrapWithOverflow = ETextFlag.WrapWithOverflow,  // 按词折行，放不下则裁剪
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
                UpdateLayout();
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
                //MeshDirty = true;
                UpdateLayout();
            }
        }
        int mLineCount = 1;
        struct stTextLineData
        {
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
            Center = ETextFlag.Center,
        }
        [Rtti.Meta, BindProperty, Category("Text")]
        public ETextDirection FlowDirection
        {
            get
            {
                return (ETextDirection)(TextFlag & (ETextFlag.LeftToRight | ETextFlag.RightToLeft | ETextFlag.Center));
            }
            set
            {
                OnValueChange(value, FlowDirection);
                WriteFlag(ETextFlag.LeftToRight | ETextFlag.RightToLeft | ETextFlag.Center, false);
                WriteFlag((ETextFlag)value, true);
                //MeshDirty = true;
                UpdateLayout();
            }
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
            batch.Middleground.PushClip(in mCurFinalRect);
            for(int i=0;i<mTextInLines.Count; i++)
            {
                var x = mCurFinalRect.Left + mTextInLines[i].Rect.Left;
                var y = mCurFinalRect.Top + mTextInLines[i].Rect.Top;
                var text = mText.Substring(mTextInLines[i].StartIndex, mTextInLines[i].EndIndex - mTextInLines[i].StartIndex);
                batch.Middleground.AddText(text, x, y, Color);
            }
            //batch.Middleground.AddText(mText, mCurFinalRect.Left, mCurFinalRect.Top, Color4f.FromABGR(Color.LightPink));
            batch.Middleground.PopClip();
            batch.Middleground.PopMatrix();
            batch.Middleground.PopFont();
            batch.Middleground.PopTransformIndex();
        }

        //RectangleF CalculateLinesRectangele()
        //{
        //    if (string.IsNullOrEmpty(mText))
        //        return RectangleF.Empty;
        //    float left = float.MaxValue;
        //    float top = float.MaxValue;
        //    float right = float.MinValue;
        //    float bottom = float.MinValue;
        //    for(int i=0; i<mTextInLines.Count; i++)
        //    {
        //        var width = mTextInLines[i].Rect.Width;
        //        var height = mTextInLines[i].Rect.Height;
        //        left = Math.Min(mTextInLines[i].Rect.X, left);
        //        top = Math.Min(mTextInLines[i].Rect.Y, top);
        //        right = Math.Max(mTextInLines[i].Rect.Right, right);
        //        bottom = Math.Max(mTextInLines[i].Rect.Bottom, bottom);
        //    }
        //    return new RectangleF(left, top, right - left, bottom - top);
        //}

        void CalculateLines(string text, 
            in UNativeArray<FTWord> words, 
            int startIndex, 
            in SizeF areaSize, 
            float fontScaleDelta, 
            float lineHeight, 
            ETextDirection flowDirection,
            ref List<stTextLineData> lines, 
            bool generateLine, 
            ref SizeF linesSize)
        {
            if (string.IsNullOrEmpty(mText))
                return;

            if(TextWrapping == ETextWrapping.NoWrap)
            {
                float width = 0;
                for(int i=0; i<words.Count; i++)
                {
                    width += words[i].Advance.X * fontScaleDelta;
                }
                linesSize.Width = MathHelper.Max(width, linesSize.Width);
                linesSize.Height += lineHeight;
                if(generateLine)
                {
                    var line = new stTextLineData()
                    {
                        StartIndex = startIndex,
                        EndIndex = text.Length,
                        Rect = new RectangleF(0, 0, width, lineHeight),
                    };
                    lines.Add(line);
                }
            }
            else
            {
                float currentWidth = 0;
                int currentCount = 0;
                for(int i=startIndex; i<words.Count; i++)
                {
                    var wordWidth = words[i].Advance.X * fontScaleDelta;
                    var curWordCulture = UEngine.Instance.LocalizationManager.GetCulture(mText[i]);
                    if(currentWidth + wordWidth > areaSize.Width)
                    {
                        if((curWordCulture == LocalizationManager.ECulture.Separator) &&
                           (i == startIndex))
                        {
                            currentCount += 1;
                            currentWidth += wordWidth;
                            break;
                        }
                        else
                        {
                            LocalizationManager.ECulture breakerCulture;
                            var breakerIdx = UEngine.Instance.LocalizationManager.GetLastWordBreaker(Text, startIndex, i, out breakerCulture);
                            if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
                                (breakerCulture == LocalizationManager.ECulture.Punctuation))
                                breakerIdx += 1;
                            if (i == startIndex)
                            {
                                currentCount += 1;
                                currentWidth += wordWidth;
                                break;
                            }
                            else if(breakerIdx <= startIndex)
                            {
                                if (TextWrapping == ETextWrapping.Wrap)
                                    break;
                                else if(TextWrapping == ETextWrapping.WrapWithOverflow)
                                {
                                    currentCount += 1;
                                    currentWidth += wordWidth;
                                }
                            }
                            else
                            {
                                currentCount = breakerIdx - startIndex;
                                var tempIdxStart = breakerIdx;
                                if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
                                    (breakerCulture == LocalizationManager.ECulture.Punctuation))
                                    tempIdxStart = breakerIdx - 1;
                                for (int tempIdx = tempIdxStart; tempIdx < i; tempIdx++)
                                {
                                    var tempWordWidth = words[tempIdx].Advance.X * fontScaleDelta;
                                    currentWidth -= tempWordWidth;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        currentCount += 1;
                        currentWidth += wordWidth;
                    }
                }
                if (generateLine)
                {
                    float lineX  = 0;
                    switch(flowDirection)
                    {
                        case ETextDirection.LeftToRight:
                            lineX = 0;
                            break;
                        case ETextDirection.RightToLeft:
                            lineX = areaSize.Width - currentWidth;
                            break;
                        case ETextDirection.Center:
                            lineX = (areaSize.Width - currentWidth) * 0.5f;
                            break;
                    }
                    var line = new stTextLineData()
                    {
                        StartIndex = startIndex,
                        EndIndex = startIndex + currentCount,
                        Rect = new RectangleF(lineX, linesSize.Height, currentWidth, lineHeight),
                    };
                    
                    lines.Add(line);
                }
                linesSize.Width = MathHelper.Max(currentWidth, linesSize.Width);
                linesSize.Height += lineHeight;
                if(startIndex + currentCount < text.Length)
                    CalculateLines(text, in words, startIndex + currentCount, in areaSize, fontScaleDelta, lineHeight, flowDirection, ref lines, generateLine, ref linesSize);
            }
        }

        //void CalculateLines(in SizeF size)
        //{
        //    mTextInLines.Clear();
        //    if (string.IsNullOrEmpty(mText))
        //        return;
        //    var words = new UNativeArray<FTWord>();
        //    FontAsset.GetWords(ref words, mText);

        //    LocalizationManager.ECulture breakerCulture;
        //    var scaleDelta = mFontSize * 1.0f / FontAsset.FontSize;
        //    var lineHeight = mFontSize * mLineSpacingScale;
        //    switch (FlowDirection)
        //    {
        //        case ETextDirection.LeftToRight:
        //            {
        //                var line = new stTextLineData()
        //                {
        //                    StartIndex = 0,
        //                    EndIndex = 0,
        //                    Rect = new RectangleF(0, 0, 0, lineHeight),
        //                };
        //                if (TextWrapping == ETextWrapping.NoWrap)
        //                {
        //                    line.EndIndex = mText.Length;
        //                    line.Rect.Height = lineHeight;
        //                    for (int i = 0; i < words.Count; i++)
        //                    {
        //                        line.Rect.Width += words[i].Advance.X * scaleDelta;
        //                    }
        //                }
        //                else
        //                {
        //                    float lastHeight = 0;
        //                    for (int i = 0; i < words.Count; i++)
        //                    {
        //                        var wordWidth = words[i].Advance.X * scaleDelta;
        //                        var culture = UEngine.Instance.LocalizationManager.GetCulture(mText[i]);
        //                        if ((line.Rect.Width + wordWidth > size.Width) &&
        //                            (culture != LocalizationManager.ECulture.Separator) &&
        //                            (culture != LocalizationManager.ECulture.Punctuation))
        //                        {
        //                            switch (TextWrapping)
        //                            {
        //                                case ETextWrapping.Wrap:
        //                                    {
        //                                        var breakerIdx = UEngine.Instance.LocalizationManager.GetLastWordBreaker(Text, line.StartIndex, i, out breakerCulture);
        //                                        if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
        //                                            (breakerCulture == LocalizationManager.ECulture.Punctuation))
        //                                            breakerIdx += 1;
        //                                        if (i == line.StartIndex)
        //                                        {
        //                                            line.EndIndex = i + 1;
        //                                            line.Rect.Width += wordWidth;
        //                                        }
        //                                        else if (breakerIdx <= line.StartIndex)
        //                                        {
        //                                            line.EndIndex = i;
        //                                            line.Rect.Width += wordWidth;
        //                                            if (i < words.Count - 1)
        //                                            {
        //                                                mTextInLines.Add(line);
        //                                                lastHeight += line.Rect.Height;
        //                                                line = new stTextLineData()
        //                                                {
        //                                                    StartIndex = i,
        //                                                    Rect = new RectangleF(0, lastHeight, 0, lineHeight),
        //                                                };
        //                                                i--;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            line.EndIndex = breakerIdx;
        //                                            mTextInLines.Add(line);
        //                                            lastHeight += line.Rect.Height;
        //                                            line = new stTextLineData()
        //                                            {
        //                                                StartIndex = breakerIdx,
        //                                                Rect = new RectangleF(0, lastHeight, 0, lineHeight),
        //                                            };
        //                                            i = breakerIdx - 1;
        //                                        }
        //                                    }
        //                                    break;
        //                                case ETextWrapping.WrapWithOverflow:
        //                                    {
        //                                        var breakerIdx = UEngine.Instance.LocalizationManager.GetLastWordBreaker(Text, line.StartIndex, i, out breakerCulture);
        //                                        if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
        //                                            (breakerCulture == LocalizationManager.ECulture.Punctuation))
        //                                            breakerIdx += 1;
        //                                        if (breakerIdx <= line.StartIndex)
        //                                        {
        //                                            line.EndIndex = i + 1;
        //                                            line.Rect.Width += wordWidth;
        //                                        }
        //                                        else
        //                                        {
        //                                            line.EndIndex = breakerIdx;
        //                                            mTextInLines.Add(line);
        //                                            lastHeight += line.Rect.Height;
        //                                            line = new stTextLineData()
        //                                            {
        //                                                StartIndex = breakerIdx,
        //                                                Rect = new RectangleF(0, lastHeight, 0, lineHeight),
        //                                            };
        //                                            i = breakerIdx - 1;
        //                                        }
        //                                    }
        //                                    break;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            line.EndIndex = i + 1;
        //                            line.Rect.Width += wordWidth;
        //                        }
        //                    }
        //                }
        //                mTextInLines.Add(line);
        //            }
        //            break;
        //        case ETextDirection.RightToLeft:
        //            {
        //                var line = new stTextLineData()
        //                {
        //                    StartIndex = 0,
        //                    EndIndex = 0,
        //                    Rect = new RectangleF(size.Width, 0, 0, lineHeight),
        //                };
        //                if (TextWrapping == ETextWrapping.NoWrap)
        //                {
        //                    line.EndIndex = mText.Length;
        //                    line.Rect.Height = lineHeight;
        //                    for (int i = 0; i < words.Count; i++)
        //                    {
        //                        var wordWidth = words[i].Advance.X * scaleDelta;
        //                        line.Rect.X -= wordWidth;
        //                        line.Rect.Width += wordWidth;
        //                    }
        //                }
        //                else
        //                {
        //                    float lastHeight = 0;
        //                    for (int i = 0; i < words.Count; i++)
        //                    {
        //                        var wordWidth = words[i].Advance.X * scaleDelta;
        //                        var culture = UEngine.Instance.LocalizationManager.GetCulture(mText[i]);
        //                        if ((line.Rect.Width + wordWidth > size.Width) &&
        //                            (culture != LocalizationManager.ECulture.Separator) &&
        //                            (culture != LocalizationManager.ECulture.Punctuation))
        //                        {
        //                            switch (TextWrapping)
        //                            {
        //                                case ETextWrapping.Wrap:
        //                                    {
        //                                        var breakerIdx = UEngine.Instance.LocalizationManager.GetLastWordBreaker(Text, line.StartIndex, i, out breakerCulture);
        //                                        if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
        //                                            (breakerCulture == LocalizationManager.ECulture.Punctuation))
        //                                            breakerIdx += 1;
        //                                        if (i == line.StartIndex)
        //                                        {
        //                                            line.EndIndex = i + 1;
        //                                            line.Rect.X -= wordWidth;
        //                                            line.Rect.Width += wordWidth;
        //                                        }
        //                                        else if (breakerIdx <= line.StartIndex)
        //                                        {
        //                                            line.EndIndex = i;
        //                                            //line.Rect.X -= wordWidth;
        //                                            //line.Rect.Width += wordWidth;
        //                                            if (i < words.Count - 1)
        //                                            {
        //                                                mTextInLines.Add(line);
        //                                                lastHeight += line.Rect.Height;
        //                                                line = new stTextLineData()
        //                                                {
        //                                                    StartIndex = i,
        //                                                    Rect = new RectangleF(size.Width, lastHeight, 0, lineHeight),
        //                                                };
        //                                                i--;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            line.EndIndex = breakerIdx;
        //                                            var tempIdxStart = breakerIdx;
        //                                            if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
        //                                                (breakerCulture == LocalizationManager.ECulture.Punctuation))
        //                                                tempIdxStart = breakerIdx - 1;
        //                                            for (int tempIdx = tempIdxStart; tempIdx < i;  tempIdx++)
        //                                            {
        //                                                var tempWordWidth = words[tempIdx].Advance.X * scaleDelta;
        //                                                line.Rect.X += tempWordWidth;
        //                                                line.Rect.Width -= tempWordWidth;
        //                                            }
        //                                            mTextInLines.Add(line);
        //                                            lastHeight += line.Rect.Height;
        //                                            line = new stTextLineData()
        //                                            {
        //                                                StartIndex = breakerIdx,
        //                                                Rect = new RectangleF(size.Width, lastHeight, 0, lineHeight),
        //                                            };
        //                                            i = breakerIdx - 1;
        //                                        }
        //                                    }
        //                                    break;
        //                                case ETextWrapping.WrapWithOverflow:
        //                                    {
        //                                        var breakerIdx = UEngine.Instance.LocalizationManager.GetLastWordBreaker(Text, line.StartIndex, i, out breakerCulture);
        //                                        if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
        //                                            (breakerCulture == LocalizationManager.ECulture.Punctuation))
        //                                            breakerIdx += 1;
        //                                        if (breakerIdx <= line.StartIndex)
        //                                        {
        //                                            line.EndIndex = i + 1;
        //                                            line.Rect.X -= wordWidth;
        //                                            line.Rect.Width += wordWidth;
        //                                        }
        //                                        else
        //                                        {
        //                                            line.EndIndex = breakerIdx;
        //                                            var tempIdxStart = breakerIdx;
        //                                            if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
        //                                                (breakerCulture == LocalizationManager.ECulture.Punctuation))
        //                                                tempIdxStart = breakerIdx - 1;
        //                                            for (int tempIdx = tempIdxStart; tempIdx < i;  tempIdx++)
        //                                            {
        //                                                var tempWordWidth = words[tempIdx].Advance.X * scaleDelta;
        //                                                line.Rect.X += tempWordWidth;
        //                                                line.Rect.Width -= tempWordWidth;
        //                                            }
        //                                            mTextInLines.Add(line);
        //                                            lastHeight += line.Rect.Height;
        //                                            line = new stTextLineData()
        //                                            {
        //                                                StartIndex = breakerIdx,
        //                                                Rect = new RectangleF(size.Width, lastHeight, 0, lineHeight),
        //                                            };
        //                                            i = breakerIdx - 1;
        //                                        }
        //                                    }
        //                                    break;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            line.EndIndex = i + 1;
        //                            line.Rect.X -= wordWidth;
        //                            line.Rect.Width += wordWidth;
        //                        }
        //                    }
        //                }
        //                mTextInLines.Add(line);
        //            }
        //            break;
        //    }
        //}

        //SizeF mMeasureAvailableSize;

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            //mMeasureAvailableSize = availableSize;
            //CalculateLines(in availableSize);
            //var finalRect = CalculateLinesRectangele();
            SizeF linesSize = SizeF.Empty;
            var scaleDelta = mFontSize * 1.0f / FontAsset.FontSize;
            var lineHeight = mFontSize * mLineSpacingScale;
            var words = new UNativeArray<FTWord>();
            FontAsset.GetWords(ref words, mText);
            CalculateLines(mText, in words, 0, availableSize, scaleDelta, lineHeight, FlowDirection, ref mTextInLines, false, ref linesSize);
            return linesSize;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            //if (arrangeSize.Size == mMeasureAvailableSize)
            //    return;
            //CalculateLines(arrangeSize.Size);
            mTextInLines.Clear();
            var linesSize = SizeF.Empty;
            var scaleDelta = mFontSize * 1.0f / FontAsset.FontSize;
            var lineHeight = mFontSize * mLineSpacingScale;
            var words = new UNativeArray<FTWord>();
            FontAsset.GetWords(ref words, mText);
            CalculateLines(mText, in words, 0, arrangeSize.Size, scaleDelta, lineHeight, FlowDirection, ref mTextInLines, true, ref linesSize);
        }
    }
}
