
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Velocity : register(t0);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float4 v = Velocity.Sample(Linear, IN.UV);

	float3 lab = float3(length(v.zw), v.zw);
	float3 xyz = LABtoXYZ(lab);
	float3 rgb = XYZtoRGB(xyz);
	return float4(rgb, 1);

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