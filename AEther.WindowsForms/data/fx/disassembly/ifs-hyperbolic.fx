Macros: //
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
    float4  Weight;                     // Offset:    0, size:   16
}

//
// 16 local object(s)
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
    AddressU = uint(CLAMP /* 3 */);
    AddressV = uint(CLAMP /* 3 */);
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
Texture2D Source;

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
            BlendState = BlendAdditive;
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
                //   float4 Weight;                     // Offset:    0 Size:    16
                //
                // }
                //
                //
                // Resource Bindings:
                //
                // Name                                 Type  Format         Dim      HLSL Bind  Count
                // ------------------------------ ---------- ------- ----------- -------------- ------
                // Linear                            sampler      NA          NA             s0      1 
                // Source                            texture  float4          2d             t0      1 
                // EffectConstants                   cbuffer      NA          NA            cb1      1 
                //
                //
                //
                // Input signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_POSITION              0   xyzw        0      POS   float       
                // TEXCOORDS                0   xy          1     NONE   float   xy  
                //
                //
                // Output signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_Target                0   xyzw        0   TARGET   float   xyzw
                //
                ps_4_0
                dcl_constantbuffer CB1[1], immediateIndexed
                dcl_sampler s0, mode_default
                dcl_resource_texture2d (float,float,float,float) t0
                dcl_input_ps linear v1.xy
                dcl_output o0.xyzw
                dcl_temps 6
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; v0.w <- IN.Position.w; 
                //   v1.x <- IN.UV.x; v1.y <- IN.UV.y; 
                //   o0.x <- <PS return value>.x; o0.y <- <PS return value>.y; o0.z <- <PS return value>.z; o0.w <- <PS return value>.w
                //
                #line 15 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\ifs-hyperbolic.fx"
                mov r0.w, l(0.693147)
                add r1.xy, -v1.xyxx, l(1.000000, 1.000000, 0.000000, 0.000000)
                div r1.xy, v1.xyxx, r1.xyxx
                log r0.yz, r1.xxyx
                mul r1.xy, r0.yzyy, r0.ywyy  // r1.y <- p.y
                
                #line 19
                mul r0.w, r1.y, r1.y
                mul r0.w, r1.x, r0.w
                mad r0.w, -r0.w, l(1.921812), l(1.000000)  // r0.w <- d
                
                #line 28
                lt r1.xz, r0.wwyw, l(0.000000, 0.000000, 0.000000, 0.000000)
                
                #line 21
                sqrt r0.w, r0.w  // r0.w <- a
                
                #line 20
                discard_nz r1.x
                
                #line 28
                lt r1.x, l(0.000000), r0.y
                iadd r1.x, -r1.x, r1.z
                itof r1.x, r1.x
                mul r0.y, r0.y, l(1.386294)
                max r0.y, |r0.y|, l(0.001000)
                div r0.y, l(1.000000, 1.000000, 1.000000, 1.000000), r0.y
                mul r0.y, r0.y, r1.x  // r0.y <- div
                
                #line 30
                add r1.x, r0.w, l(1.000000)
                
                #line 37
                add r0.w, -r0.w, l(1.000000)
                mul r2.x, r0.y, r0.w  // r2.x <- q.x
                
                #line 30
                mul r0.x, r0.y, r1.x  // r0.x <- q.x
                
                #line 33
                max r0.y, |r0.x|, |r1.y|
                div r0.y, l(1.000000, 1.000000, 1.000000, 1.000000), r0.y
                min r0.w, |r0.x|, |r1.y|
                mul r0.y, r0.y, r0.w
                mul r0.w, r0.y, r0.y
                mad r1.x, r0.w, l(0.020835), l(-0.085133)
                mad r1.x, r0.w, r1.x, l(0.180141)
                mad r1.x, r0.w, r1.x, l(-0.330299)
                mad r0.w, r0.w, r1.x, l(0.999866)
                mul r1.x, r0.w, r0.y
                mad r1.x, r1.x, l(-2.000000), l(1.570796)
                lt r1.z, |r0.x|, |r1.y|
                and r1.x, r1.z, r1.x
                mad r0.y, r0.y, r0.w, r1.x
                lt r0.w, r0.x, -r0.x
                and r0.w, r0.w, l(0xc0490fdb)
                add r0.y, r0.w, r0.y
                min r0.w, r0.x, r1.y
                lt r0.w, r0.w, -r0.w
                max r1.x, r0.x, r1.y
                ge r1.x, r1.x, -r1.x
                and r0.w, r0.w, r1.x
                movc r0.y, r0.w, -r0.y, r0.y  // r0.y <- theta
                
                #line 34
                add r0.y, r0.y, r0.y
                sincos null, r0.y, r0.y
                
                #line 32
                mul r2.zw, r0.xxxz, l(0.000000, 0.000000, 1.000000, 0.693147)
                
                #line 31
                mul r0.xw, r0.xxxz, l(-1.442695, 0.000000, 0.000000, -1.000000)
                
                #line 38
                mov r2.y, r0.z
                mul r1.xz, r2.xxyx, l(-1.442695, 0.000000, -1.000000, 0.000000)
                exp r1.xz, r1.xxzx
                add r1.xz, r1.xxzx, l(1.000000, 0.000000, 1.000000, 0.000000)
                div r1.xz, l(1.000000, 1.000000, 1.000000, 1.000000), r1.xxzx
                sample r3.xyzw, r1.xzxx, t0.xyzw, s0  // r3.x <- v.x; r3.y <- v.y; r3.z <- v.z; r3.w <- v.w
                
                #line 31
                exp r0.xz, r0.xxwx
                add r0.xz, r0.xxzx, l(1.000000, 0.000000, 1.000000, 0.000000)
                div r0.xz, l(1.000000, 1.000000, 1.000000, 1.000000), r0.xxzx
                sample r4.xyzw, r0.xzxx, t0.xyzw, s0  // r4.x <- v.x; r4.y <- v.y; r4.z <- v.z; r4.w <- v.w
                
                #line 32
                dp2 r0.x, r2.zwzz, r2.zwzz  // r0.x <- r2
                
                #line 39
                dp2 r0.z, r2.xwxx, r2.xwxx  // r0.z <- r2
                
                #line 34
                lt r0.w, l(0.000000), r0.x
                max r0.x, r0.x, l(0.001000)
                div r0.x, l(1.000000, 1.000000, 1.000000, 1.000000), r0.x
                itof r0.w, -r0.w
                mul r0.x, r0.x, r0.w
                mul r0.x, r0.x, r0.y  // r0.x <- j
                
                #line 35
                mov r5.w, |r0.x|
                
                #line 40
                max r0.x, |r1.y|, |r2.x|
                div r0.x, l(1.000000, 1.000000, 1.000000, 1.000000), r0.x
                min r0.y, |r1.y|, |r2.x|
                mul r0.x, r0.x, r0.y
                mul r0.y, r0.x, r0.x
                mad r0.w, r0.y, l(0.020835), l(-0.085133)
                mad r0.w, r0.y, r0.w, l(0.180141)
                mad r0.w, r0.y, r0.w, l(-0.330299)
                mad r0.y, r0.y, r0.w, l(0.999866)
                mul r0.w, r0.y, r0.x
                mad r0.w, r0.w, l(-2.000000), l(1.570796)
                lt r1.x, |r2.x|, |r1.y|
                and r0.w, r0.w, r1.x
                mad r0.x, r0.x, r0.y, r0.w
                lt r0.y, r2.x, -r2.x
                and r0.y, r0.y, l(0xc0490fdb)
                add r0.x, r0.y, r0.x
                min r0.y, r1.y, r2.x
                max r0.w, r1.y, r2.x
                ge r0.w, r0.w, -r0.w
                lt r0.y, r0.y, -r0.y
                and r0.y, r0.w, r0.y
                movc r0.x, r0.y, -r0.x, r0.x  // r0.x <- theta
                
                #line 41
                add r0.x, r0.x, r0.x
                sincos null, r0.x, r0.x
                lt r0.y, l(0.000000), r0.z
                max r0.z, r0.z, l(0.001000)
                div r0.z, l(1.000000, 1.000000, 1.000000, 1.000000), r0.z
                itof r0.y, -r0.y
                mul r0.y, r0.z, r0.y
                mul r0.x, r0.y, r0.x  // r0.x <- j
                
                #line 42
                mov r0.w, |r0.x|
                mov r0.xyz, cb1[0].xyzx
                mul r0.xyzw, r0.xyzw, r3.xyzw
                
                #line 35
                mov r5.xyz, cb1[0].xyzx
                
                #line 42
                mad r0.xyzw, r4.xyzw, r5.xyzw, r0.xyzw  // r0.x <- w.x; r0.y <- w.y; r0.z <- w.z; r0.w <- w.w
                
                #line 44
                mul o0.xyzw, r0.xyzw, l(0.500000, 0.500000, 0.500000, 0.500000)
                
                #line 46
                ret 
                // Approximately 106 instruction slots used
                            
            };
        }

    }

}

