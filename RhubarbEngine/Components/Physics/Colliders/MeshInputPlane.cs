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
using g3;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;
using Veldrid;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.Components.Physics.Colliders
{

    [Category(new string[] { "Physics/Colliders" })]
    public class MeshInputPlane : Collider, IinputPlane
    {
        public AssetRef<RMesh> mesh;

        public Sync<Vector2u> pixelSize;

        public Sync<bool> FocusedOverride;

        public SyncDelegate onFocusLost;

        public IReadOnlyList<KeyEvent> KeyEvents { get { if (!focused) return new List<KeyEvent>(); return input.mainWindows.FrameSnapshot.KeyEvents; } }

        public IReadOnlyList<MouseEvent> MouseEvents { get { if (isNotTakingInput) return new List<MouseEvent>(); return input.mainWindows.FrameSnapshot.MouseEvents; } }

        public IReadOnlyList<char> KeyCharPresses { get { if (!focused) return new List<Char>(); return input.mainWindows.FrameSnapshot.KeyCharPresses;  } }

        private Vector2 mousePosition = Vector2.Zero;

        public Vector2 MousePosition => mousePosition;


        public void Click(Vector2 pos, InteractionSource sourc)
        {
            Setfocused();
            val = sourc;
            mousePosition = pos;
            StopMousePos = false;
        }

        public void updatePos(Vector2 pos, InteractionSource sourc)
        {
            if (StopMousePos) return;
            if (sourc != val) return;
            mousePosition = pos;
            hover = 0;
        }

        public float WheelDelta
        {
            get
            {
                if (isNotTakingInput) return 0f;
                return input.mainWindows.FrameSnapshot.WheelDelta;
            }
        }

        private InteractionSource val = InteractionSource.None;

        public InteractionSource source => val;

        public bool isNotTakingInput => (!focused || (hover > 3));


        private byte hover = 0;

        private bool _focused = false;

        public bool focused => _focused;

        public bool StopMousePos = false;

        public bool StopMouse { get { return StopMousePos; } set { StopMousePos = value; } }

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            pixelSize = new Sync<Vector2u>(this, newRefIds);
            pixelSize.value = new Vector2u(600, 600);
            FocusedOverride = new Sync<bool>(this, newRefIds);
            onFocusLost = new SyncDelegate(this, newRefIds);
            mesh = new AssetRef<RMesh>(this, newRefIds);
            mesh.loadChange += Mesh_loadChange;
            entity.enabledChanged += Entity_enabledChanged;
        }

        private void Entity_enabledChanged()
        {
            if ((!entity.isEnabled) && focused)
            {
                Removefocused();
            }
        }

        private void Mesh_loadChange(RMesh obj)
        {
            BuildShape();
        }

        public override void onLoaded()
        {
            base.onLoaded();
            BuildShape();
        }
        private void goNull()
        {
            buildCollissionObject(null);
        }
        public int[] index = new int[] { };
        public BulletSharp.Math.Vector3[] vertices = new BulletSharp.Math.Vector3[] { };


        public unsafe override void BuildShape()
        {
            if (mesh.Asset == null) { goNull(); return; };
            if (!mesh.target?.loaded ?? false) { goNull(); return; };

            // Initialize TriangleIndexVertexArray with Vector3 array
            vertices = new BulletSharp.Math.Vector3[mesh.Asset.meshes[0].VertexCount];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new BulletSharp.Math.Vector3(
                    mesh.Asset.meshes[0].GetVertex(i).x,
                    mesh.Asset.meshes[0].GetVertex(i).y,
                    mesh.Asset.meshes[0].GetVertex(i).z);
            }
            var e = mesh.Asset.meshes[0].RenderIndices().ToArray();

            // Initialize TriangleIndexIndexArray with int array
            index = new int[e.Length];
            for (int i = 0; i < index.Length; i++)
            {
                index[i] = e[i];
            }
            if (index.Length < 3) return;
            var indexVertexArray2 = new TriangleIndexVertexArray(index, vertices);
            BvhTriangleMeshShape trys = new BvhTriangleMeshShape(indexVertexArray2, false);
            startShape(trys);
        }
        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
            if (hover > 3) return;
            hover++;
        }
        public bool IsMouseDown(MouseButton button)
        {
            if (StopMousePos) return false;
            if (isNotTakingInput) return false;
            switch (source)
            {
                case InteractionSource.None:
                    break;
                case InteractionSource.LeftLaser:
                    switch (button)
                    {
                        case MouseButton.Left:
                            return input.PrimaryPress(Input.Creality.Left);
                            break;
                        case MouseButton.Middle:
                            return input.SecondaryPress(Input.Creality.Left);
                            break;
                        case MouseButton.Right:
                            return input.GrabPress(Input.Creality.Left);
                            break;
                        case MouseButton.Button1:
                            break;
                        case MouseButton.Button2:
                            break;
                        case MouseButton.Button3:
                            break;
                        case MouseButton.Button4:
                            break;
                        case MouseButton.Button5:
                            break;
                        case MouseButton.Button6:
                            break;
                        case MouseButton.Button7:
                            break;
                        case MouseButton.Button8:
                            break;
                        case MouseButton.Button9:
                            break;
                        case MouseButton.LastButton:
                            break;
                        default:
                            break;
                    }
                    break;
                case InteractionSource.LeftFinger:
                    break;
                case InteractionSource.RightLaser:
                    switch (button)
                    {
                        case MouseButton.Left:
                            return input.PrimaryPress(Input.Creality.Right);
                            break;
                        case MouseButton.Middle:
                            return input.SecondaryPress(Input.Creality.Right);
                            break;
                        case MouseButton.Right:
                            return input.GrabPress(Input.Creality.Right);
                            break;
                        case MouseButton.Button1:
                            break;
                        case MouseButton.Button2:
                            break;
                        case MouseButton.Button3:
                            break;
                        case MouseButton.Button4:
                            break;
                        case MouseButton.Button5:
                            break;
                        case MouseButton.Button6:
                            break;
                        case MouseButton.Button7:
                            break;
                        case MouseButton.Button8:
                            break;
                        case MouseButton.Button9:
                            break;
                        case MouseButton.LastButton:
                            break;
                        default:
                            break;
                    }
                    break;
                case InteractionSource.RightFinger:
                    break;
                case InteractionSource.HeadLaser:
                    return engine.inputManager.mainWindows.GetMouseButtonDown(button);
                    break;
                case InteractionSource.HeadFinger:
                    break;
                default:
                    break;
            }
            return false;
        }

        public void Setfocused()
        {
            if (_focused) {
                return;
            }
            _focused = true;
            if (!FocusedOverride.value)
            {
                input.RemoveFocus();
            }
            input.removeFocus += Removefocused;
        }

        public void Removefocused()
        {
            onFocusLost.Target?.Invoke();
            input.removeFocus -= Removefocused;
            _focused = false;
        }

        public MeshInputPlane(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public MeshInputPlane()
        {
        }
    }
}
