
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Velocity : register(t0);
Texture2D<float4> Spectrum0 : register(t1);
Texture2D<float4> Spectrum1 : register(t2);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 source = float2(.8, .5);
	float2 obstacle = float2(.5, .5);

	float4 v = Velocity.Sample(Point, IN.UV);

	if (distance(IN.UV, source) < .01)
	{
		float speed = 1.0;
		v = float4(speed * float2(-1, cos(.7534 * T)), .5 + .5 * float2(sin(T), cos(T)));
	}

	if (distance(IN.UV, obstacle) < .1)
	{
		float2 d1 = normalize(IN.UV - obstacle);
		v.xy -= dot(d1, v.xy) * d1;
		v.zw = 0;
	}

	//if (any(IN.UV < .05) | any(.95 < IN.UV))
	//	v = 0;
		//v.xy = abs(v.xy) * sign(.5 - IN.UV);

	return v;

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}