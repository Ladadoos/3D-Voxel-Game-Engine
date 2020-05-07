using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    abstract class Shader
    {
        private readonly int programID;
        private readonly int vertexShaderID;
        private readonly int fragmentShaderID;

        protected Shader(string vertexFile, string fragmentFile)
        {
            vertexShaderID = LoadShader(vertexFile, ShaderType.VertexShader);
            fragmentShaderID = LoadShader(fragmentFile, ShaderType.FragmentShader);
            programID = GL.CreateProgram();
            GL.AttachShader(programID, vertexShaderID);
            GL.AttachShader(programID, fragmentShaderID);
            BindAttributes();
            GL.LinkProgram(programID);
            GL.ValidateProgram(programID);
            GetAllUniformLocations();
        }

        protected abstract void GetAllUniformLocations();

        protected int GetUniformLocation(string uniform)
        {
            return GL.GetUniformLocation(programID, uniform);
        }

        public void Start()
        {
            GL.UseProgram(programID);
        }

        public void Stop()
        {
            GL.UseProgram(0);
        }

        public void CleanUp()
        {
            Stop();
            GL.DetachShader(programID, vertexShaderID);
            GL.DetachShader(programID, fragmentShaderID);
            GL.DeleteShader(vertexShaderID);
            GL.DeleteShader(fragmentShaderID);
            GL.DeleteProgram(programID);
        }

        protected abstract void BindAttributes();

        protected void BindAttribute(int attribute, string variableName)
        {
            GL.BindAttribLocation(programID, attribute, variableName);
        }

        public void LoadFloat(int location, float value)
        {
            GL.Uniform1(location, value);
        }

        public void LoadInt(int location, int value)
        {
            GL.Uniform1(location, value);
        }

        public void LoadVector(int location, Vector3 vector)
        {
            GL.Uniform3(location, vector);
        }

        public void LoadBoolean(int location, bool value)
        {
            float toLoad = 0;
            if (value)
            {
                toLoad = 1;
            }
            GL.Uniform1(location, toLoad);
        }

        public void LoadMatrix(int location, Matrix4 matrix)
        {
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void LoadTexture(int uniformLocation, int textureUnitLayout, int textureId, TextureTarget target = TextureTarget.Texture2D)
        {
            GL.Uniform1(uniformLocation, textureUnitLayout);
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnitLayout);
            GL.BindTexture(target, textureId);
        }

        public static int LoadShader(string file, ShaderType type)
        {
            string source = System.IO.File.ReadAllText(file);
            int shaderID = GL.CreateShader(type);
            GL.ShaderSource(shaderID, source);
            GL.CompileShader(shaderID);

            GL.GetShader(shaderID, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                Logger.Error("Could not compile shader: " + file);
                Logger.Error(GL.GetShaderInfoLog(shaderID));
            }

            return shaderID;
        }
    }
}
