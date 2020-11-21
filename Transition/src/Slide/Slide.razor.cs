﻿using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using Skclusive.Material.Core;
using Skclusive.Transition.Component;
using Skclusive.Script.DomHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Skclusive.Material.Transition.TransitionUtil;
using System.Linq;

namespace Skclusive.Material.Transition
{
    public partial class Slide : MaterialComponentBase
    {
        [Inject]
        public DomHelpers DomHelpers { set; get; }

        [Inject]
        public SlideHelper SlideHelper { set; get; }

        [Inject]
        public EventDelegator EventDelegator { set; get; }

        public Slide() : base("Slide")
        {
        }

        /// <summary>
        /// If <c>true</c>, show the component; triggers the enter or exit animation.
        /// </summary>
        [Parameter]
        public bool In { set; get; }

        /// <summary>
        /// The <see cref="Skclusive.Core.Component.Placement" /> that direction the child node will enter from.
        /// </summary>
        [Parameter]
        public Placement Placement { set; get; } = Placement.Bottom;

        /// <summary>
        /// ChildContent of the current component which gets component <see cref="ITransitionContext" />.
        /// </summary>
        [Parameter]
        public RenderFragment<ITransitionContext> ChildContent { get; set; }

        /// <summary>
        /// Callback fired before the Menu enters.
        /// </summary>
        [Parameter]
        public Func<(IReference, bool), Task> OnEnter { set; get; }

        /// <summary>
        /// Callback fired when the Menu is entering.
        /// </summary>
        [Parameter]
        public Func<(IReference, bool), Task> OnEntering { set; get; }

        /// <summary>
        /// Callback fired before the Menu exits.
        /// </summary>
        [Parameter]
        public Func<IReference, Task> OnExit { set; get; }

        /// <summary>
        /// Callback fired when the Menu has exited.
        /// </summary>
        [Parameter]
        public Func<IReference, Task> OnExited { set; get; }

        /// <summary>
        /// slide transition appear.
        /// </summary>
        [Parameter]
        public bool Appear { set; get; } = true;

        /// <summary>
        /// slide transition duration.
        /// </summary>
        [Parameter]
        public int? TransitionDuration { set; get; }

        /// <summary>
        /// slide transition delay.
        /// </summary>
        [Parameter]
        public int TransitionDelay { set; get; }

        /// <summary>
        /// slide transition timeout.
        /// </summary>
        [Parameter]
        public int Timeout { set; get; } = 225;

        /// <summary>
        /// slide appear timeout.
        /// </summary>
        [Parameter]
        public int? AppearTimeout { set; get; }

        /// <summary>
        /// slide enter timeout.
        /// </summary>
        [Parameter]
        public int? EnterTimeout { set; get; } = 225;

        /// <summary>
        /// slide exit timeout.
        /// </summary>
        [Parameter]
        public int? ExitTimeout { set; get; } = 195;

        /// <summary>
        /// By default the child component is mounted immediately along with
        /// the parent <c>Transition</c> component. If you want to "lazy mount" the component on the
        /// first <c>In="true"</c> you can set <c>MountOnEnter</c>. After the first enter transition the component will stay
        /// mounted, even on "exited", unless you also specify <c>UnmountOnExit</c>.
        /// </summary>
        [Parameter]
        public bool MountOnEnter { set; get; }

        /// <summary>
        /// By default the child component stays mounted after it reaches the <c>'exited'</c> state.
        /// Set <c>UnmountOnExit</c> if you'd prefer to unmount the component after it finishes exiting.
        /// </summary>
        [Parameter]
        public bool UnmountOnExit { set; get; }

        protected IReference RefBack { get; } = new Reference();

        protected int GetEnterDuration()
        {
            int duration;

            if (TransitionDuration.HasValue)
            {
                duration = TransitionDuration.Value;
            }
            else if (EnterTimeout.HasValue)
            {
                duration = EnterTimeout.Value;
            }
            else
            {
                duration = Timeout;
            }

            return duration;
        }

        protected int GetExitDuration()
        {
            int duration;

            if (TransitionDuration.HasValue)
            {
                duration = TransitionDuration.Value;
            }
            else if (ExitTimeout.HasValue)
            {
                duration = ExitTimeout.Value;
            }
            else
            {
                duration = Timeout;
            }

            return duration;
        }

        protected IEnumerable<Tuple<string, object>> GetChildStyles(ITransitionContext context)
        {
            return Enumerable.Empty<Tuple<string, object>>();
            // yield return Tuple.Create<string, object>("visibility", context.State == TransitionState.Exited && !In ? "hidden" : "default");
        }

        protected ITransitionContext GetChildContext(ITransitionContext context)
        {
            return new TransitionContextBuilder()
            .With(context)
            .WithRefBack(new DelegateReference(RefBack, context.RefBack))
            .WithStyles(GetChildStyles(context))
            .Build();
        }

        protected async Task HandleEnterAsync((IReference, bool) args)
        {
            (IReference refback, bool appear) = args;

            await SlideHelper.SetSlideTranslateValueAsync(Placement, refback.Current);

            await (OnEnter?.Invoke(args) ?? Task.CompletedTask);
        }

        protected async Task HandleEnteringAsync((IReference, bool) args)
        {
            (IReference refback, bool appearing) = args;

            var transition = CreateTransition("transform", GetEnterDuration(), TransitionDelay, TransitionEasing.EasingOut);

            var styles = new Dictionary<string, object>
            {
                { "transition", transition },
                { "webkitTransition", transition },
                { "transform", "none" },
                { "webkitTransform", "none" }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: true);

            await (OnEntering?.Invoke(args) ?? Task.CompletedTask);
        }

        protected async Task HandleExitAsync(IReference refback)
        {
            var transition = CreateTransition("transform", GetExitDuration(), TransitionDelay, TransitionEasing.EasingSharp);

            var styles = new Dictionary<string, object>
            {
                { "transition", transition },
                { "webkitTransition", transition }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: true);

            await SlideHelper.SetSlideTranslateValueAsync(Placement, refback.Current);

            await (OnExit?.Invoke(refback) ?? Task.CompletedTask);
        }

        protected async Task HandleExitedAsync(IReference refback)
        {
            // No need for transitions when the component is hidden
            var transition = string.Empty;

            var styles = new Dictionary<string, object>
            {
                { "transition", transition },
                { "webkitTransition", transition }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: false);

            await (OnExited?.Invoke(refback) ?? Task.CompletedTask);
        }

        protected override async Task OnAfterUpdateAsync()
        {
            await base.OnAfterUpdateAsync();

            if (!In)
            {
                await SlideHelper.SetSlideTranslateValueAsync(Placement, RefBack.Current);
            }
        }

        protected async Task OnWindowResizeAsync()
        {
            if (!In && Placement != Placement.Bottom && Placement != Placement.End)
            {
                await SlideHelper.SetSlideTranslateValueAsync(Placement, RefBack.Current);
            }
        }

        private void OnWindowResize(object sender, string e)
        {
            _ = OnWindowResizeAsync();
        }

        protected override async Task OnAfterMountAsync()
        {
            await base.OnAfterMountAsync();

            EventDelegator.OnEvent += OnWindowResize;

            // await Task.Delay(2000);

            await EventDelegator.InitAsync(default(ElementReference), "resize", 200);
        }

        protected override ValueTask DisposeAsync()
        {
            EventDelegator.OnEvent -= OnWindowResize;

            return EventDelegator.DisposeAsync();
        }
    }
}
