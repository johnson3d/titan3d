using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EngineNS.UISystem.Controls
{
   
    [Rtti.MetaClass]
    public class ImageInitializer : UIElementInitializer
    {
        [Rtti.MetaData]
       
        public Brush ImageBrush { get; set; } = new Brush();
        [Rtti.MetaData]
        public bool UseImageSize { get; set; } = true;

        public ImageInitializer()
        {
            //IsVariable = true;
        }
    }
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor_UIControlInit(typeof(ImageInitializer))]
    [Editor_UIControl("通用.图片", "图片控件", "Image.png")]
   
    public class Image : UIElement
    {
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
       
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Brush ImageBrush
        {
            get => ((ImageInitializer)mInitializer).ImageBrush;
        }

        [DisplayName("使用图片尺寸")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool UseImageSize
        {
            get => ((ImageInitializer)mInitializer).UseImageSize;
            set
            {
                var init = (ImageInitializer)mInitializer;
                if (init.UseImageSize == value)
                    return;

                init.UseImageSize = value;
                UpdateLayout();
                OnPropertyChanged("UseImageSize");
            }
        }

        public override async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            if (!await base.Initialize(rc, init))
                return false;

            var imgInit = init as ImageInitializer;
            mCurrentBrush = imgInit.ImageBrush;
            await mCurrentBrush.Initialize(rc, this);
            if(UseImageSize)
            {
                var rect = imgInit.DesignRect;
                var imgSize = mCurrentBrush.ImageSize;
                imgInit.DesignRect = new RectangleF(rect.X, rect.Y, imgSize.X, imgSize.Y);
            }
            return true;
        }

        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            if(UseImageSize)
            {
                var imgBrush = ImageBrush;
                if (imgBrush == null)
                    return SizeF.Empty;
                var imgSize = imgBrush.ImageSize;
                return new SizeF(imgSize.X, imgSize.Y);
            }
            else
            {
                return base.MeasureOverride(ref availableSize);
            }
        }
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            base.ArrangeOverride(ref arrangeSize);
        }
    }
}
