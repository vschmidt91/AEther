//
// FX Version: fx_5_0
//
// 2 local buffer(s)
//
cbuffer FrameConstants : register(b0)
{
    float   T;                          // Offset:    0, size:    4
    float   DT;                         // Offset:    4, size:    4
    float   HistogramShift;             // Offset:    8, size:    4
    float   AspectRatio;                // Offset:   12, size:    4
}

cbuffer EffectConstants : register(b1)
{
    bool    UpdateEven;                 // Offset:    0, size:    4
    bool    UpdateOdd;                  // Offset:    4, size:    4
    float3  Coefficients;               // Offset:   16, size:   12
    float   Omega;                      // Offset:   28, size:    4
}

//
// 17 local object(s)
//
RasterizerState RasterizerDefault
{
    FillMode = uint(SOLID /* 3 */);
    CullMode = uint(BACK /* 3 */);
};
RasterizerState RasterizerBoth
{
    FillMode = uint(SOLID /* 3 */);
    CullMode = uint(NONE /* 1 */);
};
RasterizerState RasterizerBack
{
    FillMode = uint(SOLID /* 3 */);
    CullMode = uint(FRONT /* 2 */);
};
DepthStencilState DepthStencilNone
{
    DepthEnable = bool(FALSE /* 0 */);
    StencilEnable = bool(FALSE /* 0 */);
};
DepthStencilState DepthStencilDefault
{
    DepthEnable = bool(TRUE /* 1 */);
    DepthFunc = uint(LESS /* 2 */);
    DepthWriteMask = uint(ALL /* 1 */);
    StencilEnable = bool(FALSE /* 0 */);
};
DepthStencilState DepthStencilDefaultReadOnly
{
    DepthEnable = bool(TRUE /* 1 */);
    DepthFunc = uint(LESS /* 2 */);
    DepthWriteMask = uint(ZERO /* 0 */);
    StencilEnable = bool(FALSE /* 0 */);
};
BlendState BlendAdditive
{
    SrcBlend[0] = uint(ONE /* 2 */);
    SrcBlend[1] = uint(ONE /* 2 */);
    SrcBlend[2] = uint(ONE /* 2 */);
    SrcBlend[3] = uint(ONE /* 2 */);
    SrcBlend[4] = uint(ONE /* 2 */);
    SrcBlend[5] = uint(ONE /* 2 */);
    SrcBlend[6] = uint(ONE /* 2 */);
    SrcBlend[7] = uint(ONE /* 2 */);
    DestBlend[0] = uint(ONE /* 2 */);
    DestBlend[1] = uint(ONE /* 2 */);
    DestBlend[2] = uint(ONE /* 2 */);
    DestBlend[3] = uint(ONE /* 2 */);
    DestBlend[4] = uint(ONE /* 2 */);
    DestBlend[5] = uint(ONE /* 2 */);
    DestBlend[6] = uint(ONE /* 2 */);
    DestBlend[7] = uint(ONE /* 2 */);
    BlendOp[0] = uint(ADD /* 1 */);
    BlendOp[1] = uint(ADD /* 1 */);
    BlendOp[2] = uint(ADD /* 1 */);
    BlendOp[3] = uint(ADD /* 1 */);
    BlendOp[4] = uint(ADD /* 1 */);
    BlendOp[5] = uint(ADD /* 1 */);
    BlendOp[6] = uint(ADD /* 1 */);
    BlendOp[7] = uint(ADD /* 1 */);
    SrcBlendAlpha[0] = uint(ONE /* 2 */);
    SrcBlendAlpha[1] = uint(ONE /* 2 */);
    SrcBlendAlpha[2] = uint(ONE /* 2 */);
    SrcBlendAlpha[3] = uint(ONE /* 2 */);
    SrcBlendAlpha[4] = uint(ONE /* 2 */);
    SrcBlendAlpha[5] = uint(ONE /* 2 */);
    SrcBlendAlpha[6] = uint(ONE /* 2 */);
    SrcBlendAlpha[7] = uint(ONE /* 2 */);
    DestBlendAlpha[0] = uint(ONE /* 2 */);
    DestBlendAlpha[1] = uint(ONE /* 2 */);
    DestBlendAlpha[2] = uint(ONE /* 2 */);
    DestBlendAlpha[3] = uint(ONE /* 2 */);
    DestBlendAlpha[4] = uint(ONE /* 2 */);
    DestBlendAlpha[5] = uint(ONE /* 2 */);
    DestBlendAlpha[6] = uint(ONE /* 2 */);
    DestBlendAlpha[7] = uint(ONE /* 2 */);
    BlendOpAlpha[0] = uint(ADD /* 1 */);
    BlendOpAlpha[1] = uint(ADD /* 1 */);
    BlendOpAlpha[2] = uint(ADD /* 1 */);
    BlendOpAlpha[3] = uint(ADD /* 1 */);
    BlendOpAlpha[4] = uint(ADD /* 1 */);
    BlendOpAlpha[5] = uint(ADD /* 1 */);
    BlendOpAlpha[6] = uint(ADD /* 1 */);
    BlendOpAlpha[7] = uint(ADD /* 1 */);
    BlendEnable[0] = bool(TRUE /* true */);
    BlendEnable[1] = bool(TRUE /* true */);
    BlendEnable[2] = bool(TRUE /* true */);
    BlendEnable[3] = bool(TRUE /* true */);
    BlendEnable[4] = bool(TRUE /* true */);
    BlendEnable[5] = bool(TRUE /* true */);
    BlendEnable[6] = bool(TRUE /* true */);
    BlendEnable[7] = bool(TRUE /* true */);
};
BlendState BlendAlpha
{
    SrcBlend[0] = uint(SRC_ALPHA /* 5 */);
    SrcBlend[1] = uint(SRC_ALPHA /* 5 */);
    SrcBlend[2] = uint(SRC_ALPHA /* 5 */);
    SrcBlend[3] = uint(SRC_ALPHA /* 5 */);
    SrcBlend[4] = uint(SRC_ALPHA /* 5 */);
    SrcBlend[5] = uint(SRC_ALPHA /* 5 */);
    SrcBlend[6] = uint(SRC_ALPHA /* 5 */);
    SrcBlend[7] = uint(SRC_ALPHA /* 5 */);
    DestBlend[0] = uint(ONE /* 2 */);
    DestBlend[1] = uint(ONE /* 2 */);
    DestBlend[2] = uint(ONE /* 2 */);
    DestBlend[3] = uint(ONE /* 2 */);
    DestBlend[4] = uint(ONE /* 2 */);
    DestBlend[5] = uint(ONE /* 2 */);
    DestBlend[6] = uint(ONE /* 2 */);
    DestBlend[7] = uint(ONE /* 2 */);
    BlendOp[0] = uint(ADD /* 1 */);
    BlendOp[1] = uint(ADD /* 1 */);
    BlendOp[2] = uint(ADD /* 1 */);
    BlendOp[3] = uint(ADD /* 1 */);
    BlendOp[4] = uint(ADD /* 1 */);
    BlendOp[5] = uint(ADD /* 1 */);
    BlendOp[6] = uint(ADD /* 1 */);
    BlendOp[7] = uint(ADD /* 1 */);
    SrcBlendAlpha[0] = uint(SRC_ALPHA /* 5 */);
    SrcBlendAlpha[1] = uint(SRC_ALPHA /* 5 */);
    SrcBlendAlpha[2] = uint(SRC_ALPHA /* 5 */);
    SrcBlendAlpha[3] = uint(SRC_ALPHA /* 5 */);
    SrcBlendAlpha[4] = uint(SRC_ALPHA /* 5 */);
    SrcBlendAlpha[5] = uint(SRC_ALPHA /* 5 */);
    SrcBlendAlpha[6] = uint(SRC_ALPHA /* 5 */);
    SrcBlendAlpha[7] = uint(SRC_ALPHA /* 5 */);
    DestBlendAlpha[0] = uint(ONE /* 2 */);
    DestBlendAlpha[1] = uint(ONE /* 2 */);
    DestBlendAlpha[2] = uint(ONE /* 2 */);
    DestBlendAlpha[3] = uint(ONE /* 2 */);
    DestBlendAlpha[4] = uint(ONE /* 2 */);
    DestBlendAlpha[5] = uint(ONE /* 2 */);
    DestBlendAlpha[6] = uint(ONE /* 2 */);
    DestBlendAlpha[7] = uint(ONE /* 2 */);
    BlendOpAlpha[0] = uint(ADD /* 1 */);
    BlendOpAlpha[1] = uint(ADD /* 1 */);
    BlendOpAlpha[2] = uint(ADD /* 1 */);
    BlendOpAlpha[3] = uint(ADD /* 1 */);
    BlendOpAlpha[4] = uint(ADD /* 1 */);
    BlendOpAlpha[5] = uint(ADD /* 1 */);
    BlendOpAlpha[6] = uint(ADD /* 1 */);
    BlendOpAlpha[7] = uint(ADD /* 1 */);
    BlendEnable[0] = bool(TRUE /* true */);
    BlendEnable[1] = bool(TRUE /* true */);
    BlendEnable[2] = bool(TRUE /* true */);
    BlendEnable[3] = bool(TRUE /* true */);
    BlendEnable[4] = bool(TRUE /* true */);
    BlendEnable[5] = bool(TRUE /* true */);
    BlendEnable[6] = bool(TRUE /* true */);
    BlendEnable[7] = bool(TRUE /* true */);
};
BlendState BlendAlphaInv
{
    SrcBlend[0] = uint(ONE /* 2 */);
    SrcBlend[1] = uint(ONE /* 2 */);
    SrcBlend[2] = uint(ONE /* 2 */);
    SrcBlend[3] = uint(ONE /* 2 */);
    SrcBlend[4] = uint(ONE /* 2 */);
    SrcBlend[5] = uint(ONE /* 2 */);
    SrcBlend[6] = uint(ONE /* 2 */);
    SrcBlend[7] = uint(ONE /* 2 */);
    DestBlend[0] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlend[1] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlend[2] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlend[3] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlend[4] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlend[5] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlend[6] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlend[7] = uint(INV_SRC_ALPHA /* 6 */);
    BlendOp[0] = uint(ADD /* 1 */);
    BlendOp[1] = uint(ADD /* 1 */);
    BlendOp[2] = uint(ADD /* 1 */);
    BlendOp[3] = uint(ADD /* 1 */);
    BlendOp[4] = uint(ADD /* 1 */);
    BlendOp[5] = uint(ADD /* 1 */);
    BlendOp[6] = uint(ADD /* 1 */);
    BlendOp[7] = uint(ADD /* 1 */);
    SrcBlendAlpha[0] = uint(ONE /* 2 */);
    SrcBlendAlpha[1] = uint(ONE /* 2 */);
    SrcBlendAlpha[2] = uint(ONE /* 2 */);
    SrcBlendAlpha[3] = uint(ONE /* 2 */);
    SrcBlendAlpha[4] = uint(ONE /* 2 */);
    SrcBlendAlpha[5] = uint(ONE /* 2 */);
    SrcBlendAlpha[6] = uint(ONE /* 2 */);
    SrcBlendAlpha[7] = uint(ONE /* 2 */);
    DestBlendAlpha[0] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlendAlpha[1] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlendAlpha[2] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlendAlpha[3] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlendAlpha[4] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlendAlpha[5] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlendAlpha[6] = uint(INV_SRC_ALPHA /* 6 */);
    DestBlendAlpha[7] = uint(INV_SRC_ALPHA /* 6 */);
    BlendOpAlpha[0] = uint(ADD /* 1 */);
    BlendOpAlpha[1] = uint(ADD /* 1 */);
    BlendOpAlpha[2] = uint(ADD /* 1 */);
    BlendOpAlpha[3] = uint(ADD /* 1 */);
    BlendOpAlpha[4] = uint(ADD /* 1 */);
    BlendOpAlpha[5] = uint(ADD /* 1 */);
    BlendOpAlpha[6] = uint(ADD /* 1 */);
    BlendOpAlpha[7] = uint(ADD /* 1 */);
    BlendEnable[0] = bool(TRUE /* true */);
    BlendEnable[1] = bool(TRUE /* true */);
    BlendEnable[2] = bool(TRUE /* true */);
    BlendEnable[3] = bool(TRUE /* true */);
    BlendEnable[4] = bool(TRUE /* true */);
    BlendEnable[5] = bool(TRUE /* true */);
    BlendEnable[6] = bool(TRUE /* true */);
    BlendEnable[7] = bool(TRUE /* true */);
};
BlendState BlendNone
{
    BlendEnable[0] = bool(FALSE /* false */);
    BlendEnable[1] = bool(FALSE /* false */);
    BlendEnable[2] = bool(FALSE /* false */);
    BlendEnable[3] = bool(FALSE /* false */);
    BlendEnable[4] = bool(FALSE /* false */);
    BlendEnable[5] = bool(FALSE /* false */);
    BlendEnable[6] = bool(FALSE /* false */);
    BlendEnable[7] = bool(FALSE /* false */);
};
SamplerState Linear
{
    Filter   = uint(MIN_MAG_MIP_LINEAR /* 21 */);
    AddressU = uint(WRAP /* 1 */);
    AddressV = uint(WRAP /* 1 */);
};
SamplerState Point
{
    Filter   = uint(MIN_MAG_MIP_POINT /* 0 */);
    AddressU = uint(WRAP /* 1 */);
    AddressV = uint(WRAP /* 1 */);
};
SamplerState LinearBorder
{
    Filter   = uint(MIN_MAG_MIP_LINEAR /* 21 */);
    AddressU = uint(WRAP /* 1 */);
    AddressV = uint(WRAP /* 1 */);
};
SamplerState PointBorder
{
    Filter   = uint(MIN_MAG_MIP_POINT /* 0 */);
    AddressU = uint(WRAP /* 1 */);
    AddressV = uint(WRAP /* 1 */);
};
SamplerState Anisotropic
{
    Filter   = uint(ANISOTROPIC /* 85 */);
    AddressU = uint(WRAP /* 1 */);
    AddressV = uint(WRAP /* 1 */);
};
Texture2D Solution;
Texture2D Target;

//
// 1 groups(s)
//
fxgroup
{
    //
    // 1 technique(s)
    //
    technique11 t0
    {
        pass p0
        {
            RasterizerState = RasterizerDefault;
            DS_StencilRef = uint(0);
            DepthStencilState = DepthStencilNone;
            AB_BlendFactor = float4(0, 0, 0, 0);
            AB_SampleMask = uint(0xffffffff);
            BlendState = BlendNone;
            VertexShader = asm {
                //
                // Generated by Microsoft (R) HLSL Shader Compiler 10.1
                //
                //
                //
                // Input signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // POSITION                 0   xyz         0     NONE   float   xy  
                //
                //
                // Output signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_POSITION              0   xyzw        0      POS   float   xyzw
                // TEXCOORDS                0   xy          1     NONE   float   xy  
                //
                vs_4_0
                dcl_input v0.xy
                dcl_output_siv o0.xyzw, position
                dcl_output o1.xy
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; 
                //   o1.x <- <VSDefault return value>.UV.x; o1.y <- <VSDefault return value>.UV.y; 
                //   o0.x <- <VSDefault return value>.Position.x; o0.y <- <VSDefault return value>.Position.y; o0.z <- <VSDefault return value>.Position.z; o0.w <- <VSDefault return value>.Position.w
                //
                #line 204 "globals.fxi"
                mov o0.xy, v0.xyxx
                mov o0.zw, l(0,0,0,1.000000)
                
                #line 202
                mad o1.x, v0.x, l(0.500000), l(0.500000)
                mad o1.y, -v0.y, l(0.500000), l(0.500000)
                
                #line 204
                ret 
                // Approximately 5 instruction slots used
                            
            };
            GeometryShader = NULL;
            PixelShader = asm {
                //
                // Generated by Microsoft (R) HLSL Shader Compiler 10.1
                //
                //
                // Buffer Definitions: 
                //
                // cbuffer EffectConstants
                // {
                //
                //   bool UpdateEven;                   // Offset:    0 Size:     4
                //   bool UpdateOdd;                    // Offset:    4 Size:     4
                //   float3 Coefficients;               // Offset:   16 Size:    12
                //   float Omega;                       // Offset:   28 Size:     4
                //
                // }
                //
                //
                // Resource Bindings:
                //
                // Name                                 Type  Format         Dim      HLSL Bind  Count
                // ------------------------------ ---------- ------- ----------- -------------- ------
                // Point                             sampler      NA          NA             s0      1 
                // Solution                          texture   float          2d             t0      1 
                // Target                            texture   float          2d             t1      1 
                // EffectConstants                   cbuffer      NA          NA            cb1      1 
                //
                //
                //
                // Input signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_POSITION              0   xyzw        0      POS   float   xy  
                // TEXCOORDS                0   xy          1     NONE   float   xy  
                //
                //
                // Output signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_Target                0   x           0   TARGET   float   x   
                //
                ps_4_0
                dcl_constantbuffer CB1[2], immediateIndexed
                dcl_sampler s0, mode_default
                dcl_resource_texture2d (float,float,float,float) t0
                dcl_resource_texture2d (float,float,float,float) t1
                dcl_input_ps_siv linear noperspective v0.xy, position
                dcl_input_ps linear v1.xy
                dcl_output o0.x
                dcl_temps 4
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; v0.w <- IN.Position.w; 
                //   v1.x <- IN.UV.x; v1.y <- IN.UV.y; 
                //   o0.x <- <PS return value>
                //
                #line 20 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\poisson-sor.fx"
                dp2 r0.x, v0.xyxx, l(1.000000, 1.000000, 0.000000, 0.000000)
                ftoi r0.x, r0.x
                and r0.x, r0.x, l(1)  // r0.x <- parity
                
                #line 21
                itof r0.x, r0.x
                movc r0.yzw, cb1[0].xxyx, l(0,1.000000,1.000000,-1.000000), l(0,0,0,-0.000000)
                add r0.z, r0.w, r0.z
                mad r0.x, r0.x, r0.z, r0.y  // r0.x <- update
                
                #line 23
                sample_aoffimmi(-1,0,0) r1.xyzw, v1.xyxx, t0.xyzw, s0
                sample_aoffimmi(1,0,0) r2.xyzw, v1.xyxx, t0.xyzw, s0
                add r1.x, r1.x, r2.x  // r1.x <- c.x
                sample_aoffimmi(0,-1,0) r2.xyzw, v1.xyxx, t0.xyzw, s0
                sample_aoffimmi(0,1,0) r3.xyzw, v1.xyxx, t0.xyzw, s0
                add r1.y, r2.x, r3.x  // r1.y <- c.y
                sample r2.xyzw, v1.xyxx, t1.xyzw, s0  // r2.x <- c.z
                
                #line 30
                mov r1.z, r2.x
                dp3 r0.y, r1.xyzx, cb1[1].xyzx  // r0.y <- p2
                
                #line 29
                sample r1.xyzw, v1.xyxx, t0.xyzw, s0  // r1.x <- p1
                
                #line 31
                add r0.y, r0.y, -r1.x
                mul r0.y, r0.y, cb1[1].w
                
                #line 32
                mad o0.x, r0.x, r0.y, r1.x
                
                #line 34
                ret 
                // Approximately 21 instruction slots used
                            
            };
        }

    }

}

