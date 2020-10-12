#ifndef VOLUMETRIC_LIGHTS_CUSTOM_SHADOW
#define VOLUMETRIC_LIGHTS_CUSTOM_SHADOW

TEXTURE2D(_ShadowTexture);
SAMPLER(sampler_ShadowTexture);
float4x4 _ShadowMatrix;

float4 shadowTextureStart;
float4 shadowTextureEnd;
half3 _ShadowIntensity;

void ComputeShadowTextureCoords(float3 rayStart, float3 rayDir, float t0, float t1) {
    shadowTextureStart = mul(_ShadowMatrix, float4(rayStart + rayDir * t0, 1.0));
    shadowTextureEnd = mul(_ShadowMatrix, float4(rayStart + rayDir * t1, 1.0));
}

half SampleShadowMap(inout float4 shadowCoords) {
    shadowCoords.xyz /= shadowCoords.w;
    float shadowDepth = SAMPLE_DEPTH_TEXTURE(_ShadowTexture, sampler_ShadowTexture, shadowCoords.xy );
    #if UNITY_REVERSED_Z
        shadowCoords.z = shadowCoords.w - shadowCoords.z;
        shadowDepth = shadowCoords.w - shadowDepth;
    #endif
#if VL_POINT
    shadowCoords.z = clamp(shadowCoords.z, -shadowCoords.w, shadowCoords.w);
#endif    
    half shadowTest = shadowCoords.z<0 || shadowDepth > shadowCoords.z;
    return shadowTest;
}

half GetShadowAttenAtEnd() {
    half s = SampleShadowMap(shadowTextureEnd);
    s = s * _ShadowIntensity.x + _ShadowIntensity.y;
    return s;
}

half GetShadowAtten(float t, float t0, float t1) {
    float x = (t-t0) / (t1-t0);
    float4 shadowCoords = lerp(shadowTextureStart, shadowTextureEnd, x);
    half s = SampleShadowMap(shadowCoords);
    s = s * _ShadowIntensity.x + _ShadowIntensity.y;
    return s;
}


half GetShadowAttenWS(float3 wpos) {
    float4 shadowCoords = mul(_ShadowMatrix, float4(wpos, 1.0));
    half shadowTest = SampleShadowMap(shadowCoords);
    // ignore particles outside of shadow map
    shadowTest *= all(shadowCoords.xy > 0 && shadowCoords.xy < 1);
    return shadowTest;
}


#endif // VOLUMETRIC_LIGHTS_CUSTOM_SHADOW

