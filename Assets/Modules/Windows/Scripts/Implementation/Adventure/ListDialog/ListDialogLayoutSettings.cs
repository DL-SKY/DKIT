using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog
{
    public enum ListDialogLayoutMode
    {
        Vertical = 0,
        Grid = 1,
    }

    /// <summary>
    /// Scroll-content layout for <see cref="ListDialogView"/>. Provided by the dialog ViewModel.
    /// </summary>
    public sealed class ListDialogLayoutSettings
    {
        public ListDialogLayoutMode Mode = ListDialogLayoutMode.Vertical;
        public int ColumnCount = 1;
        public float CellWidth = 100f;
        public float CellHeight = 48f;
        public float SpacingX = 6f;
        public float SpacingY = 6f;
        public int PaddingLeft = 8;
        public int PaddingRight = 8;
        public int PaddingTop = 8;
        public int PaddingBottom = 8;

        public RectOffset CreatePadding()
        {
            return new RectOffset(PaddingLeft, PaddingRight, PaddingTop, PaddingBottom);
        }

        public static ListDialogLayoutSettings VerticalList(float rowHeight = 48f, float spacing = 6f)
        {
            return new ListDialogLayoutSettings
            {
                Mode = ListDialogLayoutMode.Vertical,
                ColumnCount = 1,
                CellHeight = rowHeight,
                SpacingY = spacing,
                SpacingX = 0f,
                PaddingLeft = 8,
                PaddingRight = 8,
                PaddingTop = 8,
                PaddingBottom = 8,
            };
        }

        public static ListDialogLayoutSettings AvatarGrid(
            int columns = 3,
            float cellWidth = 96f,
            float cellHeight = 112f,
            float spacing = 8f)
        {
            return new ListDialogLayoutSettings
            {
                Mode = ListDialogLayoutMode.Grid,
                ColumnCount = columns,
                CellWidth = cellWidth,
                CellHeight = cellHeight,
                SpacingX = spacing,
                SpacingY = spacing,
                PaddingLeft = 8,
                PaddingRight = 8,
                PaddingTop = 8,
                PaddingBottom = 8,
            };
        }
    }
}
