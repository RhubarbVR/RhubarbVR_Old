using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbEngine.Components.Assets;

namespace RhubarbEngine.World
{
	public class StaticAssets
	{
		private readonly World _world;

		private TilledUnlitShader _tilledUnlitShader;
		public TilledUnlitShader TilledUnlitShader
		{
			get
			{
				if (_tilledUnlitShader == null)
				{
					var comp = _world.RootEntity.GetFirstComponent<TilledUnlitShader>();
					if (comp == null)
					{
						comp = _world.RootEntity.AttachComponent<TilledUnlitShader>();
					}
					_tilledUnlitShader = comp;
				}
				return _tilledUnlitShader;
			}
		}

		private BasicUnlitShader _basicUnlitShader;
		public BasicUnlitShader BasicUnlitShader
		{
			get
			{
				if (_basicUnlitShader == null)
				{
					var comp = _world.RootEntity.GetFirstComponent<BasicUnlitShader>();
					if (comp == null)
					{
						comp = _world.RootEntity.AttachComponent<BasicUnlitShader>();
					}
					_basicUnlitShader = comp;
				}
				return _basicUnlitShader;
			}
		}


		private OverLayedUnlitShader _overLayedUnlitShader;
		public OverLayedUnlitShader OverLayedUnlitShader
		{
			get
			{
				if (_overLayedUnlitShader == null)
				{
					var comp = _world.RootEntity.GetFirstComponent<OverLayedUnlitShader>();
					if (comp == null)
					{
						comp = _world.RootEntity.AttachComponent<OverLayedUnlitShader>();
					}
					_overLayedUnlitShader = comp;
				}
				return _overLayedUnlitShader;
			}
		}

		public StaticAssets(World world)
		{
			_world = world;
		}
	}
}
