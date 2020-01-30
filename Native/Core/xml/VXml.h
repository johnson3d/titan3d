#pragma once
#include "../precompile.h"

#include "rapidxml.h"
#include "rapidxml_utils.h"
#include "rapidxml_print.h"

typedef rapidxml::xml_node<char> VXmlNodeA;
typedef rapidxml::xml_node<wchar_t> VXmlNodeW;

typedef rapidxml::xml_attribute<char> VXmlAttribA;
typedef rapidxml::xml_attribute<wchar_t> VXmlAttribW;

typedef rapidxml::file<char> VXmlFileA;
typedef rapidxml::file<wchar_t> VXmlFileW;

typedef rapidxml::xml_document<char> VXmlDocA;
typedef rapidxml::xml_document<wchar_t> VXmlDocW;

struct VXmlHolderA
{
	 VXmlHolderA();
	 ~VXmlHolderA();

	VXmlDocA mDoc;
	char* mData;

	 VXmlNodeA* NewNode(LPCSTR name,LPCSTR value,vBOOL bDeepCopyStr=TRUE);
	 VXmlAttribA* NewAttrib(LPCSTR name,LPCSTR value,vBOOL bDeepCopyStr=TRUE);
	 void SetNodeValue(VXmlNodeA* node,LPCSTR value,vBOOL bDeepCopyStr=TRUE);
	 void SetAttribValue(VXmlAttribA* attrib,LPCSTR value,vBOOL bDeepCopyStr=TRUE);

	 void ClearStrings();

private:
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
	std::list<char*, VMem::malloc_allocator< char*, MyAllocator > >	mStringHolder;
};

struct VXmlHolderW
{
	 VXmlHolderW();
	 ~VXmlHolderW();

	VXmlDocW mDoc;
	wchar_t* mData;
};

extern "C"
{
	VFX_API void RapidXmlA_SaveXML(VXmlHolderA* xmlHolder, const char* filename);

	VFX_API VXmlHolderA* RapidXml_LoadFileA(LPCSTR filename);
	VFX_API void RapidXmlA_Delete(VXmlHolderA*);
}

#ifdef UNICODE
	#define VXmlNode VXmlNodeW
	#define VXmlAttrib VXmlAttribW
	#define VXmlFile VXmlFileW
	#define VXmlDoc VXmlDocW

	#define RapidXml_LoadFile RapidXml_LoadFileW
#else
	#define VXmlNode VXmlNodeA
	#define VXmlAttrib VXmlAttribA
	#define VXmlFile VXmlFileA
	#define VXmlDoc VXmlDocA

	#define RapidXml_LoadFile RapidXml_LoadFileA
#endif