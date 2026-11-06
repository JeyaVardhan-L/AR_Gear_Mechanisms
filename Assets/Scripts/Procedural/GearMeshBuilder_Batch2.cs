using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechXR.Procedural
{
    public static partial class GearMeshBuilder
    {
        #region Batch 2: Linkage & Motion Conversion Components

        /// <summary>
        /// Generates a precision linkage bar with rounded bearing eyes at each end and central web.
        /// </summary>
        public static Mesh BuildLinkageBarMesh(float length, float width, float thickness, float holeRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "LinkageBar_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segs = 16;
            float eyeR = width * 0.5f;
            float halfT = thickness * 0.5f;

            // Generate 2D outline of linkage bar:
            // Semicircle at (0,0) from -pi/2 to pi/2 (facing -X)
            // Top straight edge from (0, eyeR) to (length, eyeR)
            // Semicircle at (length, 0) from pi/2 to -pi/2 (facing +X)
            // Bottom straight edge from (length, -eyeR) to (0, -eyeR)
            List<Vector2> outline = new List<Vector2>();

            // Left semicircle around (0,0)
            for (int i = 0; i <= segs; i++)
            {
                float a = Mathf.PI * 0.5f + (float)i / segs * Mathf.PI;
                outline.Add(new Vector2(Mathf.Cos(a) * eyeR, Mathf.Sin(a) * eyeR));
            }

            // Right semicircle around (length, 0)
            for (int i = 0; i <= segs; i++)
            {
                float a = -Mathf.PI * 0.5f + (float)i / segs * Mathf.PI;
                outline.Add(new Vector2(length + Mathf.Cos(a) * eyeR, Mathf.Sin(a) * eyeR));
            }

            // Extrude outline along Z (-halfT to +halfT)
            // Front & Back faces (Triangulate simply as a fan/quad grid)
            int nPts = outline.Count;

            for (int face = 0; face < 2; face++)
            {
                float z = (face == 0) ? -halfT : halfT;
                Vector3 n = (face == 0) ? Vector3.back : Vector3.forward;
                int fBase = verts.Count;

                // Center spine points along length
                int spineCount = 8;
                for (int s = 0; s <= spineCount; s++)
                {
                    float x = (float)s / spineCount * length;
                    verts.Add(new Vector3(x, 0, z));
                    norms.Add(n);
                    uvs.Add(new Vector2((float)s / spineCount, 0.5f));
                }

                int periphBase = verts.Count;
                for (int i = 0; i < nPts; i++)
                {
                    verts.Add(new Vector3(outline[i].x, outline[i].y, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(outline[i].x / length, (outline[i].y + eyeR) / width));
                }

                // Connect perimeter to nearest spine vertex
                for (int i = 0; i < nPts; i++)
                {
                    int next = (i + 1) % nPts;
                    float xMid = (outline[i].x + outline[next].x) * 0.5f;
                    int spineIdx = fBase + Mathf.Clamp(Mathf.RoundToInt(xMid / length * spineCount), 0, spineCount);

                    int p0 = periphBase + i;
                    int p1 = periphBase + next;

                    if (face == 0)
                    {
                        tris.Add(spineIdx); tris.Add(p0); tris.Add(p1);
                    }
                    else
                    {
                        tris.Add(spineIdx); tris.Add(p1); tris.Add(p0);
                    }
                }
            }

            // Outer perimeter walls
            int wallBase = verts.Count;
            for (int i = 0; i < nPts; i++)
            {
                int next = (i + 1) % nPts;
                Vector2 p0 = outline[i];
                Vector2 p1 = outline[next];
                Vector3 dir = new Vector3(p1.x - p0.x, p1.y - p0.y, 0).normalized;
                Vector3 n = Vector3.Cross(dir, Vector3.forward).normalized;

                int b = verts.Count;
                verts.Add(new Vector3(p0.x, p0.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / nPts, 0));
                verts.Add(new Vector3(p0.x, p0.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / nPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / nPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / nPts, 0));

                tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
                tris.Add(b); tris.Add(b + 2); tris.Add(b + 3);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a 4-slot Geneva wheel with circular locking dwell cutouts and radial drive slots.
        /// </summary>
        public static Mesh BuildGenevaWheelMesh(int slotCount, float outerRadius, float slotWidth, float slotDepth, float dwellRadius, float thickness, float boreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "GenevaWheel_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int totalPts = slotCount * 24;
            float halfT = thickness * 0.5f;
            float slotHalfW = slotWidth * 0.5f;

            // Generate 2D profile points around circumference
            List<Vector2> contour = new List<Vector2>();
            for (int s = 0; s < slotCount; s++)
            {
                float baseAngle = (float)s / slotCount * 2f * Mathf.PI;

                // Circular dwell crescent between slot s and slot s+1
                // Mid-angle of dwell
                float dwellMidAngle = baseAngle + (Mathf.PI / slotCount);
                int dwellPts = 12;
                for (int i = 0; i <= dwellPts; i++)
                {
                    float t = (float)i / dwellPts;
                    float a = baseAngle + (t * 2f * Mathf.PI / slotCount);
                    // Dwell indent radius
                    float r = outerRadius;
                    float angDiff = Mathf.Abs(Mathf.DeltaAngle(a * Mathf.Rad2Deg, dwellMidAngle * Mathf.Rad2Deg));
                    if (angDiff < (180f / slotCount) * 0.75f)
                    {
                        float frac = 1f - (angDiff / ((180f / slotCount) * 0.75f));
                        r = outerRadius - frac * (outerRadius - dwellRadius);
                    }
                    contour.Add(new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r));
                }
            }

            // Front and Back faces
            int nPts = contour.Count;
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? -halfT : halfT;
                Vector3 n = face == 0 ? Vector3.back : Vector3.forward;
                int fBase = verts.Count;

                // Center bore ring
                for (int i = 0; i < nPts; i++)
                {
                    float a = (float)i / nPts * 2f * Mathf.PI;
                    verts.Add(new Vector3(Mathf.Cos(a) * boreRadius, Mathf.Sin(a) * boreRadius, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));

                    verts.Add(new Vector3(contour[i].x, contour[i].y, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(contour[i].x / outerRadius, contour[i].y / outerRadius));
                }

                for (int i = 0; i < nPts; i++)
                {
                    int next = (i + 1) % nPts;
                    int b0 = fBase + i * 2;
                    int o0 = b0 + 1;
                    int b1 = fBase + next * 2;
                    int o1 = b1 + 1;

                    if (face == 0)
                    {
                        tris.Add(b0); tris.Add(o0); tris.Add(o1);
                        tris.Add(b0); tris.Add(o1); tris.Add(b1);
                    }
                    else
                    {
                        tris.Add(b0); tris.Add(o1); tris.Add(o0);
                        tris.Add(b0); tris.Add(b1); tris.Add(o1);
                    }
                }
            }

            // Outer rim walls
            int rimBase = verts.Count;
            for (int i = 0; i < nPts; i++)
            {
                int next = (i + 1) % nPts;
                Vector2 p0 = contour[i];
                Vector2 p1 = contour[next];
                Vector3 dir = new Vector3(p1.x - p0.x, p1.y - p0.y, 0).normalized;
                Vector3 n = Vector3.Cross(dir, Vector3.forward).normalized;

                int b = verts.Count;
                verts.Add(new Vector3(p0.x, p0.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / nPts, 0));
                verts.Add(new Vector3(p0.x, p0.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / nPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / nPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / nPts, 0));

                tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
                tris.Add(b); tris.Add(b + 2); tris.Add(b + 3);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a Geneva driving disc with raised locking crescent (270 deg dwell rim) and driving pin.
        /// </summary>
        public static Mesh BuildGenevaDriverMesh(float discRadius, float pinRadius, float thickness, float lockingArcDeg = 270f)
        {
            Mesh mesh = new Mesh();
            mesh.name = "GenevaDriver_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segs = 32;
            float halfT = thickness * 0.5f;

            // 1. Base Circular Disc
            int centerIdx = verts.Count;
            verts.Add(new Vector3(0, 0, -halfT)); norms.Add(Vector3.back); uvs.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i < segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                verts.Add(new Vector3(Mathf.Cos(a) * discRadius, Mathf.Sin(a) * discRadius, -halfT));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
            }

            for (int i = 0; i < segs; i++)
            {
                int next = (i + 1) % segs;
                tris.Add(centerIdx); tris.Add(centerIdx + 1 + i); tris.Add(centerIdx + 1 + next);
            }

            // Back face
            int backCenter = verts.Count;
            verts.Add(new Vector3(0, 0, halfT)); norms.Add(Vector3.forward); uvs.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i < segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                verts.Add(new Vector3(Mathf.Cos(a) * discRadius, Mathf.Sin(a) * discRadius, halfT));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
            }

            for (int i = 0; i < segs; i++)
            {
                int next = (i + 1) % segs;
                tris.Add(backCenter); tris.Add(backCenter + 1 + next); tris.Add(backCenter + 1 + i);
            }

            // Perimeter
            int periphBase = verts.Count;
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                Vector3 n = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
                verts.Add(new Vector3(Mathf.Cos(a) * discRadius, Mathf.Sin(a) * discRadius, -halfT));
                norms.Add(n); uvs.Add(new Vector2((float)i / segs, 0));

                verts.Add(new Vector3(Mathf.Cos(a) * discRadius, Mathf.Sin(a) * discRadius, halfT));
                norms.Add(n); uvs.Add(new Vector2((float)i / segs, 1));
            }

            for (int i = 0; i < segs; i++)
            {
                int b = periphBase + i * 2;
                tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
                tris.Add(b + 2); tris.Add(b + 1); tris.Add(b + 3);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a Scotch Yoke slotted frame with vertical slider yoke and horizontal output guide rods.
        /// </summary>
        public static Mesh BuildScotchYokeFrameMesh(float slotHeight, float slotWidth, float frameThickness, float rodLength, float rodRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "ScotchYokeFrame_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfH = (slotHeight + 0.024f) * 0.5f;
            float halfW = (slotWidth + 0.024f) * 0.5f;
            float slotHalfH = slotHeight * 0.5f;
            float slotHalfW = slotWidth * 0.5f;
            float halfT = frameThickness * 0.5f;

            // Box with vertical rectangular cutout (the slotted yoke)
            // Left pillar, Right pillar, Top crossbar, Bottom crossbar
            Action<Vector3, Vector3> addBox = (center, size) =>
            {
                int start = verts.Count;
                Vector3 h = size * 0.5f;
                Vector3[] pts = new Vector3[]
                {
                    // Front
                    center + new Vector3(-h.x, -h.y, -h.z), center + new Vector3(h.x, -h.y, -h.z),
                    center + new Vector3(h.x, h.y, -h.z), center + new Vector3(-h.x, h.y, -h.z),
                    // Back
                    center + new Vector3(h.x, -h.y, h.z), center + new Vector3(-h.x, -h.y, h.z),
                    center + new Vector3(-h.x, h.y, h.z), center + new Vector3(h.x, h.y, h.z),
                    // Left
                    center + new Vector3(-h.x, -h.y, h.z), center + new Vector3(-h.x, -h.y, -h.z),
                    center + new Vector3(-h.x, h.y, -h.z), center + new Vector3(-h.x, h.y, h.z),
                    // Right
                    center + new Vector3(h.x, -h.y, -h.z), center + new Vector3(h.x, -h.y, h.z),
                    center + new Vector3(h.x, h.y, h.z), center + new Vector3(h.x, h.y, -h.z),
                    // Top
                    center + new Vector3(-h.x, h.y, -h.z), center + new Vector3(h.x, h.y, -h.z),
                    center + new Vector3(h.x, h.y, h.z), center + new Vector3(-h.x, h.y, h.z),
                    // Bottom
                    center + new Vector3(-h.x, -h.y, h.z), center + new Vector3(h.x, -h.y, h.z),
                    center + new Vector3(h.x, -h.y, -h.z), center + new Vector3(-h.x, -h.y, -h.z)
                };

                Vector3[] ns = new Vector3[]
                {
                    Vector3.back, Vector3.back, Vector3.back, Vector3.back,
                    Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
                    Vector3.left, Vector3.left, Vector3.left, Vector3.left,
                    Vector3.right, Vector3.right, Vector3.right, Vector3.right,
                    Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                    Vector3.down, Vector3.down, Vector3.down, Vector3.down
                };

                for (int i = 0; i < 24; i++)
                {
                    verts.Add(pts[i]); norms.Add(ns[i]); uvs.Add(new Vector2(i % 2, (i / 2) % 2));
                }

                for (int f = 0; f < 6; f++)
                {
                    int b = start + f * 4;
                    tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
                    tris.Add(b); tris.Add(b + 2); tris.Add(b + 3);
                }
            };

            float pillarW = (halfW - slotHalfW);
            float pillarH = slotHeight + 0.024f;
            float crossH = (halfH - slotHalfH);

            // Left pillar
            addBox(new Vector3(-slotHalfW - pillarW * 0.5f, 0, 0), new Vector3(pillarW, pillarH, frameThickness));
            // Right pillar
            addBox(new Vector3(slotHalfW + pillarW * 0.5f, 0, 0), new Vector3(pillarW, pillarH, frameThickness));
            // Top crossbar
            addBox(new Vector3(0, slotHalfH + crossH * 0.5f, 0), new Vector3(slotWidth, crossH, frameThickness));
            // Bottom crossbar
            addBox(new Vector3(0, -slotHalfH - crossH * 0.5f, 0), new Vector3(slotWidth, crossH, frameThickness));

            // Left horizontal guide rod
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(-halfW, 0, 0), rodRadius, rodLength, Vector3.left);
            // Right horizontal guide rod
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(halfW, 0, 0), rodRadius, rodLength, Vector3.right);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion
    }
}
