
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Spectrum0 : register(t0);
Texture2D<float4> Spectrum1 : register(t1);

float2 cmul(float2 a, float2 b)
{
	return float2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
}

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 c = 2 * (IN.UV - .5);

	float2 z = c;
	float2 dz = float2(1, 0);

	int i = 0;
	int n = 100;
	for (i = 0; i < n; ++i)
	{

		dz = 2 * cmul(z, dz) + 1;
		z = cmul(z, z) + c;
		if (length(z) > 10) break;
 
	}

	float zn = length(z);
	float d = zn * log(zn) / length(dz);
	// float d = (i + 1 - log(log(zn) / log(2)) / log(2)) / n;
	float l = (zn > 2) * exp(-15 * d);

	float3 lab = float3(l, 0, 0);
	float3 xyz = LABtoXYZ(lab);
	float3 rgb = XYZtoRGB(xyz);

	return float4(saturate(rgb), 1);

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