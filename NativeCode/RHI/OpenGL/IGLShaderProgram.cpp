#include "IGLShaderProgram.h"
#include "IGLVertexShader.h"
#include "IGLPixelShader.h"
#include "IGLInputLayout.h"
#include "IGLCommandList.h"
#include "IGLRenderContext.h"
#include "IGLShaderReflector.h"
#include "../Utility/GraphicsProfiler.h"

#define new VNEW

NS_BEGIN

IGLShaderProgram::IGLShaderProgram()
{
	mNoPSProfilering = FALSE;
	mProgramReflector = AutoRef<ShaderReflector>(new ShaderReflector());
}

IGLShaderProgram::~IGLShaderProgram()
{
	Cleanup();
}

void IGLShaderProgram::Cleanup()
{
	mProgram.reset();
}

vBOOL IGLShaderProgram::LinkShaders(IRenderContext* rc)
{
	auto sdk = GLSdk::ImmSDK;
	ASSERT(mProgram);
	ASSERT(sdk->IsGLThread());
	RHI_ASSERT(mInputLayout != nullptr);

	auto glvs = mVertexShader.UnsafeConvertTo<IGLVertexShader>();
	sdk->AttachShader(mProgram, glvs->mShader);

	auto glps = mPixelShader.UnsafeConvertTo<IGLPixelShader>();
	sdk->AttachShader(mProgram, glps->mShader);

	sdk->LinkProgram(mProgram, this);

	GLint Result = GL_FALSE;
	int InfoLogLength;
	sdk->GetProgramiv(mProgram, GL_LINK_STATUS, &Result);
	sdk->GetProgramiv(mProgram, GL_INFO_LOG_LENGTH, &InfoLogLength);

	if (InfoLogLength > 0) 
	{
		std::vector<char> ProgramErrorMessage;
		ProgramErrorMessage.resize(InfoLogLength + 1);
		ProgramErrorMessage[InfoLogLength] = (char)0;
		sdk->GetProgramInfoLog(mProgram, InfoLogLength, &InfoLogLength, &ProgramErrorMessage[0]);
		VFX_LTRACE(ELTT_Graphics, "glGetProgramInfoLog = %s\r\n", &ProgramErrorMessage[0]);
	}

	sdk->DetachShader(mProgram, (mVertexShader.UnsafeConvertTo<IGLVertexShader>())->mShader);
	sdk->DetachShader(mProgram, (mPixelShader.UnsafeConvertTo<IGLPixelShader>())->mShader);

	if (Result == GL_FALSE)
	{
		VFX_LTRACE(ELTT_Graphics, "GL_LINK_STATUS = GL_FALSE\r\n");
		return FALSE;
	}

	IGLShaderReflector::ReflectProgram((IGLRenderContext*)rc, mProgram, EST_UnknownShader, mProgramReflector);

	for (size_t i = 0; i < mInputLayout->mDesc.Layouts.size(); i++)
	{
		//attribName = "in_+ elem.SemanticName + std::to_string(elem.SemanticIndex);;
		auto& elem = mInputLayout->mDesc.Layouts[i];
		std::string attribName = std::string("in_var_") + elem.SemanticName.c_str();
		if (elem.SemanticIndex > 0)
			attribName += std::to_string(elem.SemanticIndex);

		GLint loc;
		sdk->GetAttribLocation(&loc, mProgram, attribName.c_str());

		mVBSlotMapper[elem.InputSlot] = loc;
	}
	return TRUE;
}

/*
glBindBufferBase(GL_UNIFORM_BUFFER, 0, rain_buffer);
glGetUniformLocation();
glGetUniformBlockIndex();//�õ� Uniform Block ����������
glGetUniformIndices();//����һ�� Uniform Block �ڱ������ֵ����飨��������������Ȼ�󷵻���Щ����������������������ĸ�������
glGetActiveUniformsiv();//��ָ�� GL_UNIFORM_BLOCK_DATA_SIZE ����ʱ�����Բ�ѯ Block �ռ�Ĵ�С����������Ϊ�洢 Uniform Block �Ļ����������ݷ���ռ�
glGetActiveUniformsiv()// ��ָ�� GL_UNIFORM_OFFSET ����ʱ��ͨ�� Uniform Block �ڱ����������õ� Block ��ͬ������ƫ����
*/

void IGLShaderProgram::ApplyShaders(ICommandList* cmd)
{
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	if (cmd->mProfiler != nullptr && cmd->mProfiler->mNoPixelShader)
	{
		if (mNoPSProfilering == FALSE)
		{
			sdk->UseProgram(mProgram, this);
			IPixelShader* ps = cmd->mProfiler->mPSEmpty;
			auto glps = (IGLVertexShader*)ps;
			sdk->AttachShader(mProgram, glps->mShader);
			mNoPSProfilering = TRUE;
		}
	}
	else
	{
		if (mNoPSProfilering == TRUE)
		{
			sdk->UseProgram(mProgram, this);
			auto glps = mPixelShader.UnsafeConvertTo<IGLVertexShader>();
			sdk->AttachShader(mProgram, glps->mShader);
			mNoPSProfilering = FALSE;
		}
	}
	if (cmd->mCurrentState.TrySet_ShaderProgram(this)==false)
		return;
	sdk->UseProgram(mProgram, this);
}

UINT IGLShaderProgram::FindCBuffer(const char* name)
{
	return mProgramReflector->FindCBuffer(name);
}

UINT IGLShaderProgram::GetCBufferNumber()
{
	return (UINT)mProgramReflector->mCBDescArray.size();
}

IConstantBufferDesc* IGLShaderProgram::GetCBuffer(UINT index)
{
	return mProgramReflector->GetCBuffer(index);
}

UINT IGLShaderProgram::GetShaderResourceNumber() const
{
	return (UINT)mProgramReflector->mTexBindInfoArray.size();
}

bool IGLShaderProgram::GetShaderResourceBindInfo(UINT Index, TSBindInfo* info, int dataSize) const
{
	auto pSrv = mProgramReflector->GetSRV(Index);
	if (pSrv==nullptr)
		return false;

	if (dataSize >= sizeof(TSBindInfo))
		*info = *pSrv;
	else
		memcpy(info, pSrv, dataSize);
	return true;
}

UINT IGLShaderProgram::GetTextureBindSlotIndex(const char* name)
{
	return mProgramReflector->FindSRV(name);
	/*for (size_t i = 0; i < mTextures.size(); i++)
	{
		if (mTextures[i].Name == name)
			return (int)i;
	}
	return -1;*/
}

UINT IGLShaderProgram::GetSamplerNumber() const
{
	return (UINT)mProgramReflector->mSamplerBindInfoArray.size();
}

bool IGLShaderProgram::GetSamplerBindInfo(UINT Index, TSBindInfo* info, int dataSize) const
{
	auto pSampler = mProgramReflector->GetSampler(Index);
	if (pSampler == nullptr)
		return false;
	if (dataSize >= sizeof(TSBindInfo))
		*info = *pSampler;
	else
		memcpy(info, pSampler, dataSize);
	return true;
}

UINT IGLShaderProgram::GetSamplerBindSlotIndex(const char* name)
{
	return mProgramReflector->FindSampler(name);
	/*for (size_t i = 0; i < mSamplers.size(); i++)
	{
		if (mSamplers[i].Name == name)
			return (int)i;
	}
	return -1;*/
}

bool IGLShaderProgram::Init(IGLRenderContext* rc, const IShaderProgramDesc* desc)
{
	auto sdk = GLSdk::ImmSDK;
	
	mProgram = sdk->CreateProgram();

	BindInputLayout(desc->InputLayout);
	BindVertexShader(desc->VertexShader);
	BindPixelShader(desc->PixelShader);
	
	return true;
}

NS_END