using RDFCommon.OVns.general;

namespace RDFCommon.OVns
{
    public class SpecialTypesClass
    {
        //public static UriPrefixed String = new UriPrefixed(xmlSchemaPrefix, "string", xmlSchemaNs);
        //public static UriPrefixed Bool = new UriPrefixed(xmlSchemaPrefix, "boolean", xmlSchemaNs);
        //public static UriPrefixed Decimal = new UriPrefixed(xmlSchemaPrefix, "decimal", xmlSchemaNs);
        //public static UriPrefixed Integer = new UriPrefixed(xmlSchemaPrefix, "integr", xmlSchemaNs);

        //public static UriPrefixed Float = new UriPrefixed(xmlSchemaPrefix, "float", xmlSchemaNs);
        //public static UriPrefixed Double = new UriPrefixed(xmlSchemaPrefix, "double", xmlSchemaNs);

        //public static UriPrefixed Date = new UriPrefixed(xmlSchemaPrefix, "date", xmlSchemaNs);
        //public static UriPrefixed Time = new UriPrefixed(xmlSchemaPrefix, "time", xmlSchemaNs);
        //public static UriPrefixed DateTime = new UriPrefixed(xmlSchemaPrefix, "dateTime", xmlSchemaNs);
        //public static UriPrefixed DateTimeStamp = new UriPrefixed(xmlSchemaPrefix, "dateTimeStamp", xmlSchemaNs);

        //public static UriPrefixed GYear = new UriPrefixed(xmlSchemaPrefix, "gYear", xmlSchemaNs);
        //public static UriPrefixed GMonth = new UriPrefixed(xmlSchemaPrefix, "gMonth", xmlSchemaNs);
        //public static UriPrefixed GDay = new UriPrefixed(xmlSchemaPrefix, "gDay", xmlSchemaNs);
        //public static UriPrefixed GYearMonth = new UriPrefixed(xmlSchemaPrefix, "gYearMonth", xmlSchemaNs);
        //public static UriPrefixed GMonthDay = new UriPrefixed(xmlSchemaPrefix, "gMonthDay", xmlSchemaNs);
        //public static UriPrefixed Duration = new UriPrefixed(xmlSchemaPrefix, "duration", xmlSchemaNs);
        //public static UriPrefixed YearMonthDuration = new UriPrefixed(xmlSchemaPrefix, "yearMonthDuration", xmlSchemaNs);
        //public static UriPrefixed DayTimeDuration = new UriPrefixed(xmlSchemaPrefix, "dayTimeDuration", xmlSchemaNs);


        //public static UriPrefixed Byte = new UriPrefixed(xmlSchemaPrefix, "byte", xmlSchemaNs);
        //public static UriPrefixed Short = new UriPrefixed(xmlSchemaPrefix, "short", xmlSchemaNs);
        //public static UriPrefixed Int = new UriPrefixed(xmlSchemaPrefix, "int", xmlSchemaNs);
        //public static UriPrefixed Long = new UriPrefixed(xmlSchemaPrefix, "long", xmlSchemaNs);
        //public static UriPrefixed UnsignedByte = new UriPrefixed(xmlSchemaPrefix, "unsignedByte", xmlSchemaNs);
        //public static UriPrefixed unsignedShort = new UriPrefixed(xmlSchemaPrefix, "unsignedShort", xmlSchemaNs);
        //public static UriPrefixed unsignedInt = new UriPrefixed(xmlSchemaPrefix, "unsignedInt", xmlSchemaNs);
        //public static UriPrefixed unsignedLong = new UriPrefixed(xmlSchemaPrefix, "unsignedLong", xmlSchemaNs);
        //public static UriPrefixed positiveInteger = new UriPrefixed(xmlSchemaPrefix, "positiveInteger", xmlSchemaNs);
        //public static UriPrefixed nonNegativeInteger = new UriPrefixed(xmlSchemaPrefix, "nonNegativeInteger", xmlSchemaNs);
        //public static UriPrefixed negativeInteger = new UriPrefixed(xmlSchemaPrefix, "negativeInteger", xmlSchemaNs);
        //public static UriPrefixed nonPositiveInteger = new UriPrefixed(xmlSchemaPrefix, "nonPositiveInteger", xmlSchemaNs);


        //public static UriPrefixed hexBinary = new UriPrefixed(xmlSchemaPrefix, "hexBinary", xmlSchemaNs);
        //public static UriPrefixed base64Binary = new UriPrefixed(xmlSchemaPrefix, "base64Binary", xmlSchemaNs);
        //public static UriPrefixed anyURI = new UriPrefixed(xmlSchemaPrefix, "anyURI", xmlSchemaNs);
        //public static UriPrefixed language = new UriPrefixed(xmlSchemaPrefix, "language", xmlSchemaNs);
        //public static UriPrefixed normalizedString = new UriPrefixed(xmlSchemaPrefix, "normalizedString", xmlSchemaNs);
        //public static UriPrefixed token = new UriPrefixed(xmlSchemaPrefix, "token", xmlSchemaNs);
        //public static UriPrefixed NMTOKEN = new UriPrefixed(xmlSchemaPrefix, "NMTOKEN", xmlSchemaNs);
        //public static UriPrefixed Name = new UriPrefixed(xmlSchemaPrefix, "Name", xmlSchemaNs);
        //public static UriPrefixed NCName = new UriPrefixed(xmlSchemaPrefix, "NCName", xmlSchemaNs);

        //public static UriPrefixed LangString = new UriPrefixed("rdf:", "langString", rdf_syntax_ns);
        ////public static UriPrefixed SimpleLiteral = new UriPrefixed("smpl:", "", simple_literal);

        //public static UriPrefixed RdfFirst = new UriPrefixed("rdf:", "first", rdf_syntax_ns);
        //public static UriPrefixed RdfRest = new UriPrefixed("rdf:", "rest", rdf_syntax_ns);
        //   public static UriPrefixed RdfType = new UriPrefixed("rdf:", "type", rdf_syntax_ns);
        //public static UriPrefixed Nil = new UriPrefixed("rdf:", "nil", rdf_syntax_ns);
        //public ObjectVariants date;
        //public ObjectVariants @string;
        ////public ObjectVariants simpleLiteral;
        //public ObjectVariants langString;
        //public ObjectVariants integer;
        //public ObjectVariants @decimal;
        //public ObjectVariants @double;
        //public ObjectVariants @bool;
        //public ObjectVariants @float;
        //public ObjectVariants timeDuration;
        //public ObjectVariants dateTime;
        public ObjectVariants nil=new OV_iri(Nil);
        public ObjectVariants first=new OV_iri(RdfFirst);
        public ObjectVariants rest=new OV_iri(RdfRest);
        public ObjectVariants type;
        //private const string rdf_syntax_ns = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        //private const string xmlSchemaNs = "http://www.w3.org/2001/XMLSchema#";
        //private const string xmlSchemaPrefix="xsd:";

        //public static string[] GetAll()
        //{
        //    return new[]
        //    {
        //      // String,Bool,Decimal,Integer, Float, Double, 
        //      // Date, Time, DateTime, DateTimeStamp,
        //      // GYear, GMonth, GDay, GYearMonth,           GMonthDay,      Duration   ,YearMonthDuration  , DayTimeDuration,
        //      // Byte, Short, Int, Long, UnsignedByte, unsignedShort, unsignedInt, unsignedLong,  positiveInteger,   nonNegativeInteger, negativeInteger, nonPositiveInteger,
        //      //hexBinary, base64Binary, anyURI, language, normalizedString, token, NMTOKEN,  Name, NCName   
        //     Nil   ,RdfFirst,  RdfRest, RdfType
        //    };
        //}
       public SpecialTypesClass(NodeGenerator nodeGenerator)
        {
        //    date =nodeGenerator. CreateUriNode(Date);
        //    @string = nodeGenerator.CreateUriNode(String);
        //    //simpleLiteral = simple_literal_equals_string_literal
        //    //    ? String
        //    //    :  nodeGenerator.CreateUriNode(SpecialTypes.SimpleLiteral);
        //    langString = nodeGenerator.CreateUriNode(String);
        //    integer = nodeGenerator.CreateUriNode(Integer);
        //    @decimal = nodeGenerator.CreateUriNode(Decimal);
        //    @double = nodeGenerator.CreateUriNode(Double);
        //    @bool = nodeGenerator.CreateUriNode(Bool);
        //    @float = nodeGenerator.CreateUriNode(Float);
        //    timeDuration = nodeGenerator.CreateUriNode(DayTimeDuration);
        //    dateTime = nodeGenerator.CreateUriNode(DateTime);
            //nil = nodeGenerator.AddIri(Nil);
            //first = nodeGenerator.AddIri(RdfFirst);
            //rest = nodeGenerator.AddIri(RdfRest);
            type = nodeGenerator.AddIri(RdfType);        
        }

        public const string String = "http://www.w3.org/2001/XMLSchema#string";
        public const string Bool = "http://www.w3.org/2001/XMLSchema#boolean";
        public const string Decimal = "http://www.w3.org/2001/XMLSchema#decimal";
        public const string Integer = "http://www.w3.org/2001/XMLSchema#integer";

        public const string Float = "http://www.w3.org/2001/XMLSchema#float";
        public const string Double = "http://www.w3.org/2001/XMLSchema#double";

        public const string Date = "http://www.w3.org/2001/XMLSchema#date";
        public const string Time = "http://www.w3.org/2001/XMLSchema#time";
        public const string DateTime = "http://www.w3.org/2001/XMLSchema#dateTime";
        public const string DateTimeStamp = "http://www.w3.org/2001/XMLSchema#dateTimeStamp";

        public const string GYear = "http://www.w3.org/2001/XMLSchema#gYear";
        public const string GMonth = "http://www.w3.org/2001/XMLSchema#gMonth";
        public const string GDay = "http://www.w3.org/2001/XMLSchema#gDay";
        public const string GYearMonth = "http://www.w3.org/2001/XMLSchema#gYearMonth";
        public const string GMonthDay = "http://www.w3.org/2001/XMLSchema#gMonthDay";
        public const string Duration = "http://www.w3.org/2001/XMLSchema#duration";
        public const string YearMonthDuration = "http://www.w3.org/2001/XMLSchema#yearMonthDuration";
        public const string DayTimeDuration = "http://www.w3.org/2001/XMLSchema#dayTimeDuration";


        public const string Byte = "http://www.w3.org/2001/XMLSchema#byte";
        public const string Short = "http://www.w3.org/2001/XMLSchema#short";
        public const string Int = "http://www.w3.org/2001/XMLSchema#int";
        public const string Long = "http://www.w3.org/2001/XMLSchema#long";
        public const string UnsignedByte = "http://www.w3.org/2001/XMLSchema#unsignedByte";
        public const string unsignedShort = "http://www.w3.org/2001/XMLSchema#unsignedShort";
        public const string unsignedInt = "http://www.w3.org/2001/XMLSchema#unsignedInt";
        public const string unsignedLong = "http://www.w3.org/2001/XMLSchema#unsignedLong";
        public const string positiveInteger = "http://www.w3.org/2001/XMLSchema#positiveInteger";
        public const string nonNegativeInteger = "http://www.w3.org/2001/XMLSchema#nonNegativeInteger";
        public const string negativeInteger = "http://www.w3.org/2001/XMLSchema#negativeInteger";
        public const string nonPositiveInteger = "http://www.w3.org/2001/XMLSchema#nonPositiveInteger";


        public const string hexBinary = "http://www.w3.org/2001/XMLSchema#hexBinary";
        public const string base64Binary = "http://www.w3.org/2001/XMLSchema#base64Binary";
        public const string anyURI = "http://www.w3.org/2001/XMLSchema#anyURI";
        public const string language = "http://www.w3.org/2001/XMLSchema#language";
        public const string normalizedString = "http://www.w3.org/2001/XMLSchema#normalizedString";
        public const string token = "http://www.w3.org/2001/XMLSchema#token";
        public const string NMTOKEN = "http://www.w3.org/2001/XMLSchema#NMTOKEN";
        public const string Name = "http://www.w3.org/2001/XMLSchema#Name";
        public const string NCName = "http://www.w3.org/2001/XMLSchema#NCName";

        public const string LangString = "http://www.w3.org/1999/02/22-rdf-syntax-ns#langString";
        //public const string SimpleLiteral = new UriPrefixed("smpl:", "", simple_literal);

        public const string RdfFirst = "http://www.w3.org/1999/02/22-rdf-syntax-ns#first";
        public const string RdfRest = "http://www.w3.org/1999/02/22-rdf-syntax-ns#rest";
        public const string RdfType = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
        public const string Nil = "http://www.w3.org/1999/02/22-rdf-syntax-ns#nil";
    }

}