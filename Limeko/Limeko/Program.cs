using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Limeko
{
    public class Core
    {
        public static Window WindowInstance { get; private set; }

        public static void Main()
        {
            Console.Title = "Limeko Console";
            WindowInstance = new Window();
            WindowInstance.Run();
        }

        public class Window : GameWindow
        {
            float _deltaTime;
            float _fixedDeltaTime;

            public static Vector2 WindowSize;

            Vector2 _lastMouse;
            bool _firstMove = true;
            float _sensitivity = 0.15f;

            static int targetFrameRate = 90;


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
                Console.WriteLine("<--> Starting Editor (Internal) <-->");

                WindowSize = new Vector2(Size.X, Size.Y);


                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.FrontFace(FrontFaceDirection.Ccw);



                Console.WriteLine("> Setting background");
                GL.ClearColor(0.63f, 1f, 0.3f, 1f);

                Console.WriteLine("OpenGL Core Running.");




                Console.Clear();
                // Utils.Misc.PrintLimeko(true);
                // Utils.Misc.PrintLicenseDisclaimer();

                Console.WriteLine("LIMEKO!!!!!!!!!!");
                Console.WriteLine("OpenGL: Running & Configured");
            }

            protected override void OnUnload()
            {
                base.OnUnload();
                // _shader.Dispose();
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base .OnUpdateFrame(e);
                // Runs every frame.
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
}