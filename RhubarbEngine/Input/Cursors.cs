using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.Enums;
using ImGuiNET;

namespace RhubarbEngine.Input
{
    public enum Cursors
    {
        Pointer = 0,
        Cross = 1,
        Hand = 2,
        IBeam = 3,
        Wait = 4,
        Help = 5,
        EastResize = 6,
        NorthResize = 7,
        NortheastResize = 8,
        NorthwestResize = 9,
        SouthResize = 10,
        SoutheastResize = 11,
        SouthwestResize = 12,
        WestResize = 13,
        NorthSouthResize = 14,
        EastWestResize = 15,
        NortheastSouthwestResize = 16,
        NorthwestSoutheastResize = 17,
        ColumnResize = 18,
        RowResize = 19,
        MiddlePanning = 20,
        EastPanning = 21,
        NorthPanning = 22,
        NortheastPanning = 23,
        NorthwestPanning = 24,
        SouthPanning = 25,
        SoutheastPanning = 26,
        SouthwestPanning = 27,
        WestPanning = 28,
        Move = 29,
        VerticalText = 30,
        Cell = 31,
        ContextMenu = 32,
        Alias = 33,
        Progress = 34,
        NoDrop = 35,
        Copy = 36,
        None = 37,
        NotAllowed = 38,
        ZoomIn = 39,
        ZoomOut = 40,
        Grab = 41,
        Grabbing = 42,
        MiddlePanningVertical = 43,
        MiddlePanningHorizontal = 44,
        Custom = 45,
        DndNone = 46,
        DndMove = 47,
        DndCopy = 48,
        DndLink = 49
    }

    public static class CursorsEnumCaster
    {
        public static Cursors CursorType(CursorType b)
        {
            return (Cursors)(int)b;
        }

        public static Cursors ImGuiMouse(ImGuiMouseCursor b)
        {
            switch (b)
            {
                case ImGuiMouseCursor.None:
                    return Cursors.None;
                case ImGuiMouseCursor.Arrow:
                    return Cursors.Pointer;
                case ImGuiMouseCursor.TextInput:
                    return Cursors.IBeam;
                case ImGuiMouseCursor.ResizeAll:
                    return Cursors.NorthwestSoutheastResize;
                case ImGuiMouseCursor.ResizeNS:
                    return Cursors.NorthSouthResize;
                case ImGuiMouseCursor.ResizeEW:
                    return Cursors.EastWestResize;
                case ImGuiMouseCursor.ResizeNESW:
                    return Cursors.NortheastSouthwestResize;
                case ImGuiMouseCursor.ResizeNWSE:
                    return Cursors.NorthwestSoutheastResize;
                case ImGuiMouseCursor.Hand:
                    return Cursors.Hand;
                case ImGuiMouseCursor.NotAllowed:
                    return Cursors.NotAllowed;
                case ImGuiMouseCursor.COUNT:
                    return Cursors.Pointer;
                default:
                    return Cursors.Pointer;
            }
        }
    }
}
