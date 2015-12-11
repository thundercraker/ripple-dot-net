using System;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class TransactionType : EnumItem, ISerializedType
    {
        public readonly byte[] Bytes;
        public static Enumeration<TransactionType> Values = new Enumeration<TransactionType>();

        public TransactionType(string name, int ordinal) : base(name, ordinal)
        {
            Bytes = Bits.GetBytes((ushort)ordinal);
            Values.AddEnum(this);
        }
        public void ToBytes(IBytesSink sink)
        {
            sink.Add(Bytes);
        }

        public JToken ToJson()
        {
            return ToString();
        }

        public static TransactionType FromJson(JToken token)
        {
            return token.Type == JTokenType.String ?
                Values[token.ToString()] : Values[(int)token];
        }
        public static readonly TransactionType Invalid = new TransactionType(nameof(Invalid), -1);
        public static readonly TransactionType Payment = new TransactionType(nameof(Payment), 0);
        public static readonly TransactionType Claim = new TransactionType(nameof(Claim), 1);
        public static readonly TransactionType WalletAdd = new TransactionType(nameof(WalletAdd), 2);
        public static readonly TransactionType AccountSet = new TransactionType(nameof(AccountSet), 3);
        public static readonly TransactionType PasswordFund = new TransactionType(nameof(PasswordFund), 4);
        public static readonly TransactionType SetRegularKey = new TransactionType(nameof(SetRegularKey), 5);
        public static readonly TransactionType NickNameSet = new TransactionType(nameof(NickNameSet), 6);
        public static readonly TransactionType OfferCreate = new TransactionType(nameof(OfferCreate), 7);
        public static readonly TransactionType OfferCancel = new TransactionType(nameof(OfferCancel), 8);
        public static readonly TransactionType Contract = new TransactionType(nameof(Contract), 9);
        public static readonly TransactionType TicketCreate = new TransactionType(nameof(TicketCreate), 10);
        public static readonly TransactionType TicketCancel = new TransactionType(nameof(TicketCancel), 11);
        public static readonly TransactionType SignerListSet = new TransactionType(nameof(SignerListSet), 12);
        public static readonly TransactionType TrustSet = new TransactionType(nameof(TrustSet), 20);
        public static readonly TransactionType EnableAmendment = new TransactionType(nameof(EnableAmendment), 100);
        public static readonly TransactionType SetFee = new TransactionType(nameof(SetFee), 101);

        public static TransactionType FromParser(BinaryParser parser, int? hint=null)
        {
            return Values[Bits.ToUInt16(parser.Read(2), 0)];
        }
    }
}