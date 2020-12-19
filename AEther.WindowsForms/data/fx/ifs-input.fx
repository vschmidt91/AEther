
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Source : register(t0);
Texture2D<float4> Spectrum0 : register(t1);
Texture2D<float4> Spectrum1 : register(t2);

cbuffer Effect : register(b2)
{
	float4 Weight;
};

float4 PS(const PSDefaultin IN) : SV_Target
{

	//return 1;
	
	float2 p = Stretch(IN.UV);

	float f = .5 - .5 * normalize(p).y;
	float d = length(IN.UV - .5);
	float lr = IN.UV.x;
	
	float2 uv = float2(f, HistogramShift - .25 * d);
	float4 l = Spectrum0.Sample(Linear, uv);
	float4 r = Spectrum1.Sample(Linear, uv);
	float4 s = lerp(l, r, lr);

	return s * length(p);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0_level_9_3, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0_level_9_3, PS()));

	}
}