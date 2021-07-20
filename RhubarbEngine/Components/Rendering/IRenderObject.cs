using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Components.Rendering
{
    public enum RenderFrequency
    {
        OneToOne,
        Half,
        Eighth,
        Sixth
    }

    public interface IRenderObject
    {
        RenderFrequency renderFrac { get; }
        void Render();
    }
}
