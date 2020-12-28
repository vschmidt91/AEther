
#include "states.fxi"
#include "globals.fxi"

Texture2D<float> Target : register(t0);

cbuffer EffectConstants : register(b3)
{
	float Scale;
	float Omega;
};

float PS(const PSDefaultin IN) : SV_Target
{

	return -.25 * Omega * Scale * Scale * Target.Sample(Point, IN.UV);

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