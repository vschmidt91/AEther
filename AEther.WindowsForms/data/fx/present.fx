
#include "states.fxi"
#include "globals.fxi"

Texture2D<float3> Light : register(t0);

float3 MapOctahedron(float2 uv)
{
	float d = dot(abs(uv), 1);
	float2 p = lerp(uv, sign(uv) * (1 - abs(uv)), d > 1);
	return float3(p, 1 - d);
}

float3 ToneMap(float3 rgb)
{
	return rgb / (1 + rgb);
}

float4 PS(const PSDefaultin IN) : SV_Target
{

	float4 seed = float4(T, T, IN.UV);
	float3 light = Light.Sample(Point, IN.UV);
	float3 rgb = ToneMap(light) + Dither4(seed) / 256;
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