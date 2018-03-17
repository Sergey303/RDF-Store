namespace ConsoleEndpoint.Sparql
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class Prologue
    {
        public Dictionary<string, string> namspace2Prefix = new Dictionary<string, string>();
        public Dictionary<string, string> prefix2Namspace = new Dictionary<string, string>();
          
        public static readonly Regex PrefixNSSlpit = new Regex("^([^:]*:)(.*)$");
        private string baseUri;
        public string StringRepresentationOfProlog;

        public string GetUriFromPrefixed(string p)
        {
            if (p.StartsWith("<") && p.EndsWith(">"))
                return this.GetFromIri(p); 
            var uriPrefixed = SplitPrefixed(p);
            // if(prefix=="_")   throw new NotImplementedException();
            if (!this.prefix2Namspace.TryGetValue(uriPrefixed.prefix, out var fullNamespace))
                throw new Exception("prefix " + uriPrefixed.prefix);
            
            return fullNamespace + uriPrefixed.localname;
        }

       
        public static PrefixLocalName SplitPrefixed(string p)
        {  
            var match = PrefixNSSlpit.Match(p);
            var prefix = match.Groups[1].Value;
            var localName = match.Groups[2].Value;
            return new PrefixLocalName(prefix, localName);
        }
                public static NamespaceLocalName SplitUri(string p)
                {
                    
            var rsi = p.LastIndexOf('\\');
            var lsi = p.LastIndexOf('/');
            var ssi = p.LastIndexOf('#');
            var dot = p.LastIndexOf('.');
            var i = Math.Max(rsi, Math.Max(lsi, Math.Max(ssi, dot)));
            return new NamespaceLocalName( p.Substring(i + 1), p.Substring(0, i+1));
        }
        public string GetUriFromPrefixedNamespace(string p)
        {
            var match = PrefixNSSlpit.Match(p);
            var prefix = match.Groups[1].Value;
           
          //  if (prefix == "_" ) throw new NotImplementedException();
            if (!this.prefix2Namspace.TryGetValue(prefix, out var fullNamespace)) throw new Exception("prefix " + prefix);
            return fullNamespace;
        }

        public string GetFromString(string p)
        {
            if (p.StartsWith("<") && p.EndsWith(">"))
            {
                return this.GetFromIri(p);
            }
            if (p.StartsWith("http://") || p.StartsWith("mailto:"))
            {
                return p;
            }
            return this.GetUriFromPrefixed(p);
        }

        public string GetFromIri(string p)
        {      
            if(p.StartsWith("<") && p.EndsWith(">"))           
            p = p.Substring(1, p.Length - 2);
            return this.baseUri == null ? p : this.baseUri + p;
        }




        public void SetBase(string p)
        {
            this.baseUri = p;
        }

        public void AddPrefix(string prefix, string ns)
        {
            ns = ns.Substring(1, ns.Length - 2);
            this.prefix2Namspace.Add(prefix, ns);
            this.namspace2Prefix.Add(ns, prefix);
        }


      
    }

    public struct IriPrefixed
    {
        public readonly string ns, prefix, localName;

        public IriPrefixed(string ns, string prefix, string localName)
        {
            this.ns = ns;
            this.prefix = prefix;
            this.localName = localName;
        }
    }

    public struct PrefixLocalName
    {
        public readonly string prefix;
        public readonly string localname;

        public PrefixLocalName(string prefix, string localname) 
        {
            this.prefix = prefix;
            this.localname = localname;
        }
    }
     public struct NamespaceLocalName
    {
        public readonly string @namespace;
        public readonly string localname;

        public NamespaceLocalName(string @namespace, string localname) 
        {
            this.@namespace = @namespace;
            this.localname = localname;
        }

         public string FullName { get { return this.@namespace + this.localname; } }
    }
}