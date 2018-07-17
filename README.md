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

## Building

You will need: 
* [.NET Core 2.1.300](https://www.microsoft.com/net/download/windows) (or newer)
* [Node.js](https://nodejs.org/en/)
* typescript (type `npm install -g typescript` in a console after installing Node.js)
* [Visual Studio Code](https://code.visualstudio.com/download)

Open repository in VS Code. Use `Ctrl+Shift+B` to run build task and choose `build - Blazor.WebGL` task. 

You are ready to go! Now go and build [examples](examples/TestGame). 
