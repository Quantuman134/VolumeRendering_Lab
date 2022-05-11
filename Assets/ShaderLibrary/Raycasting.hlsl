#ifndef RAYCASTING_INCLUDED
#define RAYCASTING_INCLUDED

#define STEPS_NUM_STD 512
#define NOISE_BIAS_ENABLE 0
#define PHONG_SHADING_ENABLE 0
#define CATMULL_ROM_SPLINE_ENABLE 0
#define VIRTUAL_SAMPLES_NUM 3

#include "Resources.hlsl"
#include "Voxel.hlsl"
#include "Light.hlsl"
#include "PhongShading.hlsl"
#include "Common.hlsl"
#include "CatmullRomSpline.hlsl"

float _MinVal;
float _MaxVal;

float4 _LightColor;
float3 _LightDirection;

float3 _ObjectPos;
float3 _SceneCameraPos;

struct RaycastingSetting
{
    uint stepsNum;
};

struct RaycastingInput
{
    float3 vertexLocal;
    float2 texUV;
};

struct RaycastingOutput
{
    float4 color;
    float depth;
};

void RaycastingOutputIni(out RaycastingOutput raycastingOutput);
RaycastingOutput Raycasting(RaycastingInput raycastingInput, RaycastingSetting raycastingsetting);
float AlphaCorrection(float alpha, uint stepsNum);
float4 AlphaBlendB2F(float4 col, float4 src);
float localToDepth(float3 localPos);

void RaycastingOutputIni(out RaycastingOutput raycastingOutput)
{
    raycastingOutput.color = float4(0.0, 0.0, 0.0, 0.0);
    raycastingOutput.depth = 0.0;
}

RaycastingOutput Raycasting(RaycastingInput raycastingInput, RaycastingSetting raycastingSetting)
{
    const float stepSize = 1.732f / raycastingSetting.stepsNum;
    float3 rayDir = ObjSpaceViewDir(float4(raycastingInput.vertexLocal, 0.0f));
    rayDir = normalize(rayDir);
    float3 rayStartPos = raycastingInput.vertexLocal;
    rayStartPos = rayStartPos + float3(0.5f, 0.5f, 0.5f);
#if (!NOISE_BIAS_ENABLE)
    rayStartPos = rayStartPos + 0.5 * stepSize * rayDir;
#else
    rayStartPos = rayStartPos + stepSize * rayDir * getNoiseBias(raycastingInput.texUV);
#endif
    
    //for Phong shading
    float3 view_dir = WorldSpaceViewDir(float4(raycastingInput.vertexLocal, 0.0f));
    Light light = GetLightIni();
    light.color = _LightColor;
    light.direction = _LightDirection;
    
    RaycastingOutput raycastingOutput;
    RaycastingOutputIni(raycastingOutput);
    
    //sampling
    uint iDepth = 0;
#if (!CATMULL_ROM_SPLINE_ENABLE)
    for (uint iStep = 0; iStep < raycastingSetting.stepsNum; iStep++)
    {
        const float t = iStep * stepSize;
        const float3 currPos = rayStartPos + rayDir * t;
        
        if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f)
        {
            break;
        }
        
        float3 pos2View = ObjSpaceViewDir(float4(raycastingInput.vertexLocal, 0.0f));
        float3 pos2Curr = currPos - rayStartPos;
		//prevent to render the voxels behind to the viewpoint
        if ((pos2Curr.x / pos2View.x) > 1)
        {
            break;
        }
        
        Voxel voxel = GetVoxelIni();
        voxel.pos = currPos;
        voxel.intensity = getDensity(voxel.pos);
        
        if (voxel.intensity >= _MinVal && voxel.intensity <= _MaxVal)
        {
            float4 src = float4(0.0, 0.0, 0.0, 0.0);
            const float density = voxel.intensity;
            src = getTF1DColor(density);
            src.a = AlphaCorrection(src.a, raycastingSetting.stepsNum);
            voxel.color = src;
            
#if (PHONG_SHADING_ENABLE)
            voxel.normLocal = getNorm(voxel.pos);
            voxel.color = PhongShading(voxel, light, view_dir, getPhongSettingIni());
#endif
            raycastingOutput.color = AlphaBlendB2F(raycastingOutput.color, voxel.color);
            //raycastingOutput.color = float4(1.0, 0.0, 0.0, 1.0);
        }

        if (voxel.color.a > 0.15f)
        {
            iDepth = iStep;
        }
        if (raycastingOutput.color.a > 1.0f)
        {
            break;
        }

    }
#else
    float4 sampleVector = float4(0.0f, 0.0f, 0.0f, 0.0f);
    #if (PHONG_SHADING_ENABLE)
    float3x4 sampleNormMatrix = {
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0
    };
    #endif
    for (uint iStep = 0; iStep <= raycastingSetting.stepsNum; iStep++)
    {
        const float t = iStep * stepSize;
        const float3 currPos = rayStartPos + rayDir * t;
        
        if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f)
        {
            break;
        }
        
        float3 pos2View = ObjSpaceViewDir(float4(raycastingInput.vertexLocal, 0.0f));
        float3 pos2Curr = currPos - rayStartPos;
		//prevent to render the voxels behind the viewpoint
        if ((pos2Curr.x / pos2View.x) > 1)
        {
            break;
        }
        
        Voxel voxel = GetVoxelIni();
        if(iStep == raycastingSetting.stepsNum)
        {
            voxel.pos = currPos;
            voxel.intensity = 0.0;
            float4LeftShift(sampleVector, voxel.intensity);
#if (PHONG_SHADING_ENABLE)
            voxel.normLocal = float3(0.0, 0.0, 0.0);
            float3x4LeftShift(sampleNormMatrix, voxel.normLocal);
#endif
        }
        else
        {
            voxel.pos = currPos;
            voxel.intensity = getDensity(voxel.pos);
            float4LeftShift(sampleVector, voxel.intensity);
#if (PHONG_SHADING_ENABLE)
            voxel.normLocal = getNorm(voxel.pos);
            float3x4LeftShift(sampleNormMatrix, voxel.normLocal);
#endif
        }
        
        if (iStep >= 2)
        {
            for (int i = 0; i <= VIRTUAL_SAMPLES_NUM; i++)
            {
                Voxel virtualVoxel = GetVoxelIni();
                virtualVoxel.intensity = CatmullRomInterpolate(sampleVector, i / (1 + VIRTUAL_SAMPLES_NUM));
                if (virtualVoxel.intensity >= _MinVal && virtualVoxel.intensity <= _MaxVal)
                {
                    float4 src = float4(0.0, 0.0, 0.0, 0.0);
                    const float density = virtualVoxel.intensity;
                    src = getTF1DColor(density);
                    src.a = AlphaCorrection(src.a, raycastingSetting.stepsNum * (1 + VIRTUAL_SAMPLES_NUM));
                    virtualVoxel.color = src;
            
#if (PHONG_SHADING_ENABLE)
                    virtualVoxel.normLocal = CatmullRomInterpolate(sampleNormMatrix, i / (1 + VIRTUAL_SAMPLES_NUM));
                    virtualVoxel.color = PhongShading(virtualVoxel, light, view_dir, getPhongSettingIni());
#endif
                    raycastingOutput.color = AlphaBlendB2F(raycastingOutput.color, virtualVoxel.color);
                    
                    if (virtualVoxel.color.a > 0.15f)
                    {
                        iDepth = iStep;
                    }
                }
            }
        }

        if (raycastingOutput.color.a > 1.0f)
        {
            break;
        }
    }
#endif
    
    if (iDepth != 0)
    {
        raycastingOutput.depth = localToDepth(rayStartPos + rayDir * (iDepth * stepSize) - float3(0.5f, 0.5f, 0.5f));
    }
    return raycastingOutput;
}

float AlphaCorrection(float alpha, uint stepsNum)
{
    float stepsNumStd = STEPS_NUM_STD;
    return min(alpha * (stepsNumStd / stepsNum), 1.0);
}

float4 AlphaBlendB2F(float4 col, float4 src)
{
    float4 output = float4(0.0, 0.0, 0.0, 0.0);
    output.rgb = src.a * src.rgb + (1.0f - src.a) * col.rgb;
    output.a = src.a + (1.0f - src.a) * col.a;
    
    return output;
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

#endif