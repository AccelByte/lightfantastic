using NUnit.Framework;
using UnityEditor.GGP;

namespace Tests
{
    public class StadiaSdkTests
    {
        [Test]
        public void ValidateStadiaSdkStatus()
        {
            GGPRunner.InitializePaths();
            Assert.IsTrue(GGPRunner.SdkInPath);
            Assert.IsTrue(GGPRunner.SdkInitialized);
        }
    }
}
