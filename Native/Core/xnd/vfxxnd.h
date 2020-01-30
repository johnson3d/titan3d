/********************************************************************
	created:	2006/08/16
	created:	16:8:2006   11:30
	filename: 	d:\New-Work\Victory\Code\victorycoreex\vfxxnd.h
	file path:	d:\New-Work\Victory\Code\victorycoreex
	file base:	vfxxnd
	file ext:	h
	author:		johnson
	
	purpose:	
*********************************************************************/
#pragma once

#include "../precompile.h"
#include "../thread/vfxcritical.h"
#include "../r2m/file_2_memory.h"

NS_BEGIN

struct XNDBuffer : public EngineNS::VIUnknown
{
	VMemFile		Stream;
};

class XNDNode;
class XNDAttrib;
struct VRes2Memory;
#define XND_USE_POOL

class XNDData
{
	struct MyAllocator
	{
		static void * Malloc(size_t size)
		{
			return _vfxMemoryNew(size, __FILE__, __LINE__);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};

	enum {
		ChunkCount = 16,
	};

	VMem::vector<void*, MyAllocator>		mNodeMemChunks;
	XNDNode*					mFreeNodeChunk;
	int							mFreeNodeIndex;

	VMem::vector<void*, MyAllocator>		mAttribMemChunks;
	XNDAttrib*					mFreeAttribChunk;
	int							mFreeAttribIndex;
public:
	VCritical					mLocker;
	int							mLiveNumber;
	static int XNDDataNumber;
	
	XNDData();
	~XNDData();
	void Release()
	{
		//Do Nothing
	}
	XNDAttrib* NewAttrib(XNDNode* pNode);
	XNDNode* NewNode();
	XNDNode* NewNode(XNDNode* pParent);
};

/*!	XND属性访问器：
*	\param
*		负责真正从Stream中读取信息，或者向Stream中写入信息。
*		由XND节点维护，用户应调用对应节点的AddAttrib来创建属性访问器。
*/
const vIID vIID_XNDAttrib = 0x838484794c918324;
class XNDAttrib : public EngineNS::VIUnknown
{
	friend XNDNode;
	friend XNDData;
protected:
	XNDData*		Manager;
	VStringA		Name;
	VStringA	    Key;	// 关键字
	UINT_PTR		Offset;
	UINT_PTR		Length;
	VMemFile		Stream;
	BYTE			Version;

	vBOOL			mReading;
	vBOOL			mWriting;
	std::string		mBeginReadFile;
	int				mBeginReadLine;
public:
	VRes2Memory*	Resource;
protected:
	XNDAttrib(XNDNode* pNode, XNDData* manager);
public:
	VDef_ReadOnly(Reading);
	VDef_ReadOnly(Writing);
#if defined(XND_USE_POOL)
	void* operator new(size_t size, const char* file, int line)//_cdecl
	{
		return _vfxMemoryNew(size,file,line);
	}
	void* operator new(size_t size)//_cdecl 
	{
		return _vfxMemoryNew(size, __FILE__, __LINE__);
	}
	void* operator new(size_t size, void* p, const char* file, int line)
	{
		return p;
	}
	void operator delete(void* p, size_t size, const char* file, int line)
	{

	}
	void operator delete(void* p, size_t size);
#endif
	
	// XNDAttrib();
	 ~XNDAttrib();
	 void Cleanup();

	 bool IsDDC();
	 void SetDDC(bool isDDC);

	 void BeginRead( const char* file, int line );
	 void EndRead();
	 void BeginWrite();
	 void EndWrite();

	const VString& GetName() const{
		return Name;
	}
	void SetName(LPCSTR name) {
		Name = name;
	}
	const VString& GetKey() const {
		return Key;
	}
	void SetKey(const char* key) {
		Key = key;
	}
	void WriteText(const char* lpBuf)
	{
		auto len = (UINT)strlen(lpBuf);
		Write(&len, sizeof(UINT));
		Write(lpBuf, len);
	}
	void ReadText(std::string& str)
	{
		UINT len;
		Read(&len, sizeof(UINT));
		str.resize(len);
		if (len == 0)
			return;
		Read(&str[0], len);
	}
	void ReadText(VStringA& str)
	{
		UINT len;
		Read(&len, sizeof(UINT));
		str.resize(len);
		if (len == 0)
			return;
		Read(&str[0], len);
	}
	template<class Type>
	UINT_PTR Write(const Type& value) {
		return Write(&value, sizeof(Type));
	}
	template<class Type>
	UINT_PTR Read(Type& value) {
		return Read(&value, sizeof(Type));
	}
	UINT_PTR Write(const void* lpBuf, UINT_PTR nCount){
		return Stream.Write(lpBuf,nCount);
	}
	UINT_PTR Read(void* lpBuf, UINT_PTR nCount);
	UINT_PTR GetPosition() const{
		return Stream.GetPosition();
	}
	UINT_PTR Seek(INT_PTR lOff,VFile_Base::SeekPosition eFrom){
		return Stream.Seek(lOff,eFrom);
	}
	UINT_PTR GetLength() const{
		return Length;
	}
/*
	UINT_PTR GetLength() const{
		return Stream.GetLength();
	}
*/
	void * GetMemory() const{
		return Stream.GetMemory();
	}

	inline BYTE GetVersion(){
		return Version;
	}
	inline void SetVersion(BYTE ver){
		Version = ver;
	}
	 XNDAttrib( const XNDAttrib& rh );
	 XNDAttrib& operator = ( const XNDAttrib& rh );
};


/*!	XND节点：
*	\param
*		负责真正从Stream中读取信息，或者向Stream中写入信息。
*		由XND节点维护，用户应调用对应节点的AddAttrib来创建属性访问器。
*/
const vIID vIID_XNDNode = 0xedf68bbc4c91830d;
class XNDNode : public EngineNS::VIUnknown
{
public:
	struct MyAllocator
	{
		static void * Malloc(size_t size)
		{
			return _vfxMemoryNew(size, __FILE__, __LINE__);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};
	typedef VMem::vector<XNDAttrib*, MyAllocator>	XNDAttribVector;
	typedef VMem::vector<XNDNode*, MyAllocator>		XNDNodeVector;
protected:
	XNDData*		Manager;
	VString			mName;
	vIID			m_ClassID;
	DWORD			m_UserFlags;
	
	XNDAttribVector	m_Attribs;
	XNDNodeVector	m_Childs;

	std::map<VString, XNDNode*>		mNameSorted;

	XNDNode*		m_pParent;
public:
	VRes2Memory*	Resource;
protected:
	friend XNDData;
	/*!	注意，这个方法应该专用于控制XND树根，树叶不应使用这个方法
	*/
	 XNDNode(XNDData* manager);
	/*!	注意，这个方法应该专用于控制XND树叶，树根不应使用此方法。
	*	特殊性：经此初始化的对象的AddRef和Release实际上是对根的AddRef和Release。
	*/
	 XNDNode(XNDData* manager, XNDNode* pParent);
public:
#if defined(XND_USE_POOL)
	void* operator new(size_t size, const char* file, int line)
	{
		return _vfxMemoryNew(size, file, line);
	}
	void* operator new(size_t size)
	{
		return _vfxMemoryNew(size, __FILE__, __LINE__);
	}
	void* operator new(size_t size, void* p, const char* file, int line)
	{
		return p;
	}
	void operator delete(void* p, size_t size, const char* file, int line)
	{
		
	}
	void operator delete(void* p, size_t size);
#endif
	 ~XNDNode();
	 void Cleanup();
	
	LPCSTR GetResourceName();
	inline const VStringA& GetName() const{
		return mName;
	}
	inline vIID GetClassID() const {
		return m_ClassID;
	}
	 void SetName( LPCSTR szName );
	inline void SetClassID( const vIID& id ){
		m_ClassID = id;
	}
	inline DWORD GetUserFlags(){
		return m_UserFlags;
	}
	inline void SetUserFlags( DWORD UserFlags ){
		m_UserFlags = UserFlags;
	}
	//inline XNDNode* GetParent() {
	//	return m_pParent;
	//}
	//inline XNDNode* GetRoot() {
	//	return m_pParent ? m_pParent->GetRoot() : this;
	//}
	inline XNDNode* GetParent(){
		return m_pParent;
	}
	inline XNDNode* GetRoot(){
		return m_pParent ? m_pParent->GetRoot() : this;
	}

	// XNDNode(const XNDNode& rh);
	// XNDNode& operator = ( const XNDNode& rh );
	 void CopyNodeData( const XNDNode& rh );

	 void AppendNode( const VString& name, XNDNode* pNode );

	 XNDNode* AddNode( LPCSTR szName , const vIID& ClassID , DWORD UserFlags );
	 XNDNode* AddNode(const XNDNode* node);
	// void AddNode( XNDNode* pNode );
	 vBOOL DelNode( XNDNode* pNode );
	
	 XNDAttrib* AddAttrib( LPCSTR szName );
	 XNDAttrib* AddAttrib(const XNDAttrib* childAttrib);
	 vBOOL DelAttrib( LPCSTR szName );
	
	//如果找不到Attrib，就创建一个
	 XNDAttrib* GetAttribForce( LPCSTR szName );
	//查找到指定Attrib
	 XNDAttrib* GetAttrib( LPCSTR szName );
	 XNDNode* GetChild( LPCSTR szName );

	 XNDNode* QuickFindChild( LPCSTR szName );

	XNDAttribVector& GetAttribVector()
	{
		return m_Attribs;
	}
	XNDNodeVector& GetChildVector()
	{
		return m_Childs;
	}

	 void TryReleaseHolder();
public:
	 vBOOL Save( VFile_Base& io );
	 vBOOL Load( VRes2Memory* pRes );

	//n0永远覆盖n1
	 static vBOOL Merge(XNDNode* target , XNDNode* n0 , XNDNode* n1 );
protected:
	 vBOOL SaveHead(VFile_Base& io);
	 vBOOL SaveData(VFile_Base& io);

	 vBOOL LoadHead( VRes2Memory* pRes , VMemoryReader& io );
	 vBOOL LoadHead1( VRes2Memory* pRes , VMemoryReader& io );
	 vBOOL LoadHead2( VRes2Memory* pRes , VMemoryReader& io );
	 vBOOL LoadHead3(VRes2Memory* pRes, VMemoryReader& io);
};

NS_END
