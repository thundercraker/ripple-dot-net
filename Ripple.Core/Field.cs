using Newtonsoft.Json.Linq;

namespace Ripple.Core
{
    public delegate ISerializedType FromJson(JToken token);

    public class Field : EnumItem
    {
        #region members
        public readonly bool IsSigningField;
        public readonly bool IsSerialised;
        public readonly bool IsVlEncoded;
        public readonly int NthOfType;
        public readonly Type Type;
        public readonly byte[] Header;

        public FromJson FromJson;

        #endregion

        public static readonly Enumeration<Field> Values = new Enumeration<Field>();
        public Field(string name, 
            int nthOfType,
            Type type, 
            bool isSigningField=true, 
            bool isSerialised=true) : 
                base(name, 
                    (type.Ordinal << 16 | nthOfType))
        {
            Type = type;
            IsSigningField = isSigningField;
            IsSerialised = isSerialised;
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
            return Type == Type.Vector256 || 
                   Type == Type.Blob || 
                   Type == Type.AccountId;
        }

        public static readonly Field Generic = new Field(nameof(Generic), 0, Type.Unknown);
        public static readonly Field Invalid = new Field(nameof(Invalid), -1, Type.Unknown);

        public static readonly Field LedgerEntryType = new Field(nameof(LedgerEntryType), 1, Type.Uint16);
        public static readonly Field TransactionType = new Field(nameof(TransactionType), 2, Type.Uint16);
        public static readonly Field SignerWeight = new Field(nameof(SignerWeight), 3, Type.Uint16);

        public static readonly Field Flags = new Field(nameof(Flags), 2, Type.Uint32);
        public static readonly Field SourceTag = new Field(nameof(SourceTag), 3, Type.Uint32);
        public static readonly Field Sequence = new Field(nameof(Sequence), 4, Type.Uint32);
        public static readonly Field PreviousTxnLgrSeq = new Field(nameof(PreviousTxnLgrSeq), 5, Type.Uint32);
        public static readonly Field LedgerSequence = new Field(nameof(LedgerSequence), 6, Type.Uint32);
        public static readonly Field CloseTime = new Field(nameof(CloseTime), 7, Type.Uint32);
        public static readonly Field ParentCloseTime = new Field(nameof(ParentCloseTime), 8, Type.Uint32);
        public static readonly Field SigningTime = new Field(nameof(SigningTime), 9, Type.Uint32);
        public static readonly Field Expiration = new Field(nameof(Expiration), 10, Type.Uint32);
        public static readonly Field TransferRate = new Field(nameof(TransferRate), 11, Type.Uint32);
        public static readonly Field WalletSize = new Field(nameof(WalletSize), 12, Type.Uint32);
        public static readonly Field OwnerCount = new Field(nameof(OwnerCount), 13, Type.Uint32);
        public static readonly Field DestinationTag = new Field(nameof(DestinationTag), 14, Type.Uint32);

        public static readonly Field HighQualityIn = new Field(nameof(HighQualityIn), 16, Type.Uint32);
        public static readonly Field HighQualityOut = new Field(nameof(HighQualityOut), 17, Type.Uint32);
        public static readonly Field LowQualityIn = new Field(nameof(LowQualityIn), 18, Type.Uint32);
        public static readonly Field LowQualityOut = new Field(nameof(LowQualityOut), 19, Type.Uint32);
        public static readonly Field QualityIn = new Field(nameof(QualityIn), 20, Type.Uint32);
        public static readonly Field QualityOut = new Field(nameof(QualityOut), 21, Type.Uint32);
        public static readonly Field StampEscrow = new Field(nameof(StampEscrow), 22, Type.Uint32);
        public static readonly Field BondAmount = new Field(nameof(BondAmount), 23, Type.Uint32);
        public static readonly Field LoadFee = new Field(nameof(LoadFee), 24, Type.Uint32);
        public static readonly Field OfferSequence = new Field(nameof(OfferSequence), 25, Type.Uint32);
        public static readonly Field FirstLedgerSequence = new Field(nameof(FirstLedgerSequence), 26, Type.Uint32); // Deprecated: do not use
        // Added new semantics in 9486fc416ca7c59b8930b734266eed4d5b714c50
        public static readonly Field LastLedgerSequence = new Field(nameof(LastLedgerSequence), 27, Type.Uint32);
        public static readonly Field TransactionIndex = new Field(nameof(TransactionIndex), 28, Type.Uint32);
        public static readonly Field OperationLimit = new Field(nameof(OperationLimit), 29, Type.Uint32);
        public static readonly Field ReferenceFeeUnits = new Field(nameof(ReferenceFeeUnits), 30, Type.Uint32);
        public static readonly Field ReserveBase = new Field(nameof(ReserveBase), 31, Type.Uint32);
        public static readonly Field ReserveIncrement = new Field(nameof(ReserveIncrement), 32, Type.Uint32);
        public static readonly Field SetFlag = new Field(nameof(SetFlag), 33, Type.Uint32);
        public static readonly Field ClearFlag = new Field(nameof(ClearFlag), 34, Type.Uint32);
        public static readonly Field SignerQuorum = new Field(nameof(SignerQuorum), 35, Type.Uint32);


        public static readonly Field IndexNext = new Field(nameof(IndexNext), 1, Type.Uint64);
        public static readonly Field IndexPrevious = new Field(nameof(IndexPrevious), 2, Type.Uint64);
        public static readonly Field BookNode = new Field(nameof(BookNode), 3, Type.Uint64);
        public static readonly Field OwnerNode = new Field(nameof(OwnerNode), 4, Type.Uint64);
        public static readonly Field BaseFee = new Field(nameof(BaseFee), 5, Type.Uint64);
        public static readonly Field ExchangeRate = new Field(nameof(ExchangeRate), 6, Type.Uint64);
        public static readonly Field LowNode = new Field(nameof(LowNode), 7, Type.Uint64);
        public static readonly Field HighNode = new Field(nameof(HighNode), 8, Type.Uint64);

        public static readonly Field EmailHash = new Field(nameof(EmailHash), 1, Type.Hash128);

        public static readonly Field LedgerHash = new Field(nameof(LedgerHash), 1, Type.Hash256);
        public static readonly Field ParentHash = new Field(nameof(ParentHash), 2, Type.Hash256);
        public static readonly Field TransactionHash = new Field(nameof(TransactionHash), 3, Type.Hash256);
        public static readonly Field AccountHash = new Field(nameof(AccountHash), 4, Type.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field PreviousTxnID = new Field(nameof(PreviousTxnID), 5, Type.Hash256);
        public static readonly Field LedgerIndex = new Field(nameof(LedgerIndex), 6, Type.Hash256);
        public static readonly Field WalletLocator = new Field(nameof(WalletLocator), 7, Type.Hash256);
        public static readonly Field RootIndex = new Field(nameof(RootIndex), 8, Type.Hash256);
        // Added in rippled commit: 9486fc416ca7c59b8930b734266eed4d5b714c50
        // ReSharper disable once InconsistentNaming
        public static readonly Field AccountTxnID = new Field(nameof(AccountTxnID), 9, Type.Hash256);
        public static readonly Field BookDirectory = new Field(nameof(BookDirectory), 16, Type.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field InvoiceID = new Field(nameof(InvoiceID), 17, Type.Hash256);
        public static readonly Field Nickname = new Field(nameof(Nickname), 18, Type.Hash256);
        public static readonly Field Amendment = new Field(nameof(Amendment), 19, Type.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field TicketID = new Field(nameof(TicketID), 20, Type.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field hash = new Field(nameof(hash), 257, Type.Hash256);
        // ReSharper disable once InconsistentNaming
        public static readonly Field index = new Field(nameof(index), 258, Type.Hash256);

        public static readonly Field Amount = new Field(nameof(Amount), 1, Type.Amount);
        public static readonly Field Balance = new Field(nameof(Balance), 2, Type.Amount);
        public static readonly Field LimitAmount = new Field(nameof(LimitAmount), 3, Type.Amount);
        public static readonly Field TakerPays = new Field(nameof(TakerPays), 4, Type.Amount);
        public static readonly Field TakerGets = new Field(nameof(TakerGets), 5, Type.Amount);
        public static readonly Field LowLimit = new Field(nameof(LowLimit), 6, Type.Amount);
        public static readonly Field HighLimit = new Field(nameof(HighLimit), 7, Type.Amount);
        public static readonly Field Fee = new Field(nameof(Fee), 8, Type.Amount);
        public static readonly Field SendMax = new Field(nameof(SendMax), 9, Type.Amount);
        public static readonly Field MinimumOffer = new Field(nameof(MinimumOffer), 16, Type.Amount);
        public static readonly Field RippleEscrow = new Field(nameof(RippleEscrow), 17, Type.Amount);
        // Added in rippled commit: e7f0b8eca69dd47419eee7b82c8716b3aa5a9e39
        public static readonly Field DeliveredAmount = new Field(nameof(DeliveredAmount), 18, Type.Amount);
        // These are auxiliary fields
        //    quality(257, Type.AMOUNT),
        // ReSharper disable once InconsistentNaming
        public static readonly Field taker_gets_funded = new Field(nameof(taker_gets_funded), 258, Type.Amount);
        // ReSharper disable once InconsistentNaming
        public static readonly Field taker_pays_funded = new Field(nameof(taker_pays_funded), 259, Type.Amount);

        public static readonly Field PublicKey = new Field(nameof(PublicKey), 1, Type.Blob);
        public static readonly Field MessageKey = new Field(nameof(MessageKey), 2, Type.Blob);
        public static readonly Field SigningPubKey = new Field(nameof(SigningPubKey), 3, Type.Blob);
        public static readonly Field TxnSignature = new Field(nameof(TxnSignature), 4, Type.Blob);
        public static readonly Field Generator = new Field(nameof(Generator), 5, Type.Blob);
        public static readonly Field Signature = new Field(nameof(Signature), 6, Type.Blob);
        public static readonly Field Domain = new Field(nameof(Domain), 7, Type.Blob);
        public static readonly Field FundCode = new Field(nameof(FundCode), 8, Type.Blob);
        public static readonly Field RemoveCode = new Field(nameof(RemoveCode), 9, Type.Blob);
        public static readonly Field ExpireCode = new Field(nameof(ExpireCode), 10, Type.Blob);
        public static readonly Field CreateCode = new Field(nameof(CreateCode), 11, Type.Blob);
        public static readonly Field MemoType = new Field(nameof(MemoType), 12, Type.Blob);
        public static readonly Field MemoData = new Field(nameof(MemoData), 13, Type.Blob);
        public static readonly Field MemoFormat = new Field(nameof(MemoFormat), 14, Type.Blob);

        public static readonly Field Account = new Field(nameof(Account), 1, Type.AccountId);
        public static readonly Field Owner = new Field(nameof(Owner), 2, Type.AccountId);
        public static readonly Field Destination = new Field(nameof(Destination), 3, Type.AccountId);
        public static readonly Field Issuer = new Field(nameof(Issuer), 4, Type.AccountId);
        public static readonly Field Target = new Field(nameof(Target), 7, Type.AccountId);
        public static readonly Field RegularKey = new Field(nameof(RegularKey), 8, Type.AccountId);

        public static readonly Field ObjectEndMarker = new Field(nameof(ObjectEndMarker), 1, Type.StObject);
        public static readonly Field TransactionMetaData = new Field(nameof(TransactionMetaData), 2, Type.StObject);
        public static readonly Field CreatedNode = new Field(nameof(CreatedNode), 3, Type.StObject);
        public static readonly Field DeletedNode = new Field(nameof(DeletedNode), 4, Type.StObject);
        public static readonly Field ModifiedNode = new Field(nameof(ModifiedNode), 5, Type.StObject);
        public static readonly Field PreviousFields = new Field(nameof(PreviousFields), 6, Type.StObject);
        public static readonly Field FinalFields = new Field(nameof(FinalFields), 7, Type.StObject);
        public static readonly Field NewFields = new Field(nameof(NewFields), 8, Type.StObject);
        public static readonly Field TemplateEntry = new Field(nameof(TemplateEntry), 9, Type.StObject);
        public static readonly Field Memo = new Field(nameof(Memo), 10, Type.StObject);
        public static readonly Field SignerEntry = new Field(nameof(SignerEntry), 11, Type.StObject);
        public static readonly Field Signer = new Field(nameof(Signer), 16, Type.StObject);

        public static readonly Field ArrayEndMarker = new Field(nameof(ArrayEndMarker), 1, Type.StArray);
        //    SigningAccounts(2, Type.StArray),
        public static readonly Field Signers = new Field(nameof(Signers), 3, Type.StArray);
        public static readonly Field SignerEntries = new Field(nameof(SignerEntries), 4, Type.StArray);
        public static readonly Field Template = new Field(nameof(Template), 5, Type.StArray);
        public static readonly Field Necessary = new Field(nameof(Necessary), 6, Type.StArray);
        public static readonly Field Sufficient = new Field(nameof(Sufficient), 7, Type.StArray);
        public static readonly Field AffectedNodes = new Field(nameof(AffectedNodes), 8, Type.StArray);
        public static readonly Field Memos = new Field(nameof(Memos), 9, Type.StArray);

        public static readonly Field CloseResolution = new Field(nameof(CloseResolution), 1, Type.Uint8);
        public static readonly Field TemplateEntryType = new Field(nameof(TemplateEntryType), 2, Type.Uint8);
        public static readonly Field TransactionResult = new Field(nameof(TransactionResult), 3, Type.Uint8);

        public static readonly Field TakerPaysCurrency = new Field(nameof(TakerPaysCurrency), 1, Type.Hash160);
        public static readonly Field TakerPaysIssuer = new Field(nameof(TakerPaysIssuer), 2, Type.Hash160);
        public static readonly Field TakerGetsCurrency = new Field(nameof(TakerGetsCurrency), 3, Type.Hash160);
        public static readonly Field TakerGetsIssuer = new Field(nameof(TakerGetsIssuer), 4, Type.Hash160);

        public static readonly Field Paths = new Field(nameof(Paths), 1, Type.PathSet);

        public static readonly Field Indexes = new Field(nameof(Indexes), 1, Type.Vector256);
        public static readonly Field Hashes = new Field(nameof(Hashes), 2, Type.Vector256);
        public static readonly Field Features = new Field(nameof(Features), 3, Type.Vector256);

        public static readonly Field Transaction = new Field(nameof(Transaction), 1, Type.Transaction);
        public static readonly Field LedgerEntry = new Field(nameof(LedgerEntry), 1, Type.LedgerEntry);
        public static readonly Field Validation = new Field(nameof(Validation), 1, Type.Validation);
    }
}