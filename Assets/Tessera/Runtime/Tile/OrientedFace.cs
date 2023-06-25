using System;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// Legacy class used when serializing. You should use SylvesOrientedFace instead.
    /// Records the painted colors and location of single face of one cube in a <see cref="TesseraTile"/>
    /// </summary>
    [Serializable]
    public struct OrientedFace
    {
        public Vector3Int offset;
        public CellFaceDir faceDir;
        public FaceDetails faceDetails;


        public OrientedFace(Vector3Int offset, CellFaceDir faceDir, FaceDetails faceDetails)
        {
            this.offset = offset;
            this.faceDir = faceDir;
            this.faceDetails = faceDetails;
        }

        public void Deconstruct(out Vector3Int offset, out CellFaceDir faceDir, out FaceDetails faceDetails)
        {
            offset = this.offset;
            faceDir = this.faceDir;
            faceDetails = this.faceDetails;
        }
    }

    /// <summary>
    /// Records the painted colors and location of single face of one cube in a <see cref="TesseraTile"/>
    /// </summary>
    [Serializable]
    public struct SylvesOrientedFace
    {
        public Vector3Int offset;
        public Sylves.CellDir dir;
        public FaceDetails faceDetails;


        public SylvesOrientedFace(Vector3Int offset, Sylves.CellDir dir, FaceDetails faceDetails)
        {
            this.offset = offset;
            this.dir = dir;
            this.faceDetails = faceDetails;
        }

        public void Deconstruct(out Vector3Int offset, out Sylves.CellDir dir, out FaceDetails faceDetails)
        {
            offset = this.offset;
            dir = this.dir;
            faceDetails = this.faceDetails;
        }
    }
}