#pragma once
#include "IRenderResource.h"
#include "IShader.h"
#include "IComputeShader.h"
#include "IVertexBuffer.h"
#include "IIndexBuffer.h"
#include "ITextureBase.h"
#include "ICommandList.h"
#include "ISamplerState.h"
#include "IConstantBuffer.h"
#include "IShaderResourceView.h"
#include "IUnorderedAccessView.h"
#include "Utility/IGeometryMesh.h"

NS_BEGIN

class IRenderContext;
class ICommandList;
class IRenderPipeline;
class IVertexShader;
class IIndexShader;
class IVertexBuffer;
class IIndexBuffer;
class ITextureBase;
class IShaderResourceView;
class ISamplerState;
class IShaderProgram;
class IMeshPrimitives;

class IDrawCall;

template<typename ResType, typename SelfType>
struct IResourcesBinder : public VIUnknownBase
{
	typedef std::vector<std::pair<UINT, AutoRef<ResType>>> CResVector;
	CResVector					VSResources;
	CResVector					PSResources;
	bool						IsDirty;

	IResourcesBinder()
	{
		IsDirty = false;
	}
	virtual std::string GetShaderResourcesHash() const
	{
		std::stringstream ss;
		for (auto& i : VSResources)
		{
			//ss << i.first;
			ss << SelfType::GetResourceHash(i.second);
		}
		for (auto& i : PSResources)
		{
			//ss << i.first;
			ss << SelfType::GetResourceHash(i.second);
		}
		return ss.str();
	}
	void BindVS(UINT slot, ResType* buffer)
	{
		if (slot == 0xFFFFFFFF)
			return;
		for (auto& i : VSResources)
		{
			if (i.first == (USHORT)slot)
			{
				if (i.second == buffer)
					return;

				i.second.StrongRef(buffer);
				IsDirty = true;
				return;
			}
		}
		AutoRef<ResType> tmp;
		tmp.StrongRef(buffer);
		VSResources.push_back(std::make_pair(slot, tmp));
		IsDirty = true;
	}
	void BindPS(UINT slot, ResType* buffer)
	{
		if (slot == 0xFFFFFFFF)
			return;
		for (auto& i : PSResources)
		{
			if (i.first == (USHORT)slot)
			{
				if (i.second == buffer)
					return;

				i.second.StrongRef(buffer);
				IsDirty = true;
				return;
			}
		}
		AutoRef<ResType> tmp;
		tmp.StrongRef(buffer);
		PSResources.push_back(std::make_pair(slot, tmp));
		IsDirty = true;
	}

	//shared with vs
	void BindCS(UINT slot, ResType* buffer)
	{
		if (slot == 0xFFFFFFFF)
			return;
		for (auto& i : VSResources)
		{
			if (i.first == (USHORT)slot)
			{
				if (i.second == buffer)
					return;

				i.second.StrongRef(buffer);
				IsDirty = true;
				return;
			}
		}
		AutoRef<ResType> tmp;
		tmp.StrongRef(buffer);
		VSResources.push_back(std::make_pair(slot, tmp));
		IsDirty = true;
	}
	ResType* FindVS(const char* name)
	{
		for (auto& i : VSResources)
		{
			if (i.second == nullptr)
				continue;

			if (SelfType::IsEqualName(i.second, name))
			{
				return i.second;
			}
		}
		return nullptr;
	}
	ResType* FindPS(const char* name)
	{
		for (auto& i : PSResources)
		{
			if (i.second == nullptr)
				continue;

			if (SelfType::IsEqualName(i.second, name))
			{
				return i.second;
			}
		}
		return nullptr;
	}
	ResType* GetResourceVS(UINT slot)
	{
		for (auto& i : VSResources)
		{
			if (i.first == slot)
			{
				return i.second;
			}
		}
		return nullptr;
	}
	ResType* GetResourcePS(UINT slot)
	{
		for (auto& i : VSResources)
		{
			if (i.first == slot)
			{
				return i.second;
			}
		}
		return nullptr;
	}
	UINT NumOfVS()
	{
		return (UINT)VSResources.size();
	}
	UINT NumOfPS()
	{
		return (UINT)PSResources.size();
	}
};

struct TR_CLASS(SV_BaseFunction = true)
	ICBufferResources : public IResourcesBinder<IConstantBuffer, ICBufferResources>
{
	ICBufferResources()
	{

	}
	static bool IsEqualName(IConstantBuffer* obj, const char* name)
	{
		return obj->Desc.Name == name;
	}
	static void* GetResourceHash(IConstantBuffer* cb)
	{
		return cb;
	}
};

struct TR_CLASS(SV_BaseFunction = true)
IShaderRViewResources : public IResourcesBinder<IShaderResourceView, IShaderRViewResources>
{
	IShaderRViewResources() 
	{
		
	}
	static bool IsEqualName(IShaderResourceView* obj, const char* name)
	{
		return false;
	}
	static void* GetResourceHash(IShaderResourceView* state)
	{
		return state->GetAPIObject();
	}
};

struct TR_CLASS(SV_BaseFunction = true)
ISamplerResources : public  IResourcesBinder<ISamplerState, ISamplerResources>
{	
	ISamplerResources()
	{
		
	}
	static bool IsEqualName(ISamplerState* obj, const char* name)
	{
		return false;
	}
	static void* GetResourceHash(ISamplerState* state)
	{
		return state;
	}
};

struct TR_CLASS(SV_BaseFunction = true)
	IUavResources : public  IResourcesBinder<IUnorderedAccessView, IUavResources>
{
	IUavResources()
	{

	}
	static bool IsEqualName(IUnorderedAccessView* obj, const char* name)
	{
		return false;
	}
	static void* GetResourceHash(IUnorderedAccessView* cb)
	{
		return cb;
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
DrawPrimitiveDesc
{
	DrawPrimitiveDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		PrimitiveType = EPT_TriangleList;
		BaseVertexIndex = 0;
		StartIndex = 0;
		NumPrimitives = 0;
		NumInstances = 1;
	}
	EPrimitiveType PrimitiveType;
	UINT BaseVertexIndex;
	UINT StartIndex;
	UINT NumPrimitives;
	UINT NumInstances;
	bool IsIndexDraw() const{
		return StartIndex != 0xFFFFFFFF;
	}
};

class TR_CLASS()
IViewPort : public VIUnknown
{
public:
	IViewPort()
	{
		TopLeftX = 0;
		TopLeftY = 0;
		Width = 0;
		Height = 0;
		MinDepth = 0;
		MaxDepth = 1.0F;
	}
	RTTI_DEF(IViewPort, 0x1e91cb6f5b04f4ef, true);

	float TopLeftX;
	float TopLeftY;
	float Width;
	float Height;
	float MinDepth;
	float MaxDepth;
};

struct TR_CLASS(SV_LayoutStruct = 8)
SRRect
{
	int MinX;
	int MinY;
	int MaxX;
	int MaxY;
};

class TR_CLASS()
IScissorRect : public VIUnknown
{
public:
	RTTI_DEF(IScissorRect, 0x486b7d125d70a687, true);
	IScissorRect()
	{

	}
	std::vector<SRRect> Rects;
	
	void SetRectNumber(UINT num);
	UINT GetRectNumber() {
		return (UINT)Rects.size();
	}
	void SetSCRect(UINT idx, int left, int top, int right, int bottom);
	void GetSCRect(UINT idx, SRRect* pRect);
};

struct TR_CLASS(SV_LayoutStruct = 8)
IDrawCallDesc
{

};

class GraphicsProfiler;
class IGpuBuffer;

class TR_CLASS()
	IDrawCall : public IRenderResource
{
	friend GraphicsProfiler;
protected:
	AutoRef<IRenderPipeline>				mPipelineState;
	
	AutoRef<IMeshPrimitives>				MeshPrimitives;
	AutoRef<IGpuBuffer>						IndirectDrawArgsBuffer;
	UINT									IndirectDrawOffsetForArgs;

	UINT									AtomIndex;
	UINT									LodLevel;
	UINT									NumInstances;

	AutoRef<ICBufferResources>				mShaderCBufferBinder;
	AutoRef<IShaderRViewResources>			mShaderSrvBinder;
	AutoRef<ISamplerResources>				mShaderSamplerBinder;
	
	AutoRef<IVertexArray>					AttachVBs;
	AutoRef<IIndexBuffer>					AttachIndexBuffer;
	AutoRef<IShaderRViewResources>			AttachSRVs;

	IManagedObjectHolder					TagHandle;

	void BuildPassDefault(ICommandList * cmd, vBOOL bImmCBuffer);
	std::string GetShaderResourcesHash() const{
		std::stringstream ss;
		if (mShaderCBufferBinder != nullptr) {
			ss << mShaderCBufferBinder->GetShaderResourcesHash();
		}
		if (mShaderSrvBinder != nullptr) {
			ss << mShaderSrvBinder->GetShaderResourcesHash();
		}
		if (mShaderSamplerBinder != nullptr) {
			ss << mShaderSamplerBinder->GetShaderResourcesHash();
		}
		return ss.str();
	}
public:
	IDrawCall();
	~IDrawCall();

	inline void operator=(const IDrawCall& rh) {
		assert(false);
	}
	IConstantBuffer* FindCBufferVS(const char* name);
	IConstantBuffer* FindCBufferPS(const char* name);
	virtual void BuildPass(ICommandList* cmd, vBOOL bImmCBuffer);
	virtual vBOOL ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer);

	IRenderPipeline* GetPipeline() {
		return mPipelineState;
	}
	virtual void BindPipeline(IRenderPipeline* pipeline);
	
	virtual void BindGeometry(IMeshPrimitives* mesh, UINT atom, float lod);
	void SetLod(float lod);
	float GetLod();
	void BindAttachVBs(IVertexArray* vbs) {
		AttachVBs.StrongRef(vbs);
	}
	void BindAttachIndexBuffer(IIndexBuffer* ib) {
		AttachIndexBuffer.StrongRef(ib);
	}
	
	void BindAttachSRVs(IShaderRViewResources* srvs) {
		AttachSRVs.StrongRef(srvs);
	}	
	ICBufferResources* GetCBufferResources() {
		if (mShaderCBufferBinder == nullptr)
		{
			mShaderCBufferBinder.WeakRef(new ICBufferResources());
		}
		return mShaderCBufferBinder;
	}
	IShaderRViewResources* GetShaderRViewResources() {
		if (mShaderSrvBinder == nullptr)
		{
			mShaderSrvBinder.WeakRef(new IShaderRViewResources());
		}
		return mShaderSrvBinder;
	}
	ISamplerResources* GetShaderSamplers() {
		if (mShaderSamplerBinder == nullptr)
		{
			mShaderSamplerBinder.WeakRef(new ISamplerResources());
		}
		return mShaderSamplerBinder;
	}
	virtual void BindShaderRViewResources(IShaderRViewResources* res)
	{
		mShaderSrvBinder.StrongRef(res);
	}
	virtual void BindShaderSamplers(ISamplerResources* samps)
	{
		mShaderSamplerBinder.StrongRef(samps);
	}
	void GetDrawPrimitive(DrawPrimitiveDesc* desc);
	void SetInstanceNumber(int instNum);
	void SetIndirectDraw(IGpuBuffer* pBuffer, UINT offset);
	
	virtual void SetViewport(ICommandList* cmd, IViewPort* vp) = 0;
	virtual void SetScissorRect(ICommandList* cmd, IScissorRect* sr) = 0;

	virtual void SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline, EPrimitiveType dpType) = 0;
	virtual void SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride) = 0;
	virtual void SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer) = 0;

	virtual void VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) = 0;
	virtual void PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) = 0;
	virtual void VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) = 0;
	virtual void PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) = 0;
	virtual void VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) = 0;
	virtual void PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) = 0;
	
	virtual void DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	virtual void DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	virtual void DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) = 0;

	void BindShaderCBuffer(UINT index, IConstantBuffer* CBuffer);
	void BindShaderCBuffer(IShaderBinder* binder, IConstantBuffer* CBuffer);

	void BindShaderSrv(UINT index, IShaderResourceView* srv);
	void BindShaderSrv(IShaderBinder* binder, IShaderResourceView* srv);

	void BindShaderSampler(UINT index, ISamplerState* sampler);
	void BindShaderSampler(IShaderBinder* binder, ISamplerState* sampler);

	ShaderReflector* GetReflector();

	void SetTagHandle(void* handle) {
		TagHandle.SetHandle(handle);
	}
	void* GetTagHandle() {
		return TagHandle.mHandle;
	}
};

class TR_CLASS()
	IComputeDrawcall : public IRenderResource
{
protected:
	AutoRef<IComputeShader>				mComputeShader;
	AutoRef<ICBufferResources>			mShaderCBufferBinder;
	AutoRef<IShaderRViewResources>		mShaderSrvBinder;
	AutoRef<IUavResources>				mShaderUavBinder;

	AutoRef<IGpuBuffer>					IndirectDrawArgsBuffer;
	UINT								IndirectDrawArgsOffset;
	UINT			mDispatchX;
	UINT			mDispatchY;
	UINT			mDispatchZ;
public:
	IComputeDrawcall()
	{
		IndirectDrawArgsOffset = 0;
		mDispatchX = 0;
		mDispatchY = 0;
		mDispatchZ = 0;
	}
	void SetComputeShader(IComputeShader * shader);
	ICBufferResources* GetCBufferResources() {
		if (mShaderCBufferBinder == nullptr)
		{
			mShaderCBufferBinder.WeakRef(new ICBufferResources());
		}
		return mShaderCBufferBinder;
	}
	ShaderReflector* GetReflector();
	IShaderRViewResources* GetShaderRViewResources() {
		if (mShaderSrvBinder == nullptr)
		{
			mShaderSrvBinder.WeakRef(new IShaderRViewResources());
		}
		return mShaderSrvBinder;
	}
	IUavResources* GetUavResources() {
		if (mShaderUavBinder == nullptr)
		{
			mShaderUavBinder.WeakRef(new IUavResources());
		}
		return mShaderUavBinder;
	}
	void SetDispatch(UINT x, UINT y, UINT z);	
	void SetDispatchIndirectBuffer(IGpuBuffer* buffer, UINT offset);
	virtual void BuildPass(ICommandList* cmd) = 0;
};

NS_END