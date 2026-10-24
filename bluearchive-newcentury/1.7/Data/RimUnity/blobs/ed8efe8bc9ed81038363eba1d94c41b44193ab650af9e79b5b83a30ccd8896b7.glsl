#version 450
layout(binding = 3, set = 2)
uniform samplerCube _CubeMap; // @default = gray
layout(binding = 21, set = 2)
uniform sampler2D _NormalTex; // @default = white
layout(binding = 7, set = 2)
uniform sampler2D _DissloveTexPlus; // @default = black
layout(binding = 5, set = 2)
uniform sampler2D _DissloveTex; // @default = white
layout(binding = 1, set = 2)
uniform sampler2D _AddTex; // @default = white

layout(binding = 15, set = 2)
uniform sampler2D _MainTex; // @default = white
layout(binding = 17, set = 2)
uniform sampler2D _MaskPlusTex; // @default = white
layout(binding = 19, set = 2)
uniform sampler2D _MaskTex; // @default = white
layout(binding = 11, set = 2)
uniform sampler2D _DistortTex; // @default = white
layout(binding = 9, set = 2)
uniform sampler2D _DistortMaskTex; // @default = white
layout(binding = 23, set = 2)
uniform sampler2D _ParaTex; // @default = white
layout(std430, set = 2, binding = 35) readonly buffer _UiRectMasks { vec4 rows[]; } rectangles;
layout(std430, set = 2, binding = 34) readonly buffer _UiMaskRegions { vec4 rows[]; } regions;
layout(std430, set = 2, binding = 33) readonly buffer _UiMaskPixels { uint values[]; } maskPixels;
layout(std430, set = 2, binding = 32) readonly buffer _UiMasks { vec4 rows[]; } masks;


#line 59 0
struct RU_MaterialData_0
{
    float _AddTexAC; // @default = 0
    float _AddTexAR; // @default = 0
    float _AddTexBlend; // @default = 0
    float _AddTexBlendMode; // @default = 0
    float _AddTexC; // @default = 0
    float _AddTexCV; // @default = 0
    vec4 _AddTexColor; // @default = vec4(0, 0, 0, 0) @color
    vec4 _AddTexRefine; // @default = vec4(1, 1, 2, 0)
    float _AddTexRo; // @default = 0
    float _AddTexUVS; // @default = 0
    float _AddTexUspeed; // @default = 0
    float _AddTexVspeed; // @default = 0
    vec4 _AddTex_ST; // @default = vec4(1, 1, 0, 0)
    float _AlphaAdd; // @default = 0
    vec4 _BackFaceColor; // @default = vec4(1, 1, 1, 0) @color
    float _CAddTexUV; // @default = 0
    float _CAddTexUVT; // @default = 1
    vec4 _CameraDepthTexture_TexelSize;
    float _CenterU; // @default = 0.5
    float _CenterV; // @default = 0.5
    float _Comparison; // @default = 8
    float _CubemapScale; // @default = 0
    float _Cullmode; // @default = 0
    float _CustomDistort; // @default = 0
    float _CustomVAT; // @default = 0
    float _CustomdataDis; // @default = 0
    float _CustomdataDisT; // @default = 0
    float _CustomdataMainTexUV; // @default = 0
    float _CustomdataMaskUV; // @default = 0
    vec4 _DIssloveColor; // @default = vec4(1, 1, 1, 1) @color
    float _DIssloveFactor; // @default = 0.5
    float _DIssloveSoft; // @default = 0.5
    float _DIssloveWide; // @default = 0.1
    float _DIssolve_rotat; // @default = 0
    vec4 _DepthColor; // @default = vec4(0, 0, 0, 0) @color
    float _DepthF; // @default = 0
    float _DepthfadeFactor; // @default = 1
    float _Depthfadeon; // @default = 0
    vec3 _Dir; // @default = vec3(0, 0, 0)
    float _DisTex_Uspeed; // @default = 0
    float _DisTex_Vspeed; // @default = 0
    vec4 _DissloveTexPlus_ST; // @default = vec4(1, 1, 0, 0)
    vec4 _DissloveTex_ST; // @default = vec4(1, 1, 0, 0)
    float _DissolveAR; // @default = 1
    float _DissolveC; // @default = 0
    float _DissolveCV; // @default = 0
    float _DissolveFactorC1; // @default = 1
    float _DissolveFactorC2; // @default = 2
    float _DissolveOffsetUC1; // @default = 1
    float _DissolveOffsetUC2; // @default = 0
    float _DissolveOffsetVC1; // @default = 1
    float _DissolveOffsetVC2; // @default = 1
    float _DissolvePlusAR; // @default = 1
    float _DissolvePlusC; // @default = 0
    float _DissolvePlusCV; // @default = 0
    float _DissolvePlusR; // @default = 0
    float _DissolveTexDivide; // @default = 1
    float _DissolveTexExp; // @default = 1
    float _DissolveTexUVS2; // @default = 0
    float _DistortAddTex; // @default = 1
    float _DistortDisTex; // @default = 0
    float _DistortFactor; // @default = 0
    float _DistortFactorC1; // @default = 1
    float _DistortFactorC2; // @default = 3
    float _DistortMainTex; // @default = 1
    float _DistortMask; // @default = 0
    float _DistortMaskTexAR; // @default = 1
    float _DistortMaskTexC; // @default = 0
    float _DistortMaskTexCV; // @default = 0
    float _DistortMaskTexR; // @default = 0
    vec4 _DistortMaskTex_ST; // @default = vec4(1, 1, 0, 0)
    float _DistortNormalTex; // @default = 1
    float _DistortRemap; // @default = 0
    float _DistortTexAR; // @default = 1
    vec4 _DistortTex_ST; // @default = vec4(1, 1, 0, 0)
    float _DistortTex_Uspeed; // @default = 0
    float _DistortTex_Vspeed; // @default = 0
    float _Dst; // @default = 10
    float _FNLfanxiangkaiguan; // @default = 0
    float _Fail; // @default = 0
    float _IfAddTex; // @default = 0
    float _IfAddTexAlpha; // @default = 0
    float _IfAddTexColor; // @default = 1
    float _IfCubemap; // @default = 0
    float _IfCustomLight; // @default = 0
    float _IfDissolveColor; // @default = 1
    float _IfDissolveOffsetC; // @default = 0
    float _IfDissolvePlus; // @default = 0
    float _IfFNLAlpha; // @default = 0
    float _IfFlowmap; // @default = 0
    float _IfMaskColor; // @default = 0
    float _IfMaskPlusTex; // @default = 0
    float _IfNormalDistort; // @default = 0
    float _IfPara; // @default = 0
    float _IfStaticNormal; // @default = 0
    float _IfVAT; // @default = 0
    float _LightScale; // @default = 0
    float _MainAlpha; // @default = 1
    vec4 _MainColor; // @default = vec4(1, 1, 1, 1) @color
    float _MainOffsetUC1; // @default = 0
    float _MainOffsetUC2; // @default = 0
    float _MainOffsetVC1; // @default = 0
    float _MainOffsetVC2; // @default = 1
    float _MainTexAC; // @default = 0
    vec4 _MainTexRefine; // @default = vec4(1, 1, 2, 0)
    vec4 _MainTex_ST; // @default = vec4(1, 1, 0, 0)
    float _MainTex_Uspeed; // @default = 0
    float _MainTex_Vspeed; // @default = 0
    float _MainTex_ar; // @default = 0
    float _MainTex_rotat; // @default = 0
    float _MaintexC; // @default = 0
    float _MaintexCV; // @default = 0
    float _MaskAlphaRA; // @default = 1
    float _MaskC; // @default = 0
    float _MaskCV; // @default = 0
    float _MaskOffsetUC1; // @default = 0
    float _MaskOffsetUC2; // @default = 2
    float _MaskOffsetVC1; // @default = 0
    float _MaskOffsetVC2; // @default = 3
    float _MaskPlusAR; // @default = 1
    float _MaskPlusC; // @default = 0
    float _MaskPlusCV; // @default = 0
    float _MaskPlusR; // @default = 0
    vec4 _MaskPlusTex_ST; // @default = vec4(1, 1, 0, 0)
    float _MaskPlusUspeed; // @default = 0
    float _MaskPlusVspeed; // @default = 0
    float _MaskTexUVS; // @default = 0
    vec4 _MaskTex_ST; // @default = vec4(1, 1, 0, 0)
    float _Mask_Uspeed; // @default = 0
    float _Mask_Vspeed; // @default = 0
    float _Mask_rotat; // @default = 0
    float _Mask_scale; // @default = 1
    float _NormalScale; // @default = 0
    float _NormalTexC; // @default = 0
    float _NormalTexCV; // @default = 0
    float _NormalTex_Rotat; // @default = 0
    vec4 _NormalTex_ST; // @default = vec4(1, 1, 0, 0)
    float _NormalTex_Uspeed; // @default = 0
    float _NormalTex_Vspeed; // @default = 0
    vec4 _ParaTex_ST; // @default = vec4(0, 0, 1, 1)
    float _Parallax; // @default = 0
    float _ParticleVAT; // @default = 0
    float _Pass; // @default = 0
    vec4 _ProjectionParams;
    vec4 _RU_MapAmbient;
    vec4 _RU_MapMoonColor;
    vec4 _RU_MapMoonDirection;
    vec4 _RU_MapSunColor;
    vec4 _RU_MapSunDirection;
    float _Reference; // @default = 0
    mat4 _RimUnity_DepthInvVP;
    mat4 _RimUnity_DepthVP;
    float _Scr; // @default = 5
    float _ScreenAsMain; // @default = 0
    float _StaticNormalOffset; // @default = 0
    float _StaticNormalScale; // @default = 0
    float _StencilStyle; // @default = 0
    vec4 _TexCenter; // @default = vec4(0, 0, 0, 0)
    vec4 _Time;
    float _ToggleSwitch0; // @default = 0
    float _VATFrameC1; // @default = 1
    float _VATFrameC2; // @default = 4
    float _VATFrameFactor; // @default = 0
    vec4 _VATPositionTex_TexelSize;
    float _VATTime; // @default = 0
    float _VTOAR; // @default = 1
    float _VTOC; // @default = 0
    float _VTOCV; // @default = 0
    float _VTOFactor; // @default = 0
    float _VTOFactorC1; // @default = 1
    float _VTOFactorC2; // @default = 4
    float _VTOMaskAR; // @default = 1
    float _VTOMaskC; // @default = 0
    float _VTOMaskCV; // @default = 0
    float _VTOMaskR; // @default = 0
    vec4 _VTOMaskTex_ST; // @default = vec4(1, 1, 0, 0)
    float _VTOR; // @default = 0
    float _VTORemap; // @default = 0
    float _VTOTexExp; // @default = 1
    vec4 _VTOTex_ST; // @default = vec4(1, 1, 0, 0)
    float _VTOTex_Uspeed; // @default = 0
    float _VTOTex_Vspeed; // @default = 0
    vec3 _WorldSpaceCameraPos;
    vec4 _ZBufferParams;
    float _Ztest; // @default = 4
    float _Zwrite; // @default = 0
    vec4 _fnl_color; // @default = vec4(1, 1, 1, 0) @color
    float _fnl_power; // @default = 1
    float _fnl_sacle; // @default = 0
    float _qubaohedu; // @default = 0
    float _screenVTOon; // @default = 0
    float _softFacotr; // @default = 1
    float _soft_sting; // @default = 0
    float _softback; // @default = 0
    float _sot_sting_A; // @default = 0
    vec4 unity_OrthoParams;
};


#line 257
layout(std140, set = 2, binding = 36) readonly buffer _RimUnityMaterial {
    float _AddTexAC; // @default = 0
    float _AddTexAR; // @default = 0
    float _AddTexBlend; // @default = 0
    float _AddTexBlendMode; // @default = 0
    float _AddTexC; // @default = 0
    float _AddTexCV; // @default = 0
    vec4 _AddTexColor; // @default = vec4(0, 0, 0, 0) @color
    vec4 _AddTexRefine; // @default = vec4(1, 1, 2, 0)
    float _AddTexRo; // @default = 0
    float _AddTexUVS; // @default = 0
    float _AddTexUspeed; // @default = 0
    float _AddTexVspeed; // @default = 0
    vec4 _AddTex_ST; // @default = vec4(1, 1, 0, 0)
    float _AlphaAdd; // @default = 0
    vec4 _BackFaceColor; // @default = vec4(1, 1, 1, 0) @color
    float _CAddTexUV; // @default = 0
    float _CAddTexUVT; // @default = 1
    float _CenterU; // @default = 0.5
    float _CenterV; // @default = 0.5
    float _CubemapScale; // @default = 0
    float _CustomDistort; // @default = 0
    float _CustomVAT; // @default = 0
    float _CustomdataDis; // @default = 0
    float _CustomdataDisT; // @default = 0
    float _CustomdataMainTexUV; // @default = 0
    float _CustomdataMaskUV; // @default = 0
    vec4 _DIssloveColor; // @default = vec4(1, 1, 1, 1) @color
    float _DIssloveFactor; // @default = 0.5
    float _DIssloveSoft; // @default = 0.5
    float _DIssloveWide; // @default = 0.1
    float _DIssolve_rotat; // @default = 0
    vec4 _DepthColor; // @default = vec4(0, 0, 0, 0) @color
    float _DepthF; // @default = 0
    float _DepthfadeFactor; // @default = 1
    float _Depthfadeon; // @default = 0
    vec4 _Dir; // @default = vec4(0, 0, 0, 0)
    float _DisTex_Uspeed; // @default = 0
    float _DisTex_Vspeed; // @default = 0
    vec4 _DissloveTexPlus_ST; // @default = vec4(1, 1, 0, 0)
    vec4 _DissloveTex_ST; // @default = vec4(1, 1, 0, 0)
    float _DissolveAR; // @default = 1
    float _DissolveC; // @default = 0
    float _DissolveCV; // @default = 0
    float _DissolveFactorC1; // @default = 1
    float _DissolveFactorC2; // @default = 2
    float _DissolveOffsetUC1; // @default = 1
    float _DissolveOffsetUC2; // @default = 0
    float _DissolveOffsetVC1; // @default = 1
    float _DissolveOffsetVC2; // @default = 1
    float _DissolvePlusAR; // @default = 1
    float _DissolvePlusC; // @default = 0
    float _DissolvePlusCV; // @default = 0
    float _DissolvePlusR; // @default = 0
    float _DissolveTexDivide; // @default = 1
    float _DissolveTexExp; // @default = 1
    float _DissolveTexUVS2; // @default = 0
    float _DistortAddTex; // @default = 1
    float _DistortDisTex; // @default = 0
    float _DistortFactor; // @default = 0
    float _DistortFactorC1; // @default = 1
    float _DistortFactorC2; // @default = 3
    float _DistortMainTex; // @default = 1
    float _DistortMask; // @default = 0
    float _DistortMaskTexAR; // @default = 1
    float _DistortMaskTexC; // @default = 0
    float _DistortMaskTexCV; // @default = 0
    float _DistortMaskTexR; // @default = 0
    vec4 _DistortMaskTex_ST; // @default = vec4(1, 1, 0, 0)
    float _DistortNormalTex; // @default = 1
    float _DistortRemap; // @default = 0
    float _DistortTexAR; // @default = 1
    vec4 _DistortTex_ST; // @default = vec4(1, 1, 0, 0)
    float _DistortTex_Uspeed; // @default = 0
    float _DistortTex_Vspeed; // @default = 0
    float _FNLfanxiangkaiguan; // @default = 0
    float _IfAddTex; // @default = 0
    float _IfAddTexAlpha; // @default = 0
    float _IfAddTexColor; // @default = 1
    float _IfCubemap; // @default = 0
    float _IfCustomLight; // @default = 0
    float _IfDissolveColor; // @default = 1
    float _IfDissolveOffsetC; // @default = 0
    float _IfDissolvePlus; // @default = 0
    float _IfFNLAlpha; // @default = 0
    float _IfFlowmap; // @default = 0
    float _IfMaskColor; // @default = 0
    float _IfMaskPlusTex; // @default = 0
    float _IfNormalDistort; // @default = 0
    float _IfPara; // @default = 0
    float _IfStaticNormal; // @default = 0
    float _IfVAT; // @default = 0
    float _LightScale; // @default = 0
    float _MainAlpha; // @default = 1
    vec4 _MainColor; // @default = vec4(1, 1, 1, 1) @color
    float _MainOffsetUC1; // @default = 0
    float _MainOffsetUC2; // @default = 0
    float _MainOffsetVC1; // @default = 0
    float _MainOffsetVC2; // @default = 1
    float _MainTexAC; // @default = 0
    vec4 _MainTexRefine; // @default = vec4(1, 1, 2, 0)
    vec4 _MainTex_ST; // @default = vec4(1, 1, 0, 0)
    float _MainTex_Uspeed; // @default = 0
    float _MainTex_Vspeed; // @default = 0
    float _MainTex_ar; // @default = 0
    float _MainTex_rotat; // @default = 0
    float _MaintexC; // @default = 0
    float _MaintexCV; // @default = 0
    float _MaskAlphaRA; // @default = 1
    float _MaskC; // @default = 0
    float _MaskCV; // @default = 0
    float _MaskOffsetUC1; // @default = 0
    float _MaskOffsetUC2; // @default = 2
    float _MaskOffsetVC1; // @default = 0
    float _MaskOffsetVC2; // @default = 3
    float _MaskPlusAR; // @default = 1
    float _MaskPlusC; // @default = 0
    float _MaskPlusCV; // @default = 0
    float _MaskPlusR; // @default = 0
    vec4 _MaskPlusTex_ST; // @default = vec4(1, 1, 0, 0)
    float _MaskPlusUspeed; // @default = 0
    float _MaskPlusVspeed; // @default = 0
    float _MaskTexUVS; // @default = 0
    vec4 _MaskTex_ST; // @default = vec4(1, 1, 0, 0)
    float _Mask_Uspeed; // @default = 0
    float _Mask_Vspeed; // @default = 0
    float _Mask_rotat; // @default = 0
    float _Mask_scale; // @default = 1
    float _NormalScale; // @default = 0
    float _NormalTexC; // @default = 0
    float _NormalTexCV; // @default = 0
    float _NormalTex_Rotat; // @default = 0
    vec4 _NormalTex_ST; // @default = vec4(1, 1, 0, 0)
    float _NormalTex_Uspeed; // @default = 0
    float _NormalTex_Vspeed; // @default = 0
    vec4 _ParaTex_ST; // @default = vec4(0, 0, 1, 1)
    float _Parallax; // @default = 0
    float _ParticleVAT; // @default = 0
    vec4 _ProjectionParams;
    vec4 _RU_MapMoonColor;
    vec4 _RU_MapMoonDirection;
    vec4 _RU_MapSunColor;
    vec4 _RU_MapSunDirection;
    mat4 _RimUnity_DepthInvVP;
    mat4 _RimUnity_DepthVP;
    float _ScreenAsMain; // @default = 0
    float _StaticNormalOffset; // @default = 0
    float _StaticNormalScale; // @default = 0
    vec4 _TexCenter; // @default = vec4(0, 0, 0, 0)
    vec4 _Time;
    float _ToggleSwitch0; // @default = 0
    float _VATFrameC1; // @default = 1
    float _VATFrameC2; // @default = 4
    float _VATFrameFactor; // @default = 0
    vec4 _VATPositionTex_TexelSize;
    float _VATTime; // @default = 0
    float _VTOAR; // @default = 1
    float _VTOC; // @default = 0
    float _VTOCV; // @default = 0
    float _VTOFactor; // @default = 0
    float _VTOFactorC1; // @default = 1
    float _VTOFactorC2; // @default = 4
    float _VTOMaskAR; // @default = 1
    float _VTOMaskC; // @default = 0
    float _VTOMaskCV; // @default = 0
    float _VTOMaskR; // @default = 0
    vec4 _VTOMaskTex_ST; // @default = vec4(1, 1, 0, 0)
    float _VTOR; // @default = 0
    float _VTORemap; // @default = 0
    float _VTOTexExp; // @default = 1
    vec4 _VTOTex_ST; // @default = vec4(1, 1, 0, 0)
    float _VTOTex_Uspeed; // @default = 0
    float _VTOTex_Vspeed; // @default = 0
    vec4 _WorldSpaceCameraPos;
    vec4 _ZBufferParams;
    vec4 _fnl_color; // @default = vec4(1, 1, 1, 0) @color
    float _fnl_power; // @default = 1
    float _fnl_sacle; // @default = 0
    float _qubaohedu; // @default = 0
    float _screenVTOon; // @default = 0
    float _softFacotr; // @default = 1
    float _soft_sting; // @default = 0
    float _softback; // @default = 0
    float _sot_sting_A; // @default = 0
    vec4 unity_OrthoParams;

float _UiMaskCount; // @default = 0.0
float _UiRectMaskCount; // @default = 0.0
} RU_Material_0;

#line 616
layout(binding = 31, set = 2)
uniform sampler2D _VTOTex; // @default = white


#line 618



#line 605
layout(binding = 29, set = 2)
uniform sampler2D _VTOMaskTex; // @default = white


#line 607



#line 594
layout(binding = 27, set = 2)
uniform sampler2D _VATPositionTex; // @default = black


#line 596



#line 583
layout(binding = 25, set = 2)
uniform sampler2D _VATNormalTex; // @default = black


#line 585



#line 15
struct RU_CameraData_0
{
    mat4 ViewProj_0;
};


#line 16
layout(binding = 0, std140) uniform block_RU_CameraData_0
{
    mat4 ViewProj_0;
}RU_Camera_0;

#line 17
struct RU_ObjectData_0
{
    mat4 model_0;
};


#line 18
layout(push_constant, std430) uniform block_RU_ObjectData_0
{
    mat4 model_0;
}RU_Object_0;

#line 14946 1
float saturate_0(float x_0)
{

#line 14954
    return clamp(x_0, 0.0, 1.0);
}


#line 623 0
vec4 RU_tex2Dlod_VTOTex_0(vec4 uv_0)
{

#line 623
    return (textureLod(_VTOTex, (vec2(uv_0.x, 1.0 - uv_0.y)), (uv_0.w)));
}


#line 612
vec4 RU_tex2Dlod_VTOMaskTex_0(vec4 uv_1)
{

#line 612
    return (textureLod(_VTOMaskTex, (vec2(uv_1.x, 1.0 - uv_1.y)), (uv_1.w)));
}


#line 656
float customchanelswitch45_g1078_0(float Custome_0, float xyzw_0, out float chanel_0, float C1x_0, float C1y_0, float C1z_0, float C1w_0, float C2x_0, float C2y_0, float C2z_0, float C2w_0, float none_0)
{

#line 656
    bool _S1 = Custome_0 == 0.0;

#line 656
    bool _S2;

#line 656
    if(_S1)
    {

#line 656
        _S2 = xyzw_0 == 0.0;

#line 656
    }
    else
    {

#line 656
        _S2 = false;

#line 656
    }

#line 656
    if(_S2)
    {

#line 656
        chanel_0 = C1x_0;

#line 656
    }
    else
    {

#line 656
        if(_S1)
        {

#line 656
            _S2 = xyzw_0 == 1.0;

#line 656
        }
        else
        {

#line 656
            _S2 = false;

#line 656
        }

#line 656
        if(_S2)
        {

#line 656
            chanel_0 = C1y_0;

#line 656
        }
        else
        {

#line 656
            if(_S1)
            {

#line 656
                _S2 = xyzw_0 == 2.0;

#line 656
            }
            else
            {

#line 656
                _S2 = false;

#line 656
            }

#line 656
            if(_S2)
            {

#line 656
                chanel_0 = C1z_0;

#line 656
            }
            else
            {

#line 656
                if(_S1)
                {

#line 656
                    _S2 = xyzw_0 == 3.0;

#line 656
                }
                else
                {

#line 656
                    _S2 = false;

#line 656
                }

#line 656
                if(_S2)
                {

#line 656
                    chanel_0 = C1w_0;

#line 656
                }
                else
                {

#line 656
                    if(_S1)
                    {

#line 656
                        _S2 = xyzw_0 == 4.0;

#line 656
                    }
                    else
                    {

#line 656
                        _S2 = false;

#line 656
                    }

#line 656
                    if(_S2)
                    {

#line 656
                        chanel_0 = 0.0;

#line 656
                    }
                    else
                    {

#line 656
                        bool _S3 = Custome_0 == 1.0;

#line 656
                        if(_S3)
                        {

#line 656
                            _S2 = xyzw_0 == 0.0;

#line 656
                        }
                        else
                        {

#line 656
                            _S2 = false;

#line 656
                        }

#line 656
                        if(_S2)
                        {

#line 656
                            chanel_0 = C2x_0;

#line 656
                        }
                        else
                        {

#line 656
                            if(_S3)
                            {

#line 656
                                _S2 = xyzw_0 == 1.0;

#line 656
                            }
                            else
                            {

#line 656
                                _S2 = false;

#line 656
                            }

#line 656
                            if(_S2)
                            {

#line 656
                                chanel_0 = C2y_0;

#line 656
                            }
                            else
                            {

#line 656
                                if(_S3)
                                {

#line 656
                                    _S2 = xyzw_0 == 2.0;

#line 656
                                }
                                else
                                {

#line 656
                                    _S2 = false;

#line 656
                                }

#line 656
                                if(_S2)
                                {

#line 656
                                    chanel_0 = C2z_0;

#line 656
                                }
                                else
                                {

#line 656
                                    if(_S3)
                                    {

#line 656
                                        _S2 = xyzw_0 == 3.0;

#line 656
                                    }
                                    else
                                    {

#line 656
                                        _S2 = false;

#line 656
                                    }

#line 656
                                    if(_S2)
                                    {

#line 656
                                        chanel_0 = C2w_0;

#line 656
                                    }
                                    else
                                    {

#line 656
                                        if(_S3)
                                        {

#line 656
                                            _S2 = xyzw_0 == 4.0;

#line 656
                                        }
                                        else
                                        {

#line 656
                                            _S2 = false;

#line 656
                                        }

#line 656
                                        if(_S2)
                                        {

#line 656
                                            chanel_0 = 0.0;

#line 656
                                        }

#line 656
                                    }

#line 656
                                }

#line 656
                            }

#line 656
                        }

#line 656
                    }

#line 656
                }

#line 656
            }

#line 656
        }

#line 656
    }

#line 656
    return 0.0;
}


#line 656
float customchanelswitch45_g1077_0(float Custome_1, float xyzw_1, out float chanel_1, float C1x_1, float C1y_1, float C1z_1, float C1w_1, float C2x_1, float C2y_1, float C2z_1, float C2w_1, float none_1)
{

#line 656
    bool _S4 = Custome_1 == 0.0;

#line 656
    bool _S5;

#line 656
    if(_S4)
    {

#line 656
        _S5 = xyzw_1 == 0.0;

#line 656
    }
    else
    {

#line 656
        _S5 = false;

#line 656
    }

#line 656
    if(_S5)
    {

#line 656
        chanel_1 = C1x_1;

#line 656
    }
    else
    {

#line 656
        if(_S4)
        {

#line 656
            _S5 = xyzw_1 == 1.0;

#line 656
        }
        else
        {

#line 656
            _S5 = false;

#line 656
        }

#line 656
        if(_S5)
        {

#line 656
            chanel_1 = C1y_1;

#line 656
        }
        else
        {

#line 656
            if(_S4)
            {

#line 656
                _S5 = xyzw_1 == 2.0;

#line 656
            }
            else
            {

#line 656
                _S5 = false;

#line 656
            }

#line 656
            if(_S5)
            {

#line 656
                chanel_1 = C1z_1;

#line 656
            }
            else
            {

#line 656
                if(_S4)
                {

#line 656
                    _S5 = xyzw_1 == 3.0;

#line 656
                }
                else
                {

#line 656
                    _S5 = false;

#line 656
                }

#line 656
                if(_S5)
                {

#line 656
                    chanel_1 = C1w_1;

#line 656
                }
                else
                {

#line 656
                    if(_S4)
                    {

#line 656
                        _S5 = xyzw_1 == 4.0;

#line 656
                    }
                    else
                    {

#line 656
                        _S5 = false;

#line 656
                    }

#line 656
                    if(_S5)
                    {

#line 656
                        chanel_1 = 0.0;

#line 656
                    }
                    else
                    {

#line 656
                        bool _S6 = Custome_1 == 1.0;

#line 656
                        if(_S6)
                        {

#line 656
                            _S5 = xyzw_1 == 0.0;

#line 656
                        }
                        else
                        {

#line 656
                            _S5 = false;

#line 656
                        }

#line 656
                        if(_S5)
                        {

#line 656
                            chanel_1 = C2x_1;

#line 656
                        }
                        else
                        {

#line 656
                            if(_S6)
                            {

#line 656
                                _S5 = xyzw_1 == 1.0;

#line 656
                            }
                            else
                            {

#line 656
                                _S5 = false;

#line 656
                            }

#line 656
                            if(_S5)
                            {

#line 656
                                chanel_1 = C2y_1;

#line 656
                            }
                            else
                            {

#line 656
                                if(_S6)
                                {

#line 656
                                    _S5 = xyzw_1 == 2.0;

#line 656
                                }
                                else
                                {

#line 656
                                    _S5 = false;

#line 656
                                }

#line 656
                                if(_S5)
                                {

#line 656
                                    chanel_1 = C2z_1;

#line 656
                                }
                                else
                                {

#line 656
                                    if(_S6)
                                    {

#line 656
                                        _S5 = xyzw_1 == 3.0;

#line 656
                                    }
                                    else
                                    {

#line 656
                                        _S5 = false;

#line 656
                                    }

#line 656
                                    if(_S5)
                                    {

#line 656
                                        chanel_1 = C2w_1;

#line 656
                                    }
                                    else
                                    {

#line 656
                                        if(_S6)
                                        {

#line 656
                                            _S5 = xyzw_1 == 4.0;

#line 656
                                        }
                                        else
                                        {

#line 656
                                            _S5 = false;

#line 656
                                        }

#line 656
                                        if(_S5)
                                        {

#line 656
                                            chanel_1 = 0.0;

#line 656
                                        }

#line 656
                                    }

#line 656
                                }

#line 656
                            }

#line 656
                        }

#line 656
                    }

#line 656
                }

#line 656
            }

#line 656
        }

#line 656
    }

#line 656
    return 0.0;
}


#line 601
vec4 RU_tex2Dlod_VATPositionTex_0(vec4 uv_2)
{

#line 601
    return (textureLod(_VATPositionTex, (vec2(uv_2.x, 1.0 - uv_2.y)), (uv_2.w)));
}


#line 656
vec3 RotateAroundAxis_0(vec3 center_0, vec3 original_0, vec3 u_0, float angle_0)
{

#line 656
    float C_0 = cos(angle_0);

#line 656
    float S_0 = sin(angle_0);

#line 656
    float t_0 = 1.0 - C_0;

#line 656
    float _S7 = u_0.x;

#line 656
    float _S8 = t_0 * _S7;

#line 656
    float _S9 = u_0.y;

#line 656
    float _S10 = _S8 * _S9;

#line 656
    float _S11 = u_0.z;

#line 656
    float _S12 = S_0 * _S11;

#line 656
    float _S13 = _S8 * _S11;

#line 656
    float _S14 = S_0 * _S9;

#line 656
    float _S15 = t_0 * _S9;

#line 656
    float _S16 = _S15 * _S11;

#line 656
    float _S17 = S_0 * _S7;

#line 656
    return (((original_0 - center_0) * (mat3(_S8 * _S7 + C_0, _S10 - _S12, _S13 + _S14, _S10 + _S12, _S15 * _S9 + C_0, _S16 - _S17, _S13 - _S14, _S16 + _S17, t_0 * _S11 * _S11 + C_0)))) + center_0;
}


#line 590
vec4 RU_tex2Dlod_VATNormalTex_0(vec4 uv_3)
{

#line 590
    return (textureLod(_VATNormalTex, (vec2(uv_3.x, 1.0 - uv_3.y)), (uv_3.w)));
}


#line 23
vec4 UnityObjectToClipPos_0(vec4 p_0)
{

#line 23
    return ((((((p_0) * (transpose(RU_Object_0.model_0))))) * (transpose(RU_Camera_0.ViewProj_0))));
}


#line 24
vec4 UnityObjectToClipPos_1(vec3 p_1)
{

#line 24
    return UnityObjectToClipPos_0(vec4(p_1, 1.0));
}


#line 35
vec4 ComputeScreenPos_0(vec4 p_2)
{

#line 35
    return vec4(p_2.xy * 0.5 + p_2.w * 0.5, p_2.zw);
}


#line 656
struct appdata_full_custom_0
{
    vec4 vertex_0;
    vec4 tangent_0;
    vec3 normal_0;
    vec4 texcoord_0;
    vec4 texcoord1_0;
    vec4 texcoord2_0;
    vec4 texcoord3_0;
    vec4 color_0;
    vec4 ase_texcoord7_0;
    vec4 ase_texcoord4_0;
};


#line 656
struct Input_0
{
    vec4 uv4_texcoord4_0;
    vec3 worldPos_0;
    vec4 uv_texcoord_0;
    vec3 viewDir_0;
    vec3 internalSurfaceTtoW0_0;
    vec3 internalSurfaceTtoW1_0;
    vec3 internalSurfaceTtoW2_0;
    vec3 worldNormal_0;
    vec4 uv2_texcoord2_0;
    vec4 uv3_texcoord3_0;
    vec4 vertexColor_0;
    vec4 screenPos_0;
    vec4 screenPosition88_0;
    vec3 worldRefl_0;
    float ASEIsFrontFacing_0;
    vec3 ruLocalPosition_0;
};


#line 657
void vertexDataFunc_0(inout appdata_full_custom_0 v_0, out Input_0 o_0)
{

#line 657
    const vec4 _S18 = vec4(0.0);

#line 657
    const vec3 _S19 = vec3(0.0);

#line 657
    o_0.uv4_texcoord4_0 = _S18;

#line 657
    o_0.worldPos_0 = _S19;

#line 657
    o_0.uv_texcoord_0 = _S18;

#line 657
    o_0.viewDir_0 = _S19;

#line 657
    o_0.internalSurfaceTtoW0_0 = _S19;

#line 657
    o_0.internalSurfaceTtoW1_0 = _S19;

#line 657
    o_0.internalSurfaceTtoW2_0 = _S19;

#line 657
    o_0.worldNormal_0 = _S19;

#line 657
    o_0.uv2_texcoord2_0 = _S18;

#line 657
    o_0.uv3_texcoord3_0 = _S18;

#line 657
    o_0.vertexColor_0 = _S18;

#line 657
    o_0.screenPos_0 = _S18;

#line 657
    o_0.screenPosition88_0 = _S18;

#line 657
    o_0.worldRefl_0 = _S19;

#line 657
    o_0.ASEIsFrontFacing_0 = 0.0;

#line 657
    o_0.ruLocalPosition_0 = _S19;

#line 425
    vec4 break36_g1064_0 = RU_Material_0._VTOTex_ST.zwxy;

#line 657
    vec3 ase_vertex3Pos_0 = v_0.vertex_0.xyz;

#line 657
    float cos55_g1066_0 = cos(RU_Material_0._VTOR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1066_0 = sin(RU_Material_0._VTOR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    const vec2 _S20 = vec2(0.5, 0.5);

#line 657
    vec2 rotator55_g1066_0 = (((mat2(cos55_g1066_0, - sin55_g1066_0, sin55_g1066_0, cos55_g1066_0)) * (RU_Material_0._Time.y * vec2(RU_Material_0._VTOTex_Uspeed, RU_Material_0._VTOTex_Vspeed) + (v_0.texcoord_0.xy * vec2(break36_g1064_0.x, break36_g1064_0.y) + vec2(break36_g1064_0.z, break36_g1064_0.w)) - _S20))) + _S20;

#line 657
    float _S21 = rotator55_g1066_0.x;

#line 657
    float clampResult60_g1066_0 = clamp(_S21, 0.0, 1.0);

#line 657
    float _S22 = rotator55_g1066_0.y;

#line 657
    float clampResult50_g1066_0 = clamp(_S22, 0.0, 1.0);

#line 657
    float _S23;

#line 657
    if(float(int(RU_Material_0._VTOC)) == 0.0)
    {

#line 657
        _S23 = _S21;

#line 657
    }
    else
    {

#line 657
        _S23 = clampResult60_g1066_0;

#line 657
    }

#line 657
    float _S24;

#line 657
    if(float(int(RU_Material_0._VTOCV)) == 0.0)
    {

#line 657
        _S24 = _S22;

#line 657
    }
    else
    {

#line 657
        _S24 = clampResult50_g1066_0;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1066_0 = RU_tex2Dlod_VTOTex_0(vec4(vec2(_S23, _S24), 0.0, 0.0));

#line 657
    if(float(int(RU_Material_0._VTOAR)) == 0.0)
    {

#line 657
        _S23 = tex2DNode27_g1066_0.w;

#line 657
    }
    else
    {

#line 657
        _S23 = tex2DNode27_g1066_0.x;

#line 657
    }

#line 657
    float temp_output_1513_0_0 = saturate_0(pow(_S23, RU_Material_0._VTOTexExp));

#line 416
    vec4 break36_g1062_0 = RU_Material_0._VTOMaskTex_ST.zwxy;

#line 657
    float cos263_0 = cos(RU_Material_0._VTOMaskR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin263_0 = sin(RU_Material_0._VTOMaskR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator263_0 = (((mat2(cos263_0, - sin263_0, sin263_0, cos263_0)) * (v_0.texcoord_0.xy * vec2(break36_g1062_0.x, break36_g1062_0.y) + vec2(break36_g1062_0.z, break36_g1062_0.w) - _S20))) + _S20;

#line 657
    float _S25 = rotator263_0.x;

#line 657
    float clampResult257_0 = clamp(_S25, 0.0, 1.0);

#line 657
    float _S26 = rotator263_0.y;

#line 657
    float clampResult373_0 = clamp(_S26, 0.0, 1.0);

#line 657
    if(bool(RU_Material_0._VTOMaskC))
    {

#line 657
        _S23 = clampResult257_0;

#line 657
    }
    else
    {

#line 657
        _S23 = _S25;

#line 657
    }

#line 657
    if(bool(RU_Material_0._VTOMaskCV))
    {

#line 657
        _S24 = clampResult373_0;

#line 657
    }
    else
    {

#line 657
        _S24 = _S26;

#line 657
    }

#line 657
    vec4 tex2DNode81_0 = RU_tex2Dlod_VTOMaskTex_0(vec4(vec2(_S23, _S24), 0.0, 0.0));

#line 657
    float chanel45_g1078_0 = 0.0;

#line 657
    float _S27 = customchanelswitch45_g1078_0(RU_Material_0._VTOFactorC1, RU_Material_0._VTOFactorC2, chanel45_g1078_0, v_0.texcoord_0.z, v_0.texcoord_0.w, v_0.texcoord1_0.x, v_0.texcoord1_0.y, v_0.texcoord1_0.z, v_0.texcoord1_0.w, v_0.texcoord2_0.xy.x, v_0.texcoord2_0.xy.y, 0.0);

#line 657
    float VTOFactorC1855_0 = chanel45_g1078_0;

#line 657
    if((RU_Material_0._VTORemap) == 0.0)
    {

#line 657
        _S23 = temp_output_1513_0_0;

#line 657
    }
    else
    {

#line 657
        _S23 = -0.5 + temp_output_1513_0_0;

#line 657
    }

#line 657
    if(bool(RU_Material_0._VTOMaskAR))
    {

#line 657
        _S24 = tex2DNode81_0.x;

#line 657
    }
    else
    {

#line 657
        _S24 = tex2DNode81_0.w;

#line 657
    }

#line 657
    float _S28 = _S23 * _S24;

#line 657
    if(bool(RU_Material_0._ToggleSwitch0))
    {

#line 657
        _S23 = VTOFactorC1855_0;

#line 657
    }
    else
    {

#line 657
        _S23 = RU_Material_0._VTOFactor;

#line 657
    }

#line 657
    float temp_output_78_0_0 = _S28 * _S23;

#line 657
    vec3 ase_vertexNormal_0 = v_0.normal_0.xyz;

#line 657
    float chanel45_g1077_0 = 0.0;

#line 657
    float _S29 = customchanelswitch45_g1077_0(RU_Material_0._VATFrameC1, RU_Material_0._VATFrameC2, chanel45_g1077_0, v_0.texcoord_0.z, v_0.texcoord_0.w, v_0.texcoord1_0.x, v_0.texcoord1_0.y, v_0.texcoord1_0.z, v_0.texcoord1_0.w, v_0.texcoord2_0.xy.x, v_0.texcoord2_0.xy.y, 0.0);

#line 657
    float VATFrameC1919_0 = chanel45_g1077_0;

#line 657
    float _S30 = RU_Material_0._VATPositionTex_TexelSize.x * (RU_Material_0._VATTime - 1.0);

#line 657
    if(bool(RU_Material_0._CustomVAT))
    {

#line 657
        _S23 = VATFrameC1919_0;

#line 657
    }
    else
    {

#line 657
        _S23 = RU_Material_0._VATFrameFactor;

#line 657
    }

#line 657
    vec4 _S31 = vec4(vec2(_S30 * _S23 + v_0.ase_texcoord7_0.xy.x, v_0.ase_texcoord7_0.xy.y), 0.0, 0.0);

#line 657
    vec4 tex2DNode1886_0 = RU_tex2Dlod_VATPositionTex_0(_S31);

#line 657
    vec3 appendResult1962_0 = vec3(tex2DNode1886_0.x, tex2DNode1886_0.y, tex2DNode1886_0.z);

#line 657
    vec3 appendResult50_g1068_0 = vec3(v_0.ase_texcoord4_0.x, v_0.ase_texcoord4_0.y, v_0.ase_texcoord4_0.z);

#line 657
    vec3 temp_output_28_0_g1068_0 = vec3(v_0.texcoord2_0.z, 0.0, 0.0) + vec3(0.0, 0.0, v_0.texcoord3_0.x);

#line 657
    const vec3 _S32 = vec3(0.0, 0.0, 0.0);

#line 657
    const vec3 _S33 = vec3(0.0, 1.0, 0.0);

#line 657
    vec3 _S34 = normalize(_S33);

#line 657
    vec3 rotatedValue41_g1068_0 = RotateAroundAxis_0(_S32, temp_output_28_0_g1068_0, _S34, 1.57079637050628662);

#line 657
    float temp_output_30_0_g1068_0 = atan(v_0.texcoord2_0.z / v_0.texcoord3_0.x);

#line 657
    float _S35 = v_0.texcoord3_0.y;

#line 657
    const vec3 _S36 = vec3(1.0, 0.0, 0.0);

#line 657
    vec3 rotatedValue33_g1068_0 = RotateAroundAxis_0(_S32, RotateAroundAxis_0(_S32, RotateAroundAxis_0(_S32, tex2DNode1886_0.xyz, normalize(_S36), _S35), normalize(RotateAroundAxis_0(_S32, vec3(0.0, 0.0, 1.0), _S36, _S35)), v_0.texcoord3_0.w), _S34, v_0.texcoord3_0.z);

#line 657
    vec3 rotatedValue37_g1068_0 = RotateAroundAxis_0(_S32, rotatedValue33_g1068_0, _S33, temp_output_30_0_g1068_0);

#line 657
    vec3 rotatedValue38_g1068_0 = RotateAroundAxis_0(_S32, rotatedValue33_g1068_0, _S33, temp_output_30_0_g1068_0 + 3.14159274101257324);

#line 657
    vec3 ifLocalVar40_g1068_0;

#line 657
    if((v_0.texcoord3_0.x) >= 0.0)
    {

#line 657
        ifLocalVar40_g1068_0 = rotatedValue37_g1068_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar40_g1068_0 = rotatedValue38_g1068_0;

#line 657
    }

#line 657
    vec3 rotatedValue43_g1068_0 = RotateAroundAxis_0(_S32, ifLocalVar40_g1068_0, normalize(rotatedValue41_g1068_0), - atan(v_0.texcoord2_0.w / length(temp_output_28_0_g1068_0)));

#line 657
    vec3 VTO82_0;

#line 657
    if((RU_Material_0._IfVAT) == 0.0)
    {

#line 657
        VTO82_0 = temp_output_78_0_0 * ase_vertexNormal_0;

#line 657
    }
    else
    {

#line 657
        if(bool(RU_Material_0._ParticleVAT))
        {

#line 657
            ifLocalVar40_g1068_0 = appendResult50_g1068_0 * rotatedValue43_g1068_0;

#line 657
        }
        else
        {

#line 657
            ifLocalVar40_g1068_0 = appendResult1962_0;

#line 657
        }

#line 657
        VTO82_0 = ifLocalVar40_g1068_0;

#line 657
    }

#line 657
    vec3 temp_output_1959_0_0;

#line 657
    if((RU_Material_0._screenVTOon) == 0.0)
    {

#line 657
        temp_output_1959_0_0 = VTO82_0;

#line 657
    }
    else
    {

#line 657
        temp_output_1959_0_0 = _S19;

#line 657
    }

#line 657
    v_0.vertex_0.xyz = v_0.vertex_0.xyz + temp_output_1959_0_0;

#line 657
    v_0.vertex_0[3] = 1.0;

#line 657
    vec4 VTONormal1902_0;

#line 657
    if((RU_Material_0._IfVAT) == 0.0)
    {

#line 657
        VTONormal1902_0 = vec4(ase_vertexNormal_0, 0.0);

#line 657
    }
    else
    {

#line 657
        VTONormal1902_0 = RU_tex2Dlod_VATNormalTex_0(_S31);

#line 657
    }

#line 657
    v_0.normal_0 = VTONormal1902_0.xyz;

#line 657
    o_0.screenPosition88_0 = ComputeScreenPos_0(UnityObjectToClipPos_1(ase_vertex3Pos_0));

#line 657
    return;
}


#line 28
vec3 UnityObjectToWorldNormal_0(vec3 normal_1)
{

#line 29
    mat3 _S37 = mat3(transpose(RU_Object_0.model_0)[0].xyz, transpose(RU_Object_0.model_0)[1].xyz, transpose(RU_Object_0.model_0)[2].xyz);
    mat3 cofactors_0 = mat3(cross(_S37[1], _S37[2]), cross(_S37[2], _S37[0]), cross(_S37[0], _S37[1]));
    return normalize((((normal_1) * (cofactors_0))) * float((int(sign((dot(_S37[0], cofactors_0[0])))))));
}


#line 33
vec3 UnityObjectToWorldDir_0(vec3 direction_0)
{

#line 33
    return normalize((((direction_0) * (mat3(transpose(RU_Object_0.model_0)[0].xyz, transpose(RU_Object_0.model_0)[1].xyz, transpose(RU_Object_0.model_0)[2].xyz)))));
}


#line 33
layout(location = 1)
out vec4 entryPointParam_RU_AseVert_uv4_texcoord4_0;


#line 33
layout(location = 5)
out vec3 entryPointParam_RU_AseVert_worldPos_0;


#line 33
layout(location = 6)
out vec4 entryPointParam_RU_AseVert_uv_texcoord_0;


#line 33
layout(location = 7)
out vec3 entryPointParam_RU_AseVert_viewDir_0;


#line 33
layout(location = 8)
out vec3 entryPointParam_RU_AseVert_internalSurfaceTtoW0_0;


#line 33
layout(location = 9)
out vec3 entryPointParam_RU_AseVert_internalSurfaceTtoW1_0;


#line 33
layout(location = 10)
out vec3 entryPointParam_RU_AseVert_internalSurfaceTtoW2_0;


#line 33
layout(location = 11)
out vec3 entryPointParam_RU_AseVert_worldNormal_0;


#line 33
layout(location = 14)
out vec4 entryPointParam_RU_AseVert_uv2_texcoord2_0;


#line 33
layout(location = 15)
out vec4 entryPointParam_RU_AseVert_uv3_texcoord3_0;


#line 33
layout(location = 16)
out vec4 entryPointParam_RU_AseVert_vertexColor_0;


#line 33
layout(location = 17)
out vec4 entryPointParam_RU_AseVert_screenPos_0;


#line 33
layout(location = 18)
out vec4 entryPointParam_RU_AseVert_screenPosition88_0;


#line 33
layout(location = 19)
out vec3 entryPointParam_RU_AseVert_worldRefl_0;


#line 33
layout(location = 20)
out vec3 entryPointParam_RU_AseVert_ruLocalPosition_0;


#line 33
layout(location = 0) in vec4 v_vertex_0; // @vertex Position required


#line 33
layout(location = 4) in vec4 v_tangent_0; // @vertex Tangent optional


#line 33
layout(location = 3) in vec3 v_normal_0; // @vertex Normal optional


#line 33
layout(location = 1) in vec4 v_texcoord_0; // @vertex TexCoord0 optional


#line 33
layout(location = 5) in vec4 v_texcoord1_0; // @vertex TexCoord1 optional


#line 33
layout(location = 6) in vec4 v_texcoord2_0; // @vertex TexCoord2 optional


#line 33
layout(location = 7) in vec4 v_texcoord3_0; // @vertex TexCoord3 optional


#line 33
layout(location = 2) in vec4 v_color_0; // @vertex Color optional


#line 33
layout(location = 11) in vec4 v_ase_texcoord7_0; // @vertex TexCoord7 optional


#line 33
layout(location = 8) in vec4 v_ase_texcoord4_0; // @vertex TexCoord4 optional


#line 657
struct RU_AseVaryings_0
{
    vec4 position_0;
    vec4 uv4_texcoord4_1;
    vec3 worldPos_1;
    vec4 uv_texcoord_1;
    vec3 viewDir_1;
    vec3 internalSurfaceTtoW0_1;
    vec3 internalSurfaceTtoW1_1;
    vec3 internalSurfaceTtoW2_1;
    vec3 worldNormal_1;
    vec4 uv2_texcoord2_1;
    vec4 uv3_texcoord3_1;
    vec4 vertexColor_1;
    vec4 screenPos_1;
    vec4 screenPosition88_1;
    vec3 worldRefl_1;
    vec3 ruLocalPosition_1;
};


#line 657
void main()
{

#line 657
    appdata_full_custom_0 _S38;

#line 657
    _S38.vertex_0 = v_vertex_0;

#line 657
    _S38.tangent_0 = v_tangent_0;

#line 657
    _S38.normal_0 = v_normal_0;

#line 657
    _S38.texcoord_0 = v_texcoord_0;

#line 657
    _S38.texcoord1_0 = v_texcoord1_0;

#line 657
    _S38.texcoord2_0 = v_texcoord2_0;

#line 657
    _S38.texcoord3_0 = v_texcoord3_0;

#line 657
    _S38.color_0 = v_color_0;

#line 657
    _S38.ase_texcoord7_0 = v_ase_texcoord7_0;

#line 657
    _S38.ase_texcoord4_0 = v_ase_texcoord4_0;

#line 657
    const vec4 _S39 = vec4(0.0);

#line 657
    const vec3 _S40 = vec3(0.0);

#line 657
    Input_0 custom_0;

#line 657
    custom_0.uv4_texcoord4_0 = _S39;

#line 657
    custom_0.worldPos_0 = _S40;

#line 657
    custom_0.uv_texcoord_0 = _S39;

#line 657
    custom_0.viewDir_0 = _S40;

#line 657
    custom_0.internalSurfaceTtoW0_0 = _S40;

#line 657
    custom_0.internalSurfaceTtoW1_0 = _S40;

#line 657
    custom_0.internalSurfaceTtoW2_0 = _S40;

#line 657
    custom_0.worldNormal_0 = _S40;

#line 657
    custom_0.uv2_texcoord2_0 = _S39;

#line 657
    custom_0.uv3_texcoord3_0 = _S39;

#line 657
    custom_0.vertexColor_0 = _S39;

#line 657
    custom_0.screenPos_0 = _S39;

#line 657
    custom_0.screenPosition88_0 = _S39;

#line 657
    custom_0.worldRefl_0 = _S40;

#line 657
    custom_0.ASEIsFrontFacing_0 = 0.0;

#line 657
    custom_0.ruLocalPosition_0 = _S40;

#line 657
    vertexDataFunc_0(_S38, custom_0);

#line 657
    RU_AseVaryings_0 o_1;

#line 657
    o_1.position_0 = _S39;

#line 657
    o_1.uv4_texcoord4_1 = _S39;

#line 657
    o_1.worldPos_1 = _S40;

#line 657
    o_1.uv_texcoord_1 = _S39;

#line 657
    o_1.viewDir_1 = _S40;

#line 657
    o_1.internalSurfaceTtoW0_1 = _S40;

#line 657
    o_1.internalSurfaceTtoW1_1 = _S40;

#line 657
    o_1.internalSurfaceTtoW2_1 = _S40;

#line 657
    o_1.worldNormal_1 = _S40;

#line 657
    o_1.uv2_texcoord2_1 = _S39;

#line 657
    o_1.uv3_texcoord3_1 = _S39;

#line 657
    o_1.vertexColor_1 = _S39;

#line 657
    o_1.screenPos_1 = _S39;

#line 657
    o_1.screenPosition88_1 = _S39;

#line 657
    o_1.worldRefl_1 = _S40;

#line 657
    o_1.ruLocalPosition_1 = _S40;

#line 657
    vec4 _S41 = UnityObjectToClipPos_0(_S38.vertex_0);

#line 657
    o_1.position_0 = _S41;

#line 657
    vec3 world_0 = (((_S38.vertex_0) * (transpose(RU_Object_0.model_0)))).xyz;

#line 657
    vec3 n_0 = UnityObjectToWorldNormal_0(_S38.normal_0);

#line 657
    vec3 t_1 = UnityObjectToWorldDir_0(_S38.tangent_0.xyz);

#line 657
    vec3 b_0 = cross(n_0, t_1) * _S38.tangent_0.w * float((int(sign((determinant(mat3(transpose(RU_Object_0.model_0)[0].xyz, transpose(RU_Object_0.model_0)[1].xyz, transpose(RU_Object_0.model_0)[2].xyz)))))));

#line 657
    vec3 view_0 = normalize((RU_Material_0._WorldSpaceCameraPos.xyz) - world_0);

#line 657
    o_1.uv4_texcoord4_1 = _S38.texcoord3_0;

#line 657
    o_1.worldPos_1 = world_0;

#line 657
    o_1.uv_texcoord_1 = _S38.texcoord_0;

#line 657
    o_1.viewDir_1 = vec3(dot(view_0, t_1), dot(view_0, b_0), dot(view_0, n_0));

#line 657
    o_1.internalSurfaceTtoW0_1 = vec3(t_1.x, b_0.x, n_0.x);

#line 657
    o_1.internalSurfaceTtoW1_1 = vec3(t_1.y, b_0.y, n_0.y);

#line 657
    o_1.internalSurfaceTtoW2_1 = vec3(t_1.z, b_0.z, n_0.z);

#line 657
    o_1.worldNormal_1 = n_0;

#line 657
    o_1.uv2_texcoord2_1 = _S38.texcoord1_0;

#line 657
    o_1.uv3_texcoord3_1 = _S38.texcoord2_0;

#line 657
    o_1.vertexColor_1 = _S38.color_0;

#line 657
    o_1.screenPos_1 = ComputeScreenPos_0(_S41);

#line 657
    o_1.screenPosition88_1 = custom_0.screenPosition88_0;

#line 657
    o_1.worldRefl_1 = - view_0;

#line 657
    o_1.ruLocalPosition_1 = _S38.vertex_0.xyz;

#line 657
    RU_AseVaryings_0 _S42 = o_1;

#line 657
    gl_Position = o_1.position_0;

#line 657
    entryPointParam_RU_AseVert_uv4_texcoord4_0 = _S42.uv4_texcoord4_1;

#line 657
    entryPointParam_RU_AseVert_worldPos_0 = _S42.worldPos_1;

#line 657
    entryPointParam_RU_AseVert_uv_texcoord_0 = _S42.uv_texcoord_1;

#line 657
    entryPointParam_RU_AseVert_viewDir_0 = _S42.viewDir_1;

#line 657
    entryPointParam_RU_AseVert_internalSurfaceTtoW0_0 = _S42.internalSurfaceTtoW0_1;

#line 657
    entryPointParam_RU_AseVert_internalSurfaceTtoW1_0 = _S42.internalSurfaceTtoW1_1;

#line 657
    entryPointParam_RU_AseVert_internalSurfaceTtoW2_0 = _S42.internalSurfaceTtoW2_1;

#line 657
    entryPointParam_RU_AseVert_worldNormal_0 = _S42.worldNormal_1;

#line 657
    entryPointParam_RU_AseVert_uv2_texcoord2_0 = _S42.uv2_texcoord2_1;

#line 657
    entryPointParam_RU_AseVert_uv3_texcoord3_0 = _S42.uv3_texcoord3_1;

#line 657
    entryPointParam_RU_AseVert_vertexColor_0 = _S42.vertexColor_1;

#line 657
    entryPointParam_RU_AseVert_screenPos_0 = _S42.screenPos_1;

#line 657
    entryPointParam_RU_AseVert_screenPosition88_0 = _S42.screenPosition88_1;

#line 657
    entryPointParam_RU_AseVert_worldRefl_0 = _S42.worldRefl_1;

#line 657
    entryPointParam_RU_AseVert_ruLocalPosition_0 = _S42.ruLocalPosition_1;

#line 657
    return;
}


// @rimunity-host-matrices 1
