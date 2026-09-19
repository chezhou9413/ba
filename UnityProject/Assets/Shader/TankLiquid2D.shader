Shader "RaveAsset/2D/TankLiquid2D"
{
    Properties
    {
        [Header(Texture)]
        [PerRendererData] _MainTex ("Sprite 液体范围", 2D) = "white" {}
        _NoiseTex ("流动噪声贴图", 2D) = "gray" {}

        [Header(Fill)]
        _FillAmount ("液体进度", Range(0, 1)) = 0.5
        _FillCurve ("容量增长曲线", Range(0.2, 4)) = 1.8
        _FillSoftness ("液面边缘柔化", Range(0.0001, 0.2)) = 0.02

        [Header(Ellipse Fill)]
        _AutoFitMask ("自适应蒙版范围", Range(0, 1)) = 1
        _LiquidCenterOffsetX ("液体中心微调 X", Range(-0.5, 0.5)) = 0
        _LiquidCenterOffsetY ("液体中心微调 Y", Range(-0.5, 0.5)) = 0
        _LiquidVerticalShift ("容量变化纵向位移倍率", Range(-1, 1)) = 0.12
        _LiquidAngle ("液体透视角度", Range(-45, 45)) = -10
        _LiquidMinWidth ("最低容量宽度倍率", Range(0.01, 3)) = 0.18
        _LiquidMaxWidth ("最高容量宽度倍率", Range(0.01, 3)) = 1.35
        _LiquidMinHeight ("最低容量高度倍率", Range(0.01, 3)) = 0.12
        _LiquidMaxHeight ("最高容量高度倍率", Range(0.01, 3)) = 1.35

        [Header(Liquid)]
        _LiquidColor ("液体颜色", Color) = (0.15, 0.65, 1.0, 0.85)
        _BubbleColor ("气泡颜色", Color) = (1, 1, 1, 0.45)

        [Header(Flow Noise)]
        _NoiseStrength ("流动噪声强度", Range(0, 1)) = 0.22
        _NoiseScale ("流动噪声缩放", Range(1, 80)) = 18
        _NoiseSpeed ("流动噪声速度", Range(-5, 5)) = 0.35
        _NoiseContrast ("流动噪声对比", Range(0.2, 4)) = 1.4
        _NoiseDirection ("流动噪声方向 XY", Vector) = (1, 0.35, 0, 0)

        [Header(Edge Flow)]
        _EdgeFlowStrength ("水面晃动强度", Range(0, 1)) = 0.35
        _EdgeFlowWidth ("水面波纹幅度", Range(0.001, 0.2)) = 0.035
        _EdgeFlowScale ("水面波纹频率", Range(1, 80)) = 8
        _EdgeFlowSpeed ("水面晃动速度", Range(-5, 5)) = 0.55
        _EdgeFlowDirection ("水面倾斜参数 XY", Vector) = (0.25, 1, 0, 0)

        [Header(Bubbles)]
        _BubbleStrength ("气泡强度", Range(0, 1)) = 0.35
        _BubbleDensity ("气泡密度", Range(1, 80)) = 24
        _BubbleSize ("气泡大小", Range(0.01, 0.25)) = 0.055
        _BubbleSpeed ("气泡速度", Range(-5, 5)) = 0.35
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
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float _FillAmount;
            float _FillCurve;
            float _FillSoftness;
            float _AutoFitMask;
            float _LiquidCenterOffsetX;
            float _LiquidCenterOffsetY;
            float _LiquidVerticalShift;
            float _LiquidAngle;
            float _LiquidMinWidth;
            float _LiquidMaxWidth;
            float _LiquidMinHeight;
            float _LiquidMaxHeight;
            fixed4 _LiquidColor;
            fixed4 _BubbleColor;
            float _NoiseStrength;
            float _NoiseScale;
            float _NoiseSpeed;
            float _NoiseContrast;
            float4 _NoiseDirection;
            float _EdgeFlowStrength;
            float _EdgeFlowWidth;
            float _EdgeFlowScale;
            float _EdgeFlowSpeed;
            float4 _EdgeFlowDirection;
            float _BubbleStrength;
            float _BubbleDensity;
            float _BubbleSize;
            float _BubbleSpeed;

            // 顶点输入负责接收面片顶点坐标、UV 和顶点色。
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            // 片元输入负责把裁剪空间坐标、主贴图 UV、原始范围 UV 和顶点色传给片元阶段。
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 rangeUv : TEXCOORD1;
                fixed4 color : COLOR;
            };

            // 顶点函数负责把本地顶点变换到裁剪空间，并计算纹理采样坐标。
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.rangeUv = v.uv;
                o.color = v.color;
                return o;
            }

            // 容量函数负责把线性进度转换成示意图中较平滑的圆盖增长量。
            float GetLiquidGrowth()
            {
                float amount = saturate(_FillAmount);
                float softenedCurve = lerp(1.0, max(_FillCurve, 0.0001), 0.55);
                return pow(amount, softenedCurve);
            }

            // 蒙版采样函数负责估算 Sprite 透明范围的中心和尺寸，用于让液体自动适配不同蒙版。
            float4 GetMaskData()
            {
                float2 minUv = float2(1.0, 1.0);
                float2 maxUv = float2(0.0, 0.0);
                float hasMask = 0.0;

                [unroll]
                for (int y = 0; y < 8; y++)
                {
                    [unroll]
                    for (int x = 0; x < 8; x++)
                    {
                        float2 sampleUv = (float2(x, y) + 0.5) * 0.125;
                        float sampleMask = step(0.01, tex2D(_MainTex, sampleUv).a);

                        // 只用有 alpha 的采样点参与范围估算，避免透明画布影响中心。
                        minUv = lerp(minUv, min(minUv, sampleUv), sampleMask);
                        maxUv = lerp(maxUv, max(maxUv, sampleUv), sampleMask);
                        hasMask = max(hasMask, sampleMask);
                    }
                }

                float2 sampledCenter = (minUv + maxUv) * 0.5;
                float2 sampledSize = max(maxUv - minUv, float2(0.01, 0.01));
                float2 center = lerp(float2(0.5, 0.5), sampledCenter, hasMask * saturate(_AutoFitMask));
                float2 size = lerp(float2(1.0, 1.0), sampledSize, hasMask * saturate(_AutoFitMask));
                return float4(center, size);
            }

            // 中心偏移函数负责让低进度保留视角偏移，高进度逐渐回到蒙版中心以贴近整罐圆盖。
            float2 GetLiquidCenterOffset(float growth, float4 maskData)
            {
                float2 maskSize = maskData.zw;
                float offsetFade = 1.0 - smoothstep(0.18, 0.92, growth);
                return float2(_LiquidCenterOffsetX, _LiquidCenterOffsetY) * maskSize * offsetFade;
            }

            // 中心函数负责根据示意图让液体从底部小椭圆过渡到居中的整罐圆盖。
            float2 GetLiquidCenter(float growth, float4 maskData)
            {
                float2 maskCenter = maskData.xy;
                float2 maskSize = maskData.zw;
                float stageLift = _LiquidVerticalShift * maskSize.y * (growth - 0.5);
                float liftFade = 1.0 - smoothstep(0.7, 1.0, growth);
                return maskCenter + GetLiquidCenterOffset(growth, maskData) + float2(0.0, stageLift * liftFade);
            }

            // 局部坐标函数负责把蒙版坐标转换成以球罐中心为原点的标准化坐标。
            float2 GetMaskLocalUv(float2 uv, float growth, float4 maskData)
            {
                float2 center = GetLiquidCenter(growth, maskData);
                float2 maskSize = maskData.zw;
                float2 local = (uv - center) / max(maskSize * 0.5, float2(0.0001, 0.0001));
                float angle = -_LiquidAngle * 0.01745329252;
                float angleSin = sin(angle);
                float angleCos = cos(angle);
                return float2(local.x * angleCos - local.y * angleSin, local.x * angleSin + local.y * angleCos);
            }

            // 半径函数负责给气泡和噪声提供随容量变化的局部范围。
            float2 GetLiquidRadii(float growth, float4 maskData)
            {
                float2 maskSize = maskData.zw;
                float widthGrowth = smoothstep(0.0, 1.0, growth);
                float heightGrowth = smoothstep(0.0, 1.0, pow(growth, 0.85));
                float width = maskSize.x * lerp(_LiquidMinWidth, _LiquidMaxWidth, widthGrowth);
                float height = maskSize.y * lerp(_LiquidMinHeight, _LiquidMaxHeight, heightGrowth);
                return float2(max(width * 0.5, 0.0001), max(height * 0.5, 0.0001));
            }

            // 液体体积函数负责生成示意图中的球罐液面截面，并允许边缘用噪声产生轻微晃动。
            float GetLiquidBodyMask(float2 uv, float growth, float4 maskData, float edgeJitter)
            {
                float amount = saturate(_FillAmount);
                float2 local = GetMaskLocalUv(uv, growth, maskData);
                float surfaceWidth = lerp(_LiquidMinWidth, _LiquidMaxWidth, smoothstep(0.0, 1.0, growth));
                float surfaceArc = lerp(_LiquidMinHeight, _LiquidMaxHeight, smoothstep(0.0, 1.0, pow(growth, 0.85)));
                float planeY = lerp(-1.35, 1.22, pow(amount, 1.35));
                float normalizedX = local.x / max(surfaceWidth, 0.0001);
                float arcMask = saturate(1.0 - normalizedX * normalizedX);
                float curvedSurfaceY = planeY + sqrt(arcMask) * surfaceArc * 0.55 + edgeJitter;
                float sideFade = lerp(1.0 - smoothstep(0.98, 1.02, abs(normalizedX)), 1.0, smoothstep(0.45, 0.9, amount));
                float bodyMask = 1.0 - smoothstep(curvedSurfaceY, curvedSurfaceY + max(_FillSoftness, 0.0001), local.y);
                float naturalFull = smoothstep(0.985, 1.0, amount);
                return lerp(bodyMask * sideFade, 1.0, naturalFull);
            }

            // 随机函数负责根据格子坐标生成稳定的程序化随机值。
            float Hash21(float2 value)
            {
                return frac(sin(dot(value, float2(127.1, 311.7))) * 43758.5453);
            }

            // 噪声采样函数负责读取用户配置的噪声贴图，并转换成以 0 为中心的明暗扰动。
            float SampleFlowNoise(float2 uv, float scale, float speed, float2 direction)
            {
                float2 safeDirection = normalize(direction + float2(0.0001, 0.0001));
                float2 noiseBase = uv * scale;
                float2 noiseUv = noiseBase * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                noiseUv += safeDirection * (_Time.y * speed);
                float noiseValue = tex2D(_NoiseTex, noiseUv).r;
                float centeredNoise = (noiseValue - 0.5) * 2.0;
                return sign(centeredNoise) * pow(abs(centeredNoise), max(_NoiseContrast, 0.0001));
            }

            // 边缘晃动函数负责用规则倾斜和正弦波模拟水瓶里液面轻微摇晃。
            float GetEdgeJitter(float2 uv, float growth, float4 maskData)
            {
                float2 maskSize = max(maskData.zw, float2(0.0001, 0.0001));
                float2 localUv = (uv - maskData.xy) / maskSize;
                float swayPhase = _Time.y * _EdgeFlowSpeed;
                float sway = sin(swayPhase) * _EdgeFlowStrength;
                float tilt = localUv.x * sway * _EdgeFlowDirection.x;
                float ripple = sin((localUv.x * _EdgeFlowScale) + swayPhase * 1.7) * _EdgeFlowWidth * _EdgeFlowDirection.y;
                float amountFade = 1.0 - smoothstep(0.985, 1.0, saturate(_FillAmount));
                return (tilt + ripple) * amountFade;
            }

            // 流动噪声函数负责在整体液体表面采样用户指定噪声贴图，生成随时间移动的明暗变化。
            float GetFlowNoise(float2 uv, float4 maskData, float fillMask)
            {
                float2 maskCenter = maskData.xy;
                float2 maskSize = max(maskData.zw, float2(0.0001, 0.0001));
                float2 localUv = (uv - maskCenter) / maskSize;
                return SampleFlowNoise(localUv, _NoiseScale, _NoiseSpeed, _NoiseDirection.xy) * _NoiseStrength * fillMask;
            }

            // 气泡函数负责在液体体积内部生成随时间缓慢漂浮的圆形亮点。
            float GetBubbleMask(float2 uv, float growth, float4 maskData, float fillMask)
            {
                float2 center = GetLiquidCenter(growth, maskData);
                float2 radii = GetLiquidRadii(growth, maskData);
                float angle = -_LiquidAngle * 0.01745329252;
                float angleSin = sin(angle);
                float angleCos = cos(angle);
                float2 offset = uv - center;
                float2 local = float2(offset.x * angleCos - offset.y * angleSin, offset.x * angleSin + offset.y * angleCos);
                float2 bubbleUv = (local / max(radii, float2(0.0001, 0.0001))) * 0.5 + 0.5;
                bubbleUv.y += _Time.y * _BubbleSpeed * 0.08;
                bubbleUv *= max(_BubbleDensity, 1.0);

                float2 cell = floor(bubbleUv);
                float2 cellUv = frac(bubbleUv);
                float randomValue = Hash21(cell);
                float2 bubbleCenter = float2(Hash21(cell + 13.7), Hash21(cell + 41.3));
                float bubbleRadius = _BubbleSize * lerp(0.45, 1.25, randomValue);
                float bubble = 1.0 - smoothstep(bubbleRadius, bubbleRadius + 0.025, length(cellUv - bubbleCenter));
                float sparseMask = step(0.62, randomValue);
                return bubble * sparseMask * fillMask * _BubbleStrength;
            }

            // 片元函数负责用 Sprite 主贴图透明区域限制液体范围，并应用液体进度和气泡。
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 rangeSample = tex2D(_MainTex, i.rangeUv);
                float rangeAlpha = rangeSample.a;

                float growth = GetLiquidGrowth();
                float4 maskData = GetMaskData();
                float edgeJitter = GetEdgeJitter(i.rangeUv, growth, maskData);
                float fillMask = GetLiquidBodyMask(i.rangeUv, growth, maskData, edgeJitter) * rangeAlpha;
                float flowNoise = GetFlowNoise(i.rangeUv, maskData, fillMask);
                float bubbleMask = GetBubbleMask(i.rangeUv, growth, maskData, fillMask);
                float visibleMask = saturate(fillMask);
                clip(visibleMask - 0.001);

                fixed4 liquid = _LiquidColor * i.color;
                liquid.rgb *= 1.0 + flowNoise;
                liquid.rgb = lerp(liquid.rgb, _BubbleColor.rgb, saturate(bubbleMask * _BubbleColor.a));
                liquid.a *= visibleMask;
                return liquid;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
