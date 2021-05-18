using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace RhubarbEngine.Render
{
    public class RenderQueue
    {
        private const int DefaultCapacity = 250;

        private readonly List<RenderItemIndex> _indices = new List<RenderItemIndex>(DefaultCapacity);
        private readonly List<Renderable> _renderables = new List<Renderable>(DefaultCapacity);

        public List<Renderable> Renderables { get { return _renderables; } }
        public int Count => _renderables.Count;

        public void Clear()
        {
            _indices.Clear();
            _renderables.Clear();
        }

        public void AddRange(List<Renderable> Renderables, Vector3 viewPosition)
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

        public void Sort()
        {
            _indices.Sort();
        }

        public void Sort(Comparer<RenderOrderKey> keyComparer)
        {
            _indices.Sort(
                (RenderItemIndex first, RenderItemIndex second)
                    => keyComparer.Compare(first.Key, second.Key));
        }

        public void Sort(Comparer<RenderItemIndex> comparer)
        {
            _indices.Sort(comparer);
        }

    }
}
