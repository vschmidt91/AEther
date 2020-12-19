
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);
Texture2D<float4> Spectrum0 : register(t1);
Texture2D<float4> Spectrum1 : register(t2);

cbuffer Effect : register(b2)
{
	float4 Weight;
	float4 Transform;
	float2 Offset;
};

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = Stretch(IN.UV);

	float2x2 A = (float2x2)Transform;
	p = mul(A, p + Offset);

	float r2 = dot(p, p);
	float theta = atan2(p.y, p.x);
	float d = 1 - 4 * p.x * p.x * p.y * p.y;
	clip(d);
	float a = sqrt(d);

	float2 q;
	float4 v;
	float j;
	float4 rgba = 0;
	
	float div = srcp(2 * p.x);

	q = float2((1 + a) * div, p.y);
	v = Source.Sample(Linear, Squash(q));
	r2 = dot(q, q);
	theta = atan2(q.y, q.x);
	j = cos(2 * theta) * srcp(r2);
	rgba += det(A) * j * Weight * v;

	q = float2((1 - a) * div, p.y);
	v = Source.Sample(Linear, Squash(q));
	r2 = dot(q, q);
	theta = atan2(q.y, q.x);
	j = cos(2 * theta) * srcp(r2);
	rgba += det(A) * j * Weight * v;
	rgba = abs(rgba);
	
	return clamp(rgba, -rcp(EPSILON), +rcp(EPSILON));

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0_level_9_3, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0_level_9_3, PS()));

	}
}