Shader "Custom/GlassyShaderWithOutline"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Emission ("Emission", Range(0,10)) = 1
        _Parallax ("Parallax", Float) = 0.1
        _OutlineColor("_OutlineColor",Color) = (1,1,1,1)
        _OutlineThickness("_OutlineThickness", float) = 0.05
        _Red("Red", Range(0.0,1.0)) = 0
    }
    SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}
        Cull Back
        ZWrite On
        LOD 200

       

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma multi_compile_instancing

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
        };

        half _Glossiness;
        half _Metallic;
        half _Emission;
        half _Parallax;
        fixed4 _Color;
        float _Red;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            
            fixed4 colored = _Color;
            fixed2 parallaxOffset = IN.viewDir * _Parallax;
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex+parallaxOffset);
            fixed2 parallaxOffset01 = IN.viewDir * _Parallax*-2;
            fixed4 tex01 = tex2D(_MainTex, IN.uv_MainTex + parallaxOffset01+ fixed2(0,0.5));
            tex *= tex01 * 3*colored;
            tex = clamp(tex, 0.01, 1);

                fixed4 c =  tex+colored;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
            //o.Albedo += tex.rgb * _Emission;
            float fresnel = clamp(pow((1 - dot(IN.worldNormal, IN.viewDir)) * 1.1, 3),0,1);
            
            //o.Albedo *= 10;
            o.Albedo += fresnel;
            o.Albedo = lerp(o.Albedo, colored, 1-fresnel);           
            o.Albedo.rgb = lerp(o.Albedo.rgb, float3(1, 0, 0), _Red * 0.5);

            //o.Alpha += fresnel+0.5;
            //o.Emission = tex01*_Color*0.5;
            o.Emission = o.Albedo*0.5 * colored +tex*0.5;
            o.Emission = clamp(o.Emission, 0, 0.8);
            //o.Emission = clamp(o.Emission, 0, 0.8);
            //o.Emission *= 1.5;
            //o.Alpha = clamp(o.Alpha, 0, 1);
        }
        ENDCG

         Pass{
            Cull Front
            ZWrite On
            Offset 1, 1

            CGPROGRAM

            //include useful shader functions
            #include "UnityCG.cginc"

            //define vertex and fragment shader
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            uniform fixed4 _OutlineColor;
            uniform fixed _OutlineThickness;

            struct appdata{
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f{
                float4 position : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);                
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.position = UnityObjectToClipPos(v.vertex + normalize(v.normal) * _OutlineThickness);
                return o;
            }

            //the fragment shader
            fixed4 frag(v2f i) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(i);
                return _OutlineColor;
            }

            ENDCG
        }
        
    }
    FallBack "Diffuse"
}
