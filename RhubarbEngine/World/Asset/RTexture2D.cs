using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using g3;
using Veldrid;
using System.Runtime.CompilerServices;
using RhubarbEngine.Render;
using System.Numerics;
using Veldrid.Utilities;

namespace RhubarbEngine.World.Asset
{
    public class RTexture2D : IAsset
    {

        public List<IDisposable> disposables = new List<IDisposable>();

        public TextureView view;

        private Texture texture;

        public virtual void createResource(ResourceFactory fact)
        {
            if(texture == null)
            {
                return;
            }
            view = fact.CreateTextureView(texture);
        }

        public void Dispose() {
            foreach (IDisposable dep in disposables)
            {
                dep.Dispose();
            }
        }

        public void addDisposable(IDisposable val)
        {
            disposables.Add(val);
        }

        public RTexture2D(Texture e)
        {
            texture = e;
        }
    }
}
