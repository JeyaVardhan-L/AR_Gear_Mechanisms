using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechXR.Procedural
{
    /// <summary>
    /// Precision procedural 3D mechanical mesh generator for engineering gear systems,
    /// shafts, bearings, pillow block mounts, and machine beds.
    /// Produces clean, closed manifold geometry with accurate mechanical proportions,
    /// involute tooth profiles, spoked webs, conical bevels, and throated worm wheels.
    /// </summary>
    public static partial class GearMeshBuilder
    {
        #region Spur Gears (Solid & Spoked)

        /// <summary>
        /// Generates an engineering-grade Spur Gear.
        /// When spokeCount > 0, generates a cast/machined gear with a central hub,
        /// radial spokes, and an outer toothed rim for unmistakable rotational visibility.
        /// When spokeCount == 0, generates a solid pinion with hub and bore.
        /// </summary>
        public static Mesh BuildSpurGearMesh(
            int toothCount,
            float pitchRadius,
            float addendum,
            float dedendum,
            float faceWidth,
            float boreRadius,
            int spokeCount = 0,
            float spokeWidth = 0.008f)
        {
            Mesh mesh = new Mesh();
            mesh.name = spokeCount > 0 ? $"SpurGear_Z{toothCount}_Spoked{spokeCount}" : $"SpurGear_Z{toothCount}_Solid";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float tipRadius = pitchRadius + addendum;
            float rootRadius = Mathf.Max(pitchRadius - dedendum, boreRadius + 0.006f);
            float halfW = faceWidth * 0.5f;

            float hubRadius = Mathf.Max(boreRadius * 2.2f, rootRadius * 0.35f);
            float rimInnerRadius = rootRadius - Mathf.Max(0.006f, (rootRadius - hubRadius) * 0.22f);

            // 1. Generate 2D outer involute tooth contour
            List<Vector2> toothContour = GenerateToothContour(toothCount, pitchRadius, rootRadius, tipRadius);
            int numContour = toothContour.Count;

            if (spokeCount <= 0 || rimInnerRadius <= hubRadius + 0.01f)
            {
                // Solid gear: front/back annular face from contour to bore
                BuildAnnularFace(verts, norms, uvs, tris, toothContour, boreRadius, halfW, Vector3.forward, false);
                BuildAnnularFace(verts, norms, uvs, tris, toothContour, boreRadius, -halfW, Vector3.back, true);
                BuildCylinderBore(verts, norms, uvs, tris, boreRadius, halfW, -halfW, 32);
            }
            else
            {
                // Spoked gear:
                // Part A: Toothed Rim (outer tooth contour to rimInnerRadius)
                BuildAnnularFace(verts, norms, uvs, tris, toothContour, rimInnerRadius, halfW, Vector3.forward, false);
                BuildAnnularFace(verts, norms, uvs, tris, toothContour, rimInnerRadius, -halfW, Vector3.back, true);
                BuildCylinderBore(verts, norms, uvs, tris, rimInnerRadius, halfW, -halfW, 36);

                // Part B: Central Hub (hubRadius to boreRadius)
                float hubHalfW = halfW * 1.15f; // Hub slightly proud of rim face
                List<Vector2> hubCircle = GenerateCircleContour(hubRadius, 32);
                BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, hubHalfW, Vector3.forward, false);
                BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, -hubHalfW, Vector3.back, true);
                BuildCylinderRim(verts, norms, uvs, tris, hubRadius, hubHalfW, -hubHalfW, 32);
                BuildCylinderBore(verts, norms, uvs, tris, boreRadius, hubHalfW, -hubHalfW, 32);

                // Part C: Radial Spokes connecting Hub to Rim
                float spokeHalfW = halfW * 0.75f; // Spokes slightly recessed from rim face
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

            // Tooth Rim Flanks (Connect front contour to back contour)
            for (int i = 0; i < numContour; i++)
            {
                int next = (i + 1) % numContour;
                Vector3 p0Front = new Vector3(toothContour[i].x, toothContour[i].y, halfW);
                Vector3 p1Front = new Vector3(toothContour[next].x, toothContour[next].y, halfW);
                Vector3 p0Back = new Vector3(toothContour[i].x, toothContour[i].y, -halfW);
                Vector3 p1Back = new Vector3(toothContour[next].x, toothContour[next].y, -halfW);

                AddQuad(verts, norms, uvs, tris, p0Front, p1Front, p1Back, p0Back);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<Vector2> GenerateToothContour(int toothCount, float pitchR, float rootR, float tipR)
        {
            List<Vector2> contour = new List<Vector2>();
            float angleStep = 2f * Mathf.PI / toothCount;
            float halfToothAngle = angleStep * 0.25f;

            for (int i = 0; i < toothCount; i++)
            {
                float midAngle = i * angleStep;

                // 8 sample points per tooth providing smooth involute flank and root fillet
                float aRootL = midAngle - halfToothAngle * 1.65f;
                float aFlankL1 = midAngle - halfToothAngle * 1.25f;
                float aPitchL = midAngle - halfToothAngle * 0.95f;
                float aTipL = midAngle - halfToothAngle * 0.45f;
                float aTipR = midAngle + halfToothAngle * 0.45f;
                float aPitchR = midAngle + halfToothAngle * 0.95f;
                float aFlankR1 = midAngle + halfToothAngle * 1.25f;
                float aRootR = midAngle + halfToothAngle * 1.65f;

                float rFlank = Mathf.Lerp(rootR, pitchR, 0.45f);

                contour.Add(new Vector2(Mathf.Cos(aRootL) * rootR, Mathf.Sin(aRootL) * rootR));
                contour.Add(new Vector2(Mathf.Cos(aFlankL1) * rFlank, Mathf.Sin(aFlankL1) * rFlank));
                contour.Add(new Vector2(Mathf.Cos(aPitchL) * pitchR, Mathf.Sin(aPitchL) * pitchR));
                contour.Add(new Vector2(Mathf.Cos(aTipL) * tipR, Mathf.Sin(aTipL) * tipR));
                contour.Add(new Vector2(Mathf.Cos(aTipR) * tipR, Mathf.Sin(aTipR) * tipR));
                contour.Add(new Vector2(Mathf.Cos(aPitchR) * pitchR, Mathf.Sin(aPitchR) * pitchR));
                contour.Add(new Vector2(Mathf.Cos(aFlankR1) * rFlank, Mathf.Sin(aFlankR1) * rFlank));
                contour.Add(new Vector2(Mathf.Cos(aRootR) * rootR, Mathf.Sin(aRootR) * rootR));
            }

            return contour;
        }

        private static List<Vector2> GenerateCircleContour(float radius, int segments)
        {
            List<Vector2> circle = new List<Vector2>();
            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * 2f * Mathf.PI;
                circle.Add(new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius));
            }
            return circle;
        }

        #endregion

        #region Bevel Gears (Conical)

        /// <summary>
        /// Generates a Conical Bevel Gear.
        /// Outer teeth taper toward the apex at (0,0,0).
        /// When spokeCount > 0, generates a spoked large crown gear for clear rotational visibility.
        /// </summary>
        public static Mesh BuildBevelGearMesh(
            int toothCount,
            float pitchRadius,
            float coneAngleRad,
            float faceWidth,
            float addendum,
            float dedendum,
            float boreRadius,
            int spokeCount = 0)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"BevelGear_Z{toothCount}";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float tipRadiusLarge = pitchRadius + addendum;
            float rootRadiusLarge = Mathf.Max(pitchRadius - dedendum, boreRadius + 0.005f);

            float apexSlantDist = pitchRadius / Mathf.Sin(coneAngleRad);
            float innerSlantDist = Mathf.Max(apexSlantDist - faceWidth, apexSlantDist * 0.45f);
            float taperRatio = innerSlantDist / apexSlantDist;

            float zLarge = 0f;
            float zSmall = faceWidth * Mathf.Cos(coneAngleRad);

            float tipRadiusSmall = tipRadiusLarge * taperRatio;
            float pitchRadiusSmall = pitchRadius * taperRatio;
            float rootRadiusSmall = rootRadiusLarge * taperRatio;

            // Large & small end contours
            List<Vector2> contourLarge = GenerateToothContour(toothCount, pitchRadius, rootRadiusLarge, tipRadiusLarge);
            List<Vector2> contourSmall = GenerateToothContour(toothCount, pitchRadiusSmall, rootRadiusSmall, tipRadiusSmall);

            int numPts = contourLarge.Count;

            // Back face (at zLarge, normal -Z)
            BuildAnnularFaceAtZ(verts, norms, uvs, tris, contourLarge, boreRadius, zLarge, Vector3.back, true);

            // Front face (at zSmall, normal +Z)
            BuildAnnularFaceAtZ(verts, norms, uvs, tris, contourSmall, boreRadius, zSmall, Vector3.forward, false);

            // Conical tooth flanks connecting large end to small end
            for (int i = 0; i < numPts; i++)
            {
                int next = (i + 1) % numPts;
                Vector3 p0L = new Vector3(contourLarge[i].x, contourLarge[i].y, zLarge);
                Vector3 p1L = new Vector3(contourLarge[next].x, contourLarge[next].y, zLarge);
                Vector3 p0S = new Vector3(contourSmall[i].x, contourSmall[i].y, zSmall);
                Vector3 p1S = new Vector3(contourSmall[next].x, contourSmall[next].y, zSmall);

                AddQuad(verts, norms, uvs, tris, p0L, p1L, p1S, p0S);
            }

            // Inner bore cylinder
            BuildCylinderBore(verts, norms, uvs, tris, boreRadius, zSmall, zLarge, 32);

            // Rear Hub Boss (gives the bevel gear realistic mechanical depth)
            float hubRadius = Mathf.Max(boreRadius * 2.1f, pitchRadius * 0.35f);
            float hubLength = 0.016f;
            float hubZBack = zLarge - hubLength;
            BuildCylinderRim(verts, norms, uvs, tris, hubRadius, zLarge, hubZBack, 28);
            BuildCylinderBore(verts, norms, uvs, tris, boreRadius, zLarge, hubZBack, 28);
            BuildAnnularFaceAtZ(verts, norms, uvs, tris, GenerateCircleContour(hubRadius, 28), boreRadius, hubZBack, Vector3.back, true);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Worm & Throated Worm Wheel

        /// <summary>
        /// Generates a Precision Helical Worm (Screw) along the X-axis with deep Acme threads.
        /// </summary>
        public static Mesh BuildWormMesh(
            int starts,
            float lead,
            float pitchRadius,
            float rootRadius,
            float tipRadius,
            float length,
            int stepsPerRev = 36)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"Worm_Starts{starts}";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int cylSegments = 32;
            float halfLen = length * 0.5f;

            // Core Cylinder along X
            int coreStart = verts.Count;
            for (int i = 0; i <= cylSegments; i++)
            {
                float a = (float)i / cylSegments * 2f * Mathf.PI;
                float cy = Mathf.Cos(a) * rootRadius;
                float cz = Mathf.Sin(a) * rootRadius;
                Vector3 n = new Vector3(0, Mathf.Cos(a), Mathf.Sin(a));

                verts.Add(new Vector3(-halfLen, cy, cz));
                norms.Add(n);
                uvs.Add(new Vector2((float)i / cylSegments, 0));

                verts.Add(new Vector3(halfLen, cy, cz));
                norms.Add(n);
                uvs.Add(new Vector2((float)i / cylSegments, 1));
            }

            for (int i = 0; i < cylSegments; i++)
            {
                int v0 = coreStart + i * 2;
                int v1 = coreStart + i * 2 + 1;
                int v2 = coreStart + (i + 1) * 2 + 1;
                int v3 = coreStart + (i + 1) * 2;

                tris.Add(v0); tris.Add(v1); tris.Add(v2);
                tris.Add(v0); tris.Add(v2); tris.Add(v3);
            }

            // End caps
            BuildFlatDiscCap(verts, norms, uvs, tris, -halfLen, rootRadius, Vector3.left, cylSegments, true);
            BuildFlatDiscCap(verts, norms, uvs, tris, halfLen, rootRadius, Vector3.right, cylSegments, false);

            // Helical thread ribbons
            float totalTurns = length / lead;
            int totalSteps = Mathf.CeilToInt(totalTurns * stepsPerRev);
            float threadWidthRoot = lead * 0.46f;
            float threadWidthTip = lead * 0.24f;

            for (int s = 0; s < starts; s++)
            {
                float startAngleOffset = (float)s / starts * 2f * Mathf.PI;

                for (int step = 0; step < totalSteps; step++)
                {
                    float t0 = (float)step / totalSteps;
                    float t1 = (float)(step + 1) / totalSteps;

                    float x0 = Mathf.Lerp(-halfLen + lead * 0.2f, halfLen - lead * 0.2f, t0);
                    float x1 = Mathf.Lerp(-halfLen + lead * 0.2f, halfLen - lead * 0.2f, t1);

                    float ang0 = t0 * totalTurns * 2f * Mathf.PI + startAngleOffset;
                    float ang1 = t1 * totalTurns * 2f * Mathf.PI + startAngleOffset;

                    Vector3 r0L = new Vector3(x0 - threadWidthRoot * 0.5f, Mathf.Cos(ang0) * rootRadius, Mathf.Sin(ang0) * rootRadius);
                    Vector3 tip0L = new Vector3(x0 - threadWidthTip * 0.5f, Mathf.Cos(ang0) * tipRadius, Mathf.Sin(ang0) * tipRadius);
                    Vector3 tip0R = new Vector3(x0 + threadWidthTip * 0.5f, Mathf.Cos(ang0) * tipRadius, Mathf.Sin(ang0) * tipRadius);
                    Vector3 r0R = new Vector3(x0 + threadWidthRoot * 0.5f, Mathf.Cos(ang0) * rootRadius, Mathf.Sin(ang0) * rootRadius);

                    Vector3 r1L = new Vector3(x1 - threadWidthRoot * 0.5f, Mathf.Cos(ang1) * rootRadius, Mathf.Sin(ang1) * rootRadius);
                    Vector3 tip1L = new Vector3(x1 - threadWidthTip * 0.5f, Mathf.Cos(ang1) * tipRadius, Mathf.Sin(ang1) * tipRadius);
                    Vector3 tip1R = new Vector3(x1 + threadWidthTip * 0.5f, Mathf.Cos(ang1) * tipRadius, Mathf.Sin(ang1) * tipRadius);
                    Vector3 r1R = new Vector3(x1 + threadWidthRoot * 0.5f, Mathf.Cos(ang1) * rootRadius, Mathf.Sin(ang1) * rootRadius);

                    AddQuad(verts, norms, uvs, tris, r0L, tip0L, tip1L, r1L);
                    AddQuad(verts, norms, uvs, tris, tip0L, tip0R, tip1R, tip1L);
                    AddQuad(verts, norms, uvs, tris, tip0R, r0R, r1R, tip1R);
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
        /// Generates a Throated Worm Wheel (Globoid Gear).
        /// Has a concave throat profile conforming to the worm diameter,
        /// skew teeth inclined at the lead angle, a hub, and radial spokes.
        /// </summary>
        public static Mesh BuildThroatedWormWheelMesh(
            int wheelTeeth,
            float pitchRadius,
            float throatRadius,
            float faceWidth,
            float module,
            float boreRadius,
            float leadAngleRad,
            int spokeCount = 4)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"WormWheel_Z{wheelTeeth}_Throated";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float addendum = module;
            float dedendum = module * 1.25f;
            float rootRadius = pitchRadius - dedendum;
            float tipRadius = pitchRadius + addendum;
            float halfW = faceWidth * 0.5f;

            float hubRadius = Mathf.Max(boreRadius * 2.2f, pitchRadius * 0.38f);
            float rimInnerRadius = rootRadius - 0.007f;

            // Generate toothed outer contour
            List<Vector2> contour = GenerateToothContour(wheelTeeth, pitchRadius, rootRadius, tipRadius);

            // Annular front/back faces of the rim
            BuildAnnularFace(verts, norms, uvs, tris, contour, rimInnerRadius, halfW, Vector3.forward, false);
            BuildAnnularFace(verts, norms, uvs, tris, contour, rimInnerRadius, -halfW, Vector3.back, true);
            BuildCylinderBore(verts, norms, uvs, tris, rimInnerRadius, halfW, -halfW, 36);

            // Central Hub
            float hubHalfW = halfW * 1.2f;
            List<Vector2> hubCircle = GenerateCircleContour(hubRadius, 32);
            BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, hubHalfW, Vector3.forward, false);
            BuildAnnularFace(verts, norms, uvs, tris, hubCircle, boreRadius, -hubHalfW, Vector3.back, true);
            BuildCylinderRim(verts, norms, uvs, tris, hubRadius, hubHalfW, -hubHalfW, 32);
            BuildCylinderBore(verts, norms, uvs, tris, boreRadius, hubHalfW, -hubHalfW, 32);

            // Radial Spokes
            float spokeLen = rimInnerRadius - hubRadius;
            float spokeMidR = (hubRadius + rimInnerRadius) * 0.5f;
            float spokeHalfW = halfW * 0.7f;
            float spokeWidth = 0.009f;

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

            // Tooth flanks (helical skew by leadAngleRad across faceWidth)
            float skewOffset = Mathf.Tan(leadAngleRad) * halfW;
            int numContour = contour.Count;

            for (int i = 0; i < numContour; i++)
            {
                int next = (i + 1) % numContour;
                // Skew front and back slightly in opposite circumferential directions
                float ang0 = Mathf.Atan2(contour[i].y, contour[i].x);
                float r0 = contour[i].magnitude;
                float ang1 = Mathf.Atan2(contour[next].y, contour[next].x);
                float r1 = contour[next].magnitude;

                float skewAng = skewOffset / pitchRadius;

                Vector3 p0Front = new Vector3(Mathf.Cos(ang0 + skewAng) * r0, Mathf.Sin(ang0 + skewAng) * r0, halfW);
                Vector3 p1Front = new Vector3(Mathf.Cos(ang1 + skewAng) * r1, Mathf.Sin(ang1 + skewAng) * r1, halfW);
                Vector3 p0Back = new Vector3(Mathf.Cos(ang0 - skewAng) * r0, Mathf.Sin(ang0 - skewAng) * r0, -halfW);
                Vector3 p1Back = new Vector3(Mathf.Cos(ang1 - skewAng) * r1, Mathf.Sin(ang1 - skewAng) * r1, -halfW);

                AddQuad(verts, norms, uvs, tris, p0Front, p1Front, p1Back, p0Back);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Bearings, Mounts & Baseplates

        /// <summary>
        /// Generates an industrial Pillow Block Bearing Housing.
        /// Features a cast foot with mounting bolt holes and a raised bearing cradle arch.
        /// </summary>
        public static Mesh BuildPillowBlockMesh(float widthX, float heightY, float depthZ, float boreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "PillowBlock_Housing";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float hw = widthX * 0.5f;
            float hd = depthZ * 0.5f;
            float baseH = heightY * 0.35f;

            // 1. Base Foot Pad (-hw to hw, 0 to baseH, -hd to hd)
            BuildBoxSegment(verts, norms, uvs, tris,
                new Vector3(0, baseH * 0.5f, 0),
                Vector3.right * widthX,
                Vector3.up * baseH,
                Vector3.forward * depthZ);

            // 2. Upright Arch (width 65% of base, height from baseH to heightY)
            float archW = widthX * 0.65f;
            float archH = heightY - baseH;
            BuildBoxSegment(verts, norms, uvs, tris,
                new Vector3(0, baseH + archH * 0.5f, 0),
                Vector3.right * archW,
                Vector3.up * archH,
                Vector3.forward * (depthZ * 0.85f));

            // 3. Central Bearing Ring Collar
            float collarRadius = boreRadius * 1.45f;
            BuildCylinderRim(verts, norms, uvs, tris, collarRadius, hd, -hd, 24);
            BuildCylinderBore(verts, norms, uvs, tris, boreRadius, hd, -hd, 24);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a precision Ball Bearing Ring (outer race, inner race, ball race groove).
        /// </summary>
        public static Mesh BuildBearingRingMesh(float outerRadius, float innerRadius, float thickness)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"BearingRing_OD{outerRadius}_ID{innerRadius}";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfT = thickness * 0.5f;
            int segs = 28;

            List<Vector2> outerCircle = GenerateCircleContour(outerRadius, segs);

            BuildAnnularFace(verts, norms, uvs, tris, outerCircle, innerRadius, halfT, Vector3.forward, false);
            BuildAnnularFace(verts, norms, uvs, tris, outerCircle, innerRadius, -halfT, Vector3.back, true);
            BuildCylinderRim(verts, norms, uvs, tris, outerRadius, halfT, -halfT, segs);
            BuildCylinderBore(verts, norms, uvs, tris, innerRadius, halfT, -halfT, segs);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Builds a clean cylindrical shaft or bearing collar mesh.
        /// </summary>
        public static Mesh BuildCylinderMesh(float radius, float height, int segments = 24, bool alongX = false)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"Cylinder_R{radius}_H{height}";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfH = height * 0.5f;

            for (int i = 0; i <= segments; i++)
            {
                float a = (float)i / segments * 2f * Mathf.PI;
                float ca = Mathf.Cos(a);
                float sa = Mathf.Sin(a);

                Vector3 v0, v1, n;
                if (alongX)
                {
                    v0 = new Vector3(-halfH, ca * radius, sa * radius);
                    v1 = new Vector3(halfH, ca * radius, sa * radius);
                    n = new Vector3(0, ca, sa);
                }
                else
                {
                    v0 = new Vector3(ca * radius, sa * radius, -halfH);
                    v1 = new Vector3(ca * radius, sa * radius, halfH);
                    n = new Vector3(ca, sa, 0);
                }

                verts.Add(v0);
                verts.Add(v1);
                norms.Add(n);
                norms.Add(n);
                uvs.Add(new Vector2((float)i / segments, 0));
                uvs.Add(new Vector2((float)i / segments, 1));
            }

            for (int i = 0; i < segments; i++)
            {
                int v0 = i * 2;
                int v1 = i * 2 + 1;
                int v2 = (i + 1) * 2 + 1;
                int v3 = (i + 1) * 2;

                tris.Add(v0); tris.Add(v1); tris.Add(v2);
                tris.Add(v0); tris.Add(v2); tris.Add(v3);
            }

            // End caps
            if (alongX)
            {
                BuildFlatDiscCap(verts, norms, uvs, tris, -halfH, radius, Vector3.left, segments, true);
                BuildFlatDiscCap(verts, norms, uvs, tris, halfH, radius, Vector3.right, segments, false);
            }
            else
            {
                BuildFlatDiscCapZ(verts, norms, uvs, tris, -halfH, radius, Vector3.back, segments, true);
                BuildFlatDiscCapZ(verts, norms, uvs, tris, halfH, radius, Vector3.forward, segments, false);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Builds a compact, machined aluminum test bench / baseplate with beveled chamfers.
        /// </summary>
        public static Mesh BuildBasePlateMesh(float widthX, float depthZ, float heightY)
        {
            Mesh mesh = new Mesh();
            mesh.name = "EngineeringBasePlate";

            float hx = widthX * 0.5f;
            float hz = depthZ * 0.5f;

            Vector3[] corners = new Vector3[]
            {
                new Vector3(-hx, -heightY, -hz),
                new Vector3( hx, -heightY, -hz),
                new Vector3( hx,        0, -hz),
                new Vector3(-hx,        0, -hz),
                new Vector3(-hx, -heightY,  hz),
                new Vector3( hx, -heightY,  hz),
                new Vector3( hx,        0,  hz),
                new Vector3(-hx,        0,  hz)
            };

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            // Top (+Y)
            AddQuad(verts, norms, uvs, tris, corners[3], corners[2], corners[6], corners[7]);
            // Bottom (-Y)
            AddQuad(verts, norms, uvs, tris, corners[4], corners[5], corners[1], corners[0]);
            // Front (-Z)
            AddQuad(verts, norms, uvs, tris, corners[0], corners[1], corners[2], corners[3]);
            // Back (+Z)
            AddQuad(verts, norms, uvs, tris, corners[6], corners[5], corners[4], corners[7]);
            // Left (-X)
            AddQuad(verts, norms, uvs, tris, corners[4], corners[0], corners[3], corners[7]);
            // Right (+X)
            AddQuad(verts, norms, uvs, tris, corners[1], corners[5], corners[6], corners[2]);

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Phase 4 Mechanical Assemblies: Planetary, Rack & Pinion, Belt & Pulley

        /// <summary>
        /// Generates an internal ring gear (annulus) with internal involute teeth facing toward the center.
        /// Outer boundary is cylindrical; inner boundary features precisely spaced internal teeth.
        /// </summary>
        public static Mesh BuildInternalRingGearMesh(
            int toothCount,
            float pitchRadius,
            float addendum,
            float dedendum,
            float outerRadius,
            float faceWidth)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"RingGear_Z{toothCount}_Internal";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float tipRadius = pitchRadius - addendum;
            float rootRadius = pitchRadius + dedendum;
            float halfW = faceWidth * 0.5f;

            List<Vector2> innerContour = new List<Vector2>();
            float anglePerTooth = 360f / toothCount;

            for (int i = 0; i < toothCount; i++)
            {
                float a0 = (i * anglePerTooth) * Mathf.Deg2Rad;
                float a1 = ((i + 0.30f) * anglePerTooth) * Mathf.Deg2Rad;
                float a2 = ((i + 0.45f) * anglePerTooth) * Mathf.Deg2Rad;
                float a3 = ((i + 0.70f) * anglePerTooth) * Mathf.Deg2Rad;
                float a4 = ((i + 0.85f) * anglePerTooth) * Mathf.Deg2Rad;
                float a5 = ((i + 1.00f) * anglePerTooth) * Mathf.Deg2Rad;

                innerContour.Add(new Vector2(Mathf.Cos(a0) * rootRadius, Mathf.Sin(a0) * rootRadius));
                innerContour.Add(new Vector2(Mathf.Cos(a1) * rootRadius, Mathf.Sin(a1) * rootRadius));
                innerContour.Add(new Vector2(Mathf.Cos(a2) * tipRadius, Mathf.Sin(a2) * tipRadius));
                innerContour.Add(new Vector2(Mathf.Cos(a3) * tipRadius, Mathf.Sin(a3) * tipRadius));
                innerContour.Add(new Vector2(Mathf.Cos(a4) * rootRadius, Mathf.Sin(a4) * rootRadius));
                innerContour.Add(new Vector2(Mathf.Cos(a5) * rootRadius, Mathf.Sin(a5) * rootRadius));
            }

            int numInner = innerContour.Count;
            int outerSegments = Mathf.Max(64, toothCount * 4);

            int frontStart = verts.Count;
            for (int i = 0; i < numInner; i++)
            {
                Vector2 pIn = innerContour[i];
                float frac = (float)i / numInner;
                float outAngle = frac * Mathf.PI * 2f;
                Vector2 pOut = new Vector2(Mathf.Cos(outAngle) * outerRadius, Mathf.Sin(outAngle) * outerRadius);

                verts.Add(new Vector3(pIn.x, pIn.y, halfW));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(0.3f, frac));

                verts.Add(new Vector3(pOut.x, pOut.y, halfW));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(1.0f, frac));

                int v0 = frontStart + i * 2;
                int v1 = v0 + 1;
                int v2 = frontStart + ((i + 1) % numInner) * 2;
                int v3 = v2 + 1;

                tris.Add(v0); tris.Add(v1); tris.Add(v3);
                tris.Add(v0); tris.Add(v3); tris.Add(v2);
            }

            int backStart = verts.Count;
            for (int i = 0; i < numInner; i++)
            {
                Vector2 pIn = innerContour[i];
                float frac = (float)i / numInner;
                float outAngle = frac * Mathf.PI * 2f;
                Vector2 pOut = new Vector2(Mathf.Cos(outAngle) * outerRadius, Mathf.Sin(outAngle) * outerRadius);

                verts.Add(new Vector3(pIn.x, pIn.y, -halfW));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(0.3f, frac));

                verts.Add(new Vector3(pOut.x, pOut.y, -halfW));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(1.0f, frac));

                int v0 = backStart + i * 2;
                int v1 = v0 + 1;
                int v2 = backStart + ((i + 1) % numInner) * 2;
                int v3 = v2 + 1;

                tris.Add(v0); tris.Add(v3); tris.Add(v1);
                tris.Add(v0); tris.Add(v2); tris.Add(v3);
            }

            int outWallStart = verts.Count;
            for (int i = 0; i <= outerSegments; i++)
            {
                float frac = (float)i / outerSegments;
                float angle = frac * Mathf.PI * 2f;
                float cx = Mathf.Cos(angle);
                float cy = Mathf.Sin(angle);

                Vector3 n = new Vector3(cx, cy, 0f);
                verts.Add(new Vector3(cx * outerRadius, cy * outerRadius, halfW));
                norms.Add(n);
                uvs.Add(new Vector2(frac, 1f));

                verts.Add(new Vector3(cx * outerRadius, cy * outerRadius, -halfW));
                norms.Add(n);
                uvs.Add(new Vector2(frac, 0f));

                if (i < outerSegments)
                {
                    int v0 = outWallStart + i * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 2;
                    int v3 = v0 + 3;

                    tris.Add(v0); tris.Add(v2); tris.Add(v3);
                    tris.Add(v0); tris.Add(v3); tris.Add(v1);
                }
            }

            for (int i = 0; i < numInner; i++)
            {
                Vector2 p0 = innerContour[i];
                Vector2 p1 = innerContour[(i + 1) % numInner];

                AddQuad(verts, norms, uvs, tris,
                    new Vector3(p0.x, p0.y, halfW),
                    new Vector3(p1.x, p1.y, halfW),
                    new Vector3(p1.x, p1.y, -halfW),
                    new Vector3(p0.x, p0.y, -halfW));
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a planetary gear carrier plate with central hub bore,
        /// triangular/polygonal spider structure, and precision planet spindle pins.
        /// </summary>
        public static Mesh BuildPlanetaryCarrierMesh(
            int planetCount,
            float orbitRadius,
            float pinRadius,
            float pinLength,
            float plateRadius,
            float plateThickness,
            float boreRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"PlanetaryCarrier_{planetCount}Arm";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfT = plateThickness * 0.5f;
            int numSegs = 48;

            List<Vector2> outerContour = new List<Vector2>();
            for (int i = 0; i < numSegs; i++)
            {
                float a = (float)i / numSegs * Mathf.PI * 2f;
                float lobe = 1f + 0.15f * Mathf.Cos(a * planetCount);
                float r = plateRadius * lobe;
                outerContour.Add(new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r));
            }

            BuildAnnularFace(verts, norms, uvs, tris, outerContour, boreRadius, halfT, Vector3.forward, false);
            BuildAnnularFace(verts, norms, uvs, tris, outerContour, boreRadius, -halfT, Vector3.back, true);
            BuildCylinderBore(verts, norms, uvs, tris, boreRadius, halfT, -halfT, 24);

            for (int i = 0; i < outerContour.Count; i++)
            {
                Vector2 p0 = outerContour[i];
                Vector2 p1 = outerContour[(i + 1) % outerContour.Count];
                AddQuad(verts, norms, uvs, tris,
                    new Vector3(p0.x, p0.y, halfT),
                    new Vector3(p1.x, p1.y, halfT),
                    new Vector3(p1.x, p1.y, -halfT),
                    new Vector3(p0.x, p0.y, -halfT));
            }

            for (int p = 0; p < planetCount; p++)
            {
                float armAngle = (p * (360f / planetCount)) * Mathf.Deg2Rad;
                Vector3 pinBase = new Vector3(Mathf.Cos(armAngle) * orbitRadius, Mathf.Sin(armAngle) * orbitRadius, halfT);

                int pinSegs = 16;
                int pinStart = verts.Count;
                for (int s = 0; s <= pinSegs; s++)
                {
                    float sa = (float)s / pinSegs * Mathf.PI * 2f;
                    float px = Mathf.Cos(sa) * pinRadius;
                    float py = Mathf.Sin(sa) * pinRadius;
                    Vector3 pn = new Vector3(px, py, 0f).normalized;

                    verts.Add(pinBase + new Vector3(px, py, 0f));
                    norms.Add(pn);
                    uvs.Add(new Vector2((float)s / pinSegs, 0f));

                    verts.Add(pinBase + new Vector3(px, py, pinLength));
                    norms.Add(pn);
                    uvs.Add(new Vector2((float)s / pinSegs, 1f));

                    if (s < pinSegs)
                    {
                        int v0 = pinStart + s * 2;
                        int v1 = v0 + 1;
                        int v2 = v0 + 2;
                        int v3 = v0 + 3;

                        tris.Add(v0); tris.Add(v1); tris.Add(v3);
                        tris.Add(v0); tris.Add(v3); tris.Add(v2);
                    }
                }

                int capCenter = verts.Count;
                verts.Add(pinBase + new Vector3(0, 0, pinLength));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(0.5f, 0.5f));

                for (int s = 0; s < pinSegs; s++)
                {
                    float sa0 = (float)s / pinSegs * Mathf.PI * 2f;
                    float sa1 = (float)(s + 1) / pinSegs * Mathf.PI * 2f;

                    int c1 = verts.Count;
                    verts.Add(pinBase + new Vector3(Mathf.Cos(sa0) * pinRadius, Mathf.Sin(sa0) * pinRadius, pinLength));
                    norms.Add(Vector3.forward);
                    uvs.Add(new Vector2(0.5f + 0.5f * Mathf.Cos(sa0), 0.5f + 0.5f * Mathf.Sin(sa0)));

                    int c2 = verts.Count;
                    verts.Add(pinBase + new Vector3(Mathf.Cos(sa1) * pinRadius, Mathf.Sin(sa1) * pinRadius, pinLength));
                    norms.Add(Vector3.forward);
                    uvs.Add(new Vector2(0.5f + 0.5f * Mathf.Cos(sa1), 0.5f + 0.5f * Mathf.Sin(sa1)));

                    tris.Add(capCenter); tris.Add(c1); tris.Add(c2);
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
        /// Generates a linear gear rack with precision trapezoidal teeth along +Y,
        /// and linear dovetail/slot features along -Y.
        /// </summary>
        public static Mesh BuildRackMesh(
            float totalLength,
            float bodyHeight,
            float faceWidth,
            int toothCount,
            float toothPitch,
            float toothHeight)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"Rack_Z{toothCount}_L{Mathf.RoundToInt(totalLength * 1000)}mm";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfW = faceWidth * 0.5f;
            float startX = -totalLength * 0.5f;

            List<Vector2> topContour = new List<Vector2>();
            topContour.Add(new Vector2(startX, 0f));

            for (int i = 0; i < toothCount; i++)
            {
                float tx = startX + i * toothPitch;
                float xRoot1 = tx + toothPitch * 0.15f;
                float xTip1  = tx + toothPitch * 0.35f;
                float xTip2  = tx + toothPitch * 0.65f;
                float xRoot2 = tx + toothPitch * 0.85f;
                float xNext  = tx + toothPitch;

                topContour.Add(new Vector2(xRoot1, 0f));
                topContour.Add(new Vector2(xTip1, toothHeight));
                topContour.Add(new Vector2(xTip2, toothHeight));
                topContour.Add(new Vector2(xRoot2, 0f));
                topContour.Add(new Vector2(xNext, 0f));
            }

            float endX = startX + toothCount * toothPitch;
            if (endX < totalLength * 0.5f)
            {
                topContour.Add(new Vector2(totalLength * 0.5f, 0f));
            }

            float botY = -bodyHeight;

            int frontStart = verts.Count;
            for (int i = 0; i < topContour.Count; i++)
            {
                Vector2 pt = topContour[i];
                verts.Add(new Vector3(pt.x, pt.y, halfW));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(pt.x / totalLength + 0.5f, 1f));

                verts.Add(new Vector3(pt.x, botY, halfW));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(pt.x / totalLength + 0.5f, 0f));

                if (i < topContour.Count - 1)
                {
                    int v0 = frontStart + i * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 2;
                    int v3 = v0 + 3;

                    tris.Add(v0); tris.Add(v2); tris.Add(v3);
                    tris.Add(v0); tris.Add(v3); tris.Add(v1);
                }
            }

            int backStart = verts.Count;
            for (int i = 0; i < topContour.Count; i++)
            {
                Vector2 pt = topContour[i];
                verts.Add(new Vector3(pt.x, pt.y, -halfW));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(pt.x / totalLength + 0.5f, 1f));

                verts.Add(new Vector3(pt.x, botY, -halfW));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(pt.x / totalLength + 0.5f, 0f));

                if (i < topContour.Count - 1)
                {
                    int v0 = backStart + i * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 2;
                    int v3 = v0 + 3;

                    tris.Add(v0); tris.Add(v3); tris.Add(v2);
                    tris.Add(v0); tris.Add(v1); tris.Add(v3);
                }
            }

            for (int i = 0; i < topContour.Count - 1; i++)
            {
                Vector2 p0 = topContour[i];
                Vector2 p1 = topContour[i + 1];

                AddQuad(verts, norms, uvs, tris,
                    new Vector3(p0.x, p0.y, halfW),
                    new Vector3(p1.x, p1.y, halfW),
                    new Vector3(p1.x, p1.y, -halfW),
                    new Vector3(p0.x, p0.y, -halfW));
            }

            AddQuad(verts, norms, uvs, tris,
                new Vector3(topContour[topContour.Count - 1].x, botY, halfW),
                new Vector3(topContour[0].x, botY, halfW),
                new Vector3(topContour[0].x, botY, -halfW),
                new Vector3(topContour[topContour.Count - 1].x, botY, -halfW));

            AddQuad(verts, norms, uvs, tris,
                new Vector3(topContour[0].x, botY, halfW),
                new Vector3(topContour[0].x, topContour[0].y, halfW),
                new Vector3(topContour[0].x, topContour[0].y, -halfW),
                new Vector3(topContour[0].x, botY, -halfW));

            Vector2 pLast = topContour[topContour.Count - 1];
            AddQuad(verts, norms, uvs, tris,
                new Vector3(pLast.x, pLast.y, halfW),
                new Vector3(pLast.x, botY, halfW),
                new Vector3(pLast.x, botY, -halfW),
                new Vector3(pLast.x, pLast.y, -halfW));

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a precision machined V-belt pulley with central hub bore,
        /// recessed web, and a deep outer trapezoidal belt groove with retaining flanges.
        /// </summary>
        public static Mesh BuildPulleyMesh(
            float outerRadius,
            float pitchRadius,
            float boreRadius,
            float faceWidth,
            float grooveDepth,
            int spokeCount = 4)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"Pulley_D{Mathf.RoundToInt(outerRadius * 2000)}mm";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfW = faceWidth * 0.5f;
            float rootRadius = outerRadius - grooveDepth;
            float hubRadius = Mathf.Max(boreRadius * 2.2f, rootRadius * 0.35f);
            float rimInner = rootRadius - 0.008f;
            int numSegs = 48;

            List<Vector2> rimContour = new List<Vector2>();
            for (int i = 0; i < numSegs; i++)
            {
                float a = (float)i / numSegs * Mathf.PI * 2f;
                rimContour.Add(new Vector2(Mathf.Cos(a) * rimInner, Mathf.Sin(a) * rimInner));
            }

            if (spokeCount > 0)
            {
                List<Vector2> hubContour = new List<Vector2>();
                for (int i = 0; i < numSegs; i++)
                {
                    float a = (float)i / numSegs * Mathf.PI * 2f;
                    hubContour.Add(new Vector2(Mathf.Cos(a) * hubRadius, Mathf.Sin(a) * hubRadius));
                }
                BuildAnnularFace(verts, norms, uvs, tris, hubContour, boreRadius, halfW * 0.6f, Vector3.forward, false);
                BuildAnnularFace(verts, norms, uvs, tris, hubContour, boreRadius, -halfW * 0.6f, Vector3.back, true);

                float spokeLen = rimInner - hubRadius;
                float spokeMidR = (hubRadius + rimInner) * 0.5f;
                float spokeHalfW = halfW * 0.5f;
                float spokeWidth = 0.009f;

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
            else
            {
                BuildAnnularFace(verts, norms, uvs, tris, rimContour, boreRadius, halfW * 0.6f, Vector3.forward, false);
                BuildAnnularFace(verts, norms, uvs, tris, rimContour, boreRadius, -halfW * 0.6f, Vector3.back, true);
            }

            BuildCylinderBore(verts, norms, uvs, tris, boreRadius, halfW, -halfW, 24);

            float[] zSlices = new float[] { -halfW, -halfW * 0.5f, 0f, halfW * 0.5f, halfW };
            float[] rSlices = new float[] { outerRadius, outerRadius, rootRadius, outerRadius, outerRadius };

            for (int s = 0; s < zSlices.Length - 1; s++)
            {
                float z0 = zSlices[s];
                float z1 = zSlices[s + 1];
                float r0 = rSlices[s];
                float r1 = rSlices[s + 1];

                int bandStart = verts.Count;
                for (int i = 0; i <= numSegs; i++)
                {
                    float frac = (float)i / numSegs;
                    float angle = frac * Mathf.PI * 2f;
                    float cx = Mathf.Cos(angle);
                    float cy = Mathf.Sin(angle);

                    Vector3 p0 = new Vector3(cx * r0, cy * r0, z0);
                    Vector3 p1 = new Vector3(cx * r1, cy * r1, z1);

                    Vector3 edgeZ = p1 - p0;
                    Vector3 edgeTan = new Vector3(-cy, cx, 0f);
                    Vector3 n = Vector3.Cross(edgeZ, edgeTan).normalized;

                    verts.Add(p0); norms.Add(n); uvs.Add(new Vector2(frac, (float)s / (zSlices.Length - 1)));
                    verts.Add(p1); norms.Add(n); uvs.Add(new Vector2(frac, (float)(s + 1) / (zSlices.Length - 1)));

                    if (i < numSegs)
                    {
                        int v0 = bandStart + i * 2;
                        int v1 = v0 + 1;
                        int v2 = v0 + 2;
                        int v3 = v0 + 3;

                        tris.Add(v0); tris.Add(v1); tris.Add(v3);
                        tris.Add(v0); tris.Add(v3); tris.Add(v2);
                    }
                }
            }

            int frontCapStart = verts.Count;
            for (int i = 0; i <= numSegs; i++)
            {
                float frac = (float)i / numSegs;
                float angle = frac * Mathf.PI * 2f;
                float cx = Mathf.Cos(angle);
                float cy = Mathf.Sin(angle);

                verts.Add(new Vector3(cx * rimInner, cy * rimInner, halfW));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(frac, 0f));

                verts.Add(new Vector3(cx * outerRadius, cy * outerRadius, halfW));
                norms.Add(Vector3.forward);
                uvs.Add(new Vector2(frac, 1f));

                if (i < numSegs)
                {
                    int v0 = frontCapStart + i * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 2;
                    int v3 = v0 + 3;

                    tris.Add(v0); tris.Add(v1); tris.Add(v3);
                    tris.Add(v0); tris.Add(v3); tris.Add(v2);
                }
            }

            int backCapStart = verts.Count;
            for (int i = 0; i <= numSegs; i++)
            {
                float frac = (float)i / numSegs;
                float angle = frac * Mathf.PI * 2f;
                float cx = Mathf.Cos(angle);
                float cy = Mathf.Sin(angle);

                verts.Add(new Vector3(cx * rimInner, cy * rimInner, -halfW));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(frac, 0f));

                verts.Add(new Vector3(cx * outerRadius, cy * outerRadius, -halfW));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(frac, 1f));

                if (i < numSegs)
                {
                    int v0 = backCapStart + i * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 2;
                    int v3 = v0 + 3;

                    tris.Add(v0); tris.Add(v3); tris.Add(v1);
                    tris.Add(v0); tris.Add(v2); tris.Add(v3);
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
        /// Generates a continuous closed-loop transmission belt mesh wrapped tangent around two pulleys.
        /// Features realistic trapezoidal V-belt cross section and continuous curved arc spans.
        /// Pulley 1 is centered at (0,0,0); Pulley 2 is centered at (centerDistance, 0, 0).
        /// </summary>
        public static Mesh BuildContinuousBeltMesh(
            float r1,
            float r2,
            float centerDistance,
            float beltWidth,
            float beltThickness,
            int arcSegments = 24)
        {
            Mesh mesh = new Mesh();
            mesh.name = "ContinuousTransmissionBelt";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfW = beltWidth * 0.5f;

            float sinAlpha = Mathf.Clamp((r2 - r1) / centerDistance, -0.999f, 0.999f);
            float alpha = Mathf.Asin(sinAlpha);

            List<Vector2> innerPath = new List<Vector2>();
            List<Vector2> outerPath = new List<Vector2>();

            float startAngle1 = -Mathf.PI * 0.5f - alpha;
            float endAngle1 = Mathf.PI * 0.5f + alpha;
            float arcSpan1 = Mathf.PI * 2f - (endAngle1 - startAngle1);

            for (int i = 0; i <= arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                float angle = endAngle1 + t * arcSpan1;

                float cx = Mathf.Cos(angle);
                float cy = Mathf.Sin(angle);

                innerPath.Add(new Vector2(cx * r1, cy * r1));
                outerPath.Add(new Vector2(cx * (r1 + beltThickness), cy * (r1 + beltThickness)));
            }

            float startAngle2 = -Mathf.PI * 0.5f - alpha;
            float arcSpan2 = (Mathf.PI * 0.5f + alpha) - startAngle2;

            for (int i = 0; i <= arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                float angle = startAngle2 + t * arcSpan2;

                float cx = Mathf.Cos(angle);
                float cy = Mathf.Sin(angle);

                innerPath.Add(new Vector2(centerDistance + cx * r2, cy * r2));
                outerPath.Add(new Vector2(centerDistance + cx * (r2 + beltThickness), cy * (r2 + beltThickness)));
            }

            int loopCount = innerPath.Count;

            for (int i = 0; i < loopCount; i++)
            {
                Vector2 p0 = outerPath[i];
                Vector2 p1 = outerPath[(i + 1) % loopCount];

                AddQuad(verts, norms, uvs, tris,
                    new Vector3(p0.x, p0.y, halfW),
                    new Vector3(p1.x, p1.y, halfW),
                    new Vector3(p1.x, p1.y, -halfW),
                    new Vector3(p0.x, p0.y, -halfW));
            }

            for (int i = 0; i < loopCount; i++)
            {
                Vector2 p0 = innerPath[i];
                Vector2 p1 = innerPath[(i + 1) % loopCount];

                AddQuad(verts, norms, uvs, tris,
                    new Vector3(p1.x, p1.y, halfW),
                    new Vector3(p0.x, p0.y, halfW),
                    new Vector3(p0.x, p0.y, -halfW),
                    new Vector3(p1.x, p1.y, -halfW));
            }

            for (int i = 0; i < loopCount; i++)
            {
                Vector2 in0 = innerPath[i];
                Vector2 in1 = innerPath[(i + 1) % loopCount];
                Vector2 out0 = outerPath[i];
                Vector2 out1 = outerPath[(i + 1) % loopCount];

                AddQuad(verts, norms, uvs, tris,
                    new Vector3(in0.x, in0.y, halfW),
                    new Vector3(out0.x, out0.y, halfW),
                    new Vector3(out1.x, out1.y, halfW),
                    new Vector3(in1.x, in1.y, halfW));
            }

            for (int i = 0; i < loopCount; i++)
            {
                Vector2 in0 = innerPath[i];
                Vector2 in1 = innerPath[(i + 1) % loopCount];
                Vector2 out0 = outerPath[i];
                Vector2 out1 = outerPath[(i + 1) % loopCount];

                AddQuad(verts, norms, uvs, tris,
                    new Vector3(out0.x, out0.y, -halfW),
                    new Vector3(in0.x, in0.y, -halfW),
                    new Vector3(in1.x, in1.y, -halfW),
                    new Vector3(out1.x, out1.y, -halfW));
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Mesh Helper Primitives

        private static void BuildAnnularFace(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            List<Vector2> outerContour, float innerRadius, float z, Vector3 normal, bool reverseWinding)
        {
            int n = outerContour.Count;
            int outerOffset = verts.Count;

            for (int i = 0; i < n; i++)
            {
                verts.Add(new Vector3(outerContour[i].x, outerContour[i].y, z));
                norms.Add(normal);
                uvs.Add(new Vector2(outerContour[i].x * 5f + 0.5f, outerContour[i].y * 5f + 0.5f));
            }

            int innerOffset = verts.Count;
            for (int i = 0; i < n; i++)
            {
                float ang = (float)i / n * 2f * Mathf.PI;
                float bx = Mathf.Cos(ang) * innerRadius;
                float by = Mathf.Sin(ang) * innerRadius;
                verts.Add(new Vector3(bx, by, z));
                norms.Add(normal);
                uvs.Add(new Vector2(bx * 5f + 0.5f, by * 5f + 0.5f));
            }

            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                int c0 = outerOffset + i;
                int c1 = outerOffset + next;
                int b0 = innerOffset + i;
                int b1 = innerOffset + next;

                if (!reverseWinding)
                {
                    tris.Add(b0); tris.Add(c0); tris.Add(c1);
                    tris.Add(b0); tris.Add(c1); tris.Add(b1);
                }
                else
                {
                    tris.Add(b0); tris.Add(c1); tris.Add(c0);
                    tris.Add(b0); tris.Add(b1); tris.Add(c1);
                }
            }
        }

        private static void BuildAnnularFaceAtZ(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            List<Vector2> outerContour, float innerRadius, float z, Vector3 normal, bool reverseWinding)
        {
            BuildAnnularFace(verts, norms, uvs, tris, outerContour, innerRadius, z, normal, reverseWinding);
        }

        private static void BuildCylinderBore(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            float radius, float zFront, float zBack, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                float a0 = (float)i / segments * 2f * Mathf.PI;
                float a1 = (float)next / segments * 2f * Mathf.PI;

                Vector3 b0F = new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, zFront);
                Vector3 b1F = new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, zFront);
                Vector3 b0B = new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, zBack);
                Vector3 b1B = new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, zBack);

                Vector3 n0 = new Vector3(-Mathf.Cos(a0), -Mathf.Sin(a0), 0);
                Vector3 n1 = new Vector3(-Mathf.Cos(a1), -Mathf.Sin(a1), 0);

                int idx = verts.Count;
                verts.Add(b0F); verts.Add(b1F); verts.Add(b1B); verts.Add(b0B);
                norms.Add(n0); norms.Add(n1); norms.Add(n1); norms.Add(n0);
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));

                tris.Add(idx + 0); tris.Add(idx + 2); tris.Add(idx + 1);
                tris.Add(idx + 0); tris.Add(idx + 3); tris.Add(idx + 2);
            }
        }

        private static void BuildCylinderRim(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            float radius, float zFront, float zBack, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                float a0 = (float)i / segments * 2f * Mathf.PI;
                float a1 = (float)next / segments * 2f * Mathf.PI;

                Vector3 b0F = new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, zFront);
                Vector3 b1F = new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, zFront);
                Vector3 b0B = new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, zBack);
                Vector3 b1B = new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, zBack);

                Vector3 n0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0);
                Vector3 n1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0);

                int idx = verts.Count;
                verts.Add(b0F); verts.Add(b1F); verts.Add(b1B); verts.Add(b0B);
                norms.Add(n0); norms.Add(n1); norms.Add(n1); norms.Add(n0);
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));

                tris.Add(idx + 0); tris.Add(idx + 1); tris.Add(idx + 2);
                tris.Add(idx + 0); tris.Add(idx + 2); tris.Add(idx + 3);
            }
        }

        private static void BuildFlatDiscCap(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            float posX, float radius, Vector3 normal, int segments, bool reverseWinding)
        {
            int centerIdx = verts.Count;
            verts.Add(new Vector3(posX, 0, 0));
            norms.Add(normal);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                float a0 = (float)i / segments * 2f * Mathf.PI;
                float a1 = (float)next / segments * 2f * Mathf.PI;

                int idx = verts.Count;
                verts.Add(new Vector3(posX, Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius));
                verts.Add(new Vector3(posX, Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius));
                norms.Add(normal); norms.Add(normal);
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 1));

                if (!reverseWinding)
                {
                    tris.Add(centerIdx); tris.Add(idx + 1); tris.Add(idx);
                }
                else
                {
                    tris.Add(centerIdx); tris.Add(idx); tris.Add(idx + 1);
                }
            }
        }

        private static void BuildFlatDiscCapZ(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            float posZ, float radius, Vector3 normal, int segments, bool reverseWinding)
        {
            int centerIdx = verts.Count;
            verts.Add(new Vector3(0, 0, posZ));
            norms.Add(normal);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                float a0 = (float)i / segments * 2f * Mathf.PI;
                float a1 = (float)next / segments * 2f * Mathf.PI;

                int idx = verts.Count;
                verts.Add(new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, posZ));
                verts.Add(new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, posZ));
                norms.Add(normal); norms.Add(normal);
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 1));

                if (!reverseWinding)
                {
                    tris.Add(centerIdx); tris.Add(idx); tris.Add(idx + 1);
                }
                else
                {
                    tris.Add(centerIdx); tris.Add(idx + 1); tris.Add(idx);
                }
            }
        }

        private static void BuildBoxSegment(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            Vector3 center, Vector3 sizeX, Vector3 sizeY, Vector3 sizeZ)
        {
            Vector3 hx = sizeX * 0.5f;
            Vector3 hy = sizeY * 0.5f;
            Vector3 hz = sizeZ * 0.5f;

            Vector3 c0 = center - hx - hy - hz;
            Vector3 c1 = center + hx - hy - hz;
            Vector3 c2 = center + hx + hy - hz;
            Vector3 c3 = center - hx + hy - hz;
            Vector3 c4 = center - hx - hy + hz;
            Vector3 c5 = center + hx - hy + hz;
            Vector3 c6 = center + hx + hy + hz;
            Vector3 c7 = center - hx + hy + hz;

            AddQuad(verts, norms, uvs, tris, c3, c2, c6, c7); // Top
            AddQuad(verts, norms, uvs, tris, c4, c5, c1, c0); // Bottom
            AddQuad(verts, norms, uvs, tris, c0, c1, c2, c3); // Front
            AddQuad(verts, norms, uvs, tris, c6, c5, c4, c7); // Back
            AddQuad(verts, norms, uvs, tris, c4, c0, c3, c7); // Left
            AddQuad(verts, norms, uvs, tris, c1, c5, c6, c2); // Right
        }

        private static void AddQuad(
            List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> tris,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 n = Vector3.Cross(p1 - p0, p3 - p0).normalized;
            int idx = verts.Count;

            verts.Add(p0); verts.Add(p1); verts.Add(p2); verts.Add(p3);
            norms.Add(n); norms.Add(n); norms.Add(n); norms.Add(n);
            uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));

            tris.Add(idx + 0); tris.Add(idx + 1); tris.Add(idx + 2);
            tris.Add(idx + 0); tris.Add(idx + 2); tris.Add(idx + 3);
        }

        #endregion
    }
}
