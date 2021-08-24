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

namespace RhubarbEngine.Components.PrivateSpace
{
    public class LaserVisual : Component
    {

        public Sync<InteractionSource> source;

        public SyncRef<Entity> Currsor;

        public override void OnAttach()
        {
            base.OnAttach();
            var (curs,mesh) = MeshHelper.AddMesh<SphereMesh>(entity, "Currsor");
            Currsor.target = curs;
            mesh.Radius.value = 0.05f;
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            source = new Sync<InteractionSource>(this, newRefIds);
            source.value = InteractionSource.HeadLaser;
            Currsor = new SyncRef<Entity>(this, newRefIds);
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
            Currsor.target.SetGlobalPos(new Vector3f(pos.x, pos.y,pos.z));
        }

        public LaserVisual(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public LaserVisual()
        {
        }

    }
}
