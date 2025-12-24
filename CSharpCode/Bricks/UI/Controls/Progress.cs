using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    [Editor_UIControl("Controls.Progress", "Progress", "")]
    public partial class TtProgress : TtUIElement, EngineNS.EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        TtBrush mBackgroundBrush;
        [BindProperty, Rtti.Meta, Category("Appearance")]
        public TtBrush BackgroundBrush
        {
            get => mBackgroundBrush;
            set
            {
                OnValueChange(value, mBackgroundBrush);
                mBackgroundBrush.HostElement = this;
                mBackgroundBrush = value;
            }
        }

        TtBrush mProgressBrush;
        [BindProperty, Rtti.Meta, Category("Appearance")]
        public TtBrush ProgressBrush
        {
            get => mProgressBrush;
            set
            {
                OnValueChange(value, mProgressBrush);
                mProgressBrush.HostElement = this;
                mProgressBrush = value;
            }
        }

        float mPercent = 0.0f;
        [BindProperty, Rtti.Meta, Category("Progress")]
        [EGui.Controls.PropertyGrid.TtValueRange(0, 1)]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float Percent
        {
            get => mPercent;
            set
            {
                OnValueChange(value, mPercent);
                mPercent = value;
                MeshDirty = true;
            }
        }

        public enum EFillType
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop,
            FromCenter,
            Pie,
            PieReverse,
        }
        EFillType mFillType = EFillType.LeftToRight;
        [BindProperty, Rtti.Meta, Category("Progress")]
        public EFillType FillType
        {
            get => mFillType;
            set
            {
                OnValueChange(value, mFillType);
                mFillType = value;
                MeshDirty = true;
                IsPropertyVisibleDirty = true;
            }
        }

        float mPieStartAngle = 90.0f;
        [BindProperty, Rtti.Meta, Category("Progress")]
        [PGShowWithProperty<EFillType>(PropertyName = "FillType", PropertyValue = EFillType.Pie)]
        [PGShowWithProperty<EFillType>(PropertyName = "FillType", PropertyValue = EFillType.PieReverse)]
        public float PieStartAngle
        {
            get => mPieStartAngle;
            set
            {
                OnValueChange(value, mPieStartAngle);
                mPieStartAngle = value;
                MeshDirty = true;
            }
        }

        public TtProgress()
        {
            mBackgroundBrush = new TtBrush();
            mBackgroundBrush.HostElement = this;
            mProgressBrush = new TtBrush();
            mProgressBrush.HostElement = this;
            var task = Initialize();
            TtEngine.Instance.TaskCollector.AddWaitTask(task);
        }
        public async Thread.Async.TtTask Initialize()
        {
            NoHitTest = true;

        }
        public override bool IsReadyToDraw()
        {
            return mBackgroundBrush.IsReadyToDraw() &&
                   mProgressBrush.IsReadyToDraw();
        }
        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            mBackgroundBrush.Draw(this, in mDesignClipRect, in mCurFinalRect, batch);
            batch.Middleground.NewDrawCmd();
            batch.Middleground.PushZOffset(-0.01f);
            var rect = mCurFinalRect;
            Vector4 uv = new Vector4(0, 0, 1.0f, 1.0f);
            switch(mFillType)
            {
                case EFillType.LeftToRight:
                    {
                        rect.Width *= Percent;
                        uv.Z = Percent;
                        mProgressBrush.Draw(this, in mDesignClipRect, in rect, batch, in Vector4.Zero, in Thickness.Empty, in uv);
                    }
                    break;
                case EFillType.RightToLeft:
                    {
                        var width = rect.Width;
                        rect.Width *= Percent;
                        rect.X += width - rect.Width;
                        uv.X = (1 - Percent);
                        mProgressBrush.Draw(this, in mDesignClipRect, in rect, batch, in Vector4.Zero, in Thickness.Empty, in uv);
                    }
                    break;
                case EFillType.TopToBottom:
                    {
                        rect.Height *= Percent;
                        uv.W = Percent;
                        mProgressBrush.Draw(this, in mDesignClipRect, in rect, batch, in Vector4.Zero, in Thickness.Empty, in uv);
                    }
                    break;
                case EFillType.BottomToTop:
                    {
                        var height = rect.Height;
                        rect.Height *= Percent;
                        rect.Y += height - rect.Height;
                        uv.Y = (1 - Percent);
                        mProgressBrush.Draw(this, in mDesignClipRect, in rect, batch, in Vector4.Zero, in Thickness.Empty, in uv);
                    }
                    break;
                case EFillType.FromCenter:
                    {
                        var width = rect.Width;
                        var height = rect.Height;
                        rect.Width *= Percent;
                        rect.Height *= Percent;
                        rect.X += (width - rect.Width) * 0.5f;
                        rect.Y += (height - rect.Height) * 0.5f;
                        uv.X = (1 - Percent) * 0.5f;
                        uv.Y = (1 - Percent) * 0.5f;
                        uv.Z = Percent * 0.5f;
                        uv.W = Percent * 0.5f;
                        mProgressBrush.Draw(this, in mDesignClipRect, in rect, batch, in Vector4.Zero, in Thickness.Empty, in uv);
                    }
                    break;
                case EFillType.Pie:
                    {

                    }
                    break;
                case EFillType.PieReverse:
                    {

                    }
                    break;
            }
            batch.Middleground.PopZOffset();
        }
    }
}
