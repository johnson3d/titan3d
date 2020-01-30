using EngineNS;
using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.Animation.Skeleton;
using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Controls.Skeleton
{
    public class EditorBoneDetial : IPlaceable
    {
        CGfxBone mBone = null;
        CGfxBone mParentBone = null;
        public EditorBoneDetial(CGfxSkeleton skeleton, CGfxBone bone, CGfxBone parentBone)
        {
            mBone = bone;
            mParentBone = parentBone;
            if (bone.ChildNumber > 0)
            {
                for (uint i = 0; i < bone.ChildNumber; ++i)
                {
                    var childIndex = bone.GetChild(i);
                    var child = skeleton.GetBone(childIndex);
                    mChildren.Add(new EditorBoneDetial(skeleton, child, bone));
                }
            }
        }
        [Category("基本"), DisplayName("名称")]
        public string Name
        {
            get { return mBone.BoneDesc.Name; }
            set { mBone.BoneDesc.Name = value; }
        }
        EngineNS.GamePlay.Component.GPlacementComponent mPlacementComponent = new EngineNS.GamePlay.Component.GPlacementComponent();
        public void OnPlacementChanged(GPlacementComponent placement)
        {
            throw new NotImplementedException();
        }
        public EngineNS.GamePlay.Component.GPlacementComponent Placement
        {
            get
            {
                var martix = LocalMatrix;
                mPlacementComponent.SetMatrix(ref martix);
                return mPlacementComponent;
            }
            set
            {
                mPlacementComponent = value;
                LocalMatrix = value.Transform;
            }
        }
        public Matrix LocalMatrix
        {
            get
            {
                if (mParentBone == null)
                    return BindMatrix;
                return BindMatrix * mParentBone.BoneDesc.InvBindMatrix;
            }
            set
            {
                if (mParentBone == null)
                    BindMatrix = value;
                else
                    BindMatrix = value * mParentBone.BoneDesc.BindMatrix;
            }
        }
        [Browsable(false)]
        Matrix BindMatrix
        {
            get { return mBone.BoneDesc.BindMatrix; }
            set
            {
                mBone.BoneDesc.BindMatrix = value;
                var inv = value;
                inv.Inverse();
                mBone.BoneDesc.InvBindMatrix = inv;
            }
        }
        public BoneType Type
        {
            get { return mBone.BoneDesc.Type; }
        }
        [Browsable(false)]
        public int Index
        {
            get
            { return mBone.IndexInTable; }
        }
        ObservableCollection<EditorBoneDetial> mChildren = new ObservableCollection<EditorBoneDetial>();
        [Browsable(false)]
        public ObservableCollection<EditorBoneDetial> Children
        {
            get
            {
                return mChildren;
            }

        }

       
    }
}
