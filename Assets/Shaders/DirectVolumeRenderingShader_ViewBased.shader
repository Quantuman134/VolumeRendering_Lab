Shader "VolumeRendering/DirectVolumeRenderingShader_ViewBased"
{
    Properties
	{
		_DataTex("Data Texture (Generated)", 3D) = "" {}
		_NormTex("Normal Vector Texture (Generated)", 3D) = "" {}
		_GradTex("Gradient Vector Texture (Generated)", 3D) = "" {}
		_GradMoldTex("Gradient Mold Texture (Generated)", 3D) = "" {}
		_NoiseTex("Noise Texture (Generated)", 2D) = "" {}
		_TFTex("Transfer Function Texture2D (Generated)", 2D) = "" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
	}

    SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}
		LOD 100
		Cull Off//表面剔除,决定了back-to-front的体绘制颜色合成方式
		ZTest LEqual//默认值
		ZWrite On//默认值
		Blend SrcAlpha OneMinusSrcAlpha//传统透明度

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma DEPTHWRITE_ON

			#include "UnityCG.cginc"
			#include "../ShaderLibrary/CatMullRomSpline.hlsl"

			struct vert_in
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 uv : TEXCOORD0;
				
			};

			struct frag_in
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 vertexLocal : TEXCOORD1;
				float3 normal : NORMAL;
			};

			struct frag_out
			{
				float4 colour : SV_TARGET;
				float depth : SV_DEPTH;
			};

			sampler3D _DataTex;
			sampler3D _NormTex;
			sampler3D _GradTex;
			sampler3D _GradMoldTex;
			sampler2D _TFTex;
			sampler2D _NoiseTex;

			float _MinVal;
			float _MaxVal;

			float getDensity(float3 pos);
			float3 getNorm(float3 pos);
			float3 getGrad(float3 pos);
			float getGradMold(float3 pos);
			float4 getTF1DColour(float density);
			float localToDepth(float3 localPos);

			float3 getViewPos(float3 v);
			float getViewPosSegmentationID(float3 v);
			int RenderingStrategy(frag_in i, float3 currPos);

			float3 norm_cal_color(float3 pos, float delta);
			float4 PhongShading(float4 color, float3 pos, float3 view_dir);

			//Gets the density at the specified position
			float getDensity(float3 pos)
			{
				return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f));
			}

			//Get the normal vector
			float3 getNorm(float3 pos)
			{
				return tex3Dlod(_NormTex, float4(pos.x, pos.y, pos.z, 0.0f));
			}
			//Get the gradient vector
			float3 getGrad(float3 pos)
			{
				return tex3Dlod(_GradTex, float4(pos.x, pos.y, pos.z, 0.0f));
			}
			//Get the mold of gradient vector
			float getGradMold(float3 pos)
			{
				return tex3Dlod(_GradMoldTex, float4(pos.x, pos.y, pos.z, 0.0f));
			}
			//Gets the colour from a 1D Transfer Function (x = density)
			float4 getTF1DColour(float density)
			{
				return tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));
			}

			//Converts local position to depth value
			float localToDepth(float3 localPos)
			{
				float4 clipPos = UnityObjectToClipPos(float4(localPos, 1.0f));

#if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
				return (clipPos.z / clipPos.w) * 0.5 + 0.5;
#else
				return clipPos.z / clipPos.w;
#endif
			}	
			//Get viewPos
			float3 getViewPos(float3 v)
			{
				float3 vecver2view = ObjSpaceViewDir(float4(v, 0.0f));
				float3 viewPos = v + vecver2view;
				return viewPos;
			}

			float3 norm_cal_color(float3 pos, float delta)//calculate the volume data norm according to the gradient of voxel's color
			{
				float3 norm = float3(0.0f, 0.0f, 0.0f);
				float x1, x2, y1, y2, z1, z2;
				x1 = pos.x - delta;
				x2 = pos.x + delta;
				y1 = pos.y - delta;
				y2 = pos.y + delta;
				z1 = pos.z - delta;
				z2 = pos.z + delta;
				if (pos.x - delta < 0)
				{
					x1 = pos.x;
				}
				if (pos.x + delta > 1)
				{
					x2 = pos.x;
				}
				if (pos.y - delta < 0)
				{
					y1 = pos.y;
				}
				if (pos.y + delta > 1)
				{
					y2 = pos.y;
				}
				if (pos.z - delta < 0)
				{
					z1 = pos.z;
				}
				if (pos.z + delta > 1)
				{
					z2 = pos.z;
				}

				float4 color_temp1, color_temp2;
				color_temp1 = getTF1DColour(float3(x1, pos.y, pos.z));
				color_temp2 = getTF1DColour(float3(x2, pos.y, pos.z));
				norm.x = 1.0f / 2 * length(float3((color_temp2.x - color_temp1.x), (color_temp2.y - color_temp1.y), (color_temp2.z - color_temp1.z)));

				color_temp1 = getTF1DColour(float3(pos.x, y1, pos.z));
				color_temp2 = getTF1DColour(float3(pos.x, y2, pos.z));
				norm.y = 1.0f / 2 * length(float3((color_temp2.x - color_temp1.x), (color_temp2.y - color_temp1.y), (color_temp2.z - color_temp1.z)));

				color_temp1 = getTF1DColour(float3(pos.x, pos.y, z1));
				color_temp2 = getTF1DColour(float3(pos.x, pos.y, z2));
				norm.z = 1.0f / 2 * length(float3((color_temp2.x - color_temp1.x), (color_temp2.y - color_temp1.y), (color_temp2.z - color_temp1.z)));

				norm = normalize(norm);
				return norm;
			}

			float4 PhongShading(float4 color, float3 pos, float3 view_dir)
			{
				float3 light_dir = normalize(float3(1.0f, -1.0f, 0.0f));//direction light
				float k_ambient = 0.4f;
				float k_diffuse = 0.8f;
				float k_specular = 0.7f;
				int spec_pow = 16;
				float3 norm = getNorm(pos);
				norm = UnityObjectToWorldNormal(norm);
				float ambi = k_ambient;
				float diff = max(dot(norm, light_dir), 0.0) * k_diffuse;
				float spec = max(dot(normalize(view_dir - light_dir), norm), 0.0);
				spec = pow(spec, spec_pow) * k_specular;

				float4 phongColor = color;
				phongColor.x = phongColor.x * (ambi + diff + spec);
				phongColor.y = phongColor.y * (ambi + diff + spec);
				phongColor.z = phongColor.z * (ambi + diff + spec);
				return phongColor;
			}

			void float4LeftShift(inout float4 shiftedVector, float inputValue = 0.0f)
			{
				shiftedVector.x = shiftedVector.y;
				shiftedVector.y = shiftedVector.z;
				shiftedVector.z = shiftedVector.w;
				shiftedVector.w = inputValue;
			}

			frag_in vert_main(vert_in v)
			{
				frag_in o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.vertexLocal = v.vertex;
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			//Direct Volume Rendering
			frag_out frag_dvr(frag_in i)
			{	
				#define PHONG_SHADING_ENABLE false
				#define SIMPSON_INTERGRATION_ENABLE false
				#define CATMULLROM_SPLINE_ENABLE 0
				#define VIRTUAL_SAMPLE_NUM 128
				#define NUM_STEPS 512
				#define NUM_STEPS_STD 512
				const float stepSize = 1.732f / NUM_STEPS;
				const float stepSize_std = 1.732f / NUM_STEPS_STD;
				float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0));
				rayDir = normalize(rayDir);
				float3 rayStartPos = i.vertexLocal;
				rayStartPos = rayStartPos + float3(0.5f, 0.5f, 0.5f);
				//rayStartPos = rayStartPos + (2.0f*rayDir / NUM_STEPS)*tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

				float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
				float4 col_2 = float4(0.0f, 0.0f, 0.0f, 0.0f); //for Simpson Integration
				uint iDepth = 0;

#if (CATMULLROM_SPLINE_ENABLE)
				float4 sampleVector = float4(0.0f, 0.0f, 0.0f, 0.0f);
#endif
				for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
				{
					const float t = iStep * stepSize;
					const float3 currPos = rayStartPos + rayDir * t;
					
					if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y<0.0f || currPos.y >1.0f || currPos.z<0.0f || currPos.z>1.0f) // TODO: avoid branch?
					{
						break;
					}
					
					float3 pos2View = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
					float3 pos2Curr = currPos - rayStartPos;
					//prevent to render the voxels behind to user
					if ((pos2Curr.x/pos2View.x)>1)
					{
						break;
					}
#if (!CATMULLROM_SPLINE_ENABLE)
					//alpha blend
					float4 src = float4(0.0f, 0.0f, 0.0f, 0.0f);

					// Get the density/sample value of the current position
					const float density = getDensity(currPos);

					//Apply transfer function
					src = getTF1DColour(density);
					//Alpha value normalization based on step size
					src.a = src.a * (stepSize / stepSize_std);
					
					if (PHONG_SHADING_ENABLE)
					{
						src = PhongShading(src, currPos, WorldSpaceViewDir(float4(i.vertexLocal, 0.0f)));
					}

					//density threshold
					if (density < _MinVal || density > _MaxVal)
						src.a = 0.0f;
					
					col.rgb = src.a*src.rgb + (1.0f - src.a) * col.rgb;
					col.a = src.a + (1.0f - src.a) * col.a;

					if(SIMPSON_INTERGRATION_ENABLE)
					{
						float4 src_2 = float4(0.0f, 0.0f, 0.0f, 0.0f);
						float3 currPos_2 = currPos + rayDir * 0.5f*stepSize;
						const float density_2 = getDensity(currPos_2);
						src_2 = getTF1DColour(density_2);
						src_2.a = src_2.a * (stepSize / stepSize_std);

						if (PHONG_SHADING_ENABLE)
						{
							src_2 = PhongShading(src_2, currPos_2, WorldSpaceViewDir(float4(i.vertexLocal, 0.0f)));
						}

						if (density_2 < _MinVal || density_2 > _MaxVal)
							src_2.a = 0.0f;

						col_2.rgb = src_2.a*src_2.rgb + (1.0f - src_2.a)*col_2.rgb;
						col_2.a = src_2.a + (1.0f - src_2.a)*col_2.a;
					}
#else
					const float density = getDensity(currPos);
					float4LeftShift(sampleVector, density);
					float4 src = float4(0.0f, 0.0f, 0.0f, 0.0f);
					if (iStep >= 2)
					{
						src = getTF1DColour(density);
						src.a = src.a * (stepSize / stepSize_std) / (1 + VIRTUAL_SAMPLE_NUM);

						if (density < _MinVal || density > _MaxVal)
							src.a = 0.0f;

						col.rgb = src.a*src.rgb + (1.0f - src.a) * col.rgb;
						col.a = src.a + (1.0f - src.a) * col.a;

						for(int i = 1; i <= VIRTUAL_SAMPLE_NUM; i++)
						{
							const float virtualDensity = CatmullRomInterpolate(sampleVector, i / (1 + VIRTUAL_SAMPLE_NUM));
							src = getTF1DColour(virtualDensity);
							src.a = src.a * (stepSize / stepSize_std) / (1 + VIRTUAL_SAMPLE_NUM);

							if (density < _MinVal || density > _MaxVal)
								src.a = 0.0f;

							col.rgb = src.a*src.rgb + (1.0f - src.a) * col.rgb;
							col.a = src.a + (1.0f - src.a) * col.a;
						}
					}
#endif
					if (src.a > 0.15f)
						iDepth = iStep;
					if (col.a > 1.0f)
						break;
				}
				
				frag_out output;
				if (!SIMPSON_INTERGRATION_ENABLE)
				{
					output.colour = col;
				}
				else
				{
					output.colour = 1.0f/3 * col + 2.0f/3 * col_2;
				}

				//分配深度
				
				if (iDepth != 0)
					output.depth = localToDepth(rayStartPos + rayDir * (iDepth * stepSize) - float3(0.5f, 0.5f, 0.5f));
				else
					output.depth = 0;
				
				return output;
			}

			frag_in vert(vert_in v)
			{
				return vert_main(v);
			}

			frag_out frag(frag_in i)
			{
				return frag_dvr(i);
			}

			ENDCG
		}
	}
}