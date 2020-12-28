
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Velocity : register(t0);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float dx = Velocity.Sample(PointBorder, IN.UV, int2(+1, 0)).x - Velocity.Sample(PointBorder, IN.UV, int2(-1, 0)).x;
	float dy = Velocity.Sample(PointBorder, IN.UV, int2(0, +1)).y - Velocity.Sample(PointBorder, IN.UV, int2(0, -1)).y;

	float div = .5 * (dx + dy);

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