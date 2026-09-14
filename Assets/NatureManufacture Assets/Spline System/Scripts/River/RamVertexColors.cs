using System.Collections.Generic;
using UnityEngine;

namespace NatureManufacture.RAM
{
    public class RamVertexColors
    {
        private RamSpline ramSpline;

        public RamVertexColors(RamSpline ramSpline)
        {
            this.ramSpline = ramSpline;
        }

        public Color[] ManageVertexColors(Color[] colors, Vector3[] normals, int vertCount)
        {
            if (colors == null || colors.Length != vertCount)
            {
                colors = new Color[vertCount];
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.black;
            }


            if (ramSpline.VertexPainterData.OverridenColors) return colors;


            for (int i = 0; i < vertCount; i++)
            {
                float red = ramSpline.BaseProfile.redColorCurve.Evaluate(normals[i].y);
                float green = ramSpline.BaseProfile.greenColorCurve.Evaluate(normals[i].y);
                float blue = ramSpline.BaseProfile.blueColorCurve.Evaluate(normals[i].y);
                float alpha = ramSpline.BaseProfile.alphaColorCurve.Evaluate(normals[i].y);

                colors[i] = new Color(red, green, blue, alpha);
            }


            return colors;
        }

        public void GetLakesBlend(MeshRenderer meshRenderer)
        {
            //find all lakes that bounds collide with ramspline
            LakePolygon[] lakes = Object.FindObjectsByType<LakePolygon>(FindObjectsSortMode.None);
            var ramMeshRenderer = ramSpline.GetComponent<MeshRenderer>();


            var ray = new Ray
            {
                direction = Vector3.down
            };


            ramSpline.VertexPainterData.OverridenColors = true;

            foreach (LakePolygon lake in lakes)
            {
                if (!ramMeshRenderer.bounds.Intersects(lake.GetComponent<MeshRenderer>().bounds)) continue;


                Debug.Log($"Lake bounds intersects with ram spline bounds {lake.name}");

                var meshCollider = lake.gameObject.AddComponent<MeshCollider>();

                foreach (var point in ramSpline.NmSpline.Points)
                {
                    Vector3 position = point.Position + ramSpline.transform.position;
                    ray.origin = position + Vector3.up * 1000;

                    //distance = Vector3.Distance(vertices[i], meshCollider.ClosestPoint(vertices[i]));

                    bool hited = meshCollider.Raycast(ray, out RaycastHit hit, 2000);

                    if (!hited) continue;


                    float yDistance = position.y - hit.point.y;


                    //Debug.Log(Mathf.Clamp01(yDistance));
                }

                Object.DestroyImmediate(meshCollider);
            }
        }

        public static void GenerateBlendWithLakePolygon(RamSpline ramSpline)
        {
            //find all lakes that bounds collide with ramspline
            LakePolygon[] lakes = Object.FindObjectsByType <LakePolygon>(FindObjectsSortMode.None);
            var ramMeshRenderer = ramSpline.GetComponent<MeshRenderer>();

            Mesh mesh = ramSpline.meshFilter.sharedMesh;

            Vector3[] vertices = mesh.vertices;

            Color[] colors = mesh.colors;
            //List<Vector4> uv4 = new();
            /*mesh.GetUVs(4, uv4);

            if (uv4.Count == 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    uv4.Add(Vector4.zero);
                }
            }*/


            float vertLength = vertices.Length;
            var ray = new Ray
            {
                direction = Vector3.down
            };

            ramSpline.VertexPainterData.OverridenColors = true;

            foreach (LakePolygon lake in lakes)
            {
                if (!ramMeshRenderer.bounds.Intersects(lake.GetComponent<MeshRenderer>().bounds)) continue;


                Debug.Log($"Lake bounds intersects with ram spline bounds {lake.name}");

                var meshCollider = lake.gameObject.AddComponent<MeshCollider>();

                for (int i = 0; i < vertLength; i++)
                {
                    ray.origin = vertices[i] + Vector3.up * 1000;

                    //distance = Vector3.Distance(vertices[i], meshCollider.ClosestPoint(vertices[i]));

                    bool hited = meshCollider.Raycast(ray, out RaycastHit hit, 2000);

                    if (!hited) continue;


                    float yDistance = vertices[i].y - hit.point.y;


                    colors[i].a = Mathf.Clamp01(yDistance);
                    //colors[i].r = Mathf.Clamp01(yDistance);

                    // uv4[i] = new Vector4(-yDistance, 0, 0, 0);
                }

                Object.DestroyImmediate(meshCollider);
            }

            mesh.SetColors(colors);
            // mesh.SetUVs(4, uv4);
        }
    }
}