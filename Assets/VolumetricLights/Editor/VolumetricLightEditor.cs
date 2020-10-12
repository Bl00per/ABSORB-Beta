using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace VolumetricLights {

    [CustomEditor(typeof(VolumetricLight))]
    public partial class VolumetricLightEditor : Editor {

        VolumetricLightProfile cachedProfile;
        Editor cachedProfileEditor;
        SerializedProperty profile;
        SerializedProperty useCustomBounds, bounds;
        SerializedProperty areaWidth, areaHeight;
        SerializedProperty customRange, useCustomSize;
        SerializedProperty targetCamera;

        static GUIStyle boxStyle;

        void OnEnable() {
            profile = serializedObject.FindProperty("profile");
            useCustomBounds = serializedObject.FindProperty("useCustomBounds");
            bounds = serializedObject.FindProperty("bounds");
            useCustomSize = serializedObject.FindProperty("useCustomSize");
            areaWidth = serializedObject.FindProperty("areaWidth");
            areaHeight = serializedObject.FindProperty("areaHeight");
            customRange = serializedObject.FindProperty("customRange");
            targetCamera = serializedObject.FindProperty("targetCamera");
        }


        public override void OnInspectorGUI() {

            VolumetricLight vl = (VolumetricLight)target;
            if (vl.lightComp != null && vl.lightComp.type == LightType.Directional) {
                EditorGUILayout.HelpBox("Volumetric Lights works with Point, Spot and Area lights only.\nYou can place a volumetric area light in the desired area instead or use Volumetric Fog & Mist asset for other fog related effects.", MessageType.Info);
                if (GUILayout.Button("Create a Volumetric Fog Area")) {
                    CreateVolumetricAreaLight();
                }
                if (GUILayout.Button("Volumetric Fog & Mist information")) {
                    Application.OpenURL("https://assetstore.unity.com/packages/slug/162694?aid=1101lGsd");
                }
                return;
            }

            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset pipe = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipe == null) {
                EditorGUILayout.HelpBox("Universal Rendering Pipeline asset is not set in Project Settings / Graphics !", MessageType.Error);
                return;
            }

            if (!pipe.supportsCameraDepthTexture) {
                EditorGUILayout.HelpBox("Depth Texture option is required in Universal Rendering Pipeline asset!", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
                GUI.enabled = false;
            } else if (!pipe.supportsHDR && pipe.msaaSampleCount == 1 && pipe.renderScale == 1f && (vl.profile == null || !vl.profile.flipDepthTexture)) {
                EditorGUILayout.HelpBox("Depth Texture might be inverted due to current pipeline setup. To fix depth texture orientation, enable Flip Depth Texture option in the Volumetric Light profile or enable HDR or MSAA in Universal Rendering Pipeline asset.", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
            }


            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.padding = new RectOffset(15, 10, 5, 5);
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(profile);

            if (profile.objectReferenceValue != null) {
                if (cachedProfile != profile.objectReferenceValue) {
                    cachedProfile = null;
                }
                if (cachedProfile == null) {
                    cachedProfile = (VolumetricLightProfile)profile.objectReferenceValue;
                    cachedProfileEditor = CreateEditor(profile.objectReferenceValue);
                }

                // Drawing the profile editor
                EditorGUILayout.BeginVertical(boxStyle);
                cachedProfileEditor.OnInspectorGUI();
                EditorGUILayout.EndVertical();

                // Additional options
                if (vl.lightComp != null) {
                    EditorGUILayout.PropertyField(useCustomSize);
                    if (useCustomSize.boolValue) {
                        EditorGUI.indentLevel++;
                        switch (vl.lightComp.type) {
                            case LightType.Area:
                                EditorGUILayout.PropertyField(areaWidth, new GUIContent("Width"));
                                EditorGUILayout.PropertyField(areaHeight, new GUIContent("Height"));
                                break;
                            case LightType.Disc:
                                EditorGUILayout.PropertyField(areaWidth, new GUIContent("Radius"));
                                break;
                            case LightType.Spot:
                            case LightType.Point:
                                break;
                        }
                        EditorGUILayout.PropertyField(customRange, new GUIContent("Range"));
                        EditorGUI.indentLevel--;
                    }
                }

                if (vl.ps != null) {
                    if (GUILayout.Button("Select Particle System")) {
                        Selection.activeGameObject = vl.ps.gameObject;
                    }
                }

                if (vl.profile != null && vl.profile.enableShadows) {
                    if (vl.lightComp != null && vl.lightComp.type == LightType.Point) {
                        EditorGUILayout.PropertyField(targetCamera);
                    }
                }
                EditorGUILayout.PropertyField(useCustomBounds);
                if (useCustomBounds.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(bounds);
                    EditorGUI.indentLevel--;
                }
            } else {
                EditorGUILayout.HelpBox("Create or assign a profile.", MessageType.Info);
                if (GUILayout.Button("New Volumetric Light Profile")) {
                    CreateFogProfile();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void CreateFogProfile() {
            VolumetricLightProfile fp = CreateInstance<VolumetricLightProfile>();
            fp.name = "New Volumetric Light Profile";
            AssetDatabase.CreateAsset(fp, "Assets/" + fp.name + ".asset");
            AssetDatabase.SaveAssets();
            profile.objectReferenceValue = fp;
            EditorGUIUtility.PingObject(fp);
        }

        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        protected virtual void OnSceneGUI() {
            VolumetricLight vl = (VolumetricLight)target;
            if (!vl.useCustomBounds) return;

            m_BoundsHandle.center = vl.bounds.center;
            m_BoundsHandle.size = vl.bounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck()) {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(vl, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                vl.bounds = newBounds;
                vl.UpdateVolumeGeometry();
            }
        }

        void CreateVolumetricAreaLight() {
            GameObject go = new GameObject("Volumetric Area Light", typeof(Light));
            Light light = go.GetComponent<Light>();
            light.type = LightType.Area;
            light.areaSize = new Vector2(50, 20);
            light.range = 20;
            light.enabled = false;
            VolumetricLight current = (VolumetricLight)target;
            go.transform.forward = current.transform.forward;
            VolumetricLight vl = go.AddComponent<VolumetricLight>();
            vl.profile.density = 0.015f;
            vl.profile.useNoise = false;
            vl.profile.alwaysOn = true;
            vl.Refresh();
            DestroyImmediate(current);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
            GUIUtility.ExitGUI();
        }
    }

}