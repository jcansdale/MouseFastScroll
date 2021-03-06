﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Tvl.VisualStudio.MouseFastScroll.IntegrationTests
{
    using System.Windows;
    using Microsoft.VisualStudio.Text.Formatting;
    using WindowsInput.Native;

    public class Editor_OutOfProc : TextViewWindow_OutOfProc
    {
        internal Editor_OutOfProc(VisualStudioInstance visualStudioInstance)
            : base(visualStudioInstance)
        {
            EditorInProc = (Editor_InProc)TextViewWindowInProc;
        }

        internal Editor_InProc EditorInProc
        {
            get;
        }

        internal override TextViewWindow_InProc CreateInProcComponent(VisualStudioInstance visualStudioInstance)
            => CreateInProcComponent<Editor_InProc>(visualStudioInstance);

        public void Activate()
            => EditorInProc.Activate();

        internal string GetText()
            => EditorInProc.GetText();

        internal void SetText(string value)
            => EditorInProc.SetText(value);

        public void MoveCaret(int position)
            => EditorInProc.MoveCaret(position);

        public bool IsCaretOnScreen()
            => EditorInProc.IsCaretOnScreen();

        /// <summary>
        /// Sends key strokes to the active editor in Visual Studio. Various types are supported by this method:
        /// <see cref="string"/> (each character will be sent separately, <see cref="char"/>, and
        /// <see cref="VirtualKeyCode"/>).
        /// </summary>
        public void SendKeys(params object[] keys)
        {
            Activate();
            VisualStudioInstance.SendKeys.Send(keys);
        }

        public int GetFirstVisibleLine()
            => EditorInProc.GetFirstVisibleLine();

        public int GetLastVisibleLine()
            => EditorInProc.GetLastVisibleLine();

        public VisibilityState GetLastVisibleLineState()
            => (VisibilityState)EditorInProc.GetLastVisibleLineState();

        public Point GetCenterOfEditorOnScreen()
            => EditorInProc.GetCenterOfEditorOnScreen();

        public double GetZoomLevel()
            => EditorInProc.GetZoomLevel();
    }
}
