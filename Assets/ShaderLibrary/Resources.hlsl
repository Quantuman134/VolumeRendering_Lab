#ifndef RESOURCES_INCLUDED
#define RESOURCES_INCLUDED


sampler3D _DataTex;
sampler2D _TFTex;
sampler2D _NoiseTex;
sampler3D _NormTex;

float getDensity(float3 pos)
{
    return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0));
}

float4 getTF1DColor(float density)
{
    return tex2Dlod(_TFTex, float4(density, 0.0, 0.0, 0.0));
}

float getNoiseBias(float2 uv)
{
    return tex2D(_NoiseTex, uv).r;
}

float3 getNorm(float3 pos)
{
    return tex3Dlod(_NormTex, float4(pos.x, pos.y, pos.z, 0.0f));
}

#endif