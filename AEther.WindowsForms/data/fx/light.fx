
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"
#include "light.fxi"
#include "brdf.fxi"

#define NumSamples 4
#define Dithering 1
#define ShadowBias 1e-2

Texture2D<float> Depth : register(t0);
Texture2D<float4> Normal : register(t1);
Texture2D<float4> Color : register(t2);
TextureCube<float> Shadow : register(t3);

float3 PS(const PSDefaultin IN) : SV_TARGET
{

	int3 index = (int3)float3(IN.Position.xy, 0);

	float depth = FarPlane * Depth.Load(index);
	float4 normal = Normal.Load(index);
	float4 color = Color.Load(index);

	float3 farPos = TopLeft + IN.UV.x * HStep + IN.UV.y * VStep;
	float3 V = normalize(farPos - ViewPosition);
	float3 N = normalize(normal.xyz);
	float3 pos = ViewPosition + depth * V;

#ifdef DIRECTIONAL
	float3 L = -LightPosition;
#else
	float3 lightVector = LightPosition - pos;
	float lightDistance = length(lightVector);
	float lightDistanceInv = rcp(lightDistance);
	float3 L = normalize(lightVector);
	float shadow = ShadowFarPlane * Shadow.Sample(Linear, -lightVector);
#endif

	float3 Ls = 1;
#ifdef DIRECTIONAL
#else
	Ls *= lightDistanceInv * lightDistanceInv;
	Ls *= (lightDistance < shadow + ShadowBias);
#endif
	Ls *= saturate(dot(N, L));
	Ls *= color.rgb / PI;
	Ls *= exp(-depth * Extinction);

	float3 Lv = 0;

#ifdef VOLUMETRIC

#ifdef DIRECTIONAL
	float phaseConst = PhaseHG(dot(L, V), Anisotropy);
#else
	float tm = dot(LightPosition - ViewPosition, V);
	float D = sqrt(LightDistance * LightDistance - tm * tm);
	float2 theta = ParameterToAngle(D, float2(0, depth) - tm);
#endif

	float4 seed = float4(T, T, 12345 * IN.UV);
	float4 offset = (float4(0, 1, 2, 3) + Dithering * Dither4(seed)) / 4;
	for (int i = 0; i < NumSamples; ++i)
	{

		float4 p = (i + offset) / NumSamples;

#ifdef DIRECTIONAL
		float4 ts = DistanceSample(depth, p);
		float4 phases = phaseConst;
		float4 depths = ts;
#else
		float4 thetas = lerp(theta[0], theta[1], p);
		float4 ts = AngleToParameter(D, thetas);
		float4 phases = PhaseHG4(sin(thetas), Anisotropy);
		float4 depths = sqrt(D * D + ts * ts) + (tm + ts);
#endif

		[unroll(4)]
		for (int j = 0; j < 4; ++j)
		{
			float3 Lvj = 1;
			Lvj *= phases;
			Lvj *= exp(-depths * Extinction);

#ifdef DIRECTIONAL
#else
			float3 pj = ViewPosition + (tm + ts[j]) * V;
			float3 lj = LightPosition - pj;
			float shadowj = ShadowFarPlane * Shadow.Sample(Linear, -lj);
			Lvj *= (length(lj) < shadowj + ShadowBias);
#endif

			Lv += Lvj;
		}

	}

#ifdef DIRECTIONAL
	//Lv *= depth;
#else
	Lv /= D;
#endif

	Lv *= Scattering;
	Lv /= NumSamples;

#endif

	float3 Li = Ls + Lv;
	Li *= LightIntensity;

	return Li;

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