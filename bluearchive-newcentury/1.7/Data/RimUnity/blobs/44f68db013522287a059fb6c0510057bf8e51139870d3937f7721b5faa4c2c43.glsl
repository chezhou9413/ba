#version 450
layout(binding = 0, set = 2, std140) uniform block_RU_MaterialData_0
{
    vec4 _MainTex_ST; // @default = vec4(0, 0, 1, 1)

    float _UiMaskCount; // @default = 0.0
    float _UiRectMaskCount; // @default = 0.0
}RU_Material_0;


#line 63 0
layout(binding = 1, set = 2)
uniform sampler2D _MainTex; // @default = white


#line 65



#line 67
vec4 RU_tex2D_MainTex_0(vec2 uv_0)
{

#line 67
    return (texture(_MainTex, (vec2(uv_0.x, 1.0 - uv_0.y))));
}


#line 1098 1
layout(location = 0)
out vec4 entryPointParam_frag_0;


#line 1098
layout(location = 2) in vec4 i_color_0; // @vertex Color optional


#line 1098
layout(location = 1) in vec2 i_uv_0; // @vertex TexCoord0 optional


#line 75 0
void ruUiOriginalMain()
{

#line 75
    entryPointParam_frag_0 = i_color_0 * RU_tex2D_MainTex_0(i_uv_0) * i_color_0.w;

#line 75
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
