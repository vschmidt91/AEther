
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"
#include "light.fxi"
#include "brdf.fxi"

#define NumSamples 32
#define Dithering 1
#define ShadowBias 1e-3

Texture2D<float> Depth : register(t0);
Texture2D<float4> Normal : register(t1);
Texture2D<float4> Color : register(t2);
TextureCube<float> Shadow : register(t3);
Texture2D<float3> Airlight : register(t4);

struct VSin
{
	float3 Position : POSITION;
};

struct PSin
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORDS;
	float3 ViewDirection : POSITION;
};

PSin VS(const VSin IN)
{

	PSin OUT;
	OUT.Position = float4(IN.Position.xy, 0, 1);
	OUT.UV = float2(.5 + .5 * IN.Position.x, .5 - .5 * IN.Position.y);
	OUT.ViewDirection = mul(float3(OUT.UV, 1), (float3x3)ViewDirectionMatrix);
	return OUT;

}

float3 PS(const PSin IN) : SV_TARGET
{

	//return Airlight.Sample(Linear, IN.UV).rgb;

	int3 index = (int3)float3(IN.Position.xy, 0);
	float depth = FarPlane * Depth.Load(index);
	float4 normal = Normal.Load(index);
	float4 color = Color.Load(index);
	float3 V = normalize(IN.ViewDirection);
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
	//float3 brdfSpecular = BRDFref(L, -V, N, specular, roughness);
	float3 brdfSpecular = specular / PI;

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

#ifdef ENABLE_SHADOWS


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

		float3 Lvi = 1;
		Lvi *= PhaseHG(sin(thetai), Anisotropy);
		Lvi *= step(ri, si);

#ifdef EQUIANGULAR
		Lvi *= exp(-(ri + ti) * Extinction);
#else
		Lvi /= ri2;
		Lvi *= exp(-ri * Extinction);
		Lvi *= exp(-ti * (Extinction - Extinction.r));
#endif

		Lv += Lvi;

	}

#ifdef EQUIANGULAR
	Lv *= theta[1] - theta[0];
	Lv /= D;
#else
	Lv *= 1 - exp(-Extinction.r * depth);
	Lv /= Extinction.r;
#endif

	Lv *= Scattering;
	Lv /= NumSamples;

#else


	float2 airlightX = .5 + theta / PI;
	//float2 airlightX = .5 + .5 * (float2(0, depth) - tm) / LightScale;
	float airlightY = rsqrt(1 + D);

	Lv += Airlight.Sample(Linear, float2(airlightX[0], airlightY));
	Lv -= Airlight.Sample(Linear, float2(airlightX[1], airlightY));
	Lv /= AirlightScale;

	//Lv = AirlightIntegral(theta, D, 32);

	Lv *= exp(-tm * Extinction);
	Lv /= D;

#endif

	return LightIntensity * (Ls + Lv);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}