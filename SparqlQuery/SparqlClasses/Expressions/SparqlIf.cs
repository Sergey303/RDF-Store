using System;

namespace SparqlQuery.SparqlClasses.Expressions
{
    class SparqlIf : SparqlExpression
    {
        public SparqlIf(SparqlExpression conditionExpression1, SparqlExpression sparqlExpression2, SparqlExpression sparqlExpression3)
        { 
            switch (NullableTripleExt.Get(conditionExpression1.Const, sparqlExpression2.Const, sparqlExpression3.Const))
            {
                case NT.AllNull:
                    this.Operator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.Operator(result) : sparqlExpression3.Operator(result);
                        }
 throw new ArgumentException();
                    };
                    this.TypedOperator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.TypedOperator(result) : sparqlExpression3.TypedOperator(result);
                        }
 throw new ArgumentException();
                    };
                    this.AggregateLevel = SetAggregateLevel(conditionExpression1.AggregateLevel, sparqlExpression2.AggregateLevel, sparqlExpression3.AggregateLevel);
                    break;
                case NT.FirstNotNull:
                    if ((bool) conditionExpression1.Const.Content)
                    {
                        this.Operator = sparqlExpression2.Operator;
                        this.TypedOperator = sparqlExpression2.TypedOperator;
                    }
                    else
                    {
                        this.Operator = sparqlExpression3.Operator;
                        this.TypedOperator = sparqlExpression3.TypedOperator;   
                    }

                    this.AggregateLevel = SetAggregateLevel(sparqlExpression2.AggregateLevel, sparqlExpression3.AggregateLevel);

                    break;
                case NT.SecondNotNull:
                    this.Operator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.Const.Content : sparqlExpression3.Operator(result);
                        }
 throw new ArgumentException();
                    };
                    this.TypedOperator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.Const : sparqlExpression3.TypedOperator(result);
                        }
 throw new ArgumentException();
                    };
                    this.AggregateLevel = SetAggregateLevel(conditionExpression1.AggregateLevel, sparqlExpression3.AggregateLevel);

                    break;
                case NT.ThirdNotNull:
                    this.Operator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.Operator(result) : sparqlExpression3.Const.Content;
                        }
 throw new ArgumentException();
                    };
                    this.TypedOperator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.TypedOperator(result) : sparqlExpression3.Const;
                        }
 throw new ArgumentException();
                    };
                    this.AggregateLevel = SetAggregateLevel(conditionExpression1.AggregateLevel, sparqlExpression2.AggregateLevel);

                    break;
                case (NT)252:// ~NT.ThirdNotNull:
                    if ((bool)conditionExpression1.Const.Content)
                    {
                        this.Const = sparqlExpression2.Const;
                    }
                    else
                    {
                        this.Operator = sparqlExpression3.Operator;
                        this.TypedOperator = sparqlExpression3.TypedOperator;
                    }

                    this.AggregateLevel =sparqlExpression3.AggregateLevel;
                    break;
                case (NT)253:// ~NT.SecondNotNull:
                    if ((bool)conditionExpression1.Const.Content)
                    {
                        this.Operator = sparqlExpression2.Operator;
                        this.TypedOperator = sparqlExpression2.TypedOperator;
                    }
                    else
                    {
                        this.Const = sparqlExpression3.Const;
                    }

                    this.AggregateLevel = sparqlExpression2.AggregateLevel;
                    break;
                case (NT)254:// ~NT.FirstNotNull:
                    this.Operator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.Const.Content : sparqlExpression3.Const.Content;
                        }
 throw new ArgumentException();
                    };
                    this.TypedOperator = result =>
                    {
                        var condition = conditionExpression1.Operator(result);
                        if (condition is bool)
                        {
                            return (bool)condition ? sparqlExpression2.Const : sparqlExpression3.Const;
                        }
 throw new ArgumentException();
                    };
                    this.AggregateLevel = conditionExpression1.AggregateLevel;                    
                    break;
                case (NT)255:// ~NT.AllNull:
                    this.Const = (bool) conditionExpression1.Const.Content ? sparqlExpression2.Const : sparqlExpression3.Const;
                    this.AggregateLevel = VariableDependenceGroupLevel.Const;                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
        }
    }
}
