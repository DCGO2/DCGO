using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoLayout3D
{
    [System.Serializable]
    public struct Padding
    {
        public Vector3 lower;
        public Vector3 upper;
    }

    public enum Direction
    {
        LowerToUpper, UpperToLower
    }

    public enum AxisOrder
    {
        XYZ, XZY, YXZ, YZX, ZXY, ZYX
    }

    public enum AxisAlignment
    {
        Upper, Middle, Lower
    }

    [System.Serializable]
    public struct Bool3
    {
        public bool x, y, z;
    }

    public struct Int3
    {
        public int x, y, z;
    }

    public enum ConstraintType
    {
        Flexible, FixedCellCount
    }

    [System.Serializable]
    public class Constraint
    {
        public ConstraintType constraintType = ConstraintType.Flexible;
        public int constraintCount = 1;
    }

    public abstract class LayoutGroup3D : LayoutElement3D
    {
        [Header(" ")] public Padding padding;

        protected List<LayoutElement3D> elementList = new List<LayoutElement3D>();

        public abstract void UpdateLayout();

#if UNITY_EDITOR
        //update layout every frame only in editor
        private void Update()
        {
            //if (!Application.isPlaying) //update layout every frame only in edit mode
            UpdateLayout();
        }
#endif
    }
}