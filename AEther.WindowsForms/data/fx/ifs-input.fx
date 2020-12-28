
#include "states.fxi"
#include "globals.fxi"

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = Stretch(IN.UV);

	//return float4(abs(normalize(p)), 0, 1);
	return float4(.03 * abs(rcp(p)), 0, 1);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}