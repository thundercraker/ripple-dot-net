namespace Ripple.Crypto
{
    using Org.BouncyCastle.Math;

	public interface IKeyPair
	{
		string CanonicalPubHex();
		byte[] CanonicalPubBytes();

        BigInteger Pub();
        BigInteger Priv();
		string PrivHex();

		bool VerifySignature(byte[] message, byte[] sigBytes);
		byte[] SignMessage(byte[] message);

		byte[] Pub160Hash();
        string ID();
	}

}