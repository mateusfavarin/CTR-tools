﻿using CTRFramework;
using CTRFramework.Big;
using CTRFramework.Shared;
using CTRFramework.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ctrviewer
{
    public class Game1 : Game
    {
        public static List<string> alphalist = new List<string>();

        EngineSettings settings;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        public static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public static Dictionary<string, TriList> instTris = new Dictionary<string, TriList>();
        public static Dictionary<string, QuadList> instmodels = new Dictionary<string, QuadList>();

        List<InstancedModel> instanced = new List<InstancedModel>();
        List<InstancedModel> paths = new List<InstancedModel>();

        List<Kart> karts = new List<Kart>();


        List<VertexPositionColorTexture[]> bbox = new List<VertexPositionColorTexture[]>();

        Menu menu;

        //effects
        BasicEffect effect;
        BasicEffect instanceEffect;

        //cameras
        FirstPersonCamera camera;
        FirstPersonCamera rightCamera;
        FirstPersonCamera lowcamera;
        FirstPersonCamera skycamera;

        //ctr scenes
        List<Scene> scn = new List<Scene>();

        //hi and low scenes converted to monogame
        List<MGLevel> MeshHigh = new List<MGLevel>();
        List<MGLevel> MeshLow = new List<MGLevel>();

        //sky
        MGLevel sky;
        Color backColor = Color.Blue;


        public static PlayerIndex activeGamePad = PlayerIndex.One;


        //meh
        public static int currentflag = 1;

        //get version only once, because we don't want this to be allocated every frame.
        public static string version = Meta.GetVersionInfo();


        public Game1()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            graphics.HardwareModeSwitch = false;

            settings = EngineSettings.Load();
            settings.onWindowedChanged = SwitchDisplayMode;
            settings.onVertexLightingChanged = UpdateEffects;
            settings.onAntiAliasChanged = UpdateAntiAlias;
            settings.onVerticalSyncChanged = UpdateVSync;
            settings.onFieldOfViewChanged = UpdateFOV;
        }

        public void SwitchDisplayMode()
        {
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            if (settings.Windowed)
            {
                graphics.PreferredBackBufferWidth = graphics.PreferredBackBufferWidth * settings.WindowScale / 100;
                graphics.PreferredBackBufferHeight = graphics.PreferredBackBufferHeight * settings.WindowScale / 100;
            }

            UpdateSplitscreenViewports();

            graphics.IsFullScreen = !settings.Windowed;
            graphics.ApplyChanges();
        }

        public Viewport vpFull;
        public Viewport vpLeft;
        public Viewport vpRight;
        public Viewport vpTop;
        public Viewport vpBottom;

        public void UpdateSplitscreenViewports()
        {
            vpFull.MaxDepth = graphics.GraphicsDevice.Viewport.MaxDepth;
            vpFull.MinDepth = graphics.GraphicsDevice.Viewport.MinDepth;
            vpFull.Width = graphics.PreferredBackBufferWidth;
            vpFull.Height = graphics.PreferredBackBufferHeight;
            vpFull.X = 0;
            vpFull.Y = 0;

            vpLeft.MaxDepth = graphics.GraphicsDevice.Viewport.MaxDepth;
            vpLeft.MinDepth = graphics.GraphicsDevice.Viewport.MinDepth;
            vpLeft.Width = graphics.PreferredBackBufferWidth / 2;
            vpLeft.Height = graphics.PreferredBackBufferHeight;
            vpLeft.X = 0;
            vpLeft.Y = 0;

            vpRight.MaxDepth = graphics.GraphicsDevice.Viewport.MaxDepth;
            vpRight.MinDepth = graphics.GraphicsDevice.Viewport.MinDepth;
            vpRight.Width = graphics.PreferredBackBufferWidth / 2;
            vpRight.Height = graphics.PreferredBackBufferHeight;
            vpRight.X = graphics.PreferredBackBufferWidth / 2;
            vpRight.Y = 0;

            vpTop.MaxDepth = graphics.GraphicsDevice.Viewport.MaxDepth;
            vpTop.MinDepth = graphics.GraphicsDevice.Viewport.MinDepth;
            vpTop.Width = graphics.PreferredBackBufferWidth;
            vpTop.Height = graphics.PreferredBackBufferHeight / 2;
            vpTop.X = 0;
            vpTop.Y = 0;

            vpBottom.MaxDepth = graphics.GraphicsDevice.Viewport.MaxDepth;
            vpBottom.MinDepth = graphics.GraphicsDevice.Viewport.MinDepth;
            vpBottom.Width = graphics.PreferredBackBufferWidth;
            vpBottom.Height = graphics.PreferredBackBufferHeight / 2;
            vpBottom.X = 0;
            vpBottom.Y = graphics.PreferredBackBufferHeight / 2;
        }

        AlphaTestEffect alphaTestEffect;

        public void UpdateEffects()
        {
            effect = new BasicEffect(graphics.GraphicsDevice);
            effect.VertexColorEnabled = settings.VertexLighting;
            effect.TextureEnabled = true;
            effect.DiffuseColor = new Vector3(settings.VertexLighting ? 2 : 1);

            alphaTestEffect = new AlphaTestEffect(GraphicsDevice);
            alphaTestEffect.AlphaFunction = CompareFunction.Greater;
            alphaTestEffect.ReferenceAlpha = 0;
            alphaTestEffect.VertexColorEnabled = settings.VertexLighting;
            alphaTestEffect.DiffuseColor = effect.DiffuseColor;


            effect.FogEnabled = true;
            effect.FogColor = new Vector3(backColor.R / 255f, backColor.G / 255f, backColor.B / 255f);
            effect.FogStart = camera.FarClip / 4 * 3;
            effect.FogEnd = camera.FarClip;

            //effect.DiffuseColor = new Vector3(0.5f, 1.0f, 1.5f);

            instanceEffect = new BasicEffect(graphics.GraphicsDevice);
            instanceEffect.VertexColorEnabled = true;
            instanceEffect.TextureEnabled = false;
        }

        public void UpdateVSync()
        {
            graphics.SynchronizeWithVerticalRetrace = settings.VerticalSync;
            IsFixedTimeStep = settings.VerticalSync;
            graphics.ApplyChanges();
        }

        public void UpdateAntiAlias()
        {
            graphics.PreferMultiSampling = !graphics.PreferMultiSampling;
            graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = settings.AntiAliasLevel;
        }

        public void UpdateFOV()
        {
            camera.ViewAngle = settings.FieldOfView;
            lowcamera.ViewAngle = settings.FieldOfView;
            skycamera.ViewAngle = settings.FieldOfView;
            rightCamera.ViewAngle = settings.FieldOfView;
        }

        protected override void Initialize()
        {
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            UpdateAntiAlias();
            UpdateVSync();
            graphics.ApplyChanges();

            IsMouseVisible = false;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            camera = new FirstPersonCamera(this);
            rightCamera = new FirstPersonCamera(this);
            lowcamera = new FirstPersonCamera(this);
            skycamera = new FirstPersonCamera(this);

            UpdateEffects();

            DisableLodCamera();

            for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                GamePadState state = GamePad.GetState(i);
                if (state.IsConnected)
                {
                    activeGamePad = i;
                    break;
                }
            }

            Samplers.Refresh();
            Samplers.InitRasterizers();

            SwitchDisplayMode();

            base.Initialize();
        }


        private void EnableLodCamera()
        {
            lodEnabled = true;
            /*
            camera.NearClip = 1f;
            camera.FarClip = 10000f;
            lowcamera.NearClip = 9000f;
            lowcamera.FarClip = 50000f;
            */
            lowcamera.NearClip = 1f;
            lowcamera.FarClip = 100000f;
            camera.NearClip = 1f;
            camera.FarClip = 2f;

            camera.Update(null);
            lowcamera.Update(null);
        }

        private void DisableLodCamera()
        {
            lodEnabled = false;
            camera.NearClip = 1f;
            camera.FarClip = 100000f;
            lowcamera.NearClip = 1f;
            lowcamera.FarClip = 2f;
            camera.Update(null);
            lowcamera.Update(null);
        }


        void LoadGenericTextures()
        {
            textures.Add("test", Content.Load<Texture2D>("test"));
            textures.Add("flag", Content.Load<Texture2D>("flag"));

            if ((DateTime.Now.Month == 12 && DateTime.Now.Day >= 20) || (DateTime.Now.Month == 1 && DateTime.Now.Day <= 7))
            {
                textures.Add("logo", Content.Load<Texture2D>("logo_xmas"));
            }
            else
            {
                textures.Add("logo", Content.Load<Texture2D>("logo"));
            }
        }


        Texture2D tint;

        protected override void LoadContent()
        {
            LoadGenericTextures();

            effect.Texture = textures["test"];
            //effect.TextureEnabled = true;

            font = Content.Load<SpriteFont>("File");

            tint = new Texture2D(GraphicsDevice, 1, 1);
            tint.SetData(new Color[] { Color.Black });

            menu = new Menu(font);
            //graphics.GraphicsDevice.Viewport.Height = 2;

            UpdateSplitscreenViewports();

            AddCone("greencone", Color.Green);
            AddCone("yellowcone", Color.Yellow);
            AddCone("redcone", Color.Red);
            AddCone("purplecone", Color.Purple);
            AddCone("cyancone", Color.Cyan);
            AddCone("limecone", Color.Lime);
            AddCone("goldcone", Color.Gold);
            AddCone("bluecone", Color.Blue);
            AddCone("browncone", Color.Brown);
        }



        public void AddCone(string name, Color c)
        {
            QuadList modl = new QuadList();

            List<VertexPositionColorTexture> vptc = new List<VertexPositionColorTexture>();

            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), MGConverter.Blend(Color.Black, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            modl.PushQuad(vptc);

            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), MGConverter.Blend(Color.Black, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            modl.PushQuad(vptc);

            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            modl.PushQuad(vptc);

            modl.Seal();

            instmodels.Add(name, modl);
        }


        bool gameLoaded = false;

        private void LoadStuff(string[] lev)
        {
            gameLoaded = false;

            LoadLevel(lev);
            ResetCamera();

            gameLoaded = true;
        }

        private void LoadTextures()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (Scene s in scn)
            {
                foreach (var t in s.ctrvram.textures)
                {
                    //first look for texture replacement
                    string path = $".\\levels\\newtex\\{t.Key}.png";

                    bool alpha = false;

                    if (File.Exists(path))
                    {
                        if (!textures.ContainsKey(t.Key))
                        {
                            textures.Add(t.Key, settings.GenerateMips ? MipHelper.LoadTextureFromFile(GraphicsDevice, path, out alpha) : Texture2D.FromFile(GraphicsDevice, path));
                            continue;
                        }
                    }

                    if (!textures.ContainsKey(t.Key))
                        textures.Add(t.Key, settings.GenerateMips ? MipHelper.LoadTextureFromBitmap(GraphicsDevice, t.Value, out alpha) : MipHelper.GetTexture2DFromBitmap(GraphicsDevice, t.Value, out alpha, mipmaps: false));

                    if (alpha)
                        if (!alphalist.Contains(t.Key))
                            alphalist.Add(t.Key);
                }
            }



                /*
            foreach (string s in qb.textureList)
            {
                string path = $"levels\\tex\\{s}.png";
                string path_new = $"levels\\newtex\\{s}.png";

                if (File.Exists(path_new))
                    path = path_new;

                if (File.Exists(path))
                {
                    if (!textures.ContainsKey(s))
                        textures.Add(s, settings.GenerateMips ? MipHelper.LoadTextureFromFile(GraphicsDevice, path) : Texture2D.FromFile(GraphicsDevice, path));
                }
                else Console.WriteLine("Missing texture: " + s);
            }
            */

            sw.Stop();

            Console.WriteLine($"Loaded textures in {sw.Elapsed.TotalSeconds}");
        }


        private void LoadLevel(string[] lev)
        {
            if (lev == null)
                lev = new string[] { };

            Dispose();
            LoadGenericTextures(); //making sure we have default textures loaded. maybe should just allocate statically?

            if (File.Exists("karts.lev"))
            {
                Scene karts = Scene.FromFile("karts.lev");

                foreach (DynamicModel m in karts.dynamics)
                {
                    if (!instTris.ContainsKey(m.Name) && m.Name == "selectkart")
                    {
                        List<VertexPositionColorTexture> li = new List<VertexPositionColorTexture>();

                        foreach (var x in m.headers[0].verts)
                            li.Add(MGConverter.ToVptc(x, new Vector2b(0, 0)));

                        TriList t = new TriList();
                        t.textureEnabled = false;
                        t.textureName = "test";
                        t.scrollingEnabled = false;
                        t.PushTri(li);
                        t.Seal();

                        instTris.Add(m.Name, t);
                    }

                }
            }

            RenderEnabled = false;

            //wait for the end of frame, in case we are still rendering.
            while (IsDrawing) { };

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("LoadLevel()");

            string[] files = new string[] { };

            if (lev.Length == 0)
            {
                if (Directory.Exists(@"levels\"))
                    files = Directory.GetFiles(@"levels\", "*.lev");
            }
            else
            {
                files = lev;
            }

            if (files.Length == 0)
            {
                Console.WriteLine("no files");
                return;
            }

            foreach (string s in files)
            {
                scn.Add(Scene.FromFile(s));
            }

            Console.WriteLine("scenes parsed at: " + sw.Elapsed.TotalSeconds);

            //loading textures between scenes and conversion to monogame for alpha textures info
            LoadTextures();

            foreach (Scene s in scn)
            {
                MeshHigh.Add(new MGLevel(s, Detail.Med));
                MeshLow.Add(new MGLevel(s, Detail.Low));
            }

            Console.WriteLine("converted scenes to monogame render at: " + sw.Elapsed.TotalSeconds);

            //force 1st scene sky and back color
            if (scn.Count > 0)
            {
                backColor = MGConverter.ToColor(scn[0].header.backColor);
                if (scn[0].skybox != null)
                    sky = new MGLevel(scn[0].skybox);
            }

            foreach (Scene s in scn)
            {
                if (s.unkadv != null)
                {
                    foreach (PosAng pa in s.unkadv.smth)
                        instanced.Add(new InstancedModel("limecone", new Vector3(pa.Position.X, pa.Position.Y, pa.Position.Z), Vector3.Zero, 3));
                }

                if (s.header.ptru2 != 0)
                {
                    foreach (Vector3s v in s.posu2)
                    {
                        instanced.Add(new InstancedModel("goldcone", new Vector3(v.X, v.Y, v.Z), Vector3.Zero, 3));
                    }
                }

                if (s.header.ptrTrialData != 0)
                {
                    foreach (PosAng v in s.posu1)
                    {
                        instanced.Add(new InstancedModel("browncone", new Vector3(v.Position.X, v.Position.Y, v.Position.Z), Vector3.Zero, 30));
                    }
                }
            }


            foreach (Scene s in scn)
                foreach (DynamicModel m in s.dynamics)
                {
                    if (!instTris.ContainsKey(m.Name))
                    {
                        List<VertexPositionColorTexture> li = new List<VertexPositionColorTexture>();

                        foreach (var x in m.headers[0].verts)
                            li.Add(MGConverter.ToVptc(x, new Vector2b(0, 0)));

                        TriList t = new TriList();
                        t.textureEnabled = false;
                        t.textureName = "test";
                        t.scrollingEnabled = false;
                        t.PushTri(li);
                        t.Seal();

                        instTris.Add(m.Name, t);
                    }

                }

            karts.Add(new Kart("selectkart", MGConverter.ToVector3(scn[0].header.startGrid[0].Position), Vector3.Left, 0.5f));


            Console.WriteLine("extracted dynamics at: " + sw.Elapsed.TotalSeconds);

            foreach (Scene s in scn)
            {
                foreach (PosAng pa in s.header.startGrid)
                    instanced.Add(new InstancedModel("purplecone", new Vector3(pa.Position.X, pa.Position.Y, pa.Position.Z), Vector3.Zero, 3));

                foreach (PickupHeader ph in s.pickups)
                    instanced.Add(new InstancedModel(
                        ph.ModelName,
                        new Vector3(ph.Position.X, ph.Position.Y, ph.Position.Z),
                        new Vector3((float)(ph.Angle.X / 4094f * Math.PI * 2), (float)(ph.Angle.Y / 4094f * Math.PI * 2), (float)(ph.Angle.Z / 4094f * Math.PI * 2)),
                        1));

                foreach (PosAng n in s.restartPts)
                    paths.Add(new InstancedModel("cyancone", new Vector3(n.Position.X, n.Position.Y, n.Position.Z), Vector3.Zero, 3));

                if (s.nav.paths.Count == 3)
                {
                    foreach (NavFrame n in s.nav.paths[0].frames)
                        paths.Add(new InstancedModel("greencone", new Vector3(n.position.X, n.position.Y, n.position.Z), Vector3.Zero, 3));
                    foreach (NavFrame n in s.nav.paths[1].frames)
                        paths.Add(new InstancedModel("yellowcone", new Vector3(n.position.X, n.position.Y, n.position.Z), Vector3.Zero, 3));
                    foreach (NavFrame n in s.nav.paths[2].frames)
                        paths.Add(new InstancedModel("redcone", new Vector3(n.position.X, n.position.Y, n.position.Z), Vector3.Zero, 3));
                }
            }



            //foreach (Scene s in scn)
            //    s.ExportTexturesAll(Path.Combine(Meta.BasePath, "levels\\tex"));


            Console.WriteLine("textures extracted at: " + sw.Elapsed.TotalSeconds);

            //files = Directory.GetFiles("tex", "*.png");

            foreach (Scene s in scn)
            {
                foreach (var b in s.visdata)
                {
                    bbox.Add(MGConverter.ToLineList(b.bbox));
                }
            }

            sw.Stop();

            Console.WriteLine("textures loaded. level done: " + sw.Elapsed.TotalSeconds);

            UpdateEffects();

            RenderEnabled = true;
        }

        public void ResetCamera()
        {
            if (scn.Count > 0)
            {
                camera.Position = MGConverter.ToVector3(scn[0].header.startGrid[0].Position);
                lowcamera.Position = camera.Position;
                rightCamera.Position = camera.Position;

                camera.SetRotation((float)(scn[0].header.startGrid[0].Angle.X / 4096 * Math.PI * 2), (float)(scn[0].header.startGrid[0].Angle.Z / 4096 * Math.PI * 2));
                rightCamera.SetRotation((float)(scn[0].header.startGrid[0].Angle.X / 4096 * Math.PI * 2), (float)(scn[0].header.startGrid[0].Angle.Z / 4096 * Math.PI * 2));
                lowcamera.SetRotation((float)(scn[0].header.startGrid[0].Angle.X / 4096 * Math.PI * 2), (float)(scn[0].header.startGrid[0].Angle.Z / 4096 * Math.PI * 2));
                skycamera.SetRotation((float)(scn[0].header.startGrid[0].Angle.X / 4096 * Math.PI * 2), (float)(scn[0].header.startGrid[0].Angle.Z / 4096 * Math.PI * 2));

                UpdateCameras(new GameTime());

                Console.WriteLine(scn[0].header.startGrid[0].Angle.ToString());
            }
        }

        protected override void UnloadContent()
        {
        }

        public bool updatemouse = false;
        public static bool InMenu = false;
        public static bool HideInvisible = true;
        public static bool RenderEnabled = true;
        public static bool ControlsEnabled = true;
        public static bool IsDrawing = false;

        public bool lodEnabled = false;

        GamePadState oldstate = GamePad.GetState(activeGamePad);
        GamePadState newstate = GamePad.GetState(activeGamePad);

        KeyboardState oldkb = new KeyboardState();
        KeyboardState newkb = new KeyboardState();

        protected override void Update(GameTime gameTime)
        {
            if (loading == null)
                LoadGame();

            //x += 0.01f ;
            //if (x > Math.PI * 2)
            //    x = 0;
            //camera.SetRotation(x, y);
            //Console.WriteLine(x);

            if (IsActive)
            {
                newstate = GamePad.GetState(activeGamePad);
                newkb = Keyboard.GetState();


                foreach (Kart k in karts)
                    k.Update(gameTime);

                if (newstate.Buttons.Start == ButtonState.Pressed && newstate.Buttons.Back == ButtonState.Pressed)
                    Exit();

                if (settings.StereoPair)
                {
                    if (newstate.IsButtonDown(Buttons.RightShoulder))
                        settings.StereoPairSeparation += 10;

                    if (newstate.IsButtonDown(Buttons.LeftShoulder))
                        settings.StereoPairSeparation -= 10;

                    if (newstate.IsButtonDown(Buttons.RightShoulder) && newstate.IsButtonDown(Buttons.LeftShoulder))
                        settings.StereoPairSeparation = -260;
                }

                if ((newkb.IsKeyDown(Keys.Enter) && newkb.IsKeyDown(Keys.RightAlt)) && !(oldkb.IsKeyDown(Keys.Enter) && newkb.IsKeyDown(Keys.RightAlt)))
                {
                    settings.Windowed = !settings.Windowed;
                }


                if (Keyboard.GetState().IsKeyDown(Keys.OemMinus)) settings.FieldOfView--;
                if (Keyboard.GetState().IsKeyDown(Keys.OemPlus)) settings.FieldOfView++;

                if ((newstate.Buttons.Start == ButtonState.Pressed && oldstate.Buttons.Start != newstate.Buttons.Start) ||
                    (newkb.IsKeyDown(Keys.Escape) && newkb.IsKeyDown(Keys.Escape) != oldkb.IsKeyDown(Keys.Escape)))
                {
                    InMenu = !InMenu;
                }

                if (InMenu)
                {
                    menu.Update(oldstate, newstate, oldkb, newkb);

                    //currentflag = menu.items.Find(x => x.Title == "current flag: {0}").rangeval;

                    if (menu.Exec)
                    {
                        switch (menu.SelectedItem.Action)
                        {
                            case "close":
                                InMenu = false;
                                break;
                            case "load":
                                LoadGame();
                                InMenu = false;
                                break;
                            case "loadbig":
                                LoadLevelFromBig(menu.SelectedItem.Value, 0, 2);
                                break;
                            case "loadbigadv":
                                LoadLevelFromBig(menu.SelectedItem.Value, 0, 3);
                                break;
                            case "link":
                                menu.SetMenu(font);
                                break;
                            case "toggle":
                                switch (menu.SelectedItem.Param)
                                {
                                    case "inst": settings.Models = !settings.Models; break;
                                    case "paths": settings.BotsPath = !settings.BotsPath; break;
                                    case "lod": lodEnabled = !lodEnabled; if (lodEnabled) EnableLodCamera(); else DisableLodCamera(); break;
                                    case "antialias": settings.AntiAlias = !settings.AntiAlias; break;
                                    case "invis": HideInvisible = !HideInvisible; break;
                                    case "campos": settings.ShowCamPos = !settings.ShowCamPos; break;
                                    case "visbox": settings.VisData = !settings.VisData; break;
                                    case "filter": Samplers.EnableFiltering = !Samplers.EnableFiltering; Samplers.Refresh(); break;
                                    case "wire": Samplers.EnableWireframe = !Samplers.EnableWireframe; break;
                                    case "genmips": settings.GenerateMips = !settings.GenerateMips; break;
                                    case "window": settings.Windowed = !settings.Windowed; break;
                                    case "vcolor": settings.VertexLighting = !settings.VertexLighting; break;
                                    case "stereo": settings.StereoPair = !settings.StereoPair; break;
                                    case "sky": settings.Sky = !settings.Sky; break;
                                    case "vsync": settings.VerticalSync = !settings.VerticalSync;  break;
                                    default: Console.WriteLine("unimplemented toggle: " + menu.SelectedItem.Param); break;
                                }
                                break;

                            case "exit":
                                Exit();
                                break;
                        }

                        menu.Exec = !menu.Exec;
                    }

                    if (newstate.Buttons.B == ButtonState.Pressed && newstate.Buttons.B != oldstate.Buttons.B)
                    {
                        bool togglemenu = true;

                        foreach (MenuItem m in menu.items)
                        {
                            Console.WriteLine(m.Action + " " + m.Title);
                            if (m.Action == "link" && m.Title == "BACK")
                            {
                                menu.SetMenu(font, m.Param);
                                togglemenu = false;
                            }
                        }

                        if (togglemenu) InMenu = !InMenu;
                    }
                }
                else
                {
                    foreach (MGLevel mg in MeshHigh)
                        mg.Update(gameTime);

                    if (ControlsEnabled)
                        UpdateCameras(gameTime);
                }

                oldms = newms;
                newms = Mouse.GetState();
                
                oldstate = newstate;
                oldkb = newkb;

            }

            base.Update(gameTime);
        }

        MouseState oldms = new MouseState();
        MouseState newms = new MouseState();

        private void UpdateCameras(GameTime gameTime)
        {
            oldms = newms;
            newms = Mouse.GetState();

            if (IsActive && newms.X >= 0 && newms.Y >= 0 && newms.LeftButton == ButtonState.Pressed)
            {
                IsMouseVisible = false;
                updatemouse = true;
                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            }
            else
            {
                IsMouseVisible = true;
                updatemouse = false;
            }

            skycamera.Update(gameTime, updatemouse, false, newms, oldms);
            camera.Update(gameTime, updatemouse, true, newms, oldms);
            rightCamera.Position = camera.Position + Vector3.Transform(Vector3.Right * settings.StereoPairSeparation, Matrix.CreateFromYawPitchRoll(camera.leftRightRot, camera._upDownRot, 0));
            rightCamera.rotationSpeed = camera.rotationSpeed;
            rightCamera.Target = camera.Target;
            rightCamera.Update(gameTime, updatemouse, true, newms, oldms);
            lowcamera.Copy(gameTime, camera);
        }

        private void UpdateProjectionMatrices()
        {
            camera.UpdateProjectionMatrix();
            rightCamera.UpdateProjectionMatrix();
            lowcamera.UpdateProjectionMatrix();
            skycamera.UpdateProjectionMatrix();
        }

        //public static bool twoSided = false;

        private void DrawLevel(FirstPersonCamera cam = null)
        {
            if (RenderEnabled)
            {
                //if (loading != null && gameLoaded)
                //{
                //if we have a sky and sky is enabled
                if (sky != null && settings.Sky)
                {
                    effect.View = skycamera.ViewMatrix;
                    effect.Projection = skycamera.ProjectionMatrix;

                    Vector3 x = effect.DiffuseColor;

                    effect.DiffuseColor = new Vector3(1, 1, 1);
                    sky.RenderSky(graphics, effect, alphaTestEffect);
                    effect.DiffuseColor = x;

                    //clear z buffer
                    GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Green, 1, 0);
                }

                //render depending on lod
                if (lodEnabled)
                {
                    effect.View = lowcamera.ViewMatrix;
                    effect.Projection = lowcamera.ProjectionMatrix;

                    if (settings.Models)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in instanced)
                            v.Render(graphics, instanceEffect, alphaTestEffect, lowcamera);
                    }

                    if (settings.BotsPath)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in paths)
                            v.Render(graphics, instanceEffect, alphaTestEffect, lowcamera);
                    }

                    Samplers.SetToDevice(graphics, EngineRasterizer.Default);

                    foreach (MGLevel qb in MeshLow)
                        qb.Render(graphics, effect, alphaTestEffect);

                    foreach (Kart k in karts)
                        k.Render(graphics, instanceEffect, alphaTestEffect, lowcamera);

                }
                else
                {
                    if (settings.Models)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in instanced)
                            v.Render(graphics, instanceEffect, alphaTestEffect, camera);
                    }

                    if (settings.BotsPath)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in paths)
                            v.Render(graphics, instanceEffect, alphaTestEffect, camera);
                    }

                    Samplers.SetToDevice(graphics, EngineRasterizer.Default);

                    effect.View = (cam != null ? cam.ViewMatrix : camera.ViewMatrix);
                    effect.Projection = camera.ProjectionMatrix;

                    alphaTestEffect.View = effect.View;
                    alphaTestEffect.Projection = effect.Projection;

                    foreach (MGLevel qb in MeshHigh)
                        qb.Render(graphics, effect, alphaTestEffect);

                    foreach (Kart k in karts)
                        k.Render(graphics, instanceEffect, alphaTestEffect, camera);

                }

                if (settings.VisData)
                {
                    //GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Green, 1, 0);

                    //texture enabled makes visdata invisible
                    effect.TextureEnabled = false;

                    foreach (var x in bbox)
                    {
                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, x, 0, x.Length / 2);
                        }
                    }
                }
            }
        }


        Task loading;

        private void LoadGame()
        {
            LoadStuff(null);
            loading = Task.Run(() => { });
            //loading = Task.Run(() => LoadStuff());
            //loading.Wait();
        }

        BigFileReader big;
        Howl howl;

        private void LoadLevelFromBig(int levelId = 0, int mode = 0, int files = 2)
        {
            if (big == null)
            {
                if (File.Exists("bigfile.big"))
                {
                    big = new BigFileReader(File.OpenRead("bigfile.big"));
                }
                else
                {
                    return;
                }
            }

            if (levelId == -1 && files == 3)
            {
                string[] levels = new string[5];

                for (int i = 0; i < 5; i++)
                {
                    big.FileCursor = 200 + i * 3;

                    Directory.CreateDirectory(Path.Combine(Meta.BasePath, Directory.GetParent(big.GetFilename()).FullName));
                    File.WriteAllBytes(Path.Combine(Meta.BasePath, big.GetFilename()), big.ReadFile());

                    big.NextFile();

                    levels[i] = Path.Combine(Meta.BasePath, big.GetFilename());
                    File.WriteAllBytes(levels[i], big.ReadFile());
                }

                LoadStuff(levels);
            }
            else
            {
                big.FileCursor = (files == 3 ? 200 + levelId * 3 : levelId * 8) + mode * files;

                Directory.CreateDirectory(Path.Combine(Meta.BasePath, Directory.GetParent(big.GetFilename()).FullName));

                File.WriteAllBytes(Path.Combine(Meta.BasePath, big.GetFilename()), big.ReadFile());

                big.NextFile();
                File.WriteAllBytes(Path.Combine(Meta.BasePath, big.GetFilename()), big.ReadFile());

                LoadStuff(new string[] { Path.Combine(Meta.BasePath, big.GetFilename()) });
            }

            if (howl == null)
            {
                if (File.Exists("kart.hwl"))
                {
                    howl = Howl.FromFile("kart.hwl");
                }
                else
                {
                    return;
                }
            }

            //howl.ExportAllSamples();
        }


        protected override void Draw(GameTime gameTime)
        {
            IsDrawing = true;

            GraphicsDevice.Clear(backColor);

            //graphics.GraphicsDevice.Viewport = vpFull;
            //DrawLevel();

            if (settings.StereoPair)
            {
                graphics.GraphicsDevice.Viewport = vpLeft;
                UpdateProjectionMatrices();
                DrawLevel();

                graphics.GraphicsDevice.Viewport = vpRight;
                UpdateProjectionMatrices();
                DrawLevel(rightCamera);

                graphics.GraphicsDevice.Viewport = vpFull;
                UpdateProjectionMatrices();
            }
            else
            {
                graphics.GraphicsDevice.Viewport = vpFull;
                DrawLevel();
            }

            if (InMenu)
                menu.Render(GraphicsDevice, spriteBatch, font, tint);


            spriteBatch.Begin(depthStencilState: DepthStencilState.Default);

            if (InMenu)
            {
                spriteBatch.Draw(
                    textures["logo"],
                    new Vector2((graphics.GraphicsDevice.Viewport.Width - textures["logo"].Width * (graphics.GraphicsDevice.Viewport.Height / 1080f)) / 2, 50 * graphics.GraphicsDevice.Viewport.Height / 1080f),
                    new Rectangle(0,0, textures["logo"].Width, textures["logo"].Height),
                    Color.White,
                    0,
                    new Vector2(textures["logo"].Width / 2, 0),
                    graphics.GraphicsDevice.Viewport.Height / 1080f,
                    SpriteEffects.None,
                    0.5f
                    );

                spriteBatch.DrawString(
                    font,
                    version,
                    new Vector2(((graphics.PreferredBackBufferWidth - font.MeasureString(version).X * graphics.GraphicsDevice.Viewport.Height / 1080f) / 2), graphics.PreferredBackBufferHeight - 60 * graphics.GraphicsDevice.Viewport.Height / 1080f),
                    Color.Aquamarine,
                    0,
                    new Vector2(0, 0),
                    graphics.GraphicsDevice.Viewport.Height / 1080f,
                    SpriteEffects.None,
                     0.5f
                    );
            }

            if (!gameLoaded)
                spriteBatch.DrawString(font, "LOADING...", new Vector2(graphics.PreferredBackBufferWidth / 2 - (font.MeasureString("LOADING...").X / 2), graphics.PreferredBackBufferHeight / 2), Color.Yellow);

            if (scn.Count == 0 && gameLoaded)
                spriteBatch.DrawString(font, "No levels loaded.\r\nPut LEV/VRM files in levels folder.\r\n...or put BIGFILE.BIG in root folder\r\nand use load level menu.".ToString(), new Vector2(20, 60), Color.Yellow);

            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) || Keyboard.GetState().IsKeyDown(Keys.OemPlus))
                spriteBatch.DrawString(font, String.Format("FOV {0}", camera.ViewAngle.ToString("0.##")), new Vector2(graphics.PreferredBackBufferWidth - font.MeasureString(String.Format("FOV {0}", camera.ViewAngle.ToString("0.##"))).X - 20, 20), Color.Yellow);

            if (settings.ShowCamPos)
                spriteBatch.DrawString(font, $"({(int)camera.Position.X}, {(int)camera.Position.Y}, {(int)camera.Position.Z})", new Vector2(20, 20), Color.Yellow);

            //spriteBatch.DrawString(font, String.Format("sp: {0}\r\nac:{1}", karts[0].Speed, karts[0].Accel), new Vector2(20, 20), Color.Yellow);

            spriteBatch.End();


            base.Draw(gameTime);

            IsDrawing = false;
        }

        protected override void Dispose(bool disposing)
        {
            alphalist.Clear();
            paths.Clear();
            textures.Clear();
            scn.Clear();
            MeshHigh.Clear();
            MeshLow.Clear();
            instanced.Clear();
            bbox.Clear();

            sky = null;
        }

    }
}
