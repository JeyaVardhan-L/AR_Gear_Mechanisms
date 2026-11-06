using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechXR.Procedural
{
    public static partial class GearMeshBuilder
    {
        #region Helical & Herringbone Gears

        /// <summary>
        /// Generates a precision Helical Gear with a defined helix angle beta.
        /// Flank involute profiles are continuously twisted along the face width,
        /// demonstrating progressive tooth engagement and axial thrust generation.
        /// </summary>
        public static Mesh BuildHelicalGearMesh(
            int toothCount,
            float pitchRadius,
            float addendum,
            float dedendum,
            float faceWidth,
            float boreRadius,
            float helixAngleDeg,
            int spokeCount = 0,
            float spokeWidth = 0.008f)
        {
            Mesh mesh = new Mesh();
            mesh.name = spokeCount > 0 ? $"HelicalGear_Z{toothCount}_Spoked{spokeCount}_b{helixAngleDeg:F0}" : $"HelicalGear_Z{toothCount}_Solid_b{helixAngleDeg:F0}";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float tipRadius = pitchRadius + addendum;
            float rootRadius = Mathf.Max(pitchRadius - dedendum, boreRadius + 0.006f);
            float halfW = faceWidth * 0.5f;

            float hubRadius = Mathf.Max(boreRadius * 2.2f, rootRadius * 0.35f);
            float rimInnerRadius = rootRadius - Mathf.Max(0.006f, (rootRadius - hubRadius) * 0.22f);

            List<Vector2> baseContour = GenerateToothContour(toothCount, pitchRadius, rootRadius, tipRadius);
            int numContour = baseContour.Count;

            int sliceCount = 6;
            List<List<Vector2>> slices = new List<List<Vector2>>();
            float[] zValues = new float[sliceCount];

            float tanBeta = Mathf.Tan(helixAngleDeg * Mathf.Deg2Rad);

            for (int s = 0; s < sliceCount; s++)
            {
                float t = (float)s / (sliceCount - 1);
                float z = Mathf.Lerp(-halfW, halfW, t);
                zValues[s] = z;
                float twistAngle = (z * tanBeta) / pitchRadius;
                slices.Add(RotateContour(baseContour, twistAngle));
            }

            // End cap at back (z = -halfW)
            float innerCapRadius = (spokeCount <= 0 || rimInnerRadius <= hubRadius + 0.01f) ? boreRadius : rimInnerRadius;
            BuildAnnularFace(verts, norms, uvs, tris, slices[0], innerCapRadius, -halfW, Vector3.back, true);

            // End cap at front (z = +halfW)
            BuildAnnularFace(verts, norms, uvs, tris, slices[sliceCount - 1], innerCapRadius, halfW, Vector3.forward, false);

            // Flanks between slices
            for (int s = 0; s < sliceCount - 1; s++)
            {
                float z0 = zValues[s];
                float z1 = zValues[s + 1];
                List<Vector2> c0 = slices[s];
                List<Vector2> c1 = slices[s + 1];

                for (int i = 0; i < numContour; i++)
                {
                    int next = (i + 1) % numContour;

                    Vector3 p0 = new Vector3(c0[i].x, c0[i].y, z0);
                    Vector3 p1 = new Vector3(c0[next].x, c0[next].y, z0);
                    Vector3 p2 = new Vector3(c1[next].x, c1[next].y, z1);
                    Vector3 p3 = new Vector3(c1[i].x, c1[i].y, z1);

                    AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);
                }
            }

            if (spokeCount <= 0 || rimInnerRadius <= hubRadius + 0.01f)
            {
                BuildCylinderBore(verts, norms, uvs, tris, boreRadius, halfW, -halfW, 32);
            }
            else
            {
                BuildCylinderBore(verts, norms, uvs, tris, rimInnerRadius, halfW, -halfW, 36);

                // Central Hub
                float hubHalfW = halfW * 1.15f;
                List<Vector2> hubCircle = GenerateCircleContour(hubRadius, 32);
                BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, hubHalfW, Vector3.forward, false);
                BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, -hubHalfW, Vector3.back, true);
                BuildCylinderRim(verts, norms, uvs, tris, hubRadius, hubHalfW, -hubHalfW, 32);
                BuildCylinderBore(verts, norms, uvs, tris, boreRadius, hubHalfW, -hubHalfW, 32);

                // Radial Spokes
                float spokeHalfW = halfW * 0.75f;
                float spokeLen = rimInnerRadius - hubRadius;
                float spokeMidR = (hubRadius + rimInnerRadius) * 0.5f;

                for (int s = 0; s < spokeCount; s++)
                {
                    float angleDeg = s * (360f / spokeCount);
                    float rad = angleDeg * Mathf.Deg2Rad;
                    Vector3 spokeDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                    Vector3 spokePerp = new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0);

                    Vector3 center = spokeDir * spokeMidR;
                    BuildBoxSegment(verts, norms, uvs, tris,
                        center,
                        spokeDir * spokeLen,
                        spokePerp * spokeWidth,
                        Vector3.forward * (spokeHalfW * 2f));
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
        /// Generates a Double-Helical Herringbone Gear with opposing chevron teeth.
        /// Opposed helix angles (+beta and -beta) cancel net axial thrust forces.
        /// </summary>
        public static Mesh BuildHerringboneGearMesh(
            int toothCount,
            float pitchRadius,
            float addendum,
            float dedendum,
            float faceWidth,
            float boreRadius,
            float helixAngleDeg,
            int spokeCount = 0,
            float spokeWidth = 0.008f)
        {
            Mesh mesh = new Mesh();
            mesh.name = spokeCount > 0 ? $"HerringboneGear_Z{toothCount}_Spoked{spokeCount}" : $"HerringboneGear_Z{toothCount}_Solid";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float tipRadius = pitchRadius + addendum;
            float rootRadius = Mathf.Max(pitchRadius - dedendum, boreRadius + 0.006f);
            float halfW = faceWidth * 0.5f;

            float hubRadius = Mathf.Max(boreRadius * 2.2f, rootRadius * 0.35f);
            float rimInnerRadius = rootRadius - Mathf.Max(0.006f, (rootRadius - hubRadius) * 0.22f);

            List<Vector2> baseContour = GenerateToothContour(toothCount, pitchRadius, rootRadius, tipRadius);
            int numContour = baseContour.Count;

            int halfSlices = 4;
            int totalSlices = halfSlices * 2 - 1;
            List<List<Vector2>> slices = new List<List<Vector2>>();
            float[] zValues = new float[totalSlices];

            float tanBeta = Mathf.Tan(helixAngleDeg * Mathf.Deg2Rad);

            // Back half: z = -halfW to 0 (angle = -z * tanBeta / pitchRadius)
            for (int s = 0; s < halfSlices; s++)
            {
                float t = (float)s / (halfSlices - 1);
                float z = Mathf.Lerp(-halfW, 0f, t);
                zValues[s] = z;
                // Chevron: twist is symmetric about z = 0
                float twistAngle = (-Mathf.Abs(z) * tanBeta) / pitchRadius;
                slices.Add(RotateContour(baseContour, twistAngle));
            }

            // Front half: z = 0 to +halfW
            for (int s = 1; s < halfSlices; s++)
            {
                float t = (float)s / (halfSlices - 1);
                float z = Mathf.Lerp(0f, halfW, t);
                int idx = halfSlices - 1 + s;
                zValues[idx] = z;
                float twistAngle = (-Mathf.Abs(z) * tanBeta) / pitchRadius;
                slices.Add(RotateContour(baseContour, twistAngle));
            }

            // Back cap
            float innerCapRadius = (spokeCount <= 0 || rimInnerRadius <= hubRadius + 0.01f) ? boreRadius : rimInnerRadius;
            BuildAnnularFace(verts, norms, uvs, tris, slices[0], innerCapRadius, -halfW, Vector3.back, true);

            // Front cap
            BuildAnnularFace(verts, norms, uvs, tris, slices[totalSlices - 1], innerCapRadius, halfW, Vector3.forward, false);

            // Flanks across all slices
            for (int s = 0; s < totalSlices - 1; s++)
            {
                float z0 = zValues[s];
                float z1 = zValues[s + 1];
                List<Vector2> c0 = slices[s];
                List<Vector2> c1 = slices[s + 1];

                for (int i = 0; i < numContour; i++)
                {
                    int next = (i + 1) % numContour;

                    Vector3 p0 = new Vector3(c0[i].x, c0[i].y, z0);
                    Vector3 p1 = new Vector3(c0[next].x, c0[next].y, z0);
                    Vector3 p2 = new Vector3(c1[next].x, c1[next].y, z1);
                    Vector3 p3 = new Vector3(c1[i].x, c1[i].y, z1);

                    AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);
                }
            }

            if (spokeCount <= 0 || rimInnerRadius <= hubRadius + 0.01f)
            {
                BuildCylinderBore(verts, norms, uvs, tris, boreRadius, halfW, -halfW, 32);
            }
            else
            {
                BuildCylinderBore(verts, norms, uvs, tris, rimInnerRadius, halfW, -halfW, 36);

                // Central Hub
                float hubHalfW = halfW * 1.15f;
                List<Vector2> hubCircle = GenerateCircleContour(hubRadius, 32);
                BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, hubHalfW, Vector3.forward, false);
                BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, -hubHalfW, Vector3.back, true);
                BuildCylinderRim(verts, norms, uvs, tris, hubRadius, hubHalfW, -hubHalfW, 32);
                BuildCylinderBore(verts, norms, uvs, tris, boreRadius, hubHalfW, -hubHalfW, 32);

                // Radial Spokes
                float spokeHalfW = halfW * 0.75f;
                float spokeLen = rimInnerRadius - hubRadius;
                float spokeMidR = (hubRadius + rimInnerRadius) * 0.5f;

                for (int s = 0; s < spokeCount; s++)
                {
                    float angleDeg = s * (360f / spokeCount);
                    float rad = angleDeg * Mathf.Deg2Rad;
                    Vector3 spokeDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                    Vector3 spokePerp = new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0);

                    Vector3 center = spokeDir * spokeMidR;
                    BuildBoxSegment(verts, norms, uvs, tris,
                        center,
                        spokeDir * spokeLen,
                        spokePerp * spokeWidth,
                        Vector3.forward * (spokeHalfW * 2f));
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<Vector2> RotateContour(List<Vector2> contour, float angleRad)
        {
            float cosA = Mathf.Cos(angleRad);
            float sinA = Mathf.Sin(angleRad);
            int count = contour.Count;
            List<Vector2> res = new List<Vector2>(count);
            for (int i = 0; i < count; i++)
            {
                float x = contour[i].x * cosA - contour[i].y * sinA;
                float y = contour[i].x * sinA + contour[i].y * cosA;
                res.Add(new Vector2(x, y));
            }
            return res;
        }

        #endregion

        #region Automotive Differential Carrier

        /// <summary>
        /// Generates a Differential Carrier Housing with coaxial bearing journals,
        /// a bevel crown mounting flange, cross-pin bore, and side inspection windows.
        /// </summary>
        public static Mesh BuildDifferentialCarrierMesh(
            float cageRadius,
            float cageLengthX,
            float journalRadius,
            float journalLengthX,
            float flangeRadius,
            float flangeThicknessX,
            float crossPinRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "DifferentialCarrier_Housing";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int radialSegments = 32;
            float halfCageX = cageLengthX * 0.5f;

            // 1. Central Spherical/Cylindrical Cage with Window Cutouts
            // Windows are located at top and bottom (around Y axis) so internal gears are fully visible
            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                float a0 = (float)i / radialSegments * 2f * Mathf.PI;
                float a1 = (float)next / radialSegments * 2f * Mathf.PI;

                // Check if segment falls in window aperture (e.g. 60° to 120° and 240° to 300°)
                float deg0 = Mathf.Abs(Mathf.Rad2Deg * Mathf.Atan2(Mathf.Sin(a0), Mathf.Cos(a0)));
                bool inWindow = (deg0 > 55f && deg0 < 125f);

                if (!inWindow)
                {
                    Vector3 yz0 = new Vector3(0, Mathf.Cos(a0) * cageRadius, Mathf.Sin(a0) * cageRadius);
                    Vector3 yz1 = new Vector3(0, Mathf.Cos(a1) * cageRadius, Mathf.Sin(a1) * cageRadius);

                    Vector3 p0 = new Vector3(-halfCageX, yz0.y, yz0.z);
                    Vector3 p1 = new Vector3(halfCageX, yz0.y, yz0.z);
                    Vector3 p2 = new Vector3(halfCageX, yz1.y, yz1.z);
                    Vector3 p3 = new Vector3(-halfCageX, yz1.y, yz1.z);

                    AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);
                }
            }

            // 2. Left Bearing Journal Hub (along -X)
            float leftJournalStart = -halfCageX;
            float leftJournalEnd = -halfCageX - journalLengthX;
            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                float a0 = (float)i / radialSegments * 2f * Mathf.PI;
                float a1 = (float)next / radialSegments * 2f * Mathf.PI;

                Vector3 p0 = new Vector3(leftJournalEnd, Mathf.Cos(a0) * journalRadius, Mathf.Sin(a0) * journalRadius);
                Vector3 p1 = new Vector3(leftJournalStart, Mathf.Cos(a0) * journalRadius, Mathf.Sin(a0) * journalRadius);
                Vector3 p2 = new Vector3(leftJournalStart, Mathf.Cos(a1) * journalRadius, Mathf.Sin(a1) * journalRadius);
                Vector3 p3 = new Vector3(leftJournalEnd, Mathf.Cos(a1) * journalRadius, Mathf.Sin(a1) * journalRadius);

                AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);
            }

            // 3. Right Bearing Journal Hub (along +X)
            float rightJournalStart = halfCageX;
            float rightJournalEnd = halfCageX + journalLengthX;
            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                float a0 = (float)i / radialSegments * 2f * Mathf.PI;
                float a1 = (float)next / radialSegments * 2f * Mathf.PI;

                Vector3 p0 = new Vector3(rightJournalStart, Mathf.Cos(a0) * journalRadius, Mathf.Sin(a0) * journalRadius);
                Vector3 p1 = new Vector3(rightJournalEnd, Mathf.Cos(a0) * journalRadius, Mathf.Sin(a0) * journalRadius);
                Vector3 p2 = new Vector3(rightJournalEnd, Mathf.Cos(a1) * journalRadius, Mathf.Sin(a1) * journalRadius);
                Vector3 p3 = new Vector3(rightJournalStart, Mathf.Cos(a1) * journalRadius, Mathf.Sin(a1) * journalRadius);

                AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);
            }

            // 4. Crown Gear Mounting Flange (ring at -halfCageX)
            float flangeX0 = -halfCageX;
            float flangeX1 = -halfCageX + flangeThicknessX;
            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                float a0 = (float)i / radialSegments * 2f * Mathf.PI;
                float a1 = (float)next / radialSegments * 2f * Mathf.PI;

                // Outer rim of flange
                Vector3 p0 = new Vector3(flangeX0, Mathf.Cos(a0) * flangeRadius, Mathf.Sin(a0) * flangeRadius);
                Vector3 p1 = new Vector3(flangeX1, Mathf.Cos(a0) * flangeRadius, Mathf.Sin(a0) * flangeRadius);
                Vector3 p2 = new Vector3(flangeX1, Mathf.Cos(a1) * flangeRadius, Mathf.Sin(a1) * flangeRadius);
                Vector3 p3 = new Vector3(flangeX0, Mathf.Cos(a1) * flangeRadius, Mathf.Sin(a1) * flangeRadius);

                AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);

                // Radial face of flange
                Vector3 f0 = new Vector3(flangeX0, Mathf.Cos(a0) * cageRadius, Mathf.Sin(a0) * cageRadius);
                Vector3 f1 = new Vector3(flangeX0, Mathf.Cos(a0) * flangeRadius, Mathf.Sin(a0) * flangeRadius);
                Vector3 f2 = new Vector3(flangeX0, Mathf.Cos(a1) * flangeRadius, Mathf.Sin(a1) * flangeRadius);
                Vector3 f3 = new Vector3(flangeX0, Mathf.Cos(a1) * cageRadius, Mathf.Sin(a1) * cageRadius);

                AddQuad(verts, norms, uvs, tris, f0, f1, f2, f3);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Harmonic Drive (Strain Wave Gear)

        /// <summary>
        /// Generates the Wave Generator: Elliptical cam core with ball bearing race.
        /// </summary>
        public static Mesh BuildHarmonicDriveWaveGeneratorMesh(
            float majorRadius,
            float minorRadius,
            float faceWidth,
            float boreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "HarmonicDrive_WaveGenerator";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int segments = 48;
            float halfW = faceWidth * 0.5f;

            List<Vector2> ellipseContour = new List<Vector2>(segments);
            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * 2f * Mathf.PI;
                ellipseContour.Add(new Vector2(Mathf.Cos(a) * majorRadius, Mathf.Sin(a) * minorRadius));
            }

            // Front cap
            BuildAnnularFace(verts, norms, uvs, tris, ellipseContour, boreRadius, halfW, Vector3.forward, false);
            // Back cap
            BuildAnnularFace(verts, norms, uvs, tris, ellipseContour, boreRadius, -halfW, Vector3.back, true);

            // Outer rim
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                Vector3 p0 = new Vector3(ellipseContour[i].x, ellipseContour[i].y, halfW);
                Vector3 p1 = new Vector3(ellipseContour[next].x, ellipseContour[next].y, halfW);
                Vector3 p2 = new Vector3(ellipseContour[next].x, ellipseContour[next].y, -halfW);
                Vector3 p3 = new Vector3(ellipseContour[i].x, ellipseContour[i].y, -halfW);

                AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);
            }

            // Bore
            BuildCylinderBore(verts, norms, uvs, tris, boreRadius, halfW, -halfW, 32);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates the Flexspline: Thin elastic cup with external tooth ring and rear diaphragm.
        /// </summary>
        public static Mesh BuildHarmonicFlexsplineMesh(
            int toothCount,
            float pitchRadius,
            float addendum,
            float dedendum,
            float toothFaceWidth,
            float cupLength,
            float wallThickness,
            float hubBoreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"HarmonicDrive_Flexspline_Z{toothCount}";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float tipRadius = pitchRadius + addendum;
            float rootRadius = pitchRadius - dedendum;
            float innerRadius = rootRadius - wallThickness;

            // 1. Toothed Rim at front: z = 0 to +toothFaceWidth
            List<Vector2> toothContour = GenerateToothContour(toothCount, pitchRadius, rootRadius, tipRadius);
            int numContour = toothContour.Count;

            // Front cap of toothed rim
            BuildAnnularFace(verts, norms, uvs, tris, toothContour, innerRadius, toothFaceWidth, Vector3.forward, false);

            // Flanks of teeth
            for (int i = 0; i < numContour; i++)
            {
                int next = (i + 1) % numContour;
                Vector3 p0 = new Vector3(toothContour[i].x, toothContour[i].y, toothFaceWidth);
                Vector3 p1 = new Vector3(toothContour[next].x, toothContour[next].y, toothFaceWidth);
                Vector3 p2 = new Vector3(toothContour[next].x, toothContour[next].y, 0f);
                Vector3 p3 = new Vector3(toothContour[i].x, toothContour[i].y, 0f);

                AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);
            }

            // 2. Flexible Cup Cylindrical Body: z = 0 back to -cupLength
            int segments = 48;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                float a0 = (float)i / segments * 2f * Mathf.PI;
                float a1 = (float)next / segments * 2f * Mathf.PI;

                // Outer cup wall
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * rootRadius, Mathf.Sin(a0) * rootRadius, 0f);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * rootRadius, Mathf.Sin(a1) * rootRadius, 0f);
                Vector3 p2 = new Vector3(Mathf.Cos(a1) * rootRadius, Mathf.Sin(a1) * rootRadius, -cupLength);
                Vector3 p3 = new Vector3(Mathf.Cos(a0) * rootRadius, Mathf.Sin(a0) * rootRadius, -cupLength);

                AddQuad(verts, norms, uvs, tris, p0, p1, p2, p3);

                // Inner cup wall
                Vector3 q0 = new Vector3(Mathf.Cos(a1) * innerRadius, Mathf.Sin(a1) * innerRadius, toothFaceWidth);
                Vector3 q1 = new Vector3(Mathf.Cos(a0) * innerRadius, Mathf.Sin(a0) * innerRadius, toothFaceWidth);
                Vector3 q2 = new Vector3(Mathf.Cos(a0) * innerRadius, Mathf.Sin(a0) * innerRadius, -cupLength);
                Vector3 q3 = new Vector3(Mathf.Cos(a1) * innerRadius, Mathf.Sin(a1) * innerRadius, -cupLength);

                AddQuad(verts, norms, uvs, tris, q0, q1, q2, q3);
            }

            // 3. Rear Base Diaphragm at z = -cupLength
            List<Vector2> outerCircle = GenerateCircleContour(rootRadius, segments);
            BuildAnnularFace(verts, norms, uvs, tris, outerCircle, hubBoreRadius, -cupLength, Vector3.back, true);
            BuildCylinderBore(verts, norms, uvs, tris, hubBoreRadius, toothFaceWidth, -cupLength, 32);

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
