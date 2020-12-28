
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);

cbuffer Effect : register(b2)
{
	float4 Weight;
};

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = Stretch(IN.UV);
	
	float4 v;
	float2 q;
	float j;

	float p2 = dot(p, p);
	q = p * srcp(p2);
	j = srcp(p2 * p2);
	v = Source.Sample(Point, Squash(q));
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