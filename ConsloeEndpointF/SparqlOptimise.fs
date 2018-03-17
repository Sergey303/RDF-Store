module SparqlOptimise
open System.Linq
open ConsoleEndpoint.Interfaces
open ConsoleEndpoint.Sparql


let GetNextFilters (sparqlFilters) (existsVariavles:string[])=  
            let testFilter (filter:SparqlFilter) = filter.Variables |> Seq.forall (fun s-> existsVariavles.Contains(s))                                                        
            sparqlFilters |> Seq.filter testFilter;

let isSVar (triple:SparqlTriple) = triple.isSubjectVariable
let isPVar (triple:SparqlTriple) = triple.isPredicateVariable
let isOVar (triple:SparqlTriple) = triple.isObjectVariable
let sVar (triple:SparqlTriple) = triple.subjectVarName
let pVar (triple:SparqlTriple) = triple.predicateVarName
let oVar (triple:SparqlTriple) = triple.objectVarName

let AllKnown (existsVariavles:string[]) triple= 
    (not (isSVar triple) || existsVariavles.Contains(sVar triple)) 
        && 
    (not (isPVar triple) || existsVariavles.Contains(pVar triple)) 
        && 
    (not (isOVar triple) || existsVariavles.Contains(oVar triple));
let GetTriplesWithAllKnown (sparqlWere: SparqlTriple[]) (existsVariavles:string[]) = sparqlWere |> Array.filter (AllKnown existsVariavles);

let getSubjectSearchCount (counts:IStore4SparqlOptimise) (triple:SparqlTriple) (variableBindigs:SparqlResult) =
    if (not (isSVar triple)) then counts.GetCountOfSubject triple.subject 
    else if (variableBindigs.Contains(sVar triple)) then counts.GetCountOfSubject (variableBindigs.GetAsIri(sVar triple));
    else System.UInt32.MaxValue
let getPredicateSearchCount (counts:IStore4SparqlOptimise) (triple:SparqlTriple) (variableBindigs:SparqlResult) =
    if (not (isPVar triple)) then counts.GetCountOfPredicate triple.predicate
    else if (variableBindigs.Contains(pVar triple)) then counts.GetCountOfPredicate (variableBindigs.GetAsIri(pVar triple));
    else System.UInt32.MaxValue
let getObjSearchCount (counts:IStore4SparqlOptimise) (triple:SparqlTriple) (variableBindigs:SparqlResult) =
    if (not (isOVar triple)) then counts.GetCountOfObject triple._object
    else if (variableBindigs.Contains(sVar triple)) then counts.GetCountOfSubject (variableBindigs.GetAsIri(oVar triple));
    else System.UInt32.MaxValue
let GetTripleWithMinSearch (counts:IStore4SparqlOptimise) (sparqlWere: SparqlTriple[]) (variableBindigs:SparqlResult) = 
    let minSearch4Triple triple= 
        let s = getSubjectSearchCount counts triple variableBindigs;
        let p = getPredicateSearchCount counts triple variableBindigs;
        let o = getObjSearchCount counts triple variableBindigs;        
        seq { yield s; yield p; yield o } |> Seq.min
    sparqlWere |> Array.sortBy minSearch4Triple

