"use strict";

interface MonoPlatform
{
    getObjectFieldsBaseAddress(address : number);
    readInt32Field(baseAddress : number, offset? : number);
    readFloatField(baseAddress : number, offset? : number);
    toJavaScriptString(address : number);
    findMethod(assemblyName: string, namespace: string, className: string, methodName: string) : any;
    callMethod(method: any, target: null, args: any[]) : any;
}

var Module : {
    stackSave() : any,
    stackRestore(stack : any);
    stackAlloc(bytes : number) : number;
    setValue(handle : number, value : any, type : string);
}

var Blazor : { 
    registerFunction(name : string, func : (...args : any[]) => any),
    invokeDotNetMethod(methodInfo : { type : { assembly : string, name : string }, method : { name : string }}, ...args : any[]),
    platform : MonoPlatform
}

function timestamp()
{
    return window.performance && window.performance.now ? window.performance.now() : new Date().getTime();
}

(function()
{
    interface CanvasContext
    {
        Loop : boolean,
        Context : WebGLRenderingContext;
        Shaders : WebGLShader[];
        Programs : { 
            Program : WebGLProgram, 
            Uniforms : {
                [id : string] : WebGLUniformLocation
            }
        }[],
        Buffers : WebGLBuffer[],
        Textures : WebGLTexture[]
    }

    var canvasContexts : CanvasContext[] = [];

    Blazor.registerFunction("Blazor.WebGL.WebGLContext.RegisterCanvas", registerCanvasFromId); 
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.RegisterCanvasElement", registerCanvas); 
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.InvokeUnmarshalled", invokeUnmarshalled);
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.Invoke", invoke);
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.InvokeWithContext", invokeWithContext);
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.CreateShader", createShader);
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.CreateProgram", createProgram);
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.CreateBuffer", createBuffer);
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.StartLoop", startLoop);
    Blazor.registerFunction("Blazor.WebGL.WebGLContext.StopLoop", stopLoop);
    Blazor.registerFunction("Blazor.GameBase.Start", gameStart);

    var dt, last;
    
    function frame(time)
    {
        dt = (time - last) / 1000;
        //console.log("1: " + time);
        
        try
        {
            Blazor.invokeDotNetMethod({
                    type: {
                        assembly: 'Blazor.WebGL',
                        name: 'Blazor.GameBase'
                    },
                    method: {
                        name: 'StaticUpdate'
                    }
                },
                dt);
        }
        catch(ex)
        {
            console.log(ex);
        }

        last = time;

        requestAnimationFrame(frame);
    }

    // ------ Looping -------------------
    var canvasContextLooping = false;
    var staticUpdateMethodHandle = null;

    function contextLoop(time : number)
    {
        if(!staticUpdateMethodHandle)
            staticUpdateMethodHandle = Blazor.platform.findMethod("Blazor.WebGL", "Blazor.WebGL", "WebGLContext", "StaticUpdate");

        try
        {
            for(var i = 0; i < canvasContexts.length; i++)
            {
                if(!canvasContexts[i].Loop)
                    continue;

                // Blazor.invokeDotNetMethod({
                //         type: {
                //             assembly: 'Blazor.WebGL',
                //             name: 'Blazor.WebGL.WebGLContext'
                //         },
                //         method: {
                //             name: 'StaticUpdate'
                //         }
                //     },
                //     i, time);

                const stack = Module.stackSave();

                try 
                {
                    const idHandle = Module.stackAlloc(4);
                    const timeHandle = Module.stackAlloc(4);

                    Module.setValue(idHandle, i, "i32");
                    Module.setValue(timeHandle, time, "float");

                    Blazor.platform.callMethod(staticUpdateMethodHandle, null, [idHandle, timeHandle]);
                } 
                finally 
                {
                  Module.stackRestore(stack);
                }
            }
        }
        catch(ex)
        {
            console.log(ex);
        }

        if(canvasContextLooping)
            requestAnimationFrame(contextLoop);
    }

    function startLoop(id : number)
    {
        canvasContexts[id].Loop = true;

        if(!canvasContextLooping)
        {
            canvasContextLooping = true;
            requestAnimationFrame(contextLoop);
        }   
    }

    function stopLoop(id : number)
    {
        canvasContexts[id].Loop = false;
        var isAnyLooping = false;

        for(var i = 0 ; i < canvasContexts.length; i++)
            if(canvasContexts[i].Loop)
            {
                isAnyLooping = true;
                break;
            }

        if(!isAnyLooping)
            canvasContextLooping = false;
    }
    // ----------------------------------------

    function gameStart()
    {
        requestAnimationFrame(frame); // start the first frame
    }
    
    function resolveArguments(id : number, args : any[])
    {
        const context = canvasContexts[id];

        for(var i = 0; i < args.length; i++)
        {
            const arg : { id : any, handleType : number, type : number } = args[i];
            
            if(arg.handleType != 0x22334455)
                continue;

            if(arg.type == 0)
                args[i] = context.Shaders[arg.id];
            else if(arg.type == 1)
                args[i] = context.Programs[arg.id].Program;
            else if(arg.type == 2)
                args[i] = context.Buffers[arg.id];
            else if(arg.type == 3)
                args[i] = new Float32Array(arg.id);
            else if(arg.type == 4)
            {
                const programId = arg.id[0];
                const name = arg.id[1];
                const uniforms = context.Programs[programId].Uniforms;

                if(!uniforms[name])
                    uniforms[name] = context.Context.getUniformLocation(context.Programs[programId].Program, name);
                    
                args[i] = uniforms[name];
            }
            else if(arg.type == 5)
                args[i] = new Int32Array(arg.id);
            else if(arg.type == 6)
                args[i] = new Uint16Array(arg.id);
        }
    }

    function findFreeSpotInArray(array : any[]) : number
    {
        var freeId = -1;

        for(var i = 0; i < array.length; i++)
        {
            if(array[i] == null)
            {
                freeId = i;
                break;
            }
        }

        return freeId;
    }

    function putInFreeSpotInArray<T>(array : T[], value : T) : number
    {
        var freeId = findFreeSpotInArray(array);

        if(freeId != -1)
        {
            array[freeId] = value;
            return freeId;
        }
        
        array.push(value);
        return array.length - 1;
    }

    function registerCanvas(canvas : HTMLCanvasElement) : number
    {
        var context = canvas.getContext("webgl");

        return putInFreeSpotInArray(canvasContexts, 
        { 
            Context: context, 
            Shaders: [], 
            Programs: [], 
            Buffers: [],
            Textures: []
        });
    }

    function registerCanvasFromId(id : string) : number
    { 
        var canvas = <HTMLCanvasElement>document.getElementById(id); 
        
        return registerCanvas(canvas);
    }

    function invokeUnmarshalled(canvasId : number, methodId : number)
    {
        canvasId = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(canvasId));
        methodId = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(methodId));

        const contextHolder = canvasContexts[canvasId]; 
        const argumentStart = 2;

        const isPowerOf2 = function(value) {
            return (value & (value - 1)) == 0;
        };

        if(methodId == 0) // clearDepth
        {
            var mode = Blazor.platform.readFloatField(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            contextHolder.Context.clearDepth(mode);
        }
        else if(methodId == 1) // clearColor
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));

            contextHolder.Context.clearColor(
                (mode & 0x000000FF) / 255.0, 
                ((mode & 0x0000FF00) >> 8) / 255.0, 
                ((mode & 0x00FF0000) >> 16) / 255.0, 
                (((mode & 0xFF000000) >> 24) & 0xFF) / 255.0);
        }
        else if(methodId == 2) // enable
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));

            contextHolder.Context.enable(mode);
        }
        else if(methodId == 3) // depthFunc
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));

            contextHolder.Context.depthFunc(mode);
        }
        else if(methodId == 4) // clear
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));

            contextHolder.Context.clear(mode);
        }
        else if(methodId == 5) // viewport
        {
            var mode = Blazor.platform.readFloatField(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            var offset = Blazor.platform.readFloatField(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));
            var type = Blazor.platform.readFloatField(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));
            var count = Blazor.platform.readFloatField(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 3]));

            contextHolder.Context.viewport(mode, offset, type, count);
        }
        else if(methodId == 6) // vertexAttribPointer
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            var offset = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));
            var type = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));
            var count = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 3]));
            var arg5 = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 4]));
            var arg6 = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 5]));

            contextHolder.Context.vertexAttribPointer(mode, offset, type, count, arg5, arg6);
        }
        else if(methodId == 7) // enableVertexAttribArray
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));

            contextHolder.Context.enableVertexAttribArray(mode);
        }
        else if(methodId == 8) // useProgram
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));

            contextHolder.Context.useProgram(contextHolder.Programs[mode].Program);
        }
        else if(methodId == 9) // bindBuffer
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            var offset = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));

            contextHolder.Context.bindBuffer(mode, contextHolder.Buffers[offset]);
        }
        else if(methodId == 10) // drawArrays
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            var offset = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));
            var type = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));

            contextHolder.Context.drawArrays(mode, offset, type);
        }
        else if(methodId == 11) // uniformMatrix4fv
        {
            var programId = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            var name = Blazor.platform.toJavaScriptString(arguments[argumentStart + 1]);
            var type = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));
            
            var uniforms = contextHolder.Programs[programId].Uniforms;

            if(!uniforms[name])
                uniforms[name] = contextHolder.Context.getUniformLocation(contextHolder.Programs[programId].Program, name);

            var arg4ArrayStart = Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 3]) + 8;
            var count : any = new Float32Array(16);

            for(var i = 0; i < 16; i++)
                count[i] = Blazor.platform.readFloatField(arg4ArrayStart, i * 4);

            contextHolder.Context.uniformMatrix4fv(uniforms[name], type, count);
        }
        else if(methodId == 12) // drawElements
        {
            var mode = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            var offset = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));
            var type = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));
            var count = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 3]));

            contextHolder.Context.drawElements(mode, count, type, offset);
        }
        else if(methodId == 13) // loadTexture
        {
            const context = contextHolder.Context;
            const uri = Blazor.platform.toJavaScriptString(arguments[argumentStart]);  

            const texture = context.createTexture();
            context.bindTexture(context.TEXTURE_2D, texture);
          
            // Because images have to be download over the internet
            // they might take a moment until they are ready.
            // Until then put a single pixel in the texture so we can
            // use it immediately. When the image has finished downloading
            // we'll update the texture with the contents of the image.
            const level = 0;
            const internalFormat = context.RGBA;
            const width = 1;
            const height = 1;
            const border = 0;
            const srcFormat = context.RGBA;
            const srcType = context.UNSIGNED_BYTE;
            const pixel = new Uint8Array([0, 0, 255, 255]);  // opaque blue
            context.texImage2D(context.TEXTURE_2D, level, internalFormat,
                          width, height, border, srcFormat, srcType,
                          pixel);
          
            const progressHandle = arguments[argumentStart + 1];

            const image = new Image();
            image.onload = function() 
            {
                context.bindTexture(context.TEXTURE_2D, texture);
                context.texImage2D(context.TEXTURE_2D, level, internalFormat,
                            srcFormat, srcType, image);
          
                // WebGL1 has different requirements for power of 2 images
                // vs non power of 2 images so check if the image is a
                // power of 2 in both dimensions.
                if (isPowerOf2(image.width) && isPowerOf2(image.height)) {
                    // Yes, it's a power of 2. Generate mips.
                    context.generateMipmap(context.TEXTURE_2D);
                } else {
                    // No, it's not a power of 2. Turn of mips and set
                    // wrapping to clamp to edge
                    context.texParameteri(context.TEXTURE_2D, context.TEXTURE_WRAP_S, context.CLAMP_TO_EDGE);
                    context.texParameteri(context.TEXTURE_2D, context.TEXTURE_WRAP_T, context.CLAMP_TO_EDGE);
                    context.texParameteri(context.TEXTURE_2D, context.TEXTURE_MIN_FILTER, context.LINEAR);
                }

                const method = Blazor.platform.findMethod("Blazor.WebGL", "Blazor.WebGL", "WebGLContext", "SetProgress");
                
                var stack = Module.stackSave();

                try
                {
                    var widthHandle = Module.stackAlloc(4);
                    var heightHandle = Module.stackAlloc(4);
                    Module.setValue(widthHandle, image.naturalWidth, "i32");
                    Module.setValue(heightHandle, image.naturalHeight, "i32");

                    Blazor.platform.callMethod(method, null, [progressHandle, widthHandle, heightHandle]);
                }
                finally
                {
                    Module.stackRestore(stack);
                }        
            };
            image.src = uri;
          
            return putInFreeSpotInArray(contextHolder.Textures, texture);
        }
        else if(methodId == 14) // bindTexture
        {
            const id = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            
            contextHolder.Context.bindTexture(contextHolder.Context.TEXTURE_2D, contextHolder.Textures[id]);
        }
        else if(methodId == 15) // activeTexture
        {
            const number = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            
            contextHolder.Context.activeTexture(number);
        }
        else if(methodId == 16) // uniform[1234][fi][v]
        {
            const programId = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            const locationId = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));
            const dimensions = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));
            const type = Blazor.platform.toJavaScriptString(arguments[argumentStart + 3]);

            const args = [];

            for(var i = argumentStart + 4; i < argumentStart + 4 + dimensions; i++)
                args.push(Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[i])));

            if(type == "i" || type == "f")
            {
                args.unshift(contextHolder.Programs[programId].Uniforms[locationId]);
                contextHolder.Context["uniform" + dimensions + type].apply(contextHolder.Context, args);
            }
            else if(type == "iv")
                contextHolder.Context["uniform" + dimensions + type].call(contextHolder.Context, 
                    contextHolder.Programs[programId].Uniforms[locationId], new Int32Array(args));
            else if(type == "fv")
                contextHolder.Context["uniform" + dimensions + type].call(contextHolder.Context, 
                    contextHolder.Programs[programId].Uniforms[locationId], new Float32Array(args));
        }
        else if(methodId == 17) // createTexture
        {
            const context = contextHolder.Context;

            const width = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            const height = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));
            const textureFormat = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));

            const texture = context.createTexture();
            context.bindTexture(context.TEXTURE_2D, texture);
          
            const srcFormat = context.RGBA;
            const srcType = context.UNSIGNED_BYTE;
            const pixel = new Uint8Array(width * height * 4); 

            context.texImage2D(context.TEXTURE_2D, 0, textureFormat, width, height, 0, srcFormat, srcType, pixel);
        
            // WebGL1 has different requirements for power of 2 images
            // vs non power of 2 images so check if the image is a
            // power of 2 in both dimensions.
            if (isPowerOf2(width) && isPowerOf2(height)) {
                // Yes, it's a power of 2. Generate mips.
                context.generateMipmap(context.TEXTURE_2D);
            } else {
                // No, it's not a power of 2. Turn of mips and set
                // wrapping to clamp to edge
                context.texParameteri(context.TEXTURE_2D, context.TEXTURE_WRAP_S, context.CLAMP_TO_EDGE);
                context.texParameteri(context.TEXTURE_2D, context.TEXTURE_WRAP_T, context.CLAMP_TO_EDGE);
                context.texParameteri(context.TEXTURE_2D, context.TEXTURE_MIN_FILTER, context.LINEAR);
            }

            return putInFreeSpotInArray(contextHolder.Textures, texture);
        }
        else if(methodId == 18) // setData
        {
            const context = contextHolder.Context;

            const textureId = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart]));
            const width = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 1]));
            const height = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 2]));
            const textureFormat = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 3]));
            
            const srcFormat = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 4]));
            const srcType = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 5]));

            var dataCount = Blazor.platform.readInt32Field(Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 6]) + 4);
            var dataStart = Blazor.platform.getObjectFieldsBaseAddress(arguments[argumentStart + 6]) + 8;

            var texture = contextHolder.Textures[textureId];
            var pixel = new Uint8Array(width * height * 4);

            context.bindTexture(context.TEXTURE_2D, texture);

            for(var i = 0; i < dataCount; i++)
            {
                var data = Blazor.platform.readInt32Field(dataStart + i * 4);
                pixel[i * 4] = data & 0xFF;
                pixel[i * 4 + 1] = (data & 0xFF00) >> 8;
                pixel[i * 4 + 2] = (data & 0xFF0000) >> 16;
                pixel[i * 4 + 3] = ((data & 0xFF000000) >> 24) & 0xFF;
            }
            
            context.texImage2D(context.TEXTURE_2D, 0, textureFormat, width, height, 0, srcFormat, srcType, pixel);

            // WebGL1 has different requirements for power of 2 images
            // vs non power of 2 images so check if the image is a
            // power of 2 in both dimensions.
            if (isPowerOf2(width) && isPowerOf2(height)) {
                // Yes, it's a power of 2. Generate mips.
                context.generateMipmap(context.TEXTURE_2D);
            } else {
                // No, it's not a power of 2. Turn of mips and set
                // wrapping to clamp to edge
                context.texParameteri(context.TEXTURE_2D, context.TEXTURE_WRAP_S, context.CLAMP_TO_EDGE);
                context.texParameteri(context.TEXTURE_2D, context.TEXTURE_WRAP_T, context.CLAMP_TO_EDGE);
                context.texParameteri(context.TEXTURE_2D, context.TEXTURE_MIN_FILTER, context.LINEAR);
            }
        }
    }

    function invoke(id: number, method: string, args: object[])
    {
        resolveArguments(id, args);

        var context = canvasContexts[id].Context;
        return context[method].apply(context, args);
    }

    function invokeWithContext(id: number, contextType: number, contextId, method: string, args: object[])
    {
        resolveArguments(id, args);
        var webGlContext = canvasContexts[id].Context;

        if(contextType == 0) // shaders
        {
            var shader = canvasContexts[id].Shaders[contextId];
            args.unshift(shader);
         
            if(method == "deleteShader")
                canvasContexts[id].Shaders[contextId] = null;
         
            return webGlContext[method].apply(webGlContext, args);
        }
        else if(contextType == 1) // programs
        {
            var program = canvasContexts[id].Programs[contextId];

            if(method == "deleteProgram")
                canvasContexts[id].Programs[contextId] = null;

            if(method == "linkProgram")
            {
                args.unshift(program.Program);
                var result = webGlContext[method].apply(webGlContext, args);

                var attributeCount = webGlContext.getProgramParameter(program.Program, webGlContext.ACTIVE_ATTRIBUTES);

                for(var i = 0; i < attributeCount; i++)
                {
                    console.log(webGlContext.getActiveAttrib(program.Program, i).name);
                }

                var attributeCount = webGlContext.getProgramParameter(program.Program, webGlContext.ACTIVE_UNIFORMS);

                for(var i = 0; i < attributeCount; i++)
                {
                    console.log(webGlContext.getActiveUniform(program.Program, i).name);
                }

                return result;
            }

            args.unshift(program.Program);
            return webGlContext[method].apply(webGlContext, args);
        }
        else if(contextType == 2) // buffers
        {
            var buffer = canvasContexts[id].Buffers[contextId];
            args.unshift(buffer);

            if(method == "deleteBuffer")
                canvasContexts[id].Buffers[contextId] = null;

            return webGlContext[method].apply(webGlContext, args);
        }

    }

    function createShader(id : number, shaderType: number) : number
    {
        var shader = canvasContexts[id].Context.createShader(shaderType);

        return putInFreeSpotInArray(canvasContexts[id].Shaders, shader);
    }

    function createProgram(id : number) : number
    {
        var program = canvasContexts[id].Context.createProgram();

        return putInFreeSpotInArray(canvasContexts[id].Programs, { Program: program, Uniforms: {} });
    }

    function createBuffer(id : number) : number
    {
        var buffer = canvasContexts[id].Context.createBuffer();
        
        return putInFreeSpotInArray(canvasContexts[id].Buffers, buffer);
    }
})();