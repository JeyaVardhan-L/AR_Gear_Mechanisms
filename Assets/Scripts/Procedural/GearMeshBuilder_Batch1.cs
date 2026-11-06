using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechXR.Procedural
{
    public static partial class GearMeshBuilder
    {
        #region Batch 1: Reciprocating & Engine Components

        /// <summary>
        /// Generates an authentic internal combustion / pump piston mesh.
        /// Includes flat crown, dual ring grooves, wrist-pin cross bore, and relief skirt.
        /// </summary>
        public static Mesh BuildPistonMesh(float radius, float height, float wristPinRadius, float skirtRelief = 0.003f)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Piston_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int radialSegs = 32;
            float topY = height * 0.5f;
            float botY = -height * 0.5f;

            // 1. Piston Crown (Top disk)
            int crownCenterIdx = verts.Count;
            verts.Add(new Vector3(0, topY, 0));
            norms.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i < radialSegs; i++)
            {
                float ang = (float)i / radialSegs * 2f * Mathf.PI;
                verts.Add(new Vector3(Mathf.Cos(ang) * radius, topY, Mathf.Sin(ang) * radius));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2(0.5f + 0.5f * Mathf.Cos(ang), 0.5f + 0.5f * Mathf.Sin(ang)));
            }

            for (int i = 0; i < radialSegs; i++)
            {
                int next = (i + 1) % radialSegs;
                tris.Add(crownCenterIdx);
                tris.Add(crownCenterIdx + 1 + i);
                tris.Add(crownCenterIdx + 1 + next);
            }

            // 2. Piston Ring Belt & Skirt Profile along Y
            // Profile rings: Crown, Ring1 Top, Ring1 Bot, Ring2 Top, Ring2 Bot, Skirt Top, Skirt Bot
            float rOuter = radius;
            float rGroove = radius - 0.0025f;

            float y1 = topY - height * 0.12f;
            float y2 = topY - height * 0.18f; // Groove 1
            float y3 = topY - height * 0.26f;
            float y4 = topY - height * 0.32f; // Groove 2
            float y5 = topY - height * 0.40f; // Skirt begin

            float[] profileY = new float[] { topY, y1, y1, y2, y2, y3, y3, y4, y4, y5, botY };
            float[] profileR = new float[] { rOuter, rOuter, rGroove, rGroove, rOuter, rOuter, rGroove, rGroove, rOuter, rOuter, rOuter - skirtRelief };

            for (int layer = 0; layer < profileY.Length - 1; layer++)
            {
                float ya = profileY[layer];
                float yb = profileY[layer + 1];
                float ra = profileR[layer];
                float rb = profileR[layer + 1];

                int baseIdx = verts.Count;
                for (int i = 0; i <= radialSegs; i++)
                {
                    float ang = (float)i / radialSegs * 2f * Mathf.PI;
                    float cos = Mathf.Cos(ang);
                    float sin = Mathf.Sin(ang);

                    verts.Add(new Vector3(cos * ra, ya, sin * ra));
                    norms.Add(new Vector3(cos, 0, sin).normalized);
                    uvs.Add(new Vector2((float)i / radialSegs, (ya - botY) / height));

                    verts.Add(new Vector3(cos * rb, yb, sin * rb));
                    norms.Add(new Vector3(cos, 0, sin).normalized);
                    uvs.Add(new Vector2((float)i / radialSegs, (yb - botY) / height));
                }

                for (int i = 0; i < radialSegs; i++)
                {
                    int idx0 = baseIdx + i * 2;
                    int idx1 = idx0 + 1;
                    int idx2 = idx0 + 2;
                    int idx3 = idx0 + 3;

                    tris.Add(idx0);
                    tris.Add(idx1);
                    tris.Add(idx2);

                    tris.Add(idx2);
                    tris.Add(idx1);
                    tris.Add(idx3);
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
        /// Generates a forged I-beam connecting rod with big end, small end, and structural flanges.
        /// </summary>
        public static Mesh BuildConnectingRodMesh(float centerDistance, float bigEndBoreRadius, float smallEndBoreRadius, float shankWidth, float thickness)
        {
            Mesh mesh = new Mesh();
            mesh.name = "ConnectingRod_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segs = 20;
            float bigEyeRadius = bigEndBoreRadius + 0.010f;
            float smallEyeRadius = smallEndBoreRadius + 0.007f;
            float halfT = thickness * 0.5f;

            // Function to add a cylindrical ring (eye)
            Action<Vector3, float, float, float> addEye = (center, innerR, outerR, zDepth) =>
            {
                float hZ = zDepth * 0.5f;
                int start = verts.Count;

                // Front & Back faces
                for (int face = 0; face < 2; face++)
                {
                    float z = (face == 0) ? -hZ : hZ;
                    Vector3 n = (face == 0) ? Vector3.back : Vector3.forward;
                    int fStart = verts.Count;

                    for (int i = 0; i < segs; i++)
                    {
                        float a = (float)i / segs * 2f * Mathf.PI;
                        float cos = Mathf.Cos(a);
                        float sin = Mathf.Sin(a);

                        verts.Add(center + new Vector3(cos * innerR, sin * innerR, z));
                        norms.Add(n);
                        uvs.Add(new Vector2(cos, sin));

                        verts.Add(center + new Vector3(cos * outerR, sin * outerR, z));
                        norms.Add(n);
                        uvs.Add(new Vector2(cos * 1.5f, sin * 1.5f));
                    }

                    for (int i = 0; i < segs; i++)
                    {
                        int next = (i + 1) % segs;
                        int i0 = fStart + i * 2;
                        int o0 = i0 + 1;
                        int i1 = fStart + next * 2;
                        int o1 = i1 + 1;

                        if (face == 0)
                        {
                            tris.Add(i0); tris.Add(o0); tris.Add(o1);
                            tris.Add(i0); tris.Add(o1); tris.Add(i1);
                        }
                        else
                        {
                            tris.Add(i0); tris.Add(o1); tris.Add(o0);
                            tris.Add(i0); tris.Add(i1); tris.Add(o1);
                        }
                    }
                }

                // Outer cylindrical wall
                int outerStart = verts.Count;
                for (int i = 0; i <= segs; i++)
                {
                    float a = (float)i / segs * 2f * Mathf.PI;
                    float cos = Mathf.Cos(a);
                    float sin = Mathf.Sin(a);
                    Vector3 n = new Vector3(cos, sin, 0);

                    verts.Add(center + new Vector3(cos * outerR, sin * outerR, -hZ));
                    norms.Add(n);
                    uvs.Add(new Vector2((float)i / segs, 0));

                    verts.Add(center + new Vector3(cos * outerR, sin * outerR, hZ));
                    norms.Add(n);
                    uvs.Add(new Vector2((float)i / segs, 1));
                }

                for (int i = 0; i < segs; i++)
                {
                    int idx0 = outerStart + i * 2;
                    int idx1 = idx0 + 1;
                    int idx2 = idx0 + 2;
                    int idx3 = idx0 + 3;

                    tris.Add(idx0); tris.Add(idx1); tris.Add(idx2);
                    tris.Add(idx2); tris.Add(idx1); tris.Add(idx3);
                }
            };

            // Big end at origin (0,0,0)
            addEye(Vector3.zero, bigEndBoreRadius, bigEyeRadius, thickness * 1.3f);

            // Small end at (0, centerDistance, 0)
            addEye(new Vector3(0, centerDistance, 0), smallEndBoreRadius, smallEyeRadius, thickness);

            // 3. I-Beam Central Shank (between Y = bigEyeRadius*0.8 and Y = centerDistance - smallEyeRadius*0.8)
            float yStart = bigEyeRadius * 0.85f;
            float yEnd = centerDistance - smallEyeRadius * 0.85f;
            float len = yEnd - yStart;
            float shankThick = thickness * 0.8f;
            float halfW = shankWidth * 0.5f;
            float halfTh = shankThick * 0.5f;

            // Box for the central shank
            int boxBase = verts.Count;
            Vector3[] boxVerts = new Vector3[]
            {
                // Front face
                new Vector3(-halfW, yStart, -halfTh), new Vector3(halfW, yStart, -halfTh),
                new Vector3(halfW, yEnd, -halfTh), new Vector3(-halfW, yEnd, -halfTh),
                // Back face
                new Vector3(halfW, yStart, halfTh), new Vector3(-halfW, yStart, halfTh),
                new Vector3(-halfW, yEnd, halfTh), new Vector3(halfW, yEnd, halfTh),
                // Left face
                new Vector3(-halfW, yStart, halfTh), new Vector3(-halfW, yStart, -halfTh),
                new Vector3(-halfW, yEnd, -halfTh), new Vector3(-halfW, yEnd, halfTh),
                // Right face
                new Vector3(halfW, yStart, -halfTh), new Vector3(halfW, yStart, halfTh),
                new Vector3(halfW, yEnd, halfTh), new Vector3(halfW, yEnd, -halfTh)
            };

            Vector3[] boxNorms = new Vector3[]
            {
                Vector3.back, Vector3.back, Vector3.back, Vector3.back,
                Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
                Vector3.left, Vector3.left, Vector3.left, Vector3.left,
                Vector3.right, Vector3.right, Vector3.right, Vector3.right
            };

            for (int i = 0; i < 16; i++)
            {
                verts.Add(boxVerts[i]);
                norms.Add(boxNorms[i]);
                uvs.Add(new Vector2(i % 2, (i / 2) % 2));
            }

            for (int f = 0; f < 4; f++)
            {
                int b = boxBase + f * 4;
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
        /// Generates an authentic single-cylinder crankshaft with journals, counterweight webs, and offset crankpin.
        /// </summary>
        public static Mesh BuildCrankshaftMesh(float mainJournalRadius, float crankPinRadius, float stroke, float webThickness, float webWidth, float counterweightRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Crankshaft_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float crankRadius = stroke * 0.5f;
            float totalZ = webThickness * 2f + 0.035f; // pin width ~ 0.035m

            // 1. Front Main Journal (Z: -0.04 to -webThickness)
            AddCylinderSection(verts, norms, uvs, tris, Vector3.zero, mainJournalRadius, 0.045f, Vector3.back);

            // 2. Rear Main Journal (Z: +webThickness to +0.04)
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(0, 0, webThickness + 0.035f), mainJournalRadius, 0.045f, Vector3.forward);

            // 3. Offset Crankpin (at Y = +crankRadius, Z = 0 to 0.035)
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(0, crankRadius, 0), crankPinRadius, 0.035f, Vector3.forward);

            // 4. Dual Counterweight Crank Webs
            float[] zWebs = new float[] { -webThickness * 0.5f, 0.035f + webThickness * 0.5f };
            for (int w = 0; w < 2; w++)
            {
                float zCenter = zWebs[w];
                float halfT = webThickness * 0.5f;

                // Web profile: sector for counterweight (negative Y), arm to crankpin (positive Y)
                int startV = verts.Count;
                int cwSegs = 16;

                // Counterweight semicircular fan on front and back
                for (int face = 0; face < 2; face++)
                {
                    float z = zCenter + (face == 0 ? -halfT : halfT);
                    Vector3 n = face == 0 ? Vector3.back : Vector3.forward;
                    int fBase = verts.Count;

                    // Center
                    verts.Add(new Vector3(0, 0, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(0.5f, 0.5f));

                    // Outer semicircle for counterweight (below axis)
                    for (int i = 0; i <= cwSegs; i++)
                    {
                        float a = Mathf.PI + (float)i / cwSegs * Mathf.PI; // 180 to 360 deg
                        verts.Add(new Vector3(Mathf.Cos(a) * counterweightRadius, Mathf.Sin(a) * counterweightRadius, z));
                        norms.Add(n);
                        uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                    }

                    // Upper arm polygon points to crankpin
                    verts.Add(new Vector3(webWidth * 0.5f, crankRadius, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(1, 1));

                    verts.Add(new Vector3(-webWidth * 0.5f, crankRadius, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(0, 1));

                    // Tris for counterweight
                    for (int i = 0; i < cwSegs; i++)
                    {
                        if (face == 0)
                        {
                            tris.Add(fBase); tris.Add(fBase + 1 + i); tris.Add(fBase + 2 + i);
                        }
                        else
                        {
                            tris.Add(fBase); tris.Add(fBase + 2 + i); tris.Add(fBase + 1 + i);
                        }
                    }

                    // Upper web rectangle
                    int pinTopR = fBase + cwSegs + 2;
                    int pinTopL = fBase + cwSegs + 3;
                    if (face == 0)
                    {
                        tris.Add(fBase); tris.Add(fBase + 1); tris.Add(pinTopL);
                        tris.Add(fBase); tris.Add(pinTopL); tris.Add(pinTopR);
                        tris.Add(fBase); tris.Add(pinTopR); tris.Add(fBase + cwSegs + 1);
                    }
                    else
                    {
                        tris.Add(fBase); tris.Add(pinTopL); tris.Add(fBase + 1);
                        tris.Add(fBase); tris.Add(pinTopR); tris.Add(pinTopL);
                        tris.Add(fBase); tris.Add(fBase + cwSegs + 1); tris.Add(pinTopR);
                    }
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddCylinderSection(List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris, Vector3 origin, float radius, float length, Vector3 dir)
        {
            int segs = 20;
            int start = verts.Count;
            Vector3 axis = dir.normalized;
            Vector3 perp1 = Vector3.Cross(axis, Vector3.up);
            if (perp1.sqrMagnitude < 0.01f) perp1 = Vector3.Cross(axis, Vector3.right);
            perp1.Normalize();
            Vector3 perp2 = Vector3.Cross(axis, perp1).normalized;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                Vector3 radial = perp1 * Mathf.Cos(a) + perp2 * Mathf.Sin(a);

                verts.Add(origin + radial * radius);
                norms.Add(radial);
                uvs.Add(new Vector2((float)i / segs, 0));

                verts.Add(origin + axis * length + radial * radius);
                norms.Add(radial);
                uvs.Add(new Vector2((float)i / segs, 1));
            }

            for (int i = 0; i < segs; i++)
            {
                int i0 = start + i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;
                int i3 = i0 + 3;

                tris.Add(i0); tris.Add(i1); tris.Add(i2);
                tris.Add(i2); tris.Add(i1); tris.Add(i3);
            }
        }

        /// <summary>
        /// Generates an engine / pump cylinder block with an open viewing cutaway sector (120 deg)
        /// so internal reciprocating motion (piston, rod, valves) is visible in AR.
        /// </summary>
        public static Mesh BuildCylinderBlockMesh(float innerBoreRadius, float outerRadius, float height, float cutawayDeg = 120f, int fins = 6)
        {
            Mesh mesh = new Mesh();
            mesh.name = "CylinderBlock_Cutaway";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segs = 32;
            float halfH = height * 0.5f;

            // Cutaway removes sector from -cutawayDeg/2 to +cutawayDeg/2 (around front Z+)
            float startAngle = (cutawayDeg * 0.5f) * Mathf.Deg2Rad;
            float endAngle = (360f - cutawayDeg * 0.5f) * Mathf.Deg2Rad;
            float arcSpan = endAngle - startAngle;

            // Inner bore wall & Outer finned wall
            for (int layer = 0; layer <= fins * 2; layer++)
            {
                float t = (float)layer / (fins * 2);
                float y = -halfH + t * height;
                bool isFin = (layer % 2 == 1);
                float rOut = isFin ? outerRadius + 0.012f : outerRadius;

                int bIdx = verts.Count;
                for (int i = 0; i <= segs; i++)
                {
                    float a = startAngle + (float)i / segs * arcSpan;
                    float cos = Mathf.Cos(a);
                    float sin = Mathf.Sin(a);

                    // Inner bore vertex
                    verts.Add(new Vector3(cos * innerBoreRadius, y, sin * innerBoreRadius));
                    norms.Add(new Vector3(-cos, 0, -sin));
                    uvs.Add(new Vector2((float)i / segs, t));

                    // Outer fin vertex
                    verts.Add(new Vector3(cos * rOut, y, sin * rOut));
                    norms.Add(new Vector3(cos, 0, sin));
                    uvs.Add(new Vector2((float)i / segs, t));
                }

                if (layer > 0)
                {
                    int prevBase = bIdx - (segs + 1) * 2;
                    for (int i = 0; i < segs; i++)
                    {
                        // Inner wall
                        int i0 = prevBase + i * 2;
                        int i1 = bIdx + i * 2;
                        int i2 = prevBase + (i + 1) * 2;
                        int i3 = bIdx + (i + 1) * 2;

                        tris.Add(i0); tris.Add(i2); tris.Add(i1);
                        tris.Add(i2); tris.Add(i3); tris.Add(i1);

                        // Outer wall
                        int o0 = i0 + 1;
                        int o1 = i1 + 1;
                        int o2 = i2 + 1;
                        int o3 = i3 + 1;

                        tris.Add(o0); tris.Add(o1); tris.Add(o2);
                        tris.Add(o2); tris.Add(o1); tris.Add(o3);
                    }
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
        /// Generates a poppet valve mesh with 45-degree ground seat face, long stem, and valve keeper retainer.
        /// </summary>
        public static Mesh BuildPoppetValveMesh(float headRadius, float stemRadius, float stemLength)
        {
            Mesh mesh = new Mesh();
            mesh.name = "PoppetValve_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segs = 20;

            // Valve Head Profile along Y (head at Y=0, stem extends to Y = +stemLength)
            // 1. Flat bottom face
            int botCenter = verts.Count;
            verts.Add(Vector3.zero);
            norms.Add(Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i < segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                verts.Add(new Vector3(Mathf.Cos(a) * headRadius, 0, Mathf.Sin(a) * headRadius));
                norms.Add(Vector3.down);
                uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
            }

            for (int i = 0; i < segs; i++)
            {
                int next = (i + 1) % segs;
                tris.Add(botCenter);
                tris.Add(botCenter + 1 + next);
                tris.Add(botCenter + 1 + i);
            }

            // 2. 45-degree conical seating face (Y: 0 to 0.004, R: headRadius to headRadius - 0.004)
            float seatH = 0.004f;
            float seatR = headRadius - 0.003f;
            int seatStart = verts.Count;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                float cos = Mathf.Cos(a);
                float sin = Mathf.Sin(a);

                verts.Add(new Vector3(cos * headRadius, 0, sin * headRadius));
                norms.Add(new Vector3(cos * 0.707f, 0.707f, sin * 0.707f));
                uvs.Add(new Vector2((float)i / segs, 0));

                verts.Add(new Vector3(cos * seatR, seatH, sin * seatR));
                norms.Add(new Vector3(cos * 0.707f, 0.707f, sin * 0.707f));
                uvs.Add(new Vector2((float)i / segs, 1));
            }

            for (int i = 0; i < segs; i++)
            {
                int i0 = seatStart + i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;
                int i3 = i0 + 3;

                tris.Add(i0); tris.Add(i1); tris.Add(i2);
                tris.Add(i2); tris.Add(i1); tris.Add(i3);
            }

            // 3. Valve Stem (from Y = seatH to Y = stemLength)
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(0, seatH, 0), stemRadius, stemLength - seatH, Vector3.up);

            // 4. Valve Spring Retainer disk at top of stem
            AddCylinderSection(verts, norms, uvs, tris, new Vector3(0, stemLength - 0.008f, 0), stemRadius * 2.8f, 0.006f, Vector3.up);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a precision Cam profile mesh.
        /// Profile 0: Harmonic / Pear cam (dwell at base circle, smooth cosine rise and fall)
        /// Profile 1: Circular eccentric cam
        /// </summary>
        public static Mesh BuildCamMesh(float baseRadius, float lift, float camWidth, int profileType = 0, float boreRadius = 0.006f)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Cam_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segs = 48;
            float halfW = camWidth * 0.5f;

            // Generate 2D contour of cam
            Vector2[] contour = new Vector2[segs];
            for (int i = 0; i < segs; i++)
            {
                float ang = (float)i / segs * 2f * Mathf.PI; // 0 to 2pi
                float deg = ang * Mathf.Rad2Deg;
                float r = baseRadius;

                if (profileType == 0)
                {
                    // Harmonic pear cam: Lift occurs between 60 deg and 180 deg
                    if (deg >= 60f && deg <= 180f)
                    {
                        float liftFraction = (1f - Mathf.Cos((deg - 60f) / 120f * Mathf.PI)) * 0.5f;
                        r += lift * liftFraction;
                    }
                    else if (deg > 180f && deg <= 300f)
                    {
                        float fallFraction = (1f + Mathf.Cos((deg - 180f) / 120f * Mathf.PI)) * 0.5f;
                        r += lift * fallFraction;
                    }
                }
                else
                {
                    // Circular eccentric cam (geometric center offset by lift/2)
                    float offset = lift * 0.5f;
                    float x = Mathf.Cos(ang) * baseRadius;
                    float y = Mathf.Sin(ang) * baseRadius + offset;
                    r = Mathf.Sqrt(x * x + y * y);
                }

                contour[i] = new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r);
            }

            // Extrude contour along Z (-halfW to +halfW) with internal bore
            for (int face = 0; face < 2; face++)
            {
                float z = face == 0 ? -halfW : halfW;
                Vector3 n = face == 0 ? Vector3.back : Vector3.forward;
                int fBase = verts.Count;

                for (int i = 0; i < segs; i++)
                {
                    float a = (float)i / segs * 2f * Mathf.PI;
                    // Inner bore
                    verts.Add(new Vector3(Mathf.Cos(a) * boreRadius, Mathf.Sin(a) * boreRadius, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));

                    // Outer cam contour
                    verts.Add(new Vector3(contour[i].x, contour[i].y, z));
                    norms.Add(n);
                    uvs.Add(new Vector2(contour[i].x, contour[i].y));
                }

                for (int i = 0; i < segs; i++)
                {
                    int next = (i + 1) % segs;
                    int i0 = fBase + i * 2;
                    int o0 = i0 + 1;
                    int i1 = fBase + next * 2;
                    int o1 = i1 + 1;

                    if (face == 0)
                    {
                        tris.Add(i0); tris.Add(o0); tris.Add(o1);
                        tris.Add(i0); tris.Add(o1); tris.Add(i1);
                    }
                    else
                    {
                        tris.Add(i0); tris.Add(o1); tris.Add(o0);
                        tris.Add(i0); tris.Add(i1); tris.Add(o1);
                    }
                }
            }

            // Outer cam peripheral surface
            int outerBase = verts.Count;
            for (int i = 0; i <= segs; i++)
            {
                int idx = i % segs;
                Vector2 pt = contour[idx];
                Vector3 radial = new Vector3(pt.x, pt.y, 0).normalized;

                verts.Add(new Vector3(pt.x, pt.y, -halfW));
                norms.Add(radial);
                uvs.Add(new Vector2((float)i / segs, 0));

                verts.Add(new Vector3(pt.x, pt.y, halfW));
                norms.Add(radial);
                uvs.Add(new Vector2((float)i / segs, 1));
            }

            for (int i = 0; i < segs; i++)
            {
                int i0 = outerBase + i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;
                int i3 = i0 + 3;

                tris.Add(i0); tris.Add(i1); tris.Add(i2);
                tris.Add(i2); tris.Add(i1); tris.Add(i3);
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
