using System;
using RDFCommon.OVns.date;
using RDFCommon.OVns.general;
using RDFCommon.OVns.numeric;

namespace RDFCommon.OVns
{
    public   static class ObjectVariantsEx
    {
       //public delegate ObjectVariants Create(object writable, Func<int, string> decode = null);

       // public static readonly Create[] w2ov =
       // {
       //     (s, nt) => new OV_iri((string) s),
       //     (s, decode) => new OV_iriint((int) s, decode),
       //     (s, nt) => new OV_bool((bool) s),
       //     (s, nt) => new OV_string((string) s),
       //     (strLang, nt) => new OV_langstring((string) ((object[]) strLang)[0], (string) ((object[]) strLang)[1]),
       //     (s, nt) => new OV_double((double) s),
       //     (s, nt) => new OV_decimal(Convert.ToDecimal(s)),
       //     (s, nt) => new OV_float((float) s),
       //     (s, nt) => new OV_int((int) s),
       //     (s, nt) => new OV_dateTimeStamp(DateTimeOffset.FromFileTime((long) s)), 
       //     (s, nt) => new OV_dateTime(DateTime.FromBinary((long) s)),
       //     (s, nt) => new OV_date(DateTime.FromBinary((long) s)),
       //     (s, nt) => new OV_time(DateTimeOffset.FromFileTime((long)s)),
       //     (typed, nt) => new OV_typed((string) ((object[]) typed)[0], (string) ((object[]) typed)[1]),
       //     (typed, decode) => new OV_typedint((string) ((object[]) typed)[0], (int) ((object[]) typed)[1], decode),
       // };


        public static ObjectVariants Writable2OVariant(this object[] @object, Func<int, string> getStringByCode = null)
        {
            switch ((ObjectVariantEnum) @object[0])
            {
                    case ObjectVariantEnum.Iri:
                    return new OV_iri((string) @object[1]);
                case ObjectVariantEnum.IriInt:
                    return new OV_iriint((int) @object[1], getStringByCode);
                    break;
                case ObjectVariantEnum.Bool:
                    return new OV_bool((bool) @object[1]);
                    break;
                case ObjectVariantEnum.Str:
                    return new OV_string((string) @object[1]);
                    break;
                case ObjectVariantEnum.LanguagedString:
                    return new OV_langstring((string) ((object[]) @object[1])[0], (string) ((object[]) @object[1])[1]);
                    break;
                case ObjectVariantEnum.Double:
                    return new OV_double((double) @object[1]);
                    break;
                case ObjectVariantEnum.Decimal:
                    return new OV_decimal(Convert.ToDecimal(@object[1]));
                    break;
                case ObjectVariantEnum.Float:
                    return new OV_float((float) @object[1]);
                    break;
                case ObjectVariantEnum.Int:
                    return new OV_int((int) @object[1]);
                    break;
                case ObjectVariantEnum.DateTimeZone:
                    return new OV_dateTimeStamp(DateTimeOffset.FromFileTime((long) @object[1]));
                    break;
                case ObjectVariantEnum.DateTime:
                    return new OV_dateTime(DateTime.FromBinary((long) @object[1]));
                    break;
                case ObjectVariantEnum.Date:
                    return new OV_date(DateTime.FromBinary((long) @object[1]));
                    break;
                case ObjectVariantEnum.Time:
                    return new OV_time(DateTimeOffset.FromFileTime((long) @object[1]));
                    break;
                case ObjectVariantEnum.Typed:
                    return new OV_typed((string) ((object[]) @object[1])[0], (string) ((object[]) @object[1])[1]);
                    break;
                case ObjectVariantEnum.OtherIntType:
                    throw new NotImplementedException();
                    return new OV_typedint((string) ((object[]) @object[1])[0], (int) ((object[]) @object[1])[1], null);
                    break;
                case ObjectVariantEnum.Index:
                    return new OV_index((int) @object[1]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
          
        }

        public static ObjectVariants ToOVariant(this object @object, NodeGenerator nt = null)
        {
            if (nt != null)
            {
                var nodeGeneratorInt = nt as NodeGeneratorInt;
                if (nodeGeneratorInt != null)
                    return Writable2OVariant((object[]) @object, nodeGeneratorInt.coding_table.GetString);
            }
            return Writable2OVariant((object[]) @object);
        }

        //public IComparable ToComparable(this object @object)
        //{
        //    var o = (object[])@object;
        //    if(o[0]==4 ||)
        //}
      
    }
}