using LiteDB;
using System;

namespace LiteDbMigrator
{
    public static class FieldConverters
    {
        public static readonly Func<BsonValue, BsonValue> FromIntToDecimal = bv =>
        {
            if (bv.IsInt32) return new BsonValue(Convert.ToDecimal(bv.AsInt32));
            if (bv.IsDecimal) return bv;
            return new BsonValue(0.0m); // fallback
        };

        public static readonly Func<BsonValue, BsonValue> FromDecimalToInt = bv =>
        {
            if (bv.IsDecimal) return new BsonValue((int)Math.Round(bv.AsDecimal));
            if (bv.IsInt32) return bv;
            return new BsonValue(0); // fallback
        };

        public static readonly Func<BsonValue, BsonValue> ToString = bv =>
        {
            var val = bv.RawValue.ToString();
            return val;
        };
    }
}
