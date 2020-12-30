
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = 2 * IN.UV - 1;
	float2 q = 3 * p * float2(1, 1 / AspectRatio);

	float4 v = Source.Sample(Linear, Squash(q));
	//v = Source.Sample(Linear, IN.UV);

	float4 dither = Dither4(float4(IN.UV, IN.UV + 1) + frac(T));

	float3 rgb = .1 * v.xyz * log(1 + v.w) + dither.rgb / 256;

	return float4(rgb, 1);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);
		
		SetVertexShader(CompileShader(vs_4_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}