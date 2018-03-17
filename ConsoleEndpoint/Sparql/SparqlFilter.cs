namespace ConsoleEndpoint.Sparql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ConsoleEndpoint.Interfaces;

    public class SparqlFilter : ISparqlGraphPattern
    {
        private HashSet<string> _variables;

        private Func<SparqlResult, bool> _operator;

        private SparqlFilter(Func<SparqlResult, bool> @operator,  HashSet<string> variables)
        {
            this._operator = @operator;
            this._variables = variables;
        }


        public IEnumerable<string> Variables => this._variables;

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings, IStore store)
        {
            return variableBindings.Where(this._operator);
        }

        public static (SparqlFilter Filtr, bool? Const) Create((Func<SparqlResult, object>, object, IEnumerable<string>) sparqlExpression)
        {
            var (@operator, constant, variables) = sparqlExpression;
            if (constant == null)
            {
                return (new SparqlFilter(result => (bool)@operator(result), new HashSet<string>(variables)), null);
            }
            else
            {
                return (null, (bool)constant);
            }
        }


        public static (Func<SparqlResult, object>, object, IEnumerable<string>) Smaller((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) => CreateBinary((o, o1) => (double)o < (double)o1, l, r);

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) Greather((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) => CreateBinary((o, o1) => (double)o > (double)o1, l, r);

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) SmallerOrEquals((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) => CreateBinary((o, o1) => (double)o <= (double)o1, l, r);

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) GreatherOrEquals((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) => CreateBinary((o, o1) => (double)o >= (double)o1, l, r);

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) PlusExpression((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) => CreateBinary((o, o1) => (double)o + (double)o1, l, r);

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) MinusExpression((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) => CreateBinary((o, o1) => (double)o - (double)o1, l, r);
        public static (Func<SparqlResult, object>, object, IEnumerable<string>) MultiplyExpression((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) => CreateBinary((o, o1) => (double)o * (double)o1, l, r);
        public static (Func<SparqlResult, object>, object, IEnumerable<string>) DivideExpression((Func<SparqlResult, object>, object, IEnumerable<string>) l, (Func<SparqlResult, object>, object, IEnumerable<string>) r) =>
            CreateBinary((o, o1) => (double)o / (double)o1, l, r);

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) NotExpression((Func<SparqlResult, object> expr, object constant, IEnumerable<string> variables) expr) =>
            CreateUnary(o => !(bool)o, expr);
        public static (Func<SparqlResult, object>, object, IEnumerable<string>) MinesExpression((Func<SparqlResult, object> expr, object constant, IEnumerable<string> variables) expr) => CreateUnary(o => -(double)o, expr);

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) CreateVariableExpression(string VarName) => ((SparqlResult sr) => sr[VarName], null, Enumerable.Repeat(VarName, 1));

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) CreateBinary(
            Func<object, object, object> op,
            (Func<SparqlResult, object>, object, IEnumerable<string>) l,
            (Func<SparqlResult, object>, object, IEnumerable<string>) r)
        {
            //VariableDependenceGroupLevel aggregateLevel;
            var(lOperator, lConst, lVariables) = l;
            var(rOperator, rConst, rVariables) = r;
            
            if (lConst == null && rConst == null)
            {
                return (result => op(lOperator(result), rOperator(result)), null, lVariables.Concat(rVariables));
                //aggregateLevel = SetAggregateLevel(lExp.AggregateLevel, rExp.AggregateLevel);
            }
            else if (lConst == null)
            {
                return (result => op(lOperator(result), rConst), null, lVariables);
                //aggregateLevel = lExp.AggregateLevel;
            }
            else if (rConst == null)
            {
                return (result => op(lConst, rOperator(result)), null, rVariables);
                //aggregateLevel = rExp.AggregateLevel;
            }
            else
            {
                return (default(Func<SparqlResult, object>), op(lConst, rConst), Enumerable.Empty<string>());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="operator"></param>
        /// <param name="child"></param>
        /// <param name="resultType"></param>
        /// <returns>SparqlUnaryExpression&lt;Tin, Tout&gt;
        /// Or object const</returns>
        public static (Func<SparqlResult, object>, object, IEnumerable<string>) CreateUnary(
            Func<object, object> @operator,
            (Func<SparqlResult, object> expression, object constant, IEnumerable<string> variables) child) =>
            child.constant == null
                ? ((SparqlResult result) => @operator(child.expression(result)), null, child.variables)
                : (default(Func<SparqlResult, object>), @operator(child.constant), Enumerable.Empty<string>());

        public static (Func<SparqlResult, object>, object, IEnumerable<string>) Variable(string varName) => (result => result[varName], null, Enumerable.Repeat(varName, 1));
    }
}