﻿using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using Skclusive.Material.Core;
using Skclusive.Transition.Component;
using Skclusive.Script.DomHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Skclusive.Material.Transition.TransitionUtil;
using System.Globalization;

namespace Skclusive.Material.Transition
{
    public partial class Collapse : MaterialComponentBase
    {
        [Inject]
        public DomHelpers DomHelpers { set; get; }

        public Collapse() : base("Collapse")
        {
        }

        /// <summary>
        /// If <c>true</c>, show the component; triggers the enter or exit animation.
        /// </summary>
        [Parameter]
        public bool In { set; get; }

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
        /// Callback fired when the Menu has entered.
        /// </summary>
        [Parameter]
        public Func<(IReference, bool), Task> OnEntered { set; get; }

        /// <summary>
        /// Callback fired before the Menu exits.
        /// </summary>
        [Parameter]
        public Func<IReference, Task> OnExit { set; get; }

        /// <summary>
        /// Callback fired when the Menu is exiting.
        /// </summary>
        [Parameter]
        public Func<IReference, Task> OnExiting { set; get; }

        /// <summary>
        /// Callback fired when the Menu has exited.
        /// </summary>
        [Parameter]
        public Func<IReference, Task> OnExited { set; get; }

        /// <summary>
        /// collapse transition duration.
        /// </summary>
        [Parameter]
        public int? TransitionDuration { set; get; }

        /// <summary>
        /// collapse transition delay.
        /// </summary>
        [Parameter]
        public int TransitionDelay { set; get; }

        /// <summary>
        /// collapse transition timeout.
        /// </summary>
        [Parameter]
        public int Timeout { set; get; } = 225;

        /// <summary>
        /// collapse appear timeout.
        /// </summary>
        [Parameter]
        public int? AppearTimeout { set; get; }

        /// <summary>
        /// collapse enter timeout.
        /// </summary>
        [Parameter]
        public int? EnterTimeout { set; get; }

        /// <summary>
        /// collapse exit timeout.
        /// </summary>
        [Parameter]
        public int? ExitTimeout { set; get; }

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

        /// <summary>
        /// The height of the container when collapsed.
        /// </summary>
        [Parameter]
        public int CollapsedHeight { set; get; }

        /// <summary>
        /// Reference attached to the wrapper element of the component.
        /// </summary>
        [Parameter]
        public IReference WrapperRef { get; set; } = new Reference();

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

        protected async Task HandleEnterAsync((IReference, bool) args)
        {
            (IReference refback, bool appear) = args;

            var styles = new Dictionary<string, object>
            {
                { "height", $"{CollapsedHeight.ToString(CultureInfo.InvariantCulture)}px" }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: true);

            await (OnEnter?.Invoke(args) ?? Task.CompletedTask);
        }

        protected async Task HandleEnteringAsync((IReference, bool) args)
        {
            (IReference refback, bool appearing) = args;

            var wrappedHeight = await DomHelpers.GetHeightAsync(WrapperRef.Current, true);

            var styles = new Dictionary<string, object>
            {
                { "height", $"{wrappedHeight.ToString(CultureInfo.InvariantCulture)}px" },
                { "transition-duration", $"{GetEnterDuration()}ms" }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: true);

            await (OnEntering?.Invoke(args) ?? Task.CompletedTask);
        }

        protected async Task HandleEnteredAsync((IReference, bool) args)
        {
            (IReference refback, bool appeared) = args;

            var styles = new Dictionary<string, object>
            {
                { "height", "auto" }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: true);

            await (OnEntered?.Invoke(args) ?? Task.CompletedTask);
        }

        protected async Task HandleExitAsync(IReference refback)
        {
            var wrappedHeight = await DomHelpers.GetHeightAsync(WrapperRef.Current, true);

            var styles = new Dictionary<string, object>
            {
                { "height", $"{wrappedHeight.ToString(CultureInfo.InvariantCulture)}px" }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: true);

            await (OnExit?.Invoke(refback) ?? Task.CompletedTask);
        }

        protected async Task HandleExitingAsync(IReference refback)
        {
            var styles = new Dictionary<string, object>
            {
                { "height", $"{CollapsedHeight.ToString(CultureInfo.InvariantCulture)}px" },
                { "transition-duration", $"{GetExitDuration()}ms" }
            };

            await DomHelpers.SetStyleAsync(refback.Current, styles, trigger: true);

            await (OnExiting?.Invoke(refback) ?? Task.CompletedTask);
        }

        protected Task HandleExitedAsync(IReference refback)
        {
            return OnExited?.Invoke(refback) ?? Task.CompletedTask;
        }

        protected string GetTransition(int duration, int delay)
        {
            var opacity = CreateTransition("opacity", duration, delay, TransitionEasing.EasingSharp);

            var transform = CreateTransition("transform", duration * 0.666, delay, TransitionEasing.EasingSharp);

            return $"{opacity},{transform}";
        }

        protected void SetTransition(IReference refback, int duration, int delay)
        {
            var transition = GetTransition(duration, delay);

            var styles = new Dictionary<string, object>
            {
                { "transition", transition },
                { "webkitTransition", transition }
            };

            SetStyle(refback, styles, true);
        }

        protected void SetStyle(IReference refback, IDictionary<string, object> styles, bool trigger = false)
        {
            _ = DomHelpers.SetStyleAsync(refback.Current, styles, trigger);
        }
    }
}
