Shader "Custom/CGPBackup"
{
	Properties
	{
		//Color at lower depth ranges
		_DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)

		//Color at higher depth ranges
		_DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)

		//Max distance of depth. Used in normalized fashion so 1 is biggest depth found
		_DepthMaxDistance("Depth Maximum Distance", Float) = 1

		//Color of edges and surface noise
		_EdgeColor("Edge Color", Color) = (1,1,1,1)

		//Noise texture used to generate edge/noise color.
		_SurfaceNoise("Surface Noise", 2D) = "white" {}

	//Speed, in UVs per second the noise will scroll. Only the xy components are used.
	_SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)

		//Values in the noise texture above this cutoff are rendered on the surface.
		_SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777

		//Red/green normal map used as noise texture to create distortion in the surface.
		_SurfaceDistortion("Surface Distortion", 2D) = "white" {}

		_SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27

			//Control the distance that surfaces below the water will contribute
			//to foam being rendered.
			_EdgeMaxDistance("Edge Maximum Distance", Float) = 0.4
			_EdgeMinDistance("Edge Minimum Distance", Float) = 0.04

			_SquishStrength("Squish Strength", Float) = 50
			_WobbleStrength("Wobble Strength", Float) = 50

			//Specular power value
			_ShineValue("Shine Value", Float) = 0
	}
		SubShader
		{
			Tags
			{
				//"RenderType" = "Opaque"
			   "Queue" = "Transparent"
		   }

		   Pass
		   {
			   Blend SrcAlpha OneMinusSrcAlpha
			   ZWrite Off
			   Cull Off

			   CGPROGRAM
			   #define SMOOTHSTEP_AA 0.01

			   #pragma vertex vert
			   #pragma fragment frag

			   #include "UnityCG.cginc"
			   #include "UnityLightingCommon.cginc"


			//normal blending
			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 noiseUV : TEXCOORD0;
				float2 distortUV : TEXCOORD1;
				float4 screenPosition : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
				float3 viewNormal : NORMAL;
			};

			sampler2D _SurfaceNoise;
			//Tiling and offset parameters are saved in this. Used in TRANSFORM_TEX, but we don't use directly
			float4 _SurfaceNoise_ST;

			sampler2D _SurfaceDistortion;
			float4 _SurfaceDistortion_ST;

			//Pseudo randomizer
			float random(float val)
			{
				return frac(sin(dot(val, float2(12.9898, 78.233))) * 43758.5453123);
			}

			float _SquishStrength;
			float _WobbleStrength;

			v2f vert(appdata v)
			{
				v2f o;

				//Transforms a point from object space to view space
				o.vertex = UnityObjectToClipPos(v.vertex);
				//Computes texture coordinate for doing a screenspace-mapped texture sample
				o.screenPosition = ComputeScreenPos(o.vertex);
				//UV for the two distotions. Used for screen scaling?
				o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
				o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);

				o.viewNormal = COMPUTE_VIEW_NORMAL;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);

				float randX = random(o.distortUV.x);
				float randY = random(o.distortUV.y);

				//sign returns either 1, 0 or -1 depending if input is positive, zero or negative
				o.vertex.x += sign(v.vertex.x) * sin(_Time.w) / _SquishStrength;
				o.vertex.y += sign(v.vertex.y) * cos(_Time.w) / _SquishStrength;
				o.vertex.x += sign(v.vertex.x) * sin(_Time.w * randX) / _WobbleStrength;
				o.vertex.y += sign(v.vertex.y) * cos(_Time.w * randY) / _WobbleStrength;

				return o;
			}

			float4 _DepthGradientShallow;
			float4 _DepthGradientDeep;
			float4 _EdgeColor;
			float _ShineValue;

			float _DepthMaxDistance;
			float _EdgeMaxDistance;
			float _EdgeMinDistance;
			float _SurfaceNoiseCutoff;
			float _SurfaceDistortionAmount;

			float2 _SurfaceNoiseScroll;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraNormalsTexture;

			float4 frag(v2f i) : SV_Target
			{
				//Find depth value for every pixel
				//Tex2Dproj and UNITY_PROJ_COORD divides xy by z so zooming appears realistic with the object
				float existingDepth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosition)).r;
			//Convert the depth from non-linear 0...1 range to linear
			float existingDepthLinear = LinearEyeDepth(existingDepth01);

			//Difference between the surface and the object behind it.
			float depthDifference = existingDepthLinear - i.screenPosition.w;

			//Color the blob based on the depth using the two colors.
			float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
			float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);

			//Find normal value for object surfaces on every pixel
			float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition));

			//More edge depending on both depth difference and normals difference. Makes edges against perpendicular surfaces have larger edges
			float3 normalDot = saturate(dot(existingNormal, i.viewNormal));
			float edgeDistance = lerp(_EdgeMaxDistance, _EdgeMinDistance, normalDot);
			float edgeDepthDifference = saturate(depthDifference / edgeDistance);

			//Multiply by value to easily change the resulting effect
			float surfaceNoiseCutoff = edgeDepthDifference * _SurfaceNoiseCutoff;

			//Get texture pixel values in the -1 to 1 range
			float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;

			//Modify noise by the distortion values we found. Multiply with time for a smoothly moving effect
			float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x,
			(i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);
			float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

			//Use smoothstep to ensure we get some anti-aliasing in the transition from edge to surface. This doesn't appear to work with URP
			float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);

			float4 surfaceNoiseColor = _EdgeColor;
			surfaceNoiseColor.a *= surfaceNoise;

			fixed4 col = waterColor;

			//Normalize view, light and normal vectors
			float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
			float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
			float3 worldNormal = normalize(i.worldNormal);

			//Find recflection vector. Otherwise use R = 2N * NdotL - L
			float3 reflectionDir = reflect(-lightDir, worldNormal);

			float RdotV = max(0, dot(reflectionDir, viewDir));

			//pow to define the strength of the reflection. _LightColor0 is the light source
			fixed4 shineColor = pow(RdotV, _ShineValue) * _LightColor0;

			col += shineColor / 2;

			//Use normal blending defined earlier to combine the edge with the surface color.
			return alphaBlend(surfaceNoiseColor, col);
			}
		ENDCG
		}
	}
}