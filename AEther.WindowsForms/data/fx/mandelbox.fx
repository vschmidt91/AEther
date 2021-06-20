
#include "states.fxi"
#include "globals.fxi"

float4 PS(const PSDefaultin IN) : SV_Target
{

	float scale = 1.3;

	float3 z0;
	z0.xy = 2 * (IN.UV - .5);
	z0.z = sin(.1 * T);

	float3 z = z0;
	float dz = 1;
	float a;

	for (int i = 0; i < 7; ++i)
	{

		// box fold
		z = 2 * clamp(z, -1, +1) - z;

		// sphere fold
		a = 1.0 / clamp(dot(z, z), 0.25, 1.0);
		z *= a;
		dz *= a;

		// affine
		z = scale * z + z0;
		dz = scale * dz + 1;
 
	}

	float d = length(z) / abs(dz);
	float l = 1 - saturate(10 * d);
	float3 lab = float3(l, 0, 0);
	float3 xyz = LABtoXYZ(lab);
	float3 rgb = XYZtoRGB(xyz);

	return float4(saturate(rgb), l - .5);

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