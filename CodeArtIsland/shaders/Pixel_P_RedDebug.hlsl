float4 main(float4 position : SV_POSITION) : SV_TARGET
{
   return float4(1.0, position.x / 1280.0f, sin(position.y / 10.0f + position.x / 20.0f), 1.0);
}