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
		private IEnumerable<Renderable> sorted;

		public IEnumerable<Renderable> Renderables { get { return sorted; } }
		public int Count => _renderables.Count;

		public void Clear()
		{
			_indices.Clear();
			_renderables.Clear();
		}

		public void AddRange(IList<Renderable> Renderables, Vector3 viewPosition, ref RhubarbEngine.Utilities.BoundingFrustum frustum, Matrix4x4 view)
		{
			for (int i = 0; i < Renderables.Count; i++)
			{
				Renderable Renderable = Renderables[i];
				if (Renderable != null)
				{
					Add(Renderable, viewPosition, ref frustum, view);
				}
			}
		}

		public void AddRange(IReadOnlyList<Renderable> Renderables, Vector3 viewPosition, ref RhubarbEngine.Utilities.BoundingFrustum frustum, Matrix4x4 view)
		{
			for (int i = 0; i < Renderables.Count; i++)
			{
				Renderable Renderable = Renderables[i];
				if (Renderable != null)
				{
					Add(Renderable, viewPosition, ref frustum, view);
				}
			}
		}

		public void AddRange(IEnumerable<Renderable> Renderables, Vector3 viewPosition, ref RhubarbEngine.Utilities.BoundingFrustum frustum, Matrix4x4 view)
		{
			foreach (Renderable item in Renderables)
			{
				if (item != null)
				{
					Add(item, viewPosition, ref frustum, view);
				}
			}
		}

		public void Add(Renderable item, Vector3 viewPosition, ref RhubarbEngine.Utilities.BoundingFrustum frustum, Matrix4x4 view)
		{
			if (item.Cull(ref frustum, view))
			{
				return;
			}
			_renderables.Add(item);
			_indices.Add(new RenderItemIndex(item.GetRenderOrderKey(viewPosition), _renderables.IndexOf(item)));
		}

		public void Order()
		{
			sorted = from parer in _indices.AsParallel() orderby parer.Key.Value descending select _renderables[parer.ItemIndex];
		}
	}
}
