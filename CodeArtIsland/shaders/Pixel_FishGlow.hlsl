#define MAX_FISH_COUNT 16

cbuffer TransformBuffer : register(b0)
{
    matrix World;
    matrix View;
	matrix Projection;
};

cbuffer FishBuffer : register(b1)
{
	int FishCount;
	float GlowMultiplier;
	float IntensityMultiplier;
	float randMul;
	float4 FishPosition[MAX_FISH_COUNT];
	float4 FishColor[MAX_FISH_COUNT];
};

struct VSOut
{
	float4 pos : SV_POSITION;
	float4 tex : TEXCOORD0;
};

float2 psRand(float vertID)
{
	return normalize(fmod(float2(tan((vertID + 91) * 17), tan((vertID + 6) * 83)), float2(1, 1)));
}

float2 rand_2_10(in float2 uv) {
	float noiseX = (frac(sin(dot(uv, float2(12.9898, 78.233) * 2.0)) * (43758.5453 + randMul)));
	float noiseY = (frac(sin(dot(uv, float2(56.4781, 18.752) * 2.0)) * (34824.8135 + randMul)));
	//return float2(noiseX, noiseY) * 2 - float2(1, 1);
	noiseX *= 2.0 * 3.141592653589796265894f;
	return float2(cos(noiseX) * noiseY, sin(noiseX) * noiseY);
}

float4 main(VSOut input) : SV_TARGET
{
	float3 col;

	input.tex.x = input.tex.x * 2.0f - 1.0f;
	input.tex.y = -input.tex.y * 2.0f + 1.0f;

//return input.tex;
	for (int i = 0; i < FishCount; i++)
	{
		//float4x4 worldViewProj = mul(Projection, View);
		float4 pos = mul(FishPosition[i], mul(View, Projection));
		pos.xyz /= pos.w;

		float2 dif = pos.xy - input.tex.xy;
		dif.x *= 16.0f / 9.0f;
		dif += clamp(dot(dif, dif) * rand_2_10(input.pos) * 0.4, -0.08, 0.08);
		float d = dot(dif, dif);
		//d += (psRandM(FishPosition[i].x * 100.0f + d) + 1) * 0.001f;
		//if (d < 0.00025)
		//	return float4(1, 0, 0, 1);
		//pos.z /= 10.0f;
		//if (d < 0.00025)
		//	return pos.z / 10.0f;
		col += IntensityMultiplier / (d * pos.w * GlowMultiplier + 1) * FishColor[i].rgb;
	}

	return float4(col, 1);
}