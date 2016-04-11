Shader "Custom/Vignette" 
{
	Properties 
	{
	    _fade      ("Opacity",   float) = 1
		_intensity ("Intensity", float) = 1
		_offsetX   ("OffsetX",   float) = 0
		_offsetY   ("OffsetY",   float) = 0
		_width     ("Width",     float) = 1
		_height    ("Height",    float) = 1
		_ellipse   ("Ellipse",   float) = 4
		_fuzzy     ("Fuzzy",     float) = 1
	}
	
	SubShader 
	{ 
		Tags 
		{ 
		  "QUEUE"="Transparent" 
		  "RenderType"="Transparent" 
		}
		Pass 
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 vertex : POSITION;
				float2  texcoord : TEXCOORD0;
			};

			struct appdata_t 
			{
				float4 vertex : POSITION;
			};

			float _offsetX;
			float _offsetY;
			float _fade;
			float _intensity;
			float _width;
			float _height;
			float _ellipse;
			float _fuzzy;
			
			v2f vert(in appdata_t v) 
			{
				float4 tmpvar_2 = mul(UNITY_MATRIX_MVP, v.vertex);
				float2 tmpvar_3 = (tmpvar_2.xy / tmpvar_2.w);
				
				float2 tmpvar_1;
				tmpvar_1.x = (tmpvar_3.x + _offsetX);
				tmpvar_1.y = (tmpvar_3.y + _offsetY) * 2.5;
				
				v2f o;
				o.vertex   = tmpvar_2;
				o.texcoord = tmpvar_1;
				
				return o;
			}

			half4 frag(v2f IN) : COLOR
			{
				half4 col;
				
				col.xyz   = half3(0.0, 0.0, 0.0);
				
				// col.w  = clamp ((((IN.texcoord.x * IN.texcoord.x) * _width) + (IN.texcoord.y * IN.texcoord.y) * _height), 0.0, 1.0);
				
				col.w = clamp
				        (
							pow(abs(IN.texcoord.x / 0.5) * _width,  _ellipse) + 
							pow(abs(IN.texcoord.y / 0.5) * _height, _ellipse),
							0.0, 
							1.0
						);
						
				if (col.w < 1.0f)
				{	
					col.w *= _fade * _intensity * _fuzzy;
				}				
				else
				{
					col.w *= _fade * _intensity;
				}				
				
				
				return  col;
			}

			ENDCG

		}
	}
	
	FallBack "Diffuse"
}