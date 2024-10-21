﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public class ObjectWithCreateEditor : PGCustomValueEditorAttribute
    {
        EngineNS.EGui.UIProxy.ImageButtonProxy mImageButton;

        protected override async Task<bool> Initialize_Override()
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
            await mImageButton.Initialize();
            return await base.Initialize_Override();
        }

        public override bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            if (info.Readonly)
                ImGuiAPI.Text("null");
            else if (!info.Type.SystemType.IsSubclassOf(typeof(System.Array)) && info.Readonly == false)
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                var index = ImGuiAPI.TableGetColumnIndex();
                var width = Math.Min(ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X, 100.0f);
                mImageButton.Size = new Vector2(width, 0);// ImGuiAPI.GetFrameHeight());
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Button, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGColor);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonActive, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGActiveColor);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonHovered, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGHoverColor);
                if (mImageButton.OnDraw(in drawList, in Support.TtAnyPointer.Default))
                {
                    newValue = Rtti.TtTypeDescManager.CreateInstance(info.Type.SystemType);
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
            bool retValue = false;
            newValue = info.Value;
            ImGuiAPI.SetNextItemWidth(-1);
            ImGuiStyle* style = ImGuiAPI.GetStyle();
            var offsetY = (style->FramePadding.Y - EGui.UIProxy.StyleConfig.Instance.PGCheckboxFramePadding.Y);
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            cursorPos.Y += offsetY;
            ImGuiAPI.SetCursorScreenPos(in cursorPos);
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGCheckboxFramePadding);
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            if(multiValue != null)
            {
                int refVal = -1;
                if(multiValue.HasDifferentValue())
                    refVal = -1;
                else
                    refVal = (System.Convert.ToBoolean(multiValue.Values[0]))?1:0;

                retValue = EGui.UIProxy.CheckBox.DrawCheckBoxTristate(name, ref refVal, info.Readonly);//ImGuiAPI.CheckBoxTristate(name, ref refVal) && !info.Readonly;
                if(retValue)
                    newValue = (refVal > 0) ? true : false;
            }
            else
            {
                var v = (System.Convert.ToBoolean(info.Value));
                string errStr = null;
                retValue = EGui.UIProxy.CheckBox.DrawCheckBox(name, ref v, info.Readonly);//ImGuiAPI.Checkbox(name, ref v) && !info.Readonly;
                if(info.HostProperty.CustomValueEditor != null) 
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if(!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if(retValue && string.IsNullOrEmpty(errStr))
                    newValue = v;
            }
            //ImGuiAPI.PopStyleVar(1);
            return retValue;
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
            var minValue = sbyte.MinValue;
            var maxValue = sbyte.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if(multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToSByte(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToSByte(info.Value);
                float speed = 1.0f;
                if(info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if(vR != null)
                    {
                        minValue = (sbyte)vR.Min;
                        maxValue = (sbyte)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if(vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_S8, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                string errStr = null;
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (changed && !info.Readonly && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = Int16.MinValue;
            var maxValue = Int16.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if(multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToInt16(str);
                });
                if (changed)
                    retValue = true;
            }
            else
            {
                var v = System.Convert.ToInt16(info.Value);
                float speed = 1.0f;
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (Int16)vR.Min;
                        maxValue = (Int16)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                string errStr = null;
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_S16, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = Int32.MinValue;
            var maxValue = Int32.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if(multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToUInt32(str);
                });
                if (changed)
                    retValue = true;
            }
            else
            {
                var v = System.Convert.ToInt32(info.Value);
                float speed = 1.0f;
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (Int32)vR.Min;
                        maxValue = (Int32)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_S32, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                string errStr = null;
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = Int64.MinValue;
            var maxValue = Int64.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if(multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToInt64(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToInt64(info.Value);
                float speed = 1.0f;
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (Int64)vR.Min;
                        maxValue = (Int64)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_S64, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                string errStr = null;
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = byte.MinValue;
            var maxValue = byte.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToByte(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToByte(info.Value);
                float speed = 1.0f;
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (byte)vR.Min;
                        maxValue = (byte)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                string errStr = null;
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_U8, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                if(info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = UInt16.MinValue;
            var maxValue = UInt16.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToUInt16(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToUInt16(info.Value);
                float speed = 1.0f;
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (UInt16)vR.Min;
                        maxValue = (UInt16)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_U16, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                string errStr = null;
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
            var minValue = UInt32.MinValue;
            var maxValue = UInt32.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToUInt32(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToUInt32(info.Value);
                float speed = 1.0f;
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (UInt32)vR.Min;
                        maxValue = (UInt32)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_U32, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                string errStr = null;
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = UInt64.MinValue;
            var maxValue = UInt64.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToUInt64(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToUInt64(info.Value);
                float speed = 1.0f;
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (UInt64)vR.Min;
                        maxValue = (UInt64)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                }
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_U64, &v, speed, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                string errStr = null;
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = float.MinValue;
            var maxValue = float.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToSingle(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToSingle(info.Value);
                float speed = 1.0f;
                string format = "%.6f";
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (float)vR.Min;
                        maxValue = (float)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                    {
                        speed = vStep.Step;
                    }
                    var vFormat = info.HostProperty.GetAttribute<PGValueFormat>();
                    if(vFormat != null)
                    {
                        format = vFormat.Format;
                    }
                }
                string errStr = null;
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_Float, &v, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                if (info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
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
            var minValue = double.MinValue;
            var maxValue = double.MaxValue;
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            bool retValue = false;
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return System.Convert.ToDouble(str);
                });
                if (changed)
                {
                    retValue = true;
                }
            }
            else
            {
                var v = System.Convert.ToDouble(info.Value);
                float speed = 1.0f;
                string format = "%.6f";
                if (info.HostProperty != null)
                {
                    var vR = info.HostProperty.GetAttribute<PGValueRange>();
                    if (vR != null)
                    {
                        minValue = (double)vR.Min;
                        maxValue = (double)vR.Max;
                    }
                    var vStep = info.HostProperty.GetAttribute<PGValueChangeStep>();
                    if (vStep != null)
                        speed = vStep.Step;
                    var vFormat = info.HostProperty.GetAttribute<PGValueFormat>();
                    if (vFormat != null)
                        format = vFormat.Format;
                }
                string errStr = null;
                var changed = ImGuiAPI.DragScalar2(name, ImGuiDataType_.ImGuiDataType_Double, &v, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                if(info.HostProperty.CustomValueEditor != null)
                {
                    errStr = info.HostProperty.CustomValueEditor.GetErrorString(in info, v);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        var min = ImGuiAPI.GetItemRectMin();
                        var max = ImGuiAPI.GetItemRectMax();
                        var drawList = ImGuiAPI.GetWindowDrawList();
                        drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            ImGuiAPI.BeginTooltip();
                            ImGuiAPI.SetTooltip(errStr);
                            ImGuiAPI.EndTooltip();
                        }
                    }
                }
                if (!info.Readonly && changed && string.IsNullOrEmpty(errStr))
                {
                    newValue = v;
                    retValue = true;
                }
            }
            return retValue;
        }
    }
    public class StringEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            return OnDraw(info.HostProperty.CustomValueEditor, in info, out newValue);
        }
        public static unsafe bool OnDraw(PGCustomValueEditorAttribute attr, in EditorInfo info, out object newValue, 
            ImGuiInputTextFlags_ flags = ImGuiInputTextFlags_.ImGuiInputTextFlags_None, ImGuiAPI.FDelegate_ImGuiInputTextCallback callback = null)
        {
            bool valueChanged = false;
            newValue = info.Value;
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
            var name = TName.FromString2("##", info.Name).ToString();
            var multiValue = info.Value as PropertyMultiValue;
            if(multiValue != null && multiValue.HasDifferentValue())
            {
                var changed = multiValue.Draw(name, out newValue, info.Readonly, (str) =>
                {
                    return str;
                });
                if(changed)
                {
                    return true;
                }
            }
            else
            {
                var v = info.Value as string;
                
                {
                    //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
                    if (info.Readonly)
                        ImGuiAPI.Text(v);
                    else
                    {
                        string errStr = null;
                        var changed = ImGuiAPI.InputText(name, ref v, flags, callback);
                        if(attr != null)
                        {
                            errStr = attr.GetErrorString(in info, v);
                            if(!string.IsNullOrEmpty(errStr))
                            {
                                var min = ImGuiAPI.GetItemRectMin();
                                var max = ImGuiAPI.GetItemRectMax();
                                var drawList = ImGuiAPI.GetWindowDrawList();
                                drawList.AddRect(min, max, 0xff0000ff, 0.2f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

                                if(ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                {
                                    ImGuiAPI.BeginTooltip();
                                    ImGuiAPI.SetTooltip(errStr);
                                    ImGuiAPI.EndTooltip();
                                }
                            }
                        }
                        //ImGuiAPI.PopStyleVar(1);
                        //if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0 && !bReadonly)
                        if (changed && !info.Readonly && string.IsNullOrEmpty(errStr))
                        {
                            newValue = v;
                            valueChanged = true;
                            //prop.SetValue(ref target, v);
                        }
                    }
                }
            }
            return valueChanged;
        }
    }
    public class EnumEditor : PGCustomValueEditorAttribute
    {
        //EGui.UIProxy.ImageProxy mImage;
        EGui.UIProxy.ComboBox mComboBox;
        class DrawData
        {
            public Rtti.TtTypeDesc Type;
            public object Value;
            public object NewValue;
            public bool Readonly;
            public bool ValueChanged;
        }
        [System.ThreadStatic]
        static DrawData mDrawData = new DrawData();
        protected override async Task<bool> Initialize_Override()
        {
            mComboBox = new UIProxy.ComboBox()
            {
                ComboOpenAction = ComboOpenAction,
            };
            await mComboBox.Initialize();

            return await base.Initialize_Override();
        }

        protected override void Cleanup_Override()
        {
            mComboBox?.Cleanup();
            mComboBox = null;
            base.Cleanup_Override();
        }

        unsafe void ComboOpenAction(in Support.TtAnyPointer drawData)
        {
            var data = drawData.RefObject as DrawData;
            var propertyType = data.Type.SystemType;

            int item_current_idx = -1;
            var members = data.Type.SystemType.GetEnumNames();
            var values = data.Type.SystemType.GetEnumValues();
            var sz = new Vector2(0, 0);
            var attrs = propertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                UInt32 enumFlags = 0;
                var multiValue = data.Value as PropertyMultiValue;
                if (multiValue != null)
                {
                    if (!multiValue.HasDifferentValue())
                        enumFlags = System.Convert.ToUInt32(multiValue.Values[0]);
                }
                else
                    enumFlags = System.Convert.ToUInt32(data.Value);
                uint newFlags = 0;
                for (int i = 0; i < members.Length; i++)
                {
                    var m = values.GetValue(i).ToString();
                    var e_v = System.Enum.Parse(propertyType, m);
                    var v = System.Convert.ToUInt32(e_v);
                    var bSelected = (((enumFlags & v) == v) ? true : false);
                    bool bStylePushed = false;
                    if (bSelected)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PopupHoverColor);
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.ItemHightlightHoveredColor);
                        bStylePushed = true;
                    }
                    ImGuiAPI.Selectable(members[i], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_DontClosePopups, in sz);
                    if (bStylePushed)
                        ImGuiAPI.PopStyleColor(3);
                    if (bSelected)
                    {
                        newFlags |= v;
                    }
                    else
                    {
                        
                    }
                }
                if (newFlags != enumFlags && !data.Readonly)
                {
                    data.NewValue = System.Enum.ToObject(propertyType, newFlags);
                    data.ValueChanged = true;
                    //    prop.SetValue(ref target, newFlags);
                }
            }
            else
            {
                for (int j = 0; j < members.Length; j++)
                {
                    var bSelected = true;
                    if (members[j] == data.Value.ToString())
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PopupHoverColor);
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.ItemHightlightHoveredColor);
                    }

                    if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz))
                    {
                        item_current_idx = j;
                    }

                    if (members[j] == data.Value.ToString())
                        ImGuiAPI.PopStyleColor(3);
                }
                if (item_current_idx >= 0 && !data.Readonly)
                {
                    data.NewValue = values.GetValue(item_current_idx);
                    data.ValueChanged = true;
                    //prop.SetValue(ref target, v);
                }
            }
        }
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            if (mComboBox == null)
                return false;
            var drawList = ImGuiAPI.GetWindowDrawList();
            var index = ImGuiAPI.TableGetColumnIndex();
            mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_None | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton;
            mComboBox.Width = ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
            mComboBox.Name = TName.FromString2("##", info.Name).ToString();
            mComboBox.PreviewValue = info.Value.ToString();

            mDrawData.Type = info.Type;
            mDrawData.Value = info.Value;
            mDrawData.NewValue = info.Value;
            mDrawData.Readonly = info.Readonly;
            mDrawData.ValueChanged = false;
            Support.TtAnyPointer anyPointer = new Support.TtAnyPointer();
            anyPointer.RefObject = mDrawData;
            var winSize = new Vector2(mComboBox.Width, -1);
            ImGuiAPI.SetNextWindowSize(in winSize, ImGuiCond_.ImGuiCond_Always);
            mComboBox.OnDraw(in drawList, in anyPointer);
            newValue = mDrawData.NewValue;

            return mDrawData.ValueChanged;
        }
    }

    public class ArrayEditor : PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            var multiValue = newValue as PropertyMultiValue;
            if(multiValue != null)
            {
                ImGuiAPI.Text(multiValue.MultiValueString);
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
                IndentTextureId = (ulong)info.HostPropertyGrid.IndentDec.GetImagePtrPointer(),
                MinHeight = 0,
                CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                IndentImageWidth = info.HostPropertyGrid.Indent,
                IndentTextureUVMin = Vector2.Zero,
                IndentTextureUVMax = Vector2.One,
                IndentColor = info.HostPropertyGrid.IndentColor,
                HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
            };
            for (int i = 0; i < lst.Length; i++)
            {
                ImGuiAPI.TableNextRow(in rowData);

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
                    var vtype = Rtti.TtTypeDesc.TypeOf(obj.GetType());
                    var elementEditorInfo = new PGCustomValueEditorAttribute.EditorInfo()
                    {
                        Name = info.Name + i,
                        Type = vtype,
                        Value = obj,
                        Readonly = info.Readonly,
                        HostPropertyGrid = info.HostPropertyGrid,
                        Flags = info.Flags,
                        Expand = treeNodeRet,
                        HostProperty = info.HostProperty,
                    };
                    object newValue;
                    var changed = PropertyGrid.DrawPropertyGridItem(ref elementEditorInfo, out newValue);
                    if (changed && !info.Readonly)
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
        EGui.Controls.UTypeSelector mTypeSelector;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;
            var multiValue = newValue as PropertyMultiValue;
            if(multiValue != null)
            {
                ImGuiAPI.Text(multiValue.MultiValueString);
            }
            else
            {
                var listOpAtt = info.HostProperty.GetAttribute<PGListOperationCallbackAttribute>();
                var baseTypeAtt = info.HostProperty.GetAttribute<PGBaseType>();
                if (info.Readonly == false)
                {
                    //ImGuiAPI.SameLine(0, -1);
                    //var sz = new Vector2(0, 0);
                    //ImGuiAPI.PushID(info.Name);
                    //ImGuiAPI.SameLine(0, -1);
                    //ImGuiAPI.OpenPopupOnItemClick("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                    //var pos = ImGuiAPI.GetItemRectMin();
                    //var size = ImGuiAPI.GetItemRectSize();
                    //if (ImGuiAPI.ArrowButton("##OpenAddItemList", ImGuiDir.ImGuiDir_Down))
                    //{
                    //    ImGuiAPI.OpenPopup("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                    //}
                    //if (ImGuiAPI.BeginPopup("AddItem", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                    //{
                        var dict = info.Value as System.Collections.IList;
                        if (dict.GetType().GenericTypeArguments.Length == 1)
                        {
                            Type baseType = null;
                            if(baseTypeAtt != null)
                                baseType = baseTypeAtt.BaseType;
                            else
                                baseType = dict.GetType().GenericTypeArguments[0];
                            if(mTypeSelector == null)
                                mTypeSelector = new EGui.Controls.UTypeSelector();
                            mTypeSelector.CtrlId = "##ComboTypeSelector_" + info.Name;
                            mTypeSelector.BaseType = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(baseType.FullName);
                            mTypeSelector.OnDraw(150, 6);
                            if (mTypeSelector.SelectedType != null)
                            {
                                var newItem = Rtti.TtTypeDescManager.CreateInstance(mTypeSelector.SelectedType);
                                var idx = dict.Count;
                                listOpAtt?.OnPreInsert(idx, newItem, info.ObjectInstance);
                                dict.Insert(idx, newItem);
                                valueChanged = true;
                                listOpAtt?.OnAfterInsert(idx, newItem, info.ObjectInstance);
                                mTypeSelector.SelectedType = null;
                            }
                            ImGuiAPI.SameLine(0, -1);
                        }
                    //    ImGuiAPI.EndPopup();
                    //}
                    //ImGuiAPI.SameLine(0, -1);
                    //ImGuiAPI.PopID();
                }
                ImGuiAPI.Text(info.Type.ToString());
                //ImGuiAPI.SameLine(0, -1);
                //var showChild = ImGuiAPI.TreeNode(info.Name, "");
                //ImGuiAPI.NextColumn();
                //ImGuiAPI.Text(info.Type.ToString());
                //ImGuiAPI.NextColumn();
                if (info.Expand)
                {
                    var dict = info.Value as System.Collections.IList;
                    if (OnList(in info, dict, listOpAtt))
                        valueChanged = true;
                }
            }
            return valueChanged;
        }

        private unsafe bool OnList(in EditorInfo info, System.Collections.IList lst, PGListOperationCallbackAttribute listOpAtt)
        {
            bool itemChanged = false;
            var sz = new Vector2(0, 0);
            ImGuiTableRowData rowData = new ImGuiTableRowData()
            {
                IndentTextureId = (ulong)info.HostPropertyGrid.IndentDec.GetImagePtrPointer(),
                MinHeight = 0,
                CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                IndentImageWidth = info.HostPropertyGrid.Indent,
                IndentTextureUVMin = Vector2.Zero,
                IndentTextureUVMax = Vector2.One,
                IndentColor = info.HostPropertyGrid.IndentColor,
                HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
            };
            for (int i = 0; i < lst.Count; i++)
            {
                var name = "[" + i.ToString() + "]";
                var obj = lst[i];

                ImGuiAPI.AlignTextToFramePadding();
                ImGuiAPI.TableNextRow(in rowData);
                ImGuiAPI.TableSetColumnIndex(0);
                if (info.HostPropertyGrid.IsReadOnly == false)
                {
                    ImGuiAPI.Indent(5);
                    bool operated = false;
                    ImGuiAPI.PushID(TName.FromString2("##ListDel_", i.ToString()).ToString());
                    if (ImGuiAPI.Button("-", in sz))
                    {
                        //removeList.Add(i);
                        listOpAtt?.OnPreRemoveAt(i, info.ObjectInstance);
                        lst.RemoveAt(i);
                        operated = true;
                        itemChanged = true;
                        listOpAtt?.OnAfterRemoveAt(i, info.ObjectInstance);
                    }
                    ImGuiAPI.PopID();
                    if (operated)
                    {
                        ImGuiAPI.Unindent(5);
                        return true;
                    }

                    operated = false;
                    ImGuiAPI.SameLine(0, -1);
                    ImGuiAPI.PushID(TName.FromString2("##ListAdd_", i.ToString()).ToString());
                    if (ImGuiAPI.Button("+", in sz))
                    {
                        //addList.Add(new KeyValuePair<int, object>(i, obj));
                        var newItem = Rtti.TtTypeDescManager.CreateInstance(obj.GetType());
                        listOpAtt?.OnPreInsert(i, newItem, info.ObjectInstance);
                        lst.Insert(i, newItem);
                        itemChanged = true;
                        listOpAtt?.OnAfterInsert(i, newItem, info.ObjectInstance);
                    }
                    ImGuiAPI.PopID();
                    if (operated)
                    {
                        ImGuiAPI.Unindent(5);
                        return true;
                    }
                    ImGuiAPI.Unindent(5);
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
                        Type = Rtti.TtTypeDesc.TypeOf(obj.GetType()),
                        Value = obj,
                        Readonly = info.Readonly,
                        HostPropertyGrid = info.HostPropertyGrid,
                        Flags = info.Flags,
                        Expand = treeNodeRet,
                        HostProperty = info.HostProperty,
                    };
                    object newValue;
                    listOpAtt?.OnPreValueChanged(i, obj, info.ObjectInstance);
                    var changed = PropertyGrid.DrawPropertyGridItem(ref elementEditorInfo, out newValue);
                    if (changed && !info.Readonly)
                    {
                        lst[i] = newValue;
                        itemChanged = true;
                        listOpAtt?.OnAfterValueChanged(i, newValue, info.ObjectInstance);
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
            var multiValue = newValue as PropertyMultiValue;
            if(multiValue != null)
            {
                ImGuiAPI.Text(multiValue.MultiValueString);
            }
            else
            {
                if (info.Readonly == false)
                {
                    //ImGuiAPI.SameLine(0, -1);
                    //var sz = new Vector2(0, 0);
                    //ImGuiAPI.PushID(info.Name);
                    if (ImGuiAPI.ArrowButton("##OpenAddItemDict", ImGuiDir.ImGuiDir_Down))
                    {
                        //ImGuiAPI.PopID();
                        ImGuiAPI.OpenPopup("AddDictElement", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                    }
                    else
                    {
                        //ImGuiAPI.PopID();
                    }
                    ImGuiAPI.SameLine(0, -1);
                }

                {
                    Rtti.TtTypeDesc keyType = null, valueType = null;
                    var dict = info.Value as System.Collections.IDictionary;
                    if (dict.GetType().GenericTypeArguments.Length == 2)
                    {
                        keyType = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[0].FullName);
                        valueType = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[1].FullName);
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
                    ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_None);
                    mKVCreator.CreateFinished = false;
                    mKVCreator.OnDraw("AddDictElement");
                    if (mKVCreator.CreateFinished)
                    {
                        dict[mKVCreator.KeyData] = mKVCreator.ValueData;
                    }
                }

                //var showChild = ImGuiAPI.TreeNode(info.Name, "");
                //ImGuiAPI.NextColumn();
                ImGuiAPI.Text(info.Type.ToString());
                //ImGuiAPI.NextColumn();
                if (info.Expand)
                {
                    var dict = info.Value as System.Collections.IDictionary;
                    if (OnDictionary(in info, dict))
                        valueChanged = true;
                    //ImGuiAPI.TreePop();
                }
            }

            return valueChanged;
        }
        private unsafe bool OnDictionary(in EditorInfo info, System.Collections.IDictionary dict)
        {
            bool itemValueChanged = false;
            var sz = new Vector2(0, 0);
            var iter = dict.GetEnumerator();
            int idx = 0;
            ImGuiTableRowData rowData = new ImGuiTableRowData()
            {
                IndentTextureId = (ulong)info.HostPropertyGrid.IndentDec.GetImagePtrPointer(),
                MinHeight = 0,
                CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                IndentImageWidth = info.HostPropertyGrid.Indent,
                IndentTextureUVMin = Vector2.Zero,
                IndentTextureUVMax = Vector2.One,
                IndentColor = info.HostPropertyGrid.IndentColor,
                HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
            };
            Vector2 tempPadding = new Vector2(0, 0);
            while (iter.MoveNext())
            {
                var name = "[" + idx.ToString() + "]";// iter.Key.ToString();
                ImGuiAPI.AlignTextToFramePadding();
                ImGuiAPI.TableNextRow(in rowData);
                ImGuiAPI.TableSetColumnIndex(0);
                if (info.HostPropertyGrid.IsReadOnly == false)
                {
                    bool operated = false;
                    ImGuiAPI.Indent(5);
                    ImGuiAPI.PushID(TName.FromString2("##ListDel_", name).ToString());
                    if (ImGuiAPI.Button("-", in sz))
                    {
                        //removeList.Add(iter.Key);
                        dict.Remove(iter.Key);
                        operated = true;
                    }
                    ImGuiAPI.PopID();
                    ImGuiAPI.Unindent(5);
                    if (operated)
                        return true;
                    ImGuiAPI.SameLine(0, -1);
                }
                var flags = info.Flags;
                if (PropertyGrid.IsLeafTreeNode(iter.Value, null))
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in tempPadding);
                var treeNodeRet = ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.PopStyleVar(1);
                ImGuiAPI.SameLine(0, 5);
                var keyEditorInfo = new EditorInfo()
                {
                    Name = iter.Key.ToString() + idx,
                    Type = Rtti.TtTypeDesc.TypeOf(iter.Key.GetType()),
                    Value = iter.Key,
                    Readonly = info.Readonly,
                    HostPropertyGrid = info.HostPropertyGrid,
                    Flags = info.Flags,
                    Expand = treeNodeRet,
                    HostProperty = info.HostProperty,
                };
                object newKeyValue;
                if(PropertyGrid.DrawPropertyGridItem(ref keyEditorInfo, out newKeyValue))
                {
                    dict[newKeyValue] = iter.Value;
                    dict.Remove(iter.Key);

                    if (treeNodeRet)
                        ImGuiAPI.TreePop();

                    return true;
                }
                //ImGuiAPI.NextColumn();
                ImGuiAPI.TableSetColumnIndex(1);
                ImGuiAPI.SetNextItemWidth(-1);
                Rtti.TtTypeDesc valueType = null;
                if (iter.Value != null)
                    valueType = Rtti.TtTypeDesc.TypeOf(iter.Value.GetType());
                var valueEditorInfo = new EditorInfo()
                {
                    Name = iter.Value.ToString() + idx,
                    Type = valueType,
                    Value = iter.Value,
                    Readonly = info.Readonly,
                    HostPropertyGrid = info.HostPropertyGrid,
                    Flags = info.Flags,
                    Expand = treeNodeRet,
                    HostProperty = info.HostProperty,
                };
                object newValue;
                if(PropertyGrid.DrawPropertyGridItem(ref valueEditorInfo, out newValue) && !info.Readonly)
                {
                    dict[iter.Key] = newValue;
                    itemValueChanged = true;
                }

                if (treeNodeRet)
                    ImGuiAPI.TreePop();

                idx++;
            }

            return itemValueChanged;
        }
    }
}