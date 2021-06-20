
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"
#include "light.fxi"

float3 PS(const PSDefaultin IN) : SV_Target
{

	float D = pow(IN.UV.y, -2) - 1;

	float2 t = LightScale * (-1 + 2 * float2(IN.UV.x, 1));
	//float2 theta = ParameterToAngle(D, t);
	float2 theta = PI * (float2(IN.UV.x, 1) - .5);

	float3 rgb = AirlightScale * AirlightIntegral(theta, D, 256);
	return rgb;

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