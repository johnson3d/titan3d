#pragma once
#include "../../base/IUnknown.h"
#include "../../base/string/vfxstring.h"
#include "../../base/thread/vfxcritical.h"

#include <string>

typedef UINT64 ClassID;
typedef UINT64 ObjectID;

class CoreProfiler;
enum TR_ENUM(SV_EnumNoFlags=true)
	EClrLogStringType
{
	ObjectAlloc = 1,
	ObjectsAllocdByClass,
	GCStart,
	GCFinish,
	ObjectReferences,
};

class TR_CLASS()
	ClrString : public EngineNS::VIUnknown
{
public:
	std::string mText;
	EClrLogStringType mType;
	
	ClrString(const char* text = "");
	~ClrString();
	void SetText(const char* text);

	const void* GetStringPtr() const{
		return mText.c_str();
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
	ClrClass
{
	ClassID Id;
	VNameString Name;
};

class TR_CLASS()
	CoreCLRManager
{
public:
	static bool IsStart;
	UINT Flags = 0;
	bool PauseLog = false;
	std::queue<ClrString*>		mStrings;
	VCritical		mLocker;
	CoreProfiler* CoreProfiler = nullptr;
public:
	CoreCLRManager();
	~CoreCLRManager();
	void FinalCleanup();

	static void Start();
	static void Stop();
	static CoreCLRManager* GetInstance();

	bool bMessageBox;
	void SetMessageBox(bool b) {
		bMessageBox = b;
	}
	void ShowMessageBox(const char* info);

	int GetLogNum() const {
		return (int)mStrings.size();
	}
	ClrString* PopLog();
	void PushLog(EClrLogStringType type, const char* info);

	bool IsCacheClassLoadFinished = false;
	bool IsObjectRefercenses = false;
	ClassID ProfileClass = 0;
	void SetProfileClass(UINT64 kls) {
		ProfileClass = kls;
	}
	
	std::vector<ClrClass*> CachedClasses;
	std::map<ClassID, ClrClass*> CachedClassesMap;
	ClrClass* GetCachedClasse(ClassID classId);
	void ClassLoadFinished(ClassID classId, HRESULT hrStatus);
	int GetCachedClassNum() {
		return (int)CachedClasses.size();
	}
	ClrClass** GetCachedClassPtr() {
		if (CachedClasses.size() == 0)
			return nullptr;
		return &CachedClasses[0];
	}
private:
	friend class CoreProfiler;
	void ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID* objectRefIds);
	void ObjectAllocated(ObjectID objectId, ClassID classId);
	void ObjectsAllocatedByClass(ULONG cClassCount, ClassID* classIds, ULONG* cObjects);
};

