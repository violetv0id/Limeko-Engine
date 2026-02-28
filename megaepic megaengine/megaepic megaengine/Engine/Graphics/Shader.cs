using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Xml.Linq;

namespace Limeko.Graphics
{
    public class Shader
    {
        public int Handle { get; private set; }

        public Shader(string vertPath, string fragPath)
        {
            string vertSource = File.ReadAllText(vertPath);
            string fragSource = File.ReadAllText(fragPath);

            // --- Vertex shader ---
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertSource);
            GL.CompileShader(vertexShader);
            CheckShader(vertexShader);

            // --- Fragment shader ---
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragSource);
            GL.CompileShader(fragmentShader);
            CheckShader(fragmentShader);

            // --- Program ---
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
                throw new Exception(GL.GetProgramInfoLog(Handle));

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }

        public void SetFloat(string name, float value)
        {
            int location = Graphics.Utils.GetUniformLocation(name, Handle);
            if(location != -1) GL.Uniform1(location, value);
        }

        public void SetColor(string name, Vector3 rgb)
        {
            int location = Graphics.Utils.GetUniformLocation(name, Handle);
            if(location != -1) GL.Uniform3(location, rgb);
        }

        public void SetMatrix4(string name, Matrix4 value)
        {
            int location = Graphics.Utils.GetUniformLocation(name, Handle);
            if (location != -1)
                GL.UniformMatrix4(location, false, ref value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            int location = Graphics.Utils.GetUniformLocation(name, Handle);
            if(location != -1) GL.Uniform3(location, value);
        }

        public void SetInt(string name, int value)
        {
            GL.Uniform1(Utils.GetUniformLocation(name, Handle), value);
        }

        private void CheckShader(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if(success == 0) throw new Exception(GL.GetShaderInfoLog(shader));
        }
    }
}