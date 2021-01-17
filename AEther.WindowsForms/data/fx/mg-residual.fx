
#include "states.fxi"
#include "globals.fxi"

Texture2D<float> Solution : register(t0);
Texture2D<float> Target : register(t1);

cbuffer EffectConstants : register(b1)
{
	float2 Coefficients;
};

float PS(const PSDefaultin IN) : SV_Target
{

	float b = Target.Sample(Point, IN.UV);
	float u = Solution.Sample(Point, IN.UV);

	float2 c = -2 * u + float2
	(
		Solution.Sample(Point, IN.UV, int2(-1, 0)) + Solution.Sample(Point, IN.UV, int2(+1, 0)),
		Solution.Sample(Point, IN.UV, int2(0, -1)) + Solution.Sample(Point, IN.UV, int2(0, +1))
	);

	float r = b - dot(c, Coefficients);

	return r;

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