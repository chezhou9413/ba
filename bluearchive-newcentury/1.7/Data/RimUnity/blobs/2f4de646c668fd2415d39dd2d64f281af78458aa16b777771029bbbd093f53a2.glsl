#version 450
layout(set = 2, binding = 11) uniform sampler2D _UiMaskAtlas;
layout(binding = 9, set = 2)
uniform sampler2D _RU_ShadowMap;
layout(binding = 5, set = 2)
uniform sampler2D _MainTex; // @default = white
layout(std430, set = 2, binding = 13) readonly buffer _UiRectMasks { vec4 rows[]; } rectangles;
layout(std430, set = 2, binding = 12) readonly buffer _UiMaskRegions { vec4 rows[]; } regions;
layout(std430, set = 2, binding = 10) readonly buffer _UiMasks { vec4 rows[]; } masks;


#line 58 0
struct RU_MaterialData_0
{
    float _BumpScale; // @default = 1
    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    float _Cutoff; // @default = 0.5
    vec4 _EmissionColor; // @default = vec4(0, 0, 0, 0) @color
    float _FlipbookMode; // @default = 0
    float _Glossiness; // @default = 0.5
    vec4 _MainTex_ST; // @default = vec4(0, 0, 1, 1)
    float _Metallic; // @default = 0
    float _RU_DisableShadows;
    vec4 _RU_MapAmbient;
    vec4 _RU_MapMoonColor;
    vec4 _RU_MapMoonDirection;
    vec4 _RU_MapSunColor;
    vec4 _RU_MapSunDirection;
    vec4 _RU_ShadowParams;
    vec4 _RU_ShadowTexel;
    mat4 _RU_ShadowVP;
    float _RimUnityParticle; // @default = 0
    vec3 _WorldSpaceCameraPos;
};


#line 79
layout(binding = 0, set = 2, std140) uniform block_RU_MaterialData_0
{    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    float _FlipbookMode; // @default = 0
    float _Glossiness; // @default = 0.5
    vec4 _MainTex_ST; // @default = vec4(0, 0, 1, 1)
    float _Metallic; // @default = 0
    float _RU_DisableShadows;
    vec4 _RU_MapAmbient;
    vec4 _RU_MapMoonColor;
    vec4 _RU_MapMoonDirection;
    vec4 _RU_MapSunColor;
    vec4 _RU_MapSunDirection;
    vec4 _RU_ShadowParams;
    vec4 _RU_ShadowTexel;
    mat4 _RU_ShadowVP;
    float _RimUnityParticle; // @default = 0
    vec4 _WorldSpaceCameraPos;

float _UiMaskCount; // @default = 0.0
float _UiRectMaskCount; // @default = 0.0
}RU_Material_0;

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


#line 55 0
struct appdata_full_0
{
    vec4 vertex_0;
    vec4 tangent_0;
    vec3 normal_0;
    vec4 texcoord_0;
    vec4 texcoord1_0;
    vec4 texcoord2_0;
    vec4 texcoord3_0;
    vec4 color_0;
};


#line 169
struct Input_0
{
    vec4 uv_MainTex_0;
    vec4 color_1;
    float frameBlend_0;
};


#line 169
void ParticleVertex_0(inout appdata_full_0 v_0, out Input_0 o_0)
{

#line 169
    const vec4 _S1 = vec4(0.0);

#line 169
    o_0.uv_MainTex_0 = _S1;

#line 169
    o_0.color_1 = _S1;

#line 169
    o_0.frameBlend_0 = 0.0;

#line 169
    o_0.frameBlend_0 = saturate_0(v_0.texcoord1_0.x) * RU_Material_0._RimUnityParticle * RU_Material_0._FlipbookMode;

#line 169
    return;
}


#line 23
vec4 UnityObjectToClipPos_0(vec4 p_0)
{

#line 23
    return ((((((p_0) * (transpose(RU_Object_0.model_0))))) * (transpose(RU_Camera_0.ViewProj_0))));
}



vec3 UnityObjectToWorldNormal_0(vec3 normal_1)
{

#line 29
    mat3 _S2 = mat3(transpose(RU_Object_0.model_0)[0].xyz, transpose(RU_Object_0.model_0)[1].xyz, transpose(RU_Object_0.model_0)[2].xyz);
    mat3 cofactors_0 = mat3(cross(_S2[1], _S2[2]), cross(_S2[2], _S2[0]), cross(_S2[0], _S2[1]));
    return normalize((((normal_1) * (cofactors_0))) * float((int(sign((dot(_S2[0], cofactors_0[0])))))));
}


#line 33
vec3 UnityObjectToWorldDir_0(vec3 direction_0)
{

#line 33
    return normalize((((direction_0) * (mat3(transpose(RU_Object_0.model_0)[0].xyz, transpose(RU_Object_0.model_0)[1].xyz, transpose(RU_Object_0.model_0)[2].xyz)))));
}


#line 33
layout(location = 1)
out vec4 entryPointParam_RU_SurfaceVert_uv_MainTex_0;


#line 33
layout(location = 5)
out vec4 entryPointParam_RU_SurfaceVert_color_0;


#line 33
layout(location = 6)
out float entryPointParam_RU_SurfaceVert_frameBlend_0;


#line 33
layout(location = 7)
out vec3 entryPointParam_RU_SurfaceVert_ruNormal_0;


#line 33
layout(location = 8)
out vec4 entryPointParam_RU_SurfaceVert_ruTangent_0;


#line 33
layout(location = 9)
out vec3 entryPointParam_RU_SurfaceVert_ruWorld_0;


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


#line 169
struct RU_SurfaceVaryings_0
{
    vec4 position_0;
    vec4 uv_MainTex_1;
    vec4 color_2;
    float frameBlend_1;
    vec3 ruNormal_0;
    vec4 ruTangent_0;
    vec3 ruWorld_0;
};


#line 169
void main()
{

#line 169
    appdata_full_0 _S3;

#line 169
    _S3.vertex_0 = v_vertex_0;

#line 169
    _S3.tangent_0 = v_tangent_0;

#line 169
    _S3.normal_0 = v_normal_0;

#line 169
    _S3.texcoord_0 = v_texcoord_0;

#line 169
    _S3.texcoord1_0 = v_texcoord1_0;

#line 169
    _S3.texcoord2_0 = v_texcoord2_0;

#line 169
    _S3.texcoord3_0 = v_texcoord3_0;

#line 169
    _S3.color_0 = v_color_0;

#line 169
    const vec4 _S4 = vec4(0.0);

#line 169
    Input_0 custom_0;

#line 169
    custom_0.uv_MainTex_0 = _S4;

#line 169
    custom_0.color_1 = _S4;

#line 169
    custom_0.frameBlend_0 = 0.0;

#line 169
    ParticleVertex_0(_S3, custom_0);

#line 169
    const vec3 _S5 = vec3(0.0);

#line 169
    RU_SurfaceVaryings_0 o_1;

#line 169
    o_1.position_0 = _S4;

#line 169
    o_1.uv_MainTex_1 = _S4;

#line 169
    o_1.color_2 = _S4;

#line 169
    o_1.frameBlend_1 = 0.0;

#line 169
    o_1.ruNormal_0 = _S5;

#line 169
    o_1.ruTangent_0 = _S4;

#line 169
    o_1.ruWorld_0 = _S5;

#line 169
    o_1.position_0 = UnityObjectToClipPos_0(_S3.vertex_0);

#line 169
    o_1.uv_MainTex_1 = vec4(_S3.texcoord_0.xy * RU_Material_0._MainTex_ST.zwxy.xy + RU_Material_0._MainTex_ST.zwxy.zw, _S3.texcoord_0.zw);

#line 169
    o_1.color_2 = _S3.color_0;

#line 169
    o_1.frameBlend_1 = custom_0.frameBlend_0;

#line 169
    o_1.ruNormal_0 = UnityObjectToWorldNormal_0(_S3.normal_0);

#line 169
    o_1.ruTangent_0 = vec4(UnityObjectToWorldDir_0(_S3.tangent_0.xyz), _S3.tangent_0.w * float((int(sign((determinant(mat3(transpose(RU_Object_0.model_0)[0].xyz, transpose(RU_Object_0.model_0)[1].xyz, transpose(RU_Object_0.model_0)[2].xyz))))))));

#line 169
    o_1.ruWorld_0 = (((_S3.vertex_0) * (transpose(RU_Object_0.model_0)))).xyz;

#line 169
    RU_SurfaceVaryings_0 _S6 = o_1;

#line 169
    gl_Position = o_1.position_0;

#line 169
    entryPointParam_RU_SurfaceVert_uv_MainTex_0 = _S6.uv_MainTex_1;

#line 169
    entryPointParam_RU_SurfaceVert_color_0 = _S6.color_2;

#line 169
    entryPointParam_RU_SurfaceVert_frameBlend_0 = _S6.frameBlend_1;

#line 169
    entryPointParam_RU_SurfaceVert_ruNormal_0 = _S6.ruNormal_0;

#line 169
    entryPointParam_RU_SurfaceVert_ruTangent_0 = _S6.ruTangent_0;

#line 169
    entryPointParam_RU_SurfaceVert_ruWorld_0 = _S6.ruWorld_0;

#line 169
    return;
}


// @rimunity-host-matrices 1
