
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Spectrum0 : register(t0);
Texture2D<float4> Spectrum1 : register(t1);

float3x3 outer_product(float3 a, float3 b)
{
	return float3x3(
		a[0] * b[0], a[0] * b[1], a[0] * b[2], 
		a[1] * b[0], a[1] * b[1], a[1] * b[2], 
		a[2] * b[0], a[2] * b[1], a[2] * b[2]
	);
}

float4 PS(const PSDefaultin IN) : SV_Target
{

	float scale = -1.5;

	float3 z0;
	z0.xy = 2 * (IN.UV - .5);
	z0.z = sin(.3 * T);
	z0.z = 0;
	z0 *= 2;

	float3 z = z0;
	float3x3 I = {1, 0, 0, 0, 1, 0, 0, 0, 1};
	float3x3 J = I;
	float dz = 1;

	int n = 3;
	for (int i = 0; i < n; ++i)
	{

		// box fold
		z = 2 * clamp(z, -1, +1) - z;
		float3 s = lerp(-1, +1, abs(z) < 1);
		J = mul(J, float3x3(s[0], 0, 0, 0, s[1], 0, 0, 0, s[2]));

		// sphere fold
		float r2 = dot(z, z);
		float a = 1.0 / clamp(r2, 0.25, 1.0);
		z *= a;
		dz *= a;
		if (r2 < .25)
		{
			J *= a;
		}
		else if (r2 < 1)
		{
			J = mul(J, a * (I - 2 * a * outer_product(z, z)));
		}

		// affine
		z = scale * z + z0;
		dz = abs(scale) * dz + 1;
		J = scale * J + I;
 
	}

	if (IN.UV.x < .5)
	{
		dz = pow(abs(determinant(J)), 1/3);
	}
	float zn = length(z);
	float d = zn * log(zn) / abs(dz);
	float l = .2 * d;

	float2 uv = float2(l, .5);
	l = 3 * Spectrum0.Sample(Point, uv).b;

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