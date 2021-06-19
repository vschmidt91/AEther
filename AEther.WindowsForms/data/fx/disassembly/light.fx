//
// FX Version: fx_5_0
//
// 3 local buffer(s)
//
cbuffer FrameConstants : register(b0)
{
    float   T;                          // Offset:    0, size:    4
    float   DT;                         // Offset:    4, size:    4
    float   HistogramShift;             // Offset:    8, size:    4
    float   AspectRatio;                // Offset:   12, size:    4
}

cbuffer CameraConstants : register(b1)
{
    float4x4 View;                      // Offset:    0, size:   64
    float4x4 Projection;                // Offset:   64, size:   64
    float3  ViewPosition;               // Offset:  128, size:   12
    float   FarPlane;                   // Offset:  140, size:    4
    float4x4 FarPosMatrix;              // Offset:  144, size:   64
}

cbuffer LightConstants : register(b2)
{
    float4x4 LightView;                 // Offset:    0, size:   64
    float4x4 LightProjection;           // Offset:   64, size:   64
    float3  LightIntensity;             // Offset:  128, size:   12
    float   Anisotropy;                 // Offset:  140, size:    4
    float3  LightPosition;              // Offset:  144, size:   12
    float   LightDistance;              // Offset:  156, size:    4
    float3  Emission;                   // Offset:  160, size:   12
    float   ShadowFarPlane;             // Offset:  172, size:    4
    float3  Scattering;                 // Offset:  176, size:   12
    float3  Absorption;                 // Offset:  192, size:   12
}

//
// 20 local object(s)
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
SamplerState Airlight_LookupSampler
{
    Filter   = uint(MIN_MAG_MIP_LINEAR /* 21 */);
};
Texture2D Depth;
Texture2D Normal;
Texture2D Color;
TextureCube Shadow;

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
                // cbuffer FrameConstants
                // {
                //
                //   float T;                           // Offset:    0 Size:     4
                //   float DT;                          // Offset:    4 Size:     4 [unused]
                //   float HistogramShift;              // Offset:    8 Size:     4 [unused]
                //   float AspectRatio;                 // Offset:   12 Size:     4 [unused]
                //
                // }
                //
                // cbuffer CameraConstants
                // {
                //
                //   float4x4 View;                     // Offset:    0 Size:    64 [unused]
                //   float4x4 Projection;               // Offset:   64 Size:    64 [unused]
                //   float3 ViewPosition;               // Offset:  128 Size:    12
                //   float FarPlane;                    // Offset:  140 Size:     4
                //   float4x4 FarPosMatrix;             // Offset:  144 Size:    64
                //
                // }
                //
                // cbuffer LightConstants
                // {
                //
                //   float4x4 LightView;                // Offset:    0 Size:    64 [unused]
                //   float4x4 LightProjection;          // Offset:   64 Size:    64 [unused]
                //   float3 LightIntensity;             // Offset:  128 Size:    12
                //   float Anisotropy;                  // Offset:  140 Size:     4
                //   float3 LightPosition;              // Offset:  144 Size:    12
                //   float LightDistance;               // Offset:  156 Size:     4
                //   float3 Emission;                   // Offset:  160 Size:    12 [unused]
                //   float ShadowFarPlane;              // Offset:  172 Size:     4
                //   float3 Scattering;                 // Offset:  176 Size:    12
                //   float3 Absorption;                 // Offset:  192 Size:    12
                //
                // }
                //
                //
                // Resource Bindings:
                //
                // Name                                 Type  Format         Dim      HLSL Bind  Count
                // ------------------------------ ---------- ------- ----------- -------------- ------
                // Linear                            sampler      NA          NA             s0      1 
                // Depth                             texture   float          2d             t0      1 
                // Normal                            texture  float4          2d             t1      1 
                // Color                             texture  float4          2d             t2      1 
                // Shadow                            texture   float        cube             t3      1 
                // FrameConstants                    cbuffer      NA          NA            cb0      1 
                // CameraConstants                   cbuffer      NA          NA            cb1      1 
                // LightConstants                    cbuffer      NA          NA            cb2      1 
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
                // SV_TARGET                0   xyz         0   TARGET   float   xyz 
                //
                ps_4_0
                dcl_constantbuffer CB0[1], immediateIndexed
                dcl_constantbuffer CB1[12], immediateIndexed
                dcl_constantbuffer CB2[13], immediateIndexed
                dcl_sampler s0, mode_default
                dcl_resource_texture2d (float,float,float,float) t0
                dcl_resource_texture2d (float,float,float,float) t1
                dcl_resource_texture2d (float,float,float,float) t2
                dcl_resource_texturecube (float,float,float,float) t3
                dcl_input_ps_siv linear noperspective v0.xy, position
                dcl_input_ps linear v1.xy
                dcl_output o0.xyz
                dcl_temps 9
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; v0.w <- IN.Position.w; 
                //   v1.x <- IN.UV.x; v1.y <- IN.UV.y; 
                //   o0.x <- <PS return value>.x; o0.y <- <PS return value>.y; o0.z <- <PS return value>.z
                //
                #line 20 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\light.fx"
                ftoi r0.xy, v0.xyxx  // r0.x <- index.x; r0.y <- index.y
                mov r0.zw, l(0,0,0,0)  // r0.w <- index.z
                
                #line 21
                ld r1.xyzw, r0.xyww, t0.xyzw
                mul r1.y, r1.x, cb1[8].w  // r1.y <- depth
                
                #line 22
                ld r2.xyzw, r0.xyww, t1.xyzw  // r2.x <- normal.x; r2.y <- normal.y; r2.z <- normal.z; r2.w <- normal.w
                
                #line 23
                ld r0.xyzw, r0.xyzw, t2.xyzw  // r0.x <- color.x; r0.y <- color.y; r0.z <- color.z; r0.w <- color.w
                
                #line 24
                mov r3.xy, v1.xyxx
                mov r3.z, l(1.000000)
                dp3 r4.x, r3.xyzx, cb1[9].xyzx  // r4.x <- farPos.x
                dp3 r4.y, r3.xyzx, cb1[10].xyzx  // r4.y <- farPos.y
                dp3 r4.z, r3.xyzx, cb1[11].xyzx  // r4.z <- farPos.z
                
                #line 25
                add r3.xyz, r4.xyzx, -cb1[8].xyzx
                dp3 r1.z, r3.xyzx, r3.xyzx
                rsq r1.z, r1.z
                mul r3.xyz, r1.zzzz, r3.xyzx  // r3.x <- V.x; r3.y <- V.y; r3.z <- V.z
                
                #line 26
                dp3 r1.z, r2.xyzx, r2.xyzx
                rsq r1.z, r1.z
                mul r2.xyz, r1.zzzz, r2.xyzx  // r2.x <- N.x; r2.y <- N.y; r2.z <- N.z
                
                #line 27
                mad r4.xyz, r1.yyyy, r3.xyzx, cb1[8].xyzx  // r4.x <- pos.x; r4.y <- pos.y; r4.z <- pos.z
                
                #line 28
                add r4.xyz, -r4.xyzx, cb2[9].xyzx  // r4.x <- lightVector.x; r4.y <- lightVector.y; r4.z <- lightVector.z
                
                #line 29
                dp3 r1.z, r4.xyzx, r4.xyzx
                sqrt r1.z, r1.z  // r1.z <- lightDistance
                
                #line 30
                div r1.w, l(1.000000, 1.000000, 1.000000, 1.000000), r1.z  // r1.w <- lightDistanceInv
                
                #line 31
                mul r5.xyz, r4.xyzx, r1.wwww  // r5.x <- L.x; r5.y <- L.y; r5.z <- L.z
                
                #line 32
                mov r6.xyz, -r4.xyzx
                sample r6.xyzw, r6.xyzx, t3.xyzw, s0
                
                #line 33
                eq r3.w, r0.w, l(1.000000)  // r3.w <- isMetal
                
                #line 34
                and r3.w, r3.w, l(0x3f800000)
                mad r6.yzw, r3.wwww, -r0.xxyz, r0.xxyz  // r6.y <- diffuse.x; r6.z <- diffuse.y; r6.w <- diffuse.z
                
                #line 35
                add r0.xyz, r0.xyzx, l(-1.000000, -1.000000, -1.000000, 0.000000)
                mad r0.xyz, r3.wwww, r0.xyzx, l(1.000000, 1.000000, 1.000000, 0.000000)  // r0.x <- specular.x; r0.y <- specular.y; r0.z <- specular.z
                
                #line 36
                mul r2.w, r2.w, r2.w
                mul r3.w, r2.w, r2.w  // r3.w <- roughness
                
                #line 37
                mul r6.yzw, r6.yyzw, l(0.000000, 0.318319, 0.318319, 0.318319)  // r6.y <- brdfDiffuse.x; r6.z <- brdfDiffuse.y; r6.w <- brdfDiffuse.z
                
                #line 44 "brdf.fxi"
                mad r4.xyz, r1.wwww, r4.xyzx, -r3.xyzx
                dp3 r4.w, r4.xyzx, r4.xyzx
                rsq r4.w, r4.w
                mul r4.xyz, r4.wwww, r4.xyzx  // r4.x <- h.x; r4.y <- h.y; r4.z <- h.z
                
                #line 48
                dp3 r4.w, r5.xyzx, r4.xyzx  // r4.w <- LoH
                
                #line 49
                dp3 r5.w, r4.xyzx, r2.xyzx
                
                #line 23
                mul r7.x, r5.w, r5.w
                mad r2.w, r2.w, r2.w, l(-1.000000)
                mad r2.w, r7.x, r2.w, l(1.000000)  // r2.w <- d
                
                #line 24
                ge r5.w, r5.w, l(0.000000)
                and r5.w, r5.w, l(0x3f800000)
                mul r5.w, r3.w, r5.w
                mul r2.w, r2.w, r2.w
                mul r2.w, r2.w, l(3.141500)
                div r2.w, r5.w, r2.w  // r2.w <- <BRDF_D return value>
                
                #line 38
                add r5.w, -r4.w, l(1.000000)
                mul r7.x, r5.w, r5.w
                mul r7.x, r7.x, r7.x
                mul r5.w, r5.w, r7.x
                add r7.xyz, -r0.xyzx, l(1.000000, 1.000000, 1.000000, 0.000000)
                mad r0.xyz, r5.wwww, r7.xyzx, r0.xyzx  // r0.x <- <BRDF_F return value>.x; r0.y <- <BRDF_F return value>.y; r0.z <- <BRDF_F return value>.z
                
                #line 50
                mul r0.xyz, r0.xyzx, r2.wwww  // r0.x <- x.x; r0.y <- x.y; r0.z <- x.z
                
                #line 51
                dp3_sat r2.w, -r3.xyzx, r4.xyzx
                
                #line 30
                mul r4.x, r2.w, r2.w  // r4.x <- XoH2
                
                #line 31
                mad r2.w, -r2.w, r2.w, l(1.000000)
                div r2.w, r2.w, r4.x  // r2.w <- tan2
                
                #line 32
                mad r2.w, r3.w, r2.w, l(1.000000)
                sqrt r2.w, r2.w
                add r2.w, r2.w, l(1.000000)
                div r2.w, l(2.000000), r2.w  // r2.w <- <BRDF_G return value>
                
                #line 51
                mul r0.xyz, r0.xyzx, r2.wwww
                
                #line 29
                mov_sat r4.w, r4.w  // r4.w <- XoH
                
                #line 30
                mul r2.w, r4.w, r4.w  // r2.w <- XoH2
                
                #line 31
                mad r4.x, -r4.w, r4.w, l(1.000000)
                div r2.w, r4.x, r2.w  // r2.w <- tan2
                
                #line 32
                mad r2.w, r3.w, r2.w, l(1.000000)
                sqrt r2.w, r2.w
                add r2.w, r2.w, l(1.000000)
                div r2.w, l(2.000000), r2.w  // r2.w <- <BRDF_G return value>
                
                #line 42 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\light.fx"
                mul r1.w, r1.w, r1.w  // r1.w <- Ls.x
                
                #line 43
                mad r3.w, cb2[10].w, r6.x, l(0.001000)
                ge r1.z, r3.w, r1.z
                and r1.z, r1.z, l(0x3f800000)
                mul r1.z, r1.z, r1.w  // r1.z <- Ls.x
                
                #line 44
                dp3_sat r1.w, r2.xyzx, r5.xyzx
                mul r1.z, r1.w, r1.z
                
                #line 45
                mad r0.xyz, r0.xyzx, r2.wwww, -r6.yzwy
                mad r0.xyz, r0.wwww, r0.xyzx, r6.yzwy
                mul r0.xyz, r0.xyzx, r1.zzzz  // r0.x <- Ls.x; r0.y <- Ls.y; r0.z <- Ls.z
                
                #line 46
                add r2.xyz, cb2[11].xyzx, cb2[12].xyzx
                mul r4.xyz, -r1.yyyy, r2.xyzx
                mul r4.xyz, r4.xyzx, l(1.442695, 1.442695, 1.442695, 0.000000)
                exp r4.xyz, r4.xyzx
                
                #line 48
                add r5.xyz, -cb1[8].xyzx, cb2[9].xyzx
                dp3 r0.w, r5.xyzx, r3.xyzx  // r0.w <- tm
                
                #line 49
                mul r1.z, r0.w, r0.w
                mad r1.z, cb2[9].w, cb2[9].w, -r1.z
                sqrt r1.z, r1.z  // r1.z <- D
                
                #line 50
                mov r1.x, l(0)
                add r1.xy, -r0.wwww, r1.xyxx
                min r5.xy, r1.zzzz, |r1.xyxx|
                max r5.zw, r1.zzzz, |r1.xxxy|
                div r5.zw, l(1.000000, 1.000000, 1.000000, 1.000000), r5.zzzw
                mul r5.xy, r5.zwzz, r5.xyxx
                mul r5.zw, r5.xxxy, r5.xxxy
                mad r6.xy, r5.zwzz, l(0.020835, 0.020835, 0.000000, 0.000000), l(-0.085133, -0.085133, 0.000000, 0.000000)
                mad r6.xy, r5.zwzz, r6.xyxx, l(0.180141, 0.180141, 0.000000, 0.000000)
                mad r6.xy, r5.zwzz, r6.xyxx, l(-0.330299, -0.330299, 0.000000, 0.000000)
                mad r5.zw, r5.zzzw, r6.xxxy, l(0.000000, 0.000000, 0.999866, 0.999866)
                mul r6.xy, r5.zwzz, r5.xyxx
                lt r6.zw, r1.zzzz, |r1.xxxy|
                mad r6.xy, r6.xyxx, l(-2.000000, -2.000000, 0.000000, 0.000000), l(1.570796, 1.570796, 0.000000, 0.000000)
                and r6.xy, r6.zwzz, r6.xyxx
                mad r5.xy, r5.xyxx, r5.zwzz, r6.xyxx
                min r5.zw, r1.zzzz, r1.xxxy
                max r1.xy, r1.zzzz, r1.xyxx
                lt r5.zw, r5.zzzw, -r5.zzzw
                ge r1.xy, r1.xyxx, -r1.xyxx
                and r1.xy, r1.xyxx, r5.zwzz
                movc r1.xy, r1.xyxx, -r5.xyxx, r5.xyxx  // r1.x <- theta.x; r1.y <- theta.y
                
                #line 187 "globals.fxi"
                dp2 r1.w, v1.xyxx, l(1.000000, 1.618034, 0.000000, 0.000000)
                sincos null, r1.w, r1.w
                mul r1.w, r1.w, l(12345.678711)
                frc r5.y, r1.w  // r5.y <- <Dither2 return value>
                
                #line 60 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\light.fx"
                add r1.y, -r1.x, r1.y
                
                #line 48 "light.fxi"
                mad r1.w, cb2[8].w, cb2[8].w, l(1.000000)
                add r2.w, cb2[8].w, cb2[8].w
                
                #line 49
                mad r3.w, -cb2[8].w, cb2[8].w, l(1.000000)
                
                #line 187 "globals.fxi"
                mov r6.x, cb0[0].x
                
                #line 54 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\light.fx"
                mov r7.xyz, l(0,0,0,0)  // r7.x <- Lv.x; r7.y <- Lv.y; r7.z <- Lv.z
                mov r4.w, l(0)  // r4.w <- i
                loop 
                  ige r5.z, r4.w, l(16)
                  breakc_nz r5.z
                
                #line 57
                  itof r6.y, r4.w  // r6.y <- v.y
                
                #line 187 "globals.fxi"
                  dp2 r5.z, r6.xyxx, l(1.000000, 1.618034, 0.000000, 0.000000)
                  sincos null, r5.z, r5.z
                  mul r5.z, r5.z, l(12345.678711)
                  frc r5.x, r5.z  // r5.x <- <Dither2 return value>
                  dp2 r5.x, r5.xyxx, l(1.000000, 1.618034, 0.000000, 0.000000)
                  sincos null, r5.x, r5.x
                  mul r5.x, r5.x, l(12345.678711)
                  frc r5.x, r5.x  // r5.x <- <Dither2 return value>
                
                #line 58 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\light.fx"
                  add r5.x, r5.x, r6.y
                  mul r5.x, r1.y, r5.x
                
                #line 60
                  mad r5.x, r5.x, l(0.062500), r1.x  // r5.x <- thetai
                
                #line 61
                  sincos r5.x, r8.x, r5.x
                  div r5.z, r5.x, r8.x
                  mad r5.z, r1.z, r5.z, r0.w  // r5.z <- ti
                
                #line 66
                  mad r6.yzw, r5.zzzz, r3.xxyz, cb1[8].xxyz  // r6.y <- pi.x; r6.z <- pi.y; r6.w <- pi.z
                
                #line 67
                  add r6.yzw, -r6.yyzw, cb2[9].xxyz  // r6.y <- li.x; r6.z <- li.y; r6.w <- li.z
                
                #line 68
                  dp3 r5.w, r6.yzwy, r6.yzwy  // r5.w <- ri2
                
                #line 69
                  sqrt r7.w, r5.w  // r7.w <- ri
                
                #line 70
                  mov r6.yzw, -r6.yyzw
                  sample r8.xyzw, r6.yzwy, t3.xyzw, s0
                  mul r6.y, r8.x, cb2[10].w  // r6.y <- si
                
                #line 48 "light.fxi"
                  mad r5.x, -r2.w, r5.x, r1.w  // r5.x <- d
                
                #line 49
                  mul r6.z, r5.x, r5.x
                  mul r5.x, r5.x, r6.z
                  rsq r5.x, r5.x
                  mul r5.x, r3.w, r5.x
                  mul r5.x, r5.x, l(0.079580)  // r5.x <- <PhaseHG return value>
                
                #line 74 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\light.fx"
                  mul r6.y, r6.y, r6.y
                  ge r5.w, r6.y, r5.w
                  and r5.w, r5.w, l(0x3f800000)
                  mul r5.x, r5.w, r5.x  // r5.x <- Lvj.x
                
                #line 92
                  add r5.z, r5.z, r7.w
                  mul r6.yzw, r2.xxyz, -r5.zzzz
                  mul r6.yzw, r6.yyzw, l(0.000000, 1.442695, 1.442695, 1.442695)
                  exp r6.yzw, r6.yyzw
                
                #line 98
                  mad r7.xyz, r5.xxxx, r6.yzwy, r7.xyzx
                
                #line 100
                  iadd r4.w, r4.w, l(1)
                endloop 
                
                #line 103
                mul r1.xyw, r1.yyyy, r7.xyxz  // r1.x <- Lv.x; r1.y <- Lv.y; r1.w <- Lv.z
                
                #line 104
                div r1.xyz, r1.xywx, r1.zzzz  // r1.z <- Lv.z
                
                #line 110
                mul r1.xyz, r1.xyzx, cb2[11].xyzx
                
                #line 111
                mul r1.xyz, r1.xyzx, l(0.062500, 0.062500, 0.062500, 0.000000)
                
                #line 113
                mad r0.xyz, r0.xyzx, r4.xyzx, r1.xyzx
                mul o0.xyz, r0.xyzx, cb2[8].xyzx
                ret 
                // Approximately 174 instruction slots used
                            
            };
        }

    }

}

