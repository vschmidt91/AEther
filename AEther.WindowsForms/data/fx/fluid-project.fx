﻿
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Velocity : register(t0);
Texture2D<float> Pressure : register(t1);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float px = Pressure.Sample(PointBorder, IN.UV, int2(+1, 0)) - Pressure.Sample(PointBorder, IN.UV, int2(-1, 0));
	float py = Pressure.Sample(PointBorder, IN.UV, int2(0, +1)) - Pressure.Sample(PointBorder, IN.UV, int2(0, -1));

	float2 pg = 0.5 * float2(px, py);

	float4 v = Velocity.Sample(Point, IN.UV);
	
	v.xy -= pg;

	return v;

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