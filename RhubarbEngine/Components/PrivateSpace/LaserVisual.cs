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
using RhubarbEngine.Components.Physics.Colliders;
using BulletSharp;
using BulletSharp.Math;
using RNumerics;
using Veldrid;
using RhubarbEngine.Helpers;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Transform;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.Components.PrivateSpace
{
	public class LaserVisual : AssetProvider<RTexture2D>
	{

		public Sync<InteractionSource> source;

		public SyncRef<Entity> Currsor;
        public SyncRef<Entity> CurrsorDir;
        public SyncRef<Entity> Laser;
		public SyncRef<CurvedTubeMesh> LaserMesh;

		public SyncRef<Render.Material.Fields.ColorField> colorField;
		public SyncRef<Render.Material.Fields.ColorField> planeColorField;

		private bool _bind;

		public override void OnAttach()
		{
			base.OnAttach();
            var (cDirurs, _, cDirmit) = MeshHelper.AddMesh<PlaneMesh>(Entity, World.staticAssets.OverLayedUnlitShader, "CurrsorDir", 0);

            var (curs, _, cmit) = MeshHelper.AddMesh<PlaneMesh>(Entity, World.staticAssets.OverLayedUnlitShader, "Currsor", 0);
			var (Lasere, lmesh, mit) = MeshHelper.AddMesh<CurvedTubeMesh>(Entity, World.staticAssets.OverLayedUnlitShader, "Laser", 3);
			Laser.Target = Lasere;
			Lasere.rotation.Value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
			LaserMesh.Target = lmesh;
			Currsor.Target = curs;
            CurrsorDir.Target = cDirurs;
            var look = curs.AttachComponent<LookAtUser>();
			look.offset.Value = Quaternionf.CreateFromEuler(0f, 90f, 0f);
			var scaler = curs.AttachComponent<DistanceScaler>();
			scaler.scale.Value = 0.025f;
			scaler.pow.Value = 0.5;
			scaler.offset.Value = new Vector3f(-0.1);

            var looke = cDirurs.AttachComponent<LookAtUser>();
            looke.offset.Value = Quaternionf.CreateFromEuler(0f, 90f, 0f);
            var scalere = cDirurs.AttachComponent<DistanceScaler>();
            scalere.scale.Value = 0.025f;
            scalere.pow.Value = 0.5;
            scalere.offset.Value = new Vector3f(-0.1);

            var pos = cmit.GetField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
			var pos2 = mit.GetField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
			pos.field.Value = 0.1f;
			pos2.field.Value = 0.2f;

            var pos3 = cDirmit.GetField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
            pos3.field.Value = 0.1f;
            cDirmit.GetField<Render.Material.Fields.ColorField>("TintColor", Render.Shader.ShaderType.MainFrag).field.Value = Colorf.White;
            colorField.Target = mit.GetField<Render.Material.Fields.ColorField>("TintColor", Render.Shader.ShaderType.MainFrag);
			planeColorField.Target = cmit.GetField<Render.Material.Fields.ColorField>("TintColor", Render.Shader.ShaderType.MainFrag);
			var textureField = cmit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			textureField.field.Target = this;

            var textureFieldr = cDirmit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            textureFieldr.field.Target = this;
        }

		public override void BuildSyncObjs(bool newRefIds)
		{
            source = new Sync<InteractionSource>(this, newRefIds)
            {
                Value = InteractionSource.HeadLaser
            };
            Currsor = new SyncRef<Entity>(this, newRefIds);
			Laser = new SyncRef<Entity>(this, newRefIds);
			LaserMesh = new SyncRef<CurvedTubeMesh>(this, newRefIds);
			colorField = new SyncRef<Render.Material.Fields.ColorField>(this, newRefIds);
			planeColorField = new SyncRef<Render.Material.Fields.ColorField>(this, newRefIds);
            CurrsorDir = new SyncRef<Entity>(this, newRefIds);

        }

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (Currsor.Target == null)
            {
                return;
            }

            var pos = Vector3.Zero;
            var pose = Vector3.Zero;
            var left = false;
			switch (source.Value)
			{
				case InteractionSource.LeftLaser:
					pose = Input.LeftLaser.Pos;
                    pos = Input.LeftLaser.Destination;
                    left = true;
					break;
				case InteractionSource.RightLaser:
					pose = Input.RightLaser.Pos;
                    pos = Input.RightLaser.Destination;
                    break;
				case InteractionSource.HeadLaser:
					pose = Input.RightLaser.Pos;
                    pos = Input.RightLaser.Destination;
                    break;
				default:
					break;
			}
			if (!_bind)
			{
				if (left)
				{
					Input.LeftLaser.CursorChange += UpdateCursor;
				}
				else
				{
					Input.RightLaser.CursorChange += UpdateCursor;
				}
				_bind = true;
			}
			var hitvector = Vector3.Zero;
			switch (source.Value)
			{
				case InteractionSource.LeftLaser:
					hitvector = Input.LeftLaser.Normal;
					break;
				case InteractionSource.RightLaser:
					hitvector = Input.RightLaser.Normal;
					break;
				case InteractionSource.HeadLaser:
					hitvector = Input.RightLaser.Normal;
					break;
				default:
					break;
			}
			var newpos = new Vector3f(pos.X, pos.Y, pos.Z);
			Currsor.Target.SetGlobalPos(newpos);
            CurrsorDir.Target.SetGlobalPos(new Vector3f(pose.X, pose.Y, pose.Z));
            if (LaserMesh.Target == null)
            {
                return;
            }

            if (Laser.Target == null)
            {
                return;
            }

            var mesh = LaserMesh.Target;
			mesh.Endpoint.Value = Laser.Target.GlobalPointToLocal(newpos);
			var val = Entity.GlobalPos().Distance(new Vector3f(pos.X, pos.Y, pos.Z));
			mesh.StartHandle.Value = Vector3d.AxisY * (val / 4);
			var e = Laser.Target.GlobalRot().Inverse() * new Vector3f(hitvector.X, hitvector.Y, hitvector.Z);
			mesh.EndHandle.Value = e * (val / 6);
			switch (source.Value)
			{
				case InteractionSource.LeftLaser:
					Currsor.Target.enabled.Value = Input.LeftLaser.Isvisible;
                    CurrsorDir.Target.enabled.Value = Input.LeftLaser.Isvisible && Input.LeftLaser.IsLocked;
                    Laser.Target.enabled.Value = Input.LeftLaser.Isvisible;
					break;
				case InteractionSource.RightLaser:
					Currsor.Target.enabled.Value = Input.RightLaser.Isvisible;
                    CurrsorDir.Target.enabled.Value = Input.RightLaser.Isvisible && Input.RightLaser.IsLocked;
                    Laser.Target.enabled.Value = Input.RightLaser.Isvisible;
					break;
				case InteractionSource.HeadLaser:
					Currsor.Target.enabled.Value = Input.RightLaser.Isvisible;
                    CurrsorDir.Target.enabled.Value = Input.RightLaser.Isvisible && Input.RightLaser.IsLocked;
                    Laser.Target.enabled.Value = Input.RightLaser.Isvisible;
					break;
				default:
					break;
			}
		}

		private void UpdateCursor(Input.Cursors newcursor)
		{
			var color = new Colorf(1f, 0.7f, 1f, 0.7f);
			switch (newcursor)
			{
				case RhubarbEngine.Input.Cursors.Grabbing:
					color = new Colorf(0.7f, 0.7f, 1f, 0.7f);
					break;
				default:
					break;
			}
			if (colorField.Target != null)
            {
                colorField.Target.field.Value = color;
            }

            if (planeColorField.Target != null)
            {
                planeColorField.Target.field.Value = color;
            }

            Load(new RTexture2D(Engine.RenderManager.Cursors[(int)newcursor]));
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
            UpdateCursor(RhubarbEngine.Input.Cursors.None);
		}

		public LaserVisual(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public LaserVisual()
		{
		}

	}
}
