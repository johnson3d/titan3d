using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CodeGenerateSystem.Base
{
    public class AnimStateTransitionCurve : EditorCommon.Controls.Curves.Line
    {

        /// <summary>
        /// 文本朝上的依赖属性
        /// </summary>
        public static readonly DependencyProperty IsUpProperty = DependencyProperty.Register(
            "IsUp",
            typeof(bool),
            typeof(AnimStateTransitionCurve),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// 是否朝上
        /// </summary>
        public bool IsUp
        {
            get { return (bool)this.GetValue(IsUpProperty); }
            set { this.SetValue(IsUpProperty, value); }
        }


        public AnimStateTransitionCurve()
        {

        }
        public void AddTransitionControl(BaseNodeControl ctrl)
        {
            if (ctrl == null)
                return;
            mTransitionControlList.Add(ctrl);
            UpdateTransitionPos();
        }
        public void RemoveTransitionControl(BaseNodeControl ctrl)
        {
            mTransitionControlList.Remove(ctrl);
            UpdateTransitionPos();
        }
        List<BaseNodeControl> mTransitionControlList = new List<BaseNodeControl>();
        public List<BaseNodeControl> TransitionControlList
        {
            get => mTransitionControlList;
        }
        public void UpdateTransitionPos()
        {
            var vec = this.EndPoint - this.StartPoint;
            vec.Normalize();
            EngineNS.Vector3 vec3 = new EngineNS.Vector3((float)vec.X, (float)vec.Y, 0);
            var Z = EngineNS.Vector3.UnitZ;
            var Y = EngineNS.Vector3.Cross(vec3, Z);
            Y.Normalize();
            Y *= 22;
            var localYOffset = new Vector(Y.X, Y.Y);
            Vector start = new Vector(StartPoint.X, StartPoint.Y);
            Vector end = new Vector(EndPoint.X, EndPoint.Y);
            var localMidPoint = (start + end) * 0.5f;
            var stride = 36 * vec;
            var t = mTransitionControlList.Count / 2;
            bool isOdd = false;
            if (mTransitionControlList.Count % 2 != 0)
            {
                isOdd = true;//奇数
            }
            Vector pos;
            for (int i = 0; i < t; ++i)
            {
                if(isOdd)
                  pos = localMidPoint - stride * (i + 1);
                else
                {
                    pos = localMidPoint - stride * (i +0.5f);
                }
                SetCtrlLocation(mTransitionControlList[i], pos + localYOffset);
            }
            if (isOdd)
            {
                SetCtrlLocation(mTransitionControlList[t], localMidPoint + localYOffset);
            }
            for (int i = 0; i < t; ++i)
            {
                int index = 0;
                if (isOdd)
                {
                    pos = localMidPoint + stride * (i + 1);
                    index = i + t + 1;
                }
                else
                {
                    pos = localMidPoint + stride * (i + 0.5f);
                    index = i + t;
                }
                SetCtrlLocation(mTransitionControlList[index], pos + localYOffset);
            }

        }
        void SetCtrlLocation(BaseNodeControl ctrl, Vector pos)
        {
            ctrl.SetLocation(pos.X, pos.Y);
        }
        #region Overrides

        /// <summary>
        /// 重载渲染事件
        /// </summary>
        /// <param name="drawingContext">绘图上下文</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var startPoint = this.StartPoint;

            var vec = this.EndPoint - this.StartPoint;
            var angle = this.GetAngle(this.StartPoint, this.EndPoint);

            // 使用旋转变换,使其与线平行
            var transform = new RotateTransform(angle)
            {
                CenterX = this.StartPoint.X,
                CenterY = this.StartPoint.Y
            };
            drawingContext.PushTransform(transform);


            var offsetY = this.StrokeThickness;
            if (this.IsUp)
            {
                // 计算文本的行数
                //double textLineCount = formattedText.Width / formattedText.MaxTextWidth;
                //if (textLineCount < 1)
                //{
                //    // 怎么也得有一行
                //    textLineCount = 1;
                //}

                //// 计算朝上的偏移
                //offsetY = (-formattedText.Height * textLineCount) - this.StrokeThickness;
            }

            startPoint = startPoint + new Vector(0, offsetY);
            //drawingContext.DrawText(formattedText, startPoint);
            drawingContext.Pop();


        }

        #endregion Overrides
        #region Private Methods

        /// <summary>
        /// 获取两个点的倾角
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <returns>两个点的倾角</returns>
        private double GetAngle(Point start, Point end)
        {
            var vec = end - start;

            // X轴
            var xaxis = new Vector(1, 0);
            return Vector.AngleBetween(xaxis, vec);
        }

        #endregion Private Methods
    }
}
