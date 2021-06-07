
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Histogram0 : register(t0);
Texture2D<float4> Histogram1 : register(t1);
Texture2D<float4> Spectrum0 : register(t2);
Texture2D<float4> Spectrum1 : register(t3);

cbuffer EffectConstants : register(b1)
{
	float HistogramShift;
};

float4 PS(const PSDefaultin IN) : SV_Target
{

	float scale = 1.3;

	float3 z0 = float3(2*(IN.UV - .5), sin(.1*T));

	float3 z = z0;
	float dz = 1;
	float a;
	float c = 1;

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

		c = min(c, abs(z.x));
 
	}

	float d = length(z) / abs(dz);
	//return c;
	//d = pow(d, .2);
	d = exp(-3 * (1 - d));

	float3 y = normalize(z);
	float b = -1 + abs(atan2(z.x, z.y) / 3.14);
	//float b = .5 + .5 * normalize(z.xy).y;
	b -= d;

	float4 l = Spectrum0.Sample(Linear, float2(b, 0));
	float4 r = Spectrum1.Sample(Linear, float2(b, 0));
	//float4 l = Histogram0.Sample(Linear, float2(b, HistogramShift - d));
	//float4 r = Histogram1.Sample(Linear, float2(b, HistogramShift - d));
	float4 v = lerp(l, r, IN.UV.x);

	float3 lab = exp(-d) * (1 - exp(-3 * length(v.rg))) * float3(1, normalize(z.xy));
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