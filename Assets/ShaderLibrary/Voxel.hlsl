#ifndef VOXEL_INCLUDED
#define VOXEL_INCLUDED

struct Voxel
{
    float3 pos;
    float intensity;
    float4 color;
    float3 normLocal;
};

Voxel GetVoxelIni()
{
    Voxel voxel;
    voxel.pos = float3(0.0, 0.0, 0.0);
    voxel.color = float4(0.0, 0.0, 0.0, 0.0);
    voxel.normLocal = float3(0.0, 0.0, 0.0);
    
    return voxel;
}

#endif