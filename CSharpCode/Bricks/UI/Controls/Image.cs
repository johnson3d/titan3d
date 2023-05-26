using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtImage : TtUIElement
    {
        TtCanvasBrush mDrawBrush = new TtCanvasBrush();
        EGui.UUvAnim mUVAnim;
        [Browsable(false)]
        public EGui.UUvAnim UVAnim
        {
            get => mUVAnim;
            set => mUVAnim = value;
        }
        RName mUVAnimAsset;
        [Rtti.Meta, UI.Bind.BindProperty]
        [DisplayName("Texture"), Category("Brush")]
        [RName.PGRName(FilterExts = EGui.UUvAnim.AssetExt)]
        public RName UVAnimAsset
        {
            get => mUVAnimAsset;
            set
            {
                mUVAnimAsset = value;
                _ = UpdateUVAnim(mUVAnimAsset);
                OnValueChange(value);
            }
        }

        public TtImage()
        {
            _ = Initialize();
        }

        async Thread.Async.TtTask UpdateUVAnim(RName uvAnimAssist)
        {
            mUVAnim = await UEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(mUVAnimAsset);
            IsPropertyVisibleDirty = true;
            RootUIHost.MeshDirty = true;
        }

        async Thread.Async.TtTask Initialize()
        {
            mDrawBrush.Name = "utest/ddd.uminst";
            var texture = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/ground_01.srv"));
            mDrawBrush.SetSrv(texture);
        }

        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            //mBrush.Name = "utest/ddd.uminst";
            //var texture = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/groundsnow.srv"));
            //mBrush.mCoreObject.SetSrv(texture.mCoreObject);
            //batch.Middleground.PushBrush(mBrush);
            if(mUVAnim != null)
            {
                mDrawBrush.SetSrv(mUVAnim.mTexture);
                Vector2 uvMin, uvMax;
                mUVAnim.GetUV(0, out uvMin, out uvMax);
                mDrawBrush.SetUV(in uvMin, in uvMax);
            }
            else
            {
                // default texture
            }

            var outCmd = new EngineNS.Canvas.FSubDrawCmd();
            batch.Middleground.PushClip(mDesignClipRect);
            //var brush = new TtCanvasBrush();
            //brush.Name = "utest/ddd.uminst";
            //var texture = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/ground_01.srv"));
            //brush.mCoreObject.SetSrv(texture.mCoreObject);
            batch.Middleground.AddImage(mDrawBrush.mCoreObject, mCurFinalRect.Left, mCurFinalRect.Top, mCurFinalRect.Width, mCurFinalRect.Height, Color.DarkRed, ref outCmd);
            batch.Middleground.PopClip();
        }

        public override void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            base.GetProperties(ref collection, parentIsValueType);

            var thisType = Rtti.UTypeDesc.TypeOf(this.GetType());
            // Color
            var colorProDesc = PropertyCollection.PropertyDescPool.QueryObjectSync();
            colorProDesc.Name = "Color";
            colorProDesc.DisplayName = colorProDesc.Name;
            colorProDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(UInt32));
            colorProDesc.CustomValueEditor = new UByte4ToColor4PickerEditorAttribute();
            colorProDesc.Attributes = new AttributeCollection(new Attribute[] { colorProDesc.CustomValueEditor });
            colorProDesc.Description = "Set hint color";
            colorProDesc.IsReadonly = false;
            colorProDesc.IsBrowsable = true;
            colorProDesc.Category = "Brush";
            colorProDesc.ParentIsValueType = parentIsValueType ? parentIsValueType : thisType.IsValueType;
            colorProDesc.DeclaringType = thisType;
            collection.Add(colorProDesc);
        }
        public override object GetPropertyValue(string propertyName)
        {
            switch(propertyName)
            {
                case "Color":
                    {
                        if (mUVAnim != null)
                            return mUVAnim.Color;
                        return 0xffffffff;
                    }
            }
            return base.GetPropertyValue(propertyName);
        }
        public override void SetPropertyValue(string propertyName, object value)
        {
            switch(propertyName)
            {
                case "Color":
                    {
                        if (mUVAnim != null)
                            mUVAnim.Color = (UInt32)value;
                    }
                    break;
            }
            base.SetPropertyValue(propertyName, value);
        }
    }
}
