#include "VXml.h"
#include "../debug/vfxdebug.h"
#include "../../IUnknown.h"
#include "../r2m/VPakFile.h"

#define new VNEW

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wnull-conversion"
#endif

#if !defined(PLATFORM_WIN)
int strcpy_s(char* _Destination, size_t _SizeInBytes, char const* _Source)
{
	strcpy(_Destination, _Source);
	return strlen(_Source);
}
#endif

VXmlHolderA::VXmlHolderA()
	: mData(NULL)
{
}
VXmlHolderA::~VXmlHolderA()
{
	if(mData)
	{
		delete[] mData;
		mData=NULL;
	}
	ClearStrings();
}

 VXmlNodeA* VXmlHolderA::NewNode(LPCSTR name,LPCSTR value,vBOOL bDeepCopyStr)
{
	VXmlNodeA* result = NULL;
	if(bDeepCopyStr)
	{
		char* saveName = NULL;
		if(name != NULL)
		{
			int len = (int)strlen(name);
			saveName = new char[len+1];
			strcpy_s(saveName,len+1,name);
			saveName[len] = (int)NULL;
		}

		char* saveValue = NULL;
		if(value != NULL)
		{
			int len = (int)strlen(value);
			saveValue = new char[len+1];
			strcpy_s(saveValue,len+1,value);
			saveValue[len] = (int)NULL;
		}

		result = mDoc.allocate_node(rapidxml::node_element,
			saveName,
			saveValue);

		if(saveName != NULL)
			mStringHolder.push_back(saveName);
		if(saveValue != NULL)
			mStringHolder.push_back(saveValue);
	}
	else
	{
		result = mDoc.allocate_node(rapidxml::node_element,
				name,
				value);
	}
	return result;
}

 VXmlAttribA* VXmlHolderA::NewAttrib(LPCSTR name,LPCSTR value,vBOOL bDeepCopyStr)
{
	VXmlAttribA* result = NULL;
	if(bDeepCopyStr)
	{
		char* saveName = NULL;
		if(name != NULL)
		{
			int len = (int)strlen(name);
			saveName = new char[len+1];
			strcpy_s(saveName,len+1,name);
			saveName[len] = (int)NULL;
		}

		char* saveValue = NULL;
		if(value != NULL)
		{
			int len = (int)strlen(value);
			saveValue = new char[len+1];
			strcpy_s(saveValue,len+1,value);
			saveValue[len] = (int)NULL;
		}

		result = mDoc.allocate_attribute(saveName,saveValue);

		if(saveName != NULL)
			mStringHolder.push_back(saveName);
		if(saveValue != NULL)
			mStringHolder.push_back(saveValue);
	}
	else
	{
		result = mDoc.allocate_attribute(name,value);
	}
	return result;
}

 void VXmlHolderA::SetNodeValue(VXmlNodeA* node,LPCSTR value,vBOOL bDeepCopyStr)
{
	if(bDeepCopyStr)
	{
		int len = (int)strlen(value);
		char* saveValue = new char[len+1];
		strcpy_s(saveValue,len,value);
		saveValue[len] = (int)NULL;

		node->value(saveValue);
		mStringHolder.push_back(saveValue);
	}
	else
	{
		node->value(value);
	}
}

 void VXmlHolderA::SetAttribValue(VXmlAttribA* attrib,LPCSTR value,vBOOL bDeepCopyStr)
{
	if(bDeepCopyStr)
	{
		int len = (int)strlen(value);
		char* saveValue = new char[len+1];
		strcpy_s(saveValue,len,value);
		saveValue[len] = (int)NULL;

		attrib->value(saveValue);
		mStringHolder.push_back(saveValue);
	}
	else
	{
		attrib->value(value);
	}
}

 void VXmlHolderA::ClearStrings()
{
	for(std::list<char*>::iterator i=mStringHolder.begin();i!=mStringHolder.end();i++)
	{
		delete[] (*i);
	}
	mStringHolder.clear();
}

VXmlHolderW::VXmlHolderW()
	: mData(NULL)
{

}
VXmlHolderW::~VXmlHolderW()
{
	if(mData)
	{
		delete[] mData;
		mData=NULL;
	}
}

extern "C"  void vfxMemory_SetDebugInfo(void* memory, LPCSTR info);
VCritical GXMLLoadLocker;
using namespace EngineNS;

extern "C"
{
	VFX_API VXmlHolderA* RapidXml_LoadFileA(LPCSTR filename)
	{
		AutoRef<VRes2Memory> f2m(F2MManager::Instance.GetF2M(filename));
		if (f2m == nullptr)
			return nullptr;
		auto pMem = f2m->Ptr();
		VXmlHolderA* result = NULL;
		try
		{
			result = new VXmlHolderA();
			vfxMemory_SetDebugInfo(result, filename);
			DWORD len = (DWORD)f2m->Length();
			result->mData = new char[len + 1];
			memcpy(result->mData, pMem, len);
			result->mData[len] = (int)NULL;
			result->mDoc.parse<0>(result->mData);
			
			f2m->Free();
			f2m->TryReleaseHolder();
			return result;
		}
		catch (...)
		{
			if (result != NULL)
				delete result;

			f2m->Free();
			f2m->TryReleaseHolder();
			return NULL;
		}
		//VAutoLock(GXMLLoadLocker);
		//VXmlHolderA* result = NULL;

		//VString name(filename);
		//ViseFile file;
		//if (file.Open(name.c_str(), VFile::modeRead/*|VFile::shareDenyWrite*/) == FALSE)
		//{
		//	return NULL;
		//}

		//try
		//{
		//	result = new VXmlHolderA();
		//	vfxMemory_SetDebugInfo(result, filename);
		//	DWORD len = (DWORD)file.GetLength();
		//	result->mData = new char[len + 1];
		//	file.Read(result->mData, len);
		//	result->mData[len] = (int)NULL;
		//	result->mDoc.parse<0>(result->mData);
		//	file.Close();
		//	return result;
		//}
		//catch (...)
		//{
		//	if (result != NULL)
		//		delete result;
		//	file.Close();
		//	return NULL;
		//}
	}

	VFX_API void RapidXmlA_Delete(VXmlHolderA* p)
	{
		delete p;
	}
	VFX_API VStringObject* RapidXmlA_GetXMLString(VXmlHolderA* xmlHolder)
	{
		VStringObject* result = new VStringObject();
		rapidxml::print(std::back_inserter(result->mText), xmlHolder->mDoc, 0);
		result->mText = std::string("<?xml version=\"1.0\" encoding=\"gb2312\"?>\r\n") + result->mText;
#if defined(ANDROID) || defined(IOS)
		result->mText = VStringA_Utf82Gbk(text.c_str());
#endif
		return result;
	}
	VFX_API void RapidXmlA_SaveXML(VXmlHolderA* xmlHolder, const char* filename)
	{
		if (xmlHolder == NULL)
			return;

		VStringA text;
		rapidxml::print(std::back_inserter(text), xmlHolder->mDoc, 0);
		text = VStringA("<?xml version=\"1.0\" encoding=\"gb2312\"?>\r\n") + text;
#if defined(ANDROID) || defined(IOS)
		text = VStringA_Utf82Gbk(text.c_str());
#endif
		ViseFile file;
		if (file.Open(filename, VFile::modeWrite | VFile::modeCreate) == TRUE)
		{
			file.Write(text.c_str(), text.length());
			file.Close();
		}
		//FILE* fp = fopen(filename, "wb");
		//if(fp)
		//{
		//	fwrite(text.c_str(), 1, text.length(), fp);
		//	fclose(fp);
		//}
	}

	VFX_API VXmlNodeA* RapidXmlA_RootNode(VXmlHolderA* xmlHolder)
	{
		return xmlHolder->mDoc.first_node();
	}
	VFX_API VXmlHolderA* RapidXmlA_NewXmlHolder()
	{
		return new VXmlHolderA();
	}
	VFX_API void RapidXmlA_append_node(VXmlHolderA* xmlHolder, VXmlNodeA* node)
	{
		xmlHolder->mDoc.append_node(node);
	}
	VFX_API VXmlHolderA* RapidXmlA_ParseXML(char* xmlString)
	{
#if defined(ANDROID) || defined(IOS)
		auto ansiStr = VStringA_Utf82Gbk(xmlString);
#else
		auto ansiStr = VStringA(xmlString); 
#endif

		int size = (int)ansiStr.length() +1;
		VXmlHolderA* pHolder = new VXmlHolderA();
		if(pHolder->mData)
			delete[] pHolder->mData;
		pHolder->mData = new CHAR[size];
		memcpy(pHolder->mData, ansiStr.c_str(), sizeof(CHAR)*(size-1));
		pHolder->mData[size-1]=0;

		pHolder->mDoc.parse<0>(pHolder->mData);
		return pHolder;
	}
	VFX_API char* RapidXmlA_GetStringFromXML(VXmlHolderA* xmlHoder)
	{
		if(xmlHoder == NULL)
			return NULL;

		VStringA text;
		rapidxml::print(std::back_inserter(text), xmlHoder->mDoc, 0);
#if defined(ANDROID) || defined(IOS)
		auto ansiStr = VStringA_Gbk2Utf8(text.c_str());
		int length = ansiStr.length();
		CHAR* retStr = new CHAR[length + 1];
		memcpy(retStr, ansiStr.c_str(), sizeof(CHAR)*length);
		retStr[length] = (int)NULL;
		return retStr;
#else
		char* retStr = new char[text.length() + 1];
		strcpy_s(retStr, text.length() + 1, text.c_str());
		retStr[text.length()] = NULL;
		return retStr;
#endif // ANDROID
		
	}
	VFX_API void RapidXmlA_FreeString(char* str)
	{
		delete[] str;
	}

	VFX_API VXmlNodeA* RapidXmlNodeA_allocate_node(VXmlHolderA* holder, const char* name, const char* value)
	{
		return holder->NewNode(name, value);
	}
	VFX_API VXmlAttribA* RapidXmlNodeA_allocate_attribute(VXmlHolderA* holder, const char* name, const char* value)
	{
		return holder->NewAttrib(name, value);
	}
	// VXmlNodeA* RapidXmlNodeA_allocate_node(VXmlNodeA* node, const char* name, const char* value)
	//{
	//	return node->document()->allocate_node(rapidxml::node_element, name, value);
	//}
	// VXmlAttribA* RapidXmlNodeA_allocate_attribute(VXmlNodeA* node, const char* name, const char* value)
	//{
	//	return node->document()->allocate_attribute(name, value);
	//}
	VFX_API void RapidXmlNodeA_append_node(VXmlNodeA* node, VXmlNodeA* childNode)
	{
		node->append_node(childNode);
	}
	VFX_API void RapidXmlNodeA_append_attribute(VXmlNodeA* node, VXmlAttribA* childAttr)
	{
		node->append_attribute(childAttr);
	}

	VFX_API void RapidXmlNodeA_remove_node(VXmlNodeA* node, VXmlNodeA* childNode)
	{
		node->remove_node(childNode);
	}
	VFX_API void RapidXmlNodeA_remove_attribute(VXmlNodeA* node, VXmlAttribA* childAttr)
	{
		node->remove_attribute(childAttr);
	}

	VFX_API VXmlNodeA* RapidXmlNodeA_first_node(VXmlNodeA* node,const char* name)
	{
		return node->first_node(name);
	}
	VFX_API VXmlAttribA* RapidXmlNodeA_first_attribute(VXmlNodeA* node,const char* name)
	{
        return node->first_attribute(name);
	}
	VFX_API VXmlNodeA* RapidXmlNodeA_next_sibling(VXmlNodeA* node)
	{
		return node->next_sibling();
	}

	VFX_API char* CreateUTF8_By_GBK(char* src)
	{
		//auto ansiStr1 = VStringA::Gbk2Utf8("µÇÂ½ÖÐ!1231_128");
#if defined(ANDROID) || defined(IOS)
		auto ansiStr = VStringA_Gbk2Utf8(src);
#else
		VStringA ansiStr = src;
#endif
		size_t length = ansiStr.length();
		CHAR* retStr = new CHAR[length + 1];
		memcpy(retStr, ansiStr.c_str(), sizeof(CHAR)*length);
		retStr[length] = (int)NULL;
		return retStr;
	}
	
	VFX_API char* RapidXmlNodeA_name(VXmlNodeA* node, int* pNeedFreeStr)
	{
		CHAR* value = node->name();
		*pNeedFreeStr = TRUE;
		return CreateUTF8_By_GBK(value);
//#ifdef ANDROID
//		CHAR* value = node->name();
//		*pNeedFreeStr = TRUE;
//		return CreateUTF8_By_GBK(value);
//		/*size_t len = strlen(value);
//		WordCodeHelper helper;
//		helper.SetOriginCode("GB18030//TRANSLIT");
//		helper.SetDestCode("UTF-8");
//		CHAR desChar[256];
//		size_t oriLen = len;
//		CHAR* desText = WordCodeHelper::GetFixedCharBuffer(desChar, oriLen);
//		size_t desLen = 256;
//		if (oriLen > 256)
//			desLen = oriLen;
//		else
//			desLen = 256;
//		size_t leftLen = desLen;
//		helper.ChangeCodeStatic(helper.mOriginCode, helper.mDestCode, value, &len, desText, &leftLen);
//		int length = desLen - leftLen;
//		desText[length] = NULL;
//		CHAR* retStr = new CHAR[length + 1];
//		memcpy(retStr, desText, sizeof(CHAR)*length);
//		retStr[length] = NULL;
//		WordCodeHelper::ReleaseFixedCharBuffer(desText, oriLen);
//		return retStr;*/
//#else
//		*pNeedFreeStr = FALSE;
//		return  node->name();
//#endif // ANDROID
		//return node->name();
	}
	VFX_API char* RapidXmlNodeA_value(VXmlNodeA* node, int* pNeedFreeStr)
	{
		CHAR* value = node->value();
		*pNeedFreeStr = TRUE;
		return CreateUTF8_By_GBK(value);
//#ifdef ANDROID
//		CHAR* value = node->value();
//		*pNeedFreeStr = TRUE;
//		return CreateUTF8_By_GBK(value);
//		/*size_t len = strlen(value);
//		WordCodeHelper helper;
//		helper.SetOriginCode("GB18030//TRANSLIT");
//		helper.SetDestCode("UTF-8");
//		CHAR desChar[256];
//		size_t oriLen = len;
//		CHAR* desText = WordCodeHelper::GetFixedCharBuffer(desChar, oriLen);
//		size_t desLen = 256;
//		if (oriLen > 256)
//			desLen = oriLen;
//		else
//			desLen = 256;
//		size_t leftLen = desLen;
//		helper.ChangeCodeStatic(helper.mOriginCode, helper.mDestCode, value, &len, desText, &leftLen);
//		int length = desLen - leftLen;
//		desText[length] = NULL;
//		CHAR* retStr = new CHAR[length + 1];
//		memcpy(retStr, desText, sizeof(CHAR)*length);
//		retStr[length] = NULL;
//		WordCodeHelper::ReleaseFixedCharBuffer(desText, oriLen);
//		return retStr;*/
//#else
//		*pNeedFreeStr = FALSE;
//		return  node->value();
//#endif // ANDROID
		//return node->value();
	}
	VFX_API char* RapidXmlNodeA_GetStringFromNode(VXmlNodeA* node)
	{
		VStringA text;
		rapidxml::print(std::back_inserter(text), *node, 0);

		CHAR* value = (CHAR*)text.c_str();
		return CreateUTF8_By_GBK(value);
//
//#ifdef ANDROID
//		CHAR* value = (CHAR*)text.c_str();
//		return CreateUTF8_By_GBK(value);
//		/*size_t len = strlen(value);
//		WordCodeHelper helper;
//		helper.SetOriginCode("GB18030//TRANSLIT");
//		helper.SetDestCode("UTF-8");
//		CHAR desChar[256];
//		size_t oriLen = len;
//		CHAR* desText = WordCodeHelper::GetFixedCharBuffer(desChar, oriLen);
//		size_t desLen = 256;
//		if (oriLen > 256)
//			desLen = oriLen;
//		else
//			desLen = 256;
//		size_t leftLen = desLen;
//		helper.ChangeCodeStatic(helper.mOriginCode, helper.mDestCode, value, &len, desText, &leftLen);
//		int length = desLen - leftLen;
//		desText[length] = NULL;
//		CHAR* retStr = new CHAR[length + 1];
//		memcpy(retStr, desText, sizeof(CHAR)*length);
//		retStr[length] = NULL;
//		WordCodeHelper::ReleaseFixedCharBuffer(desText, oriLen);
//		return retStr;*/
//#else
//		char* retStr = new char[text.length() + 1];
//		strcpy_s(retStr, text.length() + 1, text.c_str());
//		retStr[text.length()] = NULL;
//		return retStr;
//#endif // ANDROID
//
	}
	VFX_API void RapidXmlNodeA_FreeString(char* str)
	{
		delete[] str;
	}

	VFX_API char* RapidXmlAttribA_name(VXmlAttribA* attr, int* pNeedFreeStr)
	{
		CHAR* value = attr->name();
		*pNeedFreeStr = TRUE;
		return CreateUTF8_By_GBK(value);
//#ifdef ANDROID
//		CHAR* value = attr->name();
//		*pNeedFreeStr = TRUE;
//		return CreateUTF8_By_GBK(value);
//		/*size_t len = strlen(value);
//		WordCodeHelper helper;
//		helper.SetOriginCode("GB18030//TRANSLIT");
//		helper.SetDestCode("UTF-8");
//		CHAR desChar[256];
//		size_t oriLen = len;
//		CHAR* desText = WordCodeHelper::GetFixedCharBuffer(desChar, oriLen);
//		size_t desLen = 256;
//		if (oriLen > 256)
//			desLen = oriLen;
//		else
//			desLen = 256;
//		size_t leftLen = desLen;
//		helper.ChangeCodeStatic(helper.mOriginCode, helper.mDestCode, value, &len, desText, &leftLen);
//		int length = desLen - leftLen;
//		desText[length] = NULL;
//		CHAR* retStr = new CHAR[length + 1];
//		memcpy(retStr, desText, sizeof(CHAR)*length);
//		retStr[length] = NULL;
//		WordCodeHelper::ReleaseFixedCharBuffer(desText, oriLen);
//		return retStr;*/
//#else
//		*pNeedFreeStr = FALSE;
//		return  attr->name();
//#endif // ANDROID
		//return attr->name();
	}

	
	VFX_API char* RapidXmlAttribA_value(VXmlAttribA* attr, int* pNeedFreeStr)
	{
		CHAR* value = attr->value();
		*pNeedFreeStr = TRUE;
		return CreateUTF8_By_GBK(value);
//#ifdef ANDROID
//		CHAR* value = attr->value();
//		*pNeedFreeStr = TRUE;
//		return CreateUTF8_By_GBK(value);
//		/*size_t len = strlen(value);
//		WordCodeHelper helper;
//		helper.SetOriginCode("GB18030//TRANSLIT");
//		helper.SetDestCode("UTF-8");
//		CHAR desChar[256];
//		size_t oriLen = len;
//		CHAR* desText = WordCodeHelper::GetFixedCharBuffer(desChar, oriLen);
//		size_t desLen = 256;
//		if (oriLen > 256)
//			desLen = oriLen;
//		else
//			desLen = 256;
//		size_t leftLen = desLen;
//		helper.ChangeCodeStatic(helper.mOriginCode, helper.mDestCode, value, &len, desText, &leftLen);
//		int length = desLen - leftLen;
//		desText[length] = NULL;
//		CHAR* retStr = new CHAR[length+1];
//		memcpy(retStr, desText, sizeof(CHAR)*length);
//		retStr[length] = NULL;
//		WordCodeHelper::ReleaseFixedCharBuffer(desText, oriLen);
//		return retStr;*/
//#else
//		*pNeedFreeStr = FALSE;
//		return  attr->value();
//#endif // ANDROID
	}
	VFX_API VXmlAttribA* RapidXmlAttribA_next_sibling(VXmlAttribA* attr)
	{
		return attr->next_attribute();
	}
};

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif
