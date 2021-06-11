
#include "states.fxi"
#include "globals.fxi"

RWStructuredBuffer<Particle> Particles : register(u0);

[numthreads(1, 1, 1)]
void CS(uint3 index : SV_DispatchThreadID)
{
	uint i = index.x;
	Particle p = Particles[i];
	if (p.Lifetime <= 0)
	{
		p = (Particle)0;
		p.Position.xyz = 3 * cos(float3(6.54, 8.76, 3.42) * i);
		p.Position.w = 3 * abs(cos(2.644 * i));
		p.Momentum.xyz = 2 * normalize(cos(float3(456, 678, 234) * i));
		p.Momentum.w = 0;
		p.Acceleration.y = -1;
		p.Color = abs(cos(float4(1.321, 1.432, 1.543, 1.654) * i));
		p.Lifetime = 5 * (1 + abs(cos(3.876 * i)));
	}
	p.Momentum += DT * p.Acceleration;
	p.Position += DT * p.Momentum;
	p.Lifetime -= DT;
	Particles[i] = p;
}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendNone, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetComputeShader(CompileShader(cs_4_0, CS()));
		SetVertexShader(0);
		SetGeometryShader(0);
		SetPixelShader(0);

	}
}