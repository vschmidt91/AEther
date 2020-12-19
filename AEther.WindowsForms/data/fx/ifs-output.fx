
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);
Texture2D<float4> Spectrum0 : register(t1);
Texture2D<float4> Spectrum1 : register(t2);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = 2 * IN.UV - 1;
	float2 q = 2 * p * float2(1, 1 / AspectRatio);

	float4 v = Source.Sample(Linear, Squash(q));
	v = Uncompress(v);

	//float3 rgb = -log(1 + abs(v.xyz)) / log(EPSILON);
	
	float4 d = -log(1 + v) / log(EPSILON);
	d = pow(d, 3);
	float lr = IN.UV.x;

	float4 l = Spectrum0.Sample(Linear, float2(d.x, 0));
	float4 r = Spectrum1.Sample(Linear, float2(d.x, 0));
	float4 s = lerp(l, r, lr);

	float4 dither = Dither4(float4(IN.UV, IN.UV + 1) + frac(T));

	float4 rgba = d + dither / 256;
	rgba.a = 1;
	return rgba;

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);
		
		SetVertexShader(CompileShader(vs_4_0_level_9_3, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0_level_9_3, PS()));

	}
}