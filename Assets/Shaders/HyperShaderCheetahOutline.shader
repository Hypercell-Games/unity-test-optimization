Shader "HyperShaderCheetahOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ShadowStrength("Shadow Strength", Range(0.0,1.0)) = 0.4
        //_Curvature ("_LightDir", Vector) = (0.5,0.5,1,1) 
        _Color("_Color", Color) = (1,1,1,1)
            _Color01("_Color01", Color) = (1,1,1,1)
        _SelfShadowColor("_SelfShadowColor", Color) = (1,1,1,1)
        _SpecularColor("_SpecularColor", Color) = (1,1,1,1)
        _Glossiness("_Glossiness", float) = 0.5
        _Falloff("_Falloff", Range(0.0,1.0)) = 0
        _FalloffPower("_Falloff Power",float) = 1.0
        _FalloffColor("Falloff Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Float) = 1
        _GrayDependence("GrayDependence", Range(0.0,1.0)) = 0.0
        _Shine("Shine", Range(0.0,1.0)) = 0
        _Red("Red", Range(0.0,1.0)) = 0
        _Tutor("Tutor", Range(0.0,1.0)) = 0

            _OutlineColor("_OutlineColor",Color) = (1,1,1,1)
        _OutlineThickness("_OutlineThickness", float) = 0.27
    }
    SubShader
    {
            Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            ZWrite On
            Offset 1, 1

            Tags
            {
                "LightMode" = "ForwardBase"
            }
            // make sure that all uniforms are correctly set

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"


            #pragma multi_compile_fwdbase
            #include "AutoLight.cginc"

            uniform fixed4 _OutlineColor;
            uniform fixed _OutlineThickness;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float color : COLOR;
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;


            };


            vertexOutput vert(vertexInput v)
            {
                vertexOutput o;

                v.vertex.xyz += v.normal * _OutlineThickness;

                o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }


            float4 frag(vertexOutput i) : COLOR
            {
                fixed4 c = _OutlineColor;

                return c;
            }
            ENDCG
        }

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite On

            Tags
            {
                "LightMode" = "ForwardBase"
            }
            // make sure that all uniforms are correctly set

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"


            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #include "AutoLight.cginc"

            uniform float4 _LightColor0;
            uniform fixed4 _Color, _Color01, _SelfShadowColor, _SpecularColor;
            float4 _Curvature;
            float _ShadowStrength;
            float _Alpha;
            float _Glossiness;
            float _Falloff;
            float _FalloffPower;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FalloffColor;
            float _GrayDependence;
            float _Gray;
            float _Shine;
            float _Red;
            float _Tutor;
            float _TutorGlobal;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float color : COLOR;
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1)
                UNITY_FOG_COORDS(2)
            };


            vertexOutput vert(vertexInput v)
            {
                vertexOutput o;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float3 normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(WorldSpaceViewDir(v.vertex));

                float diff = 1 - dot(normal, lightDir);
                //float verticalAmbient =  saturate(lerp(1,  1 + (normal.y * 0.5), _Curvature.z ));
                //fixed3 colored = lerp(_ShadowColor, _Diffuse * _LightColor0, diff) * verticalAmbient;
                //float fakeShadow = 1 - normalize(dot(lightDir, normal));
                //fakeShadow *= 2;
                fixed3 colored = _Color;
                colored = lerp(colored, colored * _SelfShadowColor.xyz, diff * _SelfShadowColor.a);

                // spec start

                //float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
                float3 specularDirection = normalize(lightDir + viewDir);
                // The amount of light that gets reflected.
                float NdotS = saturate(dot(normal, specularDirection));
                float intensity = pow(NdotS, _Glossiness);
                float4 specular = intensity * _SpecularColor;
                //specular *= _LightColor0;

                //Fresnel (falloff)
                float fresnel = (1 - dot(normal, viewDir)) * _Falloff;
                fresnel = pow(fresnel, _FalloffPower);


                o.col = float4(colored, 1.0) + specular;
                o.col = lerp(o.col, _FalloffColor, fresnel);
                o.pos = UnityObjectToClipPos(v.vertex);
                //spec end

                //o.col = float4(colored, 1.0);
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }


            float4 frag(vertexOutput i) : COLOR
            {
                float atten = 1 - (1 - SHADOW_ATTENUATION(i)) * _ShadowStrength;
                fixed4 c = i.col;
                c = lerp(c, c * _Color01, 1-tex2D(_MainTex, i.uv).r);
                c = lerp(c, c * UNITY_LIGHTMODEL_AMBIENT, 1 - atten);
                UNITY_APPLY_FOG(i.fogCoord, c);
                //c.a = _Alpha + (_Alpha == 0.1 ? sin(_Time.w * 1.5) * 0.03 : 0.0);
                c.a = _Alpha;

                c.rgb = lerp(c.rgb, (c.r + c.g + c.b) * 0.333, _Gray * _GrayDependence);
                c.rgb = lerp(c.rgb, (c.r + c.g + c.b) * 0.333, (1.0 - _Tutor) * _TutorGlobal);

                //c.rgba = lerp(c.rgba, float4(1.0, 1.0, 1.0, 1.0), _Shine);
                c.rgba = lerp(c.rgba, float4(0.1, 1.0, 0.0, c.a < 1 ? 0.5 : 1), _Shine);

                c.rgb = lerp(c.rgb, float3(1, 0, 0), _Red * 0.5);

                return c;
            }
            ENDCG
        }
    }
    Fallback "VertexLit"

}