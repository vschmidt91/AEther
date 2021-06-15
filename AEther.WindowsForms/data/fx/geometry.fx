
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"

StructuredBuffer<Instance> Instances : register(t0);
cbuffer GeometryConstants : register(b2)
{
	Instance SingleInstance;
};

Texture2D<float4> ColorMap : register(t1);

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
	float4 Normal : NORMAL0;
	float2 UV : TEXCOORDS0;
	float4 Color : COLOR0;
	float3 WorldPosition : POSITION0;
};

struct PSout
{
	float Depth : SV_DEPTH;
	float4 Normal : SV_Target0;
	float4 Color : SV_Target1;
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
	float4 viewPos = mul(View, worldPos);
	float4 clipPos = mul(Projection, viewPos);

	float3 normal = mul((float3x3)instance.World, IN.Normal);

	PSin OUT;
	OUT.Position = clipPos;
	OUT.Normal.xyz = normal;
	OUT.Normal.w = 0;
	OUT.UV = IN.UV;
	OUT.Color = instance.Color;
	OUT.WorldPosition = worldPos.xyz;
	return OUT;

}

PSout PS(const PSin IN)
{

	float4 color = ColorMap.Sample(Linear, IN.UV);

	PSout OUT;
	OUT.Depth = distance(IN.WorldPosition, ViewPosition) / FarPlane;
	OUT.Normal.xyz = normalize(IN.Normal.xyz);
	OUT.Normal.w = IN.Normal.w;
	OUT.Color = IN.Color * color;
	return OUT;

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