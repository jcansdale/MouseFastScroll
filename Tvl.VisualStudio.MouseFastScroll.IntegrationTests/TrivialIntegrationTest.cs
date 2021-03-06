﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Tvl.VisualStudio.MouseFastScroll.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.VisualStudio.Text.Formatting;
    using WindowsInput;
    using WindowsInput.Native;
    using Xunit;
    using vsSaveChanges = EnvDTE.vsSaveChanges;

    public abstract class TrivialIntegrationTest : AbstractIntegrationTest
    {
        protected TrivialIntegrationTest(VisualStudioInstanceFactory instanceFactory, Version version)
            : base(instanceFactory, version)
        {
        }

        [Fact]
        public void TestOpenAndCloseIDE()
        {
            var currentVersion = VisualStudioInstance.RetryRpcCall(() => VisualStudio.Dte.Version);
            var expectedVersion = VisualStudio.Version;
            if (expectedVersion.Major >= 15)
            {
                // The Setup APIs provide the full version number, but DTE.Version is always [Major].0
                expectedVersion = new Version(expectedVersion.Major, 0);
            }

            Assert.Equal(expectedVersion.ToString(), currentVersion);
        }

        [Fact]
        public void BasicScrollingBehavior()
        {
            var window = VisualStudioInstance.RetryRpcCall(() => VisualStudio.Dte.ItemOperations.NewFile(Name: Guid.NewGuid() + ".txt"));

            string initialText = string.Join(string.Empty, Enumerable.Range(0, 400).Select(i => Guid.NewGuid() + Environment.NewLine));
            VisualStudio.Editor.SetText(initialText);

            string additionalTypedText = Guid.NewGuid().ToString() + "\n" + Guid.NewGuid().ToString();
            VisualStudio.Editor.SendKeys(additionalTypedText);

            string expected = initialText + additionalTypedText.Replace("\n", Environment.NewLine);
            Assert.Equal(expected, VisualStudio.Editor.GetText());

            Assert.Equal(expected.Length, VisualStudio.Editor.GetCaretPosition());

            // Move the caret and verify the final position. Note that the MoveCaret operation does not scroll the view.
            int firstVisibleLine = VisualStudio.Editor.GetFirstVisibleLine();
            Assert.True(firstVisibleLine > 0, "Expected the view to start after the first line at this point.");
            VisualStudio.Editor.MoveCaret(0);
            Assert.Equal(0, VisualStudio.Editor.GetCaretPosition());
            Assert.Equal(firstVisibleLine, VisualStudio.Editor.GetFirstVisibleLine());

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator.Keyboard
                    .KeyDown(VirtualKeyCode.CONTROL)
                    .KeyPress(VirtualKeyCode.HOME)
                    .KeyUp(VirtualKeyCode.CONTROL);
            });

            Assert.True(VisualStudio.Editor.IsCaretOnScreen());
            firstVisibleLine = VisualStudio.Editor.GetFirstVisibleLine();
            Assert.Equal(0, firstVisibleLine);

            int lastVisibleLine = VisualStudio.Editor.GetLastVisibleLine();
            VisibilityState lastVisibleLineState = VisualStudio.Editor.GetLastVisibleLineState();
            Assert.True(firstVisibleLine < lastVisibleLine);

            Point point = VisualStudio.Editor.GetCenterOfEditorOnScreen();
            int horizontalResolution = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
            int verticalResolution = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
            point = new ScaleTransform(65535.0 / horizontalResolution, 65535.0 / verticalResolution).Transform(point);

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator.Mouse
                    .MoveMouseTo(point.X, point.Y)
                    .VerticalScroll(-1);
            });

            Assert.Equal(0, VisualStudio.Editor.GetCaretPosition());
            Assert.Equal(3, VisualStudio.Editor.GetFirstVisibleLine());

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator.Mouse
                    .MoveMouseTo(point.X, point.Y)
                    .VerticalScroll(1);
            });

            Assert.Equal(0, VisualStudio.Editor.GetCaretPosition());
            Assert.Equal(0, VisualStudio.Editor.GetFirstVisibleLine());

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator
                    .Mouse.MoveMouseTo(point.X, point.Y)
                    .Keyboard.KeyDown(VirtualKeyCode.CONTROL)
                    .Mouse.VerticalScroll(-1)
                    .Keyboard.Sleep(10).KeyUp(VirtualKeyCode.CONTROL);
            });

            int expectedLastVisibleLine = lastVisibleLine + (lastVisibleLineState == VisibilityState.FullyVisible ? 1 : 0);
            Assert.Equal(0, VisualStudio.Editor.GetCaretPosition());
            Assert.Equal(expectedLastVisibleLine, VisualStudio.Editor.GetFirstVisibleLine());

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator
                    .Mouse.MoveMouseTo(point.X, point.Y)
                    .Keyboard.KeyDown(VirtualKeyCode.CONTROL)
                    .Mouse.VerticalScroll(1)
                    .Keyboard.Sleep(10).KeyUp(VirtualKeyCode.CONTROL);
            });

            Assert.Equal(0, VisualStudio.Editor.GetCaretPosition());
            Assert.Equal(0, VisualStudio.Editor.GetFirstVisibleLine());

            VisualStudioInstance.RetryRpcCall(() => window.Close(vsSaveChanges.vsSaveChangesNo));
        }

        /// <summary>
        /// Verifies that the Ctrl+Scroll operations do not change the zoom level in the editor.
        /// </summary>
        [Fact]
        public void ZoomDisabled()
        {
            var window = VisualStudioInstance.RetryRpcCall(() => VisualStudio.Dte.ItemOperations.NewFile(Name: Guid.NewGuid() + ".txt"));

            string initialText = string.Join(string.Empty, Enumerable.Range(0, 400).Select(i => Guid.NewGuid() + Environment.NewLine));
            VisualStudio.Editor.SetText(initialText);

            string additionalTypedText = Guid.NewGuid().ToString() + "\n" + Guid.NewGuid().ToString();
            VisualStudio.Editor.SendKeys(additionalTypedText);

            string expected = initialText + additionalTypedText.Replace("\n", Environment.NewLine);
            Assert.Equal(expected, VisualStudio.Editor.GetText());

            Assert.Equal(expected.Length, VisualStudio.Editor.GetCaretPosition());

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator.Keyboard
                    .KeyDown(VirtualKeyCode.CONTROL)
                    .KeyPress(VirtualKeyCode.HOME)
                    .KeyUp(VirtualKeyCode.CONTROL);
            });

            int firstVisibleLine = VisualStudio.Editor.GetFirstVisibleLine();
            Assert.Equal(0, firstVisibleLine);

            int lastVisibleLine = VisualStudio.Editor.GetLastVisibleLine();
            VisibilityState lastVisibleLineState = VisualStudio.Editor.GetLastVisibleLineState();
            Assert.True(firstVisibleLine < lastVisibleLine);

            double zoomLevel = VisualStudio.Editor.GetZoomLevel();

            Point point = VisualStudio.Editor.GetCenterOfEditorOnScreen();
            int horizontalResolution = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
            int verticalResolution = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
            point = new ScaleTransform(65535.0 / horizontalResolution, 65535.0 / verticalResolution).Transform(point);

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator
                    .Mouse.MoveMouseTo(point.X, point.Y)
                    .Keyboard.KeyDown(VirtualKeyCode.CONTROL)
                    .Mouse.VerticalScroll(-1)
                    .Keyboard.Sleep(10).KeyUp(VirtualKeyCode.CONTROL);
            });

            int expectedLastVisibleLine = lastVisibleLine + (lastVisibleLineState == VisibilityState.FullyVisible ? 1 : 0);
            Assert.Equal(0, VisualStudio.Editor.GetCaretPosition());
            Assert.Equal(expectedLastVisibleLine, VisualStudio.Editor.GetFirstVisibleLine());
            Assert.Equal(zoomLevel, VisualStudio.Editor.GetZoomLevel());

            VisualStudio.SendKeys.Send(inputSimulator =>
            {
                inputSimulator
                    .Mouse.MoveMouseTo(point.X, point.Y)
                    .Keyboard.KeyDown(VirtualKeyCode.CONTROL)
                    .Mouse.VerticalScroll(1)
                    .Keyboard.Sleep(10).KeyUp(VirtualKeyCode.CONTROL);
            });

            Assert.Equal(0, VisualStudio.Editor.GetCaretPosition());
            Assert.Equal(0, VisualStudio.Editor.GetFirstVisibleLine());
            Assert.Equal(zoomLevel, VisualStudio.Editor.GetZoomLevel());

            VisualStudioInstance.RetryRpcCall(() => window.Close(vsSaveChanges.vsSaveChangesNo));
        }

        [VersionTrait(typeof(VS2012))]
        public sealed class VS2012 : TrivialIntegrationTest
        {
            public VS2012(VisualStudioInstanceFactory instanceFactory)
                : base(instanceFactory, Versions.VisualStudio2012)
            {
            }
        }

        [VersionTrait(typeof(VS2013))]
        public sealed class VS2013 : TrivialIntegrationTest
        {
            public VS2013(VisualStudioInstanceFactory instanceFactory)
                : base(instanceFactory, Versions.VisualStudio2013)
            {
            }
        }

        [VersionTrait(typeof(VS2015))]
        public sealed class VS2015 : TrivialIntegrationTest
        {
            public VS2015(VisualStudioInstanceFactory instanceFactory)
                : base(instanceFactory, Versions.VisualStudio2015)
            {
            }
        }

        [VersionTrait(typeof(VS2017))]
        public sealed class VS2017 : TrivialIntegrationTest
        {
            public VS2017(VisualStudioInstanceFactory instanceFactory)
                : base(instanceFactory, Versions.VisualStudio2017)
            {
            }
        }
    }
}
