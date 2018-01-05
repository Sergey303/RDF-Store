using System;
using System.Collections.Generic;
using System.Linq;
using RDFCommon;
using RDFCommon.OVns;
using RDFCommon.OVns.general;
using RDFCommon.OVns.numeric;

namespace RDFTurtleParser
{



    public class Parser {
	public const int _EOF = 0;
	public const int _iriref = 1;
	public const int _pnamens = 2;
	public const int _pnameln = 3;
	public const int _blanknodelabel = 4;
	public const int _langtag = 5;
	public const int _integer = 6;
	public const int _decimal = 7;
	public const int _double = 8;
	public const int _stringliteralquote = 9;
	public const int _stringliteralsinglequote = 10;
	public const int _stringliterallongsinglequote = 11;
	public const int _stringliterallongquote = 12;
	public const int _anon = 13;
	public const int maxT = 29;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public string graphName;

NodeGenerator ng=NodeGenerator.Create();

public Action<string, string, ObjectVariants> ft;
private readonly Prologue prologue = new Prologue();

/*______________________________________________*/


	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void RDFTurtle() {
		Turtledoc();
	}

	void Turtledoc() {
		while (StartOf(1)) {
			Statement();
		}
	}

	void Statement() {
		if (StartOf(2)) {
			Directive();
		} else if (StartOf(3)) {
			Triples();
			Expect(14);
		} else SynErr(30);
	}

	void Directive() {
		if (la.kind == 15) {
			Prefixid();
		} else if (la.kind == 16) {
			Base();
		} else if (la.kind == 18) {
			Sparqlprefix();
		} else if (la.kind == 17) {
			Sparqlbase();
		} else SynErr(31);
	}

	void Triples() {
		string s; 
		if (StartOf(4)) {
			Subject(out s);
			Predicateobjectlist(s);
		} else if (la.kind == 22) {
			Blanknodepropertylist(out s);
			if (StartOf(5)) {
				Predicateobjectlist(s);
			}
		} else SynErr(32);
	}

	void Prefixid() {
		Expect(15);
		Expect(2);
		string pn=t.val; 
		Expect(1);
		prologue.AddPrefix(pn, t.val); 
		Expect(14);
	}

	void Base() {
		Expect(16);
		Expect(1);
		prologue.SetBase(t.val); 
		Expect(14);
	}

	void Sparqlprefix() {
		Expect(18);
		Expect(2);
		string pn=t.val; 
		Expect(1);
		prologue.AddPrefix(pn, t.val); 
	}

	void Sparqlbase() {
		Expect(17);
		Expect(1);
		prologue.SetBase(t.val); 
	}

	void Subject(out string value) {
		value=null; 
		if (la.kind == 1 || la.kind == 2 || la.kind == 3) {
			Iri(out value);
		} else if (la.kind == 4 || la.kind == 13) {
			Blanknode(out value);
		} else if (la.kind == 24) {
			Collection(out value);
		} else SynErr(33);
	}

	void Predicateobjectlist(string s) {
		string p; 
		Verb(out p);
		Objectlist(s,p);
		while (la.kind == 19) {
			Get();
			if (StartOf(5)) {
				Verb(out p);
				Objectlist(s,p);
			}
		}
	}

	void Blanknodepropertylist(out string value) {
		Expect(22);
		value= ng.CreateBlank(); 
		Predicateobjectlist(value);
		Expect(23);
	}

	void Verb(out string p) {
		p=null; 
		if (la.kind == 1 || la.kind == 2 || la.kind == 3) {
			Iri(out p);
		} else if (la.kind == 21) {
			Get();
			p = SpecialTypesClass.RdfType; 
		} else SynErr(34);
	}

	void Objectlist(string s, string p) {
		ObjectVariants ov; 
		Object(out ov);
		ft(s,p,ov); 
		while (la.kind == 20) {
			Get();
			Object(out ov);
			ft(s,p,ov); 
		}
	}

	void Object(out ObjectVariants value) {
		value=null;  string iri; 
		if (StartOf(3)) {
			if (la.kind == 1 || la.kind == 2 || la.kind == 3) {
				Iri(out iri);
			} else if (la.kind == 4 || la.kind == 13) {
				Blanknode(out iri);
			} else if (la.kind == 24) {
				Collection(out iri);
			} else {
				Blanknodepropertylist(out iri);
			}
			value = new OV_iri(iri); 
		} else if (StartOf(6)) {
			Rdfliteral(out value);
		} else if (la.kind == 6 || la.kind == 7 || la.kind == 8) {
			Numericliteral(out value);
		} else if (la.kind == 27 || la.kind == 28) {
			Booleanliteral(out value);
		} else SynErr(35);
	}

	void Iri(out string value) {
		value=null; 
		if (la.kind == 1) {
			Get();
			value=prologue.GetFromIri(t.val.Substring(1, t.val.Length-2)); 
		} else if (la.kind == 3) {
			Get();
			value=prologue.GetUriFromPrefixed(t.val); 
		} else if (la.kind == 2) {
			Get();
			value=prologue.GetUriFromPrefixedNamespace(t.val); 
		} else SynErr(36);
	}

	void Blanknode(out string value) {
		value=null; 
		if (la.kind == 4) {
			Get();
			value=ng.CreateBlank(t.val,graphName); 
		} else if (la.kind == 13) {
			Get();
			value=ng.CreateBlank(); 
		} else SynErr(37);
	}

	void Collection(out string value) {
		Expect(24);
		ObjectVariants ov; var nodes = new List<ObjectVariants>(); 
		while (StartOf(7)) {
			Object(out ov);
			nodes.Add(ov); 
		}
		Expect(25);
		var rdfFirst = SpecialTypesClass.RdfFirst;
		       var rdfRest = SpecialTypesClass.RdfRest;
		           string sparqlBlankNodeFirst = ng.CreateBlank();
		           string sparqlBlankNodeNext = ng.CreateBlank();
		       foreach (var node in nodes.Take(nodes.Count - 1))
		       {
		           ft(sparqlBlankNodeNext, rdfFirst, node);
		           ft(sparqlBlankNodeNext, rdfRest, new OV_iri(sparqlBlankNodeNext = ng.CreateBlank()));
		       }
		       ft(sparqlBlankNodeNext, rdfFirst, nodes[nodes.Count - 1]);
		       ft(sparqlBlankNodeNext, rdfRest, new OV_iri(SpecialTypesClass.Nil));
		       value = sparqlBlankNodeFirst;
		
	}

	void Rdfliteral(out ObjectVariants value) {
		value=null; 
		String();
		string str=t.val; 
		if (la.kind == 5 || la.kind == 26) {
			if (la.kind == 5) {
				Get();
				value=new OV_langstring(str.Trim('"', '\''), t.val); 
			} else {
				Get();
				string literalType; 
				Iri(out literalType);
				value = ng.CreateLiteralNode(str, literalType); 
			}
		}
		if(value==null) value=new OV_string(str.Trim('"', '\'')); 
	}

	void Numericliteral(out ObjectVariants value) {
		value=null; 
		if (la.kind == 6) {
			Get();
			value=new OV_int(t.val); 
		} else if (la.kind == 7) {
			Get();
			value=new OV_decimal(t.val.Replace(".", ",")); 
		} else if (la.kind == 8) {
			Get();
            value = new OV_double(t.val.Replace(".", ",")); 
		} else SynErr(38);
	}

	void Booleanliteral(out ObjectVariants value) {
		value=null; 
		if (la.kind == 27) {
			Get();
			value = new OV_bool(true); 
		} else if (la.kind == 28) {
			Get();
			value=new OV_bool(false); 
		} else SynErr(39);
	}

	void String() {
		if (la.kind == 9) {
			Get();
		} else if (la.kind == 10) {
			Get();
		} else if (la.kind == 11) {
			Get();
		} else if (la.kind == 12) {
			Get();
		} else SynErr(40);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		RDFTurtle();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, T,T,T,x, x,x,T,x, T,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, T,x,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,x,T, T,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "iriref expected"; break;
			case 2: s = "pnamens expected"; break;
			case 3: s = "pnameln expected"; break;
			case 4: s = "blanknodelabel expected"; break;
			case 5: s = "langtag expected"; break;
			case 6: s = "integer expected"; break;
			case 7: s = "decimal expected"; break;
			case 8: s = "double expected"; break;
			case 9: s = "stringliteralquote expected"; break;
			case 10: s = "stringliteralsinglequote expected"; break;
			case 11: s = "stringliterallongsinglequote expected"; break;
			case 12: s = "stringliterallongquote expected"; break;
			case 13: s = "anon expected"; break;
			case 14: s = "\".\" expected"; break;
			case 15: s = "\"@prefix\" expected"; break;
			case 16: s = "\"@base\" expected"; break;
			case 17: s = "\"base\" expected"; break;
			case 18: s = "\"prefix\" expected"; break;
			case 19: s = "\";\" expected"; break;
			case 20: s = "\",\" expected"; break;
			case 21: s = "\"a\" expected"; break;
			case 22: s = "\"[\" expected"; break;
			case 23: s = "\"]\" expected"; break;
			case 24: s = "\"(\" expected"; break;
			case 25: s = "\")\" expected"; break;
			case 26: s = "\"^^\" expected"; break;
			case 27: s = "\"true\" expected"; break;
			case 28: s = "\"false\" expected"; break;
			case 29: s = "??? expected"; break;
			case 30: s = "invalid Statement"; break;
			case 31: s = "invalid Directive"; break;
			case 32: s = "invalid Triples"; break;
			case 33: s = "invalid Subject"; break;
			case 34: s = "invalid Verb"; break;
			case 35: s = "invalid Object"; break;
			case 36: s = "invalid Iri"; break;
			case 37: s = "invalid Blanknode"; break;
			case 38: s = "invalid Numericliteral"; break;
			case 39: s = "invalid Booleanliteral"; break;
			case 40: s = "invalid String"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}