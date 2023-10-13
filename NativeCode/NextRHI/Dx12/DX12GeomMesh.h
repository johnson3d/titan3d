#pragma once
#include "../NxGpuDevice.h"
#include "../NxGeomMesh.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12CommandList;
	class DX12VertexArray : public FVertexArray
	{
	public:
		static void Commit(DX12CommandList* cmdlist, UINT NumOfVA, DX12VertexArray** VAs);
		virtual void Commit(ICommandList* cmdlist) override;
		virtual void BindVB(EVertexStreamType stream, IVbView* buffer) override;
		struct FRefResources : public IGpuResource
		{
			std::vector<AutoRef<IGpuResource>>	Resources;
		};

		AutoRef<FRefResources>	RefResources;
	};
}

NS_END

