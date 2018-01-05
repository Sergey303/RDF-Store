using System;
using System.Xml;

namespace SparqlQuery.SparqlClasses.Expressions
{
   public class SparqlSubstringExpression  : SparqlExpression
   {
       private SparqlExpression strExpression;

       private SparqlExpression substExpression;

       private SparqlExpression lengthExpression;

       private RdfQuery11Translator q;

       internal void SetString(SparqlExpression value)
       {
           this.strExpression = value;
       }

       public void SetStartPosition(SparqlExpression substr)
       {
           this.substExpression = substr;
           switch (NullablePairExt.Get(this.strExpression.Const, this.substExpression.Const))
           {
               case NP.bothNull:
                   this.Operator = result => this.strExpression.Operator(result).Substring(this.substExpression.Operator(result));
                   this.TypedOperator =
                       result => this.strExpression.TypedOperator(result)
                               .Change(o => o.Substring(this.substExpression.Operator(result)));
                   this.AggregateLevel = SetAggregateLevel(this.strExpression.AggregateLevel, this.substExpression.AggregateLevel);
                   break;
               case NP.leftNull:
                   this.Operator = result => this.strExpression.Operator(result).Substring(this.substExpression.Const.Content);
                   this.TypedOperator =
                       result => this.substExpression.Const.
                           Change(o => this.strExpression.Operator(result).Substring(o));
                   this.AggregateLevel = this.strExpression.AggregateLevel;
                   break;
               case NP.rigthNull:
                   this.Operator = result => ((string)this.strExpression.Const.Content).Substring(this.substExpression.Operator(result));
                   this.TypedOperator = result => this.strExpression.Const.Change(o => o.Substring(this.substExpression.Operator(result)));
                   this.AggregateLevel = this.substExpression.AggregateLevel;
                   break;
               case NP.bothNotNull:
                   this.Const = this.strExpression.Const.Change(o => o.Substring(this.substExpression.Const.Content));
                   break;
               default:
                   throw new ArgumentOutOfRangeException();
           }
       }

       internal void SetLength(SparqlExpression lengthExpression)
       {
          this.lengthExpression = lengthExpression;
           switch (NullableTripleExt.Get(this.strExpression.Const, this.substExpression.Const, lengthExpression.Const))
           {
               case NT.AllNull:
                   this.TypedOperator = result => this.strExpression.TypedOperator(result).Change(o => o.Substring(this.substExpression.Operator(result), lengthExpression.Operator(result)));
                   this.Operator = result => this.strExpression.Operator(result).Substring(this.substExpression.Operator(result), lengthExpression.Operator(result));
                   this.AggregateLevel = SetAggregateLevel(this.strExpression.AggregateLevel, this.substExpression.AggregateLevel, lengthExpression.AggregateLevel);
                   break;
               case NT.FirstNotNull :
                   this.TypedOperator = result => this.strExpression.Const.Change(o => o.Substring(this.substExpression.Operator(result), lengthExpression.Operator(result)));
                   this.Operator = result => ((string)this.strExpression.Const.Content).Substring(this.substExpression.Operator(result), lengthExpression.Operator(result));
                   this.AggregateLevel = SetAggregateLevel(this.substExpression.AggregateLevel, lengthExpression.AggregateLevel);                   
                   break;
               case NT.SecondNotNull:
                   this.TypedOperator = result => this.strExpression.TypedOperator(result).Change(o => o.Substring(this.substExpression.Const.Content, lengthExpression.Operator(result)));
                   this.Operator = result => this.strExpression.Operator(result).Substring(this.substExpression.Const.Content, lengthExpression.Operator(result));
                   this.AggregateLevel = SetAggregateLevel(this.strExpression.AggregateLevel, lengthExpression.AggregateLevel);                   
                   break;
               case NT.ThirdNotNull:
                   this.TypedOperator = result => this.strExpression.TypedOperator(result).Change(o => o.Substring(this.substExpression.Operator(result), lengthExpression.Const.Content));
                   this.Operator = result => this.strExpression.Operator(result).Substring(this.substExpression.Operator(result), lengthExpression.Const.Content);
                   this.AggregateLevel = SetAggregateLevel(this.strExpression.AggregateLevel, this.substExpression.AggregateLevel);                   
                   break;
               case (NT)254: // ~ NT.FirstNotNull 
                   this.TypedOperator = result => this.strExpression.TypedOperator(result).Change(o => o.Substring(this.substExpression.Const.Content, lengthExpression.Const.Content));
                   this.Operator = result => this.strExpression.Operator(result).Substring(this.substExpression.Const.Content, lengthExpression.Const.Content);
                   this.AggregateLevel = this.strExpression.AggregateLevel;
                   break;
               case (NT)253:// ~NT.SecondNotNull 
                   this.TypedOperator = result => this.strExpression.Const.Change(o => o.Substring(this.substExpression.Operator(result), lengthExpression.Const.Content));
                   this.Operator = result => ((string)this.strExpression.Const.Content).Substring(this.substExpression.Operator(result), (int) lengthExpression.Const.Content);
                   this.AggregateLevel = this.substExpression.AggregateLevel;
                   break;
               case (NT)252: // ~ NT.ThirdNotNull 
                   this.TypedOperator = result => this.strExpression.Const.Change(o => o.Substring(this.substExpression.Const.Content, lengthExpression.Operator(result)));
                   this.Operator = result => ((string)this.strExpression.Const.Content).Substring((int)this.substExpression.Const.Content, lengthExpression.Operator(result));
                   this.AggregateLevel = lengthExpression.AggregateLevel;
                   break;
               case (NT)255: // ~NT.AllNull
                   this.Const = this.strExpression.Const.Change(o => o.Substring(this.substExpression.Const.Content, lengthExpression.Const.Content));
                   break;
               default:
                   throw new ArgumentOutOfRangeException();
           }
       }

       public override void WriteXml(XmlWriter writer)
       {
           writer.WriteStartElement("substring");
            writer.WriteAttributeString("type", this.GetType().ToString());
            this.strExpression.WriteXml(writer);
           this.substExpression.WriteXml(writer);
            if(this.lengthExpression!=null) this.lengthExpression.WriteXml(writer);
            writer.WriteEndElement();
       }
   }
}
