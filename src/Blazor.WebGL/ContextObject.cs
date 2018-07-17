using System;

namespace Blazor.WebGL
{
    public class ContextObject
    {
        public int HandleType { get { return 0x22334455; } }
        public int Type { get; set; }
        public object Id { get; set; }

        public ContextObject(int contextType, object contextId)
        {
            this.Type = contextType;
            this.Id = contextId;
        }

        public ContextObject(ContextType contextType, object data)
        {
            Type = (int)contextType;
            Id = data;
        }
    }

    public enum ContextType
    {
        Shader = 0,
        ShaderProgram = 1,
        Buffer = 2,
        WrapIntoFloat32Array = 3,
        UniformLocation = 4,
        WrapIntoInt32Array = 5,
        WrapIntoUInt16Array = 6
    }
}