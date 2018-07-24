using System;
using System.Collections.Generic;
using Blazor.WebGL;
using Blazor.WebGL.Math;

namespace WebGame.Graphics
{
    public class RenderBatch<T, TDescriptor> 
        where T : struct, IVertexFormat
        where TDescriptor : IVertexFormatDescriptor, new()
    {
        private readonly WebGLContext context;
        private readonly IVertexFormatDescriptor descriptor;

        private int itemCount;
        private int indicesCount;
        private DefaultVertexFormat[] itemPool;

        private WebGLBuffer modelBuffer;
        private WebGLBuffer indicesBuffer;

        private Material material;

        private int textureCount;
        private Texture2D[] textures;

        public event EventHandler Flushed;

        public RenderBatch(WebGLContext context, int elementCount)
        {
            this.context = context;
            this.descriptor = new TDescriptor();
            itemPool = new DefaultVertexFormat[elementCount * descriptor.ElementVertexCount];

            modelBuffer = context.CreateBuffer(BufferType.ARRAY_BUFFER, BufferUsage.STREAM_DRAW);
            modelBuffer.Bind();
            modelBuffer.SetDataSize(itemPool.Length * descriptor.ByteSize);

            System.Console.WriteLine("Buffer size: " + itemPool.Length * descriptor.ByteSize);

            indicesBuffer = context.CreateBuffer(BufferType.ELEMENT_ARRAY_BUFFER, BufferUsage.STATIC_DRAW);

            indicesBuffer.Bind();
            indicesBuffer.SetData(descriptor.CreateIndices(elementCount));

            textures = new Texture2D[10];
        }

        float rotation;

        public void Begin(Material material)
        {
            this.material = material;

            itemCount = 0;
            indicesCount = 0;
            textureCount = 0;
        }

        public void End()
        {
            Flush();
        }

        public void Draw(IDrawable<T> drawable)
        {
            var vertices = drawable.Draw();

            bool contains = false;
            int samplerIndex = 0;

            for(int i = 0; i < textureCount; i++)
                if(textures[i] == drawable.Texture)
                {
                    contains = true;
                    samplerIndex = i;
                    break;
                }
                
            if(!contains)
            {
                if(textureCount >= textures.Length)
                    Flush();
                    
                textures[textureCount] = drawable.Texture;
                samplerIndex = textureCount;
                textureCount++;
            }   

            for(int i = 0; i < vertices.Length; i++)
                vertices[i].SamplerIndex = samplerIndex; 

            if(itemCount + vertices.Length > itemPool.Length)
                Flush();
            
            System.Array.Copy(vertices, 0, itemPool, itemCount, vertices.Length);

            itemCount += vertices.Length;
            indicesCount += descriptor.ElementIndicesCount;
        }

        private void Flush()
        {
            modelBuffer.Bind();
            modelBuffer.SetData(itemPool, itemCount);          

            foreach(var attribute in descriptor.AttributeFormats)
            {
                var value = attribute.Value;

                material.Program.Attribute(attribute.Key).VertexAttributePointer(
                    value.ComponentCount,
                    value.ComponentType,
                    value.IsNormalized,
                    value.Stride,
                    value.Offset
                );

                material.Program.Attribute(attribute.Key).EnableVertexAttributeArray();
            }  

            material.Program.Use();

            if(material.Matrix4Uniforms != null)
                foreach(var matrix in material.Matrix4Uniforms)
                {
                    var m = matrix.Value;
                    material.Program.Uniform(matrix.Key).Set(ref m);
                }

            if(material.Int32Uniforms != null)
                foreach(var pair in material.Int32Uniforms)
                {
                    material.Program.Uniform(pair.Key).Set(pair.Value);
                }

            indicesBuffer.Bind();

            for(int i = 0; i < textureCount; i++)
            {
                material.Program.Uniform("uSampler" + i).Set(i);
                context.Textures[i] = textures[i];
            }

            int offset = 0;
            int vertexCount = indicesCount;
            context.DrawElements(WebGLDrawMode.TRIANGLES, offset, WebGLType.UNSIGNED_SHORT, vertexCount);

            itemCount = 0;
            indicesCount = 0;
            textureCount = 0;

            Flushed?.Invoke(this, EventArgs.Empty);
        }
    }
}