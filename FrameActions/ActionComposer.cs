using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FrameActions
{
    public class ActionComposer
    {
        private readonly IDependencyProvider _dependencyProvider;
        private readonly Dictionary<Type,Type>_routing;
        private readonly List<Action> _actions;
        private readonly Queue<Action> _actionQueue;

        public ActionComposer(IDependencyProvider dependencyProvider)
        {
            _dependencyProvider = dependencyProvider;
            _routing = new Dictionary<Type, Type>();
            _actions = new List<Action>();
            _actionQueue = new Queue<Action>();
        }

        public void QueueCommand<T>(T simpleRequest)
        {
            var proxyFrameAction = new ProxyFrameAction<T>(GetFrameAction<T>());
            _actionQueue.Enqueue(() => proxyFrameAction.ExecuteFrameAction(simpleRequest));
        }

        public void Submit()
        {
            _actions.AddRange(_actionQueue);
        }

        public void ExecuteFrame()
        {
            _actions.ForEach(action => action());    
        }

        public void Route<TRequestType, TActionType>() where TActionType : IFrameAction<TRequestType>
        {
            _routing.Add(typeof(TRequestType), typeof(TActionType));
        }

        private object GetFrameAction<T>()
        {
            var actionType = _routing[typeof(T)];

            var dependency = _dependencyProvider.GetType().GetMethod("Get").MakeGenericMethod(actionType)
                .Invoke(_dependencyProvider, Array.Empty<object>());

            var frameAction = dependency.GetType().GetMethods().First(m => m.Name == "ValueOr")
                .Invoke(dependency, new object[] {null});
            return frameAction;
        }

        private class ProxyFrameAction<T>
        {
            private readonly object _frameAction;
            private MethodInfo _method;

            public ProxyFrameAction(object frameAction)
            {
                _frameAction = frameAction;
                _method = _frameAction.GetType().GetMethod("NewFrame");
            }

            public void ExecuteFrameAction(T payload)
            {
                _method.Invoke(_frameAction, new object[] {payload});
            }
        }
    }
}