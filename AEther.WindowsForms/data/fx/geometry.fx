
#include "states.fxi"
#include "globals.fxi"

cbuffer CameraConstants : register(b1)
{
	float4x4 View;
	float4x4 Projection;
};

#ifdef INSTANCING
StructuredBuffer<Instance> Instances : register(t0);
#else
cbuffer GeometryConstants : register(b2)
{
	Instance SingleInstance;
};
#endif

struct VSin
{
	float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORDS0;
#ifdef INSTANCING
	uint ID : SV_InstanceID;
#endif
};

struct PSin
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORDS0;
	float3 Color : COLOR0;
};

PSin VS(const VSin IN)
{

	float4 pos = float4(IN.Position, 1);

#ifdef INSTANCING
	Instance instance = Instances[IN.ID];
#else
	Instance instance = SingleInstance;
#endif
	float4x4 world = instance.World;
	float3 color = instance.Color;

	pos = mul(world, pos);
	pos = mul(View, pos);
	pos = mul(Projection, pos);

	float3 normal = IN.Normal;
	normal = mul((float3x3)world, normal);

	PSin OUT;
	OUT.Position = pos;
	OUT.Normal = normal;
	OUT.UV = IN.UV;
	OUT.Color = color;
	return OUT;

}

float4 PS(const PSin IN) : SV_Target
{

	return float4(IN.Color, 1);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerBoth);
		SetDepthStencilState(DepthStencilDefault, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}