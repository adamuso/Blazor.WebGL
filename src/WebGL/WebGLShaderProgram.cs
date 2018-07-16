using System;
using System.Collections.Generic;

namespace Blazor.WebGL
{
    public class WebGLShaderProgram
    {
        private Dictionary<string, long> locations;

        public int Id { get; private set; }
        internal WebGLContext Context { get; set; }

        public WebGLShaderProgram(WebGLContext context, int id)
        {
            this.Context = context;
            this.Id = id;
            this.locations = new Dictionary<string, long>();
        }

        public void Attach(WebGLShader shader)
        {
            Context.InvokeCanvasMethod<object>(1, Id, "attachShader", new object[] { new ContextObject(0, shader.Id) });
        }

        public long Attribute(string name)
        {
            if(!locations.ContainsKey(name))
                locations.Add(name, Context.InvokeCanvasMethod<long>(1, Id, "getAttribLocation", new object[] { name }));

            return locations[name];
        }

        public WebGLUniformLocation Uniform(string name)
        {
            return new WebGLUniformLocation(this, name);
        }

        public void Link()
        {
            Context.InvokeCanvasMethod<object>(1, Id, "linkProgram", new object[0]);

            var status = Context.InvokeCanvasMethod<bool>(1, Id, "getProgramParameter", new object[] { (int)ProgramParameter.LINK_STATUS });

            if(!status)
            {
                string error = Context.InvokeCanvasMethod<string>(1, Id, "getProgramInfoLog", new object[0]);
                Context.InvokeCanvasMethod<object>(1, Id, "deleteProgram", new object[0]);

                throw new WebGLException("Shader cannot be compiled. Error: \n" + error);
            }
        }

        public void Use()
        {
            Context.UseProgram(this);
        }
    }
}