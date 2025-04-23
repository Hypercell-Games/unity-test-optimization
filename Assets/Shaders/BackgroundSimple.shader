// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SkyboxSimple"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Color01 ("_Color01", Color) = (1,1,1,1)
        _Color02 ("_Color02", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "PreviewType" = "Skybox"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 scrPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            uniform fixed4 _Color01, _Color02;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.scrPos = ComputeScreenPos(o.vertex);
                return o;

            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 screenPos = i.scrPos.xy / i.scrPos.w;
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = lerp(_Color01, _Color02, screenPos.y);
                return col;
            }
            ENDCG
        }
    }
}
