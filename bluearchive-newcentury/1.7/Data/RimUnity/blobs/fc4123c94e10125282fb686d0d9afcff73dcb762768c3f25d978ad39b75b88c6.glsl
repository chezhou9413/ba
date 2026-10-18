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
    vec4 _ClipRect;
    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    vec4 _TextureSampleAdd;
};


#line 62
layout(binding = 0, set = 2, std140) uniform block_RU_MaterialData_0
{
    vec4 _ClipRect;
    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    vec4 _TextureSampleAdd;

float _UiMaskCount; // @default = 0.0
float _UiRectMaskCount; // @default = 0.0
}RU_Material_0;

#line 23
vec4 UnityObjectToClipPos_0(vec4 p_0)
{

#line 23
    return ((((((p_0) * (transpose(RU_Object_0.model_0))))) * (transpose(RU_Camera_0.ViewProj_0))));
}


#line 79
vec3 TargetToGammaSpace_0(vec3 targetColor_0)
{

#line 79
    return targetColor_0;
}


#line 79
vec4 PMAGammaToTargetSpaceSaturated_0(vec4 gammaPMAColor_0)
{

#line 79
    return gammaPMAColor_0;
}


#line 79
layout(location = 2)
out vec4 entryPointParam_vert_color_0;


#line 79
layout(location = 1)
out vec2 entryPointParam_vert_texcoord_0;


#line 79
layout(location = 5)
out vec4 entryPointParam_vert_worldPosition_0;


#line 79
layout(location = 0) in vec4 IN_vertex_0; // @vertex Position required


#line 79
layout(location = 2) in vec4 IN_color_0; // @vertex Color optional


#line 79
layout(location = 1) in vec2 IN_texcoord_0; // @vertex TexCoord0 optional


#line 79
struct VertexOutput_0
{
    vec4 vertex_0;
    vec4 color_0;
    vec2 texcoord_0;
    vec4 worldPosition_0;
};


#line 79
void main()
{

#line 79
    VertexOutput_0 OUT_0;

#line 79
    OUT_0.worldPosition_0 = IN_vertex_0;

#line 79
    OUT_0.vertex_0 = UnityObjectToClipPos_0(IN_vertex_0);

#line 79
    OUT_0.texcoord_0 = IN_texcoord_0;

#line 79
    OUT_0.color_0 = PMAGammaToTargetSpaceSaturated_0(vec4(TargetToGammaSpace_0(IN_color_0.xyz), IN_color_0.w)) * vec4(RU_Material_0._Color.xyz * RU_Material_0._Color.w, RU_Material_0._Color.w);

#line 79
    VertexOutput_0 _S1 = OUT_0;

#line 79
    gl_Position = OUT_0.vertex_0;

#line 79
    entryPointParam_vert_color_0 = _S1.color_0;

#line 79
    entryPointParam_vert_texcoord_0 = _S1.texcoord_0;

#line 79
    entryPointParam_vert_worldPosition_0 = _S1.worldPosition_0;

#line 79
    return;
}


// @rimunity-host-matrices 1
