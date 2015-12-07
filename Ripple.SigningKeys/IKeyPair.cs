namespace Ripple.Crypto
{
	public interface IKeyPair
	{
		byte[] CanonicalPubBytes();

        bool Verify(byte[] message, byte[] signature);
		byte[] Sign(byte[] message);

        byte[] PubKeyHash();
        string ID();
	}
}