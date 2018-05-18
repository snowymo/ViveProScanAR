using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR
{
    public enum ColliderShapeType
    {
        UNDEFINED = 0,
        CONVEX_SHAPE = 1,
        BOUND_RECT_SHAPE = 2,
        MESH_SHAPE = 4,
    }    

    public enum PlaneOrientation
    {
        UNDEFINED = 0,
        HORIZONTAL = 8,
        VERTICAL = 16,
        OBLIQUE = 32,
        FRAGMENT = 64,
    }

    [ExecuteInEditMode]
    public class ViveSR_StaticColliderInfo : MonoBehaviour
    {
        [SerializeField] private ColliderShapeType shapeType;
        [SerializeField] private PlaneOrientation orientation;
        public float ApproxArea = 0.0f;
        public Vector3 GroupNormal = Vector3.zero;
        private uint PropBits;

        void Start()
        {
            PropBits = (uint)shapeType | (uint)orientation;
        }

        public void SetBit(uint bit)
        {
            if (bit == (uint)ColliderShapeType.CONVEX_SHAPE || bit == (uint)ColliderShapeType.BOUND_RECT_SHAPE || bit == (uint)ColliderShapeType.MESH_SHAPE)
                shapeType = (ColliderShapeType)bit;
            else if (bit == (uint)PlaneOrientation.HORIZONTAL || bit == (uint)PlaneOrientation.VERTICAL || bit == (uint)PlaneOrientation.OBLIQUE || bit == (uint)PlaneOrientation.FRAGMENT)
                orientation = (PlaneOrientation)bit;

            PropBits = (uint)shapeType | (uint)orientation;
        }

        public bool CheckHasAllBit(uint bit)
        {
            return ((PropBits & bit) == bit);
        }
    }
}

