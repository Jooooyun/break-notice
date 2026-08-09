using BreakNotice.Contest;
using NUnit.Framework;

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
    }
}
