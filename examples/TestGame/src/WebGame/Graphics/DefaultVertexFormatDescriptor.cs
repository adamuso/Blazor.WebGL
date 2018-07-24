using System.Collections.Generic;
using Blazor.WebGL;

namespace WebGame.Graphics
{
    public class DefaultVertexFormatDescriptor : IVertexFormatDescriptor
    {
        public int ByteSize { get { return 40; } }
        public int ElementVertexCount { get { return 4; } }
        public int ElementIndicesCount { get { return 6; } }

        public Dictionary<string, VertexAttributeFormat> AttributeFormats { get; set; }

        public DefaultVertexFormatDescriptor()
        {
            AttributeFormats = new Dictionary<string, VertexAttributeFormat>();

            AttributeFormats["aPosition"] = new VertexAttributeFormat()
            {
                ComponentCount = 3,
                ComponentType = WebGLType.FLOAT,
                IsNormalized = false,
                Stride = 40,
                Offset = 0
            };

            AttributeFormats["aTexture"] = new VertexAttributeFormat()
            {
                ComponentCount = 2,
                ComponentType = WebGLType.FLOAT,
                IsNormalized = false,
                Stride = 40,
                Offset = 12
            };

            AttributeFormats["aColor"] = new VertexAttributeFormat()
            {
                ComponentCount = 4,
                ComponentType = WebGLType.FLOAT,
                IsNormalized = false,
                Stride = 40,
                Offset = 20
            };

            AttributeFormats["aSamplerIndex"] = new VertexAttributeFormat()
            {
                ComponentCount = 1,
                ComponentType = WebGLType.FLOAT,
                IsNormalized = false,
                Stride = 40,
                Offset = 36
            };
        }

        public ushort[] CreateIndices(int elementCount)
        {
            ushort[] indices = new ushort[elementCount * 6];

            for(int i = 0; i < elementCount; i++)
            {
                indices[i * 6 + 0] = (ushort)(0 + i * 4);
                indices[i * 6 + 1] = (ushort)(1 + i * 4);
                indices[i * 6 + 2] = (ushort)(2 + i * 4);
                indices[i * 6 + 3] = (ushort)(2 + i * 4);
                indices[i * 6 + 4] = (ushort)(3 + i * 4);
                indices[i * 6 + 5] = (ushort)(0 + i * 4);
            } 

            return indices;
        }
    }
}