#include "IGLShaderReflector.h"
#include "IGLRenderContext.h"

#define new VNEW

NS_BEGIN

#define GL_DescSetBindPoint(type, dest, bindPoint) do{\
		switch (type) \
		{\
				case EST_UnknownShader:\
					dest.VSBindPoint = bindPoint;\
					dest.PSBindPoint = bindPoint;\
					dest.CSBindPoint = bindPoint;\
					break;\
				case EST_VertexShader:\
					dest.VSBindPoint = bindPoint;\
					break;\
				case EST_PixelShader:\
					dest.PSBindPoint = bindPoint;\
					break;\
				case EST_ComputeShader:\
					dest.CSBindPoint = bindPoint;\
					break;\
		}\
		dest.BindCount = bindPoint;\
	}\
	while(0);

std::shared_ptr<GLSdk::GLBufferId> IGLShaderReflector::Reflect(IGLRenderContext* rc, const std::shared_ptr<GLSdk::GLBufferId>& mShader, IShader* pShader)
{
	IShaderDesc* pShaderDesc = pShader->GetDesc();
	auto sdk = GLSdk::ImmSDK;

	std::shared_ptr<GLSdk::GLBufferId> mProgram = sdk->CreateProgram();

	sdk->AttachShader(mProgram, mShader);
	sdk->LinkProgram(mProgram, nullptr);

	GLint Result = GL_FALSE;
	int InfoLogLength = 0;

	sdk->GetProgramiv(mProgram, GL_LINK_STATUS, &Result);
	sdk->GetProgramiv(mProgram, GL_INFO_LOG_LENGTH, &InfoLogLength);
	/*if (InfoLogLength > 0) {
		std::vector<char> ProgramErrorMessage(InfoLogLength + 1);
		sdk->GetProgramInfoLog(mProgram, InfoLogLength, NULL, &ProgramErrorMessage[0]);
		RHI_Trace("%s\n", &ProgramErrorMessage[0]);
	}*/

	sdk->DetachShader(mProgram, mShader);

	ReflectProgram(rc, mProgram, pShaderDesc->ShaderType, pShaderDesc->Reflector);

	return mProgram;
}

void IGLShaderReflector::ReflectProgram(IGLRenderContext* rc, const std::shared_ptr<GLSdk::GLBufferId>& mProgram, EShaderType shaderType, ShaderReflector* pReflector)
{
	auto sdk = GLSdk::ImmSDK;

	auto csReflector = pReflector;
	csReflector->mCBDescArray.clear();
	csReflector->mTexBindInfoArray.clear();
	csReflector->mSamplerBindInfoArray.clear();

	//reflection
	GLchar szName[256];
	GLuint uboCount = 0;

	sdk->GetProgramiv(mProgram, GL_ACTIVE_UNIFORM_BLOCKS, (GLint*)&uboCount);
	for (GLuint cbIndex = 0; cbIndex < uboCount; cbIndex++)
	{
		IConstantBufferDesc desc;
		memset(szName, 0, sizeof(szName));
		GLsizei len = 0;

		sdk->GetActiveUniformBlockName(mProgram, cbIndex, sizeof(szName), &len, szName);
		desc.Name = szName;

		if (desc.Name.AsStdString().find("type_") == 0)
		{
			desc.Name = desc.Name.AsStdString().substr(5);
		}

		GLenum props_binding = GL_BUFFER_BINDING;
		GLint outSize;
		GLint bindLoc;
		
		if (IRenderContext::mChooseShaderModel <= 3)
		{
			bindLoc = cbIndex;
		}
		else
		{
			sdk->ES31_GetProgramResourceiv(mProgram, GL_UNIFORM_BLOCK, cbIndex, 1, &props_binding, 4, &outSize, &bindLoc);
		}
		sdk->UniformBlockBinding(mProgram, cbIndex, bindLoc);

		GLint bufferSize;
		sdk->GetActiveUniformBlockiv(mProgram, cbIndex, GL_UNIFORM_BLOCK_DATA_SIZE, &bufferSize);
		desc.Size = bufferSize;

		GLint varCount;
		sdk->GetActiveUniformBlockiv(mProgram, cbIndex, GL_UNIFORM_BLOCK_ACTIVE_UNIFORMS, &varCount);

		RHI_ASSERT(varCount > 0);

		std::vector<GLint> indices_block;
		indices_block.resize(varCount);
		sdk->GetActiveUniformBlockiv(mProgram, cbIndex, GL_UNIFORM_BLOCK_ACTIVE_UNIFORM_INDICES, &indices_block[0]);

		std::vector<GLint> offset_block;
		offset_block.resize(varCount);
		sdk->GetActiveUniformsiv(mProgram, (GLsizei)indices_block.size(), (const GLuint*)&indices_block[0], GL_UNIFORM_OFFSET, &offset_block[0]);

		std::vector<GLint> size_block;
		size_block.resize(varCount);
		sdk->GetActiveUniformsiv(mProgram, (GLsizei)indices_block.size(), (const GLuint*)&indices_block[0], GL_UNIFORM_SIZE, &size_block[0]);

		std::vector<GLint> type_block;
		type_block.resize(varCount);
		sdk->GetActiveUniformsiv(mProgram, (GLsizei)indices_block.size(), (const GLuint*)&indices_block[0], GL_UNIFORM_TYPE, &type_block[0]);

		for (size_t i = 0; i < indices_block.size(); i++)
		{
			char varNames[256];
			GLsizei strLen = 0;

			GLenum type;
			GLint arraySize;
			//glGetActiveUniformName(mProgram, indices_block[i], 256, &strLen, varNames);
			sdk->GetActiveUniform(mProgram, indices_block[i], 256, &strLen, &arraySize, &type, varNames);

			ConstantVarDesc varDesc;
			varDesc.Name = varNames;
			varDesc.Size = GLTypeToShaderVarSize(type_block[i])*arraySize;
			varDesc.Offset = offset_block[i];
			varDesc.Type = GLTypeToShaderVarType(type_block[i]);
			varDesc.Elements = (UINT)arraySize;

			auto segs = StringHelper::split(varDesc.Name, ".");
			if (segs.size() == 2)
			{
				varDesc.Name = segs[1];
			}

			desc.Vars.push_back(varDesc);
		}
		//fuck opengl!
		std::map<std::string, ConstantVarDesc*> structVars;
		for (auto iter = desc.Vars.begin(); iter != desc.Vars.end(); )
		{
			auto strPos = iter->Name.AsStdString().find_first_of(".");
			if (strPos != std::string::npos)
			{
				iter->Name = iter->Name.AsStdString().substr(0, strPos);
				auto stIter = structVars.find(iter->Name);
				if (stIter != structVars.end())
				{
					if (iter->Offset < stIter->second->Offset)
						stIter->second->Offset = iter->Offset;
					stIter->second->Size += iter->Size;
				}
				else
				{
					auto stVar = new ConstantVarDesc();
					stVar->Type = SVT_Struct;
					stVar->Name = iter->Name;
					stVar->Offset = iter->Offset;
					stVar->Size = iter->Size;
					stVar->Elements = 1;
					structVars.insert(std::make_pair(iter->Name.AsStdString(), stVar));
				}
				iter = desc.Vars.erase(iter);
				continue;
			}
			strPos = iter->Name.AsStdString().find_first_of("[");
			if (strPos != std::string::npos)
			{
				iter->Name = iter->Name.AsStdString().substr(0, strPos);
			}
			iter++;
		}

		for (auto i : structVars)
		{
			desc.Vars.push_back(*i.second);
			delete i.second;
		}
		structVars.clear();

		GL_DescSetBindPoint(shaderType, desc, bindLoc);
		desc.BindCount = 1;
		csReflector->mCBDescArray.push_back(desc);
	}

	GLint uniformCount = 0;
	sdk->GetProgramiv(mProgram, GL_ACTIVE_UNIFORMS, (GLint*)&uniformCount);
	for (GLint uIdx = 0; uIdx < uniformCount; uIdx++)
	{
		GLint uniformType;
		sdk->GetActiveUniformsiv(mProgram, 1, (const GLuint*)&uIdx, GL_UNIFORM_TYPE, &uniformType);

		char varNames[256];
		GLsizei strLen = 0;

		GLenum type;
		GLint arraySize;
		sdk->GetActiveUniform(mProgram, uIdx, 256, &strLen, &arraySize, &type, varNames);

		GLint bindLoc;
		sdk->GetUniformLocation(&bindLoc, mProgram, varNames);

		switch (uniformType)
		{
		case GL_SAMPLER_1D:
		case GL_SAMPLER_2D:
		case GL_SAMPLER_3D:
		case GL_SAMPLER_CUBE:
		{
			std::string spirv_name_samp = "gDefaultSamplerState";
			std::string spirv_name = varNames;// "SPIRV_Cross_Combined" + mTextures[i].Name + "Samp_" + mTextures[i].Name;
			if (spirv_name.find("SPIRV_Cross_Combined") == 0)
			{
				spirv_name = spirv_name.substr(strlen("SPIRV_Cross_Combined"));
				auto pos = spirv_name.find("Samp_");
				if (pos == std::string::npos)
				{
					pos = spirv_name.find("gDefaultSamplerState");
				}
				if (pos != std::string::npos)
				{
					spirv_name_samp = spirv_name.substr(pos);
					spirv_name = spirv_name.substr(0, pos);
				}
			}

			TSBindInfo tDesc;
			tDesc.Name = spirv_name;
			GL_DescSetBindPoint(shaderType, tDesc, bindLoc);
			tDesc.BindCount = 1;

			TSBindInfo sDesc;
			sDesc.Name = spirv_name_samp;// "Samp_" + spirv_name;
			GL_DescSetBindPoint(shaderType, sDesc, bindLoc);
			sDesc.BindCount = 1;

			csReflector->mTexBindInfoArray.push_back(tDesc);
			csReflector->mSamplerBindInfoArray.push_back(sDesc);
		}
		break;
		case GL_UNSIGNED_INT_IMAGE_BUFFER:
		{//RWBuffer<uint>
			/*GLenum props_binding = GL_LOCATION;
			GLint outSize;
			sdk->GetProgramResourceiv(mProgram, GL_UNIFORM, uIdx, 1, &props_binding, 4, &outSize, &bindLoc);*/

			IConstantBufferDesc desc;
			desc.Type = SIT_UAV_RWTYPED;
			desc.Name = varNames;
			GL_DescSetBindPoint(shaderType, desc, bindLoc);
			desc.BindCount = 1;

			if (shaderType == EST_ComputeShader)
			{
				csReflector->mCBDescArray.push_back(desc);
			}
			else
			{
				TSBindInfo bindInfo;
				bindInfo.Name = desc.Name;
				bindInfo.BindCount = desc.BindCount;
				bindInfo.Type = desc.Type;
				bindInfo.VSBindPoint = desc.VSBindPoint;
				bindInfo.PSBindPoint = desc.PSBindPoint;
				bindInfo.CSBindPoint = desc.CSBindPoint;

				TSBindInfo sampBindInfo;
				sampBindInfo = bindInfo;
				sampBindInfo.Name = std::string("CS_") + sampBindInfo.Name.c_str();

				csReflector->mTexBindInfoArray.push_back(bindInfo);
				csReflector->mSamplerBindInfoArray.push_back(sampBindInfo);
			}
		}
		break;
		default:
			break;
		}
	}

	if (IRenderContext::mChooseShaderModel > 3)
	{
		GLint storageBlockCount = 0;
		sdk->ES31_GetProgramInterfaceiv(mProgram, GL_SHADER_STORAGE_BLOCK, GL_ACTIVE_RESOURCES, &storageBlockCount);
		for (GLint uIdx = 0; uIdx < storageBlockCount; uIdx++)
		{
			IConstantBufferDesc desc;
			GLsizei length = 0;
			sdk->ES31_GetProgramResourceName(mProgram, GL_SHADER_STORAGE_BLOCK, uIdx, 256, &length, szName);

			GLenum props_binding = GL_BUFFER_BINDING;
			GLint outSize;
			GLint bindLoc;
			sdk->ES31_GetProgramResourceiv(mProgram, GL_SHADER_STORAGE_BLOCK, uIdx, 1, &props_binding, 4, &outSize, &bindLoc);

			desc.Name = szName;

			auto segs = StringHelper::split(desc.Name, "_");
			if (segs.size() <= 1)
			{
				desc.Type = SIT_UAV_RWSTRUCTURED;
			}
			else if (segs[1] == "RWStructuredBuffer")
			{
				desc.Type = SIT_UAV_RWSTRUCTURED;
			}
			else if (segs[1] == "StructuredBuffer")
			{
				desc.Type = SIT_STRUCTURED;
			}
			else
			{
				desc.Type = SIT_STRUCTURED;
			}

			GL_DescSetBindPoint(shaderType, desc, bindLoc);
			desc.BindCount = 1;

			if (shaderType == EST_ComputeShader)
			{
				csReflector->mCBDescArray.push_back(desc);
			}
			else
			{
				TSBindInfo bindInfo;
				bindInfo.Name = desc.Name;
				bindInfo.BindCount = desc.BindCount;
				bindInfo.Type = desc.Type;
				bindInfo.VSBindPoint = desc.VSBindPoint;
				bindInfo.PSBindPoint = desc.PSBindPoint;
				bindInfo.CSBindPoint = desc.CSBindPoint;

				TSBindInfo sampBindInfo;
				sampBindInfo = bindInfo;
				sampBindInfo.Name = std::string("CS_") + sampBindInfo.Name.c_str();

				csReflector->mTexBindInfoArray.push_back(bindInfo);
				csReflector->mSamplerBindInfoArray.push_back(sampBindInfo);
			}
		}
	}
}

NS_END
