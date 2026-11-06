using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechXR.Procedural
{
    public static partial class GearMeshBuilder
    {
        #region Batch 3: Power Transmission & Drive Components

        /// <summary>
        /// Generates a Cardan / Universal Joint fork yoke with dual prongs and journal bores.
        /// </summary>
        public static Mesh BuildCardanYokeMesh(float hubRadius, float hubLength, float forkSpan, float forkThickness, float pinBoreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "CardanYoke_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfSpan = forkSpan * 0.5f;
            float prongLen = 0.035f;

            // 1. Hub Cylinder along Z (Z: -hubLength to 0)
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(0, 0, -hubLength), hubRadius, hubLength, Vector3.forward);

            // 2. Left and Right Fork Prongs (along +Z from Z=0 to Z=+prongLen, positioned at +X and -X)
            float prongW = 0.012f;
            Action<float> addProng = (sign) =>
            {
                float xBase = sign * halfSpan;
                int b = verts.Count;
                Vector3 sz = new Vector3(prongW, forkThickness, prongLen);
                Vector3 h = sz * 0.5f;
                Vector3 c = new Vector3(xBase, 0, prongLen * 0.5f);

                Vector3[] pts = new Vector3[]
                {
                    // Front
                    c + new Vector3(-h.x, -h.y, -h.z), c + new Vector3(h.x, -h.y, -h.z),
                    c + new Vector3(h.x, h.y, -h.z), c + new Vector3(-h.x, h.y, -h.z),
                    // Back
                    c + new Vector3(h.x, -h.y, h.z), c + new Vector3(-h.x, -h.y, h.z),
                    c + new Vector3(-h.x, h.y, h.z), c + new Vector3(h.x, h.y, h.z),
                    // Left
                    c + new Vector3(-h.x, -h.y, h.z), c + new Vector3(-h.x, -h.y, -h.z),
                    c + new Vector3(-h.x, h.y, -h.z), c + new Vector3(-h.x, h.y, h.z),
                    // Right
                    c + new Vector3(h.x, -h.y, -h.z), c + new Vector3(h.x, -h.y, h.z),
                    c + new Vector3(h.x, h.y, h.z), c + new Vector3(h.x, h.y, -h.z),
                    // Top
                    c + new Vector3(-h.x, h.y, -h.z), c + new Vector3(h.x, h.y, -h.z),
                    c + new Vector3(h.x, h.y, h.z), c + new Vector3(-h.x, h.y, h.z),
                    // Bottom
                    c + new Vector3(-h.x, -h.y, h.z), c + new Vector3(h.x, -h.y, h.z),
                    c + new Vector3(h.x, -h.y, -h.z), c + new Vector3(-h.x, -h.y, -h.z)
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
                    int idx = b + f * 4;
                    tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
                    tris.Add(idx); tris.Add(idx + 2); tris.Add(idx + 3);
                }
            };

            addProng(-1f);
            addProng(1f);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a 4-armed Cardan spider / cross trunnion.
        /// </summary>
        public static Mesh BuildCardanSpiderMesh(float pinRadius, float span)
        {
            Mesh mesh = new Mesh();
            mesh.name = "CardanSpider_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfSpan = span * 0.5f;

            // Horizontal cross pin along X (-halfSpan to +halfSpan)
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(-halfSpan, 0, 0), pinRadius, span, Vector3.right);

            // Vertical cross pin along Y (-halfSpan to +halfSpan)
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(0, -halfSpan, 0), pinRadius, span, Vector3.up);

            // Central cube block
            float cubeH = pinRadius * 2.4f;
            Vector3 h = Vector3.one * cubeH * 0.5f;
            int b = verts.Count;
            Vector3[] pts = new Vector3[]
            {
                new Vector3(-h.x,-h.y,-h.z), new Vector3(h.x,-h.y,-h.z), new Vector3(h.x,h.y,-h.z), new Vector3(-h.x,h.y,-h.z),
                new Vector3(h.x,-h.y,h.z), new Vector3(-h.x,-h.y,h.z), new Vector3(-h.x,h.y,h.z), new Vector3(h.x,h.y,h.z)
            };
            // Front & Back
            for (int f = 0; f < 2; f++)
            {
                int baseF = verts.Count;
                float z = f == 0 ? -h.z : h.z;
                Vector3 n = f == 0 ? Vector3.back : Vector3.forward;
                verts.Add(new Vector3(-h.x, -h.y, z)); norms.Add(n); uvs.Add(new Vector2(0, 0));
                verts.Add(new Vector3(h.x, -h.y, z)); norms.Add(n); uvs.Add(new Vector2(1, 0));
                verts.Add(new Vector3(h.x, h.y, z)); norms.Add(n); uvs.Add(new Vector2(1, 1));
                verts.Add(new Vector3(-h.x, h.y, z)); norms.Add(n); uvs.Add(new Vector2(0, 1));

                if (f == 0)
                {
                    tris.Add(baseF); tris.Add(baseF + 1); tris.Add(baseF + 2);
                    tris.Add(baseF); tris.Add(baseF + 2); tris.Add(baseF + 3);
                }
                else
                {
                    tris.Add(baseF); tris.Add(baseF + 2); tris.Add(baseF + 1);
                    tris.Add(baseF); tris.Add(baseF + 3); tris.Add(baseF + 2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates an ANSI/DIN roller chain sprocket with curved tooth flanks and roller seating roots.
        /// </summary>
        public static Mesh BuildSprocketMesh(int teeth, float pitch, float rollerRadius, float thickness, float boreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Sprocket_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            // Pitch diameter D = p / sin(pi / Z)
            float pitchRadius = (pitch / Mathf.Sin(Mathf.PI / teeth)) * 0.5f;
            float rootRadius = pitchRadius - rollerRadius;
            float tipRadius = pitchRadius + rollerRadius * 1.1f;
            float halfT = thickness * 0.5f;

            int ptsPerTooth = 6;
            int totalPts = teeth * ptsPerTooth;
            Vector2[] contour = new Vector2[totalPts];

            for (int t = 0; t < teeth; t++)
            {
                float baseA = (float)t / teeth * 2f * Mathf.PI;
                float stepA = (2f * Mathf.PI / teeth);

                for (int p = 0; p < ptsPerTooth; p++)
                {
                    float frac = (float)p / ptsPerTooth;
                    float a = baseA + frac * stepA;
                    float r = rootRadius;

                    // Tooth shape: root valley at p=0, tooth tip at p=3
                    if (p == 0) r = rootRadius;
                    else if (p == 1) r = pitchRadius * 0.95f;
                    else if (p == 2 || p == 3) r = tipRadius;
                    else if (p == 4) r = pitchRadius * 0.95f;
                    else r = rootRadius;

                    contour[t * ptsPerTooth + p] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
                }
            }

            // Front & Back faces
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? -halfT : halfT;
                Vector3 n = face == 0 ? Vector3.back : Vector3.forward;
                int fBase = verts.Count;

                for (int i = 0; i < totalPts; i++)
                {
                    float a = (float)i / totalPts * 2f * Mathf.PI;
                    verts.Add(new Vector3(Mathf.Cos(a) * boreRadius, Mathf.Sin(a) * boreRadius, z));
                    norms.Add(n); uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));

                    verts.Add(new Vector3(contour[i].x, contour[i].y, z));
                    norms.Add(n); uvs.Add(new Vector2(contour[i].x / tipRadius, contour[i].y / tipRadius));
                }

                for (int i = 0; i < totalPts; i++)
                {
                    int next = (i + 1) % totalPts;
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

            // Tooth Rim Perimeter
            int rimBase = verts.Count;
            for (int i = 0; i < totalPts; i++)
            {
                int next = (i + 1) % totalPts;
                Vector2 p0 = contour[i];
                Vector2 p1 = contour[next];
                Vector3 dir = new Vector3(p1.x - p0.x, p1.y - p0.y, 0).normalized;
                Vector3 n = Vector3.Cross(dir, Vector3.forward).normalized;

                int b = verts.Count;
                verts.Add(new Vector3(p0.x, p0.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / totalPts, 0));
                verts.Add(new Vector3(p0.x, p0.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / totalPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / totalPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / totalPts, 0));

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
        /// Generates a continuous roller chain loop track connecting two sprockets at center distance C.
        /// </summary>
        public static Mesh BuildRollerChainLoopMesh(float centerDist, float r1, float r2, float chainThickness, float chainWidth)
        {
            Mesh mesh = new Mesh();
            mesh.name = "RollerChainLoop_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segsPerArc = 20;
            int segsPerStrand = 16;
            float halfW = chainWidth * 0.5f;

            // Generate closed 2D loop path
            List<Vector2> path = new List<Vector2>();

            // Arc around Sprocket 1 (at 0,0) from pi/2 to 3pi/2
            for (int i = 0; i <= segsPerArc; i++)
            {
                float a = Mathf.PI * 0.5f + (float)i / segsPerArc * Mathf.PI;
                path.Add(new Vector2(Mathf.Cos(a) * r1, Mathf.Sin(a) * r1));
            }

            // Lower straight strand from (0, -r1) to (centerDist, -r2)
            for (int i = 1; i < segsPerStrand; i++)
            {
                float t = (float)i / segsPerStrand;
                path.Add(new Vector2(t * centerDist, -Mathf.Lerp(r1, r2, t)));
            }

            // Arc around Sprocket 2 (at centerDist, 0) from -pi/2 to +pi/2
            for (int i = 0; i <= segsPerArc; i++)
            {
                float a = -Mathf.PI * 0.5f + (float)i / segsPerArc * Mathf.PI;
                path.Add(new Vector2(centerDist + Mathf.Cos(a) * r2, Mathf.Sin(a) * r2));
            }

            // Upper straight strand from (centerDist, r2) to (0, r1)
            for (int i = 1; i < segsPerStrand; i++)
            {
                float t = (float)i / segsPerStrand;
                path.Add(new Vector2((1f - t) * centerDist, Mathf.Lerp(r2, r1, t)));
            }

            // Extrude ribbon around path
            int nPts = path.Count;
            for (int i = 0; i <= nPts; i++)
            {
                int idx = i % nPts;
                Vector2 pt = path[idx];

                verts.Add(new Vector3(pt.x, pt.y, -halfW));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2((float)i / nPts, 0));

                verts.Add(new Vector3(pt.x, pt.y, halfW));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2((float)i / nPts, 1));
            }

            for (int i = 0; i < nPts; i++)
            {
                int b = i * 2;
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
        /// Generates an asymmetric sawtooth Ratchet Wheel mesh.
        /// </summary>
        public static Mesh BuildRatchetWheelMesh(int teeth, float radius, float toothDepth, float thickness, float boreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "RatchetWheel_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int totalPts = teeth * 2;
            Vector2[] contour = new Vector2[totalPts];
            float halfT = thickness * 0.5f;

            for (int i = 0; i < teeth; i++)
            {
                float a0 = (float)i / teeth * 2f * Mathf.PI;
                float a1 = (float)(i + 1) / teeth * 2f * Mathf.PI;

                // Tooth tip at a0, radius R
                contour[i * 2] = new Vector2(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius);

                // Tooth root steep face at a0 + tiny offset, radius R - depth
                float aRoot = a0 + (2f * Mathf.PI / teeth) * 0.08f;
                contour[i * 2 + 1] = new Vector2(Mathf.Cos(aRoot) * (radius - toothDepth), Mathf.Sin(aRoot) * (radius - toothDepth));
            }

            // Front & Back faces
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? -halfT : halfT;
                Vector3 n = face == 0 ? Vector3.back : Vector3.forward;
                int fBase = verts.Count;

                for (int i = 0; i < totalPts; i++)
                {
                    float a = (float)i / totalPts * 2f * Mathf.PI;
                    verts.Add(new Vector3(Mathf.Cos(a) * boreRadius, Mathf.Sin(a) * boreRadius, z));
                    norms.Add(n); uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));

                    verts.Add(new Vector3(contour[i].x, contour[i].y, z));
                    norms.Add(n); uvs.Add(new Vector2(contour[i].x / radius, contour[i].y / radius));
                }

                for (int i = 0; i < totalPts; i++)
                {
                    int next = (i + 1) % totalPts;
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

            // Perimeter walls
            int wallBase = verts.Count;
            for (int i = 0; i < totalPts; i++)
            {
                int next = (i + 1) % totalPts;
                Vector2 p0 = contour[i];
                Vector2 p1 = contour[next];
                Vector3 dir = new Vector3(p1.x - p0.x, p1.y - p0.y, 0).normalized;
                Vector3 n = Vector3.Cross(dir, Vector3.forward).normalized;

                int b = verts.Count;
                verts.Add(new Vector3(p0.x, p0.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / totalPts, 0));
                verts.Add(new Vector3(p0.x, p0.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)i / totalPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / totalPts, 1));
                verts.Add(new Vector3(p1.x, p1.y, -halfT)); norms.Add(n); uvs.Add(new Vector2((float)(i + 1) / totalPts, 0));

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
        /// Generates a spring-loaded locking pawl arm.
        /// </summary>
        public static Mesh BuildPawlMesh(float length, float width, float noseDepth, float thickness)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Pawl_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfT = thickness * 0.5f;
            float halfW = width * 0.5f;

            // 2D Profile: Pivot at (0,0), Arm extends to (length, 0), Hook nose hooks downward to (length, -noseDepth)
            Vector2[] pts = new Vector2[]
            {
                new Vector2(-halfW, -halfW),
                new Vector2(-halfW, halfW),
                new Vector2(length, halfW),
                new Vector2(length, -noseDepth),
                new Vector2(length - 0.008f, -noseDepth),
                new Vector2(length - 0.008f, -halfW)
            };

            int n = pts.Length;
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? -halfT : halfT;
                Vector3 norm = face == 0 ? Vector3.back : Vector3.forward;
                int fBase = verts.Count;

                for (int i = 0; i < n; i++)
                {
                    verts.Add(new Vector3(pts[i].x, pts[i].y, z));
                    norms.Add(norm);
                    uvs.Add(new Vector2(pts[i].x / length, pts[i].y / width));
                }

                // Simple polygon triangulation fan
                for (int i = 1; i < n - 1; i++)
                {
                    if (face == 0)
                    {
                        tris.Add(fBase); tris.Add(fBase + i); tris.Add(fBase + i + 1);
                    }
                    else
                    {
                        tris.Add(fBase); tris.Add(fBase + i + 1); tris.Add(fBase + i);
                    }
                }
            }

            // Perimeter
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                Vector2 p0 = pts[i];
                Vector2 p1 = pts[next];
                Vector3 dir = new Vector3(p1.x - p0.x, p1.y - p0.y, 0).normalized;
                Vector3 nrm = Vector3.Cross(dir, Vector3.forward).normalized;

                int b = verts.Count;
                verts.Add(new Vector3(p0.x, p0.y, -halfT)); norms.Add(nrm); uvs.Add(new Vector2((float)i / n, 0));
                verts.Add(new Vector3(p0.x, p0.y, halfT)); norms.Add(nrm); uvs.Add(new Vector2((float)i / n, 1));
                verts.Add(new Vector3(p1.x, p1.y, halfT)); norms.Add(nrm); uvs.Add(new Vector2((float)(i + 1) / n, 1));
                verts.Add(new Vector3(p1.x, p1.y, -halfT)); norms.Add(nrm); uvs.Add(new Vector2((float)(i + 1) / n, 0));

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
        /// Generates a trapezoidal Acme-threaded lead screw shaft.
        /// </summary>
        public static Mesh BuildLeadScrewMesh(float radius, float pitch, float length)
        {
            Mesh mesh = new Mesh();
            mesh.name = "LeadScrew_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int turns = Mathf.Max(3, Mathf.RoundToInt(length / pitch));
            int segsPerTurn = 24;
            float threadDepth = pitch * 0.40f;
            float rootRadius = radius - threadDepth;

            // Generate helical ribbon for screw thread
            int totalSegs = turns * segsPerTurn;
            for (int i = 0; i <= totalSegs; i++)
            {
                float t = (float)i / totalSegs;
                float z = t * length;
                float a = (float)i / segsPerTurn * 2f * Mathf.PI;
                float cos = Mathf.Cos(a);
                float sin = Mathf.Sin(a);

                // Root vertex
                verts.Add(new Vector3(cos * rootRadius, sin * rootRadius, z));
                norms.Add(new Vector3(cos, sin, 0));
                uvs.Add(new Vector2(0, t * turns));

                // Crest vertex
                verts.Add(new Vector3(cos * radius, sin * radius, z));
                norms.Add(new Vector3(cos, sin, 0));
                uvs.Add(new Vector2(1, t * turns));
            }

            for (int i = 0; i < totalSegs; i++)
            {
                int b = i * 2;
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

        #endregion
    }
}
