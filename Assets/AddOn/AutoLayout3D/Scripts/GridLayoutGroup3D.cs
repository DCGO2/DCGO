using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoLayout3D
{
    public class GridLayoutGroup3D : LayoutGroup3D
    {
        public Vector3 cellSize = Vector3.one;
        public Vector3 spacing = Vector3.zero;
        public AxisOrder axisOrder = AxisOrder.XZY; 
        public AxisAlignment childAlignmentX = AxisAlignment.Middle;
        public AxisAlignment childAlignmentY = AxisAlignment.Middle;
        public AxisAlignment childAlignmentZ = AxisAlignment.Middle;
        public Direction childDirectionX = Direction.LowerToUpper;
        public Direction childDirectionY = Direction.LowerToUpper;
        public Direction childDirectionZ = Direction.LowerToUpper;
        public Bool3 childControlSize = new Bool3();
        public Bool3 childForceExpand = new Bool3();
        public Constraint constraintX = new Constraint();
        public Constraint constraintY = new Constraint();
        public Constraint constraintZ = new Constraint();

        private void Check()
        {
            if (cellSize.x < 0.0f) cellSize.x = 0.0f;
            if (cellSize.y < 0.0f) cellSize.y = 0.0f;
            if (cellSize.z < 0.0f) cellSize.z = 0.0f;
            if (constraintX.constraintCount < 1) constraintX.constraintCount = 1;
            if (constraintY.constraintCount < 1) constraintY.constraintCount = 1;
            if (constraintZ.constraintCount < 1) constraintZ.constraintCount = 1;
        }

        public override void UpdateLayout()
        {
            Check();
            elementList.Clear();
            foreach (Transform child in transform)
            {
                LayoutElement3D element = child.GetComponent<LayoutElement3D>();
                if (!child.gameObject.activeSelf || element == null) continue;
                elementList.Add(element);
            }

            Vector3 c_size = new Vector3();
            c_size.x = size.x - padding.lower.x - padding.upper.x;
            c_size.y = size.y - padding.lower.y - padding.upper.y;
            c_size.z = size.z - padding.lower.z - padding.upper.z;

            Vector3 c_center = center;
            c_center.x += (padding.lower.x - padding.upper.x) * 0.5f;
            c_center.y += (padding.lower.y - padding.upper.y) * 0.5f;
            c_center.z += (padding.lower.z - padding.upper.z) * 0.5f;

            Int3 c_count_max = new Int3();
            c_count_max.x = constraintX.constraintType == ConstraintType.Flexible ? 
                Mathf.FloorToInt((c_size.x + spacing.x) / (cellSize.x + spacing.x)) : constraintX.constraintCount;
            c_count_max.y = constraintY.constraintType == ConstraintType.Flexible ?
                Mathf.FloorToInt((c_size.y + spacing.y) / (cellSize.y + spacing.y)) : constraintY.constraintCount;
            c_count_max.z = constraintZ.constraintType == ConstraintType.Flexible ?
                Mathf.FloorToInt((c_size.z + spacing.z) / (cellSize.z + spacing.z)) : constraintZ.constraintCount;

            Int3 c_count = new Int3();
            int countUnit, countRow, countPlane;
            countUnit = elementList.Count;
            switch (axisOrder)
            {
                case AxisOrder.XYZ:
                    countRow = Mathf.CeilToInt((float)elementList.Count / c_count_max.x);
                    countPlane = Mathf.CeilToInt((float)elementList.Count / c_count_max.x / c_count_max.y);
                    c_count.x = countUnit < c_count_max.x ? countUnit : c_count_max.x;
                    c_count.y = countRow < c_count_max.y ? countRow : c_count_max.y;
                    c_count.z = countPlane < c_count_max.z ? countPlane : c_count_max.z;
                    constraintZ.constraintType = ConstraintType.Flexible;
                    break;
                case AxisOrder.XZY:
                    countRow = Mathf.CeilToInt((float)elementList.Count / c_count_max.x);
                    countPlane = Mathf.CeilToInt((float)elementList.Count / c_count_max.x / c_count_max.z);
                    c_count.x = countUnit < c_count_max.x ? countUnit : c_count_max.x;
                    c_count.z = countRow < c_count_max.z ? countRow : c_count_max.z;
                    c_count.y = countPlane < c_count_max.y ? countPlane : c_count_max.y;
                    constraintY.constraintType = ConstraintType.Flexible;
                    break;
                case AxisOrder.YXZ:
                    countRow = Mathf.CeilToInt((float)elementList.Count / c_count_max.y);
                    countPlane = Mathf.CeilToInt((float)elementList.Count / c_count_max.y / c_count_max.x);
                    c_count.y = countUnit < c_count_max.y ? countUnit : c_count_max.y;
                    c_count.x = countRow < c_count_max.x ? countRow : c_count_max.x;
                    c_count.z = countPlane < c_count_max.z ? countPlane : c_count_max.z;
                    constraintZ.constraintType = ConstraintType.Flexible;
                    break;
                case AxisOrder.YZX:
                    countRow = Mathf.CeilToInt((float)elementList.Count / c_count_max.y);
                    countPlane = Mathf.CeilToInt((float)elementList.Count / c_count_max.y / c_count_max.z);
                    c_count.y = countUnit < c_count_max.y ? countUnit : c_count_max.y;
                    c_count.z = countRow < c_count_max.z ? countRow : c_count_max.z;
                    c_count.x = countPlane < c_count_max.x ? countPlane : c_count_max.x;
                    constraintX.constraintType = ConstraintType.Flexible;
                    break;
                case AxisOrder.ZXY:
                    countRow = Mathf.CeilToInt((float)elementList.Count / c_count_max.z);
                    countPlane = Mathf.CeilToInt((float)elementList.Count / c_count_max.z / c_count_max.x);
                    c_count.z = countUnit < c_count_max.z ? countUnit : c_count_max.z;
                    c_count.x = countRow < c_count_max.x ? countRow : c_count_max.x;
                    c_count.y = countPlane < c_count_max.y ? countPlane : c_count_max.y;
                    constraintY.constraintType = ConstraintType.Flexible;
                    break;
                case AxisOrder.ZYX:
                    countRow = Mathf.CeilToInt((float)elementList.Count / c_count_max.z);
                    countPlane = Mathf.CeilToInt((float)elementList.Count / c_count_max.z / c_count_max.y);
                    c_count.z = countUnit < c_count_max.z ? countUnit : c_count_max.z;
                    c_count.y = countRow < c_count_max.y ? countRow : c_count_max.y;
                    c_count.x = countPlane < c_count_max.x ? countPlane : c_count_max.x;
                    constraintX.constraintType = ConstraintType.Flexible;
                    break;
            }

            Vector3 sum_withSpace = new Vector3();
            sum_withSpace.x = (cellSize.x + spacing.x) * c_count.x - spacing.x;
            sum_withSpace.y = (cellSize.y + spacing.y) * c_count.y - spacing.y;
            sum_withSpace.z = (cellSize.z + spacing.z) * c_count.z - spacing.z;

            Vector3 offset = Vector3.zero;
            Vector3 direction = Vector3.zero;
            direction.x = childDirectionX == Direction.LowerToUpper ? 1.0f : -1.0f;
            direction.y = childDirectionY == Direction.LowerToUpper ? 1.0f : -1.0f;
            direction.z = childDirectionZ == Direction.LowerToUpper ? 1.0f : -1.0f;
            
            switch (childAlignmentX)
            {
                case AxisAlignment.Lower:
                    offset.x = -c_size.x * 0.5f;
                    if (childDirectionX == Direction.UpperToLower) offset.x += sum_withSpace.x;
                    break;
                case AxisAlignment.Middle:
                    offset.x = -direction.x * sum_withSpace.x * 0.5f;
                    break;
                case AxisAlignment.Upper:
                    offset.x = c_size.x * 0.5f;
                    if (childDirectionX == Direction.LowerToUpper) offset.x -= sum_withSpace.x;
                    break;
            }

            switch (childAlignmentY)
            {
                case AxisAlignment.Lower:
                    offset.y = -c_size.y * 0.5f;
                    if (childDirectionY == Direction.UpperToLower) offset.y += sum_withSpace.y;
                    break;
                case AxisAlignment.Middle:
                    offset.y = -direction.y * sum_withSpace.y * 0.5f;
                    break;
                case AxisAlignment.Upper:
                    offset.y = c_size.y * 0.5f;
                    if (childDirectionY == Direction.LowerToUpper) offset.y -= sum_withSpace.y;
                    break;
            }

            switch (childAlignmentZ)
            {
                case AxisAlignment.Lower:
                    offset.z = -c_size.z * 0.5f;
                    if (childDirectionZ == Direction.UpperToLower) offset.z += sum_withSpace.z;
                    break;
                case AxisAlignment.Middle:
                    offset.z = -direction.z * sum_withSpace.z * 0.5f;
                    break;
                case AxisAlignment.Upper:
                    offset.z = c_size.z * 0.5f;
                    if (childDirectionZ == Direction.LowerToUpper) offset.z -= sum_withSpace.z;
                    break;
            }

            Vector3 scaleFactor = Vector3.zero;
            if (childForceExpand.x) scaleFactor.x = cellSize.x;
            if (childForceExpand.y) scaleFactor.y = cellSize.y;
            if (childForceExpand.z) scaleFactor.z = cellSize.z;

            Vector3 offset_curr = offset;
            Int3 counter = new Int3();
            foreach (LayoutElement3D element in elementList)
            {
                Vector3 scale = element.transform.localScale;
                if (childControlSize.x) scale.x = scaleFactor.x / element.size.x;
                if (childControlSize.y) scale.y = scaleFactor.y / element.size.y;
                if (childControlSize.z) scale.z = scaleFactor.z / element.size.z;
                element.transform.localScale = scale;

                Vector3 elementOffset = Vector3.one;
                elementOffset.x = cellSize.x * 0.5f * direction.x - element.center.x * scale.x;
                elementOffset.y = cellSize.y * 0.5f * direction.y - element.center.y * scale.y;
                elementOffset.z = cellSize.z * 0.5f * direction.z - element.center.z * scale.z;

                element.transform.localPosition = c_center + offset_curr + elementOffset;

                switch (axisOrder)
                {
                    case AxisOrder.XYZ:
                        counter.x++;
                        if (counter.x >= c_count.x)
                        {
                            counter.x = 0;
                            offset_curr.x = offset.x;

                            counter.y++;
                            if (counter.y >= c_count.y)
                            {
                                counter.y = 0;
                                offset_curr.y = offset.y;

                                counter.z++;
                                offset_curr.z += (cellSize.z + spacing.z) * direction.z;
                            }
                            else
                            {
                                offset_curr.y += (cellSize.y + spacing.y) * direction.y;
                            }
                        }
                        else
                        {
                            offset_curr.x += (cellSize.x + spacing.x) * direction.x;
                        }
                        break;
                    case AxisOrder.XZY:
                        counter.x++;
                        if (counter.x >= c_count.x)
                        {
                            counter.x = 0;
                            offset_curr.x = offset.x;

                            counter.z++;
                            if (counter.z >= c_count.z)
                            {
                                counter.z = 0;
                                offset_curr.z = offset.z;

                                counter.y++;
                                offset_curr.y += (cellSize.y + spacing.y) * direction.y;
                            }
                            else
                            {
                                offset_curr.z += (cellSize.z + spacing.z) * direction.z;
                            }
                        }
                        else
                        {
                            offset_curr.x += (cellSize.x + spacing.x) * direction.x;
                        }
                        break;
                    case AxisOrder.YXZ:
                        counter.y++;
                        if (counter.y >= c_count.y)
                        {
                            counter.y = 0;
                            offset_curr.y = offset.y;

                            counter.x++;
                            if (counter.x >= c_count.x)
                            {
                                counter.x = 0;
                                offset_curr.x = offset.x;

                                counter.z++;
                                offset_curr.z += (cellSize.z + spacing.z) * direction.z;
                            }
                            else
                            {
                                offset_curr.x += (cellSize.x + spacing.x) * direction.x;
                            }
                        }
                        else
                        {
                            offset_curr.y += (cellSize.y + spacing.y) * direction.y;
                        }
                        break;
                    case AxisOrder.YZX:
                        counter.y++;
                        if (counter.y >= c_count.y)
                        {
                            counter.y = 0;
                            offset_curr.y = offset.y;

                            counter.z++;
                            if (counter.z >= c_count.z)
                            {
                                counter.z = 0;
                                offset_curr.z = offset.z;

                                counter.x++;
                                offset_curr.x += (cellSize.x + spacing.x) * direction.x;
                            }
                            else
                            {
                                offset_curr.z += (cellSize.z + spacing.z) * direction.z;
                            }
                        }
                        else
                        {
                            offset_curr.y += (cellSize.y + spacing.y) * direction.y;
                        }
                        break;
                    case AxisOrder.ZXY:
                        counter.z++;
                        if (counter.z >= c_count.z)
                        {
                            counter.z = 0;
                            offset_curr.z = offset.z;

                            counter.x++;
                            if (counter.x >= c_count.x)
                            {
                                counter.x = 0;
                                offset_curr.x = offset.x;

                                counter.y++;
                                offset_curr.y += (cellSize.y + spacing.y) * direction.y;
                            }
                            else
                            {
                                offset_curr.x += (cellSize.x + spacing.x) * direction.x;
                            }
                        }
                        else
                        {
                            offset_curr.z += (cellSize.z + spacing.z) * direction.z;
                        }
                        break;
                    case AxisOrder.ZYX:
                        counter.z++;
                        if (counter.z >= c_count.z)
                        {
                            counter.z = 0;
                            offset_curr.z = offset.z;

                            counter.y++;
                            if (counter.y >= c_count.y)
                            {
                                counter.y = 0;
                                offset_curr.y = offset.y;

                                counter.x++;
                                offset_curr.x += (cellSize.x + spacing.x) * direction.x;
                            }
                            else
                            {
                                offset_curr.y += (cellSize.y + spacing.y) * direction.y;
                            }
                        }
                        else
                        {
                            offset_curr.z += (cellSize.z + spacing.z) * direction.z;
                        }
                        break;
                }
            }
            elementList.Clear();
        }
    }
}