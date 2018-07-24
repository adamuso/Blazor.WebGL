using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Blazor.WebGL.Math;

namespace WebAssembly
{
    internal static class Runtime
    {
        // The exact namespace, type, and method names must match the corresponding entry in
        // driver.c in the Mono distribution

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern TRes InvokeJSWithArgs<TRes>(int handle, string method, object[] args, ref int exception);
    }   
}

namespace Blazor.WebGL
{    
    public class WebGLContext
    {
        #region Statics

        private static Dictionary<int, WebGLContext> contexts;

        static WebGLContext()
        {
            contexts = new Dictionary<int, WebGLContext>();
        }

        public static void StaticUpdate(int id, float time)
        { 
            if(!contexts.ContainsKey(id))
                return;

            contexts[id].Update(time);
        }

        #endregion

        private const string MethodPrefix = "Blazor.WebGL.WebGLContext.";

        private bool loopingEnabled;
        private Rectangle viewport;
        private WebGLBuffer currentlyBoundArrayBuffer;
        private WebGLBuffer currentlyBoundElementArrayBuffer;
        private WebGLShaderProgram currentlyUsedShaderProgram;

        private object[] invokeArguments;

        private int Id { get; set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Rectangle Viewport 
        {
            get { return viewport; }
            set 
            {
                SetViewport(Viewport);
                viewport = value;
            }
        }

        public ActiveTexture2DCollection Textures { get; private set; }

        public bool IsLoopingEnabled 
        { 
            get { return loopingEnabled; }
            set 
            {
                if(value)
                    RegisteredFunction.Invoke<object>(MethodPrefix + "StartLoop", Id);
                else
                    RegisteredFunction.Invoke<object>(MethodPrefix + "StopLoop", Id);

                loopingEnabled = value;
            } 
        }
        
        public event Action<float> Updated;

        internal void Initialize(ElementRef canvas, int width, int height)
        {
            Width = width;
            Height = height;

            Textures = new ActiveTexture2DCollection(this, 32);

            Id = RegisteredFunction.Invoke<int>(MethodPrefix + "RegisterCanvasElement", canvas);
            contexts.Add(Id, this);

            invokeArguments = new object[16];
        }

        #region Base

        public void ClearColor(Color color)
        {
            //RegisteredFunction.InvokeUnmarshalled<int, uint, object>(MethodPrefix + "InvokeUnmarshalled", 1, color.ToUInt32());
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.ClearColor, color.ToUInt32());

            //InvokeCanvasMethod("clearColor", new object[] { color.R, color.G, color.B, color.A });      
        }

        public void ClearDepth(float value)
        {
            //RegisteredFunction.InvokeUnmarshalled<int, int, object>(MethodPrefix + "InvokeUnmarshalled", 0, (int)value);
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.ClearDepth, value);
            //InvokeCanvasMethod("clearDepth", new object[] { value });
        }

        public void Enable(WebGLOption option)
        {
            int o = (int)option;
            //RegisteredFunction.InvokeUnmarshalled<int, int, object>(MethodPrefix + "InvokeUnmarshalled", 2, o);
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.Enable, o);
            //InvokeCanvasMethod("enable", new object[] { (int)option });
        }

        public void DepthFunction(DepthFunction function)
        {
            int f = (int)function;
            //RegisteredFunction.InvokeUnmarshalled<int, int, object>(MethodPrefix + "InvokeUnmarshalled", 3, f);
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.DepthFunction, f);
            //InvokeCanvasMethod("depthFunc", new object[] { (int)function });
        }

        public void Clear(ClearBuffer what)
        {
            int w = (int)what;
            //RegisteredFunction.InvokeUnmarshalled<int, int, object>(MethodPrefix + "InvokeUnmarshalled", 4, w);
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.Clear, w);
            //InvokeCanvasMethod("clear", new object[] { (int)what });
        }
        
        internal void SetViewport(Rectangle rectangle)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.Viewport, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            //InvokeCanvasMethod("viewport", new object[] { rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height });
        }

        public void BlendFunc(BlendFactor sourceFactor, BlendFactor destinationFactor)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.BlendFunc, sourceFactor, destinationFactor);
        }

        #endregion

        #region Shaders
        public WebGLShader CompileShader(ShaderType type, string source)
        {
            int shaderId = RegisteredFunction.Invoke<int>(MethodPrefix + "CreateShader", Id, (int)type); 

            InvokeCanvasMethod<object>(0, shaderId, "shaderSource", new object[] { source });
            InvokeCanvasMethod<object>(0, shaderId, "compileShader", new object[0]);
            
            var status = InvokeCanvasMethod<bool>(0, shaderId, "getShaderParameter", 
                new object[] { (int)ShaderParameter.COMPILE_STATUS });

            if(!status)
            {
                string error = InvokeCanvasMethod<string>(0, shaderId, "getShaderInfoLog", new object[0]);

                InvokeCanvasMethod<object>(0, shaderId, "deleteShader", new object[0]);

                throw new WebGLException("Shader cannot be compiled. Error: \n" + error);
            }

            return new WebGLShader(this, shaderId);           
        }

        public WebGLShaderProgram CreateShaderProgram()
        {
            int programId = RegisteredFunction.Invoke<int>(MethodPrefix + "CreateProgram", Id); 

            return new WebGLShaderProgram(this, programId);
        }
        internal void SetProgramUniform(WebGLUniformLocation location, int dimensions, string type, object value)
        {
            if(currentlyUsedShaderProgram != location.Program)
                throw new WebGLException("Program of this location is not bound.");

            if(dimensions < 1 || dimensions > 4)
                throw new ArgumentOutOfRangeException("dimensions");

            object[] args = new object[4 + dimensions];
            args[0] = location.Program.Id;
            args[1] = location.Name;
            args[2] = dimensions;
            args[3] = type;

            if(type == "f" || type == "fv")
            {
                if(value is float[] && ((float[])value).Length == dimensions)
                {
                    Array.Copy((float[])value, 0, args, 4, dimensions);
                }
                else if(value is float)
                {
                    args[4] = value;
                }
                else
                    throw new ArgumentException("Values is invalid for the specified type and dimensions", "value");

            }
            else if(type == "i" || type == "iv")
            {
                if(value is int[] && ((int[])value).Length == dimensions)
                {
                    Array.Copy((int[])value, 0, args, 4, dimensions);
                }
                else if(value is int)
                {
                    args[4] = value;
                }
                else
                    throw new ArgumentException("Values is invalid for the specified type and dimensions", "value");
            }
            else
                throw new ArgumentException("Type is invalid", "type");
                
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.Uniform1234fiv, args);
        }

        internal void UniformMatrix4fv(WebGLUniformLocation location, bool transpose, ref Matrix4 value)
        {
            int transposeInt = transpose ? 1 : 0;
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.UniformMatrix4fv, location.Program.Id, location.Name, transpose, value.ToArray());
            //InvokeCanvasMethod("uniformMatrix4fv", new object[] { location, transpose, new ContextObject(ContextType.WrapIntoFloat32Array, value.ToArray()) });
        }

        internal void UseProgram(WebGLShaderProgram program)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.UseProgram, program.Id);
            
            // InvokeCanvasMethod<object>((int)ContextType.ShaderProgram, program.Id, "useProgram", new object[0]);
            currentlyUsedShaderProgram = program;
        }

        #endregion

        #region Buffers
        
        public WebGLBuffer CreateBuffer(BufferType type, BufferUsage usage) 
        {
            int bufferId = RegisteredFunction.Invoke<int>(MethodPrefix + "CreateBuffer", Id); 

            return new WebGLBuffer(this, bufferId, type, usage);
        }

        internal void BindBuffer(WebGLBuffer buffer)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.BindBuffer, buffer.Type, buffer.Id);

            //InvokeCanvasMethod("bindBuffer",  new object[] { (int)buffer.Type, new ContextObject(2, buffer.Id )});

            if(buffer.Type == BufferType.ARRAY_BUFFER)
                currentlyBoundArrayBuffer = buffer;
            else if(buffer.Type == BufferType.ELEMENT_ARRAY_BUFFER)
                currentlyBoundElementArrayBuffer = buffer;
        }

        internal void BufferData(WebGLBuffer buffer, float[] data)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.BufferData, buffer.Type, data, buffer.Usage);

            // InvokeCanvasMethod("bufferData", 
            //     new object[] { (int)buffer.Type, new ContextObject(ContextType.WrapIntoFloat32Array, data), (int)usage });
        }

        internal void BufferData(WebGLBuffer buffer, int size)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.BufferDataSize, buffer.Type, size, buffer.Usage);

            // InvokeCanvasMethod("bufferData", 
            //     new object[] { (int)buffer.Type, new ContextObject(ContextType.WrapIntoFloat32Array, data), (int)usage });
        }

        internal void BufferData<T>(WebGLBuffer buffer, T[] data, int elementByteSize, 
            int length) 
            where T : struct
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.BufferDataStruct, buffer.Type, 
                elementByteSize, data, length, buffer.Usage);
        }

        internal void BufferData(WebGLBuffer buffer, ushort[] data)
        {
            BufferData(buffer, data, 2, data.Length);
        }

        internal void VertexAttributePointer(long index, int size, WebGLType type, bool normalized, int stride, int offset)
        {
            int normalizedInt = normalized ? 0 : 1;

            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.VertexAttributePointer, index, size, type, normalizedInt, stride, offset);
            // InvokeCanvasMethod("vertexAttribPointer", new object[] { index, size, (int)type, normalized, stride, offset });
        }

        internal void EnableVertexAttributeArray(long index)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.EnableVertexAttributeArray, index);
            //InvokeCanvasMethod("enableVertexAttribArray", new object[] { index });
        }

        #endregion

        #region Textures

        internal class LoadTextureProgress
        {
            public int Status { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public LoadTextureProgress(int test)
            {
                Status = test;
            }
        }

        internal static void SetProgress(LoadTextureProgress progress, int width, int height)
        {
            progress.Status = 1;
            progress.Width = width;
            progress.Height = height;
        }

        internal int CreateTexture(int width, int height, PixelFormat format)
        {
            return InvokeCanvasMethodUnmarshalled<int>(UnmarshalledCanvasMethod.CreateTexture, width, height, format);
        }

        internal void SetTextureData(Texture2D texture, int width, int height, PixelFormat format, PixelFormat sourceFormat, PixelType soruceType, int[] data)
        {
            if(data.Length != width * height)
                throw new ArgumentException("Data length must be equal to width times height", nameof(data));

            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.SetTextureData, texture.Id, width, height, format, sourceFormat, soruceType, data);
        }


        public Task<Texture2D> LoadTextureAsync(string uri)
        {
            return Task.Run(async () =>
            {
                LoadTextureProgress progress = new LoadTextureProgress(2137);
                Console.WriteLine("Status: " + progress.Status);
                int textureId = InvokeCanvasMethodUnmarshalled<int>(UnmarshalledCanvasMethod.LoadTexture, uri, progress);

                while(progress.Status != 1)
                    await Task.Delay(10);

                Console.WriteLine("Status: " + progress.Status);
    
                Console.WriteLine("Loaded");
                Texture2D texture = new Texture2D(this, textureId, progress.Width, progress.Height);

                return texture;
            });
        }

        internal void BindTexture(Texture2D texture)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.BindTexture, texture.Id);
        }

        internal void ActiveTexture(WebGLTextureIndex index)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.ActiveTexture, index);   
        }

        #endregion

        public void DrawArrays(WebGLDrawMode mode, int offset, int vertexCount)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.DrawArrays, mode, offset, vertexCount);
            //InvokeCanvasMethod("drawArrays", new object[] { (int)mode, offset, vertexCount });
        }

        public void DrawElements(WebGLDrawMode mode, int offset, WebGLType type, int vertexCount)
        {
            InvokeCanvasMethodUnmarshalled<object>(UnmarshalledCanvasMethod.DrawElements, mode, offset, type, vertexCount);
            //InvokeCanvasMethod("drawArrays", new object[] { (int)mode, offset, vertexCount });
        }

        private void Update(float time)
        {
            if(Updated != null)
                Updated(time);
        }

        internal T InvokeCanvasMethod<T>(int contextType, object contextId, string method, object[] args)
        {
            return RegisteredFunction.Invoke<T>(MethodPrefix + "InvokeWithContext", Id, contextType, contextId, method, args);
        }

        private T InvokeCanvasMethodUnmarshalled2<T>(UnmarshalledCanvasMethod method, params object[] args)
        {
            object[] realArgs = new object[args.Length + 2];
            Array.Copy(args, 0, realArgs, 2, args.Length);
            realArgs[0] = Id;
            realArgs[1] = (int)method;

            // return RegisteredFunction.InvokeUnmarshalled<T>(MethodPrefix + "InvokeUnmarshalled", realArgs);
            return RegisteredFunction.InvokeUnmarshalled<T>("G", realArgs);
        }

        private T InvokeCanvasMethodUnmarshalled<T>(UnmarshalledCanvasMethod method, params object[] args)
        {
            invokeArguments[0] = Id;
            invokeArguments[1] = (int)method;
            Array.Copy(args, 0, invokeArguments, 2, args.Length);

            int a = 10;
            return WebAssembly.Runtime.InvokeJSWithArgs<T>(0, null, invokeArguments, ref a);
        }

        private enum UnmarshalledCanvasMethod : int
        {
            ClearDepth = 0,
            ClearColor = 1,
            Enable = 2,
            DepthFunction = 3,
            Clear = 4,
            Viewport = 5,
            VertexAttributePointer = 6,
            EnableVertexAttributeArray = 7,
            UseProgram = 8,
            BindBuffer = 9,
            DrawArrays = 10,
            UniformMatrix4fv = 11,
            DrawElements = 12,
            LoadTexture = 13,
            BindTexture = 14,
            ActiveTexture = 15,
            Uniform1234fiv = 16,
            CreateTexture = 17,
            SetTextureData = 18,
            BufferData = 19,
            BufferDataSize = 20,
            BufferDataStruct = 21,
            BlendFunc = 22
        }
    }
}