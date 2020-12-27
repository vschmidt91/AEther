
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

	float b = Target.Sample(Point, IN.UV);
	
	float Ax = -4 * Solution.Sample(Point, IN.UV);
	Ax += Solution.Sample(Point, IN.UV, int2(-1, 0));
	Ax += Solution.Sample(Point, IN.UV, int2(+1, 0));
	Ax += Solution.Sample(Point, IN.UV, int2(0, -1));
	Ax += Solution.Sample(Point, IN.UV, int2(0, +1));

	return b - Ax / (Scale * Scale);

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