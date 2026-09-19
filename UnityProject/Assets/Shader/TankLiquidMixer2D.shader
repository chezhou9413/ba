Shader "RaveAsset/2D/TankLiquidMixer2D"
{
    Properties
    {
        [Header(Texture)]
        [PerRendererData] _MainTex ("Sprite 蒙版范围", 2D) = "white" {}

        [Header(Mask)]
        _MaskCutoff ("蒙版透明阈值", Range(0, 1)) = 0.01
        _AutoFitMask ("自动使用蒙版中心", Range(0, 1)) = 1
        _MaskSizeX ("备用蒙版宽度", Range(0.05, 1)) = 0.42
        _MaskSizeY ("备用蒙版高度", Range(0.05, 1)) = 0.42
        _MixerCenterOffsetX ("搅拌中心偏移 X", Range(-0.5, 0.5)) = 0
        _MixerCenterOffsetY ("搅拌中心偏移 Y", Range(-0.5, 0.5)) = 0
        _MaskShrink ("边缘收缩", Range(0, 0.2)) = 0.025

        [Header(Color)]
        _LiquidColor ("液体颜色", Color) = (0.92, 0.91, 0.86, 0.97)
        _DeepColor ("外圈阴影", Color) = (0.58, 0.57, 0.52, 0.98)
        _FoamColor ("中心白色流线", Color) = (1, 0.99, 0.94, 0.82)
        _HighlightColor ("水面高光", Color) = (1, 1, 1, 0.36)

        [Header(Mixer)]
        _SwirlStrength ("旋涡扭转强度", Range(0, 8)) = 5.6
        _SwirlRadius ("旋涡半径", Range(0.05, 2)) = 0.9
        _SwirlSpeed ("旋涡速度", Range(-10, 10)) = 1.65
        _VortexPull ("外圈压暗强度", Range(0, 1)) = 0.42
        _BladeCount ("旋臂数量", Range(2, 8)) = 3
        _BladeSharpness ("旋臂锐度", Range(1, 16)) = 4.8

        [Header(Flow)]
        _DistortionStrength ("流线弯曲强度", Range(0, 0.25)) = 0.08
        _RadialWaveScale ("环向流线密度", Range(1, 80)) = 6.5
        _RadialWaveStrength ("环向流线强度", Range(0, 1)) = 0.18
        _FoamStrength ("白色旋臂强度", Range(0, 1)) = 0.55
        _FoamRingScale ("旋臂卷曲密度", Range(1, 80)) = 5.2
        _FoamCenterBoost ("中心白色增强", Range(0, 1)) = 0.78
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MaskCutoff;
            float _AutoFitMask;
            float _MaskSizeX;
            float _MaskSizeY;
            float _MixerCenterOffsetX;
            float _MixerCenterOffsetY;
            float _MaskShrink;
            fixed4 _LiquidColor;
            fixed4 _DeepColor;
            fixed4 _FoamColor;
            fixed4 _HighlightColor;
            float _SwirlStrength;
            float _SwirlRadius;
            float _SwirlSpeed;
            float _VortexPull;
            float _BladeCount;
            float _BladeSharpness;
            float _DistortionStrength;
            float _RadialWaveScale;
            float _RadialWaveStrength;
            float _FoamStrength;
            float _FoamRingScale;
            float _FoamCenterBoost;

            //顶点输入负责接收贴花面片的顶点、UV 和顶点色。
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            //片元输入负责把裁剪空间坐标、采样 UV、原始蒙版 UV 和顶点色传给片元阶段。
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 maskUv : TEXCOORD1;
                fixed4 color : COLOR;
            };

            //顶点函数负责把贴花顶点变换到裁剪空间，并保留原始 UV 作为蒙版坐标。
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.maskUv = v.uv;
                o.color = v.color;
                return o;
            }

            //蒙版数据函数负责用透明度估算液体蒙版中心，使旋涡落在蒙版真实中心。
            float4 GetMaskData()
            {
                float2 minUv = float2(1.0, 1.0);
                float2 maxUv = float2(0.0, 0.0);
                float2 weightedCenter = float2(0.0, 0.0);
                float alphaSum = 0.0;

                [unroll]
                for (int y = 0; y < 16; y++)
                {
                    [unroll]
                    for (int x = 0; x < 16; x++)
                    {
                        float2 sampleUv = (float2(x, y) + 0.5) * 0.0625;
                        float sampleAlpha = tex2D(_MainTex, sampleUv).a;
                        float sampleMask = step(_MaskCutoff, sampleAlpha);
                        weightedCenter += sampleUv * sampleMask;
                        alphaSum += sampleMask;
                        minUv = lerp(minUv, min(minUv, sampleUv), sampleMask);
                        maxUv = lerp(maxUv, max(maxUv, sampleUv), sampleMask);
                    }
                }

                float hasMask = step(0.001, alphaSum);
                float autoFit = hasMask * saturate(_AutoFitMask);
                float2 sampledCenter = weightedCenter / max(alphaSum, 0.0001);
                float2 sampledSize = max(maxUv - minUv, float2(0.01, 0.01));
                float2 fallbackSize = max(float2(_MaskSizeX, _MaskSizeY), float2(0.01, 0.01));
                float2 center = lerp(float2(0.5, 0.5), sampledCenter, autoFit);
                float2 size = lerp(fallbackSize, sampledSize, autoFit);
                return float4(center, size);
            }

            //搅拌中心函数负责把人工偏移叠加到蒙版中心上。
            float2 GetMixerCenter(float4 maskData)
            {
                return maskData.xy + float2(_MixerCenterOffsetX, _MixerCenterOffsetY) * maskData.zw;
            }

            //本地坐标函数负责把 UV 转成以搅拌中心为原点、按蒙版尺寸归一化的坐标。
            float2 GetMixerLocalUv(float2 uv, float4 maskData)
            {
                float2 center = GetMixerCenter(maskData);
                float2 size = max(maskData.zw, float2(0.0001, 0.0001));
                return (uv - center) / size;
            }

            //旋转函数负责按给定角度旋转二维坐标。
            float2 Rotate2D(float2 value, float angle)
            {
                float angleSin = sin(angle);
                float angleCos = cos(angle);
                return float2(value.x * angleCos - value.y * angleSin, value.x * angleSin + value.y * angleCos);
            }

            //安全归一化函数负责避免中心点附近出现除零导致的异常方向。
            float2 SafeNormalize(float2 value)
            {
                return value * rsqrt(max(dot(value, value), 0.00001));
            }

            //旋涡衰减函数负责让搅拌强度集中在设定半径内，并向外柔和消失。
            float GetSwirlFalloff(float radius)
            {
                float safeRadius = max(_SwirlRadius, 0.0001);
                return 1.0 - smoothstep(safeRadius * 0.45, safeRadius, radius);
            }

            //旋涡坐标函数负责把本地坐标按半径和时间扭转，形成干净的环向拉扯。
            float2 GetSwirledLocal(float2 localUv, float radius, float falloff)
            {
                float timePhase = _Time.y * _SwirlSpeed;
                float innerBoost = 1.0 / (radius * 3.0 + 0.35);
                float swirlAngle = timePhase + _SwirlStrength * falloff * innerBoost;
                return Rotate2D(localUv, swirlAngle);
            }

            //旋臂函数负责生成无噪声的螺旋亮色条带。
            float GetBladeMask(float2 localUv, float2 swirledLocal, float radius, float falloff)
            {
                float angle = atan2(swirledLocal.y, swirledLocal.x);
                float timePhase = _Time.y * _SwirlSpeed;
                float curvedRadius = radius + sin(angle * max(_BladeCount, 1.0) + timePhase) * _DistortionStrength * 0.08;
                float spiralPhase = angle * max(_BladeCount, 1.0) + curvedRadius * _FoamRingScale * 6.28318 - timePhase * 2.6;
                float bladeWave = sin(spiralPhase) * 0.5 + 0.5;
                float bladeMask = smoothstep(0.52, 0.96, bladeWave);
                bladeMask = pow(saturate(bladeMask), max(_BladeSharpness * 0.55, 0.0001));
                float centerFade = 1.0 - smoothstep(0.01, max(_SwirlRadius, 0.0001) * 0.82, radius);
                return bladeMask * saturate(falloff + centerFade * 0.35);
            }

            //环流函数负责生成无噪声的同心流线，避免液面变成随机云纹。
            float GetRingMask(float2 swirledLocal, float radius, float falloff)
            {
                float angle = atan2(swirledLocal.y, swirledLocal.x);
                float ringPhase = radius * _RadialWaveScale * 6.28318 - angle * 1.15 - _Time.y * abs(_SwirlSpeed) * 1.8;
                float ringWave = sin(ringPhase) * 0.5 + 0.5;
                return smoothstep(0.54, 0.95, ringWave) * _RadialWaveStrength * falloff;
            }

            //中心函数负责在旋涡中心生成稳定的白色汇聚区。
            float GetCenterMask(float radius)
            {
                return 1.0 - smoothstep(0.0, max(_SwirlRadius, 0.0001) * 0.30, radius);
            }

            //边缘函数负责让蒙版内部边缘略微收缩，避免透明边缘出现硬色边。
            float GetInnerMask(float maskAlpha)
            {
                float cutoff = saturate(_MaskCutoff + _MaskShrink);
                return smoothstep(cutoff, min(cutoff + 0.08, 1.0), maskAlpha);
            }

            //颜色函数负责把基础液色、干净流线和中心白色合成为最终水面颜色。
            fixed4 GetLiquidColor(float bladeMask, float ringMask, float centerMask, float radius, fixed4 vertexColor)
            {
                float outerShade = smoothstep(0.0, max(_SwirlRadius, 0.0001), radius) * _VortexPull;
                fixed4 liquid = lerp(_LiquidColor, _DeepColor, saturate(outerShade));
                float highlightMask = saturate(bladeMask * 0.55 + ringMask);
                float whiteMask = saturate(centerMask * _FoamCenterBoost + bladeMask * _FoamStrength + ringMask * 0.45);
                liquid.rgb = lerp(liquid.rgb, _HighlightColor.rgb, highlightMask * _HighlightColor.a);
                liquid.rgb = lerp(liquid.rgb, _FoamColor.rgb, whiteMask * _FoamColor.a);
                liquid *= vertexColor;
                return liquid;
            }

            //片元函数负责用 Sprite alpha 作为蒙版，并在蒙版中心绘制干净的旋涡流线。
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 maskSample = tex2D(_MainTex, i.maskUv);
                float maskAlpha = maskSample.a;
                clip(maskAlpha - _MaskCutoff);

                float4 maskData = GetMaskData();
                float2 localUv = GetMixerLocalUv(i.maskUv, maskData);
                float radius = length(localUv);
                float falloff = GetSwirlFalloff(radius);
                float2 swirledLocal = GetSwirledLocal(localUv, radius, falloff);

                float bladeMask = GetBladeMask(localUv, swirledLocal, radius, falloff);
                float ringMask = GetRingMask(swirledLocal, radius, falloff);
                float centerMask = GetCenterMask(radius);
                float innerMask = GetInnerMask(maskAlpha);

                fixed4 liquid = GetLiquidColor(bladeMask, ringMask, centerMask, radius, i.color);
                liquid.a *= saturate(innerMask * lerp(0.82, 1.0, centerMask * 0.35 + bladeMask * 0.25));
                return liquid;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
