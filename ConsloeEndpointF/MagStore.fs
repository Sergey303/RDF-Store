module MagStore
open Polar.DB;
open System
open System.Diagnostics

type TripleObjectVariant = Dumm | Iri of int | Str of string
let TripleObjectVariant2Writeble (tripleObjectVariant:TripleObjectVariant) :obj[] =  
    match tripleObjectVariant with
    | Dumm -> null
    | Iri iri-> [|0:> obj, iri:> obj|]
    | Str str-> [|1 :> obj, str:> obj |]

let Writeble2TripleObjectVariant (o:obj) = match o with 
| null -> Dumm 
| :? array<obj> as value -> match value.[0] :?> int with
                                        | 0 -> TripleObjectVariant.Iri (value.[1] :?> int) 
                                        | 1 -> TripleObjectVariant.Str (value.[1] :?> string)
                                        | _ -> failwith("aadsa");
| _ -> failwith("aaasddddadadasd");


let  Triple2Writeble struct(s,p,o) : obj[] = [|s, p, TripleObjectVariant2Writeble(o) |]
let  Writeble2Triple (writable:obj[]) = struct (writable.[0] :?> int,writable.[1]:?> int, Writeble2TripleObjectVariant writable.[2]) 

let tp_ov = new PTypeUnion(
                new NamedType("dummy", new PType(PTypeEnumeration.none)),
                new NamedType("iri", new PType(PTypeEnumeration.integer)),
                new NamedType("str", new PType(PTypeEnumeration.sstring)));
let tp_triple=new PTypeRecord(
                new NamedType("subj", new PType(PTypeEnumeration.integer)),
                new NamedType("pred", new PType(PTypeEnumeration.integer)), 
                new NamedType("obj", tp_ov));
let store tab_stream spo_stream =
    let table = new UniversalSequenceBase(tp_triple, tab_stream);    
    let keyFunc i (o:obj) = (o :?> obj[]).[i] :?> int
    let compare a b = (a:>IComparable).CompareTo(b)
    let rec spoICompare (i:int) (a:obj) (b:obj) :int = match (i, compare (keyFunc i a) (keyFunc i b))  with 
                                    | (2, c) -> c
                                    | (_, 0) -> spoICompare (i+1) a b
                                    | (_, c) -> c;
    let spo_comparer = Collections.Generic.Comparer.Create(new Comparison<obj>(spoICompare 0))    
    let index_spo = new UniversalSequenceCompKey32(spo_stream, new Func<obj, int>(keyFunc 0) ,spo_comparer , table);   
    let mutable scaleFunc = None;
    let GetKeyFromIndex i= ((index_spo.GetByIndex i) :?> obj[]).[0] :?> int
    let longSize = table.Count();
    if(longSize > 0L) then
        scaleFunc <- Some (Scale.GetDiaFunc32 [|for i in 0L .. longSize-1L -> GetKeyFromIndex(i) |]);
    let Load triples =
        table.Clear();
        for tri in triples do
            index_spo.AppendElement([| keyFunc 0 tri, table.AppendElement(tri) |]) |> ignore 
        table.Flush();
        index_spo.Flush();        
    let Build=    
             let nelements = int (table.Count());
             let arr_offs = Array.zeroCreate<int64> (nelements);
             arr_offs.[0] <- 8L;
             for i = 1 to nelements-1 do
                arr_offs.[i] <- table.ElementOffset();
             let keys = arr_offs |> Array.map (table.GetElement >> (keyFunc 0))            
             Array.Sort(keys, arr_offs);
             scaleFunc <- Some (Scale.GetDiaFunc32 keys);
             let arr_triples =  Array.zeroCreate<ValueTuple<int,int,TripleObjectVariant>> nelements;
             let LocalSort index length :unit =        
                // выделен блок, начало index, длина lengh, читаем по офсетам, пишем по местам
                for i =index to index + length-1 do
                    let t = table.GetElement(arr_offs.[i]) :?> obj[]
                    arr_triples.[i] <- Writeble2Triple t                    
                // сортируем
                Array.Sort(arr_triples, arr_offs, index, length, spo_comparer);
                //// чистим память от объектов
                //for j = index to index + length do
                //    arr_triples.[j] <- null;           
           
             // Выделяем группы одинаковых ключей и сортируем по компаратору
             let mutable index = 0;
             let mutable length = 0;
             let mutable current_key = keys.[index];
             for i = 0 to nelements-1 do
                let key = keys.[i];

                if (key = current_key) then length<-length+1              
                else                
                    if (length > 39) then 
                        LocalSort index length;
                    // начинается новый блок
                    index <- i;
                    length <- 1;
                    current_key <- key;                
            
             if (length > 39) then 
                    LocalSort index length;

             // Записываем итог
             index_spo.Clear();
             for i = 0 to nelements-1 do index_spo.AppendElement([| keys.[i], arr_offs.[i] |]);
             index_spo.Flush();             
             1    
    let Look = 
            let GetOffsetFromIndex (pair:obj) = (pair :?> obj[]).[1] :?> int64
            for _ in index_spo.ElementValues() 
                            |> Seq.take 10                             
                            |> Seq.map  (GetOffsetFromIndex >> 
                                            table.GetElement >> 
                                            tp_triple.Interpret >> 
                                            Console.Write) do
            Console.WriteLine();
            let nprobe = 1000;
            let rnd = new Random();
            let sw = new System.Diagnostics.Stopwatch();    
            sw.Restart();
            for i = 0 to nprobe-1 do
                let subj = rnd.Next((int)(index_spo.Count() / 2L));
                let sample = struct (subj,0,Dumm);
                let writebleSample=Triple2Writeble sample;
                let key = keyFunc 0 writebleSample;
                let (start,number) = match scaleFunc with 
                                        | Some scaleF ->
                                            let dia = scaleF.Invoke(key);
                                            (dia.start, dia.numb);
                                        | None -> (0L, index_spo.Count());
                
                let res = index_spo.BinarySearchAll(start, number, key, writebleSample) //|> Seq.map table.GetElement
                                    |> Seq.length                                  
                Console.WriteLine("res.Count() = " + res.ToString());
                
            sw.Stop();
            Console.WriteLine(nprobe.ToString()+" GetAll search ok. duration= "+sw.ElapsedMilliseconds.ToString());
        
    let GetTriplesBySubj (subj:int) :Collections.Generic.IEnumerable<obj> =        
        let writebleSample=Triple2Writeble struct (subj, 0, Dumm);
        let key = keyFunc 0 writebleSample;
        let (start,number) = match scaleFunc with 
                                        | Some scaleF ->
                                            let dia = scaleF.Invoke(key);
                                            (dia.start, dia.numb);
                                        | None -> (0L, index_spo.Count());  
        index_spo.BinarySearchAll(start, number, key, writebleSample) |> Seq.map table.GetElement
    (Load, Build,GetTriplesBySubj)
        
        

  