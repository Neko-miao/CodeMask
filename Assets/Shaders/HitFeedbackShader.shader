// ================================================
// MaskSystem - 打击反馈着色器
// 爆炸、波纹、粒子效果
// ================================================

Shader "MaskSystem/HitFeedback"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 0.9, 0.2, 1)
        _SecondaryColor ("Secondary Color", Color) = (1, 0.5, 0, 1)
        
        _Progress ("Animation Progress", Range(0, 1)) = 0
        _EffectType ("Effect Type (0=Perfect,1=Normal,2=Miss)", Range(0, 2)) = 0
        _Intensity ("Intensity", Range(0.5, 2.0)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float4 _MainColor;
            float4 _SecondaryColor;
            float _Progress;
            float _EffectType;
            float _Intensity;
            
            // Hash function
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(41.41, 28.28))) * 43758.5453);
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // 绘制圆环
            float drawRing(float2 uv, float2 center, float radius, float width)
            {
                float dist = length(uv - center);
                float inner = smoothstep(radius - width - 0.01, radius - width + 0.01, dist);
                float outer = smoothstep(radius + 0.01, radius - 0.01, dist);
                return inner * outer;
            }
            
            // 绘制放射线
            float drawRays(float2 uv, float2 center, int numRays, float progress)
            {
                float2 p = uv - center;
                float angle = atan2(p.y, p.x);
                float dist = length(p);
                
                float rayAngle = 6.28318 / numRays;
                float ray = abs(fmod(angle + 3.14159, rayAngle) - rayAngle * 0.5);
                ray = 1.0 - smoothstep(0, rayAngle * 0.2, ray);
                
                // 随进度向外扩散
                float rayLength = progress * 0.6;
                float rayMask = smoothstep(0, rayLength * 0.3, dist) * smoothstep(rayLength + 0.05, rayLength - 0.05, dist);
                
                return ray * rayMask;
            }
            
            // Perfect 效果 - 金色爆发
            float4 perfectEffect(float2 uv, float progress)
            {
                float4 color = float4(0, 0, 0, 0);
                float2 center = float2(0.5, 0.5);
                
                // 扩散环
                float ringRadius = progress * 0.7;
                float ringWidth = 0.05 * (1.0 - progress);
                float ring = drawRing(uv, center, ringRadius, ringWidth);
                
                // 第二个环（稍慢）
                float ring2Radius = progress * 0.5;
                float ring2 = drawRing(uv, center, ring2Radius, ringWidth * 1.5);
                
                // 放射线
                float rays = drawRays(uv, center, 12, progress);
                
                // 中心光芒
                float dist = length(uv - center);
                float centerGlow = (1.0 - progress) * 0.5 / (dist * dist * 10.0 + 0.1);
                
                // 星星粒子
                float particles = 0;
                
                // Particle 0
                float pAngle0 = 0 * 0.785398;
                float pDist0 = progress * (0.3 + hash(float2(0, 0)) * 0.3);
                float2 pPos0 = center + float2(cos(pAngle0), sin(pAngle0)) * pDist0;
                float pSize0 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize0, 0, length(uv - pPos0));
                
                // Particle 1
                float pAngle1 = 1 * 0.785398;
                float pDist1 = progress * (0.3 + hash(float2(1, 1)) * 0.3);
                float2 pPos1 = center + float2(cos(pAngle1), sin(pAngle1)) * pDist1;
                float pSize1 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize1, 0, length(uv - pPos1));
                
                // Particle 2
                float pAngle2 = 2 * 0.785398;
                float pDist2 = progress * (0.3 + hash(float2(2, 2)) * 0.3);
                float2 pPos2 = center + float2(cos(pAngle2), sin(pAngle2)) * pDist2;
                float pSize2 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize2, 0, length(uv - pPos2));
                
                // Particle 3
                float pAngle3 = 3 * 0.785398;
                float pDist3 = progress * (0.3 + hash(float2(3, 3)) * 0.3);
                float2 pPos3 = center + float2(cos(pAngle3), sin(pAngle3)) * pDist3;
                float pSize3 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize3, 0, length(uv - pPos3));
                
                // Particle 4
                float pAngle4 = 4 * 0.785398;
                float pDist4 = progress * (0.3 + hash(float2(4, 4)) * 0.3);
                float2 pPos4 = center + float2(cos(pAngle4), sin(pAngle4)) * pDist4;
                float pSize4 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize4, 0, length(uv - pPos4));
                
                // Particle 5
                float pAngle5 = 5 * 0.785398;
                float pDist5 = progress * (0.3 + hash(float2(5, 5)) * 0.3);
                float2 pPos5 = center + float2(cos(pAngle5), sin(pAngle5)) * pDist5;
                float pSize5 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize5, 0, length(uv - pPos5));
                
                // Particle 6
                float pAngle6 = 6 * 0.785398;
                float pDist6 = progress * (0.3 + hash(float2(6, 6)) * 0.3);
                float2 pPos6 = center + float2(cos(pAngle6), sin(pAngle6)) * pDist6;
                float pSize6 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize6, 0, length(uv - pPos6));
                
                // Particle 7
                float pAngle7 = 7 * 0.785398;
                float pDist7 = progress * (0.3 + hash(float2(7, 7)) * 0.3);
                float2 pPos7 = center + float2(cos(pAngle7), sin(pAngle7)) * pDist7;
                float pSize7 = 0.02 * (1.0 - progress);
                particles += smoothstep(pSize7, 0, length(uv - pPos7));
                
                // 组合
                float4 gold = float4(1, 0.85, 0.2, 1);
                float4 white = float4(1, 1, 0.9, 1);
                
                color += gold * ring * (1.0 - progress * 0.5);
                color += gold * 0.8 * ring2;
                color += lerp(gold, white, 0.5) * rays * 0.6;
                color += white * centerGlow;
                color += white * particles;
                
                color.a *= (1.0 - progress) * _Intensity;
                
                return color;
            }
            
            // Normal 效果 - 绿色波纹
            float4 normalEffect(float2 uv, float progress)
            {
                float4 color = float4(0, 0, 0, 0);
                float2 center = float2(0.5, 0.5);
                
                // 波纹
                float ringRadius = progress * 0.5;
                float ringWidth = 0.03 * (1.0 - progress * 0.5);
                float ring = drawRing(uv, center, ringRadius, ringWidth);
                
                // 中心渐隐
                float dist = length(uv - center);
                float fade = (1.0 - progress) * 0.3 / (dist * 5.0 + 0.1);
                
                float4 green = float4(0.3, 0.9, 0.4, 1);
                float4 white = float4(0.8, 1, 0.9, 1);
                
                color += green * ring;
                color += lerp(green, white, 0.3) * fade;
                
                color.a *= (1.0 - progress) * _Intensity * 0.8;
                
                return color;
            }
            
            // Miss 效果 - 红色破碎
            float4 missEffect(float2 uv, float progress)
            {
                float4 color = float4(0, 0, 0, 0);
                float2 center = float2(0.5, 0.5);
                
                float dist = length(uv - center);
                
                // 破碎碎片
                float2 p = uv - center;
                float angle = atan2(p.y, p.x);
                
                // 碎片效果
                float shatter = 0;
                
                // Fragment 0
                float fragAngle0 = hash(float2(0, 0)) * 6.28;
                float fragDist0 = progress * (0.1 + hash(float2(0, 0.5)) * 0.4);
                float2 fragPos0 = center + float2(cos(fragAngle0), sin(fragAngle0)) * fragDist0;
                float fragSize0 = 0.03 * (1.0 - progress * 0.5);
                shatter += smoothstep(fragSize0, 0, length(uv - fragPos0));
                
                // Fragment 1
                float fragAngle1 = hash(float2(1, 1)) * 6.28;
                float fragDist1 = progress * (0.1 + hash(float2(1, 1.5)) * 0.4);
                float2 fragPos1 = center + float2(cos(fragAngle1), sin(fragAngle1)) * fragDist1;
                float fragSize1 = 0.03 * (1.0 - progress * 0.5);
                shatter += smoothstep(fragSize1, 0, length(uv - fragPos1));
                
                // Fragment 2
                float fragAngle2 = hash(float2(2, 2)) * 6.28;
                float fragDist2 = progress * (0.1 + hash(float2(2, 2.5)) * 0.4);
                float2 fragPos2 = center + float2(cos(fragAngle2), sin(fragAngle2)) * fragDist2;
                float fragSize2 = 0.03 * (1.0 - progress * 0.5);
                shatter += smoothstep(fragSize2, 0, length(uv - fragPos2));
                
                // Fragment 3
                float fragAngle3 = hash(float2(3, 3)) * 6.28;
                float fragDist3 = progress * (0.1 + hash(float2(3, 3.5)) * 0.4);
                float2 fragPos3 = center + float2(cos(fragAngle3), sin(fragAngle3)) * fragDist3;
                float fragSize3 = 0.03 * (1.0 - progress * 0.5);
                shatter += smoothstep(fragSize3, 0, length(uv - fragPos3));
                
                // Fragment 4
                float fragAngle4 = hash(float2(4, 4)) * 6.28;
                float fragDist4 = progress * (0.1 + hash(float2(4, 4.5)) * 0.4);
                float2 fragPos4 = center + float2(cos(fragAngle4), sin(fragAngle4)) * fragDist4;
                float fragSize4 = 0.03 * (1.0 - progress * 0.5);
                shatter += smoothstep(fragSize4, 0, length(uv - fragPos4));
                
                // Fragment 5
                float fragAngle5 = hash(float2(5, 5)) * 6.28;
                float fragDist5 = progress * (0.1 + hash(float2(5, 5.5)) * 0.4);
                float2 fragPos5 = center + float2(cos(fragAngle5), sin(fragAngle5)) * fragDist5;
                float fragSize5 = 0.03 * (1.0 - progress * 0.5);
                shatter += smoothstep(fragSize5, 0, length(uv - fragPos5));
                
                // X 形状
                float xShape = 0;
                float xSize = 0.2 * (1.0 - progress * 0.3);
                float2 p1 = abs(p);
                float diag1 = abs(p1.x - p1.y);
                float diag2 = abs(p1.x + p1.y - 0.001);
                xShape = smoothstep(0.03, 0, min(diag1, diag2)) * smoothstep(xSize, 0, dist);
                
                float4 red = float4(0.9, 0.2, 0.2, 1);
                float4 darkRed = float4(0.5, 0.1, 0.1, 1);
                
                color += red * shatter;
                color += lerp(darkRed, red, 0.5) * xShape * (1.0 - progress);
                
                color.a *= (1.0 - progress) * _Intensity;
                
                return color;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float4 color;
                
                if (_EffectType < 0.5)
                {
                    // Perfect
                    color = perfectEffect(i.uv, _Progress);
                }
                else if (_EffectType < 1.5)
                {
                    // Normal
                    color = normalEffect(i.uv, _Progress);
                }
                else
                {
                    // Miss
                    color = missEffect(i.uv, _Progress);
                }
                
                return color;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}

