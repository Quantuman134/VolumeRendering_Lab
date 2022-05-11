#ifndef VOLUME_RENDERING_PASS_INCLUDED
#define VOLUME_RENDERING_PASS_INCLUDED

#include "../ShaderLibrary/Resources.hlsl"
#include "../ShaderLibrary/Raycasting.hlsl"

#define STEPS_NUM 512

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

frag_in vertDVRPass(vert_in v)
{
    frag_in o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.vertexLocal = v.vertex;
    o.normal = UnityObjectToWorldNormal(v.normal);
    return o;
}

frag_out fragDVRPass(frag_in i)
{
    frag_out output;
    RaycastingSetting raycastingSetting;
    raycastingSetting.stepsNum = STEPS_NUM;
    RaycastingInput raycastingInput;
    raycastingInput.vertexLocal = i.vertexLocal;
    raycastingInput.texUV = i.uv;
    RaycastingOutput raycastingOutput = Raycasting(raycastingInput, raycastingSetting);
    output.colour = raycastingOutput.color;
    output.depth = raycastingOutput.depth;
    
    return output;
}

#endif