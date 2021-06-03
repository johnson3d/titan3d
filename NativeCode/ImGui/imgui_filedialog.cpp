#include "imgui_filedialog.h"
#include "imgui.h"
#include "ImGuiFileDialog.h"
#include "../../Base/CoreSDK.h"
#include "../../Base/debug/vfxnew.h"

#define new VNEW

namespace ImGui
{
	ImGuiFileDialog::ImGuiFileDialog()
	{
		mDialog = new IGFD::FileDialog();
	}

	ImGuiFileDialog::~ImGuiFileDialog()
	{
		EngineNS::Safe_Delete<IGFD::FileDialog>(mDialog);
	}

	void ImGuiFileDialog::OpenDialog(const char* vKey, const char* vTitle, const char* vFilters, const char* vPath)
	{
		if (mDialog)
		{
			mDialog->OpenDialog(
				vKey, vTitle, vFilters, vPath);
		}
	}

	void ImGuiFileDialog::OpenModal(const char* vKey, const char* vTitle, const char* vFilters, const char* vPath)
	{
		if (mDialog)
		{
			mDialog->OpenModal(
				vKey, vTitle, vFilters, vPath);
		}
	}

	bool ImGuiFileDialog::DisplayDialog(const char* vKey)
	{
		if (mDialog)
		{
			return mDialog->Display(vKey);
		}

		return false;
	}

	void ImGuiFileDialog::CloseDialog()
	{
		if (mDialog)
		{
			mDialog->Close();
		}
	}

	bool ImGuiFileDialog::IsOk()
	{
		if (mDialog)
		{
			if (mDialog->IsOk())
			{
				mFilePathName = mDialog->GetFilePathName();
				mCurrentPath = mDialog->GetCurrentPath();
				mCurrentFileName = mDialog->GetCurrentFileName();
				mCurrentFilter = mDialog->GetCurrentFilter();
			}
			return mDialog->IsOk();
		}

		return false;
	}

	bool ImGuiFileDialog::WasKeyOpenedThisFrame(const char* vKey)
	{
		if (mDialog)
		{
			mDialog->WasOpenedThisFrame(vKey);
		}

		return false;
	}
	bool ImGuiFileDialog::WasOpenedThisFrame()
	{
		if (mDialog)
		{
			mDialog->WasOpenedThisFrame();
		}

		return false;
	}
	bool ImGuiFileDialog::IsKeyOpened(const char* vCurrentOpenedKey)
	{
		if (mDialog)
		{
			mDialog->IsOpened(vCurrentOpenedKey);
		}

		return false;
	}
	bool ImGuiFileDialog::IsOpened()
	{
		if (mDialog)
		{
			mDialog->IsOpened();
		}

		return false;
	}

	const char* ImGuiFileDialog::GetFilePathName()
	{
		if (mDialog && mDialog->IsOk())
		{
			return mFilePathName.c_str();
		}
		return nullptr;
	}

	const char* ImGuiFileDialog::GetCurrentFileName()
	{
		if (mDialog && mDialog->IsOk())
		{
			return mCurrentFileName.c_str();
		}
		return nullptr;
	}
	const char* ImGuiFileDialog::GetCurrentPath()
	{
		if (mDialog && mDialog->IsOk())
		{
			return mCurrentPath.c_str();
		}

		return nullptr;
	}


	const char* ImGuiFileDialog::GetCurrentFilter()
	{
		if (mDialog && mDialog->IsOk())
		{
			return mCurrentFilter.c_str();
		}
		return nullptr;
	}

	void* ImGuiFileDialog::GetUserDatas()
	{
		if (mDialog)
		{
			return mDialog->GetUserDatas();
		}

		return nullptr;
	}

	void ImGuiFileDialog::SetExtentionInfos(
		const char* vFilter,
		ImVec4 vColor,
		const char* vIcon)
	{
		if (mDialog)
		{
			mDialog->SetExtentionInfos(vFilter, vColor, vIcon);
		}
	}

	bool ImGuiFileDialog::GetExtentionInfos(
		const char* vFilter,
		ImVec4* vOutColor,
		char** vOutIcon)
	{
		if (mDialog)
		{
			std::string icon;
			bool res = mDialog->GetExtentionInfos(vFilter, vOutColor, &icon);
			if (!icon.empty() && vOutIcon)
			{
				size_t siz = icon.size() + 1U;
				*vOutIcon = new char[siz];
#ifndef MSVC
				strncpy(*vOutIcon, icon.c_str(), siz);
#else
				strncpy_s(*vOutIcon, siz, icon.c_str(), siz);
#endif
				* vOutIcon[siz - 1U] = '\0';
			}
			return res;
		}

		return false;
	}

	void ImGuiFileDialog::ClearExtentionInfos()
	{
		if (mDialog)
		{
			mDialog->ClearExtentionInfos();
		}
	}
}
