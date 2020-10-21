#include "vfxxnd.h"
#include "vfxDerivedDataCache.h"

//#define new VNEW

NS_BEGIN

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wnull-conversion"
#pragma clang diagnostic ignored "-Wreorder"
#else
#pragma warning(disable:4291)//vc compiler's bug?
#endif

//VMem::vector<XNDData*,VMem::VMemAllocator> GXNDataHolder;
//VCritical GXNDLocker;

extern "C"  void vfxMemory_CheckMemoryState(LPCSTR str);

//extern "C"  void TestMemOverRange(const char* name);
#define TestMemOverRange(a) /**/

int XNDData::XNDDataNumber = 0;

XNDData::XNDData()
{
	mLiveNumber = 0;
	XNDDataNumber++;
	mFreeNodeIndex = ChunkCount;
	mFreeAttribIndex = ChunkCount;
}

XNDData::~XNDData()
{
	XNDDataNumber--;
#if defined(XND_USE_POOL)
	{
		for (auto i = mAttribMemChunks.begin(); i != mAttribMemChunks.end(); i++)
		{
			_vfxMemoryDelete(*i, NULL, 0);
		}
		mAttribMemChunks.clear();

		for (auto i = mNodeMemChunks.begin(); i != mNodeMemChunks.end(); i++)
		{
			_vfxMemoryDelete(*i, NULL, 0);
		}
		mNodeMemChunks.clear();
	}
#endif
}

XNDAttrib* XNDData::NewAttrib(XNDNode* pNode)
{ 
	VAutoLock(mLocker);
	mLiveNumber++;
#if defined(XND_USE_POOL)
	if (mFreeAttribIndex >= ChunkCount)
	{
		mFreeAttribChunk = (XNDAttrib*)_vfxMemoryNew(sizeof(XNDAttrib)*ChunkCount, __FILE__, __LINE__);
		memset((void*)mFreeAttribChunk, 0, sizeof(XNDAttrib)*ChunkCount);
		mAttribMemChunks.push_back(mFreeAttribChunk);
		mFreeAttribIndex = 0;
	}
	XNDAttrib* attr = &mFreeAttribChunk[mFreeAttribIndex];
	mFreeAttribIndex++;
	//attr->XNDAttrib::XNDAttrib(pNode, this);
	//return attr;
	return new(attr,__FILE__,__LINE__) XNDAttrib(pNode, this);
#else
	return ::new XNDAttrib(pNode, this);
#endif
}

XNDNode* XNDData::NewNode()
{
	VAutoLock(mLocker);
	mLiveNumber++;
#if defined(XND_USE_POOL)
	{
		if (mFreeNodeIndex >= ChunkCount)
		{
			mFreeNodeChunk = (XNDNode*)_vfxMemoryNew(sizeof(XNDNode)*ChunkCount, __FILE__, __LINE__);
			memset((void*)mFreeNodeChunk, 0, sizeof(XNDNode)*ChunkCount);
			mNodeMemChunks.push_back(mFreeNodeChunk);
			mFreeNodeIndex = 0;
		}
		XNDNode* node = &mFreeNodeChunk[mFreeNodeIndex];
		mFreeNodeIndex++;
		//node->XNDNode::XNDNode(this);
		//return node;
		return new(node, __FILE__, __LINE__) XNDNode(this);
	}
#else
	{
		return ::new XNDNode(this);
	}
#endif
}

XNDNode* XNDData::NewNode(XNDNode* pParent)
{
	VAutoLock(mLocker);
	mLiveNumber++;
#if defined(XND_USE_POOL)
	{
		if (mFreeNodeIndex >= ChunkCount)
		{
			mFreeNodeChunk = (XNDNode*)_vfxMemoryNew(sizeof(XNDNode)*ChunkCount, __FILE__, __LINE__);
			memset((void*)mFreeNodeChunk, 0, sizeof(XNDNode)*ChunkCount);
			mNodeMemChunks.push_back(mFreeNodeChunk);
			mFreeNodeIndex = 0;
		}
		XNDNode* node = &mFreeNodeChunk[mFreeNodeIndex];
		mFreeNodeIndex++;
		//node->XNDNode::XNDNode(this, pParent);
		//return node;
		return new(node, __FILE__, __LINE__) XNDNode(this, pParent);
	}
#else
	{
		return ::new XNDNode(this, pParent);
	}
#endif
}

#if defined(XND_USE_POOL)
void XNDAttrib::operator delete (void * p, size_t size)
{
	auto obj = (XNDAttrib*)p;
	obj->Manager->mLocker.Lock();
	obj->Manager->mLiveNumber--;
	if (obj->Manager->mLiveNumber == 0)
	{
		obj->Manager->mLocker.Unlock();
		delete obj->Manager;
	}
	else
	{
		obj->Manager->mLocker.Unlock();
	}
#if defined(XND_USE_POOL)
#else
	::delete p;
#endif
}
#endif

XNDAttrib::XNDAttrib(XNDNode* pNode, XNDData* manager)
	//: AuxLeafIUnknown<XNDAttrib,VIUnknown>(pNode)	
	: Offset(0)
	, Resource(NULL)
	, Length(0)
	, Version(0)
{
	Manager = manager;
	mReading = FALSE;
	mWriting = FALSE;
	mBeginReadLine = 0;
}

XNDAttrib::~XNDAttrib()
{
	Cleanup();
}

void XNDAttrib::Cleanup()
{
	Safe_Release( Resource );
}

XNDAttrib::XNDAttrib( const XNDAttrib& rh )
{
	Name = rh.GetName();
	Key = rh.GetKey();
	Offset = rh.Offset;
	Length = rh.Length;
	Version = rh.Version;

	Resource = rh.Resource;
	if(Resource)
		Resource->AddRef();
	
	Stream.OpenAndCopy( NULL , (UINT)rh.Stream.GetLength() );
	XNDAttrib& rh_noconst = (XNDAttrib&)rh;
	Stream.Write( rh_noconst.Stream.GetMemory() , (UINT)rh_noconst.Stream.GetLength() );
}

UINT_PTR XNDAttrib::Read(void* lpBuf, UINT_PTR nCount) 
{
	if (nCount == 0)
		return 0;
	auto readNum = Stream.Read(lpBuf, nCount);
	if (readNum == 0 && Resource != nullptr)
	{
		VFX_LTRACE(ELTT_Warning, "XndAttrib Read 0 byte at %s\r\n", this->Resource->Name());
	}
	return readNum;
}

XNDAttrib& XNDAttrib::operator = ( const XNDAttrib& rh )
{
	Name = rh.GetName();
	Key = rh.GetKey();
	Offset = rh.Offset;
	Length = rh.Length;
	Version = rh.Version;

	Resource = rh.Resource;
	if(Resource)
		Resource->AddRef();

	Stream.OpenAndCopy( NULL , (UINT)rh.Stream.GetLength() );
	XNDAttrib& rh_noconst = (XNDAttrib&)rh;
	Stream.Write( rh_noconst.Stream.GetMemory() , (UINT)rh_noconst.Stream.GetLength() );
	
	return *this;
}

void XNDAttrib::BeginWrite()
{
	mWriting = TRUE;
	Stream.Close();
	Stream.OpenAndCopy( NULL , (UINT)Length );
}

void XNDAttrib::EndWrite()
{
	//Stream.Close();
	mWriting = FALSE;
}

bool XNDAttrib::IsDDC()
{
	return (Version & (1 << 7)) ? true : false;
}

void XNDAttrib::SetDDC(bool isDDC)
{
	if(isDDC)
		Version |= (1 << 7);
	else
		Version &= (~(1 << 7));
}

void XNDAttrib::BeginRead(const char* file, int line)
{
#if defined(_DEBUG)
	mBeginReadFile = file;
	mBeginReadLine = line;
#endif
	mReading = TRUE;

	if (IsDDC())
	{
		UINT_PTR length;
		auto pMem = vfxDerivedDataCache::Instance.LoadData("", "", 0, &length);
		if (pMem)
		{
			ASSERT(length == Length);
			Stream.Close();
			Stream.OpenAndCopy(pMem, (UINT)length);
			delete[] pMem;
		}
	}

	if( Resource )
	{
		BYTE* pMem = ((BYTE*)Resource->Ptr(Offset,Length));
		Stream.Close();
		Stream.OpenAndCopy( pMem , (UINT)Length );
	}
	else
	{
		Stream.Seek( 0 , VFile_Base::begin );
	}
}

void XNDAttrib::EndRead()
{
	if( Resource )
	{
		Stream.Close();
		Resource->Free();
	}
	mReading = FALSE;
#if defined(_DEBUG)
	mBeginReadFile = "";
	mBeginReadLine = 0;
#endif
}

//------------------------------------------------------
#if defined(XND_USE_POOL)
void XNDNode::operator delete (void * p, size_t size)
{
	auto obj = (XNDNode*)p;
	obj->Manager->mLocker.Lock();
	obj->Manager->mLiveNumber--;
	if (obj->Manager->mLiveNumber == 0)
	{
		obj->Manager->mLocker.Unlock();
		delete obj->Manager;
	}
	else
	{
		obj->Manager->mLocker.Unlock();
	}
#if defined(XND_USE_POOL)
#else
	::delete p;
#endif
}
#endif

XNDNode::XNDNode(XNDData* manager)
	: m_pParent(NULL)
	, m_ClassID(0)
	, m_UserFlags(0)
	, Resource(NULL)
{
	Manager = manager;
}

XNDNode::XNDNode(XNDData* manager, XNDNode* pParent)
: m_pParent(pParent)
, m_ClassID(0)
, m_UserFlags(0)
, Resource(NULL)
{
	Manager = manager;
}

XNDNode::~XNDNode()
{
	Cleanup();
	Safe_Release( Resource );
}

LPCSTR XNDNode::GetResourceName()
{
	if (Resource)
		return Resource->Name();
	return vT("none");
}

void XNDNode::Cleanup()
{
	for( size_t i=0 ; i<m_Attribs.size() ; i++ )
	{
		Safe_Release( m_Attribs[i] );
	}
	m_Attribs.clear();
	for ( size_t i=0 ; i<m_Childs.size() ; i++ )
	{
		Safe_Release( m_Childs[i] );
	}
	m_Childs.clear();
}

//XNDNode::XNDNode(const XNDNode& rh)
//{
//	SetName(rh.GetName());
//	m_ClassID = rh.m_ClassID;
//	m_UserFlags = rh.m_UserFlags;
//
//	m_Attribs = rh.m_Attribs;
//	
//	for( size_t i=0 ; i<rh.m_Childs.size() ; i++ )
//	{
//		XNDNode* pNode = Manager->NewNode( *rh.m_Childs[i] );
//		m_Childs.push_back( pNode );
//	}
//}

void XNDNode::TryReleaseHolder()
{
	if(Resource)
		Resource->TryReleaseHolder();
}

//XNDNode& XNDNode::operator = ( const XNDNode& rh )
//{
//	SetName(rh.GetName());
//	m_ClassID = rh.m_ClassID;
//	m_UserFlags = rh.m_UserFlags;
//
//	m_Attribs = rh.m_Attribs;
//
//	for( size_t i=0 ; i<rh.m_Childs.size() ; i++ )
//	{
//		XNDNode* pNode = Manager->NewNode(this);
//		//pNode->SetNodeInfo( rh.m_Childs[i]->GetName() , rh.m_Childs[i]->GetClassID() );
//		*pNode = *rh.m_Childs[i];
//		m_Childs.push_back( pNode );
//	}
//	return *this;
//}

void XNDNode::CopyNodeData( const XNDNode& rh )
{
	SetName(rh.GetName().c_str());
	m_ClassID = rh.m_ClassID;
	m_UserFlags = rh.m_UserFlags;

	m_Attribs = rh.m_Attribs;
}

//void XNDNode::AddNode( XNDNode* pNode )
//{
//	pNode->AddRef();
//	m_Childs.push_back( pNode );
//}

void XNDNode::AppendNode( const VString& name, XNDNode* pNode )
{
	XNDNode* pTarNode = this->AddNode(name.c_str(),pNode->GetClassID(),pNode->GetUserFlags());
	pNode->SetName(name.c_str());

	for( size_t i=0 ; i<pNode->m_Attribs.size() ; i++ )
	{
		XNDAttrib* pSrc = pNode->m_Attribs[i];
		XNDAttrib* pAttr =  pTarNode->AddAttrib(pSrc->GetName().c_str());
		pAttr->Version = pSrc->Version;

		pSrc->BeginRead(__FILE__, __LINE__);
		UINT_PTR len = pSrc->GetLength();
		BYTE* data = new BYTE[len];
		pSrc->Read(data,len);
		pSrc->EndRead();

		pAttr->BeginWrite();
		pAttr->Write( data , len );
		pAttr->EndWrite();

		delete[] data;
	}

	for( size_t i=0 ; i<pNode->m_Childs.size() ; i++ )
	{
		XNDNode* pSrc = pNode->m_Childs[i];
		//XNDNode* pTar = pTarNode->AddNode(childName , pSrc->GetClassID() , pSrc->GetUserFlags() );
		pTarNode->AppendNode(pSrc->GetName(), pSrc);
	}
}

XNDNode* XNDNode::AddNode( LPCSTR szName , const vIID& ClassID , DWORD UserFlags )
{
	XNDNode* pNode = Manager->NewNode(this);
	pNode->SetName(szName);
	pNode->m_ClassID = ClassID;
	pNode->m_UserFlags = UserFlags;

	m_Childs.push_back( pNode );

	return pNode;
}

XNDNode* XNDNode::AddNode(const XNDNode* node)
{
	XNDNode* pNode = Manager->NewNode(this);
	pNode->CopyNodeData(*node);
	m_Childs.push_back(pNode);
	return pNode;
}

vBOOL XNDNode::DelNode( XNDNode* pNode )
{
	for( size_t i=0 ; i<m_Childs.size() ; i++ )
	{
		if( m_Childs[i]==pNode )
		{
			pNode->Release();
			m_Childs.erase( m_Childs.begin()+i );
			return TRUE;
		}
	}
	return FALSE;
}

XNDAttrib* XNDNode::AddAttrib( LPCSTR szName )
{
	XNDAttrib* pAttr = Manager->NewAttrib(this);
	pAttr->Name = szName;
	m_Attribs.push_back( pAttr );
	return pAttr;
}

XNDAttrib* XNDNode::AddAttrib(const XNDAttrib* childAttrib)
{
	XNDAttrib* pAttr = Manager->NewAttrib(this);
	*pAttr = *childAttrib;
	m_Attribs.push_back(pAttr);
	return pAttr;
}

vBOOL XNDNode::DelAttrib( LPCSTR szName )
{
	for( size_t i=0 ; i<m_Attribs.size() ; i++ )
	{
		if( m_Attribs[i]->GetName()==szName )
		{
			XNDAttrib* pAttr = m_Attribs[i];
			m_Attribs.erase( m_Attribs.begin()+i );
			pAttr->Release();
			return TRUE;
		}
	}
	return FALSE;
}

XNDAttrib* XNDNode::GetAttribForce( LPCSTR szName )
{
	XNDAttrib* pAttr = GetAttrib( szName );
	if( pAttr==NULL )
	{
		XNDAttrib* temp = Manager->NewAttrib(this);
		temp->Name = szName;
		m_Attribs.push_back( temp );
		return temp;
	}
	return pAttr;
}

XNDAttrib* XNDNode::GetAttrib( LPCSTR szName )
{
	for( size_t i=0 ; i<m_Attribs.size() ; i++ )
	{
		if( m_Attribs[i]->GetName()==szName )
		{
			return m_Attribs[i];
		}
	}
	return NULL;
}

XNDNode* XNDNode::GetChild( LPCSTR szName )
{
	for( size_t i=0 ; i<m_Childs.size() ; i++ )
	{
		if( m_Childs[i]->GetName()==szName )
		{
			return m_Childs[i];
		}
	}
	return NULL;
}

XNDNode* XNDNode::QuickFindChild( LPCSTR szName )
{
	if(mNameSorted.size()!=m_Childs.size())
	{
		mNameSorted.clear();
		for(auto i=m_Childs.begin();i!=m_Childs.end();i++)
		{
			XNDNode* pNode = *i;
			mNameSorted.insert(std::make_pair(pNode->GetName(), pNode));
		}
	}

	auto iter = mNameSorted.find(szName);
	if(iter==mNameSorted.end())
		return NULL;

	return iter->second;
}

vBOOL XNDNode::SaveHead( VFile_Base& io )
{
	auto name = GetName();
	UINT nLen = (UINT)name.length();
	io.Write( &nLen , sizeof(nLen) );
	io.Write(&name[0] , sizeof(CHAR)*nLen );
	//m_strName.ReleaseBuffer(0);

	io.Write( &m_ClassID , sizeof(m_ClassID) );
	io.Write( &m_UserFlags , sizeof(m_UserFlags) );

	UINT nAttrib = (UINT)m_Attribs.size();
	io.Write( &nAttrib , sizeof(nAttrib) );
	for ( size_t i=0 ; i<m_Attribs.size() ; i++ )
	{
		nLen = (UINT)m_Attribs[i]->GetName().length();
		io.Write( &nLen , sizeof(nLen) );
		io.Write(&(m_Attribs[i]->GetName())[0] , sizeof(CHAR)*nLen );
		nLen = (UINT)m_Attribs[i]->GetKey().length();
		io.Write(&nLen, sizeof(nLen));
		io.Write(&(m_Attribs[i]->GetKey())[0], sizeof(CHAR)*nLen);
		//m_Attribs[i]->GetName().ReleaseBuffer(0);
		io.Write( &m_Attribs[i]->Offset , sizeof(UINT) );
		
		m_Attribs[i]->Length = m_Attribs[i]->Stream.GetLength();
		io.Write( &m_Attribs[i]->Length , sizeof(UINT) );
		
		io.Write(&m_Attribs[i]->Version, sizeof(BYTE));
	}
	
	UINT nChild = (UINT)m_Childs.size();
	io.Write( &nChild , sizeof(nChild) );
	for ( size_t i=0 ; i<m_Childs.size() ; i++ )
	{
		m_Childs[i]->SaveHead( io );
	}

	return TRUE;
}

vBOOL XNDNode::SaveData( VFile_Base& io )
{
	for ( size_t i=0 ; i<m_Attribs.size() ; i++ )
	{
		m_Attribs[i]->Offset = io.GetPosition();
		io.Write( m_Attribs[i]->Stream.GetMemory() , m_Attribs[i]->Stream.GetLength() );
	}
	for ( size_t i=0 ; i<m_Childs.size() ; i++ )
	{
		m_Childs[i]->SaveData( io );
	}
	return TRUE;
}

vBOOL XNDNode::LoadHead( VRes2Memory* pRes , VMemoryReader& io )
{
	//vfxMemory_CheckMemoryState("XNDLoad");
	CHAR szName[64];
	io.Read( szName , sizeof(szName) );
	szName[_countof(szName) - 1] = 0;
	SetName(szName);
	io.Read( &m_ClassID , sizeof(m_ClassID) );
	io.Read( &m_UserFlags , sizeof(m_UserFlags) );

	UINT nAttrib = 0;
	io.Read( &nAttrib , sizeof(nAttrib) );
	m_Attribs.resize( nAttrib );
	for ( size_t i=0 ; i<nAttrib ; i++ )
	{
		m_Attribs[i] = Manager->NewAttrib(this);
		pRes->AddRef();
		m_Attribs[i]->Resource = pRes;
		
		io.Read( szName , sizeof(szName) );
		szName[_countof(szName) - 1] = 0;
		m_Attribs[i]->SetName(szName);
		UINT nOffset;
		io.Read( &nOffset , sizeof(UINT) );
		m_Attribs[i]->Offset = nOffset;
		UINT nLength;
		io.Read( &nLength , sizeof(UINT) );
		m_Attribs[i]->Length = nLength;
	}

	UINT nChild = 0;
	io.Read( &nChild , sizeof(nChild) );
	for ( UINT i=0 ; i<nChild ; i++ )
	{
		XNDNode* pNode = Manager->NewNode(this);
		pRes->AddRef();
		pNode->Resource = pRes;
		pNode->LoadHead( pRes , io );
		m_Childs.push_back( pNode );
	}

	return TRUE;
}

vBOOL XNDNode::LoadHead1( VRes2Memory* pRes , VMemoryReader& io )
{
	INT nLen;
	io.Read( &nLen , sizeof(INT) );
	if( nLen>0 )
	{
		CHAR* pChar = ::new(__FILE__,__LINE__) CHAR[nLen+1];
		io.Read( pChar , nLen*sizeof(CHAR) );
		pChar[nLen] = (int)NULL;
		SetName(pChar);
		delete[] pChar;
	}
	else
	{
		SetName("");
	}

	io.Read( &m_ClassID , sizeof(m_ClassID) );
	io.Read( &m_UserFlags , sizeof(m_UserFlags) );

	UINT nAttrib = 0;
	io.Read( &nAttrib , sizeof(nAttrib) );
	m_Attribs.resize( nAttrib );
	for ( size_t i=0 ; i<nAttrib ; i++ )
	{
		m_Attribs[i] = Manager->NewAttrib(this);
		pRes->AddRef();
		m_Attribs[i]->Resource = pRes;

		io.Read( &nLen , sizeof(INT) );
		if( nLen>0 )
		{
			CHAR* pChar = ::new(__FILE__, __LINE__) CHAR[nLen+1];
			io.Read( pChar , nLen*sizeof(CHAR) );
			pChar[nLen] = (int)NULL;
			m_Attribs[i]->SetName(pChar);
			delete[] pChar;
		}
		else
		{
			m_Attribs[i]->SetName("");
		}
		
		UINT nOffset;
		io.Read( &nOffset , sizeof(UINT) );
		m_Attribs[i]->Offset = nOffset;
		UINT nLength;
		io.Read( &nLength , sizeof(UINT) );
		m_Attribs[i]->Length = nLength;
	}

	UINT nChild = 0;
	io.Read( &nChild , sizeof(nChild) );
	for ( UINT i=0 ; i<nChild ; i++ )
	{
		XNDNode* pNode = Manager->NewNode(this);
		pRes->AddRef();
		pNode->Resource = pRes;
		pNode->LoadHead1( pRes , io );
		m_Childs.push_back( pNode );
	}

	return TRUE;
}

vBOOL XNDNode::LoadHead2( VRes2Memory* pRes , VMemoryReader& io )
{
	INT nLen;
	io.Read( &nLen , sizeof(INT) );
	if( nLen>0 )
	{
		CHAR* pChar = ::new(__FILE__, __LINE__) CHAR[nLen+1];
		io.Read( pChar , nLen*sizeof(CHAR) );
		pChar[nLen] = (int)NULL;
		SetName(pChar);
		delete[] pChar;
	}
	else
	{
		SetName("");
	}

	io.Read( &m_ClassID , sizeof(m_ClassID) );
	io.Read( &m_UserFlags , sizeof(m_UserFlags) );

	UINT nAttrib = 0;
	io.Read( &nAttrib , sizeof(nAttrib) );
	m_Attribs.resize( nAttrib );
	for ( size_t i=0 ; i<nAttrib ; i++ )
	{
		m_Attribs[i] = Manager->NewAttrib(this);
		pRes->AddRef();
		m_Attribs[i]->Resource = pRes;

		io.Read( &nLen , sizeof(INT) );
		if( nLen>0 )
		{
			CHAR* pChar = ::new(__FILE__, __LINE__) CHAR[nLen+1];
			io.Read( pChar , nLen*sizeof(CHAR) );
			pChar[nLen] = (int)NULL;
			m_Attribs[i]->SetName(pChar);
			delete[] pChar;
		}
		else
		{
			m_Attribs[i]->SetName("");
		}

		UINT nOffset;
		io.Read( &nOffset , sizeof(UINT) );
		m_Attribs[i]->Offset = nOffset;
		UINT nLength;
		io.Read( &nLength , sizeof(UINT) );
		m_Attribs[i]->Length = nLength;
		BYTE ver;
		io.Read(&ver, sizeof(BYTE));
		m_Attribs[i]->Version = ver;
	}

	UINT nChild = 0;
	io.Read( &nChild , sizeof(nChild) );
	for ( UINT i=0 ; i<nChild ; i++ )
	{
		XNDNode* pNode = Manager->NewNode(this);
		pRes->AddRef();
		pNode->Resource = pRes;
		pNode->LoadHead2( pRes , io );
		m_Childs.push_back( pNode );
	}

	return TRUE;
}

vBOOL XNDNode::LoadHead3(VRes2Memory* pRes, VMemoryReader& io)
{
	TestMemOverRange("XNDNode.LoadHead3 0");
	INT nLen;
	io.Read(&nLen, sizeof(INT));
	TestMemOverRange("XNDNode.LoadHead3 1.1");
	if (nLen > 0)
	{
		CHAR* pChar = ::new(__FILE__, __LINE__) CHAR[nLen + 1];
		TestMemOverRange("XNDNode.LoadHead3 1.2.1");
		io.Read(pChar, nLen*sizeof(CHAR));
		TestMemOverRange("XNDNode.LoadHead3 1.2.2");
		pChar[nLen] = (int)NULL;
		TestMemOverRange("XNDNode.LoadHead3 1.2.3.0");
		SetName(pChar);
		TestMemOverRange(VStringA_FormatV("XNDNode.LoadHead3 1.2.3, %s", pChar).c_str());
		delete[] pChar;
	}
	else
	{
		SetName("");
		TestMemOverRange("XNDNode.LoadHead3 1.2.4");
	}
	TestMemOverRange("XNDNode.LoadHead3 1.2");

	io.Read(&m_ClassID, sizeof(m_ClassID));
	io.Read(&m_UserFlags, sizeof(m_UserFlags));
	TestMemOverRange("XNDNode.LoadHead3 1.3");

	UINT nAttrib = 0;
	io.Read(&nAttrib, sizeof(nAttrib));
	TestMemOverRange("XNDNode.LoadHead3 1.4.00");
	m_Attribs.resize(nAttrib);
	TestMemOverRange("XNDNode.LoadHead3 1.4.0");
	for (size_t i = 0; i < nAttrib; i++)
	{
		m_Attribs[i] = Manager->NewAttrib(this);
		TestMemOverRange("XNDNode.LoadHead3 1.4.1");
		pRes->AddRef();
		m_Attribs[i]->Resource = pRes;

		io.Read(&nLen, sizeof(INT));
		TestMemOverRange(VStringA_FormatV("XNDNode.LoadHead3 1.4.2:%d",nLen).c_str());
		if (nLen > 0)
		{
			CHAR* pChar = ::new(__FILE__, __LINE__) CHAR[nLen + 1];
			TestMemOverRange("XNDNode.LoadHead3 1.4.3.1:");
			io.Read(pChar, nLen*sizeof(CHAR));
			TestMemOverRange("XNDNode.LoadHead3 1.4.3.2");
			pChar[nLen] = (int)NULL;
			TestMemOverRange("XNDNode.LoadHead3 1.4.3.3");
			m_Attribs[i]->SetName(pChar);
			TestMemOverRange(VStringA_FormatV("XNDNode.LoadHead3 1.4.3.4, %s", pChar).c_str());
			delete[] pChar;
		}
		else
		{
			m_Attribs[i]->SetName("");
			TestMemOverRange("XNDNode.LoadHead3 1.4.3.5");
		}
		TestMemOverRange("XNDNode.LoadHead3 1.4.3");

		io.Read(&nLen, sizeof(INT));
		if (nLen > 0)
		{
			CHAR* pChar = ::new(__FILE__, __LINE__) CHAR[nLen + 1];
			TestMemOverRange("XNDNode.LoadHead3 1.4.4.1");
			io.Read(pChar, nLen*sizeof(CHAR));
			TestMemOverRange("XNDNode.LoadHead3 1.4.4.2");
			pChar[nLen] = (int)NULL;
			TestMemOverRange("XNDNode.LoadHead3 1.4.4.3");
			m_Attribs[i]->SetKey(pChar);
			TestMemOverRange("XNDNode.LoadHead3 1.4.4.4");
			delete[] pChar;
		}
		else
		{
			m_Attribs[i]->SetKey("");
			TestMemOverRange("XNDNode.LoadHead3 1.4.4.5");
		}
		TestMemOverRange("XNDNode.LoadHead3 1.4.4");

		UINT nOffset;
		io.Read(&nOffset, sizeof(UINT));
		m_Attribs[i]->Offset = nOffset;
		TestMemOverRange("XNDNode.LoadHead3 1.4.5");
		UINT nLength;
		io.Read(&nLength, sizeof(UINT));
		m_Attribs[i]->Length = nLength;
		TestMemOverRange("XNDNode.LoadHead3 1.4.6");
		BYTE ver;
		io.Read(&ver, sizeof(BYTE));
		m_Attribs[i]->Version = ver;
	}
	TestMemOverRange("XNDNode.LoadHead3 1.4");

	UINT nChild = 0;
	io.Read(&nChild, sizeof(nChild));
	for (UINT i = 0; i < nChild; i++)
	{
		XNDNode* pNode = Manager->NewNode(this);
		pRes->AddRef();
		pNode->Resource = pRes;
		pNode->LoadHead3(pRes, io);
		m_Childs.push_back(pNode);
	}

	TestMemOverRange("XNDNode.LoadHead3 1");
	return TRUE;
}

vBOOL XNDNode::Save( VFile_Base& io )
{
	UINT_PTR beginPos = io.GetPosition();

	CHAR szName[8];
	memset( szName , 0 , sizeof(szName) );
	strcpy( szName , "XNDVer4" );
	//for (int i = 0; i < 8; i++)
	//{
	//	USHORT svWord = (USHORT)szName[i];
	//	io.Write(&svWord, sizeof(USHORT));
	//}
	io.Write( szName , sizeof(szName) );

	DWORD HeadSize = 0;
	io.Write( &HeadSize , sizeof(DWORD) );

	SaveHead( io );
	HeadSize = (DWORD)io.GetPosition()-sizeof(szName)-4;
	SaveData( io );

	io.Seek( beginPos+sizeof(szName) , VFile_Base::begin );

	io.Write( &HeadSize , sizeof(DWORD) );
	SaveHead( io );

	return TRUE;
}

vBOOL XNDNode::Load( VRes2Memory* pRes )
{
	ASSERT(pRes != NULL);
	if(pRes == NULL)
		return FALSE;
	pRes->AddRef();
	Safe_Release( Resource );
	Resource = pRes;

	CHAR szName[8];
	auto firstChunkSize = 8 * sizeof(CHAR) + sizeof(DWORD);
	auto pMem = pRes->Ptr(0, firstChunkSize);
	ASSERT(pMem);
	VMemoryReader io(pMem, firstChunkSize);
	//for (int i = 0; i < 8; i++)
	//{
	//	USHORT svWord;
	//	io.Read(&svWord, sizeof(USHORT));
	//	szName[i] = svWord;
	//}
	io.Read( szName , sizeof(szName) );
	szName[_countof(szName) - 1] = 0;
	DWORD headSize;
	io.Read( &headSize , sizeof(headSize) );
	pRes->Free();
	
	if( strcmp(szName,"XNDVer2")==0 )
	{	
		pMem = pRes->Ptr(firstChunkSize, headSize);
		ASSERT(pMem);
		VMemoryReader ioUse(pMem, headSize);
		if( FALSE == LoadHead1( pRes , ioUse ) )
		{
			pRes->Free();
			return FALSE;
		}
		else
		{
			pRes->Free();
			return TRUE;
		}
	}
	else if(strcmp(szName, "XNDVer3")==0)
	{
		pMem = pRes->Ptr(firstChunkSize, headSize);
		ASSERT(pMem);
		VMemoryReader ioUse(pMem, headSize);
		if( FALSE == LoadHead2( pRes , ioUse ) )
		{
			pRes->Free();
			return FALSE;
		}
		else
		{
			pRes->Free();
			return TRUE;
		}
	}
	else if (strcmp(szName, "XNDVer4") == 0)
	{
		pMem = pRes->Ptr(firstChunkSize, headSize);
		ASSERT(pMem);
		VMemoryReader ioUse(pMem, headSize);
		if (FALSE == LoadHead3(pRes, ioUse))
		{
			pRes->Free();
			return FALSE;
		}
		else
		{
			pRes->Free();
			return TRUE;
		}
	}
	else
	{
		return FALSE;
	}
}

void XNDNode::SetName( LPCSTR szName )
{
	mName = szName;
}

vBOOL XNDNode::Merge(XNDNode* target , XNDNode* n0 , XNDNode* n1 )
{
	if(n0)
	{
		for( size_t i=0 ; i<n0->m_Attribs.size() ; i++ )
		{
			XNDAttrib* pAttr =  target->AddAttrib(n0->m_Attribs[i]->GetName().c_str());
			n0->m_Attribs[i]->BeginRead(__FILE__, __LINE__);
			UINT_PTR len = n0->m_Attribs[i]->GetLength();
			BYTE* data = ::new(__FILE__, __LINE__) BYTE[len];
			n0->m_Attribs[i]->Read(data,len);
			n0->m_Attribs[i]->EndRead();

			pAttr->BeginWrite();
			pAttr->Write( data , len );
			pAttr->EndWrite();

			delete[] data;
		
		}
	}
	if(n1)
	{
		for( size_t i=0 ; i<n1->m_Attribs.size() ; i++ )
		{
			XNDAttrib* pAttr = target->GetAttrib(n1->m_Attribs[i]->GetName().c_str());
			if(pAttr!=NULL)
			{
				continue;
			}
			pAttr =  target->AddAttrib(n1->m_Attribs[i]->GetName().c_str());
			n1->m_Attribs[i]->BeginRead(__FILE__, __LINE__);
			UINT_PTR len = n1->m_Attribs[i]->GetLength();
			BYTE* data = ::new(__FILE__, __LINE__) BYTE[len];
			n1->m_Attribs[i]->Read(data,len);
			n1->m_Attribs[i]->EndRead();		

			pAttr->BeginWrite();
			pAttr->Write( data , len );
			pAttr->EndWrite();

			delete[] data;		
		}
	}
	if(n0)
	{
		for( size_t i=0 ; i<n0->m_Childs.size() ; i++ )
		{
			target->AddNode( n0->GetName().c_str(), n0->GetClassID() , n0->GetUserFlags() );
		}
	}
	if(n1)
	{
		for( size_t i=0 ; i<n1->m_Childs.size() ; i++ )
		{
			XNDNode* node = target->GetChild(n1->GetName().c_str());
			if(node)
				continue;

			target->AddNode( n1->GetName().c_str(), n1->GetClassID() , n1->GetUserFlags() );
		}
	}
	for( size_t i=0 ; i<target->m_Childs.size() ; i++ )
	{
		XNDNode* ctarget = target->m_Childs[i];
		XNDNode* cn0 = NULL;
		if(n0)
		{
			cn0 = n0->GetChild(ctarget->GetName().c_str());
		}
		XNDNode* cn1 = NULL;
		if(n1)
		{
			cn1 = n1->GetChild(ctarget->GetName().c_str());
		}
		Merge(ctarget,cn0,cn1);
	}
	return TRUE;
}

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API XNDNode* XNDNode_New()
	{
		XNDData* xndData = ::new(__FILE__, __LINE__) XNDData();
		auto ret = xndData->NewNode();
		xndData->Release();
		return ret;
	}
	VFX_API int XNDNode_GetResourceRefCount(XNDNode* node)
	{
		return node->Resource->GetRefCount();
	}

	 VFX_API void XNDNode_AddRef(XNDNode* node)
	{
		if(node)
			node->AddRef();
	}
	
	 VFX_API void XNDNode_TryReleaseHolder(XNDNode* node)
	{
		if(node)
			node->TryReleaseHolder();
	}

	VFX_API VRes2Memory* XNDNode_GetR2M(XNDNode* node)
	{
		if (node)
			return node->Resource;
		return nullptr;
	}

	 VFX_API void XNDNode_SetName(XNDNode* node, LPCSTR name)
	{
		node->SetName(name);
	}

	VFX_API const char* XNDNode_GetName(XNDNode* node)
	{
		return node->GetName().c_str();
	}

	VFX_API vIID XNDNode_GetClassID(XNDNode* node)
	{
		return node->GetClassID();
	}

	 VFX_API XNDNode* XNDNode_AddNode(XNDNode* node,LPCSTR name,INT64 classId,DWORD flags)
	{
		return node->AddNode(name,classId,flags);
	}
	VFX_API XNDNode* XNDNode_AddNodeWithSource(XNDNode* node, XNDNode* srcNode)
	{
		return node->AddNode(srcNode);
	}

	VFX_API vBOOL XNDNode_DelNode(XNDNode* node,XNDNode* childNode)
	{
		return node->DelNode(childNode);
	}

	 VFX_API XNDNode* XNDNode_FindNode(XNDNode* node,LPCSTR name)
	{
		return node->GetChild(name);
	}

	VFX_API XNDAttrib* XNDNode_AddAttrib(XNDNode* node,LPCSTR name)
	{
		return node->AddAttrib(name);
	}
	VFX_API XNDAttrib* XNDNode_AddAttribWithSource(XNDNode* node,XNDAttrib* childAtt)
	{
		return node->AddAttrib(childAtt);
	}

	VFX_API XNDAttrib* XNDNode_FindAttrib(XNDNode* node,LPCSTR name)
	{
		return node->GetAttrib(name);
	}

	VFX_API vBOOL XNDNode_DelAttrib(XNDNode* node,LPCSTR name)
	{
		return node->DelAttrib(name);
	}

	VFX_API int XNDNode_GetNodeNumber(XNDNode* node)
	{
		return (int)node->GetChildVector().size();
	}

	VFX_API XNDNode* XNDNode_GetNode(XNDNode* node,int iNode)
	{
		XNDNode::XNDNodeVector& nodes = node->GetChildVector();
		return nodes[iNode];
	}

	VFX_API int XNDNode_GetAttribNumber(XNDNode* node)
	{
		return (int)node->GetAttribVector().size();
	}

	VFX_API XNDAttrib* XNDNode_GetAttrib(XNDNode* node,int iAttrib)
	{
		XNDNode::XNDAttribVector& attribs = node->GetAttribVector();
		return attribs[iAttrib];
	}

	VFX_API vBOOL XNDNode_Save(XNDNode* node,VFile_Base* file)
	{
		return node->Save(*file);
	}

	VFX_API vBOOL XNDNode_Load(XNDNode* node,VRes2Memory* pRes)
	{
		return node->Load(pRes);
	}

	VFX_API BYTE XNDAttrib_GetVersion(XNDAttrib* attrib)
	{
		if(attrib != NULL)
			return attrib->GetVersion();
		return 0;
	}
	VFX_API void XNDAttrib_SetVersion(XNDAttrib* attrib, BYTE ver)
	{
		if(attrib != NULL)
			return attrib->SetVersion(ver);
	}

	VFX_API UINT_PTR XNDAttrib_GetLength(XNDAttrib* attrib)
	{
		if(attrib != NULL)
			return attrib->GetLength();
		return 0;
	}

	VFX_API VRes2Memory* XNDAttrib_GetR2M(XNDAttrib* attrib)
	{
		if (attrib)
			return attrib->Resource;
		return nullptr;
	}

	VFX_API const char* XNDAttrib_GetKey(XNDAttrib* attrib)
	{
		if (attrib != NULL)
		{
			const char* chrKey = attrib->GetKey().c_str();
			return chrKey;
		}
		return NULL;
	}
	VFX_API void XNDAttrib_SetKey(XNDAttrib* attrib, const char* chr)
	{
		if (attrib != NULL)
		{
			attrib->SetKey(chr);
		}
	}

	VFX_API const char* XNDAttrib_GetName(XNDAttrib* attrib)
	{
		const char* wszName = attrib->GetName().c_str();
		return wszName;
	}

	VFX_API void XNDAttrib_BeginRead(XNDAttrib* attrib, const char* file, int line)
	{
		attrib->BeginRead(file, line);
	}

	VFX_API void XNDAttrib_EndRead(XNDAttrib* attrib)
	{
		attrib->EndRead();
	}

	VFX_API void XNDAttrib_BeginWrite(XNDAttrib* attrib)
	{
		attrib->BeginWrite();
	}

	VFX_API void XNDAttrib_EndWrite(XNDAttrib* attrib)
	{
		attrib->EndWrite();
	}

	VFX_API int XNDAttrib_Read(XNDAttrib* attrib,void* data,int size)
	{
		return (int)attrib->Read(data,size);
	}
	VFX_API CHAR* XNDAttrib_ReadStringA(XNDAttrib* attrib)
	{
		UINT length = 0;
		attrib->Read(&length,sizeof(UINT));
		if ((UINT)(attrib->GetLength() - attrib->GetPosition()) < length)
		{
			const char* file = "Xnd";
			if(attrib->Resource)
				file = attrib->Resource->Name();
			VFX_LTRACE(ELTT_Error, "XND(%s)->Attrib(%s): ReadStringW Error!length=%d/%d\r\n", file, attrib->GetName().c_str(), length, (UINT)(attrib->GetLength() - attrib->GetPosition()));
			return NULL;
		}
		CHAR* str = new CHAR[length+1];
		if (length > 0)
		{
			attrib->Read(str, sizeof(CHAR)*length);
		}
		str[length] = (int)NULL;
		return str;
	}

	VFX_API void XNDAttrib_FreeStringA(CHAR* str)
	{
		delete[] str;
	}

	VFX_API int XNDAttrib_Write(XNDAttrib* attrib,void* data,int size)
	{
		return (int)attrib->Write(data,size);
	}

	VFX_API void XNDAttrib_WriteStringA(XNDAttrib* attrib,CHAR* data)
	{
		int length = (int)strlen(data);
		attrib->Write(&length,sizeof(int));
		attrib->Write(data,sizeof(CHAR)*length);
	}
	VFX_API vBOOL SDK_XNDAttrib_GetReading(XNDAttrib* attrib)
	{
		if (attrib == nullptr)
			return FALSE;
		return attrib->GetReading();
	}
	VFX_API vBOOL SDK_XNDAttrib_GetWriting(XNDAttrib* attrib)
	{
		if (attrib == nullptr)
			return FALSE;
		return attrib->GetWriting();
	}
};

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif