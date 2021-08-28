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
using g3;
using Veldrid;
using RhubarbEngine.Helpers;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Transform;

namespace RhubarbEngine.Components.PrivateSpace
{
    public class LaserVisual : Component
    {

        public Sync<InteractionSource> source;

        public SyncRef<Entity> Currsor;
        public SyncRef<Entity> Laser;
        public SyncRef<CurvedTubeMesh> LaserMesh;

        public override void OnAttach()
        {
            base.OnAttach();
            var (curs, mesh, cmit) = MeshHelper.AddMesh<PlaneMesh>(entity, world.staticAssets.overLayedUnlitShader, "Currsor");
            var (Lasere, lmesh, mit) = MeshHelper.AddMesh<CurvedTubeMesh>(entity, world.staticAssets.overLayedUnlitShader,"Laser");
            Laser.target = Lasere;
            Lasere.rotation.value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
            LaserMesh.target = lmesh;
            Currsor.target = curs;
            var look = curs.attachComponent<LookAtUser>();
            look.offset.value = Quaternionf.CreateFromEuler(0f, 90f, 0f);
            var scaler = curs.attachComponent<DistanceScaler>();
            scaler.scale.value = 0.025f;
            scaler.pow.value = 0.5;
            scaler.offset.value = new Vector3f(-0.1);
            var pos = cmit.getField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
            var pos2 = mit.getField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
            pos.field.value = 0.3f;
            pos2.field.value = 0.6f;
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            source = new Sync<InteractionSource>(this, newRefIds);
            source.value = InteractionSource.HeadLaser;
            Currsor = new SyncRef<Entity>(this, newRefIds);
            Laser = new SyncRef<Entity>(this, newRefIds);
            LaserMesh = new SyncRef<CurvedTubeMesh>(this, newRefIds);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
            if (Currsor.target == null) return;
            Vector3d pos = Vector3d.Zero;
            switch (source.value)
            {
                case InteractionSource.LeftLaser:
                    pos = input.LeftLaser.pos;
                    break;
                case InteractionSource.RightLaser:
                    pos = input.RightLaser.pos;
                    break;
                case InteractionSource.HeadLaser:
                    pos = input.RightLaser.pos;
                    break;
                default:
                    break;
            }
            Vector3d hitvector = Vector3d.Zero;
            switch (source.value)
            {
                case InteractionSource.LeftLaser:
                    hitvector = input.LeftLaser.normal;
                    break;
                case InteractionSource.RightLaser:
                    hitvector = input.RightLaser.normal;
                    break;
                case InteractionSource.HeadLaser:
                    hitvector = input.RightLaser.normal;
                    break;
                default:
                    break;
            }
            var newpos = new Vector3f(pos.x, pos.y, pos.z);
            Currsor.target.SetGlobalPos(newpos);
            if (LaserMesh.target == null) return;
            if (Laser.target == null) return;
            var mesh = LaserMesh.target;
            mesh.Endpoint.value = Laser.target.GlobalPointToLocal(newpos);
            var val = entity.globalPos().Distance(new Vector3f(pos.x, pos.y, pos.z));
            mesh.StartHandle.value = Vector3d.AxisY * (val/4);
            var e = Laser.target.globalRot().Inverse() * new Vector3f(hitvector.x, hitvector.y, hitvector.z);
            mesh.EndHandle.value = e * (val / 6);
        }

        public LaserVisual(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public LaserVisual()
        {
        }

    }
}
