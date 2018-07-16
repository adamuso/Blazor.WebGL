# Blazor.WebGL 

Library for Blazor wrapping the WebGL methods for 3D rendering in canvas.

## Usage

Add to your _ViewImports.cshtml.

```cs
@using Blazor.WebGL
@addTagHelper *, Blazor.WebGL
```

Use the component.

```cs
@inherits TestProject.MyComponent

<WebGLCanvas ref="@Canvas"/>
```

Get the WebGLContext and use it for your needs.

```cs
namespace TestProject
{
    public class MyComponent : BlazorComponent
    {
        protected WebGLCanvas Canvas { get; set; }

        protected override void OnAfterRender()
        {
            var context = Canvas.Context;

            context.ClearColor(new Color(0.5f, 0.5f, 0.5f, 1));
            context.Enable(WebGLOption.DEPTH_TEST);
            context.DepthFunction(DepthFunction.LEQUAL);
            context.Clear(ClearBuffer.COLOR_BUFFER_BIT | ClearBuffer.DEPTH_BUFFER_BIT);
        }
    }
}
```