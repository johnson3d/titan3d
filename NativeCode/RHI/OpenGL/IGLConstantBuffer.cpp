#include "IGLConstantBuffer.h"
#include "IGLShaderProgram.h"
#include "IGLCommandList.h"
#include "IGLRenderContext.h"

#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

IGLConstantBuffer::IGLConstantBuffer()
{
}

IGLConstantBuffer::~IGLConstantBuffer()
{
	Cleanup();
}

void IGLConstantBuffer::Cleanup()
{
	mBuffer.reset();
}

//void IGLConstantBuffer::UpdateDrawPass(ICommandList* cmd)
//{
//	if (mDirty == false)
//		return;
//	mDirty = false;
//
//	GLSdk* sdk = ((IGLCommandList*)cmd)->mCmdList;
//
//	auto immCmd = cmd->GetContext()->GetImmCommandList();
//	this->AddRef();
//	sdk->PushCommand([=]()->void {
//		UpdateContent(immCmd, &VarBuffer[0], (UINT)VarBuffer.size());
//		
//		this->Release();
//	}, "UpdateDrawPass");
//}

bool IGLConstantBuffer::UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size)
{
	AUTO_SAMP("Native.IConstantBuffer.UpdateContent");
	if (Size > Desc.Size)
		return false;
	RHI_ASSERT(Size == Desc.Size);

	GLSdk* sdk = ((IGLCommandList*)cmd)->mCmdList;

	if (mBuffer == nullptr || mBuffer->BufferId==0)
	{
		return false;
	}

	sdk->BindBuffer(GL_UNIFORM_BUFFER, mBuffer);
	
#if FALSE
	BYTE* pDestBuffer;
	sdk->MapBufferRange((void**)&pDestBuffer, GL_UNIFORM_BUFFER, 0, Size, GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
	if (pDestBuffer != nullptr)
	{
		/*for (auto i = Desc.Vars.begin(); i != Desc.Vars.end(); i++)
		{
			ConstantVarDesc& var = *i;
			
			if (var.Dirty)
			{
				memcpy(pDestBuffer + var.Offset, (BYTE*)pBuffer + var.Offset, var.Size);
				var.Dirty = FALSE;
			}
		}*/
		memcpy(pDestBuffer, pBuffer, Size);
		GLboolean ret;
		sdk->UnmapBuffer(&ret, GL_UNIFORM_BUFFER);
	}
#else
	sdk->BufferData(GL_UNIFORM_BUFFER, Desc.Size, pBuffer, GL_STATIC_DRAW);//GL_DYNAMIC_DRAW
#endif
	
	sdk->BindBuffer(GL_UNIFORM_BUFFER, 0);
	
	return true;
}

bool IGLConstantBuffer::Init(IGLRenderContext* rc, const IConstantBufferDesc* desc)
{
	Desc = *desc;
	std::shared_ptr<GLSdk> sdk(new GLSdk(TRUE));
	
	mBuffer = sdk->GenBufferId();
	sdk->BindBuffer(GL_UNIFORM_BUFFER, mBuffer);
	sdk->BufferData(GL_UNIFORM_BUFFER, desc->Size, NULL, GL_DYNAMIC_DRAW);

	GLSdk::ImmSDK->PushCommandDirect([=]()->void
	{
		sdk->Execute();
	}, "IGLConstantBuffer::Init");

	return true;
}

NS_END