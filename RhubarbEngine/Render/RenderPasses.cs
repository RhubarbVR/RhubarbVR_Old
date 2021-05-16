using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace RhubarbEngine.Render
{
    [Flags]
    public enum RenderPasses : int
    {
        Standard = 1 << 0,
        AlphaBlend = 1 << 1,
        Overlay = 1 << 2,
        ShadowMapNear = 1 << 3,
        ShadowMapMid = 1 << 4,
        ShadowMapFar = 1 << 5,
        Duplicator = 1 << 6,
        SwapchainOutput = 1 << 7,
        ReflectionMap = 1 << 8,
        AllShadowMap = ShadowMapNear | ShadowMapMid | ShadowMapFar,
    }
}
