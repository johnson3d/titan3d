#pragma once

#define TypeDefault(type) VGetTypeDefault<type>();

#define CSharpAPI0(NSName, ClassName, MethodName) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self) \
	{\
		if(self==nullptr) return;\
		self->MethodName();\
	};
#define CSharpReturnAPI0(TResult, NSName, ClassName, MethodName) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName();\
	};

#define CSharpAPI1(NSName, ClassName, MethodName, Arg0) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, Arg0 p0) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0);\
	};
#define CSharpReturnAPI1(TResult, NSName, ClassName, MethodName, Arg0) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, Arg0 p0) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0);\
	};

#define CSharpAPI2(NSName, ClassName, MethodName, Arg0, Arg1) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0, p1);\
	};
#define CSharpReturnAPI2(TResult, NSName, ClassName, MethodName, Arg0, Arg1) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1);\
	};

#define CSharpAPI3(NSName, ClassName, MethodName, Arg0, Arg1, Arg2) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0, p1, p2);\
	};
#define CSharpReturnAPI3(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2);\
	};

#define CSharpAPI4(NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0, p1, p2, p3);\
	};
#define CSharpReturnAPI4(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3);\
	};

#define CSharpAPI5(NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0, p1, p2, p3, p4);\
	};
#define CSharpReturnAPI5(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4);\
	};

#define CSharpAPI6(NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0, p1, p2, p3, p4, p5);\
	};
#define CSharpReturnAPI6(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4, p5);\
	};

#define CSharpAPI7(NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5, Arg6) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5, Arg6 p6) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0, p1, p2, p3, p4, p5, p6);\
	};
#define CSharpReturnAPI7(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5, Arg6) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5, Arg6 p6) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4, p5, p6);\
	};

#define CSharpAPI8(NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7) VFX_API void SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5, Arg6 p6, Arg7 p7) \
	{\
		if(self==nullptr) return;\
		self->MethodName(p0, p1, p2, p3, p4, p5, p6, p7);\
	};
#define CSharpReturnAPI8(TResult, NSName, ClassName, MethodName, Arg0, Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7) VFX_API TResult SDK_##ClassName##_##MethodName(NSName::ClassName* self, \
			Arg0 p0, Arg1 p1, Arg2 p2, Arg3 p3, Arg4 p4, Arg5 p5, Arg6 p6, Arg7 p7) \
	{\
		if(self==nullptr) return TypeDefault(TResult);\
		return self->MethodName(p0, p1, p2, p3, p4, p5, p6, p7);\
	};