﻿//-------------------------------------------------------------------------------
// <copyright file="SuccessfulTransitionWithExecutedActionsTestBase.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System.Collections.Generic;
    using Appccelerate.StateMachine.Extensions;
    using Appccelerate.StateMachine.Machine.ActionHolders;
    using FluentAssertions;
    using Xunit;

    public abstract class SuccessfulTransitionWithExecutedActionsTestBase : TransitionTestBase
    {
        [Fact]
        public void ReturnsSuccessfulTransitionResult()
        {
            ITransitionResult<States, Events> result = this.Testee.Fire(this.TransitionContext);

            result.Should().BeSuccessfulTransitionResultWithNewState(this.Target);
        }

        [Fact]
        public void ExecutesActions()
        {
            bool executed = false;

            this.Testee.Actions.Add(new ArgumentLessActionHolder(() => executed = true));

            this.Testee.Fire(this.TransitionContext);

            executed.Should().BeTrue("actions should be executed");
        }

        [Fact]
        public void TellsExtensionsAboutExecutedTransition()
        {
            var extension = new FakeExtension();
            this.ExtensionHost.Extension = extension;

            this.Testee.Fire(this.TransitionContext);

            extension.Items.Should().Contain(new FakeExtension.Item(
                this.StateMachineInformation,
                this.Source,
                this.Target,
                this.TransitionContext));
        }

        public class FakeExtension : ExtensionBase<States, Events>
        {
            private readonly List<Item> items = new List<Item>();

            public override void ExecutedTransition(
                IStateMachineInformation<States, Events> stateMachine,
                ITransition<States, Events> transition,
                ITransitionContext<States, Events> transitionContext)
            {
                this.items.Add(new Item(stateMachine, transition.Source, transition.Target, transitionContext));
            }

            public IReadOnlyCollection<Item> Items => this.items;

            public class Item
            {
                private bool Equals(Item other)
                {
                    return
                        Equals(this.StateMachine, other.StateMachine) &&
                        Equals(this.Source, other.Source) &&
                        (Equals(this.Target, other.Target) || (this.Target == null && other.Target == other.Source)) && // in case of an internal-transition, this.Target (from TransitionContext) is null (wherease it would be == this.Source in case of an self-transition) therefor we check the we did not switch state in this case
                        Equals(this.TransitionContext, other.TransitionContext);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj))
                    {
                        return false;
                    }

                    if (ReferenceEquals(this, obj))
                    {
                        return true;
                    }

                    if (obj.GetType() != this.GetType())
                    {
                        return false;
                    }

                    return this.Equals((Item)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        var hashCode = this.StateMachine != null ? this.StateMachine.GetHashCode() : 0;
                        hashCode = (hashCode * 397) ^ (this.Source != null ? this.Source.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (this.Target != null ? this.Target.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (this.TransitionContext != null ? this.TransitionContext.GetHashCode() : 0);
                        return hashCode;
                    }
                }

                public Item(
                    IStateMachineInformation<States, Events> stateMachine,
                    IState<States, Events> source,
                    IState<States, Events> target,
                    ITransitionContext<States, Events> transitionContext)
                {
                    this.StateMachine = stateMachine;
                    this.Source = source;
                    this.Target = target;
                    this.TransitionContext = transitionContext;
                }

                public IStateMachineInformation<States, Events> StateMachine { get; }

                public IState<States, Events> Source { get; }

                public IState<States, Events> Target { get; }

                public ITransitionContext<States, Events> TransitionContext { get; }
            }
        }
    }
}