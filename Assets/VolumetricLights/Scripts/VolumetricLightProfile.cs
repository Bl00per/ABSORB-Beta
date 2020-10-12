using UnityEngine;

namespace VolumetricLights {

    public delegate void OnSettingsChanged();

    public enum ShadowResolution {
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048
    }

    public enum ShadowBakeInterval {
        OnStart,
        EveryFrame
    }

    public enum AttenuationMode {
        Simple,
        Quadratic
    }

    [CreateAssetMenu(menuName = "Volumetric Light Profile", fileName = "VolumetricLightProfile", order = 335)]
    public class VolumetricLightProfile : ScriptableObject {

        public enum BlendMode {
            Additive = 0,
            Blend = 1,
            PreMultiply = 2
        }

        [Header("Rendering")]
        public BlendMode blendMode = BlendMode.Additive;

        [Tooltip("Determines the general accuracy of the effect. The greater this value, the more accurate effect (shadow occlusion as well). Try to keep this value as low as possible while maintainig a good visual result.")]
        [Range(1, 256)] public int raymarchQuality = 8;

        [Tooltip("Determines the minimum step size. Increase to improve performance / decrease to improve accuracy. When increasing this value, you can also increase 'Jittering' amount to improve quality.")]
        public float raymarchMinStep = 0.1f;

        [Tooltip("Increase to reduce inaccuracy due to low number of samples (due to a high raymarch step size).")]
        public float jittering = 0.5f;

        [Tooltip("Increase to reduce banding artifacts. Usually jittering has a bigger impact in reducing artifacts.")]
        [Range(0, 2)] public float dithering = 1f;

        [Tooltip("Uses blue noise for jittering computation reducing moiré pattern of normal jitter. Usually not needed unless you use shadow occlusion. It adds an additional texture fetch so use only if it provides you a clear visual improvement.")]
        public bool useBlueNoise;

        [Tooltip("The render queue for this renderer. By default, all transparent objects use a render queue of 3000. Use a lower value to render before all transparent objects.")]
        public int renderQueue = 3101;

        [Tooltip("Optional sorting layer Id (number) for this renderer. By default 0. Usually used to control the order with other transparent renderers, like Sprite Renderer.")]
        public int sortingLayerID;

        [Tooltip("Optional sorting order for this renderer. Used to control the order with other transparent renderers, like Sprite Renderer.")]
        public int sortingOrder;

        [Tooltip("Use only if depth texture is inverted. Alternatively you can enable MSAA, HDR or change the render scale in the pipeline asset.")]
        public bool flipDepthTexture;

        [Tooltip("Ignores light enable state. Always show volumetric fog. This option is useful to produce fake volumetric lights.")]
        public bool alwaysOn;

        [Header("Appearance")]
        public bool useNoise = true;
        public Texture3D noiseTexture;
        [Range(0, 3)] public float noiseStrength = 1f;
        public float noiseScale = 5f;
        public float noiseFinalMultiplier = 1f;

        public float density = 0.2f;

        public Color mediumAlbedo = Color.white;

        [Tooltip("Overall brightness multiplier.")]
        public float brightness = 1f;

        [Tooltip("Attenuation Mode")]
        public AttenuationMode attenuationMode = AttenuationMode.Simple;

        [Tooltip("Constant coefficient (a) of the attenuation formula. By modulating these coefficients, you can tweak the attenuation quadratic curve 1/(a + b*x + c*x*x).")]
        public float attenCoefConstant = 1f;

        [Tooltip("Linear coefficient (b) of the attenuation formula. By modulating these coefficients, you can tweak the attenuation quadratic curve 1/(a + b*x + c*x*x).")]
        public float attenCoefLinear = 2f;

        [Tooltip("Quadratic coefficient (c) of the attenuation formula. By modulating these coefficients, you can tweak the attenuation quadratic curve 1/(a + b*x + c*x*x).")]
        public float attenCoefQuadratic = 1f;

        [Tooltip("Attenuation of light intensity based on square of distance. Plays with brightness to achieve a more linear or realistic (quadratic attenuation effect).")]
        public float distanceFallOff = 1f;

        [Tooltip("Brightiness increase when looking against light source.")]
        public float diffusionIntensity;

        [Range(0, 1), Tooltip("Smooth edges")] public float border = 0.5f;

        [Header("Spot Light")]
        [Tooltip("Radius of the tip of the cone. Only applies to spot lights.")] public float tipRadius;

        [Header("Area Light")]
        [Range(0f, 80f)] public float frustumAngle;

        [Header("Animation")]
        [Tooltip("Noise animation direction and speed.")]
        public Vector3 windDirection = new Vector3(0.03f, 0.02f, 0);

        [Header("Dust Particles")]
        public bool enableDustParticles;
        public float dustBrightness = 1f;
        public float dustMinSize = 0.01f;
        public float dustMaxSize = 0.02f;
        public float dustWindSpeed = 1f;
        [Tooltip("Dims particle intensity beyond this distance to camera")]
        public float dustDistanceAttenuation = 10f;
        [Tooltip("Fully enable/disable particle system renderer when far from main camera in order to optimize performance")]
        public bool dustAutoToggle;
        [Tooltip("Distance to the light source at which the particule system is fully deactivated. Note that the distance is to the light position regardless of its light range or volume so you should consider the light area or range into this distance as well.")]
        public float dustDistanceDeactivation = 70f;

        [Header("Shadow Occlusion")]
        public bool enableShadows;
        [Range(0, 1)] public float shadowIntensity = 0.7f;
        public ShadowResolution shadowResolution = ShadowResolution._256;
        public LayerMask shadowCullingMask = ~2;
        public ShadowBakeInterval shadowBakeInterval = ShadowBakeInterval.OnStart;
        public float shadowNearDistance = 0.1f;
        [Tooltip("Fully enable/disable shadows when far from main camera in order to optimize performance")]
        public bool shadowAutoToggle;
        [Tooltip("Max distance to main camera to render shadows for this volumetric light.")]
        public float shadowDistanceDeactivation = 250f;
        [Tooltip("Use default render texture format when rendering shadows. Disabling this option will force shadows to render to a single depth buffer, which is faster, but can cause issues with shaders using Grab Pass.")]
        public bool shadowUseDefaultRTFormat = true;


        private void OnEnable() {
            if (noiseTexture == null) {
                noiseTexture = Resources.Load<Texture3D>("Textures/NoiseTex3D1");
            }
        }

        private void OnValidate() {
            tipRadius = Mathf.Max(0, tipRadius);
            density = Mathf.Max(0, density);
            noiseScale = Mathf.Max(0.1f, noiseScale);
            diffusionIntensity = Mathf.Max(0, diffusionIntensity);
            dustMaxSize = Mathf.Max(dustMaxSize, dustMinSize);
            distanceFallOff = Mathf.Max(distanceFallOff, 0);
            brightness = Mathf.Max(brightness, 0);
            border = Mathf.Max(0.002f, border);
            attenCoefConstant = Mathf.Max(0.0001f, attenCoefConstant);
            attenCoefLinear = Mathf.Max(0, attenCoefLinear);
            attenCoefQuadratic = Mathf.Max(0, attenCoefQuadratic);
            dustBrightness = Mathf.Max(0, dustBrightness);
            dustMinSize = Mathf.Max(0, dustMinSize);
            dustMaxSize = Mathf.Max(0, dustMaxSize);
            shadowNearDistance = Mathf.Max(0, shadowNearDistance);
            dustDistanceAttenuation = Mathf.Max(0, dustDistanceAttenuation);
            raymarchMinStep = Mathf.Max(0.1f, raymarchMinStep);
            jittering = Mathf.Max(0, jittering);

            // Update all lights using this profile
            VolumetricLight[] vls = FindObjectsOfType<VolumetricLight>();
                for (int k = 0; k < vls.Length; k++) {
                if (vls[k] != null && vls[k].profile == this) {
                    vls[k].UpdateMaterialProperties();
                }
            }


        }

    }
}