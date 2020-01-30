using System;

namespace EditorCommon.Controls.Curves
{
    [Flags]
    public enum ArrowHalf
    {
        /// <summary>
        /// 左侧半箭头
        /// </summary>
        Up = 0,

        /// <summary>
        /// 右侧半箭头
        /// </summary>
        Down= 1,

        /// <summary>
        /// 两侧箭头
        /// </summary>
        Both = 2,
    }
    /// <summary>
    /// 箭头所在端
    /// </summary>
    [Flags]
    public enum ArrowEnds
    {
        /// <summary>
        /// 无箭头
        /// </summary>
        None = 0,

        /// <summary>
        /// 开始方向箭头
        /// </summary>
        Start = 1,

        /// <summary>
        /// 结束方向箭头
        /// </summary>
        End = 2,

        /// <summary>
        /// 两端箭头
        /// </summary>
        Both = 3
    }
}