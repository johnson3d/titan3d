using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Controls.Animation
{
    public class AnimationBlendSpaceDetial : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        protected AnimationBlendSpaceControl mBlendSpaceCtrl = null;
        public AnimationBlendSpaceDetial()
        {

        }
        public AnimationBlendSpaceDetial(AnimationBlendSpaceControl ctrl)
        {
            mBlendSpaceCtrl = ctrl;
            if (mBlendSpaceCtrl.BlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
            {
                mAxisesDetial = new AnimationBlendAxis1DDetial(mBlendSpaceCtrl);
            }
            if (mBlendSpaceCtrl.BlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_2D)
            {
                mAxisesDetial = new AnimationBlendAxis2DDetial(mBlendSpaceCtrl);
            }
        }
        protected void InitializeAxises()
        {
            
        }
        protected AnimationBlendAxis1DDetial mAxisesDetial = null;
        [DisplayName("AxisSetting")]
        public AnimationBlendAxis1DDetial AxisesDetial
        {
            get => mAxisesDetial;
            set
            {
                mAxisesDetial = value;
                OnPropertyChanged("AxisesDetial");
            }
        }
    }
    public class AnimationAdditiveBlendSpaceDetial : AnimationBlendSpaceDetial
    {
        EngineNS.RName mReferenceAnimationName = EngineNS.RName.EmptyName;
        [DisplayName("ReferenceAnimation")]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public EngineNS.RName ReferenceAnimationName
        {
            get => mReferenceAnimationName;
            set
            {
                mReferenceAnimationName = value;
                AdditiveBlendSpace.ReferenceAnimation = value;
                OnPropertyChanged("ReferenceAnimationName");
            }
        }
        EngineNS.RName mPreviewAnimationName = EngineNS.RName.EmptyName;
        [DisplayName("PreviewAnimation")]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public EngineNS.RName PreviewAnimationName
        {
            get => mPreviewAnimationName;
            set
            {
                mPreviewAnimationName = value;
                
                OnPropertyChanged("PreviewAnimationName");
            }
        }
        public AdditiveBlendSpace AdditiveBlendSpace
        {
            get
            {
                return mBlendSpaceCtrl.AnimationBlendSpace as AdditiveBlendSpace;
            }
        }
        public AnimationAdditiveBlendSpaceDetial()
        {

        }
        public AnimationAdditiveBlendSpaceDetial(AnimationBlendSpaceControl ctrl):base(ctrl)
        {
            
        }
    }

    public class AnimationBlendAxis1DDetial : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        protected AnimationBlendSpaceControl mBlendSpaceCtrl = null;
        public AnimationBlendAxis1DDetial(AnimationBlendSpaceControl ctrl)
        {
            mBlendSpaceCtrl = ctrl;
            XDimensionName = mBlendSpaceCtrl.XDimensionName;
            XMax = mBlendSpaceCtrl.XMax;
            XMin = mBlendSpaceCtrl.XMin;
            XDimensionPieces = (int)mBlendSpaceCtrl.DimensionPieces.X;
        }
        string mXDimensionName = "None";
        [Category("Horizontal Axis"), DisplayName("Name")]
        public string XDimensionName
        {
            get => mXDimensionName;
            set
            {
                mXDimensionName = value;
                mBlendSpaceCtrl.XDimensionName = value;
                OnPropertyChanged("XDimensionName");
            }
        }
        float mXMin = 0;
        [Category("Horizontal Axis"), DisplayName("Minimum Value")]
        public float XMin
        {
            get => mXMin;
            set
            {
                if (value <= mXMax)
                {
                    mXMin = value;
                    mBlendSpaceCtrl.XMin = value;
                    OnPropertyChanged("XMin");
                }
            }
        }

        float mXMax = 100;
        [Category("Horizontal Axis"), DisplayName("Maximum Value")]
        public float XMax
        {
            get => mXMax;
            set
            {
                mXMax = value;
                mBlendSpaceCtrl.XMax = value;
                OnPropertyChanged("XMax");
            }
        }
        //五条线，四个格
        int mXDimensionPieces = 4;
        [Category("Horizontal Axis"), DisplayName("Num of Grid")]
        public int XDimensionPieces
        {
            get => mXDimensionPieces;
            set
            {
                if (value <= 0)
                    return;
                mXDimensionPieces = value;
                var pieces = mBlendSpaceCtrl.DimensionPieces;
                pieces.X = value;
                mBlendSpaceCtrl.DimensionPieces = pieces;
                OnPropertyChanged("XDimensionPieces");
            }
        }

    }
    public class AnimationBlendAxis2DDetial : AnimationBlendAxis1DDetial
    {
        public AnimationBlendAxis2DDetial(AnimationBlendSpaceControl ctrl) : base(ctrl)
        {
            mBlendSpaceCtrl = ctrl;
            XDimensionName = mBlendSpaceCtrl.XDimensionName;
            XMax = mBlendSpaceCtrl.XMax;
            XMin = mBlendSpaceCtrl.XMin;
            XDimensionPieces = (int)mBlendSpaceCtrl.DimensionPieces.X;

            YDimensionName = mBlendSpaceCtrl.YDimensionName;
            YMax = mBlendSpaceCtrl.YMax;
            YMin = mBlendSpaceCtrl.YMin;
            YDimensionPieces = (int)mBlendSpaceCtrl.DimensionPieces.Y;
        }
        string mYDimensionName = "None";
        [Category("Vertical Axis"), DisplayName("Name")]
        public string YDimensionName
        {
            get => mYDimensionName;
            set
            {
                mYDimensionName = value;
                mBlendSpaceCtrl.YDimensionName = value;
                OnPropertyChanged("YDimensionName");
            }
        }
        float mYMin = 0;
        [Category("Vertical Axis"), DisplayName("Minimum Value")]
        public float YMin
        {
            get => mYMin;
            set
            {
                if (value <= mYMax)
                {
                    mYMin = value;
                    mBlendSpaceCtrl.YMin = value;
                    OnPropertyChanged("YMin");
                }
            }
        }

        float mYMax = 100;
        [Category("Vertical Axis"), DisplayName("Maximum Value")]
        public float YMax
        {
            get => mYMax;
            set
            {
                mYMax = value;
                mBlendSpaceCtrl.YMax = value;
                OnPropertyChanged("YMax");
            }
        }
        int mYDimensionPieces = 4;
        [Category("Vertical Axis"), DisplayName("Num of Grid")]
        public int YDimensionPieces
        {
            get => mYDimensionPieces;
            set
            {
                if (value <= 0)
                    return;
                mYDimensionPieces = value;
                var pieces = mBlendSpaceCtrl.DimensionPieces;
                pieces.Y = value;
                mBlendSpaceCtrl.DimensionPieces = pieces;
                OnPropertyChanged("YDimensionPieces");
            }
        }
    }
}

