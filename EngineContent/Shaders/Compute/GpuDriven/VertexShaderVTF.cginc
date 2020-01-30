Texture2D<float4> gVertexTexture : register(t14);
SamplerState Samp_gVertexTexture;

Texture2D<float4> gInstanceDataTexture : register(t13);
SamplerState Samp_gInstanceDataTexture;

#define VertexStride 3u
#define VertexTextureSize 512u
#define VertexTexPixel 0.001953125f

MeshVertex GetVertexData(uint index)
{
	MeshVertex o;
	float2 uv;
	float2 uv1;
	float2 uv2;

	uint dataIndex = index * VertexStride;
	uint row = dataIndex / VertexTextureSize;
	uint col = dataIndex % VertexTextureSize;	
	
	uv.x = (((float)col) + 0.5f) * VertexTexPixel;
	uv.y = (((float)row) + 0.5f) * VertexTexPixel;
	float4 v1 = gVertexTexture.SampleLevel(Samp_gVertexTexture, uv, 0);

	uint dataIndex1 = dataIndex + 1;
	uint row1 = dataIndex1 / VertexTextureSize;
	uint col1 = dataIndex1 % VertexTextureSize;
	uv1.x = (((float)col1) + 0.5f) * VertexTexPixel;
	uv1.y = (((float)row1) + 0.5f) * VertexTexPixel;
	float4 v2 = gVertexTexture.SampleLevel(Samp_gVertexTexture, uv1, 0);

	uint dataIndex2 = dataIndex1 + 1;
	uint row2 = dataIndex2 / VertexTextureSize;
	uint col2 = dataIndex2 % VertexTextureSize;
	uv2.x = (((float)col2) + 0.5f) * VertexTexPixel;
	uv2.y = (((float)row2) + 0.5f) * VertexTexPixel;
	float4 v3 = gVertexTexture.SampleLevel(Samp_gVertexTexture, uv2, 0);

	o.Position = v1.xyz;
	o.DiffuseU = v1.w;
	o.Normal = v2.xyz;
	o.DiffuseV = v2.w;
	o.Tangent = v3;

	return o;
}

#define InstanceStride 9u
#define InstanceTextureSize 256u
#define InstTexPixel 0.00390625f

MeshInstanceData GetInstanceData(uint index)
{
	MeshInstanceData o = (MeshInstanceData)0;;
	float2 uv;

	uint dataIndex = index * InstanceStride;
	uint row = dataIndex / InstanceTextureSize;
	uint col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v1 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v2 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v3 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v4 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	/*dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v5 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v6 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v7 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v8 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);

	dataIndex += 1;
	row = dataIndex / InstanceTextureSize;
	col = dataIndex % InstanceTextureSize;
	uv.x = ((float)col + 0.5f) * InstTexPixel;
	uv.y = ((float)row + 0.5f) * InstTexPixel;
	float4 v9 = gInstanceDataTexture.SampleLevel(Samp_gInstanceDataTexture, uv, 0);*/

	float4x4 temp =
	{
		v1.x, v2.x, v3.x, v4.x,
		v1.y, v2.y, v3.y, v4.y,
		v1.z, v2.z, v3.z, v4.z,
		v1.w, v2.w, v3.w, v4.w,
	};
	o.Matrix = temp;

	return o;
}
