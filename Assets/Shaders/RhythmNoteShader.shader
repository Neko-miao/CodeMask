// ================================================
// MaskSystem - 节奏音符着色器
// 程序化发光音符效果
// ================================================

Shader "MaskSystem/RhythmNote"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 0.3, 0.3, 1)
        _GlowColor ("Glow Color", Color) = (1, 0.5, 0.5, 1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        
        _PulseSpeed ("Pulse Speed", Range(0.5, 5.0)) = 2.0
        _PulseIntensity ("Pulse Intensity", Range(0.0, 1.0)) = 0.3
        _GlowRadius ("Glow Radius", Range(0.0, 0.5)) = 0.2
        _OutlineWidth ("Outline Width", Range(0.01, 0.1)) = 0.03
        
        _NoteType ("Note Type (0=Attack,1=Defense,2=Idle,3=Rage)", Range(0, 3)) = 0
        _InJudgeZone ("In Judge Zone", Range(0, 1)) = 0
        _JudgeResult ("Judge Result (0=None,1=Perfect,2=Normal,3=Miss)", Range(0, 3)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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
            float4 _GlowColor;
            float4 _OutlineColor;
            float _PulseSpeed;
            float _PulseIntensity;
            float _GlowRadius;
            float _OutlineWidth;
            float _NoteType;
            float _InJudgeZone;
            float _JudgeResult;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // 绘制基本形状
            float drawCircle(float2 uv, float2 center, float radius)
            {
                float dist = length(uv - center);
                return smoothstep(radius + 0.01, radius - 0.01, dist);
            }
            
            // 绘制圆环
            float drawRing(float2 uv, float2 center, float innerRadius, float outerRadius)
            {
                float dist = length(uv - center);
                float inner = smoothstep(innerRadius - 0.01, innerRadius + 0.01, dist);
                float outer = smoothstep(outerRadius + 0.01, outerRadius - 0.01, dist);
                return inner * outer;
            }
            
            // 绘制菱形
            float drawDiamond(float2 uv, float2 center, float size)
            {
                float2 p = abs(uv - center);
                float dist = p.x + p.y;
                return smoothstep(size + 0.02, size - 0.02, dist);
            }
            
            // 绘制三角形
            float drawTriangle(float2 uv, float2 center, float size)
            {
                float2 p = uv - center;
                p.y -= size * 0.2;
                
                float2 n1 = normalize(float2(-1, -0.577));
                float2 n2 = normalize(float2(1, -0.577));
                float2 n3 = normalize(float2(0, 1.155));
                
                float d1 = dot(p, n1);
                float d2 = dot(p, n2);
                float d3 = dot(p, n3);
                
                float maxDist = max(max(d1, d2), d3);
                return smoothstep(size * 0.6 + 0.02, size * 0.6 - 0.02, maxDist);
            }
            
            // 绘制星形
            float drawStar(float2 uv, float2 center, float size)
            {
                float2 p = uv - center;
                float angle = atan2(p.y, p.x);
                float dist = length(p);
                
                // 5角星
                float star = abs(fmod(angle + 3.14159, 1.2566) - 0.6283);
                float radius = size * (0.4 + 0.2 * star);
                
                return smoothstep(radius + 0.02, radius - 0.02, dist);
            }
            
            // 发光效果
            float glow(float2 uv, float2 center, float radius, float intensity)
            {
                float dist = length(uv - center);
                return intensity / (dist * dist * 20.0 + 1.0) * smoothstep(radius * 2.0, 0, dist);
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5; // Center UV
                float time = _Time.y;
                
                // 脉动效果
                float pulse = 1.0 + sin(time * _PulseSpeed) * _PulseIntensity;
                
                // 判定区内时增强脉动
                if (_InJudgeZone > 0.5)
                {
                    pulse = 1.0 + sin(time * _PulseSpeed * 2.0) * _PulseIntensity * 2.0;
                }
                
                float4 color = float4(0, 0, 0, 0);
                float baseSize = 0.3;
                
                // 根据音符类型绘制不同形状
                float shape = 0;
                float4 mainCol = _MainColor;
                float4 glowCol = _GlowColor;
                
                // Attack - 圆形 (红色)
                if (_NoteType < 0.5)
                {
                    shape = drawCircle(uv, float2(0, 0), baseSize * pulse);
                    mainCol = float4(1, 0.3, 0.3, 1);
                    glowCol = float4(1, 0.5, 0.3, 1);
                }
                // Defense - 菱形 (蓝色)
                else if (_NoteType < 1.5)
                {
                    shape = drawDiamond(uv, float2(0, 0), baseSize * pulse);
                    mainCol = float4(0.3, 0.5, 1, 1);
                    glowCol = float4(0.5, 0.7, 1, 1);
                }
                // Idle - 小圆 (灰色)
                else if (_NoteType < 2.5)
                {
                    shape = drawCircle(uv, float2(0, 0), baseSize * 0.5 * pulse);
                    mainCol = float4(0.5, 0.5, 0.5, 0.5);
                    glowCol = float4(0.6, 0.6, 0.6, 0.3);
                }
                // Rage - 星形 (橙色)
                else
                {
                    shape = drawStar(uv, float2(0, 0), baseSize * pulse);
                    mainCol = float4(1, 0.5, 0, 1);
                    glowCol = float4(1, 0.7, 0.2, 1);
                    
                    // 狂暴音符额外火焰效果
                    float flame = sin(uv.x * 20.0 + time * 5.0) * 0.02;
                    flame += sin(uv.y * 15.0 + time * 3.0) * 0.02;
                    shape += drawStar(uv + float2(flame, flame * 0.5), float2(0, 0), baseSize * pulse * 1.1) * 0.3;
                }
                
                // 外轮廓
                float outline = 0;
                if (_NoteType < 0.5)
                    outline = drawRing(uv, float2(0, 0), baseSize * pulse - _OutlineWidth, baseSize * pulse);
                else if (_NoteType < 1.5)
                    outline = drawDiamond(uv, float2(0, 0), baseSize * pulse + _OutlineWidth) - drawDiamond(uv, float2(0, 0), baseSize * pulse - _OutlineWidth);
                else if (_NoteType < 2.5)
                    outline = drawRing(uv, float2(0, 0), baseSize * 0.5 * pulse - _OutlineWidth * 0.5, baseSize * 0.5 * pulse);
                else
                    outline = 0; // 星形不画轮廓
                
                // 发光
                float glowEffect = glow(uv, float2(0, 0), baseSize, _GlowRadius * pulse);
                
                // 组合颜色
                color = mainCol * shape;
                color += _OutlineColor * max(0, outline);
                color += glowCol * glowEffect;
                
                // 判定结果特效
                if (_JudgeResult > 0.5)
                {
                    float resultPulse = 1.0 + sin(time * 10.0) * 0.5;
                    float4 resultColor;
                    
                    if (_JudgeResult < 1.5) // Perfect
                    {
                        resultColor = float4(1, 0.9, 0.2, 1);
                        // 金色爆发效果
                        float burst = drawRing(uv, float2(0, 0), baseSize * resultPulse * 1.5, baseSize * resultPulse * 1.7);
                        color += resultColor * burst * 0.8;
                    }
                    else if (_JudgeResult < 2.5) // Normal
                    {
                        resultColor = float4(0.3, 0.8, 0.3, 1);
                        float burst = drawRing(uv, float2(0, 0), baseSize * resultPulse * 1.2, baseSize * resultPulse * 1.4);
                        color += resultColor * burst * 0.5;
                    }
                    else // Miss
                    {
                        resultColor = float4(0.8, 0.2, 0.2, 1);
                        // 红色破碎效果
                        float crack = abs(sin(atan2(uv.y, uv.x) * 8.0 + time * 2.0)) * 0.5;
                        color = lerp(color, resultColor, crack * shape);
                    }
                }
                
                // 判定区高亮
                if (_InJudgeZone > 0.5)
                {
                    float highlight = sin(time * 8.0) * 0.2 + 0.8;
                    color.rgb *= highlight;
                    color.a = min(1, color.a * 1.2);
                }
                
                return color;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}

