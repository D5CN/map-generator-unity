Shader "D5Power/BillBoard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _Speed ("MoveSpeed", Range(20,50)) = 25 // speed of the swaying
        _Rigidness("Rigidness", Range(1,50)) = 25 // lower makes it look more "liquid" higher makes it look rigid
        _SwayMax("Sway Max", Range(0, 0.1)) = .005 // how far the swaying goes
        _YOffset("Y offset", float) = 0.0// y offset, below this is no animation
        _MaxWidth("Max Displacement Width", Range(0, 2)) = 0.1 // width of the line around the dissolve
        _Radius("Radius", Range(0,5)) = 1 // width of the line around the dissolve
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

        float _Radius;
        float _Speed;
        float _SwayMax;
        float _YOffset;
        float _Rigidness;
        float _MaxWidth;
        uniform float3 _Positions[100];
        uniform float _PositionArray;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

            float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;// world position
            float x = sin(wpos.x / _Rigidness + (_Time.x * _Speed)) *(v.vertex.y - _YOffset) * 5;// x axis movements
            float z = sin(wpos.z / _Rigidness + (_Time.x * _Speed)) *(v.vertex.y - _YOffset) * 5;// z axis movements
            v.vertex.x += (step(0,v.vertex.y - _YOffset) * x * _SwayMax);// apply the movement if the vertex's y above the YOffset
            v.vertex.z += (step(0,v.vertex.y - _YOffset) * z * _SwayMax);
            
            // interaction radius movement for every position in array
            for  (int i = 0; i < _PositionArray; i++){
                    float3 dis =  distance(_Positions[i], wpos); // distance for radius
                    float3 radius = 1-  saturate(dis /_Radius); // in world radius based on objects interaction radius
                    float3 sphereDisp = wpos - _Positions[i]; // position comparison
                    sphereDisp *= radius; // position multiplied by radius for falloff
                    v.vertex.xz += clamp(sphereDisp.xz * step(_YOffset, v.vertex.y), -_MaxWidth,_MaxWidth);// vertex movement based on falloff and clamped
                }
			
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
