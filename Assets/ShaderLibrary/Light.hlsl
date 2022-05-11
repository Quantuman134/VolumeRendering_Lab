#ifndef LIGHT_INCLUDED
#define LIGHT_INCLUDED

struct Light
{
    float4 color;
    float3 direction;
};

Light GetLightIni()
{
    Light light;
    light.color = float4(1.0, 0.957, 0.839, 1.0);
    light.direction = normalize(float3(1.0, -1.0, 0.0));
    
    return light;
}

#endif