using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EditorCommon.Controls
{
    public class ImageEx : Image
    {
        public bool DrawImageWith9Cells
        {
            get { return (bool)GetValue(DrawImageWith9CellsProperty); }
            set { SetValue(DrawImageWith9CellsProperty, value); }
        }
        public static readonly DependencyProperty DrawImageWith9CellsProperty =
            DependencyProperty.Register("DrawImageWith9Cells", typeof(bool), typeof(ImageEx), new FrameworkPropertyMetadata(false));
        public Int32Rect ClipRect
        {
            get { return (Int32Rect)GetValue(ClipRectProperty); }
            set { SetValue(ClipRectProperty, value); }
        }
        public static readonly DependencyProperty ClipRectProperty =
            DependencyProperty.Register("ClipRect", typeof(Int32Rect), typeof(ImageEx), new FrameworkPropertyMetadata(Int32Rect.Empty));
        public Thickness ClipPadding
        {
            get { return (Thickness)GetValue(ClipPaddingProperty); }
            set { SetValue(ClipPaddingProperty, value); }
        }
        public static readonly DependencyProperty ClipPaddingProperty =
            DependencyProperty.Register("ClipPadding", typeof(Thickness), typeof(ImageEx), new FrameworkPropertyMetadata(new Thickness(0)));

        protected override void OnRender(DrawingContext dc)
        {
            if (DrawImageWith9Cells == false)
            {
                base.OnRender(dc);
                return;
            }
            //if(ClipRect != Int32Rect.Empty)
            //{
            //    DrawBitblt(dc);
            //    return;
            //}
            RenderWith9Cells(dc);
        }

        ImageSource SplitImage(BitmapSource source, Int32Rect clipRect)
        {
            ImageSource results = null;
            var stride = clipRect.Width * ((source.Format.BitsPerPixel + 7) / 8);
            var pixelsCount = clipRect.Width * clipRect.Height;//tileWidth * tileHeight;
            var tileRect = new Int32Rect(0, 0, clipRect.Width, clipRect.Height);

            var pixels = new int[pixelsCount];
            //var copyRect = new Int32Rect(col * tileWidth, row * tileHeight, tileWidth, tileHeight);
            source.CopyPixels(clipRect, pixels, stride, 0);
            var wb = new WriteableBitmap(
                clipRect.Width,
                clipRect.Height,
                source.DpiX,
                source.DpiY,
                source.Format,
                source.Palette);
            wb.Lock();
            wb.WritePixels(tileRect, pixels, stride, 0);
            wb.Unlock();

            results = wb;
            return results;
        }
        ImageSource[] Get9CellImageSource(BitmapSource source, Int32Rect clipRect)
        {
            ImageSource[] results = new ImageSource[9];
            Int32Rect rect = Int32Rect.Empty;
            int rightSideWidth = (int)(source.PixelWidth - clipRect.X - clipRect.Width);
            //top-left
            rect.Width = clipRect.X;
            rect.Height = clipRect.Y;
            if(rect.Width != 0 && rect.Height != 0)
                results[0] = SplitImage(source, rect);
            //return results;
            //top-middle
            rect.X += rect.Width;
            rect.Width = clipRect.Width;
            if(rect.Width != 0 && rect.Height != 0)
                results[1] = SplitImage(source, rect);
            //top-right
            rect.X += rect.Width;
            rect.Width = rightSideWidth;
            if(rect.Width != 0 && rect.Height != 0)
                results[2] = SplitImage(source, rect);

            //left side
            rect = Int32Rect.Empty;
            rect.Y = clipRect.Y;
            rect.Width = clipRect.X;
            rect.Height = clipRect.Height;
            if(rect.Width != 0 && rect.Height != 0)
                results[3] = SplitImage(source, rect);

            //middle
            rect.X += rect.Width;
            rect.Width = clipRect.Width;
            if(rect.Width != 0 && rect.Height != 0)
                results[4] = SplitImage(source, rect);  

            //right side
            rect.X += rect.Width;
            rect.Width = rightSideWidth;
            if(rect.Width != 0 && rect.Height != 0)
                results[5] = SplitImage(source, rect);
            //bottom-left
            rect = Int32Rect.Empty;
            rect.Y = clipRect.Y + clipRect.Height;
            // rect.X = clipRect.X;
            rect.Height = source.PixelHeight - clipRect.Height - clipRect.Y;
            rect.Width = clipRect.X;
            if(rect.Width != 0 && rect.Height != 0)
                results[6] = SplitImage(source, rect);

            //bottom-middle
            rect.X += rect.Width;
            rect.Width = clipRect.Width;
            if (rect.Width != 0 && rect.Height != 0)
                results[7] = SplitImage(source, rect);
            //bottom-right
            rect.X += rect.Width;
            rect.Width = rightSideWidth;
            if (rect.Width != 0 && rect.Height != 0)
                results[8] = SplitImage(source, rect);
            return results;
        }
        //private void DrawBitblt(DrawingContext dc)
        //{
        //    //增加剪裁后九宫
        //    if (DrawImageWith9Cells == false)
        //    {
        //        ImageSource source = GetImageSource();
        //        Rect rect = new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight));
        //        dc.DrawImage(Source, rect);
        //    }
        //    else
        //    {
        //        RenderWith9Cells(dc);
        //    }
        //}
        private void RenderWith9Cells(System.Windows.Media.DrawingContext dc)
        {
            if (Source == null) return;
            //if (ClipPadding.Right == 0 || ClipPadding.Bottom == 0) return;
 
            ImageSource source = Source;
            Uri u = new Uri(source.ToString());
            var image = new BitmapImage(u);
            if (ClipRect != Int32Rect.Empty)
            {
                //预剪裁
                image = SplitImage(image, ClipRect) as BitmapImage;
            }
            double contentWidth = image.PixelWidth - ClipPadding.Left - ClipPadding.Right;
            double contentHeight = image.PixelHeight - ClipPadding.Top - ClipPadding.Bottom;
            Int32Rect contentRect = new Int32Rect((int)ClipPadding.Left, (int)ClipPadding.Top, (int)contentWidth, (int)contentHeight);
 
            //  Rect r = new Rect(new Point(),RenderSize);
            //  dc.DrawImage(image,r);
            //return;
            // image.BeginInit();
            // image.EndInit(); 
            ImageSource[] images = Get9CellImageSource(image, contentRect);
            if (images == null || images.Length != 9)
            {
                base.OnRender(dc);
                return;
            }
            DrawFrame(dc, images, contentRect);
            // DrawContent(contentRect, dc);
        }
        private void DrawFrame(System.Windows.Media.DrawingContext drawingContext, ImageSource[] images, Int32Rect contentRect)
        {
            if (ActualWidth == 0 || ActualHeight == 0)
                return;
            Rect drawRect = new Rect(new Point(), new Size(contentRect.X, contentRect.Y));
            double drawWidth = ActualWidth - ClipPadding.Left - ClipPadding.Right;
            double drawHeight = ActualHeight - ClipPadding.Top - ClipPadding.Bottom;
            if (drawWidth < 0 || drawHeight < 0)
                return;
            if(images[0] != null)
                drawingContext.DrawImage(images[0], drawRect);

            drawRect.X += drawRect.Width;
            drawRect.Width = drawWidth;
            if (images[1] != null)
                drawingContext.DrawImage(images[1], drawRect);
            //for (int i = (int)drawRect.X; i < ActualWidth - contentRect.Width - contentRect.X; i += contentRect.Width)
            //{
            //    drawRect.X += drawRect.Width;
            //    drawRect.Width = contentRect.Width;
            //    drawingContext.DrawImage(images[1], drawRect);
            //}
            drawRect.X += drawRect.Width;
            drawRect.Width = ClipPadding.Right;
            if (images[2] != null)
                drawingContext.DrawImage(images[2], drawRect);
            //中间

            drawRect.X = 0;
            drawRect.Y = contentRect.Y;
            drawRect.Width = contentRect.X;
            drawRect.Height = drawHeight;
            if (images[3] != null)
                drawingContext.DrawImage(images[3], drawRect);

            drawRect.X += drawRect.Width;
            drawRect.Width = drawWidth;
            if (images[4] != null)
                drawingContext.DrawImage(images[4], drawRect);

            drawRect.X += drawRect.Width;
            drawRect.Width = ClipPadding.Right;
            if (images[5] != null)
                drawingContext.DrawImage(images[5], drawRect);

            //下边
            drawRect.X = 0;
            drawRect.Y = ActualHeight - ClipPadding.Bottom;
            drawRect.Width = ClipPadding.Left;
            drawRect.Height = ClipPadding.Bottom;
            if (images[6] != null)
                drawingContext.DrawImage(images[6], drawRect);

            drawRect.X += drawRect.Width;
            drawRect.Width = drawWidth;
            if (images[7] != null)
                drawingContext.DrawImage(images[7], drawRect);

            drawRect.X += drawRect.Width;
            drawRect.Width = ClipPadding.Right;
            if (images[8] != null)
                drawingContext.DrawImage(images[8], drawRect);
        }
    }
}
