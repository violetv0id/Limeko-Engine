using Avalonia;
using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;

namespace Limeko
{
    public class Core
    {
        /// <summary>
        /// The static instance of this program's active window.
        /// </summary>
        public static Window WindowInstance { get; private set; }

        public static void Main()
        {
            // Editor.SplashScreen.Show();

            Console.Title = "Limeko Console";
            WindowInstance = new Window();
            WindowInstance.Run();
        }

        public class Window : GameWindow
        {
            float _deltaTime;
            float _fixedDeltaTime;

            public static Vector2 WindowSize;

            public static int targetFrameRate = 90;


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
                Title = "Limeko",
                StartVisible = false
            };


            public Window() : base(gameSettings, windowSettings)
            { }

            protected override async void OnLoad()
            {
                base.OnLoad();

                // Initialize the Editor window.
                // UI, Editor subsystems, etc.
                // Do *not* Initialize Physics--that's for runtime.

                // Slowly learning from my mistakes.

                Editor.EditorInit();


                Console.WriteLine("<--> Starting Editor (Internal) <-->");

                WindowSize = new Vector2(Size.X, Size.Y);

                Rendering.ConfigureOpenGL();

                Console.WriteLine("> Setting background");
                // Slightly above black to avoid confusion
                GL.ClearColor(0.02f, 0.02f, 0.02f, 1f);

                Console.WriteLine("OpenGL Core Running.");




                Console.Clear();

                Editor.Utils.Misc.PrintLimeko(true);
                Editor.Utils.Misc.PrintLicenseDisclaimer();

                Console.WriteLine("\n\n#= Dev-Stats =#\n");

                Console.WriteLine($"> Project Path: {Editor.Utils.GetActiveProjectPath()}");
                Console.WriteLine($"> Default Project Path: {Editor.Utils.GetDefaultProjectPath()}\n");

                Console.WriteLine("> OpenGL: Running & Configured");
                Console.WriteLine("> Bepu: Not Implemented");
                Console.WriteLine("> Editor UI: Not Implemented");
                Console.WriteLine("> Render Pipeline: Not Implemented");
                Console.WriteLine("> Audio System: Not Implemented");

                this.IsVisible = true;
            }

            protected override void OnUnload()
            {
                base.OnUnload();
                // Dispose of all Shaders, free any assets, etc.
                // _shader.Dispose();
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base .OnUpdateFrame(e);
                // Runs every frame.

                Input.Update(KeyboardState);
            }

            protected override void OnResize(ResizeEventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(0, 0, Size.X, Size.Y);
                WindowSize = new Vector2(Size.X, Size.Y);
            }

            protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs e)
            {

                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                Matrix4 model =
                    Matrix4.CreateRotationY(_deltaTime) *
                    Matrix4.CreateRotationX(_deltaTime * 0.5f);


                // EDITOR VIEW CAMERA
                /*
                Vector3 front = new Vector3(
                    MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians()) *
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
                */


                // PER-OBJECT RENDERING
                /*
                foreach (var obj in RenderingCore.GetRegistered())
                {
                    obj.Material.Bind();

                    var shader = obj.Material.Shader;

                    shader.SetInt("uLightCount", 1);

                    shader.SetVector3("uLightDirs[0]",
                        Vector3.Normalize(new Vector3(-0.3f, -1f, -0.2f)));

                    shader.SetVector3("uLightColors[0]", Vector3.One);
                    shader.SetFloat("uLightIntensity[0]", 1.0f);

                    // optional but important
                    shader.SetFloat("uAmbient", 0.2f);

                    obj.Material.Shader.SetMatrix4("uModel", obj.GetModelMatrix());
                    obj.Material.Shader.SetMatrix4("uView", view);
                    obj.Material.Shader.SetMatrix4("uProjection", projection);

                    obj.Mesh.Draw(Limeko.Core.WindowInstance._showDebug ? PrimitiveType.Lines : PrimitiveType.Triangles);
                }
                */

                SwapBuffers();
            }

            public void OnSettingsUpdated()
            {
                this.UpdateFrequency = (float)targetFrameRate;
            }

            public void ReloadAssets()
            {
                Console.WriteLine("Not Implemented.");
            }
        }
    }

    public class Input
    {
        // not implemented

        // supports multiple keyboards, although the current input method-
        // -does not support multiple keyboards.
        // Maybe switch to an input library?


        // mouse control // eventually move to a separate Input class.
        Vector2 _lastMouse;
        bool _firstMove = true;
        float _sensitivity = 0.15f;

        public static Dictionary<Keyboard, KeyboardState> keyboards = new();

        /// <summary>
        /// An internal method for updating inputs. Do not call this directly!
        /// </summary>
        public static void Update(KeyboardState keyboard)
        {
            
        }

        // Inefficient but functional
        public enum Key
        {
            Q,W,E,R,T,Y,U,I,O,P,A,S,D,F,G,H,J,K,L,Z,X,C,V,B,N,M,ZERO,ONE,TWO,THREE,FOUR,FIVE,SIX,SEVEN,EIGHT,NINE,ESCAPE,COMMA,PERIOD,COLON,QUOTE
        }

        public class Keyboard
        {
            public virtual void OnKeyDown(Key key)
            {

            }
        }
    }

    public class Rendering
    {
        // Logic

        /// <summary>
        /// Configures OpenGL to not render backfaces and to blend.
        /// </summary>
        public static void ConfigureOpenGL()
        {
            GL.Enable(EnableCap.CullFace); // don't cull faces we can't see
            GL.Enable(EnableCap.Blend); // dunno?
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // some sort of bleding for transparency(?)
            GL.FrontFace(FrontFaceDirection.Ccw); // counter-clockwise faces are ignored(?)
        }


        // Components

        public class Renderer
        {
            public Material material;
            public Mesh mesh;
        }

        /// <summary>
        /// Holds a Shader, and displays instanced variables for it.
        /// (Per-Material shader instance control control)
        /// </summary>
        public class Material
        {
            // creating a new material defaults to Lit.
            public Material()
            {
                // shader = Renderer.DefaultLit();
            }

            public Shader shader;
        }

        /// <summary>
        /// Holds data about how things should be rendered, shaded, textured, and colored.
        /// </summary>
        public class Shader
        {

        }
    }

    public class EntitySystem
    {
        /// <summary>
        /// The base class for every object.
        /// Serves as a 'GameObject' component.
        /// </summary>
        public class Entity
        {
            public string name;
            public Transform transform;
        }

        /// <summary>
        /// Controls the position, rotation, and scale of an entity, and additionally all of it's children.
        /// </summary>
        public class Transform
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }
    }

    public class Physics
    {
        /// <summary>
        /// The amount of gravity objects will experience in M/s.
        /// </summary>
        public static Vector3 gravity = new Vector3(0f, 9.80665f, 0f);
        // not fully implemented

        public void StartSimulation()
        {
            throw new Exception("Fuck naw");
        }
    }

    public class Audio
    {
        /// <summary>
        /// Plays Audio in both 2D stereo space and 3D world space.
        /// </summary>
        public class Speaker
        {
            public AudioTrack? track;
            public float volume;
            public float pitch;

            public float spatialMix = 0f;
            // not implemented
        }

        /// <summary>
        /// A generalized class for all supported audio types. (.mp3, .wav, etc.)
        /// </summary>
        public class AudioTrack
        {
            // not implemented
            /*
            public AudioCodec codec { get; private set; }
            public byte[] audioData;
            */
        }
    }

    public class Editor
    {
        public static bool isProjectOpen;
        // the currently open project.
        public static string activeProjectPath = "";

        // the default location new projects are created at.
        public static string defaultProjectPath = "";

        /// <summary>
        /// Initialiezes core User and Editor data.
        /// </summary>
        public static void EditorInit()
        {
            // Configure and Assign the Default Project Path.
            // Eventually support settings like a custom path.
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dP = Path.Combine(programData, "Limeko/Projects");
            if(!Directory.Exists(dP)) Directory.CreateDirectory(dP);
            defaultProjectPath = dP;
        }

        /// <summary>
        /// Loads an existing Project, given one is not open.
        /// Internal Method--Don't call directly!
        /// </summary>
        /// <param name="path"></param>
        public static void LoadProject(string path)
        {

        }

        /// <summary>
        /// Unloads the currently open Project, given one is open.
        /// Internal Method--Don't call directly!
        /// </summary>
        public static void UnloadProject()
        {

        }

        public static class SplashScreen
        {
            public static void Show()
            {
                // No Logic
            }
        }

        public static class Utils
        {
            public static string GetActiveProjectPath()
            {
                string? path = Editor.isProjectOpen && !string.IsNullOrEmpty(Editor.activeProjectPath) ? Editor.activeProjectPath : null;
                return path;
            }

            public static string GetDefaultProjectPath()
            {
                string? path = !string.IsNullOrEmpty(Editor.defaultProjectPath) ? Editor.defaultProjectPath : null;
                return path;
            }

            public static class Misc
            {
                public static void PrintLimeko(bool spacer)
                {
                    if (spacer) Console.WriteLine("");
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
                    if (spacer) Console.WriteLine("");
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
        }
    }
}