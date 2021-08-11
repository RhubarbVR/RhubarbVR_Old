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
        private World _world;

        private TilledUnlitShader _tilledUnlitShader;
        public TilledUnlitShader tilledUnlitShader
        {
            get
            {
                if (_tilledUnlitShader == null)
                {
                    var comp = _world.RootEntity.getFirstComponent<TilledUnlitShader>();
                    if (comp == null)
                    {
                        comp = _world.RootEntity.attachComponent<TilledUnlitShader>();
                    }
                    _tilledUnlitShader = comp;
                }
                return _tilledUnlitShader;
            }
        }

        private BasicUnlitShader _basicUnlitShader;
        public BasicUnlitShader basicUnlitShader {
            get {
                if(_basicUnlitShader == null) 
                {
                   var comp = _world.RootEntity.getFirstComponent<BasicUnlitShader>();
                    if (comp == null)
                    {
                        comp = _world.RootEntity.attachComponent<BasicUnlitShader>();
                    }
                    _basicUnlitShader = comp;
                }
                return _basicUnlitShader;
            }
        }
        
        public StaticAssets(World world)
        {
            _world = world;
        }
    }
}
