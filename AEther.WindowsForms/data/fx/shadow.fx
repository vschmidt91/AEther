﻿
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"
#include "light.fxi"

StructuredBuffer<Instance> Instances : register(t0);
cbuffer GeometryConstants : register(b3)
{
	Instance SingleInstance;
};

struct VSin
{
	float3 Position : POSITION;
	uint ID : SV_InstanceID;
};

struct PSin
{
	float4 Position : SV_POSITION;
	float Depth : DEPTH;
};

PSin VS(const VSin IN)
{

#ifdef INSTANCING
	Instance instance = Instances[IN.ID];
#else
	Instance instance = SingleInstance;
#endif

	float4 pos = float4(IN.Position, 1);
	float4 worldPos = mul(instance.World, pos);
	float4 viewPos = mul(LightView, worldPos);
	float4 clipPos = mul(LightProjection, viewPos);

	PSin OUT;
	OUT.Position = clipPos;
	OUT.Depth = distance(LightPosition, worldPos.xyz) / ShadowFarPlane;
	return OUT;

}

float PS(const PSin IN) : SV_DEPTH
{

	return IN.Depth;

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilDefault, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}