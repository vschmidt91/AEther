
#include "states.fxi"
#include "globals.fxi"

Texture2D<float3> Light : register(t0);
Texture2D<float> Shadow : register(t1);

float3 ToneMap(float3 rgb)
{
	return rgb * rcp(1 + rgb);
}

float4 PS(const PSDefaultin IN) : SV_Target
{

	float3 light = Light.Sample(Point, IN.UV);
	float shadow = Shadow.Sample(Point, IN.UV);

	//return shadow;

	float4 seed = float4(T, DT, IN.UV);
	float3 dither = Dither4(seed).rgb;
	float3 rgb = ToneMap(light) + dither / 256;

	return float4(rgb, 1);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerBoth);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}