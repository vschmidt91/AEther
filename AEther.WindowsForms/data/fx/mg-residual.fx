
#include "states.fxi"
#include "globals.fxi"

Texture2D<float> Solution : register(t0);
Texture2D<float> Target : register(t1);

cbuffer EffectConstants : register(b3)
{
	float Scale;
};

float PS(const PSDefaultin IN) : SV_Target
{

	uint2 idx = (uint2)IN.Position.xy;

	float b = Target[idx];
	
	float Ax = -4 * Solution[idx];
	Ax += Solution[idx + int2(-1, 0)];
	Ax += Solution[idx + int2(+1, 0)];
	Ax += Solution[idx + int2(0, -1)];
	Ax += Solution[idx + int2(0, +1)];

	return b - Ax / (Scale * Scale);

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