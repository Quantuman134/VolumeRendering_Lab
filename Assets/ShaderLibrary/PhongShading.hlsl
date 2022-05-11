#ifndef PHONG_SHADING_INCLUDED
#define PHONG_SHADING_INCLUDED

#include "Voxel.hlsl"
#include "Light.hlsl"

struct PhongSetting
{
    float k_ambient;
    float k_diffuse;
    float k_specular;
    int spec_pow;
};

PhongSetting getPhongSettingIni()
{
    PhongSetting phongSetting;
    phongSetting.k_ambient = 0.6;
    phongSetting.k_diffuse = 0.5;
    phongSetting.k_specular = 0.8;
    phongSetting.spec_pow = 256;
    
    return phongSetting;
}

float4 PhongShading(Voxel voxel, Light light, float3 view_dir, PhongSetting phongSetting)
{
    float k_ambient = phongSetting.k_ambient;
    float k_diffuse = phongSetting.k_diffuse;
    float k_specular = phongSetting.k_specular;
    int spec_pow = phongSetting.spec_pow;
    float4 lightColor = light.color;
    float3 lightDir = light.direction;
    float3 voxelNorm = UnityObjectToWorldNormal(voxel.normLocal);
    
    float4 ambient_col = float4(0.0, 0.0, 0.0, 0.0);
    float4 diffuse_col = float4(0.0, 0.0, 0.0, 0.0);
    float4 specular_col = float4(0.0, 0.0, 0.0, 0.0);
    
    //ambient
    ambient_col = k_ambient * voxel.color * lightColor;
    
    //diffuse
    diffuse_col = k_diffuse * max(dot(-lightDir, voxelNorm), 0.0) * voxel.color * lightColor;
    
    //specular
    float4 specular = max(dot(normalize(-view_dir - lightDir), voxelNorm), 0.0);
    specular_col = k_specular * pow(specular, spec_pow);
    
    float4 col = ambient_col + diffuse_col + specular_col;
    col.a = voxel.color.a;
    return col;
}

#endif