using Cross.Threading;
using Cross.UI.Events;
using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI
{
    public class PlatformIndependentInitializer : IPlatformIndependentInitializer
    {
        public Task<IAppWindow<TRenderTarget>> CreateAppWindowAsync<TRenderTarget>(IAppPlatform<TRenderTarget> platform, IComponent rootComponent) where TRenderTarget : class, IRenderable
        {
            var eventSink = new EventSink<TRenderTarget>(rootComponent);
            return platform.NewWindowAsync(eventSink);
        }

        private class ComponentNode : IComponentResource
        {
            public UIContext UIContext => _UIContext ?? throw new InvalidOperationException();

            public ComponentNode(ComponentNodeInitializer nodeInitializer, IComponentTreeNode<ComponentNode> componentNode)
            {
                _ComponentNode = componentNode;
                _NodeInitializer = nodeInitializer;

            }

            private IComponentTreeNode<ComponentNode> _ComponentNode;
            private ComponentNodeInitializer _NodeInitializer;
            private UIContext? _UIContext;
                
            public void Initialize()
            {
                var dispatchProvider = _NodeInitializer.DispatchProvider;
                var attrProvider = _NodeInitializer.AttributeProvider;
                var documentDict = _NodeInitializer.DocumentDict;
                var uiValidator = _NodeInitializer.UIValidator;
                _UIContext = new UIContext(
                    dispatchProvider.CreateContext(_ComponentNode),
                    attrProvider.CreateContext(_ComponentNode),
                    _ComponentNode,
                    documentDict,
                    uiValidator);
                var docKey = _ComponentNode.Component.DocumentKey;
                if (docKey != null)
                    _NodeInitializer.DocumentDict.TryAdd(docKey, _ComponentNode);
                _ComponentNode.Component.Initialize(
                    attrProvider.CreateContext(_ComponentNode),
                    dispatchProvider.CreateBindingOnlyContext(_ComponentNode));
            }

            public void Dispose()
            {
                var component = _ComponentNode.Component;
                _NodeInitializer.DispatchProvider.ReleaseComponentHandlers(component);
                _NodeInitializer.AttributeProvider.ReleaseComponentAttributes(component);
                var docKey = _ComponentNode.Component.DocumentKey;
                if (docKey != null)
                    _NodeInitializer.DocumentDict.TryRemove(docKey, out var _);
            }
        }

        private class ComponentNodeInitializer : INodeResourceFactory<ComponentNode>
        {
            public IEventDispatchProvider<ComponentNode> DispatchProvider => _DispatchProvider;
            public IAttributeProvider AttributeProvider => _AttributeProvider;
            public ConcurrentDictionary<Key<IComponent>, IComponentTreeNode> DocumentDict => _DocumentDict;
            public IValidated UIValidator => _UIValidator;

            public ComponentNodeInitializer(IEventDispatchProvider<ComponentNode> dispatchProvider, IAttributeProvider attrProvider, IValidated uiValidator)
            {
                _DispatchProvider = dispatchProvider;
                _AttributeProvider = attrProvider;
                _UIValidator = uiValidator;
            }

            private IEventDispatchProvider<ComponentNode> _DispatchProvider;
            private IAttributeProvider _AttributeProvider;
            private ConcurrentDictionary<Key<IComponent>, IComponentTreeNode> _DocumentDict = new ConcurrentDictionary<Key<IComponent>, IComponentTreeNode>();
            private IValidated _UIValidator;

            public ComponentNode InitializeNode(IComponentTreeNode<ComponentNode> node)
            {
                return new ComponentNode(this, node);
            }
        }

        private class UIContextProvider : IUIContextProvider<ComponentNode>
        {
            public IUIContext GetUIContext(IComponentTreeNode<ComponentNode> node)
            {
                return node.Resources.UIContext;
            } 
        }

        private class EventSink<TRenderTarget> : ILayoutEventSink<TRenderTarget> where TRenderTarget : class, IRenderable
        {
            private IComponent _RootComponent;

            public EventSink(IComponent rootComponent)
            {
                _RootComponent = rootComponent;
            }

            private UIContextProvider? _ContextProvider;
            private EventDispatcher<ComponentNode>? _EventDispatcher;
            private AttributeStore? _AttributeStore;
            private ComponentNodeInitializer? _NodeInitializer;
            private ComponentTree<ComponentNode, TRenderTarget>? _ComponentTree;
            private IAppWindow<TRenderTarget>? _Window;
            private IComponentTreeNode<ComponentNode>? _InputFocus;
            private IComponentTreeNode<ComponentNode>? _LastMouseHit;
             
            public void OnLoaded(IAppWindow<TRenderTarget> window, Size2DF size)
            {
                _Window = window;
                _ContextProvider = new UIContextProvider();
                _EventDispatcher = new EventDispatcher<ComponentNode>(_ContextProvider);
                LayoutEvents.RegisterEventTypes(_EventDispatcher);
                _AttributeStore = new AttributeStore();
                _NodeInitializer = new ComponentNodeInitializer(_EventDispatcher, _AttributeStore, window);
                _ComponentTree = new ComponentTree<ComponentNode, TRenderTarget>(_RootComponent, _AttributeStore, size, _NodeInitializer, window);
                _InputFocus = _ComponentTree.RootNode;
                window.SetCompositionSource(_ComponentTree);
            }

            public void OnKeyboardEvent(KeyboardEventArgs e)
            {
                if (_InputFocus == null)
                    return;
                var eventKey = e.Type switch
                {
                    KeyboardEventType.Up => LayoutEvents.KeyUp,
                    KeyboardEventType.Down => LayoutEvents.KeyDown,
                    _ => throw new NotImplementedException()
                };
                DispatchErrorLogged(_InputFocus, eventKey, e);
            }

            public void OnKeyInputEvent(KeyInputEventArgs e)
            {
                if (_InputFocus == null)
                    return;
                DispatchErrorLogged(_InputFocus, LayoutEvents.KeyInput, e);
            }

            public void OnMouseEvent(Events.MouseEventArgs e)
            {
                if (_ComponentTree == null)
                    return;
                var hitNode = GetHitComponent(_ComponentTree.RootNode, e.Location);
                if (e.Type == MouseEventType.Click && _InputFocus != hitNode)
                {
                    var oldFocus = _InputFocus!;
                    _InputFocus = hitNode;
                    DispatchErrorLogged(oldFocus, LayoutEvents.LostFocus, new BasicEventArgs(oldFocus.Component));
                    DispatchErrorLogged(hitNode, LayoutEvents.Focused, new BasicEventArgs(hitNode.Component));
                }
                if (e.Type == MouseEventType.Leave)
                {
                    if (_LastMouseHit != null)
                    {
                        DispatchErrorLogged(_LastMouseHit, LayoutEvents.MouseLeave, ClientizeMouseEvent(e, _LastMouseHit, MouseEventType.Leave));
                        _LastMouseHit = null;
                    }
                } else if (_LastMouseHit != hitNode)
                {
                    if (_LastMouseHit != null)
                        DispatchErrorLogged(_LastMouseHit, LayoutEvents.MouseLeave, ClientizeMouseEvent(e, _LastMouseHit, MouseEventType.Leave));
                    _LastMouseHit = hitNode;
                    DispatchErrorLogged(hitNode, LayoutEvents.MouseEnter, ClientizeMouseEvent(e, hitNode, MouseEventType.Enter));
                }
                var eventKey = e.Type switch
                {
                    MouseEventType.Up => LayoutEvents.MouseUp,
                    MouseEventType.Down => LayoutEvents.MouseDown,
                    MouseEventType.Move => LayoutEvents.MouseMove,
                    MouseEventType.Enter => null,
                    MouseEventType.Leave => null,
                    MouseEventType.Wheel => LayoutEvents.MouseWheel,
                    MouseEventType.Click => LayoutEvents.MouseClick,
                    MouseEventType.DoubleClick => LayoutEvents.MouseDoubleClick,
                    _ => null
                };
                if (eventKey != null)
                    DispatchErrorLogged(hitNode, eventKey, ClientizeMouseEvent(e, hitNode, e.Type));
            }

            public void OnShown()
            {
                if (_Window == null || _ComponentTree == null)
                    throw new InvalidOperationException();
                _Window.InvalidateAndWaitAsync().ContinueWith(t =>
                {
                    DispatchErrorLogged(_ComponentTree.RootNode, LayoutEvents.WindowShown, new BasicEventArgs(null));
                });
            }

            public void OnUserClose(InterruptibleEventArgs e)
            {
                if (_ComponentTree == null)
                    return;
                var ctx = _EventDispatcher!.CreateContext(_ComponentTree.RootNode);
                ctx.DispatchEventAsync(LayoutEvents.WindowClosing, e).Wait();
            }

            public void OnWindowResize(Size2DF size)
            {
                if (_ComponentTree == null)
                    return;
                _ComponentTree.Resize(size);
                DispatchErrorLogged(_ComponentTree.RootNode, LayoutEvents.WindowResized, new WindowResizedEventArgs(null, size));
            }

            public void OnWindowStateChanged(WindowState newState)
            {
                if (_ComponentTree == null)
                    return;
                DispatchErrorLogged(_ComponentTree.RootNode, LayoutEvents.WindowStateChanged, new WindowStateChangedEventArgs(null, newState));
            }

            private Events.MouseEventArgs ClientizeMouseEvent(Events.MouseEventArgs e, IComponentTreeNode node, MouseEventType eventType)
            {
                var clientLocation = e.Location - node.ContentRect.TopLeft;
                return new Events.MouseEventArgs(e.Source, clientLocation, eventType, e.ButtonsDown, e.DeltaButtons, e.DeltaWheel);
            }

            private IComponentTreeNode<ComponentNode> GetHitComponent(IComponentTreeNode<ComponentNode> root, Point2DF point)
            {
                foreach (var childNode in root.Children)
                {
                    if (childNode.ContentRect.Contains(point))
                        return GetHitComponent(childNode, point);
                }
                return root;
            }

            private void DispatchErrorLogged<TEventType, TEventArg>(IComponentTreeNode<ComponentNode> node, Key<TEventType> key, TEventArg arg) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument
            {
                if (_EventDispatcher == null)
                    return;
                var ctx = _EventDispatcher.CreateContext(node);
                ctx.DispatchEventAsync(key, arg).ContinueWith((Task t) =>
                {
                    var ex = t.Exception;
                    if (ex != null)
                        ctx.DispatchEventAsync(LayoutEvents.UnhandledException, new UnhandledLayoutExceptionEventArgs(node.Component, ex));
                });
            }
        }
    }
}
