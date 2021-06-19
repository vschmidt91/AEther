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

cbuffer GeometryConstants : register(b2)
{
    Instance SingleInstance;            // Offset:    0, size:   96
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
StructuredBuffer Instances;
Texture2D ColorMap;

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
            RasterizerState = RasterizerBoth;
            DS_StencilRef = uint(0);
            DepthStencilState = DepthStencilDefault;
            AB_BlendFactor = float4(0, 0, 0, 0);
            AB_SampleMask = uint(0xffffffff);
            BlendState = BlendNone;
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
                //   float4x4 View;                     // Offset:    0 Size:    64
                //   float4x4 Projection;               // Offset:   64 Size:    64
                //   float3 ViewPosition;               // Offset:  128 Size:    12 [unused]
                //   float FarPlane;                    // Offset:  140 Size:     4 [unused]
                //   float4x4 FarPosMatrix;             // Offset:  144 Size:    64 [unused]
                //
                // }
                //
                // Resource bind info for Instances
                // {
                //
                //   struct
                //   {
                //       
                //       float4x4 World;                // Offset:    0
                //       float4 Color;                  // Offset:   64
                //       float Roughness;               // Offset:   80
                //       float3 _;                      // Offset:   84
                //
                //   } $Element;                        // Offset:    0 Size:    96
                //
                // }
                //
                //
                // Resource Bindings:
                //
                // Name                                 Type  Format         Dim      HLSL Bind  Count
                // ------------------------------ ---------- ------- ----------- -------------- ------
                // Instances                         texture  struct         r/o             t0      1 
                // CameraConstants                   cbuffer      NA          NA            cb1      1 
                //
                //
                //
                // Input signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // POSITION                 0   xyz         0     NONE   float   xyz 
                // NORMAL                   0   xyz         1     NONE   float   xyz 
                // TEXCOORDS                0   xy          2     NONE   float   xy  
                // SV_InstanceID            0   x           3   INSTID    uint   x   
                //
                //
                // Output signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_POSITION              0   xyzw        0      POS   float   xyzw
                // NORMAL                   0   xyzw        1     NONE   float   xyzw
                // TEXCOORDS                0   xy          2     NONE   float   xy  
                // COLOR                    0   xyzw        3     NONE   float   xyzw
                // POSITION                 0   xyz         4     NONE   float   xyz 
                //
                vs_4_0
                dcl_globalFlags refactoringAllowed | enableRawAndStructuredBuffers
                dcl_constantbuffer CB1[8], immediateIndexed
                dcl_resource_structured t0, 96
                dcl_input v0.xyz
                dcl_input v1.xyz
                dcl_input v2.xy
                dcl_input_sgv v3.x, instance_id
                dcl_output_siv o0.xyzw, position
                dcl_output o1.xyzw
                dcl_output o2.xy
                dcl_output o3.xyzw
                dcl_output o4.xyz
                dcl_temps 8
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; 
                //   v1.x <- IN.Normal.x; v1.y <- IN.Normal.y; v1.z <- IN.Normal.z; 
                //   v2.x <- IN.UV.x; v2.y <- IN.UV.y; 
                //   v3.x <- IN.ID; 
                //   o4.x <- <VS return value>.WorldPosition.x; o4.y <- <VS return value>.WorldPosition.y; o4.z <- <VS return value>.WorldPosition.z; 
                //   o3.x <- <VS return value>.Color.x; o3.y <- <VS return value>.Color.y; o3.z <- <VS return value>.Color.z; o3.w <- <VS return value>.Color.w; 
                //   o2.x <- <VS return value>.UV.x; o2.y <- <VS return value>.UV.y; 
                //   o1.x <- <VS return value>.Normal.x; o1.y <- <VS return value>.Normal.y; o1.z <- <VS return value>.Normal.z; o1.w <- <VS return value>.Normal.w; 
                //   o0.x <- <VS return value>.Position.x; o0.y <- <VS return value>.Position.y; o0.z <- <VS return value>.Position.z; o0.w <- <VS return value>.Position.w
                //
                #line 42 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\geometry.fx"
                ld_structured r0.xyzw, v3.x, l(48), t0.xyzw  // r0.x <- instance.World._m03; r0.y <- instance.World._m13; r0.z <- instance.World._m23; r0.w <- instance.World._m33
                
                #line 48
                mov r1.w, r0.y
                
                #line 42
                ld_structured r2.xyzw, v3.x, l(0), t0.xyzw  // r2.x <- instance.World._m00; r2.y <- instance.World._m10; r2.z <- instance.World._m20; r2.w <- instance.World._m30
                
                #line 48
                mov r1.x, r2.y
                
                #line 42
                ld_structured r3.xyzw, v3.x, l(16), t0.xyzw  // r3.x <- instance.World._m01; r3.y <- instance.World._m11; r3.z <- instance.World._m21; r3.w <- instance.World._m31
                
                #line 48
                mov r1.y, r3.y
                
                #line 42
                ld_structured r4.xyzw, v3.x, l(32), t0.xyzw  // r4.x <- instance.World._m02; r4.y <- instance.World._m12; r4.z <- instance.World._m22; r4.w <- instance.World._m32
                
                #line 48
                mov r1.z, r4.y
                mov r5.xyz, v0.xyzx
                mov r5.w, l(1.000000)
                dp4 r6.y, r1.xyzw, r5.xyzw  // r6.y <- worldPos.y
                
                #line 52
                dp3 o1.y, r1.xyzx, v1.xyzx
                
                #line 49
                mul r1.xyzw, r6.yyyy, cb1[1].xyzw
                
                #line 48
                mov r7.w, r0.x
                mov r7.x, r2.x
                mov r7.y, r3.x
                mov r7.z, r4.x
                dp4 r6.x, r7.xyzw, r5.xyzw  // r6.x <- worldPos.x
                
                #line 52
                dp3 o1.x, r7.xyzx, v1.xyzx
                
                #line 49
                mad r1.xyzw, cb1[0].xyzw, r6.xxxx, r1.xyzw
                
                #line 48
                mov r7.w, r0.z
                mov r7.x, r2.z
                mov r0.x, r2.w
                mov r7.y, r3.z
                mov r0.y, r3.w
                mov r7.z, r4.z
                mov r0.z, r4.w
                dp4 r0.x, r0.xyzw, r5.xyzw  // r0.x <- worldPos.w
                dp4 r6.z, r7.xyzw, r5.xyzw  // r6.z <- worldPos.z
                
                #line 52
                dp3 o1.z, r7.xyzx, v1.xyzx
                
                #line 49
                mad r1.xyzw, cb1[2].xyzw, r6.zzzz, r1.xyzw
                
                #line 61
                mov o4.xyz, r6.xyzx
                
                #line 49
                mad r0.xyzw, cb1[3].xyzw, r0.xxxx, r1.xyzw  // r0.x <- viewPos.x; r0.y <- viewPos.y; r0.z <- viewPos.z; r0.w <- viewPos.w
                
                #line 50
                mul r1.xyzw, r0.yyyy, cb1[5].xyzw
                mad r1.xyzw, cb1[4].xyzw, r0.xxxx, r1.xyzw
                mad r1.xyzw, cb1[6].xyzw, r0.zzzz, r1.xyzw
                mad o0.xyzw, cb1[7].xyzw, r0.wwww, r1.xyzw
                
                #line 42
                ld_structured o1.w, v3.x, l(80), t0.xxxx
                
                #line 61
                mov o2.xy, v2.xyxx
                
                #line 42
                ld_structured o3.xyzw, v3.x, l(64), t0.xyzw
                
                #line 61
                ret 
                // Approximately 41 instruction slots used
                            
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
                //   float4x4 FarPosMatrix;             // Offset:  144 Size:    64 [unused]
                //
                // }
                //
                //
                // Resource Bindings:
                //
                // Name                                 Type  Format         Dim      HLSL Bind  Count
                // ------------------------------ ---------- ------- ----------- -------------- ------
                // Linear                            sampler      NA          NA             s0      1 
                // ColorMap                          texture  float4          2d             t1      1 
                // CameraConstants                   cbuffer      NA          NA            cb1      1 
                //
                //
                //
                // Input signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_POSITION              0   xyzw        0      POS   float       
                // NORMAL                   0   xyzw        1     NONE   float   xyzw
                // TEXCOORDS                0   xy          2     NONE   float   xy  
                // COLOR                    0   xyzw        3     NONE   float   xyzw
                // POSITION                 0   xyz         4     NONE   float   xyz 
                //
                //
                // Output signature:
                //
                // Name                 Index   Mask Register SysValue  Format   Used
                // -------------------- ----- ------ -------- -------- ------- ------
                // SV_Target                0   xyzw        0   TARGET   float   xyzw
                // SV_Target                1   xyzw        1   TARGET   float   xyzw
                // SV_DEPTH                 0    N/A   oDepth    DEPTH   float    YES
                //
                ps_4_0
                dcl_constantbuffer CB1[9], immediateIndexed
                dcl_sampler s0, mode_default
                dcl_resource_texture2d (float,float,float,float) t1
                dcl_input_ps linear v1.xyzw
                dcl_input_ps linear v2.xy
                dcl_input_ps linear v3.xyzw
                dcl_input_ps linear v4.xyz
                dcl_output o0.xyzw
                dcl_output o1.xyzw
                dcl_output oDepth
                dcl_temps 2
                //
                // Initial variable locations:
                //   v0.x <- IN.Position.x; v0.y <- IN.Position.y; v0.z <- IN.Position.z; v0.w <- IN.Position.w; 
                //   v1.x <- IN.Normal.x; v1.y <- IN.Normal.y; v1.z <- IN.Normal.z; v1.w <- IN.Normal.w; 
                //   v2.x <- IN.UV.x; v2.y <- IN.UV.y; 
                //   v3.x <- IN.Color.x; v3.y <- IN.Color.y; v3.z <- IN.Color.z; v3.w <- IN.Color.w; 
                //   v4.x <- IN.WorldPosition.x; v4.y <- IN.WorldPosition.y; v4.z <- IN.WorldPosition.z; 
                //   o1.x <- <PS return value>.Color.x; o1.y <- <PS return value>.Color.y; o1.z <- <PS return value>.Color.z; o1.w <- <PS return value>.Color.w; 
                //   o0.x <- <PS return value>.Normal.x; o0.y <- <PS return value>.Normal.y; o0.z <- <PS return value>.Normal.z; o0.w <- <PS return value>.Normal.w; 
                //   oDepth <- <PS return value>.Depth
                //
                #line 68 "C:\Users\Ryzen\git\AEther\AEther.WindowsForms\bin\Debug\net6.0-windows\geometry.fx"
                sample r0.xyzw, v2.xyxx, t1.xyzw, s0  // r0.x <- color.x; r0.y <- color.y; r0.z <- color.z; r0.w <- color.w
                
                #line 70
                eq r1.x, r0.w, l(0.000000)
                discard_nz r1.x
                
                #line 74
                add r1.xyz, v4.xyzx, -cb1[8].xyzx
                dp3 r1.x, r1.xyzx, r1.xyzx
                sqrt r1.x, r1.x
                div oDepth, r1.x, cb1[8].w
                
                #line 75
                dp3 r1.x, v1.xyzx, v1.xyzx
                rsq r1.x, r1.x
                mul o0.xyz, r1.xxxx, v1.xyzx
                
                #line 77
                mov r0.w, l(1.000000)
                mul o1.xyzw, r0.xyzw, v3.xyzw
                
                #line 78
                mov o0.w, v1.w
                ret 
                // Approximately 14 instruction slots used
                            
            };
        }

    }

}

