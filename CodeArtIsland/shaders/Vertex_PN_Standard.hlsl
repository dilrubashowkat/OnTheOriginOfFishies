struct VSIn
{
	float4 pos : SV_POSITION;
	float4 nor : NORMAL;
};

struct VSOut
{
	float4 pos : SV_POSITION;
	float3 nor : TEXCOORD0;
	float4 prePos : TEXCOORD1;
};

cbuffer TransformBuffer : register(b0)
{
    matrix World;
    matrix View;
	matrix Projection;
	matrix NormalMatrix;
};

cbuffer ParamsBuffer : register(b2)
{
	float4 Color;
	float ExplodeMul;
	int RandSeed;
};

VSOut main(VSIn input, uint id:SV_VERTEXID)
{
	VSOut output;
	
	output.prePos = mul(float4(input.pos.xyz, 1), World);
	output.nor = mul(float4(input.nor.xyz, 1), NormalMatrix).xyz;
	output.pos = mul(output.prePos, mul(View, Projection));

	return output;
}