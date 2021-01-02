
#include "states.fxi"
#include "globals.fxi"
#include "fluid-boundary.fxi"

Texture2D<float4> Velocity : register(t0);
Texture2D<float4> Spectrum0 : register(t1);
Texture2D<float4> Spectrum1 : register(t2);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float f = .5 - .5 * normalize(IN.UV - .5).y;
	float lr = IN.UV.x;

	float2 uv = float2(f, .5);
	float4 l = Spectrum0.Sample(Linear, uv);
	float4 r = Spectrum1.Sample(Linear, uv);
	float4 s = lerp(l, r, lr);

	float4 v = Velocity.Sample(Point, IN.UV);

	/*
	v *= .99;
	v.xy += .03 * length(s) * normalize(IN.UV - .5);
	v.zw += .03 * max(0, s.rr);
	*/

	float2 source = float2(.8, .5);
	if (distance(IN.UV, source) < .02)
	{
		float speed = 0.5;
		v = float4(speed * float2(-1, cos(.5 * T)), float2(sin(T), cos(T)));
	}

	return setBoundary(IN.UV, v);

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