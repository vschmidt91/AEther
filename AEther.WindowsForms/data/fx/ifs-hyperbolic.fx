
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

	float r2 = dot(p, p);
	float theta = atan2(p.y, p.x);
	float d = 1 - 4 * p.x * p.x * p.y * p.y;
	clip(d);
	float a = sqrt(d);

	float2 q;
	float4 v;
	float j;
	float4 w = 0;
	
	float div = srcp(2 * p.x);

	q = float2((1 + a) * div, p.y);
	v = Source.Sample(Linear, Squash(q));
	r2 = dot(q, q);
	theta = atan2(q.y, q.x);
	j = cos(2 * theta) * srcp(r2);
	w += v * float4(Weight.rgb, abs(j));

	q = float2((1 - a) * div, p.y);
	v = Source.Sample(Linear, Squash(q));
	r2 = dot(q, q);
	theta = atan2(q.y, q.x);
	j = cos(2 * theta) * srcp(r2);
	w += v * float4(Weight.rgb, abs(j));

	w /= 2;

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