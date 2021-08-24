using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace RhubarbEngine.Render
{
    public class RenderQueue
    {
        private const int DefaultCapacity = 250;

        private readonly SynchronizedCollection<RenderItemIndex> _indices = new SynchronizedCollection<RenderItemIndex>(DefaultCapacity);
        private readonly SynchronizedCollection<Renderable> _renderables = new SynchronizedCollection<Renderable>(DefaultCapacity);

        public SynchronizedCollection<Renderable> Renderables { get { return _renderables; } }
        public int Count => _renderables.Count;

        public void Clear()
        {
            _indices.Clear();
            _renderables.Clear();
        }

        public void AddRange(IList<Renderable> Renderables, Vector3 viewPosition)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderable Renderable = Renderables[i];
                if (Renderable != null)
                {
                    Add(Renderable, viewPosition);
                }
            }
        }

        public void AddRange(IReadOnlyList<Renderable> Renderables, Vector3 viewPosition)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderable Renderable = Renderables[i];
                if (Renderable != null)
                {
                    Add(Renderable, viewPosition);
                }
            }
        }

        public void AddRange(IEnumerable<Renderable> Renderables, Vector3 viewPosition)
        {
            foreach (Renderable item in Renderables)
            {
                if (item != null)
                {
                    Add(item, viewPosition);
                }
            }
        }

        public void Add(Renderable item, Vector3 viewPosition)
        {
            int index = _renderables.Count;
            _indices.Add(new RenderItemIndex(item.GetRenderOrderKey(viewPosition), index));
            _renderables.Add(item);
        }
    }
}
