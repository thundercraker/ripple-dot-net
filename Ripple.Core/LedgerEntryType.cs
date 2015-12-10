using System;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class LedgerEntryType : EnumItem, ISerializedType
    {
        public readonly byte[] Bytes;
        public static Enumeration<LedgerEntryType> Values = new Enumeration<LedgerEntryType>();

        public LedgerEntryType(string name, int ordinal) : base(name, ordinal)
        {
            Bytes = Bits.GetBytes((ushort) ordinal);
            Values.AddEnum(this);
        }
        public void ToBytes(IBytesSink sink)
        {
            sink.Add(Bytes);
        }
        public static LedgerEntryType FromJson(JToken token)
        {
            return token.Type == JTokenType.String ?
                Values[token.ToString()] : Values[(int) token];
        }
        public static readonly LedgerEntryType Invalid = new LedgerEntryType(nameof(Invalid), -1);
        public static readonly LedgerEntryType AccountRoot = new LedgerEntryType(nameof(AccountRoot), 'a');
        public static readonly LedgerEntryType DirectoryNode = new LedgerEntryType(nameof(DirectoryNode), 'd');
        public static readonly LedgerEntryType GeneratorMap = new LedgerEntryType(nameof(GeneratorMap), 'g');
        public static readonly LedgerEntryType RippleState = new LedgerEntryType(nameof(RippleState), 'r');
        // Deprecated
        //public static readonly LedgerEntryType Nickname = new LedgerEntryType(nameof(Nickname), 'n');
        public static readonly LedgerEntryType Offer = new LedgerEntryType(nameof(Offer), 'o');
        public static readonly LedgerEntryType Contract = new LedgerEntryType(nameof(Contract), 'c');
        public static readonly LedgerEntryType LedgerHashes = new LedgerEntryType(nameof(LedgerHashes), 'h');
        public static readonly LedgerEntryType EnabledAmendments = new LedgerEntryType(nameof(EnabledAmendments), 'f');
        public static readonly LedgerEntryType FeeSettings = new LedgerEntryType(nameof(FeeSettings), 's');
        public static readonly LedgerEntryType Ticket = new LedgerEntryType(nameof(Ticket), 'T');
        public static readonly LedgerEntryType SignerList = new LedgerEntryType(nameof(SignerList), 'S');
    }
}