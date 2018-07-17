using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TestGame.Shared;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Layouts;
using Blazor.WebGL;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace WebGame
{
    public class GameBase : IComponent, IHandleAfterRender, IHandleEvent
    {
        #region Statics            
        private static GameBase game;
        private float lastTime;

        public static void StaticUpdate(float delta)
        { 
            if(game == null || game.Context == null)
                return;

            game.Draw(delta);
            game.Update(delta);
        }
        #endregion

        private bool isInitialized;
        
        protected WebGLContext Context { get; private set; }
        protected WebGLCanvas Canvas { get; set; }

        public GameBase()
        {
            if(game != null)
                throw new Exception("There can be only one game running at the time");

            game = this;
        
            renderFragment = BuildRenderTree;
        }

        protected virtual void Initialize()
        {

        }

        protected virtual Task LoadContent()
        {
            return null;
        }

        protected virtual void Draw(float delta)
        {
            
        }

        protected virtual void Update(float delta)
        {

        }

        // protected sealed override void OnAfterRender()
        // {
        //     Context = Canvas.Context;           
        // }

        // protected override void BuildRenderTree(Microsoft.AspNetCore.Blazor.RenderTree.RenderTreeBuilder builder) => base.BuildRenderTree(builder);

        // protected sealed override void OnInit() => base.OnInit();

        // protected sealed override void OnParametersSet() => base.OnParametersSet();

        // protected sealed override Task OnAfterRenderAsync() 
        // {
        //     return Task.Run(async () =>
        //     {
        //         InitializeInternal();

        //         await LoadContent();

        //         Context.IsLoopingEnabled = true;
        //         Context.Updated += UpdateInternal;
        //         //RegisteredFunction.Invoke<object>("WebGame.GameBase.Start");
        //     });
        // }

        // protected sealed override Task OnInitAsync() => base.OnInitAsync();

        // protected sealed override Task OnParametersSetAsync() => base.OnParametersSetAsync();

        // protected sealed override bool ShouldRender() => base.ShouldRender();

        
        private void UpdateInternal(float time)
        { 
            float delta = (time - lastTime) / 1000.0f;

            if(game == null || game.Context == null)
                return;

            game.Draw(delta);
            game.Update(delta);

            lastTime = time;
        }

        private void InitializeInternal()
        {
            isInitialized = true;

            Initialize();
        }

        #region Component implementation

        // TODO: Should a GameBase implement IComponent (or derrive from BlazorComponent) or should there 
        //       be a separate component which takes specific game class type as argument and handles all 
        //       the blazor component logic? 

        private RenderFragment renderFragment;
        private RenderHandle renderHandle;

        protected virtual void BuildRenderTree(RenderTreeBuilder builder)
        {
            
        }

        void IComponent.Init(RenderHandle renderHandle)
        {
            this.renderHandle = renderHandle;
        }

        void IComponent.SetParameters(ParameterCollection parameters)
        {
            // parameters.AssignToProperties(this);
            // Full, static method syntax is used because VS Code with Omnisharp cannot resolve the
            // extension method properly and is always marking it as an error. 
            // (But it is obviously not one ¯\_(ツ)_/¯). 
            ParameterCollectionExtensions.AssignToProperties(parameters, this);

            this.renderHandle.Render(renderFragment);
        }

        void IHandleAfterRender.OnAfterRender()
        {
            Context = Canvas.Context;

            Task.Run(async () =>
            {
                InitializeInternal();

                await LoadContent();

                Context.IsLoopingEnabled = true;
                Context.Updated += UpdateInternal;
                //RegisteredFunction.Invoke<object>("WebGame.GameBase.Start");
            });
        }

        void IHandleEvent.HandleEvent(EventHandlerInvoker binding, UIEventArgs args)
        {
            if(args is UIKeyboardEventArgs keyboardArgs)
            {
                
            }
        }
        #endregion
    }
}