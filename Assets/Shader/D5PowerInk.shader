Shader "D5Power/D5PowerInk"
{
	Properties
	{
		//原始画面
		_MainTex ("Texture", 2D) = "white" {}
	    //高斯模糊画面
	    _BlurTex("Blur",2D) = "white"{}
		//水墨画面
		_PaintTex("PaintTex",2D)="white"{}
	}
	CGINCLUDE
    #include "UnityCG.cginc"
	//深度法线图
	sampler2D _CameraDepthNormalsTexture;
    sampler2D _MainTex;
	sampler2D _BlurTex;
	sampler2D _PaintTex;
	sampler2D _NoiseTex;
	float4 _BlurTex_TexelSize;
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;
	float4 _PaintTex_TexelSize;
	float4 _offsets;
	float _EdgeWidth;
	float _Sensitive;
	int _PaintFactor;
	
	//取灰度
	float luminance(fixed3 color) {
		return 0.2125*color.r + 0.7154*color.g + 0.0721*color.b;
	}
	//高斯模糊部分
	struct v2f_blur
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float4 uv01:TEXCOORD1;
		float4 uv23:TEXCOORD2;
		float4 uv45:TEXCOORD3;
	};

	v2f_blur vert_blur(appdata_img v)
	{
		v2f_blur o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		_offsets *= _MainTex_TexelSize.xyxy;
		o.uv01 = v.texcoord.xyxy + _offsets.xyxy*float4(1, 1, -1, -1);
		o.uv23 = v.texcoord.xyxy + _offsets.xyxy*float4(1, 1, -1, -1)*2.0;
		o.uv45 = v.texcoord.xyxy + _offsets.xyxy*float4(1, 1, -1, -1)*3.0;
		return o;
	}

	float4 frag_blur(v2f_blur i) : SV_Target
	{
		float4 color = float4(0,0,0,0);
		color += 0.40*tex2D(_MainTex, i.uv);
		color += 0.15*tex2D(_MainTex, i.uv01.xy);
		color += 0.15*tex2D(_MainTex, i.uv01.zw);
		color += 0.10*tex2D(_MainTex, i.uv23.xy);
		color += 0.10*tex2D(_MainTex, i.uv23.zw);
		color += 0.05*tex2D(_MainTex, i.uv45.xy);
		color += 0.05*tex2D(_MainTex, i.uv45.zw);
		return color;
	}
	//边缘检测部分
	struct v2f_edge{
		float2 uv:TEXCOORD0;
		float4 vertex:SV_POSITION;
	};

	v2f_edge vert_edge(appdata_img v){
		v2f_edge o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		return o;
	}

	float4 frag_edge(v2f_edge i):SV_Target{
		//噪声
		float n = tex2D(_NoiseTex,i.uv).r;
		float3 col0 = tex2D(_CameraDepthNormalsTexture, i.uv + _EdgeWidth * _BlurTex_TexelSize.xy*float2(1,1)).xyz;
		float3 col1 = tex2D(_CameraDepthNormalsTexture, i.uv + _EdgeWidth * _BlurTex_TexelSize.xy*float2(1,-1)).xyz;
		float3 col2 = tex2D(_CameraDepthNormalsTexture, i.uv + _EdgeWidth * _BlurTex_TexelSize.xy*float2(-1, 1)).xyz;
		float3 col3 = tex2D(_CameraDepthNormalsTexture, i.uv + _EdgeWidth * _BlurTex_TexelSize.xy*float2(-1,-1)).xyz;
		float edge = luminance(pow(col0 - col3, 2) + pow(col1 - col2, 2));
		edge = pow(edge, 0.2);
		if (edge<_Sensitive)
		{
			edge = 0;
		}
		else
		{
			edge = n;
		}
		float3 color = tex2D(_BlurTex, i.uv);
		float3 finalColor = edge * float3(0, 0, 0) + (1 - edge)*color*(0.95+0.1*n);
		return float4(finalColor, 1.0);
	}
	//画笔滤波部分
	struct v2f_paint {
		float2 uv:TEXCOORD0;
		float4 vertex:SV_POSITION;
	};

	v2f_paint vert_paint(appdata_img v) {
		v2f_paint o;
		o.uv = v.texcoord;
		o.vertex = UnityObjectToClipPos(v.vertex);
		return o;
	}

	float4 frag_paint(v2f_paint i):SV_Target{
		float3 m0 = 0.0;
		float3 m1 = 0.0;
		float3 s0 = 0.0;
		float3 s1 = 0.0;
		float3 c = 0.0;
		int radius = _PaintFactor;
		int r = (radius + 1)*(radius + 1);
		for (int j = -radius; j <= 0; ++j)
		{
			for (int k = -radius; k <= 0; ++k)
			{
				c = tex2D(_PaintTex, i.uv + _PaintTex_TexelSize.xy * float2(k, j)).xyz;
				m0 += c;
				s0 += c * c;
			}
		}
	    for (int j = 0; j <= radius; ++j)
	    {
		    for (int k = 0; k <= radius; ++k)
		    {
			    c = tex2D(_PaintTex, i.uv + _PaintTex_TexelSize.xy * float2(k, j)).xyz;
			    m1 += c;
			    s1 += c * c;
		    }
	    }
	    float4 finalFragColor = 0.;
	    float min_sigma2 = 1e+2;
	    m0 /= r;
	    s0 = abs(s0 / r - m0 * m0);
	    float sigma2 = s0.r + s0.g + s0.b;
	    if (sigma2 < min_sigma2)
	    {
		    min_sigma2 = sigma2;
		    finalFragColor = float4(m0, 1.0);
	    }
	    m1 /= r;
	    s1 = abs(s1 / r - m1 * m1);
	    sigma2 = s1.r + s1.g + s1.b;
	    if (sigma2 < min_sigma2)
	    {
		    min_sigma2 = sigma2;
		    finalFragColor = float4(m1, 1.0);
	    }
		return finalFragColor;
	}

	ENDCG

	SubShader
	{
		Pass
		{
			CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_blur
			ENDCG
		}

		Pass
		{
			CGPROGRAM
            #pragma vertex vert_edge
            #pragma fragment frag_edge
			ENDCG
		}

		Pass
		{
			CGPROGRAM
            #pragma vertex vert_paint
            #pragma fragment frag_paint
			ENDCG
		}
	}
}