using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Controls
{
    [Rtti.MetaClass]
    public class UserControlInitializer : Containers.BorderInitializer
    {
        [Rtti.MetaData]
        public bool WidthAuto { get; set; } = false;
        [Rtti.MetaData]
        public float Width { get; set; } = 1920;
        [Rtti.MetaData]
        public bool HeightAuto { get; set; } = false;
        [Rtti.MetaData]
        public float Height { get; set; } = 1080;
    }
    [Editor_UIControlInit(typeof(UserControlInitializer))]
    [Editor_BaseElementAttribute("UserWidget.png")]
    public class UserControl : Containers.Border
    {
        [Browsable(false)]
        public RName RName
        {
            get;
            protected set;
        }

        [Browsable(false)]
        public bool Width_Auto
        {
            get
            {
                var init = (UserControlInitializer)mInitializer;
                if (init != null)
                    return init.WidthAuto;
                return false;
            }
            set
            {
                var init = (UserControlInitializer)mInitializer;
                if (init == null)
                    return;
                if (init.WidthAuto == value)
                    return;

                init.WidthAuto = value;
                UpdateLayout();
                OnPropertyChanged("Width_Auto");
            }
        }

        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.UIEditor_PropertysWithAutoSet]
        public int Width
        {
            get
            {
                var init = (UserControlInitializer)mInitializer;
                if(init != null)
                    return (int)init.Width;
                return 0;
            }
            set
            {
                var init = (UserControlInitializer)mInitializer;
                if (init == null)
                    return;
                if (init.Width == value)
                    return;

                init.Width = value;
                UpdateLayout();
                OnPropertyChanged("Width");
            }
        }

        [Browsable(false)]
        public bool Height_Auto
        {
            get
            {
                var init = (UserControlInitializer)mInitializer;
                if (init != null)
                    return init.HeightAuto;
                return false;
            }
            set
            {
                var init = (UserControlInitializer)mInitializer;
                if (init == null)
                    return;
                if (init.HeightAuto == value)
                    return;

                init.HeightAuto = value;
                UpdateLayout();
                OnPropertyChanged("Height_Auto");
            }
        }
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.UIEditor_PropertysWithAutoSet]
        public int Height
        {
            get
            {
                var init = (UserControlInitializer)mInitializer;
                if (init != null)
                    return (int)init.Height;
                return 0;
            }
            set
            {
                var init = (UserControlInitializer)mInitializer;
                if (init == null)
                    return;
                if (init.Height == value)
                    return;

                init.Height = value;
                UpdateLayout();
                OnPropertyChanged("Height");
            }
        }

        Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async Task<Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await Thread.Async.TaskLoader.Awaitload(WaitContext);
        }
        //public void SaveUI()
        //{
        //    if (RName == null || RName == EngineNS.RName.EmptyName)
        //        return;
        //    var holder = IO.XndHolder.NewXNDHolder();
        //    this.Save2Xnd(holder.Node);
        //    IO.XndHolder.SaveXND(RName.Address, holder);
        //}
        //public async Task<bool> LoadUIAsync(CRenderContext rc, RName rname)
        //{
        //    RName = rname;
        //    var xnd = await IO.XndHolder.LoadXND(rname.Address);
        //    if (xnd == null)
        //        return false;
        //    var result = await LoadXnd(rc, xnd.Node);
        //    if(result == false)
        //    {
        //        Thread.Async.TaskLoader.Release(ref WaitContext, null);
        //        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "UI", $"LoadUIAsync {rname.Address} Failed");
        //        return false;
        //    }

        //    Thread.Async.TaskLoader.Release(ref WaitContext, this);
        //    return true;
        //}
        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            var size = availableSize;
            if (!Width_Auto)
                size.Width = Width;
            if (!Height_Auto)
                size.Height = Height;
            var retValue = base.MeasureOverride(ref size);
            if (!Width_Auto)
                retValue.Width = Width;
            if (!Height_Auto)
                retValue.Height = Height;
            return retValue;
        }
    }
}
