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

float3 psRand(int vertID)
{
	return normalize(fmod(float3(tan((vertID + 91 + RandSeed) * 17), tan((vertID + 6 + RandSeed) * 83), tan((vertID + 56 + RandSeed) * 34.253)), float3(1, 1, 1)));
	/*float noiseX = (frac(sin(dot(float2(vertID, 0), float2(12.9898, 78.233) * 2.0)) * 43758.5453));
	float noiseY = sqrt(1 - noiseX * noiseX);
	return float2(noiseX, noiseY);*/
}

float psRandM(int vertID)
{
	return fmod(tan((vertID + 72 + RandSeed) * 56), 1);
}

VSOut main(VSIn input, uint id:SV_VERTEXID)
{
	VSOut output;
	
	//float4x4 worldViewProj = mul(World, mul(View, Projection));
	output.prePos = mul(float4(input.pos.xyz + psRand(id / 3) * ExplodeMul * psRandM(id / 3) * float3(1, 1, 2), 1), World);
	output.nor = mul(float4(input.nor.xyz, 1), NormalMatrix).xyz;
	output.pos = mul(output.prePos, mul(View, Projection));
	
	//output.pos = mul(input.pos, World);
	//output.pos = mul(output.pos, View);
	//output.pos = mul(output.pos, Projection);
	//output.col = input.col;

	return output;
}