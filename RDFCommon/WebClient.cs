using System;
using System.Net;

namespace RDFCommon
{
    public class LongWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 20 * 60 * 60 * 1000;
            return w;
        }
    }
}
