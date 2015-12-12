using Newtonsoft.Json.Linq;

namespace Ripple.Core
{
    public delegate ISerializedType FromJson(JToken token);
    public delegate ISerializedType FromParser(BinaryParser parser, int? hint = null);

    public class Field : EnumItem
    {
        #region members
        public readonly bool IsSigningField;
        public readonly bool IsSerialised;
        public readonly bool IsVlEncoded;
        public readonly int NthOfType;
        public readonly FieldType Type;
        public readonly byte[] Header;

        public FromJson FromJson;
        public FromParser FromParser;

        #endregion

        public static readonly Enumeration<Field> Values = new Enumeration<Field>();
        public Field(string name,
            int nthOfType,
            FieldType type,
            bool isSigningField=true,
            bool isSerialised=true) :
                base(name,
                    (type.Ordinal << 16 | nthOfType))
        {
            // catch auxiliary fields
            var valid = nthOfType > 0 && NthOfType < 256 &&
                        type.Ordinal > 0 && type.Ordinal < 256;
            Type = type;
            IsSigningField = valid && isSigningField;
            IsSerialised = valid && isSerialised;
            NthOfType = nthOfType;
            IsVlEncoded = IsVlEncodedType();
            Header = CalculateHeader();
            Values.AddEnum(this);
        }

        public static implicit operator Field(string s)
        {
            return Values[s];
        }

        private byte[] CalculateHeader()
        {
            var nth = NthOfType;
            var type = Type.Ordinal;

            if (type < 16)
            {
                if (nth < 16) // common type, common name
                    return new [] {(byte) ((type << 4) | nth)};
                // common type, uncommon name
                return new[] {(byte) (type << 4), (byte) nth};
            }
            if (nth < 16)
                // uncommon type, common name
                return new[] {(byte) nth, (byte) type};
            // uncommon type, uncommon name
            return new byte[] {0, (byte) type, (byte) nth};
        }

        private bool IsVlEncodedType()
        {
            return Type == FieldType.Vector256 ||
                   Type == FieldType.Blob ||
                   Type == FieldType.AccountId;
        }

        public static readonly Field Generic = new Field(nameof(Generic), 0, FieldType.Unknown);
        public static readonly Field Invalid = new Field(nameof(Invalid), -1, FieldType.Unknown);

        public static readonly Field LedgerEntryType = new Field(nameof(LedgerEntryType), 1, FieldType.Uint16);
        public static readonly Field TransactionType = new Field(nameof(TransactionType), 2, FieldType.Uint16);
        public static readonly Field SignerWeight = new Field(nameof(SignerWeight), 3, FieldType.Uint16);

        public static readonly Field Flags = new Field(nameof(Flags), 2, FieldType.Uint32);
        public static readonly Field SourceTag = new Field(nameof(SourceTag), 3, FieldType.Uint32);
        public static readonly Field Sequence = new Field(nameof(Sequence), 4, FieldType.Uint32);
        public static readonly Field PreviousTxnLgrSeq = new Field(nameof(PreviousTxnLgrSeq), 5, FieldType.Uint32);
        public static readonly Field LedgerSequence = new Field(nameof(LedgerSequence), 6, FieldType.Uint32);
        public static readonly Field CloseTime = new Field(nameof(CloseTime), 7, FieldType.Uint32);
        public static readonly Field ParentCloseTime = new Field(nameof(ParentCloseTime), 8, FieldType.Uint32);
        public static readonly Field SigningTime = new Field(nameof(SigningTime), 9, FieldType.Uint32);
        public static readonly Field Expiration = new Field(nameof(Expiration), 10, FieldType.Uint32);
        public static readonly Field TransferRate = new Field(nameof(TransferRate), 11, FieldType.Uint32);
        public static readonly Field WalletSize = new Field(nameof(WalletSize), 12, FieldType.Uint32);
        public static readonly Field OwnerCount = new Field(nameof(OwnerCount), 13, FieldType.Uint32);
        public static readonly Field DestinationTag = new Field(nameof(DestinationTag), 14, FieldType.Uint32);

        public static readonly Field HighQualityIn = new Field(nameof(HighQualityIn), 16, FieldType.Uint32);
        public static readonly Field HighQualityOut = new Field(nameof(HighQualityOut), 17, FieldType.Uint32);
        public static readonly Field LowQualityIn = new Field(nameof(LowQualityIn), 18, FieldType.Uint32);
        public static readonly Field LowQualityOut = new Field(nameof(LowQualityOut), 19, FieldType.Uint32);
        public static readonly Field QualityIn = new Field(nameof(QualityIn), 20, FieldType.Uint32);
        public static readonly Field QualityOut = new Field(nameof(QualityOut), 21, FieldType.Uint32);
        public static readonly Field StampEscrow = new Field(nameof(StampEscrow), 22, FieldType.Uint32);
        public static readonly Field BondAmount = new Field(nameof(BondAmount), 23, FieldType.Uint32);
        public static readonly Field LoadFee = new Field(nameof(LoadFee), 24, FieldType.Uint32);
        public static readonly Field OfferSequence = new Field(nameof(OfferSequence), 25, FieldType.Uint32);
        public static readonly Field FirstLedgerSequence = new Field(nameof(FirstLedgerSequence), 26, FieldType.Uint32); // Deprecated: do not use
        // Added new semantics in 9486fc416ca7c59b8930b734266eed4d5b714c50
        public static readonly Field LastLedgerSequence = new Field(nameof(LastLedgerSequence), 27, FieldType.Uint32);
        public static readonly Field TransactionIndex = new Field(nameof(TransactionIndex), 28, FieldType.Uint32);
        public static readonly Field OperationLimit = new Field(nameof(OperationLimit), 29, FieldType.Uint32);
        public static readonly Field ReferenceFeeUnits = new Field(nameof(ReferenceFeeUnits), 30, FieldType.Uint32);
        public static readonly Field ReserveBase = new Field(nameof(ReserveBase), 31, FieldType.Uint32);
        public static readonly Field ReserveIncrement = new Field(nameof(ReserveIncrement), 32, FieldType.Uint32);
        public static readonly Field SetFlag = new Field(nameof(SetFlag), 33, FieldType.Uint32);
        public static readonly Field ClearFlag = new Field(nameof(ClearFlag), 34, FieldType.Uint32);
        public static readonly Field SignerQuorum = new Field(nameof(SignerQuorum), 35, FieldType.Uint32);


        public static readonly Field IndexNext = new Field(nameof(IndexNext), 1, FieldType.Uint64);
        public static readonly Field IndexPrevious = new Field(nameof(IndexPrevious), 2, FieldType.Uint64);
        public static readonly Field BookNode = new Field(nameof(BookNode), 3, FieldType.Uint64);
        public static readonly Field OwnerNode = new Field(nameof(OwnerNode), 4, FieldType.Uint64);
        public static readonly Field BaseFee = new Field(nameof(BaseFee), 5, FieldType.Uint64);
        public static readonly Field ExchangeRate = new Field(nameof(ExchangeRate), 6, FieldType.Uint64);
        public static readonly Field LowNode = new Field(nameof(LowNode), 7, FieldType.Uint64);
        public static readonly Field HighNode = new Field(nameof(HighNode), 8, FieldType.Uint64);

        public static readonly Field EmailHash = new Field(nameof(EmailHash), 1, FieldType.Hash128);

        public static readonly Field LedgerHash = new Field(nameof(LedgerHash), 1, FieldType.Hash256);
        public static readonly Field ParentHash = new Field(nameof(ParentHash), 2, FieldType.Hash256);
        public static readonly Field TransactionHash = new Field(nameof(TransactionHash), 3, FieldType.Hash256);
        public static readonly Field AccountHash = new Field(nameof(AccountHash), 4, FieldType.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field PreviousTxnID = new Field(nameof(PreviousTxnID), 5, FieldType.Hash256);
        public static readonly Field LedgerIndex = new Field(nameof(LedgerIndex), 6, FieldType.Hash256);
        public static readonly Field WalletLocator = new Field(nameof(WalletLocator), 7, FieldType.Hash256);
        public static readonly Field RootIndex = new Field(nameof(RootIndex), 8, FieldType.Hash256);
        // Added in rippled commit: 9486fc416ca7c59b8930b734266eed4d5b714c50
        // ReSharper disable once InconsistentNaming
        public static readonly Field AccountTxnID = new Field(nameof(AccountTxnID), 9, FieldType.Hash256);
        public static readonly Field BookDirectory = new Field(nameof(BookDirectory), 16, FieldType.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field InvoiceID = new Field(nameof(InvoiceID), 17, FieldType.Hash256);
        public static readonly Field Nickname = new Field(nameof(Nickname), 18, FieldType.Hash256);
        public static readonly Field Amendment = new Field(nameof(Amendment), 19, FieldType.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field TicketID = new Field(nameof(TicketID), 20, FieldType.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field hash = new Field(nameof(hash), 257, FieldType.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field index = new Field(nameof(index), 258, FieldType.Hash256);

        public static readonly Field Amount = new Field(nameof(Amount), 1, FieldType.Amount);
        public static readonly Field Balance = new Field(nameof(Balance), 2, FieldType.Amount);
        public static readonly Field LimitAmount = new Field(nameof(LimitAmount), 3, FieldType.Amount);
        public static readonly Field TakerPays = new Field(nameof(TakerPays), 4, FieldType.Amount);
        public static readonly Field TakerGets = new Field(nameof(TakerGets), 5, FieldType.Amount);
        public static readonly Field LowLimit = new Field(nameof(LowLimit), 6, FieldType.Amount);
        public static readonly Field HighLimit = new Field(nameof(HighLimit), 7, FieldType.Amount);
        public static readonly Field Fee = new Field(nameof(Fee), 8, FieldType.Amount);
        public static readonly Field SendMax = new Field(nameof(SendMax), 9, FieldType.Amount);
        public static readonly Field DeliverMin = new Field(nameof(DeliverMin), 10, FieldType.Amount);
        public static readonly Field MinimumOffer = new Field(nameof(MinimumOffer), 16, FieldType.Amount);
        public static readonly Field RippleEscrow = new Field(nameof(RippleEscrow), 17, FieldType.Amount);
        // Added in rippled commit: e7f0b8eca69dd47419eee7b82c8716b3aa5a9e39
        public static readonly Field DeliveredAmount = new Field(nameof(DeliveredAmount), 18, FieldType.Amount);
        // These are auxiliary fields
        //    quality(257, FieldType.AMOUNT),
        // ReSharper disable once InconsistentNaming
        public static readonly Field taker_gets_funded = new Field(nameof(taker_gets_funded), 258, FieldType.Amount);
        // ReSharper disable once InconsistentNaming
        public static readonly Field taker_pays_funded = new Field(nameof(taker_pays_funded), 259, FieldType.Amount);

        public static readonly Field PublicKey = new Field(nameof(PublicKey), 1, FieldType.Blob);
        public static readonly Field MessageKey = new Field(nameof(MessageKey), 2, FieldType.Blob);
        public static readonly Field SigningPubKey = new Field(nameof(SigningPubKey), 3, FieldType.Blob);

        // ReSharper disable once RedundantArgumentNameForLiteralExpression
        public static readonly Field TxnSignature = new Field(nameof(TxnSignature), 4, FieldType.Blob, isSigningField: false);
        public static readonly Field Generator = new Field(nameof(Generator), 5, FieldType.Blob);
        public static readonly Field Signature = new Field(nameof(Signature), 6, FieldType.Blob);
        public static readonly Field Domain = new Field(nameof(Domain), 7, FieldType.Blob);
        public static readonly Field FundCode = new Field(nameof(FundCode), 8, FieldType.Blob);
        public static readonly Field RemoveCode = new Field(nameof(RemoveCode), 9, FieldType.Blob);
        public static readonly Field ExpireCode = new Field(nameof(ExpireCode), 10, FieldType.Blob);
        public static readonly Field CreateCode = new Field(nameof(CreateCode), 11, FieldType.Blob);
        public static readonly Field MemoType = new Field(nameof(MemoType), 12, FieldType.Blob);
        public static readonly Field MemoData = new Field(nameof(MemoData), 13, FieldType.Blob);
        public static readonly Field MemoFormat = new Field(nameof(MemoFormat), 14, FieldType.Blob);

        public static readonly Field Account = new Field(nameof(Account), 1, FieldType.AccountId);
        public static readonly Field Owner = new Field(nameof(Owner), 2, FieldType.AccountId);
        public static readonly Field Destination = new Field(nameof(Destination), 3, FieldType.AccountId);
        public static readonly Field Issuer = new Field(nameof(Issuer), 4, FieldType.AccountId);
        public static readonly Field Target = new Field(nameof(Target), 7, FieldType.AccountId);
        public static readonly Field RegularKey = new Field(nameof(RegularKey), 8, FieldType.AccountId);

        public static readonly Field ObjectEndMarker = new Field(nameof(ObjectEndMarker), 1, FieldType.StObject);
        public static readonly Field TransactionMetaData = new Field(nameof(TransactionMetaData), 2, FieldType.StObject);
        public static readonly Field CreatedNode = new Field(nameof(CreatedNode), 3, FieldType.StObject);
        public static readonly Field DeletedNode = new Field(nameof(DeletedNode), 4, FieldType.StObject);
        public static readonly Field ModifiedNode = new Field(nameof(ModifiedNode), 5, FieldType.StObject);
        public static readonly Field PreviousFields = new Field(nameof(PreviousFields), 6, FieldType.StObject);
        public static readonly Field FinalFields = new Field(nameof(FinalFields), 7, FieldType.StObject);
        public static readonly Field NewFields = new Field(nameof(NewFields), 8, FieldType.StObject);
        public static readonly Field TemplateEntry = new Field(nameof(TemplateEntry), 9, FieldType.StObject);
        public static readonly Field Memo = new Field(nameof(Memo), 10, FieldType.StObject);
        public static readonly Field SignerEntry = new Field(nameof(SignerEntry), 11, FieldType.StObject);
        public static readonly Field Signer = new Field(nameof(Signer), 16, FieldType.StObject);

        public static readonly Field ArrayEndMarker = new Field(nameof(ArrayEndMarker), 1, FieldType.StArray);
        //    SigningAccounts(2, FieldType.StArray),

        // ReSharper disable once RedundantArgumentNameForLiteralExpression
        public static readonly Field Signers = new Field(nameof(Signers), 3, FieldType.StArray, isSigningField: false);
        public static readonly Field SignerEntries = new Field(nameof(SignerEntries), 4, FieldType.StArray);
        public static readonly Field Template = new Field(nameof(Template), 5, FieldType.StArray);
        public static readonly Field Necessary = new Field(nameof(Necessary), 6, FieldType.StArray);
        public static readonly Field Sufficient = new Field(nameof(Sufficient), 7, FieldType.StArray);
        public static readonly Field AffectedNodes = new Field(nameof(AffectedNodes), 8, FieldType.StArray);
        public static readonly Field Memos = new Field(nameof(Memos), 9, FieldType.StArray);

        public static readonly Field CloseResolution = new Field(nameof(CloseResolution), 1, FieldType.Uint8);
        public static readonly Field TemplateEntryType = new Field(nameof(TemplateEntryType), 2, FieldType.Uint8);
        public static readonly Field TransactionResult = new Field(nameof(TransactionResult), 3, FieldType.Uint8);

        public static readonly Field TakerPaysCurrency = new Field(nameof(TakerPaysCurrency), 1, FieldType.Hash160);
        public static readonly Field TakerPaysIssuer = new Field(nameof(TakerPaysIssuer), 2, FieldType.Hash160);
        public static readonly Field TakerGetsCurrency = new Field(nameof(TakerGetsCurrency), 3, FieldType.Hash160);
        public static readonly Field TakerGetsIssuer = new Field(nameof(TakerGetsIssuer), 4, FieldType.Hash160);

        public static readonly Field Paths = new Field(nameof(Paths), 1, FieldType.PathSet);

        public static readonly Field Indexes = new Field(nameof(Indexes), 1, FieldType.Vector256);
        public static readonly Field Hashes = new Field(nameof(Hashes), 2, FieldType.Vector256);
        public static readonly Field Features = new Field(nameof(Features), 3, FieldType.Vector256);

        public static readonly Field Transaction = new Field(nameof(Transaction), 1, FieldType.Transaction);
        public static readonly Field LedgerEntry = new Field(nameof(LedgerEntry), 1, FieldType.LedgerEntry);
        public static readonly Field Validation = new Field(nameof(Validation), 1, FieldType.Validation);
    }
}