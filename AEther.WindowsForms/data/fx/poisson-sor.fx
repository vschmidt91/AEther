
#include "states.fxi"
#include "globals.fxi"
#include "fluid-boundary.fxi"

Texture2D<float> Solution : register(t0);
Texture2D<float> Target : register(t1);

cbuffer EffectConstants : register(b1)
{
	bool UpdateEven;
	bool UpdateOdd;
	float3 Coefficients;
	float Omega;
};

float PS(const PSDefaultin IN) : SV_Target
{

	int parity = (int)dot(IN.Position.xy, 1) & 1;
	float update = lerp(UpdateEven, UpdateOdd, parity);

	float3 c = float3
	(
		Solution.Sample(Point, IN.UV, int2(-1, 0)) + Solution.Sample(Point, IN.UV, int2(+1, 0)),
		Solution.Sample(Point, IN.UV, int2(0, -1)) + Solution.Sample(Point, IN.UV, int2(0, +1)),
		Target.Sample(Point, IN.UV)
	);
	float p1 = Solution.Sample(Point, IN.UV);
	float p2 = dot(c, Coefficients);
	float p3 = lerp(p1, p2, Omega);
	float p4 = lerp(p1, p3, update);

	return p4;

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