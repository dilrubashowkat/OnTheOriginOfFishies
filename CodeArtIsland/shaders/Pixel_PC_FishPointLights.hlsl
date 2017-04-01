#define MAX_FISH_COUNT 16

cbuffer FishBuffer : register(b1)
{
	int FishCount;
	float GlowMultiplier;
	float IntensityMultiplier;
	float randMul;
	float4 FishPosition[MAX_FISH_COUNT];
	float4 FishColor[MAX_FISH_COUNT];
};

cbuffer ParamsBuffer : register(b2)
{
	float4 Color;
};

cbuffer SunBuffer : register(b4)
{
	float4 SunDirection;	//w intensity
	float4 SunColor;
};

struct VSOut
{
	float4 pos : SV_POSITION;
	float3 nor : TEXCOORD0;
	float4 prePos : TEXCOORD1;
};

float4 main(VSOut input) : SV_TARGET
{
	input.nor = normalize(input.nor);

	float3 lightSum = float3(0.1, 0.1, 0.1);

	lightSum += saturate(dot(-SunDirection.xyz, input.nor) * SunDirection.w) * SunColor.xyz;

	for (int i = 0; i < FishCount; i++)
	{
		float3 dif = (FishPosition[i].xyz - input.prePos.xyz / input.prePos.w) / 4.0f;
		float d = dot(dif, dif);
		dif = normalize(dif);

		lightSum += saturate(dot(dif, input.nor)) * FishColor[i].xyz *
			IntensityMultiplier / (d * GlowMultiplier + 1);
	}

	//return float4(normalize(input.nor), 1);
	return float4(lightSum * Color.xyz, Color.w);
}