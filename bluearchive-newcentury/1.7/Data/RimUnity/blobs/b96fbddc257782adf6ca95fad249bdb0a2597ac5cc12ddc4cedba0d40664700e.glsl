#version 450
#extension GL_EXT_control_flow_attributes : require
float RU_CompatSmoothstep(float low, float high, float x) {
    float t = clamp((x - low) / (high - low), float(0.0), float(1.0));
    return t * t * (3.0 - 2.0 * t);
}
vec2 RU_CompatSmoothstep(vec2 low, vec2 high, vec2 x) {
    vec2 t = clamp((x - low) / (high - low), vec2(0.0), vec2(1.0));
    return t * t * (3.0 - 2.0 * t);
}
vec2 RU_CompatSmoothstep(float low, float high, vec2 x) { return RU_CompatSmoothstep(vec2(low), vec2(high), x); }
vec3 RU_CompatSmoothstep(vec3 low, vec3 high, vec3 x) {
    vec3 t = clamp((x - low) / (high - low), vec3(0.0), vec3(1.0));
    return t * t * (3.0 - 2.0 * t);
}
vec3 RU_CompatSmoothstep(float low, float high, vec3 x) { return RU_CompatSmoothstep(vec3(low), vec3(high), x); }
vec4 RU_CompatSmoothstep(vec4 low, vec4 high, vec4 x) {
    vec4 t = clamp((x - low) / (high - low), vec4(0.0), vec4(1.0));
    return t * t * (3.0 - 2.0 * t);
}
vec4 RU_CompatSmoothstep(float low, float high, vec4 x) { return RU_CompatSmoothstep(vec4(low), vec4(high), x); }


#line 99 0
layout(binding = 1, set = 2)
uniform sampler2D _MainTex; // @default = white


#line 101



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

#line 14961 1
vec2 saturate_0(vec2 x_0)
{

#line 14969
    return clamp(x_0, vec2(0.0), vec2(1.0));
}


#line 14946
float saturate_1(float x_1)
{

#line 14954
    return clamp(x_1, 0.0, 1.0);
}


#line 103 0
vec4 RU_tex2D_MainTex_0(vec2 uv_0)
{

#line 103
    return (texture(_MainTex, (vec2(uv_0.x, 1.0 - uv_0.y))));
}


#line 103
layout(location = 0)
out vec4 entryPointParam_frag_0;


#line 103
layout(location = 2) in vec4 input_color_0; // @vertex Color optional


#line 103
layout(location = 1) in vec2 input_texcoord_0; // @vertex TexCoord0 optional


#line 111
void ruUiOriginalMain()
{

#line 111
    vec4 _S1 = (RU_tex2D_MainTex_0(input_texcoord_0) + RU_Material_0._TextureSampleAdd) * input_color_0;

#line 111
    vec4 sampled_0 = _S1;

#line 111
    vec3 _S2 = _S1.xyz;

#line 111
    sampled_0.xyz = mix(_S2, dot(_S2, vec3(0.29899999499320984, 0.58700001239776611, 0.11400000005960464)) * RU_Material_0._FillColor.xyz, vec3(saturate_1(RU_Material_0._FillColorStrength)));

#line 111
    vec2 progressAxis_0 = RU_Material_0._ProgressAxis.xy;

#line 111
    vec2 progressAxis_1 = progressAxis_0 / max(length(progressAxis_0), 0.00009999999747379);

#line 111
    float _S3 = progressAxis_1.y;

#line 111
    float _S4 = - _S3;

#line 111
    float _S5 = progressAxis_1.x;

#line 111
    float _S6 = abs(_S5);

#line 111
    vec2 _S7 = saturate_0((input_texcoord_0 - RU_Material_0._UvRect.xy) / max(RU_Material_0._UvRect.zw, vec2(0.00000999999974738, 0.00000999999974738))) - 0.5;

#line 111
    float alongArc_0 = dot(_S7, progressAxis_1) / max(_S6 + abs(_S3), 0.00009999999747379) + 0.5;

#line 111
    float acrossArc_0 = dot(_S7, vec2(_S4, _S5)) / max(abs(_S4) + _S6, 0.00009999999747379) + 0.5;

#line 111
    float alongArc_1 = mix(alongArc_0, 1.0 - alongArc_0, step(0.5, RU_Material_0._Reverse));

#line 111
    float _S8 = max((fwidth((alongArc_1))) * 1.25, RU_Material_0._CutoffSoftness);

#line 111
    float visibility_0 = 1.0 - RU_CompatSmoothstep(RU_Material_0._Progress - _S8, RU_Material_0._Progress + _S8, alongArc_1);

#line 111
    float _S9 = max(RU_Material_0._GlowWidth, 0.00009999999747379);

#line 111
    float normalizedX_0 = abs(alongArc_1 - RU_Material_0._Progress) / _S9;

#line 111
    float normalizedY_0 = abs(acrossArc_0 - 0.5) / 0.55000001192092896;

#line 111
    float _S10 = normalizedX_0 * normalizedX_0;

#line 111
    float _S11 = normalizedY_0 * normalizedY_0;

#line 111
    float softHalo_0 = (exp2((- (_S10 * 0.72000002861022949 + _S11 * 0.85000002384185791))));

#line 111
    float activeEndpoint_0 = step(0.00009999999747379, RU_Material_0._Progress) * saturate_1(RU_Material_0._GlowEnabled);

#line 111
    float glow_0 = (softHalo_0 * 0.69999998807907104 + (exp2((- (_S10 * 7.5 + _S11 * 2.40000009536743164)))) * 1.64999997615814209) * activeEndpoint_0 * (1.0 + sin(RU_Material_0._Time.y * RU_Material_0._PulseSpeed) * RU_Material_0._PulseAmount);

#line 111
    float _S12 = max(RU_Material_0._SpriteAspect, 0.00999999977648258);

#line 111
    int particleIndex_0 = 0;

#line 111
    float particles_0 = 0.0;

#line 111
    [[unroll]]
    for(;;)
    {

#line 111
        if(particleIndex_0 < 5)
        {
        }
        else
        {

#line 111
            break;
        }

#line 111
        float particleNumber_0 = float(particleIndex_0) + 1.0;

#line 111
        float seed_0 = fract(sin(particleNumber_0 * 78.233001708984375) * 43758.546875);

#line 111
        float life_0 = fract(RU_Material_0._Time.y * RU_Material_0._ParticleSpeed * (0.34999999403953552 + seed_0 * 0.30000001192092896) + seed_0);

#line 111
        float _S13 = life_0 - 0.5;

#line 111
        float _S14 = fract(sin(particleNumber_0 * 39.42499923706054688 + 17.12999916076660156) * 24634.634765625) - 0.5;

#line 111
        float particleRadius_0 = RU_Material_0._ParticleSize * mix(1.0, 0.30000001192092896, life_0);

#line 111
        float particles_1 = particles_0 + (1.0 - RU_CompatSmoothstep(particleRadius_0, particleRadius_0 * 1.75, length(vec2((alongArc_1 - (RU_Material_0._Progress + _S14 * _S9 * 2.09999990463256836 + _S13 * _S9 * _S14 * 2.40000009536743164)) * _S12, acrossArc_0 - (0.5 + (seed_0 - 0.5) * 0.72000002861022949 + _S13 * _S14 * 0.2199999988079071))))) * (4.0 * life_0 * (1.0 - life_0));

#line 111
        particleIndex_0 = particleIndex_0 + 1;

#line 111
        particles_0 = particles_1;

#line 111
    }

#line 111
    float particles_2 = particles_0 * (activeEndpoint_0 * RU_Material_0._ParticleIntensity);

#line 111
    float baseAlpha_0 = sampled_0.w * visibility_0;

#line 111
    float effectMask_0 = mix(1.0, saturate_1(sampled_0.w * 8.0), saturate_1(RU_Material_0._GlowContainment));

#line 111
    float haloAlpha_0 = softHalo_0 * activeEndpoint_0 * 0.20000000298023224 * effectMask_0;

#line 111
    float particleAlpha_0 = saturate_1(particles_2 * 0.64999997615814209) * effectMask_0;

#line 111
    float _S15 = max(RU_Material_0._CompletionDuration, 0.00009999999747379);

#line 111
    float completionAge_0 = RU_Material_0._Time.y - RU_Material_0._CompletionStartTime;

#line 111
    float completionFlash_0 = RU_CompatSmoothstep(0.0, _S15 * 0.18000000715255737, completionAge_0) * (1.0 - RU_CompatSmoothstep(_S15 * 0.55000001192092896, _S15, completionAge_0)) * (step(0.0, completionAge_0) * step(completionAge_0, _S15));

#line 111
    float completionShape_0 = saturate_1(baseAlpha_0 * 8.0);

#line 111
    sampled_0.xyz = sampled_0.xyz * visibility_0;

#line 111
    sampled_0.xyz = mix(sampled_0.xyz, RU_Material_0._CompletionColor.xyz, vec3(completionFlash_0 * completionShape_0 * 0.92000001668930054));

#line 111
    sampled_0.xyz = sampled_0.xyz + RU_Material_0._CompletionColor.xyz * completionFlash_0 * RU_Material_0._CompletionIntensity * baseAlpha_0;

#line 111
    sampled_0.xyz = sampled_0.xyz + RU_Material_0._GlowColor.xyz * glow_0 * RU_Material_0._GlowIntensity * (0.30000001192092896 + sampled_0.w * 0.69999998807907104);

#line 111
    sampled_0.xyz = sampled_0.xyz + RU_Material_0._GlowColor.xyz * particles_2 * (1.25 + RU_Material_0._GlowIntensity * 0.20000000298023224);

#line 111
    sampled_0[3] = saturate_1(baseAlpha_0 + haloAlpha_0 + particleAlpha_0);

#line 111
    entryPointParam_frag_0 = sampled_0;

#line 111
    return;
}


// @rimunity-host-matrices 1

layout(std430, set = 2, binding = 2) readonly buffer _UiMasks { vec4 rows[]; } masks;
layout(set = 2, binding = 3) uniform sampler2D _UiMaskAtlas;
layout(std430, set = 2, binding = 4) readonly buffer _UiMaskRegions { vec4 rows[]; } regions;
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
layout(std430, set = 2, binding = 5) readonly buffer _UiRectMasks { vec4 rows[]; } rectangles;
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

void main() { applyUiMasks(); ruUiOriginalMain(); entryPointParam_frag_0.a *= rectCoverage(); }
