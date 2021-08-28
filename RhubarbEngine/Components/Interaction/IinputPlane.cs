using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World;
using Veldrid; 
namespace RhubarbEngine.Components.Interaction
{
    public enum InteractionSource
    {
        None,
        LeftLaser,
        LeftFinger,
        RightLaser,
        RightFinger,
        HeadLaser,
        HeadFinger,
    }
    public interface IinputPlane : IWorldObject, InputSnapshot
    {
        public InteractionSource source { get; }

        public bool focused { get; }
        public bool StopMouse { get; set; }

        public void Setfocused();

        public void Removefocused();

        public void SetCursor(Input.Cursors cursor);

    }
}
