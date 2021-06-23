using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public class ObjectWithCreateEditor : PGCustomValueEditorAttribute
    {
        EngineNS.EGui.UIProxy.ImageButtonProxy mImageButton;

        public ObjectWithCreateEditor()
        {
            mImageButton = new UIProxy.ImageButtonProxy()
            {
                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                Size = new Vector2(0, 0),
                UVMin = new Vector2(299.0f / 1024, 4.0f / 1024),
                UVMax = new Vector2(315.0f / 1024, 20.0f / 1024),
                ImageSize = new Vector2(16, 16),
                ShowBG = true,
                ImageColor = 0xFFFFFFFF,
            };
        }

        public override bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            if (!info.Type.SystemType.IsSubclassOf(typeof(System.Array)) && info.Readonly == false)
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                var index = ImGuiAPI.TableGetColumnIndex();
                var width = Math.Min(ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X, 100.0f);
                mImageButton.Size = new Vector2(width, 0);// ImGuiAPI.GetFrameHeight());
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Button, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGColor);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonActive, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGActiveColor);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonHovered, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGHoverColor);
                if (mImageButton.OnDraw(ref drawList))
                {
                    newValue = System.Activator.CreateInstance(info.Type.SystemType);
                    valueChanged = true;
                    //prop.SetValue(ref target, v);
                }
                ImGuiAPI.PopStyleColor(3);
            }
            return valueChanged;
        }
    }

    public class BoolEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            ImGuiAPI.SetNextItemWidth(-1);
            var v = (System.Convert.ToBoolean(info.Value));
            var saved = v;
            ImGuiStyle* style = ImGuiAPI.GetStyle();
            var offsetY = (style->FramePadding.Y - EGui.UIProxy.StyleConfig.Instance.PGCheckboxFramePadding.Y);
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            cursorPos.Y += offsetY;
            ImGuiAPI.SetCursorScreenPos(ref cursorPos);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGCheckboxFramePadding);
            ImGuiAPI.Checkbox(TName.FromString2("##", info.Name).ToString(), ref v);
            newValue = v;
            ImGuiAPI.PopStyleVar(1);
            return (v != saved);
            //if (v != saved)
            //{
            //    prop.SetValue(ref target, v);
            //}
        }

    }
    public class SByteEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.GetColumnIndex();//ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToSByte(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = sbyte.MinValue;
            var maxValue = sbyte.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_S8, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class Int16Editor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToInt16(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = Int16.MinValue;
            var maxValue = Int16.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_S16, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class Int32Editor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToInt32(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = Int32.MinValue;
            var maxValue = Int32.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_S32, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class Int64Editor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToInt64(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = Int64.MinValue;
            var maxValue = Int64.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_S64, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class ByteEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToByte(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = byte.MinValue;
            var maxValue = byte.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_U8, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class UInt16Editor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToUInt16(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = UInt16.MinValue;
            var maxValue = UInt16.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_U16, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class UInt32Editor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToUInt32(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = UInt32.MinValue;
            var maxValue = UInt32.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_U32, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class UInt64Editor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToUInt64(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = UInt64.MinValue;
            var maxValue = UInt64.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_U64, &v, 1.0f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class FloatEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToSingle(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            //ImGuiAPI.InputFloat(TName.FromString2("##", info.Name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
            var minValue = float.MinValue;
            var maxValue = float.MaxValue;
            var changed = ImGuiAPI.DragScalar2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, &v, 0.1f, &minValue, &maxValue, "%.6f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class DoubleEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = System.Convert.ToDouble(info.Value);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = double.MinValue;
            var maxValue = double.MaxValue;
            var changed = ImGuiAPI.DragScalar2(("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Double, &v, .1f, &minValue, &maxValue, "%.6f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
            //ImGuiAPI.InputFloat(TName.FromString2("##", info.Name).ToString(), ref (*(float*)&v), 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
            //ImGuiAPI.PopStyleVar(1);
            if (changed)
            {
                newValue = v;
                return true;
            }
            return false;
        }
    }
    public class StringEditor : PGCustomValueEditorAttribute
    {
        byte[] TextBuffer = new byte[1024 * 4];

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var v = info.Value as string;
            var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
            fixed (byte* pBuffer = &TextBuffer[0])
            {
                CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
                //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
                ImGuiAPI.InputText(TName.FromString2("##", info.Name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                //ImGuiAPI.PopStyleVar(1);
                if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
                {
                    newValue = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                    valueChanged = true;
                    //prop.SetValue(ref target, v);
                }
                System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
            }
            return valueChanged;
        }
    }
    public class EnumEditor : PGCustomValueEditorAttribute
    {
        EGui.UIProxy.ImageProxy mImage;
        public EnumEditor()
        {
            mImage = new UIProxy.ImageProxy()
            {
                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                ImageSize = new Vector2(16, 16),
                UVMin = new Vector2(543.0f / 1024, 3.0f / 1024),
                UVMax = new Vector2(559.0f / 1024, 19.0f / 1024),
            };
        }

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
            var drawList = ImGuiAPI.GetWindowDrawList();
            ImGuiAPI.SetNextItemWidth(width);
            var propertyType = info.Value.GetType();
            var attrs1 = propertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);

            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, EGui.UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, ref EGui.UIProxy.StyleConfig.Instance.PopupWindowsPadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, ref EGui.UIProxy.StyleConfig.Instance.PopupItemSpacing);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_PopupBorderSize, EGui.UIProxy.StyleConfig.Instance.PopupBordersize);
            var style = ImGuiAPI.GetStyle();
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var label = TName.FromString2("##", info.Name).ToString();
            var label_size = ImGuiAPI.CalcTextSize(label, true, -1);
            var endPos = cursorPos + new Vector2(width + ((label_size.X > 0.0f) ? (style->ItemInnerSpacing.X + label_size.X) : 0.0f), label_size.Y + style->FramePadding.Y * 2.0f);
            var hovered = ImGuiAPI.IsMouseHoveringRectInCurrentWindow(ref cursorPos, ref endPos, true);
            if(hovered)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, EGui.UIProxy.StyleConfig.Instance.PGItemBorderHoveredColor);
            var comboOpen = ImGuiAPI.BeginCombo(label, info.Value.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton);
            if (hovered)
                ImGuiAPI.PopStyleColor(1);
            var itemSize = ImGuiAPI.GetItemRectSize();
            var pos = cursorPos + new Vector2(itemSize.X - mImage.ImageSize.X - style->FramePadding.X, style->FramePadding.Y * 0.5f);// cursorPos + new Vector2(0, fontSize * 0.134f * 0.5f);// + new Vector2(itemRectMax.X - mImage.ImageSize.X, fontSize * 0.134f * 0.5f);
            mImage.OnDraw(ref drawList, ref pos);
            if (comboOpen)
            {
                int item_current_idx = -1;
                var members = info.Type.SystemType.GetEnumNames();
                var values = info.Type.SystemType.GetEnumValues();
                var sz = new Vector2(0, 0);
                if (attrs1 != null && attrs1.Length > 0)
                {
                    var enumFlags = System.Convert.ToUInt32(info.Value);
                    uint newFlags = 0;
                    for (int i = 0; i<members.Length; i++)
                    {
                        var m = values.GetValue(i).ToString();
                        var e_v = System.Enum.Parse(propertyType, m);
                        var v = System.Convert.ToUInt32(e_v);
                        var bSelected = (((enumFlags & v) == v) ? true : false);
                        bool bStylePushed = false;
                        if(bSelected)
                        {
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PopupHoverColor);
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.ItemHightlightHoveredColor);
                            bStylePushed = true;
                        }
                        ImGuiAPI.Selectable(members[i], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_DontClosePopups, ref sz);
                        if (bStylePushed)
                            ImGuiAPI.PopStyleColor(3);
                        if (bSelected)
                        {
                            newFlags |= v;
                        }
                        else
                        {
                            newFlags &= ~v;
                        }
                    }
                    if (newFlags != enumFlags)
                    {
                        newValue = System.Enum.ToObject(propertyType, newFlags);
                        valueChanged = true;
                        //    prop.SetValue(ref target, newFlags);
                    }
                }
                else
                {
                    for (int j = 0; j < members.Length; j++)
                    {
                        var bSelected = true;
                        if (members[j] == info.Value.ToString())
                        {
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PopupHoverColor);
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.ItemHightlightHoveredColor);
                        }

                        if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
                        {
                            item_current_idx = j;
                        }

                        if (members[j] == info.Value.ToString())
                            ImGuiAPI.PopStyleColor(3);
                    }
                    if (item_current_idx >= 0)
                    {
                        newValue = (int)values.GetValue(item_current_idx);
                        valueChanged = true;
                        //prop.SetValue(ref target, v);
                    }
                }

                ImGuiAPI.EndCombo();
            }
            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.PopStyleVar(3);

            return valueChanged;
        }
    }

    public class ArrayEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            if(newValue is PropertyMultiValue)
            {
                ImGuiAPI.Text(newValue.ToString());
            }
            else
            {
                ImGuiAPI.Text(info.Type.ToString());
                if (info.Expand)
                {
                    var lst = info.Value as System.Array;
                    if(OnArray(in info, lst))
                    {
                        valueChanged = true;
                    }
                }
            }

            return valueChanged;
        }

        private unsafe bool OnArray(in EditorInfo info, System.Array lst)
        {
            bool valueChanged = false;
            var sz = new Vector2(0, 0);
            ImGuiTableRowData rowData = new ImGuiTableRowData()
            {
                IndentTextureId = info.HostPropertyGrid.IndentDec.GetImagePtrPointer(),
                MinHeight = 0,
                CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                IndentImageWidth = info.HostPropertyGrid.Indent,
                IndentTextureUVMin = Vector2.Zero,
                IndentTextureUVMax = Vector2.UnitXY,
                IndentColor = info.HostPropertyGrid.IndentColor,
                HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
            };
            for (int i = 0; i < lst.Length; i++)
            {
                ImGuiAPI.TableNextRow(ref rowData);

                var name = "[" + i.ToString() + "]";
                var obj = lst.GetValue(i);

                ImGuiAPI.TableSetColumnIndex(0);
                ImGuiAPI.PushID(info.Name);
                ImGuiAPI.AlignTextToFramePadding();
                var flags = info.Flags;
                if (PropertyGrid.IsLeafTreeNode(obj, null))
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                var treeNodeRet = ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.TableNextColumn();
                ImGuiAPI.SetNextItemWidth(-1);

                if (obj == null)
                {
                    ImGuiAPI.Text("null");
                }
                else
                {
                    var vtype = Rtti.UTypeDesc.TypeOf(obj.GetType());
                    var elementEditorInfo = new PGCustomValueEditorAttribute.EditorInfo()
                    {
                        Name = info.Name + i,
                        Type = vtype,
                        Value = obj,
                        Readonly = false,
                        HostPropertyGrid = info.HostPropertyGrid,
                        Flags = info.Flags,
                    };
                    object newValue;
                    var changed = PropertyGrid.DrawPropertyGridItem(ref elementEditorInfo, out newValue);
                    if (changed)
                    {
                        lst.SetValue(newValue, i);
                        valueChanged = true;
                    }
                }
                if(treeNodeRet)
                    ImGuiAPI.TreePop();

                ImGuiAPI.PopID();
            }
            return valueChanged;
        }
    }

    public class ListEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            if (info.HostPropertyGrid.IsReadOnly == false)
            {
                //ImGuiAPI.SameLine(0, -1);
                //var sz = new Vector2(0, 0);
                //ImGuiAPI.PushID(info.Name);
                //ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.OpenPopupOnItemClick("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                //var pos = ImGuiAPI.GetItemRectMin();
                //var size = ImGuiAPI.GetItemRectSize();
                if (ImGuiAPI.ArrowButton("##OpenAddItemList", ImGuiDir_.ImGuiDir_Down))
                {
                    ImGuiAPI.OpenPopup("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                }
                if (ImGuiAPI.BeginPopup("AddItem", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                {
                    var dict = info.Value as System.Collections.IList;
                    if (dict.GetType().GenericTypeArguments.Length == 1)
                    {
                        var listElementType = dict.GetType().GenericTypeArguments[0];
                        var typeSlt = new EGui.Controls.TypeSelector();
                        typeSlt.BaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(listElementType.FullName);
                        typeSlt.OnDraw(150, 6);
                        if (typeSlt.SelectedType != null)
                        {
                            var newItem = System.Activator.CreateInstance(listElementType);
                            dict.Insert(dict.Count, newItem);
                        }
                    }
                    ImGuiAPI.EndPopup();
                }
                //ImGuiAPI.PopID();
            }
            //ImGuiAPI.SameLine(0, -1);
            //var showChild = ImGuiAPI.TreeNode(info.Name, "");
            //ImGuiAPI.NextColumn();
            //ImGuiAPI.Text(info.Type.ToString());
            //ImGuiAPI.NextColumn();
            if (info.Expand)
            {
                var dict = info.Value as System.Collections.IList;
                if (OnList(in info, dict))
                    valueChanged = true;
            }
            return valueChanged;
        }

        private unsafe bool OnList(in EditorInfo info, System.Collections.IList lst)
        {
            bool itemChanged = false;
            var sz = new Vector2(0, 0);
            ImGuiTableRowData rowData = new ImGuiTableRowData()
            {
                IndentTextureId = info.HostPropertyGrid.IndentDec.GetImagePtrPointer(),
                MinHeight = 0,
                CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                IndentImageWidth = info.HostPropertyGrid.Indent,
                IndentTextureUVMin = Vector2.Zero,
                IndentTextureUVMax = Vector2.UnitXY,
                IndentColor = info.HostPropertyGrid.IndentColor,
                HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
            };
            for (int i = 0; i < lst.Count; i++)
            {
                var name = "[" + i.ToString() + "]";
                var obj = lst[i];

                ImGuiAPI.AlignTextToFramePadding();
                ImGuiAPI.TableNextRow(ref rowData);
                ImGuiAPI.TableSetColumnIndex(0);
                if (info.HostPropertyGrid.IsReadOnly == false)
                {
                    bool operated = false;
                    ImGuiAPI.PushID(TName.FromString2("##ListDel_", i.ToString()).ToString());
                    if (ImGuiAPI.Button("-", ref sz))
                    {
                        //removeList.Add(i);
                        lst.RemoveAt(i);
                        operated = true;
                    }
                    ImGuiAPI.PopID();
                    if (operated)
                        return true;

                    operated = false;
                    ImGuiAPI.SameLine(0, -1);
                    ImGuiAPI.PushID(TName.FromString2("##ListAdd_", i.ToString()).ToString());
                    if (ImGuiAPI.Button("+", ref sz))
                    {
                        //addList.Add(new KeyValuePair<int, object>(i, obj));
                        lst.Insert(i, obj);
                    }
                    ImGuiAPI.PopID();
                    if (operated)
                        return true;
                    ImGuiAPI.SameLine(0, -1);
                }
                var flags = info.Flags;
                if (PropertyGrid.IsLeafTreeNode(obj, null))
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                var treeNodeRet = ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.TableSetColumnIndex(1);
                ImGuiAPI.SetNextItemWidth(-1);

                if (obj == null)
                {
                    ImGuiAPI.Text("null");
                }
                else
                {
                    var elementEditorInfo = new EditorInfo()
                    {
                        Name = info.Name + i,
                        Type = Rtti.UTypeDesc.TypeOf(obj.GetType()),
                        Value = obj,
                        Readonly = false,
                        HostPropertyGrid = info.HostPropertyGrid,
                        Flags = info.Flags,
                    };
                    object newValue;
                    var changed = PropertyGrid.DrawPropertyGridItem(ref elementEditorInfo, out newValue);
                    if (changed)
                    {
                        lst[i] = newValue;
                        itemChanged = true;
                    }
                }
                if (treeNodeRet)
                    ImGuiAPI.TreePop();
            }

            return itemChanged;
        }

    }

    public class DictionaryEditor : PGCustomValueEditorAttribute
    {
        KeyValueCreator mKVCreator = null;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            if (info.HostPropertyGrid.IsReadOnly == false)
            {
                ImGuiAPI.SameLine(0, -1);
                var sz = new Vector2(0, 0);
                ImGuiAPI.PushID(info.Name);
                if (ImGuiAPI.ArrowButton("##OpenAddItemDict", ImGuiDir_.ImGuiDir_Down))
                {
                    ImGuiAPI.PopID();
                    ImGuiAPI.OpenPopup("AddDictElement", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                }
                else
                {
                    ImGuiAPI.PopID();
                }
            }

            {
                Rtti.UTypeDesc keyType = null, valueType = null;
                var dict = info.Value as System.Collections.IDictionary;
                if (dict.GetType().GenericTypeArguments.Length == 2)
                {
                    keyType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[0].FullName);
                    valueType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[1].FullName);
                }
                if (mKVCreator == null)
                {
                    mKVCreator = new KeyValueCreator();
                }
                if (mKVCreator.CtrlName != info.Name)
                {
                    mKVCreator.CtrlName = info.Name;
                    mKVCreator.KeyTypeSlt.BaseType = keyType;
                    mKVCreator.ValueTypeSlt.BaseType = valueType;
                }

                var size = new Vector2(300, 500);
                ImGuiAPI.SetNextWindowSize(ref size, ImGuiCond_.ImGuiCond_None);
                mKVCreator.CreateFinished = false;
                mKVCreator.OnDraw("AddDictElement");
                if (mKVCreator.CreateFinished)
                {
                    dict[mKVCreator.KeyData] = mKVCreator.ValueData;
                }
            }

            ImGuiAPI.SameLine(0, -1);
            var showChild = ImGuiAPI.TreeNode(info.Name, "");
            ImGuiAPI.NextColumn();
            ImGuiAPI.Text(info.Type.ToString());
            ImGuiAPI.NextColumn();
            if (showChild)
            {
                var dict = info.Value as System.Collections.IDictionary;
                if (OnDictionary(in info, dict))
                    valueChanged = true;
                ImGuiAPI.TreePop();
            }

            return valueChanged;
        }
        private unsafe bool OnDictionary(in EditorInfo info, System.Collections.IDictionary dict)
        {
            bool itemValueChanged = false;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            var sz = new Vector2(0, 0);
            var iter = dict.GetEnumerator();
            int idx = 0;
            while (iter.MoveNext())
            {
                var name = iter.Key.ToString();
                ImGuiAPI.AlignTextToFramePadding();
                if (info.HostPropertyGrid.IsReadOnly == false)
                {
                    bool operated = false;
                    ImGuiAPI.PushID(TName.FromString2("##ListDel_", name).ToString());
                    if (ImGuiAPI.Button("-", ref sz))
                    {
                        //removeList.Add(iter.Key);
                        dict.Remove(iter.Key);
                        operated = true;
                    }
                    ImGuiAPI.PopID();
                    if (operated)
                        return true;
                    ImGuiAPI.SameLine(0, -1);
                }
                ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.SameLine(0, -1);
                var keyEditorInfo = new EditorInfo()
                {
                    Name = iter.Key.ToString() + idx,
                    Type = Rtti.UTypeDesc.TypeOf(iter.Key.GetType()),
                    Value = iter.Key,
                    Readonly = false,
                    HostPropertyGrid = info.HostPropertyGrid,
                    Flags = info.Flags,
                };
                object newKeyValue;
                if(PropertyGrid.DrawPropertyGridItem(ref keyEditorInfo, out newKeyValue))
                {
                    dict[newKeyValue] = iter.Value;
                    dict.Remove(iter.Key);
                    return true;
                }
                ImGuiAPI.NextColumn();
                ImGuiAPI.SetNextItemWidth(-1);
                Rtti.UTypeDesc valueType = null;
                if (iter.Value != null)
                    valueType = Rtti.UTypeDesc.TypeOf(iter.Value.GetType());
                var valueEditorInfo = new EditorInfo()
                {
                    Name = iter.Value.ToString() + idx,
                    Type = valueType,
                    Value = iter.Value,
                    Readonly = false,
                    HostPropertyGrid = info.HostPropertyGrid,
                    Flags = info.Flags,
                };
                object newValue;
                if(PropertyGrid.DrawPropertyGridItem(ref valueEditorInfo, out newValue))
                {
                    dict[iter.Key] = newValue;
                    itemValueChanged = true;
                }

                idx++;
            }

            return itemValueChanged;
        }
    }
}