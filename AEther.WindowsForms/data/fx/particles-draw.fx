
#include "states.fxi"
#include "globals.fxi"

cbuffer CameraConstants : register(b1)
{
	float4x4 View;
	float4x4 Projection;
};

StructuredBuffer<Particle> Particles : register(t0);
Texture2D<float4> Texture : register(t1);

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
	float4 Color : COLOR0;
};

PSin VS(const VSin IN)
{

	float4 pos = float4(IN.Position, 1);

	Particle p = Particles[IN.ID];

	pos.xyz = p.Position.w * pos.xyz + p.Position.xyz;
	pos = mul(View, pos);
	pos = mul(Projection, pos);

	PSin OUT;
	OUT.Position = pos;
	OUT.Normal = IN.Normal;
	OUT.UV = IN.UV;
	OUT.Color = p.Color;
	return OUT;

}

float4 PS(const PSin IN) : SV_Target
{

	float4 textureColor = Texture.Sample(Linear, IN.UV);

	return IN.Color * textureColor;

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerBoth);
		SetDepthStencilState(DepthStencilDefault, 0);
		SetBlendState(BlendAlpha, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}