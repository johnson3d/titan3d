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

	struct FRootParameter
	{
		void Reset()
		{
			IsSamplers = false;
			RootIndex = -1;
			HeapStartIndex = -1;
			Descriptors.clear();
			TempShaderBinders.clear();
		}
		enum EGraphicsRootType
		{
			VS_Begin = 0,
			VS_Cbv = VS_Begin,
			VS_Srv,
			VS_Uav,
			VS_Sampler,
			VS_End = VS_Sampler,

			PS_Begin = VS_End + 1,
			PS_Cbv = PS_Begin,
			PS_Srv,
			PS_Uav,
			PS_Sampler,
			PS_End = PS_Sampler,

			GraphicsNumber,
		};
		enum EComputeRootType
		{
			CS_Begin = 0,
			CS_Cbv = CS_Begin,
			CS_Srv,
			CS_Uav,
			CS_Sampler,
			CS_End = CS_Sampler,

			ComputeNumber,
		};
		bool					IsSamplers = false;
		int						RootIndex = -1;
		int						HeapStartIndex = -1;
		std::vector<D3D12_DESCRIPTOR_RANGE>		Descriptors;
		std::vector<FShaderBinder*>				TempShaderBinders;
		void BuildShaderBinders() {
			for (auto& i : TempShaderBinders)
			{
				i->DescriptorIndex += HeapStartIndex;
			}
			TempShaderBinders.clear();
		}
		D3D12_DESCRIPTOR_RANGE* GetDescriptorAddress() {
			if (Descriptors.size() == 0)
				return nullptr;
			return &Descriptors[0];
		}
		inline bool IsValidRoot() const {
			return Descriptors.size() > 0;
		}
		void PushShaderBinder(FShaderBinder* pBinder, D3D12_DESCRIPTOR_RANGE_TYPE type)
		{
			if (pBinder == nullptr)
				return;
			D3D12_DESCRIPTOR_RANGE rangeVS{};
			rangeVS.RangeType = type;
			rangeVS.NumDescriptors = pBinder->BindCount;
			rangeVS.BaseShaderRegister = pBinder->Slot;
			rangeVS.RegisterSpace = pBinder->Space;
			rangeVS.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			//ASSERT(pBinder->DescriptorIndex == -1); DescriptorIndex saved by shader, check valid only for GraphicsEffect
			pBinder->DescriptorIndex = (UINT)Descriptors.size();
			Descriptors.push_back(rangeVS);
			TempShaderBinders.push_back(pBinder);
		}

		void BuildDX12RootParameters(int& StartIndex, D3D12_SHADER_VISIBILITY shaderVis, std::vector<D3D12_ROOT_PARAMETER>& dxRootParameters)
		{
			D3D12_ROOT_PARAMETER tmp{};
			tmp.ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
			tmp.ShaderVisibility = shaderVis;
			tmp.DescriptorTable.NumDescriptorRanges = (UINT)Descriptors.size();
			if (tmp.DescriptorTable.NumDescriptorRanges > 0)
			{
				tmp.DescriptorTable.pDescriptorRanges = GetDescriptorAddress();
				RootIndex = (UINT)dxRootParameters.size();
				dxRootParameters.push_back(tmp);
				HeapStartIndex = StartIndex;
				StartIndex += (int)tmp.DescriptorTable.NumDescriptorRanges;
				BuildShaderBinders();
			}
		}
	};
	
	class DX12GraphicsEffect : public IGraphicsEffect
	{
	public:
		~DX12GraphicsEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) override;

		void Push2RootParamters(FEffectBinder* binder);
		AutoRef<ID3D12CommandSignature> GetIndirectDrawIndexCmdSig(DX12GpuDevice* device, ICommandList* cmdlist);
		AutoRef<ID3D12CommandSignature> GetIndirectDrawCmdSig(DX12GpuDevice* device, ICommandList* cmdlist);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		AutoRef<ID3D12RootSignature>	mSignature;
		AutoRef<ID3D12CommandSignature>	mCmdSignature;
		UINT							mIndirectOffset = 0;

		//AutoRef<ID3D12CommandSignature>	CmdSigForIndirectDrawIndex;
		UINT							mCbvSrvUavNumber = 0;
		UINT							mSamplerNumber = 0;

		FRootParameter					mRootParameters[FRootParameter::GraphicsNumber];
	};

	class DX12ComputeEffect : public IComputeEffect
	{
	public:
		~DX12ComputeEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist) override;

		void Push2Root(FShaderBinder* binder);
		AutoRef<ID3D12CommandSignature> GetIndirectDispatchCmdSig(DX12GpuDevice* device, ICommandList* cmdlist);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		AutoRef<ID3D12RootSignature>	mSignature;
		UINT							mIndirectOffset = 0;
		AutoRef<ID3D12CommandSignature>	mCmdSignature;
		AutoRef<ID3D12PipelineState>	mPipelineState;

		UINT							mCbvSrvUavNumber = 0;
		UINT							mSamplerNumber = 0;
		FRootParameter					mRootParameters[FRootParameter::ComputeNumber];
	};
}

NS_END
