//------------------------------------------------------------------------------------------------------------------
// Volumetric Lights
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;

namespace VolumetricLights {

    [ExecuteInEditMode, RequireComponent(typeof(Light)), AddComponentMenu("Effects/Volumetric Light", 1000)]
    public partial class VolumetricLight : MonoBehaviour {

        // Common
        public bool useCustomBounds;
        public Bounds bounds;
        public VolumetricLightProfile profile;
        public float customRange = 1f;
        [Tooltip("Currently only used for point light occlusion orientation.")]
        public Transform targetCamera;

        // Area
        public bool useCustomSize;
        public float areaWidth = 1f, areaHeight = 1f;

        [NonSerialized]
        public Light lightComp;

        const string SKW_NOISE = "VL_NOISE";
        const string SKW_BLUENOISE = "VL_BLUENOISE";
        const string SKW_SPOT = "VL_SPOT";
        const string SKW_POINT = "VL_POINT";
        const string SKW_AREA_RECT = "VL_AREA_RECT";
        const string SKW_AREA_DISC = "VL_AREA_DISC";
        const string SKW_SHADOWS = "VL_SHADOWS";
        const string SKW_DIFFUSION = "VL_DIFFUSION";
        const string SKW_PHYSICAL_ATTEN = "VL_PHYSICAL_ATTEN";
        const float GOLDEN_RATIO = 0.618033989f;

        MeshFilter mf;
        MeshRenderer meshRenderer;
        Material fogMat, fogMatLight, fogMatEmpty;
        Vector4 windDirectionAcum;
        bool requireUpdateMaterial;
        List<string> keywords;
        static Texture2D blueNoiseTex;
        float distanceToCameraSqr;

        [NonSerialized]
        public static Transform mainCamera;

        bool profileIsInstanced;

        public VolumetricLightProfile settings {
            get {
                if (!profileIsInstanced && profile != null) {
                    profile = Instantiate(profile);
                    profileIsInstanced = true;
                }
                requireUpdateMaterial = true;
                return profile;
            }
            set {
                profile = value;
                profileIsInstanced = false;
            }
        }


        void OnEnable() {
            lightComp = GetComponent<Light>();
            if (gameObject.layer == 0) { // if object is in default layer, move it to transparent fx layer
                gameObject.layer = 1;
            }
            Refresh();
        }

        public void Refresh() {
            CheckProfile();
            DestroyMesh();
            CheckMesh();
            CheckShadows();
            UpdateMaterialPropertiesNow();
        }


        private void OnValidate() {
            requireUpdateMaterial = true;
        }

        private void OnDisable() {
            TurnOff();
        }

        void TurnOff() {
            if (meshRenderer != null) {
                meshRenderer.enabled = false;
            }
            ShadowsDisable();
            ParticlesDisable();
        }

        private void OnDestroy() {
            if (fogMatEmpty != null) {
                DestroyImmediate(fogMatEmpty);
                fogMatEmpty = null;
            }
            if (fogMatLight != null) {
                DestroyImmediate(fogMatLight);
                fogMatLight = null;
            }
            if (meshRenderer != null) {
                meshRenderer.enabled = false;
            }
            ShadowsDispose();
        }

        void LateUpdate() {

            bool isActiveAndEnabled = lightComp.isActiveAndEnabled || (profile != null && profile.alwaysOn);
            if (isActiveAndEnabled) {
                if (meshRenderer != null && !meshRenderer.enabled) {
                    requireUpdateMaterial = true;
                }
            } else {
                if (meshRenderer != null && meshRenderer.enabled) {
                    TurnOff();
                }
                return;
            }

            if (CheckMesh()) {
                if (!Application.isPlaying) {
                    ParticlesDisable();
                }
                ScheduleShadowCapture();
                requireUpdateMaterial = true;
            }

            if (requireUpdateMaterial) {
                requireUpdateMaterial = false;
                UpdateMaterialPropertiesNow();
            }

            if (fogMat == null || meshRenderer == null || profile == null) return;

            UpdateVolumeGeometry();
            UpdateDiffusionTerm();

            if (profile.dustAutoToggle || profile.shadowAutoToggle) {
                ComputeDistanceToCamera();
            }

            if (profile.enableDustParticles) {
                if (!Application.isPlaying) {
                    ParticlesResetIfTransformChanged();
                }
                UpdateParticlesVisibility();
            }

            fogMat.SetColor("_LightColor", lightComp.color * profile.mediumAlbedo * (lightComp.intensity * profile.brightness));
            float deltaTime = Time.deltaTime;
            windDirectionAcum.x += profile.windDirection.x * deltaTime;
            windDirectionAcum.y += profile.windDirection.y * deltaTime;
            windDirectionAcum.z += profile.windDirection.z * deltaTime;
            windDirectionAcum.w = GOLDEN_RATIO * (Time.frameCount % 480);
            fogMat.SetVector("_WindDirection", windDirectionAcum);

            if (profile.enableShadows) {
                ShadowsUpdate();
            }
        }


        void ComputeDistanceToCamera() {
            if (mainCamera == null) {
                if (Camera.main != null) {
                    mainCamera = Camera.main.transform;
                }
                if (mainCamera == null) return;
            }
            Vector3 camPos = mainCamera.position;
            Vector3 pos = transform.position;
            distanceToCameraSqr = (camPos - pos).sqrMagnitude;
        }

        void UpdateDiffusionTerm() {
            Vector4 toLightDir = -transform.forward;
            toLightDir.w = profile.diffusionIntensity;
            fogMat.SetVector("_ToLightDir", toLightDir);
        }


        public void UpdateVolumeGeometry() {
            UpdateVolumeGeometryMaterial(fogMat);
            if (profile.enableDustParticles && particleMaterial != null) {
                UpdateVolumeGeometryMaterial(particleMaterial);
                particleMaterial.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);
            }
        }

        void UpdateVolumeGeometryMaterial(Material mat) {
            if (mat == null) return;

            Vector4 tipData = transform.position;
            tipData.w = profile.tipRadius;
            mat.SetVector("_ConeTipData", tipData);

            if (customRange < 0.001f) customRange = 0.001f;
            Vector4 coneAxis = transform.forward * customRange;
            coneAxis.w = customRange * customRange;
            mat.SetVector("_ConeAxis", coneAxis);

            float maxDistSqr = generatedRange * generatedRange;
            float falloff = Mathf.Max(0.0001f, profile.distanceFallOff);
            float pointAttenX = -1f / (maxDistSqr * falloff);
            float pointAttenY = maxDistSqr / (maxDistSqr * falloff);
            mat.SetVector("_ExtraGeoData", new Vector4(generatedBaseRadius, pointAttenX, pointAttenY));

            if (!useCustomBounds) {
                bounds = meshRenderer.bounds;
            }
            mat.SetVector("_BoundsCenter", bounds.center);
            mat.SetVector("_BoundsExtents", bounds.extents);
            if (generatedType == LightType.Area) {
                float baseMultiplierComputed = (generatedAreaFrustumMultiplier - 1f) / generatedRange;
                mat.SetVector("_AreaExtents", new Vector4(areaWidth * 0.5f, areaHeight * 0.5f, generatedRange, baseMultiplierComputed));
            } else if (generatedType == LightType.Disc) {
                float baseMultiplierComputed = (generatedAreaFrustumMultiplier - 1f) / generatedRange;
                mat.SetVector("_AreaExtents", new Vector4(areaWidth * areaWidth, areaHeight, generatedRange, baseMultiplierComputed));
            }
        }


        public void UpdateMaterialProperties() {
            requireUpdateMaterial = true;
        }

        void UpdateMaterialPropertiesNow() {

            bool alwaysOn = profile != null && profile.alwaysOn;
            if (this == null || !isActiveAndEnabled || lightComp == null || (!lightComp.isActiveAndEnabled && !alwaysOn))
            {
                ShadowsDisable();
                return;
            }

            if (meshRenderer == null) {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (profile == null) {
                if (meshRenderer != null) {
                    if (fogMatEmpty == null) {
                        fogMatEmpty = new Material(Shader.Find("VolumetricLights/Empty"));
                        fogMatEmpty.hideFlags = HideFlags.DontSave;
                    }
                    meshRenderer.sharedMaterial = fogMatEmpty;
                }
                return;
            }

            if (fogMatLight == null) {
                fogMatLight = new Material(Shader.Find("VolumetricLights/VolumetricLightURP"));
                fogMatLight.hideFlags = HideFlags.DontSave;
            }
            fogMat = fogMatLight;

            if (meshRenderer != null) {
                meshRenderer.sharedMaterial = fogMat;
            }

            if (fogMat == null || profile == null) return;

            if (meshRenderer != null) {
                meshRenderer.sortingLayerID = profile.sortingLayerID;
                meshRenderer.sortingOrder = profile.sortingOrder;
            }
            fogMat.renderQueue = profile.renderQueue;

            switch(profile.blendMode) {
                case VolumetricLightProfile.BlendMode.Additive:
                    fogMat.SetInt("_BlendSrc", (int)UnityEngine.Rendering.BlendMode.One);
                    fogMat.SetInt("_BlendDest", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case VolumetricLightProfile.BlendMode.Blend:
                    fogMat.SetInt("_BlendSrc", (int)UnityEngine.Rendering.BlendMode.One);
                    fogMat.SetInt("_BlendDest", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case VolumetricLightProfile.BlendMode.PreMultiply:
                    fogMat.SetInt("_BlendSrc", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    fogMat.SetInt("_BlendDest", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
            }
            fogMat.SetTexture("_MainTex", profile.noiseTexture);
            fogMat.SetFloat("_NoiseStrength", profile.noiseStrength);
            fogMat.SetFloat("_NoiseScale", 0.1f / profile.noiseScale);
            fogMat.SetFloat("_NoiseFinalMultiplier", profile.noiseFinalMultiplier);
            fogMat.SetFloat("_Border", profile.border);
            fogMat.SetFloat("_DistanceFallOff", profile.distanceFallOff);
            fogMat.SetVector("_FallOff", new Vector3(profile.attenCoefConstant, profile.attenCoefLinear, profile.attenCoefQuadratic));
            fogMat.SetFloat("_Density", profile.density);
            fogMat.SetVector("_RayMarchSettings", new Vector4(profile.raymarchQuality, profile.dithering * 0.01f, profile.jittering, profile.raymarchMinStep));
            if (profile.jittering > 0) {
                if (blueNoiseTex == null) blueNoiseTex = Resources.Load<Texture2D>("Textures/blueNoiseVL");
                fogMat.SetTexture("_BlueNoise", blueNoiseTex);
            }
            fogMat.SetInt("_FlipDepthTexture", profile.flipDepthTexture ? 1 : 0);

            if (keywords == null) {
                keywords = new List<string>();
            } else {
                keywords.Clear();
            }
            if (profile.useBlueNoise) keywords.Add(SKW_BLUENOISE);
            if (profile.useNoise) keywords.Add(SKW_NOISE);
            switch (lightComp.type) {
                case LightType.Spot: keywords.Add(SKW_SPOT); break;
                case LightType.Point: keywords.Add(SKW_POINT); break;
                case LightType.Area: keywords.Add(SKW_AREA_RECT); break;
                case LightType.Disc: keywords.Add(SKW_AREA_DISC); break;
            }
            if (profile.attenuationMode == AttenuationMode.Quadratic) {
                keywords.Add(SKW_PHYSICAL_ATTEN);
            }
            if (profile.diffusionIntensity > 0) {
                keywords.Add(SKW_DIFFUSION);
            }
            if (useCustomBounds) {
                keywords.Add(SKW_CUSTOM_BOUNDS);
            }

            ShadowsSupportCheck();
            if (profile.enableShadows) {
                keywords.Add(SKW_SHADOWS);
            }
            fogMat.shaderKeywords = keywords.ToArray();

            ParticlesCheckSupport();
        }


        void CheckProfile() {
            if (profile == null) {
                profile = ScriptableObject.CreateInstance<VolumetricLightProfile>();
            }
        }
    }
}
