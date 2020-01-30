using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Diagnostics;
using EngineNS.Profiler;
using System.Threading.Tasks;

namespace EditorCommon
{
    public class Editor_PropertyGridSortTypeAttribute : Attribute
    {
        public enum enSortType
        {
            NoSort,
            Custom,
        }
        public enSortType SortType;
        public IComparer<WPG.Data.Property> Comparer;
        public Editor_PropertyGridSortTypeAttribute()
        {
            SortType = enSortType.NoSort;
        }
        public Editor_PropertyGridSortTypeAttribute(IComparer<WPG.Data.Property> comparer)
        {
            SortType = enSortType.Custom;
            Comparer = comparer;
        }
    }

    public interface ICopyPasteData { }

    public class Program
    {
        public readonly static string PackageExtension = "package";
        //public static List<Window> ShowedWindows = new List<Window>();
        //static System.Windows.Threading.Dispatcher mMainDispatcher;
        //public static System.Windows.Threading.Dispatcher MainDispatcher
        //{
        //    get
        //    {
        //        if (mMainDispatcher == null && Application.Current != null)
        //            return Application.Current.Dispatcher;
        //        return mMainDispatcher;
        //    }
        //    protected set
        //    {
        //        mMainDispatcher = value;
        //    }
        //}
        static Dictionary<Type, object> mDefaultClassValueDic = new Dictionary<Type, object>();
        public static object GetClassPropertyDefaultValue(Type classType, string propertyName)
        {
            try
            {
                if (classType == null)
                    return null;
                if (classType.IsInterface)
                    return null;
                if(classType.GetCustomAttributes(typeof(EngineNS.Editor.Editor_NoDefaultObjectAttribute), true).Length>0)
                {
                    return null;
                }
                var proInfo = classType.GetProperty(propertyName);
                if (proInfo == null)
                    return null;

                object classIns = null;
                if (!mDefaultClassValueDic.TryGetValue(classType, out classIns))
                {
                    classIns = System.Activator.CreateInstance(classType);
                    mDefaultClassValueDic[classType] = classIns;
                }
                if (classIns == null)
                    return null;

                return proInfo.GetValue(classIns);
            }
            catch(System.Exception)
            {
                return null;
            }
        }
        public static async System.Threading.Tasks.Task OpenEditor(EditorCommon.Resources.ResourceEditorContext context)
        {
            await PluginAssist.Process.OnOpenEditor(context);
        }

        public static FrameworkElement GetParent(FrameworkElement childElement, Type parentType)
        {
            if (childElement == null)
                return null;

            var parent = childElement.Parent as FrameworkElement;
            while (parent != null)
            {
                if (parent.GetType() == parentType)
                    return parent;

                var baseType = parent.GetType().BaseType;
                while (baseType != null)
                {
                    if (baseType == parentType)
                        return parent;

                    baseType = baseType.BaseType;
                }

                parent = parent.Parent as FrameworkElement;
            }

            return null;
        }


        public static List<DependencyObject> GetChildren(DependencyObject parent, Type childType)
        {
            var retList = new List<DependencyObject>();
            if (parent == null)
                return retList;
            //var count = VisualTreeHelper.GetChildrenCount(parent);
            //for(int i=0; i<count; i++)
            //{
            //    var child = VisualTreeHelper.GetChild(parent, i);
            //    if (child.GetType() == childType)
            //        retList.Add(child);
            //    else
            //        retList.AddRange(GetChildren(child, childType));
            //}
            var children = LogicalTreeHelper.GetChildren(parent);
            foreach(var child in children)
            {
                var cType = child.GetType();
                if ((cType == childType) ||
                   (cType.IsSubclassOf(childType)) ||
                   (cType.GetInterface(childType.FullName) != null))
                {
                    retList.Add(child as DependencyObject);
                }
                else
                    retList.AddRange(GetChildren(child as DependencyObject, childType));
            }
            return retList;
        }

        public static bool RemoveElementFromParent(FrameworkElement element)
        {
            if (element == null)
                return false;

            if (element.Parent == null)
                return false;


            if (element.Parent is ItemsControl)
            {
                var itemsCtrl = element.Parent as ItemsControl;
                itemsCtrl.Items.Remove(element);
            }
            else if (element.Parent is Panel)
            {
                var panel = element.Parent as Panel;
                panel.Children.Remove(element);
            }
            else if (element.Parent is ContentControl)
            {
                var contentCtrl = element.Parent as ContentControl;
                contentCtrl.Content = null;
            }
            else
                return false;

            return true;
        }

        public static int GetParentChildrenCount(FrameworkElement element)
        {
            if (element == null)
                return int.MaxValue;

            if (element.Parent == null)
                return int.MaxValue;

            if (element.Parent is ItemsControl)
            {
                var itemsCtrl = element.Parent as ItemsControl;
                return itemsCtrl.Items.Count;
            }
            else if (element.Parent is Panel)
            {
                var panel = element.Parent as Panel;
                return panel.Children.Count;
            }
            else if (element.Parent is ContentControl)
            {
                var contentCtrl = element.Parent as ContentControl;
                if (contentCtrl.Content != null)
                    return 1;
                else
                    return 0;
            }

            return -1;
        }

        public static int SetElementParent(FrameworkElement element, FrameworkElement parent)
        {
            if (element == null || parent == null)
                return -1;

            Program.RemoveElementFromParent(element);

            if (element is TabItem)
            {
                if (parent is TabControl)
                {
                    ((TabControl)parent).Items.Add(element);
                }
                else
                {
                    var tabCtrl = new DockControl.Controls.DockAbleTabControl();
                    SetElementParent(tabCtrl, parent);
                    tabCtrl.Items.Add(element);
                }
            }
            else if (parent is ItemsControl)
            {
                var itemsCtrl = parent as ItemsControl;
                itemsCtrl.Items.Add(element);
            }
            else if (parent is Panel)
            {
                var panel = parent as Panel;
                panel.Children.Add(element);
            }
            else if (parent is ContentControl)
            {
                var cCtrl = parent as ContentControl;
                cCtrl.Content = element;
            }
            else
                return 0;

            return 1;
        }
        public static object ShowPropertyGridInWindows(object insObj, object inspector = null)
        {
            var mLastPropertyGrid = inspector as WPG.PropertyGrid;
            if (mLastPropertyGrid == null)
            {
                var win = new DockControl.DockAbleWindow();
                mLastPropertyGrid = new WPG.PropertyGrid();
                mLastPropertyGrid.Instance = insObj;
                win.MainGrid.Children.Add(mLastPropertyGrid);
                win.Show();
                return mLastPropertyGrid;
            }
            else
            {
                mLastPropertyGrid.Instance = insObj;
                return mLastPropertyGrid;
            }
        }

        #region ResourceAssist

        public static string SnapshotExt
        {
            get { return ".snap"; }
        }
        public static string ResourceInfoExt
        {
            get { return ".rinfo"; }
        }
        public static string ResourcItemDragType
        {
            get { return "ResourceItem"; }
        }
        public static bool IsValidRName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (!Regex.IsMatch(name, @"^[0-9a-zA-Z_]{1,}$"))
            {
                return false;
            }
            return true;
        }
        public static string GetValidName(string absFolder, string prefix, string ext)
        {
            if (string.IsNullOrEmpty(absFolder))
                return "";
            ext = "*" + ext;
            var gms = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, ext);
            List<string> names = new List<string>();
            foreach(var i in gms)
            {
                var name = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(i, false).ToLower();
                names.Add(name);
            }
            UInt32 index = 0;
            while (true)
            {
                var name = $"{prefix}_{++index}";
                name = name.ToLower();
                bool find = false;
                foreach (var i in names)
                {
                    if (i == name)
                    {
                        find = true;
                        break;
                    }
                }
                if (find == false)
                    return name;
            }
        }
        #endregion

        #region CopyPaste

        static Dictionary<string, ICopyPasteData> mCopyPasteDataDictionary = new Dictionary<string, ICopyPasteData>();
        public static ICopyPasteData GetCopyPasteData(string key)
        {
            ICopyPasteData data;
            mCopyPasteDataDictionary.TryGetValue(key, out data);
            return data;
        }
        public static void SetCopyPasteData(string key, ICopyPasteData data)
        {
            mCopyPasteDataDictionary[key] = data;
        }

        #endregion

        #region Build Game Dll

        public static bool NeedBuildGameDll = false;
        static BuildGameDllTick mBuildGameDllTick;
        public static void BuildGameDll(bool force)
        {
            if(mBuildGameDllTick == null)
            {
                mBuildGameDllTick = new BuildGameDllTick();
                EngineNS.CEngine.Instance.TickManager.AddTickInfo(mBuildGameDllTick);
            }
            mBuildGameDllTick.EnableTick = true;
            mBuildGameDllTick.ResetDelayTime();
            mBuildGameDllTick.Force = force;
        }
        public static async Task<bool> BuildGameDllImmediately(bool force = false)
        {
            var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(1, new System.Threading.AutoResetEvent(false));
            var retValue = await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                if (!force)
                {
                    if (NeedBuildGameDll == false)
                    {
                        smp.Release();
                        return false;
                    }
                    if (EngineNS.CIPlatform.Instance.PlayMode != EngineNS.CIPlatform.enPlayMode.Editor)
                    {
                        smp.Release();
                        return false;
                    }
                    if (DockControl.DockManager.Instance.CurrentActiveWindow == null)
                    {
                        smp.Release();
                        return false;
                    }
                }

                var p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.StandardInput.WriteLine($"\"{EditorCommon.GameProjectConfig.Instance.MSBuildAbsFileName}\" \"{EditorCommon.GameProjectConfig.Instance.GameProjFileName}\" /p:Configuration=Debug" + " &exit");
                p.StandardInput.AutoFlush = true;
                var outputStaream = p.StandardOutput.ReadToEnd();
                //Console.WriteLine(outputStaream);
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Info, "Macross", outputStaream);
                var errorStream = p.StandardError.ReadToEnd();
                //Console.WriteLine(errorStream);
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Info, "Macross", errorStream);
                p.WaitForExit();
                p.Close();

                NeedBuildGameDll = false;
                if (!string.IsNullOrEmpty(errorStream) || outputStaream.Contains(": error"))
                {
                    smp.Release();
                    return false;
                }

                smp.Release();
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            smp.Wait(int.MaxValue);

            return retValue;
        }

        class BuildGameDllTick : EngineNS.ITickInfo
        {
            public bool EnableTick
            {
                get;
                set;
            }
            public bool Force = false;
            public long DelayTime = 500;
            public void ResetDelayTime()
            {
                DelayTime = 500;
            }

            public void BeforeFrame()
            {
            }

            public TimeScope GetLogicTimeScope()
            {
                return null;
            }

            public void TickLogic()
            {
                if(EnableTick)
                {
                    // 延迟一段事件，防止短时间内大量调用
                    DelayTime -= EngineNS.CEngine.Instance.EngineElapseTime;
                    if(DelayTime <= 0)
                    {
                        var noUse = Program.BuildGameDllImmediately(Force);
                        ResetDelayTime();
                        EnableTick = false;
                    }
                }
            }

            public void TickRender()
            {
            }

            public void TickSync()
            {
            }
        }

        #endregion
    }
}
