
#include "states.fxi"
#include "globals.fxi"
#include "fluid-boundary.fxi"

#define KERNEL_SIZE 3

Texture2D<float4> Velocity : register(t0);

cbuffer EffectConstants : register(b1)
{
	float Variance;
};

float4 PS(const PSDefaultin IN) : SV_Target
{

	float4 v = 0;
	float w = 0;
	for (int dx = -KERNEL_SIZE; dx <= +KERNEL_SIZE; ++dx)
	{
		for (int dy = -KERNEL_SIZE; dy <= +KERNEL_SIZE; ++dy)
		{
			float f = dx * dx + dy * dy;
			f = exp(-.5 * f / Variance);
			v += f * Velocity.Sample(Point, IN.UV, int2(dx, dy));
			w += f;
		}
	}
	v /= w;

	v *= getBoundary(IN.UV);
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