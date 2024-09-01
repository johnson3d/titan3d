using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI
{
    public class TtUIRender : ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        Canvas.TtCanvas mCanvas = new Canvas.TtCanvas();

        public void TickLogic(float ellapse)
        {
            throw new NotImplementedException();
        }

        public void TickRender(float ellapse)
        {
            throw new NotImplementedException();
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            throw new NotImplementedException();
        }
    }


    [BindableObject, PGNoCategory]
    public partial class TtBrush : IPropertyCustomization
    {
        [System.ComponentModel.Browsable(false)]
        public virtual bool IsPropertyVisibleDirty
        {
            get;
            set;
        } = false;
        public virtual void GetProperties(ref EngineNS.EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = System.ComponentModel.TypeDescriptor.GetProperties(this);
            switch(BrushType)
            {
                case EBrushType.Image:
                    var pro = pros["BorderThickness"];
                    if (pro != null)
                        pros.Remove(pro);
                    break;
                case EBrushType.Border:
                    break;
                case EBrushType.Rectangle:
                    break;
            }
            collection.InitValue(this, EngineNS.Rtti.UTypeDesc.TypeOf(this.GetType()), pros, parentIsValueType);

            TtBindingOperations.DefaultGetBindProperties(this, mBindExprDic, ref collection);
        }

        TtCanvasBrush mDrawBrush = new TtCanvasBrush();
        //public TtUIElement HostElement;

        //EGui.UUvAnim mUVAnim;
        //[Browsable(false)]
        //public EGui.UUvAnim UVAnim
        //{
        //    get => mUVAnim;
        //    set => mUVAnim = value;
        //}
        RName mUVAnimAsset;
        [Rtti.Meta, BindProperty]
        [DisplayName("Texture")]
        [RName.PGRName(FilterExts = EGui.TtUVAnim.AssetExt)]
        public RName UVAnimAsset
        {
            get => mUVAnimAsset;
            set
            {
                OnValueChange(value, mUVAnimAsset);
                mUVAnimAsset = value;
                if (value == null)
                    mUVAnimTask = null;
                else
                    mUVAnimTask = TtEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(mUVAnimAsset);
                UpdateMesh();
            }
        }
        Thread.Async.TtTask<EGui.TtUVAnim>? mUVAnimTask;

        RName mMaterial = RName.GetRName("ui/uimat_inst_default.uminst", RName.ERNameType.Engine);
        [Rtti.Meta, BindProperty]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
        public RName Material
        {
            get => mMaterial;
            set
            {
                OnValueChange(value, mMaterial);
                mMaterial = value;
            }
        }

        public enum EBrushType
        {
            Image,
            Border,
            Rectangle,
        }
        EBrushType mBrushType = EBrushType.Image;
        [Rtti.Meta, BindProperty]
        [DisplayName("Type")]
        public EBrushType BrushType
        {
            get => mBrushType;
            set
            {
                OnValueChange(value, mBrushType);
                mBrushType = value;
                UpdateMesh();
            }
        }

        EngineNS.Color4b mColor = EngineNS.Color4b.White;
        [Rtti.Meta, BindProperty]
        [EGui.Controls.PropertyGrid.Color4PickerEditor()]
        public EngineNS.Color4b Color
        {
            get => mColor;
            set
            {
                if (mColor == value)
                    return;
                OnValueChange(value, mColor);
                mColor = value;

                if (mDrawBrush != null)
                    mDrawBrush.Color = value;

                if (mDrawCmd.DrawCmd.DrawCount > 1)
                    UpdateMesh();
            }
        }

        //Thickness mBorderThickness;
        //[Rtti.Meta, BindProperty]
        //public Thickness BorderThickness
        //{
        //    get => mBorderThickness;
        //    set
        //    {
        //        mBorderThickness = value;
        //        UpdateMesh();
        //        OnValueChange(value);
        //    }
        //}

        //Vector4 mCornerRadius = Vector4.Zero;
        //[Rtti.Meta, BindProperty, CornerRadiusEditor]
        //public Vector4 CornerRadius
        //{
        //    get => mCornerRadius;
        //    set
        //    {
        //        mCornerRadius = value;
        //        UpdateMesh();
        //        OnValueChange(value);
        //    }
        //}

        public TtBrush()
        {
        }

        public TtBrush(Color4b color, EBrushType brushType)
        {
            mColor = color;
            mBrushType = brushType;
        }
        public TtBrush(RName uvAnim)
        {
            UVAnimAsset = uvAnim;
            mBrushType = EBrushType.Image;
        }

        void UpdateMesh()
        {
            IsPropertyVisibleDirty = true;
            if (HostElement != null)
                HostElement.MeshDirty = true;
        }

        Thread.Async.TtTask<NxRHI.TtSrView>? mDefaultTextureTask = null;
        public bool IsReadyToDraw()
        {
            if (mDefaultTextureTask == null)
            {
                mDefaultTextureTask = TtEngine.Instance.GfxDevice.TextureManager.GetTexture(TtEngine.Instance.Config.UIDefaultTexture);
                return false;
            }
            else if (mDefaultTextureTask.Value.IsCompleted == false)
            {
                return false;
            }
            if(mUVAnimTask != null)
            {
                if (mUVAnimTask.Value.IsCompleted == false)
                    return false;
            }

            return true;
        }

        public void Draw(TtUIElement host, in RectangleF clipRect, in RectangleF drawRect, TtCanvasDrawBatch batch)
        {
            Draw(host, clipRect, drawRect, batch, Vector4.Zero);
        }
        public void Draw(TtUIElement host, in RectangleF clipRect, in RectangleF drawRect, TtCanvasDrawBatch batch, in Vector4 cornerRadius)
        {
            Draw(host, clipRect, drawRect, batch, cornerRadius, Thickness.Empty);
        }
        EngineNS.Canvas.FSubDrawCmd mDrawCmd = new EngineNS.Canvas.FSubDrawCmd();
        public void Draw(TtUIElement host, in RectangleF clipRect, in RectangleF drawRect, TtCanvasDrawBatch batch, in Vector4 cornerRadius, in Thickness borderThickness)
        {
            if (!IsReadyToDraw())
                return;

            HostElement = host;

#if DEBUG_UI
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "UI", $"Draw element:{HostElement.Name}({HostElement.GetType().FullName}), BrushType:{BrushType}");
#endif

            if (RName.IsEmpty(mMaterial))
                mDrawBrush.Name = "@MatInst:ui/uimat_inst_default.uminst:Engine";
            else
                mDrawBrush.Name = "@MatInst:" + mMaterial.ToString();
            if (mUVAnimTask != null)
            {
                var texture = mUVAnimTask.Value.Result.Texture;
                if (texture == null)
                    texture = mDefaultTextureTask.Value.Result;
                mDrawBrush.SetSrv(texture);
                Vector2 uvMin, uvMax;
                mUVAnimTask.Value.Result.GetUV(0, out uvMin, out uvMax);
                mDrawBrush.SetUV(in uvMin, in uvMax);
                //mDrawBrush.Name = mUVAnimTask.Value.Result.
            }
            else
            {
                // default texture
                mDrawBrush.SetSrv(mDefaultTextureTask.Value.Result);
            }
            mDrawBrush.Color = Color;

            batch.Middleground.PushClip(clipRect);
            batch.Middleground.PushTransformIndex(HostElement.TransformIndex);
            switch(BrushType)
            {
                case EBrushType.Image:
                    batch.Middleground.AddImage(mDrawBrush.mCoreObject, drawRect.Left, drawRect.Top, drawRect.Width, drawRect.Height, mColor, ref mDrawCmd);
                    break;
                case EBrushType.Border:
                    batch.Middleground.PushBrush(mDrawBrush);
                    batch.Middleground.AddRect(drawRect, borderThickness, cornerRadius, mColor, ref mDrawCmd);
                    batch.Middleground.PopBrush();
                    break;
                case EBrushType.Rectangle:
                    batch.Middleground.PushBrush(mDrawBrush);
                    batch.Middleground.AddRectFill(drawRect, cornerRadius, mColor, ref mDrawCmd);
                    batch.Middleground.PopBrush();
                    break;
            }
            batch.Middleground.PopTransformIndex();
            batch.Middleground.PopClip();
        }
    }
}
