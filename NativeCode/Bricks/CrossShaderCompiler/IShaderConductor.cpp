#include "IShaderConductor.h"
#include "../../NextRHI/NxRHI.h"
#include "../../Base/io/vfxfile.h"
#include "../../Base/io/MemStream.h"
#include "../../Base/CoreSDK.h"
//#include "../../../3rd/native/ShaderConductor/Include/ShaderConductor/SpirvPostProcess.hpp"

#if defined(PLATFORM_WIN)
#include "../../NextRHI/Dx11/DX11PreHead.h"
#include "../../NextRHI/Dx12/DX12Shader.h"
#include "../../NextRHI/Vulkan/VKShader.h"
#pragma comment(lib, "ShaderConductor.lib")
#elif defined(PLATFORM_MAC)
// Mac 走进程内 dxc（libdxcompiler.dylib 运行时 dlopen）。
// 不 include dxcapi.h/WinAdapter.h：其 Win 基础 typedef（LONG/HRESULT 等）与引擎 vfxtypes 冲突，
// 按官方头手工声明用到的 COM 子集，虚函数顺序即 vtable 布局，与 dxc 二进制一致
#include <dlfcn.h>
#include <mach-o/dyld.h>
#include <limits.h>
#include <sstream>

#ifndef STDMETHODCALLTYPE
#define STDMETHODCALLTYPE
#endif
#ifndef E_POINTER
#define E_POINTER ((int)0x80004003)
#endif
#ifndef E_NOINTERFACE
#define E_NOINTERFACE ((int)0x80004002)
#endif
#include "../../NextRHI/Vulkan/VKShader.h"
#endif

#define  new VNEW

using namespace EngineNS;
using namespace EngineNS::NxRHI;

#if defined(PLATFORM_WIN)

struct UEngineInclude : public ID3DInclude
{
	NxRHI::FShaderCompiler* ShaderConductor;
	std::map<LPCVOID, std::string> Data2FileMap;
	virtual COM_DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE Open(D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes)
	{
		std::string parent;
		auto iter = Data2FileMap.find(pParentData);
		if (iter != Data2FileMap.end())
		{
			parent = iter->second;
		}

		std::string root = parent + pFileName;
		if (strlen(pFileName) > 2 && pFileName[1] == ':')
		{
			//todo: not windows platform
			root = pFileName;
		}
		auto ar = ShaderConductor->GetShaderCodeStream(root.c_str(), pFileName);
		if (ar == nullptr)
		{
			VFX_LTRACE(ELTT_Graphics, "Include %s failed", root.c_str());
			*ppData = nullptr;
			*pBytes = 0;
			return S_OK;
		}

		*ppData = ar->GetSourceCode();
		*pBytes = ar->GetSize();

		auto dir = GetPath(root);
		Data2FileMap.insert(std::make_pair(*ppData, dir));

		return S_OK;
	}
	virtual COM_DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE Close(LPCVOID pData)
	{
		return S_OK;
	}
	static std::string GetPath(std::string file)
	{
		for (size_t i = 0; i < file.size(); i++)
		{
			if (file[i] == '\\')
				file[i] = '/';
		}
		size_t pos = std::string::npos;
		for (int i = (int)file.size() - 1; i >= 0; i--)
		{
			if (file[i] == '/')
			{
				pos = i;
				break;
			}
		}
		std::string result = file.substr(0, pos + 1);
		return result;
	}
};
#endif

#if defined(PLATFORM_MAC)

struct TDxcGUID { uint32_t Data1; uint16_t Data2; uint16_t Data3; uint8_t Data4[8]; };
typedef TDxcGUID TDxcIID;
typedef const TDxcGUID& TDxcRefIID;

//dxcapi.h 的 COM 子集（GUID 值与虚函数序照抄官方头）
struct TDxcUnknown
{
	virtual int STDMETHODCALLTYPE QueryInterface(TDxcRefIID riid, void** ppv) = 0;
	virtual uint32_t STDMETHODCALLTYPE AddRef() = 0;
	virtual uint32_t STDMETHODCALLTYPE Release() = 0;
};
struct TDxcBlob : public TDxcUnknown
{
	virtual void* STDMETHODCALLTYPE GetBufferPointer() = 0;
	virtual size_t STDMETHODCALLTYPE GetBufferSize() = 0;
};
struct TDxcBlobEncoding : public TDxcBlob
{
	virtual int STDMETHODCALLTYPE GetEncoding(int* pKnown, uint32_t* pCodePage) = 0;
};
struct TDxcBlobUtf8 : public TDxcBlobEncoding
{
	virtual const char* STDMETHODCALLTYPE GetStringPointer() = 0;
	virtual size_t STDMETHODCALLTYPE GetStringLength() = 0;
};
struct TDxcBlobWide : public TDxcBlobEncoding
{
	virtual const wchar_t* STDMETHODCALLTYPE GetStringPointer() = 0;
	virtual size_t STDMETHODCALLTYPE GetStringLength() = 0;
};
struct TDxcIncludeHandler : public TDxcUnknown
{
	virtual int STDMETHODCALLTYPE LoadSource(const wchar_t* pFilename, TDxcBlob** ppIncludeSource) = 0;
};
struct TDxcOperationResult : public TDxcUnknown
{
	virtual int STDMETHODCALLTYPE GetStatus(int* pStatus) = 0;
	virtual int STDMETHODCALLTYPE GetResult(TDxcBlob** ppResult) = 0;
	virtual int STDMETHODCALLTYPE GetErrorBuffer(TDxcBlobEncoding** ppErrors) = 0;
};
enum EDxcOutKind
{
	TDXC_OUT_NONE = 0,
	TDXC_OUT_OBJECT = 1,
	TDXC_OUT_ERRORS = 2,
};
struct TDxcResult : public TDxcOperationResult
{
	virtual int STDMETHODCALLTYPE HasOutput(int dxcOutKind) = 0;
	virtual int STDMETHODCALLTYPE GetOutput(int dxcOutKind, TDxcRefIID iid, void** ppvObject, TDxcBlobWide** ppOutputName) = 0;
	virtual uint32_t GetNumOutputs() = 0;
	virtual int GetOutputByIndex(uint32_t Index) = 0;
	virtual int PrimaryOutput() = 0;
};
struct TDxcBuffer { const void* Ptr; size_t Size; uint32_t Encoding; };
struct TDxcCompiler3 : public TDxcUnknown
{
	virtual int STDMETHODCALLTYPE Compile(const TDxcBuffer* pSource, const wchar_t* const* pArguments, uint32_t argCount, TDxcIncludeHandler* pIncludeHandler, TDxcRefIID riid, void** ppResult) = 0;
	virtual int STDMETHODCALLTYPE Disassemble(const TDxcBuffer* pObject, TDxcRefIID riid, void** ppResult) = 0;
};
typedef int (*TDxcCreateInstanceProc)(const TDxcGUID& rclsid, const TDxcIID& riid, void** ppv);

static const TDxcGUID TDXC_IID_Unknown = { 0x00000000, 0x0000, 0x0000, { 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 } };
static const TDxcGUID TDXC_IID_Blob = { 0x8BA5FB08, 0x5195, 0x40e2, { 0xAC, 0x58, 0x0D, 0x98, 0x9C, 0x3A, 0x01, 0x02 } };
static const TDxcGUID TDXC_IID_BlobUtf8 = { 0x3DA636C9, 0xBA71, 0x4024, { 0xA3, 0x01, 0x30, 0xCB, 0xF1, 0x25, 0x30, 0x5B } };
static const TDxcGUID TDXC_IID_Result = { 0x58346CDA, 0xDDE7, 0x4497, { 0x94, 0x61, 0x6F, 0x87, 0xAF, 0x5E, 0x06, 0x59 } };
static const TDxcGUID TDXC_IID_IncludeHandler = { 0x7f61fc7d, 0x950d, 0x467f, { 0xb3, 0xe3, 0x3c, 0x02, 0xfb, 0x49, 0x18, 0x7c } };
static const TDxcGUID TDXC_IID_Compiler3 = { 0x228B4687, 0x5A6A, 0x4730, { 0x90, 0x0C, 0x97, 0x02, 0xB2, 0x20, 0x3F, 0x54 } };
static const TDxcGUID TDXC_CLSID_Compiler = { 0x73e22d93, 0xe6ce, 0x47f3, { 0xb5, 0xbf, 0xf0, 0x66, 0x4f, 0x39, 0xc1, 0xb0 } };

static const uint32_t TDXC_CP_UTF8 = 65001;

static std::wstring Utf8ToWideStr(const char* s)
{
	std::wstring result;
	if (s == nullptr)
		return result;
	mbstate_t state{};
	const char* src = s;
	size_t need = mbsrtowcs(nullptr, &src, 0, &state);
	if (need == (size_t)-1)
	{
		//locale 非 UTF8 时的 ASCII 快路径
		result.reserve(strlen(s));
		for (const char* p = s; *p; ++p)
			result.push_back((wchar_t)(unsigned char)*p);
		return result;
	}
	result.resize(need);
	src = s;
	mbsrtowcs(&result[0], &src, need, &state);
	return result;
}
static std::string WideStrToUtf8(const wchar_t* w)
{
	std::string result;
	if (w == nullptr)
		return result;
	mbstate_t state{};
	const wchar_t* src = w;
	size_t need = wcsrtombs(nullptr, &src, 0, &state);
	if (need == (size_t)-1)
	{
		for (const wchar_t* p = w; *p; ++p)
			result.push_back((char)(unsigned char)*p);
		return result;
	}
	result.resize(need);
	src = w;
	wcsrtombs(&result[0], &src, need, &state);
	return result;
}

struct UDxcMemBlob : public TDxcBlob
{
	//Storage 必须声明在 Ptr 之前：string 版构造用 Storage.data() 初始化 Ptr，初始化顺序随声明序
	std::string Storage;
	const void* Ptr;
	size_t      Size;
	uint32_t    RefCount = 1;
	UDxcMemBlob(const void* p, size_t s) : Ptr(p), Size(s) {}
	UDxcMemBlob(std::string&& code) : Storage(std::move(code)), Ptr(Storage.data()), Size(Storage.size()) {}
	int STDMETHODCALLTYPE QueryInterface(TDxcRefIID riid, void** ppv) override
	{
		if (ppv == nullptr)
			return E_POINTER;
		if (memcmp(&riid, &TDXC_IID_Unknown, sizeof(TDxcGUID)) == 0 ||
			memcmp(&riid, &TDXC_IID_Blob, sizeof(TDxcGUID)) == 0)
		{
			*ppv = (TDxcBlob*)this;
			AddRef();
			return S_OK;
		}
		*ppv = nullptr;
		return E_NOINTERFACE;
	}
	uint32_t STDMETHODCALLTYPE AddRef() override { return ++RefCount; }
	uint32_t STDMETHODCALLTYPE Release() override { uint32_t r = --RefCount; if (r == 0) delete this; return r; }
	void* STDMETHODCALLTYPE GetBufferPointer() override { return (void*)Ptr; }
	size_t STDMETHODCALLTYPE GetBufferSize() override { return Size; }
};

//对齐 Windows UEngineInclude：'@Engine/...' 交托管展开，相对 include 由 clang 拼接（主文件名走位置参数）
// Mac dxc：DX_AUTOBIND 资源无显式 register，dxc 自动分配的 SPIRV binding 连续编号
// 且 -fvk-*-shift 对自动分配不生效，跨 stage 撞 binding（VS b0 与 PS t0 同为 0）。
// 这里按 DX 寄存器规则显式注入 register，分段与下方 -fvk-shift 对应：
// SRV->t、Sampler->s、UAV(RW/Append/Consume/RasterizerOrdered)->u、cbuffer/ConstantBuffer->b
// 统计一行里显式 register(bN)/DX_BIND_X(N) 占用的最大号（跳过宏定义行，auto 分配避开这些号）
static void ScanExplicitBindRegister(const std::string& line, int autoBindCounters[4])
{
	for (size_t p = line.find("register("); p != std::string::npos; p = line.find("register(", p + 1))
	{
		size_t c = p + strlen("register(");
		int slot = -1;
		if (line[c] == 't') slot = 0;
		else if (line[c] == 's') slot = 1;
		else if (line[c] == 'u') slot = 2;
		else if (line[c] == 'b') slot = 3;
		if (slot < 0)
			continue;
		int n = atoi(line.c_str() + c + 1);
		if (n + 1 > autoBindCounters[slot])
			autoBindCounters[slot] = n + 1;
	}
	for (size_t p = line.find("DX_BIND_"); p != std::string::npos; p = line.find("DX_BIND_", p + 1))
	{
		size_t c = p + strlen("DX_BIND_");
		int slot = -1;
		if (line[c] == 'T') slot = 0;
		else if (line[c] == 'S') slot = 1;
		else if (line[c] == 'U') slot = 2;
		else if (line[c] == 'B') slot = 3;
		if (slot < 0)
			continue;
		int n = atoi(line.c_str() + c + 2);
		if (n + 1 > autoBindCounters[slot])
			autoBindCounters[slot] = n + 1;
	}
}

//pass1：行级统计 code 里显式 register(xN)/DX_BIND_X(N) 的占号（跳过 # 预处理行防宏定义行污染）
static void ScanCodeExplicitBinds(const std::string& code, int autoBindCounters[4])
{
	size_t scanPos = 0;
	while (scanPos < code.size())
	{
		size_t lineEnd = code.find('\n', scanPos);
		if (lineEnd == std::string::npos)
			lineEnd = code.size();
		std::string line = code.substr(scanPos, lineEnd - scanPos);
		size_t firstNonWs = line.find_first_not_of(" \t");
		if (firstNonWs != std::string::npos && line[firstNonWs] != '#')
			ScanExplicitBindRegister(line, autoBindCounters);
		scanPos = lineEnd + 1;
	}
}

static void RewriteDxcAutoBindRegisters(std::string& code, int autoBindCounters[4])
{
	if (code.find("DX_AUTOBIND") == std::string::npos)
		return;
	//pass1: auto 分配必须避开同编译单元里显式 register(bN)/DX_BIND_X(N) 已占的号，否则撞号
	ScanCodeExplicitBinds(code, autoBindCounters);
	std::string out;
	out.reserve(code.size() + 128);
	size_t pos = 0;
	while (pos < code.size())
	{
		size_t lineEnd = code.find('\n', pos);
		if (lineEnd == std::string::npos)
			lineEnd = code.size();
		std::string line = code.substr(pos, lineEnd - pos);
		size_t firstNonWs = line.find_first_not_of(" \t");
		bool isPreprocessor = (firstNonWs != std::string::npos && line[firstNonWs] == '#');
		size_t ab = line.find("DX_AUTOBIND");
		if (ab != std::string::npos && isPreprocessor == false && line.find("//") > ab)
		{
			size_t i = 0;
			while (i < line.size() && (line[i] == ' ' || line[i] == '\t'))
				i++;
			size_t j = i;
			while (j < line.size() && line[j] != ' ' && line[j] != '\t')
				j++;
			std::string typeTok = line.substr(i, j - i);
			int slot = 0;
			if (typeTok.compare(0, 7, "Sampler") == 0 || typeTok.compare(0, 7, "sampler") == 0)
				slot = 1;
			else if (typeTok.compare(0, 2, "RW") == 0 ||
				typeTok.compare(0, 17, "RasterizerOrdered") == 0 ||
				typeTok.compare(0, 22, "AppendStructuredBuffer") == 0 ||
				typeTok.compare(0, 23, "ConsumeStructuredBuffer") == 0)
				slot = 2;
			else if (typeTok.compare(0, 7, "cbuffer") == 0 || typeTok.compare(0, 14, "ConstantBuffer") == 0)
				slot = 3;
			char reg[40];
			snprintf(reg, sizeof(reg), ": register(%c%d)", "tsub"[slot], autoBindCounters[slot]++);
			line.replace(ab, strlen("DX_AUTOBIND"), reg);
		}
		out.append(line);
		if (lineEnd < code.size())
			out.push_back('\n');
		pos = lineEnd + 1;
	}
	code.swap(out);
}

struct UEngineDxcInclude : public TDxcIncludeHandler
{
	NxRHI::FShaderCompiler* Compiler = nullptr;
	std::string MainName;
	std::string MainCode;
	std::string MainDir;//主文件目录（含尾'/'），clang 未拼接时的兜底
	int AutoBindCounters[4] = { 0,0,0,0 };//t/s/u/b，跨主文件与 include 共享，按编译器见序编号
	//主文件改写前预扫 include 图的显式占号：主文件的 auto 分配先于 include 被处理，
	//若 include 里后来才出现显式 register，会与已分配的 auto 号撞号（跨 stage 布局重复 binding）
	void PreScanIncludes(const std::string& code, std::vector<std::string>& visited, int depth)
	{
		if (depth > 16)
			return;
		size_t pos = 0;
		while (pos < code.size())
		{
			size_t lineEnd = code.find('\n', pos);
			if (lineEnd == std::string::npos)
				lineEnd = code.size();
			std::string line = code.substr(pos, lineEnd - pos);
			size_t firstNonWs = line.find_first_not_of(" \t");
			if (firstNonWs != std::string::npos && line[firstNonWs] == '#')
			{
				size_t k = firstNonWs + 1;
				while (k < line.size() && (line[k] == ' ' || line[k] == '\t'))
					k++;
				if (line.compare(k, 7, "include") == 0)
				{
					size_t q1 = line.find('"', k + 7);
					size_t q2 = (q1 == std::string::npos) ? std::string::npos : line.find('"', q1 + 1);
					size_t a1 = line.find('<', k + 7);
					size_t a2 = (a1 == std::string::npos) ? std::string::npos : line.find('>', a1 + 1);
					std::string inc;
					if (q1 != std::string::npos && q2 != std::string::npos)
						inc = line.substr(q1 + 1, q2 - q1 - 1);
					else if (a1 != std::string::npos && a2 != std::string::npos)
						inc = line.substr(a1 + 1, a2 - a1 - 1);
					if (inc.empty() == false)
						PreScanResolve(inc, visited, depth);
				}
			}
			pos = lineEnd + 1;
		}
	}
	void PreScanResolve(const std::string& inc, std::vector<std::string>& visited, int depth)
	{
		std::string name = inc;
		{
			auto p = name.find('@');
			if (p != std::string::npos)
				name = name.substr(p);
		}
		if (name.empty())
			return;
		for (auto& v : visited)
		{
			if (v.compare(name) == 0)
				return;
		}
		visited.push_back(name);
		std::vector<std::string> candidates;
		if (name[0] != '/')
			candidates.push_back(MainDir + name);
		candidates.push_back(name);
		for (auto& c : candidates)
		{
			if (c.compare(MainName) == 0)
				return;
			auto ar = Compiler->GetShaderCodeStream(c.c_str(), c.c_str());
			if (ar == nullptr || ar->GetSourceCode() == nullptr || ar->GetSize() == 0)
				continue;
			std::string incCode((const char*)ar->GetSourceCode(), (size_t)ar->GetSize());
			ScanCodeExplicitBinds(incCode, AutoBindCounters);
			PreScanIncludes(incCode, visited, depth + 1);
			return;
		}
	}
	int STDMETHODCALLTYPE LoadSource(const wchar_t* pFilename, TDxcBlob** ppIncludeSource) override
	{
		*ppIncludeSource = nullptr;
		std::string name = WideStrToUtf8(pFilename);
		{
			auto pos = name.find('@');
			if (pos != std::string::npos)
				name = name.substr(pos);
		}
		if (name.empty())
			return E_FAIL;

		std::vector<std::string> candidates;
		bool isAbs = name[0] == '/';
		if (isAbs == false && name.compare(MainName) != 0)
		{
			candidates.push_back(MainDir + name);
		}
		candidates.push_back(name);

		// 对齐 Windows UEngineInclude：托管返回非 null 空内容（如 ScreenMaterial 的空
		// MaterialVar）也是有效 include，S_OK + 0 字节继续编译；只有托管拒绝（nullptr）
		// 才算 file not found。多候选时优先取非空命中
		FShaderCode* emptyFallback = nullptr;
		for (auto& c : candidates)
		{
			if (c.compare(MainName) == 0)
			{
				auto blob = new UDxcMemBlob(MainCode.data(), MainCode.size());
				*ppIncludeSource = blob;
				return S_OK;
			}
			auto ar = Compiler->GetShaderCodeStream(c.c_str(), c.c_str());
			if (ar == nullptr)
				continue;
			if (ar->GetSourceCode() != nullptr && ar->GetSize() > 0)
			{
				std::string incCode((const char*)ar->GetSourceCode(), (size_t)ar->GetSize());
				RewriteDxcAutoBindRegisters(incCode, AutoBindCounters);
				auto blob = new UDxcMemBlob(std::move(incCode));
				*ppIncludeSource = blob;
				return S_OK;
			}
			if (emptyFallback == nullptr)
				emptyFallback = ar;
		}
		if (emptyFallback != nullptr)
		{
			static const char s_emptyBlob = 0;
			*ppIncludeSource = new UDxcMemBlob(&s_emptyBlob, 0);
			return S_OK;
		}
		return E_FAIL;
	}
	uint32_t STDMETHODCALLTYPE AddRef() override { return 2; }
	uint32_t STDMETHODCALLTYPE Release() override { return 1; }
	int STDMETHODCALLTYPE QueryInterface(TDxcRefIID riid, void** ppv) override
	{
		if (ppv == nullptr)
			return E_POINTER;
		if (memcmp(&riid, &TDXC_IID_Unknown, sizeof(TDxcGUID)) == 0 ||
			memcmp(&riid, &TDXC_IID_IncludeHandler, sizeof(TDxcGUID)) == 0)
		{
			*ppv = (TDxcIncludeHandler*)this;
			return S_OK;
		}
		*ppv = nullptr;
		return E_NOINTERFACE;
	}
};

// 默认位置由宿主 exe 反推：binaries/net8.0/MainEditor → 仓库根/3rd/native/Dxc/Mac，
// 与启动工作目录无关（从 IDE/任意 shell 启动都不再需要 TITAN_DXC_LIB）
static std::string GetDxcDefaultLib()
{
	char exePath[PATH_MAX];
	uint32_t size = sizeof(exePath);
	if (_NSGetExecutablePath(exePath, &size) != 0)
		return std::string();
	std::string dir(exePath);
	auto pos = dir.find_last_of('/');
	if (pos == std::string::npos)
		return std::string();
	dir.resize(pos);
	return dir + "/../../3rd/native/Dxc/Mac/libdxcompiler.dylib";
}

static TDxcCreateInstanceProc GetDxcCreateInstance()
{
	static TDxcCreateInstanceProc s_proc = nullptr;
	static bool s_tried = false;
	if (s_tried)
		return s_proc;
	s_tried = true;
	const std::string defaultLib = GetDxcDefaultLib();
	const char* libCandidates[] = {
		getenv("TITAN_DXC_LIB"),
		defaultLib.c_str(),
		"libdxcompiler.dylib",
		"3rd/native/Dxc/Mac/libdxcompiler.dylib",
	};
	for (auto path : libCandidates)
	{
		if (path == nullptr || path[0] == 0)
			continue;
		void* h = dlopen(path, RTLD_LAZY | RTLD_LOCAL);
		if (h == nullptr)
			continue;
		s_proc = (TDxcCreateInstanceProc)dlsym(h, "DxcCreateInstance");
		if (s_proc != nullptr)
			break;
	}
	return s_proc;
}
#endif

NS_BEGIN

IShaderConductor* IShaderConductor::GetInstance()
{
	static IShaderConductor obj;
	return &obj;
}

//std::string 

//std::string DefaultLoadCallback(const std::string& includeName)
//{
//	const char* shaderRoot = "E:/Engine/Content/Shaders/";
//	std::string full = includeName;
//	for (size_t i = 0; i < full.size(); i++)
//	{
//		if (full[i] == '\\')
//			full[i] = '/';
//	}
//	auto pos = full.find_first_of(shaderRoot);
//	if (pos != std::string::npos)
//	{
//		full.substr(pos);
//	}
//}

bool IShaderConductor::CompileShader(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* shader, const char* entry, NxRHI::EShaderType type, const char* sm,
			const NxRHI::IShaderDefinitions* defines, bool bDebugShader, NxRHI::EShaderLanguage sl, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule)
{
	if (sl == NxRHI::EShaderLanguage::SL_DXBC)
	{
#if defined(PLATFORM_WIN)
		auto ar = compiler->GetShaderCodeStream(shader, shader);
		if (ar == nullptr)
			return false;

		std::vector<D3D_SHADER_MACRO> d11macros;
		if (defines != nullptr)
		{
			D3D_SHADER_MACRO tmp;
			for (size_t i = 0; i < defines->Definitions.size(); i++)
			{	
				tmp.Name = defines->Definitions[i].Name.c_str();
				tmp.Definition = defines->Definitions[i].Definition.c_str();
				d11macros.push_back(tmp);
			}
			tmp.Name = nullptr;
			tmp.Definition = nullptr;
			d11macros.push_back(tmp);
		}

		D3D10_SHADER_MACRO* pMacros = nullptr;
		if (d11macros.size() > 0)
			pMacros = &d11macros[0];

		std::string shaderTarget;
		switch (type)
		{
		case EShaderType::SDT_VertexShader:
			shaderTarget = "vs_";
			break;
		case EShaderType::SDT_PixelShader:
			shaderTarget = "ps_";
			break;
		case EShaderType::SDT_ComputeShader:
			shaderTarget = "cs_";
			break;
		default:
			break;
		}
		shaderTarget += sm;

		DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;

		if (bDebugShader)
		{
			dwShaderFlags |= D3DCOMPILE_DEBUG;
		}
		ID3DBlob* pBlob;
		ID3DBlob* pError;
		AutoPtr<UEngineInclude> engineInc(new UEngineInclude());
		engineInc->ShaderConductor = compiler;
		engineInc->Data2FileMap.insert(std::make_pair(nullptr, UEngineInclude::GetPath(shader)));
		auto hr = D3DCompile(ar->GetSourceCode(), ar->GetSize(), shader, pMacros, engineInc, entry, shaderTarget.c_str(), dwShaderFlags, 0, &pBlob, &pError);
		if (pError != NULL)
		{
			std::string pErrorStr = (char*)pError->GetBufferPointer();
			output->PushData(pErrorStr.c_str(), (UINT)pErrorStr.length());
			//VFX_LTRACE(ELTT_Graphics, (char*)pError->GetBufferPointer());
			pError->Release();
		}
		if (FAILED(hr))
		{
			ASSERT(false);
			return false;
		}

		desc->SetRhiData((BYTE*)pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());

#endif
	}

	if (sl == NxRHI::EShaderLanguage::SL_DXIL ||
		sl == NxRHI::EShaderLanguage::SL_SPIRV ||
		sl == NxRHI::EShaderLanguage::SL_GLSL ||
		sl == NxRHI::EShaderLanguage::SL_METAL)
	{
		auto ret = CompileHLSL(compiler, desc, shader, entry, type, sm, defines, sl, bDebugShader, extHlslVersion, dxcArgs, output, asModule);
		if (sl == NxRHI::EShaderLanguage::SL_DXIL)
			return ret;
		//Spirv is not ready for all shaders
		//ASSERT(ret);
		//return ret;
		return ret;
	}

	return true;
}

bool IShaderConductor::CompileHLSL(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* hlsl, const char* entry, NxRHI::EShaderType type, std::string sm,
	const NxRHI::IShaderDefinitions* defines, NxRHI::EShaderLanguage sl, bool debugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule)
{
#if defined(PLATFORM_WIN)
	auto ar = compiler->GetShaderCodeStream(hlsl, hlsl);
	std::string codeText((const char*)ar->GetSourceCode(), (size_t)ar->GetSize());
	
	const int spirvShiftStride = 4 * typeMaxBinding;
	int spirvShiftBase = 0;
	ShaderConductor::ShaderStage stage = ShaderConductor::ShaderStage::VertexShader;
	switch (type)
	{
	case NxRHI::EShaderType::SDT_VertexShader:
		stage = ShaderConductor::ShaderStage::VertexShader;
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_PixelShader:
		stage = ShaderConductor::ShaderStage::PixelShader;
		spirvShiftBase = 1 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_ComputeShader:
		stage = ShaderConductor::ShaderStage::ComputeShader;
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_AmplificationShader:
		stage = ShaderConductor::ShaderStage::AmplificationShader;
		spirvShiftBase = 2 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_MeshShader:
		stage = ShaderConductor::ShaderStage::MeshShader;
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_RayTracing:
		spirvShiftBase = 0 * spirvShiftStride;
		//stage = ShaderConductor::ShaderStage:;
		break;
	default:
		break;
	}

	std::string essl_version = "310";
	if (defines != nullptr)
	{
		auto pShaderModel = defines->FindDefine("ShaderModel");
		if (pShaderModel != nullptr)
		{
			if (pShaderModel->Definition == "3")
			{
				essl_version = "300";
			}
			else if (pShaderModel->Definition == "4")
			{
				essl_version = "310";
			}
			else if (pShaderModel->Definition == "5")
			{
				essl_version = "320";
			}
		}
	}

	auto add_DxcArgs = [](std::string& OutArgs, const char* arg)->void
	{
		if (OutArgs.length() == 0)
		{
			OutArgs += arg;
		}
		else
		{
			OutArgs += ' ';
			OutArgs += arg;
		}
	};

	std::string finalDxcArgs;
	if (dxcArgs != nullptr)
	{
		finalDxcArgs = dxcArgs;
		/*if (finalDxcArgs[finalDxcArgs.size() - 1] != ' ')
		{
			finalDxcArgs += ' ';
		}*/
	}

	ShaderConductor::Compiler::SourceDesc src{};
	src.source = codeText.c_str();
	src.entryPoint = entry;
	src.hlslExtVersion = extHlslVersion;
	//src.dxcArgString = dxcArgs;
	src.stage = stage;
	src.fileName = hlsl;
	src.loadIncludeCallback = [=](const char* includeName)->ShaderConductor::Blob
	{
		auto inc = std::string(includeName);
		auto pos = inc.find_first_of('@');
		if (pos != std::string::npos)
		{
			inc = inc.substr(pos);
		}
		auto ar_inc = compiler->GetShaderCodeStream(inc.c_str(), inc.c_str());
		
		if (ar_inc != nullptr)
			return ShaderConductor::Blob(ar_inc->GetSourceCode(), static_cast<uint32_t>(ar_inc->GetSize()));
		else
			return ShaderConductor::Blob(nullptr, static_cast<uint32_t>(0));
	};

	std::vector<ShaderConductor::MacroDefine> defs;
	if (defines != nullptr)
	{
		defs.resize(defines->Definitions.size());
		for (size_t i = 0; i < defines->Definitions.size(); i++)
		{
			defs[i].name = defines->Definitions[i].Name.c_str();
			defs[i].value = defines->Definitions[i].Definition.c_str();
		}
		src.numDefines = (int)defs.size();
		src.defines = &defs[0];
	}
	else
	{
		src.numDefines = 0;
		src.defines = nullptr;
	}

	ShaderConductor::Compiler::Options opt;
	opt.enableDebugInfo = debugShader;
	opt.disableOptimizations = debugShader;
	opt.packMatricesInRowMajor = false;
	opt.enable16bitTypes = true;
	auto _pos = sm.find_first_of('_');
	auto f1 = sm.substr(0, _pos);
	auto f2 = sm.substr(_pos + 1);
	opt.shaderModel.major_ver = atoi(f1.c_str());
	opt.shaderModel.minor_ver = atoi(f2.c_str());
	opt.shiftAllCBuffersBindings = spirvShiftBase + typeMaxBinding * 0;
	//opt.shiftAllCBuffersSet = 1;
	opt.shiftAllTexturesBindings = spirvShiftBase + typeMaxBinding * 1;
	//opt.shiftAllTexturesSet = 1;
	opt.shiftAllSamplersBindings = spirvShiftBase + typeMaxBinding * 2;
	//opt.shiftAllSamplersSet = 1;
	opt.shiftAllUABuffersBindings = spirvShiftBase + typeMaxBinding * 3;
	//opt.shiftAllUABuffersSet = 1;
	opt.needReflection = true;

	std::vector<ShaderConductor::Compiler::TargetDesc> dest;
	std::vector<ShaderConductor::Compiler::ResultDesc> result;

	ShaderConductor::Compiler::TargetDesc tmp{};
	ShaderConductor::Compiler::ResultDesc tmp2{};

	tmp.asModule = asModule;
	
	if (debugShader)
	{
		add_DxcArgs(finalDxcArgs, "-Qembed_debug");
	}

	switch (sl)
	{
		case NxRHI::EShaderLanguage::SL_DXBC:
			break;
		case NxRHI::EShaderLanguage::SL_DXIL:
			tmp.version = sm.c_str();// "6_2";
			tmp.language = ShaderConductor::ShadingLanguage::Dxil;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case NxRHI::EShaderLanguage::SL_GLSL:
			if (opt.shaderModel.major_ver >= 6)
			{
				add_DxcArgs(finalDxcArgs, "-fspv-target-env=vulkan1.2");
			}
			tmp.version = essl_version.c_str();
			tmp.language = ShaderConductor::ShadingLanguage::Essl;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case NxRHI::EShaderLanguage::SL_SPIRV:
			if (opt.shaderModel.major_ver >= 6)
			{
				add_DxcArgs(finalDxcArgs, "-fspv-target-env=vulkan1.2");
			}
			tmp.language = ShaderConductor::ShadingLanguage::SpirV;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case NxRHI::EShaderLanguage::SL_METAL:
			if (opt.shaderModel.major_ver >= 6)
			{
				add_DxcArgs(finalDxcArgs, "-fspv-target-env=vulkan1.2");
			}
			tmp.language = ShaderConductor::ShadingLanguage::Msl_iOS;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		default:
			break;
	}
	
	src.dxcArgString = finalDxcArgs.c_str();
	ShaderConductor::Compiler::Compile(src, opt, &dest[0], (uint32_t)dest.size(), &result[0]);

	for (size_t i = 0; i < result.size(); i++)
	{
		auto& tmp = result[i];
		if (tmp.errorWarningMsg.Size() != 0)
		{
			output->PushData((char*)tmp.errorWarningMsg.Data(), tmp.errorWarningMsg.Size());
		}
		if (tmp.hasError == false)
		{
			if (dest[i].language == ShaderConductor::ShadingLanguage::Dxil)
			{
				auto& finalResult = result[i];
				size_t sz = finalResult.target.Size();
				desc->RhiData.resize(sz);
				memcpy(&desc->RhiData[0], (char*)finalResult.target.Data(), finalResult.target.Size());
				desc->Language = EShaderLanguage::SL_DXIL;

#if defined(HasModule_Dx12)
				if (asModule)
				{
					auto pDx12Reflection = (ID3D12LibraryReflection*)finalResult.reflection.GetD3D12LibraryReflection();
					if (pDx12Reflection != nullptr)
					{
						DX12Shader::Reflect(desc, pDx12Reflection);
					}
				}
				else 
				{
					auto pDx12Reflection = (ID3D12ShaderReflection*)finalResult.reflection.GetD3D12ShaderReflection();
					if (pDx12Reflection != nullptr)
					{
						DX12Shader::Reflect(desc, pDx12Reflection);
					}
				}
				
#endif
			}
			else if(dest[i].language == ShaderConductor::ShadingLanguage::Essl)
			{
				auto& data = result[i].target;
				desc->RhiData.resize(data.Size());
				memcpy(&desc->RhiData[0], (char*)data.Data(), data.Size());
				desc->Language = EShaderLanguage::SL_GLSL;
			}
			else if (dest[i].language == ShaderConductor::ShadingLanguage::Msl_iOS)
			{
				auto& data = result[i].target;
				desc->RhiData.resize(data.Size());
				memcpy(&desc->RhiData[0], (char*)data.Data(), data.Size());
				desc->Language = EShaderLanguage::SL_METAL;
			}
			else if (dest[i].language == ShaderConductor::ShadingLanguage::SpirV)
			{
				auto& finalResult = result[i];
				size_t sz = finalResult.target.Size();
				desc->RhiData.resize(sz);
				memcpy(&desc->RhiData[0], (char*)finalResult.target.Data(), finalResult.target.Size());
				desc->Language = EShaderLanguage::SL_SPIRV;
#if defined(HasModule_Vulkan)
				VKShader::Reflect(desc);
#endif
			}
		}
		else
		{
			return false;
		}
	}

	if (CoreSDK::OnShaderTranslated != nullptr)
	{
		CoreSDK::OnShaderTranslated(desc);
	}

	return true;
#elif defined(PLATFORM_MAC)
	auto ar = compiler->GetShaderCodeStream(hlsl, hlsl);
	if (ar == nullptr)
		return false;
	std::string codeText((const char*)ar->GetSourceCode(), (size_t)ar->GetSize());

	TDxcCreateInstanceProc pfnDxcCreateInstance = GetDxcCreateInstance();
	if (pfnDxcCreateInstance == nullptr)
	{
		VFX_LTRACE(ELTT_Graphics, "[DxcMac] libdxcompiler.dylib not found (TITAN_DXC_LIB / <exe>/../../3rd/native/Dxc/Mac / cwd 3rd/native/Dxc/Mac)");
		return false;
	}

	TDxcCompiler3* dxcCompiler = nullptr;
	int hr = pfnDxcCreateInstance(TDXC_CLSID_Compiler, TDXC_IID_Compiler3, (void**)&dxcCompiler);
	if (hr < 0)
	{
		VFX_LTRACE(ELTT_Graphics, "[DxcMac] DxcCreateInstance failed 0x%08x", (UINT)hr);
		return false;
	}

	const int spirvShiftStride = 4 * typeMaxBinding;
	int spirvShiftBase = 0;
	switch (type)
	{
	case NxRHI::EShaderType::SDT_VertexShader:
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_PixelShader:
		spirvShiftBase = 1 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_ComputeShader:
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_AmplificationShader:
		spirvShiftBase = 2 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_MeshShader:
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_RayTracing:
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	default:
		break;
	}
	const char* stage = "vs";
	switch (type)
	{
	case NxRHI::EShaderType::SDT_VertexShader:
		stage = "vs";
		break;
	case NxRHI::EShaderType::SDT_PixelShader:
		stage = "ps";
		break;
	case NxRHI::EShaderType::SDT_ComputeShader:
		stage = "cs";
		break;
	case NxRHI::EShaderType::SDT_AmplificationShader:
		stage = "as";
		break;
	case NxRHI::EShaderType::SDT_MeshShader:
		stage = "ms";
		break;
	case NxRHI::EShaderType::SDT_RayTracing:
		stage = "lib";
		break;
	default:
		break;
	}

	std::string finalDxcArgs;
	if (dxcArgs != nullptr)
		finalDxcArgs = dxcArgs;

	std::vector<std::wstring> argStorage;
	auto addArgW = [&](const std::wstring& a)->void
	{
		argStorage.push_back(a);
	};
	auto addArg = [&](const char* a)->void
	{
		argStorage.push_back(Utf8ToWideStr(a));
	};

	auto _pos = sm.find_first_of('_');
	int smMajor = atoi(sm.substr(0, _pos).c_str());

	std::string target = std::string(stage) + "_" + sm;
	addArg("-E");
	addArg(entry);
	addArg("-T");
	addArg(target.c_str());
	if (sl == NxRHI::EShaderLanguage::SL_SPIRV)
	{
		//对齐 Windows ShaderConductor：b=base+0*stride段, t=+1, s=+2, u=+3（全按 space 0）
		char num[32];
		addArg("-spirv");
		snprintf(num, sizeof(num), "%d", spirvShiftBase + typeMaxBinding * 0);
		addArg("-fvk-b-shift"); addArg(num); addArg("0");
		snprintf(num, sizeof(num), "%d", spirvShiftBase + typeMaxBinding * 1);
		addArg("-fvk-t-shift"); addArg(num); addArg("0");
		snprintf(num, sizeof(num), "%d", spirvShiftBase + typeMaxBinding * 2);
		addArg("-fvk-s-shift"); addArg(num); addArg("0");
		snprintf(num, sizeof(num), "%d", spirvShiftBase + typeMaxBinding * 3);
		addArg("-fvk-u-shift"); addArg(num); addArg("0");
		if (smMajor >= 6)
			addArg("-fspv-target-env=vulkan1.2");
	}
	if (extHlslVersion != nullptr && extHlslVersion[0] != 0)
	{
		addArg("-HV");
		addArg(extHlslVersion);
	}
	addArg("-enable-16bit-types");
	if (debugShader)
	{
		addArg("-Zi");
		addArg("-Qembed_debug");
	}
	if (defines != nullptr)
	{
		for (size_t i = 0; i < defines->Definitions.size(); i++)
		{
			std::string def = defines->Definitions[i].Name.c_str();
			def += '=';
			def += defines->Definitions[i].Definition.c_str();
			addArg("-D");
			addArg(def.c_str());
		}
	}
	{
		std::istringstream iss(finalDxcArgs);
		std::string tok;
		while (iss >> tok)
			addArg(tok.c_str());
	}
	//源文件名作位置参数：clang 据此建立主文件目录，相对 include 拼接后进 include handler
	addArg(hlsl);

	UEngineDxcInclude includeHandler;
	includeHandler.Compiler = compiler;
	includeHandler.MainName = hlsl;
	{
		std::string mainDir = hlsl;
		for (size_t i = 0; i < mainDir.size(); i++)
		{
			if (mainDir[i] == '\\')
				mainDir[i] = '/';
		}
		size_t pos = mainDir.find_last_of('/');
		includeHandler.MainDir = (pos != std::string::npos) ? mainDir.substr(0, pos + 1) : "";
	}
	{
		//主文件改写前先预扫 include 图：auto 分配必须避开任意顺序下出现的显式占号
		std::vector<std::string> visited;
		includeHandler.PreScanIncludes(codeText, visited, 0);
	}
	RewriteDxcAutoBindRegisters(codeText, includeHandler.AutoBindCounters);
	includeHandler.MainCode = codeText;

	TDxcBuffer srcBuf{};
	srcBuf.Ptr = codeText.data();
	srcBuf.Size = codeText.size();
	srcBuf.Encoding = TDXC_CP_UTF8;

	TDxcResult* result = nullptr;
	std::vector<const wchar_t*> argPtrs;
	argPtrs.reserve(argStorage.size());
	for (auto& w : argStorage)
		argPtrs.push_back(w.c_str());
	hr = dxcCompiler->Compile(&srcBuf, (const wchar_t* const*)argPtrs.data(), (uint32_t)argPtrs.size(), &includeHandler, TDXC_IID_Result, (void**)&result);
	dxcCompiler->Release();
	if (hr < 0 || result == nullptr)
	{
		VFX_LTRACE(ELTT_Graphics, "[DxcMac] Compile call failed 0x%08x (%s)", (UINT)hr, hlsl);
		return false;
	}

	int status = -1;
	result->GetStatus(&status);

	TDxcBlobUtf8* errors = nullptr;
	result->GetOutput(TDXC_OUT_ERRORS, TDXC_IID_BlobUtf8, (void**)&errors, nullptr);
	if (errors != nullptr && errors->GetStringLength() > 0)
	{
		output->PushData(errors->GetStringPointer(), (UINT)errors->GetStringLength());
	}
	if (errors != nullptr)
		errors->Release();

	if (status < 0)
	{
		result->Release();
		VFX_LTRACE(ELTT_Graphics, "[DxcMac] compile failed: %s(%s)", hlsl, entry);
		return false;
	}

	TDxcBlob* object = nullptr;
	hr = result->GetOutput(TDXC_OUT_OBJECT, TDXC_IID_Blob, (void**)&object, nullptr);
	result->Release();
	if (hr < 0 || object == nullptr || object->GetBufferSize() == 0)
	{
		if (object != nullptr)
			object->Release();
		VFX_LTRACE(ELTT_Graphics, "[DxcMac] no object output: %s(%s)", hlsl, entry);
		return false;
	}

	size_t sz = object->GetBufferSize();
	desc->RhiData.resize(sz);
	memcpy(&desc->RhiData[0], object->GetBufferPointer(), sz);
	object->Release();
	desc->Language = EShaderLanguage::SL_SPIRV;
#if defined(HasModule_Vulkan)
	VKShader::Reflect(desc);
#endif

	if (CoreSDK::OnShaderTranslated != nullptr)
	{
		CoreSDK::OnShaderTranslated(desc);
	}

	return true;
#else
	return false;
#endif
}

//bool CompileRayTracingLibrary(IDxcBlob** ppDXILBlob) 
//{
//	try {
//		// 初始化DXC
//		CComPtr<IDxcUtils> pUtils;
//		CComPtr<IDxcCompiler3> pCompiler;
//		DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&pUtils));
//		DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&pCompiler));
//
//		// 加载HLSL源码
//		CComPtr<IDxcBlobEncoding> pSource;
//		pUtils->LoadFile(L"RayTracing.hlsl", nullptr, &pSource);
//
//		// 配置编译参数
//		std::vector<LPCWSTR> args = {
//			L"-T", L"lib_6_6",
//			L"-E", L"RayGen",
//			L"-Zi", L"-Qembed_debug",
//			DXC_ARG_WARNINGS_ARE_ERRORS
//		};
//
//		// 执行编译
//		DxcBuffer sourceBuffer = {
//			pSource->GetBufferPointer(),
//			pSource->GetBufferSize(),
//			DXC_CP_UTF8
//		};
//
//		CComPtr<IDxcResult> pResult;
//		pCompiler->Compile(&sourceBuffer, args.data(), (UINT)args.size(),
//			nullptr, IID_PPV_ARGS(&pResult));
//
//		// 错误处理
//		CComPtr<IDxcBlobUtf8> pErrors;
//		pResult->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(&pErrors), nullptr);
//		if (pErrors && pErrors->GetStringLength() > 0) {
//			OutputDebugStringA(pErrors->GetStringPointer());
//		}
//
//		HRESULT status;
//		pResult->GetStatus(&status);
//		if (FAILED(status)) return false;
//
//		// 提取DXIL
//		return SUCCEEDED(pResult->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(ppDXILBlob), nullptr));
//	}
//	catch (...) {
//		return false;
//	}
//}

bool IShaderConductor::CompileDXR(NxRHI::FShaderCompiler* compiler, IBlobObject* lib, const char* sm, const NxRHI::IShaderDefinitions* defines, bool bDebugShader, NxRHI::EShaderLanguage sl)
{
	return false;
}

NS_END
