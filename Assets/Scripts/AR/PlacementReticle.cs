using UnityEngine;

namespace MechXR.AR
{
    /// <summary>
    /// Procedurally creates and animates an engineering-style AR placement reticle.
    /// Indicates valid detected planar surfaces with an electric-blue target ring.
    /// </summary>
    public class PlacementReticle : MonoBehaviour
    {
        [SerializeField] private float outerRadius = 0.12f;
        [SerializeField] private float innerRadius = 0.09f;
        [SerializeField] private int segments = 40;
        [SerializeField] private Color reticleColor = new Color(0.0f, 0.85f, 1.0f, 0.85f); // Electric Blue

        private MeshRenderer meshRenderer;

        private void Awake()
        {
            BuildReticleMesh();
        }

        private void BuildReticleMesh()
        {
            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.name = "ARPlacementReticleMesh";

            Vector3[] verts = new Vector3[segments * 2];
            Vector3[] norms = new Vector3[segments * 2];
            Vector2[] uvs = new Vector2[segments * 2];
            int[] tris = new int[segments * 6];

            float angleStep = 2f * Mathf.PI / segments;
            for (int i = 0; i < segments; i++)
            {
                float ang = i * angleStep;
                float cos = Mathf.Cos(ang);
                float sin = Mathf.Sin(ang);

                verts[i * 2 + 0] = new Vector3(cos * innerRadius, 0.001f, sin * innerRadius);
                verts[i * 2 + 1] = new Vector3(cos * outerRadius, 0.001f, sin * outerRadius);

                norms[i * 2 + 0] = Vector3.up;
                norms[i * 2 + 1] = Vector3.up;

                uvs[i * 2 + 0] = new Vector2(0.5f + cos * 0.4f, 0.5f + sin * 0.4f);
                uvs[i * 2 + 1] = new Vector2(0.5f + cos * 0.5f, 0.5f + sin * 0.5f);

                int next = (i + 1) % segments;
                int idx = i * 6;

                tris[idx + 0] = i * 2 + 0;
                tris[idx + 1] = i * 2 + 1;
                tris[idx + 2] = next * 2 + 1;

                tris[idx + 3] = i * 2 + 0;
                tris[idx + 4] = next * 2 + 1;
                tris[idx + 5] = next * 2 + 0;
            }

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();

            mf.mesh = mesh;

            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader != null ? shader : Shader.Find("Diffuse"));
            mat.color = reticleColor;
            meshRenderer.material = mat;
        }

        public void SetVisible(bool visible)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = visible;
            }
        }
    }
}

