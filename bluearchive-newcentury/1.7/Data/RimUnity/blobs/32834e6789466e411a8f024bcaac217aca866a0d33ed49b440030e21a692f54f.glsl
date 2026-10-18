#version 450
layout(set = 2, binding = 3) uniform sampler2D _UiMaskAtlas;
layout(binding = 1, set = 2)
uniform sampler2D _MainTex; // @default = white
layout(std430, set = 2, binding = 5) readonly buffer _UiRectMasks { vec4 rows[]; } rectangles;
layout(std430, set = 2, binding = 4) readonly buffer _UiMaskRegions { vec4 rows[]; } regions;
layout(std430, set = 2, binding = 2) readonly buffer _UiMasks { vec4 rows[]; } masks;


#line 15 0
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

#line 57
struct RU_MaterialData_0
{
    float _Angle; // @default = 45
    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    float _LineAlpha; // @default = 0.5
    vec4 _LineColor; // @default = vec4(1, 1, 1, 1) @color
    float _LineSpacing; // @default = 0.2
    float _LineWidth; // @default = 0.1
    float _ScrollSpeed; // @default = 1
    vec4 _Time;
};


#line 67
layout(binding = 0, set = 2, std140) uniform block_RU_MaterialData_0
{
    float _Angle; // @default = 45
    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    float _LineAlpha; // @default = 0.5
    vec4 _LineColor; // @default = vec4(1, 1, 1, 1) @color
    float _LineSpacing; // @default = 0.2
    float _LineWidth; // @default = 0.1
    float _ScrollSpeed; // @default = 1
    vec4 _Time;

float _UiMaskCount; // @default = 0.0
float _UiRectMaskCount; // @default = 0.0
}RU_Material_0;

#line 23
vec4 UnityObjectToClipPos_0(vec4 p_0)
{

#line 23
    return ((((((p_0) * (transpose(RU_Object_0.model_0))))) * (transpose(RU_Camera_0.ViewProj_0))));
}


#line 23
layout(location = 2)
out vec4 entryPointParam_vert_color_0;


#line 89
layout(location = 1)
out vec2 entryPointParam_vert_texcoord_0;


#line 89
layout(location = 0) in vec4 v_vertex_0; // @vertex Position required


#line 89
layout(location = 2) in vec4 v_color_0; // @vertex Color optional


#line 89
layout(location = 1) in vec2 v_texcoord_0; // @vertex TexCoord0 optional


#line 89
struct v2f_0
{
    vec4 vertex_0;
    vec4 color_0;
    vec2 texcoord_0;
};


#line 89
void main()
{

#line 89
    v2f_0 OUT_0;

#line 89
    OUT_0.vertex_0 = UnityObjectToClipPos_0(v_vertex_0);

#line 89
    OUT_0.texcoord_0 = v_texcoord_0;

#line 89
    OUT_0.color_0 = v_color_0 * RU_Material_0._Color;

#line 89
    v2f_0 _S1 = OUT_0;

#line 89
    gl_Position = OUT_0.vertex_0;

#line 89
    entryPointParam_vert_color_0 = _S1.color_0;

#line 89
    entryPointParam_vert_texcoord_0 = _S1.texcoord_0;

#line 89
    return;
}


// @rimunity-host-matrices 1
