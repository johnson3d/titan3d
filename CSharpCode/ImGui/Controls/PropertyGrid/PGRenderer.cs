using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    partial class PropertyGrid : IPanel
    {
        public bool IsReadOnly = false;
        private bool mVisible = true;
        public bool Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = value;
            }
        }
        public uint DockId { get; set; } = uint.MaxValue;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public string PGName
        {
            get;
            set;
        } = "Property Grid";
        //byte[] TextBuffer = new byte[1024 * 4];
        //List<KeyValuePair<object, System.Reflection.PropertyInfo>> mCallstack = new List<KeyValuePair<object, System.Reflection.PropertyInfo>>();

        public string SearchInfo
        {
            get
            {
                if (mSearchBar != null)
                    return mSearchBar.InfoText.ToLower();
                return "";
            }
            set
            {
                if (mSearchBar != null)
                    mSearchBar.InfoText = value;
            }
        }
        public string FilterString
        {
            get
            {
                if (mSearchBar != null)
                    return mSearchBar.SearchText.ToLower();
                return "";
            }
            set
            {
                if (mSearchBar != null)
                    mSearchBar.SearchText = value;
            }
        }

        EGui.UIProxy.SearchBarProxy mSearchBar;
        EGui.UIProxy.ImageButtonProxy mOpenInPropertyMatrix;
        EGui.UIProxy.ImageButtonProxy mConfig;
        EGui.UIProxy.ImageButtonProxy mDelete;
        public EGui.UIProxy.ImageProxy IndentDec;
        EGui.UIProxy.ImageProxy mDropShadowDec;

        public void Cleanup()
        {
            mSearchBar?.Cleanup();
            mSearchBar = null;
            mOpenInPropertyMatrix?.Cleanup();
            mOpenInPropertyMatrix = null;
            mConfig?.Cleanup();
            mConfig = null;
            mDelete?.Cleanup();
            mDelete = null;
            IndentDec?.Dispose();
            IndentDec = null;
            mDropShadowDec?.Dispose();
            foreach(var tag in mDrawTargetDic.Values)
            {
                foreach(var proDesc in tag.Values)
                    proDesc.Cleanup();
            }
            mDrawTargetDic.Clear();
        }

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            if(mSearchBar == null)
            {
                mSearchBar = new UIProxy.SearchBarProxy();
                await mSearchBar.Initialize();
                mSearchBar.InfoText = "Search Details";
            }
            if(mOpenInPropertyMatrix == null)
            {
                mOpenInPropertyMatrix = new UIProxy.ImageButtonProxy()
                {
                    ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                    Size = new Vector2(20, 20),
                    UVMin = new Vector2(521.0f / 1024, 4.0f / 1024),
                    UVMax = new Vector2(537.0f / 1024, 20.0f / 1024),
                    ImageSize = new Vector2(16, 16),
                };
            }
            if(mConfig == null)
            {
                mConfig = new UIProxy.ImageButtonProxy()
                {
                    ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                    Size = new Vector2(20, 20),
                    UVMin = new Vector2(3.0f / 1024, 691.0f / 1024),
                    UVMax = new Vector2(19.0f / 1024, 707.0f / 1024),
                    ImageSize = new Vector2(16, 16),
                };
            }
            if(mDelete == null)
            {
                mDelete = new UIProxy.ImageButtonProxy()
                {
                    ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                    Size = new Vector2(33, 0),
                    UVMin = new Vector2(539.0f / 1024, 21.0f / 1024),
                    UVMax = new Vector2(555.0f / 1024, 37.0f / 1024),
                    ImageSize = new Vector2(16, 16),
                    ShowBG = true,
                    ImageColor = 0xFFFFFFFF,
                };
            }
            if(IndentDec == null)
            {
                IndentDec = new UIProxy.ImageProxy()
                {
                    ImageFile = RName.GetRName("icons/indentdec.srv", RName.ERNameType.Engine),
                    ImageSize = new Vector2(32, 2),
                    UVMin = Vector2.Zero,
                    UVMax = Vector2.UnitXY,
                };
            }
            if(mDropShadowDec == null)
            {
                mDropShadowDec = new UIProxy.ImageProxy()
                {
                    ImageFile = RName.GetRName("icons/shadowtop.srv", RName.ERNameType.Engine),
                    ImageSize = new Vector2(32, 2),
                    UVMin = Vector2.Zero,
                    UVMax = Vector2.UnitXY,
                };
            }

            return true;
        }
        
        public void OnDraw()
        {
            OnDraw(false, true, false);
        }
        public void OnDraw(bool bShowReadOnly, bool bNewForm/*=true*/, bool bKeepColums/*=false*/)
        {
            if (Visible == false)
                return;
            //mCallstack.Clear();
            //mCallstack.Add(new KeyValuePair<object, System.Reflection.PropertyInfo>(null, null));

            if (bNewForm)
            {
                if (ImGuiAPI.Begin($"{PGName}", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                //var sz = new Vector2(-1);
                //if (ImGuiAPI.BeginChild($"{PGName}", ref sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var winPos = ImGuiAPI.GetWindowPos();
                    var winSize = ImGuiAPI.GetWindowSize();
                    OnDrawContent(bShowReadOnly, in winPos, in winSize, bKeepColums);
                }
                //ImGuiAPI.EndChild();
                ImGuiAPI.End();
            }
            else
            {
                var winPos = ImGuiAPI.GetWindowPos();
                var winSize = ImGuiAPI.GetWindowSize();
                var sz = new Vector2(-1);
                if(ImGuiAPI.BeginChild($"{PGName}", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    OnDrawContent(bShowReadOnly, in winPos, in winSize, bKeepColums);
                }
                ImGuiAPI.EndChild();
            }
        }

        private unsafe void OnDrawHeadBar(ref ImDrawList drawList)
        {
            var winWidth = ImGuiAPI.GetWindowWidth();
            ImGuiAPI.BeginGroup();
            if (mSearchBar != null)
            {
                var indentValue = 8.0f;
                mSearchBar.Width = winWidth - indentValue * 2 - mOpenInPropertyMatrix.Size.X - mOpenInPropertyMatrix.Size.Y - 8 * 2;
                ImGuiAPI.Indent(indentValue);
                mSearchBar.OnDraw(ref drawList, ref Support.UAnyPointer.Default);
                ImGuiAPI.Unindent(indentValue);
            }
            if (mOpenInPropertyMatrix != null)
            {
                ImGuiAPI.SameLine(0, -1);
                var size = ImGuiAPI.GetItemRectSize();
                var curPos = ImGuiAPI.GetCursorScreenPos();
                Vector2 offset = Vector2.Zero;
                curPos += new Vector2(0, (size.Y - mOpenInPropertyMatrix.Size.Y) * 0.5f);
                ImGuiAPI.SetCursorScreenPos(in curPos);
                mOpenInPropertyMatrix.OnDraw(ref drawList, ref Support.UAnyPointer.Default);
            }
            if (mConfig != null)
            {
                ImGuiAPI.SameLine(0, -1);
                var size = ImGuiAPI.GetItemRectSize();
                var curPos = ImGuiAPI.GetCursorScreenPos();
                Vector2 offset = Vector2.Zero;
                curPos += new Vector2(0, (size.Y - mOpenInPropertyMatrix.Size.Y) * 0.5f);
                ImGuiAPI.SetCursorScreenPos(in curPos);
                mConfig.OnDraw(ref drawList, ref Support.UAnyPointer.Default);
            }
            ImGuiAPI.EndGroup();
        }
        private void OnDrawContent(bool bShowReadOnly, in Vector2 drawPos, in Vector2 drawSize, bool bKeepColums = false)
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            var posMin = drawPos;//ImGuiAPI.GetCursorPos(); //ImGuiAPI.GetWindowPos();
            var posMax = posMin + drawSize;// ImGuiAPI.GetWindowSize();
            drawList.AddRectFilled(in posMin, in posMax, EGui.UIProxy.StyleConfig.Instance.PanelBackground, 0.0f, ImDrawFlags_.ImDrawFlags_None);

            if (ImGuiAPI.IsWindowDocked())
            {
                DockId = ImGuiAPI.GetWindowDockID();
            }
            if (bShowReadOnly)
            {
                //var base_flags = (int)(ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | 
                //    ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick | 
                //    ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanAvailWidth);
                //ImGuiAPI.CheckboxFlags("IsReadOnly", ref base_flags, (int)ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed);
                //ImGuiAPI.CheckboxFlags("IsReadOnly1", ref base_flags, (int)ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_FramePadding);
                //ImGuiAPI.Checkbox("IsReadOnly", ref IsReadOnly);
                EGui.UIProxy.CheckBox.DrawCheckBox("IsReadOnly", ref IsReadOnly, false);
            }

            if (bKeepColums == false)
            {
                //ImGuiAPI.Text($"{Target?.ToString()}");
                OnDrawHeadBar(ref drawList);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in EGui.UIProxy.StyleConfig.Instance.PGNormalFramePadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameBorderSize, EGui.UIProxy.StyleConfig.Instance.PGNormalFrameBorderSize);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameRounding, EGui.UIProxy.StyleConfig.Instance.PGNormalFrameRounding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in EGui.UIProxy.StyleConfig.Instance.PGNormalItemSpacing);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor);
                ImGuiAPI.Separator();

                UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)Slate.UBaseRenderer.enFont.Font_13px);

                Vector2 size = Vector2.Zero;
                if(ImGuiAPI.BeginChild($"{PGName}_Properties", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    Vector2 outerSize = Vector2.Zero;
                    //if (ImGuiAPI.BeginTable("PGTable", 2, mTabFlags, ref outerSize, 0.0f))
                    //{
                    //    ImGuiAPI.TableSetupColumn(TName.FromString("Name").ToString(), ImGuiTableColumnFlags_.ImGuiTableColumnFlags_None, 0, 0);
                    //    ImGuiAPI.TableSetupColumn(TName.FromString("Value").ToString(), ImGuiTableColumnFlags_.ImGuiTableColumnFlags_None, 0, 0);

                    //ImGuiAPI.Columns(2, "PGTable", true);
                    object newValue = null;
                        OnDraw(this.Target, out newValue);//, mCallstack);
                        //ImGuiAPI.Columns(1, "", true);
                    //    ImGuiAPI.EndTable();
                    //}
                }
                ImGuiAPI.EndChild();

                ImGuiAPI.PopFont();

                //ImGuiAPI.Separator();
                ImGuiAPI.PopStyleVar(4);
                ImGuiAPI.PopStyleColor(2);
            }
            else
            {
                Vector2 size = Vector2.Zero;
                if(ImGuiAPI.BeginChild($"{PGName}_Properties", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    object newValue = null;
                    OnDraw(this.Target, out newValue);//, mCallstack);
                }
                ImGuiAPI.EndChild();
            }
        }

        static bool CanNewObject(Rtti.UTypeDesc type)
        {
            if (type.SystemType == typeof(string))
                return false;
            if (type.SystemType == typeof(RName))
                return false;
            if (type.SystemType.IsSubclassOf(typeof(Array)))
                return false;
            return true;
        }

        private bool DrawNameLabel(string displayName, ImGuiTreeNodeFlags_ flags, ref PGCustomValueEditorAttribute.EditorInfo itemEditorInfo)
        {
            ImGuiAPI.TableSetColumnIndex(0);
            var frameHeight = ImGuiAPI.GetFrameHeight();
            var textSize = ImGuiAPI.CalcTextSize(displayName, false, -1);
            var itemNamePadding = new Vector2(0, (frameHeight - textSize.Y - UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in itemNamePadding);
            ImGuiAPI.AlignTextToFramePadding();
            var treeNodeRet = ImGuiAPI.TreeNodeEx(displayName, flags, displayName);
            itemEditorInfo.Expand = treeNodeRet && ((flags & ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf) == 0);
            ImGuiAPI.PopStyleVar(1);
            return treeNodeRet;
        }

        public float Indent = 21.0f;
        public float EndRowPadding = 0.0f;
        public float BeginRowPadding = 0.0f;
        public UInt32 IndentColor = 0x60000000;
        //CustomPropertyDescriptor mLastPropDesc = null;
        //Vector2 mHightlightRowMin = Vector2.Zero;
        //Vector2 mHightlightRowMax = Vector2.Zero;
        public bool UseProvider = true;
        static readonly ImGuiTableFlags_ mTabFlags = ImGuiTableFlags_.ImGuiTableFlags_BordersInner | ImGuiTableFlags_.ImGuiTableFlags_Resizable;// | ImGuiTableFlags_.ImGuiTableFlags_SizingFixedFit;
        public Rtti.UTypeDesc HideInheritDeclareType = null;
        Dictionary<object, Dictionary<string, CustomPropertyDescriptorCollection>> mDrawTargetDic = new Dictionary<object, Dictionary<string, CustomPropertyDescriptorCollection>>();
        private unsafe bool OnDraw(object target, out object targetNewValue, bool isSubPropertyGrid = false)
        {
            targetNewValue = target;
            if (target == null)
                return false;
            if (this.GetType() == typeof(System.Type))
                return false;
            bool retValue = false;
            //var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen;
            bool useCategory = true;
            Dictionary<string, CustomPropertyDescriptorCollection> propertiesDic;
            if(!mDrawTargetDic.TryGetValue(target, out propertiesDic))
            {
                propertiesDic = PropertyCollection.CollectionProperties(target, useCategory, target.GetType().IsValueType);
                mDrawTargetDic[target] = propertiesDic;
            }
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_TableBorderLight, EGui.UIProxy.StyleConfig.Instance.PGCellBorderInnerColor);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_CellPadding, in EGui.UIProxy.StyleConfig.Instance.PGCellPadding);
            var workRectMin = ImGuiAPI.GetWindowContentRegionMin();
            var workRectMax = ImGuiAPI.GetWindowContentRegionMax();
            var drawList = ImGuiAPI.GetWindowDrawList();
            foreach (var proDicValue in propertiesDic)
            {
                string categoryName = proDicValue.Key ?? "Other";
                bool showCollection = true;
                if (useCategory)
                {
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PGCategoryBG);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, EGui.UIProxy.StyleConfig.Instance.PGCategoryBG);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.PGCategoryBG);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor);
                    //ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PGHeadColor);
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameRounding, 0);
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in EGui.UIProxy.StyleConfig.Instance.PGCategoryPadding);
                    UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)Slate.UBaseRenderer.enFont.Font_Bold_13px);

                    if(isSubPropertyGrid)
                    {
                        ImGuiTableRowData rowData = new ImGuiTableRowData()
                        {
                            IndentTextureId = IndentDec.GetImagePtrPointer().ToPointer(),
                            MinHeight = 0,
                            CellPaddingYEnd = EndRowPadding,
                            CellPaddingYBegin = 0,
                            IndentImageWidth = Indent,
                            IndentTextureUVMin = Vector2.Zero,
                            IndentTextureUVMax = Vector2.UnitXY,
                            IndentColor = IndentColor,
                            HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                            Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                        };
                        ImGuiAPI.TableNextRow_FirstColumn(in rowData);
                        //ImGuiAPI.TableSetCellPaddingY(0);
                        ////ImGuiAPI.TableNextRow_NoCellPaddingY(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                        //ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                        //ImGuiAPI.TableSetColumnIndex(0);
                        //ImGuiAPI.TableSetCellPaddingY(EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y);
                        BeginRowPadding = EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y;
                        EndRowPadding = 0.0f;
                    }
                    else
                    {
                        BeginRowPadding = EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y;
                        EndRowPadding = EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y;
                    }
                    showCollection = ImGuiAPI.CollapsingHeader_SpanAllColumns(categoryName, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen);

                    ImGuiAPI.PopStyleColor(4);
                    ImGuiAPI.PopStyleVar(2);
                    ImGuiAPI.PopFont();
                }

                bool tableBegin = false;
                if(!isSubPropertyGrid)
                {
                    Vector2 outerSize = Vector2.Zero;
                    tableBegin = ImGuiAPI.BeginTable("PGTable", 2, mTabFlags, in outerSize, 0.0f);
                    if(tableBegin)
                    {
                        ImGuiAPI.TableSetupColumn(TName.FromString("Name").ToString(), ImGuiTableColumnFlags_.ImGuiTableColumnFlags_None, 0, 0);
                        ImGuiAPI.TableSetupColumn(TName.FromString("Value").ToString(), ImGuiTableColumnFlags_.ImGuiTableColumnFlags_None, 0, 0);
                    }
                }
                if (tableBegin || isSubPropertyGrid)
                {
                    //Vector2 tableWorkRectMin, tableWorkRectMax;
                    //ImGuiAPI.GetTableWorkRect(&tableWorkRectMin, &tableWorkRectMax);

                    /////////////////////////////////////////////
                    //Vector2 size = Vector2.Zero;
                    //ImGuiAPI.Selectable("Select_AAA", true, ImGuiSelectableFlags_.ImGuiSelectableFlags_SpanAllColumns, ref size);
                    /////////////////////////////////////////////

                    //ImGuiAPI.table
                    if (showCollection)
                    {
                        var collection = proDicValue.Value;
                        for (int i = 0; i < collection.Count; i++)
                        {
                            var propDesc = collection[i];
                            if (HideInheritDeclareType != null && target == this.Target)
                            {
                                if (propDesc.DeclaringType.SystemType.IsSubclassOf(HideInheritDeclareType.SystemType) == false)
                                    continue;
                            }

                            object propertyValue; //Support.AnyPointer
                            try
                            {
                                propertyValue = propDesc.GetValue(target);
                            }
                            catch
                            {
                                continue;
                            }

                            var displayName = propDesc.GetDisplayName(target);
                            if (string.IsNullOrEmpty(displayName))
                                continue;
                            if (!string.IsNullOrEmpty(FilterString) && !displayName.ToLower().Contains(FilterString))
                                continue;

                            var showType = propDesc.GetPropertyType(target);
                            var showTypeDesc = Rtti.UTypeDesc.TypeOf(showType);
                            var isReadonly = propDesc.GetIsReadonly(target);
                            var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow;
                            if (IsLeafTreeNode(propDesc, propertyValue, showTypeDesc))
                                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;

                            var itemEditorInfo = new PGCustomValueEditorAttribute.EditorInfo()
                            {
                                Name = displayName,
                                Type = showTypeDesc,
                                Value = propertyValue,
                                ObjectInstance = target,
                                Readonly = IsReadOnly? true : isReadonly,
                                Expand = false,
                                HostPropertyGrid = this,
                                Flags = flags,
                                RowHeight = propDesc.RowHeight,
                            };
                            ImGuiTableRowData rowData = new ImGuiTableRowData()
                            {
                                IndentTextureId = IndentDec.GetImagePtrPointer().ToPointer(),
                                MinHeight = 0,
                                CellPaddingYEnd = EndRowPadding,
                                CellPaddingYBegin = BeginRowPadding,
                                IndentImageWidth = Indent,
                                IndentTextureUVMin = Vector2.Zero,
                                IndentTextureUVMax = Vector2.UnitXY,
                                IndentColor = IndentColor,
                                HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                                Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                            };
                            ImGuiAPI.TableNextRow(in rowData);
                            EndRowPadding = EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y;

                            float y = 0;
                            ImGuiAPI.GetTableRowStartY(&y);
                            //ImGuiAPI.GotoColumns(0);
                            //var cursorStartPos = ImGuiAPI.GetCursorScreenPos();
                            //propDesc.RowRectMin.X = tableWorkRectMin.X;// workRectMin.X;
                            //propDesc.RowRectMin.Y = y;
                            //propDesc.RowRectMax.X = tableWorkRectMax.X;// workRectMax.X;
                            //if (mLastPropDesc != null)
                            //{
                            //    mLastPropDesc.RowRectMax.Y = y;
                            //}
                            //mHightlightRowMin.X = tableWorkRectMin.X;
                            //mHightlightRowMin.Y = y;
                            //mHightlightRowMax.X = tableWorkRectMax.X;
                            //propDesc.RowHeight = propDesc.RowRectMax.Y - propDesc.RowRectMin.Y;
                            //if (propDesc.IsMouseHovered)
                            //    drawList.AddRectFilled(ref mHightlightRowMin, ref mHightlightRowMax, EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor, 0, ImDrawFlags_.ImDrawFlags_None);

                            if (propertyValue == null && CanNewObject(showTypeDesc))
                            {
                                object newValue;
                                var retVal = DrawNameLabel(displayName, flags, ref itemEditorInfo);
                                ImGuiAPI.TableSetColumnIndex(1);
                                //ImGuiAPI.GotoColumns(1);
                                PushPGEditorStyleValues();
                                var changed = EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.ObjectWithCreateEditor.OnDraw(in itemEditorInfo, out newValue);
                                PopPGEditorStyleValues();
                                if (changed)
                                {
                                    propDesc.SetValue(ref target, newValue);
                                    if(propDesc.ParentIsValueType)
                                    {
                                        retVal = true;
                                        targetNewValue = target;
                                    }
                                }
                                if (retVal)
                                    ImGuiAPI.TreePop();
                            }
                            else
                            {
                                var editorOnDraw = propDesc.CustomValueEditor;
                                if (editorOnDraw != null && editorOnDraw.UserDraw)
                                {
                                    if (editorOnDraw.IsFullRedraw)
                                    {
                                        try
                                        {
                                            object newValue;
                                            PushPGEditorStyleValues();
                                            var changed = editorOnDraw.OnDraw(in itemEditorInfo, out newValue);
                                            PopPGEditorStyleValues();
                                            if (changed)
                                            {
                                                propDesc.SetValue(ref target, newValue);
                                                if(propDesc.ParentIsValueType)
                                                {
                                                    retValue = true;
                                                    targetNewValue = target;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }
                                    else
                                    {
                                        var treeNodeRet = DrawNameLabel(displayName, flags, ref itemEditorInfo);
                                        //ImGuiAPI.TableSetColumnIndex(0);
                                        //var frameHeight = ImGuiAPI.GetFrameHeight();
                                        //var textSize = ImGuiAPI.CalcTextSize(displayName, false, -1);
                                        //var itemNamePadding = new Vector2(0, (frameHeight - textSize.Y - UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f);
                                        //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref itemNamePadding);
                                        //ImGuiAPI.AlignTextToFramePadding();
                                        //var treeNodeRet = ImGuiAPI.TreeNodeEx(displayName, flags, displayName);
                                        //itemEditorInfo.Expand = treeNodeRet && ((flags & ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf) == 0);
                                        //ImGuiAPI.PopStyleVar(1);
                                        ImGuiAPI.TableSetColumnIndex(1);
                                        //ImGuiAPI.GotoColumns(1);
                                        try
                                        {
                                            object newValue;
                                            PushPGEditorStyleValues();
                                            var changed = editorOnDraw.OnDraw(in itemEditorInfo, out newValue);
                                            PopPGEditorStyleValues();
                                            if (changed)
                                            {
                                                propDesc.SetValue(ref target, newValue);
                                                if(propDesc.ParentIsValueType)
                                                {
                                                    retValue = true;
                                                    targetNewValue = target;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }

                                        if (treeNodeRet)
                                            ImGuiAPI.TreePop();
                                    }
                                }
                                else
                                {
                                    var treeNodeRet = DrawNameLabel(displayName, flags, ref itemEditorInfo);
                                    //ImGuiAPI.TableSetColumnIndex(0);
                                    //var frameHeight = ImGuiAPI.GetFrameHeight();
                                    //var textSize = ImGuiAPI.CalcTextSize(displayName, false, -1);
                                    //var itemNamePadding = new Vector2(0, (frameHeight - textSize.Y - UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f);
                                    //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref itemNamePadding);
                                    //ImGuiAPI.AlignTextToFramePadding();
                                    //var treeNodeRet = ImGuiAPI.TreeNodeEx(displayName, flags, displayName);
                                    //itemEditorInfo.Expand = treeNodeRet && ((flags & ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf) == 0);
                                    //ImGuiAPI.PopStyleVar(1);
                                    ImGuiAPI.TableSetColumnIndex(1);
                                    //ImGuiAPI.GotoColumns(1);

                                    object newValue;
                                    var valueChanged = DrawPropertyGridItem(ref itemEditorInfo, out newValue);
                                    if (valueChanged)
                                    {
                                        propDesc.SetValue(ref target, newValue);
                                        if(propDesc.ParentIsValueType)
                                        {
                                            retValue = true;
                                            targetNewValue = target;
                                        }
                                    }

                                    if (treeNodeRet)
                                        ImGuiAPI.TreePop();
                                }
                            }

                            //ImGuiAPI.GetTableRowEndY(&y);
                            //mHightlightRowMax.Y = y;
                            //propDesc.IsMouseHovered = ImGuiAPI.IsMouseHoveringRectInCurrentWindow(ref mHightlightRowMin, ref mHightlightRowMax, false);
                            //propDesc.RowRectMax.Y = y;

                            //mLastPropDesc = propDesc;
                        }
                    }
                    if(tableBegin)
                    {
                        ImGuiTableRowData rowData = new ImGuiTableRowData()
                        {
                            IndentTextureId = IndentDec.GetImagePtrPointer().ToPointer(),
                            MinHeight = 0,
                            CellPaddingYEnd = EndRowPadding,
                            CellPaddingYBegin = BeginRowPadding,
                            IndentImageWidth = Indent,
                            IndentTextureUVMin = Vector2.Zero,
                            IndentTextureUVMax = Vector2.UnitXY,
                            IndentColor = IndentColor,
                            HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                            Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                        };
                        ImGuiAPI.TableNextRow(in rowData);
                        ImGuiAPI.EndTable();
                    }
                }
            }
            ImGuiAPI.PopStyleColor(1);
            ImGuiAPI.PopStyleVar(1);

            return retValue;
        }
        static void PushPGEditorStyleValues()
        {
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        }
        static void PopPGEditorStyleValues()
        {
            ImGuiAPI.PopStyleVar(1);
        }
        public static bool IsLeafTreeNode(CustomPropertyDescriptor propDesc, object value, Rtti.UTypeDesc propertyTypeDesc)
        {
            if (propDesc.CustomValueEditor != null)
                return !propDesc.CustomValueEditor.Expandable;

            return IsLeafTreeNode(value, propertyTypeDesc);
        }
        public static bool IsLeafTreeNode(object value, Rtti.UTypeDesc typeDesc)
        {
            if(typeDesc == null)
                typeDesc = Rtti.UTypeDesc.TypeOf(value.GetType());
            var editor = EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.GetEditorType(typeDesc);
            if (editor != null)
                return !editor.Expandable;
            if (typeDesc.SystemType.IsEnum)
                return true;

            var attrs = typeDesc.SystemType.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                var editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
                return !editorOnDraw.Expandable;
            }
            if (value != null)
                return false;

            return true;
        }

        public static unsafe bool DrawPropertyGridItem(ref PGCustomValueEditorAttribute.EditorInfo info, out object newValue)
        {
            PushPGEditorStyleValues();
            newValue = info.Value;
            bool valueChanged = false;
            try
            {
                if (info.Value == null && CanNewObject(info.Type))
                {
                    valueChanged = EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.ObjectWithCreateEditor.OnDraw(in info, out newValue);
                }
                else if (EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.DrawTypeEditor(in info, out newValue, out valueChanged))
                {
                    //return valueChanged;
                }
                else if (info.Type.IsEnum)
                {
                    valueChanged = EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.EnumEditor.OnDraw(in info, out newValue);
                }
                else if (info.Type.IsArray)
                {
                    valueChanged = EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.ArrayEditor.OnDraw(in info, out newValue);
                }
                else if (info.Type.SystemType.GetInterface(typeof(System.Collections.IList).FullName) != null)
                {
                    valueChanged = EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.ListEditor.OnDraw(in info, out newValue);
                }
                else if (info.Type.SystemType.GetInterface(typeof(System.Collections.IDictionary).FullName) != null)
                {
                    valueChanged = EngineNS.UEngine.Instance.PGTypeEditorManagerInstance.DictionaryEditor.OnDraw(in info, out newValue);
                }
                else
                {
                    PGCustomValueEditorAttribute editorOnDraw = null;
                    var attrs = info.Type.SystemType.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), false);
                    if(attrs != null && attrs.Length > 0)
                    {
                        editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
                    }
                    if (editorOnDraw != null)
                    {
                        if(editorOnDraw.IsFullRedraw)
                        {
                            ImGuiAPI.NextColumn();
                            //ImGuiAPI.TableNextColumn();
                            valueChanged = editorOnDraw.OnDraw(in info, out newValue);
                        }
                        else
                            valueChanged = editorOnDraw.OnDraw(in info, out newValue);
                    }
                    else
                    {
                        //var multiValue = info.Value as PropertyMultiValue;
                        //if (multiValue != null)
                        //{
                        //    ImGuiAPI.Text(multiValue.MultiValueString);
                        //}
                        //else
                        {
                            ImGuiAPI.Text(info.Type.ToString());
                            ImGuiAPI.SameLine(0, 10);
                            if (info.Type.IsValueType == false)
                            {
                                var drawList = ImGuiAPI.GetWindowDrawList();
                                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Button, EGui.UIProxy.StyleConfig.Instance.PGDeleteButtonBGColor);
                                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonActive, EGui.UIProxy.StyleConfig.Instance.PGDeleteButtonBGActiveColor);
                                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonHovered, EGui.UIProxy.StyleConfig.Instance.PGDeleteButtonBGHoverColor);
                                if (info.Readonly == false && info.HostPropertyGrid.mDelete.OnDraw(ref drawList, ref Support.UAnyPointer.Default))
                                {
                                    newValue = null;
                                    valueChanged = true;
                                }
                                ImGuiAPI.PopStyleColor(3);
                            }
                            if (info.Expand)
                            {
                                var multiValue = info.Value as PropertyMultiValue;
                                if(multiValue != null)
                                {
                                    valueChanged = info.HostPropertyGrid.OnDraw(multiValue.Values, out newValue, true);
                                    newValue = multiValue;
                                }
                                else
                                    valueChanged = info.HostPropertyGrid.OnDraw(info.Value, out newValue, true);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            PopPGEditorStyleValues();
            return valueChanged;
        }

        //private unsafe void OnDraw2(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        //{
        //    if (target == null)
        //        return;

        //    if (this.GetType() == typeof(System.Type))
        //    {
        //        return;
        //    }

        //    ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen;
        //    var props = target.GetType().GetProperties();
        //    foreach (var i in props)
        //    {
        //        if (HideInheritDeclareType != null)
        //        {
        //            if (i.DeclaringType.IsSubclassOf(HideInheritDeclareType.SystemType) == false)
        //                continue;
        //        }
        //        PGCustomValueEditorAttribute editorOnDraw = null;
        //        var attrs = i.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), true);
        //        if (attrs != null && attrs.Length > 0)
        //        {
        //            editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
        //            if (editorOnDraw.HideInPG)
        //                continue;
        //        }
        //        bool isReadOnly = (editorOnDraw != null && editorOnDraw.ReadOnly);

        //        object obj;
        //        try
        //        {
        //            if (i.PropertyType == typeof(System.Type))
        //            {
        //                continue;
        //            }
        //            if (i.DeclaringType.FullName == "System.RuntimeType")
        //            {
        //                continue;
        //            }
        //            if (i.Name == "Item")
        //            {
        //                continue;
        //            }
        //            obj = i.GetValue(target);
        //        }
        //        catch
        //        {
        //            continue;
        //        }

        //        System.Type showType = i.PropertyType;
        //        if (obj == null && showType != typeof(string) && showType != typeof(RName))
        //        {   
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            //ImGuiAPI.Text(i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            var sz = new Vector2(0, 0);
        //            if (!showType.IsSubclassOf(typeof(System.Array)) && isReadOnly == false)
        //            {
        //                if (ImGuiAPI.Button("New", ref sz))
        //                {
        //                    var v = System.Activator.CreateInstance(showType);
        //                    foreach (var j in this.TargetObjects)
        //                    {
        //                        SetValue(this, j, callstack, i, target, v);
        //                    }
        //                }
        //            }
        //            ImGuiAPI.NextColumn();
        //            continue;
        //        }
        //        else
        //        {
        //            if (obj != null)
        //                showType = obj.GetType();
        //        }

        //        if (editorOnDraw != null && editorOnDraw.UserDraw)
        //        {
        //            if (editorOnDraw.IsFullRedraw)
        //            {
        //                try
        //                {
        //                    editorOnDraw.OnDraw(i, target, obj, this, callstack);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex.ToString());
        //                }
        //            }
        //            else
        //            {
        //                ImGuiAPI.AlignTextToFramePadding();
        //                ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //                ImGuiAPI.NextColumn();
        //                try
        //                {
        //                    editorOnDraw.OnDraw(i, target, obj, this, callstack);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex.ToString());
        //                }
        //                ImGuiAPI.NextColumn();
        //            }
        //        }
        //        else if (showType == typeof(bool))
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            var v = (System.Convert.ToBoolean(obj));
        //            var saved = v;
        //            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGCheckboxFramePadding);
        //            ImGuiAPI.Checkbox(TName.FromString2("##", i.Name).ToString(), ref v);
        //            ImGuiAPI.PopStyleVar(1);
        //            if (v != saved)
        //            {
        //                foreach (var j in this.TargetObjects)
        //                {
        //                    SetValue(this, j, callstack, i, target, (bool)v);
        //                }
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (showType == typeof(int) || 
        //                showType == typeof(sbyte) || 
        //                showType == typeof(short) || 
        //                showType == typeof(long))
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();                    
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            var v = System.Convert.ToInt32(obj);
        //            var saved = v;
        //            ImGuiAPI.InputInt(TName.FromString2("##", i.Name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
        //            if (v != saved)
        //            {
        //                foreach (var j in this.TargetObjects)
        //                {
        //                    SetValue(this, j, callstack, i, target, EngineNS.Support.TConvert.ToObject(showType, v.ToString()));
        //                }
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (showType == typeof(uint) || 
        //            showType == typeof(byte) ||
        //            showType == typeof(ushort) ||
        //            showType == typeof(ulong))
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        //            var buffer = BigStackBuffer.CreateInstance(256);
        //            var oldValue = obj.ToString();
        //            buffer.SetText(oldValue);
        //            ImGuiAPI.InputText(TName.FromString2("##", i.Name).ToString(), buffer.GetBuffer(), (uint)buffer.GetSize(),
        //                ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsHexadecimal, null, (void*)0);
        //            var newValue = buffer.AsText();
        //            if (newValue != oldValue)
        //            {
        //                var v = Support.TConvert.ToObject(showType, newValue);
        //                foreach (var j in this.TargetObjects)
        //                {
        //                    SetValue(this, j, callstack, i, target, v);
        //                }
        //            }
        //            buffer.DestroyMe();
        //            ImGuiAPI.PopStyleVar(1);
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (showType == typeof(float))
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            var v = System.Convert.ToSingle(obj);
        //            var saved = v;
        //            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        //            ImGuiAPI.InputFloat(TName.FromString2("##", i.Name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //            ImGuiAPI.PopStyleVar(1);
        //            if (v != saved)
        //            {
        //                foreach (var j in this.TargetObjects)
        //                {
        //                    SetValue(this, j, callstack, i, target, v);
        //                }
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (showType == typeof(double))
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            var v = System.Convert.ToDouble(obj);
        //            var saved = v;
        //            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        //            ImGuiAPI.InputFloat(TName.FromString2("##", i.Name).ToString(), ref (*(float*)&v), 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //            ImGuiAPI.PopStyleVar(1);
        //            if (v != saved)
        //            {
        //                foreach (var j in this.TargetObjects)
        //                {
        //                    SetValue(this, j, callstack, i, target, v);
        //                }
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (showType == typeof(string))
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            var v = obj as string;
        //            var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
        //            fixed (byte* pBuffer = &TextBuffer[0])
        //            {
        //                CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
        //                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        //                ImGuiAPI.InputText(TName.FromString2("##", i.Name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
        //                ImGuiAPI.PopStyleVar(1);
        //                if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
        //                {
        //                    v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
        //                    foreach (var j in this.TargetObjects)
        //                    {
        //                        SetValue(this, j, callstack, i, target, v);
        //                    }
        //                }
        //                System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (showType.IsEnum)
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.SetNextItemWidth(-1);
        //            var attrs1 = i.PropertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
        //            if (attrs1 != null && attrs1.Length > 0)
        //            {
        //                if (ImGuiAPI.TreeNode("Flags"))
        //                {
        //                    var members = showType.GetEnumNames();
        //                    var values = showType.GetEnumValues();
        //                    var sz = new Vector2(0, 0);
        //                    uint newFlags = 0;
        //                    var EnumFlags = System.Convert.ToUInt32(obj);
        //                    for (int j = 0; j < members.Length; j++)
        //                    {
        //                        var m = values.GetValue(j).ToString();
        //                        var e_v = System.Enum.Parse(i.PropertyType, m);
        //                        var v = System.Convert.ToUInt32(e_v);
        //                        var bSelected = (((EnumFlags & v) == 0) ? false : true);
        //                        ImGuiAPI.Checkbox(members[j], ref bSelected);
        //                        if (bSelected)
        //                        {
        //                            newFlags |= v;
        //                        }
        //                        else
        //                        {
        //                            newFlags &= ~v;
        //                        }
        //                    }
        //                    if (newFlags != EnumFlags)
        //                    {
        //                        foreach (var j in this.TargetObjects)
        //                        {
        //                            SetValue(this, j, callstack, i, target, System.Enum.ToObject(i.PropertyType, newFlags));
        //                        }
        //                    }
        //                    ImGuiAPI.TreePop();
        //                }
        //            }
        //            else
        //            {
        //                if (ImGuiAPI.BeginCombo(TName.FromString2("##", i.Name).ToString(), obj.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
        //                {
        //                    int item_current_idx = -1;
        //                    var members = showType.GetEnumNames();
        //                    var values = showType.GetEnumValues();
        //                    var sz = new Vector2(0, 0);
        //                    for (int j = 0; j < members.Length; j++)
        //                    {
        //                        var bSelected = true;
        //                        if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
        //                        {
        //                            item_current_idx = j;
        //                        }
        //                    }
        //                    if (item_current_idx >= 0)
        //                    {
        //                        var v = (int)values.GetValue(item_current_idx);
        //                        foreach (var j in this.TargetObjects)
        //                        {
        //                            SetValue(this, j, callstack, i, target, v);
        //                        }
        //                    }

        //                    ImGuiAPI.EndCombo();
        //                }
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (obj is Array)
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.SameLine(0, -1);
        //            var showChild = ImGuiAPI.TreeNode(i.Name, "");
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.Text(i.ToString());
        //            ImGuiAPI.NextColumn();
        //            if (showChild)
        //            {
        //                var lst = obj as System.Array;
        //                OnArray(target, callstack, i, lst);
        //                ImGuiAPI.TreePop();
        //            }
        //        }
        //        else if (obj is System.Collections.IList)
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            if (this.IsReadOnly == false)
        //            {
        //                ImGuiAPI.SameLine(0, -1);
        //                var sz = new Vector2(0, 0);
        //                ImGuiAPI.PushID(i.Name);
        //                ImGuiAPI.SameLine(0, -1);
        //                ImGuiAPI.OpenPopupOnItemClick("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
        //                //var pos = ImGuiAPI.GetItemRectMin();
        //                //var size = ImGuiAPI.GetItemRectSize();
        //                if (ImGuiAPI.ArrowButton("##OpenAddItemList", ImGuiDir_.ImGuiDir_Down))
        //                {
        //                    ImGuiAPI.OpenPopup("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
        //                }
        //                if (ImGuiAPI.BeginPopup("AddItem", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
        //                {
        //                    var dict = obj as System.Collections.IList;
        //                    if (dict.GetType().GenericTypeArguments.Length == 1)
        //                    {
        //                        var listElementType = dict.GetType().GenericTypeArguments[0];
        //                        var typeSlt = new EGui.Controls.TypeSelector();
        //                        typeSlt.BaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(listElementType.FullName);
        //                        typeSlt.OnDraw(150, 6);
        //                        if (typeSlt.SelectedType != null)
        //                        {
        //                            foreach (var j in this.TargetObjects)
        //                            {
        //                                AddList(j, callstack, i, target, dict.Count, System.Activator.CreateInstance(listElementType));
        //                            }
        //                        }
        //                    }
        //                    ImGuiAPI.EndPopup();
        //                }
        //                ImGuiAPI.PopID();
        //            }
        //            ImGuiAPI.SameLine(0, -1);
        //            var showChild = ImGuiAPI.TreeNode(i.Name, "");
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.Text(i.ToString());
        //            ImGuiAPI.NextColumn();
        //            if (showChild)
        //            {
        //                var dict = obj as System.Collections.IList;
        //                OnList(target, callstack, i, dict);
        //                ImGuiAPI.TreePop();
        //            }
        //        }
        //        else if (obj is System.Collections.IDictionary)
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            if (this.IsReadOnly == false)
        //            {
        //                ImGuiAPI.SameLine(0, -1);
        //                var sz = new Vector2(0, 0);
        //                ImGuiAPI.PushID(i.Name);
        //                if (ImGuiAPI.ArrowButton("##OpenAddItemDict", ImGuiDir_.ImGuiDir_Down))
        //                {
        //                    ImGuiAPI.PopID();
        //                    ImGuiAPI.OpenPopup("AddDictElement", ImGuiPopupFlags_.ImGuiPopupFlags_None);
        //                }
        //                else
        //                {
        //                    ImGuiAPI.PopID();
        //                }
        //            }

        //            {
        //                Rtti.UTypeDesc keyType = null, valueType = null;
        //                var dict = obj as System.Collections.IDictionary;
        //                if (dict.GetType().GenericTypeArguments.Length == 2)
        //                {
        //                    keyType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[0].FullName);
        //                    valueType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[1].FullName);
        //                }
        //                if (mKVCreator == null)
        //                {
        //                    mKVCreator = new KeyValueCreator();
        //                }
        //                if (mKVCreator.CtrlName != i.Name)
        //                {
        //                    mKVCreator.CtrlName = i.Name;
        //                    mKVCreator.KeyTypeSlt.BaseType = keyType;
        //                    mKVCreator.ValueTypeSlt.BaseType = valueType;
        //                }
                        
        //                var size = new Vector2(300, 500);
        //                ImGuiAPI.SetNextWindowSize(ref size, ImGuiCond_.ImGuiCond_None);
        //                mKVCreator.CreateFinished = false;
        //                mKVCreator.OnDraw("AddDictElement");
        //                if (mKVCreator.CreateFinished)
        //                {
        //                    foreach (var j in this.TargetObjects)
        //                    {
        //                        SetDictionaryValue(j, callstack, i, target, mKVCreator.KeyData, mKVCreator.ValueData);
        //                    }
        //                }
        //            }

        //            ImGuiAPI.SameLine(0, -1);                    
        //            var showChild = ImGuiAPI.TreeNode(i.Name, "");
        //            ImGuiAPI.NextColumn();
        //            ImGuiAPI.Text(i.ToString());
        //            ImGuiAPI.NextColumn();
        //            if (showChild)
        //            {
        //                var dict = obj as System.Collections.IDictionary;
        //                OnDictionary(target, callstack, i, dict);
        //                ImGuiAPI.TreePop();
        //            }
        //        }
        //        else if ((editorOnDraw = PGTypeEditorManager.Instance.GetEditorType(showType)) != null)
        //        {
        //            ImGuiAPI.AlignTextToFramePadding();
        //            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //            ImGuiAPI.NextColumn();
        //            try
        //            {
        //                editorOnDraw.OnDraw(i, target, obj, this, callstack);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(ex.ToString());
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else// if (showType.IsValueType == false)
        //        {
        //            attrs = i.PropertyType.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), false);
        //            if (attrs != null && attrs.Length > 0)
        //            {
        //                editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
        //            }
        //            if (editorOnDraw != null)
        //            {
        //                if (editorOnDraw.IsFullRedraw)
        //                {
        //                    try
        //                    {
        //                        editorOnDraw.OnDraw(i, target, obj, this, callstack);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine(ex.ToString());
        //                    }
        //                }
        //                else
        //                {
        //                    ImGuiAPI.AlignTextToFramePadding();
        //                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //                    ImGuiAPI.NextColumn();
        //                    try
        //                    {
        //                        editorOnDraw.OnDraw(i, target, obj, this, callstack);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine(ex.ToString());
        //                    }
        //                    ImGuiAPI.NextColumn();
        //                }
        //            }
        //            else
        //            {
        //                ImGuiAPI.AlignTextToFramePadding();
        //                ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
        //                ImGuiAPI.SameLine(0, -1);
        //                var showChild = ImGuiAPI.TreeNode(i.Name, "");
        //                ImGuiAPI.NextColumn();
        //                if (showType.IsValueType == false)
        //                {
        //                    var sz = new Vector2(0, 0);
        //                    if (isReadOnly == false && ImGuiAPI.Button("Remove:", ref sz))
        //                    {
        //                        foreach (var j in this.TargetObjects)
        //                        {
        //                            SetValue(this, j, callstack, i, target, null);
        //                        }
        //                    }
        //                    ImGuiAPI.SameLine(0, -1);
        //                }
        //                ImGuiAPI.Text(i.ToString());
        //                ImGuiAPI.NextColumn();
        //                if (showChild)
        //                {
        //                    callstack.Add(new KeyValuePair<object, System.Reflection.PropertyInfo>(target, i));
        //                    OnDraw2(obj, callstack);

        //                    ImGuiAPI.TreePop();
        //                }
        //            }
        //        }


        //        var drawList = ImGuiAPI.GetWindowDrawList();
        //        var cursorPos = ImGuiAPI.GetCursorScreenPos();
        //        var columnWidth = ImGuiAPI.GetColumnWidth(ImGuiAPI.GetColumnIndex());
        //        var endPos = cursorPos + new Vector2(columnWidth, 0);
        //        drawList.AddLine(ref cursorPos, ref endPos, EGui.UIProxy.StyleConfig.Instance.SeparatorColor, 1);
        //        ImGuiAPI.NextColumn();
        //        cursorPos = ImGuiAPI.GetCursorScreenPos();
        //        columnWidth = ImGuiAPI.GetColumnWidth(ImGuiAPI.GetColumnIndex());
        //        endPos = cursorPos + new Vector2(columnWidth, 0);
        //        drawList.AddLine(ref cursorPos, ref endPos, EGui.UIProxy.StyleConfig.Instance.SeparatorColor, 1);
        //        ImGuiAPI.NextColumn();
        //    }
        //    callstack.RemoveAt(callstack.Count - 1);
        //}
        //KeyValueCreator mKVCreator = null;
        
        //private unsafe void OnList(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, System.Collections.IList lst)
        //{
        //    ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
        //    List<KeyValuePair<int, object>> changedList = new List<KeyValuePair<int, object>>();
        //    List<int> removeList = new List<int>();
        //    List<KeyValuePair<int, object>> addList = new List<KeyValuePair<int, object>>();
        //    var sz = new Vector2(0, 0);
        //    for (int i = 0; i < lst.Count; i++)
        //    {
        //        var name = i.ToString();
        //        var obj = lst[i];

        //        ImGuiAPI.AlignTextToFramePadding();                
        //        if (IsReadOnly == false)
        //        {
        //            ImGuiAPI.PushID(TName.FromString2("##ListDel_", i.ToString()).ToString());
        //            if (ImGuiAPI.Button("-", ref sz))
        //            {
        //                removeList.Add(i);
        //            }
        //            ImGuiAPI.PopID();

        //            ImGuiAPI.SameLine(0, -1);
        //            ImGuiAPI.PushID(TName.FromString2("##ListAdd_", i.ToString()).ToString());
        //            if (ImGuiAPI.Button("+", ref sz))
        //            {
        //                addList.Add(new KeyValuePair<int, object>(i, obj));
        //            }
        //            ImGuiAPI.PopID();
        //            ImGuiAPI.SameLine(0, -1);
        //        }                
        //        ImGuiAPI.TreeNodeEx(name, flags, name);
        //        ImGuiAPI.NextColumn();
        //        ImGuiAPI.SetNextItemWidth(-1);

        //        if(obj==null)
        //        {
        //            ImGuiAPI.Text("null");
        //            ImGuiAPI.NextColumn();
        //        }
        //        else
        //        {
        //            var vtype = obj.GetType();
        //            if (vtype == typeof(bool))
        //            {
        //                var v = System.Convert.ToBoolean(obj);
        //                var saved = v;
        //                ImGuiAPI.Checkbox(TName.FromString2("##Value", name).ToString(), ref v);
        //                if (v != saved)
        //                {
        //                    changedList.Add(new KeyValuePair<int, object>(i, v));
        //                }
        //                ImGuiAPI.NextColumn();
        //            }
        //            else if (vtype == typeof(int) || vtype == typeof(uint) || 
        //                vtype == typeof(sbyte) || vtype == typeof(byte) || 
        //                vtype == typeof(short) || vtype == typeof(ushort) ||
        //                vtype == typeof(long) || vtype == typeof(ulong))
        //            {
        //                var v = System.Convert.ToInt32(obj);
        //                var saved = v;
        //                ImGuiAPI.InputInt(TName.FromString2("##Value", name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //                if (v != saved)
        //                {
        //                    changedList.Add(new KeyValuePair<int, object>(i, EngineNS.Support.TConvert.ToObject(vtype, v.ToString())));
        //                }
        //                ImGuiAPI.NextColumn();
        //            }
        //            else if (vtype == typeof(float))
        //            {
        //                var v = System.Convert.ToSingle(obj);
        //                var saved = v;
        //                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        //                ImGuiAPI.InputFloat(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //                ImGuiAPI.PopStyleVar(1);
        //                if (v != saved)
        //                {
        //                    changedList.Add(new KeyValuePair<int, object>(i, v));
        //                }
        //                ImGuiAPI.NextColumn();
        //            }
        //            else if (vtype == typeof(double))
        //            {
        //                var v = System.Convert.ToDouble(obj);
        //                var saved = v;
        //                ImGuiAPI.InputDouble(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //                if (v != saved)
        //                {
        //                    changedList.Add(new KeyValuePair<int, object>(i, v));
        //                }
        //                ImGuiAPI.NextColumn();
        //            }
        //            else if (vtype == typeof(string))
        //            {
        //                var v = obj as string;
        //                var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
        //                fixed (byte* pBuffer = &TextBuffer[0])
        //                {
        //                    CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
        //                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        //                    ImGuiAPI.InputText(TName.FromString2("##Value", name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
        //                    ImGuiAPI.PopStyleVar(1);
        //                    if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
        //                    {
        //                        v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
        //                        changedList.Add(new KeyValuePair<int, object>(i, v));
        //                    }
        //                    System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
        //                }
        //                ImGuiAPI.NextColumn();
        //            }
        //            else if (vtype.IsEnum)
        //            {
        //                var attrs1 = prop.PropertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
        //                if (attrs1 != null && attrs1.Length > 0)
        //                {
        //                    if (ImGuiAPI.TreeNode("Flags"))
        //                    {
        //                        var members = vtype.GetEnumNames();
        //                        var values = vtype.GetEnumValues();
        //                        sz = new Vector2(0, 0);
        //                        uint newFlags = 0;
        //                        var EnumFlags = System.Convert.ToUInt32(obj);
        //                        for (int j = 0; j < members.Length; j++)
        //                        {
        //                            var m = values.GetValue(j).ToString();
        //                            var e_v = System.Enum.Parse(prop.PropertyType, m);
        //                            var v = System.Convert.ToUInt32(e_v);
        //                            var bSelected = ((EnumFlags & v) == 0) ? false : true;
        //                            ImGuiAPI.Checkbox(members[j], ref bSelected);
        //                            if (bSelected)
        //                            {
        //                                newFlags |= v;
        //                            }
        //                            else
        //                            {
        //                                newFlags &= ~v;
        //                            }
        //                        }
        //                        if (newFlags != EnumFlags)
        //                        {
        //                            changedList.Add(new KeyValuePair<int, object>(i, newFlags));
        //                        }
        //                        ImGuiAPI.TreePop();
        //                    }
        //                }
        //                else
        //                {
        //                    if (ImGuiAPI.BeginCombo(TName.FromString2("##Enum", name).ToString(), obj.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
        //                    {
        //                        int item_current_idx = -1;
        //                        var members = vtype.GetEnumNames();
        //                        var values = vtype.GetEnumValues();
        //                        sz = new Vector2(0, 0);
        //                        for (int j = 0; j < members.Length; j++)
        //                        {
        //                            var bSelected = true;
        //                            if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
        //                            {
        //                                item_current_idx = j;
        //                            }
        //                        }
        //                        if (item_current_idx >= 0)
        //                        {
        //                            var v = (int)values.GetValue(item_current_idx);
        //                            changedList.Add(new KeyValuePair<int, object>(i, v));
        //                        }

        //                        ImGuiAPI.EndCombo();
        //                    }
        //                }
        //                ImGuiAPI.NextColumn();
        //            }
        //            else
        //            {
        //                if (ImGuiAPI.TreeNode($"{prop.Name}_{name}", obj.ToString()))
        //                {
        //                    ImGuiAPI.NextColumn();
        //                    var keyPG = new PropertyGrid();
        //                    keyPG.IsReadOnly = this.IsReadOnly;
        //                    keyPG.SingleTarget = obj;
        //                    keyPG.OnDraw(false, false, true);
        //                    ImGuiAPI.TreePop();
        //                }
        //                else
        //                {
        //                    ImGuiAPI.NextColumn();
        //                }
        //            }
        //        }
        //    }

        //    foreach (var i in changedList)
        //    {
        //        foreach (var j in this.TargetObjects)
        //        {
        //            SetListValue(j, callstack, prop, target, i.Key, i.Value);
        //        }
        //    }
        //    foreach (var i in removeList)
        //    {
        //        foreach (var j in this.TargetObjects)
        //        {
        //            RemoveList(j, callstack, prop, target, i);
        //        }
        //    }
        //    foreach (var i in addList)
        //    {
        //        foreach (var j in this.TargetObjects)
        //        {
        //            AddList(j, callstack, prop, target, i.Key, i.Value);
        //        }
        //    }
        //}
        //private unsafe void OnDictionary(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, System.Collections.IDictionary dict)
        //{
        //    ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
        //    List<KeyValuePair<object, object>> changedList = new List<KeyValuePair<object, object>>();
        //    List<object> removeList = new List<object>();
        //    var sz = new Vector2(0, 0);
        //    var iter = dict.GetEnumerator();
        //    while (iter.MoveNext())
        //    {
        //        var name = iter.Key.ToString();
        //        ImGuiAPI.AlignTextToFramePadding();
        //        if (IsReadOnly == false)
        //        {
        //            ImGuiAPI.PushID(TName.FromString2("##ListDel_", name).ToString());
        //            if (ImGuiAPI.Button("-", ref sz))
        //            {
        //                removeList.Add(iter.Key);
        //            }
        //            ImGuiAPI.PopID();
        //            ImGuiAPI.SameLine(0, -1);
        //        }
        //        ImGuiAPI.TreeNodeEx(name, flags, name);
        //        ImGuiAPI.NextColumn();
        //        ImGuiAPI.SetNextItemWidth(-1);
        //        var vtype = iter.Value.GetType();
        //        var obj = iter.Value;
        //        if (vtype == typeof(bool))
        //        {
        //            var v = (System.Convert.ToBoolean(obj));
        //            var saved = v;
        //            ImGuiAPI.Checkbox(TName.FromString2("##Value", name).ToString(), ref v);
        //            if (v != saved)
        //            {
        //                changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (vtype == typeof(int) || vtype == typeof(uint) ||
        //                vtype == typeof(sbyte) || vtype == typeof(byte) ||
        //                vtype == typeof(short) || vtype == typeof(ushort) ||
        //                vtype == typeof(long) || vtype == typeof(ulong))
        //        {
        //            var v = System.Convert.ToInt32(obj);
        //            var saved = v;
        //            ImGuiAPI.InputInt(TName.FromString2("##Value", name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //            if (v != saved)
        //            {
        //                changedList.Add(new KeyValuePair<object, object>(iter.Key, EngineNS.Support.TConvert.ToObject(vtype, v.ToString())));
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (vtype == typeof(float))
        //        {
        //            var v = System.Convert.ToSingle(obj);
        //            var saved = v;
        //            ImGuiAPI.InputFloat(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //            if (v != saved)
        //            {
        //                changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (vtype == typeof(double))
        //        {
        //            var v = System.Convert.ToDouble(obj);
        //            var saved = v;
        //            ImGuiAPI.InputDouble(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
        //            if (v != saved)
        //            {
        //                changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (vtype == typeof(string))
        //        {
        //            var v = obj as string;
        //            var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
        //            if (CoreSDK.SDK_StrLen(strPtr.ToPointer()) >= (uint)TextBuffer.Length)
        //            {
        //                TextBuffer = new byte[(int)CoreSDK.SDK_StrLen(strPtr.ToPointer()) + 512];
        //            }
        //            fixed (byte* pBuffer = &TextBuffer[0])
        //            {
        //                CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
        //                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
        //                ImGuiAPI.InputText(TName.FromString2("##Value", name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
        //                ImGuiAPI.PopStyleVar(1);
        //                if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
        //                {
        //                    v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
        //                    changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
        //                }
        //                System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else if (vtype.IsEnum)
        //        {
        //            var attrs1 = prop.PropertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
        //            if (attrs1 != null && attrs1.Length > 0)
        //            {
        //                if (ImGuiAPI.TreeNode("Flags"))
        //                {
        //                    var members = vtype.GetEnumNames();
        //                    var values = vtype.GetEnumValues();
        //                    sz = new Vector2(0, 0);
        //                    uint newFlags = 0;
        //                    var EnumFlags = System.Convert.ToUInt32(obj);
        //                    for (int j = 0; j < members.Length; j++)
        //                    {
        //                        var m = values.GetValue(j).ToString();
        //                        var e_v = System.Enum.Parse(prop.PropertyType, m);
        //                        var v = System.Convert.ToUInt32(e_v);
        //                        var bSelected = (((EnumFlags & v) == 0) ? false : true);
        //                        ImGuiAPI.Checkbox(members[j], ref bSelected);
        //                        if (bSelected)
        //                        {
        //                            newFlags |= v;
        //                        }
        //                        else
        //                        {
        //                            newFlags &= ~v;
        //                        }
        //                    }
        //                    if (newFlags != EnumFlags)
        //                    {
        //                        changedList.Add(new KeyValuePair<object, object>(iter.Key, newFlags));
        //                    }
        //                    ImGuiAPI.TreePop();
        //                }
        //            }
        //            else
        //            {
        //                if (ImGuiAPI.BeginCombo(TName.FromString2("##Enum_", name).ToString(), obj.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
        //                {
        //                    int item_current_idx = -1;
        //                    var members = vtype.GetEnumNames();
        //                    var values = vtype.GetEnumValues();
        //                    sz = new Vector2(0, 0);
        //                    for (int j = 0; j < members.Length; j++)
        //                    {
        //                        var bSelected = true;
        //                        if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
        //                        {
        //                            item_current_idx = j;
        //                        }
        //                    }
        //                    if (item_current_idx >= 0)
        //                    {
        //                        var v = (int)values.GetValue(item_current_idx);
        //                        changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
        //                    }

        //                    ImGuiAPI.EndCombo();
        //                }
        //            }
        //            ImGuiAPI.NextColumn();
        //        }
        //        else
        //        {
        //            if (ImGuiAPI.TreeNode($"{prop.Name}_{name}", obj.ToString()))
        //            {
        //                ImGuiAPI.NextColumn();
        //                var keyPG = new PropertyGrid();
        //                keyPG.SingleTarget = obj;
        //                keyPG.OnDraw(false, false, true);
        //                ImGuiAPI.TreePop();
        //            }
        //            else
        //            {
        //                ImGuiAPI.NextColumn();
        //            }
        //        }
        //    }

        //    foreach(var i in changedList)
        //    {
        //        foreach (var j in this.TargetObjects)
        //        {
        //            SetDictionaryValue(j, callstack, prop, target, i.Key, i.Value);
        //        }
        //    }
        //    foreach (var i in removeList)
        //    {
        //        foreach (var j in this.TargetObjects)
        //        {
        //            RemoveDictionary(j, callstack, prop, target, i);
        //        }
        //    }
        //}
        //private static void RemoveList(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, int index)
        //{
        //    List<object> ownerChain = new List<object>();
        //    ownerChain.Add(null);
        //    ownerChain.Add(root);
        //    for (int i = 1; i < callstack.Count; i++)
        //    {
        //        root = callstack[i].Value.GetValue(root);
        //        if (root == null)
        //            return;
        //        ownerChain.Add(root);
        //    }
        //    var dict = prop.GetValue(owner, null) as System.Collections.IList;
        //    dict.RemoveAt(index);
        //}
        //private static void AddList(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, int index, object value)
        //{
        //    List<object> ownerChain = new List<object>();
        //    ownerChain.Add(null);
        //    ownerChain.Add(root);
        //    for (int i = 1; i < callstack.Count; i++)
        //    {
        //        root = callstack[i].Value.GetValue(root);
        //        if (root == null)
        //            return;
        //        ownerChain.Add(root);
        //    }
        //    var dict = prop.GetValue(owner, null) as System.Collections.IList;
        //    dict.Insert(index, value);
        //}
        //private static void SetListValue(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, int index, object value)
        //{
        //    List<object> ownerChain = new List<object>();
        //    ownerChain.Add(null);
        //    ownerChain.Add(root);
        //    for (int i = 1; i < callstack.Count; i++)
        //    {
        //        root = callstack[i].Value.GetValue(root);
        //        if (root == null)
        //            return;
        //        ownerChain.Add(root);
        //    }
        //    var dict = prop.GetValue(owner, null) as System.Collections.IList;
        //    dict[index] = value;
        //}
        //public static void RemoveDictionary(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, object key)
        //{
        //    List<object> ownerChain = new List<object>();
        //    ownerChain.Add(null);
        //    ownerChain.Add(root);
        //    for (int i = 1; i < callstack.Count; i++)
        //    {
        //        root = callstack[i].Value.GetValue(root);
        //        if (root == null)
        //            return;
        //        ownerChain.Add(root);
        //    }
        //    var dict = prop.GetValue(owner, null) as System.Collections.IDictionary;
        //    dict.Remove(key);
        //}
        //public static void SetDictionaryValue(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, object key, object value)
        //{
        //    List<object> ownerChain = new List<object>();
        //    ownerChain.Add(null);
        //    ownerChain.Add(root);
        //    for (int i = 1; i < callstack.Count; i++)
        //    {
        //        root = callstack[i].Value.GetValue(root);
        //        if (root == null)
        //            return;
        //        ownerChain.Add(root);
        //    }
        //    var dict = prop.GetValue(owner, null) as System.Collections.IDictionary;
        //    try
        //    {
        //        dict[key] = value;
        //    }
        //    catch
        //    {

        //    }
        //}
        public static void SetValue(PropertyGrid pg, object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, object value)
        {
            if (pg.IsReadOnly)
                return;

            PGCustomValueEditorAttribute editorOnDraw = null;
            var attrs = prop.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
                if (editorOnDraw.HideInPG)
                    return;
            }
            bool isReadOnly = (editorOnDraw != null && editorOnDraw.ReadOnly);
            if (isReadOnly)
                return;

            List<object> ownerChain = new List<object>();
            ownerChain.Add(null);
            ownerChain.Add(root);
            for (int i=1; i<callstack.Count; i++)
            {
                root = callstack[i].Value.GetValue(root);
                if (root == null)
                    return;
                ownerChain.Add(root);
            }
            if (prop.CanWrite && prop.SetMethod.IsPublic)
            {   
                prop.SetValue(owner, value);
            }
            int curStack = callstack.Count - 1;
            while (owner.GetType().IsValueType && curStack>=0)
            {
                value = owner;
                prop = callstack[curStack].Value;
                if (prop == null)
                    break;
                owner = ownerChain[curStack];
                if (prop.CanWrite)
                {
                    prop.SetValue(owner, value);
                }
                curStack--;
            }
        }
    }
}
