struct VSOut
{
	float4 pos : SV_POSITION;
	float4 tex : TEXCOORD0;
};

VSOut main(uint id:SV_VERTEXID)
{
	VSOut output;

	output.pos.x = (float)(id / 2) * 4.0f - 1.0f;
	output.pos.y = (float)(id % 2) * 4.0f - 1.0f;
	output.pos.z = 0.0f;
	output.pos.w = 1.0f;

	output.tex.x = (float)(id / 2) * 2.0f;
	output.tex.y = 1.0 - (float)(id % 2) * 2.0f;

	return output;
}