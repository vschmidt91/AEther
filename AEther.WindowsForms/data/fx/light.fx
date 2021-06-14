
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"
#include "light.fxi"
#include "brdf.fxi"

#define NumSamples 32
#define Dithering 1

Texture2D<float> Depth : register(t0);
Texture2D<float4> Normal : register(t1);
Texture2D<float4> Color : register(t2);
Texture2D<float> Shadow : register(t3);

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

#ifdef DIRECTIONAL_LIGHT
	float3 rectPos = RectifyDirectionalLight(V, depth, FarPlane);
	float3 L = -LightPositionOrDirection;
#else
	float3 rectPos = RectifyPointLight(V, depth);
	float3 lightVector = LightPositionOrDirection - pos;
	float lightDistance = length(lightVector);
	float lightDistanceInv = rcp(lightDistance);
	float3 L = normalize(lightVector);
#endif

	float2 shadowUV = .5 + .5 * rectPos.xy;
	float shadow = Shadow.Sample(Linear, shadowUV);

	float3 diffuse = color.rgb;

	float3 Ls = 1;
#ifdef DIRECTIONAL_LIGHT
#else
	Ls *= lightDistanceInv * lightDistanceInv;
#endif
	Ls *= PSM(shadow, rectPos.z);
	//Ls *= saturate(dot(N, L));
	Ls *= diffuse / PI;
	Ls *= exp(-depth * Extinction);

	float3 Lv = 0;


#ifdef DIRECTIONAL_LIGHT
	float phaseConst = PhaseHG(dot(L, V), Anisotropy);
#else
	float tm = dot(LightPositionOrDirection - ViewPosition, V);
	float D = sqrt(LightDistance * LightDistance - tm * tm);
	float2 theta = ParameterToAngle(D, float2(0, depth) - tm);
#endif

	for (int i = 0; i < NumSamples; ++i)
	{

		float4 seed = float4(T, i, IN.UV);
		float offset = Dithering * Dither4(seed).x;
		float p = (i + offset) / NumSamples;

#ifdef DIRECTIONAL_LIGHT
		float tj = DistanceSample(depth, p);
		float phase = phaseConst;
		float depthj = tj;
#else
		float thetaj = lerp(theta[0], theta[1], p);
		float tj = AngleToParameter(D, thetaj);
		float phase = PhaseHG(sin(thetaj), Anisotropy);
		float depthj = sqrt(D * D + tj * tj) + (tm + tj);
#endif

		float2 shadowUVj = shadowUV * float2(p, 1);
		float shadowj = Shadow.Sample(Point, shadowUVj);

		float3 Lvj = 1;
		Lvj *= phase;
		Lvj *= exp(-max(0, depthj) * Extinction);
		Lvj *= PSM(shadowj, rectPos.z);
		Lv += Lvj;
	}

#ifdef DIRECTIONAL_LIGHT
	//Lv *= depth;
#else
	Lv /= D;
#endif

	Lv *= Scattering;
	Lv /= NumSamples;

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