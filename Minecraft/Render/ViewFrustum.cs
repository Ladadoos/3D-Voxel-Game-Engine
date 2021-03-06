﻿using OpenTK;
using System;

namespace Minecraft
{
    /// <summary>
    /// Representation of a view frustum used for culling objects.
    /// </summary>
    class ViewFrustum
    {
        //Implementation based on: https://cgvr.cs.uni-bremen.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

        struct FrustumPlane
        {
            public Vector3 normal;
            public float distanceToOrigin;

            public float GetSignedDistance(Vector3 point)
            {
                return Vector3.Dot(point, normal) - distanceToOrigin;
            }
        }

        private float nearWidth, nearHeight, farWidth, farHeight;
        private readonly FrustumPlane[] frustumPlanes = new FrustumPlane[6];

        public ViewFrustum(ProjectionMatrixInfo projectionInfo)
        {
            CalculateNearFarWidthHeight(projectionInfo);
        }

        private void CalculateNearFarWidthHeight(ProjectionMatrixInfo pInfo)
        {
            float aspectRatio = pInfo.WindowPixelWidth / (float)pInfo.WindowPixelHeight;
            const float extesion = 2;
            float tan = (float)Math.Tan(pInfo.FieldOfView * 0.5F) * extesion;
            nearHeight = tan * pInfo.DistanceNearPlane;
            nearWidth = nearHeight * aspectRatio;
            farHeight = tan * pInfo.DistanceFarPlane;
            farWidth = farHeight * aspectRatio;
        }

        public void UpdateFrustumPoints(Camera camera)
        {
            Vector3 zAxis = -camera.Forward;
            Vector3 xAxis = camera.Right;
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

            Vector3 nearCenter = camera.Position - zAxis * camera.CurrentProjection.DistanceNearPlane;
            Vector3 farCenter = camera.Position - zAxis * camera.CurrentProjection.DistanceFarPlane;

            frustumPlanes[0].normal = -zAxis; //near plane
            frustumPlanes[0].distanceToOrigin = Vector3.Dot(frustumPlanes[0].normal, nearCenter);

            frustumPlanes[1].normal = zAxis; //far plane
            frustumPlanes[1].distanceToOrigin = Vector3.Dot(frustumPlanes[1].normal, farCenter);

            Vector3 temporary, normal;

            temporary = (nearCenter + yAxis * nearHeight) - camera.Position;
            temporary.Normalize();
            normal = Vector3.Cross(temporary, xAxis);
            frustumPlanes[2].normal = normal; //top plane
            frustumPlanes[2].distanceToOrigin = Vector3.Dot(frustumPlanes[2].normal, nearCenter + yAxis * nearHeight);

            temporary = (nearCenter - yAxis * nearHeight) - camera.Position;
            temporary.Normalize();
            normal = Vector3.Cross(xAxis, temporary);
            frustumPlanes[3].normal = normal; //bottom plane
            frustumPlanes[3].distanceToOrigin = Vector3.Dot(frustumPlanes[3].normal, nearCenter - yAxis * nearHeight);

            temporary = (nearCenter - xAxis * nearWidth) - camera.Position;
            temporary.Normalize();
            normal = Vector3.Cross(temporary, yAxis);
            frustumPlanes[4].normal = normal; //left plane
            frustumPlanes[4].distanceToOrigin = Vector3.Dot(frustumPlanes[4].normal, nearCenter - xAxis * nearWidth);

            temporary = (nearCenter + xAxis * nearWidth) - camera.Position;
            temporary.Normalize();
            normal = Vector3.Cross(yAxis, temporary);
            frustumPlanes[5].normal = normal; //right plane
            frustumPlanes[5].distanceToOrigin = Vector3.Dot(frustumPlanes[5].normal, nearCenter + xAxis * nearWidth);
        }

        public bool IsSphereInFrustum(Vector3 position, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (frustumPlanes[i].GetSignedDistance(position) < -radius)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsAABBInFrustum(AxisAlignedBox aabb)
        {
            Vector3[] corners = aabb.GetAllCorners();
            for (int i = 0; i < 6; i++)
            {
                int inside = 0;
                foreach (Vector3 corner in corners)
                {
                    if (frustumPlanes[i].GetSignedDistance(corner) >= 0)
                    {
                        inside++;
                    }
                }
                if (inside == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
