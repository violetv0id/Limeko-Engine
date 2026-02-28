using BepuPhysics.Collidables;
using OpenTK.Mathematics;
using Limeko;
using OpenTK;
using Limeko.Entities;
using Limeko.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
            Vector3 rotatedOffset =
                Vector3.Transform(Offset.Position,
                AttachedEntity.Transform.Rotation);


            return
                Matrix4.CreateScale(AttachedEntity.Transform.Scale * Offset.Scale) *
                Matrix4.CreateFromQuaternion(AttachedEntity.Transform.Rotation * Offset.Rotation) *
                Matrix4.CreateTranslation(AttachedEntity.Transform.Position + rotatedOffset);
        }
    }
}

namespace Limeko.Rendering
{
    public class Material
    {
        public Shader _Shader;
        // display shader parameters
    }
}