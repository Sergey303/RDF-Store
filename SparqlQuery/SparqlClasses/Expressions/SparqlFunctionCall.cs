using System;
using System.Linq;
using RDFCommon;
using RDFCommon.OVns;
using RDFCommon.OVns.date;
using RDFCommon.OVns.general;
using RDFCommon.OVns.numeric;

namespace SparqlQuery.SparqlClasses.Expressions
{
    public class SparqlFunctionCall  :SparqlExpression
    {
   // private SparqlUriNode sparqlUriNode;
 

        public SparqlFunctionCall(string sparqlUriNode, SparqlArgs sparqlArgs):base(SetAggregateLevel(sparqlArgs[0].AggregateLevel), sparqlArgs.Any(expression => expression.IsStoreUsed))
        {
            // TODO: Complete member initialization
            // this.sparqlUriNode = sparqlUriNode;
            
            SparqlExpression arg = sparqlArgs[0];
            Func<object, dynamic> f = null;
            Func<dynamic, ObjectVariants> ctor = null;
            if (Equals(sparqlUriNode, SpecialTypesClass.Bool))
            {
                f = o =>
                {
                    if (o is string)
                        return bool.Parse((string) o);
                    if (o is double || o is int || o is float || o is decimal)
                        return Math.Abs(Convert.ToDouble(o)) > 0;
                    if (o is bool)
                        return (bool) o;
                    throw new ArgumentException();
                };
                ctor = o => new OV_bool(o);
            }
            else if (Equals(sparqlUriNode, SpecialTypesClass.Double))
            {
                f = o =>
                {
                    if (o is string)
                        return double.Parse(((string) o).Replace(".", ","));
                    if (o is double || o is int || o is float || o is decimal)
                        return Convert.ToDouble(o);
                    throw new ArgumentException();
                };
                ctor = o => new OV_double(o);
            }
            else if (Equals(sparqlUriNode, SpecialTypesClass.Float))
            {                    
                    ctor = o => new OV_float(o);
                    f = o =>
                    {
                        var s = o as string;
                        if (s != null)
                            return float.Parse(s.Replace(".", ","));
                        if (o is double || o is int || o is float || o is decimal)
                            return (float) Convert.ToDouble(o);
                        throw new ArgumentException();     
                    };
            }
            else if (Equals(sparqlUriNode, SpecialTypesClass.Decimal))
            {
                ctor = o => new OV_decimal(o);
                f = o =>
                {
                    var s = o as string;
                    if (s != null)
                        return new OV_decimal(decimal.Parse(s.Replace(".", ",")));
                    if (o is double || o is int || o is float || o is decimal)
                        return new OV_decimal(Convert.ToDecimal(o));
                    throw new ArgumentException();
                };
            }
            else if (Equals(sparqlUriNode, SpecialTypesClass.Int))
            {

                ctor = o => new OV_int(o);
                f = o =>
                {
                    var s = o as string;
                    if (s != null)
                        return new OV_int(int.Parse(s));
                    if (o is double || o is int || o is float || o is decimal)
                        return new OV_int(Convert.ToInt32(o));
                    throw new ArgumentException();
                };
            }
            else if (Equals(sparqlUriNode, SpecialTypesClass.DateTime))
            {
                ctor = o => new OV_dateTime(o);
                f = o =>
                {
                    var s = o as string;
                    if (s != null)
                        return new OV_dateTime(DateTime.Parse(s));
                    if (o is DateTime)
                        return o;
                    throw new ArgumentException();
                };

            }
            else if (Equals(sparqlUriNode, SpecialTypesClass.String))
            {
                ctor = o => new OV_string(o);
                f= o => o.ToString();        
            }
            else
                throw new NotImplementedException("mathod call " + sparqlUriNode);

            if (arg.Const != null) this.Const = ctor(f(arg.Const.Content));
            else
            {
                this.Operator = result => f(arg.Operator(result));
                this.TypedOperator = result => ctor(this.Operator(result));
            }
        }

        // internal readonly Func<SparqlResult, dynamic> Func;
    }
}
