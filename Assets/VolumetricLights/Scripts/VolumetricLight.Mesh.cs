//------------------------------------------------------------------------------------------------------------------
// Volumetric Lights
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;

namespace VolumetricLights {

    public partial class VolumetricLight : MonoBehaviour {

        const int sides = 32;

        List<Vector3> vertices = new List<Vector3>(32);
        List<int> indices = new List<int>(32);
        float generatedRange = -1;
        float generatedTipRadius = -1;
        float generatedSpotAngle = -1;
        float generatedBaseRadius;
        float generatedAreaWidth, generatedAreaHeight, generatedAreaFrustumAngle, generatedAreaFrustumMultiplier;
        LightType generatedType;

        void DestroyMesh() {
            mf = GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) {
                DestroyImmediate(mf.sharedMesh);
            }
        }

        bool CheckMesh() {

            if (meshRenderer != null) {
                bool isEnabled = lightComp.enabled || (profile != null && profile.alwaysOn);
                meshRenderer.enabled = isEnabled;
            }

#if UNITY_EDITOR
            if (!useCustomSize) {
                areaWidth = lightComp.areaSize.x;
                areaHeight = lightComp.areaSize.y;
                customRange = lightComp.range;
            }
#endif

            bool needMesh = false;
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) {
                needMesh = true;
            }

            switch (lightComp.type) {
                case LightType.Spot:
                    if (needMesh || generatedType != LightType.Spot || customRange != generatedRange || lightComp.spotAngle != generatedSpotAngle || (profile != null && profile.tipRadius != generatedTipRadius)) {
                        GenerateConeMesh();
                        return true;
                    }
                    break;
                case LightType.Point:
                    if (needMesh || generatedType != LightType.Point || customRange != generatedRange) {
                        GenerateSphereMesh();
                        return true;
                    }
                    break;
                case LightType.Area:
                    if (needMesh || generatedType != LightType.Area || customRange != generatedRange || areaWidth != generatedAreaWidth || areaHeight != generatedAreaHeight || (profile != null && profile.frustumAngle != generatedAreaFrustumAngle) || transform.localScale != Vector3.one) {
                        GenerateCubeMesh();
                        return true;
                    }
                    break;
                case LightType.Disc:
                    if (needMesh || generatedType != LightType.Disc || customRange != generatedRange || areaWidth != generatedAreaWidth || (profile != null && profile.frustumAngle != generatedAreaFrustumAngle) || transform.localScale != Vector3.one) {
                        GenerateCylinderMesh();
                        return true;
                    }
                    break;
                default:
                    if (meshRenderer != null) {
                        meshRenderer.enabled = false;
                    }
                    break;
            }
            return false;
        }

        void UpdateMesh(string name) {
            mf = GetComponent<MeshFilter>();
            if (mf == null) {
                mf = gameObject.AddComponent<MeshFilter>();
            }
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh();
            } else {
                mesh.Clear();
            }
            mesh.name = name;
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mf.mesh = mesh;

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
            } else {
                meshRenderer.enabled = true;
            }
        }

        #region Cone mesh generation

        void GenerateConeMesh() {

            transform.localScale = Vector3.one;

            vertices.Clear();
            indices.Clear();

            generatedType = LightType.Spot;
            generatedTipRadius = profile != null ? profile.tipRadius : 0.1f;
            generatedSpotAngle = lightComp.spotAngle;
            generatedRange = customRange;
            generatedBaseRadius = Mathf.Tan(lightComp.spotAngle * Mathf.Deg2Rad * 0.5f) * customRange;

            Vector3 p = Vector3.zero;

            // Top & Bottom vertices
            for (int k = 0; k < sides; k++) {
                float r = 2f * Mathf.PI * k / sides;
                float cos = Mathf.Cos(r);
                float sin = Mathf.Sin(r);
                p.x = cos * generatedTipRadius;
                p.y = sin * generatedTipRadius;
                p.z = 0;
                vertices.Add(p);
                p.x = cos * generatedBaseRadius;
                p.y = sin * generatedBaseRadius;
                p.z = customRange;
                vertices.Add(p);
            }

            // Build faces
            int vertexCount = sides * 2;
            for (int k = 0; k < vertexCount; k += 2) {
                int tl = k;
                int bl = k + 1;
                int tr = (k + 2) % vertexCount;
                int br = (k + 3) % vertexCount;
                // First tri
                indices.Add(tl);
                indices.Add(tr);
                indices.Add(bl);
                // Second tri
                indices.Add(tr);
                indices.Add(br);
                indices.Add(bl);
            }

            // Build top cap
            vertices.Add(Vector3.zero);
            int topCapCenterIndex = vertexCount;
            for (int k = 0; k < vertexCount; k += 2) {
                // First tri
                indices.Add(k);
                indices.Add(topCapCenterIndex);
                indices.Add((k + 2) % vertexCount);
            }

            // Build bottom cap
            vertices.Add(new Vector3(0, 0, customRange));
            int bottomCapCenterIndex = vertexCount + 1;
            for (int k = 0; k < vertexCount; k += 2) {
                // First tri
                indices.Add(bottomCapCenterIndex);
                indices.Add(k + 1);
                indices.Add((k + 3) % vertexCount);
            }

            UpdateMesh("Capped Cone");
        }

        #endregion

        #region Cube mesh generation

        static Vector3[] faceVerticesForward = {
            new Vector3 (0.5f, -0.5f, 1f),
            new Vector3 (0.5f, 0.5f, 1f),
            new Vector3 (-0.5f, -0.5f, 1f),
            new Vector3 (-0.5f, 0.5f, 1f)
        };
        static Vector3[] faceVerticesBack = {
            new Vector3 (-0.5f, -0.5f, 0f),
            new Vector3 (-0.5f, 0.5f, 0f),
            new Vector3 (0.5f, -0.5f, 0f),
            new Vector3 (0.5f, 0.5f, 0f)
        };
        static Vector3[] faceVerticesLeft = {
            new Vector3 (-0.5f, -0.5f, 1f),
            new Vector3 (-0.5f, 0.5f, 1f),
            new Vector3 (-0.5f, -0.5f, 0f),
            new Vector3 (-0.5f, 0.5f, 0f)
        };
        static Vector3[] faceVerticesRight = {
            new Vector3 (0.5f, -0.5f, 0f),
            new Vector3 (0.5f, 0.5f, 0f),
            new Vector3 (0.5f, -0.5f, 1f),
            new Vector3 (0.5f, 0.5f, 1f)
        };
        static Vector3[] faceVerticesTop =  {
            new Vector3 (-0.5f, 0.5f, 0f),
            new Vector3 (-0.5f, 0.5f, 1f),
            new Vector3 (0.5f, 0.5f, 0f),
            new Vector3 (0.5f, 0.5f, 1f)
        };
        static Vector3[] faceVerticesBottom = {
            new Vector3 (0.5f, -0.5f, 0f),
            new Vector3 (0.5f, -0.5f, 1f),
            new Vector3 (-0.5f, -0.5f, 0f),
            new Vector3 (-0.5f, -0.5f, 1f)
        };

        void GenerateCubeMesh() {

            transform.localScale = Vector3.one;

            generatedType = LightType.Area;
            generatedRange = customRange;
            generatedAreaWidth = areaWidth;
            generatedAreaHeight = areaHeight;
            if (profile == null) {
                generatedAreaFrustumAngle = 0;
                generatedAreaFrustumMultiplier = 1f;
            } else {
                generatedAreaFrustumAngle = profile.frustumAngle;
                generatedAreaFrustumMultiplier = 1f + Mathf.Tan(profile.frustumAngle * Mathf.Deg2Rad);
            }

            vertices.Clear();
            indices.Clear();

            AddFace(faceVerticesBack);
            AddFace(faceVerticesForward);
            AddFace(faceVerticesLeft);
            AddFace(faceVerticesRight);
            AddFace(faceVerticesTop);
            AddFace(faceVerticesBottom);

            UpdateMesh("Box");
        }

        void AddFace(Vector3[] faceVertices) {
            int index = vertices.Count;
            for (int k = 0; k < faceVertices.Length; k++) {
                Vector3 v = faceVertices[k];
                v.x *= generatedAreaWidth;
                if (v.z > 0) {
                    v.x *= generatedAreaFrustumMultiplier;
                    v.y *= generatedAreaFrustumMultiplier;
                }
                v.y *= generatedAreaHeight;
                v.z *= generatedRange;
                vertices.Add(v);
            }
            indices.Add(index);
            indices.Add(index + 1);
            indices.Add(index + 3);
            indices.Add(index + 3);
            indices.Add(index + 2);
            indices.Add(index);
        }

        #endregion

        #region Sphere mesh generation

        void GenerateSphereMesh() {

            transform.localScale = Vector3.one;

            generatedRange = customRange;
            generatedType = LightType.Point;

            vertices.Clear();
            indices.Clear();

            const int nbLong = 24;
            const int nbLat = 16;

            const float _2pi = Mathf.PI * 2f;

            vertices.Add(Vector3.up * generatedRange);
            for (int lat = 0; lat < nbLat; lat++) {
                float a1 = Mathf.PI * (float)(lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++) {
                    float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices.Add(new Vector3(sin1 * cos2, cos1, sin1 * sin2) * generatedRange);
                }
            }
            vertices.Add(Vector3.down * generatedRange);

            //Top Cap
            for (int lon = 0; lon < nbLong; lon++) {
                indices.Add(lon + 2);
                indices.Add(lon + 1);
                indices.Add(0);
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++) {
                for (int lon = 0; lon < nbLong; lon++) {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    indices.Add(current);
                    indices.Add(current + 1);
                    indices.Add(next + 1);

                    indices.Add(current);
                    indices.Add(next + 1);
                    indices.Add(next);
                }
            }

            //Bottom Cap
            int vertexCount = vertices.Count;
            for (int lon = 0; lon < nbLong; lon++) {
                indices.Add(vertexCount - 1);
                indices.Add(vertexCount - (lon + 2) - 1);
                indices.Add(vertexCount - (lon + 1) - 1);
            }

            UpdateMesh("Sphere");

        }

        #endregion

        #region Cylinder mesh generation

        void GenerateCylinderMesh() {

            transform.localScale = Vector3.one;

            generatedType = LightType.Disc;
            generatedRange = customRange;
            generatedAreaWidth = generatedAreaHeight = areaWidth;
            if (profile == null) {
                generatedAreaFrustumAngle = 0;
                generatedAreaFrustumMultiplier = 1f;
            } else {
                generatedAreaFrustumAngle = profile.frustumAngle;
                generatedAreaFrustumMultiplier = 1f + Mathf.Tan(profile.frustumAngle * Mathf.Deg2Rad);
            }

            vertices.Clear();
            indices.Clear();

            Vector3 p = Vector3.zero;

            // Top & Bottom vertices
            for (int k = 0; k < sides; k++) {
                float r = 2f * Mathf.PI * k / sides;
                float cos = Mathf.Cos(r);
                float sin = Mathf.Sin(r);
                p.x = cos * generatedAreaWidth;
                p.y = sin * generatedAreaWidth;
                p.z = 0;
                vertices.Add(p);
                p.x *= generatedAreaFrustumMultiplier;
                p.y *= generatedAreaFrustumMultiplier;
                p.z = generatedRange;
                vertices.Add(p);
            }

            // Build faces
            int vertexCount = sides * 2;
            for (int k = 0; k < vertexCount; k += 2) {
                int tl = k;
                int bl = k + 1;
                int tr = (k + 2) % vertexCount;
                int br = (k + 3) % vertexCount;
                // First tri
                indices.Add(tl);
                indices.Add(tr);
                indices.Add(bl);
                // Second tri
                indices.Add(tr);
                indices.Add(br);
                indices.Add(bl);
            }

            // Build top cap
            vertices.Add(Vector3.zero);
            int topCapCenterIndex = vertexCount;
            for (int k = 0; k < vertexCount; k += 2) {
                // First tri
                indices.Add(k);
                indices.Add(topCapCenterIndex);
                indices.Add((k + 2) % vertexCount);
            }

            // Build bottom cap
            vertices.Add(new Vector3(0, 0, generatedRange));
            int bottomCapCenterIndex = vertexCount + 1;
            for (int k = 0; k < vertexCount; k += 2) {
                // First tri
                indices.Add(bottomCapCenterIndex);
                indices.Add(k + 1);
                indices.Add((k + 3) % vertexCount);
            }

            UpdateMesh("Cylinder");
        }

        #endregion

    }


}
