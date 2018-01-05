using RDFCommon;
using RDFCommon.Interfaces;
using RDFCommon.OVns;
using RDFCommon.OVns.general;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlIsNum : SparqlExpression
    {
        public SparqlIsNum(SparqlExpression value)
            :base(value.AggregateLevel, value.IsStoreUsed)
        {
            if (value.Const != null) this.Const = new OV_bool(value.Const is INumLiteral);
            else
            {
                this.Operator = result => value.TypedOperator(result) is INumLiteral;
                this.TypedOperator = result => new OV_bool(value.TypedOperator(result) is INumLiteral); // todo
            }

            // f is double ||
            // f is long ||
            // f is int ||
            // f is short ||
            // f is byte ||
            // f is ulong ||
            // f is uint ||
            // f is ushort;
        }
    }
}
// xsd:nonPositiveInteger
//xsd:negativeInteger
//xsd:long
//xsd:int
//xsd:short
//xsd:byte
//xsd:nonNegativeInteger
//xsd:unsignedLong
//xsd:unsignedInt
//xsd:unsignedShort
//xsd:unsignedByte
//xsd:positiveInteger;