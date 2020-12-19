
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);

float4 PS(PSDefaultin IN) : SV_Target
{

	return Source.Sample(Linear, IN.UV);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerBoth);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}