using System.IO;
using BreakNotice.Contest;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class BrowserPointerCaptureContractTests
    {
        [TestCase(false, false, "POINTER LOCK UNAVAILABLE")]
        [TestCase(true, false, "CLICK GAME VIEW TO CAPTURE MOUSE")]
        [TestCase(true, true, "MOUSE CAPTURED - ESC TO RELEASE")]
        public void StatusText_ReportsActualBrowserPointerState(
            bool supported,
            bool locked,
            string expected)
        {
            Assert.That(
                BrowserPointerCapture.BuildStatusText(supported, locked),
                Is.EqualTo(expected));
        }

        [Test]
        public void StatusText_ReportsRejectedBrowserRequestWithoutFalseSuccess()
        {
            Assert.That(
                BrowserPointerCapture.BuildStatusText(true, false, true),
                Is.EqualTo("MOUSE CAPTURE BLOCKED - CLICK GAME VIEW TO RETRY"));
        }

        [Test]
        public void StatusText_ReportsPendingRequestWithoutFalseFailure()
        {
            Assert.That(
                BrowserPointerCapture.BuildStatusText(
                    supported: true,
                    locked: false,
                    failed: false,
                    pending: true),
                Is.EqualTo("CAPTURING MOUSE..."));
        }

        [Test]
        public void StatusText_ReportsEmbeddedBrowserFallbackAsActiveLook()
        {
            Assert.That(
                BrowserPointerCapture.BuildStatusText(
                    supported: true,
                    locked: false,
                    failed: true,
                    pending: false,
                    fallbackActive: true),
                Is.EqualTo("MOUSE LOOK ACTIVE - ESC TO RELEASE"));
        }

        [Test]
        public void WebBridge_RequestsOnceAfterPrimaryClickFocusTransition()
        {
            string bridgePath = Path.Combine(
                Application.dataPath,
                "Plugins",
                "WebGL",
                "BreakNoticePointerLock.jslib");
            string source = File.ReadAllText(bridgePath);

            StringAssert.Contains(
                "canvas.addEventListener('click', state.onClick, false);",
                source);
            StringAssert.Contains("state.pending ||", source);
            StringAssert.Contains("state.failIntoFallback();", source);
            StringAssert.Contains("canvas.addEventListener('mousemove', state.onMouseMove, false);", source);
            StringAssert.DoesNotContain("addEventListener('pointerdown'", source);
        }
    }
}
