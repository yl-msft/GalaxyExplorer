// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace GalaxyExplorer
{
    public class InputRouter : MonoBehaviour
    {
        public delegate void KeyboadSelectionDelegate();
        public KeyboadSelectionDelegate OnKeyboadSelection;

        void Start()
        {
            // Register key events
            KeyboardManager.KeyEvent keyDownEvent = KeyboardManager.KeyEvent.KeyDown;
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Space, keyDownEvent), SpaceTapKeyboardHandler);
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Backspace, keyDownEvent), BackSpaceKeyboardHandler);
        }

        private void SpaceTapKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
        {
            HandleKeyboardSelection();
        }

        private void BackSpaceKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
        {
            GalaxyExplorerManager.Instance.TransitionManager.LoadPrevScene();
            OnKeyboadSelection?.Invoke();
        }

        private void HandleKeyboardSelection()
        {
            OnKeyboadSelection?.Invoke();
        }
    }
}
