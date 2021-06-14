
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
#ifdef INSTANCING
	uint ID : SV_InstanceID;
#endif
};

struct GSin
{
	float3 Position : POSITION0;
};

struct PSin
{
	float4 Position : SV_POSITION;
	float Depth : DEPTH;
};

GSin VS(const VSin IN)
{

#ifdef INSTANCING
	Instance instance = Instances[IN.ID];
#else
	Instance instance = SingleInstance;
#endif

	float4 pos = float4(IN.Position, 1);
	float3 worldPos = mul(instance.World, pos).xyz;
	float3 eye = worldPos - ViewPosition;
#ifdef DIRECTIONAL_LIGHT
	float3 rectPos = RectifyDirectionalLight(normalize(eye), length(eye), FarPlane);
#else
	float3 rectPos = RectifyPointLight(normalize(eye), length(eye));
#endif

	GSin OUT;
	OUT.Position = rectPos;
	return OUT;

}

[maxvertexcount(6)]
void GS(triangle GSin IN[3], inout TriangleStream<PSin> OUT)
{

	PSin vertex = (PSin)0;

	float3 sy = float3(IN[0].Position.y, IN[1].Position.y, IN[2].Position.y);

	if (any(sy < -.8) && any(sy > +.8))
	{

		int i;
		for (i = 0; i < 3; ++i)
		{
			float4 pos = float4(IN[i].Position, 1);
			pos.y = pos.y - sign(pos.y) + 1;
			vertex.Position = float4(pos.x, -pos.y, pos.zw);
			vertex.Depth = pos.z;
			OUT.Append(vertex);
		}

		OUT.RestartStrip();

		for (i = 0; i < 3; ++i)
		{
			float4 pos = float4(IN[i].Position, 1);
			pos.y = pos.y - sign(pos.y) - 1;
			vertex.Position = float4(pos.x, -pos.y, pos.zw);
			vertex.Depth = pos.z;
			OUT.Append(vertex);
		}

		OUT.RestartStrip();

	}
	else
	{

		for (int i = 0; i < 3; ++i)
		{
			float4 pos = float4(IN[i].Position, 1);
			vertex.Position = float4(pos.x, -pos.y, pos.zw);
			vertex.Depth = pos.z;
			OUT.Append(vertex);
		}

		OUT.RestartStrip();

	}

}

float PS(const PSin IN) : SV_Target0
{

	return IN.Depth;

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerBoth);
		SetDepthStencilState(DepthStencilDefault, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(CompileShader(gs_4_0, GS()));
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}