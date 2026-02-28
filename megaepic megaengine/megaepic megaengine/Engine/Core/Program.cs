using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using Limeko.Entities;
using Limeko.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StbImageSharp;
using System.Diagnostics;
using System.Drawing;
using Font = SixLabors.Fonts.Font;

namespace Limeko
{
    class Core
    {
        public static Window WindowInstance { get; private set; }

        public static bool vsync = true;
        public static int targetFrameRate = 90;
        private static List<(Image<Rgba32> image, Vector2 position)> uiToRender = new();

        public class Program
        {
            static void Main()
            {
                WindowInstance = new Window();
                WindowInstance.Run();
            }
        }

        public class Window : GameWindow
        {
            float _time;

            public static Vector2 WindowSize;


            Vector3 _cameraPosition = new Vector3(0, 0.5f, 3);
            float _yaw = -90f;
            float _pitch = 0f;

            float _speed = 3f;
            float _sensitivity = 0.15f;

            Vector2 _lastMouse;
            bool _firstMove = true;


            double _fpsTimer;
            int _frameCount;
            int _fps;
            bool _showDebug = false;


            int _vao;
            int _vbo;
            Shader _shader;

            int _uiVao;
            int _uiVbo;
            Shader _uiShader;

            int _devVao;
            int _devVbo;
            public Shader _debugShader;


            static GameWindowSettings gameSettings = new GameWindowSettings()
            {
                UpdateFrequency = targetFrameRate
            };

            static NativeWindowSettings windowSettings = new NativeWindowSettings()
            {
                MinimumClientSize = new Vector2i(320, 180),
                ClientSize = new Vector2i(1280, 720),
                WindowState = WindowState.Normal,
                Vsync = VSyncMode.On,
                Title = "Limeko"
            };


            public Window() : base(gameSettings, windowSettings)
            { }

            protected override void OnLoad()
            {
                base.OnLoad();
                Console.WriteLine("Starting...");

                // init

                this.VSync = vsync ? VSyncMode.On : VSyncMode.Off;


                //  visual stuff first

                var iconImage = Utils.Images.GetImageByPath(Path.Combine(Utils.Paths.EngineData, "Icon/icon.png"));
                this.Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(iconImage.Width, iconImage.Height, iconImage.Data));

                WindowSize = new Vector2(Size.X, Size.Y);


                //  ui things

                // UserInterface.Common.LoadFont(Path.Combine(Utils.Paths.EngineData, "Fonts/common.ttf"));


                float[] quad = Graphics.Primitives.Quad();

                _uiVao = GL.GenVertexArray();
                _uiVbo = GL.GenBuffer();

                GL.BindVertexArray(_uiVao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _uiVbo);
                GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
                GL.EnableVertexAttribArray(1);



                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(FrontFaceDirection.Ccw);

                ReloadShaders();


                // initialize physics

                Console.WriteLine("Initializing Bepu...");
                Physics.Initialize();
                Console.WriteLine("Bepu initialized!");



                // spawn objects


                Entity floor = new();
                floor.Transform.Scale = new Vector3(20f, 1f, 20f);
                floor.Rigidbody.isStatic = true;
                floor.PhysicsShape = new Box(floor.Transform.Scale.X, floor.Transform.Scale.Y, floor.Transform.Scale.Z);
                floor.Transform.Position = new Vector3(0, -2f, 0);
                floor.Awake();

                Graphics.Utils.CreateObject(floor, new Graphics.Mesh(Primitives.Cube()));


                // System.Random rng = new System.Random();

                float objectDistance = 3f;
                Vector3 objectColor = (1f, 0.43f, 0.7f);

                string meshFolder = Path.Combine(Utils.Paths.EngineData, "Meshes");

                int move = 0;
                foreach(var meshPath in Directory.GetFiles(meshFolder, "*.obj").OrderBy(p => p))
                {
                    Transform offset = new Transform();
                    offset.Position.Y = -0.5f;

                    Entity entity = new Entity();
                    entity.Rigidbody.mass = 5f;
                    entity.Rigidbody.isStatic = false;
                    entity.Transform.Position = new Vector3(move * objectDistance, 0f, 0f);
                    Graphics.Utils.CreateObject(entity, new Graphics.Mesh(Utils.MeshLoader.Load_OBJ(meshPath)), offset, objectColor);
                    entity.Awake();
                    move++;
                }

                Console.WriteLine($"Loaded {move} models.");



                Console.WriteLine("> Setting background");
                GL.ClearColor(0.63f, 1f, 0.3f, 1f);

                Console.WriteLine("OpenGL running.");




                Console.Clear();
                Utils.Misc.PrintLimeko(true);
                Utils.Misc.PrintLicenseDisclaimer();

                Console.WriteLine("\n\nPress F1 to enter dev mode.  |  Press F2 to reload assets");
            }

            protected override void OnUnload()
            {
                _shader.Dispose();
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                Vector3 front;
                front.X = MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_yaw)) *
                          MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch));
                front.Y = MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch));
                front.Z = MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(_yaw)) *
                          MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch));

                front = Vector3.Normalize(front);


                if(_showDebug)
                {
                    
                }


                var input = KeyboardState;
                var mouse = MouseState;

                float delta = (float)e.Time;
                Physics.Step(delta);

                bool rightClick = mouse.IsButtonDown(MouseButton.Button2);

                this.CursorState = rightClick ? CursorState.Grabbed : CursorState.Normal;
                this.Cursor = MouseCursor.Default;
                if(rightClick)
                {
                    if (_firstMove)
                    {
                        _lastMouse = mouse.Position;
                        _firstMove = false;
                    }
                    else
                    {
                        var deltaMouse = mouse.Position - _lastMouse;
                        _lastMouse = mouse.Position;

                        _yaw += deltaMouse.X * _sensitivity;
                        _pitch -= deltaMouse.Y * _sensitivity;

                        _pitch = OpenTK.Mathematics.MathHelper.Clamp(_pitch, -89f, 89f);
                    }
                }
                else _firstMove = true;


                Vector3 worldUp = Vector3.UnitY;
                Vector3 right = Vector3.Normalize(Vector3.Cross(front, worldUp));
                Vector3 up = Vector3.Normalize(Vector3.Cross(right, front));

                if (input.IsKeyDown(Keys.W)) _cameraPosition += front * _speed * delta;
                if (input.IsKeyDown(Keys.S)) _cameraPosition -= front * _speed * delta;

                if (input.IsKeyDown(Keys.A)) _cameraPosition -= right * _speed * delta;
                if (input.IsKeyDown(Keys.D)) _cameraPosition += right * _speed * delta;

                if (input.IsKeyDown(Keys.E)) _cameraPosition += up * _speed * delta;
                if (input.IsKeyDown(Keys.Q)) _cameraPosition -= up * _speed * delta;

                if (input.IsKeyReleased(Keys.F))
                {
                    Entity pew = new Entity();
                    pew.Transform.Position = _cameraPosition;
                    pew.Rigidbody.isStatic = false;
                    Graphics.Utils.CreateObject(pew, new Graphics.Mesh(Graphics.Primitives.Cube()));
                    pew.Awake();
                    System.Numerics.Vector3 force = new System.Numerics.Vector3(70f, 0f, 0f);
                    pew.Rigidbody.Description.Velocity = new BodyVelocity(force);
                }

                _speed += mouse.ScrollDelta.Y;
                _speed = Math.Clamp(_speed, 1f, 30f);

                if (input.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F1)) _showDebug = !_showDebug;
                if (input.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F2)) ReloadShaders();

                if (input.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F9)) Utils.Misc.OpenWebpage("https://github.com/violetv0id/Limeko-Engine/blob/86032362746b41fc365ed2ea4f40fabdf4dea6e5/LICENSE");
            }

            protected override void OnResize(ResizeEventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(0, 0, Size.X, Size.Y);
                WindowSize = new Vector2(Size.X, Size.Y);
            }

            protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs e)
            {
                //  Time
                _time += (float)e.Time;
                _frameCount++;
                _fpsTimer += e.Time;

                if (_showDebug) this.Title = $"Limeko [{_fps} FPS]";
                else this.Title = "Limeko";

                if (_fpsTimer >= 1.0)
                {
                    _fps = _frameCount;
                    _frameCount = 0;
                    _fpsTimer = 0.0;
                }



                
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //  Shader
                _shader.Use();
                _shader.SetVector3("uLightDir", new Vector3(-0.3f, -1f, -0.2f));

                Matrix4 model =
                    Matrix4.CreateRotationY(_time) *
                    Matrix4.CreateRotationX(_time * 0.5f);

                Vector3 front = new Vector3(
                    MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_yaw)) *
                    MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch)),
                    MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch)),
                    MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(_yaw)) *
                    MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(_pitch))
                );

                Matrix4 view = Matrix4.LookAt(
                    _cameraPosition,
                    _cameraPosition + Vector3.Normalize(front),
                    Vector3.UnitY);

                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                    OpenTK.Mathematics.MathHelper.DegreesToRadians(70f),
                    Size.X / (float)Size.Y,
                    0.01f,
                    100f);

                _shader.SetMatrix4("uModel", model);
                _shader.SetMatrix4("uView", view);
                _shader.SetMatrix4("uProjection", projection);
                foreach(var obj in RenderingCore.GetRegistered())
                {
                    _shader.SetColor("uRandomColor", obj.Color);
                    _shader.SetMatrix4("uModel", obj.GetModelMatrix());
                    obj.Mesh.Draw(_showDebug ? PrimitiveType.Lines : PrimitiveType.Triangles);
                }



                GL.Disable(EnableCap.DepthTest);

                _uiShader.Use();
                GL.BindVertexArray(_uiVao);

                /*
                foreach (var element in uiToRender)
                {
                    float x = element.position.X;
                    float y = element.position.Y;

                    float w = element.image.Width;
                    float h = element.image.Height;

                    float[] quad = Graphics.Primitives.ScreenSpaceQuad(new Vector2(x, y), new Vector2(w, h));

                    GL.BindBuffer(BufferTarget.ArrayBuffer, _uiVbo);
                    GL.BufferSubData(BufferTarget.ArrayBuffer,
                        IntPtr.Zero,
                        quad.Length * sizeof(float),
                        quad);

                    int texture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texture);

                    GL.TexParameter(TextureTarget.Texture2D,
                        TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D,
                        TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    element.image.DangerousTryGetSinglePixelMemory(out var mem);
                    Span<Rgba32> pixels = mem.Span;
                    ref Rgba32 pixelRef = ref MemoryMarshal.GetReference(pixels);

                    GL.TexImage2D(
                        TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        element.image.Width,
                        element.image.Height,
                        0,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        ref pixelRef);

                    _uiShader.Use();

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, texture);
                    _uiShader.SetInt("uTexture", 0);

                    GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                    GL.DeleteTexture(texture);
                }
                */


                SwapBuffers();
                uiToRender.Clear();
            }

            public void OnSettingsUpdated()
            {
                this.VSync = vsync ? VSyncMode.On : VSyncMode.Off;
                this.UpdateFrequency = (float)targetFrameRate;
            }

            public void ReloadShaders()
            {
                Console.WriteLine("Refreshing shader cache...");

                // dispose of active
                _shader?.Dispose();
                _uiShader?.Dispose();

                try
                {
                    _shader = new Shader(
                    Path.Combine(Utils.Paths.EngineData, "Shaders/basic.vert"),
                    Path.Combine(Utils.Paths.EngineData, "Shaders/basic.frag"));
                }
                catch(Exception ex) { Console.WriteLine($"Shader Compilation error: {ex.Message}"); };

                try
                {
                    _uiShader = new Shader(
                    Path.Combine(Utils.Paths.EngineData, "Shaders/ui.vert"),
                    Path.Combine(Utils.Paths.EngineData, "Shaders/ui.frag"));
                }
                catch (Exception ex) { Console.WriteLine($"Shader Compilation error: {ex.Message}"); };

                // clear uniform cache
                Graphics.Utils.ClearUniformCache();

                // init them, even when we aren't going to use them.
                _shader.Use();
                _uiShader.Use();
            }
        }

        public static Vector2 PixelToNDC(float x, float y)
        {
            float ndcX = (x / Window.WindowSize.X) * 2f - 1f;
            float ndcY = 1f - (y / Window.WindowSize.Y) * 2f; //  flip
            return new Vector2(ndcX, ndcY);
        }

        public static void QueueUI(Image<Rgba32> toRender, Vector2 position)
        {
            uiToRender.Add((toRender, position));
        }
    }

    public static class Physics
    {
        public static Simulation Simulation;
        static BufferPool _bufferPool;
        public static List<Entity> RegisteredBodies = new();

        public static bool initialized { get; private set; }

        static float accumulator;
        const float FixedTimestep = 1f / 60f;


        public static void Initialize()
        {
            initialized = false;
            _bufferPool = new BufferPool();

            Console.WriteLine($"[BEPU]: Fixed Timestep is {FixedTimestep}");

            var solveDescription = new SolveDescription(
                velocityIterationCount: 8,
                substepCount: 1);
            Console.WriteLine($"[BEPU]: Solver IC: {solveDescription.VelocityIterationCount}");
            Console.WriteLine($"[BEPU]: Solver Substep: {solveDescription.SubstepCount}");

            Simulation = Simulation.Create(
                _bufferPool,
                new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new System.Numerics.Vector3(0, -9.81f, 0)),
                solveDescription);
            initialized = true;
        }

        public static void RegisterBody(Entity entity)
        {
            RegisteredBodies.Add(entity);

            var pose = new RigidPose
            {
                Position = (System.Numerics.Vector3)entity.Transform.Position,
                Orientation = (System.Numerics.Quaternion)entity.Transform.Rotation
            };

            if (entity.PhysicsShape is Box box)
            {
                entity.ShapeIndex = Simulation.Shapes.Add(box);

                if (entity.Rigidbody.isStatic)
                {
                    entity.StaticHandle =
                        Simulation.Statics.Add(new StaticDescription(pose, entity.ShapeIndex));
                }
                else
                {
                    var inertia = box.ComputeInertia(entity.Rigidbody.mass);

                    entity.Rigidbody.Description = BodyDescription.CreateDynamic(pose, inertia, new CollidableDescription(entity.ShapeIndex, 0.1f), new BodyActivityDescription(0.01f));
                    entity.DynamicHandle =
                        Simulation.Bodies.Add(entity.Rigidbody.Description);
                }
            }
            else if (entity.PhysicsShape is Sphere sphere)
            {
                entity.ShapeIndex = Simulation.Shapes.Add(sphere);

                if (entity.Rigidbody.isStatic)
                {
                    entity.StaticHandle =
                        Simulation.Statics.Add(new StaticDescription(pose, entity.ShapeIndex));
                }
                else
                {
                    var inertia = sphere.ComputeInertia(entity.Rigidbody.mass);

                    entity.DynamicHandle =
                        Simulation.Bodies.Add(
                            BodyDescription.CreateDynamic(
                                pose,
                                inertia,
                                new CollidableDescription(entity.ShapeIndex, 0.1f),
                                new BodyActivityDescription(0.01f)));
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"Unsupported shape type: {entity.PhysicsShape?.GetType().Name}");
            }
        }

        public static void Step(float dt)
        {
            dt = MathF.Min(dt, 0.1f);
            accumulator += dt;

            while (accumulator >= FixedTimestep)
            {
                Simulation.Timestep(FixedTimestep);

                foreach (var entity in RegisteredBodies)
                {
                    if (entity.Rigidbody.isStatic)
                        continue;

                    var body = Simulation.Bodies.GetBodyReference(entity.DynamicHandle);

                    entity.Transform.Position =
                        (Vector3)body.Pose.Position;

                    entity.Transform.Rotation =
                        (Quaternion)body.Pose.Orientation;
                }

                accumulator -= FixedTimestep;
            }
        }

        public static bool PickEntity(System.Numerics.Vector3 origin, System.Numerics.Vector3 direction, float maxDistance, out Entity picked)
        {
            var handler = new PickHandler();
            Physics.Simulation.RayCast(origin, direction, maxDistance, ref handler);

            picked = handler.HitEntity;
            return picked != null;
        }

        public class PickHandler : IRayHitHandler
        {
            public Entity HitEntity = null;
            public float HitT = float.MaxValue;

            public bool AllowTest(CollidableReference collidable) => true;
            public bool AllowTest(CollidableReference collidable, int childIndex) => true;

            public void OnRayHit(in RayData ray, ref float maximumT, float t, in System.Numerics.Vector3 normal, CollidableReference collidable, int childIndex)
            {
                if (t >= HitT) return; // already have closer hit

                // check dynamic bodies first
                if (collidable.Mobility == CollidableMobility.Dynamic)
                {
                    var bodyHandle = collidable.BodyHandle;
                    HitEntity = Physics.RegisteredBodies
                                   .FirstOrDefault(e => e.DynamicHandle == bodyHandle);
                }
                else if (collidable.Mobility == CollidableMobility.Static)
                {
                    var staticHandle = collidable.StaticHandle;
                    HitEntity = Physics.RegisteredBodies
                                   .FirstOrDefault(e => e.StaticHandle == staticHandle);
                }

                if (HitEntity != null)
                {
                    HitT = t;
                    maximumT = t; // shorten the ray so farther hits are ignored
                }
            }
        }
    }

    public static class Utils
    {
        public static class Paths
        {
            public static string EngineCore = @"Engine\Core";
            public static string EngineData = @"Engine\Data";
            public static string EngineGraphics = @"Engine\Graphics";
            public static string EnginePhysics = @"Engine\Phyics";
        }

        public static class Math
        {
            public enum MathOperation
            {
                Add,
                Subtract,
                Multiply,
                Divide
            }

            public static float Vector3Magnitude(System.Numerics.Vector3 input)
            {
                float r = input.X;
                if(input.Y > r) r = input.Y;
                if(input.Z > r) r = input.Z;
                return r;
            }

            public static System.Numerics.Vector3 Vector3Operation(System.Numerics.Vector3 a, System.Numerics.Vector3 b, MathOperation operation)
            {
                System.Numerics.Vector3 ret = a;
                switch(operation)
                {
                    case MathOperation.Add:
                        ret.X += b.X; ret.Y += b.Y; ret.Z += b.Z;
                        break;
                    case MathOperation.Subtract:
                        ret.X -= b.X; ret.Y -= b.Y; ret.Z -= b.Z;
                        break;
                    case MathOperation.Multiply:
                        ret.X *= b.X; ret.Y *= b.Y; ret.Z *= b.Z;
                        break;
                    case MathOperation.Divide:
                        ret.X /= b.X; ret.Y /= b.Y; ret.Z /= b.Z;
                        break;
                }
                return ret;
            }
        }

        public static class Images
        {
            public static ImageResult GetImageByPath(string path)
            {
                using var stream = File.OpenRead(path);
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                return image;
            }
        }

        public static class Misc
        {
            public static void PrintLimeko(bool spacer)
            {
                if(spacer) Console.WriteLine("");
                Console.WriteLine("                                                           .-'''-.     ");
                Console.WriteLine(".---.                                                     '   _    \\   ");
                Console.WriteLine("|   |.--. __  __   ___         __.....__          .     /   /` '.   \\  ");
                Console.WriteLine("|   ||__||  |/  `.'   `.   .-''         '.      .'|    .   |     \\  '  ");
                Console.WriteLine("|   |.--.|   .-.  .-.   ' /     .-''\"'-.  `.  .'  |    |   '      |  ' ");
                Console.WriteLine("|   ||  ||  |  |  |  |  |/     /________\\   \\<    |    \\    \\     / /  ");
                Console.WriteLine("|   ||  ||  |  |  |  |  ||                  | |   | ____`.   ` ..' /   ");
                Console.WriteLine("|   ||  ||  |  |  |  |  |\\    .-------------' |   | \\ .'   '-...-'`    ");
                Console.WriteLine("|   ||  ||  |  |  |  |  | \\    '-.____...---. |   |/  .                ");
                Console.WriteLine("|   ||__||__|  |__|  |__|  `.             .'  |    /\\  \\               ");
                Console.WriteLine("'---'                        `''-...... -'    |   |  \\  \\              ");
                Console.WriteLine("                                              '    \\  \\  \\             ");
                Console.WriteLine("                                             '------'  '---'           ");
                if(spacer) Console.WriteLine("");
            }

            public static void PrintLicenseDisclaimer()
            {
                Console.WriteLine("Limeko-Engine  Copyright (C) 2026  lunark");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.");
                Console.WriteLine("This is free software, and you are welcome to redistribute it");
                Console.WriteLine("under certain conditions. Press F9 to learn more.");
            }

            public static void OpenWebpage(string url)
            {
                ProcessStartInfo info = new ProcessStartInfo
                { FileName = url, UseShellExecute = true };
                Process.Start(info);
            }
        }

        public static class MeshLoader
        {
            public static float[] Load_OBJ(string path)
            {
                var positions = new List<Vector3>();
                var normals = new List<Vector3>();
                var vertices = new List<float>();

                foreach (var line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    switch (parts[0])
                    {
                        case "v":
                            positions.Add(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])));
                            break;

                        case "vn":
                            normals.Add(Vector3.Normalize(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3]))));
                            break;

                        case "f":
                            {
                                for (int i = 2; i < parts.Length - 1; i++)
                                {
                                    AddVertex(parts[1], positions, normals, vertices);
                                    AddVertex(parts[i], positions, normals, vertices);
                                    AddVertex(parts[i + 1], positions, normals, vertices);
                                }
                                break;
                            }
                    }
                }

                return vertices.ToArray();
            }

            private static void AddVertex(
                string token,
                List<Vector3> positions,
                List<Vector3> normals,
                List<float> vertices)
            {
                var indices = token.Split('/');

                int vIndex = int.Parse(indices[0]) - 1;
                Vector3 pos = positions[vIndex];

                Vector3 norm = Vector3.UnitY;

                if (indices.Length >= 3 && !string.IsNullOrEmpty(indices[2]))
                {
                    int nIndex = int.Parse(indices[2]) - 1;
                    if (nIndex >= 0 && nIndex < normals.Count)
                        norm = normals[nIndex];
                }

                vertices.Add(pos.X);
                vertices.Add(pos.Y);
                vertices.Add(pos.Z);

                vertices.Add(norm.X);
                vertices.Add(norm.Y);
                vertices.Add(norm.Z);
            }
        }
    }
}

namespace Limeko.Entities
{
    public class Utils
    {
        /*
        public static Entity CreateEntity(Transform transform, Graphics.Mesh mesh)
        {

        }
        */
    }

    public class EntityManagement
    {
        public static List<Entity> RegisteredEntities = new();

        public static void Register(Entity entity)
        {
            entity.Id = RegisteredEntities.Count;
            Physics.RegisterBody(entity);
            RegisteredEntities.Add(entity);
        }
    }

    public class Entity
    {
        public int Id = 0;

        public Transform Transform = new();

        public BodyHandle DynamicHandle;
        public StaticHandle StaticHandle;
        public TypedIndex ShapeIndex;

        public IShape PhysicsShape;
        public Rigidbody Rigidbody = new();

        public void Awake()
        {
            if(PhysicsShape == null) PhysicsShape = new Box(1f, 1f, 1f);
            EntityManagement.Register(this);
        }
    }

    public class Transform
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
    }

    public class Rigidbody
    {
        public float mass = 1f;
        public bool isStatic = true;
        public BodyDescription Description;

        public BodyInertia inertia;
    }
}

namespace Limeko.Editor
{

}

namespace Limeko.Rendering
{
    public class Material
    {
        public Shader _Shader;
        // display shader parameters
    }
}

namespace Limeko.Graphics
{
    public class RenderingCore
    {
        private static List<RenderObject> _objects = new();

        public static void Register(RenderObject obj)
        {
            _objects.Add(obj);
        }
        public static void Unregister(RenderObject obj)
        {
            try { _objects.Remove(obj); }
            catch (Exception ex) { Console.WriteLine($"Error while Unregistering {obj.AttachedEntity.Id}"); }
        }
        public static List<RenderObject> GetRegistered()
        {
            return _objects;
        }

        public static int CreateFrameBuffer()
        {
            int frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            return frameBuffer;
        }

        public static void BindFrameBuffer(int frameBuffer, int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            GL.Viewport(0, 0, width, height);
        }


        public static int CreateTextureAttachment(int width, int height)
        {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height,
                0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, texture, 0);
            return texture;
        }

        public static int CreateDepthTextureAttachment(int width, int height)
        {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, width, height,
                0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, texture, 0);
            return texture;
        }

        public static int CreateDepthBufferAttachment(int width, int height)
        {
            int depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);
            return depthBuffer;
        }
    }

    static class Utils
    {
        static Dictionary<(int, string), int> _uniformCache = new();

        public static int GetUniformLocation(string name, int program)
        {
            var key = (program, name);
            if (_uniformCache.TryGetValue(key, out int loc)) return loc;

            loc = GL.GetUniformLocation(program, name);
            _uniformCache[key] = loc;
            return loc;
        }

        public static void ClearUniformCache()
        {
            _uniformCache.Clear();
        }

        public static void CreateObject(Entity entity, Mesh mesh)
        {
            Graphics.RenderingCore.Register(new RenderObject
            {
                Mesh = mesh,
                AttachedEntity = entity,
                Color = new Vector3(0.8f, 0.8f, 0.8f) // light gray floor
            });
        }

        public static void CreateObject(Entity entity, Mesh mesh, Transform offset)
        {
            Graphics.RenderingCore.Register(new RenderObject
            {
                Mesh = mesh,
                AttachedEntity = entity,
                Offset = offset,
                Color = new Vector3(0.8f, 0.8f, 0.8f) // default is light gray
            });
        }

        public static void CreateObject(Entity entity, Mesh mesh, Vector3 color)
        {
            Graphics.RenderingCore.Register(new RenderObject
            {
                Mesh = mesh,
                AttachedEntity = entity,
                Color = color
            });
        }

        public static void CreateObject(Entity entity, Mesh mesh, Transform offset, Vector3 color)
        {
            Graphics.RenderingCore.Register(new RenderObject
            {
                Mesh = mesh,
                AttachedEntity = entity,
                Offset = offset,
                Color = color
            });
        }

        public static void DrawDebugShapes(Matrix4 view, Matrix4 projection)
        {
            foreach (var entity in Physics.RegisteredBodies)
            {
                var model = Matrix4.CreateScale(entity.Transform.Scale) *
                            Matrix4.CreateFromQuaternion(entity.Transform.Rotation) *
                            Matrix4.CreateTranslation(entity.Transform.Position);

                if (entity.PhysicsShape is Box box)
                {
                    Core.WindowInstance._debugShader.SetMatrix4("uModel", model);
                    Core.WindowInstance._debugShader.SetMatrix4("uView", view);
                    Core.WindowInstance._debugShader.SetMatrix4("uProjection", projection);
                    Core.WindowInstance._debugShader.SetColor("uColor", new Vector3(0.3f, 0.3f, 1f));
                    Graphics.Primitives.Cube();
                }
                // add capsules, convexes etc similarly
            }
        }
    }

    public static class Primitives
    {
        public static float[] Cube()
        {
            return new float[]
            {
                 // FRONT
                -0.5f,-0.5f, 0.5f,  0,0,1,
                 0.5f,-0.5f, 0.5f,  0,0,1,
                 0.5f, 0.5f, 0.5f,  0,0,1,
                 0.5f, 0.5f, 0.5f,  0,0,1,
                -0.5f, 0.5f, 0.5f,  0,0,1,
                -0.5f,-0.5f, 0.5f,  0,0,1,

                 // BACK
                -0.5f,-0.5f,-0.5f,  0,0,-1,
                -0.5f, 0.5f,-0.5f,  0,0,-1,
                 0.5f, 0.5f,-0.5f,  0,0,-1,
                 0.5f, 0.5f,-0.5f,  0,0,-1,
                 0.5f,-0.5f,-0.5f,  0,0,-1,
                -0.5f,-0.5f,-0.5f,  0,0,-1,

                 // LEFT
                -0.5f, 0.5f, 0.5f, -1,0,0,
                -0.5f, 0.5f,-0.5f, -1,0,0,
                -0.5f,-0.5f,-0.5f, -1,0,0,
                -0.5f,-0.5f,-0.5f, -1,0,0,
                -0.5f,-0.5f, 0.5f, -1,0,0,
                -0.5f, 0.5f, 0.5f, -1,0,0,

                 // RIGHT
                 0.5f, 0.5f, 0.5f,  1,0,0,
                 0.5f,-0.5f,-0.5f,  1,0,0,
                 0.5f, 0.5f,-0.5f,  1,0,0,
                 0.5f,-0.5f,-0.5f,  1,0,0,
                 0.5f, 0.5f, 0.5f,  1,0,0,
                 0.5f,-0.5f, 0.5f,  1,0,0,

                 // TOP
                -0.5f, 0.5f,-0.5f,  0,1,0,
                -0.5f, 0.5f, 0.5f,  0,1,0,
                 0.5f, 0.5f, 0.5f,  0,1,0,
                 0.5f, 0.5f, 0.5f,  0,1,0,
                 0.5f, 0.5f,-0.5f,  0,1,0,
                -0.5f, 0.5f,-0.5f,  0,1,0,

                 // BOTTOM
                -0.5f,-0.5f,-0.5f,  0,-1,0,
                 0.5f,-0.5f, 0.5f,  0,-1,0,
                -0.5f,-0.5f, 0.5f,  0,-1,0,
                 0.5f,-0.5f, 0.5f,  0,-1,0,
                -0.5f,-0.5f,-0.5f,  0,-1,0,
                 0.5f,-0.5f,-0.5f,  0,-1,0,
            };
        }

        public static float[] Quad()
        {
            return new float[]
            {
                 // pos         uv
                -1f, -1f,     0f, 0f,
                 1f, -1f,     1f, 0f,
                 1f,  1f,     1f, 1f,

                 1f,  1f,     1f, 1f,
                -1f,  1f,     0f, 1f,
                -1f, -1f,     0f, 0f,
            };
        }

        public static float[] ScreenSpaceQuad(Vector2 position, Vector2 elementSize)
        {
            float x = position.X;
            float y = position.Y;

            float w = elementSize.X;
            float h = elementSize.Y;

            var tl = Core.PixelToNDC(x, y);
            var tr = Core.PixelToNDC(x + w, y);
            var br = Core.PixelToNDC(x + w, y + h);
            var bl = Core.PixelToNDC(x, y + h);

            return new float[]
            {
                tl.X, tl.Y, 0f, 0f,
                tr.X, tr.Y, 1f, 0f,
                br.X, br.Y, 1f, 1f,

                br.X, br.Y, 1f, 1f,
                bl.X, bl.Y, 0f, 1f,
                tl.X, tl.Y, 0f, 0f,
            };
        }
    }

    public class Mesh
    {
        int _vao;
        int _vbo;
        int _vertexCount;

        public Mesh(float[] vertices)
        {
            _vertexCount = vertices.Length / 6;

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          vertices.Length * sizeof(float),
                          vertices,
                          BufferUsageHint.StaticDraw);

            //  position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                                   false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            //   normal
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float,
                                   false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        public void Draw(PrimitiveType type = PrimitiveType.Triangles)
        {
            GL.BindVertexArray(_vao);
            GL.DrawArrays(type, 0, _vertexCount);
        }
    }

    public class RenderObject
    {
        public Mesh Mesh;
        public Entities.Entity AttachedEntity;
        public Transform Offset = new();
        public Vector3 Color = new Vector3(0.8f, 0.8f, 0.8f);

        public Matrix4 GetModelMatrix()
        {
            return
                Matrix4.CreateScale(AttachedEntity.Transform.Scale + (Offset.Scale - Vector3.One)) *
                Matrix4.CreateFromQuaternion(AttachedEntity.Transform.Rotation + Offset.Rotation) *
                Matrix4.CreateTranslation(AttachedEntity.Transform.Position + Offset.Position);
        }
    }
}

namespace Limeko.UserInterface
{
    public static class Common
    {
        public static Font activeFont;
        public static FontFamily fontFamily;

        public static void LoadFont(string path)
        {
            var collection = new FontCollection();
            FontFamily family = collection.Add(path);

            activeFont = family.CreateFont(32);
        }

        public static void DrawText(string text, Vector2 position)
        {
            Image<Rgba32> image = new Image<Rgba32>(512, 128);
            image.Mutate(ctx =>
            {
                ctx.DrawText(
                    text,
                    activeFont,
                    SixLabors.ImageSharp.Color.White,
                    new SixLabors.ImageSharp.PointF(10, 40));
            });
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            Limeko.Core.QueueUI(image, position);
        }

        public static void DrawImage(OpenTK.Windowing.Common.Input.Image image, Vector2 position)
        {
            // throw new NotImplementedException();
        }

        public static bool Button(string text, Vector2 position)
        {
            return false; //  TODO: replace with logic for button pressing, and return true.
        }
    }
}