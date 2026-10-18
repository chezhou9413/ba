#version 450


#line 59 0
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


#line 80
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

#line 123
layout(binding = 5, set = 2)
uniform sampler2D _MainTex; // @default = white


#line 125



#line 145
layout(binding = 9, set = 2)
uniform sampler2D _RU_ShadowMap;


#line 147



#line 14946 1
float saturate_0(float x_0)
{

#line 14954
    return clamp(x_0, 0.0, 1.0);
}


#line 160 0
vec3 RU_SafeNormalize_0(vec3 p_0)
{

#line 160
    return p_0 * (inversesqrt((max(dot(p_0, p_0), 0.00100000004749745))));
}


#line 127
vec4 RU_tex2D_MainTex_0(vec2 uv_0)
{

#line 127
    return (texture(_MainTex, (vec2(uv_0.x, 1.0 - uv_0.y))));
}


#line 170
struct Input_0
{
    vec4 uv_MainTex_0;
    vec4 color_0;
    float frameBlend_0;
};


#line 161
struct SurfaceOutputStandard_0
{
    vec3 Albedo_0;
    vec3 Normal_0;
    vec3 Emission_0;
    float Metallic_0;
    float Smoothness_0;
    float Occlusion_0;
    float Alpha_0;
};


#line 170
void surf_0(Input_0 IN_0, inout SurfaceOutputStandard_0 o_0)
{

#line 170
    vec4 _S1;

#line 170
    if((IN_0.frameBlend_0) == 0.0)
    {

#line 170
        _S1 = RU_tex2D_MainTex_0(IN_0.uv_MainTex_0.xy);

#line 170
    }
    else
    {

#line 170
        _S1 = mix(RU_tex2D_MainTex_0(IN_0.uv_MainTex_0.xy), RU_tex2D_MainTex_0(IN_0.uv_MainTex_0.zw * RU_Material_0._MainTex_ST.zwxy.xy + RU_Material_0._MainTex_ST.zwxy.zw), vec4(IN_0.frameBlend_0));

#line 170
    }

#line 170
    vec4 albedo_0 = _S1 * RU_Material_0._Color * mix(vec4(1.0, 1.0, 1.0, 1.0), IN_0.color_0, vec4(RU_Material_0._RimUnityParticle));

#line 170
    o_0.Albedo_0 = albedo_0.xyz;

#line 170
    o_0.Metallic_0 = RU_Material_0._Metallic;

#line 170
    o_0.Smoothness_0 = RU_Material_0._Glossiness;

#line 170
    o_0.Alpha_0 = albedo_0.w;

#line 170
    return;
}


#line 149
vec4 RU_tex2D_RU_ShadowMap_0(vec2 uv_1)
{

#line 149
    return (texture(_RU_ShadowMap, (vec2(uv_1.x, 1.0 - uv_1.y))));
}


#line 161
float RU_SurfaceShadow_0(vec3 world_0, vec3 normal_0)
{

#line 161
    bool _S2;

#line 161
    if((RU_Material_0._RU_ShadowParams.x) == 0.0)
    {

#line 161
        _S2 = true;

#line 161
    }
    else
    {

#line 161
        _S2 = (RU_Material_0._RU_DisableShadows) != 0.0;

#line 161
    }

#line 161
    if(_S2)
    {

#line 161
        return 1.0;
    }

#line 161
    vec4 clip_0 = (((vec4(world_0 + normal_0 * RU_Material_0._RU_ShadowTexel.z, 1.0)) * (transpose(RU_Material_0._RU_ShadowVP))));

#line 161
    vec3 p_1 = clip_0.xyz / clip_0.w;

#line 161
    if((any(bvec2((greaterThan(abs(p_1.xy),vec2(1.0)))))))
    {

#line 161
        _S2 = true;

#line 161
    }
    else
    {

#line 161
        _S2 = (p_1.z) < 0.0;

#line 161
    }

#line 161
    if(_S2)
    {

#line 161
        _S2 = true;

#line 161
    }
    else
    {

#line 161
        _S2 = (p_1.z) > 1.0;

#line 161
    }

#line 161
    if(_S2)
    {

#line 161
        return 1.0;
    }

#line 161
    vec2 _S3 = vec2(p_1.x * 0.5 + 0.5, 0.5 - p_1.y * 0.5);

#line 161
    int y_0 = -1;

#line 161
    float visible_0 = 0.0;

#line 161
    for(;;)
    {

#line 161
        if(y_0 <= 1)
        {
        }
        else
        {

#line 161
            break;
        }

#line 161
        int x_1 = -1;

#line 161
        for(;;)
        {

#line 161
            if(x_1 <= 1)
            {
            }
            else
            {

#line 161
                break;
            }

#line 161
            int _S4;

#line 161
            if((p_1.z + RU_Material_0._RU_ShadowParams.w) >= (RU_tex2D_RU_ShadowMap_0(_S3 + vec2(float(x_1), float(y_0)) * RU_Material_0._RU_ShadowTexel.xy * RU_Material_0._RU_ShadowParams.z).x))
            {

#line 161
                _S4 = 1;

#line 161
            }
            else
            {

#line 161
                _S4 = 0;

#line 161
            }

#line 161
            float visible_1 = visible_0 + float(_S4);

#line 161
            x_1 = x_1 + 1;

#line 161
            visible_0 = visible_1;

#line 161
        }

#line 161
        y_0 = y_0 + 1;

#line 161
    }

#line 161
    return mix(1.0, visible_0 / 9.0, RU_Material_0._RU_ShadowParams.y);
}


#line 160
vec3 RU_GraphRadiance_0(vec3 color_1)
{

#line 160
    return color_1;
}


#line 160
float RU_Pow5_0(float x_2)
{

#line 160
    float x2_0 = x_2 * x_2;

#line 160
    return x2_0 * x2_0 * x_2;
}


#line 160
vec3 RU_StandardDirect_0(vec3 diffuse_0, vec3 specular_0, float smoothness_0, vec3 n_0, vec3 v_0, vec3 l_0, vec3 radiance_0)
{

#line 160
    vec3 h_0 = RU_SafeNormalize_0(l_0 + v_0);

#line 160
    float nv_0 = abs(dot(n_0, v_0));

#line 160
    float nl_0 = saturate_0(dot(n_0, l_0));

#line 160
    float nh_0 = saturate_0(dot(n_0, h_0));

#line 160
    float lh_0 = saturate_0(dot(l_0, h_0));

#line 160
    float perceptualRoughness_0 = 1.0 - smoothness_0;

#line 160
    float _S5 = max(perceptualRoughness_0 * perceptualRoughness_0, 0.0020000000949949);

#line 160
    float _S6 = 0.5 + 2.0 * lh_0 * lh_0 * perceptualRoughness_0 - 1.0;

#line 160
    float diffuseTerm_0 = (1.0 + _S6 * RU_Pow5_0(1.0 - nl_0)) * (1.0 + _S6 * RU_Pow5_0(1.0 - nv_0)) * nl_0;

#line 160
    float _S7 = 1.0 - _S5;

#line 160
    float a2_0 = _S5 * _S5;

#line 160
    float d_0 = (nh_0 * a2_0 - nh_0) * nh_0 + 1.0;

#line 160
    float _S8 = max(0.0, sqrt(max(0.00009999999747379, 0.5 / (nl_0 * (nv_0 * _S7 + _S5) + nv_0 * (nl_0 * _S7 + _S5) + 0.00000999999974738) * a2_0 / (d_0 * d_0 + 1.00000001168609742e-07))) * nl_0);

#line 160
    int _S9;

#line 160
    if((any(bvec3((specular_0)))))
    {

#line 160
        _S9 = 1;

#line 160
    }
    else
    {

#line 160
        _S9 = 0;

#line 160
    }

#line 160
    return (diffuse_0 * diffuseTerm_0 + _S8 * float(_S9) * (specular_0 + (1.0 - specular_0) * RU_Pow5_0(1.0 - lh_0))) * radiance_0;
}


#line 160
layout(location = 0)
out vec4 entryPointParam_RU_SurfaceFrag_0;


#line 160
layout(location = 1) in vec4 i_uv_MainTex_0; // @vertex TexCoord0 optional


#line 160
layout(location = 5) in vec4 i_color_0; // @vertex TexCoord1 optional


#line 160
layout(location = 6) in float i_frameBlend_0; // @vertex TexCoord2 optional


#line 160
layout(location = 7) in vec3 i_ruNormal_0; // @vertex TexCoord3 optional


#line 160
layout(location = 8) in vec4 i_ruTangent_0; // @vertex TexCoord4 optional


#line 160
layout(location = 9) in vec3 i_ruWorld_0; // @vertex TexCoord5 optional


#line 170
void ruUiOriginalMain()
{

#line 170
    const vec4 _S10 = vec4(0.0);

#line 170
    Input_0 input_0;

#line 170
    input_0.uv_MainTex_0 = _S10;

#line 170
    input_0.color_0 = _S10;

#line 170
    input_0.frameBlend_0 = 0.0;

#line 170
    input_0.uv_MainTex_0 = i_uv_MainTex_0;

#line 170
    input_0.color_0 = i_color_0;

#line 170
    input_0.frameBlend_0 = i_frameBlend_0;

#line 170
    vec3 n_1 = normalize(i_ruNormal_0);

#line 170
    vec3 t_0 = RU_SafeNormalize_0(i_ruTangent_0.xyz);

#line 170
    vec3 b_0 = cross(n_1, t_0) * i_ruTangent_0.w;

#line 170
    vec3 view_0 = normalize((RU_Material_0._WorldSpaceCameraPos.xyz) - i_ruWorld_0);

#line 170
    const vec3 _S11 = vec3(0.0);

#line 170
    SurfaceOutputStandard_0 output_0;

#line 170
    output_0.Albedo_0 = _S11;

#line 170
    output_0.Normal_0 = _S11;

#line 170
    output_0.Emission_0 = _S11;

#line 170
    output_0.Metallic_0 = 0.0;

#line 170
    output_0.Smoothness_0 = 0.0;

#line 170
    output_0.Occlusion_0 = 0.0;

#line 170
    output_0.Alpha_0 = 0.0;

#line 170
    output_0.Normal_0 = vec3(0.0, 0.0, 1.0);

#line 170
    output_0.Alpha_0 = 1.0;

#line 170
    output_0.Occlusion_0 = 1.0;

#line 170
    surf_0(input_0, output_0);

#line 170
    vec3 n_2 = normalize(t_0 * output_0.Normal_0.x + b_0 * output_0.Normal_0.y + n_1 * output_0.Normal_0.z);

#line 170
    vec3 diffuse_1 = output_0.Albedo_0 * (0.7790837287902832 * (1.0 - output_0.Metallic_0));

#line 170
    vec3 specular_1 = mix(vec3(0.22091630101203918), output_0.Albedo_0, vec3(output_0.Metallic_0));

#line 170
    entryPointParam_RU_SurfaceFrag_0 = vec4(diffuse_1 * RU_GraphRadiance_0(RU_Material_0._RU_MapAmbient.xyz) * output_0.Occlusion_0 + RU_StandardDirect_0(diffuse_1, specular_1, output_0.Smoothness_0, n_2, view_0, RU_SafeNormalize_0(RU_Material_0._RU_MapSunDirection.xyz), RU_GraphRadiance_0(RU_Material_0._RU_MapSunColor.xyz) * RU_SurfaceShadow_0(i_ruWorld_0, n_2)) + RU_StandardDirect_0(diffuse_1, specular_1, output_0.Smoothness_0, n_2, view_0, RU_SafeNormalize_0(RU_Material_0._RU_MapMoonDirection.xyz), RU_GraphRadiance_0(RU_Material_0._RU_MapMoonColor.xyz)) + output_0.Emission_0, output_0.Alpha_0);

#line 170
    return;
}


// @rimunity-host-matrices 1

layout(std430, set = 2, binding = 10) readonly buffer _UiMasks { vec4 rows[]; } masks;
layout(set = 2, binding = 11) uniform sampler2D _UiMaskAtlas;
layout(std430, set = 2, binding = 12) readonly buffer _UiMaskRegions { vec4 rows[]; } regions;
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
    if (options.x < 0.5) return texelFetch(_UiMaskAtlas, r.xy + maskTexel(ivec2(floor(p + 0.5)), r.zw, wrap), 0).a;
    ivec2 b = ivec2(floor(p)); vec2 f = fract(p);
    float a = texelFetch(_UiMaskAtlas, r.xy + maskTexel(b, r.zw, wrap), 0).a;
    float c = texelFetch(_UiMaskAtlas, r.xy + maskTexel(b + ivec2(1, 0), r.zw, wrap), 0).a;
    float d = texelFetch(_UiMaskAtlas, r.xy + maskTexel(b + ivec2(0, 1), r.zw, wrap), 0).a;
    float e = texelFetch(_UiMaskAtlas, r.xy + maskTexel(b + ivec2(1, 1), r.zw, wrap), 0).a;
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
layout(std430, set = 2, binding = 13) readonly buffer _UiRectMasks { vec4 rows[]; } rectangles;
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

void main() { applyUiMasks(); ruUiOriginalMain(); entryPointParam_RU_SurfaceFrag_0.a *= rectCoverage(); }
