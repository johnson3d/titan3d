#include "imgui_binding.h"

#if defined(PLATFORM_WIN)
#include "imgui_impl_win32.cpp"
#endif

NS_BEGIN

StructImpl(ImVec2)
StructImpl(ImVec4)

StructImpl(items_getter)
StructImpl(values_getter)
StructImpl(alloc_func)
StructImpl(free_func)
StructImpl(ImGuiSizeCallback)
StructImpl(ImGuiInputTextCallback)
StructImpl(ImDrawCallback)

StructImpl(Renderer_CreateWindow)
StructImpl(Renderer_SetWindowSize)
StructImpl(Renderer_RenderWindow)

StructImpl(FGetClipboardTextFn)
StructImpl(FSetClipboardTextFn)

//AuxRttiStruct<void __cdecl(struct ImGuiSizeCallbackData *)> AuxRttiStruct<void __cdecl(struct ImGuiSizeCallbackData *)>::Instance;
//AuxRttiStruct<bool __cdecl(void *, int, char const * *)> AuxRttiStruct<bool __cdecl(void *, int, char const * *)>::Instance;
//AuxRttiStruct<int __cdecl(struct ImGuiInputTextCallbackData *)> AuxRttiStruct<int __cdecl(struct ImGuiInputTextCallbackData *)>::Instance;
//AuxRttiStruct<float __cdecl(void *, int)> AuxRttiStruct<float __cdecl(void *, int)>::Instance;
//AuxRttiStruct<void * __cdecl(unsigned __int64, void *)> AuxRttiStruct<void * __cdecl(unsigned __int64, void *)>::Instance;
//AuxRttiStruct<void __cdecl(void *, void *)> AuxRttiStruct<void __cdecl(void *, void *)>::Instance;

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
	if (ImGui::IsKeyDown(ImGuiKey_::ImGuiKey_Backspace)) 
	{
		//We delete the last char automatically, since it is what the user wants to delete, but only if there is something (selected/marked/hovered)
		//FIXME: This worked fine, when not used as helper function
		if (data->SelectionEnd != data->SelectionStart)
			if (data->BufTextLen > 0) //...and the buffer isn't empty			
				if (data->CursorPos > 0) //...and the cursor not at pos 0
					data->DeleteChars(data->CursorPos - 1, 1);
		return 0;
	}
	if (ImGui::IsKeyDown(ImGuiKey_::ImGuiKey_Delete))
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
				strcpy_s((char*)buffer, strlen(items[i]), items[i]);
			}
		}

		ImGui::EndPopup();
	}
	ImGui::PopID();

	ImGui::SameLine(0, ImGui::GetStyle().ItemInnerSpacing.x);
	ImGui::Text(id);

	return ret;
}

NS_END

