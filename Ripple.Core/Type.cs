namespace Ripple.Core
{
    public class Type : EnumItem
    {
        public static Enumeration<Type> Values = new Enumeration<Type>(); 
        public Type(string name, int ordinal) : base(name, ordinal)
        {
            Values.AddEnum(this);
        }
        public static readonly Type Unknown = new Type(nameof(Unknown), -2);
        public static readonly Type Done = new Type(nameof(Done), -1);
        public static readonly Type NotPresent = new Type(nameof(NotPresent), 0);
        public static readonly Type Uint16 = new Type(nameof(Uint16), 1);
        public static readonly Type Uint32 = new Type(nameof(Uint32), 2);
        public static readonly Type Uint64 = new Type(nameof(Uint64), 3);
        public static readonly Type Hash128 = new Type(nameof(Hash128), 4);
        public static readonly Type Hash256 = new Type(nameof(Hash256), 5);
        public static readonly Type Amount = new Type(nameof(Amount), 6);
        public static readonly Type Blob = new Type(nameof(Blob), 7);
        public static readonly Type AccountId = new Type(nameof(AccountId), 8);
        public static readonly Type StObject = new Type(nameof(StObject), 14);
        public static readonly Type StArray = new Type(nameof(StArray), 15);
        public static readonly Type Uint8 = new Type(nameof(Uint8), 16);
        public static readonly Type Hash160 = new Type(nameof(Hash160), 17);
        public static readonly Type PathSet = new Type(nameof(PathSet), 18);
        public static readonly Type Vector256 = new Type(nameof(Vector256), 19);
        public static readonly Type Transaction = new Type(nameof(Transaction), 10001);
        public static readonly Type LedgerEntry = new Type(nameof(LedgerEntry), 10002);
        public static readonly Type Validation = new Type(nameof(Validation), 10003);


        /*

        public static readonly Type Uint8 = new Type(nameof(Uint8), 16);
          public static readonly Type Uint16 = new Type(nameof(Uint16), 1);
        public static readonly Type Uint32 = new Type(nameof(Uint32), 2);
        public static readonly Type Uint64 = new Type(nameof(Uint64), 3);

        public static readonly Type Hash128 = new Type(nameof(Hash128), 4);
        public static readonly Type Hash256 = new Type(nameof(Hash256), 5);
        public static readonly Type Hash160 = new Type(nameof(Hash160), 17);
            public static readonly Type AccountId = new Type(nameof(AccountId), 8);
        
        public static readonly Type Vector256 = new Type(nameof(Vector256), 19);
        public static readonly Type Blob = new Type(nameof(Blob), 7);
        
        public static readonly Type Amount = new Type(nameof(Amount), 6);
        public static readonly Type PathSet = new Type(nameof(PathSet), 18);
        
        public static readonly Type StObject = new Type(nameof(StObject), 14);
        public static readonly Type StArray = new Type(nameof(StArray), 15);

        */
    }
}