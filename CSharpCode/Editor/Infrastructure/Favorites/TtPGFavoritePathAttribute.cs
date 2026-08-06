using EngineNS.EGui.Controls.PropertyGrid;
using System;

namespace EngineNS.Editor.Infrastructure
{
    /// <summary>
    /// 通用"收藏夹路径"属性编辑器: 给 string 属性挂上它, Details 面板里这一行就会多出
    ///   [★] 把当前值加入收藏夹(TtEditorFavoritePaths)
    ///   [▼] 从收藏夹里挑一条写回属性(下拉里每条右侧的 x 可以移除该条)
    /// 这样"加入收藏夹"这个动作统一收在 Details 面板里, 不需要各编辑器各自开右键菜单。
    ///
    /// 用法:
    ///   // 生产端: 只读属性 + 只给收藏按钮 (例如骨架编辑器里当前选中骨骼的路径)
    ///   [Editor.Infrastructure.TtPGFavoritePath(Channel = TtEditorFavoritePaths.ChannelBone, AllowPick = false)]
    ///   public string BonePath => ...;
    ///
    ///   // 消费端: 可写属性 + 下拉挑选
    ///   [Editor.Infrastructure.TtPGFavoritePath(Channel = TtEditorFavoritePaths.ChannelBone, AllowAdd = false)]
    ///   public string TargetBonePath { get; set; }
    ///
    /// Channel 不写时按属性类型去 TtEditorFavoritePaths.RegisterTypeChannel 登记表里推导。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class TtPGFavoritePathAttribute : TtPGCustomValueEditorAttribute
    {
        /// <summary>显式指定通道名; 为空时按属性类型推导</summary>
        public string Channel = null;
        /// <summary>是否显示 [★] 加入收藏夹按钮</summary>
        public bool AllowAdd = true;
        /// <summary>是否显示 [▼] 从收藏夹挑选(只读属性下自动失效)</summary>
        public bool AllowPick = true;
        /// <summary>下拉里显示 Display 还是完整 Path; 默认显示 Display(骨骼名), hover 看完整路径</summary>
        public bool ShowFullPathInList = false;

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            var current = info.Value as string;
            newValue = current;

            var channel = TtEditorFavoritePaths.ResolveChannel(Channel, info.Type?.SystemType);
            bool changed = false;

            ImGuiAPI.PushID(info.Name ?? "FavoritePath");

            // 值本身: 只展示, 不在这里做文本编辑(路径由收藏夹/生产端给出, 手打容易写错)
            var display = string.IsNullOrEmpty(current) ? "(None)" : current;
            int buttonCount = (AllowAdd ? 1 : 0) + ((AllowPick && !info.Readonly) ? 1 : 0);
            var columnIndex = ImGuiAPI.TableGetColumnIndex();
            var avail = ImGuiAPI.GetColumnWidth(columnIndex) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
            var buttonWidth = ImGuiAPI.GetFrameHeight();
            var textWidth = avail - buttonCount * (buttonWidth + 4);
            if (textWidth < 40)
                textWidth = 40;

            ImGuiAPI.SetNextItemWidth(textWidth);
            ImGuiAPI.InputText("##FavoritePathValue", ref display, ImGuiInputTextFlags_.ImGuiInputTextFlags_ReadOnly);
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None) && !string.IsNullOrEmpty(current))
                EGui.Controls.CtrlUtility.DrawHelper(current);

            var btSize = new Vector2(buttonWidth, buttonWidth);

            if (AllowAdd)
            {
                ImGuiAPI.SameLine(0, 4);
                bool already = TtEditorFavoritePaths.Contains(channel, current);
                bool canAdd = !string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(current) && !already;
                // ImGuiAPI 没绑 BeginDisabled, 按现有写法用 "点了也不生效 + tooltip 说明原因" 处理
                if (ImGuiAPI.Button(already ? "*" : "+", in btSize) && canAdd)
                {
                    TtEditorFavoritePaths.Add(channel, current, MakeDisplay(current), info.Name);
                }
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    if (string.IsNullOrEmpty(channel))
                        EGui.Controls.CtrlUtility.DrawHelper("No favorite channel for this property");
                    else if (string.IsNullOrEmpty(current))
                        EGui.Controls.CtrlUtility.DrawHelper("Nothing to add");
                    else if (already)
                        EGui.Controls.CtrlUtility.DrawHelper($"Already in favorites [{channel}]");
                    else
                        EGui.Controls.CtrlUtility.DrawHelper($"Add to favorites [{channel}]");
                }
            }

            if (AllowPick && !info.Readonly)
            {
                ImGuiAPI.SameLine(0, 4);
                var list = TtEditorFavoritePaths.Get(channel);
                if (ImGuiAPI.Button("v", in btSize))
                    ImGuiAPI.OpenPopup("##FavoritePathList", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    EGui.Controls.CtrlUtility.DrawHelper($"Pick from favorites [{channel}] ({list.Count})");

                if (ImGuiAPI.BeginPopup("##FavoritePathList", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (list.Count == 0)
                    {
                        ImGuiAPI.TextDisabled("(favorites empty)");
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var entry = list[i];
                            var label = ShowFullPathInList ? entry.Path : entry.DisplayOrPath;
                            bool selected = entry.Path == current;
                            if (ImGuiAPI.Selectable(label + "##fav" + i, selected,
                                ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                            {
                                newValue = entry.Path;
                                changed = entry.Path != current;
                            }
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                EGui.Controls.CtrlUtility.DrawHelper(entry.Path);
                            ImGuiAPI.SameLine(0, 8);
                            if (ImGuiAPI.SmallButton("x##favdel" + i))
                            {
                                TtEditorFavoritePaths.Remove(channel, entry.Path);
                                break;
                            }
                        }
                    }
                    ImGuiAPI.EndPopup();
                }
            }

            ImGuiAPI.PopID();
            return changed;
        }

        /// <summary>
        /// 下拉里的短名: 取 ':' 后面那段(骨骼名这类), 没有分隔符就用原串
        /// </summary>
        static string MakeDisplay(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            var idx = path.LastIndexOf(':');
            if (idx < 0 || idx + 1 >= path.Length)
                return path;
            return path.Substring(idx + 1);
        }
    }
}
