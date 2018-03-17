using System;
using System.Collections.Generic;

namespace ConsoleEndpoint.Sparql
{
    using ConsoleEndpoint.Interfaces;

   public class SparqlTriple : ISparqlGraphPattern
    {

        private readonly string subjectSource;

        private readonly string predicateSource;

        private bool isSubjectCoded;

        private bool isPredicateCoded;

        private bool isObjectCoded;

        public readonly bool isSubjectVariable;

        public readonly bool isPredicateVariable;

        public readonly bool isObjectVariable;

        public readonly string subjectVarName;

        public readonly string predicateVarName;

        public readonly string objectVarName;

        public int subject;

        public int predicate;

        public object[] _object;

        private bool _constTriplePartExistsInStore = true;

        private readonly HashSet<string> variables=new HashSet<string>();

        public SparqlTriple(
            string sVar = null,
            string pVar = null,
            string oVar = null,
            string subjectSource = null,
            string predicateSource = null,
            string objectSource = null,
            OVT objectType=OVT.iri,
            int? subject = null,
            int? predicate = null,
            object[] o = null)
        {
            this.subjectSource = subjectSource;
            this.predicateSource = predicateSource;
            
            this.subjectVarName = sVar;
            this.predicateVarName = pVar;
            this.objectVarName = oVar;
            if (sVar != null)
            {
                this.isSubjectVariable = true;
                this.variables.Add(sVar);
            }
            else
            {
                this.isSubjectVariable = false;
                if (subject != null)
                {
                    this.subject = subject.Value;
                    this.isSubjectCoded = true;
                }
                else if (subjectSource != null)
                {
                    this.subjectSource = subjectSource;
                    this.isSubjectCoded = false;
                }
                else
                {
                    throw new AggregateException();
                }
            }

            if (pVar != null)
            {
                this.isPredicateVariable = true;
                this.variables.Add(pVar);
            }
            else
            {
                this.isPredicateVariable = false;
                if (predicate != null)
                {
                    this.predicate = predicate.Value;
                    this.isPredicateCoded = true;
                }
                else if (predicateSource != null)
                {
                    this.predicateSource = predicateSource;
                    this.isPredicateCoded = false;
                }
                else
                {
                    throw new AggregateException();
                }
            }

            if (oVar != null)
            {
                this.isObjectVariable = true;
                this.variables.Add(oVar);
            }
            else
            {
                this.isObjectVariable = false;
                if (o != null)
                {
                    this._object = o;
                }
                else if (objectSource != null)
                {
                    this._object = new object[] { objectType, objectSource };
                }
                else
                {
                    throw new AggregateException();
                }
            }
        }

        bool CodeConstants(IStore store)
        {
            if (!this._constTriplePartExistsInStore)
            {
                return false;
            }

            if (!this.isSubjectVariable && !this.isSubjectCoded)
            {
                this.subject = store.Nametable.GetCode(
                    this.subjectSource,
                    out this._constTriplePartExistsInStore);
                this.isSubjectCoded = true;
                if (!this._constTriplePartExistsInStore)
                {
                    return false;
                }
            }

            if (!this.isPredicateVariable && !this.isPredicateCoded)
            {
                this.predicate = store.Nametable.GetCode(
                    this.predicateSource,
                    out this._constTriplePartExistsInStore);
                this.isPredicateCoded = true;
                if (!this._constTriplePartExistsInStore)
                {
                    return false;
                }
            }

            if (!this.isObjectVariable && !this.isObjectCoded)
            {
                var(vid, ov) = this._object;
                if ((OVT)(int)vid == OVT.iri)
                {
                    this._object[1] = store.Nametable.GetCode((string)ov, out this._constTriplePartExistsInStore);
                }
                else
                {
                    this._constTriplePartExistsInStore = store.ContainsObject(this._object);
                }

                this.isObjectCoded = true;
                if (!this._constTriplePartExistsInStore)
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<SparqlResult> Run(IEnumerable<SparqlResult> variableBindings, IStore store)
        {
            if (!this._constTriplePartExistsInStore || !this.CodeConstants(store))
            {
                yield break;
            }

            foreach (var variableBinding in variableBindings)
            {
                int s=0, p;
                object[] o;
                var hasValueCase = HasValue.None;
                if (!this.isSubjectVariable)
                {
                    hasValueCase |= HasValue.Subject;
                    s = this.subject;
                }
                else if (variableBinding.Contains(this.subjectVarName))
                {
                    hasValueCase |= HasValue.Subject;
                    s = variableBinding.GetAsIri(this.subjectVarName);
                }
                else
                {
                    hasValueCase |= HasValue.None;
                }

                if (this.isPredicateVariable && !variableBinding.Contains(this.predicateVarName))
                {
                    hasValueCase |= HasValue.None;
                }
                else
                {
                    hasValueCase |= HasValue.Predicate;
                }

                hasValueCase |= this.isObjectVariable && !variableBinding.Contains(this.objectVarName)
                                    ? HasValue.None
                                    : HasValue.Object;
                
                 p = this.isPredicateVariable
                            ? (hasValueCase & HasValue.Predicate) != 0
                                  ? variableBinding.GetAsIri(this.predicateVarName)
                                  : 0
                            : this.predicate;
                o = this.isObjectVariable
                                 ? (hasValueCase & HasValue.Object) != 0
                                       ? variableBinding.GetAsOV(this.objectVarName)
                                       : null
                                 : this._object;
                switch (hasValueCase)
                {
                    case HasValue.Subject | HasValue.Predicate | HasValue.Object:
                        if (store.Contains(s, p, o))
                        {
                            yield return variableBinding;
                        }

                        break;
                    case HasValue.Subject | HasValue.Predicate:
                        foreach (var row in store.spO(s, p))
                        {
                            variableBinding[this.objectVarName] = (object[])row[2];
                            yield return variableBinding;
                        }

                        break;
                    case HasValue.Subject | HasValue.Object:
                        foreach (var row in store.sPo(s, o))
                        {
                            variableBinding[this.predicateVarName] = (int)row[1];
                            yield return variableBinding;
                        }

                        break;
                    case HasValue.Predicate | HasValue.Object:
                        foreach (var row in store.Spo(p, o))
                        {
                            variableBinding[this.subjectVarName] = (int)row[0];
                            yield return variableBinding;
                        }

                        break;
                    case HasValue.Subject:
                        foreach (var row in store.sPO(s))
                        {
                            variableBinding[this.predicateVarName] = (int)row[1];
                            variableBinding[this.objectVarName] = (object[])row[2];
                            yield return variableBinding;
                        }

                        break;
                    case HasValue.Predicate:
                        foreach (var row in store.SpO(p))
                        {
                            variableBinding[this.subjectVarName] = (int)row[0];
                            variableBinding[this.objectVarName] = (object[])row[2];
                            yield return variableBinding;
                        }

                        break;
                    case HasValue.Object:
                        foreach (var row in store.SPo(o))
                        {
                            variableBinding[this.subjectVarName] = (int)row[0];
                            variableBinding[this.predicateVarName] = (int)row[1];
                            yield return variableBinding;
                        }

                        break;
                    case HasValue.None:
                        foreach (var row in store.SPO())
                        {
                            variableBinding[this.subjectVarName] = (int)row[0];
                            variableBinding[this.predicateVarName] = (int)row[1];
                            variableBinding[this.objectVarName] = (object[])row[2];
                            yield return variableBinding;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public IEnumerable<string> Variables
        {
            get
            {
                if (this.isSubjectVariable)
                {
                    yield return this.subjectVarName;
                }

                if (this.isPredicateVariable)
                {
                    yield return this.predicateVarName;
                }

                if (this.isObjectVariable)
                {
                    yield return this.objectVarName;
                }
            }
        }

        /// <summary>
        /// The variables in triple.
        /// </summary>
        [Flags]
        private enum HasValue
        {
            /// <summary>
            /// all const.
            /// </summary>
            None = 0,

            /// <summary>
            /// The subject is variable.
            /// </summary>
            Subject = 1,

            /// <summary>
            /// The predicate is variable.
            /// </summary>
            Predicate = 2,

            /// <summary>
            /// The object is variable.
            /// </summary>
            Object = 4,
        }
    }
}