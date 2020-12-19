
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Velocity : register(t0);
Texture2D<float4> Spectrum0 : register(t1);
Texture2D<float4> Spectrum1 : register(t2);

float4 PS(const PSDefaultin IN) : SV_Target
{

	int2 idx = (int2)IN.Position.xy;

	float4 v = Velocity[idx];

	float speed = .25;
	if (.8 < IN.UV.x & abs(IN.UV.y - .5) < .03)
		v = float4(speed * float2(-1, cos(.7534 * T)), .5 + .5 * float2(sin(T), cos(T)));

	float2 c1 = float2(.5, .5);
	float2 d1 = normalize(IN.UV - c1);
	if (distance(IN.UV, c1) < .1)
	{
		v.xy -= min(0, dot(d1, v.xy)) * d1;
		v.zw = 0;
	}

	if (any(IN.UV < .1) | any(.9 < IN.UV))
		v = 0;
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

		SetVertexShader(CompileShader(vs_5_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_5_0, PS()));

	}
}