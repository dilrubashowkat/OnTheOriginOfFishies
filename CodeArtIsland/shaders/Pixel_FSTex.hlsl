#define MAX_FISH_COUNT 16

Texture2D Tex : register(t0);

struct VSOut
{
	float4 pos : SV_POSITION;
	float4 tex : TEXCOORD0;
};

float4 main(VSOut input) : SV_TARGET
{
	return Tex.Load(int3(input.pos.xy, 0));
}