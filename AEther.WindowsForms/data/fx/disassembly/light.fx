Macros: (EQUIANGULAR,True)//
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
    float4x4 ViewDirectionMatrix;       // Offset:  144, size:   64
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
    float   LightFarPlane;              // Offset:  172, size:    4
    float3  Scattering;                 // Offset:  176, size:   12
    float3  Absorption;                 // Offset:  192, size:   12
}

//
// 21 local object(s)
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
SamplerState Airlight_LookupSampler
{
    Filter   = uint(MIN_MAG_MIP_LINEAR /* 21 */);
};
Texture2D Depth;
Texture2D Normal;
Texture2D Color;
TextureCube Shadow;
Texture2D Airlight;

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
                // Buffer Definitions: 
                //
                // cbuffer CameraConstants
                // {
                //
                //   float4x4 View;                     // Offset:    0 Size:    64 [unused]
                //   float4x4 Projection;               // Offset:   64 Size:    64 [unused]
                //   float3 ViewPosition;               // Offset:  128 Size:    12 [unused]
                //   float FarPlane;                    // Offset:  140 Size:     4 [unused]
                //   float4x4 ViewDirectionMatrix;      // Offset:  144 Size:    64
                //
                // }
                //
                //
                // Resource Bindings:
                //
                // Name                                 Type  Format         Dim      HLSL Bind  Count
                // ------------------------------ ---------- ------- ----------- -------------- ------
                // CameraConstants                   cbuffer      NA          NA            cb1      1 
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
                // POSITION                 0   xyz         2     NONE   float   xyz 
                //
                vs_4_0
                dcl_constantbuffer CB1[12], immediateIndexed
                dcl_input v0.xy
                dcl_output_siv o0.xyzw, position
                dcl_output o1.xy
                dcl_output o2.xyz
                dcl_temps 1
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; 
                //   o2.x <- <VS return value>.ViewDirection.x; o2.y <- <VS return value>.ViewDirection.y; o2.z <- <VS return value>.ViewDirection.z; 
                //   o1.x <- <VS return value>.UV.x; o1.y <- <VS return value>.UV.y; 
                //   o0.x <- <VS return value>.Position.x; o0.y <- <VS return value>.Position.y; o0.z <- <VS return value>.Position.z; o0.w <- <VS return value>.Position.w
                //
                #line 37 "C:\Users\volke\Source\Repos\AEther\AEther.WindowsForms\bin\Debug\net9.0-windows7.0\light.fx"
                mov o0.xy, v0.xyxx
                mov o0.zw, l(0,0,0,1.000000)
                mad o1.xy, v0.xyxx, l(0.500000, -0.500000, 0.000000, 0.000000), l(0.500000, 0.500000, 0.000000, 0.000000)
                
                #line 36
                mad r0.xy, v0.xyxx, l(0.500000, -0.500000, 0.000000, 0.000000), l(0.500000, 0.500000, 0.000000, 0.000000)
                mov r0.z, l(1.000000)
                dp3 o2.x, r0.xyzx, cb1[9].xyzx
                dp3 o2.y, r0.xyzx, cb1[10].xyzx
                dp3 o2.z, r0.xyzx, cb1[11].xyzx
                
                #line 37
                ret 
                // Approximately 9 instruction slots used
                            
            };
            GeometryShader = NULL;
            PixelShader = asm {
                //
                // Generated by Microsoft (R) HLSL Shader Compiler 10.1
                //
                //
                // Buffer Definitions: 
                //
                // cbuffer CameraConstants
                // {
                //
                //   float4x4 View;                     // Offset:    0 Size:    64 [unused]
                //   float4x4 Projection;               // Offset:   64 Size:    64 [unused]
                //   float3 ViewPosition;               // Offset:  128 Size:    12
                //   float FarPlane;                    // Offset:  140 Size:     4
                //   float4x4 ViewDirectionMatrix;      // Offset:  144 Size:    64 [unused]
                //
                // }
                //
                // cbuffer LightConstants
                // {
                //
                //   float4x4 LightView;                // Offset:    0 Size:    64 [unused]
                //   float4x4 LightProjection;          // Offset:   64 Size:    64 [unused]
                //   float3 LightIntensity;             // Offset:  128 Size:    12
                //   float Anisotropy;                  // Offset:  140 Size:     4 [unused]
                //   float3 LightPosition;              // Offset:  144 Size:    12
                //   float LightDistance;               // Offset:  156 Size:     4
                //   float3 Emission;                   // Offset:  160 Size:    12 [unused]
                //   float LightFarPlane;               // Offset:  172 Size:     4
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
                // Airlight                          texture  float3          2d             t4      1 
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
                // TEXCOORDS                0   xy          1     NONE   float       
                // POSITION                 0   xyz         2     NONE   float   xyz 
                //
                //
                // Output signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_TARGET                0   xyz         0   TARGET   float   xyz 
                //
                ps_4_0
                dcl_constantbuffer CB1[9], immediateIndexed
                dcl_constantbuffer CB2[13], immediateIndexed
                dcl_sampler s0, mode_default
                dcl_resource_texture2d (float,float,float,float) t0
                dcl_resource_texture2d (float,float,float,float) t1
                dcl_resource_texture2d (float,float,float,float) t2
                dcl_resource_texturecube (float,float,float,float) t3
                dcl_resource_texture2d (float,float,float,float) t4
                dcl_input_ps_siv linear noperspective v0.xy, position
                dcl_input_ps linear v2.xyz
                dcl_output o0.xyz
                dcl_temps 9
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; v0.w <- IN.Position.w; 
                //   v1.x <- IN.UV.x; v1.y <- IN.UV.y; 
                //   v2.x <- IN.ViewDirection.x; v2.y <- IN.ViewDirection.y; v2.z <- IN.ViewDirection.z; 
                //   o0.x <- <PS return value>.x; o0.y <- <PS return value>.y; o0.z <- <PS return value>.z
                //
                #line 50 "C:\Users\volke\Source\Repos\AEther\AEther.WindowsForms\bin\Debug\net9.0-windows7.0\light.fx"
                dp3 r0.x, v2.xyzx, v2.xyzx
                rsq r0.x, r0.x
                mul r0.xyz, r0.xxxx, v2.xyzx  // r0.x <- V.x; r0.y <- V.y; r0.z <- V.z
                
                #line 46
                ftoi r1.xy, v0.xyxx  // r1.x <- index.x; r1.y <- index.y
                mov r1.zw, l(0,0,0,0)  // r1.w <- index.z
                
                #line 47
                ld r2.xyzw, r1.xyww, t0.xyzw
                mul r2.y, r2.x, cb1[8].w  // r2.y <- depth
                
                #line 52
                mad r3.xyz, r2.yyyy, r0.xyzx, cb1[8].xyzx  // r3.x <- pos.x; r3.y <- pos.y; r3.z <- pos.z
                
                #line 53
                add r3.xyz, -r3.xyzx, cb2[9].xyzx  // r3.x <- lightVector.x; r3.y <- lightVector.y; r3.z <- lightVector.z
                
                #line 54
                dp3 r0.w, r3.xyzx, r3.xyzx
                sqrt r4.y, r0.w  // r4.y <- lightDistance
                
                #line 55
                div r0.w, l(1.000000, 1.000000, 1.000000, 1.000000), r4.y  // r0.w <- lightDistanceInv
                
                #line 44 "brdf.fxi"
                mad r5.xyz, r0.wwww, r3.xyzx, -r0.xyzx
                dp3 r2.z, r5.xyzx, r5.xyzx
                rsq r2.z, r2.z
                mul r5.xyz, r2.zzzz, r5.xyzx  // r5.x <- h.x; r5.y <- h.y; r5.z <- h.z
                
                #line 56 "C:\Users\volke\Source\Repos\AEther\AEther.WindowsForms\bin\Debug\net9.0-windows7.0\light.fx"
                mul r6.xyz, r3.xyzx, r0.wwww  // r6.x <- L.x; r6.y <- L.y; r6.z <- L.z
                
                #line 69
                mul r0.w, r0.w, r0.w  // r0.w <- Ls.x
                
                #line 57
                mov r3.xyz, -r3.xyzx
                sample r3.xyzw, r3.xyzx, t3.xyzw, s0
                
                #line 70
                mad r2.z, cb2[10].w, r3.x, l(0.100000)
                ge r2.z, r2.z, r4.y
                and r2.z, r2.z, l(0x3f800000)
                mul r0.w, r0.w, r2.z
                
                #line 48 "brdf.fxi"
                dp3_sat r2.z, r6.xyzx, r5.xyzx  // r2.z <- LoH
                
                #line 38
                add r2.w, -r2.z, l(1.000000)
                
                #line 53
                mul r2.z, r2.z, r2.z
                
                #line 38
                mul r3.x, r2.w, r2.w
                mul r3.x, r3.x, r3.x
                mul r2.w, r2.w, r3.x
                
                #line 49 "C:\Users\volke\Source\Repos\AEther\AEther.WindowsForms\bin\Debug\net9.0-windows7.0\light.fx"
                ld r3.xyzw, r1.xyzw, t2.xyzw  // r3.x <- color.x; r3.y <- color.y; r3.z <- color.z; r3.w <- color.w
                
                #line 48
                ld r1.xyzw, r1.xyww, t1.xyzw  // r1.x <- normal.x; r1.y <- normal.y; r1.z <- normal.z; r1.w <- normal.w
                
                #line 60
                eq r4.z, r3.w, l(1.000000)  // r4.z <- isMetal
                
                #line 61
                and r4.z, r4.z, l(0x3f800000)
                
                #line 62
                add r7.xyz, -r3.wwww, r3.xyzx
                mad r7.xyz, r4.zzzz, r7.xyzx, r3.wwww  // r7.x <- colorSpecular.x; r7.y <- colorSpecular.y; r7.z <- colorSpecular.z
                
                #line 61
                mad r3.xyz, r4.zzzz, -r3.xyzx, r3.xyzx  // r3.x <- colorDiffuse.x; r3.y <- colorDiffuse.y; r3.z <- colorDiffuse.z
                
                #line 38 "brdf.fxi"
                add r8.xyz, -r7.xyzx, l(1.000000, 1.000000, 1.000000, 0.000000)
                mad r7.xyz, r2.wwww, r8.xyzx, r7.xyzx  // r7.x <- <BRDF_F return value>.x; r7.y <- <BRDF_F return value>.y; r7.z <- <BRDF_F return value>.z
                
                #line 49
                dp3 r2.w, r5.xyzx, r1.xyzx
                
                #line 23
                mul r2.w, r2.w, r2.w
                add r4.z, r1.w, l(-1.000000)
                mad r2.w, r2.w, r4.z, l(1.000000)  // r2.w <- d
                
                #line 24
                mul r2.w, r2.w, r2.w
                mul r2.w, r2.w, l(3.141500)
                div r1.w, r1.w, r2.w  // r1.w <- <BRDF_D return value>
                
                #line 71 "C:\Users\volke\Source\Repos\AEther\AEther.WindowsForms\bin\Debug\net9.0-windows7.0\light.fx"
                dp3_sat r1.x, r1.xyzx, r6.xyzx
                mul r0.w, r0.w, r1.x
                
                #line 49 "brdf.fxi"
                mul r1.x, r1.w, l(0.250000)  // r1.x <- x.x
                
                #line 50
                mul r1.xyz, r7.xyzx, r1.xxxx  // r1.y <- x.y; r1.z <- x.z
                
                #line 53
                div r1.xyz, r1.xyzx, r2.zzzz
                
                #line 65 "C:\Users\volke\Source\Repos\AEther\AEther.WindowsForms\bin\Debug\net9.0-windows7.0\light.fx"
                mad r1.xyz, -r3.xyzx, l(0.318319, 0.318319, 0.318319, 0.000000), r1.xyzx
                
                #line 63
                mul r3.xyz, r3.xyzx, l(0.318319, 0.318319, 0.318319, 0.000000)  // r3.x <- brdfDiffuse.x; r3.y <- brdfDiffuse.y; r3.z <- brdfDiffuse.z
                
                #line 65
                mad r1.xyz, r3.wwww, r1.xyzx, r3.xyzx  // r1.x <- brdf.x; r1.y <- brdf.y; r1.z <- brdf.z
                
                #line 72
                mul r1.xyz, r0.wwww, r1.xyzx  // r1.x <- Ls.x; r1.y <- Ls.y; r1.z <- Ls.z
                
                #line 76
                add r3.xyz, -cb1[8].xyzx, cb2[9].xyzx
                dp3 r0.x, r3.xyzx, r0.xyzx  // r0.x <- tm
                
                #line 78
                mov r2.x, l(0)
                add r0.yz, -r0.xxxx, r2.xxyx
                mul r2.xz, r0.yyzy, r0.yyzy
                add r0.yz, r0.yyzy, r0.yyzy
                mul r2.xz, r2.xxzx, l(0.281250, 0.000000, 0.281250, 0.000000)
                
                #line 77
                mul r0.w, r0.x, r0.x
                mad r0.w, cb2[9].w, cb2[9].w, -r0.w
                sqrt r0.w, r0.w  // r0.w <- D
                
                #line 78
                mov r4.x, cb2[9].w
                add r3.xy, r4.xyxx, r0.wwww
                mad r2.xz, r3.xxyx, r3.xxyx, r2.xxzx
                mul r0.yz, r0.yyzy, r3.xxyx
                div r0.yz, r0.yyzy, r2.xxzx  // r0.y <- theta.x; r0.z <- theta.y
                
                #line 135
                mad r3.xy, r0.yzyy, l(0.318319, 0.318319, 0.000000, 0.000000), l(0.500000, 0.500000, 0.000000, 0.000000)  // r3.x <- airlightX.x; r3.y <- airlightX.y
                
                #line 137
                add r0.y, r0.w, l(1.000000)
                rsq r3.z, r0.y  // r3.z <- airlightY
                
                #line 139
                sample r4.xyzw, r3.xzxx, t4.xyzw, s0  // r4.x <- Lv.x; r4.y <- Lv.y; r4.z <- Lv.z
                
                #line 140
                sample r3.xyzw, r3.yzyy, t4.xyzw, s0
                add r2.xzw, -r3.xxyz, r4.xxyz  // r2.x <- Lv.x; r2.z <- Lv.y; r2.w <- Lv.z
                
                #line 141
                mul r2.xzw, r2.xxzw, l(0.010000, 0.000000, 0.010000, 0.010000)
                
                #line 74
                add r3.xyz, cb2[11].xyzx, cb2[12].xyzx
                
                #line 145
                mul r0.xyz, -r0.xxxx, r3.xyzx
                
                #line 74
                mul r3.xyz, -r2.yyyy, r3.xyzx
                mul r3.xyz, r3.xyzx, l(1.442695, 1.442695, 1.442695, 0.000000)
                exp r3.xyz, r3.xyzx
                
                #line 145
                mul r0.xyz, r0.xyzx, l(1.442695, 1.442695, 1.442695, 0.000000)
                exp r0.xyz, r0.xyzx
                mul r0.xyz, r0.xyzx, r2.xzwx  // r0.x <- Lv.x; r0.y <- Lv.y; r0.z <- Lv.z
                
                #line 146
                div r0.xyz, r0.xyzx, r0.wwww
                
                #line 150
                mad r0.xyz, r1.xyzx, r3.xyzx, r0.xyzx
                mul o0.xyz, r0.xyzx, cb2[8].xyzx
                ret 
                // Approximately 89 instruction slots used
                            
            };
        }

    }

}

