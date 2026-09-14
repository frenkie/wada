#ifndef CUSTOM_WADA_LIGHTING_INCLUDED
#define CUSTOM_WADA_LIGHTING_INCLUDED


#ifndef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif
#ifndef UNIVERSAL_SHADOWS_INCLUDED
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#endif
#ifndef UNIVERSAL_INPUT_INCLUDED
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#endif


/*
- Handles additional lights (e.g. additional directional, point, spotlights)
- To work in the Unlit Graph, the following keywords must be defined in the blackboard :
	- Boolean Keyword, Global Multi-Compile "_ADDITIONAL_LIGHT_SHADOWS"
	- Boolean Keyword, Global Multi-Compile "_ADDITIONAL_LIGHTS"
*/
void AdditionalLightsShadows_half(float LightIndex, float3 WorldPosition, half4 Shadowmask,
                                  out half ShadowAtten)
{
    ShadowAtten = 0;

    #ifndef SHADERGRAPH_PREVIEW
    uint pixelLightCount = GetAdditionalLightsCount();
    int index = int(LightIndex);

    if (pixelLightCount > index)
    {
        Light light = GetAdditionalLight(index, WorldPosition, Shadowmask);
        ShadowAtten = AdditionalLightShadow(index, WorldPosition, light.direction, Shadowmask,
                                            _AdditionalLightsOcclusionProbes[index]);
    }

    #endif
}

#endif // CUSTOM_WADA_LIGHTING_INCLUDED
