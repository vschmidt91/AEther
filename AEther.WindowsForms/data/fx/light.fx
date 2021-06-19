
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"
#include "light.fxi"
#include "brdf.fxi"

#define NumSamples 16
#define Dithering 1
#define ShadowBias 1e-3

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
	float3 farPos = mul(float3(IN.UV, 1), (float3x3)FarPosMatrix);
	float3 V = normalize(farPos - ViewPosition);
	float3 N = normalize(normal.xyz);
	float3 pos = ViewPosition + depth * V;
	float3 lightVector = LightPosition - pos;
	float lightDistance = length(lightVector);
	float lightDistanceInv = 1 / lightDistance;
	float3 L = lightDistanceInv * lightVector;
	float shadow = ShadowFarPlane * Shadow.Sample(Linear, -lightVector);
	bool isMetal = color.a == 1;
	float3 diffuse = lerp(color.rgb, 0, isMetal);
	float3 specular = lerp(1, color.rgb, isMetal);
	float roughness = pow(normal.a, 4);
	float3 brdfDiffuse = diffuse / PI;
	float3 brdfSpecular = BRDFref(L, -V, N, specular, roughness);

	float3 Ls = 1;
	Ls *= lightDistanceInv * lightDistanceInv;
	Ls *= step(lightDistance, shadow + ShadowBias);
	Ls *= saturate(dot(N, L));
	Ls *= lerp(brdfDiffuse, brdfSpecular, color.a);
	Ls *= exp(-depth * Extinction);

	float tm = dot(LightPosition - ViewPosition, V);
	float D = sqrt(LightDistance * LightDistance - tm * tm);
	float2 theta = ParameterToAngle(D, float2(0, depth) - tm);

	float3 Lv = 0;

	for (int i = 0; i < NumSamples; ++i)
	{

		float offset = Dithering * Dither4(float4(T, i, IN.UV));
		float p = (i + offset) / NumSamples;
#ifdef EQUIANGULAR
		float thetai = lerp(theta[0], theta[1], p);
		float ti = AngleToParameter(D, thetai) + tm;
#else
		float ti = ExtinctionSampleCDFinv(depth, p);
		float thetai = ParameterToAngle(D, ti - tm);
#endif
		float3 pi = ViewPosition + ti * V;
		float3 li = LightPosition - pi;
		float ri2 = dot(li, li);
		float ri = sqrt(ri2);
		float si = ShadowFarPlane * Shadow.Sample(Linear, -li);

		float3 Lvj = 1;
		Lvj *= PhaseHG(sin(thetai), Anisotropy);
		Lvj *= step(ri2, si * si);

		/*
		* 
		Lvj *= exp(-(ri + ti) * Extinction);
		Lvj /= ri2;

#ifdef EQUIANGULAR
		float lightDensity = D / ((theta[1] - theta[0]) * ri2);
		Lvj /= lightDensity;
#else
		float extinctionDensity = ExtinctionSamplePDF(depth, ti);
		Lvj /= extinctionDensity;
#endif

		*/

#ifdef EQUIANGULAR
		Lvj *= exp(-(ri + ti) * Extinction);
#else
		Lvj *= exp(-ri * Extinction);
		Lvj /= ri2;
#endif

		Lv += Lvj;

	}

#ifdef EQUIANGULAR
	Lv *= theta[1] - theta[0];
	Lv /= D;
#else
	Lv *= 1 - exp(-depth * Extinction);
	Lv /= Extinction;
#endif

	Lv *= Scattering;
	Lv /= NumSamples;

	return LightIntensity * (Ls + Lv);

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