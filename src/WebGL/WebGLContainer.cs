using System;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Layouts;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Blazor.WebGL
{
    public class WebGLContainer : BlazorLayoutComponent
    {

        protected ElementRef Canvas { get; set; }

        public WebGLContext Context { get; private set; }

        [Parameter]
        public int Width { get; set; }
        [Parameter]
        public int Height { get; set; }

        public WebGLContainer()
        {
            Context = new WebGLContext();
            Width = 400;
            Height = 400;
        }

        protected override void OnAfterRender()
        {
            Context.Initialize(Canvas, Width, Height);
            
            Context.ClearColor(new Color(0, 0, 0, 1));
            Context.Enable(WebGLOption.DEPTH_TEST);
            Context.DepthFunction(DepthFunction.LEQUAL);
            Context.Clear(ClearBuffer.COLOR_BUFFER_BIT | ClearBuffer.DEPTH_BUFFER_BIT);
        }
    }
}