using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazor.WebGL;
using Blazor.WebGL.Math;

namespace WebGame
{
    public class TestGame : GameBase 
    {
        private WebGLShaderProgram program;
        private WebGLBuffer cubeBuffer;
        private WebGLBuffer colorsBuffer;
        private WebGLBuffer indicesBuffer;
        private Texture2D texture;
        private float rotation;

        protected override void Initialize()
        {
            base.Initialize();

            string vsSource = @"
                attribute vec4 aVertexPosition;
                attribute vec2 aTextureCoord;

                uniform mat4 uModelViewMatrix;
                uniform mat4 uProjectionMatrix;

                varying highp vec2 vTextureCoord;

                void main(void) {
                    gl_Position = uProjectionMatrix * uModelViewMatrix * aVertexPosition;
                    vTextureCoord = aTextureCoord;
                }";
                
            string fsSource = @"
                varying highp vec2 vTextureCoord;

                uniform sampler2D uSampler;

                void main(void) {
                    gl_FragColor = texture2D(uSampler, vTextureCoord);
                }";

            var vertexShader = Context.CompileShader(ShaderType.VERTEX_SHADER, vsSource);
            var fragmentShader = Context.CompileShader(ShaderType.FRAGMENT_SHADER, fsSource);

            program = Context.CreateShaderProgram();
            program.Attach(vertexShader);
            program.Attach(fragmentShader);

            Console.WriteLine("t1");
            program.Link();

            Console.WriteLine("t2");
            cubeBuffer = InitializeBuffers();
        }

        protected override async Task LoadContent()
        {
            texture = await Context.LoadTextureAsync("res/birb.jpg");
        
            //texture = new Texture2D(Context, 2, 2);
            //texture.SetData(new Color[] { new Color(1, 0, 0, 1), new Color(0, 1, 0, 1), new Color(0, 0, 1, 1), new Color(1, 1, 0, 1) });
        }

        private WebGLBuffer InitializeBuffers()
        {
            // Create a buffer for the square's positions.

            Console.WriteLine("t3");
            var buffer = Context.CreateBuffer(BufferType.ARRAY_BUFFER);

            // Select the positionBuffer as the one to apply buffer
            // operations to from here out.

            Console.WriteLine("t4");
            buffer.Bind();

            // Now create an array of positions for the square.

            var positions = new float [] 
            {
                // Front face
                -1.0f, -1.0f,  1.0f,
                1.0f, -1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                
                // Back face
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                1.0f,  1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                
                // Top face
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                1.0f,  1.0f, -1.0f,
                
                // Bottom face
                -1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,
                
                // Right face
                1.0f, -1.0f, -1.0f,
                1.0f,  1.0f, -1.0f,
                1.0f,  1.0f,  1.0f,
                1.0f, -1.0f,  1.0f,
                
                // Left face
                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,
            };

            // Now pass the list of positions into WebGL to build the
            // shape. We do this by creating a Float32Array from the
            // JavaScript array, then use it to fill the current buffer.

            Console.WriteLine("t5");
            buffer.SetData(positions, BufferUsage.STATIC_DRAW);

            colorsBuffer = Context.CreateBuffer(BufferType.ARRAY_BUFFER);

            float[] faceColors = {
                // Front
                0.0f,  0.0f,
                1.0f,  0.0f,
                1.0f,  1.0f,
                0.0f,  1.0f,
                // Back
                0.0f,  0.0f,
                1.0f,  0.0f,
                1.0f,  1.0f,
                0.0f,  1.0f,
                // Top
                0.0f,  0.0f,
                1.0f,  0.0f,
                1.0f,  1.0f,
                0.0f,  1.0f,
                // Bottom
                0.0f,  0.0f,
                1.0f,  0.0f,
                1.0f,  1.0f,
                0.0f,  1.0f,
                // Right
                0.0f,  0.0f,
                1.0f,  0.0f,
                1.0f,  1.0f,
                0.0f,  1.0f,
                // Left
                0.0f,  0.0f,
                1.0f,  0.0f,
                1.0f,  1.0f,
                0.0f,  1.0f,
            };

            // Convert the array of colors into a table for all the vertices.
            colorsBuffer.Bind();
            colorsBuffer.SetData(faceColors, BufferUsage.STATIC_DRAW);

            indicesBuffer = Context.CreateBuffer(BufferType.ELEMENT_ARRAY_BUFFER);

            // This array defines each face as two triangles, using the
            // indices into the vertex array to specify each triangle's
            // position.

            ushort[] indices = {
                0,  1,  2,      0,  2,  3,    // front
                4,  5,  6,      4,  6,  7,    // back
                8,  9,  10,     8,  10, 11,   // top
                12, 13, 14,     12, 14, 15,   // bottom
                16, 17, 18,     16, 18, 19,   // right
                20, 21, 22,     20, 22, 23,   // left
            };

            // Now send the element array to GL
            indicesBuffer.Bind();
            indicesBuffer.SetData(indices, BufferUsage.STATIC_DRAW);

            return buffer;
        }

        protected override void Draw(float delta)
        {
            Context.ClearColor(new Color(0, 0, 0.5f, 1.0f));
            Context.ClearDepth(1.0f);
            Context.Enable(WebGLOption.DEPTH_TEST);
            Context.DepthFunction(DepthFunction.LEQUAL);

            Context.Clear(ClearBuffer.COLOR_BUFFER_BIT | ClearBuffer.DEPTH_BUFFER_BIT);

            float fieldOfView = 45.0f * (float)Math.PI / 180.0f;   // in radians
            float aspect = Context.Width / Context.Height;
            float zNear = 0.1f;
            float zFar = 100.0f;

            Matrix4 projectionMatrix = Matrix4.Perspective(fieldOfView, aspect, zNear, zFar);
        
            Matrix4 modelViewMatrix = Matrix4.Identity;
            modelViewMatrix.Translate(new Vector3(-0.0f, 0.0f, -6.0f));
            modelViewMatrix.Rotate(rotation * 2f, new Vector3(0, 1, 0.7f));

            {
                int numComponents = 3;  // pull out 2 values per iteration
                WebGLType type = WebGLType.FLOAT;    // the data in the buffer is 32bit floats
                bool normalize = false;  // don't normalize
                int stride = 0;         // how many bytes to get from one set of values to the next
                                        // 0 = use type and numComponents above
                int offset = 0;         // how many bytes inside the buffer to start from

                cubeBuffer.Bind();

                program.Attribute("aVertexPosition").VertexAttributePointer(
                    numComponents,
                    type,
                    normalize,
                    stride,
                    offset);

                program.Attribute("aVertexPosition").EnableVertexAttributeArray();
            }

              {
                int numComponents = 2;
                WebGLType type = WebGLType.FLOAT;
                bool normalize = false;
                int stride = 0;
                int offset = 0;

                colorsBuffer.Bind();

                program.Attribute("aTextureCoord").VertexAttributePointer(
                    numComponents,
                    type,
                    normalize,
                    stride,
                    offset);

                program.Attribute("aTextureCoord").EnableVertexAttributeArray();
            }

            program.Use();

            program.Uniform("uProjectionMatrix").Set(projectionMatrix);
            program.Uniform("uModelViewMatrix").Set(modelViewMatrix);
        
            indicesBuffer.Bind();

            if(texture == null)
                return;

            Context.Textures[0] = texture;
            program.Uniform("uSampler").Set(0);

            {
                int offset = 0;
                int vertexCount = 36;
                Context.DrawElements(WebGLDrawMode.TRIANGLE_STRIP, offset, WebGLType.UNSIGNED_SHORT, vertexCount);
            }
        }

        protected override void Update(float delta)
        {
            //Console.WriteLine(delta);

            rotation += delta;
        }
    }
}