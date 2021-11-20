using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpText.Core;

namespace RhubarbEngine.World.Asset
{
    public class RFont : IAsset
    {
        public Font font;

        public RFont(Font font)
        {
            this.font = font;
        }

        public void Dispose()
        {
        }
    }
}
