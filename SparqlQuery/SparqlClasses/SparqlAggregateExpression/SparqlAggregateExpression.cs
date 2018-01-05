using System;
using SparqlQuery.SparqlClasses.Expressions;

namespace SparqlQuery.SparqlClasses.SparqlAggregateExpression
{
    using System.Xml;

    public abstract class SparqlAggregateExpression  : SparqlExpression
    {
        public bool isAll;
        public bool IsDistinct;
             private SparqlExpression expression;

     

        internal void IsAll()
        {
            this.isAll = true;
        }

        public SparqlExpression Expression
        {
            get { return this.expression; }
            set
            {
                this.expression = value;
                switch (this.Expression.AggregateLevel)
                {
                    case VariableDependenceGroupLevel.Const:
                    case VariableDependenceGroupLevel.UndependableFunc:
                    case VariableDependenceGroupLevel.SimpleVariable:
                        this.AggregateLevel = VariableDependenceGroupLevel.Group;
                        break;
                    case VariableDependenceGroupLevel.Group:
                        this.AggregateLevel = VariableDependenceGroupLevel.GroupOfGroups;
                        break;
                    case VariableDependenceGroupLevel.GroupOfGroups:
                        throw new Exception("grop of group of group is not implemented");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                this.Create();
            }
        }

        protected abstract void Create();

        public string Separator { get; set; }

        public override void 
            
            WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("al",  AggregateLevel.ToString());
            writer.WriteAttributeString("isALl", this.isAll.ToString());
            writer.WriteAttributeString("isDistinct", this.IsDistinct.ToString());
            this.expression.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

    // public class SparqlAggregateTreeExpression :SparqlAggregateExpression
    // {
    // public SparqlExpression SecondExpression;
    // }
    // public static class SparqlExpressionConstructor
    // {
    // public static SparqlExpression Create(this SparqlExpression child, Func<SparqlExpression, SparqlExpression> ctor)
    // {
    // var aggregateExpression = child as SparqlAggregateExpression;
    // if (aggregateExpression != null)
    // {
    // aggregateExpression.ParentExpression = ctor(aggregateExpression.ParentExpression);
    // return aggregateExpression;
    // }
    // else
    // return ctor(child);
    // }
    // public static SparqlExpression Create(this SparqlExpression child, SparqlExpression second, Func<SparqlExpression, SparqlExpression, SparqlExpression> ctor)
    // {
    // var aggregateExpression = child as SparqlAggregateExpression;
    // var secondAggregateExpression = second as SparqlAggregateExpression;
    // switch (NullablePairExt.Get(aggregateExpression, secondAggregateExpression))
    // {
    // case NP.bothNull:
    // return ctor(child, second);
    // break;
    // case NP.leftNull:
    // secondAggregateExpression.ParentExpression = ctor(child, secondAggregateExpression.ParentExpression);
    // return secondAggregateExpression;
    // break;
    // case NP.rigthNull:
    // aggregateExpression.ParentExpression = ctor(aggregateExpression.ParentExpression, second);
    // return secondAggregateExpression;
    // break;
    // case NP.bothNotNull:
    // return new SparqlAggregateTreeExpression() {Expression = aggregateExpression, SecondExpression=secondAggregateExpression};
    // break;
    // default:
    // throw new ArgumentOutOfRangeException();
    // }
          
    // }
    // }
}

