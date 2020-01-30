using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation
{
    /// <summary>
    /// 时间轴接口
    /// </summary>
    public interface TimeLineObjectInterface
    {
        /// <summary>
        /// 时间轴的名称
        /// </summary>
        string TimeLineObjectName
        {
            get;
            set;
        }
        /// <summary>
        /// 在时间轴上添加关键帧
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="name">名称</param>
        /// <returns>返回该时间轴上的关键帧对象</returns>
        TimeLineKeyFrameObjectInterface AddKeyFrameObject(Int64 startTime, Int64 endTime, string name);
        /// <summary>
        /// 删除关键帧对象
        /// </summary>
        /// <param name="frame">时间轴上的关键帧接口对象</param>
        void RemoveKeyFrameObject(TimeLineKeyFrameObjectInterface frame);
        /// <summary>
        /// 获取关键帧
        /// </summary>
        /// <returns>返回该时间轴上关键帧列表</returns>
        List<TimeLineKeyFrameObjectInterface> GetKeyFrames();
        /// <summary>
        /// 获取关键帧的类型
        /// </summary>
        /// <returns>返回关键帧的类型</returns>
        Type GetKeyFrameType();
    }
    /// <summary>
    /// 时间轴上的关键帧接口
    /// </summary>
    public interface TimeLineKeyFrameObjectInterface
    {
        /// <summary>
        /// 关键帧名称
        /// </summary>
        string KeyFrameName
        {
            get;
            set;
        }
        /// <summary>
        /// 关键帧的开始时间
        /// </summary>
        Int64 KeyFrameMilliTimeStart
        {
            get;
            set;
        }
        /// <summary>
        /// 关键帧的结束时间
        /// </summary>
        Int64 KeyFrameMilliTimeEnd
        {
            get;
            set;
        }
        /// <summary>
        /// 是否可修改长度
        /// </summary>
        /// <returns>可以修改返回true，否则返回false</returns>
        bool CanModityLength();

        /// <summary>
        /// 更新说明信息
        /// </summary>
        /// <returns></returns>
        string UpdateToolTip();
    }
    /// <summary>
    /// 3D盒子属性类
    /// </summary>
    public class V3dBox3Attribute : Attribute
    {
        /// <summary>
        /// 正常的颜色
        /// </summary>
        public Color NormalColor
        {
            get;
            set;
        } = Color.Red;
        /// <summary>
        /// 选中时的颜色
        /// </summary>
        public Color SelectedColor
        {
            get;
            set;
        } = Color.OrangeRed;
    }
}
