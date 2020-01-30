#pragma once

#include "vfxReflectionMethod.h"

namespace vfx
{
	namespace vgc
	{
		class GCObject;

		class GCObject : public Reflection::VReflectBase
		{
		public:
			DYNAMIC_REFLECT(,0x11ec4acd4b98c0cf);
			static GCObject*			m_HeadObj;
			static GCObject*			m_TailObj;

			GCObject*			m_PrevObj;
			GCObject*			m_NextObj;
		protected:
			vBOOL				m_bReachabled;
		public:
			 void MarkChild();

		protected:
			virtual GCObject* Begin(){
				return NULL;
			}
			virtual GCObject* Next( GCObject* pNow ){
				return NULL;
			}
		};
	}
}