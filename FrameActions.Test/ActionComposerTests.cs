using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Optional;

namespace FrameActions.Test
{
    public class ActionComposerTests
    {
        class DependencyProviderSpy : IDependencyProvider
        {
            public int _called;

            public Option<T> Get<T>()
            {
                _called++;
                return Option.None<T>();
            }
        }

        class LoadableDependencyProvider : IDependencyProvider
        {
            private Dictionary<Type, Func<dynamic>> _dependency;

            public LoadableDependencyProvider()
            {
                _dependency = new Dictionary<Type, Func<dynamic>>();
            }

            public void Register<T>(Func<dynamic> resolver)
            {
                _dependency.Add(typeof(T), resolver);
            }


            public Option<T> Get<T>()
            {
                return Option.Some<T>(_dependency[typeof(T)]());
            }
        }
       
        [Test]
        public void canComposeNoActions()
        {
            var spy = new DependencyProviderSpy();
            var composer = new ActionComposer(spy);
            composer.Submit();

            spy._called.Should().Be(0);
        }

        public class PunchAnimator : IFrameAction<PunchAnimation>
        {
            public int _called;
            public PunchAnimation _payload;

            public void NewFrame(PunchAnimation payload)
            {
                _called++;
                _payload = payload;
            }
        }

        public struct PunchAnimation
        {
            public int intensity;
        }
        
        [Test]
        public void canComposeOneAction()
        {
            var punchAnimator = new PunchAnimator();
            
            var dependencyResolver = new LoadableDependencyProvider();

            dependencyResolver.Register<PunchAnimator>(() => punchAnimator);
            
            var composer = new ActionComposer(dependencyResolver);

            composer.Route<PunchAnimation, PunchAnimator>();

            var punchAnimation = new PunchAnimation();
            punchAnimation.intensity = 100;
            
            composer.QueueCommand(punchAnimation); 
            composer.Submit();
            composer.ExecuteFrame();

            punchAnimator._called.Should().Be(1);
            punchAnimator._payload.Should().Be(punchAnimation);
        }
        
        [Test]
        public void DoesNotExecuteUnSubmittedCommands()
        {
            var action = new PunchAnimator();
            
            var dependencyResolver = new LoadableDependencyProvider();

            dependencyResolver.Register<PunchAnimator>(() => action);
            
            var composer = new ActionComposer(dependencyResolver);

            composer.Route<PunchAnimation, PunchAnimator>();

            var punchAnimation = new PunchAnimation();
            punchAnimation.intensity = 100;
            
            composer.QueueCommand(punchAnimation); 
            composer.ExecuteFrame();

            action._called.Should().Be(0);
            action._payload.Should().NotBe(punchAnimation);
        }
    }
}