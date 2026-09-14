// /**
//  * Created by Pawel Homenko on  08/2022
//  */

using System;
using UnityEngine;

namespace NatureManufacture.RAM
{
    [Serializable]
    public struct NmSplinePoint
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private float width;
        [SerializeField] private float snap;
        [SerializeField] private float lerpValue;
        [SerializeField] private Quaternion orientation;
        [SerializeField] private Quaternion rotation;
        [SerializeField] private Vector3 normal;
        [SerializeField] private Vector3 tangent;
        [SerializeField] private Vector3 binormal;
        [SerializeField] private float distance;
        [SerializeField] private int density;
        [SerializeField] private int id;

        public NmSplinePoint(int id =0)
        {
            width = 1;
            snap = 0;
            lerpValue = 0;
            distance = 0;
            density = 1;
            position = default;
            orientation = default;
            rotation = default;
            normal = default;
            tangent = default;
            binormal = default;
            this.id = id;
        }

        public NmSplinePoint(Vector3 position)
        {
            this.position = position;
            width = 0;
            snap = 0;
            lerpValue = 0;
            orientation = default;
            rotation = default;
            normal = default;
            tangent = default;
            binormal = default;
            distance = 0;
            density = 0;
            id = 0;
        }

        public NmSplinePoint(Vector3 position, Quaternion orientation, Quaternion rotation, Vector3 normal, Vector3 tangent, Vector3 binormal, float width, float snap, float lerpValue, float distance, int density)
        {
            this.position = position;
            this.orientation = orientation;
            this.rotation = rotation;
            this.normal = normal;
            this.tangent = tangent;
            this.binormal = binormal;
            this.width = width;
            this.snap = snap;
            this.lerpValue = lerpValue;
            this.distance = distance;
            this.density = density;
            id = 0;
        }

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public float Width
        {
            get => width;
            set => width = value;
        }

        public float Snap
        {
            get => snap;
            set => snap = value;
        }

        public float LerpValue
        {
            get => lerpValue;
            set => lerpValue = value;
        }

        public Quaternion Orientation
        {
            get => orientation;
            set => orientation = value;
        }

        public Vector3 Tangent
        {
            get => tangent;
            set => tangent = value;
        }

        public Vector3 Binormal
        {
            get => binormal;
            set => binormal = value;
        }

        public Vector3 Normal
        {
            get => normal;
            set => normal = value;
        }

        public float Distance
        {
            get => distance;
            set => distance = value;
        }

        public int Density
        {
            get => density;
            set => density = value;
        }

        public Quaternion Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        public int ID
        {
            get => id;
            set => id = value;
        }
    }
}