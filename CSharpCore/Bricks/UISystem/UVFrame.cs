using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS;
using EngineNS.IO.Serializer;

namespace EngineNS.UISystem
{
    public class Scale9DrawInfo
    {
        public enum enScale9Type
        {
            None            = -1,
            LeftTop         = 0,
            Top             = 1,
            RightTop        = 2,
            Right           = 3,
            RightBottom     = 4,
            Bottom          = 5,
            LeftBottom      = 6,
            Left            = 7,
            Center          = 8,
            TotalSize       = 9,
        }

        private enScale9Type mScale9Type;
        public enScale9Type Scale9Type
        {
            get { return mScale9Type; }
        }
        public RectangleF mDrawUVRect = new RectangleF();
        //public SlimDX.Size mDrawSize = new SlimDX.Size();
        public Rectangle mDrawRect = new Rectangle();

        public Scale9DrawInfo(enScale9Type type)
        {
            mScale9Type = type;
        }

        public Rectangle GetDrawRect(UVFrame hostFrame, Rectangle orcRect)
        {
            switch (mScale9Type)
            {
                case enScale9Type.LeftTop:
                    {
                        mDrawRect.X = orcRect.X;
                        mDrawRect.Y = orcRect.Y;
                        mDrawRect.Width = hostFrame.LeftPixel;
                        mDrawRect.Height = hostFrame.TopPixel;
                    }
                    break;

                case enScale9Type.Top:
                    {
                        mDrawRect.X = orcRect.X + hostFrame.LeftPixel;
                        mDrawRect.Y = orcRect.Y;
                        mDrawRect.Width = orcRect.Width - hostFrame.LeftPixel - hostFrame.RightPixel;
                        mDrawRect.Height = hostFrame.TopPixel;
                    }
                    break;

                case enScale9Type.RightTop:
                    {
                        mDrawRect.X = orcRect.X + orcRect.Width - hostFrame.RightPixel;
                        mDrawRect.Y = orcRect.Y;
                        mDrawRect.Width = hostFrame.RightPixel;
                        mDrawRect.Height = hostFrame.TopPixel;
                    }
                    break;

                case enScale9Type.Right:
                    {
                        mDrawRect.X = orcRect.X + orcRect.Width - hostFrame.RightPixel;
                        mDrawRect.Y = orcRect.Y + hostFrame.TopPixel;
                        mDrawRect.Width = hostFrame.RightPixel;
                        mDrawRect.Height = orcRect.Height - hostFrame.TopPixel - hostFrame.BottomPixel;
                    }
                    break;

                case enScale9Type.RightBottom:
                    {
                        mDrawRect.X = orcRect.X + orcRect.Width - hostFrame.RightPixel;
                        mDrawRect.Y = orcRect.Y + orcRect.Height - hostFrame.BottomPixel;
                        mDrawRect.Width = hostFrame.RightPixel;
                        mDrawRect.Height = hostFrame.BottomPixel;
                    }
                    break;

                case enScale9Type.Bottom:
                    {
                        mDrawRect.X = orcRect.X + hostFrame.LeftPixel;
                        mDrawRect.Y = orcRect.Y + orcRect.Height - hostFrame.BottomPixel;
                        mDrawRect.Width = orcRect.Width - hostFrame.LeftPixel - hostFrame.RightPixel;
                        mDrawRect.Height = hostFrame.BottomPixel;
                    }
                    break;

                case enScale9Type.LeftBottom:
                    {
                        mDrawRect.X = orcRect.X;
                        mDrawRect.Y = orcRect.Y + orcRect.Height - hostFrame.BottomPixel;
                        mDrawRect.Width = hostFrame.LeftPixel;
                        mDrawRect.Height = hostFrame.BottomPixel;
                    }
                    break;

                case enScale9Type.Left:
                    {
                        mDrawRect.X = orcRect.X;
                        mDrawRect.Y = orcRect.Y + hostFrame.TopPixel;
                        mDrawRect.Width = hostFrame.LeftPixel;
                        mDrawRect.Height = orcRect.Height - hostFrame.TopPixel - hostFrame.BottomPixel;
                    }
                    break;

                case enScale9Type.Center:
                    {
                        mDrawRect.X = orcRect.X + hostFrame.LeftPixel;
                        mDrawRect.Y = orcRect.Y + hostFrame.TopPixel;
                        mDrawRect.Width = orcRect.Width - hostFrame.LeftPixel - hostFrame.RightPixel;
                        mDrawRect.Height = orcRect.Height - hostFrame.TopPixel - hostFrame.BottomPixel;
                    }
                    break;
            }

            return mDrawRect;
        }
    }

    [Rtti.MetaClass]
    public class UVFrame : IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        protected UVAnim mParentAnim = null;
        [Browsable(false)]
        public UVAnim ParentAnim
        {
            get => mParentAnim;
            set
            {
                mParentAnim = value;
                if (mParentAnim != null)
                {
                    var noUse = UpdateScale9Infos();
                }
            }
        }

        RectangleF mUVRect = new RectangleF(0.0f, 0.0f, 1.0f, 1.0f);
        [Rtti.MetaData]
        public float U
        {
            get => mUVRect.X;
            set
            {
                mUVRect.X = value;
                OnPropertyChanged("U");
            }
        }
        [Rtti.MetaData]
        public float V
        {
            get => mUVRect.Y;
            set
            {
                mUVRect.Y = value;
                OnPropertyChanged("V");
            }
        }
        [Rtti.MetaData]
        public float SizeU
        {
            get => mUVRect.Width;
            set
            {
                mUVRect.Width = value;
                OnPropertyChanged("SizeU");
            }
        }
        [Rtti.MetaData]
        public float SizeV
        {
            get => mUVRect.Height;
            set
            {
                mUVRect.Height = value;
                OnPropertyChanged("SizeV");
            }
        }

        #region 九宫格

        float mUTile = 1;
        //[Browsable(false)]
        public float UTile
        {
            get => mUTile;
            set
            {
                mUTile = value;
                OnPropertyChanged("UTile");
            }
        }
        float mVTile = 1;
        //[Browsable(false)]
        public float VTile
        {
            get => mVTile;
            set
            {
                mVTile = value;
                OnPropertyChanged("VTile");
            }
        }

        // 九宫格数据
        int mLeftPixel = 0;
        [Browsable(false)]
        public int LeftPixel
        {
            get { return mLeftPixel; }
        }
        // 九宫格数据
        int mRightPixel = 0;
        [Browsable(false)]
        public int RightPixel
        {
            get { return mRightPixel; }
        }
        // 九宫格数据
        int mTopPixel = 0;
        [Browsable(false)]
        public int TopPixel
        {
            get { return mTopPixel; }
        }
        // 九宫格数据
        int mBottomPixel = 0;
        [Browsable(false)]
        public int BottomPixel
        {
            get { return mBottomPixel; }
        }

        Thickness mScale9Info = Thickness.Empty;
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("Scale9Setter")]
        [Rtti.MetaData]
        public Thickness Scale9Info
        {
            get => mScale9Info;
            set
            {
                mScale9Info = value;
                var noUse = UpdateScale9Infos();
                OnPropertyChanged("Scale9Info");
            }
        }
        [Browsable(false)]
        public bool HasScale9Info
        {
            get
            {
                if (mScale9Info == Thickness.Empty)
                    return false;
                return true;
            }
        }

        Scale9DrawInfo[] mScale9DrawRectangles = new Scale9DrawInfo[(int)Scale9DrawInfo.enScale9Type.TotalSize];
        [Browsable(false)]
        public Scale9DrawInfo[] Scale9DrawRectangles
        {
            get => mScale9DrawRectangles;
        }

        public async Task UpdateScale9Infos()
        {
            if (mParentAnim != null && mParentAnim.TextureObject != null)
            {
                var rc = CEngine.Instance.RenderContext;
                var textureObject = await CEngine.Instance.TextureManager.AwaitTextureValid(rc, mParentAnim.TextureRName);
                var desc = mParentAnim.TextureObject.TxPicDesc;

                mScale9DrawRectangles = new Scale9DrawInfo[(int)Scale9DrawInfo.enScale9Type.TotalSize];
                float leftOffset = (float)mScale9Info.Left * SizeU;
                float topOffset = (float)mScale9Info.Top * SizeV;
                float rightOffset = (float)mScale9Info.Right * SizeU;
                float bottomOffset = (float)mScale9Info.Bottom * SizeV;
                mLeftPixel = (int)(desc.Width * SizeU * mScale9Info.Left);
                mTopPixel = (int)(desc.Height * SizeV * mScale9Info.Top);
                mRightPixel = (int)(desc.Width * SizeU * mScale9Info.Right);
                mBottomPixel = (int)(desc.Height * SizeV * mScale9Info.Bottom);
                if (mScale9Info.Left > 0)
                {
                    if (mScale9Info.Top > 0)
                    {
                        Scale9DrawInfo info = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.LeftTop);
                        info.mDrawUVRect = new RectangleF(U,
                                                          V,
                                                          leftOffset,
                                                          topOffset);
                        mScale9DrawRectangles[(int)info.Scale9Type] = info;
                    }
                    if (mScale9Info.Bottom > 0)
                    {
                        Scale9DrawInfo info = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.LeftBottom);
                        info.mDrawUVRect = new RectangleF(U,
                                                          V + SizeV - bottomOffset,
                                                          leftOffset,
                                                          bottomOffset);
                        mScale9DrawRectangles[(int)info.Scale9Type] = info;
                    }
                    Scale9DrawInfo oInfo = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.Left);
                    oInfo.mDrawUVRect = new RectangleF(U,
                                                       V + topOffset,
                                                       leftOffset,
                                                       SizeV - topOffset - bottomOffset);
                    mScale9DrawRectangles[(int)oInfo.Scale9Type] = oInfo;
                }
                if (mScale9Info.Right > 0)
                {
                    if (mScale9Info.Top > 0)
                    {
                        Scale9DrawInfo info = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.RightTop);
                        info.mDrawUVRect = new RectangleF(U + SizeU - rightOffset,
                                                          V,
                                                          rightOffset,
                                                          topOffset);
                        mScale9DrawRectangles[(int)info.Scale9Type] = info;
                    }
                    if (mScale9Info.Bottom > 0)
                    {
                        Scale9DrawInfo info = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.RightBottom);
                        info.mDrawUVRect = new RectangleF(U + SizeU - rightOffset,
                                                          V + SizeV - bottomOffset,
                                                          rightOffset,
                                                          bottomOffset);
                        mScale9DrawRectangles[(int)info.Scale9Type] = info;
                    }
                    Scale9DrawInfo oInfo = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.Right);
                    oInfo.mDrawUVRect = new RectangleF(U + SizeU - rightOffset,
                                                       V + topOffset,
                                                       rightOffset,
                                                       SizeV - topOffset - bottomOffset);
                    mScale9DrawRectangles[(int)oInfo.Scale9Type] = oInfo;
                }
                if (mScale9Info.Top > 0)
                {
                    Scale9DrawInfo info = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.Top);
                    info.mDrawUVRect = new RectangleF(U + leftOffset,
                                                      V,
                                                      SizeU - leftOffset - rightOffset,
                                                      topOffset);
                    mScale9DrawRectangles[(int)info.Scale9Type] = info;
                }
                if (mScale9Info.Bottom > 0)
                {
                    Scale9DrawInfo info = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.Bottom);
                    info.mDrawUVRect = new RectangleF(U + leftOffset,
                                                      V + SizeV - bottomOffset,
                                                      SizeU - leftOffset - rightOffset,
                                                      bottomOffset);
                    mScale9DrawRectangles[(int)info.Scale9Type] = info;
                }

                Scale9DrawInfo cInfo = new Scale9DrawInfo(Scale9DrawInfo.enScale9Type.Center);
                cInfo.mDrawUVRect.X = U + leftOffset;
                cInfo.mDrawUVRect.Y = V + topOffset;
                cInfo.mDrawUVRect.Width = SizeU - leftOffset - rightOffset;
                cInfo.mDrawUVRect.Height = SizeV - topOffset - bottomOffset;
                mScale9DrawRectangles[(int)cInfo.Scale9Type] = cInfo;
            }
        }

        public void UpdateVertexes(EngineNS.Support.NativeListProxy<EngineNS.Vector3> posData, ref EngineNS.RectangleF designRect, ref EngineNS.RectangleF clipRect)
        {
            float leftDelta, rightDelta, topDelta, bottomDelta;
            
            if (clipRect.Width > 0)
            {
                var designWidth = System.Math.Abs(designRect.Width);
                leftDelta = (float)(LeftPixel) / clipRect.Width;
                rightDelta = (float)(RightPixel) / clipRect.Width;
                var deltaCL = clipRect.Left - designRect.Left;
                var deltaCR = designRect.Right - clipRect.Right;
                if ((leftDelta - (deltaCL / clipRect.Width)) < 0)
                    leftDelta = 0;
                else if (((designWidth - deltaCR) / clipRect.Width) < leftDelta)
                    leftDelta = (designWidth - deltaCR) / clipRect.Width;
                else
                    leftDelta = leftDelta - deltaCL / clipRect.Width;

                if ((rightDelta - (deltaCR / clipRect.Width)) < 0)
                    rightDelta = 0;
                else if (((designWidth - deltaCL) / clipRect.Width) < rightDelta)
                    rightDelta = (designWidth - deltaCL) / clipRect.Width;
                else
                    rightDelta = rightDelta - deltaCR / clipRect.Width;
            }
            else
            {
                leftDelta = 0;
                rightDelta = 0;
            }
            if(clipRect.Height > 0)
            {
                var designHeight = System.Math.Abs(designRect.Height);
                topDelta = (float)(TopPixel) / clipRect.Height;
                bottomDelta = (float)(BottomPixel) / clipRect.Height;
                var deltaCT = clipRect.Top - designRect.Top;
                var deltaCB = designRect.Bottom - clipRect.Bottom;
                if ((topDelta - (deltaCT / clipRect.Height)) < 0)
                    topDelta = 0;
                else if (((designHeight - deltaCB) / clipRect.Height) < topDelta)
                    topDelta = (designHeight - deltaCB) / clipRect.Height;
                else
                    topDelta = topDelta - deltaCT / clipRect.Height;

                if ((bottomDelta - (deltaCB / clipRect.Height)) < 0)
                    bottomDelta = 0;
                else if (((designHeight - deltaCT) / clipRect.Height) < bottomDelta)
                    bottomDelta = (designHeight - deltaCT) / clipRect.Height;
                else
                    bottomDelta = bottomDelta - deltaCB / clipRect.Height;
            }
            else
            {
                topDelta = 0;
                bottomDelta = 0;
            }

            unsafe
            {
                var posXs = stackalloc float[4];
                posXs[0] = 0;
                posXs[1] = leftDelta;
                posXs[2] = 1 - rightDelta;
                posXs[3] = 1;
                var posYs = stackalloc float[4];
                posYs[0] = 0;
                posYs[1] = topDelta;
                posYs[2] = 1 - bottomDelta;
                posYs[3] = 1;
                var pos = new EngineNS.Vector3(0, 0, 0);
                using (var tempDatss = EngineNS.Support.NativeListProxy<EngineNS.Vector3>.CreateNativeList())
                {
                    for (int vY = 0; vY < 4; vY++)
                    {
                        pos.Y = posYs[vY];
                        for (int vX = 0; vX < 4; vX++)
                        {
                            pos.X = posXs[vX];
                            tempDatss.Add(pos);
                        }
                    }
                    // 0,1,2,3
                    posData.Add(tempDatss[0]);
                    posData.Add(tempDatss[1]);
                    posData.Add(tempDatss[5]);
                    posData.Add(tempDatss[4]);
                    // 4,5,6,7
                    posData.Add(tempDatss[1]);
                    posData.Add(tempDatss[2]);
                    posData.Add(tempDatss[6]);
                    posData.Add(tempDatss[5]);
                    // 8,9,10,11
                    posData.Add(tempDatss[2]);
                    posData.Add(tempDatss[3]);
                    posData.Add(tempDatss[7]);
                    posData.Add(tempDatss[6]);
                    // 12,13,14,15
                    posData.Add(tempDatss[4]);
                    posData.Add(tempDatss[5]);
                    posData.Add(tempDatss[9]);
                    posData.Add(tempDatss[8]);
                    // 16,17,18,19
                    posData.Add(tempDatss[5]);
                    posData.Add(tempDatss[6]);
                    posData.Add(tempDatss[10]);
                    posData.Add(tempDatss[9]);
                    // 20,21,22,23
                    posData.Add(tempDatss[6]);
                    posData.Add(tempDatss[7]);
                    posData.Add(tempDatss[11]);
                    posData.Add(tempDatss[10]);
                    // 24,25,26,27
                    posData.Add(tempDatss[8]);
                    posData.Add(tempDatss[9]);
                    posData.Add(tempDatss[13]);
                    posData.Add(tempDatss[12]);
                    // 28,29,30,31
                    posData.Add(tempDatss[9]);
                    posData.Add(tempDatss[10]);
                    posData.Add(tempDatss[14]);
                    posData.Add(tempDatss[13]);
                    // 32,33,34,35
                    posData.Add(tempDatss[10]);
                    posData.Add(tempDatss[11]);
                    posData.Add(tempDatss[15]);
                    posData.Add(tempDatss[14]);
                }
            }
        }
        public void UpdateUVs(EngineNS.Support.NativeListProxy<EngineNS.Vector2> uvData, ref EngineNS.RectangleF designRect, ref EngineNS.RectangleF clipRect)
        {
            bool widthPositive = true, heightPositive = true;
            var designWidth = designRect.Width;
            if(designWidth < 0)
            {
                widthPositive = false;
                designWidth = -designRect.Width;
            }
            var designHeight = designRect.Height;
            if(designHeight < 0)
            {
                heightPositive = false;
                designHeight = -designRect.Height;
            }
            var leftDelta = ((clipRect.Left - designRect.Left) / designWidth) * SizeU;
            var rightDelta = ((designRect.Right - clipRect.Right) / designWidth) * SizeU;
            var topDelta = ((clipRect.Top - designRect.Top) / designHeight) * SizeV;
            var bottomDelta = ((designRect.Bottom - clipRect.Bottom) / designHeight) * SizeV;

            unsafe
            {
                var uvXs = stackalloc float[4];
                uvXs[0] = U + leftDelta;
                var scale9LeftDelta = Scale9Info.Left * SizeU;
                var scale9RightDelta = Scale9Info.Right * SizeU;
                if ((SizeU - rightDelta) < scale9LeftDelta)
                    uvXs[1] = (SizeU - rightDelta) + U;
                if (scale9LeftDelta > leftDelta)
                    uvXs[1] = scale9LeftDelta + U;
                else
                    uvXs[1] = leftDelta + U;

                if ((SizeU - leftDelta) < scale9RightDelta)
                    uvXs[2] = leftDelta + U;
                if (scale9RightDelta > rightDelta)
                    uvXs[2] = SizeU - scale9RightDelta + U;
                else
                    uvXs[2] = SizeU - rightDelta + U;
                uvXs[3] = U + SizeU - rightDelta;

                var uvYs = stackalloc float[4];
                uvYs[0] = V + topDelta;

                var scale9Topdelta = Scale9Info.Top * SizeV;
                var scale9BottomDelta = Scale9Info.Bottom * SizeV;
                if ((SizeV - bottomDelta) < scale9Topdelta)
                    uvYs[1] = (SizeV - bottomDelta) + V;
                else if (scale9Topdelta > topDelta)
                    uvYs[1] = scale9Topdelta + V;
                else
                    uvYs[1] = topDelta + V;

                if ((SizeV - topDelta) < scale9BottomDelta)
                    uvYs[2] = topDelta + V;
                else if (scale9BottomDelta > bottomDelta)
                    uvYs[2] = SizeV - scale9BottomDelta + V;
                else
                    uvYs[2] = SizeV - bottomDelta + V;

                uvYs[3] = V + SizeV - bottomDelta;

                var uv = new EngineNS.Vector2(0, 0);
                using (var tempDatas = EngineNS.Support.NativeListProxy<EngineNS.Vector2>.CreateNativeList())
                {
                    if (heightPositive)
                    {
                        for (int vY = 0; vY < 4; vY++)
                        {
                            uv.Y = uvYs[vY];

                            if (widthPositive)
                            {
                                for (int vX = 0; vX < 4; vX++)
                                {
                                    uv.X = uvXs[vX];
                                    tempDatas.Add(uv);
                                }
                            }
                            else
                            {
                                for (int vX = 3; vX >= 0; vX--)
                                {
                                    uv.X = uvXs[vX];
                                    tempDatas.Add(uv);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int vY = 3; vY >= 0; vY--)
                        {
                            uv.Y = uvYs[vY];

                            if (widthPositive)
                            {
                                for (int vX = 0; vX < 4; vX++)
                                {
                                    uv.X = uvXs[vX];
                                    tempDatas.Add(uv);
                                }
                            }
                            else
                            {
                                for (int vX = 3; vX >= 0; vX--)
                                {
                                    uv.X = uvXs[vX];
                                    tempDatas.Add(uv);
                                }
                            }
                        }
                    }

                    // 0,1,2,3
                    uvData.Add(tempDatas[0]);
                    uvData.Add(tempDatas[1]);
                    uvData.Add(tempDatas[5]);
                    uvData.Add(tempDatas[4]);
                    // 4,5,6,7
                    uvData.Add(tempDatas[1]);
                    uvData.Add(new EngineNS.Vector2(tempDatas[2].X * UTile, tempDatas[2].Y));
                    uvData.Add(new EngineNS.Vector2(tempDatas[6].X * UTile, tempDatas[6].Y));
                    uvData.Add(tempDatas[5]);
                    // 8,9,10,11
                    uvData.Add(tempDatas[2]);
                    uvData.Add(tempDatas[3]);
                    uvData.Add(tempDatas[7]);
                    uvData.Add(tempDatas[6]);
                    // 12,13,14,15
                    uvData.Add(tempDatas[4]);
                    uvData.Add(tempDatas[5]);
                    uvData.Add(new EngineNS.Vector2(tempDatas[9].X, tempDatas[9].Y * VTile));
                    uvData.Add(new EngineNS.Vector2(tempDatas[8].X, tempDatas[8].Y * VTile));
                    // 16,17,18,19
                    if (HasScale9Info)
                    {
                        uvData.Add(tempDatas[5]);
                        uvData.Add(tempDatas[6]);
                        uvData.Add(tempDatas[10]);
                        uvData.Add(tempDatas[9]);
                    }
                    else
                    {
                        uvData.Add(tempDatas[5]);
                        uvData.Add(new EngineNS.Vector2(tempDatas[6].X * UTile, tempDatas[6].Y));
                        uvData.Add(new EngineNS.Vector2(tempDatas[10].X * UTile, tempDatas[10].Y * VTile));
                        uvData.Add(new EngineNS.Vector2(tempDatas[9].X, tempDatas[9].Y * VTile));
                    }
                    // 20,21,22,23
                    uvData.Add(tempDatas[6]);
                    uvData.Add(tempDatas[7]);
                    uvData.Add(new EngineNS.Vector2(tempDatas[11].X, tempDatas[11].Y * VTile));
                    uvData.Add(new EngineNS.Vector2(tempDatas[10].X, tempDatas[10].Y * VTile));
                    // 24,25,26,27
                    uvData.Add(tempDatas[8]);
                    uvData.Add(tempDatas[9]);
                    uvData.Add(tempDatas[13]);
                    uvData.Add(tempDatas[12]);
                    // 28,29,30,31
                    uvData.Add(tempDatas[9]);
                    uvData.Add(new EngineNS.Vector2(tempDatas[10].X * UTile, tempDatas[10].Y));
                    uvData.Add(new EngineNS.Vector2(tempDatas[14].X * UTile, tempDatas[14].Y));
                    uvData.Add(tempDatas[13]);
                    // 32,33,34,35
                    uvData.Add(tempDatas[10]);
                    uvData.Add(tempDatas[11]);
                    uvData.Add(tempDatas[15]);
                    uvData.Add(tempDatas[14]);
                }
            }
        }

        #endregion

        public override ISerializer CloneObject()
        {
            var obj = base.CloneObject() as UVFrame;
            obj.mTopPixel = mTopPixel;
            obj.mLeftPixel = mLeftPixel;
            obj.mRightPixel = mRightPixel;
            obj.mBottomPixel = mBottomPixel;
            obj.mScale9DrawRectangles = Scale9DrawRectangles;
            return obj;
        }
    }
}
