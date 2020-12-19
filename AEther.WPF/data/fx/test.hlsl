sampler2D left: register(s0);
sampler2D right: register(s1);

static const float Ri = 0.3;
static const float Ro = 0.7;
static const float Gamma = 2.2;
 
float4 main(float2 uv:TEXCOORD) : COLOR
{

    /*
    float2 z = 2 * uv - 1;
    float dz = 1;

    for(int i = 0; i < 6; ++i)
    {

        z = 2 * clamp(z, -1, +1) - z;

        float a = clamp(dot(z, z), Ri*Ri, Ro*Ro);
        z /= a;
        dz /= a;

    }

    float d = 0.1 * log(1+abs(dz));
    return float4(d, d, d, 1.0);
    */

	float d = 2 * (uv.x - .5);
	float4 v = 0;
	if(d < 0)
    {
		v = tex2D(left, float2(1 - uv.y, 1 + d));
    }
	else
    {
		v = tex2D(right, float2(1 - uv.y, 1 - d));
    }

    float3 rgb = v.rgb;
    float4 pix = float4(pow(abs(rgb), Gamma), 1);
    return pix;

}