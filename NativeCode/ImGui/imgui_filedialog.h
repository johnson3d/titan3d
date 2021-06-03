#pragma once
#include "../Base/TypeUtility.h"
#include "ImGuiFileDialog.h"
#include "imgui.h"
using namespace IGFD;
namespace ImGui
{
	class TR_CLASS(SV_Dispose = delete self)
	ImGuiFileDialog
	{
	public:
			ImGuiFileDialog();
		~ImGuiFileDialog();
	public:
		void OpenDialog(
			const char* vKey,
			const char* vTitle,
			const char* vFilters,
			const char* vPath);
		// modal dialog
		void OpenModal(
			const char* vKey,
			const char* vTitle,
			const char* vFilters,
			const char* vPath);
		bool DisplayDialog(const char* vKey);
		void CloseDialog();
		bool IsOk();
		bool WasKeyOpenedThisFrame(const char* vKey);
		bool WasOpenedThisFrame();
		bool IsKeyOpened(const char* vCurrentOpenedKey);
		bool IsOpened();
		const char* GetFilePathName();
		const char* GetCurrentFileName();
		const char* GetCurrentPath();
		const char* GetCurrentFilter();
		void* GetUserDatas();
		void SetExtentionInfos(
			const char* vFilter,
			ImVec4 vColor,
			const char* vIconText);
		bool GetExtentionInfos(
			const char* vFilter,
			ImVec4* vOutColor,
			char** vOutIconText);
		void ClearExtentionInfos();

	private:
		std::string mFilePathName;
		std::string mCurrentPath;
		std::string mCurrentFileName;
		std::string mCurrentFilter;
		FileDialog* mDialog = nullptr;
	};
};

