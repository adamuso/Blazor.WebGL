using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazor.WebGL;
using Blazor.WebGL.Math;
using WebGame.Graphics;

namespace WebGame
{

    public class TestGame : GameBase 
    {
        private int drawCallCount;
        private SpriteBatch spriteBatch;
        private Material material;
        private SpriteDrawable[] drawables;

        private int benchmarkElementCount;
        private int benchmarkFrameCount;
        private float benchmarkSummedFrameTime;

        public Texture2D Texture1 { get; set; }
        public Texture2D Texture2  { get; set; }
        public SpriteManager SpriteManager { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            spriteBatch = new SpriteBatch(Context, 200);
            spriteBatch.Flushed += (sender, e) => drawCallCount++;    

            benchmarkElementCount = 100;    
        }

        protected override async Task LoadContent()
        {
            material = new Material();
            
            string vsSource = @"
                attribute vec4 aPosition;
                attribute vec2 aTexture;
                attribute vec4 aColor;
                attribute float aSamplerIndex;

                uniform mat4 uProjectionMatrix;

                varying mediump vec4 vColor;
                varying highp vec2 vTextureCoord;
                varying mediump float vSamplerIndex;

                void main(void) {
                    gl_Position = uProjectionMatrix * aPosition;
                    vTextureCoord = aTexture;
                    vColor = aColor;
                    vSamplerIndex = aSamplerIndex;
                }";
                
            string fsSource = @"
                varying highp vec2 vTextureCoord;
                varying mediump vec4 vColor;
                varying mediump float vSamplerIndex;

                uniform sampler2D uSampler0;
                uniform sampler2D uSampler1;
                uniform sampler2D uSampler2;
                uniform sampler2D uSampler3;
                uniform sampler2D uSampler4;
                uniform sampler2D uSampler5;
                uniform sampler2D uSampler6;
                uniform sampler2D uSampler7;
                uniform sampler2D uSampler8;
                uniform sampler2D uSampler9;

                void main(void) {

                    mediump vec4 color;
                
                    if(vSamplerIndex > -0.5 && vSamplerIndex < 0.5)
                    {
                        color = texture2D(uSampler0, vTextureCoord);
                    }
                    if(vSamplerIndex > 0.5 && vSamplerIndex < 1.5)
                    {
                        color = texture2D(uSampler1, vTextureCoord);
                    }
                    if(vSamplerIndex > 1.5 && vSamplerIndex < 2.5)
                    {
                        color = texture2D(uSampler2, vTextureCoord);
                    }
                    if(vSamplerIndex > 2.5 && vSamplerIndex < 3.5)
                    {
                        color = texture2D(uSampler3, vTextureCoord);
                    }
                    if(vSamplerIndex > 3.5 && vSamplerIndex < 4.5)
                    {
                        color = texture2D(uSampler4, vTextureCoord);
                    }
                    if(vSamplerIndex > 4.5 && vSamplerIndex < 5.5)
                    {
                        color = texture2D(uSampler5, vTextureCoord);
                    }
                    if(vSamplerIndex > 5.5 && vSamplerIndex < 6.5)
                    {
                        color = texture2D(uSampler6, vTextureCoord);
                    }
                    if(vSamplerIndex > 6.5 && vSamplerIndex < 7.5)
                    {
                        color = texture2D(uSampler7, vTextureCoord);
                    }
                    if(vSamplerIndex > 7.5 && vSamplerIndex < 8.5)
                    {
                        color = texture2D(uSampler8, vTextureCoord);
                    }
                    if(vSamplerIndex > 8.5 && vSamplerIndex < 9.5)
                    {
                        color = texture2D(uSampler9, vTextureCoord);
                    }
                        
                    gl_FragColor = color * vColor;
                }";

            var vertexShader = Context.CompileShader(ShaderType.VERTEX_SHADER, vsSource);
            var fragmentShader = Context.CompileShader(ShaderType.FRAGMENT_SHADER, fsSource);

            material.Program = Context.CreateShaderProgram();
            material.Program.Attach(vertexShader);
            material.Program.Attach(fragmentShader);

            material.Program.Link();

            Texture1 = await Context.LoadTextureAsync("res/birb.jpg");
            Texture2 = await Context.LoadTextureAsync("res/nyan.png");

            material.Textures = new[] { Texture1 };

            // material.Int32Uniforms = new Dictionary<string,int>() 
            // { 
            //     { "uSampler0", 0 },
            //     { "uSampler1", 1 },
            //     { "uSampler2", 2 },
            //     { "uSampler3", 3 },
            //     { "uSampler4", 4 },
            //     { "uSampler5", 5 },
            //     { "uSampler6", 6 },
            //     { "uSampler7", 7 },
            //     { "uSampler8", 8 },
            //     { "uSampler9", 9 }
            // };

            material.Matrix4Uniforms = new Dictionary<string, Matrix4>() 
            {
                { 
                    "uProjectionMatrix", 
                    Matrix4.Orthogonal(-Context.Width * 0.5f, -Context.Height * 0.5f, 
                        Context.Width * 0.5f, Context.Height * 0.5f, -1, 1)
                }
            };

            drawables = new SpriteDrawable[benchmarkElementCount];
            Random random = new Random();

            for(int i = 0; i < benchmarkElementCount; i++)
            {
                drawables[i] = new SpriteDrawable()
                {
                    Color = new Color(1, 1, 1, 1),
                    Size = new Vector2(100, 100),
                    SourceRectangle = null,
                    Texture = Texture1,
                    Transform = TransformUtils.MakeTransform(
                        Vector3.Zero,
                        new Vector3(
                            (float)random.NextDouble() * Context.Width - Context.Width * 0.5f,
                            (float)random.NextDouble() * Context.Height - Context.Height * 0.5f, 
                            0
                        ),
                        new Vector3(0, 0, (float)(random.NextDouble() * Math.PI)),
                        Vector3.One 
                    )
                };
            }

            SpriteManager = new SpriteManager();
        }
        
        protected override void Draw(float delta)
        {
            drawCallCount = 0;

            Context.ClearColor(new Color(0, 0, 0.5f, 1.0f));
            Context.ClearDepth(1.0f);
            Context.Enable(WebGLOption.DEPTH_TEST);
            Context.Enable(WebGLOption.BLEND);
            Context.DepthFunction(DepthFunction.LEQUAL);
            Context.BlendFunc(BlendFactor.SRC_ALPHA, BlendFactor.ONE_MINUS_SRC_ALPHA);

            Context.Clear(ClearBuffer.COLOR_BUFFER_BIT | ClearBuffer.DEPTH_BUFFER_BIT);

            Matrix4 projectionMatrix = Matrix4.Orthogonal(-Context.Width * 0.5f, -Context.Height * 0.5f, 
                Context.Width * 0.5f, Context.Height * 0.5f, -1.0f, 1.0f);

            spriteBatch.Begin(material);

            foreach(var drawable in drawables)
                spriteBatch.Draw(drawable); 
            
            SpriteManager.Draw(spriteBatch);

            spriteBatch.End();

            //Console.WriteLine(drawCallCount);
            //Console.WriteLine(1.0f / delta);

            benchmarkFrameCount++;
            benchmarkSummedFrameTime += delta;

            if(benchmarkSummedFrameTime > 10)
            {
                Console.WriteLine("FPS: " + 1 / (benchmarkSummedFrameTime / benchmarkFrameCount));
                benchmarkFrameCount = 0;
                benchmarkSummedFrameTime = 0;
            }
        }

        protected override void Update(float delta)
        {
            SpriteManager.Update(delta);
        }
    }
}