cbuffer ParamsBuffer : register(b2)
{
	float4 Color;
};

struct VSOut
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

float4 main(VSOut input) : SV_TARGET
{
   return input.col * Color;
}