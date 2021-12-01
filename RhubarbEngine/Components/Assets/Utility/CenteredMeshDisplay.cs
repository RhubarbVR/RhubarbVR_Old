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
using RhubarbEngine.World.Asset;
using RNumerics;
using System.Numerics;
using Veldrid;
using RhubarbEngine.Render;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;

namespace RhubarbEngine.Components.Assets
{
	[Category(new string[] { "Assets/Utility" })]
	public class CenteredMeshDisplay : Component
	{
        public AssetRef<RMesh> Mesh;

        public Driver<Vector3f> Scale;

        public Driver<Vector3f> Position;

        public override void BuildSyncObjs(bool newRefIds)
		{
            Mesh = new AssetRef<RMesh>(this, newRefIds);
            Mesh.LoadChange += Mesh_LoadChange;
            Scale = new Driver<Vector3f>(this, newRefIds);
            Scale.Changed += ChangedValue;
            Position = new Driver<Vector3f>(this, newRefIds);
            Position.Changed += ChangedValue;
        }

        private void Mesh_LoadChange(RMesh obj)
        {
            UpdateValues();
        }

        private void ChangedValue(IChangeable obj)
        {
            UpdateValues();
        }

        private void UpdateValues()
        {
            if(Mesh.Asset is not null)
            {
                Position.Drivevalue = Mesh.Asset.boundingBox.GetCenter();
                var dem = Mesh.Asset.boundingBox.GetDimensions();
                var e = MathF.Max(dem.X, MathF.Max(dem.Y, dem.Z));
                Scale.Drivevalue = new Vector3f(1 / MathF.Sqrt(MathF.Pow(e,2)*2));
            }
        }

        public override void OnAttach()
        {
            base.OnAttach();
            Position.Target = Entity.position;
            Scale.Target = Entity.scale;
        }

        public CenteredMeshDisplay(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CenteredMeshDisplay()
		{
		}
	}
}
