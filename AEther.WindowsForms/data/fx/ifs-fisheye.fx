
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);

cbuffer EffectConstants : register(b1)
{
	float4 Weight;
};

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = Stretch(IN.UV);

	float r = length(p);
	float2 q = p.yx * srcp(2 - r);
	float j = 4 * srcp(pow(1 + r, 3));
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