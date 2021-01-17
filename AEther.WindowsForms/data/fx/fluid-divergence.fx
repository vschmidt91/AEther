
#include "states.fxi"
#include "globals.fxi"
#include "fluid-boundary.fxi"

Texture2D<float4> Velocity : register(t0);

float PS(const PSDefaultin IN) : SV_Target
{

#ifdef FAST_DERIVATIVES
	float dx = ddx(Velocity.Sample(Point, IN.UV).x);
	float dy = ddy(Velocity.Sample(Point, IN.UV).y);
	float div = dx + dy;
#else
	float dx = Velocity.Sample(Point, IN.UV, int2(+1, 0)).x - Velocity.Sample(Point, IN.UV, int2(-1, 0)).x;
	float dy = Velocity.Sample(Point, IN.UV, int2(0, +1)).y - Velocity.Sample(Point, IN.UV, int2(0, -1)).y;
	float div = .5 * (dx + dy);
#endif

	div *= getBoundary(IN.UV);
	return div;

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