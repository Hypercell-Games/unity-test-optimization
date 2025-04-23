Shader "Custom/IceShaderOutline"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
            _FresnelColor ("FresnelColor", Color) = (1,1,1,1)
            _Fresnel ("Fresnel", Range(0,1)) = 0
            _FresnelPower ("FresnelPower", float) = 1

            _OutlineColor("_OutlineColor",Color) = (1,1,1,1)
        _OutlineThickness("_OutlineThickness", float) = 0.27
    }
    SubShader
    {
             Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front

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
            

       Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        CGPROGRAM
                // Physically based Standard lighting model, and enable shadows on all light types
                #pragma surface surf Standard fullforwardshadows alpha

                // Use shader model 3.0 target, to get nicer looking lighting
                #pragma target 3.0

                sampler2D _MainTex;

                struct Input
                {
                    float2 uv_MainTex;
                    float3 worldNormal;
                    float3 viewDir;
                };

                half _Glossiness, _Metallic, _Fresnel, _FresnelPower;
                fixed4 _FresnelColor;
                fixed4 _Color;

                // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
                // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
                // #pragma instancing_options assumeuniformscaling
                UNITY_INSTANCING_BUFFER_START(Props)
                    // put more per-instance properties here
                UNITY_INSTANCING_BUFFER_END(Props)

                void surf(Input IN, inout SurfaceOutputStandard o)
                {
                    // Albedo comes from a texture tinted by color
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    fixed fresnel = 1 - dot(IN.worldNormal, IN.viewDir);
                    fresnel = pow(fresnel, _FresnelPower) * 2;
                    fresnel *= _Fresnel;

                    o.Albedo = lerp(c.rgb, _FresnelColor.rgb, fresnel);
                    // Metallic and smoothness come from slider variables
                    o.Metallic = _Metallic;
                    o.Smoothness = _Glossiness;
                    o.Alpha = clamp(c.a + pow(fresnel,2),0,1);
                    //o.Alpha = 1;
                    //o.Albedo = tangentNormal;
                    o.Emission = clamp(c * 0.5 + _FresnelColor * fresnel * 0.5,0,1);
                }
                ENDCG
                    
                    
    }
    
    FallBack "VertexLit"
}
