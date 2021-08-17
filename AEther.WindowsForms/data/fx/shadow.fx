
#include "states.fxi"
#include "globals.fxi"
#include "camera.fxi"
#include "light.fxi"

StructuredBuffer<Instance> Instances : register(t0);
cbuffer InstanceConstants : register(b3)
{
	Instance SingleInstance;
};

Texture2D<float4> ColorMap : register(t1);

struct VSin
{
	float3 Position : POSITION;
	float2 UV : TEXCOORDS;
	uint ID : SV_InstanceID;
};

struct PSin
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORDS;
	float Depth : DEPTH;
};

PSin VS(const VSin IN)
{

#ifdef ENABLE_INSTANCING
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
	OUT.UV = IN.UV;
	OUT.Depth = distance(LightPosition, worldPos.xyz) / LightFarPlane;
	return OUT;

}

float PS(const PSin IN) : SV_DEPTH
{

	float4 color = ColorMap.Sample(Linear, IN.UV);

	//if (color.a == 0) discard;

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