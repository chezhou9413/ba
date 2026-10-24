#version 450
//碧蓝档案 UI：不读取场景颜色与地表深度。
layout(binding = 25, set = 2)
uniform sampler2D _VATNormalTex; // @default = black // @vertex
layout(binding = 27, set = 2)
uniform sampler2D _VATPositionTex; // @default = black // @vertex
layout(binding = 29, set = 2)
uniform sampler2D _VTOMaskTex; // @default = white // @vertex
layout(binding = 31, set = 2)
uniform sampler2D _VTOTex; // @default = white // @vertex

float RU_CompatSmoothstep(float low, float high, float x) {
    if (low == high) return step(low, x);
    float t = clamp((x - low) / (high - low), 0.0, 1.0);
    return t * t * (3.0 - 2.0 * t);
}

vec2 RU_CompatSmoothstep(vec2 low, vec2 high, vec2 x) {
    return vec2(RU_CompatSmoothstep(low.x, high.x, x.x), RU_CompatSmoothstep(low.y, high.y, x.y));
}
vec2 RU_CompatSmoothstep(float low, float high, vec2 x) { return RU_CompatSmoothstep(vec2(low), vec2(high), x); }

vec3 RU_CompatSmoothstep(vec3 low, vec3 high, vec3 x) {
    return vec3(RU_CompatSmoothstep(low.x, high.x, x.x), RU_CompatSmoothstep(low.y, high.y, x.y), RU_CompatSmoothstep(low.z, high.z, x.z));
}
vec3 RU_CompatSmoothstep(float low, float high, vec3 x) { return RU_CompatSmoothstep(vec3(low), vec3(high), x); }

vec4 RU_CompatSmoothstep(vec4 low, vec4 high, vec4 x) {
    return vec4(RU_CompatSmoothstep(low.x, high.x, x.x), RU_CompatSmoothstep(low.y, high.y, x.y), RU_CompatSmoothstep(low.z, high.z, x.z), RU_CompatSmoothstep(low.w, high.w, x.w));
}
vec4 RU_CompatSmoothstep(float low, float high, vec4 x) { return RU_CompatSmoothstep(vec4(low), vec4(high), x); }


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

#line 572
layout(binding = 23, set = 2)
uniform sampler2D _ParaTex; // @default = white


#line 574



#line 495
layout(binding = 9, set = 2)
uniform sampler2D _DistortMaskTex; // @default = white


#line 497



#line 506
layout(binding = 11, set = 2)
uniform sampler2D _DistortTex; // @default = white


#line 508



#line 550
layout(binding = 19, set = 2)
uniform sampler2D _MaskTex; // @default = white


#line 552



#line 539
layout(binding = 17, set = 2)
uniform sampler2D _MaskPlusTex; // @default = white


#line 541



#line 528
layout(binding = 15, set = 2)
uniform sampler2D _MainTex; // @default = white


#line 530



#line 517



#line 519



#line 455
layout(binding = 1, set = 2)
uniform sampler2D _AddTex; // @default = white


#line 457



#line 473
layout(binding = 5, set = 2)
uniform sampler2D _DissloveTex; // @default = white


#line 475



#line 484
layout(binding = 7, set = 2)
uniform sampler2D _DissloveTexPlus; // @default = black


#line 486



#line 561
layout(binding = 21, set = 2)
uniform sampler2D _NormalTex; // @default = white


#line 563



#line 466
layout(binding = 3, set = 2)
uniform samplerCube _CubeMap; // @default = gray


#line 468



#line 14946 1
float saturate_0(float x_0)
{

#line 14954
    return clamp(x_0, 0.0, 1.0);
}


#line 582 0
vec4 RU_tex2Dgrad_ParaTex_0(vec2 uv_0, vec2 dx_0, vec2 dy_0)
{

#line 582
    return (textureGrad(_ParaTex, (vec2(uv_0.x, 1.0 - uv_0.y)), (vec2(dx_0.x, - dx_0.y)), (vec2(dy_0.x, - dy_0.y))));
}


#line 656
vec2 RU_POM_ParaTex_0(vec2 uvs_0, vec2 dx_1, vec2 dy_1, vec3 normalWorld_0, vec3 viewWorld_0, vec3 viewDirTan_0, int minSamples_0, int maxSamples_0, float parallax_0, float refPlane_0, vec2 tilling_0, vec2 curv_0, int index_0)
{
    //关闭视差时直接使用原始坐标，不执行后续纹理步进。
    if (RU_Material_0._IfPara == 0.0) return uvs_0;

#line 656
    vec2 _S1 = uvs_0;

#line 656
    int numSteps_0 = int(mix(float(maxSamples_0), float(minSamples_0), saturate_0(dot(normalWorld_0, viewWorld_0))));

#line 656
    float layerHeight_0 = 1.0 / float(numSteps_0);

#line 656
    vec2 plane_0 = parallax_0 * (viewDirTan_0.xy / viewDirTan_0.z);

#line 656
    _S1.xy = uvs_0.xy + refPlane_0 * plane_0;

#line 656
    vec2 deltaTex_0 = - plane_0 * layerHeight_0;

#line 656
    vec2 _S2 = vec2(ivec2(0));

#line 656
    float _S3 = 1.0 - layerHeight_0;

#line 656
    float prevHeight_0 = 0.0;

#line 656
    float prevRayZ_0 = 1.0;

#line 656
    float currHeight_0 = 0.0;

#line 656
    vec2 prevTexOffset_0 = _S2;

#line 656
    int stepIndex_0 = 0;

#line 656
    vec2 currTexOffset_0 = deltaTex_0;

#line 656
    float currRayZ_0 = _S3;

#line 656
    for(;;)
    {

#line 656
        int _S4 = numSteps_0 + 1;

#line 656
        if(stepIndex_0 < _S4)
        {
        }
        else
        {

#line 656
            break;
        }

#line 656
        float currHeight_1 = RU_tex2Dgrad_ParaTex_0(_S1 + currTexOffset_0, dx_1, dy_1).x;

#line 656
        if(currHeight_1 > currRayZ_0)
        {

#line 656
            stepIndex_0 = _S4;

#line 656
        }
        else
        {

#line 656
            int _S5 = stepIndex_0 + 1;

#line 656
            vec2 currTexOffset_1 = currTexOffset_0 + deltaTex_0;

#line 656
            float currRayZ_1 = currRayZ_0 - layerHeight_0;

#line 656
            prevHeight_0 = currHeight_1;

#line 656
            prevRayZ_0 = currRayZ_0;

#line 656
            prevTexOffset_0 = currTexOffset_0;

#line 656
            stepIndex_0 = _S5;

#line 656
            currTexOffset_0 = currTexOffset_1;

#line 656
            currRayZ_0 = currRayZ_1;

#line 656
        }

#line 656
        currHeight_0 = currHeight_1;

#line 656
    }

#line 656
    vec2 deltaTex_1 = deltaTex_0;

#line 656
    float layerHeight_1 = layerHeight_0;

#line 656
    vec2 finalTexOffset_0 = _S2;

#line 656
    int sectionIndex_0 = 0;

#line 656
    for(;;)
    {

#line 656
        if(sectionIndex_0 < 8)
        {
        }
        else
        {

#line 656
            break;
        }

#line 656
        float intersection_0 = (prevHeight_0 - prevRayZ_0) / (prevHeight_0 - currHeight_0 + currRayZ_0 - prevRayZ_0);

#line 656
        vec2 _S6 = intersection_0 * deltaTex_1;

#line 656
        vec2 finalTexOffset_1 = prevTexOffset_0 + _S6;

#line 656
        float _S7 = intersection_0 * layerHeight_1;

#line 656
        float newZ_0 = prevRayZ_0 - _S7;

#line 656
        float newHeight_0 = RU_tex2Dgrad_ParaTex_0(_S1 + finalTexOffset_1, dx_1, dy_1).x;

#line 656
        if(newHeight_0 > newZ_0)
        {

#line 656
            currHeight_0 = newHeight_0;

#line 656
            deltaTex_1 = _S6;

#line 656
            layerHeight_1 = _S7;

#line 656
            currRayZ_0 = newZ_0;

#line 656
        }
        else
        {

#line 656
            float _S8 = 1.0 - intersection_0;

#line 656
            vec2 _S9 = _S8 * deltaTex_1;

#line 656
            float _S10 = _S8 * layerHeight_1;

#line 656
            prevHeight_0 = newHeight_0;

#line 656
            prevRayZ_0 = newZ_0;

#line 656
            prevTexOffset_0 = finalTexOffset_1;

#line 656
            deltaTex_1 = _S9;

#line 656
            layerHeight_1 = _S10;

#line 656
        }

#line 656
        int _S11 = sectionIndex_0 + 1;

#line 656
        finalTexOffset_0 = finalTexOffset_1;

#line 656
        sectionIndex_0 = _S11;

#line 656
    }

#line 656
    return _S1.xy + finalTexOffset_0;
}


#line 657
float customchanelswitch45_g1072_0(float Custome_0, float xyzw_0, out float chanel_0, float C1x_0, float C1y_0, float C1z_0, float C1w_0, float C2x_0, float C2y_0, float C2z_0, float C2w_0, float none_0)
{

#line 657
    bool _S12 = Custome_0 == 0.0;

#line 657
    bool _S13;

#line 657
    if(_S12)
    {

#line 657
        _S13 = xyzw_0 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S13 = false;

#line 657
    }

#line 657
    if(_S13)
    {

#line 657
        chanel_0 = C1x_0;

#line 657
    }
    else
    {

#line 657
        if(_S12)
        {

#line 657
            _S13 = xyzw_0 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S13 = false;

#line 657
        }

#line 657
        if(_S13)
        {

#line 657
            chanel_0 = C1y_0;

#line 657
        }
        else
        {

#line 657
            if(_S12)
            {

#line 657
                _S13 = xyzw_0 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S13 = false;

#line 657
            }

#line 657
            if(_S13)
            {

#line 657
                chanel_0 = C1z_0;

#line 657
            }
            else
            {

#line 657
                if(_S12)
                {

#line 657
                    _S13 = xyzw_0 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S13 = false;

#line 657
                }

#line 657
                if(_S13)
                {

#line 657
                    chanel_0 = C1w_0;

#line 657
                }
                else
                {

#line 657
                    if(_S12)
                    {

#line 657
                        _S13 = xyzw_0 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S13 = false;

#line 657
                    }

#line 657
                    if(_S13)
                    {

#line 657
                        chanel_0 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S14 = Custome_0 == 1.0;

#line 657
                        if(_S14)
                        {

#line 657
                            _S13 = xyzw_0 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S13 = false;

#line 657
                        }

#line 657
                        if(_S13)
                        {

#line 657
                            chanel_0 = C2x_0;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S14)
                            {

#line 657
                                _S13 = xyzw_0 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S13 = false;

#line 657
                            }

#line 657
                            if(_S13)
                            {

#line 657
                                chanel_0 = C2y_0;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S14)
                                {

#line 657
                                    _S13 = xyzw_0 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S13 = false;

#line 657
                                }

#line 657
                                if(_S13)
                                {

#line 657
                                    chanel_0 = C2z_0;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S14)
                                    {

#line 657
                                        _S13 = xyzw_0 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S13 = false;

#line 657
                                    }

#line 657
                                    if(_S13)
                                    {

#line 657
                                        chanel_0 = C2w_0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S14)
                                        {

#line 657
                                            _S13 = xyzw_0 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S13 = false;

#line 657
                                        }

#line 657
                                        if(_S13)
                                        {

#line 657
                                            chanel_0 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 499
vec4 RU_tex2D_DistortMaskTex_0(vec2 uv_1)
{

#line 499
    return (texture(_DistortMaskTex, (vec2(uv_1.x, 1.0 - uv_1.y))));
}


#line 510
vec4 RU_tex2D_DistortTex_0(vec2 uv_2)
{

#line 510
    return (texture(_DistortTex, (vec2(uv_2.x, 1.0 - uv_2.y))));
}


#line 657
float customchanelswitch45_g1081_0(float Custome_1, float xyzw_1, out float chanel_1, float C1x_1, float C1y_1, float C1z_1, float C1w_1, float C2x_1, float C2y_1, float C2z_1, float C2w_1, float none_1)
{

#line 657
    bool _S15 = Custome_1 == 0.0;

#line 657
    bool _S16;

#line 657
    if(_S15)
    {

#line 657
        _S16 = xyzw_1 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S16 = false;

#line 657
    }

#line 657
    if(_S16)
    {

#line 657
        chanel_1 = C1x_1;

#line 657
    }
    else
    {

#line 657
        if(_S15)
        {

#line 657
            _S16 = xyzw_1 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S16 = false;

#line 657
        }

#line 657
        if(_S16)
        {

#line 657
            chanel_1 = C1y_1;

#line 657
        }
        else
        {

#line 657
            if(_S15)
            {

#line 657
                _S16 = xyzw_1 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S16 = false;

#line 657
            }

#line 657
            if(_S16)
            {

#line 657
                chanel_1 = C1z_1;

#line 657
            }
            else
            {

#line 657
                if(_S15)
                {

#line 657
                    _S16 = xyzw_1 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S16 = false;

#line 657
                }

#line 657
                if(_S16)
                {

#line 657
                    chanel_1 = C1w_1;

#line 657
                }
                else
                {

#line 657
                    if(_S15)
                    {

#line 657
                        _S16 = xyzw_1 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S16 = false;

#line 657
                    }

#line 657
                    if(_S16)
                    {

#line 657
                        chanel_1 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S17 = Custome_1 == 1.0;

#line 657
                        if(_S17)
                        {

#line 657
                            _S16 = xyzw_1 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S16 = false;

#line 657
                        }

#line 657
                        if(_S16)
                        {

#line 657
                            chanel_1 = C2x_1;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S17)
                            {

#line 657
                                _S16 = xyzw_1 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S16 = false;

#line 657
                            }

#line 657
                            if(_S16)
                            {

#line 657
                                chanel_1 = C2y_1;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S17)
                                {

#line 657
                                    _S16 = xyzw_1 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S16 = false;

#line 657
                                }

#line 657
                                if(_S16)
                                {

#line 657
                                    chanel_1 = C2z_1;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S17)
                                    {

#line 657
                                        _S16 = xyzw_1 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S16 = false;

#line 657
                                    }

#line 657
                                    if(_S16)
                                    {

#line 657
                                        chanel_1 = C2w_1;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S17)
                                        {

#line 657
                                            _S16 = xyzw_1 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S16 = false;

#line 657
                                        }

#line 657
                                        if(_S16)
                                        {

#line 657
                                            chanel_1 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 657
float customchanelswitch45_g1075_0(float Custome_2, float xyzw_2, out float chanel_2, float C1x_2, float C1y_2, float C1z_2, float C1w_2, float C2x_2, float C2y_2, float C2z_2, float C2w_2, float none_2)
{

#line 657
    bool _S18 = Custome_2 == 0.0;

#line 657
    bool _S19;

#line 657
    if(_S18)
    {

#line 657
        _S19 = xyzw_2 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S19 = false;

#line 657
    }

#line 657
    if(_S19)
    {

#line 657
        chanel_2 = C1x_2;

#line 657
    }
    else
    {

#line 657
        if(_S18)
        {

#line 657
            _S19 = xyzw_2 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S19 = false;

#line 657
        }

#line 657
        if(_S19)
        {

#line 657
            chanel_2 = C1y_2;

#line 657
        }
        else
        {

#line 657
            if(_S18)
            {

#line 657
                _S19 = xyzw_2 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S19 = false;

#line 657
            }

#line 657
            if(_S19)
            {

#line 657
                chanel_2 = C1z_2;

#line 657
            }
            else
            {

#line 657
                if(_S18)
                {

#line 657
                    _S19 = xyzw_2 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S19 = false;

#line 657
                }

#line 657
                if(_S19)
                {

#line 657
                    chanel_2 = C1w_2;

#line 657
                }
                else
                {

#line 657
                    if(_S18)
                    {

#line 657
                        _S19 = xyzw_2 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S19 = false;

#line 657
                    }

#line 657
                    if(_S19)
                    {

#line 657
                        chanel_2 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S20 = Custome_2 == 1.0;

#line 657
                        if(_S20)
                        {

#line 657
                            _S19 = xyzw_2 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S19 = false;

#line 657
                        }

#line 657
                        if(_S19)
                        {

#line 657
                            chanel_2 = C2x_2;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S20)
                            {

#line 657
                                _S19 = xyzw_2 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S19 = false;

#line 657
                            }

#line 657
                            if(_S19)
                            {

#line 657
                                chanel_2 = C2y_2;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S20)
                                {

#line 657
                                    _S19 = xyzw_2 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S19 = false;

#line 657
                                }

#line 657
                                if(_S19)
                                {

#line 657
                                    chanel_2 = C2z_2;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S20)
                                    {

#line 657
                                        _S19 = xyzw_2 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S19 = false;

#line 657
                                    }

#line 657
                                    if(_S19)
                                    {

#line 657
                                        chanel_2 = C2w_2;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S20)
                                        {

#line 657
                                            _S19 = xyzw_2 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S19 = false;

#line 657
                                        }

#line 657
                                        if(_S19)
                                        {

#line 657
                                            chanel_2 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 554
vec4 RU_tex2D_MaskTex_0(vec2 uv_3)
{

#line 554
    return (texture(_MaskTex, (vec2(uv_3.x, 1.0 - uv_3.y))));
}


#line 543
vec4 RU_tex2D_MaskPlusTex_0(vec2 uv_4)
{

#line 543
    return (texture(_MaskPlusTex, (vec2(uv_4.x, 1.0 - uv_4.y))));
}


#line 657
float customchanelswitch45_g1080_0(float Custome_3, float xyzw_3, out float chanel_3, float C1x_3, float C1y_3, float C1z_3, float C1w_3, float C2x_3, float C2y_3, float C2z_3, float C2w_3, float none_3)
{

#line 657
    bool _S21 = Custome_3 == 0.0;

#line 657
    bool _S22;

#line 657
    if(_S21)
    {

#line 657
        _S22 = xyzw_3 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S22 = false;

#line 657
    }

#line 657
    if(_S22)
    {

#line 657
        chanel_3 = C1x_3;

#line 657
    }
    else
    {

#line 657
        if(_S21)
        {

#line 657
            _S22 = xyzw_3 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S22 = false;

#line 657
        }

#line 657
        if(_S22)
        {

#line 657
            chanel_3 = C1y_3;

#line 657
        }
        else
        {

#line 657
            if(_S21)
            {

#line 657
                _S22 = xyzw_3 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S22 = false;

#line 657
            }

#line 657
            if(_S22)
            {

#line 657
                chanel_3 = C1z_3;

#line 657
            }
            else
            {

#line 657
                if(_S21)
                {

#line 657
                    _S22 = xyzw_3 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S22 = false;

#line 657
                }

#line 657
                if(_S22)
                {

#line 657
                    chanel_3 = C1w_3;

#line 657
                }
                else
                {

#line 657
                    if(_S21)
                    {

#line 657
                        _S22 = xyzw_3 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S22 = false;

#line 657
                    }

#line 657
                    if(_S22)
                    {

#line 657
                        chanel_3 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S23 = Custome_3 == 1.0;

#line 657
                        if(_S23)
                        {

#line 657
                            _S22 = xyzw_3 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S22 = false;

#line 657
                        }

#line 657
                        if(_S22)
                        {

#line 657
                            chanel_3 = C2x_3;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S23)
                            {

#line 657
                                _S22 = xyzw_3 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S22 = false;

#line 657
                            }

#line 657
                            if(_S22)
                            {

#line 657
                                chanel_3 = C2y_3;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S23)
                                {

#line 657
                                    _S22 = xyzw_3 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S22 = false;

#line 657
                                }

#line 657
                                if(_S22)
                                {

#line 657
                                    chanel_3 = C2z_3;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S23)
                                    {

#line 657
                                        _S22 = xyzw_3 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S22 = false;

#line 657
                                    }

#line 657
                                    if(_S22)
                                    {

#line 657
                                        chanel_3 = C2w_3;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S23)
                                        {

#line 657
                                            _S22 = xyzw_3 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S22 = false;

#line 657
                                        }

#line 657
                                        if(_S22)
                                        {

#line 657
                                            chanel_3 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 657
float customchanelswitch45_g1073_0(float Custome_4, float xyzw_4, out float chanel_4, float C1x_4, float C1y_4, float C1z_4, float C1w_4, float C2x_4, float C2y_4, float C2z_4, float C2w_4, float none_4)
{

#line 657
    bool _S24 = Custome_4 == 0.0;

#line 657
    bool _S25;

#line 657
    if(_S24)
    {

#line 657
        _S25 = xyzw_4 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S25 = false;

#line 657
    }

#line 657
    if(_S25)
    {

#line 657
        chanel_4 = C1x_4;

#line 657
    }
    else
    {

#line 657
        if(_S24)
        {

#line 657
            _S25 = xyzw_4 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S25 = false;

#line 657
        }

#line 657
        if(_S25)
        {

#line 657
            chanel_4 = C1y_4;

#line 657
        }
        else
        {

#line 657
            if(_S24)
            {

#line 657
                _S25 = xyzw_4 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S25 = false;

#line 657
            }

#line 657
            if(_S25)
            {

#line 657
                chanel_4 = C1z_4;

#line 657
            }
            else
            {

#line 657
                if(_S24)
                {

#line 657
                    _S25 = xyzw_4 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S25 = false;

#line 657
                }

#line 657
                if(_S25)
                {

#line 657
                    chanel_4 = C1w_4;

#line 657
                }
                else
                {

#line 657
                    if(_S24)
                    {

#line 657
                        _S25 = xyzw_4 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S25 = false;

#line 657
                    }

#line 657
                    if(_S25)
                    {

#line 657
                        chanel_4 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S26 = Custome_4 == 1.0;

#line 657
                        if(_S26)
                        {

#line 657
                            _S25 = xyzw_4 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S25 = false;

#line 657
                        }

#line 657
                        if(_S25)
                        {

#line 657
                            chanel_4 = C2x_4;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S26)
                            {

#line 657
                                _S25 = xyzw_4 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S25 = false;

#line 657
                            }

#line 657
                            if(_S25)
                            {

#line 657
                                chanel_4 = C2y_4;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S26)
                                {

#line 657
                                    _S25 = xyzw_4 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S25 = false;

#line 657
                                }

#line 657
                                if(_S25)
                                {

#line 657
                                    chanel_4 = C2z_4;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S26)
                                    {

#line 657
                                        _S25 = xyzw_4 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S25 = false;

#line 657
                                    }

#line 657
                                    if(_S25)
                                    {

#line 657
                                        chanel_4 = C2w_4;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S26)
                                        {

#line 657
                                            _S25 = xyzw_4 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S25 = false;

#line 657
                                        }

#line 657
                                        if(_S25)
                                        {

#line 657
                                            chanel_4 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 532
vec4 RU_tex2D_MainTex_0(vec2 uv_5)
{

#line 532
    return (texture(_MainTex, (vec2(uv_5.x, 1.0 - uv_5.y))));
}


#line 657
vec4 ASE_ComputeGrabScreenPos_0(vec4 pos_0)
{

#line 657
    vec4 o_0 = pos_0;

#line 657
    o_0[1] = pos_0.w * 0.5;

#line 657
    o_0[1] = (pos_0.y - o_0.y) * RU_Material_0._ProjectionParams.x + o_0.y;

#line 657
    return o_0;
}


#line 521



#line 459
vec4 RU_tex2D_AddTex_0(vec2 uv_7)
{

#line 459
    return (texture(_AddTex, (vec2(uv_7.x, 1.0 - uv_7.y))));
}


#line 657
float customchanelswitch45_g1076_0(float Custome_5, float xyzw_5, out float chanel_5, float C1x_5, float C1y_5, float C1z_5, float C1w_5, float C2x_5, float C2y_5, float C2z_5, float C2w_5, float none_5)
{

#line 657
    bool _S27 = Custome_5 == 0.0;

#line 657
    bool _S28;

#line 657
    if(_S27)
    {

#line 657
        _S28 = xyzw_5 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S28 = false;

#line 657
    }

#line 657
    if(_S28)
    {

#line 657
        chanel_5 = C1x_5;

#line 657
    }
    else
    {

#line 657
        if(_S27)
        {

#line 657
            _S28 = xyzw_5 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S28 = false;

#line 657
        }

#line 657
        if(_S28)
        {

#line 657
            chanel_5 = C1y_5;

#line 657
        }
        else
        {

#line 657
            if(_S27)
            {

#line 657
                _S28 = xyzw_5 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S28 = false;

#line 657
            }

#line 657
            if(_S28)
            {

#line 657
                chanel_5 = C1z_5;

#line 657
            }
            else
            {

#line 657
                if(_S27)
                {

#line 657
                    _S28 = xyzw_5 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S28 = false;

#line 657
                }

#line 657
                if(_S28)
                {

#line 657
                    chanel_5 = C1w_5;

#line 657
                }
                else
                {

#line 657
                    if(_S27)
                    {

#line 657
                        _S28 = xyzw_5 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S28 = false;

#line 657
                    }

#line 657
                    if(_S28)
                    {

#line 657
                        chanel_5 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S29 = Custome_5 == 1.0;

#line 657
                        if(_S29)
                        {

#line 657
                            _S28 = xyzw_5 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S28 = false;

#line 657
                        }

#line 657
                        if(_S28)
                        {

#line 657
                            chanel_5 = C2x_5;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S29)
                            {

#line 657
                                _S28 = xyzw_5 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S28 = false;

#line 657
                            }

#line 657
                            if(_S28)
                            {

#line 657
                                chanel_5 = C2y_5;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S29)
                                {

#line 657
                                    _S28 = xyzw_5 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S28 = false;

#line 657
                                }

#line 657
                                if(_S28)
                                {

#line 657
                                    chanel_5 = C2z_5;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S29)
                                    {

#line 657
                                        _S28 = xyzw_5 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S28 = false;

#line 657
                                    }

#line 657
                                    if(_S28)
                                    {

#line 657
                                        chanel_5 = C2w_5;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S29)
                                        {

#line 657
                                            _S28 = xyzw_5 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S28 = false;

#line 657
                                        }

#line 657
                                        if(_S28)
                                        {

#line 657
                                            chanel_5 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 657
float customchanelswitch45_g1079_0(float Custome_6, float xyzw_6, out float chanel_6, float C1x_6, float C1y_6, float C1z_6, float C1w_6, float C2x_6, float C2y_6, float C2z_6, float C2w_6, float none_6)
{

#line 657
    bool _S30 = Custome_6 == 0.0;

#line 657
    bool _S31;

#line 657
    if(_S30)
    {

#line 657
        _S31 = xyzw_6 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S31 = false;

#line 657
    }

#line 657
    if(_S31)
    {

#line 657
        chanel_6 = C1x_6;

#line 657
    }
    else
    {

#line 657
        if(_S30)
        {

#line 657
            _S31 = xyzw_6 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S31 = false;

#line 657
        }

#line 657
        if(_S31)
        {

#line 657
            chanel_6 = C1y_6;

#line 657
        }
        else
        {

#line 657
            if(_S30)
            {

#line 657
                _S31 = xyzw_6 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S31 = false;

#line 657
            }

#line 657
            if(_S31)
            {

#line 657
                chanel_6 = C1z_6;

#line 657
            }
            else
            {

#line 657
                if(_S30)
                {

#line 657
                    _S31 = xyzw_6 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S31 = false;

#line 657
                }

#line 657
                if(_S31)
                {

#line 657
                    chanel_6 = C1w_6;

#line 657
                }
                else
                {

#line 657
                    if(_S30)
                    {

#line 657
                        _S31 = xyzw_6 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S31 = false;

#line 657
                    }

#line 657
                    if(_S31)
                    {

#line 657
                        chanel_6 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S32 = Custome_6 == 1.0;

#line 657
                        if(_S32)
                        {

#line 657
                            _S31 = xyzw_6 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S31 = false;

#line 657
                        }

#line 657
                        if(_S31)
                        {

#line 657
                            chanel_6 = C2x_6;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S32)
                            {

#line 657
                                _S31 = xyzw_6 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S31 = false;

#line 657
                            }

#line 657
                            if(_S31)
                            {

#line 657
                                chanel_6 = C2y_6;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S32)
                                {

#line 657
                                    _S31 = xyzw_6 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S31 = false;

#line 657
                                }

#line 657
                                if(_S31)
                                {

#line 657
                                    chanel_6 = C2z_6;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S32)
                                    {

#line 657
                                        _S31 = xyzw_6 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S31 = false;

#line 657
                                    }

#line 657
                                    if(_S31)
                                    {

#line 657
                                        chanel_6 = C2w_6;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S32)
                                        {

#line 657
                                            _S31 = xyzw_6 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S31 = false;

#line 657
                                        }

#line 657
                                        if(_S31)
                                        {

#line 657
                                            chanel_6 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 657
float customchanelswitch45_g1074_0(float Custome_7, float xyzw_7, out float chanel_7, float C1x_7, float C1y_7, float C1z_7, float C1w_7, float C2x_7, float C2y_7, float C2z_7, float C2w_7, float none_7)
{

#line 657
    bool _S33 = Custome_7 == 0.0;

#line 657
    bool _S34;

#line 657
    if(_S33)
    {

#line 657
        _S34 = xyzw_7 == 0.0;

#line 657
    }
    else
    {

#line 657
        _S34 = false;

#line 657
    }

#line 657
    if(_S34)
    {

#line 657
        chanel_7 = C1x_7;

#line 657
    }
    else
    {

#line 657
        if(_S33)
        {

#line 657
            _S34 = xyzw_7 == 1.0;

#line 657
        }
        else
        {

#line 657
            _S34 = false;

#line 657
        }

#line 657
        if(_S34)
        {

#line 657
            chanel_7 = C1y_7;

#line 657
        }
        else
        {

#line 657
            if(_S33)
            {

#line 657
                _S34 = xyzw_7 == 2.0;

#line 657
            }
            else
            {

#line 657
                _S34 = false;

#line 657
            }

#line 657
            if(_S34)
            {

#line 657
                chanel_7 = C1z_7;

#line 657
            }
            else
            {

#line 657
                if(_S33)
                {

#line 657
                    _S34 = xyzw_7 == 3.0;

#line 657
                }
                else
                {

#line 657
                    _S34 = false;

#line 657
                }

#line 657
                if(_S34)
                {

#line 657
                    chanel_7 = C1w_7;

#line 657
                }
                else
                {

#line 657
                    if(_S33)
                    {

#line 657
                        _S34 = xyzw_7 == 4.0;

#line 657
                    }
                    else
                    {

#line 657
                        _S34 = false;

#line 657
                    }

#line 657
                    if(_S34)
                    {

#line 657
                        chanel_7 = 0.0;

#line 657
                    }
                    else
                    {

#line 657
                        bool _S35 = Custome_7 == 1.0;

#line 657
                        if(_S35)
                        {

#line 657
                            _S34 = xyzw_7 == 0.0;

#line 657
                        }
                        else
                        {

#line 657
                            _S34 = false;

#line 657
                        }

#line 657
                        if(_S34)
                        {

#line 657
                            chanel_7 = C2x_7;

#line 657
                        }
                        else
                        {

#line 657
                            if(_S35)
                            {

#line 657
                                _S34 = xyzw_7 == 1.0;

#line 657
                            }
                            else
                            {

#line 657
                                _S34 = false;

#line 657
                            }

#line 657
                            if(_S34)
                            {

#line 657
                                chanel_7 = C2y_7;

#line 657
                            }
                            else
                            {

#line 657
                                if(_S35)
                                {

#line 657
                                    _S34 = xyzw_7 == 2.0;

#line 657
                                }
                                else
                                {

#line 657
                                    _S34 = false;

#line 657
                                }

#line 657
                                if(_S34)
                                {

#line 657
                                    chanel_7 = C2z_7;

#line 657
                                }
                                else
                                {

#line 657
                                    if(_S35)
                                    {

#line 657
                                        _S34 = xyzw_7 == 3.0;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        _S34 = false;

#line 657
                                    }

#line 657
                                    if(_S34)
                                    {

#line 657
                                        chanel_7 = C2w_7;

#line 657
                                    }
                                    else
                                    {

#line 657
                                        if(_S35)
                                        {

#line 657
                                            _S34 = xyzw_7 == 4.0;

#line 657
                                        }
                                        else
                                        {

#line 657
                                            _S34 = false;

#line 657
                                        }

#line 657
                                        if(_S34)
                                        {

#line 657
                                            chanel_7 = 0.0;

#line 657
                                        }

#line 657
                                    }

#line 657
                                }

#line 657
                            }

#line 657
                        }

#line 657
                    }

#line 657
                }

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    return 0.0;
}


#line 477
vec4 RU_tex2D_DissloveTex_0(vec2 uv_8)
{

#line 477
    return (texture(_DissloveTex, (vec2(uv_8.x, 1.0 - uv_8.y))));
}


#line 488
vec4 RU_tex2D_DissloveTexPlus_0(vec2 uv_9)
{

#line 488
    return (texture(_DissloveTexPlus, (vec2(uv_9.x, 1.0 - uv_9.y))));
}


#line 632



#line 646





float LinearEyeDepth_0(float z_0)
{

#line 652
    float _S38;

#line 652
    if((RU_Material_0.unity_OrthoParams.w) != 0.0)
    {

#line 652
        _S38 = RU_Material_0._ProjectionParams.z - z_0 * (RU_Material_0._ProjectionParams.z - RU_Material_0._ProjectionParams.y);

#line 652
    }
    else
    {

#line 652
        _S38 = 1.0 / (RU_Material_0._ZBufferParams.z * z_0 + RU_Material_0._ZBufferParams.w);

#line 652
    }

#line 652
    return _S38;
}


#line 565
vec4 RU_tex2D_NormalTex_0(vec2 uv_12)
{

#line 565
    return (texture(_NormalTex, (vec2(uv_12.x, 1.0 - uv_12.y))));
}


#line 51
vec3 UnpackNormal_0(vec4 n_0)
{

#line 51
    vec4 _S39 = n_0;

#line 51
    _S39[0] = _S39[0] * n_0.w;

#line 51
    vec2 xy_1 = _S39.xy * 2.0 - 1.0;

#line 51
    return vec3(xy_1, sqrt(1.0 - saturate_0(dot(xy_1, xy_1))));
}


#line 652
vec3 UnpackScaleNormal_0(vec4 packed_0, float scale_0)
{

#line 652
    vec3 _S40 = UnpackNormal_0(packed_0);

#line 652
    vec3 n_1 = _S40;

#line 652
    n_1.xy = _S40.xy * scale_0;

#line 652
    n_1[2] = sqrt(1.0 - saturate_0(dot(n_1.xy, n_1.xy)));

#line 652
    return n_1;
}


#line 652
vec3 BlendNormals_0(vec3 first_0, vec3 second_0)
{

#line 652
    return normalize(vec3(first_0.xy + second_0.xy, first_0.z * second_0.z));
}


#line 470
vec4 RU_texCUBE_CubeMap_0(vec3 direction_0)
{

#line 470
    return (texture(_CubeMap, (direction_0)));
}


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


#line 656
struct UnityGIInput_0
{
    vec3 worldPos_1;
    float atten_0;
    vec4 lightmapUV_0;
};


#line 656
struct SurfaceOutputCustomLightingCustom_0
{
    vec3 Albedo_0;
    vec3 Normal_0;
    vec3 Emission_0;
    float Metallic_0;
    float Smoothness_0;
    float Occlusion_0;
    float Alpha_0;
    Input_0 SurfInput_0;
    UnityGIInput_0 GIData_0;
};


#line 657
void surf_0(Input_0 i_0, inout SurfaceOutputCustomLightingCustom_0 o_1)
{

#line 657
    o_1.SurfInput_0 = i_0;

#line 657
    const vec3 _S41 = vec3(0.0, 0.0, 1.0);

#line 657
    o_1.Normal_0 = _S41;

#line 657
    vec2 appendResult49_g1051_0 = vec2(RU_Material_0._Mask_Uspeed, RU_Material_0._Mask_Vspeed);

#line 657
    float Ifpara1474_0 = RU_Material_0._IfPara;

#line 657
    vec2 appendResult30_g1032_0 = vec2(i_0.uv4_texcoord4_0.x, i_0.uv4_texcoord4_0.y);

#line 394
    vec4 break36_g1032_0 = RU_Material_0._MaskTex_ST.zwxy;

#line 657
    float _S42 = break36_g1032_0.x;

#line 657
    float _S43 = break36_g1032_0.y;

#line 657
    vec2 appendResult32_g1032_0 = vec2(_S42, _S43);

#line 657
    vec2 appendResult38_g1032_0 = vec2(break36_g1032_0.z, break36_g1032_0.w);

#line 657
    vec4 break537_0 = RU_Material_0._TexCenter + vec4(i_0.ruLocalPosition_0, 0.0);

#line 657
    vec2 appendResult554_0 = vec2((atan(break537_0.x / break537_0.z) - -1.57079637050628662) / 3.14159274101257324, break537_0.y);

#line 657
    vec2 _S44 = i_0.uv_texcoord_0.xy;

#line 657
    vec2 CenteredUV15_g1033_0 = _S44 - vec2(RU_Material_0._CenterU, RU_Material_0._CenterV);

#line 657
    float _S45 = length(CenteredUV15_g1033_0);

#line 657
    float _S46 = (atan((CenteredUV15_g1033_0.x),(CenteredUV15_g1033_0.y))) * 0.15915493667125702;

#line 657
    vec2 appendResult23_g1033_0 = vec2(_S45 * _S42 * 2.0, _S46 * _S43);

#line 657
    vec2 _S47 = vec2(ivec2(0));

#line 657
    vec2 ifLocalVar2192_0;

#line 657
    if((RU_Material_0._MaskTexUVS) > 1.0)
    {

#line 657
        ifLocalVar2192_0 = appendResult554_0 * appendResult32_g1032_0 + appendResult38_g1032_0;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._MaskTexUVS) == 1.0)
        {

#line 657
            ifLocalVar2192_0 = appendResult23_g1033_0 + appendResult38_g1032_0;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._MaskTexUVS) < 1.0)
            {

#line 657
                ifLocalVar2192_0 = _S44 * appendResult32_g1032_0 + appendResult38_g1032_0;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2192_0 = _S47;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 temp_output_2194_0_0;

#line 657
    if((RU_Material_0._MaskTexUVS) == 3.0)
    {

#line 657
        temp_output_2194_0_0 = appendResult30_g1032_0 * appendResult32_g1032_0 + appendResult38_g1032_0;

#line 657
    }
    else
    {

#line 657
        temp_output_2194_0_0 = ifLocalVar2192_0;

#line 657
    }

#line 657
    float Parallax1492_0 = RU_Material_0._Parallax * 0.10000000149011612;

#line 657
    vec3 ase_worldNormal_0 = vec3(dot(i_0.internalSurfaceTtoW0_0, _S41), dot(i_0.internalSurfaceTtoW1_0, _S41), dot(i_0.internalSurfaceTtoW2_0, _S41));

#line 657
    vec3 ase_worldViewDir_0 = normalize((RU_Material_0._WorldSpaceCameraPos.xyz) - i_0.worldPos_0.xyz);

#line 657
    const vec2 _S48 = vec2(0.0, 0.0);

#line 657
    vec2 OffsetPOM80_g1040_0 = RU_POM_ParaTex_0(temp_output_2194_0_0, dFdx(temp_output_2194_0_0), dFdy(temp_output_2194_0_0), ase_worldNormal_0, ase_worldViewDir_0, i_0.viewDir_0, 64, 64, Parallax1492_0, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S48, 0);

#line 657
    const vec3 _S49 = vec3(0.0);

#line 657
    float chanel45_g1072_0 = 0.0;

#line 657
    float C1x45_g1072_0 = i_0.uv_texcoord_0.z;

#line 657
    float C1y45_g1072_0 = i_0.uv_texcoord_0.w;

#line 657
    float C1z45_g1072_0 = i_0.uv2_texcoord2_0.x;

#line 657
    float C1w45_g1072_0 = i_0.uv2_texcoord2_0.y;

#line 657
    float C2x45_g1072_0 = i_0.uv2_texcoord2_0.z;

#line 657
    float C2y45_g1072_0 = i_0.uv2_texcoord2_0.w;

#line 657
    vec2 _S50 = i_0.uv3_texcoord3_0.xy;

#line 657
    float C2z45_g1072_0 = _S50.x;

#line 657
    float C2w45_g1072_0 = _S50.y;

#line 657
    float _S51 = customchanelswitch45_g1072_0(RU_Material_0._DistortFactorC1, RU_Material_0._DistortFactorC2, chanel45_g1072_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    float DistortFactorC1851_0 = chanel45_g1072_0;

#line 382
    vec4 break36_g1019_0 = RU_Material_0._DistortMaskTex_ST.zwxy;

#line 657
    float cos55_g1021_0 = cos(RU_Material_0._DistortMaskTexR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1021_0 = sin(RU_Material_0._DistortMaskTexR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    const vec2 _S52 = vec2(0.5, 0.5);

#line 657
    vec2 rotator55_g1021_0 = (((mat2(cos55_g1021_0, - sin55_g1021_0, sin55_g1021_0, cos55_g1021_0)) * (RU_Material_0._Time.y * _S48 + (_S44 * vec2(break36_g1019_0.x, break36_g1019_0.y) + vec2(break36_g1019_0.z, break36_g1019_0.w)) - _S52))) + _S52;

#line 657
    float _S53 = rotator55_g1021_0.x;

#line 657
    float clampResult60_g1021_0 = clamp(_S53, 0.0, 1.0);

#line 657
    float _S54 = rotator55_g1021_0.y;

#line 657
    float clampResult50_g1021_0 = clamp(_S54, 0.0, 1.0);

#line 657
    float temp_output_1550_0_0;

#line 657
    if(float(int(RU_Material_0._DistortMaskTexC)) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = _S53;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = clampResult60_g1021_0;

#line 657
    }

#line 657
    float temp_output_1684_0_0;

#line 657
    if(float(int(RU_Material_0._DistortMaskTexCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = _S54;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = clampResult50_g1021_0;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1021_0 = RU_tex2D_DistortMaskTex_0(vec2(temp_output_1550_0_0, temp_output_1684_0_0));

#line 657
    if((RU_Material_0._CustomDistort) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = RU_Material_0._DistortFactor;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = DistortFactorC1851_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._DistortMaskTexAR)) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = tex2DNode27_g1021_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = tex2DNode27_g1021_0.x;

#line 657
    }

#line 657
    float temp_output_1587_0_0 = temp_output_1550_0_0 * temp_output_1684_0_0;

#line 375
    vec4 break36_g1016_0 = RU_Material_0._DistortTex_ST.zwxy;

#line 657
    vec2 panner59_0 = RU_Material_0._Time.y * vec2(RU_Material_0._DistortTex_Uspeed, RU_Material_0._DistortTex_Vspeed) + (_S44 * vec2(break36_g1016_0.x, break36_g1016_0.y) + vec2(break36_g1016_0.z, break36_g1016_0.w));

#line 657
    vec4 tex2DNode54_0 = RU_tex2D_DistortTex_0(panner59_0);

#line 657
    if((RU_Material_0._DistortTexAR) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = tex2DNode54_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = tex2DNode54_0.x;

#line 657
    }

#line 657
    if((RU_Material_0._DistortRemap) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = temp_output_1550_0_0;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = -0.5 + temp_output_1550_0_0;

#line 657
    }

#line 657
    vec3 _S55 = vec3(temp_output_1587_0_0 * temp_output_1684_0_0);

#line 657
    float temp_output_1570_0_0 = 1.0 * 0.10000000149011612;

#line 657
    float _S56 = panner59_0.x;

#line 657
    float _S57 = panner59_0.y;

#line 657
    vec4 tex2DNode1577_0 = RU_tex2D_DistortTex_0(vec2(_S56 + temp_output_1570_0_0, _S57));

#line 657
    float temp_output_1579_0_0;

#line 657
    if((RU_Material_0._DistortTexAR) == 0.0)
    {

#line 657
        temp_output_1579_0_0 = tex2DNode1577_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1579_0_0 = tex2DNode1577_0.y;

#line 657
    }

#line 657
    if((RU_Material_0._DistortRemap) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = temp_output_1579_0_0;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = -0.5 + temp_output_1579_0_0;

#line 657
    }

#line 657
    vec3 appendResult1564_0 = vec3(1.0, 0.0, (temp_output_1550_0_0 - temp_output_1684_0_0) * temp_output_1587_0_0);

#line 657
    vec4 tex2DNode1576_0 = RU_tex2D_DistortTex_0(vec2(_S56, _S57 + temp_output_1570_0_0));

#line 657
    float temp_output_1580_0_0;

#line 657
    if((RU_Material_0._DistortTexAR) == 0.0)
    {

#line 657
        temp_output_1580_0_0 = tex2DNode1576_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1580_0_0 = tex2DNode1576_0.y;

#line 657
    }

#line 657
    if((RU_Material_0._DistortRemap) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = temp_output_1580_0_0;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = -0.5 + temp_output_1580_0_0;

#line 657
    }

#line 657
    vec3 normalizeResult1563_0 = normalize(cross(appendResult1564_0, vec3(0.0, 1.0, (temp_output_1550_0_0 - temp_output_1684_0_0) * temp_output_1587_0_0)));

#line 657
    vec2 appendResult61_0 = vec2(tex2DNode54_0.x, tex2DNode54_0.y);

#line 657
    vec3 DistortUV60_0;

#line 657
    if((RU_Material_0._IfFlowmap) == 0.0)
    {

#line 657
        if((RU_Material_0._IfNormalDistort) == 0.0)
        {

#line 657
            DistortUV60_0 = _S55;

#line 657
        }
        else
        {

#line 657
            DistortUV60_0 = normalizeResult1563_0;

#line 657
        }

#line 657
    }
    else
    {

#line 657
        DistortUV60_0 = vec3(temp_output_1587_0_0 * (appendResult61_0 - _S44), 0.0);

#line 657
    }

#line 657
    vec3 temp_output_1066_0_0;

#line 657
    if((RU_Material_0._DistortMask) == 0.0)
    {

#line 657
        temp_output_1066_0_0 = _S49;

#line 657
    }
    else
    {

#line 657
        temp_output_1066_0_0 = DistortUV60_0;

#line 657
    }

#line 657
    const vec2 _S58 = vec2(0.0);

#line 657
    float chanel45_g1081_0 = 0.0;

#line 657
    float _S59 = customchanelswitch45_g1081_0(RU_Material_0._MaskOffsetUC1, RU_Material_0._MaskOffsetUC2, chanel45_g1081_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    float chanel45_g1075_0 = 0.0;

#line 657
    float _S60 = customchanelswitch45_g1075_0(RU_Material_0._MaskOffsetVC1, RU_Material_0._MaskOffsetVC2, chanel45_g1075_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    vec2 appendResult1836_0 = vec2(chanel45_g1081_0, chanel45_g1075_0);

#line 657
    vec2 _S61 = RU_Material_0._Time.y * appendResult49_g1051_0;

#line 657
    bool _S62 = Ifpara1474_0 == 0.0;

#line 657
    if(_S62)
    {

#line 657
        ifLocalVar2192_0 = temp_output_2194_0_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_0 = OffsetPOM80_g1040_0;

#line 657
    }

#line 657
    vec3 _S63 = vec3(ifLocalVar2192_0, 0.0) + temp_output_1066_0_0;

#line 657
    if(bool(RU_Material_0._CustomdataMaskUV))
    {

#line 657
        ifLocalVar2192_0 = appendResult1836_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_0 = _S58;

#line 657
    }

#line 657
    float cos55_g1051_0 = cos(RU_Material_0._Mask_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1051_0 = sin(RU_Material_0._Mask_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1051_0 = (((mat2(cos55_g1051_0, - sin55_g1051_0, sin55_g1051_0, cos55_g1051_0)) * (_S61 + (_S63 + vec3(ifLocalVar2192_0, 0.0)).xy - _S52))) + _S52;

#line 657
    float _S64 = rotator55_g1051_0.x;

#line 657
    float clampResult60_g1051_0 = clamp(_S64, 0.0, 1.0);

#line 657
    float _S65 = rotator55_g1051_0.y;

#line 657
    float clampResult50_g1051_0 = clamp(_S65, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._MaskC)) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = _S64;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = clampResult60_g1051_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._MaskCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = _S65;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = clampResult50_g1051_0;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1051_0 = RU_tex2D_MaskTex_0(vec2(temp_output_1550_0_0, temp_output_1684_0_0));

#line 657
    vec2 appendResult49_g1060_0 = vec2(RU_Material_0._MaskPlusUspeed, RU_Material_0._MaskPlusVspeed);

#line 362
    vec4 break36_g1056_0 = RU_Material_0._MaskPlusTex_ST.zwxy;

#line 657
    vec2 appendResult38_g1056_0 = vec2(break36_g1056_0.z, break36_g1056_0.w);

#line 657
    float _S66 = break36_g1056_0.x;

#line 657
    float _S67 = break36_g1056_0.y;

#line 657
    vec2 appendResult31_g1056_0 = vec2(_S66, _S67);

#line 657
    vec2 appendResult23_g1057_0 = vec2(_S45 * _S66 * 2.0, _S46 * _S67);

#line 657
    vec2 ifLocalVar2195_0;

#line 657
    if((RU_Material_0._MaskTexUVS) > 1.0)
    {

#line 657
        ifLocalVar2195_0 = appendResult554_0 * appendResult31_g1056_0 + appendResult38_g1056_0;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._MaskTexUVS) == 1.0)
        {

#line 657
            ifLocalVar2195_0 = appendResult23_g1057_0 + appendResult38_g1056_0;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._MaskTexUVS) < 1.0)
            {

#line 657
                ifLocalVar2195_0 = _S44 * appendResult31_g1056_0 + appendResult38_g1056_0;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2195_0 = _S47;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    float cos55_g1060_0 = cos(RU_Material_0._MaskPlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1060_0 = sin(RU_Material_0._MaskPlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1060_0 = (((mat2(cos55_g1060_0, - sin55_g1060_0, sin55_g1060_0, cos55_g1060_0)) * (RU_Material_0._Time.y * appendResult49_g1060_0 + (temp_output_1066_0_0 + vec3(ifLocalVar2195_0, 0.0)).xy - _S52))) + _S52;

#line 657
    float _S68 = rotator55_g1060_0.x;

#line 657
    float clampResult60_g1060_0 = clamp(_S68, 0.0, 1.0);

#line 657
    float _S69 = rotator55_g1060_0.y;

#line 657
    float clampResult50_g1060_0 = clamp(_S69, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._MaskPlusC)) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = _S68;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = clampResult60_g1060_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._MaskPlusCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = _S69;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = clampResult50_g1060_0;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1060_0 = RU_tex2D_MaskPlusTex_0(vec2(temp_output_1550_0_0, temp_output_1684_0_0));

#line 657
    float _S70 = RU_Material_0._Mask_scale;

#line 657
    if(float(int(RU_Material_0._MaskAlphaRA)) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = tex2DNode27_g1051_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = tex2DNode27_g1051_0.x;

#line 657
    }

#line 657
    if((RU_Material_0._IfMaskPlusTex) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = 1.0;

#line 657
    }
    else
    {

#line 657
        if(float(int(RU_Material_0._MaskPlusAR)) == 0.0)
        {

#line 657
            temp_output_1684_0_0 = tex2DNode27_g1060_0.w;

#line 657
        }
        else
        {

#line 657
            temp_output_1684_0_0 = tex2DNode27_g1060_0.x;

#line 657
        }

#line 657
    }

#line 657
    float MaskAlpha136_0 = _S70 * (temp_output_1550_0_0 * temp_output_1684_0_0);

#line 657
    vec2 appendResult49_g1047_0 = vec2(RU_Material_0._MainTex_Uspeed, RU_Material_0._MainTex_Vspeed);

#line 657
    float chanel45_g1080_0 = 0.0;

#line 657
    float _S71 = customchanelswitch45_g1080_0(RU_Material_0._MainOffsetUC1, RU_Material_0._MainOffsetUC2, chanel45_g1080_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    float chanel45_g1073_0 = 0.0;

#line 657
    float _S72 = customchanelswitch45_g1073_0(RU_Material_0._MainOffsetVC1, RU_Material_0._MainOffsetVC2, chanel45_g1073_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    vec2 appendResult1822_0 = vec2(chanel45_g1080_0, chanel45_g1073_0);

#line 657
    vec2 temp_output_1691_0_0;

#line 657
    if((RU_Material_0._CustomdataMainTexUV) == 0.0)
    {

#line 657
        temp_output_1691_0_0 = _S58;

#line 657
    }
    else
    {

#line 657
        temp_output_1691_0_0 = appendResult1822_0;

#line 657
    }

#line 347
    vec4 break36_g1028_0 = RU_Material_0._MainTex_ST.zwxy;

#line 657
    vec2 staticSwitch607_0 = _S44 * vec2(break36_g1028_0.x, break36_g1028_0.y) + vec2(break36_g1028_0.z, break36_g1028_0.w);

#line 657
    vec2 OffsetPOM80_g1039_0 = RU_POM_ParaTex_0(staticSwitch607_0, dFdx(staticSwitch607_0), dFdy(staticSwitch607_0), ase_worldNormal_0, ase_worldViewDir_0, i_0.viewDir_0, 64, 64, Parallax1492_0, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S48, 0);

#line 657
    vec3 temp_output_1057_0_0;

#line 657
    if((RU_Material_0._DistortMainTex) == 0.0)
    {

#line 657
        temp_output_1057_0_0 = _S49;

#line 657
    }
    else
    {

#line 657
        temp_output_1057_0_0 = DistortUV60_0;

#line 657
    }

#line 657
    vec2 _S73 = RU_Material_0._Time.y * appendResult49_g1047_0;

#line 657
    vec3 _S74 = vec3(temp_output_1691_0_0, 0.0);

#line 657
    if(_S62)
    {

#line 657
        ifLocalVar2192_0 = staticSwitch607_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_0 = OffsetPOM80_g1039_0;

#line 657
    }

#line 657
    float cos55_g1047_0 = cos(RU_Material_0._MainTex_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1047_0 = sin(RU_Material_0._MainTex_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1047_0 = (((mat2(cos55_g1047_0, - sin55_g1047_0, sin55_g1047_0, cos55_g1047_0)) * (_S73 + (_S74 + (vec3(ifLocalVar2192_0, 0.0) + temp_output_1057_0_0)).xy - _S52))) + _S52;

#line 657
    float _S75 = rotator55_g1047_0.x;

#line 657
    float clampResult60_g1047_0 = clamp(_S75, 0.0, 1.0);

#line 657
    float _S76 = rotator55_g1047_0.y;

#line 657
    float clampResult50_g1047_0 = clamp(_S76, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._MaintexC)) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = _S75;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = clampResult60_g1047_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._MaintexCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = _S76;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = clampResult50_g1047_0;

#line 657
    }

#line 657
    vec2 appendResult53_g1047_0 = vec2(temp_output_1550_0_0, temp_output_1684_0_0);

#line 657
    vec4 tex2DNode64_g1047_0 = RU_tex2D_MainTex_0(appendResult53_g1047_0 + RU_Material_0._MainTexAC);

#line 657
    vec4 tex2DNode27_g1047_0 = RU_tex2D_MainTex_0(appendResult53_g1047_0);

#line 657
    vec4 tex2DNode65_g1047_0 = RU_tex2D_MainTex_0(appendResult53_g1047_0 - vec2(RU_Material_0._MainTexAC));

#line 657
    float _S77 = tex2DNode64_g1047_0.x;

#line 657
    float _S78 = (tex2DNode27_g1047_0.w + tex2DNode64_g1047_0.w + tex2DNode65_g1047_0.w) / 3.0;

#line 657
    vec4 appendResult67_g1047_0 = vec4(_S77, tex2DNode27_g1047_0.y, tex2DNode65_g1047_0.z, _S78);

#line 657
    float temp_output_2038_39_0;

#line 657
    if(float(int(RU_Material_0._MainTex_ar)) == 0.0)
    {

#line 657
        temp_output_2038_39_0 = _S78;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_0 = _S77;

#line 657
    }

#line 657
    vec4 ase_grabScreenPos_0 = ASE_ComputeGrabScreenPos_0(vec4(i_0.screenPos_0.xyz, i_0.screenPos_0.w + 9.999999960041972e-12));

#line 657


#line 657
    if((RU_Material_0._CustomdataDisT) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = i_0.vertexColor_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = 1.0;

#line 657
    }

#line 657


#line 657
        temp_output_1684_0_0 = temp_output_2038_39_0;

#line 657


#line 657
    float temp_output_7_0_0 = temp_output_1550_0_0 * temp_output_1684_0_0 * RU_Material_0._MainColor.w;

#line 657
    vec2 appendResult49_g1046_0 = vec2(RU_Material_0._AddTexUspeed, RU_Material_0._AddTexVspeed);

#line 335
    vec4 break36_g1030_0 = RU_Material_0._AddTex_ST.zwxy;

#line 657
    float _S79 = break36_g1030_0.x;

#line 657
    float _S80 = break36_g1030_0.y;

#line 657
    vec2 appendResult32_g1030_0 = vec2(_S79, _S80);

#line 657
    vec2 appendResult38_g1030_0 = vec2(break36_g1030_0.z, break36_g1030_0.w);

#line 657
    vec2 appendResult23_g1031_0 = vec2(_S45 * _S79 * 2.0, _S46 * _S80);

#line 657
    vec2 ifLocalVar1687_0;

#line 657
    if((RU_Material_0._AddTexUVS) > 1.0)
    {

#line 657
        ifLocalVar1687_0 = appendResult554_0 * appendResult32_g1030_0 + appendResult38_g1030_0;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._AddTexUVS) == 1.0)
        {

#line 657
            ifLocalVar1687_0 = appendResult23_g1031_0 + appendResult38_g1030_0;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._AddTexUVS) < 1.0)
            {

#line 657
                ifLocalVar1687_0 = _S44 * appendResult32_g1030_0 + appendResult38_g1030_0;

#line 657
            }
            else
            {

#line 657
                ifLocalVar1687_0 = _S47;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 temp_output_2191_0_0;

#line 657
    if((RU_Material_0._AddTexUVS) == 3.0)
    {

#line 657
        temp_output_2191_0_0 = appendResult30_g1032_0 * appendResult32_g1030_0 + appendResult38_g1030_0;

#line 657
    }
    else
    {

#line 657
        temp_output_2191_0_0 = ifLocalVar1687_0;

#line 657
    }

#line 657
    vec2 OffsetPOM80_g1042_0 = RU_POM_ParaTex_0(temp_output_2191_0_0, dFdx(temp_output_2191_0_0), dFdy(temp_output_2191_0_0), ase_worldNormal_0, ase_worldViewDir_0, i_0.viewDir_0, 64, 64, Parallax1492_0, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S48, 0);

#line 657
    const vec2 _S81 = vec2(1.0);

#line 657
    vec2 appendResult1703_0 = vec2(RU_Material_0._AddTex_ST.zwxy.x / RU_Material_0._MainTex_ST.zwxy.x, RU_Material_0._AddTex_ST.zwxy.y / RU_Material_0._MainTex_ST.zwxy.y);

#line 657
    vec2 _S82 = RU_Material_0._Time.y * appendResult49_g1046_0;

#line 657
    if(_S62)
    {

#line 657
        ifLocalVar2192_0 = temp_output_2191_0_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_0 = OffsetPOM80_g1042_0;

#line 657
    }

#line 657
    vec3 _S83 = vec3(ifLocalVar2192_0, 0.0);

#line 657
    if((RU_Material_0._DistortAddTex) == 0.0)
    {

#line 657
        temp_output_1066_0_0 = _S49;

#line 657
    }
    else
    {

#line 657
        temp_output_1066_0_0 = DistortUV60_0;

#line 657
    }

#line 657
    vec3 _S84 = _S83 + temp_output_1066_0_0;

#line 657
    if((RU_Material_0._CAddTexUV) == 0.0)
    {

#line 657
        ifLocalVar2192_0 = _S58;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CAddTexUVT) == 0.0)
        {

#line 657
            ifLocalVar2192_0 = _S81;

#line 657
        }
        else
        {

#line 657
            ifLocalVar2192_0 = appendResult1703_0;

#line 657
        }

#line 657
        ifLocalVar2192_0 = ifLocalVar2192_0 * temp_output_1691_0_0;

#line 657
    }

#line 657
    float cos55_g1046_0 = cos(RU_Material_0._AddTexRo / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1046_0 = sin(RU_Material_0._AddTexRo / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1046_0 = (((mat2(cos55_g1046_0, - sin55_g1046_0, sin55_g1046_0, cos55_g1046_0)) * (_S82 + (_S84 + vec3(ifLocalVar2192_0, 0.0)).xy - _S52))) + _S52;

#line 657
    float _S85 = rotator55_g1046_0.x;

#line 657
    float clampResult60_g1046_0 = clamp(_S85, 0.0, 1.0);

#line 657
    float _S86 = rotator55_g1046_0.y;

#line 657
    float clampResult50_g1046_0 = clamp(_S86, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._AddTexC)) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = _S85;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = clampResult60_g1046_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._AddTexCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = _S86;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = clampResult50_g1046_0;

#line 657
    }

#line 657
    vec2 appendResult53_g1046_0 = vec2(temp_output_1550_0_0, temp_output_1684_0_0);

#line 657
    vec4 tex2DNode64_g1046_0 = RU_tex2D_AddTex_0(appendResult53_g1046_0 + RU_Material_0._AddTexAC);

#line 657
    vec4 tex2DNode27_g1046_0 = RU_tex2D_AddTex_0(appendResult53_g1046_0);

#line 657
    vec4 tex2DNode65_g1046_0 = RU_tex2D_AddTex_0(appendResult53_g1046_0 - vec2(RU_Material_0._AddTexAC));

#line 657
    float _S87 = tex2DNode64_g1046_0.x;

#line 657
    float _S88 = (tex2DNode27_g1046_0.w + tex2DNode64_g1046_0.w + tex2DNode65_g1046_0.w) / 3.0;

#line 657
    vec4 appendResult67_g1046_0 = vec4(_S87, tex2DNode27_g1046_0.y, tex2DNode65_g1046_0.z, _S88);

#line 657
    float temp_output_2046_39_0;

#line 657
    if(float(int(RU_Material_0._AddTexAR)) == 0.0)
    {

#line 657
        temp_output_2046_39_0 = _S88;

#line 657
    }
    else
    {

#line 657
        temp_output_2046_39_0 = _S87;

#line 657
    }

#line 657
    float lerpResult1659_0 = mix(temp_output_7_0_0, temp_output_2046_39_0, RU_Material_0._AddTexBlend);

#line 657
    float ifLocalVar1676_0;

#line 657
    if((RU_Material_0._AddTexBlendMode) > 1.0)
    {

#line 657
        ifLocalVar1676_0 = temp_output_2046_39_0 * temp_output_7_0_0;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._AddTexBlendMode) == 1.0)
        {

#line 657
            ifLocalVar1676_0 = temp_output_2046_39_0 + temp_output_7_0_0;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._AddTexBlendMode) < 1.0)
            {

#line 657
                ifLocalVar1676_0 = lerpResult1659_0;

#line 657
            }
            else
            {

#line 657
                ifLocalVar1676_0 = 0.0;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    if((RU_Material_0._IfAddTexAlpha) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = temp_output_7_0_0;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = ifLocalVar1676_0;

#line 657
    }

#line 657
    float MainTexAlpha37_0 = temp_output_1550_0_0 * RU_Material_0._MainAlpha;

#line 657
    float temp_output_65_0_g1048_0 = RU_Material_0._DIssloveSoft;

#line 657
    float chanel45_g1076_0 = 0.0;

#line 657
    float _S89 = customchanelswitch45_g1076_0(RU_Material_0._DissolveFactorC1, RU_Material_0._DissolveFactorC2, chanel45_g1076_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    float DissolveFactorC1856_0 = chanel45_g1076_0;

#line 657
    float temp_output_70_0_g1048_0;

#line 657
    if((RU_Material_0._CustomdataDis) == 0.0)
    {

#line 657
        temp_output_70_0_g1048_0 = RU_Material_0._DIssloveFactor + 0.00100000004749745;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CustomdataDisT) == 0.0)
        {

#line 657
            temp_output_1550_0_0 = DissolveFactorC1856_0;

#line 657
        }
        else
        {

#line 657
            temp_output_1550_0_0 = 1.0 + i_0.vertexColor_0.w * -1.0;

#line 657
        }

#line 657
        temp_output_70_0_g1048_0 = temp_output_1550_0_0;

#line 657
    }

#line 657
    float temp_output_77_0_g1048_0 = (temp_output_65_0_g1048_0 + 1.0) * temp_output_70_0_g1048_0;

#line 657
    int temp_output_38_0_g1045_0 = int(RU_Material_0._DissolveAR);

#line 657
    vec2 appendResult49_g1045_0 = vec2(RU_Material_0._DisTex_Uspeed, RU_Material_0._DisTex_Vspeed);

#line 315
    vec4 break36_g1070_0 = RU_Material_0._DissloveTex_ST.zwxy;

#line 657
    float _S90 = break36_g1070_0.x;

#line 657
    float _S91 = break36_g1070_0.y;

#line 657
    vec2 appendResult32_g1070_0 = vec2(_S90, _S91);

#line 657
    vec2 appendResult38_g1070_0 = vec2(break36_g1070_0.z, break36_g1070_0.w);

#line 657
    vec2 appendResult23_g1071_0 = vec2(_S45 * _S90 * 2.0, _S46 * _S91);

#line 657
    vec2 ifLocalVar2207_0;

#line 657
    if((RU_Material_0._DissolveTexUVS2) > 1.0)
    {

#line 657
        ifLocalVar2207_0 = appendResult554_0 * appendResult32_g1070_0 + appendResult38_g1070_0;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._DissolveTexUVS2) == 1.0)
        {

#line 657
            ifLocalVar2207_0 = appendResult23_g1071_0 + appendResult38_g1070_0;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._DissolveTexUVS2) < 1.0)
            {

#line 657
                ifLocalVar2207_0 = _S44 * appendResult32_g1070_0 + appendResult38_g1070_0;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2207_0 = _S47;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 temp_output_2209_0_0;

#line 657
    if((RU_Material_0._DissolveTexUVS2) == 3.0)
    {

#line 657
        temp_output_2209_0_0 = appendResult30_g1032_0 * appendResult32_g1070_0 + appendResult38_g1070_0;

#line 657
    }
    else
    {

#line 657
        temp_output_2209_0_0 = ifLocalVar2207_0;

#line 657
    }

#line 657
    vec2 OffsetPOM80_g1038_0 = RU_POM_ParaTex_0(temp_output_2209_0_0, dFdx(temp_output_2209_0_0), dFdy(temp_output_2209_0_0), ase_worldNormal_0, ase_worldViewDir_0, i_0.viewDir_0, 64, 64, Parallax1492_0, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S48, 0);

#line 657
    float chanel45_g1079_0 = 0.0;

#line 657
    float _S92 = customchanelswitch45_g1079_0(RU_Material_0._DissolveOffsetUC1, RU_Material_0._DissolveOffsetUC2, chanel45_g1079_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    float chanel45_g1074_0 = 0.0;

#line 657
    float _S93 = customchanelswitch45_g1074_0(RU_Material_0._DissolveOffsetVC1, RU_Material_0._DissolveOffsetVC2, chanel45_g1074_0, C1x45_g1072_0, C1y45_g1072_0, C1z45_g1072_0, C1w45_g1072_0, C2x45_g1072_0, C2y45_g1072_0, C2z45_g1072_0, C2w45_g1072_0, 0.0);

#line 657
    vec2 appendResult1844_0 = vec2(chanel45_g1079_0, chanel45_g1074_0);

#line 657
    vec2 temp_output_1860_0_0;

#line 657
    if((RU_Material_0._IfDissolveOffsetC) == 0.0)
    {

#line 657
        temp_output_1860_0_0 = _S58;

#line 657
    }
    else
    {

#line 657
        temp_output_1860_0_0 = appendResult1844_0;

#line 657
    }

#line 657
    vec2 _S94 = RU_Material_0._Time.y * appendResult49_g1045_0;

#line 657
    if(_S62)
    {

#line 657
        ifLocalVar2192_0 = temp_output_2209_0_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_0 = OffsetPOM80_g1038_0;

#line 657
    }

#line 657
    vec3 _S95 = vec3(ifLocalVar2192_0, 0.0);

#line 657
    if((RU_Material_0._DistortDisTex) == 0.0)
    {

#line 657
        temp_output_1066_0_0 = _S49;

#line 657
    }
    else
    {

#line 657
        temp_output_1066_0_0 = DistortUV60_0;

#line 657
    }

#line 657
    float cos55_g1045_0 = cos(RU_Material_0._DIssolve_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1045_0 = sin(RU_Material_0._DIssolve_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1045_0 = (((mat2(cos55_g1045_0, - sin55_g1045_0, sin55_g1045_0, cos55_g1045_0)) * (_S94 + (_S95 + temp_output_1066_0_0 + vec3(temp_output_1860_0_0, 0.0)).xy - _S52))) + _S52;

#line 657
    float _S96 = rotator55_g1045_0.x;

#line 657
    float clampResult60_g1045_0 = clamp(_S96, 0.0, 1.0);

#line 657
    float _S97 = rotator55_g1045_0.y;

#line 657
    float clampResult50_g1045_0 = clamp(_S97, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._DissolveC)) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = _S96;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = clampResult60_g1045_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._DissolveCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_0 = _S97;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_0 = clampResult50_g1045_0;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1045_0 = RU_tex2D_DissloveTex_0(vec2(temp_output_1550_0_0, temp_output_1684_0_0));

#line 657
    bool _S98 = float(temp_output_38_0_g1045_0) == 0.0;

#line 657
    if(_S98)
    {

#line 657
        temp_output_1579_0_0 = tex2DNode27_g1045_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1579_0_0 = tex2DNode27_g1045_0.x;

#line 657
    }

#line 657
    float temp_output_85_0_g1048_0 = saturate_0(pow(temp_output_1579_0_0, RU_Material_0._DissolveTexExp));

#line 657
    int temp_output_38_0_g1044_0 = int(RU_Material_0._DissolvePlusAR);

#line 301
    vec4 break36_g1034_0 = RU_Material_0._DissloveTexPlus_ST.zwxy;

#line 657
    vec2 appendResult38_g1034_0 = vec2(break36_g1034_0.z, break36_g1034_0.w);

#line 657
    float _S99 = break36_g1034_0.x;

#line 657
    float _S100 = break36_g1034_0.y;

#line 657
    vec2 appendResult31_g1034_0 = vec2(_S99, _S100);

#line 657
    vec2 appendResult23_g1035_0 = vec2(_S45 * _S99 * 2.0, _S46 * _S100);

#line 657
    vec2 ifLocalVar2211_0;

#line 657
    if((RU_Material_0._DissolveTexUVS2) > 1.0)
    {

#line 657
        ifLocalVar2211_0 = appendResult554_0 * appendResult31_g1034_0 + appendResult38_g1034_0;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._DissolveTexUVS2) == 1.0)
        {

#line 657
            ifLocalVar2211_0 = appendResult23_g1035_0 + appendResult38_g1034_0;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._DissolveTexUVS2) < 1.0)
            {

#line 657
                ifLocalVar2211_0 = _S44 * appendResult31_g1034_0 + appendResult38_g1034_0;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2211_0 = _S47;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 OffsetPOM80_g1043_0 = RU_POM_ParaTex_0(ifLocalVar2211_0, dFdx(ifLocalVar2211_0), dFdy(ifLocalVar2211_0), ase_worldNormal_0, ase_worldViewDir_0, i_0.viewDir_0, 64, 64, Parallax1492_0, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S48, 0);

#line 657
    vec2 _S101 = RU_Material_0._Time.y * _S48;

#line 657
    if(_S62)
    {

#line 657
        ifLocalVar2192_0 = ifLocalVar2211_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_0 = OffsetPOM80_g1043_0;

#line 657
    }

#line 657
    float cos55_g1044_0 = cos(RU_Material_0._DissolvePlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1044_0 = sin(RU_Material_0._DissolvePlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1044_0 = (((mat2(cos55_g1044_0, - sin55_g1044_0, sin55_g1044_0, cos55_g1044_0)) * (_S101 + ifLocalVar2192_0 - _S52))) + _S52;

#line 657
    float _S102 = rotator55_g1044_0.x;

#line 657
    float clampResult60_g1044_0 = clamp(_S102, 0.0, 1.0);

#line 657
    float _S103 = rotator55_g1044_0.y;

#line 657
    float clampResult50_g1044_0 = clamp(_S103, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._DissolvePlusC)) == 0.0)
    {

#line 657
        temp_output_1579_0_0 = _S102;

#line 657
    }
    else
    {

#line 657
        temp_output_1579_0_0 = clampResult60_g1044_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._DissolvePlusCV)) == 0.0)
    {

#line 657
        temp_output_1580_0_0 = _S103;

#line 657
    }
    else
    {

#line 657
        temp_output_1580_0_0 = clampResult50_g1044_0;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1044_0 = RU_tex2D_DissloveTexPlus_0(vec2(temp_output_1579_0_0, temp_output_1580_0_0));

#line 657
    float _S104 = temp_output_85_0_g1048_0 / RU_Material_0._DissolveTexDivide;

#line 657
    if((RU_Material_0._IfDissolvePlus) == 0.0)
    {

#line 657
        ifLocalVar1676_0 = temp_output_85_0_g1048_0;

#line 657
    }
    else
    {

#line 657
        if(float(temp_output_38_0_g1044_0) == 0.0)
        {

#line 657
            ifLocalVar1676_0 = tex2DNode27_g1044_0.w;

#line 657
        }
        else
        {

#line 657
            ifLocalVar1676_0 = tex2DNode27_g1044_0.x;

#line 657
        }

#line 657
    }

#line 657
    float temp_output_74_0_g1048_0 = saturate_0((_S104 + ifLocalVar1676_0) / 2.0);

#line 657
    float smoothstepResult76_g1048_0 = RU_CompatSmoothstep(temp_output_77_0_g1048_0 - temp_output_65_0_g1048_0, temp_output_77_0_g1048_0, temp_output_74_0_g1048_0);

#line 657
    float temp_output_64_0_g1048_0 = temp_output_70_0_g1048_0 * (1.0 + RU_Material_0._DIssloveWide);

#line 657
    float temp_output_2237_91_0 = step(temp_output_64_0_g1048_0 - RU_Material_0._DIssloveWide, temp_output_74_0_g1048_0);

#line 657
    float DisAplha42_0;

#line 657
    if((RU_Material_0._sot_sting_A) == 0.0)
    {

#line 657
        DisAplha42_0 = smoothstepResult76_g1048_0;

#line 657
    }
    else
    {

#line 657
        DisAplha42_0 = temp_output_2237_91_0;

#line 657
    }

#line 657
    float dotResult106_0 = dot(ase_worldViewDir_0, ase_worldNormal_0);

#line 657
    if(bool(RU_Material_0._softback))
    {

#line 657
        ifLocalVar1676_0 = dotResult106_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar1676_0 = abs(dotResult106_0);

#line 657
    }

#line 657
    float softedge111_0 = pow(saturate_0(ifLocalVar1676_0), RU_Material_0._softFacotr);

#line 657
    vec4 ase_screenPosNorm88_0 = i_0.screenPosition88_0 / i_0.screenPosition88_0.w;

#line 657
    ase_screenPosNorm88_0[2] = ase_screenPosNorm88_0.z;

#line 657


#line 657
    float temp_output_94_0_0 = saturate_0(pow(1.0 - saturate_0(abs(dot(ase_worldNormal_0, normalize(ase_worldViewDir_0 + (RU_Material_0._Dir.xyz))))), RU_Material_0._fnl_power) * RU_Material_0._fnl_sacle);

#line 657


#line 657
    float _S105 = MaskAlpha136_0 * MainTexAlpha37_0 * DisAplha42_0;

#line 657
    if(bool(RU_Material_0._FNLfanxiangkaiguan))
    {

#line 657
        ifLocalVar1676_0 = softedge111_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar1676_0 = 1.0;

#line 657
    }

#line 657
    float _S106 = _S105 * ifLocalVar1676_0;

#line 657


#line 657
        ifLocalVar1676_0 = 1.0;

#line 657


#line 657
    float _S107 = _S106 * ifLocalVar1676_0;

#line 657
    if(bool(RU_Material_0._IfFNLAlpha))
    {

#line 657
        ifLocalVar1676_0 = temp_output_94_0_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar1676_0 = 1.0;

#line 657
    }

#line 657
    float _S108 = _S107 * ifLocalVar1676_0;

#line 657


#line 657
        ifLocalVar1676_0 = 0.0;

#line 657


#line 657
    float MainAlpha142_0 = saturate_0(_S108 + ifLocalVar1676_0);
    //透明度同时供后续光照入口读取。
    o_1.Alpha_0 = MainAlpha142_0;

#line 657


#line 657
    vec4 fnlColor97_0 = temp_output_94_0_0 * RU_Material_0._fnl_color * i_0.vertexColor_0;

#line 657
    const vec4 _S109 = vec4(1.0);

#line 657
    vec4 ifLocalVar1675_0;

#line 657
    if((RU_Material_0._AlphaAdd) == 0.0)
    {

#line 657
        ifLocalVar1675_0 = appendResult67_g1047_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar1675_0 = appendResult67_g1047_0 * temp_output_2038_39_0;

#line 657
    }

#line 657
    vec3 temp_output_9_0_g1053_0 = ifLocalVar1675_0.xyz;

#line 657
    vec3 lerpResult4_g1053_0 = mix(temp_output_9_0_g1053_0 * RU_Material_0._MainTexRefine.x, pow(temp_output_9_0_g1053_0, vec3(max(RU_Material_0._MainTexRefine.z, 0.00999999977648258))) * RU_Material_0._MainTexRefine.y, vec3(RU_Material_0._MainTexRefine.w));

#line 657
    vec4 _S110 = RU_Material_0._MainColor;

#line 657


#line 657
        ifLocalVar1675_0 = vec4(lerpResult4_g1053_0, 0.0);

#line 657


#line 657
    vec4 _S111 = _S110 * ifLocalVar1675_0;

#line 657
    if(bool(RU_Material_0._IfMaskColor))
    {

#line 657
        ifLocalVar1675_0 = tex2DNode27_g1051_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar1675_0 = _S109;

#line 657
    }

#line 657
    vec4 temp_output_223_0_0 = _S111 * ifLocalVar1675_0;

#line 657
    vec3 temp_output_9_0_g1052_0 = (appendResult67_g1046_0 * temp_output_2046_39_0).xyz;

#line 657
    vec4 AddTexColors479_0 = RU_Material_0._AddTexColor * vec4(mix(temp_output_9_0_g1052_0 * RU_Material_0._AddTexRefine.x, pow(temp_output_9_0_g1052_0, vec3(max(RU_Material_0._AddTexRefine.z, 0.00999999977648258))) * RU_Material_0._AddTexRefine.y, vec3(RU_Material_0._AddTexRefine.w)), 0.0);

#line 657
    vec4 lerpResult488_0 = mix(temp_output_223_0_0, AddTexColors479_0, vec4(RU_Material_0._AddTexBlend));

#line 657
    vec4 _S112 = vec4(ivec4(0));

#line 657
    if((RU_Material_0._AddTexBlendMode) > 1.0)
    {

#line 657
        ifLocalVar1675_0 = AddTexColors479_0 * temp_output_223_0_0;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._AddTexBlendMode) == 1.0)
        {

#line 657
            ifLocalVar1675_0 = AddTexColors479_0 + temp_output_223_0_0;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._AddTexBlendMode) < 1.0)
            {

#line 657
                ifLocalVar1675_0 = lerpResult488_0;

#line 657
            }
            else
            {

#line 657
                ifLocalVar1675_0 = _S112;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec4 MainColornoparticle224_0;

#line 657
    if(bool(RU_Material_0._IfAddTex))
    {

#line 657
        if((RU_Material_0._IfAddTexColor) == 0.0)
        {

#line 657
            ifLocalVar1675_0 = temp_output_223_0_0;

#line 657
        }

#line 657
        MainColornoparticle224_0 = ifLocalVar1675_0;

#line 657
    }
    else
    {

#line 657
        MainColornoparticle224_0 = temp_output_223_0_0;

#line 657
    }

#line 657
    vec4 lerpResult230_0 = mix(MainColornoparticle224_0, RU_Material_0._DIssloveColor, vec4(RU_Material_0._DIssloveColor.w));

#line 657
    if((RU_Material_0._IfDissolveColor) == 0.0)
    {

#line 657
        ifLocalVar1675_0 = i_0.vertexColor_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar1675_0 = _S109;

#line 657
    }

#line 657
    vec4 lerpResult33_0 = mix(lerpResult230_0, ifLocalVar1675_0 * MainColornoparticle224_0, vec4(smoothstepResult76_g1048_0));

#line 657
    float temp_output_234_0_0 = temp_output_2237_91_0 - step(temp_output_64_0_g1048_0, temp_output_74_0_g1048_0);

#line 657
    vec4 lerpResult244_0 = mix(MainColornoparticle224_0, lerpResult230_0 * temp_output_234_0_0, vec4(temp_output_234_0_0));

#line 657
    if((RU_Material_0._IfDissolveColor) == 0.0)
    {

#line 657
        ifLocalVar1675_0 = _S109;

#line 657
    }
    else
    {

#line 657
        ifLocalVar1675_0 = i_0.vertexColor_0;

#line 657
    }

#line 657
    if(bool(RU_Material_0._soft_sting))
    {

#line 657
        MainColornoparticle224_0 = lerpResult244_0;

#line 657
    }
    else
    {

#line 657
        MainColornoparticle224_0 = lerpResult33_0;

#line 657
    }

#line 657
    vec4 DisColor40_0 = ifLocalVar1675_0 * MainColornoparticle224_0;

#line 271
    vec4 break36_g1054_0 = RU_Material_0._NormalTex_ST.zwxy;

#line 657
    vec2 staticSwitch2179_0 = _S44 * vec2(break36_g1054_0.x, break36_g1054_0.y) + vec2(break36_g1054_0.z, break36_g1054_0.w);

#line 657
    vec2 OffsetPOM80_g1058_0 = RU_POM_ParaTex_0(staticSwitch2179_0, dFdx(staticSwitch2179_0), dFdy(staticSwitch2179_0), ase_worldNormal_0, ase_worldViewDir_0, i_0.viewDir_0, 64, 64, Parallax1492_0, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S48, 0);

#line 657
    vec2 _S113 = RU_Material_0._Time.y * vec2(RU_Material_0._NormalTex_Uspeed, RU_Material_0._NormalTex_Vspeed);

#line 657
    if(_S62)
    {

#line 657
        ifLocalVar2192_0 = staticSwitch2179_0;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_0 = OffsetPOM80_g1058_0;

#line 657
    }

#line 657
    vec3 _S114 = vec3(ifLocalVar2192_0, 0.0);

#line 657
    if((RU_Material_0._DistortNormalTex) == 0.0)
    {

#line 657
        DistortUV60_0 = _S49;

#line 657
    }

#line 657
    float cos62_g1059_0 = cos(RU_Material_0._NormalTex_Rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin62_g1059_0 = sin(RU_Material_0._NormalTex_Rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator62_g1059_0 = (((mat2(cos62_g1059_0, - sin62_g1059_0, sin62_g1059_0, cos62_g1059_0)) * (_S113 + (_S114 + DistortUV60_0).xy - _S52))) + _S52;

#line 657
    float _S115 = rotator62_g1059_0.x;

#line 657
    float clampResult67_g1059_0 = clamp(_S115, 0.0, 1.0);

#line 657
    float _S116 = rotator62_g1059_0.y;

#line 657
    float clampResult57_g1059_0 = clamp(_S116, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._NormalTexC)) == 0.0)
    {

#line 657
        temp_output_2038_39_0 = _S115;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_0 = clampResult67_g1059_0;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._NormalTexCV)) == 0.0)
    {

#line 657
        temp_output_2046_39_0 = _S116;

#line 657
    }
    else
    {

#line 657
        temp_output_2046_39_0 = clampResult57_g1059_0;

#line 657
    }

#line 657
    vec2 appendResult60_g1059_0 = vec2(temp_output_2038_39_0, temp_output_2046_39_0);

#line 657
    float temp_output_65_0_g1050_0 = RU_Material_0._DIssloveSoft;

#line 657
    float temp_output_70_0_g1050_0;

#line 657
    if((RU_Material_0._CustomdataDis) == 0.0)
    {

#line 657
        temp_output_70_0_g1050_0 = RU_Material_0._DIssloveFactor + 0.00100000004749745;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CustomdataDisT) == 0.0)
        {

#line 657
            temp_output_2038_39_0 = DissolveFactorC1856_0;

#line 657
        }
        else
        {

#line 657
            temp_output_2038_39_0 = 1.0 + i_0.vertexColor_0.w * -1.0;

#line 657
        }

#line 657
        temp_output_70_0_g1050_0 = temp_output_2038_39_0;

#line 657
    }

#line 657
    float temp_output_77_0_g1050_0 = (temp_output_65_0_g1050_0 + 1.0) * temp_output_70_0_g1050_0;

#line 657
    float temp_output_64_0_g1045_0 = 0.0 * 0.10000000149011612;

#line 657
    vec4 tex2DNode70_g1045_0 = RU_tex2D_DissloveTex_0(vec2(temp_output_1550_0_0 + temp_output_64_0_g1045_0, temp_output_1684_0_0));

#line 657
    if(_S98)
    {

#line 657
        temp_output_2038_39_0 = tex2DNode70_g1045_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_0 = tex2DNode70_g1045_0.x;

#line 657
    }

#line 657
    float temp_output_85_0_g1050_0 = saturate_0(pow(temp_output_2038_39_0, RU_Material_0._DissolveTexExp));

#line 657
    float temp_output_64_0_g1044_0 = 0.0 * 0.10000000149011612;

#line 657
    vec4 tex2DNode70_g1044_0 = RU_tex2D_DissloveTexPlus_0(vec2(temp_output_1579_0_0 + temp_output_64_0_g1044_0, temp_output_1580_0_0));

#line 657
    float _S117 = temp_output_85_0_g1050_0 / RU_Material_0._DissolveTexDivide;

#line 657
    if((RU_Material_0._IfDissolvePlus) == 0.0)
    {

#line 657
        temp_output_2038_39_0 = temp_output_85_0_g1050_0;

#line 657
    }
    else
    {

#line 657
        if(float(temp_output_38_0_g1044_0) == 0.0)
        {

#line 657
            temp_output_2038_39_0 = tex2DNode70_g1044_0.w;

#line 657
        }
        else
        {

#line 657
            temp_output_2038_39_0 = tex2DNode70_g1044_0.x;

#line 657
        }

#line 657
    }

#line 657
    float temp_output_74_0_g1050_0 = saturate_0((_S117 + temp_output_2038_39_0) / 2.0);

#line 657
    float smoothstepResult76_g1050_0 = RU_CompatSmoothstep(temp_output_77_0_g1050_0 - temp_output_65_0_g1050_0, temp_output_77_0_g1050_0, temp_output_74_0_g1050_0);

#line 657
    float temp_output_66_0_g1050_0 = RU_Material_0._DIssloveWide;

#line 657
    float temp_output_64_0_g1050_0 = temp_output_70_0_g1050_0 * (1.0 + RU_Material_0._DIssloveWide);

#line 657
    float DisAplha11397_0;

#line 657
    if((RU_Material_0._sot_sting_A) == 0.0)
    {

#line 657
        DisAplha11397_0 = smoothstepResult76_g1050_0;

#line 657
    }
    else
    {

#line 657
        DisAplha11397_0 = step(temp_output_64_0_g1050_0 - temp_output_66_0_g1050_0, temp_output_74_0_g1050_0);

#line 657
    }

#line 657
    float temp_output_1402_0_0 = RU_Material_0._StaticNormalScale * 10.0;

#line 657
    vec3 appendResult1273_0 = vec3(1.0, 0.0, (DisAplha11397_0 - DisAplha42_0) * temp_output_1402_0_0);

#line 657
    float temp_output_65_0_g1049_0 = RU_Material_0._DIssloveSoft;

#line 657
    float temp_output_70_0_g1049_0;

#line 657
    if((RU_Material_0._CustomdataDis) == 0.0)
    {

#line 657
        temp_output_70_0_g1049_0 = RU_Material_0._DIssloveFactor + 0.00100000004749745;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CustomdataDisT) == 0.0)
        {

#line 657
            temp_output_2038_39_0 = DissolveFactorC1856_0;

#line 657
        }
        else
        {

#line 657
            temp_output_2038_39_0 = 1.0 + i_0.vertexColor_0.w * -1.0;

#line 657
        }

#line 657
        temp_output_70_0_g1049_0 = temp_output_2038_39_0;

#line 657
    }

#line 657
    float temp_output_77_0_g1049_0 = (temp_output_65_0_g1049_0 + 1.0) * temp_output_70_0_g1049_0;

#line 657
    vec4 tex2DNode76_g1045_0 = RU_tex2D_DissloveTex_0(vec2(temp_output_1550_0_0, temp_output_1684_0_0 + temp_output_64_0_g1045_0));

#line 657
    if(_S98)
    {

#line 657
        temp_output_1550_0_0 = tex2DNode76_g1045_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = tex2DNode76_g1045_0.x;

#line 657
    }

#line 657
    float temp_output_85_0_g1049_0 = saturate_0(pow(temp_output_1550_0_0, RU_Material_0._DissolveTexExp));

#line 657
    vec4 tex2DNode76_g1044_0 = RU_tex2D_DissloveTexPlus_0(vec2(temp_output_1579_0_0, temp_output_1580_0_0 + temp_output_64_0_g1044_0));

#line 657
    float _S118 = temp_output_85_0_g1049_0 / RU_Material_0._DIssloveSoft;

#line 657
    if((RU_Material_0._IfDissolvePlus) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = temp_output_85_0_g1049_0;

#line 657
    }
    else
    {

#line 657
        if(float(temp_output_38_0_g1044_0) == 0.0)
        {

#line 657
            temp_output_1550_0_0 = tex2DNode76_g1044_0.w;

#line 657
        }
        else
        {

#line 657
            temp_output_1550_0_0 = tex2DNode76_g1044_0.x;

#line 657
        }

#line 657
    }

#line 657
    float temp_output_74_0_g1049_0 = saturate_0((_S118 + temp_output_1550_0_0) / 2.0);

#line 657
    float smoothstepResult76_g1049_0 = RU_CompatSmoothstep(temp_output_77_0_g1049_0 - temp_output_65_0_g1049_0, temp_output_77_0_g1049_0, temp_output_74_0_g1049_0);

#line 657
    float temp_output_66_0_g1049_0 = RU_Material_0._DIssloveWide;

#line 657
    float temp_output_64_0_g1049_0 = temp_output_70_0_g1049_0 * (1.0 + RU_Material_0._DIssloveWide);

#line 657
    float DisAplha21398_0;

#line 657
    if((RU_Material_0._sot_sting_A) == 0.0)
    {

#line 657
        DisAplha21398_0 = smoothstepResult76_g1049_0;

#line 657
    }
    else
    {

#line 657
        DisAplha21398_0 = step(temp_output_64_0_g1049_0 - temp_output_66_0_g1049_0, temp_output_74_0_g1049_0);

#line 657
    }

#line 657
    vec3 normalizeResult1265_0 = normalize(cross(appendResult1273_0, vec3(0.0, 1.0, (DisAplha21398_0 - DisAplha42_0) * temp_output_1402_0_0)));

#line 657
    vec3 _S119 = UnpackScaleNormal_0(RU_tex2D_NormalTex_0(appendResult60_g1059_0), RU_Material_0._NormalScale);

#line 657
    if((RU_Material_0._IfStaticNormal) == 0.0)
    {

#line 657
        DistortUV60_0 = _S41;

#line 657
    }
    else
    {

#line 657
        DistortUV60_0 = normalizeResult1265_0;

#line 657
    }

#line 657
    vec3 Normal1046_0 = BlendNormals_0(_S119, DistortUV60_0);

#line 657
    vec4 Cubemap1144_0;

#line 657
    if((RU_Material_0._IfCubemap) == 0.0)
    {

#line 657
        Cubemap1144_0 = vec4(0.0, 0.0, 0.0, 0.0);

#line 657
    }
    else
    {

#line 657
        Cubemap1144_0 = RU_texCUBE_CubeMap_0(reflect(i_0.worldRefl_0, vec3(dot(i_0.internalSurfaceTtoW0_0, Normal1046_0), dot(i_0.internalSurfaceTtoW1_0, Normal1046_0), dot(i_0.internalSurfaceTtoW2_0, Normal1046_0)))) * RU_Material_0._CubemapScale * RU_Material_0._LightScale;

#line 657
    }

#line 657
    if((RU_Material_0._AlphaAdd) == 0.0)
    {

#line 657
        temp_output_1550_0_0 = 1.0;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_0 = MainAlpha142_0;

#line 657
    }

#line 657


#line 657
        ifLocalVar1675_0 = vec4(0.0, 0.0, 0.0, 0.0);

#line 657


#line 657
    vec4 temp_output_145_0_0 = temp_output_1550_0_0 * (ifLocalVar1675_0 + (fnlColor97_0 + DisColor40_0 + Cubemap1144_0));

#line 657
    vec4 switchResult433_0;

#line 657
    if((i_0.ASEIsFrontFacing_0) > 0.0)
    {

#line 657
        switchResult433_0 = temp_output_145_0_0;

#line 657
    }
    else
    {

#line 657
        switchResult433_0 = temp_output_145_0_0 * RU_Material_0._BackFaceColor;

#line 657
    }

#line 657
    vec3 desaturateInitialColor299_0 = switchResult433_0.xyz;

#line 657
    o_1.Emission_0 = mix(desaturateInitialColor299_0, vec3(dot(desaturateInitialColor299_0, vec3(0.29899999499320984, 0.58700001239776611, 0.11400000005960464))), vec3(RU_Material_0._qubaohedu));

#line 657
    return;
}


#line 656
struct UnityLight_0
{
    vec3 color_0;
    vec3 dir_0;
    float ndotl_0;
};


#line 656
struct UnityIndirect_0
{
    vec3 diffuse_0;
    vec3 specular_0;
};


#line 656
struct UnityGI_0
{
    UnityLight_0 light_0;
    UnityIndirect_0 indirect_0;
};


#line 657
void LightingStandardCustomLighting_GI_0(inout SurfaceOutputCustomLightingCustom_0 s_0, UnityGIInput_0 data_0, inout UnityGI_0 gi_0)
{

#line 657
    s_0.GIData_0 = data_0;

#line 657
    return;
}


#line 652
bool IsGammaSpace_0()
{

#line 652
    return true;
}


vec3 UnityWorldSpaceLightDir_0(vec3 p_0)
{

#line 656
    vec3 _S120;

#line 656
    if((any(bvec3((RU_Material_0._RU_MapSunColor.xyz)))))
    {

#line 656
        _S120 = RU_Material_0._RU_MapSunDirection.xyz;

#line 656
    }
    else
    {

#line 656
        _S120 = RU_Material_0._RU_MapMoonDirection.xyz;

#line 656
    }

#line 656
    return _S120;
}


#line 656
UnityGI_0 UnityGI_Base_0(UnityGIInput_0 data_1, float occlusion_0, vec3 normal_0)
{

#line 656
    const vec3 _S121 = vec3(0.0);

#line 656
    UnityLight_0 _S122 = UnityLight_0(_S121, _S121, 0.0);

#line 656
    UnityIndirect_0 _S123 = UnityIndirect_0(_S121, _S121);

#line 656
    UnityGI_0 gi_1;

#line 656
    gi_1.light_0 = _S122;

#line 656
    gi_1.indirect_0 = _S123;

#line 656
    vec4 _S124;

#line 656
    if((any(bvec3((RU_Material_0._RU_MapSunColor.xyz)))))
    {

#line 656
        _S124 = RU_Material_0._RU_MapSunColor;

#line 656
    }
    else
    {

#line 656
        _S124 = RU_Material_0._RU_MapMoonColor;

#line 656
    }

#line 656
    gi_1.light_0.color_0 = _S124.xyz * data_1.atten_0;

#line 656
    gi_1.light_0.dir_0 = UnityWorldSpaceLightDir_0(data_1.worldPos_1);

#line 656
    gi_1.indirect_0.diffuse_0 = vec3(0.0, 0.0, 0.0) * occlusion_0;

#line 656
    return gi_1;
}


#line 657
vec4 LightingStandardCustomLighting_0(inout SurfaceOutputCustomLightingCustom_0 s_1, vec3 viewDir_1, UnityGI_0 gi_2)
{
    //未启用自定义光照时复用 surf 透明度，保留启用时的完整光照路径。
    if (RU_Material_0._IfCustomLight == 0.0) return vec4(0.0, 0.0, 0.0, s_1.Alpha_0);

#line 657
    UnityGIInput_0 data_2 = s_1.GIData_0;

#line 657
    Input_0 i_1 = s_1.SurfInput_0;

#line 657
    vec4 c_0 = vec4(ivec4(0));

#line 657
    vec4 color1051_0;

#line 657
    if((any(bvec3((RU_Material_0._RU_MapSunColor.xyz)))))
    {

#line 657
        color1051_0 = RU_Material_0._RU_MapSunColor;

#line 657
    }
    else
    {

#line 657
        color1051_0 = RU_Material_0._RU_MapMoonColor;

#line 657
    }

#line 657
    float ase_lightAtten_0;

#line 657
    if((color1051_0.w) == 0.0)
    {

#line 657
        ase_lightAtten_0 = 0.0;

#line 657
    }
    else
    {

#line 657
        ase_lightAtten_0 = data_2.atten_0;

#line 657
    }

#line 657
    vec2 appendResult49_g1051_1 = vec2(RU_Material_0._Mask_Uspeed, RU_Material_0._Mask_Vspeed);

#line 657
    float Ifpara1474_1 = RU_Material_0._IfPara;

#line 657
    vec2 appendResult30_g1032_1 = vec2(i_1.uv4_texcoord4_0.x, i_1.uv4_texcoord4_0.y);

#line 394
    vec4 break36_g1032_1 = RU_Material_0._MaskTex_ST.zwxy;

#line 657
    float _S125 = break36_g1032_1.x;

#line 657
    float _S126 = break36_g1032_1.y;

#line 657
    vec2 appendResult32_g1032_1 = vec2(_S125, _S126);

#line 657
    vec2 appendResult38_g1032_1 = vec2(break36_g1032_1.z, break36_g1032_1.w);

#line 657
    vec4 break537_1 = RU_Material_0._TexCenter + vec4(i_1.ruLocalPosition_0, 0.0);

#line 657
    vec2 appendResult554_1 = vec2((atan(break537_1.x / break537_1.z) - -1.57079637050628662) / 3.14159274101257324, break537_1.y);

#line 657
    vec2 _S127 = i_1.uv_texcoord_0.xy;

#line 657
    vec2 CenteredUV15_g1033_1 = _S127 - vec2(RU_Material_0._CenterU, RU_Material_0._CenterV);

#line 657
    float _S128 = length(CenteredUV15_g1033_1);

#line 657
    float _S129 = (atan((CenteredUV15_g1033_1.x),(CenteredUV15_g1033_1.y))) * 0.15915493667125702;

#line 657
    vec2 appendResult23_g1033_1 = vec2(_S128 * _S125 * 2.0, _S129 * _S126);

#line 657
    vec2 _S130 = vec2(ivec2(0));

#line 657
    vec2 ifLocalVar2192_1;

#line 657
    if((RU_Material_0._MaskTexUVS) > 1.0)
    {

#line 657
        ifLocalVar2192_1 = appendResult554_1 * appendResult32_g1032_1 + appendResult38_g1032_1;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._MaskTexUVS) == 1.0)
        {

#line 657
            ifLocalVar2192_1 = appendResult23_g1033_1 + appendResult38_g1032_1;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._MaskTexUVS) < 1.0)
            {

#line 657
                ifLocalVar2192_1 = _S127 * appendResult32_g1032_1 + appendResult38_g1032_1;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2192_1 = _S130;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 temp_output_2194_0_1;

#line 657
    if((RU_Material_0._MaskTexUVS) == 3.0)
    {

#line 657
        temp_output_2194_0_1 = appendResult30_g1032_1 * appendResult32_g1032_1 + appendResult38_g1032_1;

#line 657
    }
    else
    {

#line 657
        temp_output_2194_0_1 = ifLocalVar2192_1;

#line 657
    }

#line 657
    float Parallax1492_1 = RU_Material_0._Parallax * 0.10000000149011612;

#line 657
    const vec3 _S131 = vec3(0.0, 0.0, 1.0);

#line 657
    vec3 ase_worldNormal_1 = vec3(dot(i_1.internalSurfaceTtoW0_0, _S131), dot(i_1.internalSurfaceTtoW1_0, _S131), dot(i_1.internalSurfaceTtoW2_0, _S131));

#line 657
    vec3 ase_worldViewDir_1 = normalize((RU_Material_0._WorldSpaceCameraPos.xyz) - i_1.worldPos_0.xyz);

#line 657
    const vec2 _S132 = vec2(0.0, 0.0);

#line 657
    vec2 OffsetPOM80_g1040_1 = RU_POM_ParaTex_0(temp_output_2194_0_1, dFdx(temp_output_2194_0_1), dFdy(temp_output_2194_0_1), ase_worldNormal_1, ase_worldViewDir_1, i_1.viewDir_0, 64, 64, Parallax1492_1, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S132, 0);

#line 657
    const vec3 _S133 = vec3(0.0);

#line 657
    float chanel45_g1072_1 = 0.0;

#line 657
    float C1x45_g1072_1 = i_1.uv_texcoord_0.z;

#line 657
    float C1y45_g1072_1 = i_1.uv_texcoord_0.w;

#line 657
    float C1z45_g1072_1 = i_1.uv2_texcoord2_0.x;

#line 657
    float C1w45_g1072_1 = i_1.uv2_texcoord2_0.y;

#line 657
    float C2x45_g1072_1 = i_1.uv2_texcoord2_0.z;

#line 657
    float C2y45_g1072_1 = i_1.uv2_texcoord2_0.w;

#line 657
    vec2 _S134 = i_1.uv3_texcoord3_0.xy;

#line 657
    float C2z45_g1072_1 = _S134.x;

#line 657
    float C2w45_g1072_1 = _S134.y;

#line 657
    float _S135 = customchanelswitch45_g1072_0(RU_Material_0._DistortFactorC1, RU_Material_0._DistortFactorC2, chanel45_g1072_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    float DistortFactorC1851_1 = chanel45_g1072_1;

#line 382
    vec4 break36_g1019_1 = RU_Material_0._DistortMaskTex_ST.zwxy;

#line 657
    float cos55_g1021_1 = cos(RU_Material_0._DistortMaskTexR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1021_1 = sin(RU_Material_0._DistortMaskTexR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    const vec2 _S136 = vec2(0.5, 0.5);

#line 657
    vec2 rotator55_g1021_1 = (((mat2(cos55_g1021_1, - sin55_g1021_1, sin55_g1021_1, cos55_g1021_1)) * (RU_Material_0._Time.y * _S132 + (_S127 * vec2(break36_g1019_1.x, break36_g1019_1.y) + vec2(break36_g1019_1.z, break36_g1019_1.w)) - _S136))) + _S136;

#line 657
    float _S137 = rotator55_g1021_1.x;

#line 657
    float clampResult60_g1021_1 = clamp(_S137, 0.0, 1.0);

#line 657
    float _S138 = rotator55_g1021_1.y;

#line 657
    float clampResult50_g1021_1 = clamp(_S138, 0.0, 1.0);

#line 657
    float temp_output_1550_0_1;

#line 657
    if(float(int(RU_Material_0._DistortMaskTexC)) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = _S137;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = clampResult60_g1021_1;

#line 657
    }

#line 657
    float temp_output_1684_0_1;

#line 657
    if(float(int(RU_Material_0._DistortMaskTexCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = _S138;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = clampResult50_g1021_1;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1021_1 = RU_tex2D_DistortMaskTex_0(vec2(temp_output_1550_0_1, temp_output_1684_0_1));

#line 657
    if((RU_Material_0._CustomDistort) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = RU_Material_0._DistortFactor;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = DistortFactorC1851_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._DistortMaskTexAR)) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = tex2DNode27_g1021_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = tex2DNode27_g1021_1.x;

#line 657
    }

#line 657
    float temp_output_1587_0_1 = temp_output_1550_0_1 * temp_output_1684_0_1;

#line 375
    vec4 break36_g1016_1 = RU_Material_0._DistortTex_ST.zwxy;

#line 657
    vec2 panner59_1 = RU_Material_0._Time.y * vec2(RU_Material_0._DistortTex_Uspeed, RU_Material_0._DistortTex_Vspeed) + (_S127 * vec2(break36_g1016_1.x, break36_g1016_1.y) + vec2(break36_g1016_1.z, break36_g1016_1.w));

#line 657
    vec4 tex2DNode54_1 = RU_tex2D_DistortTex_0(panner59_1);

#line 657
    if((RU_Material_0._DistortTexAR) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = tex2DNode54_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = tex2DNode54_1.x;

#line 657
    }

#line 657
    if((RU_Material_0._DistortRemap) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = temp_output_1550_0_1;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = -0.5 + temp_output_1550_0_1;

#line 657
    }

#line 657
    vec3 _S139 = vec3(temp_output_1587_0_1 * temp_output_1684_0_1);

#line 657
    float temp_output_1570_0_1 = 1.0 * 0.10000000149011612;

#line 657
    float _S140 = panner59_1.x;

#line 657
    float _S141 = panner59_1.y;

#line 657
    vec4 tex2DNode1577_1 = RU_tex2D_DistortTex_0(vec2(_S140 + temp_output_1570_0_1, _S141));

#line 657
    float temp_output_1579_0_1;

#line 657
    if((RU_Material_0._DistortTexAR) == 0.0)
    {

#line 657
        temp_output_1579_0_1 = tex2DNode1577_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1579_0_1 = tex2DNode1577_1.y;

#line 657
    }

#line 657
    if((RU_Material_0._DistortRemap) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = temp_output_1579_0_1;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = -0.5 + temp_output_1579_0_1;

#line 657
    }

#line 657
    vec3 appendResult1564_1 = vec3(1.0, 0.0, (temp_output_1550_0_1 - temp_output_1684_0_1) * temp_output_1587_0_1);

#line 657
    vec4 tex2DNode1576_1 = RU_tex2D_DistortTex_0(vec2(_S140, _S141 + temp_output_1570_0_1));

#line 657
    float temp_output_1580_0_1;

#line 657
    if((RU_Material_0._DistortTexAR) == 0.0)
    {

#line 657
        temp_output_1580_0_1 = tex2DNode1576_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1580_0_1 = tex2DNode1576_1.y;

#line 657
    }

#line 657
    if((RU_Material_0._DistortRemap) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = temp_output_1580_0_1;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = -0.5 + temp_output_1580_0_1;

#line 657
    }

#line 657
    vec3 normalizeResult1563_1 = normalize(cross(appendResult1564_1, vec3(0.0, 1.0, (temp_output_1550_0_1 - temp_output_1684_0_1) * temp_output_1587_0_1)));

#line 657
    vec2 appendResult61_1 = vec2(tex2DNode54_1.x, tex2DNode54_1.y);

#line 657
    vec3 DistortUV60_1;

#line 657
    if((RU_Material_0._IfFlowmap) == 0.0)
    {

#line 657
        if((RU_Material_0._IfNormalDistort) == 0.0)
        {

#line 657
            DistortUV60_1 = _S139;

#line 657
        }
        else
        {

#line 657
            DistortUV60_1 = normalizeResult1563_1;

#line 657
        }

#line 657
    }
    else
    {

#line 657
        DistortUV60_1 = vec3(temp_output_1587_0_1 * (appendResult61_1 - _S127), 0.0);

#line 657
    }

#line 657
    vec3 temp_output_1066_0_1;

#line 657
    if((RU_Material_0._DistortMask) == 0.0)
    {

#line 657
        temp_output_1066_0_1 = _S133;

#line 657
    }
    else
    {

#line 657
        temp_output_1066_0_1 = DistortUV60_1;

#line 657
    }

#line 657
    const vec2 _S142 = vec2(0.0);

#line 657
    float chanel45_g1081_1 = 0.0;

#line 657
    float _S143 = customchanelswitch45_g1081_0(RU_Material_0._MaskOffsetUC1, RU_Material_0._MaskOffsetUC2, chanel45_g1081_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    float chanel45_g1075_1 = 0.0;

#line 657
    float _S144 = customchanelswitch45_g1075_0(RU_Material_0._MaskOffsetVC1, RU_Material_0._MaskOffsetVC2, chanel45_g1075_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    vec2 appendResult1836_1 = vec2(chanel45_g1081_1, chanel45_g1075_1);

#line 657
    vec2 _S145 = RU_Material_0._Time.y * appendResult49_g1051_1;

#line 657
    bool _S146 = Ifpara1474_1 == 0.0;

#line 657
    if(_S146)
    {

#line 657
        ifLocalVar2192_1 = temp_output_2194_0_1;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_1 = OffsetPOM80_g1040_1;

#line 657
    }

#line 657
    vec3 _S147 = vec3(ifLocalVar2192_1, 0.0) + temp_output_1066_0_1;

#line 657
    if(bool(RU_Material_0._CustomdataMaskUV))
    {

#line 657
        ifLocalVar2192_1 = appendResult1836_1;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_1 = _S142;

#line 657
    }

#line 657
    float cos55_g1051_1 = cos(RU_Material_0._Mask_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1051_1 = sin(RU_Material_0._Mask_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1051_1 = (((mat2(cos55_g1051_1, - sin55_g1051_1, sin55_g1051_1, cos55_g1051_1)) * (_S145 + (_S147 + vec3(ifLocalVar2192_1, 0.0)).xy - _S136))) + _S136;

#line 657
    float _S148 = rotator55_g1051_1.x;

#line 657
    float clampResult60_g1051_1 = clamp(_S148, 0.0, 1.0);

#line 657
    float _S149 = rotator55_g1051_1.y;

#line 657
    float clampResult50_g1051_1 = clamp(_S149, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._MaskC)) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = _S148;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = clampResult60_g1051_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._MaskCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = _S149;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = clampResult50_g1051_1;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1051_1 = RU_tex2D_MaskTex_0(vec2(temp_output_1550_0_1, temp_output_1684_0_1));

#line 657
    vec2 appendResult49_g1060_1 = vec2(RU_Material_0._MaskPlusUspeed, RU_Material_0._MaskPlusVspeed);

#line 362
    vec4 break36_g1056_1 = RU_Material_0._MaskPlusTex_ST.zwxy;

#line 657
    vec2 appendResult38_g1056_1 = vec2(break36_g1056_1.z, break36_g1056_1.w);

#line 657
    float _S150 = break36_g1056_1.x;

#line 657
    float _S151 = break36_g1056_1.y;

#line 657
    vec2 appendResult31_g1056_1 = vec2(_S150, _S151);

#line 657
    vec2 appendResult23_g1057_1 = vec2(_S128 * _S150 * 2.0, _S129 * _S151);

#line 657
    vec2 ifLocalVar2195_1;

#line 657
    if((RU_Material_0._MaskTexUVS) > 1.0)
    {

#line 657
        ifLocalVar2195_1 = appendResult554_1 * appendResult31_g1056_1 + appendResult38_g1056_1;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._MaskTexUVS) == 1.0)
        {

#line 657
            ifLocalVar2195_1 = appendResult23_g1057_1 + appendResult38_g1056_1;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._MaskTexUVS) < 1.0)
            {

#line 657
                ifLocalVar2195_1 = _S127 * appendResult31_g1056_1 + appendResult38_g1056_1;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2195_1 = _S130;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    float cos55_g1060_1 = cos(RU_Material_0._MaskPlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1060_1 = sin(RU_Material_0._MaskPlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1060_1 = (((mat2(cos55_g1060_1, - sin55_g1060_1, sin55_g1060_1, cos55_g1060_1)) * (RU_Material_0._Time.y * appendResult49_g1060_1 + (temp_output_1066_0_1 + vec3(ifLocalVar2195_1, 0.0)).xy - _S136))) + _S136;

#line 657
    float _S152 = rotator55_g1060_1.x;

#line 657
    float clampResult60_g1060_1 = clamp(_S152, 0.0, 1.0);

#line 657
    float _S153 = rotator55_g1060_1.y;

#line 657
    float clampResult50_g1060_1 = clamp(_S153, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._MaskPlusC)) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = _S152;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = clampResult60_g1060_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._MaskPlusCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = _S153;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = clampResult50_g1060_1;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1060_1 = RU_tex2D_MaskPlusTex_0(vec2(temp_output_1550_0_1, temp_output_1684_0_1));

#line 657
    float _S154 = RU_Material_0._Mask_scale;

#line 657
    if(float(int(RU_Material_0._MaskAlphaRA)) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = tex2DNode27_g1051_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = tex2DNode27_g1051_1.x;

#line 657
    }

#line 657
    if((RU_Material_0._IfMaskPlusTex) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = 1.0;

#line 657
    }
    else
    {

#line 657
        if(float(int(RU_Material_0._MaskPlusAR)) == 0.0)
        {

#line 657
            temp_output_1684_0_1 = tex2DNode27_g1060_1.w;

#line 657
        }
        else
        {

#line 657
            temp_output_1684_0_1 = tex2DNode27_g1060_1.x;

#line 657
        }

#line 657
    }

#line 657
    float MaskAlpha136_1 = _S154 * (temp_output_1550_0_1 * temp_output_1684_0_1);

#line 657
    vec2 appendResult49_g1047_1 = vec2(RU_Material_0._MainTex_Uspeed, RU_Material_0._MainTex_Vspeed);

#line 657
    float chanel45_g1080_1 = 0.0;

#line 657
    float _S155 = customchanelswitch45_g1080_0(RU_Material_0._MainOffsetUC1, RU_Material_0._MainOffsetUC2, chanel45_g1080_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    float chanel45_g1073_1 = 0.0;

#line 657
    float _S156 = customchanelswitch45_g1073_0(RU_Material_0._MainOffsetVC1, RU_Material_0._MainOffsetVC2, chanel45_g1073_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    vec2 appendResult1822_1 = vec2(chanel45_g1080_1, chanel45_g1073_1);

#line 657
    vec2 temp_output_1691_0_1;

#line 657
    if((RU_Material_0._CustomdataMainTexUV) == 0.0)
    {

#line 657
        temp_output_1691_0_1 = _S142;

#line 657
    }
    else
    {

#line 657
        temp_output_1691_0_1 = appendResult1822_1;

#line 657
    }

#line 347
    vec4 break36_g1028_1 = RU_Material_0._MainTex_ST.zwxy;

#line 657
    vec2 staticSwitch607_1 = _S127 * vec2(break36_g1028_1.x, break36_g1028_1.y) + vec2(break36_g1028_1.z, break36_g1028_1.w);

#line 657
    vec2 OffsetPOM80_g1039_1 = RU_POM_ParaTex_0(staticSwitch607_1, dFdx(staticSwitch607_1), dFdy(staticSwitch607_1), ase_worldNormal_1, ase_worldViewDir_1, i_1.viewDir_0, 64, 64, Parallax1492_1, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S132, 0);

#line 657
    vec3 temp_output_1057_0_1;

#line 657
    if((RU_Material_0._DistortMainTex) == 0.0)
    {

#line 657
        temp_output_1057_0_1 = _S133;

#line 657
    }
    else
    {

#line 657
        temp_output_1057_0_1 = DistortUV60_1;

#line 657
    }

#line 657
    vec2 _S157 = RU_Material_0._Time.y * appendResult49_g1047_1;

#line 657
    vec3 _S158 = vec3(temp_output_1691_0_1, 0.0);

#line 657
    if(_S146)
    {

#line 657
        ifLocalVar2192_1 = staticSwitch607_1;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_1 = OffsetPOM80_g1039_1;

#line 657
    }

#line 657
    float cos55_g1047_1 = cos(RU_Material_0._MainTex_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1047_1 = sin(RU_Material_0._MainTex_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1047_1 = (((mat2(cos55_g1047_1, - sin55_g1047_1, sin55_g1047_1, cos55_g1047_1)) * (_S157 + (_S158 + (vec3(ifLocalVar2192_1, 0.0) + temp_output_1057_0_1)).xy - _S136))) + _S136;

#line 657
    float _S159 = rotator55_g1047_1.x;

#line 657
    float clampResult60_g1047_1 = clamp(_S159, 0.0, 1.0);

#line 657
    float _S160 = rotator55_g1047_1.y;

#line 657
    float clampResult50_g1047_1 = clamp(_S160, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._MaintexC)) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = _S159;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = clampResult60_g1047_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._MaintexCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = _S160;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = clampResult50_g1047_1;

#line 657
    }

#line 657
    vec2 appendResult53_g1047_1 = vec2(temp_output_1550_0_1, temp_output_1684_0_1);

#line 657
    vec4 tex2DNode64_g1047_1 = RU_tex2D_MainTex_0(appendResult53_g1047_1 + RU_Material_0._MainTexAC);

#line 657
    float _S161 = tex2DNode64_g1047_1.x;

#line 657
    float _S162 = (RU_tex2D_MainTex_0(appendResult53_g1047_1).w + tex2DNode64_g1047_1.w + RU_tex2D_MainTex_0(appendResult53_g1047_1 - vec2(RU_Material_0._MainTexAC)).w) / 3.0;

#line 657
    float temp_output_2038_39_1;

#line 657
    if(float(int(RU_Material_0._MainTex_ar)) == 0.0)
    {

#line 657
        temp_output_2038_39_1 = _S162;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_1 = _S161;

#line 657
    }

#line 657
    vec4 ase_grabScreenPos_1 = ASE_ComputeGrabScreenPos_0(vec4(i_1.screenPos_0.xyz, i_1.screenPos_0.w + 9.999999960041972e-12));

#line 657


#line 657
    if((RU_Material_0._CustomdataDisT) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = i_1.vertexColor_0.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = 1.0;

#line 657
    }

#line 657


#line 657
        temp_output_1684_0_1 = temp_output_2038_39_1;

#line 657


#line 657
    float temp_output_7_0_1 = temp_output_1550_0_1 * temp_output_1684_0_1 * RU_Material_0._MainColor.w;

#line 657
    vec2 appendResult49_g1046_1 = vec2(RU_Material_0._AddTexUspeed, RU_Material_0._AddTexVspeed);

#line 335
    vec4 break36_g1030_1 = RU_Material_0._AddTex_ST.zwxy;

#line 657
    float _S163 = break36_g1030_1.x;

#line 657
    float _S164 = break36_g1030_1.y;

#line 657
    vec2 appendResult32_g1030_1 = vec2(_S163, _S164);

#line 657
    vec2 appendResult38_g1030_1 = vec2(break36_g1030_1.z, break36_g1030_1.w);

#line 657
    vec2 appendResult23_g1031_1 = vec2(_S128 * _S163 * 2.0, _S129 * _S164);

#line 657
    vec2 ifLocalVar1687_1;

#line 657
    if((RU_Material_0._AddTexUVS) > 1.0)
    {

#line 657
        ifLocalVar1687_1 = appendResult554_1 * appendResult32_g1030_1 + appendResult38_g1030_1;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._AddTexUVS) == 1.0)
        {

#line 657
            ifLocalVar1687_1 = appendResult23_g1031_1 + appendResult38_g1030_1;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._AddTexUVS) < 1.0)
            {

#line 657
                ifLocalVar1687_1 = _S127 * appendResult32_g1030_1 + appendResult38_g1030_1;

#line 657
            }
            else
            {

#line 657
                ifLocalVar1687_1 = _S130;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 temp_output_2191_0_1;

#line 657
    if((RU_Material_0._AddTexUVS) == 3.0)
    {

#line 657
        temp_output_2191_0_1 = appendResult30_g1032_1 * appendResult32_g1030_1 + appendResult38_g1030_1;

#line 657
    }
    else
    {

#line 657
        temp_output_2191_0_1 = ifLocalVar1687_1;

#line 657
    }

#line 657
    vec2 OffsetPOM80_g1042_1 = RU_POM_ParaTex_0(temp_output_2191_0_1, dFdx(temp_output_2191_0_1), dFdy(temp_output_2191_0_1), ase_worldNormal_1, ase_worldViewDir_1, i_1.viewDir_0, 64, 64, Parallax1492_1, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S132, 0);

#line 657
    const vec2 _S165 = vec2(1.0);

#line 657
    vec2 appendResult1703_1 = vec2(RU_Material_0._AddTex_ST.zwxy.x / RU_Material_0._MainTex_ST.zwxy.x, RU_Material_0._AddTex_ST.zwxy.y / RU_Material_0._MainTex_ST.zwxy.y);

#line 657
    vec2 _S166 = RU_Material_0._Time.y * appendResult49_g1046_1;

#line 657
    if(_S146)
    {

#line 657
        ifLocalVar2192_1 = temp_output_2191_0_1;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_1 = OffsetPOM80_g1042_1;

#line 657
    }

#line 657
    vec3 _S167 = vec3(ifLocalVar2192_1, 0.0);

#line 657
    if((RU_Material_0._DistortAddTex) == 0.0)
    {

#line 657
        temp_output_1066_0_1 = _S133;

#line 657
    }
    else
    {

#line 657
        temp_output_1066_0_1 = DistortUV60_1;

#line 657
    }

#line 657
    vec3 _S168 = _S167 + temp_output_1066_0_1;

#line 657
    if((RU_Material_0._CAddTexUV) == 0.0)
    {

#line 657
        ifLocalVar2192_1 = _S142;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CAddTexUVT) == 0.0)
        {

#line 657
            ifLocalVar2192_1 = _S165;

#line 657
        }
        else
        {

#line 657
            ifLocalVar2192_1 = appendResult1703_1;

#line 657
        }

#line 657
        ifLocalVar2192_1 = ifLocalVar2192_1 * temp_output_1691_0_1;

#line 657
    }

#line 657
    float cos55_g1046_1 = cos(RU_Material_0._AddTexRo / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1046_1 = sin(RU_Material_0._AddTexRo / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1046_1 = (((mat2(cos55_g1046_1, - sin55_g1046_1, sin55_g1046_1, cos55_g1046_1)) * (_S166 + (_S168 + vec3(ifLocalVar2192_1, 0.0)).xy - _S136))) + _S136;

#line 657
    float _S169 = rotator55_g1046_1.x;

#line 657
    float clampResult60_g1046_1 = clamp(_S169, 0.0, 1.0);

#line 657
    float _S170 = rotator55_g1046_1.y;

#line 657
    float clampResult50_g1046_1 = clamp(_S170, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._AddTexC)) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = _S169;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = clampResult60_g1046_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._AddTexCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = _S170;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = clampResult50_g1046_1;

#line 657
    }

#line 657
    vec2 appendResult53_g1046_1 = vec2(temp_output_1550_0_1, temp_output_1684_0_1);

#line 657
    vec4 tex2DNode64_g1046_1 = RU_tex2D_AddTex_0(appendResult53_g1046_1 + RU_Material_0._AddTexAC);

#line 657
    float _S171 = tex2DNode64_g1046_1.x;

#line 657
    float _S172 = (RU_tex2D_AddTex_0(appendResult53_g1046_1).w + tex2DNode64_g1046_1.w + RU_tex2D_AddTex_0(appendResult53_g1046_1 - vec2(RU_Material_0._AddTexAC)).w) / 3.0;

#line 657
    float temp_output_2046_39_1;

#line 657
    if(float(int(RU_Material_0._AddTexAR)) == 0.0)
    {

#line 657
        temp_output_2046_39_1 = _S172;

#line 657
    }
    else
    {

#line 657
        temp_output_2046_39_1 = _S171;

#line 657
    }

#line 657
    float lerpResult1659_1 = mix(temp_output_7_0_1, temp_output_2046_39_1, RU_Material_0._AddTexBlend);

#line 657
    float ifLocalVar1676_1;

#line 657
    if((RU_Material_0._AddTexBlendMode) > 1.0)
    {

#line 657
        ifLocalVar1676_1 = temp_output_2046_39_1 * temp_output_7_0_1;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._AddTexBlendMode) == 1.0)
        {

#line 657
            ifLocalVar1676_1 = temp_output_2046_39_1 + temp_output_7_0_1;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._AddTexBlendMode) < 1.0)
            {

#line 657
                ifLocalVar1676_1 = lerpResult1659_1;

#line 657
            }
            else
            {

#line 657
                ifLocalVar1676_1 = 0.0;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    if((RU_Material_0._IfAddTexAlpha) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = temp_output_7_0_1;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = ifLocalVar1676_1;

#line 657
    }

#line 657
    float MainTexAlpha37_1 = temp_output_1550_0_1 * RU_Material_0._MainAlpha;

#line 657
    float temp_output_65_0_g1048_1 = RU_Material_0._DIssloveSoft;

#line 657
    float chanel45_g1076_1 = 0.0;

#line 657
    float _S173 = customchanelswitch45_g1076_0(RU_Material_0._DissolveFactorC1, RU_Material_0._DissolveFactorC2, chanel45_g1076_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    float DissolveFactorC1856_1 = chanel45_g1076_1;

#line 657
    float temp_output_70_0_g1048_1;

#line 657
    if((RU_Material_0._CustomdataDis) == 0.0)
    {

#line 657
        temp_output_70_0_g1048_1 = RU_Material_0._DIssloveFactor + 0.00100000004749745;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CustomdataDisT) == 0.0)
        {

#line 657
            temp_output_1550_0_1 = DissolveFactorC1856_1;

#line 657
        }
        else
        {

#line 657
            temp_output_1550_0_1 = 1.0 + i_1.vertexColor_0.w * -1.0;

#line 657
        }

#line 657
        temp_output_70_0_g1048_1 = temp_output_1550_0_1;

#line 657
    }

#line 657
    float temp_output_77_0_g1048_1 = (temp_output_65_0_g1048_1 + 1.0) * temp_output_70_0_g1048_1;

#line 657
    int temp_output_38_0_g1045_1 = int(RU_Material_0._DissolveAR);

#line 657
    vec2 appendResult49_g1045_1 = vec2(RU_Material_0._DisTex_Uspeed, RU_Material_0._DisTex_Vspeed);

#line 315
    vec4 break36_g1070_1 = RU_Material_0._DissloveTex_ST.zwxy;

#line 657
    float _S174 = break36_g1070_1.x;

#line 657
    float _S175 = break36_g1070_1.y;

#line 657
    vec2 appendResult32_g1070_1 = vec2(_S174, _S175);

#line 657
    vec2 appendResult38_g1070_1 = vec2(break36_g1070_1.z, break36_g1070_1.w);

#line 657
    vec2 appendResult23_g1071_1 = vec2(_S128 * _S174 * 2.0, _S129 * _S175);

#line 657
    vec2 ifLocalVar2207_1;

#line 657
    if((RU_Material_0._DissolveTexUVS2) > 1.0)
    {

#line 657
        ifLocalVar2207_1 = appendResult554_1 * appendResult32_g1070_1 + appendResult38_g1070_1;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._DissolveTexUVS2) == 1.0)
        {

#line 657
            ifLocalVar2207_1 = appendResult23_g1071_1 + appendResult38_g1070_1;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._DissolveTexUVS2) < 1.0)
            {

#line 657
                ifLocalVar2207_1 = _S127 * appendResult32_g1070_1 + appendResult38_g1070_1;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2207_1 = _S130;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 temp_output_2209_0_1;

#line 657
    if((RU_Material_0._DissolveTexUVS2) == 3.0)
    {

#line 657
        temp_output_2209_0_1 = appendResult30_g1032_1 * appendResult32_g1070_1 + appendResult38_g1070_1;

#line 657
    }
    else
    {

#line 657
        temp_output_2209_0_1 = ifLocalVar2207_1;

#line 657
    }

#line 657
    vec2 OffsetPOM80_g1038_1 = RU_POM_ParaTex_0(temp_output_2209_0_1, dFdx(temp_output_2209_0_1), dFdy(temp_output_2209_0_1), ase_worldNormal_1, ase_worldViewDir_1, i_1.viewDir_0, 64, 64, Parallax1492_1, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S132, 0);

#line 657
    float chanel45_g1079_1 = 0.0;

#line 657
    float _S176 = customchanelswitch45_g1079_0(RU_Material_0._DissolveOffsetUC1, RU_Material_0._DissolveOffsetUC2, chanel45_g1079_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    float chanel45_g1074_1 = 0.0;

#line 657
    float _S177 = customchanelswitch45_g1074_0(RU_Material_0._DissolveOffsetVC1, RU_Material_0._DissolveOffsetVC2, chanel45_g1074_1, C1x45_g1072_1, C1y45_g1072_1, C1z45_g1072_1, C1w45_g1072_1, C2x45_g1072_1, C2y45_g1072_1, C2z45_g1072_1, C2w45_g1072_1, 0.0);

#line 657
    vec2 appendResult1844_1 = vec2(chanel45_g1079_1, chanel45_g1074_1);

#line 657
    vec2 temp_output_1860_0_1;

#line 657
    if((RU_Material_0._IfDissolveOffsetC) == 0.0)
    {

#line 657
        temp_output_1860_0_1 = _S142;

#line 657
    }
    else
    {

#line 657
        temp_output_1860_0_1 = appendResult1844_1;

#line 657
    }

#line 657
    vec2 _S178 = RU_Material_0._Time.y * appendResult49_g1045_1;

#line 657
    if(_S146)
    {

#line 657
        ifLocalVar2192_1 = temp_output_2209_0_1;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_1 = OffsetPOM80_g1038_1;

#line 657
    }

#line 657
    vec3 _S179 = vec3(ifLocalVar2192_1, 0.0);

#line 657
    if((RU_Material_0._DistortDisTex) == 0.0)
    {

#line 657
        temp_output_1066_0_1 = _S133;

#line 657
    }
    else
    {

#line 657
        temp_output_1066_0_1 = DistortUV60_1;

#line 657
    }

#line 657
    float cos55_g1045_1 = cos(RU_Material_0._DIssolve_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1045_1 = sin(RU_Material_0._DIssolve_rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1045_1 = (((mat2(cos55_g1045_1, - sin55_g1045_1, sin55_g1045_1, cos55_g1045_1)) * (_S178 + (_S179 + temp_output_1066_0_1 + vec3(temp_output_1860_0_1, 0.0)).xy - _S136))) + _S136;

#line 657
    float _S180 = rotator55_g1045_1.x;

#line 657
    float clampResult60_g1045_1 = clamp(_S180, 0.0, 1.0);

#line 657
    float _S181 = rotator55_g1045_1.y;

#line 657
    float clampResult50_g1045_1 = clamp(_S181, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._DissolveC)) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = _S180;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = clampResult60_g1045_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._DissolveCV)) == 0.0)
    {

#line 657
        temp_output_1684_0_1 = _S181;

#line 657
    }
    else
    {

#line 657
        temp_output_1684_0_1 = clampResult50_g1045_1;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1045_1 = RU_tex2D_DissloveTex_0(vec2(temp_output_1550_0_1, temp_output_1684_0_1));

#line 657
    bool _S182 = float(temp_output_38_0_g1045_1) == 0.0;

#line 657
    if(_S182)
    {

#line 657
        temp_output_1579_0_1 = tex2DNode27_g1045_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1579_0_1 = tex2DNode27_g1045_1.x;

#line 657
    }

#line 657
    float temp_output_85_0_g1048_1 = saturate_0(pow(temp_output_1579_0_1, RU_Material_0._DissolveTexExp));

#line 657
    int temp_output_38_0_g1044_1 = int(RU_Material_0._DissolvePlusAR);

#line 301
    vec4 break36_g1034_1 = RU_Material_0._DissloveTexPlus_ST.zwxy;

#line 657
    vec2 appendResult38_g1034_1 = vec2(break36_g1034_1.z, break36_g1034_1.w);

#line 657
    float _S183 = break36_g1034_1.x;

#line 657
    float _S184 = break36_g1034_1.y;

#line 657
    vec2 appendResult31_g1034_1 = vec2(_S183, _S184);

#line 657
    vec2 appendResult23_g1035_1 = vec2(_S128 * _S183 * 2.0, _S129 * _S184);

#line 657
    vec2 ifLocalVar2211_1;

#line 657
    if((RU_Material_0._DissolveTexUVS2) > 1.0)
    {

#line 657
        ifLocalVar2211_1 = appendResult554_1 * appendResult31_g1034_1 + appendResult38_g1034_1;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._DissolveTexUVS2) == 1.0)
        {

#line 657
            ifLocalVar2211_1 = appendResult23_g1035_1 + appendResult38_g1034_1;

#line 657
        }
        else
        {

#line 657
            if((RU_Material_0._DissolveTexUVS2) < 1.0)
            {

#line 657
                ifLocalVar2211_1 = _S127 * appendResult31_g1034_1 + appendResult38_g1034_1;

#line 657
            }
            else
            {

#line 657
                ifLocalVar2211_1 = _S130;

#line 657
            }

#line 657
        }

#line 657
    }

#line 657
    vec2 OffsetPOM80_g1043_1 = RU_POM_ParaTex_0(ifLocalVar2211_1, dFdx(ifLocalVar2211_1), dFdy(ifLocalVar2211_1), ase_worldNormal_1, ase_worldViewDir_1, i_1.viewDir_0, 64, 64, Parallax1492_1, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S132, 0);

#line 657
    vec2 _S185 = RU_Material_0._Time.y * _S132;

#line 657
    if(_S146)
    {

#line 657
        ifLocalVar2192_1 = ifLocalVar2211_1;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_1 = OffsetPOM80_g1043_1;

#line 657
    }

#line 657
    float cos55_g1044_1 = cos(RU_Material_0._DissolvePlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin55_g1044_1 = sin(RU_Material_0._DissolvePlusR / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator55_g1044_1 = (((mat2(cos55_g1044_1, - sin55_g1044_1, sin55_g1044_1, cos55_g1044_1)) * (_S185 + ifLocalVar2192_1 - _S136))) + _S136;

#line 657
    float _S186 = rotator55_g1044_1.x;

#line 657
    float clampResult60_g1044_1 = clamp(_S186, 0.0, 1.0);

#line 657
    float _S187 = rotator55_g1044_1.y;

#line 657
    float clampResult50_g1044_1 = clamp(_S187, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._DissolvePlusC)) == 0.0)
    {

#line 657
        temp_output_1579_0_1 = _S186;

#line 657
    }
    else
    {

#line 657
        temp_output_1579_0_1 = clampResult60_g1044_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._DissolvePlusCV)) == 0.0)
    {

#line 657
        temp_output_1580_0_1 = _S187;

#line 657
    }
    else
    {

#line 657
        temp_output_1580_0_1 = clampResult50_g1044_1;

#line 657
    }

#line 657
    vec4 tex2DNode27_g1044_1 = RU_tex2D_DissloveTexPlus_0(vec2(temp_output_1579_0_1, temp_output_1580_0_1));

#line 657
    float _S188 = temp_output_85_0_g1048_1 / RU_Material_0._DissolveTexDivide;

#line 657
    if((RU_Material_0._IfDissolvePlus) == 0.0)
    {

#line 657
        temp_output_2038_39_1 = temp_output_85_0_g1048_1;

#line 657
    }
    else
    {

#line 657
        if(float(temp_output_38_0_g1044_1) == 0.0)
        {

#line 657
            temp_output_2038_39_1 = tex2DNode27_g1044_1.w;

#line 657
        }
        else
        {

#line 657
            temp_output_2038_39_1 = tex2DNode27_g1044_1.x;

#line 657
        }

#line 657
    }

#line 657
    float temp_output_74_0_g1048_1 = saturate_0((_S188 + temp_output_2038_39_1) / 2.0);

#line 657
    float smoothstepResult76_g1048_1 = RU_CompatSmoothstep(temp_output_77_0_g1048_1 - temp_output_65_0_g1048_1, temp_output_77_0_g1048_1, temp_output_74_0_g1048_1);

#line 657
    float temp_output_2237_91_1 = step(temp_output_70_0_g1048_1 * (1.0 + RU_Material_0._DIssloveWide) - RU_Material_0._DIssloveWide, temp_output_74_0_g1048_1);

#line 657
    float DisAplha42_1;

#line 657
    if((RU_Material_0._sot_sting_A) == 0.0)
    {

#line 657
        DisAplha42_1 = smoothstepResult76_g1048_1;

#line 657
    }
    else
    {

#line 657
        DisAplha42_1 = temp_output_2237_91_1;

#line 657
    }

#line 657
    float dotResult106_1 = dot(ase_worldViewDir_1, ase_worldNormal_1);

#line 657
    if(bool(RU_Material_0._softback))
    {

#line 657
        temp_output_2038_39_1 = dotResult106_1;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_1 = abs(dotResult106_1);

#line 657
    }

#line 657
    float softedge111_1 = pow(saturate_0(temp_output_2038_39_1), RU_Material_0._softFacotr);

#line 657
    vec4 ase_screenPosNorm88_1 = i_1.screenPosition88_0 / i_1.screenPosition88_0.w;

#line 657
    ase_screenPosNorm88_1[2] = ase_screenPosNorm88_1.z;

#line 657


#line 657
    float temp_output_94_0_1 = saturate_0(pow(1.0 - saturate_0(abs(dot(ase_worldNormal_1, normalize(ase_worldViewDir_1 + (RU_Material_0._Dir.xyz))))), RU_Material_0._fnl_power) * RU_Material_0._fnl_sacle);

#line 657


#line 657
    float _S189 = MaskAlpha136_1 * MainTexAlpha37_1 * DisAplha42_1;

#line 657
    if(bool(RU_Material_0._FNLfanxiangkaiguan))
    {

#line 657
        temp_output_2038_39_1 = softedge111_1;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_1 = 1.0;

#line 657
    }

#line 657
    float _S190 = _S189 * temp_output_2038_39_1;

#line 657


#line 657
        temp_output_2038_39_1 = 1.0;

#line 657


#line 657
    float _S191 = _S190 * temp_output_2038_39_1;

#line 657
    if(bool(RU_Material_0._IfFNLAlpha))
    {

#line 657
        temp_output_2038_39_1 = temp_output_94_0_1;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_1 = 1.0;

#line 657
    }

#line 657
    float _S192 = _S191 * temp_output_2038_39_1;

#line 657


#line 657
        temp_output_2038_39_1 = 0.0;

#line 657


#line 657
    float MainAlpha142_1 = saturate_0(_S192 + temp_output_2038_39_1);

#line 657
    if(IsGammaSpace_0())
    {

#line 657
        color1051_0 = vec4(0.0, 0.0, 0.0, 0.0);

#line 657
    }
    else
    {

#line 657
        color1051_0 = vec4(0.0, 0.0, 0.0, 0.0);

#line 657
    }

#line 657
    vec3 ase_worldlightDir_0 = normalize(UnityWorldSpaceLightDir_0(i_1.worldPos_0));

#line 657
    vec3 normalizeResult1001_0 = normalize(ase_worldViewDir_1 + ase_worldlightDir_0);

#line 271
    vec4 break36_g1054_1 = RU_Material_0._NormalTex_ST.zwxy;

#line 657
    vec2 staticSwitch2179_1 = _S127 * vec2(break36_g1054_1.x, break36_g1054_1.y) + vec2(break36_g1054_1.z, break36_g1054_1.w);

#line 657
    vec2 OffsetPOM80_g1058_1 = RU_POM_ParaTex_0(staticSwitch2179_1, dFdx(staticSwitch2179_1), dFdy(staticSwitch2179_1), ase_worldNormal_1, ase_worldViewDir_1, i_1.viewDir_0, 64, 64, Parallax1492_1, 0.0, RU_Material_0._ParaTex_ST.zwxy.xy, _S132, 0);

#line 657
    vec2 _S193 = RU_Material_0._Time.y * vec2(RU_Material_0._NormalTex_Uspeed, RU_Material_0._NormalTex_Vspeed);

#line 657
    if(_S146)
    {

#line 657
        ifLocalVar2192_1 = staticSwitch2179_1;

#line 657
    }
    else
    {

#line 657
        ifLocalVar2192_1 = OffsetPOM80_g1058_1;

#line 657
    }

#line 657
    vec3 _S194 = vec3(ifLocalVar2192_1, 0.0);

#line 657
    if((RU_Material_0._DistortNormalTex) == 0.0)
    {

#line 657
        DistortUV60_1 = _S133;

#line 657
    }

#line 657
    float cos62_g1059_1 = cos(RU_Material_0._NormalTex_Rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    float sin62_g1059_1 = sin(RU_Material_0._NormalTex_Rotat / 360.0 * 3.14159274101257324 * 2.0);

#line 657
    vec2 rotator62_g1059_1 = (((mat2(cos62_g1059_1, - sin62_g1059_1, sin62_g1059_1, cos62_g1059_1)) * (_S193 + (_S194 + DistortUV60_1).xy - _S136))) + _S136;

#line 657
    float _S195 = rotator62_g1059_1.x;

#line 657
    float clampResult67_g1059_1 = clamp(_S195, 0.0, 1.0);

#line 657
    float _S196 = rotator62_g1059_1.y;

#line 657
    float clampResult57_g1059_1 = clamp(_S196, 0.0, 1.0);

#line 657
    if(float(int(RU_Material_0._NormalTexC)) == 0.0)
    {

#line 657
        temp_output_2038_39_1 = _S195;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_1 = clampResult67_g1059_1;

#line 657
    }

#line 657
    if(float(int(RU_Material_0._NormalTexCV)) == 0.0)
    {

#line 657
        temp_output_2046_39_1 = _S196;

#line 657
    }
    else
    {

#line 657
        temp_output_2046_39_1 = clampResult57_g1059_1;

#line 657
    }

#line 657
    vec2 appendResult60_g1059_1 = vec2(temp_output_2038_39_1, temp_output_2046_39_1);

#line 657
    float temp_output_65_0_g1050_1 = RU_Material_0._DIssloveSoft;

#line 657
    float temp_output_70_0_g1050_1;

#line 657
    if((RU_Material_0._CustomdataDis) == 0.0)
    {

#line 657
        temp_output_70_0_g1050_1 = RU_Material_0._DIssloveFactor + 0.00100000004749745;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CustomdataDisT) == 0.0)
        {

#line 657
            temp_output_2038_39_1 = DissolveFactorC1856_1;

#line 657
        }
        else
        {

#line 657
            temp_output_2038_39_1 = 1.0 + i_1.vertexColor_0.w * -1.0;

#line 657
        }

#line 657
        temp_output_70_0_g1050_1 = temp_output_2038_39_1;

#line 657
    }

#line 657
    float temp_output_77_0_g1050_1 = (temp_output_65_0_g1050_1 + 1.0) * temp_output_70_0_g1050_1;

#line 657
    float temp_output_64_0_g1045_1 = 0.0 * 0.10000000149011612;

#line 657
    vec4 tex2DNode70_g1045_1 = RU_tex2D_DissloveTex_0(vec2(temp_output_1550_0_1 + temp_output_64_0_g1045_1, temp_output_1684_0_1));

#line 657
    if(_S182)
    {

#line 657
        temp_output_2038_39_1 = tex2DNode70_g1045_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_2038_39_1 = tex2DNode70_g1045_1.x;

#line 657
    }

#line 657
    float temp_output_85_0_g1050_1 = saturate_0(pow(temp_output_2038_39_1, RU_Material_0._DissolveTexExp));

#line 657
    float temp_output_64_0_g1044_1 = 0.0 * 0.10000000149011612;

#line 657
    vec4 tex2DNode70_g1044_1 = RU_tex2D_DissloveTexPlus_0(vec2(temp_output_1579_0_1 + temp_output_64_0_g1044_1, temp_output_1580_0_1));

#line 657
    float _S197 = temp_output_85_0_g1050_1 / RU_Material_0._DissolveTexDivide;

#line 657
    if((RU_Material_0._IfDissolvePlus) == 0.0)
    {

#line 657
        temp_output_2038_39_1 = temp_output_85_0_g1050_1;

#line 657
    }
    else
    {

#line 657
        if(float(temp_output_38_0_g1044_1) == 0.0)
        {

#line 657
            temp_output_2038_39_1 = tex2DNode70_g1044_1.w;

#line 657
        }
        else
        {

#line 657
            temp_output_2038_39_1 = tex2DNode70_g1044_1.x;

#line 657
        }

#line 657
    }

#line 657
    float temp_output_74_0_g1050_1 = saturate_0((_S197 + temp_output_2038_39_1) / 2.0);

#line 657
    float smoothstepResult76_g1050_1 = RU_CompatSmoothstep(temp_output_77_0_g1050_1 - temp_output_65_0_g1050_1, temp_output_77_0_g1050_1, temp_output_74_0_g1050_1);

#line 657
    float temp_output_66_0_g1050_1 = RU_Material_0._DIssloveWide;

#line 657
    float temp_output_64_0_g1050_1 = temp_output_70_0_g1050_1 * (1.0 + RU_Material_0._DIssloveWide);

#line 657
    float DisAplha11397_1;

#line 657
    if((RU_Material_0._sot_sting_A) == 0.0)
    {

#line 657
        DisAplha11397_1 = smoothstepResult76_g1050_1;

#line 657
    }
    else
    {

#line 657
        DisAplha11397_1 = step(temp_output_64_0_g1050_1 - temp_output_66_0_g1050_1, temp_output_74_0_g1050_1);

#line 657
    }

#line 657
    float temp_output_1402_0_1 = RU_Material_0._StaticNormalScale * 10.0;

#line 657
    vec3 appendResult1273_1 = vec3(1.0, 0.0, (DisAplha11397_1 - DisAplha42_1) * temp_output_1402_0_1);

#line 657
    float temp_output_65_0_g1049_1 = RU_Material_0._DIssloveSoft;

#line 657
    float temp_output_70_0_g1049_1;

#line 657
    if((RU_Material_0._CustomdataDis) == 0.0)
    {

#line 657
        temp_output_70_0_g1049_1 = RU_Material_0._DIssloveFactor + 0.00100000004749745;

#line 657
    }
    else
    {

#line 657
        if((RU_Material_0._CustomdataDisT) == 0.0)
        {

#line 657
            temp_output_2038_39_1 = DissolveFactorC1856_1;

#line 657
        }
        else
        {

#line 657
            temp_output_2038_39_1 = 1.0 + i_1.vertexColor_0.w * -1.0;

#line 657
        }

#line 657
        temp_output_70_0_g1049_1 = temp_output_2038_39_1;

#line 657
    }

#line 657
    float temp_output_77_0_g1049_1 = (temp_output_65_0_g1049_1 + 1.0) * temp_output_70_0_g1049_1;

#line 657
    vec4 tex2DNode76_g1045_1 = RU_tex2D_DissloveTex_0(vec2(temp_output_1550_0_1, temp_output_1684_0_1 + temp_output_64_0_g1045_1));

#line 657
    if(_S182)
    {

#line 657
        temp_output_1550_0_1 = tex2DNode76_g1045_1.w;

#line 657
    }
    else
    {

#line 657
        temp_output_1550_0_1 = tex2DNode76_g1045_1.x;

#line 657
    }

#line 657
    float temp_output_85_0_g1049_1 = saturate_0(pow(temp_output_1550_0_1, RU_Material_0._DissolveTexExp));

#line 657
    vec4 tex2DNode76_g1044_1 = RU_tex2D_DissloveTexPlus_0(vec2(temp_output_1579_0_1, temp_output_1580_0_1 + temp_output_64_0_g1044_1));

#line 657
    float _S198 = temp_output_85_0_g1049_1 / RU_Material_0._DIssloveSoft;

#line 657
    if((RU_Material_0._IfDissolvePlus) == 0.0)
    {

#line 657
        temp_output_1550_0_1 = temp_output_85_0_g1049_1;

#line 657
    }
    else
    {

#line 657
        if(float(temp_output_38_0_g1044_1) == 0.0)
        {

#line 657
            temp_output_1550_0_1 = tex2DNode76_g1044_1.w;

#line 657
        }
        else
        {

#line 657
            temp_output_1550_0_1 = tex2DNode76_g1044_1.x;

#line 657
        }

#line 657
    }

#line 657
    float temp_output_74_0_g1049_1 = saturate_0((_S198 + temp_output_1550_0_1) / 2.0);

#line 657
    float smoothstepResult76_g1049_1 = RU_CompatSmoothstep(temp_output_77_0_g1049_1 - temp_output_65_0_g1049_1, temp_output_77_0_g1049_1, temp_output_74_0_g1049_1);

#line 657
    float temp_output_66_0_g1049_1 = RU_Material_0._DIssloveWide;

#line 657
    float temp_output_64_0_g1049_1 = temp_output_70_0_g1049_1 * (1.0 + RU_Material_0._DIssloveWide);

#line 657
    float DisAplha21398_1;

#line 657
    if((RU_Material_0._sot_sting_A) == 0.0)
    {

#line 657
        DisAplha21398_1 = smoothstepResult76_g1049_1;

#line 657
    }
    else
    {

#line 657
        DisAplha21398_1 = step(temp_output_64_0_g1049_1 - temp_output_66_0_g1049_1, temp_output_74_0_g1049_1);

#line 657
    }

#line 657
    vec3 normalizeResult1265_1 = normalize(cross(appendResult1273_1, vec3(0.0, 1.0, (DisAplha21398_1 - DisAplha42_1) * temp_output_1402_0_1)));

#line 657
    vec3 _S199 = UnpackScaleNormal_0(RU_tex2D_NormalTex_0(appendResult60_g1059_1), RU_Material_0._NormalScale);

#line 657
    if((RU_Material_0._IfStaticNormal) == 0.0)
    {

#line 657
        DistortUV60_1 = _S131;

#line 657
    }
    else
    {

#line 657
        DistortUV60_1 = normalizeResult1265_1;

#line 657
    }

#line 657
    vec3 Normal1046_1 = BlendNormals_0(_S199, DistortUV60_1);

#line 657
    vec3 normalizeResult998_0 = normalize(vec3(dot(i_1.internalSurfaceTtoW0_0, Normal1046_1), dot(i_1.internalSurfaceTtoW1_0, Normal1046_1), dot(i_1.internalSurfaceTtoW2_0, Normal1046_1)));

#line 657
    float dotResult1006_0 = dot(normalizeResult1001_0, normalizeResult998_0);

#line 657
    vec4 ase_lightColor_0;

#line 657
    if((any(bvec3((RU_Material_0._RU_MapSunColor.xyz)))))
    {

#line 657
        ase_lightColor_0 = RU_Material_0._RU_MapSunColor;

#line 657
    }
    else
    {

#line 657
        ase_lightColor_0 = RU_Material_0._RU_MapMoonColor;

#line 657
    }

#line 657
    vec4 temp_output_1007_0_0 = ase_lightColor_0 * ase_lightAtten_0;

#line 657
    vec4 light1023_0 = pow(max(dotResult1006_0, 0.0), 12.80000019073486328) * temp_output_1007_0_0 * RU_Material_0._LightScale + (temp_output_1007_0_0 * max(dot(normalizeResult998_0, ase_worldlightDir_0), 0.0) + vec4(UnityGI_Base_0(data_2, 1.0, normalizeResult998_0).indirect_0.diffuse_0 + normalizeResult998_0 * 0.00009999999747379, 0.0)) * RU_Material_0._LightScale;

#line 657
    if(bool(RU_Material_0._IfCustomLight))
    {

#line 657
        color1051_0 = MainAlpha142_1 * light1023_0;

#line 657
    }

#line 657
    c_0.xyz = color1051_0.xyz;

#line 657
    c_0[3] = MainAlpha142_1;

#line 657
    return c_0;
}


#line 657
layout(location = 0)
out vec4 entryPointParam_RU_AseFrag_0;


#line 657
layout(location = 1) in vec4 v_uv4_texcoord4_0; // @vertex TexCoord0 optional


#line 657
layout(location = 5) in vec3 v_worldPos_0; // @vertex TexCoord1 optional


#line 657
layout(location = 6) in vec4 v_uv_texcoord_0; // @vertex TexCoord2 optional


#line 657
layout(location = 7) in vec3 v_viewDir_0; // @vertex TexCoord3 optional


#line 657
layout(location = 8) in vec3 v_internalSurfaceTtoW0_0; // @vertex TexCoord4 optional


#line 657
layout(location = 9) in vec3 v_internalSurfaceTtoW1_0; // @vertex TexCoord5 optional


#line 657
layout(location = 10) in vec3 v_internalSurfaceTtoW2_0; // @vertex TexCoord6 optional


#line 657
layout(location = 11) in vec3 v_worldNormal_0; // @vertex TexCoord7 optional


#line 657
layout(location = 14)
in vec4 v_uv2_texcoord2_0;


#line 657
layout(location = 15)
in vec4 v_uv3_texcoord3_0;


#line 657
layout(location = 16)
in vec4 v_vertexColor_0;


#line 657
layout(location = 17)
in vec4 v_screenPos_0;


#line 657
layout(location = 18)
in vec4 v_screenPosition88_0;


#line 657
layout(location = 19)
in vec3 v_worldRefl_0;


#line 657
layout(location = 20)
in vec3 v_ruLocalPosition_0;


#line 657
void ruUiOriginalMain()
{

#line 657
    const vec4 _S200 = vec4(0.0);

#line 657
    const vec3 _S201 = vec3(0.0);

#line 657
    Input_0 _S202 = Input_0(_S200, _S201, _S200, _S201, _S201, _S201, _S201, _S201, _S200, _S200, _S200, _S200, _S200, _S201, 0.0, _S201);

#line 657
    Input_0 i_2;

#line 657
    i_2.uv4_texcoord4_0 = _S200;

#line 657
    i_2.worldPos_0 = _S201;

#line 657
    i_2.uv_texcoord_0 = _S200;

#line 657
    i_2.viewDir_0 = _S201;

#line 657
    i_2.internalSurfaceTtoW0_0 = _S201;

#line 657
    i_2.internalSurfaceTtoW1_0 = _S201;

#line 657
    i_2.internalSurfaceTtoW2_0 = _S201;

#line 657
    i_2.worldNormal_0 = _S201;

#line 657
    i_2.uv2_texcoord2_0 = _S200;

#line 657
    i_2.uv3_texcoord3_0 = _S200;

#line 657
    i_2.vertexColor_0 = _S200;

#line 657
    i_2.screenPos_0 = _S200;

#line 657
    i_2.screenPosition88_0 = _S200;

#line 657
    i_2.worldRefl_0 = _S201;

#line 657
    i_2.ASEIsFrontFacing_0 = 0.0;

#line 657
    i_2.ruLocalPosition_0 = _S201;

#line 657
    i_2.uv4_texcoord4_0 = v_uv4_texcoord4_0;

#line 657
    i_2.worldPos_0 = v_worldPos_0;

#line 657
    i_2.uv_texcoord_0 = v_uv_texcoord_0;

#line 657
    i_2.viewDir_0 = v_viewDir_0;

#line 657
    i_2.internalSurfaceTtoW0_0 = v_internalSurfaceTtoW0_0;

#line 657
    i_2.internalSurfaceTtoW1_0 = v_internalSurfaceTtoW1_0;

#line 657
    i_2.internalSurfaceTtoW2_0 = v_internalSurfaceTtoW2_0;

#line 657
    i_2.worldNormal_0 = v_worldNormal_0;

#line 657
    i_2.uv2_texcoord2_0 = v_uv2_texcoord2_0;

#line 657
    i_2.uv3_texcoord3_0 = v_uv3_texcoord3_0;

#line 657
    i_2.vertexColor_0 = v_vertexColor_0;

#line 657
    i_2.screenPos_0 = v_screenPos_0;

#line 657
    i_2.screenPosition88_0 = v_screenPosition88_0;

#line 657
    i_2.worldRefl_0 = v_worldRefl_0;

#line 657
    float _S203;

#line 657
    if(gl_FrontFacing)
    {

#line 657
        _S203 = 1.0;

#line 657
    }
    else
    {

#line 657
        _S203 = -1.0;

#line 657
    }

#line 657
    i_2.ASEIsFrontFacing_0 = _S203;

#line 657
    i_2.ruLocalPosition_0 = v_ruLocalPosition_0;

#line 657
    UnityGIInput_0 _S204 = UnityGIInput_0(_S201, 0.0, _S200);

#line 657
    SurfaceOutputCustomLightingCustom_0 s_2;

#line 657
    s_2.Albedo_0 = _S201;

#line 657
    s_2.Normal_0 = _S201;

#line 657
    s_2.Emission_0 = _S201;

#line 657
    s_2.Metallic_0 = 0.0;

#line 657
    s_2.Smoothness_0 = 0.0;

#line 657
    s_2.Occlusion_0 = 0.0;

#line 657
    s_2.Alpha_0 = 0.0;

#line 657
    s_2.SurfInput_0 = _S202;

#line 657
    s_2.GIData_0 = _S204;

#line 657
    s_2.Normal_0 = vec3(0.0, 0.0, 1.0);

#line 657
    s_2.Alpha_0 = 1.0;

#line 657
    surf_0(i_2, s_2);

#line 657
    UnityGIInput_0 data_3;

#line 657
    data_3.worldPos_1 = _S201;

#line 657
    data_3.atten_0 = 0.0;

#line 657
    data_3.lightmapUV_0 = _S200;

#line 657
    data_3.worldPos_1 = i_2.worldPos_0;

#line 657
    data_3.atten_0 = 1.0;

#line 657
    UnityLight_0 _S205 = UnityLight_0(_S201, _S201, 0.0);

#line 657
    UnityIndirect_0 _S206 = UnityIndirect_0(_S201, _S201);

#line 657
    UnityGI_0 gi_3;

#line 657
    gi_3.light_0 = _S205;

#line 657
    gi_3.indirect_0 = _S206;

#line 657
    vec4 _S207;

#line 657
    if((any(bvec3((RU_Material_0._RU_MapSunColor.xyz)))))
    {

#line 657
        _S207 = RU_Material_0._RU_MapSunColor;

#line 657
    }
    else
    {

#line 657
        _S207 = RU_Material_0._RU_MapMoonColor;

#line 657
    }

#line 657
    gi_3.light_0.color_0 = _S207.xyz;

#line 657
    LightingStandardCustomLighting_GI_0(s_2, data_3, gi_3);

#line 657
    vec4 result_0 = LightingStandardCustomLighting_0(s_2, normalize((RU_Material_0._WorldSpaceCameraPos.xyz) - i_2.worldPos_0), gi_3);

#line 657
    entryPointParam_RU_AseFrag_0 = vec4(result_0.xyz + s_2.Emission_0, result_0.w);

#line 657
    return;
}


// @rimunity-host-matrices 1

layout(std430, set = 2, binding = 32) readonly buffer _UiMasks { vec4 rows[]; } masks;
layout(std430, set = 2, binding = 33) readonly buffer _UiMaskPixels { uint values[]; } maskPixels;
float maskFetch(ivec2 p, int width) { uint n = uint(p.y * width + p.x); return float((maskPixels.values[n / 4u] >> ((n % 4u) * 8u)) & 255u) / 255.0; }
layout(std430, set = 2, binding = 34) readonly buffer _UiMaskRegions { vec4 rows[]; } regions;
//图集内按原图的环绕和过滤规则取样，避免相邻区域串色。
ivec2 maskTexel(ivec2 p, ivec2 size, int wrap)
{
    if (wrap == 0) return (p % size + size) % size;
    if (wrap == 2) { p = (p % (size * 2) + size * 2) % (size * 2); return min(p, size * 2 - 1 - p); }
    if (wrap == 3) p = max(p, -p - 1);
    return clamp(p, ivec2(0), size - 1);
}
float maskAlpha(int i, vec2 uv)
{
    ivec4 r = ivec4(regions.rows[i * 2]);
    vec4 options = regions.rows[i * 2 + 1];
    int wrap = int(options.y);
    vec2 p = uv * vec2(r.zw) - 0.5;
    if (options.x < 0.5) return maskFetch(r.xy + maskTexel(ivec2(floor(p + 0.5)), r.zw, wrap), int(options.z));
    ivec2 b = ivec2(floor(p)); vec2 f = fract(p);
    float a = maskFetch(r.xy + maskTexel(b, r.zw, wrap), int(options.z));
    float c = maskFetch(r.xy + maskTexel(b + ivec2(1, 0), r.zw, wrap), int(options.z));
    float d = maskFetch(r.xy + maskTexel(b + ivec2(0, 1), r.zw, wrap), int(options.z));
    float e = maskFetch(r.xy + maskTexel(b + ivec2(1, 1), r.zw, wrap), int(options.z));
    return mix(mix(a, c, f.x), mix(d, e, f.x), f.y);
}
//GUI 没有 Stencil 附件，按每层 Mask 的字形透明覆盖率裁切。
float slicedCoordinate(float p, float first, float last, float uvFirst, float uvLast)
{
    if (p < first) return p / max(first, 0.000001) * uvFirst;
    if (p > 1.0 - last) return 1.0 - (1.0 - p) / max(last, 0.000001) * uvLast;
    return uvFirst + (p - first) / max(1.0 - first - last, 0.000001) * (1.0 - uvFirst - uvLast);
}
void applyUiMasks()
{
    for (int i = 0; i < int(RU_Material_0._UiMaskCount); ++i)
    {
        vec3 p = vec3(gl_FragCoord.xy, 1.0);
        vec2 q = vec2(dot(masks.rows[i * 7].xyz, p), dot(masks.rows[i * 7 + 1].xyz, p)) / dot(masks.rows[i * 7 + 2].xyz, p);
        if (q.x < 0.0 || q.y < 0.0 || q.x > 1.0 || q.y > 1.0) discard;
        vec4 d = masks.rows[i * 7 + 4], b = masks.rows[i * 7 + 5];
        vec4 options = masks.rows[i * 7 + 6];
        if (options.x > 0.5)
        {
            if (options.y < 0.5 && q.x > d.x && q.x < 1.0 - d.z && q.y > d.y && q.y < 1.0 - d.w) discard;
            q.x = slicedCoordinate(q.x, d.x, d.z, b.x, b.z);
            q.y = slicedCoordinate(q.y, d.y, d.w, b.y, b.w);
        }
        vec4 r = masks.rows[i * 7 + 3];
        vec2 t = q * r.zw + r.xy; t.y = 1.0 - t.y;
        float a = maskAlpha(i, t);
        if (a < 0.001) discard;
    }
}
//矩形裁切和羽化使用与 Mask 相同的屏幕到局部投影约定。
layout(std430, set = 2, binding = 35) readonly buffer _UiRectMasks { vec4 rows[]; } rectangles;
float rectCoverage()
{
    float coverage = 1.0;
    for (int i = 0; i < int(RU_Material_0._UiRectMaskCount); ++i)
    {
        vec3 p = vec3(gl_FragCoord.xy, 1.0);
        vec2 q = vec2(dot(rectangles.rows[i * 4].xyz, p), dot(rectangles.rows[i * 4 + 1].xyz, p)) / dot(rectangles.rows[i * 4 + 2].xyz, p);
        if (q.x < 0.0 || q.y < 0.0 || q.x > 1.0 || q.y > 1.0) discard;
        vec2 edge = min(q, 1.0 - q);
        vec2 softness = rectangles.rows[i * 4 + 3].xy;
        vec2 alpha = clamp(edge / max(softness, vec2(0.000001)), 0.0, 1.0);
        coverage *= alpha.x * alpha.y;
    }
    return coverage;
}

void main() { applyUiMasks(); ruUiOriginalMain(); entryPointParam_RU_AseFrag_0.a *= rectCoverage(); }
