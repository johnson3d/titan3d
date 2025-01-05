#include <WinSock2.h>
#include <mswsock.h>
#include <windows.h>

#include "ClrLogger.h"
#include "ClrProfiler.h"
#include <stdlib.h>
#include <string.h>

#include "../../base/thread/vfxcritical.h"
#include "../../base/debug/vfxnew.h"

#define new VNEW

bool CoreCLRManager::IsStart = false;

ClrString::ClrString(const char* text)
{
	SetText(text);
}

ClrString::~ClrString()
{
	
}

void ClrString::SetText(const char* text)
{
	mText = text;
}

CoreCLRManager gClrLogger;

CoreCLRManager* CoreCLRManager::GetInstance()
{
	return &gClrLogger;
}

void CoreCLRManager::Start()
{
	IsStart = true;
}

void CoreCLRManager::Stop()
{
	if (IsStart == false)
		return;
	GetInstance()->FinalCleanup();
}

void CoreCLRManager::FinalCleanup()
{
	IsStart = false;
	
	while (!mStrings.empty())
	{
		auto p = mStrings.front();
		p->Release();
		mStrings.pop();
	}

	CachedClassesMap.clear();
	for (auto& i : CachedClasses)
	{
		delete i;
	}
	CachedClasses.clear();
}

CoreCLRManager::CoreCLRManager()
{
}

CoreCLRManager::~CoreCLRManager()
{

}

ClrString* CoreCLRManager::PopLog()
{
	VAutoVSLLock lk(mLocker);
	if (mStrings.empty())
		return nullptr;
	auto ret = mStrings.front();
	mStrings.pop();
	return ret;
}

void CoreCLRManager::PushLog(EClrLogStringType type, const char* info)
{
	if (IsStart == false)
		return;
	if ((Flags & (1 << type)) == 0 || PauseLog)
		return;
	VAutoVSLLock lk(mLocker);
	auto tmp = new ClrString();
	tmp->mType = type;
	tmp->mText = info;
	mStrings.push(tmp);
}

void CoreCLRManager::ShowMessageBox(const char* info)
{
	MessageBoxA(NULL, info, "ClrLogger", MB_OK);
}

ClrClass* CoreCLRManager::GetCachedClasse(ClassID classId)
{
	if (IsStart == false)
		return nullptr;
	VAutoVSLLock lk(mLocker);
	auto iter = CachedClassesMap.find(classId);
	if (iter != CachedClassesMap.end())
	{
		return iter->second;
	}

	ModuleID module;
	mdTypeDef type;
	if (SUCCEEDED(CoreProfiler->GetCorProfilerInfo()->GetClassIDInfo(classId, &module, &type)))
	{
		auto name = CoreProfiler->GetTypeName(type, module);
		auto kls = new ClrClass();
		kls->Id = classId;
		kls->Name = name;
		CachedClasses.push_back(kls);
		CachedClassesMap[classId] = kls;
		return kls;
	}
	return nullptr;
}

void CoreCLRManager::ClassLoadFinished(ClassID classId, HRESULT hrStatus)
{
	if (IsStart == false)
		return;
	if (IsCacheClassLoadFinished == false)
		return;
	GetCachedClasse(classId);
}

void CoreCLRManager::ObjectAllocated(ObjectID objectId, ClassID classId)
{
	if (IsStart == false)
		return;
	auto kls = this->GetCachedClasse(classId);
	if (kls != nullptr)
	{
		PushLog(EClrLogStringType::ObjectAlloc, kls->Name.c_str());
	}
}

void CoreCLRManager::ObjectsAllocatedByClass(ULONG cClassCount, ClassID* classIds, ULONG* cObjects)
{
	if (IsStart == false)
		return;
	std::string info("");
	for (ULONG i = 0; i < cClassCount; i++)
	{
		auto kls = this->GetCachedClasse(classIds[i]);
		if (kls != nullptr)
		{
			info += kls->Name.c_str();
			info += ",";
		}
	}
	PushLog(EClrLogStringType::ObjectsAllocdByClass, info.c_str());
}

void CoreCLRManager::ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID* objectRefIds)
{
	if (IsStart == false)
		return;
	if (ProfileClass == 0)
		return;
	if (IsObjectRefercenses)
	{
		if (classId == ProfileClass)
		{
			std::string text = "(" + std::string(GetCachedClasse(classId)->Name.c_str()) + ")[";
			for (ULONG i = 0; i < cObjectRefs; i++)
			{
				ClassID refClass;
				if (CoreProfiler->GetCorProfilerInfo()->GetClassFromObject(objectRefIds[i], &refClass) == S_OK)
				{
					text += std::string(GetCachedClasse(refClass)->Name.c_str()) + ",";
				}
			}
			text += "]";
			this->PushLog(EClrLogStringType::ObjectReferences, text.c_str());
		}
	}
}