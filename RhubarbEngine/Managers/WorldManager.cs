using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.DataStructure;
using RhubarbEngine.Render;
using RNumerics;
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Audio;
using RhubarbEngine.Components.Physics;

using BulletSharp;
using System.Numerics;
using System.Net;
using RhubarbEngine.Components.Interaction;
using DiscordRPC;
using System.Collections.Generic;
using RhubarbEngine.Utilities;
using RhubarbEngine.Helpers;

namespace RhubarbEngine.Managers
{
    public interface IWorldManager : IManager
    {
        SynchronizedCollection<World.World> Worlds { get; }
        World.World PrivateOverlay { get; }
        World.World LocalWorld { get; }
        World.World FocusedWorld { get; set; }
        PersonalSpace PersonalSpace { get; }
        bool DontSaveLocal { get; set; }
        IEngine Engine { get; }

        void AddToRenderQueue(RenderQueue gu, RemderLayers layer, BoundingFrustum frustum, Matrix4x4 view);
        void CleanUp();
        void CloseWorld(World.World world);
        World.World CreateNewWorld(string Name = "New World", bool focus = true, int maxUsers = 16);
        byte[] FocusedWorldToBytes();
        World.World LoadWorldFromBytes(byte[] data);
        void Update(DateTime startTime, DateTime Frame);
        void UpdateDiscord();
        byte[] WorldToBytes(World.World world);
    }

    public class WorldManager : IWorldManager
    {
		public IEngine Engine { get; private set; }

        public SynchronizedCollection<World.World> Worlds { get; private set; } = new();

		public World.World PrivateOverlay { get; private set; }

        public World.World LocalWorld { get; private set; }

        private World.World _focusedWorld;

		public World.World FocusedWorld { get { return _focusedWorld; } set { _focusedWorld = value; FocusedWorldChange(); } }

		public PersonalSpace PersonalSpace { get; private set; }

        public bool DontSaveLocal { get; set; } = false;

		public void UpdateDiscord()
		{
			if (FocusedWorld == LocalWorld)
			{
				Engine.DiscordRpcClient.SetPresence(new RichPresence()
				{
					Timestamps = new Timestamps(FocusedWorld.StartTime),
					Details = "The Engine",
					State = " in local World",
					Assets = new Assets()
					{
						LargeImageKey = "rhubarbvr",
						LargeImageText = "Faolan Says HI",
						SmallImageKey = "rhubarbvr2"
					}
				});
			}
			//else if ((int)FocusedWorld.accessLevel.value >= 3)
			//{
			//	engine.discordRpcClient.SetPresence(new RichPresence()
			//	{
			//		Timestamps = new Timestamps(FocusedWorld.StartTime),
			//		Details = "The Engine",
			//		State = " in " + FocusedWorld.Name.value,
			//		Secrets = new Secrets()
			//		{
			//			JoinSecret = "rhubarb://" + FocusedWorld.SessionID.value + "/",
			//			MatchSecret = "rhubarb://" + FocusedWorld.SessionID.value + "/",
			//		},
			//		Assets = new Assets()
			//		{
			//			LargeImageKey = "rhubarbvr",
			//			LargeImageText = "Faolan Says HI",
			//			SmallImageKey = "rhubarbvr2"
			//		},
			//		Party = new Party()
			//		{
			//			Privacy = Party.PrivacySetting.Public,
			//			ID = FocusedWorld.SessionID.value,
			//			Max = FocusedWorld.MaxUsers,
			//			Size = FocusedWorld.users.Count(),
			//		}
			//	});
			//}
			//else
			//{
			//	engine.discordRpcClient.SetPresence(new RichPresence()
			//	{
			//		Timestamps = new Timestamps(FocusedWorld.StartTime),
			//		Details = "The Engine",
			//		State = $" in {FocusedWorld.accessLevel.ToString()} World",
			//		Assets = new Assets()
			//		{
			//			LargeImageKey = "rhubarbvr",
			//			LargeImageText = "Faolan Says HI",
			//			SmallImageKey = "rhubarbvr2"
			//		},
			//		Party = new Party()
			//		{
			//			Privacy = Party.PrivacySetting.Private,
			//			ID = FocusedWorld.SessionID.value,
			//			Max = FocusedWorld.MaxUsers,
			//			Size = FocusedWorld.users.Count(),
			//		}
			//	});
			//}
		}

		public void FocusedWorldChange()
		{
			UpdateDiscord();
		}

		public void AddToRenderQueue(RenderQueue gu, RemderLayers layer, RhubarbEngine.Utilities.BoundingFrustum frustum, Matrix4x4 view)
		{
			try
			{
				Parallel.ForEach(Worlds, world =>
				 {
					 if (world.Focus != World.World.FocusLevel.Background)
					 {
						 world.AddToRenderQueue(gu, layer, frustum, view);
					 }
				 });
			}
			catch
			{

			}
		}

		public World.World LoadWorldFromBytes(byte[] data)
		{
			var node = new DataNodeGroup(data);
			var tempWorld = new World.World(this, node, false);
			return tempWorld;
		}
		public byte[] WorldToBytes(World.World world)
		{
			var val = Array.Empty<byte>();
			if (FocusedWorld != null)
			{
				var node = world.Serialize();
				val = node.GetByteArray();
			}
			return val;
		}

		public byte[] FocusedWorldToBytes()
		{
			return WorldToBytes(FocusedWorld);
		}

		public IManager Initialize(IEngine _engine)
		{
			Engine = _engine;

			Engine.Logger.Log("Starting Private Overlay World");
			PrivateOverlay = new World.World(this, "Private Overlay", 1, true);
			PersonalSpace = PrivateOverlay.RootEntity.AttachComponent<PersonalSpace>();
			Worlds.Add(PrivateOverlay);
			PrivateOverlay.Focus = World.World.FocusLevel.PrivateOverlay;
            if (Engine.EngineInitializer.CreateLocalWorld)
            {
                Task.Run(() =>
                {
                    Engine.Logger.Log("Starting Local World");
                    if (File.Exists(Engine.DataPath + "/LocalWorld.RWorld"))
                    {
                        try
                        {
                            var node = new DataNodeGroup(File.ReadAllBytes(Engine.DataPath + "/LocalWorld.RWorld"));
                            LocalWorld = new World.World(this, "LocalWorld", 16, false, true, node);
                        }
                        catch (Exception e)
                        {
                            DontSaveLocal = true;
                            _engine.Logger.Log("Failed To load LocalWorld" + e.ToString(), true);
                            LocalWorld = new World.World(this, "TempLoaclWorld", 16, false, true);
                            MeshHelper.BuildLocalWorld(LocalWorld);
                        }
                    }
                    else
                    {
                        try
                        {
                            LocalWorld = new World.World(this, "LocalWorld", 16, false, true);
                            MeshHelper.BuildLocalWorld(LocalWorld);
                        }
                        catch (Exception e)
                        {
                            _engine.Logger.Log("Failed To start New localWorld" + e.ToString());
                        }
                    }
                    LocalWorld.Focus = World.World.FocusLevel.Focused;
                    Worlds.Add(LocalWorld);
                    FocusedWorld = LocalWorld;
                });
            }
			return this;
		}

        //private void JoinSessionFromUUID(string session, bool v)
        //{
        //    throw new NotImplementedException();
        //}

        public void CloseWorld(World.World world)
        {
            world.Dispose();
        }

        public World.World CreateNewWorld(string Name = "New World",bool focus = true,int maxUsers = 16)
        {
            Engine.Logger.Log($"Creating New World Name:'{Name}' Focus{focus}  MaxUsers {maxUsers}");
            var newworld = new World.World(this, Name, maxUsers,false,false,null,true);
            Worlds.Add(newworld);
            newworld.Focus = focus ? World.World.FocusLevel.Focused : World.World.FocusLevel.Background;
            return newworld;
        }

        public void Update(DateTime startTime, DateTime Frame)
		{
			try
			{
				foreach (var world in Worlds)
				{
					world.Update(startTime, Frame);
				}
			}
			catch
			{

			}

		}
		public void CleanUp()
		{
			if (Engine.EngineInitializer == null && !DontSaveLocal)
			{
				File.WriteAllBytes(Engine.DataPath + "/LocalWorld.RWorld", WorldToBytes(LocalWorld));
			}
		}

        public void Update()
        {
        }
    }
}
