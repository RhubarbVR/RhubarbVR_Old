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
        private List<Renderable> sorted = new List<Renderable>(DefaultCapacity);

        public List<Renderable> Renderables { get { return sorted; } }
        public int Count => _renderables.Count;

        public void Clear()
        {
            _indices.Clear();
            _renderables.Clear();
            sorted.Clear();
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
            _renderables.Add(item);
            _indices.Add(new RenderItemIndex(item.GetRenderOrderKey(viewPosition), _renderables.IndexOf(item)));
        }

        public void Order()
        {
            //Want to make multi Threaded
            foreach (var item in from parer in _indices orderby parer.Key.Value descending select parer)
            {
                sorted.Add(_renderables[item.ItemIndex]);
            }
        }
    }
}
