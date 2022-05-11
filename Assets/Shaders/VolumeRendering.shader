Shader "VolumeRendering/VolumeRendering"
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
		[HideInInspector] _LightColor(" ", Color) = (0.0, 0.0, 0.0, 0.0)
		[HideInInspector] _LightDirection(" ", Vector) = (0.0, 0.0, 0.0, 0.0)
		[HideInInspector] _SceneCameraPos(" ", Vector) = (0.0, 0.0, 0.0, 0.0)
		[HideInInspector] _ObjectPos(" ", Vector) = (0.0, 0.0, 0.0, 0.0)
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
			#pragma vertex vertDVRPass
			#pragma fragment fragDVRPass
			#pragma DEPTHWRITE_ON

			#include "UnityCG.cginc"
			#include "VolumeRenderingPass.hlsl"

			ENDCG
		}
	}
}