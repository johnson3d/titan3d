using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Tool
{   
    public class CGfxTextureAtlas
    {
        public struct AreaData
        {
            public int Slice;
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }
        public class TexArea
        {
            public RName Name;
            public AreaData Data;
            public bool IsFree = false;
            public int GetArea()
            {
                return Data.Width * Data.Height;
            }
            public override string ToString()
            {
                return Name.ToString() + $"[{Data.X},{Data.Y},{Data.Width},{Data.Height}]";
            }
        }
        public class TexSlice
        {
            public TexSlice(int w, int h, int slice)
            {
                Width = w;
                Height = h;
                Slice = slice;

                var full = new TexArea();
                full.Data.Slice = Slice;
                full.Data.X = 0;
                full.Data.Y = 0;
                full.Data.Width = w;
                full.Data.Height = h;
                full.IsFree = true;
                FreeAreas.Add(full);
            }
            public int Slice;
            public int Width;
            public int Height;
            public List<TexArea> ValidAreas = new List<TexArea>();
            public List<TexArea> FreeAreas = new List<TexArea>();

            public TexArea PushArea(int width, int height)
            {
                for(int i= FreeAreas.Count-1; i>=0; i--)
                {
                    var cur = FreeAreas[i];
                    if (cur.Data.Width >= width && cur.Data.Height >= height)
                    {
                        FreeAreas.RemoveAt(i);
                        var result = SplitArea(cur, width, height);
                        ValidAreas.Add(result);
                        return result;
                    }
                }
                return null;
            }
            public TexArea SplitArea(TexArea area, int w, int h)
            {
                if (area.Data.Width == w && area.Data.Height == h)
                    return area;

                var result = new TexArea();
                result.Data.Slice = Slice;
                result.Data.X = area.Data.X;
                result.Data.Y = area.Data.Y;
                result.Data.Width = w;
                result.Data.Height = h;
                result.IsFree = false;

                if (area.Data.Width - w >= area.Data.Height - h)
                {
                    var n1 = new TexArea();
                    n1.Data.Slice = Slice;
                    n1.Data.X = area.Data.X + w;
                    n1.Data.Y = area.Data.Y;
                    n1.Data.Width = area.Data.Width - w;
                    n1.Data.Height = area.Data.Height;
                    n1.IsFree = true;

                    var n2 = new TexArea();
                    n2.Data.Slice = Slice;
                    n2.Data.X = area.Data.X;
                    n2.Data.Y = area.Data.Y + h;
                    n2.Data.Width = w;
                    n2.Data.Height = area.Data.Height - h;
                    n2.IsFree = true;

                    if(n1.Data.Width>0)
                        FreeAreas.Add(n1);
                    if (n2.Data.Height>0)
                        FreeAreas.Add(n2);
                }
                else
                {
                    var n1 = new TexArea();
                    n1.Data.Slice = Slice;
                    n1.Data.X = area.Data.X + w;
                    n1.Data.Y = area.Data.Y;
                    n1.Data.Width = area.Data.Width - w;
                    n1.Data.Height = h;
                    n1.IsFree = true;

                    var n2 = new TexArea();
                    n2.Data.Slice = Slice;
                    n2.Data.X = area.Data.X;
                    n2.Data.Y = area.Data.Y + h;
                    n2.Data.Width = area.Data.Width;
                    n2.Data.Height = area.Data.Height - h;
                    n2.IsFree = true;

                    if(n1.Data.Width>0)
                        FreeAreas.Add(n1);
                    if (n2.Data.Height > 0)
                        FreeAreas.Add(n2);
                }
                return result;
            }
            public void FreeArea(TexArea area)
            {
                if (ValidAreas.Contains(area))
                {
                    FreeAreas.Add(area);

                    //sort free areas
                    FreeAreas.Sort((left, right) =>
                    {
                        return left.GetArea() - right.GetArea();
                    });
                }
                else
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "TextureAtlas", $"TexArea {area} free failed");
                }
            }
            public TexArea GetMaxArea()
            {
                return null;
            }
        }

        public CGfxTextureAtlas(int w,int h)
        {
            SliceWidth = w;
            SliceHeight = h;
        }
        public int SliceWidth
        {
            get;
            private set;
        }
        public int SliceHeight
        {
            get;
            private set;
        }
        public List<TexSlice> Slices = new List<TexSlice>();
        public TexArea PushArea(int width, int height)
        {
            for (int i = 0; i < Slices.Count; i++)
            {
                var result = Slices[i].PushArea(width, height);
                if(result!=null)
                {
                    return result;
                }
            }
            var ns = new TexSlice(SliceWidth, SliceHeight, Slices.Count);
            Slices.Add(ns);
            return ns.PushArea(width, height);
        }
        public void FreeArea(TexArea area)
        {
            if (Slices.Count <= area.Data.Slice || area.Data.Slice < 0)
                return;
            Slices[area.Data.Slice].FreeArea(area);
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task TestAtlas(UISystem.Controls.Containers.Border border)
        {
            var rc = CEngine.Instance.RenderContext;
            var atlas = new EngineNS.Graphics.Tool.CGfxTextureAtlas(2048, 2048);

            List<System.Drawing.Size> areas = new List<System.Drawing.Size>();

            for (int i = 0; i < 1; i++)
            {
                areas.Add(new System.Drawing.Size(1024, 1024));
            }
            for (int i = 0; i < 3; i++)
            {
                areas.Add(new System.Drawing.Size(512, 512));
            }
            for (int i = 0; i < 5; i++)
            {
                areas.Add(new System.Drawing.Size(256, 256));
            }
            for (int i = 0; i < 10; i++)
            {
                areas.Add(new System.Drawing.Size(128, 128));
            }

            for (int i = 0; i < areas.Count; i++)
            {
                var u = atlas.PushArea(areas[i].Width, areas[i].Height);
                u.Name = RName.GetRName(i.ToString());
            }

            var tRd = new Random();
            foreach (var i in atlas.Slices)
            {
                foreach (var j in i.ValidAreas)
                {
                    System.Diagnostics.Debug.WriteLine(j.ToString());
                    UISystem.Controls.Image img = new UISystem.Controls.Image();
                    UISystem.Controls.ImageInitializer imgInit = new UISystem.Controls.ImageInitializer();

                    EngineNS.UISystem.Brush imgBrush = new EngineNS.UISystem.Brush();
                    imgBrush.ImageSize = new EngineNS.Vector2(j.Data.Width/2, j.Data.Width/2);
                    imgBrush.Color = new Color4(1, (float)tRd.NextDouble(), (float)tRd.NextDouble(), (float)tRd.NextDouble());
                    imgInit.ImageBrush = imgBrush;
                    imgInit.Id = System.Guid.NewGuid().GetHashCode();
                    imgInit.Name = j.Name.ToString();
                    //var bdSlot = new UISystem.Controls.Containers.BorderSlot();
                    //bdSlot.Margin = new Thickness(j.Data.X / 2 + border.DesignRect.X, 
                    //    j.Data.Y / 2 + border.DesignRect.X, 
                    //    j.Data.X / 2 + border.DesignRect.X + j.Data.Width / 2, 
                    //    j.Data.Y / 2 + border.DesignRect.X + j.Data.Height / 2);
                    var bdSlot = new EngineNS.UISystem.Controls.Containers.CanvasSlot();
                    imgInit.Slot = bdSlot;
                    bdSlot.X1 = j.Data.X / 2 + border.DesignRect.X;
                    bdSlot.Y1 = j.Data.Y / 2 + border.DesignRect.Y;
                    bdSlot.X2 = j.Data.Width / 2;
                    bdSlot.Y2 = j.Data.Height / 2;
                    imgInit.DesignRect = new RectangleF(j.Data.X/2 + border.DesignRect.X, j.Data.Y/ 2 + border.DesignRect.Y, j.Data.Width/2, j.Data.Height/2);
                    imgInit.DesignClipRect = imgInit.DesignRect;

                    await img.Initialize(rc, imgInit);
                    border.Parent.AddChild(img);
                }
            }
        }
    }
}
