// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "HyperShaderTriplanarDissolveWithOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ShadowStrength("Shadow Strength", Range(0.0,1.0)) = 0.4
        //_Curvature ("_LightDir", Vector) = (0.5,0.5,1,1) 
        _Color("_Color", Color) = (1,1,1,1)
        _SelfShadowColor("_SelfShadowColor", Color) = (1,1,1,1)
        _SpecularColor("_SpecularColor", Color) = (1,1,1,1)
        _Glossiness("_Glossiness", float) = 0.5
        _Falloff("_Falloff", Range(0.0,1.0)) = 0
        _FalloffPower("_Falloff Power",float) = 1.0
        _FalloffColor("Falloff Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Range(0.0,1.0)) = 1
        //_GrayDependence("GrayDependence", Range(0.0,1.0)) = 0.0
        //_Shine("Shine", Range(0.0,1.0)) = 0
        _Red("Red", Range(0.0,1.0)) = 0
        //_Tutor("Tutor", Range(0.0,1.0)) = 0
        _OutlineColor("_OutlineColor",Color) = (1,1,1,1)
        _OutlineScale("_OutlineScale", float) = 0.05
		//_tX ("TranslateX", float) = 0
		//_tY ("TranslateY", float) = 0
		//_tZ ("TranslateZ", float) = 0
		////_sX ("ScaleX", float) = 1
		////_sY ("ScaleY", float) = 1
		////_sZ ("ScaleZ", float) = 1
		//_rX ("RotateX", float) = 0
		//_rY ("RotateY", float) = 0
		//_rZ ("RotateZ", float) = 0
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
            uniform fixed _OutlineScale, _Alpha;

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

                //o.pos = UnityObjectToClipPos(v.vertex * _OutlineScale);

                float4 pos = v.vertex;
                float4 dir = float4(sign(pos.x), sign(pos.y), sign(pos.z), 1) * _OutlineScale;
                o.pos = UnityObjectToClipPos(pos + dir);

                return o;
            }


            float4 frag(vertexOutput i) : COLOR
            {
                fixed4 c = _OutlineColor;
                c.a = pow(_Alpha,6);
                //fixed4 c1 = fixed4(c.rgb, _Alpha);
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
            uniform fixed4 _Color, _SelfShadowColor, _SpecularColor;
            float4 _Curvature;
            float _ShadowStrength;
            float _Alpha;
            float _Glossiness;
            float _Falloff;
            float _FalloffPower;
            //float4x4 _ObjectMatrix;
            
			//float _tX,_tY,_tZ;
			//float _sX,_sY,_sZ;
			//float _rX,_rY,_rZ;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FalloffColor;
            //float _GrayDependence;
            //float _Gray;
            //float _Shine;
            float _Red;
            //float _Tutor;
            //float _TutorGlobal;

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
                //float2 uv1 : TEXCOORD2;
                //float2 uv2 : TEXCOORD3;
                float3 normal : NORMAL;
                SHADOW_COORDS(1)
                //UNITY_FOG_COORDS(2)
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
                //UNITY_TRANSFER_FOG(o, o.pos);

                ////float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 worldPos = v.vertex;

    //          float4x4 translateMatrix = float4x4(1,	0,	0,	_tX,
				//							 		0,	1,	0,	_tY,
				//					  				0,	0,	1,	_tZ,
				//					  				0,	0,	0,	1);
	
				//float4x4 scaleMatrix 	= float4x4( 1,	0,	0,	0,
				//							 		0,	1,  0,	0,
				//				  					0,	0,	1,  0,
				//				  					0,	0,	0,	1);

				//float angleX = radians(_rX);
				//float c = cos(angleX);
				//float s = sin(angleX);
				//float4x4 rotateXMatrix	= float4x4(	1,	0,	0,	0,
				//							 		0,	c,	-s,	0,
				//				  					0,	s,	c,	0,
				//				  					0,	0,	0,	1);

				//float angleY = radians(_rY);
				//c = cos(angleY);
				//s = sin(angleY);
				//float4x4 rotateYMatrix	= float4x4(	c,	0,	s,	0,
				//							 		0,	1,	0,	0,
				//				  					-s,	0,	c,	0,
				//				  					0,	0,	0,	1);

				//float angleZ = radians(_rZ);
				//c = cos(angleZ);
				//s = sin(angleZ);
				//float4x4 rotateZMatrix	= float4x4(	c,	-s,	0,	0,
				//							 		s,	c,	0,	0,
				//				  					0,	0,	1,	0,
				//				  					0,	0,	0,	1);

  		//		float4 localVertexPos = v.vertex;

  		//		// NOTE: the order matters, try scaling first before translating, different results
  		//		float4 localTranslated = mul(translateMatrix,localVertexPos);
  		//		float4 localScaledTranslated = mul(localTranslated,scaleMatrix);
  		//		float4 localScaledTranslatedRotX = mul(localScaledTranslated,rotateXMatrix);
  		//		float4 localScaledTranslatedRotXY = mul(localScaledTranslatedRotX,rotateYMatrix);
  		//		float4 localScaledTranslatedRotXYZ = mul(localScaledTranslatedRotXY,rotateZMatrix);
				//float4 worldPos = localScaledTranslatedRotXYZ;//UnityObjectToClipPos(localScaledTranslatedRotXYZ);
  				

                //float4 worldPos = mul(transpose(_ObjectMatrix), v.vertex);
                //worldPos = UnityObjectToClipPos(localScaledTranslatedRotXYZ);


                //o.uv = TRANSFORM_TEX(worldPos.xz, _MainTex);
                //o.uv1 = TRANSFORM_TEX(worldPos.xy, _MainTex);
                //o.uv2 = TRANSFORM_TEX(worldPos.yz, _MainTex);
                o.normal = mul(unity_ObjectToWorld, v.normal);
                return o;
            }


            float4 frag(vertexOutput i) : COLOR
            {
                float atten = 1 - (1 - SHADOW_ATTENUATION(i)) * _ShadowStrength;
                fixed4 ctex = tex2D(_MainTex, i.uv);
                fixed4 c = ctex;
                //fixed4 texy = tex2D(_MainTex, i.uv1);
                //fixed4 texz = tex2D(_MainTex, i.uv1);
                //c = lerp(c, texz, clamp(abs(i.normal.x+1), 0, 1));
                //c = lerp(c, texy, clamp(abs(i.normal.z), 0, 1));
                
                //fixed newAlpha = pow(_Alpha + (1 - c.g), 5);
                //c = lerp(c, c*c, clamp((c.g*5)*(0.9-_Alpha),0,1));
                //c.a = clamp(round(newAlpha * _Alpha), 0, 1);

                c.g = c.r;
                c.b = c.r;
                c.rgb = c.rgb * i.col;
                //c = lerp(c, texz, clamp(abs(i.normal.x*1.5),0,1));
                //c = lerp(c, texy, clamp(abs(i.normal.z * 1.5), 0, 1));
                //fixed4 c = tex2D(_MainTex, i.uv) * i.col;
                c = lerp(c, c * UNITY_LIGHTMODEL_AMBIENT, 1 - atten);
                //UNITY_APPLY_FOG(i.fogCoord, c);
                //c.a = _Alpha + (_Alpha == 0.1 ? sin(_Time.w * 1.5) * 0.03 : 0.0);
                
                

                //c.rgb = lerp(c.rgb, (c.r + c.g + c.b) * 0.333, _Gray * _GrayDependence);
                //c.rgb = lerp(c.rgb, (c.r + c.g + c.b) * 0.333, (1.0 - _Tutor) * _TutorGlobal);

                //c.rgba = lerp(c.rgba, float4(1.0, 1.0, 1.0, 1.0), _Shine);
                //c.rgba = lerp(c.rgba, float4(0.1, 1.0, 0.0, c.a < 1 ? 0.5 : 1), _Shine);

                c.rgb = lerp(c.rgb, float3(1, 0, 0), _Red * 0.5);

                return c;
            }
            ENDCG
        }

       
       
    }
    Fallback "VertexLit"

}