
#include "states.fxi"
#include "globals.fxi"

Texture2D<float> Solution : register(t0);
Texture2D<float> Target : register(t1);

cbuffer EffectConstants : register(b3)
{
	bool UpdateEven;
	bool UpdateOdd;
	float Scale;
	float Omega;
};

float PS(const PSDefaultin IN) : SV_Target
{

	float p = Solution.Sample(Point, IN.UV);
	float p2 = Target.Sample(Point, IN.UV);

	p2 *= Scale * Scale;
	p2 -= Solution.Sample(Point, IN.UV, int2(-1, 0));
	p2 -= Solution.Sample(Point, IN.UV, int2(+1, 0));
	p2 -= Solution.Sample(Point, IN.UV, int2(0, -1));
	p2 -= Solution.Sample(Point, IN.UV, int2(0, +1));

	p2 *= -Omega / 4.0;

	p2 += (1 - Omega) * p;
	
	int2 idx = IN.Position.xy;
	uint parity = (idx.x + idx.y) & 1;
	if (parity == 0 & UpdateEven)
		p = p2;
	if (parity == 1 & UpdateOdd)
		p = p2;

	return p;

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