
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Velocity : register(t0);
Texture2D<float> Pressure : register(t1);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float4 v = Velocity.Sample(Linear, IN.UV);
	float p = Velocity.Sample(Linear, IN.UV);

	//return .5 + 2.5 * p;
	return float4(v.z, v.w, 0, 1);

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