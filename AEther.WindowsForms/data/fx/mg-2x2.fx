
#include "states.fxi"
#include "globals.fxi"

Texture2D<float> Target : register(t1);

cbuffer EffectConstants : register(b3)
{
	float Scale;
};

float PS(const PSDefaultin IN) : SV_Target
{

	int2 q = IN.Position.xy;

	float y = 0;
	for (int i = 0; i < 2; ++i)
	{
		for (int j = 0; j < 2; ++j)
		{
			int2 p = int2(i, j);
			bool2 pq = p == q;
			float w = -1 - dot(pq, 1) - 4 * pq.x * pq.y;
			y += w * Target[p];
		}
	}

	y *= Scale * Scale / 24;

	return y;

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