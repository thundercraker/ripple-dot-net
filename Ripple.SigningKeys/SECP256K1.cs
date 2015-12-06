namespace Ripple.Crypto
{

    using SECNamedCurves = Org.BouncyCastle.Asn1.Sec.SecNamedCurves;
    using X9ECParameters = Org.BouncyCastle.Asn1.X9.X9ECParameters;
    using ECDomainParameters = Org.BouncyCastle.Crypto.Parameters.ECDomainParameters;
    using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;
    using Org.BouncyCastle.Math;
    using System;

    public class SECP256K1
	{
		private static readonly ECDomainParameters _ECParams;
		private static readonly X9ECParameters _X9Params;

		static SECP256K1()
		{

			_X9Params = SECNamedCurves.GetByName("secp256k1");
			_ECParams = new ECDomainParameters(_X9Params.Curve, _X9Params.G, _X9Params.N, _X9Params.H);
		}

		public static ECDomainParameters Parameters()
		{
			return _ECParams;
		}

		public static BigInteger Order()
		{
			return _ECParams.N;
		}


		public static Org.BouncyCastle.Math.EC.ECCurve Curve()
		{
			return _ECParams.Curve;
		}

		public static ECPoint BasePoint()
		{
			return _ECParams.G;
		}

		internal static byte[] BasePointMultipliedBy(BigInteger secret)
        {
            ECPoint point = BasePoint().Multiply(secret);
            var encoded = point.GetEncoded(true);
            if (!(encoded.Length == 33)) {
                throw new Exception("not compressed encoding: " + encoded.Length);
            };
            return encoded;
		}

	}

}