using BepuPhysics.Collidables;
using Limeko;
using Limeko.Entities;
using Limeko.Graphics;
using Limeko.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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

        public static void CreateObject(Entity entity, Mesh mesh, Material material)
        {
            Graphics.RenderingCore.Register(new RenderObject
            {
                Mesh = mesh,
                AttachedEntity = entity,
                Material = material
            });
        }

        public static void CreateObject(Entity entity, Mesh mesh, Material material, Transform offset)
        {
            Graphics.RenderingCore.Register(new RenderObject
            {
                Mesh = mesh,
                AttachedEntity = entity,
                Material = material
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
                // ---------- FRONT (+Z)
                -0.5f,-0.5f, 0.5f,  0,0,1,  0,0,
                 0.5f,-0.5f, 0.5f,  0,0,1,  1,0,
                 0.5f, 0.5f, 0.5f,  0,0,1,  1,1,
                 0.5f, 0.5f, 0.5f,  0,0,1,  1,1,
                -0.5f, 0.5f, 0.5f,  0,0,1,  0,1,
                -0.5f,-0.5f, 0.5f,  0,0,1,  0,0,

                // ---------- BACK (-Z)
                -0.5f,-0.5f,-0.5f,  0,0,-1, 1,0,
                -0.5f, 0.5f,-0.5f,  0,0,-1, 1,1,
                 0.5f, 0.5f,-0.5f,  0,0,-1, 0,1,
                 0.5f, 0.5f,-0.5f,  0,0,-1, 0,1,
                0.5f,-0.5f,-0.5f,  0,0,-1, 0,0,
                -0.5f,-0.5f,-0.5f,  0,0,-1, 1,0,

                // ---------- LEFT (-X)
                -0.5f, 0.5f, 0.5f, -1,0,0,  1,1,
                -0.5f, 0.5f,-0.5f, -1,0,0,  0,1,
                -0.5f,-0.5f,-0.5f, -1,0,0,  0,0,
                -0.5f,-0.5f,-0.5f, -1,0,0,  0,0,
                -0.5f,-0.5f, 0.5f, -1,0,0,  1,0,
                -0.5f, 0.5f, 0.5f, -1,0,0,  1,1,

                // ---------- RIGHT (+X)
                 0.5f, 0.5f, 0.5f,  1,0,0,  0,1,
                 0.5f,-0.5f,-0.5f,  1,0,0,  1,0,
                 0.5f, 0.5f,-0.5f,  1,0,0,  1,1,
                 0.5f,-0.5f,-0.5f,  1,0,0,  1,0,
                 0.5f, 0.5f, 0.5f,  1,0,0,  0,1,
                 0.5f,-0.5f, 0.5f,  1,0,0,  0,0,

                // ---------- TOP (+Y)
                -0.5f, 0.5f,-0.5f,  0,1,0,  0,1,
                -0.5f, 0.5f, 0.5f,  0,1,0,  0,0,
                 0.5f, 0.5f, 0.5f,  0,1,0,  1,0,
                 0.5f, 0.5f, 0.5f,  0,1,0,  1,0,
                 0.5f, 0.5f,-0.5f,  0,1,0,  1,1,
                -0.5f, 0.5f,-0.5f,  0,1,0,  0,1,

                // ---------- BOTTOM (-Y)
                -0.5f,-0.5f,-0.5f,  0,-1,0, 0,1,
                 0.5f,-0.5f, 0.5f,  0,-1,0, 1,0,
                -0.5f,-0.5f, 0.5f,  0,-1,0, 0,0,
                 0.5f,-0.5f, 0.5f,  0,-1,0, 1,0,
                -0.5f,-0.5f,-0.5f,  0,-1,0, 0,1,
                 0.5f,-0.5f,-0.5f,  0,-1,0, 1,1,
            };
        }

        public static float[] Quad()
        {
            return new float[]
            {
                // position   normal    uv
                -1f,-1f,0f,   0,0,1,   0,0,
                 1f,-1f,0f,   0,0,1,   1,0,
                 1f, 1f,0f,   0,0,1,   1,1,

                 1f, 1f,0f,   0,0,1,   1,1,
                -1f, 1f,0f,   0,0,1,   0,1,
                -1f,-1f,0f,   0,0,1,   0,0,
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
                tl.X, tl.Y, 0f,  0,0,1,  0,0,
                tr.X, tr.Y, 0f,  0,0,1,  1,0,
                br.X, br.Y, 0f,  0,0,1,  1,1,

                br.X, br.Y, 0f,  0,0,1,  1,1,
                bl.X, bl.Y, 0f,  0,0,1,  0,1,
                tl.X, tl.Y, 0f,  0,0,1,  0,0,
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
            _vertexCount = vertices.Length / 8;

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.StaticDraw);

            int stride = 8 * sizeof(float);

            // position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // normal
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float,
                false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // uv
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float,
                false, stride, 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);
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
        public Material Material;

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
        public Shader Shader;

        // textures
        public int BaseTexture = 0;

        // parameters
        public Vector3 TintColor = Vector3.One;
        public float Ambient = 0.1f;

        public Material(Shader shader)
        {
            Shader = shader;

            // default values
            BaseTexture = 0;          // means "no texture"
            TintColor = Vector3.One;  // no tint
            Ambient = 0.15f;          // nicer default lighting
        }

        public void Bind()
        {
            Shader.Use();

            // --- textures ---
            if(BaseTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, BaseTexture);
                Shader.SetInt("uBaseMap", 0);
            }

            // --- uniforms ---
            Shader.SetVector3("uTintColor", TintColor);
            Shader.SetFloat("uAmbient", Ambient);
        }
    }
}

