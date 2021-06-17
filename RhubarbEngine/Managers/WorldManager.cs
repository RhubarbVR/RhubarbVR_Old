using System;
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
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;

namespace RhubarbEngine.Managers
{
    public class WorldManager : IManager
    {
        public Engine engine;

        public List<World.World> worlds = new List<World.World>();

        public World.World privateOverlay;

        public World.World localWorld;

        public World.World focusedWorld;

        public bool dontSaveLocal = false;
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


            engine.logger.Log("Starting Local World");
            if(File.Exists(engine.dataPath + "/LocalWorld.RWorld"))
            {
                try
                {
                    localWorld = loadWorldFromBytes(File.ReadAllBytes(engine.dataPath + "/LocalWorld.RWorld"));
                }
                catch(Exception e)
                {
                    dontSaveLocal = true;
                    Logger.Log("Failed To load LocalWorld" + e.ToString(), true);
                    localWorld = new World.World(this, "TempLoaclWorld", 16);
                }
            }
            else
            {
                localWorld = new World.World(this, "LocalWorld", 16);

                // Attach random stuff here
                Entity e = localWorld.RootEntity.addChild();
                StaicMainShader shader = e.attachComponent<StaicMainShader>();
                DiscMesh bmesh = e.attachComponent<DiscMesh>();
                RMaterial mit = e.attachComponent<RMaterial>();
                MeshRender meshRender = e.attachComponent<MeshRender>();
                RGBRainbowDriver rgbainbowDriver = e.attachComponent<RGBRainbowDriver>();

                mit.Shader.target = shader;
                meshRender.Materials.Add().target = mit;
                meshRender.Mesh.target = bmesh;
                mit.setValueAtField("rambow", Render.Shader.ShaderType.MainFrag, Colorf.Blue);
                Render.Material.Fields.ColorField field = mit.getField<Render.Material.Fields.ColorField>("rambow", Render.Shader.ShaderType.MainFrag);
                rgbainbowDriver.driver.setDriveTarget(field.field);
                rgbainbowDriver.speed.value = 50f;
            }
            localWorld.Focus = World.World.FocusLevel.Focused;
            worlds.Add(localWorld);
            focusedWorld = localWorld;
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
            if (engine.engineInitializer == null && !dontSaveLocal)
            {
                File.WriteAllBytes(engine.dataPath + "/LocalWorld.RWorld", worldToBytes(localWorld));
            }
        }
    }
}
