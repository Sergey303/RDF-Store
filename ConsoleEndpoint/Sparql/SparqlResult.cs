namespace SparqlQuery.SparqlClasses.Query.Result
{
    using System;
    using System.Collections.Generic;

    public class SparqlResult //: ISparqlResult
    {
        private readonly Dictionary<string, object> _bindings = new Dictionary<string, object>();

        public object this[string varName]
        {
            get => this.Contains(varName) ? this._bindings[varName] : null;
            set
            {
                if (this._bindings.ContainsKey(varName))
                {
                    this._bindings[varName] = value;
                }
                else
                {
                    this._bindings.Add(varName, value);
                }
            }
        }

        public int GetAsIri(string varName)
        {
            switch (this._bindings[varName])
            {
                case object[] ov:
                    return (int)ov[1];
                case int code:
                    return code;
                default:
                    throw new AggregateException(varName);
            }
        }

        public object[] GetAsOV(string varName)
        {
            switch (this._bindings[varName])
            {
                case object[] ov:
                    return ov;
                case int code:
                    return new object[] { 1, code };
                default:
                    throw new AggregateException(varName);
            }
        }

        public bool Contains(string varName) => this._bindings.ContainsKey(varName);
    }

    //public interface ISparqlResult
    //{
    //    IndexedProperty<string, object[]> OV { get; }
    
    //    IndexedProperty<string, int> Iri { get; }

    //    bool Contains(string varName);

    //    object this[string varName] { get; set; }
    //}
    }