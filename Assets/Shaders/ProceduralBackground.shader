Shader "MaskSystem/ProceduralBackground"
{
    Properties
    {
        _ScrollSpeed ("Scroll Speed", Range(0.1, 5.0)) = 1.0
        _Theme ("Theme (0=Forest, 1=Ocean, 2=Sky)", Range(0, 2)) = 0
        
        // Theme Colors
        _SkyColorTop ("Sky Color Top", Color) = (0.1, 0.2, 0.4, 1)
        _SkyColorBottom ("Sky Color Bottom", Color) = (0.3, 0.5, 0.3, 1)
        _FarColor ("Far Layer Color", Color) = (0.2, 0.3, 0.2, 1)
        _MidColor ("Mid Layer Color", Color) = (0.15, 0.25, 0.15, 1)
        _NearColor ("Near Layer Color", Color) = (0.1, 0.2, 0.1, 1)
        _AccentColor ("Accent Color", Color) = (1, 0.9, 0.5, 1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100
        
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
            
            float _ScrollSpeed;
            float _Theme;
            float4 _SkyColorTop;
            float4 _SkyColorBottom;
            float4 _FarColor;
            float4 _MidColor;
            float4 _NearColor;
            float4 _AccentColor;
            
            // ============================================
            // Noise Functions (Shadertoy style)
            // ============================================
            
            // Hash function for noise
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            
            float hash2(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            // Value noise
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }
            
            // Fractal Brownian Motion (fixed 4 octaves)
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                // Unrolled loop for better compatibility
                value += amplitude * noise(p * frequency);
                amplitude *= 0.5; frequency *= 2.0;
                value += amplitude * noise(p * frequency);
                amplitude *= 0.5; frequency *= 2.0;
                value += amplitude * noise(p * frequency);
                amplitude *= 0.5; frequency *= 2.0;
                value += amplitude * noise(p * frequency);
                
                return value;
            }
            
            // Voronoi pattern for stars/particles
            float voronoi(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float minDist = 1.0;
                
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cellPt = hash2(i + neighbor) * 0.5 + 0.25;
                        float2 diff = neighbor + cellPt - f;
                        float dist = length(diff);
                        minDist = min(minDist, dist);
                    }
                }
                
                return minDist;
            }
            
            // ============================================
            // Layer Effects
            // ============================================
            
            // Stars/particles layer
            float stars(float2 uv, float density, float brightness)
            {
                float2 p = uv * density;
                float v = voronoi(p);
                float star = smoothstep(0.1, 0.0, v);
                float twinkle = sin(_Time.y * 3.0 + hash(floor(p)) * 6.28) * 0.5 + 0.5;
                return star * brightness * (0.5 + 0.5 * twinkle);
            }
            
            // Cloud/mountain silhouette
            float silhouette(float2 uv, float height, float detail)
            {
                float n = fbm(float2(uv.x * detail, 0.0));
                float edgeLine = height + n * 0.3;
                return smoothstep(edgeLine + 0.02, edgeLine - 0.02, uv.y);
            }
            
            // Bubbles (for ocean) - unrolled
            float bubbles(float2 uv, float time)
            {
                float2 p = uv * 15.0;
                p.y -= time * 2.0;
                
                float bubble = 0.0;
                float2 offset, bp;
                float size, b;
                
                // Bubble 0
                offset = float2(hash(float2(0, 0)) * 10.0, hash(float2(0, 0)) * 10.0);
                bp = frac(p + offset) - 0.5;
                size = 0.05 + hash(float2(0, 0)) * 0.1;
                b = smoothstep(size, size - 0.02, length(bp));
                bubble = max(bubble, b * 0.3);
                
                // Bubble 1
                offset = float2(hash(float2(1, 0)) * 10.0, hash(float2(0, 1)) * 10.0);
                bp = frac(p + offset) - 0.5;
                size = 0.05 + hash(float2(1, 1)) * 0.1;
                b = smoothstep(size, size - 0.02, length(bp));
                bubble = max(bubble, b * 0.3);
                
                // Bubble 2
                offset = float2(hash(float2(2, 0)) * 10.0, hash(float2(0, 2)) * 10.0);
                bp = frac(p + offset) - 0.5;
                size = 0.05 + hash(float2(2, 2)) * 0.1;
                b = smoothstep(size, size - 0.02, length(bp));
                bubble = max(bubble, b * 0.3);
                
                return bubble;
            }
            
            // Light rays (for underwater/sky)
            float lightRays(float2 uv, float time)
            {
                float2 p = uv;
                p.x += sin(p.y * 3.0 + time) * 0.1;
                float rays = sin(p.x * 8.0 + time * 0.5) * 0.5 + 0.5;
                rays = pow(rays, 3.0);
                float fade = smoothstep(0.0, 1.0, uv.y);
                return rays * fade * 0.15;
            }
            
            // Ground/road texture
            float groundPattern(float2 uv, float time)
            {
                float2 p = uv;
                p.x += time;
                
                // Horizontal lines (road/path)
                float lines = sin(p.x * 20.0) * 0.5 + 0.5;
                lines = smoothstep(0.4, 0.6, lines) * 0.3;
                
                // Noise texture
                float tex = fbm(p * 10.0) * 0.2;
                
                return lines + tex;
            }
            
            // Leaves/particles falling - simplified
            float fallingParticles(float2 uv, float time, float density)
            {
                float particles = 0.0;
                float speed;
                float2 p, id, f;
                float size, particle;
                
                // Particle 0
                speed = 0.5 + hash(float2(0, 0)) * 0.5;
                p = uv * density;
                p.x += sin(p.y * 2.0 + time) * 0.3;
                p.y += time * speed;
                p.x += hash(float2(0, 0)) * 10.0;
                id = floor(p);
                f = frac(p) - 0.5;
                size = 0.1 + hash(id) * 0.15;
                particle = smoothstep(size, size - 0.05, length(f));
                particles += particle * 0.2;
                
                // Particle 1
                speed = 0.5 + hash(float2(1, 0)) * 0.5;
                p = uv * density;
                p.x += sin(p.y * 2.0 + time + 1) * 0.3;
                p.y += time * speed;
                p.x += hash(float2(1, 1)) * 10.0;
                id = floor(p);
                f = frac(p) - 0.5;
                size = 0.1 + hash(id) * 0.15;
                particle = smoothstep(size, size - 0.05, length(f));
                particles += particle * 0.2;
                
                // Particle 2
                speed = 0.5 + hash(float2(2, 0)) * 0.5;
                p = uv * density;
                p.x += sin(p.y * 2.0 + time + 2) * 0.3;
                p.y += time * speed;
                p.x += hash(float2(2, 2)) * 10.0;
                id = floor(p);
                f = frac(p) - 0.5;
                size = 0.1 + hash(id) * 0.15;
                particle = smoothstep(size, size - 0.05, length(f));
                particles += particle * 0.2;
                
                return particles;
            }
            
            // ============================================
            // Main Vertex/Fragment
            // ============================================
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _ScrollSpeed;
                
                // Sky gradient
                float3 col = lerp(_SkyColorBottom.rgb, _SkyColorTop.rgb, uv.y);
                
                // ============================================
                // Theme-specific effects
                // ============================================
                
                if (_Theme < 0.5)
                {
                    // === FOREST THEME ===
                    
                    // Distant mountains (very slow)
                    float2 farUV = float2(uv.x + time * 0.05, uv.y);
                    float farMountain = silhouette(farUV, 0.5, 2.0);
                    col = lerp(col, _FarColor.rgb, farMountain * 0.7);
                    
                    // Mid-distance trees (medium speed)
                    float2 midUV = float2(uv.x + time * 0.15, uv.y);
                    float midTrees = silhouette(midUV, 0.35, 4.0);
                    col = lerp(col, _MidColor.rgb, midTrees * 0.8);
                    
                    // Near trees (fast)
                    float2 nearUV = float2(uv.x + time * 0.4, uv.y);
                    float nearTrees = silhouette(nearUV, 0.15, 8.0);
                    col = lerp(col, _NearColor.rgb, nearTrees * 0.9);
                    
                    // Ground
                    float ground = smoothstep(0.12, 0.08, uv.y);
                    float groundTex = groundPattern(uv, time * 0.5);
                    col = lerp(col, _NearColor.rgb * (0.5 + groundTex), ground);
                    
                    // Falling leaves
                    float leaves = fallingParticles(uv, time * 0.3, 8.0);
                    col += _AccentColor.rgb * leaves;
                    
                    // Sun glow
                    float2 sunPos = float2(0.8, 0.85);
                    float sun = smoothstep(0.15, 0.0, length(uv - sunPos));
                    col += _AccentColor.rgb * sun * 0.5;
                }
                else if (_Theme < 1.5)
                {
                    // === OCEAN THEME ===
                    
                    // Deep water gradient overlay
                    float depth = 1.0 - uv.y;
                    col = lerp(col, col * 0.3, depth * 0.5);
                    
                    // Caustic light rays from above
                    float rays = lightRays(uv, time);
                    col += _AccentColor.rgb * rays;
                    
                    // Distant seaweed/coral (slow)
                    float2 farUV = float2(uv.x + time * 0.03, uv.y);
                    float farCoral = silhouette(farUV, 0.6, 3.0);
                    col = lerp(col, _FarColor.rgb, farCoral * 0.5);
                    
                    // Mid seaweed (medium)
                    float2 midUV = float2(uv.x + time * 0.1, uv.y);
                    float midSeaweed = silhouette(midUV, 0.4, 6.0);
                    float sway = sin(time * 2.0 + midUV.x * 5.0) * 0.02;
                    col = lerp(col, _MidColor.rgb, midSeaweed * 0.6);
                    
                    // Near rocks (fast)
                    float2 nearUV = float2(uv.x + time * 0.25, uv.y);
                    float nearRocks = silhouette(nearUV, 0.15, 10.0);
                    col = lerp(col, _NearColor.rgb, nearRocks * 0.8);
                    
                    // Sandy bottom
                    float sand = smoothstep(0.1, 0.05, uv.y);
                    float sandTex = fbm(float2(uv.x + time * 0.2, uv.y) * 20.0) * 0.3;
                    col = lerp(col, _NearColor.rgb * (0.7 + sandTex), sand);
                    
                    // Bubbles
                    float bubble = bubbles(uv, time);
                    col += float3(0.5, 0.7, 1.0) * bubble;
                    
                    // Floating particles
                    float particles = fallingParticles(float2(uv.x, 1.0 - uv.y), time * 0.2, 6.0);
                    col += float3(0.3, 0.5, 0.7) * particles * 0.5;
                }
                else
                {
                    // === SKY THEME ===
                    
                    // Stars
                    float starField = stars(uv, 50.0, 0.8);
                    col += float3(1.0, 1.0, 0.9) * starField;
                    
                    // Distant clouds (very slow)
                    float2 farUV = float2(uv.x + time * 0.02, uv.y);
                    float farClouds = fbm(farUV * 3.0);
                    farClouds = smoothstep(0.4, 0.6, farClouds);
                    col = lerp(col, _FarColor.rgb, farClouds * 0.3);
                    
                    // Mid clouds (medium)
                    float2 midUV = float2(uv.x + time * 0.08, uv.y);
                    float midClouds = fbm(midUV * 5.0);
                    midClouds = smoothstep(0.45, 0.65, midClouds);
                    col = lerp(col, _MidColor.rgb, midClouds * 0.5);
                    
                    // Near clouds (fast)
                    float2 nearUV = float2(uv.x + time * 0.2, uv.y);
                    float nearClouds = fbm(nearUV * 8.0);
                    nearClouds = smoothstep(0.5, 0.7, nearClouds);
                    col = lerp(col, _NearColor.rgb, nearClouds * 0.4);
                    
                    // Cloud floor
                    float floor = smoothstep(0.2, 0.1, uv.y);
                    float floorTex = fbm(float2(uv.x + time * 0.15, uv.y) * 10.0);
                    col = lerp(col, _NearColor.rgb * (0.8 + floorTex * 0.4), floor);
                    
                    // Sun/light source with god rays
                    float2 lightPos = float2(0.7, 0.9);
                    float lightDist = length(uv - lightPos);
                    float2 toLight = normalize(uv - lightPos);
                    float godRays = 0.0;
                    // Unrolled god rays (8 directions)
                    float baseAngle = time * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle), sin(baseAngle)))), 8.0) * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle + 0.785), sin(baseAngle + 0.785)))), 8.0) * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle + 1.57), sin(baseAngle + 1.57)))), 8.0) * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle + 2.355), sin(baseAngle + 2.355)))), 8.0) * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle + 3.14), sin(baseAngle + 3.14)))), 8.0) * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle + 3.925), sin(baseAngle + 3.925)))), 8.0) * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle + 4.71), sin(baseAngle + 4.71)))), 8.0) * 0.1;
                    godRays += pow(max(0.0, dot(toLight, float2(cos(baseAngle + 5.495), sin(baseAngle + 5.495)))), 8.0) * 0.1;
                    float glow = smoothstep(0.3, 0.0, lightDist);
                    col += _AccentColor.rgb * (glow * 0.6 + godRays * smoothstep(0.5, 0.0, lightDist));
                    
                    // Sparkles/magic particles
                    float sparkles = stars(uv + float2(time * 0.1, 0), 30.0, 0.5);
                    col += _AccentColor.rgb * sparkles * 0.5;
                }
                
                // Vignette effect
                float2 vignetteUV = uv * (1.0 - uv);
                float vignette = vignetteUV.x * vignetteUV.y * 15.0;
                vignette = pow(saturate(vignette), 0.25);
                col *= vignette * 0.3 + 0.7;
                
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
    
    Fallback "Unlit/Color"
}

