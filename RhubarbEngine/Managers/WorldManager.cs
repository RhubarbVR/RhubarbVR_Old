﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.DataStructure;
using RhubarbEngine.Render;
using g3;
using RhubarbEngine.Components.Transform;


namespace RhubarbEngine.Managers
{
    public class WorldManager : IManager
    {
        public Engine engine;

        public List<World.World> worlds = new List<World.World>();

        public World.World privateOverlay;

        public World.World localWorld;

        public World.World focusedWorld;

        public void addToRenderQueue(RenderQueue gu, RemderLayers layer)
        {
            foreach(World.World world in worlds)
            {
                if (world.Focus != World.World.FocusLevel.Background)
                {
                    world.addToRenderQueue(gu, layer);
                }
            }
        } 

        public void createNewWorld()
        {

        }

        public World.World loadWorldFromBytes(byte[] data)
        {
            DataNodeGroup node = new DataNodeGroup(data);
            World.World tempWorld = new World.World(this, node, false);
            return tempWorld;
        }
        public byte[] worldToBytes(World.World world)
        {
            byte[] val = new byte[] { };
            if (focusedWorld != null)
            {
                DataNodeGroup node = world.serialize();
                val = node.getByteArray();
            }
            return val;
        }

        public byte[] focusedWorldToBytes()
        {
            return worldToBytes(focusedWorld);
        }


        public IManager initialize(Engine _engine)
        {
            engine = _engine;

            engine.logger.Log("Starting Private Overlay World");
            privateOverlay = new World.World(this, "Private Overlay", 1);
            privateOverlay.Focus = World.World.FocusLevel.PrivateOverlay;
            worlds.Add(privateOverlay);
            Entity ent = privateOverlay.RootEntity.addChild();
            ent.position.value = Vector3f.One;
            Entity ent2 = privateOverlay.RootEntity.addChild();
            ent2.position.value = Vector3f.Zero;
            ent2.attachComponent<Spinner>();
            Entity ent3 = ent2.addChild();
            ent3.position.value = new Vector3f(10f, 10f, 10f);
            //engine.logger.Log("Starting Local World");
            //if(File.Exists(engine.dataPath + "/LocalWorld.RWorld"))
            //{
            //    localWorld = loadWorldFromBytes(File.ReadAllBytes(engine.dataPath + "/LocalWorld.RWorld"));
            //}
            //else
            //{
            //   localWorld = new World.World(this,"LoaclWorld",16);
            //}
            //localWorld.Focus = World.World.FocusLevel.Focused;
            //worlds.Add(localWorld);
            //focusedWorld = localWorld;
            return this;
        }

        public void Update(DateTime startTime, DateTime Frame)
        {
            foreach (World.World world in worlds)
            {
                world.Update(startTime, Frame);
            }
        }
        public void CleanUp()
        {
            File.WriteAllBytes(engine.dataPath + "/LocalWorld.RWorld", worldToBytes(localWorld));
        }
    }
}
