
#include "states.fxi"
#include "globals.fxi"

Texture2D<float4> Histogram0 : register(t0);
Texture2D<float4> Histogram1 : register(t1);

float4 PS(const PSDefaultin IN) : SV_Target
{

	float d = 2 * (IN.UV.x - .5);
	float4 v = 0;
	float2 uv = float2(1 - IN.UV.y, HistogramShift - abs(d));
	//float2 uv = float2(1 - IN.UV.y, abs(d));
	if(d < 0)
	{
		v = Histogram0.Sample(Point, uv);
	}
	else
	{
		v = Histogram1.Sample(Point, uv);
	}

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