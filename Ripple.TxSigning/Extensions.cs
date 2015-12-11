using Ripple.Core;
using Ripple.Core.Util;

namespace Ripple.TxSigning
{
    internal static class Extensions
    {
        internal static byte[] Bytes(this HashPrefix hp)
        {
            return Bits.GetBytes((uint)hp);
        }

        internal static byte[] SigningData(this StObject so)
        {
            var list = new BytesList();
            list.Add(HashPrefix.TxSign.Bytes());
            so.ToBytes(list, f => f.IsSigningField);
            return list.Bytes();
        }

        internal static byte[] Blob(this StObject so)
        {
            var list = new BytesList();
            so.ToBytes(list, f => f.IsSerialised);
            return list.Bytes();
        }
    }
}