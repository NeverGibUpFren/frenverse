using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tessera
{
    public class CubeCellDrawingType : ICellDrawingType
    {
        public static CubeCellDrawingType Instance = new CubeCellDrawingType();

        public bool Is2D => false;

        public Vector3[] GetSubFaceVertices(Vector3Int offset, Sylves.CellDir dir, SubFace subface, Vector3 center, Vector3 cellSize)
        {
            var u = new DrawingUtil(center, cellSize);

            var up = ((Sylves.CubeDir)dir).Up();
            GetFaceCenterAndNormal(offset, dir, center, cellSize, out var faceCenter, out var forward);
            var right = Vector3.Cross(forward, up);

            return SquareFaceDrawingUtils.GetSubFaceVertices(subface, faceCenter, u.ScaleByHSize(up), u.ScaleByHSize(right));
        }


        public Vector3[] GetFaceVertices(Vector3Int offset, Sylves.CellDir dir, Vector3 center, Vector3 cellSize)
        {
            var u = new DrawingUtil(center, cellSize);

            var up = ((Sylves.CubeDir)dir).Up();
            GetFaceCenterAndNormal(offset, dir, center, cellSize, out var faceCenter, out var forward);
            var right = Vector3.Cross(forward, up);

            return SquareFaceDrawingUtils.GetFaceVertices(faceCenter, u.ScaleByHSize(up), u.ScaleByHSize(right));

        }

        public void GetFaceCenterAndNormal(Vector3Int offset, Sylves.CellDir dir, Vector3 center, Vector3 cellSize, out Vector3 faceCenter, out Vector3 faceNormal)
        {
            var u = new DrawingUtil(center, cellSize);

            var forward = ((Sylves.CubeDir)dir).Forward();

            faceCenter = center + u.ScaleBySize(offset) + u.ScaleByHSize(forward);
            faceNormal = forward;
        }


        public RaycastCellHit Raycast(Ray ray, Vector3Int offset, Vector3 center, Vector3 cellSize, float? minDistance, float? maxDistance)
        {
            var u = new DrawingUtil(center, cellSize);

            var currentHit = Raycast(center + u.ScaleBySize(offset), u.hsize, ray, out var currentHitCellDir, out var currentHitPoint);


            if (!currentHit)
            {
                return null;
            }

            var currentHitDir = (Sylves.CubeDir)currentHitCellDir;

            var d2 = (currentHitPoint - ray.origin).sqrMagnitude;

            if (maxDistance != null && d2 >= maxDistance * maxDistance)
            {
                return null;
            }

            if (minDistance != null && d2 <= minDistance * minDistance)
            {
                return null;
            }

            var hit = new RaycastCellHit
            {
                dir = currentHitCellDir,
                point = currentHitPoint,
            };

            var up = currentHitDir.Up();
            var forward = currentHitDir.Forward();
            var right = Vector3.Cross(forward, up);

            var p = currentHitPoint - (center + u.ScaleBySize(offset) + u.ScaleByHSize(forward));
            p = new Vector3(p.x / u.hsize.x, p.y / u.hsize.y, p.z / u.hsize.z);
            var p2 = new Vector2(Vector3.Dot(p, right), Vector3.Dot(p, up));
            hit.subface = p2;
            return hit;
        }

        // Casts a ray at an axis aligned box, and reutrns which face and where the hit is.
        private bool Raycast(Vector3 center, Vector3 hsize, Ray ray, out Sylves.CellDir dir, out Vector3 point)
        {
            var rayDir = ray.direction.normalized;
            // r.dir is unit direction vector of ray
            var dirfrac = new Vector3(1 / rayDir.x, 1 / rayDir.y, 1 / rayDir.z);
            // lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
            // r.org is origin of ray
            Vector3 t1 = Vector3.Scale(center - hsize - ray.origin, dirfrac);
            Vector3 t2 = Vector3.Scale(center + hsize - ray.origin, dirfrac);

            var t3 = Vector3.Min(t1, t2);
            float tmin = Max(t3);
            float tmax = Min(Vector3.Max(t1, t2));

            float t;
            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0)
            {
                t = tmax;
                dir = default;
                point = default;
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                t = tmax;
                dir = default;
                point = default;
                return false;
            }

            t = tmin;
            point = ray.origin + t * rayDir;
            dir = (Sylves.CellDir)(tmin == t3.x ? (rayDir.x > 0 ? Sylves.CubeDir.Left : Sylves.CubeDir.Right)
                : tmin == t3.y ? (rayDir.y > 0 ? Sylves.CubeDir.Down : Sylves.CubeDir.Up)
                : (rayDir.z > 0 ? Sylves.CubeDir.Back : Sylves.CubeDir.Forward));
            return true;
        }

        public SubFace RoundSubFace(Vector3Int offset, Sylves.CellDir dir, Vector2 p2, PaintMode paintMode)
        {
            return SquareFaceDrawingUtils.RoundSubFace(p2, paintMode);
        }

        public bool IsAffected(Vector3Int parentOffset, Sylves.CellDir parentDir, SubFace parentSubface, Vector3Int childOffset, Sylves.CellDir childDir, SubFace childSubface, PaintMode paintMode)
        {
            var p2 = SquareFaceDrawingUtils.ToSubFaceVector(parentSubface);
            var p1 = SquareFaceDrawingUtils.ToSubFaceVector(childSubface);


            var up = ((Sylves.CubeDir)childDir).Up();
            var forward = ((Sylves.CubeDir)childDir).Forward();
            var right = Vector3.Cross(forward, up);


            var up2 = ((Sylves.CubeDir)parentDir).Up();
            var forward2 = ((Sylves.CubeDir)parentDir).Forward();
            var right2 = Vector3.Cross(forward2, up2);

            Vector3 v1, v2;

            switch (paintMode)
            {
                case PaintMode.Pencil:
                    return childOffset == parentOffset && childDir == parentDir && p1 == p2;
                case PaintMode.Vertex:
                    v1 = childOffset * 2 + forward + right * p1.x + up * p1.y;
                    v2 = parentOffset * 2 + forward2 + right2 * p2.x + up2 * p2.y;
                    return (v1 - v2).sqrMagnitude < 0.01;
                case PaintMode.Edge:
                    var v = forward2 + p2.x * right2 + p2.y * (Vector3)up2;
                    var isX = Math.Abs(Vector3.Dot(v, right));
                    var isY = Math.Abs(Vector3.Dot(v, up));
                    v1 = childOffset * 2 + forward + right * p1.x * isX + (Vector3)up * p1.y * isY;
                    v2 = parentOffset * 2 + forward2 + right2 * p2.x + up2 * p2.y;
                    return (v1 - v2).sqrMagnitude < 0.01;
                case PaintMode.Face:
                case PaintMode.Add:
                    return childOffset == parentOffset && childDir == parentDir;
                case PaintMode.Remove:
                    return childOffset == parentOffset;
            }
            throw new Exception();
        }


        public IEnumerable<SubFace> GetSubFaces(Vector3Int offset, Sylves.CellDir dir)
        {
            yield return SubFace.BottomLeft;
            yield return SubFace.Bottom;
            yield return SubFace.BottomRight;
            yield return SubFace.Left;
            yield return SubFace.Center;
            yield return SubFace.Right;
            yield return SubFace.TopLeft;
            yield return SubFace.Top;
            yield return SubFace.TopRight;
        }

        public void SetSubFaceValue(FaceDetails faceDetails, SubFace subface, int value)
        {
            switch (subface)
            {
                case SubFace.BottomLeft: faceDetails.bottomLeft = value; break;
                case SubFace.Bottom: faceDetails.bottom = value; break;
                case SubFace.BottomRight: faceDetails.bottomRight = value; break;
                case SubFace.Left: faceDetails.left = value; break;
                case SubFace.Center: faceDetails.center = value; break;
                case SubFace.Right: faceDetails.right = value; break;
                case SubFace.TopLeft: faceDetails.topLeft = value; break;
                case SubFace.Top: faceDetails.top = value; break;
                case SubFace.TopRight: faceDetails.topRight = value; break;
            }
        }

        public int GetSubFaceValue(FaceDetails faceDetails, SubFace subface)
        {
            switch (subface)
            {
                case SubFace.BottomLeft: return faceDetails.bottomLeft;
                case SubFace.Bottom: return faceDetails.bottom;
                case SubFace.BottomRight: return faceDetails.bottomRight;
                case SubFace.Left: return faceDetails.left;
                case SubFace.Center: return faceDetails.center;
                case SubFace.Right: return faceDetails.right;
                case SubFace.TopLeft: return faceDetails.topLeft;
                case SubFace.Top: return faceDetails.top;
                case SubFace.TopRight: return faceDetails.topRight;
            }
            return 0;
        }

        public Vector3Int Move(Vector3Int offset, Sylves.CellDir dir)
        {
            return offset + ((Sylves.CubeDir)dir).Forward();
        }

        private static float Max(Vector3 v)
        {
            return Mathf.Max(v.x, v.y, v.z);
        }

        private static float Min(Vector3 v)
        {
            return Mathf.Min(v.x, v.y, v.z);
        }

        private struct DrawingUtil
        {
            public Vector3 center;
            public Vector3 size;
            public Vector3 hsize;

            public DrawingUtil(Vector3 center, Vector3 tileSize)
            {
                this.center = center;
                this.size = tileSize;
                this.hsize = size * 0.5f;
            }


            public Vector3 ScaleByHSize(Vector3 v)
            {
                return new Vector3(v.x * hsize.x, v.y * hsize.y, v.z * hsize.z);
            }

            public Vector3 ScaleBySize(Vector3 v)
            {
                return new Vector3(v.x * size.x, v.y * size.y, v.z * size.z);
            }
        }
    }
}