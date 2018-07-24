using System.Collections.Generic;

namespace Blazor.WebGL
{
    public interface IVertexFormatDescriptor
    {
        int ByteSize { get; }
        int ElementVertexCount { get; }
        int ElementIndicesCount { get; }
        Dictionary<string, VertexAttributeFormat> AttributeFormats { get; set; }
    
        ushort[] CreateIndices(int elementCount);
    }
}