Important notes: 
- For for editing convenience, Layout will update every frame inside editor.
  For runtime scripts, please call layout.UpdateLayout() to apply any changes.
  This behavior can be modified in LayoutGroup3D.cs.

LayoutElement3D: 
Basic component for auto layout 3D, it defines the layout element bounding box.
- Center: 
  Center of layout element bounding box (local transform).
- Size: 
  Size of layout element bounding box (local transform).

X/Y/Z AxisLayoutGroup3D:
The X/Y/Z AxisLayoutGroup component places its child layout elements next to each other, along x/y/z axis.
- Center: 
  Center of layout element bounding box (local transform).
- Size: 
  Size of layout element bounding box (local transform).
- Padding: 
  The padding inside the edges of the layout group.
- Spacing: 
  The spacing between layout elements along x axis.
- Child Alignment X(Y,Z): 
  The alignment to use for the child layout elements along x(y,z) axis, 
  align with the lower boundary, upper boundary, or middle of the bounding box.
- Child Direction:
  The arrangement direction to use for the child layout elements along x/y/z axis, 
  from lower to upper, or from upper to lower.
- Child Controls Size: 
  Whether the layout group controls the size of its children.
- Child Forced Expand:
  Whether to force the children to expand to fill additional available space.

GridLayoutGroup3D:
The Grid Layout Group component places its child layout elements next to each other, inside a 3D grid.
- Center:
  Center of layout element bounding box (local transform).
- Size:
  Size of layout element bounding box (local transform).
- Padding:
  The padding inside the edges of the layout group.
- Cell Size:
  The size to use for each layout element in the group.
- Spacing:
  The spacing between layout elements.
- Axis Order:
  The axis order to place child layout elements. 
  For example, the XZY order will fill up x axis first, 
  followed by XZ plane, and lastly place elements along y axis.
- Child Alignment X(Y,Z):
  The alignment to use for the child layout elements along x(y,z) axis, 
  align with the lower boundary, upper boundary, or middle of the bounding box.
- Child Direction X(Y,Z):
  The arrangement direction to use for the child layout elements along x(y,z) axis, 
  from lower to upper, or from upper to lower.
- Child Controls Size:
  Whether the layout group’s cell controls the size of its children.
- Child Forced Expand:
  Whether to force the children to expand to fill additional available space inside the layout group’s cell.
- Constraint X(Y,Z):
  Constraint the grid to a fixed number of cells along x(y,z) axis to aid the auto layout system.