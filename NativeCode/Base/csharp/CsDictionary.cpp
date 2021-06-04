#include "CsDictionary.h"

#define new VNEW

NS_BEGIN

FCsDictionaryIterator* CsDictionaryImpl::NewIterator() 
{
	auto result = new FCsDictionaryIterator();
	result->It = mContain.begin();
	return result;
}

NS_END
