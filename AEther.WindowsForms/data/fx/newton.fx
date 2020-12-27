
#include "states.fxi"
#include "globals.fxi"
#include "complex.fxi"

float2 p(float2 z)
{
	return cmul(cmul(z, z), z) - float2(1, 0);
}

float2 dp(float2 z)
{
	return 3 * cmul(z, z);
}

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 z0 = 2*(IN.UV - .5);

	float2 z = z0;
	float2 dz = 1;
	float a = 1 + sin(.12345 * T);

	float a1 = 1 + 0.3123 * T;
	float a2 = 2 - 0.3234 * T;
	float a3 = 3 + 0.3345 * T;

	//float a1 = 0;
	//float a2 = (1 / 3.0) * 2 * PI;
	//float a3 = (2 / 3.0) * 2 * PI;

	float2 r1 = float2(sin(a1), cos(a1));
	float2 r2 = float2(sin(a2), cos(a2));
	float2 r3 = float2(sin(a3), cos(a3));

	float2 p0 = -cmul(cmul(r1, r2), r3);
	float2 p1 = cmul(r1, r2)  + cmul(r1, r3) + cmul(r2, r3);
	float2 p2 = -(r1 + r2 + r3);
	float2 p3 = float2(1, 0);

	for (int i = 0; i < 8; ++i)
	{

		float2 pz = p0
			+ cmul(p1, z)
			+ cmul(p2, cmul(z, z))
			+ cmul(p3, cmul(cmul(z, z), z));

		float2 dpz = p1
			+ 2 * cmul(p2, z)
			+ 3 * cmul(p3, cmul(z, z));

		z -= a * cdiv(pz, dpz);

	}

	return float4(1 * length(z - r1), 1 * length(z - r2), 1 * length(z - r3), 1);

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