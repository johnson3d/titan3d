#include "imgui_binding.h"

#if defined(PLATFORM_WIN)
#include "imgui_impl_win32.cpp"
#endif

NS_BEGIN

static const float          DRAG_MOUSE_THRESHOLD_FACTOR = 0.50f;    // Multiplier for the default value of io.MouseDragThreshold to make DragFloat/DragInt react faster to mouse drags.

void	ImGuiAPI::ImGui_NativeWindow_EnableDpiAwareness()
{
#if defined(PLATFORM_WIN)
	ImGui_ImplWin32_EnableDpiAwareness();
#endif
}

bool     ImGuiAPI::ImGui_NativeWindow_Init(void* hwnd)
{
#if defined(PLATFORM_WIN)
	return ImGui_ImplWin32_Init(hwnd);
#else
	return false;
#endif
}
void     ImGuiAPI::ImGui_NativeWindow_Shutdown()
{
#if defined(PLATFORM_WIN)
	ImGui_ImplWin32_Shutdown();
#endif
}
void     ImGuiAPI::ImGui_NativeWindow_NewFrame()
{
#if defined(PLATFORM_WIN)
	ImGui_ImplWin32_NewFrame();
#endif
}

bool identical(const char* buf, const char* item) {
	size_t buf_size = strlen(buf);
	size_t item_size = strlen(item);
	//Check if the item length is shorter or equal --> exclude
	if (buf_size >= item_size) return false;
	for (int i = 0; i < strlen(buf); ++i)
		// set the current pos if matching or return the pos if not
		if (buf[i] != item[i]) return false;
	// Complete match
	// and the item size is greater --> include
	return true;
};

int propose(ImGuiInputTextCallbackData* data) {
	//We don't want to "preselect" anything
	if (strlen(data->Buf) == 0) return 0;

	//Get our items back
	const char** items = static_cast<std::pair<const char**, size_t>*> (data->UserData)->first;
	size_t length = static_cast<std::pair<const char**, size_t>*> (data->UserData)->second;

	//We need to give the user a chance to remove wrong input
	//We use SFML Keycodes here, because the Imgui Keys aren't working the way I thought they do...	
	if (ImGui::IsKeyDown(ImGuiKey::ImGuiKey_Backspace)) 
	{
		//We delete the last char automatically, since it is what the user wants to delete, but only if there is something (selected/marked/hovered)
		//FIXME: This worked fine, when not used as helper function
		if (data->SelectionEnd != data->SelectionStart)
			if (data->BufTextLen > 0) //...and the buffer isn't empty			
				if (data->CursorPos > 0) //...and the cursor not at pos 0
					data->DeleteChars(data->CursorPos - 1, 1);
		return 0;
	}
	if (ImGui::IsKeyDown(ImGuiKey::ImGuiKey_Delete))
		return 0; //TODO: Replace with imgui key

	for (int i = 0; i < length; i++) {
		if (identical(data->Buf, items[i])) {
			const int cursor = data->CursorPos;
			//Insert the first match
			data->DeleteChars(0, data->BufTextLen);
			data->InsertChars(0, items[i]);
			//Reset the cursor position
			data->CursorPos = cursor;
			//Select the text, so the user can simply go on writing
			data->SelectionStart = cursor;
			data->SelectionEnd = data->BufTextLen;
			break;
		}
	}
	return 0;
}

bool ImGuiAPI::TextInputComboBox(const char* id, void* buffer, UINT maxInputSize, const char** items, UINT item_len, short showMaxItems)
{
	//Check if both strings matches
	if (showMaxItems == 0)
	{
		showMaxItems = (short)item_len;
	}

	ImGui::PushID(id);
	std::pair<const char**, size_t> pass(items, item_len); //We need to pass the array length as well

	bool ret = ImGui::InputText("##in", (char*)buffer, maxInputSize, ImGuiInputTextFlags_::ImGuiInputTextFlags_CallbackAlways, propose, static_cast<void*>(&pass));

	ImGui::OpenPopupOnItemClick("combobox"); //Enable right-click
	ImVec2 pos = ImGui::GetItemRectMin();
	ImVec2 size = ImGui::GetItemRectSize();

	ImGui::SameLine(0, 0);
	if (ImGui::ArrowButton("##openCombo", ImGuiDir_Down)) 
	{
		ImGui::OpenPopup("combobox");
	}
	ImGui::OpenPopupOnItemClick("combobox"); //Enable right-click

	pos.y += size.y;
	size.x += ImGui::GetItemRectSize().x;
	size.y += 5 + (size.y * showMaxItems);
	ImGui::SetNextWindowPos(pos);
	ImGui::SetNextWindowSize(size);
	if (ImGui::BeginPopup("combobox", ImGuiWindowFlags_::ImGuiWindowFlags_NoMove)) 
	{
		ImGui::Text("Select one item or type");
		ImGui::Separator();
		for (UINT i = 0; i < item_len; i++)
		{
			if (ImGui::Selectable(items[i]))
			{
#if defined(PLATFORM_WIND)
				strcpy_s((char*)buffer, strlen(items[i]), items[i]);
#else
				strcpy((char*)buffer, items[i]);
#endif
			}
		}

		ImGui::EndPopup();
	}
	ImGui::PopID();

	ImGui::SameLine(0, ImGui::GetStyle().ItemInnerSpacing.x);
	ImGui::Text("%s", id);

	return ret;
}

static const char* PatchFormatStringFloatToInt(const char* fmt)
{
	if (fmt[0] == '%' && fmt[1] == '.' && fmt[2] == '0' && fmt[3] == 'f' && fmt[4] == 0) // Fast legacy path for "%.0f" which is expected to be the most common case.
		return "%d";
	const char* fmt_start = ImParseFormatFindStart(fmt);    // Find % (if any, and ignore %%)
	const char* fmt_end = ImParseFormatFindEnd(fmt_start);  // Find end of format specifier, which itself is an exercise of confidence/recklessness (because snprintf is dependent on libc or user).
	if (fmt_end > fmt_start && fmt_end[-1] == 'f')
	{
#ifndef IMGUI_DISABLE_OBSOLETE_FUNCTIONS
		if (fmt_start == fmt && fmt_end[0] == 0)
			return "%d";
		ImGuiContext* g = GImGui;
		ImFormatString(GImGui->TempBuffer.Data, IM_ARRAYSIZE(GImGui->TempBuffer.Data), "%.*s%%d%s", (int)(fmt_start - fmt), fmt, fmt_end); // Honor leading and trailing decorations, but lose alignment/precision.
		return GImGui->TempBuffer.Data;
#else
		IM_ASSERT(0 && "DragInt(): Invalid format string!"); // Old versions used a default parameter of "%.0f", please replace with e.g. "%d"
#endif
	}
	return fmt;
}

static const ImGuiDataTypeInfo GDataTypeInfo[] =
{
	{ sizeof(char),             "S8",   "%d",   "%d"    },  // ImGuiDataType_S8
	{ sizeof(unsigned char),    "U8",   "%u",   "%u"    },
	{ sizeof(short),            "S16",  "%d",   "%d"    },  // ImGuiDataType_S16
	{ sizeof(unsigned short),   "U16",  "%u",   "%u"    },
	{ sizeof(int),              "S32",  "%d",   "%d"    },  // ImGuiDataType_S32
	{ sizeof(unsigned int),     "U32",  "%u",   "%u"    },
#ifdef _MSC_VER
	{ sizeof(ImS64),            "S64",  "%I64d","%I64d" },  // ImGuiDataType_S64
	{ sizeof(ImU64),            "U64",  "%I64u","%I64u" },
#else
	{ sizeof(ImS64),            "S64",  "%lld", "%lld"  },  // ImGuiDataType_S64
	{ sizeof(ImU64),            "U64",  "%llu", "%llu"  },
#endif
	{ sizeof(float),            "float", "%.3f","%f"    },  // ImGuiDataType_Float (float are promoted to double in va_arg)
	{ sizeof(double),           "double","%f",  "%lf"   },  // ImGuiDataType_Double
};
IM_STATIC_ASSERT(IM_ARRAYSIZE(GDataTypeInfo) == ImGuiDataType_COUNT); 
bool ImGuiAPI::DragScalar2(const char* label, ImGuiDataType data_type, void* p_data, float v_speed, const void* p_min, const void* p_max, const char* format, ImGuiSliderFlags_ flags)
{
	ImGuiWindow* window = ImGui::GetCurrentWindow();
	if (window->SkipItems)
		return false;

	ImGuiContext& g = *GImGui;
	const ImGuiStyle& style = g.Style;
	const ImGuiID id = window->GetID(label);
	const float w = ImGui::CalcItemWidth();

	const ImVec2 label_size = ImGui::CalcTextSize(label, NULL, true);
	const ImRect frame_bb(window->DC.CursorPos, window->DC.CursorPos + ImVec2(w, label_size.y + style.FramePadding.y * 2.0f));
	const ImRect total_bb(frame_bb.Min, frame_bb.Max + ImVec2(label_size.x > 0.0f ? style.ItemInnerSpacing.x + label_size.x : 0.0f, 0.0f));

	const bool temp_input_allowed = (flags & ImGuiSliderFlags_NoInput) == 0;
	ImGui::ItemSize(total_bb, style.FramePadding.y);
	//if (!ImGui::ItemAdd(total_bb, id, &frame_bb,temp_input_allowed ? ImGuiItemAddFlags_Focusable : 0))
	if (!ImGui::ItemAdd(total_bb, id, &frame_bb, temp_input_allowed ? ImGuiItemFlags_Inputable : 0))
		return false;

	// Default format string when passing NULL
	if (format == NULL)
		format = ImGui::DataTypeGetInfo(data_type)->PrintFmt;
	else if (data_type == ImGuiDataType_S32 && strcmp(format, "%d") != 0) // (FIXME-LEGACY: Patch old "%.0f" format string to use "%d", read function more details.)
		format = PatchFormatStringFloatToInt(format);

	// Tabbing or CTRL-clicking on Drag turns it into an InputText
	const bool hovered = ImGui::ItemHoverable(frame_bb, id);
	if(hovered)
		ImGui::SetMouseCursor(ImGuiMouseCursor_ResizeEW);
	bool temp_input_is_active = temp_input_allowed && ImGui::TempInputIsActive(id);
	if (!temp_input_is_active)
	{
		bool lastFocused = (ImGui::GetCurrentContext()->LastItemData.StatusFlags & ImGuiItemStatusFlags_Deactivated) != 0;
		//const bool focus_requested = temp_input_allowed && (window->DC.LastItemStatusFlags & ImGuiItemStatusFlags_Focused) != 0;
		const bool focus_requested = temp_input_allowed && lastFocused;
		const bool clicked = (hovered && g.IO.MouseClicked[0]);
		const bool double_clicked = (hovered && g.IO.MouseDoubleClicked[0]);
		if (focus_requested || clicked || double_clicked || g.NavActivateId == id || g.NavActivateInputId == id)
		{
			ImGui::SetActiveID(id, window);
			ImGui::SetFocusID(id, window);
			ImGui::FocusWindow(window);
			g.ActiveIdUsingNavDirMask = (1 << ImGuiDir_Left) | (1 << ImGuiDir_Right);
			//if (temp_input_allowed && (focus_requested || (clicked && g.IO.KeyCtrl) || double_clicked || g.NavInputId == id))
			//	temp_input_is_active = true;
		}
		// Experimental: simple click (without moving) turns Drag into an InputText
		// FIXME: Currently polling ImGuiConfigFlags_IsTouchScreen, may either poll an hypothetical ImGuiBackendFlags_HasKeyboard and/or an explicit drag settings.
		if (temp_input_allowed && !temp_input_is_active)
			if (g.ActiveId == id && hovered && g.IO.MouseReleased[0] && !ImGui::IsMouseDragPastThreshold(0, g.IO.MouseDragThreshold * DRAG_MOUSE_THRESHOLD_FACTOR))
			{
				g.NavActivateInputId = id;
				temp_input_is_active = true;
			}
	}

	if (temp_input_is_active)
	{
		// Only clamp CTRL+Click input when ImGuiSliderFlags_AlwaysClamp is set
		const bool is_clamp_input = (flags & ImGuiSliderFlags_AlwaysClamp) != 0 && (p_min == NULL || p_max == NULL || ImGui::DataTypeCompare(data_type, p_min, p_max) < 0);
		return ImGui::TempInputScalar(frame_bb, id, label, data_type, p_data, format, is_clamp_input ? p_min : NULL, is_clamp_input ? p_max : NULL);
	}

	// Draw frame
	const ImU32 frame_col = ImGui::GetColorU32(g.ActiveId == id ? ImGuiCol_FrameBgActive : g.HoveredId == id ? ImGuiCol_FrameBgHovered : ImGuiCol_FrameBg);
	ImGui::RenderNavHighlight(frame_bb, id);
	ImGui::RenderFrame(frame_bb.Min, frame_bb.Max, frame_col, true, style.FrameRounding);

	// Drag behavior
	const bool value_changed = ImGui::DragBehavior(id, data_type, p_data, v_speed, p_min, p_max, format, flags);
	if (value_changed)
		ImGui::MarkItemEdited(id);

	// Display value using user-provided display format so user can add prefix/suffix/decorations to the value.
	char value_buf[64];
	const char* value_buf_end = value_buf + ImGui::DataTypeFormatString(value_buf, IM_ARRAYSIZE(value_buf), data_type, p_data, format);
	if (g.LogEnabled)
		ImGui::LogSetNextTextDecoration("{", "}");
	ImVec2 delta = ImVec2(style.FramePadding.x, 0);
	ImRect clipRect(frame_bb.Min + delta, frame_bb.Max - delta);
	ImGui::RenderTextClipped(clipRect.Min, clipRect.Max, value_buf, value_buf_end, NULL, ImVec2(0.0f, 0.5f), &clipRect);

	if (label_size.x > 0.0f)
		ImGui::RenderText(ImVec2(frame_bb.Max.x + style.ItemInnerSpacing.x, frame_bb.Min.y + style.FramePadding.y), label);

	IMGUI_TEST_ENGINE_ITEM_INFO(id, label, window->DC.LastItemStatusFlags);
	return value_changed;
}
bool ImGuiAPI::DragScalarN2(const char* label, ImGuiDataType data_type, void* p_data, int components, float v_speed, const void* p_min, const void* p_max, const char* format, ImGuiSliderFlags_ flags)
{
	ImGuiWindow* window = ImGui::GetCurrentWindow();
	if (window->SkipItems)
		return false;

	ImGuiContext& g = *GImGui;
	bool value_changed = false;
	ImGui::BeginGroup();
	ImGui::PushID(label);
	ImGui::PushMultiItemsWidths(components, ImGui::CalcItemWidth());
	size_t type_size = GDataTypeInfo[data_type].Size;
	for (int i = 0; i < components; i++)
	{
		ImGui::PushID(i);
		if (i > 0)
			ImGui::SameLine(0, g.Style.ItemInnerSpacing.x);
		value_changed |= DragScalar2("", data_type, p_data, v_speed, p_min, p_max, format, flags);
		ImGui::PopID();
		ImGui::PopItemWidth();
		p_data = (void*)((char*)p_data + type_size);
	}
	ImGui::PopID();

	const char* label_end = ImGui::FindRenderedTextEnd(label);
	if (label != label_end)
	{
		ImGui::SameLine(0, g.Style.ItemInnerSpacing.x);
		ImGui::TextEx(label, label_end);
	}

	ImGui::EndGroup();
	return value_changed;
}
bool ImGuiAPI::CollapsingHeader_SpanAllColumns(const char* label, ImGuiTreeNodeFlags_ flags)
{
	ImGuiWindow* window = ImGui::GetCurrentWindow();
	if (window->SkipItems)
		return false;

	const ImVec2& pos = window->DC.CursorPos;
	const float min_x = pos.x;
	const float max_x = window->ParentWorkRect.Max.x;

	ImGuiContext& g = *GImGui;

	window->ClipRect.Min.x = min_x;
	window->ClipRect.Max.x = max_x;
	auto workRectMaxXStore = window->WorkRect.Max.x;
	window->WorkRect.Max.x = max_x;

	float cellPaddingYStore = 0.0f;
	if (window->DC.CurrentColumns)
		ImGui::PushColumnsBackground();
	else if (g.CurrentTable)
	{
		cellPaddingYStore = g.CurrentTable->CellPaddingY;
		g.CurrentTable->CellPaddingY = 0;
		ImGui::TablePushBackgroundChannel();
	}

	bool retValue = ImGui::TreeNodeBehavior(window->GetID(label), flags | ImGuiTreeNodeFlags_CollapsingHeader, label);

	if (window->DC.CurrentColumns)
		ImGui::PopColumnsBackground();
	else if (g.CurrentTable)
	{
		ImGui::TablePopBackgroundChannel();
		g.CurrentTable->CellPaddingY = cellPaddingYStore;
	}

	window->WorkRect.Max.x = workRectMaxXStore;

	return retValue;
}

void ImGuiAPI::TableNextRow(const ImGuiTableRowData* rowData)
{
	ImGuiContext& g = *GImGui;
	ImGuiTable* table = g.CurrentTable;
	auto cellPaddingY = table->CellPaddingY;
	if (!table->IsLayoutLocked)
		ImGui::TableUpdateLayout(table);
	if (table->IsInsideRow)
	{
		table->CellPaddingY = rowData->CellPaddingYEnd;
		ImGui::TableEndRow(table);
	}

	const ImRect& workRect = table->WorkRect;
	ImRect rect(ImVec2(workRect.Min.x, table->RowPosY1), ImVec2(workRect.Max.x, table->RowPosY2));
	if(IsMouseHoveringRectInCurrentWindow(&rect.Min, &rect.Max, false))
	//if (ImGui::ItemHoverable(rect, table->ID))
	{
		g.CurrentWindow->DrawList->AddRectFilled(rect.Min, rect.Max, rowData->HoverColor);
	}

	if (g.Style.IndentSpacing > 0.0f && rowData->IndentTextureId)
	{
		auto indentCount = (int)(table->RowIndentOffsetX / g.Style.IndentSpacing);
		for (int indentIdx = 1; indentIdx <= indentCount; indentIdx++)
		{
			//g.CurrentWindow->DrawList->AddRectFilledMultiColor(
			//	ImVec2(indentIdx * g.Style.IndentSpacing - rowData->IndentImageWidth + rect.Min.x, rect.Min.y),
			//	ImVec2(indentIdx * g.Style.IndentSpacing + rect.Min.x, rect.Max.y),
			//	rowData->IndentColorLeft, rowData->IndentColorRight, rowData->IndentColorRight, rowData->IndentColorLeft);
			g.CurrentWindow->DrawList->AddImage(rowData->IndentTextureId,
				ImVec2(indentIdx * g.Style.IndentSpacing - rowData->IndentImageWidth + rect.Min.x, rect.Min.y),
				ImVec2(indentIdx * g.Style.IndentSpacing + rect.Min.x, rect.Max.y),
				rowData->IndentTextureUVMin, rowData->IndentTextureUVMax, rowData->IndentColor);
		}
	}

	table->LastRowFlags = table->RowFlags;
	table->RowFlags = rowData->Flags;
	table->RowMinHeight = rowData->MinHeight;
	table->CellPaddingY = rowData->CellPaddingYBegin;
	ImGui::TableBeginRow(table);

	// We honor min_row_height requested by user, but cannot guarantee per-row maximum height,
	// because that would essentially require a unique clipping rectangle per-cell.
	table->RowPosY2 += table->CellPaddingY * 2.0f;
	table->RowPosY2 = ImMax(table->RowPosY2, table->RowPosY1 + rowData->MinHeight);

	// Disable output until user calls TableNextColumn()
	table->InnerWindow->SkipItems = true;

	table->CellPaddingY = cellPaddingY;
}
void ImGuiAPI::TableNextRow_FirstColumn(const ImGuiTableRowData* rowData)
{
	ImGuiContext& g = *GImGui;
	//ImGuiWindow* window = g.CurrentWindow;
	ImGuiTable* table = g.CurrentTable;
	if (table == nullptr)
		return;

	TableNextRow(rowData);
	auto cellPaddingYStore = table->CellPaddingY;
	table->CellPaddingY = rowData->CellPaddingYBegin;
	ImGui::TableSetColumnIndex(0);
	table->CellPaddingY = cellPaddingYStore;
}
bool ImGuiAPI::ToggleButton(const char* label, bool* v, const ImVec2* size_arg, ImGuiButtonFlags flags)
{
	ImGuiWindow* window = ImGui::GetCurrentWindow();
	if (window->SkipItems)
		return false;

	ImGuiContext& g = *GImGui;
	const ImGuiStyle& style = g.Style;
	const ImGuiID id = window->GetID(label);
	const ImVec2 label_size = ImGui::CalcTextSize(label, NULL, true);

	ImVec2 pos = window->DC.CursorPos;
	if ((flags & ImGuiButtonFlags_AlignTextBaseLine) && style.FramePadding.y < window->DC.CurrLineTextBaseOffset)
		pos.y += window->DC.CurrLineTextBaseOffset - style.FramePadding.y;
	ImVec2 size = ImGui::CalcItemSize(*size_arg, label_size.x + style.FramePadding.x * 2.0f, label_size.y + style.FramePadding.y * 2.0f);

	const ImRect bb(pos, pos + size);
	ImGui::ItemSize(size, style.FramePadding.y);
	if (!ImGui::ItemAdd(bb, id))
		return false;

	if (g.CurrentItemFlags & ImGuiItemFlags_ButtonRepeat)
		flags |= ImGuiButtonFlags_Repeat;
	bool hovered, held;
	bool pressed = ImGui::ButtonBehavior(bb, id, &hovered, &held, flags);
	if (pressed)
	{
		*v = !(*v);
		ImGui::MarkItemEdited(id);
	}

	const ImU32 col = ImGui::GetColorU32((held && hovered) ? ImGuiCol_ButtonActive : hovered ? ImGuiCol_ButtonHovered : (*v)? ImGuiCol_ButtonActive : ImGuiCol_Button);
	ImGui::RenderNavHighlight(bb, id);
	ImGui::RenderFrame(bb.Min, bb.Max, col, true, style.FrameRounding);

	if (g.LogEnabled)
		ImGui::LogSetNextTextDecoration("[", "]");
	ImGui::RenderTextClipped(bb.Min + style.FramePadding, bb.Max - style.FramePadding, label, NULL, &label_size, style.ButtonTextAlign, &bb);

	IMGUI_TEST_ENGINE_ITEM_INFO(id, label, window->DC.LastItemStatusFlags);
	return pressed;
}

void ImGuiAPI::SetKeyOwner(ImGuiKey key, ImGuiID owner_id, ImGuiInputFlags flags)
{
	ImGui::SetKeyOwner(key, owner_id, flags);
}

bool ImGuiAPI::IsIDNavInput(ImGuiID id)
{
	auto context = ImGui::GetCurrentContext();
	if (context == nullptr)
		return false;
	return context->NavActivateId == id;
}

static float CalcMaxPopupHeightFromItemCount(int items_count)
{
	ImGuiContext& g = *GImGui;
	if (items_count <= 0)
		return FLT_MAX;
	return (g.FontSize + g.Style.ItemSpacing.y) * items_count - g.Style.ItemSpacing.y + (g.Style.WindowPadding.y * 2);
}
static bool BeginComboPopup(ImGuiID popup_id, const ImRect& bb, ImGuiComboFlags flags, ImGuiWindowFlags window_flags)
{
	ImGuiContext& g = *GImGui;
	if (!ImGui::IsPopupOpen(popup_id, ImGuiPopupFlags_None))
	{
		g.NextWindowData.ClearFlags();
		return false;
	}

	// Set popup size
	float w = bb.GetWidth();
	if (g.NextWindowData.Flags & ImGuiNextWindowDataFlags_HasSizeConstraint)
	{
		g.NextWindowData.SizeConstraintRect.Min.x = ImMax(g.NextWindowData.SizeConstraintRect.Min.x, w);
	}
	else if((window_flags & ImGuiWindowFlags_AlwaysAutoResize) != 0)
	{
		if ((flags & ImGuiComboFlags_HeightMask_) == 0)
			flags |= ImGuiComboFlags_HeightRegular;
		IM_ASSERT(ImIsPowerOfTwo(flags & ImGuiComboFlags_HeightMask_)); // Only one
		int popup_max_height_in_items = -1;
		if (flags & ImGuiComboFlags_HeightRegular)     popup_max_height_in_items = 8;
		else if (flags & ImGuiComboFlags_HeightSmall)  popup_max_height_in_items = 4;
		else if (flags & ImGuiComboFlags_HeightLarge)  popup_max_height_in_items = 20;
		ImGui::SetNextWindowSizeConstraints(ImVec2(w, 0.0f), ImVec2(FLT_MAX, CalcMaxPopupHeightFromItemCount(popup_max_height_in_items)));
	}

	// This is essentially a specialized version of BeginPopupEx()
	char name[16];
	ImFormatString(name, IM_ARRAYSIZE(name), "##Combo_%02d", g.BeginPopupStack.Size); // Recycle windows based on depth

	// Set position given a custom constraint (peak into expected window size so we can position it)
	// FIXME: This might be easier to express with an hypothetical SetNextWindowPosConstraints() function?
	// FIXME: This might be moved to Begin() or at least around the same spot where Tooltips and other Popups are calling FindBestWindowPosForPopupEx()?
	if (ImGuiWindow* popup_window = ImGui::FindWindowByName(name))
		if (popup_window->WasActive)
		{
			// Always override 'AutoPosLastDirection' to not leave a chance for a past value to affect us.
			ImVec2 size_expected = ImGui::CalcWindowNextAutoFitSize(popup_window);
			popup_window->AutoPosLastDirection = (flags & ImGuiComboFlags_PopupAlignLeft) ? ImGuiDir_Left : ImGuiDir_Down; // Left = "Below, Toward Left", Down = "Below, Toward Right (default)"
			ImRect r_outer = ImGui::GetPopupAllowedExtentRect(popup_window);
			ImVec2 pos = ImGui::FindBestWindowPosForPopupEx(bb.GetBL(), size_expected, &popup_window->AutoPosLastDirection, r_outer, bb, ImGuiPopupPositionPolicy_ComboBox);
			ImGui::SetNextWindowPos(pos);
		}

	// We don't use BeginPopupEx() solely because we have a custom name string, which we could make an argument to BeginPopupEx()
	//ImGuiWindowFlags window_flags = ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_Popup | ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_NoMove;
	ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(g.Style.FramePadding.x, g.Style.WindowPadding.y)); // Horizontally align ourselves with the framed text
	bool ret = ImGui::Begin(name, NULL, window_flags);
	ImGui::PopStyleVar();
	if (!ret)
	{
		ImGui::EndPopup();
		IM_ASSERT(0);   // This should never happen as we tested for IsPopupOpen() above
		return false;
	}
	return true;
}
bool ImGuiAPI::BeginCombo(const char* label, const char* preview_value, ImGuiComboFlags_ flags, ImGuiWindowFlags_ winFlags)
{
	ImGuiContext& g = *GImGui;
	ImGuiWindow* window = ImGui::GetCurrentWindow();

	ImGuiNextWindowDataFlags backup_next_window_data_flags = g.NextWindowData.Flags;
	g.NextWindowData.ClearFlags(); // We behave like Begin() and need to consume those values
	if (window->SkipItems)
		return false;

	const ImGuiStyle& style = g.Style;
	const ImGuiID id = window->GetID(label);
	IM_ASSERT((flags & (ImGuiComboFlags_NoArrowButton | ImGuiComboFlags_NoPreview)) != (ImGuiComboFlags_NoArrowButton | ImGuiComboFlags_NoPreview)); // Can't use both flags together

	const float arrow_size = (flags & ImGuiComboFlags_NoArrowButton) ? 0.0f : GetFrameHeight();
	const ImVec2 label_size = ImGui::CalcTextSize(label, NULL, true);
	const float w = (flags & ImGuiComboFlags_NoPreview) ? arrow_size : CalcItemWidth();
	const ImRect bb(window->DC.CursorPos, window->DC.CursorPos + ImVec2(w, label_size.y + style.FramePadding.y * 2.0f));
	const ImRect total_bb(bb.Min, bb.Max + ImVec2(label_size.x > 0.0f ? style.ItemInnerSpacing.x + label_size.x : 0.0f, 0.0f));
	ImGui::ItemSize(total_bb, style.FramePadding.y);
	if (!ImGui::ItemAdd(total_bb, id, &bb))
		return false;

	// Open on click
	bool hovered, held;
	bool pressed = ImGui::ButtonBehavior(bb, id, &hovered, &held);
	const ImGuiID popup_id = ImHashStr("##ComboPopup", 0, id);
	bool popup_open = ImGui::IsPopupOpen(popup_id, ImGuiPopupFlags_None);
	if (pressed && !popup_open)
	{
		ImGui::OpenPopupEx(popup_id, ImGuiPopupFlags_None);
		popup_open = true;
	}

	// Render shape
	const ImU32 frame_col = GetColorU32(hovered ? ImGuiCol_FrameBgHovered : ImGuiCol_FrameBg);
	const float value_x2 = ImMax(bb.Min.x, bb.Max.x - arrow_size);
	ImGui::RenderNavHighlight(bb, id);
	if (!(flags & ImGuiComboFlags_NoPreview))
		window->DrawList->AddRectFilled(bb.Min, ImVec2(value_x2, bb.Max.y), frame_col, style.FrameRounding, (flags & ImGuiComboFlags_NoArrowButton) ? ImDrawFlags_RoundCornersAll : ImDrawFlags_RoundCornersLeft);
	if (!(flags & ImGuiComboFlags_NoArrowButton))
	{
		ImU32 bg_col = GetColorU32((popup_open || hovered) ? ImGuiCol_ButtonHovered : ImGuiCol_Button);
		ImU32 text_col = GetColorU32(ImGuiCol_Text);
		window->DrawList->AddRectFilled(ImVec2(value_x2, bb.Min.y), bb.Max, bg_col, style.FrameRounding, (w <= arrow_size) ? ImDrawFlags_RoundCornersAll : ImDrawFlags_RoundCornersRight);
		if (value_x2 + arrow_size - style.FramePadding.x <= bb.Max.x)
			ImGui::RenderArrow(window->DrawList, ImVec2(value_x2 + style.FramePadding.y, bb.Min.y + style.FramePadding.y), text_col, ImGuiDir_Down, 1.0f);
	}
	ImGui::RenderFrameBorder(bb.Min, bb.Max, style.FrameRounding);

	// Custom preview
	if (flags & ImGuiComboFlags_CustomPreview)
	{
		g.ComboPreviewData.PreviewRect = ImRect(bb.Min.x, bb.Min.y, value_x2, bb.Max.y);
		IM_ASSERT(preview_value == NULL || preview_value[0] == 0);
		preview_value = NULL;
	}

	// Render preview and label
	if (preview_value != NULL && !(flags & ImGuiComboFlags_NoPreview))
	{
		if (g.LogEnabled)
			ImGui::LogSetNextTextDecoration("{", "}");
		ImGui::RenderTextClipped(bb.Min + style.FramePadding, ImVec2(value_x2, bb.Max.y), preview_value, NULL, NULL);
	}
	if (label_size.x > 0)
		ImGui::RenderText(ImVec2(bb.Max.x + style.ItemInnerSpacing.x, bb.Min.y + style.FramePadding.y), label);

	if (!popup_open)
		return false;

	g.NextWindowData.Flags = backup_next_window_data_flags;
	return BeginComboPopup(popup_id, bb, flags, winFlags);
}

//bool ImGuiAPI::LastItemStatusFlagsHasFocused()
//{
//	ImGuiWindow* window = ImGui::GetCurrentWindow();
//	if (window == nullptr)
//		return false;
//	return (window->DC.LastItemStatusFlags & ImGuiItemStatusFlags_Focused) != 0;
//}

NS_END

