#pragma once
#include "../NxEffect.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12Shader;
	class DX12GraphicsEffect;
	class DX12ComputeEffect;
	class DX12Buffer;

	class DX12ShaderSignatureBuilder
	{
	public:
		int			mCbvSrvUavNumber = 0;
		int			mSamplerNumber = 0;
		std::vector<AutoRef<FShaderBinder>>	mCbvSrvUavBinders;
		std::vector<AutoRef<FShaderBinder>>	mSamplerBinders;
		const FShaderBinder* FindBinder(VNameString name) const
		{
			for (auto& i : mCbvSrvUavBinders)
			{
				if (i->Name == name)
					return i;
			}
			for (auto& i : mSamplerBinders)
			{
				if (i->Name == name)
					return i;
			}
			return nullptr;
		}
		void Build(IShaderReflector* reflector);
		void Build(std::map<VNameString, AutoRef<FEffectBinder>>& binders);
		bool CreateHeap(DX12GpuDevice* device, AutoRef<DX12HeapHolder>& OutCbvSrvUavHeap, AutoRef<DX12HeapHolder>& OutSamplerHeap);

		struct FSignatureBinder
		{
			AutoRef<FShaderBinder>		Binder;
			int							RootIndex;
			int							DescriptorStart;
			int							DescriptorNum;
		};
		AutoRef<ID3D12RootSignature> CreateSignature(DX12GpuDevice* device, D3D12_ROOT_SIGNATURE_FLAGS flags,
			const std::vector<VNameString>* roots,
			IShaderReflector* pOutReflector,
			std::vector<FSignatureBinder>& OutCbvSrvUav, std::vector<FSignatureBinder>& OutSampler);
	};

	class DX12GraphicsEffect : public IGraphicsEffect
	{
	public:
		~DX12GraphicsEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) override;

		AutoRef<ID3D12CommandSignature> GetIndirectDrawIndexCmdSig(DX12GpuDevice* device, ICommandList* cmdlist);
		AutoRef<ID3D12CommandSignature> GetIndirectDrawCmdSig(DX12GpuDevice* device, ICommandList* cmdlist);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		DX12ShaderSignatureBuilder		mSignatureBuilder;
		AutoRef<ID3D12RootSignature>	mSignature;
		std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> mCbvSrvUavBinders;
		std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> mSamplerBinders;

		AutoRef<ID3D12CommandSignature>	mCmdSignature;
		UINT							mIndirectOffset = 0;

		//AutoRef<ID3D12CommandSignature>	CmdSigForIndirectDrawIndex;
	};

	class DX12ComputeEffect : public IComputeEffect
	{
	public:
		~DX12ComputeEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist) override;

		AutoRef<ID3D12CommandSignature> GetIndirectDispatchCmdSig(DX12GpuDevice* device, ICommandList* cmdlist);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		DX12ShaderSignatureBuilder		mSignatureBuilder;
		AutoRef<ID3D12RootSignature>	mSignature;
		std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> mCbvSrvUavBinders;
		std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> mSamplerBinders;

		AutoRef<ID3D12PipelineState>	mPipelineState;
		
		UINT							mIndirectOffset = 0;
		AutoRef<ID3D12CommandSignature>	mCmdSignature;
	};

	class DX12RayTracingEffect : public IRayTracingEffect
	{
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		DX12ShaderSignatureBuilder			mSignatureBuilder;

		AutoRef<ID3D12RootSignature>	mGlobalSignature;
		AutoRef<IShaderReflector>		mGlobalReflector;
		std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> mGlobalCbvSrvUavBinders;
		std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> mGlobalSamplerBinders;

		AutoRef<ID3D12StateObject>		mStateObject;
		AutoRef<ID3D12StateObjectProperties> mStateObjectProperties;

		AutoRef<FUploadBuffer>			mRayGenShaderTable;
		AutoRef<FUploadBuffer>			mMissShaderTable;
	public:
		class DX12HitGroup : public FHitGroup
		{
		public:
			AutoRef<ID3D12RootSignature>	Dx12Signature;
			std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> CbvSrvUavBinders;
			std::vector<DX12ShaderSignatureBuilder::FSignatureBinder> SamplerBinders;
			void* HitGroupShaderIdentifier;
		};
		virtual bool BuildEffect(IGpuDevice* device) override;
		virtual FHitGroup* CreateHitGroup() override
		{
			return new DX12HitGroup();
		}
		virtual bool BuildHitGroup(FHitGroup* group) override;
		virtual const IShaderReflector* GetReflector() const { 
			return mShaderLibDesc->DxILReflector;
		}
		AutoRef<ID3D12StateObject> CreateDxrStateObject(DX12GpuDevice* device, DX12RayTracingEffect* effect);
	};
}

NS_END
