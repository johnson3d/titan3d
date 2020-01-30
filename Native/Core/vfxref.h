
#ifndef VFX_REF_USE_ALLOC
#define VFX_REF_USE_ALLOC	0
#endif

//#include "vfxdebug.h"

#if VFX_REF_USE_ALLOC
namespace VFX
{
	namespace detail
	{
		template<class _Ty>
		struct JudgeAuxIUnknown
		{
			static char JudgeMethod(...);
			
			template<typename _Ty2,typename _Base,typename _Ax>
			static int JudgeMethod(AuxIUnknown<_Ty2,_Base,_Ax> &);

			enum {RET = sizeof(JudgeMethod(*(_Ty *)0))!=sizeof(char)};
		};

		struct _GlobalAllocator
		{
			static void * allocate(size_t size)
			{	// allocate array of _Count elements
				return (void *)::operator new(size);
			}

			static void deallocate(void * _Ptr)
			{	// deallocate object at _Ptr, ignore size
				::operator delete(_Ptr);
			}

			template<typename _Ty>
			static void construct(_Ty * _Ptr)
			{	// construct object at _Ptr
				VFX::__ConstructElements(_Ptr);
			}

			template<typename _Ty>
			static void destroy(_Ty * _Ptr)
			{	// destroy object at _Ptr
				VFX::__DestructElements(_Ptr);
			}
		};

		template<bool,typename _Ty>
		struct SelectAllocator
		{
			typedef typename _Ty::alloc_type alloc_type;
		};

		template<typename _Ty>
		struct SelectAllocator<false,_Ty>
		{
			typedef _GlobalAllocator alloc_type;
		};

		template<typename _Ty>
		struct extraction_allocator
		{
			enum{is_aux=JudgeAuxIUnknown<_Ty>::RET};
			typedef SelectAllocator<is_aux,_Ty> select_type;
			typedef typename select_type::alloc_type alloc_type;
		};
}
}

#pragma push_macro("new")
#undef new

#endif	//end VFX_REF_USE_ALLOC

#ifndef VFX_REF_DEFINED
#define VFX_REF_DEFINED

	template <class T>
	struct _vfxNoAddRefRelease : public T
	{
	private:
		virtual LONG AddRef() = 0;
		virtual LONG Release() = 0;
	};

	namespace VFX
	{
		namespace detail
		{
			template<typename _Ty,bool _AutoCreate>
			struct __AutoCreateReference
			{
				static _Ty * init(){ return ( _Ty *)NULL;}
			};
			template<typename _Ty>
			struct __AutoCreateReference<_Ty,true>
			{
				static _Ty * init()
				{
#if VFX_REF_USE_ALLOC
					_Ty	* _ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
					alloc_type::construct(_ref);
#else
					_Ty * _ref = VNEW _Ty;
#endif	//end VFX_REF_USE_ALLOC
					return _ref;
				}
			};
		}
	}

	template<typename _Ty,bool _AutoCreate=false>
	struct REF
	{
		typedef _Ty			element_type;
		typedef _Ty *		_Pty;
		typedef REF<_Ty>	this_type;
#if VFX_REF_USE_ALLOC
		typedef typename ::VFX::detail::extraction_allocator<_Ty>::alloc_type alloc_type;
#endif //end VFX_REF_USE_ALLOC
	private:
		_Pty _ref;
	public:
		REF():_ref(VFX::detail::__AutoCreateReference<_Ty,_AutoCreate>::init())
		{
		}
		~REF(){
			if(_ref) _ref->Release();
		}

		explicit REF(_Pty __r):_ref(__r){}
/*
		template<typename _From>
		explicit REF(const REF<_From> & _ptr){
			if(FALSE == _ptr.QueryInterface(_Ty::__UID__,(void **)&_ref))
				_ref = NULL;
		}
*/
		REF(const this_type & __r):_ref(__r._ref){
			if(_ref) _ref->AddRef();
		}
#if __CPL_FULL_VER >= 1600
		REF(this_type && __r):_ref(__r._ref){
			__r._ref = NULL;
		}
#endif
/*
		template<typename _From>
		this_type & operator = (const REF<_From> & _ptr)
		{
			if(_ref) _ref->Release();
			if(FALSE == _ptr.QueryInterface(&_ref))
				_ref = NULL;
			return *this;
		}
*/
		this_type & WeakRef(_Pty __r)
		{
			if(__r != this->_ref)
			{
				if(_ref) _ref->Release();
				_ref = __r;
			}
			return *this;
		}

		this_type & StrongRef(_Pty __r)
		{
			if(__r != this->_ref)
			{
				if(__r) __r->AddRef();
				if(_ref) _ref->Release();
				_ref = __r;
			}
			return *this;
		}

		/*this_type & operator = (_Pty __r)
		{
			if(__r != this->_ref)
			{
				if(__r) __r->AddRef();
				if(_ref) _ref->Release();
				_ref = __r;
			}
			return *this;
		}*/
		this_type & operator = (const this_type & __r)
		{
			if(_ref != __r._ref)
			{
				if(__r._ref) __r._ref->AddRef();
				if(_ref) _ref->Release();
				_ref = __r._ref;
			}
			return *this;
		}
#if __CPL_FULL_VER >= 1600
		this_type & operator = (this_type && __r)
		{
			if(_ref != __r._ref)
			{
				if(_ref) _ref->Release();
				_ref = __r._ref;
				__r._ref = NULL;
			}
			return *this;
		}
#endif

		bool operator ! () const
		{
			return (_ref == NULL);
		}
		bool operator < (const _Pty pT) const
		{
			return _ref < pT;
		}
		bool operator != (const _Pty pT) const
		{
			return _ref != pT;
		}
		bool operator == (const _Pty pT) const
		{
			return _ref == pT;
		}

		operator _Pty () const{
			return _ref;
		}
		_vfxNoAddRefRelease<_Ty> * operator -> () const{
			return (_vfxNoAddRefRelease<_Ty> *)_ref;
		}
/*
		_Pty * operator & (){
			return &_ref;
		}
		const _Pty * operator & () const{
			return &_ref;
		}
*/		_Pty ptr() const{
			return _ref;
		}
		_Pty CreateInstance()
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			alloc_type::construct(_ref);
#else
			_ref = VNEW _Ty;
#endif	//end VFX_REF_USE_ALLOC

			return _ref;
		}
		
		template<typename Arg1>
		_Pty CreateInstance(Arg1 a1)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1);
#else
			_ref = VNEW _Ty(a1);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}
		
		template<typename Arg1,typename Arg2>
		_Pty CreateInstance(Arg1 a1,Arg2 a2)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1,a2);
#else
			_ref = VNEW _Ty(a1,a2);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}
		
		template<typename Arg1,typename Arg2,typename Arg3>
		_Pty CreateInstance(Arg1 a1,Arg2 a2,Arg3 a3)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1,a2,a3);
#else
			_ref = VNEW _Ty(a1,a2,a3);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}
		
		template<typename Arg1,typename Arg2,typename Arg3,typename Arg4>
		_Pty CreateInstance(Arg1 a1,Arg2 a2,Arg3 a3,Arg4 a4)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1,a2,a3,a4);
#else
			_ref = VNEW _Ty(a1,a2,a3,a4);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}
		
		template<typename Arg1,typename Arg2,typename Arg3,typename Arg4,typename Arg5>
		_Pty CreateInstance(Arg1 a1,Arg2 a2,Arg3 a3,Arg4 a4,Arg5 a5)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1,a2,a3,a4,a5);
#else
			_ref = VNEW _Ty(a1,a2,a3,a4,a5);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}

		vBOOL QueryInterface(vIID iid, LPVOID * _pptr) const
		{
			if(_ref) return _ref->QueryInterface(iid,_pptr);
			*_pptr = NULL;
			return FALSE;
		}

		template <class QTy>
		vBOOL QueryInterface(QTy ** _pptr) const
		{
			if(_pptr == NULL) return FALSE;
			if(_ref) return _ref->QueryInterface(QTy::__UID__,(void **)_pptr);
			*_pptr = NULL;
			return FALSE;
		}

		template<typename QTy>
		vBOOL FromInterface(QTy & _ptr)
		{
			_Pty _temp = _ref;
			if(FALSE == _ptr.QueryInterface(_Ty::__UID__,(void **)&_ref))
				_ref = NULL;
			if(_temp) _temp->Release();
			return _ref != NULL;
		}
		
		template<typename QTy>
		vBOOL FromInterface(QTy * _ptr)
		{
			_Pty _temp = _ref;
			if(_ptr == NULL || FALSE == _ptr->QueryInterface(_Ty::__UID__,(void **)&_ref))
				_ref = NULL;
			if(_temp) _temp->Release();
			return _ref != NULL;
		}

		// Detach the interface (does not Release)
		_Pty Detach()
		{
			_Pty pt = _ref;
			_ref = NULL;
			return pt;
		}

		void swap(this_type & __r)
		{
			std::swap(_ref,__r._ref);
		}
	};

#if defined(__cplusplus_cli)

	template<typename _Ty>
	ref struct RREF
	{
		typedef _Ty			element_type;
		typedef _Ty *		_Pty;
		typedef RREF<_Ty>	this_type;
#if VFX_REF_USE_ALLOC
		typedef typename ::VFX::detail::extraction_allocator<_Ty>::alloc_type alloc_type;
#endif //end VFX_REF_USE_ALLOC
	private:
		_Pty _ref;
	public:
		RREF():_ref(NULL){}
		explicit RREF(_Pty __r):_ref(__r){}
/*
		template<typename _From>
		explicit RREF(const RREF<_From> % _ptr){
			if(FALSE == _ptr.QueryInterface(_Ty::__UID__,(void **)&_ref))
				_ref = NULL;
		}
*/
		RREF(this_type % __r):_ref(__r._ref){
			if(_ref) _ref->AddRef();
		}
		~RREF(){
			if(_ref){_ref->Release();_ref=NULL;}
		}

		!RREF(){
			if(_ref){_ref->Release();_ref=NULL;}
		}
/*
		template<typename _From>
		this_type% operator = (RREF<_From> & _ptr)
		{
			if(_ref) _ref->Release();
			if(FALSE == _ptr.QueryInterface(&_ref))
				_ref = NULL;
			return *this;
		}
*/
		_Pty operator = (_Pty __r)
		{
			if(__r != _ref)
			{
				if(__r) __r->AddRef();
				if(_ref) _ref->Release();
				_ref = __r;
			}
			return __r;
		}
		this_type^ operator = (this_type % __r)
		{
			if(_ref != __r._ref)
			{
				if(__r._ref) __r._ref->AddRef();
				if(_ref) _ref->Release();
				_ref = __r._ref;
			}
			return this;
		}

		bool operator ! ()
		{
			return (_ref == NULL);
		}
		bool operator < (const _Pty pT)
		{
			return _ref < pT;
		}
		bool operator != (const _Pty pT)
		{
			return _ref != pT;
		}
		bool operator == (const _Pty pT)
		{
			return _ref == pT;
		}

		operator _Pty (){
			return _ref;
		}
		_vfxNoAddRefRelease<_Ty> * operator -> (){
			return (_vfxNoAddRefRelease<_Ty> *)_ref;
		}
/*
		_Pty * operator & (){
			return &_ref;
		}
		_Pty ptr(){
			return _ref;
		}
*/
		_Pty CreateInstance()
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			alloc_type::construct(_ref);
#else
			_ref = VNEW _Ty;
#endif	//end VFX_REF_USE_ALLOC

			return _ref;
		}

		template<typename Arg1>
		_Pty CreateInstance(Arg1 a1)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1);
#else
			_ref = VNEW _Ty(a1);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}

		template<typename Arg1,typename Arg2>
		_Pty CreateInstance(const Arg1 & a1,const Arg2 & a2)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1,a2);
#else
			_ref = VNEW _Ty(a1,a2);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}

		template<typename Arg1,typename Arg2,typename Arg3>
		_Pty CreateInstance(const Arg1 & a1,const Arg2 & a2,const Arg3 & a3)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1,a2,a3);
#else
			_ref = VNEW _Ty(a1,a2,a3);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}

		template<typename Arg1,typename Arg2,typename Arg3,typename Arg4>
		_Pty CreateInstance(const Arg1 & a1,const Arg2 & a2,const Arg3 & a3,const Arg4 & a4)
		{
			if(_ref) _ref->Release();

#if VFX_REF_USE_ALLOC
			_ref = (_Ty *)alloc_type::allocate(sizeof(_Ty));
			new(_ref) _Ty(a1,a2,a3,a4);
#else
			_ref = VNEW _Ty(a1,a2,a3,a4);
#endif //end VFX_REF_USE_ALLOC

			return _ref;
		}

		vBOOL QueryInterface(vIID iid, LPVOID * pvData)
		{
			if(_ref) return _ref->QueryInterface(iid,pvData);
			pvData = NULL;
			return FALSE;
		}

		template <class QTy>
		vBOOL QueryInterface(QTy ** _pptr)
		{
			if(_pptr == NULL)
				return FALSE;
			if(_ref) return _ref->QueryInterface(QTy::__UID__,(void **)_pptr);
			*_pptr = NULL;
			return FALSE;
		}

		template<typename QTy>
		vBOOL FromInterface(QTy % _ptr)
		{
			if(_ref) _ref->Release();
			_Pty _temp;
			if(FALSE == _ptr.QueryInterface(_Ty::__UID__,(void **)&_temp))
				_ref = NULL;
			else
				_ref = _temp;
			return _ref != NULL;
		}

		template<typename QTy>
		vBOOL FromInterface(QTy * _ptr)
		{
			if(_ref) _ref->Release();
			if(_ptr == NULL)
				_ref = NULL;
			else
			{
				_Pty _temp;
				if(FALSE == _ptr->QueryInterface(_Ty::__UID__,(void **)&_temp))
					_ref = NULL;
				else
					_ref = _temp;
			}
			return _ref != NULL;
		}
	};

#endif	//end __cplusplus_cli

#endif	//end VFX_REF_DEFINED

#if VFX_REF_USE_ALLOC
#pragma pop_macro("new")
#endif	//end VFX_REF_USE_ALLOC

#if 0
void Test()
{
	v3d::REF<VFile2Memory> f2m01(new VFile2Memory);
	v3d::REF<VFile2Memory> f2m02;
	f2m02 = new VFile2Memory;
	v3d::REF<VFile2Memory> f2m03;
	f2m03 = f2m01;
	v3d::REF<VFile2Memory> f2m04(f2m01);
}
#endif
