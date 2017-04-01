struct VSIn
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

struct VSOut
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

cbuffer TransformBuffer : register(b0)
{
    matrix World;
    matrix View;
	matrix Projection;
};

VSOut main(VSIn input)
{
	VSOut output;
	
	float4x4 worldViewProj = mul(World, mul(View, Projection));
	output.pos = mul(input.pos, worldViewProj);
	//output.pos = mul(input.pos, World);
	//output.pos = mul(output.pos, View);
	//output.pos = mul(output.pos, Projection);
	output.col = input.col;

	return output;
}