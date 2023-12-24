using EngineNS.Bricks.Font;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtText : TtUIElement
    {
        TtFontSDF mFontAsset;
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

        public enum ETextTrimming : byte
        {
            None,
            CharacterEllipsis,
            WordEllipsis,
        }
        ETextTrimming mTextTrimming = ETextTrimming.None;
        [Rtti.Meta, BindProperty]
        [Category("Text")]
        public ETextTrimming TextTrimming
        {
            get => mTextTrimming;
            set
            {
                OnValueChange(value, mTextTrimming);
                mTextTrimming = value;
                MeshDirty = true;
            }
        }

        public enum ETextWrapping : byte
        {
            WrapWithOverflow,
            NoWrap,
            Wrap,
        }
        ETextWrapping mTextWrapping = ETextWrapping.Wrap;
        [Rtti.Meta, BindProperty]
        [Category("Text")]
        public ETextWrapping TextWrapping
        {
            get => mTextWrapping;
            set
            {
                OnValueChange(value, mTextWrapping);
                mTextWrapping = value;
                MeshDirty = true;
            }
        }

        public override bool IsReadyToDraw()
        {
            return true;
        }
        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            if (string.IsNullOrEmpty(mText))
                return;
            if(mFontAsset == null)
            {
                // todo: texture size 处理
                mFontAsset = UEngine.Instance.FontModule.FontManager.GetFontSDF(mFont, mFontSize, 1024, 1024);
            }

            batch.Middleground.PushTransformIndex(TransformIndex);
            batch.Middleground.PushFont(mFontAsset);
            batch.Middleground.AddText(mText, mCurFinalRect.Left, mCurFinalRect.Top, Color4f.FromABGR(Color.LightPink));
            batch.Middleground.PopFont();
            batch.Middleground.PopTransformIndex();
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            return base.MeasureOverride(availableSize);
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            base.ArrangeOverride(arrangeSize);
        }
    }
}
