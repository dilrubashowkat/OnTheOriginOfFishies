#define ITERATIONS 64
#define STEP_SIZE 4.0f

Texture2D Tex : register(t0);

cbuffer FishBuffer : register(b3)
{
	float2 CentrePoint;
	float CutOff;
	float Intensity;
};

struct VSOut
{
	float4 pos : SV_POSITION;
	float4 tex : TEXCOORD0;
};

float4 main(VSOut input) : SV_TARGET
{
	float2 p = input.pos.xy;
	//p.x += tan(p.x * 10.0f + p.y * 130.4f) * CutOff;
	//p.y += tan(CutOff);
	//p.y += cos(p.y * 15.0f) * 10.0f;
	float3 col = Tex.Load(int3(p.xy, 0)).xyz;
	float2 dir = normalize(CentrePoint - input.pos.xy);

	for (int i = 0; i < ITERATIONS; i++)
	{
		p += dir * STEP_SIZE;
		float4 lc = Tex.Load(int3(p, 0));
		col += lc.w * min(float3(0.6, 0.6, 0.6), max(float3(0, 0, 0), lc.xyz - CutOff) * Intensity * ((float)(ITERATIONS - i) / ITERATIONS));
	}

	/*for (int i = 0; i < ITERATIONS; i++)
	{
		col += Tex.Load(int3(p.x + i * STEP_SIZE, p.y, 0)).xyz * 0.04f * ((float)(ITERATIONS - i) / ITERATIONS);
		col += Tex.Load(int3(p.x - i * STEP_SIZE, p.y, 0)).xyz * 0.04f * ((float)(ITERATIONS - i) / ITERATIONS);
	}*/

	return float4(col, 1);
}