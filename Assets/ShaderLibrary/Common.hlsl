#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

void float4LeftShift(inout float4 shiftedVector, float inputValue = 0.0f)
{
    shiftedVector.x = shiftedVector.y;
    shiftedVector.y = shiftedVector.z;
    shiftedVector.z = shiftedVector.w;
    shiftedVector.w = inputValue;
}

void float3x4LeftShift(inout float3x4 shiftedMatrix, float3 inputVector = float3(0.0, 0.0, 0.0))
{
    shiftedMatrix._m00_m10_m20 = shiftedMatrix._m01_m11_m21;
    shiftedMatrix._m01_m11_m21 = shiftedMatrix._m02_m12_m22;
    shiftedMatrix._m02_m12_m22 = shiftedMatrix._m03_m13_m23;
    shiftedMatrix._m03_m13_m23 = inputVector;
}

#endif