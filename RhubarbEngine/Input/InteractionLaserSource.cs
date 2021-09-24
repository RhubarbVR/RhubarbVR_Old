using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;
using RNumerics;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Managers;
using RhubarbEngine.World.ECS;
using Veldrid;
using RhubarbEngine.Components.Physics;

namespace RhubarbEngine.Input
{
	public class InteractionLaserSource
	{
		public Creality side;

		private readonly IEngine _engine;

        private IInputManager Input
        {
            get
            {
                return _engine.InputManager;
            }
        }

        public InteractionLaserSource(Creality _side, IEngine _engine)
		{
			side = _side;
			this._engine = _engine;
		}

		private Cursors _cursor = Cursors.None;

		public event Action<Cursors> CursorChange;

		public Cursors Cursor { get { return _cursor; } set { if (_cursor == value) { return; } _cursor = value; CursorChange?.Invoke(value); } }



		private bool HasClicked()
		{
			if ((_engine.OutputType == VirtualReality.OutputType.Screen) && (side == Creality.Right))
			{
				return (_engine.InputManager.MainWindows.GetMouseButton(MouseButton.Right)) | _engine.InputManager.MainWindows.GetMouseButton(MouseButton.Left) | _engine.InputManager.MainWindows.GetMouseButton(MouseButton.Middle);
			}
			switch (side)
			{
				case Creality.None:
					break;
				case Creality.Left:
					return Input.TriggerTouching(RhubarbEngine.Input.Creality.Left) | Input.GrabPress(RhubarbEngine.Input.Creality.Left) | Input.PrimaryPress(RhubarbEngine.Input.Creality.Left);
				case Creality.Right:
					return Input.TriggerTouching(RhubarbEngine.Input.Creality.Right) | Input.GrabPress(RhubarbEngine.Input.Creality.Right) | Input.PrimaryPress(RhubarbEngine.Input.Creality.Right);
				default:
					break;
			}
			return false;
		}

		private static Vector2f GetUVPosOnTry(Vector3d p1, Vector2f p1uv, Vector3d p2, Vector2f p2uv, Vector3d p3, Vector2f p3uv, Vector3d point)
		{
			var f1 = p1 - point;
			var f2 = p2 - point;
			var f3 = p3 - point;
			var a = Vector3d.Cross(p1 - p2, p1 - p3).magnitude;
			var a1 = Vector3d.Cross(f2, f3).magnitude / a;
			var a2 = Vector3d.Cross(f3, f1).magnitude / a;
			var a3 = Vector3d.Cross(f1, f2).magnitude / a;
            var uv = (p1uv * (float)a1) + (p2uv * (float)a2) + (p3uv * (float)a3);
			return uv;
		}

		private void RightLaser()
		{
			var e = RhubarbEngine.Input.Creality.Right;
			if (Input.GrabPress(e))
			{
                Input.MainWindows.FrameSnapshot.MouseClick(MouseButton.Right);
			}
			if (Input.PrimaryPress(e))
			{
                Input.MainWindows.FrameSnapshot.MouseClick(MouseButton.Left);
			}
		}
		private void LeftLaser()
		{
			var e = RhubarbEngine.Input.Creality.Left;
			if (Input.GrabPress(e))
			{
                Input.MainWindows.FrameSnapshot.MouseClick(MouseButton.Right);
			}
			if (Input.PrimaryPress(e))
			{
                Input.MainWindows.FrameSnapshot.MouseClick(MouseButton.Left);
			}
		}


		private bool ProossesMeshInputPlane(ClosestRayResultCallback cb)
		{
			try
			{
				var inputPlane = (MeshInputPlane)cb.CollisionObject.UserObject;
				System.Numerics.Matrix4x4.Decompose(inputPlane.Entity.GlobalTrans(), out var scale, out var rotation, out var translation);
				var pixsize = inputPlane.pixelSize.Value;

				var hit = cb.HitPointWorld;
				var hitnormal = cb.HitNormalWorld;

				var stepone = (hit - new Vector3(translation.X, translation.Y, translation.Z)) / new Vector3(scale.X, scale.Y, scale.Z);
				var steptwo = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3((float)stepone.X, (float)stepone.Y, (float)stepone.Z));
				var stepthree = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateFromQuaternion(System.Numerics.Quaternion.Inverse(rotation));
				var stepfour = steptwo * stepthree;
				System.Numerics.Matrix4x4.Decompose(stepfour, out var scsdale, out var rotatdsion, out var trans);

				if (inputPlane.mesh.Asset != null)
				{
					var hittry = inputPlane.mesh.Asset.Meshes[0].InsideTry(trans);
					var tryangle = inputPlane.mesh.Asset.Meshes[0].GetTriangle(hittry);
					var mesh = inputPlane.mesh.Asset.Meshes[0];
					var p1 = mesh.GetVertexAll(tryangle.a);
					var p2 = mesh.GetVertexAll(tryangle.b);
					var p3 = mesh.GetVertexAll(tryangle.c);

					var uvpos = GetUVPosOnTry(p1.v, p1.uv, p2.v, p2.uv, p3.v, p3.uv, new Vector3d(trans.X, trans.Y, trans.Z));
					uvpos.y = (-uvpos.y) + 1;
					var pospix = uvpos * new Vector2f(pixsize.x, pixsize.y);
					var pos = (System.Numerics.Vector2)pospix;

					var source = InteractionSource.None;
					switch (side)
					{
						case Creality.Left:
							source = InteractionSource.LeftLaser;
							break;
						case Creality.Right:
							source = InteractionSource.RightLaser;
							break;
						default:
							break;
					}
					inputPlane.UpdatePos(pos, source);

					if (HasClicked())
					{
						switch (source)
						{
							case InteractionSource.LeftLaser:
								LeftLaser();
								break;
							case InteractionSource.RightLaser:
								RightLaser();
								break;
							default:
								break;
						}
						inputPlane.Click(pos, source);
					}
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		private bool ProossesInputPlane(ClosestRayResultCallback cb)
		{
			try
			{
				var inputPlane = (InputPlane)cb.CollisionObject.UserObject;
				System.Numerics.Matrix4x4.Decompose(inputPlane.Entity.GlobalTrans(), out var scale, out var rotation, out var translation);
				var size = inputPlane.size.Value;
				var pixsize = inputPlane.pixelSize.Value;

				var hit = cb.HitPointWorld;
				var hitnormal = cb.HitNormalWorld;

				var stepone = (hit - new Vector3(translation.X, translation.Y, translation.Z)) / new Vector3(scale.X, scale.Y, scale.Z);
				var steptwo = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3((float)stepone.X, (float)stepone.Y, (float)stepone.Z));
				var stepthree = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateFromQuaternion(System.Numerics.Quaternion.Inverse(rotation));
				var stepfour = steptwo * stepthree;
				System.Numerics.Matrix4x4.Decompose(stepfour, out var scsdale, out var rotatdsion, out var trans);
				var nonescaleedpos = new Vector2f(trans.X, -trans.Z);
				var posnopixs = (nonescaleedpos * (1 / size) / 2) + 0.5f;

				var pospix = posnopixs * new Vector2f(pixsize.x, pixsize.y);

				var pos = (System.Numerics.Vector2)pospix;

				var source = InteractionSource.None;
				switch (side)
				{
					case Creality.Left:
						source = InteractionSource.LeftLaser;
						break;
					case Creality.Right:
						source = InteractionSource.RightLaser;
						break;
					default:
						break;
				}
				inputPlane.UpdatePos(pos, source);
				if (HasClicked())
				{
					switch (source)
					{
						case InteractionSource.LeftLaser:
							LeftLaser();
							break;
						case InteractionSource.RightLaser:
							RightLaser();
							break;
						default:
							break;
					}
					inputPlane.Click(pos, source);
				}
				return true;
			}
			catch
			{
			}
			return false;
		}

		private bool ProssesCollider(ClosestRayResultCallback cb)
		{
			var col = (Collider)cb.CollisionObject.UserObject;
			if (col == null)
			{
				return false;
			}
			var ent = col.Entity;
			if (HasClicked())
			{
				ent.SendClick(true);
				var source = InteractionSource.None;
				switch (side)
				{
					case Creality.Left:
						source = InteractionSource.LeftLaser;
						break;
					case Creality.Right:
						source = InteractionSource.RightLaser;
						break;
					default:
						break;
				}
				if (_engine.OutputType == VirtualReality.OutputType.Screen)
				{
					ent.SendSecondary(Input.MainWindows.GetMouseButton(MouseButton.Middle));
					ent.SendPrimary(Input.MainWindows.GetMouseButton(MouseButton.Left));
					ent.SendGrip(true, col.World.HeadLaserGrabbableHolder, Input.MainWindows.GetMouseButton(MouseButton.Right));
				}
				switch (source)
				{
					case InteractionSource.None:
						break;
					case InteractionSource.LeftLaser:
						ent.SendTriggerTouching(Input.TriggerTouching(RhubarbEngine.Input.Creality.Left));
						ent.SendSecondary(Input.SecondaryPress(RhubarbEngine.Input.Creality.Left));
						ent.SendPrimary(Input.PrimaryPress(RhubarbEngine.Input.Creality.Left));
						ent.SendGrip(true, col.World.LeftLaserGrabbableHolder, Input.GrabPress(RhubarbEngine.Input.Creality.Left));
						break;
					case InteractionSource.LeftFinger:
						break;
					case InteractionSource.RightLaser:
						ent.SendTriggerTouching(Input.TriggerTouching(RhubarbEngine.Input.Creality.Right));
						ent.SendSecondary(Input.SecondaryPress(RhubarbEngine.Input.Creality.Right));
						ent.SendPrimary(Input.PrimaryPress(RhubarbEngine.Input.Creality.Right));
						ent.SendGrip(true, col.World.RightLaserGrabbableHolder, Input.GrabPress(RhubarbEngine.Input.Creality.Right));
						break;
					case InteractionSource.RightFinger:
						break;
					case InteractionSource.HeadLaser:
						ent.SendSecondary(Input.MainWindows.GetMouseButton(MouseButton.Middle));
						ent.SendPrimary(Input.MainWindows.GetMouseButton(MouseButton.Left));
						ent.SendGrip(true, col.World.HeadLaserGrabbableHolder, Input.MainWindows.GetMouseButton(MouseButton.Right));
						break;
					case InteractionSource.HeadFinger:
						break;
					default:
						break;
				}
			}
			return true;
		}

		private void ProssecesHitPoint(Vector3d pos, Vector3d normal)
		{
			Pos = pos;
			Normal = normal;
			OnHit?.Invoke(pos, normal, (pos == Vector3d.Zero) && (normal == Vector3d.Zero));
		}

		public void UnLock()
		{
			IsLocked = false;
			Cursor = Cursors.None;
		}

		public void Lock()
		{
			IsLocked = true;
			Cursor = Cursors.Grabbing;
		}

		public delegate void ProssecesHitPointAction(Vector3d pos, Vector3d normal, bool Hide);

		public event ProssecesHitPointAction OnHit;

		public Vector3d Pos { get; private set; }
		public Vector3d Normal { get; private set; }

		public bool IsLocked { get; private set; }

		public bool HasHit { get; private set; }

        public bool Isvisible
        {
            get
            {
                return _activelySnapping || HasHit || IsLocked;
            }
        }

        public Vector3 Sourcse { get; private set; }

		public Vector3 Destination { get; private set; }

		private readonly float _maxDistinatains = 100;

		private Vector3 _lastDeriction;

        private float SnapDistance
        {
            get
            {
                return _engine.SettingsObject.InteractionSettings.SnapDistance / 100;
            }
        }

        private float Smoothing
        {
            get
            {
                return _engine.SettingsObject.InteractionSettings.Smoothing;
            }
        }

        private float _lastDistance;

        private Vector3 _lastRayCastDeriction;
		private Vector3 _lastRayCastsourcse;

		private Vector3 _iRayCastDeriction;
		private Vector3 _iRayCastsourcse;

		private static Vector3 Lerp(Vector3 v1, Vector3 v2, double pos)
		{
			return v1 + ((v2 - v1) * pos);
		}
		private static bool Aprogamtly(double v1, double v2, double pos)
		{
			return Math.Abs(v1 - v2) <= pos;
		}
		private static bool Aprogamtly(Vector3 v1, Vector3 v2, double pos)
		{
			return Aprogamtly(v1.X, v2.X, pos) && Aprogamtly(v1.Y, v2.Y, pos) && Aprogamtly(v1.Z, v2.Z, pos);
		}
		private bool _snaping;
		private bool _activelySnapping;

		public void SendRayCast(Vector3 _sourcse, Vector3 deriction)
		{
			var dist = _maxDistinatains;

			Vector3 smoothedDeriction;
			Vector3 smoothedSourcse;

			if (Smoothing != 0)
			{
				var poser = _engine.PlatformInfo.DeltaSeconds * 2 * Smoothing;
				_iRayCastDeriction = Lerp(_iRayCastDeriction, deriction, poser);
				_iRayCastsourcse = Lerp(_iRayCastsourcse, _sourcse, poser);
				smoothedDeriction = Lerp(_lastRayCastDeriction, _iRayCastDeriction, poser);
				smoothedSourcse = Lerp(_lastRayCastsourcse, _iRayCastsourcse, poser);
			}
			else
			{
				smoothedDeriction = deriction;
				smoothedSourcse = _sourcse;
			}

			_lastRayCastDeriction = smoothedDeriction;
			_lastRayCastsourcse = smoothedSourcse;
			var result = Math.Sqrt(Math.Pow(smoothedDeriction.X - _lastDeriction.X, 2) + Math.Pow(smoothedDeriction.Y - _lastDeriction.Y, 2) + Math.Pow(smoothedDeriction.Z - _lastDeriction.Z, 2));
			if (Aprogamtly(smoothedDeriction, _lastDeriction, 0.003))
			{
				result = 0f;
			}
			if (IsLocked)
			{
				var point = smoothedSourcse + (smoothedDeriction * _lastDistance);
				ProssecesHitPoint(new Vector3d(point.X, point.Y, point.Z), -new Vector3d(smoothedDeriction.X, smoothedDeriction.Y, smoothedDeriction.Z));
				return;
			}
			if ((result < SnapDistance) && _snaping)
			{
				dist = _lastDistance + 0.5f;
				_activelySnapping = true;
			}
			else
			{ _activelySnapping = false; }
			ProsscesRayTestHit(smoothedSourcse, (smoothedDeriction * dist) + smoothedSourcse, smoothedDeriction);
		}

		public void UpdateLaserPos(Vector3 _sourcse, Vector3 _destination)
		{
			Sourcse = _sourcse;
			Destination = _destination;
		}
		private void ProsscesRayTestHit(Vector3 _sourcse, Vector3 _destination, Vector3 deriction)
		{
			if (!RayTestHitTest(_sourcse, _destination, _engine.WorldManager.privateOverlay))
			{
				var hittestbool = false;
				foreach (var item in _engine.WorldManager.worlds)
				{
					if ((item.Focus == World.World.FocusLevel.Overlay) && !hittestbool)
					{
						hittestbool = RayTestHitTest(_sourcse, _destination, item);
					}
				}
				if (!((!RayTestHitTest(_sourcse, _destination, _engine.WorldManager.FocusedWorld)) && !hittestbool))
				{
					HasHit = true;
					_lastDeriction = deriction;
					ProsscesHit();
				}
				else
				{
					HasHit = false;
				}
			}
			else
			{
				HasHit = true;
				_lastDeriction = deriction;
				ProsscesHit();
			}
		}

		private void ProsscesHit()
		{
			if (!HitTest(Sourcse, Destination, _engine.WorldManager.privateOverlay))
			{
				var hittestbool = false;
				foreach (var item in _engine.WorldManager.worlds)
				{
					if ((item.Focus == World.World.FocusLevel.Overlay) && !hittestbool)
					{
						hittestbool = HitTest(Sourcse, Destination, item);
					}
				}
				if ((!HitTest(Sourcse, Destination, _engine.WorldManager.FocusedWorld)) && !hittestbool)
				{
					_snaping = false;
                    Cursor = RhubarbEngine.Input.Cursors.None;
					ProssecesHitPoint(Vector3d.Zero, Vector3d.Zero);
				}
			}
		}

		private bool RayTestHitTest(Vector3 sourcse, Vector3 destination, World.World eworld)
		{
			if (eworld == null)
            {
                return false;
            }

            using var cb = new ClosestRayResultCallback(ref sourcse, ref destination);
            eworld.PhysicsWorld.RayTest(sourcse, destination, cb);
            if (cb.HasHit)
            {
                UpdateLaserPos(cb.HitPointWorld + (cb.HitNormalWorld * 0.01f), cb.HitPointWorld + (cb.HitNormalWorld * -0.02f));
                var result = Math.Sqrt(Math.Pow(cb.HitPointWorld.X - sourcse.X, 2) + Math.Pow(cb.HitPointWorld.Y - sourcse.Y, 2) + Math.Pow(cb.HitPointWorld.Z - sourcse.Z, 2));
                _lastDistance = (float)result;
                var type = cb.CollisionObject.UserObject.GetType();
                if (type == typeof(InputPlane))
                {
                    return true;
                }
                else if (type == typeof(MeshInputPlane))
                {
                    return true;
                }
                else if (typeof(Collider).IsAssignableFrom(type))
                {
                    return true;
                }
            }
            return false;
        }
		private bool HitTest(Vector3 sourcse, Vector3 destination, World.World eworld)
		{
			if (eworld == null)
            {
                return false;
            }

            using var cb = new ClosestRayResultCallback(ref sourcse, ref destination);
            eworld.PhysicsWorld.RayTest(sourcse, destination, cb);
            if (cb.HasHit)
            {
                ProssecesHitPoint(new Vector3d(cb.HitPointWorld.X, cb.HitPointWorld.Y, cb.HitPointWorld.Z), new Vector3d(cb.HitNormalWorld.X, cb.HitNormalWorld.Y, cb.HitNormalWorld.Z));
                var type = cb.CollisionObject.UserObject.GetType();
                if (type == typeof(InputPlane))
                {
                    _snaping = true;
                    return ProossesInputPlane(cb);
                }
                else if (type == typeof(MeshInputPlane))
                {
                    _snaping = true;
                    return ProossesMeshInputPlane(cb);
                }
                else if (typeof(Collider).IsAssignableFrom(type))
                {
                    _snaping = false;
                    return ProssesCollider(cb);
                }
            }
            _snaping = false;
            return false;
        }

		public void Update()
		{


		}

	}
}
