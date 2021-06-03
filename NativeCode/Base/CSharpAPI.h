#pragma once

#define TypeDefault(type) VGetTypeDefault<type>();

template <typename SrcType>
struct Type2TypeConverter
{
	typedef SrcType			TarType;
};

#define VCombine2(S,A0,A1) A0##S##A1
#define VCombine3(S,A0,A1,A2) A0##S##A1##S##A2
#define VCombine4(S,A0,A1,A2,A3) A0##S##A1##S##A2##S##A3
#define VCombineClassName(NSName, ClassName) NSName::ClassName
#define VCombineMethodName(NSName, ClassName, MethodName) NSName::ClassName::MethodName
#define VCombineHelperName(NSName, ClassName, MethodName) NSName##_##ClassName##_##MethodName##_##Helper

#define Cpp2CS0(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName,ClassName,MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self) \
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName();\
	};

#define Cpp2CS1(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0) \
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0);\
	};

#define Cpp2CS2(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
		using T1 = TFunction_traits<TFunctionType>::param_type<1>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0, \
			VCombineHelperName(NSName, ClassName, MethodName)::T1 a1) \
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0,a1);\
	};

#define Cpp2CS3(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
		using T1 = TFunction_traits<TFunctionType>::param_type<1>;\
		using T2 = TFunction_traits<TFunctionType>::param_type<2>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0, \
			VCombineHelperName(NSName, ClassName, MethodName)::T1 a1, \
			VCombineHelperName(NSName, ClassName, MethodName)::T2 a2) \
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0,a1,a2);\
	};

#define Cpp2CS4(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
		using T1 = TFunction_traits<TFunctionType>::param_type<1>;\
		using T2 = TFunction_traits<TFunctionType>::param_type<2>;\
		using T3 = TFunction_traits<TFunctionType>::param_type<3>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0, \
			VCombineHelperName(NSName, ClassName, MethodName)::T1 a1, \
			VCombineHelperName(NSName, ClassName, MethodName)::T2 a2, \
			VCombineHelperName(NSName, ClassName, MethodName)::T3 a3) \
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0,a1,a2,a3);\
	};

#define Cpp2CS5(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
		using T1 = TFunction_traits<TFunctionType>::param_type<1>;\
		using T2 = TFunction_traits<TFunctionType>::param_type<2>;\
		using T3 = TFunction_traits<TFunctionType>::param_type<3>;\
		using T4 = TFunction_traits<TFunctionType>::param_type<4>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0,\
			VCombineHelperName(NSName, ClassName, MethodName)::T1 a1,\
			VCombineHelperName(NSName, ClassName, MethodName)::T2 a2,\
			VCombineHelperName(NSName, ClassName, MethodName)::T3 a3,\
			VCombineHelperName(NSName, ClassName, MethodName)::T4 a4)\
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0,a1,a2,a3,a4);\
	};

#define Cpp2CS6(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
		using T1 = TFunction_traits<TFunctionType>::param_type<1>;\
		using T2 = TFunction_traits<TFunctionType>::param_type<2>;\
		using T3 = TFunction_traits<TFunctionType>::param_type<3>;\
		using T4 = TFunction_traits<TFunctionType>::param_type<4>;\
		using T5 = TFunction_traits<TFunctionType>::param_type<5>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0,\
			VCombineHelperName(NSName, ClassName, MethodName)::T1 a1,\
			VCombineHelperName(NSName, ClassName, MethodName)::T2 a2,\
			VCombineHelperName(NSName, ClassName, MethodName)::T3 a3,\
			VCombineHelperName(NSName, ClassName, MethodName)::T4 a4,\
			VCombineHelperName(NSName, ClassName, MethodName)::T5 a5)\
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0,a1,a2,a3,a4,a5);\
	};

#define Cpp2CS7(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
		using T1 = TFunction_traits<TFunctionType>::param_type<1>;\
		using T2 = TFunction_traits<TFunctionType>::param_type<2>;\
		using T3 = TFunction_traits<TFunctionType>::param_type<3>;\
		using T4 = TFunction_traits<TFunctionType>::param_type<4>;\
		using T5 = TFunction_traits<TFunctionType>::param_type<5>;\
		using T6 = TFunction_traits<TFunctionType>::param_type<6>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0,\
			VCombineHelperName(NSName, ClassName, MethodName)::T1 a1,\
			VCombineHelperName(NSName, ClassName, MethodName)::T2 a2,\
			VCombineHelperName(NSName, ClassName, MethodName)::T3 a3,\
			VCombineHelperName(NSName, ClassName, MethodName)::T4 a4,\
			VCombineHelperName(NSName, ClassName, MethodName)::T5 a5,\
			VCombineHelperName(NSName, ClassName, MethodName)::T6 a6)\
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0,a1,a2,a3,a4,a5,a6);\
	};

#define Cpp2CS8(NSName, ClassName, MethodName) \
	struct VCombineHelperName(NSName, ClassName, MethodName)\
	{\
		using TFunctionType = decltype(&VCombineMethodName(NSName, ClassName, MethodName));\
		using TResult = Type2TypeConverter<TFunction_traits<TFunctionType>::return_type>::TarType;\
		using T0 = TFunction_traits<TFunctionType>::param_type<0>;\
		using T1 = TFunction_traits<TFunctionType>::param_type<1>;\
		using T2 = TFunction_traits<TFunctionType>::param_type<2>;\
		using T3 = TFunction_traits<TFunctionType>::param_type<3>;\
		using T4 = TFunction_traits<TFunctionType>::param_type<4>;\
		using T5 = TFunction_traits<TFunctionType>::param_type<5>;\
		using T6 = TFunction_traits<TFunctionType>::param_type<6>;\
		using T7 = TFunction_traits<TFunctionType>::param_type<7>;\
	};\
	VFX_API VCombineHelperName(NSName, ClassName, MethodName)::TResult SDK_##ClassName##_##MethodName(VCombineClassName(NSName,ClassName)* self, \
			VCombineHelperName(NSName, ClassName, MethodName)::T0 a0,\
			VCombineHelperName(NSName, ClassName, MethodName)::T1 a1,\
			VCombineHelperName(NSName, ClassName, MethodName)::T2 a2,\
			VCombineHelperName(NSName, ClassName, MethodName)::T3 a3,\
			VCombineHelperName(NSName, ClassName, MethodName)::T4 a4,\
			VCombineHelperName(NSName, ClassName, MethodName)::T5 a5,\
			VCombineHelperName(NSName, ClassName, MethodName)::T6 a6,\
			VCombineHelperName(NSName, ClassName, MethodName)::T7 a7)\
	{\
		if(self==nullptr) {\
			return TypeDefault(VCombineHelperName(NSName, ClassName, MethodName)::TResult); \
		}\
		return self->MethodName(a0,a1,a2,a3,a4,a5,a6,a7);\
	};

#define CSharpReturnAPI0(TResult, NSName, ClassName, MethodName) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName();\
	};

#define CSharpReturnAPI1(TResult, NSName, ClassName, MethodName, Arg0) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, Arg0 p0) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0);\
	};

#define CSharpReturnAPI2(TResult, NSName, ClassName, MethodName, Arg0, Arg1) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1);\
	};

#define CSharpReturnAPI3(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2);\
	};

#define CSharpReturnAPI4(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3);\
	};

#define CSharpReturnAPI5(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4);\
	};

#define CSharpReturnAPI6(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4, p5);\
	};

#define CSharpReturnAPI7(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5, Arg6) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5, Arg6 p6) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4, p5, p6);\
	};

#define CSharpReturnAPI8(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5, Arg6 p6, Arg7 p7) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4, p5, p6, p7);\
	};
