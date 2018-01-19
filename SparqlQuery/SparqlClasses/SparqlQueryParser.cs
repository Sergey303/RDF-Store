using System.IO;
using Antlr4.Runtime;
using RDFCommon.Interfaces;
using SparqlQuery.SparqlClasses.Query.Result;

namespace SparqlQuery.SparqlClasses
{
    using SparqlQuery.SparqlClasses.Query;

    public static class SparqlQueryParser
    {
        public static SparqlQuery ParseSparql(this IStore store, string sparqlString)
        {
            ICharStream input = new AntlrInputStream(sparqlString);

            var lexer = new sparq11lTranslatorLexer(input);

            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);

            var sparqlParser = new sparq11lTranslatorParser(commonTokenStream) { q = new RdfQuery11Translator(store) };



            return sparqlParser.query().value;

        }

        public static SparqlResultSet ParseRunSparql(this IStore store, string sparqlString) => store.ParseSparql(sparqlString).Run();
        public static SparqlResultSet ParseRunSparql(this IStore store, Stream sparqlString) => store.ParseSparql(sparqlString).Run();

        public static SparqlQuery ParseSparql(this IStore store, Stream sparql)
        {

            ICharStream input = new AntlrInputStream(sparql);

            var lexer = new sparq11lTranslatorLexer(input);

            var commonTokenStream = new CommonTokenStream(lexer);

            var sparqlParser = new sparq11lTranslatorParser(commonTokenStream) { q = new RdfQuery11Translator(store) };



            return sparqlParser.query().value;

        }


    }
}
