#ifndef CATMULL_ROM_SPLINE_INCLUDED
#define CATMULL_ROM_SPLINE_INCLUDED

#define TENSION_FACTOR 0.5

float CatmullRomInterpolate(float4 sampleVector, float x)
{
	float4 factorVector = float4(0.0, 0.0, 0.0, 0.0);
	factorVector.x = TENSION_FACTOR * (-pow(x, 3) + 2 * pow(x, 2) - x);
	factorVector.y = TENSION_FACTOR * (3 * pow(x, 3) - 5 * pow(x, 2) + 2);
	factorVector.z = TENSION_FACTOR * (-3 * pow(x, 3) + 4 * pow(x, 2) + x);
	factorVector.w = TENSION_FACTOR * (pow(x, 3) - pow(x, 2));

	return dot(factorVector, sampleVector);
}

float3 CatmullRomInterpolate(float3x4 sampleMatrix, float x)
{
    float3 outputVector = float3(0.0, 0.0, 0.0);
    outputVector.x = CatmullRomInterpolate(sampleMatrix[0], x);
    outputVector.y = CatmullRomInterpolate(sampleMatrix[1], x);
    outputVector.z = CatmullRomInterpolate(sampleMatrix[2], x);
    return outputVector;
}

#endif