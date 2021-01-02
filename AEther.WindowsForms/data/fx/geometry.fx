
#include "states.fxi"
#include "globals.fxi"

StructuredBuffer<Instance> Instances : register(t0);

struct VSin
{
	float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORDS0;
	uint ID : SV_InstanceID;
};

struct PSin
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORDS0;
	float3 Color : COLOR0;
};

cbuffer EffectConstants : register(b2)
{
	float4x4 View;
	float4x4 Projection;
	float4x4 World;
	float3 Color;
}

PSin VS(VSin IN)
{

	Instance instance = Instances[IN.ID];

	float4 pos = float4(IN.Position.xyz, 1);
	pos = mul(instance.World, pos);
	pos = mul(View, pos);
	pos = mul(Projection, pos);

	PSin OUT = (PSin)0;

	OUT.Normal = normalize(mul((float3x3)World, IN.Normal));
	OUT.Position = pos;
	OUT.UV = IN.UV;
	OUT.Color = instance.Color;

	return OUT;

}

float4 PS(PSin IN) : SV_Target
{

	return float4(IN.Color, 1);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerBoth);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}