
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

	float2 q = (1 + p.y) * float2(sin(PI * p.x), cos(PI * p.x));
	float j = srcp(PI * length(p));
	float4 v = Source.Sample(Linear, Squash(q));
	float4 rgba = det(A) * j * Weight * v;
	rgba = abs(rgba);

	return clamp(rgba, -1e3, +1e3);

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