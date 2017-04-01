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
	float g_ET;
};

//float nrand(in float2 uv)
//{
//	float noiseX = (frac(sin(dot(uv, float2(12.9898 + g_ET * 0.264, 78.233 + g_ET * 0.523) * 2.0)) * 43758.5453));
//}

VSOut main(VSIn input)
{
	VSOut output;
	
	output.prePos = mul(float4(input.pos.x + sin(g_ET + input.pos.y * 0.4) * input.pos.y * 0.06, input.pos.yz, 1), World);
	output.nor = mul(float4(input.nor.xyz, 1), NormalMatrix).xyz;
	output.pos = mul(output.prePos, mul(View, Projection));
	
	return output;
}