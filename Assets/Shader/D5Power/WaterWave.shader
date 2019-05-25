// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "D5Power/Water/WaterWave" 
{
	Properties
	{
		_RippleData("frequency, scale, centralized, falloff", Vector) = (1,1,1,1)
		_AspectRatio("AspectRatio", Float) = 1
	}
 
	SubShader
	{
		Pass 
		{
			Blend One One
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
 
			struct appdata_t 
			{
				fixed4 vertex : POSITION;
			};
 
			struct v2f 
			{
				fixed4 vertex : POSITION;
				fixed3 pos : TEXCOORD0;
			};
 
			
			uniform fixed _AspectRatio;
			uniform fixed4 _RippleData; // Vector4(frequency, scale, centralized, falloff));
			fixed4 _Drop1;              // Vector4(origin.x, origin.z, time, power));
			static const half pi = 3.1415927;
 
			fixed4 ripple(fixed2 position, fixed2 origin, fixed time, fixed power)
			{
				fixed2 vec = position - origin;
				vec.x *= _AspectRatio; // 做个矫正,让非正方形水面也适用.
				fixed len = length(vec);
				vec = normalize(vec);
 
				//fixed center = time * frequency * scale;
				fixed center = time * _RippleData.y * _RippleData.x;
 
				// fixed phase = 2 * pi * ( time * frequency - len / scale);
				fixed phase = 2 * pi * time * _RippleData.x - 2 * pi * len / _RippleData.y;
				fixed intens = max(0, 0.1 - abs(center - len) * _RippleData.z) * power;
				fixed fallOff = max(0, 1 - len * _RippleData.w);
				fixed cut = step(0, phase);
				return fixed4(vec.x, 1, vec.y, 0) * sin(phase) * intens * fallOff * cut;
			}
 
			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos = v.vertex.xyz;
				return o;
			}
 
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 rip = ripple(i.pos, _Drop1.xy, _Drop1.z, _Drop1.w);
				return rip;
			}
			ENDCG
		}
	}
 
	Fallback Off
}