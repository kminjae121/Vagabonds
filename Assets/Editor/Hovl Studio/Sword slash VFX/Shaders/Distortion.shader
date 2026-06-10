Shader "Hovl/Particles/Distortion_URP"
{
    Properties
    {
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Distortionpower("Distortion power", Float) = 0.05
        _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float4 _NormalMap_ST;
                float _Distortionpower;
                float _InvFade;
            CBUFFER_END
            
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float distanceFade : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Position transformations
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.color = input.color;
                output.uv = TRANSFORM_TEX(input.uv, _NormalMap);
                
                // Distance fade calculation
                output.distanceFade = 1.0 / distance(_WorldSpaceCameraPos, positionWS);
                
                // Fog
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Screen UV
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                
                // Soft particles - Depth fade
                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(screenUV);
                #else
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
                #endif
                
                float sceneZ = LinearEyeDepth(depth, _ZBufferParams);
                float partZ = LinearEyeDepth(input.screenPos.z / input.screenPos.w, _ZBufferParams);
                float fade = saturate(_InvFade * (sceneZ - partZ));
                
                half4 finalColor = input.color;
                finalColor.a *= fade;
                
                // Normal map sampling
                half3 normalMap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                half2 distortion = normalMap.rg;
                
                // Distortion calculation (matching original shader logic)
                half clampResult = (abs(normalMap.r) + abs(normalMap.g) * 30.0) - 0.2;
                clampResult = saturate(clampResult);
                
                // Apply distortion to screen UV
                half2 distortionOffset = distortion * _Distortionpower * finalColor.a * input.distanceFade;
                half2 distortedUV = screenUV + distortionOffset;
                
                // Sample opaque texture (replaces GrabPass)
                half3 sceneColor = SampleSceneColor(distortedUV);
                
                // Final output
                half4 result = half4(sceneColor, saturate(finalColor.a * clampResult));
                
                // Apply fog
                result.rgb = MixFog(result.rgb, input.fogFactor);
                
                return result;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}