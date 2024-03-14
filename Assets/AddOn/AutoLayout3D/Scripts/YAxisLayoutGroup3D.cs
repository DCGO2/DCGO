using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoLayout3D
{
    public class YAxisLayoutGroup3D : LayoutGroup3D
    {
        public float spacing;
        public AxisAlignment childAlignmentX = AxisAlignment.Middle;
        public AxisAlignment childAlignmentY = AxisAlignment.Middle;
        public AxisAlignment childAlignmentZ = AxisAlignment.Middle;
        public Direction childDirection = Direction.LowerToUpper;
        public Bool3 childControlSize = new Bool3();
        public Bool3 childForceExpand = new Bool3();

        public override void UpdateLayout()
        {
            elementList.Clear();
            float sum = 0.0f;
            foreach (Transform child in transform)
            {
                LayoutElement3D element = child.GetComponent<LayoutElement3D>();
                if (!child.gameObject.activeSelf || element == null) continue;
                elementList.Add(element);
                sum += element.size.y * element.transform.localScale.y;
            }
            float sum_withSpace = sum + spacing * (elementList.Count - 1);

            Vector3 c_size = new Vector3();
            c_size.x = size.x - padding.lower.x - padding.upper.x;
            c_size.y = size.y - padding.lower.y - padding.upper.y;
            c_size.z = size.z - padding.lower.z - padding.upper.z;

            Vector3 c_center = center;
            c_center.x += (padding.lower.x - padding.upper.x) * 0.5f;
            c_center.y += (padding.lower.y - padding.upper.y) * 0.5f;
            c_center.z += (padding.lower.z - padding.upper.z) * 0.5f;

            Vector3 direction = Vector3.zero;
            Vector3 offset = Vector3.zero;

            direction.y = childDirection == Direction.LowerToUpper ? 1.0f : -1.0f;
            switch (childAlignmentY)
            {
                case AxisAlignment.Lower:
                    offset.y = -c_size.y * 0.5f;
                    if (childDirection == Direction.UpperToLower) offset.y += sum_withSpace;
                    break;
                case AxisAlignment.Middle:
                    offset.y = -direction.y * sum_withSpace * 0.5f;
                    break;
                case AxisAlignment.Upper:
                    offset.y = c_size.y * 0.5f;
                    if (childDirection == Direction.LowerToUpper) offset.y -= sum_withSpace;
                    break;
            }

            switch (childAlignmentX)
            {
                case AxisAlignment.Lower:
                    direction.x = 1.0f;
                    break;
                case AxisAlignment.Middle:
                    direction.x = 0.0f;
                    break;
                case AxisAlignment.Upper:
                    direction.x = -1.0f;
                    break;
            }
            offset.x = -direction.x * c_size.x * 0.5f;

            switch (childAlignmentZ)
            {
                case AxisAlignment.Lower:
                    direction.z = 1.0f;
                    break;
                case AxisAlignment.Middle:
                    direction.z = 0.0f;
                    break;
                case AxisAlignment.Upper:
                    direction.z = -1.0f;
                    break;
            }
            offset.z = -direction.z * c_size.z * 0.5f;

            float c_spacing = spacing;
            Vector3 scaleFactor = Vector3.zero;
            if (childForceExpand.y)
            {
                offset.y = -direction.y * c_size.y * 0.5f;
                scaleFactor.y = (c_size.y - (c_spacing * (elementList.Count - 1))) / elementList.Count;
                if (!childControlSize.x && sum_withSpace < c_size.y) c_spacing = (c_size.y - sum) / (elementList.Count - 1);
            }
            if (childForceExpand.x) scaleFactor.x = c_size.x;
            if (childForceExpand.z) scaleFactor.z = c_size.z;

            foreach (LayoutElement3D element in elementList)
            {
                Vector3 scale = element.transform.localScale;
                if (childControlSize.x) scale.x = scaleFactor.x / element.size.x;
                if (childControlSize.y) scale.y = scaleFactor.y / element.size.y;
                if (childControlSize.z) scale.z = scaleFactor.z / element.size.z;
                element.transform.localScale = scale;

                Vector3 elementSize = Vector3.one;
                elementSize.x = element.size.x * scale.x;
                elementSize.y = element.size.y * scale.y;
                elementSize.z = element.size.z * scale.z;

                Vector3 elementOffset = Vector3.one;
                elementOffset.x = elementSize.x * 0.5f * direction.x - element.center.x * scale.x;
                elementOffset.y = elementSize.y * 0.5f * direction.y - element.center.y * scale.y;
                elementOffset.z = elementSize.z * 0.5f * direction.z - element.center.z * scale.z;

                element.transform.localPosition = c_center + offset + elementOffset;
                offset.y += (elementSize.y + c_spacing) * direction.y;
            }
            elementList.Clear();
        }
    }
}