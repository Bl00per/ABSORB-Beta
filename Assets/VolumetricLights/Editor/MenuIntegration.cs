using UnityEditor;
using UnityEngine;

namespace VolumetricLights {

    public static class MenuIntegration {

        [MenuItem("GameObject/Light/Volumetric Point Light", false, 100)]
        public static void AddVolumetricPointLight(MenuCommand menuCommand) {
            GameObject go = new GameObject("Volumetric Point Light", typeof(Light));
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Light light = go.GetComponent<Light>();
            light.type = LightType.Point;
            go.AddComponent<VolumetricLight>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }


        [MenuItem("GameObject/Light/Volumetric Spot Light", false, 100)]
        public static void AddVolumetricSpotLight(MenuCommand menuCommand) {
            GameObject go = new GameObject("Volumetric Spot Light", typeof(Light));
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Light light = go.GetComponent<Light>();
            light.type = LightType.Spot;
            go.AddComponent<VolumetricLight>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/Light/Volumetric Rect Area Light", false, 100)]
        public static void AddVolumetricRectAreaLight(MenuCommand menuCommand) {
            GameObject go = new GameObject("Volumetric Rect Area Light", typeof(Light));
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Light light = go.GetComponent<Light>();
            light.type = LightType.Rectangle;
            go.AddComponent<VolumetricLight>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/Light/Volumetric Disc Area Light", false, 100)]
        public static void AddVolumetricDiscAreaLight(MenuCommand menuCommand) {
            GameObject go = new GameObject("Volumetric Disc Area Light", typeof(Light));
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Light light = go.GetComponent<Light>();
            light.type = LightType.Disc;
            go.AddComponent<VolumetricLight>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

    }

}