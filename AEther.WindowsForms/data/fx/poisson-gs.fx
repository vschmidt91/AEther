
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

	uint2 idx = (uint2)IN.Position.xy;

	float p = Solution[idx];

	float p2 = Scale * Scale * Target[idx];

	p2 -= Solution[idx + int2(-1, 0)];
	p2 -= Solution[idx + int2(+1, 0)];
	p2 -= Solution[idx + int2(0, -1)];
	p2 -= Solution[idx + int2(0, +1)];

	p2 *= -Omega / 4.0;

	p2 += (1 - Omega) * p;

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

		SetVertexShader(CompileShader(vs_5_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_5_0, PS()));

	}
}