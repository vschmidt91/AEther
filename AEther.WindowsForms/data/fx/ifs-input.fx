
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Spectrum0 : register(t0);
Texture2D<float4> Spectrum1 : register(t1);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float2 p = Stretch(IN.UV);

	float f = .5 - .5 * normalize(p).y;
	float d = length(IN.UV - .5);
	float lr = IN.UV.x;

	float2 uv = float2(f, HistogramShift - .25 * d);
	float4 l = Spectrum0.Sample(Linear, uv);
	float4 r = Spectrum1.Sample(Linear, uv);
	float4 s = lerp(l, r, lr);

	return float4(s.rgb * rcp(length(p)), 1);

	//return float4(abs(normalize(p)), 0, 1);
	return float4(.03 * abs(rcp(p)), 0, 1);

}

technique11 t0
{
	pass p0
	{

		SetRasterizerState(RasterizerDefault);
		SetDepthStencilState(DepthStencilNone, 0);
		SetBlendState(BlendAdditive, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetVertexShader(CompileShader(vs_4_0, VSDefault()));
		SetGeometryShader(0);
		SetPixelShader(CompileShader(ps_4_0, PS()));

	}
}