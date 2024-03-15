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

                for(int i=0; i<mBrushes.Count; i++)
                {
                    mBrushes[i].Color = mColor;
                }
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

        [Rtti.Meta, BindProperty, Category("Text")]
        public string TrimmingText
        {
            get;
            set;
        } = "...";

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
            public bool IsTrimming;
        }
        List<stTextLineData> mTextInLines = new List<stTextLineData>();
        List<string> mTextSplitWithLanguages = new List<string>();
        List<TtCanvasBrush> mBrushes = new List<TtCanvasBrush>();

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
                if(mTextInLines[i].IsTrimming)
                    text += TrimmingText;
                Support.UBlobObject blobObj = new UBlobObject();
                batch.Middleground.AddText(text, x, y, Color, blobObj);
                unsafe
                {
                    using(var reader = IO.UMemReader.CreateInstance((byte*)blobObj.DataPointer, blobObj.Size))
                    {
                        var ptrSize = sizeof(void*);
                        while(true)
                        {
                            void* ptr;
                            reader.ReadPtr(&ptr, ptrSize);
                            if (ptr == (void*)1)
                                break;
                            var canvasBrush = new EngineNS.Canvas.ICanvasBrush(ptr);
                            CoreSDK.IUnknown_AddRef(ptr);
                            mBrushes.Add(new TtCanvasBrush(canvasBrush));
                        }
                    }
                }
            }
            //batch.Middleground.AddText(mText, mCurFinalRect.Left, mCurFinalRect.Top, Color4f.FromABGR(Color.LightPink));
            batch.Middleground.PopClip();
            batch.Middleground.PopMatrix();
            batch.Middleground.PopFont();
            batch.Middleground.PopTransformIndex();
        }

        float CalculateFlowDirection(ETextDirection flowDirection, in SizeF areaSize, float newWidth)
        {
            float lineX = 0;
            switch (flowDirection)
            {
                case ETextDirection.LeftToRight:
                    lineX = 0;
                    break;
                case ETextDirection.RightToLeft:
                    lineX = areaSize.Width - newWidth;
                    break;
                case ETextDirection.Center:
                    lineX = (areaSize.Width - newWidth) * 0.5f;
                    break;
            }
            return lineX;
        }

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
                    int newCount;
                    float newWidth;
                    var isTrimming = CalculateTextTrimming(text, in words, in areaSize, fontScaleDelta, startIndex, text.Length, out newCount, width, out newWidth);

                    float lineX = CalculateFlowDirection(flowDirection, in areaSize, newWidth);
                    var line = new stTextLineData()
                    {
                        StartIndex = startIndex,
                        EndIndex = startIndex + newCount,
                        Rect = new RectangleF(lineX, 0, newWidth, lineHeight),
                        IsTrimming = isTrimming,
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
                            if (i == startIndex)
                            {
                                if(TextWrapping == ETextWrapping.Wrap)
                                {
                                    currentCount += 1;
                                    currentWidth += wordWidth;
                                }
                                else if(TextWrapping == ETextWrapping.WrapWithOverflow)
                                {
                                    LocalizationManager.ECulture breakerCulture;
                                    var breakerIdx = UEngine.Instance.LocalizationManager.GetNextWordBreaker(Text, startIndex, words.Count - 1, out breakerCulture);
                                    if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
                                        (breakerCulture == LocalizationManager.ECulture.Punctuation))
                                        breakerIdx += 1;
                                    if (breakerIdx <= startIndex)
                                        breakerIdx = words.Count;
                                    currentCount += (breakerIdx - startIndex);
                                    for(int tempIdx = startIndex; tempIdx < breakerIdx; tempIdx++)
                                    {
                                        var tempWordWidth = words[tempIdx].Advance.X * fontScaleDelta;
                                        currentWidth += tempWordWidth;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                LocalizationManager.ECulture breakerCulture;
                                var breakerIdx = UEngine.Instance.LocalizationManager.GetLastWordBreaker(Text, startIndex, i, out breakerCulture);
                                if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
                                    (breakerCulture == LocalizationManager.ECulture.Punctuation))
                                    breakerIdx += 1;
                                if(breakerIdx <= startIndex)
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
                    }
                    else
                    {
                        currentCount += 1;
                        currentWidth += wordWidth;
                    }
                }
                if (generateLine)
                {
                    int newCount;
                    float newWidth;
                    var isTrimming = CalculateTextTrimming(text, in words, in areaSize, fontScaleDelta, startIndex, currentCount, out newCount, currentWidth, out newWidth);

                    float lineX = CalculateFlowDirection(flowDirection, in areaSize, newWidth);
                    var line = new stTextLineData()
                    {
                        StartIndex = startIndex,
                        EndIndex = startIndex + newCount,
                        Rect = new RectangleF(lineX, linesSize.Height, newWidth, lineHeight),
                        IsTrimming = isTrimming,
                    };
                    
                    lines.Add(line);
                }
                linesSize.Width = MathHelper.Max(currentWidth, linesSize.Width);
                linesSize.Height += lineHeight;
                if(startIndex + currentCount < text.Length)
                    CalculateLines(text, in words, startIndex + currentCount, in areaSize, fontScaleDelta, lineHeight, flowDirection, ref lines, generateLine, ref linesSize);
            }
        }

        bool CalculateTextTrimming(string text,
            in UNativeArray<FTWord> words,
            in SizeF areaSize,
            float fontScaleDelta,
            int startIndex,
            int currentCount,
            out int newCount,
            float currentWidth,
            out float newWidth)
        {
            newCount = currentCount;
            newWidth = currentWidth;
            if (TextTrimming == ETextTrimming.None)
                return false;

            var trimmingWords = new UNativeArray<FTWord>();
            FontAsset.GetWords(ref trimmingWords, TrimmingText);
            float trimmingWordsWidth = 0;
            for(int idx = 0; idx < trimmingWords.Count; idx++)
            {
                trimmingWordsWidth += trimmingWords[idx].Advance.X * fontScaleDelta;
            }
            if (currentWidth > areaSize.Width)
            {
                switch(TextTrimming)
                {
                    case ETextTrimming.WordEllipsis:
                        {
                            while(newWidth + trimmingWordsWidth > areaSize.Width)
                            {
                                var endIndex = startIndex + newCount - 1;
                                LocalizationManager.ECulture breakerCulture;
                                var breakerIdx = UEngine.Instance.LocalizationManager.GetLastWordBreaker(Text, startIndex, endIndex, out breakerCulture);
                                //if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
                                //    (breakerCulture == LocalizationManager.ECulture.Punctuation))
                                //    breakerIdx += 1;
                                if(breakerIdx <= startIndex)
                                {
                                    newCount = 1;
                                    newWidth = words[startIndex].Advance.X * fontScaleDelta + trimmingWordsWidth;
                                    return true;
                                }
                                else
                                {
                                    newCount = breakerIdx - startIndex;
                                    var tempIdxStart = breakerIdx;
                                    //if ((breakerCulture == LocalizationManager.ECulture.Separator) ||
                                    //    (breakerCulture == LocalizationManager.ECulture.Punctuation))
                                    //    tempIdxStart = breakerIdx - 1;
                                    if(tempIdxStart == endIndex)
                                    {
                                        newCount -= 1;
                                        newWidth -= words[tempIdxStart].Advance.X * fontScaleDelta;
                                    }
                                    else
                                    {
                                        for (int tempIdx = tempIdxStart; tempIdx <= endIndex; tempIdx++)
                                        {
                                            var tempWordWidth = words[tempIdx].Advance.X * fontScaleDelta;
                                            newWidth -= tempWordWidth;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ETextTrimming.CharacterEllipsis:
                        {
                            for(int wordIdx = currentCount - 1; wordIdx >= 1; wordIdx--)
                            {
                                newWidth -= words[wordIdx + startIndex].Advance.X * fontScaleDelta;
                                if(newWidth + trimmingWordsWidth <= areaSize.Width)
                                {
                                    newCount = wordIdx;
                                    break;
                                }
                            }
                            if(newCount == currentCount)
                            {
                                newCount = 1;
                            }
                        }
                        break;
                }

                newWidth += trimmingWordsWidth;
                return true;
            }
            return false;
        }

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
