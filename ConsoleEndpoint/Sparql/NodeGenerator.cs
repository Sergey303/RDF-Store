using System;
using RDFCommon.OVns.general;

namespace RDFCommon.OVns
{
    public class NodeGenerator
    {

        
        public virtual ObjectVariants GetUri(object uri)
        {
          return new OV_iri((string) uri);
        }

      //  public SpecialTypesClass SpecialTypes { get; set; }


     

        public virtual ObjectVariants AddIri(string iri)
        {
            return new OV_iri(iri);
        }

        public static NodeGenerator Create()
        {
            var ng = new NodeGenerator();
           // ng.SpecialTypes=new SpecialTypesClass(ng);
            return ng;
        }



        //public ObjectVariants CreateLiteralNode(string p, string typeUriNode)
        //{
        //    p = p.Trim('"', '\'');

        //    switch (typeUriNode)
        //    {
        //        case SpecialTypesClass.String:
        //            return new OV_string(p);
        //        //case (SpecialTypesClass.@Bool):
        //        //    return new OV_bool(p);
        //        //case (SpecialTypesClass.@Decimal):
        //        //    return new OV_decimal(p);
        //        //case (SpecialTypesClass.Integer):
        //        //    return new OV_int(p);
        //        //case (SpecialTypesClass.@Float):
        //        //    return new OV_float(p);
        //        //case (SpecialTypesClass.@Double):
        //        //    return new OV_double(p);

        //        //case (SpecialTypesClass.Date):
        //        //    return new OV_date(p);
        //        //case (SpecialTypesClass.Time):
        //        //    return new OV_time(p);
        //        //case (SpecialTypesClass.DateTime):
        //        //    return new OV_dateTimeStamp(p);
        //        //case (SpecialTypesClass.DateTimeStamp):
        //        //    return new OV_dateTimeStamp(p);

        //        //case (SpecialTypesClass.GYear):
        //        //    return new OV_double(p);
        //        //case (SpecialTypesClass.GMonth):
        //        //    return new OV_double(p);
        //        //case (SpecialTypesClass.GDay):
        //        //    return new OV_double(p);
        //        //case (SpecialTypesClass.GYearMonth):
        //        //    return new OV_double(p);
        //        //case (SpecialTypesClass.GMonthDay):
        //        //    return new OV_double(p);
        //        //case (SpecialTypesClass.Duration):
        //        //    return new OV_double(p);
        //        //case (SpecialTypesClass.YearMonthDuration):
        //        //    return new OV_double(p);
        //        //case (SpecialTypesClass.DayTimeDuration):
        //        //    return new OV_double(p);

        //        //todo
        //        default:
        //            throw new NotImplementedException();
        //           // return CreateLiteralOtherType(p, typeUriNode);
        //    }
        //}

        public ObjectVariants CreateBlankNode(string blankNodeString, string graph=null)
        {

            return new OV_iri(blankNodeString); 
        }
        public string CreateBlank(string blankNodeString, string graph)
        {
            
            blankNodeString = blankNodeString.Substring(2);
            if (graph != null) blankNodeString = graph + "/" + blankNodeString;

            return "http://iis.nsk.su/.well-known/genid/blank/"+ blankNodeString;
        }
        public ObjectVariants CreateBlankNode()
        {
            return
                new OV_iri(CreateBlank());
        }

        public string BlankNodeGenerateNums()
        {
            return ((long)(random.NextDouble() * Math.Pow(10, 18))).ToString();
        }
        public string CreateBlank()
        {
            return "http://iis.nsk.su/.well-known/genid/blank" + BlankNodeGenerateNums();
        }
        private Random random = new Random();


        //public virtual ObjectVariants CreateLiteralOtherType(string p, string typeUriNode)
        //{
        //    return new OV_typed(p, typeUriNode);   
        //}

        public virtual bool TryGetUri(OV_iri iriString, out ObjectVariants iriCoded)
        {
            iriCoded = iriString;
            return true;
        }

        public virtual void Build()
        {
            
        }
    }
    
}
