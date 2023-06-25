using System.Collections.Generic;
using UnityEngine;

namespace Tessera
{
    // Extension to ICellType containing various methods used for drawing / interacting with the cells in the UI
    public interface ICellDrawingType
    {
        Vector3[] GetSubFaceVertices(Vector3Int offset, Sylves.CellDir dir, SubFace subface, Vector3 center, Vector3 cellSize);

        Vector3[] GetFaceVertices(Vector3Int offset, Sylves.CellDir dir, Vector3 center, Vector3 cellSize);

        void GetFaceCenterAndNormal(Vector3Int offset, Sylves.CellDir dir, Vector3 center, Vector3 cellSize, out Vector3 faceCenter, out Vector3 faceNormal);

        RaycastCellHit Raycast(Ray ray, Vector3Int offset, Vector3 center, Vector3 cellSize, float? minDistance, float? maxDistance);

        SubFace RoundSubFace(Vector3Int offset, Sylves.CellDir dir, Vector2 subface, PaintMode paintMode);

        bool IsAffected(Vector3Int parentOffset, Sylves.CellDir parentFaceDir, SubFace parentSubface, Vector3Int childOffset, Sylves.CellDir childFaceDir, SubFace childSubface, PaintMode paintMode);

        IEnumerable<SubFace> GetSubFaces(Vector3Int offset, Sylves.CellDir dir);

        void SetSubFaceValue(FaceDetails faceDetails, SubFace subface, int color);

        int GetSubFaceValue(FaceDetails faceDetails, SubFace subface);

        Vector3Int Move(Vector3Int offset, Sylves.CellDir dir);

        bool Is2D { get; }
    }

    public enum SubFace
    {
        // Square subfaces
        BottomLeft,
        Bottom,
        BottomRight,
        Left,
        Center,
        Right,
        TopLeft,
        Top,
        TopRight,

        // Clockwise from the the subface at the center of the edge facing right.
        // XAndY means adjacent to the vertex joining those two sides
        HexRight,
        HexRightAndTopRight,
        HexTopRight,
        HexTopRightAndTopLeft,
        HexTopLeft,
        HexTopLeftAndLeft,
        HexLeft,
        HexLeftAndBottomLeft,
        HexBottomLeft,
        HexBottomLeftAndBottomRight,
        HexBottomRight,
        HexBottomRightAndRight,
        HexCenter,

        TriangleBottom,
        TriangleBottomRight,
        TriangleTopRight,
        TriangleTop,
        TriangleTopLeft,
        TriangleBottomLeft,
        TriangleCenter
    }

    public class RaycastCellHit
    {
        public Sylves.CellDir dir;
        public Vector3 point;
        // Varies from -1 to 1
        public Vector2 subface;
    }
}