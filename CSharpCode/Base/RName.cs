using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Macross;
using EngineNS.Rtti;
using NPOI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EngineNS
{
    public abstract class TtRNameTagObjectAttribute : Attribute 
    {
        public abstract object GetTagObject(RName rn);
    }

    [RName.PGRName]
    public partial class RName : IComparable<RName>, IComparable
    {
        public const ushort MinVersion = 100;//don't change this value any time
        public const ushort Version100 = MinVersion + 0;
        //public void Write(RName v)
        //{
        //    Write(RName.CurrentVersion);
        //    if (v== null)
        //    {
        //        Write(RName.ERNameType.Unkown);
        //        return;
        //    }
        //    Write(v.RNameType);
        //    Write(v.Name);
        //}
        public const ushort Version101 = MinVersion + 1;//increase this value when serialization changed
        //public void Write(RName v)
        //{
        //    Write(RName.CurrentVersion);
        //    if (v== null)
        //    {
        //        Write(RName.ERNameType.Unkown);
        //        return;
        //    }
        //    else
        //    {
        //        Write(v.RNameType);
        //    }
        //    Write(v.Name);
        //    Write(v.AssetId);
        //}
        public const ushort CurrentVersion = Version101;
        private static void TryOpenAssetEditor(RName assetName)
        {
            if (assetName == null)
                return;

            var assetMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(assetName);
            if (assetMeta == null)
                return;

            var typeDesc = TtTypeDesc.TypeOf(assetMeta.TypeStr);
            var type = typeDesc?.SystemType;
            if (type == null)
                return;

            var attrs = type.GetCustomAttributes(typeof(Editor.UAssetEditorAttribute), false);
            if (attrs.Length == 0)
                return;

            var editorAttr = attrs[0] as Editor.UAssetEditorAttribute;
            if (editorAttr?.EditorType == null)
                return;

            Editor.TtAssetEditorManager.TryOpenEditor(editorAttr.EditorType, assetName, null, true).AddWaitTask();
        }
        public class PGRNameAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
        {
            public string FilterExts;   // "ext1" / "ext1,ext2"
            public System.Type MacrossType;
            public string ShaderType;
            EGui.UIProxy.ComboBox mComboBox;

            class DrawData
            {
                public RName NewValue;
            }
            [System.ThreadStatic]
            static DrawData mDrawData = new DrawData();

            public EGui.Controls.TtContentBrowser ContentBrowser;

            public float MaxWidth = -1;
            public float MinWidth = -1;
            protected override async Thread.Async.TtTask<bool> Initialize_Override()
            {
                ContentBrowser = Editor.TtEditor.NewPopupContentBrowser();
                mComboBox = new EGui.UIProxy.ComboBox()
                {
                    ComboOpenAction = ComboOpenAction
                };
                await mComboBox.Initialize();

                return await base.Initialize_Override();
            }
            ~PGRNameAttribute()
            {
                Cleanup();
            }
            protected override void Cleanup_Override()
            {
                mComboBox?.Cleanup();
                base.Cleanup_Override();
            }
            void ComboOpenAction(in Support.TtAnyPointer data)
            {
                ContentBrowser.OnDraw();
                //TtEngine.Instance.EditorInstance.RNamePopupContentBrowser.OnDraw();
            }
            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;

                var name = info.Value as RName;
                mDrawData.NewValue = name;
                //var newName = EGui.Controls.CtrlUtility.DrawRName(name, info.Name, FilterExts, info.Readonly, mSnap);

                var drawList = ImGuiAPI.GetWindowDrawList();
                ImGuiAPI.BeginGroup();
                ImGuiAPI.PushID(info.Name ?? "RName");

                var groupStart = ImGuiAPI.GetCursorScreenPos();
                var snapSize = new Vector2(64, 64);
                var snapEnd = groupStart + snapSize;
                var assetMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name);
                drawList.AddRectFilled(in groupStart, in snapEnd, 0xff202020, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
                var snapStart = groupStart;
                assetMeta?.OnDrawSnapshot(in drawList, ref snapStart, ref snapEnd);
                drawList.AddRect(in groupStart, in snapEnd, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);
                ImGuiAPI.InvisibleButton("##AssetPreview", in snapSize, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    if (assetMeta != null)
                        assetMeta.DrawTooltip();
                    else if (name != null)
                        EGui.Controls.CtrlUtility.DrawHelper(name.ToString());

                    if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        TryOpenAssetEditor(name);
                    }
                }

                var preViewStr = "null";
                if (name != null)
                    preViewStr = name.ToString();
                var editorStart = groupStart + new Vector2(snapSize.X + 8, 0);
                ImGuiAPI.SetCursorScreenPos(in editorStart);
                ImGuiAPI.Dummy(in Vector2.Zero);
                Support.TtAnyPointer anyPt = new Support.TtAnyPointer()
                {
                    RefObject = mDrawData,
                };
                var index = ImGuiAPI.TableGetColumnIndex();
                mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_None | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton | ImGuiComboFlags_.ImGuiComboFlags_HeightMask_;
                mComboBox.WinFlags = ImGuiWindowFlags_.ImGuiWindowFlags_Popup |
                                     ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                                     ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                                     ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
                mComboBox.Width = ImGuiAPI.GetColumnWidth(index) - snapSize.X - 8 - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
                if (mComboBox.Width < 120)
                    mComboBox.Width = 120;
                mComboBox.Name = string.IsNullOrEmpty(info.Name) ? "##RName" : $"##{info.Name}";
                mComboBox.PreviewValue = preViewStr;
                var contentBrowserSize = new Vector2(500, 600);
                ImGuiAPI.SetNextWindowSize(in contentBrowserSize, ImGuiCond_.ImGuiCond_Appearing);
                ContentBrowser.ExtNames = FilterExts;
                ContentBrowser.MacrossBase = TtTypeDesc.TypeOf(MacrossType);
                ContentBrowser.ShaderType = ShaderType;
                ContentBrowser.SelectedAssets.Clear();
                mComboBox.OnDraw(in drawList, in anyPt);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    EGui.Controls.CtrlUtility.DrawHelper(preViewStr);
                }
                if (ContentBrowser.SelectedAssets.Count > 0 &&
                    ContentBrowser.SelectedAssets[0].GetAssetName() != name)
                {
                    if (!info.Readonly)
                        mDrawData.NewValue = ContentBrowser.SelectedAssets[0].GetAssetName();
                    else
                        Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Info, $"RName {info.Name}: is readonly");
                }
                var pos = editorStart + new Vector2(0, ImGuiAPI.GetFrameHeight() + 4);
                ImGuiAPI.SetCursorScreenPos(in pos);
                if (info.Readonly)
                {
                    Vector4 color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                    ImGuiAPI.TextColored(in color, "readonly");
                }
                else
                {
                    var sz = new Vector2(0, 0);
                    if (ImGuiAPI.Button("F", in sz))
                    {
                        EGui.Controls.TtContentBrowser.GlobalFocusAsset = mDrawData.NewValue;
                    }
                    ImGuiAPI.SameLine(0, 8);
                    if (ImGuiAPI.Button("<", in sz))
                    {
                        mDrawData.NewValue = EGui.Controls.TtContentBrowser.GlobalSelectedAsset?.GetAssetName();
                    }
                    ImGuiAPI.SameLine(0, 8);
                    if (ImGuiAPI.Button("-", in sz))
                    {
                        mDrawData.NewValue = null;
                    }
                }

                ImGuiAPI.PopID();
                ImGuiAPI.EndGroup();

                if (mDrawData.NewValue != name)
                {
                    newValue = mDrawData.NewValue;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// PropertyGrid 用来编辑 List&lt;RName&gt; 的自定义编辑器：
        ///   - 接管整行渲染 (FullRedraw=true)，PG 不会下钻成 IList 元素，所以可以稳定地把 FilterExts
        ///     传给每行的 ContentBrowser，做到 "选元素时按指定扩展名过滤"。
        ///   - 每行都用一个 ComboBox + 共享的 ContentBrowser 弹窗选资产，并提供 F (Focus) / &lt;
        ///     (从全局选中拉取) / - (清空本行) 三个常用按钮，与 PGRNameAttribute 风格保持一致。
        ///   - 行末的 [+] 添加一行 null，[X] 删除该行；列表末尾还有 [+ Add] 整体新增按钮。
        /// 用法：
        ///   [RName.PGRNameList(FilterExts = TtMaterial.AssetExt + "," + TtMaterialInstance.AssetExt)]
        ///   public List&lt;RName&gt; MaterialNames { get; set; } = new();
        /// </summary>
        public class PGRNameListAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
        {
            public string FilterExts;
            public System.Type MacrossType;
            public string ShaderType;

            /// <summary>
            /// 是否允许用户在 PG 上"增加"列表元素 (顶部 [+ Add] 与每行末尾的 [+])。
            /// 对于"长度由外部约束决定"的列表 (例如 MaterialNames 的长度必须等于 mesh atom 数),
            /// 应在构造时传 false, 防止用户改长度后和外部数据不一致。
            /// </summary>
            public bool AllowAdd { get; }

            /// <summary>
            /// 是否允许用户在 PG 上"删除"列表元素 (顶部 [Clear] 与每行末尾的 [X])。
            /// 行末的 [-] 是"清空本行 (置 null)", 不算删除, 不受此开关影响。
            /// </summary>
            public bool AllowRemove { get; }

            // 一个 attribute 实例 = 一个属性 (例如 MaterialNames)，整列共享一个 ComboBox + ContentBrowser；
            // 当前正在弹窗的元素下标用 mActiveEditingIndex 记录, 避免不同行互相串选中态。
            EGui.UIProxy.ComboBox mComboBox;
            EGui.Controls.TtContentBrowser mContentBrowser;
            int mActiveEditingIndex = -1;

            /// <summary>
            /// 默认允许增删 (兼容老用法)。
            /// </summary>
            public PGRNameListAttribute() : this(true, true) { }

            /// <summary>
            /// 显式指定是否允许增 / 删元素。
            /// 用法示例:
            ///   [RName.PGRNameList(false, false, FilterExts = "...")]  // 长度固定, 只能改内容
            ///   [RName.PGRNameList(true,  true,  FilterExts = "...")]  // 完全可编辑 (= 默认构造器)
            /// </summary>
            public PGRNameListAttribute(bool allowAdd, bool allowRemove)
            {
                FullRedraw = true;
                AllowAdd = allowAdd;
                AllowRemove = allowRemove;
            }

            protected override async Thread.Async.TtTask<bool> Initialize_Override()
            {
                mContentBrowser = Editor.TtEditor.NewPopupContentBrowser();
                mComboBox = new EGui.UIProxy.ComboBox()
                {
                    ComboOpenAction = ComboOpenAction,
                };
                await mComboBox.Initialize();
                return await base.Initialize_Override();
            }

            ~PGRNameListAttribute()
            {
                Cleanup();
            }

            protected override void Cleanup_Override()
            {
                mComboBox?.Cleanup();
                mComboBox = null;
                base.Cleanup_Override();
            }

            void ComboOpenAction(in Support.TtAnyPointer data)
            {
                mContentBrowser.OnDraw();
            }

            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                var list = info.Value as System.Collections.IList;
                if (list == null)
                    return false;

                bool changed = false;
                var drawList = ImGuiAPI.GetWindowDrawList();

                // 先画属性名 + 元素数 + 顶部按钮，再画各元素行；用 TreeNode 保留可折叠效果。
                ImGuiAPI.TableSetColumnIndex(0);
                var nodeFlags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow |
                                ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen;
                bool open = ImGuiAPI.TreeNodeEx(info.Name, nodeFlags, $"{info.Name}");

                ImGuiAPI.TableSetColumnIndex(1);
                ImGuiAPI.AlignTextToFramePadding();
                ImGuiAPI.Text($"{list.Count} elements");
                if (!info.Readonly)
                {
                    var sz = new Vector2(0, 0);
                    if (AllowAdd)
                    {
                        ImGuiAPI.SameLine(0, 8);
                        if (ImGuiAPI.Button($"+ Add##{info.Name}_AddRoot", in sz))
                        {
                            list.Add(null);
                            changed = true;
                        }
                    }
                    if (AllowRemove)
                    {
                        ImGuiAPI.SameLine(0, 8);
                        if (ImGuiAPI.Button($"Clear##{info.Name}_Clear", in sz))
                        {
                            if (list.Count > 0)
                            {
                                list.Clear();
                                changed = true;
                            }
                        }
                    }
                }

                if (open)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (DrawElement(in info, list, i, drawList))
                        {
                            // DrawElement 内部已经修改 list, 这里只标记并跳出本帧避免下标错位。
                            changed = true;
                            break;
                        }
                    }
                    ImGuiAPI.TreePop();
                }

                if (changed)
                {
                    newValue = list;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 画单个元素行：左 snap，中间 ComboBox(弹 ContentBrowser, 用 FilterExts 过滤), 右 F/&lt;/-/+/X 按钮。
            /// 返回 true 表示本行触发了"会改变 list 长度"的操作（+/X），调用方应该立即 break 重新走下一帧。
            /// </summary>
            unsafe bool DrawElement(in EditorInfo info, System.Collections.IList list, int index, ImDrawList drawList)
            {
                var name = list[index] as RName;

                // 给本行划一个独立的 ID 作用域, 避免 ComboBox/Button 重名冲突。
                ImGuiAPI.PushID(index);

                ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                ImGuiAPI.TableSetColumnIndex(0);
                ImGuiAPI.AlignTextToFramePadding();
                ImGuiAPI.Text($"  [{index}]");

                ImGuiAPI.TableSetColumnIndex(1);
                ImGuiAPI.BeginGroup();

                var groupStart = ImGuiAPI.GetCursorScreenPos();
                var snapSize = new Vector2(48, 48);
                var snapEnd = groupStart + snapSize;
                var assetMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name);
                drawList.AddRectFilled(in groupStart, in snapEnd, 0xff202020, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
                var snapStart = groupStart;
                assetMeta?.OnDrawSnapshot(in drawList, ref snapStart, ref snapEnd);
                drawList.AddRect(in groupStart, in snapEnd, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);
                ImGuiAPI.InvisibleButton("##AssetPreview", in snapSize, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    if (assetMeta != null)
                        assetMeta.DrawTooltip();
                    else if (name != null)
                        EGui.Controls.CtrlUtility.DrawHelper(name.ToString());

                    if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        TryOpenAssetEditor(name);
                    }
                }

                var preViewStr = name == null ? "null" : name.ToString();
                var comboPos = groupStart + new Vector2(snapSize.X + 8, 0);
                ImGuiAPI.SetCursorScreenPos(in comboPos);
                ImGuiAPI.Dummy(in Vector2.Zero);

                bool listMutated = false;

                if (mComboBox != null)
                {
                    var anyPt = new Support.TtAnyPointer();
                    var colIdx = ImGuiAPI.TableGetColumnIndex();
                    mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_None
                                    | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton
                                    | ImGuiComboFlags_.ImGuiComboFlags_HeightMask_;
                    mComboBox.WinFlags = ImGuiWindowFlags_.ImGuiWindowFlags_Popup
                                       | ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar
                                       | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings
                                       | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
                    mComboBox.Width = ImGuiAPI.GetColumnWidth(colIdx) - snapSize.X - 8 - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
                    if (mComboBox.Width < 120)
                        mComboBox.Width = 120;
                    mComboBox.Name = $"##{info.Name}_combo_{index}";
                    mComboBox.PreviewValue = preViewStr;

                    var contentBrowserSize = new Vector2(500, 600);
                    ImGuiAPI.SetNextWindowSize(in contentBrowserSize, ImGuiCond_.ImGuiCond_Appearing);

                    // 共享的 ContentBrowser 在每次绘制前按"当前正在编辑的元素"刷一次 ext 和清空选中态。
                    mContentBrowser.ExtNames = FilterExts;
                    mContentBrowser.MacrossBase = MacrossType != null ? Rtti.TtTypeDesc.TypeOf(MacrossType) : null;
                    mContentBrowser.ShaderType = ShaderType;
                    mContentBrowser.SelectedAssets.Clear();
                    mActiveEditingIndex = index;

                    mComboBox.OnDraw(in drawList, in anyPt);
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        EGui.Controls.CtrlUtility.DrawHelper(preViewStr);
                    }

                    if (!info.Readonly &&
                        mActiveEditingIndex == index &&
                        mContentBrowser.SelectedAssets.Count > 0 &&
                        mContentBrowser.SelectedAssets[0].GetAssetName() != name)
                    {
                        list[index] = mContentBrowser.SelectedAssets[0].GetAssetName();
                        listMutated = true;
                    }
                }

                // 行末按钮组：F/<//-/+/X
                var buttonPos = comboPos + new Vector2(0, ImGuiAPI.GetFrameHeight() + 4);
                ImGuiAPI.SetCursorScreenPos(in buttonPos);
                var btnSz = new Vector2(0, 0);
                if (info.Readonly)
                {
                    Vector4 ro = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                    ImGuiAPI.TextColored(in ro, "readonly");
                }
                else
                {
                    if (ImGuiAPI.Button("F", in btnSz))
                    {
                        EGui.Controls.TtContentBrowser.GlobalFocusAsset = list[index] as RName;
                    }
                    ImGuiAPI.SameLine(0, 4);
                    if (ImGuiAPI.Button("<", in btnSz))
                    {
                        var picked = EGui.Controls.TtContentBrowser.GlobalSelectedAsset?.GetAssetName();
                        if (picked != list[index] as RName)
                        {
                            list[index] = picked;
                            listMutated = true;
                        }
                    }
                    ImGuiAPI.SameLine(0, 4);
                    if (ImGuiAPI.Button("-", in btnSz))
                    {
                        if (list[index] != null)
                        {
                            list[index] = null;
                            listMutated = true;
                        }
                    }
                    // 列表长度型操作：返回 true 让 OnDraw break，避免本帧后续元素下标错乱。
                    if (AllowAdd)
                    {
                        ImGuiAPI.SameLine(0, 12);
                        if (ImGuiAPI.Button("+", in btnSz))
                        {
                            list.Insert(index + 1, null);
                            listMutated = true;
                        }
                    }
                    if (AllowRemove)
                    {
                        ImGuiAPI.SameLine(0, 4);
                        if (ImGuiAPI.Button("X", in btnSz))
                        {
                            list.RemoveAt(index);
                            listMutated = true;
                        }
                    }
                }

                ImGuiAPI.EndGroup();
                ImGuiAPI.PopID();

                return listMutated;
            }
        }
        public class TtRNameStats
        {
            public uint NameUniqueIdAllocator = 0;
        }
        private static TtRNameStats Stats = new TtRNameStats();
        private uint NameUniqueId;
        public enum ERNameType : ushort
        {
            Game = 0,
            Engine,
            Transient,
            Cloud,
            Count,
            Unkown = ushort.MaxValue,
        }
        ERNameType mRNameType = ERNameType.Game;

        public static string GetRNameTypeCodeString(ERNameType type)
        {
            return $"{type.GetType().FullName.Replace("+", ".")}.{type.ToString()}";
        }

        string mName;
        string mAddress;
        public WeakReference mTagReference = null;
        internal RName(string name, ERNameType type)
        {
            lock (Stats)
            {
                System.Diagnostics.Debug.Assert(Stats.NameUniqueIdAllocator < uint.MaxValue - 1);
                NameUniqueId = Stats.NameUniqueIdAllocator;
                Stats.NameUniqueIdAllocator++;
            }
            VeryDangrouseUpdate(name, type);
        }
        public T GetTagObject<T>()
        {
            if (mTagReference == null || mTagReference.IsAlive == false)
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(TtRNameTagObjectAttribute), true);
                if (attrs.Length > 0)
                {
                    var attrTag = attrs[0] as TtRNameTagObjectAttribute;
                    mTagReference = new WeakReference(attrTag.GetTagObject(this));
                }
            }
            return (T)mTagReference.Target;
        }

        public class PGMacrossRNameAttribute<T> : PGRNameAttribute where T : class
        {
            //public UMacrossGetter<T> MacrossGetter;

            public PGMacrossRNameAttribute()
            {
                MacrossType = typeof(T);
            }

            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                var changed = base.OnDraw(info, out newValue);

                //if(MacrossGetter == null)
                //    MacrossGetter = UMacrossGetter<T>.NewInstance();
                //var newRName = (RName)newValue;
                //if (MacrossGetter.Name != newRName)
                //{
                //    MacrossGetter.Reset(TtEngine.Instance.MacrossModule);
                //    MacrossGetter.Name = newRName;
                //}
                //var obj = MacrossGetter.Get();

                ISceneNodeMacrossInterface mi = null;
                var enumrableInterface = info.ObjectInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    foreach (var objIns in (IEnumerable)info.ObjectInstance)
                    {
                        if (objIns == null)
                            continue;

                        mi = objIns as ISceneNodeMacrossInterface;
                        if (mi != null)
                            break;
                    }
                }
                else
                {
                    mi = info.ObjectInstance as ISceneNodeMacrossInterface;
                }
                if (mi != null)
                {
                    var obj = mi.GetMacrossObject();
                    if (obj != null)
                    {
                        var macrossObjInfo = new EditorInfo()
                        {
                            Name = info.Name,
                            Type = Rtti.TtTypeDesc.TypeOf(obj.GetType()),
                            Value = obj,
                            Readonly = info.Readonly,
                            HostPropertyGrid = info.HostPropertyGrid,
                            Flags = info.Flags,
                            Expand = true,
                            HostProperty = info.HostProperty,
                        };
                        object macrossObjVal;
                        TtPropertyGrid.DrawPropertyGridObjectItem(ref macrossObjInfo, out macrossObjVal);
                    }
                }

                return changed;
            }
        }

        public static bool IsEmpty(RName rName)
        {
            return string.IsNullOrEmpty(rName.mName);
        }
        public static bool IsExist(RName rName)
        {
            if (rName == null)
                return false;
            return IO.TtFileManager.FileExists(rName.Address) || IO.TtFileManager.DirectoryExists(rName.Address);
        }

        #region AMeta
        public IO.IAssetMeta AMeta
        {
            get
            {
                return TtEngine.Instance.AssetMetaManager.GetAssetMeta(this);
            }
        }
        public async Thread.Async.TtTask<T> GetAsset<T>(params object[] args) where T : class, IO.IAsset
        {
            var ameta = AMeta;
            if (ameta == null || ameta.AssetStatus != IO.IAssetMeta.EAssetStatus.Valid)
                return default(T);
            return await ameta.LoadAsset(args) as T;
        }
        public async Thread.Async.TtTask<T> CreateAsset<T>(params object[] args) where T : class, IO.IAsset
        {
            var ameta = AMeta;
            if (ameta == null || ameta.AssetStatus != IO.IAssetMeta.EAssetStatus.Valid)
                return default(T);
            return await ameta.CreateAsset(args) as T;
        }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public Guid AssetId
        {
            get
            {
                if (AMeta != null)
                    return AMeta.AssetId;
                
                return Guid.Empty;
            }
            set
            {
                if (value == Guid.Empty)
                    return;
                if (AMeta != null)
                {
                    if (AMeta.GetAssetName().Name != Name || AMeta.GetAssetName().mRNameType != mRNameType)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
            }
        }
        #endregion

        [Rtti.Meta("")]
        public ERNameType RNameType
        {
            get { return mRNameType; }
        }
        [Rtti.Meta("")]
        public string Name
        {
            get => mName;
        }
        [Rtti.Meta("")]
        public string Address
        {
            get => mAddress;
        }
        [Rtti.Meta("")]
        public string ExtName
        {
            get
            {
                return IO.TtFileManager.GetExtName(mName);
            }
        }
        [Rtti.Meta("")]
        public string PureName => IO.TtFileManager.GetPureName(mName);
        public string NoExtName => IO.TtFileManager.RemoveExtName(mName);
        public string AbsParentPath => IO.TtFileManager.GetParentPathName(Address);
        public string ParentPath => IO.TtFileManager.GetParentPathName(mName);
        public static RName GetRNameFromAbsPath(string path)
        {
            path = IO.TtFileManager.GetValidFileName(path);
            ERNameType rNameType = ERNameType.Count;
            string name = null;
            for (var i = IO.TtFileManager.ERootDir.Game; i < IO.TtFileManager.ERootDir.Count; i++)
            {
                var root = TtEngine.Instance.FileManager.GetRoot(i);
                if (path.StartsWith(root))
                {
                    switch (i)
                    {
                        case IO.TtFileManager.ERootDir.Game:
                            rNameType = ERNameType.Game;
                            break;
                        case IO.TtFileManager.ERootDir.Engine:
                            rNameType = ERNameType.Engine;
                            break;
                    }
                    name = path.Substring(root.Length);
                    break;
                }
            }
            return RNameManager.Instance.GetRName(name, rNameType);
        }
        public static RName GetRName(string name, ERNameType rNameType = ERNameType.Game)
        {
            return RNameManager.Instance.GetRName(name, rNameType);
        }
        public static RName TryGetRName(string name, ERNameType type)
        {
            return RNameManager.Instance.TryGetRName(name, type);
        }
        public static RName ParseFrom(string nameStr)
        {
            if (nameStr == null)
                return null;
            var segs = nameStr.Split(':');
            if (segs.Length < 2)
                return null;
            var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[1]);
            return RNameManager.Instance.GetRName(segs[0], rnType);
        }
        public override string ToString()
        {
            return $"{mName}:{mRNameType}";
        }
        public static string GetAddress(ERNameType type, string name, RName rn = null)
        {
            if (TtEngine.Instance==null)
                return null;
            switch (type)
            {
                case ERNameType.Engine:
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine) + name;
                case ERNameType.Game:
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game) + name;
                case ERNameType.Cloud:
                    if (rn==null)
                        return null;
                    if (SureCloudAMeta(rn)==false)
                    {
                        Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Warning, "RName", $"Cloud RName get address failed, AMeta not exist! RName:{rn} is a directory?");
                    }
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cloud) + name;
                default:
                    {
                        return null;
                    }
            }
        }
        public RName GetDirectoryRName()
        {
            var dir = IO.TtFileManager.GetBaseDirectory(mName);
            return GetRName(dir, RNameType);
        }

        #region IComparable
        internal void VeryDangrouseUpdate(string name, ERNameType type)
        {
            mName = name;
            mRNameType = type;
            mAddress = GetAddress(mRNameType, Name, this);
        }
        public override int GetHashCode()
        {
            return (int)NameUniqueId;
        }
        public int CompareTo(object obj)
        {
            var rName = (RName)obj;
            if (rName == null)
                return -1;
            return NameUniqueId.CompareTo(rName.NameUniqueId);
        }
        public int CompareTo(RName other)
        {
            return NameUniqueId.CompareTo(other.NameUniqueId);
        }
        #endregion
        internal class RNameManager
        {
            public static RNameManager Instance = new RNameManager();
            internal Dictionary<string, RName>[] mNameSets = null;
            public RNameManager()
            {
                mNameSets = new Dictionary<string, RName>[(int)ERNameType.Count];
                for (int i = 0; i < mNameSets.Length; i++)
                {
                    mNameSets[i] = new Dictionary<string, RName>();
                }
            }
            public RName GetRName(string name, ERNameType rNameType = ERNameType.Game)
            {
                name = name.Replace('\\', '/');
                name = name.ToLower();
                lock (this)
                {
                    if ((int)rNameType >= (int)ERNameType.Count)
                        return null;
                    RName result;
                    var dict = mNameSets[(int)rNameType];
                    if (dict.TryGetValue(name, out result) == false)
                    {
                        result = new RName(name, rNameType);
                        dict.Add(name, result);
                        return result;
                    }
                    return result;
                }
            }
            public RName TryGetRName(string name, ERNameType type)
            {
                name = name.Replace('\\', '/');
                name = name.ToLower();
                RName result;
                var dict = mNameSets[(int)type];
                if (dict.TryGetValue(name, out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            internal void VeryDangrouseRemove(RName rn)
            {
                foreach (var i in mNameSets[(int)rn.RNameType])
                {
                    if (i.Value == rn)
                        mNameSets[(int)rn.RNameType].Remove(i.Key);
                }
            }
            internal void VeryDangrouseAdd(RName rn)
            {
                mNameSets[(int)rn.RNameType][rn.Name] = rn;
            }
        }
    }    
}
