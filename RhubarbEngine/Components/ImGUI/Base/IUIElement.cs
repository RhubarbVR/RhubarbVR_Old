using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World;

namespace RhubarbEngine.Components.ImGUI
{
    public interface IUIElement: IWorldObject
    {
        void ImguiRender();
    }
}
