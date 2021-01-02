
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);

cbuffer EffectConstants : register(b2)
{
	float4 Weight;
};

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = Stretch(IN.UV);

	float r2 = sqrt(dot(p, p));
	float2x2 B = float2x2(sin(r2), cos(r2), -cos(r2), sin(r2));
	float2 q = mul(B, p);
	float j = 1;
	float4 v = Source.Sample(Linear, Squash(q));
	float4 w = v * float4(Weight.rgb, abs(j));

	return w;

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}