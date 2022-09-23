using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Silk.NET.Input;
using Vec2 = Silk.NET.Maths.Vector2D<float>;

namespace MafrixEngine.Input
{
    public class HardwareInput
    {
        public IInputContext inputContext;
        public HardwareInput(IInputContext context)
        {
            inputContext = context;
        }

        public void AddKeyBinding()
        {
        }
    }

    public class InputMapping
    {
        private IInputContext inputContext;
        public InputMapping(IInputContext context)
        {
            inputContext = context;
        }
    }

    public class KeyboardMapping
    {
        private IKeyboard keyboard;
        private HashSet<Key> pressedKeys;
        private HashSet<Key> holdingKeys;
        private HashSet<Key> releasedKeys;
        private Dictionary<Key, List<Action>> keyValuePairs;

        public enum KeyState
        {
            Down,
            Up,
            Holding,
        }

        public KeyboardMapping(IKeyboard kb)
        {
            keyboard = kb;
            pressedKeys = new HashSet<Key>();
            holdingKeys = new HashSet<Key>();
            releasedKeys = new HashSet<Key>();
            keyValuePairs = new Dictionary<Key, List<Action>>();
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }

        private void OnKeyDown(IKeyboard kb, Key key, int i)
        {
            pressedKeys.Add(key);
        }

        private void OnKeyUp(IKeyboard kb, Key key, int i)
        {
            releasedKeys.Add(key);
        }

        public async void Update(double delta)
        {
            foreach (var key in pressedKeys)
            {
                if(keyValuePairs.TryGetValue(key, out var actions))
                {
                    foreach (var action in actions)
                    {
                        await Task.Run(action);
                    }
                }

            }

            foreach (var key in holdingKeys)
            {
                if(keyValuePairs.TryGetValue(key, out var actions))
                {
                    foreach (var action in actions)
                    {
                        await Task.Run(action);
                    }
                }
            }
            // after process holding key complete, add new keys process in next update
            foreach (var key in pressedKeys)
            {
                holdingKeys.Add(key);
            }
            pressedKeys.Clear();

            foreach (var key in releasedKeys)
            {
                if(keyValuePairs.TryGetValue(key, out var actions))
                {
                    foreach (var action in actions)
                    {
                        await Task.Run(action);
                    }
                }
            }

            // after key released, remove key from holding key set
            holdingKeys.RemoveWhere(releasedKeys.Contains);
            releasedKeys.Clear();
        }

        public void AddKeyBinding(Key key, Action action, KeyState state = KeyState.Holding)
        {
            if(keyValuePairs.TryGetValue(key, out var actions))
            {
                actions.Add(action);
            }
            else
            {
                var newActions = new List<Action>();
                newActions.Add(action);
                keyValuePairs.Add(key, newActions);
            }
        }

        public void AddAxisBinding(Key key, Action<float> action)
        {

        }
    }

    public class MouseMapping
    {
        private IMouse mouse;
        public ICursor cursor;
        private bool isLeftClicked;
        private bool isRightClicked;
        private bool isDoubleClicked;
        private bool isMouseMove;
        private Vec2 lastPosition;
        private Vec2 currentPosition;
        public event Action<IMouse, Vec2>? OnLeftClick;
        public event Action<IMouse, Vec2>? OnRightClick;
        public event Action<IMouse, Vec2>? OnDoubleClick;
        public event Action<IMouse, Vec2>? OnMouseMove;
        public MouseMapping(IMouse m)
        {
            mouse = m;
            cursor = mouse.Cursor;
            mouse.MouseMove += HandleMouseMove;
            mouse.MouseDown += HandleMouseDown;
            mouse.MouseUp += HandleMouseUp;
            mouse.Click += HandleClick;
            mouse.DoubleClick += HandleDoubleClicked;
            mouse.DoubleClickTime = 250;
        }


        public void Update(double delta)
        {
            if(isLeftClicked)
            {
                OnLeftClick?.Invoke(mouse, currentPosition);
            }
            if(isRightClicked)
            {
                OnRightClick?.Invoke(mouse, currentPosition);
            }
            if(isDoubleClicked)
            {
                OnDoubleClick?.Invoke(mouse, currentPosition);
            }
            if(isMouseMove)
            {
                var dir = currentPosition - lastPosition;
                dir.Y = -dir.Y;
                OnMouseMove?.Invoke(mouse, dir);
            }

            lastPosition = currentPosition;
            isLeftClicked = false;
            isRightClicked = false;
            isDoubleClicked = false;
            isMouseMove = false;
        }

        private void HandleDoubleClicked(IMouse mouse, MouseButton btn, Vector2 pos)
        {
            isDoubleClicked = true;
        }

        private void HandleClick(IMouse lmouse, MouseButton btn, Vector2 pos)
        {
            switch(btn)
            {
                case MouseButton.Left:
                    isLeftClicked = true;
                    break;
                case MouseButton.Right:
                    isRightClicked = true;
                    break;
                case MouseButton.Middle:
                    break;
            }
        }

        private void HandleMouseUp(IMouse mouse, MouseButton btn)
        {
            switch (btn)
            {
                case MouseButton.Left:
                    break;
                case MouseButton.Right:
                    break;
                case MouseButton.Middle:
                    break;
            }
        }

        private void HandleMouseDown(IMouse mouse, MouseButton btn)
        {
            switch(btn)
            {
                case MouseButton.Left:
                    break;
                case MouseButton.Right:
                    break;
                case MouseButton.Middle:
                    break;
            }
        }

        private void HandleMouseMove(IMouse mouse, Vector2 move)
        {
            currentPosition.X = move.X;
            currentPosition.Y = move.Y;
            isMouseMove = true;
        }

    }
}
