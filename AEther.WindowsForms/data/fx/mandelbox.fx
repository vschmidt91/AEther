
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Histogram0 : register(t0);
Texture2D<float4> Histogram1 : register(t1);
Texture2D<float4> Spectrum0 : register(t2);
Texture2D<float4> Spectrum1 : register(t3);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float scale = 1 + .01 * DS;

	float a1 = +.12346 * S + .01 * DS;
	float a2 = -.17457 * S + .01 * DS;
	float a3 = +.13456 * S + .01 * DS;
	float3x3 A = mul(mul(Rx(a1), Ry(a2)), Rz(a3));

	float3 z0 = float3(2*(IN.UV - .5), sin(.1*S));

	float3 z = z0;
	float dz = 1;
	float a;

	for (int i = 0; i < 5; ++i)
	{

		// box fold
		z = 2 * clamp(z, -1, +1) - z;

		// sphere fold
		a = 1.0 / clamp(dot(z, z), 0.25, 4.0);
		z *= a;
		dz *= a;

		// affine
		z = scale * mul(A, z) + z0;
		dz = scale * dz + 1;

	}

	float d = length(z) / abs(dz);
	//d = pow(d, .2);
	d = exp(-3 * (1 - d));

	float3 y = normalize(z);
	float b = 1 - 1.1 * abs(atan2(z.x, z.y) / 3.14);
	//float b = .5 + .5 * normalize(z.xy).y;

	float4 l = Spectrum0.Sample(Linear, float2(b, 0));
	float4 r = Spectrum1.Sample(Linear, float2(b, 0));
	//float4 l = Histogram0.Sample(Linear, float2(b, HistogramShift - d));
	//float4 r = Histogram1.Sample(Linear, float2(b, HistogramShift - d));
	float4 v = lerp(l, r, IN.UV.x);

	//return pow(v, 2.2);

	float3 lab = exp(-d) * (1 - exp(-3 * length(v))) * float3(1, normalize(z.xy));
	float3 xyz = LABtoXYZ(lab);
	float3 rgb = XYZtoRGB(xyz);

	//rgb = 1 - exp(-5 * v * d);
	//rgb = float4(b, d, 0, 1);
	//rgb = d;

	return float4(saturate(rgb), 1);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0_level_9_3, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0_level_9_3, PS()));

	}
}