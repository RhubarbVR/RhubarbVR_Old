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
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Audio;
using RhubarbEngine.Components.Physics;

using Org.OpenAPITools.Model;
using BulletSharp;
using System.Numerics;
using System.Net;
using RhubarbEngine.Components.Interaction;
using DiscordRPC;

namespace RhubarbEngine.Managers
{
    public class WorldManager : IManager
    {
        public Engine engine;

        public SynchronizedCollection<World.World> worlds = new SynchronizedCollection<World.World>();

        public World.World privateOverlay;

        public World.World localWorld;

        private World.World _focusedWorld;

        public World.World focusedWorld { get { return _focusedWorld; } set { _focusedWorld = value; focusedWorldChange(); } }

        public PersonalSpace personalSpace;

        public bool dontSaveLocal = false;
        
        public void updateDiscord()
        {
            if (focusedWorld == localWorld)
            {
                engine.discordRpcClient.SetPresence(new RichPresence()
                {
                    Timestamps = new Timestamps(focusedWorld.startTime),
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
            else if ((int)focusedWorld.accessLevel.value >= 3)
            {
                engine.discordRpcClient.SetPresence(new RichPresence()
                {
                    Timestamps = new Timestamps(focusedWorld.startTime),
                    Details = "The Engine",
                    State = " in " + focusedWorld.Name.value,
                    Secrets = new Secrets()
                    {
                        JoinSecret = "rhubarb://" + focusedWorld.SessionID.value + "/",
                        MatchSecret = "rhubarb://" + focusedWorld.SessionID.value + "/",
                    },
                    Assets = new Assets()
                    {
                        LargeImageKey = "rhubarbvr",
                        LargeImageText = "Faolan Says HI",
                        SmallImageKey = "rhubarbvr2"
                    },
                    Party = new Party()
                    {
                        Privacy = Party.PrivacySetting.Public,
                        ID = focusedWorld.SessionID.value,
                        Max = focusedWorld.maxUsers,
                        Size = focusedWorld.users.Count(),
                    }
                });
            }
            else
            {
                engine.discordRpcClient.SetPresence(new RichPresence()
                {
                    Timestamps = new Timestamps(focusedWorld.startTime),
                    Details = "The Engine",
                    State = $" in {focusedWorld.accessLevel.ToString()} World",
                    Assets = new Assets()
                    {
                        LargeImageKey = "rhubarbvr",
                        LargeImageText = "Faolan Says HI",
                        SmallImageKey = "rhubarbvr2"
                    },
                    Party = new Party()
                    {
                        Privacy = Party.PrivacySetting.Private,
                        ID = focusedWorld.SessionID.value,
                        Max = focusedWorld.maxUsers,
                        Size = focusedWorld.users.Count(),
                    }
                });
            }
        }

        public void focusedWorldChange()
        {
            updateDiscord();
        }

        public void addToRenderQueue(RenderQueue gu, RemderLayers layer, RhubarbEngine.Utilities.BoundingFrustum frustum)
        {
            try {
                Parallel.ForEach(worlds,world =>
                {
                    if (world.Focus != World.World.FocusLevel.Background)
                    {
                        world.addToRenderQueue(gu, layer, frustum);
                    }
                });
            }
            catch
            {

            }
        }

        public void createNewWorld(AccessLevel accessLevel, SessionsType sessionsType, string name, string worlduuid, bool isOver, int maxusers, bool mobilefriendly,string templet = null, bool focus = true)
        {
            Logger.Log("Creating world", true);
            World.World world = new World.World(this, sessionsType, accessLevel, name, maxusers, worlduuid, isOver, mobilefriendly, templet);
            string conectionkey = world.netModule.token;
            Logger.Log("Creating world ConectionKey:" + conectionkey);
            try
            {
                string sessionid = engine.netApiManager.sessionApi.SessionCreatesessionPost(new CreateSessionReq(name, worlduuid, new List<string>(new[] { "" }), "", (int)sessionsType, (int)accessLevel, isOver, maxusers, mobilefriendly, conectionkey), engine.netApiManager.token);
                world.SessionID.value = sessionid;
                world.loadHostUser();
                worlds.SafeAdd(world);
                if (focus)
                {
                    world.Focus = World.World.FocusLevel.Focused;
                }
                Logger.Log("World Created sessionID: "+sessionid, true);

            }
            catch (Exception e)
            {
                Logger.Log("Create World Error"+e.ToString(), true);
            }

        }

        public void JoinSessionFromUUID(string uuid, bool focus = true)
        {
            try
            {
                Logger.Log("Joining session ID: " + uuid, true);
                World.World world = new World.World(this, "Loading", 1,false,false,null,true);
                string conectionkey = world.netModule.token;
                Logger.Log("Joining session ConectionKey:" + conectionkey);
                var join = engine.netApiManager.sessionApi.SessionJoinsessionGet(uuid, conectionkey, engine.netApiManager.token);
                world.SessionID.value = join.Uuid;
                world.joinsession(join,conectionkey);
                worlds.Add(world);
            if (focus)
            {
                world.Focus = World.World.FocusLevel.Focused;
            }
        }
            catch (Exception e)
            {
                Logger.Log("Create World Error"+e.ToString(), true);
            }
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
            privateOverlay = new World.World(this, "Private Overlay", 1,true);
            personalSpace = privateOverlay.RootEntity.attachComponent<PersonalSpace>();
            worlds.Add(privateOverlay);
            privateOverlay.Focus = World.World.FocusLevel.PrivateOverlay;

            Task.Run(() =>
            {
                engine.logger.Log("Starting Local World");
                if (File.Exists(engine.dataPath + "/LocalWorld.RWorld"))
                {
                    try
                    {
                        DataNodeGroup node = new DataNodeGroup(File.ReadAllBytes(engine.dataPath + "/LocalWorld.RWorld"));
                        localWorld = new World.World(this, "LocalWorld", 16, false, true, node);
                    }
                    catch (Exception e)
                    {
                        dontSaveLocal = true;
                        Logger.Log("Failed To load LocalWorld" + e.ToString(), true);
                        localWorld = new World.World(this, "TempLoaclWorld", 16, false, true);
                        BuildLocalWorld(localWorld);
                    }
                }
                else
                {
                    localWorld = new World.World(this, "LocalWorld", 16, false, true);
                    BuildLocalWorld(localWorld);
                }
                localWorld.Focus = World.World.FocusLevel.Focused;
                worlds.Add(localWorld);
                focusedWorld = localWorld;
                if (engine.engineInitializer.session != null)
                {
                    JoinSessionFromUUID(engine.engineInitializer.session, true);
                }
                else
                {
                    //   createNewWorld(AccessLevel.Anyone, SessionsType.Casual, "Faolan World", "", false, 16, false, "Basic");
                }
            });
            return this;
        }


        public ImGUICanvas buildUI(Entity e)
        {

            BasicUnlitShader shader = e.world.staticAssets.basicUnlitShader;
            PlaneMesh bmesh = e.attachComponent<PlaneMesh>();
            InputPlane bmeshcol = e.attachComponent<InputPlane>();
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            ImGUICanvas imGUICanvas = e.attachComponent<ImGUICanvas>();
            ImGUIInputText imGUIText = e.attachComponent<ImGUIInputText>();
            imGUICanvas.imputPlane.target = bmeshcol;
            imGUICanvas.element.target = imGUIText;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
            return imGUICanvas;
        }

        public WebBrowser buildWebBrowser(Entity e, Vector2u pixsize, Vector2f size, bool globalAudio = false)
        {
            BasicUnlitShader shader = e.world.staticAssets.basicUnlitShader;
            PlaneMesh bmesh = e.attachComponent<PlaneMesh>();
            InputPlane bmeshcol = e.attachComponent<InputPlane>();
            bmesh.Width.value = size.x;
            bmesh.Height.value = size.y;
            bmeshcol.size.value = size / 2;
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            WebBrowser imGUICanvas = e.attachComponent<WebBrowser>();
            bmeshcol.pixelSize.value = imGUICanvas.scale.value = pixsize;
            imGUICanvas.imputPlane.target = bmeshcol;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
            if (!globalAudio)
            {
                var audio = e.attachComponent<AudioOutput>();
                audio.audioSource.target = imGUICanvas;
                imGUICanvas.globalAudio.value = false;
            }
            else
            {
                imGUICanvas.globalAudio.value = true;
            }
            return imGUICanvas;
        }

        public void BuildLocalWorld(World.World world)
        {
            world.RootEntity.attachComponent<SimpleSpawn>();
            Entity floor = world.RootEntity.addChild("Floor");
            var mit = floor.attachComponent<RMaterial>();
            mit.Shader.target = world.staticAssets.tilledUnlitShader;
            var planemesh = floor.attachComponent<PlaneMesh>();
            var planecol = floor.attachComponent<BoxCollider>();
            planemesh.Width.value = 1000f;
            planemesh.Height.value = 1000f;
            planecol.boxExtents.value = new Vector3f(planemesh.Width.value, 0.01f, planemesh.Height.value);
            var meshRender = floor.attachComponent<MeshRender>();
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = planemesh;
            Render.Material.Fields.Vec2Field tilefield = mit.getField<Render.Material.Fields.Vec2Field>("Tile", Render.Shader.ShaderType.MainFrag);
            tilefield.field.value = new Vector2f(500, 500);

            GridTextue2D textue2DF = floor.attachComponent<GridTextue2D>();
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = textue2DF;

            Entity webBrowser = world.RootEntity.addChild("Floor");
            webBrowser.position.value = new Vector3f(0, 4.3, -5);
            var browser= buildWebBrowser(webBrowser, (new Vector2u(1920, 1080))/2, new Vector2f(16 / 2, 9 / 2));


            AttachSpiningCubes(world.RootEntity.addChild("Cubes"), browser);




        }

        public void AttachSpiningCubes(Entity root,AssetProvider<RTexture2D> textue2D)
        {
            var speed = 0.5f;
            var group1 = root.addChild("group1");
            group1.attachComponent<Spinner>().speed.value = new Vector3f(speed, 0, 0);
            var group2 = root.addChild("group2");
            group2.attachComponent<Spinner>().speed.value = new Vector3f(0, speed, 0);
            var group3 = root.addChild("group3");
            group3.attachComponent<Spinner>().speed.value = new Vector3f(0, 0, speed/2);
            var group4 = root.addChild("group4");
            group4.attachComponent<Spinner>().speed.value = new Vector3f(speed, speed, 0);
            var group5 = root.addChild("group5");
            group5.attachComponent<Spinner>().speed.value = new Vector3f(speed/2, speed, speed);
            var group6 = root.addChild("group6");
            group6.attachComponent<Spinner>().speed.value = new Vector3f(speed, 0, speed/2);
            var group11 = root.addChild("group1");
            group11.attachComponent<Spinner>().speed.value = new Vector3f(-speed, 0, 0);
            var group21 = root.addChild("group2");
            group21.attachComponent<Spinner>().speed.value = new Vector3f(0, -speed, 0);
            var group31 = root.addChild("group3");
            group31.attachComponent<Spinner>().speed.value = new Vector3f(0, 0, -speed);
            var group41 = root.addChild("group4");
            group41.attachComponent<Spinner>().speed.value = new Vector3f(-speed, -speed/2, 0);
            var group51 = root.addChild("group5");
            group51.attachComponent<Spinner>().speed.value = new Vector3f(-speed/2, -speed, -speed);
            var group61 = root.addChild("group6");
            group61.attachComponent<Spinner>().speed.value = new Vector3f(-speed, 0, -speed);

            
            BasicUnlitShader shader = root.world.staticAssets.basicUnlitShader;
            BoxMesh bmesh = root.attachComponent<BoxMesh>();
            RMaterial mit = root.attachComponent<RMaterial>();
            mit.Shader.target = shader;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = textue2D;
            BuildGroup(bmesh, mit, group1);
            BuildGroup(bmesh, mit, group2);
            BuildGroup(bmesh, mit, group3);
            BuildGroup(bmesh, mit, group4);
            BuildGroup(bmesh, mit, group5);
            BuildGroup(bmesh, mit, group6);
            BuildGroup(bmesh, mit, group11);
            BuildGroup(bmesh, mit, group21);
            BuildGroup(bmesh, mit, group31);
            BuildGroup(bmesh, mit, group41);
            BuildGroup(bmesh, mit, group51);
            BuildGroup(bmesh, mit, group61);

        }
        static Random random = new Random();
        static float NextFloat()
        {
            var buffer = new byte[4];
            random.NextBytes(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }
        public void BuildGroup(BoxMesh bmesh, RMaterial mit, Entity entity)
        {
            for (int i = 0; i < 6; i++)
            {
                var cubeholder = entity.addChild("CubeHolder");
                cubeholder.rotation.value = Quaternionf.CreateFromEuler(NextFloat(), NextFloat(), NextFloat());
                var cube = cubeholder.addChild("Cube");
                cube.position.value = new Vector3f(0, 15, 0);
                cube.scale.value = new Vector3f(0.5f);
                attachRender(bmesh, mit, cube);
            }
        }

        public void attachRender(BoxMesh bmesh, RMaterial mit,Entity entity)
        {
            MeshRender meshRender = entity.attachComponent<MeshRender>();
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
        }


        public Entity AddMesh<T>(Entity ea) where T: ProceduralMesh
        {
            Entity e = ea.addChild();
            BasicUnlitShader shader = e.world.staticAssets.basicUnlitShader;
            T bmesh = e.attachComponent<T>();
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            Textue2DFromUrl textue2DFromUrl = e.attachComponent<Textue2DFromUrl>();
            
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = textue2DFromUrl;


            return e;
        }


        public void Update(DateTime startTime, DateTime Frame)
        {
            try
            {
                foreach (World.World world in worlds)
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
            if (engine.engineInitializer == null && !dontSaveLocal)
            {
                File.WriteAllBytes(engine.dataPath + "/LocalWorld.RWorld", worldToBytes(localWorld));
            }
        }
    }
}
