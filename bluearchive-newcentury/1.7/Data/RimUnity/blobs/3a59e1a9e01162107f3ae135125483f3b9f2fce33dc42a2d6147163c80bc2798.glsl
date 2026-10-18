#version 450
layout(set = 2, binding = 3) uniform sampler2D _UiMaskAtlas;
layout(binding = 1, set = 2)
uniform sampler2D _MainTex; // @default = white
layout(std430, set = 2, binding = 5) readonly buffer _UiRectMasks { vec4 rows[]; } rectangles;
layout(std430, set = 2, binding = 4) readonly buffer _UiMaskRegions { vec4 rows[]; } regions;
layout(std430, set = 2, binding = 2) readonly buffer _UiMasks { vec4 rows[]; } masks;


#line 15 0
struct RU_FrameData_0
{
    vec4 sunDirection_0;
    mat4 worldToCamera_0;
    mat4 viewProj_0;
};


#line 16
layout(binding = 1, std140) uniform block_RU_FrameData_0
{
    vec4 sunDirection_0;
    mat4 worldToCamera_0;
    mat4 viewProj_0;
}RU_Frame_0;

#line 43
struct RU_MaterialData_0
{
    vec4 _ClipRect;
    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    vec4 _CompletionColor; // @default = vec4(1.3, 1.5, 1.8, 1) @color
    float _CompletionDuration; // @default = 1
    float _CompletionIntensity; // @default = 1.4
    float _CompletionStartTime; // @default = -100
    float _CutoffSoftness; // @default = 0.006
    vec4 _FillColor; // @default = vec4(1, 1, 1, 1) @color
    float _FillColorStrength; // @default = 0
    vec4 _GlowColor; // @default = vec4(0.9, 1.4, 1.8, 1) @color
    float _GlowContainment; // @default = 0.25
    float _GlowEnabled; // @default = 1
    float _GlowIntensity; // @default = 4.5
    float _GlowWidth; // @default = 0.12
    float _ParticleIntensity; // @default = 1.25
    float _ParticleSize; // @default = 0.018
    float _ParticleSpeed; // @default = 0.7
    float _Progress; // @default = 1
    vec4 _ProgressAxis; // @default = vec4(1, 0, 0, 0)
    float _PulseAmount; // @default = 0.18
    float _PulseSpeed; // @default = 3
    float _Reverse; // @default = 0
    float _SpriteAspect; // @default = 2.75
    vec4 _TextureSampleAdd;
    vec4 _Time;
    vec4 _UvRect; // @default = vec4(0, 0, 1, 1)
};


#line 71
layout(binding = 0, set = 2, std140) uniform block_RU_MaterialData_0
{    vec4 _Color; // @default = vec4(1, 1, 1, 1) @color
    vec4 _CompletionColor; // @default = vec4(1.3, 1.5, 1.8, 1) @color
    float _CompletionDuration; // @default = 1
    float _CompletionIntensity; // @default = 1.4
    float _CompletionStartTime; // @default = -100
    float _CutoffSoftness; // @default = 0.006
    vec4 _FillColor; // @default = vec4(1, 1, 1, 1) @color
    float _FillColorStrength; // @default = 0
    vec4 _GlowColor; // @default = vec4(0.9, 1.4, 1.8, 1) @color
    float _GlowContainment; // @default = 0.25
    float _GlowEnabled; // @default = 1
    float _GlowIntensity; // @default = 4.5
    float _GlowWidth; // @default = 0.12
    float _ParticleIntensity; // @default = 1.25
    float _ParticleSize; // @default = 0.018
    float _ParticleSpeed; // @default = 0.7
    float _Progress; // @default = 1
    vec4 _ProgressAxis; // @default = vec4(1, 0, 0, 0)
    float _PulseAmount; // @default = 0.18
    float _PulseSpeed; // @default = 3
    float _Reverse; // @default = 0
    float _SpriteAspect; // @default = 2.75
    vec4 _TextureSampleAdd;
    vec4 _Time;
    vec4 _UvRect; // @default = vec4(0, 0, 1, 1)

float _UiMaskCount; // @default = 0.0
float _UiRectMaskCount; // @default = 0.0
}RU_Material_0;

#line 19
vec4 UnityObjectToClipPos_0(vec4 p_0)
{

#line 19
    return (((p_0) * (transpose(RU_Frame_0.viewProj_0))));
}


#line 19
layout(location = 2)
out vec4 entryPointParam_vert_color_0;


#line 3346 1
layout(location = 1)
out vec2 entryPointParam_vert_texcoord_0;


#line 3346
layout(location = 5)
out vec4 entryPointParam_vert_worldPosition_0;


#line 3346
layout(location = 0) in vec4 input_vertex_0; // @vertex Position required


#line 3346
layout(location = 2) in vec4 input_color_0; // @vertex Color optional


#line 3346
layout(location = 1) in vec2 input_texcoord_0; // @vertex TexCoord0 optional


#line 111 0
struct v2f_0
{
    vec4 vertex_0;
    vec4 color_0;
    vec2 texcoord_0;
    vec4 worldPosition_0;
};


#line 111
void main()
{

#line 111
    v2f_0 output_0;

#line 111
    output_0.worldPosition_0 = input_vertex_0;

#line 111
    output_0.vertex_0 = UnityObjectToClipPos_0(input_vertex_0);

#line 111
    output_0.texcoord_0 = input_texcoord_0;

#line 111
    output_0.color_0 = input_color_0 * RU_Material_0._Color;

#line 111
    v2f_0 _S1 = output_0;

#line 111
    gl_Position = output_0.vertex_0;

#line 111
    entryPointParam_vert_color_0 = _S1.color_0;

#line 111
    entryPointParam_vert_texcoord_0 = _S1.texcoord_0;

#line 111
    entryPointParam_vert_worldPosition_0 = _S1.worldPosition_0;

#line 111
    return;
}


// @rimunity-host-matrices 1
