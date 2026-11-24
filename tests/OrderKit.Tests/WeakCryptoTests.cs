using System.Text;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    [TestFixture]
    public class WeakCryptoTests
    {
        [Test]
        public void Md5MatchesKnownVector()
        {
            Assert.AreEqual("900150983cd24fb0d6963f7d28e17f72", WeakCrypto.Md5Hex("abc"));
        }

        [Test]
        public void Sha1MatchesKnownVector()
        {
            Assert.AreEqual("a9993e364706816aba3e25717850c26c9cd0d89d", WeakCrypto.Sha1Hex("abc"));
        }

        [Test]
        public void DesEcbProducesOneBlockPerEightBytes()
        {
            byte[] key = Encoding.ASCII.GetBytes("orderkit");
            byte[] cipher = WeakCrypto.DesEcbEncrypt(key, Encoding.ASCII.GetBytes("12345678"));
            Assert.AreEqual(16, cipher.Length);
        }

        [Test]
        public void SeededTokenIsRepeatable()
        {
            Assert.AreEqual(WeakCrypto.NewSessionToken(7), WeakCrypto.NewSessionToken(7));
            Assert.AreEqual(32, WeakCrypto.NewSessionToken(7).Length);
        }

        [Test]
        public void TrustAllHandlerAcceptsAnyCertificate()
        {
            using (var handler = WeakCrypto.TrustAllHandler())
            {
                Assert.IsTrue(handler.ServerCertificateCustomValidationCallback(null, null, null, System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch));
            }
        }
    }
}
