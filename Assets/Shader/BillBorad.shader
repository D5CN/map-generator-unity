Shader "D5Power/BillBoard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
		Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
		#pragma vertex vert  
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			// get the camera basis vectors
			float3 forward = -normalize(UNITY_MATRIX_V._m20_m21_m22);
			float3 up = float3(0, 1, 0); //normalize(UNITY_MATRIX_V._m10_m11_m12);
			float3 right = normalize(UNITY_MATRIX_V._m00_m01_m02);
			
			// rotate to face camera
			float4x4 rotationMatrix = float4x4(right,   0,
												up,      0,
												forward, 0,
												0, 0, 0, 1);
			
			//float offset = _Object2World._m22 / 2;
			float offset = 0;
			v.vertex = mul(v.vertex + float4(0, offset, 0, 0), rotationMatrix) + float4(0, -offset, 0, 0);
			v.normal = mul(v.normal, rotationMatrix);
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
