/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

grammar sparq11lTranslator;
options	{ 	 }
@parser::namespace { SparqlQuery }
@lexer::namespace  { SparqlQuery }
@header{
	using System;
	using System;
	using System.Linq;	  
	using SparqlQuery.SparqlClasses;
	using SparqlQuery.SparqlClasses.InlineValues;
	using SparqlQuery.SparqlClasses.GraphPattern.Triples.Node;
	using SparqlQuery.SparqlClasses.Update;
	using SparqlQuery.SparqlClasses.SparqlAggregateExpression;
	using SparqlQuery.SparqlClasses.Expressions;
	using SparqlQuery.SparqlClasses.GraphPattern;
	using SparqlQuery.SparqlClasses.GraphPattern.Triples;
	using SparqlQuery.SparqlClasses.SolutionModifier;
	using SparqlQuery.SparqlClasses.Query;  
	using SparqlQuery.SparqlClasses.Query.Result;  
    using SparqlQuery.SparqlClasses.GraphPattern.Triples.Path;
	using RDFCommon;				 
	using RDFCommon.OVns;

	
}


@members{		  	


public RdfQuery11Translator q;
public RdfQuery11Translator q1;
      
}
 query returns [SparqlQuery value] : 
 (prologue 	{q.prolog.StringRepresentationOfProlog=$prologue.text;}
 ( selectQuery {$value=$selectQuery.value; $value.ResultSet.ResultType= ResultType.Select; }
  | constructQuery { $value=$constructQuery.value; $value.ResultSet.ResultType= ResultType.Construct;}  
  | describeQuery { $value=$describeQuery.value; $value.ResultSet.ResultType= ResultType.Describe;}  
  | askQuery { $value=$askQuery.value; $value.ResultSet.ResultType= ResultType.Ask;}  )
valuesClause 	{ $value.SetValues($valuesClause.value);} )
| update {$value = $update.value;} ;
 prologue: ( baseDecl | prefixDecl )*;
 baseDecl : BASE IRIREF {q.prolog.SetBase($IRIREF.text);};
 prefixDecl : PREFIX PNAME_NS IRIREF
 {
	q.prolog.AddPrefix($PNAME_NS.text, $IRIREF.text);
 };
 selectQuery returns [SparqlQuery value]: {$value=new SparqlQuery(q);} selectClause datasetClause* whereClause solutionModifier {$solutionModifier.value.Add($selectClause.value);} {$value.Create($whereClause.value, $solutionModifier.value);};
 subSelect returns [SparqlSubSelect value] : selectClause whereClause solutionModifier {$solutionModifier.value.Add($selectClause.value);} valuesClause {$value=new SparqlSubSelect($whereClause.value, $solutionModifier.value, $valuesClause.value, q);};
 selectClause returns [SparqlSelect value] : SELECT {$value=new SparqlSelect(q);} ( DISTINCT {$value.IsDistinct=true;} | REDUCED {$value.IsReduced=true;} )? ( ( var {$value.Add($var.value);} | ( '(' expression AS var {$value.Add(q.CreateExpressionAsVariable($var.value, $expression.value));} ')' ) )+ | '*' {$value.IsAll();} );
 constructQuery returns [SparqlConstructQuery value] : CONSTRUCT {$value=new SparqlConstructQuery(q);} 
 ( constructTemplate datasetClause* whereClause solutionModifier { $value.Create($constructTemplate.value, $whereClause.value, $solutionModifier.value); }
 | datasetClause* WHERE '{'( triplesTemplate { $value.Create($triplesTemplate.value); } )? '}' solutionModifier { $value.Create($solutionModifier.value); } );
 describeQuery returns [SparqlDescribeQuery value] : DESCRIBE {$value=new SparqlDescribeQuery(q);} ( (varOrIri {$value.Add($varOrIri.value);})+ | '*' {$value.IsAll();} ) datasetClause* ( whereClause {$value.Create($whereClause.value);} )? solutionModifier {$value.Create($solutionModifier.value);};
 askQuery returns [SparqlQuery value] : ASK {$value=new SparqlQuery(q);}  datasetClause* whereClause solutionModifier {$value.Create($whereClause.value, $solutionModifier.value);};
datasetClause : FROM ( defaultGraphClause | namedGraphClause )	 ;
  defaultGraphClause : sourceSelector {q.ActiveGraphs.Add($sourceSelector.value);};
 namedGraphClause :NAMED sourceSelector {q.NamedGraphs.Add($sourceSelector.value);};
 sourceSelector returns [ObjectVariants value] : iri  {$value=$iri.value;};

 whereClause returns [ISparqlGraphPattern value] : WHERE? groupGraphPattern {$value=$groupGraphPattern.value; };
 solutionModifier returns [SparqlSolutionModifier value]: {$value=new SparqlSolutionModifier();} ( groupClause {$value.Add($groupClause.value);} )? (havingClause {$value.Add($havingClause.value, q);})? (orderClause {$value.Add($orderClause.value);})? (limitOffsetClauses {$value.Add($limitOffsetClauses.value);} )? ;
 groupClause returns [SparqlSolutionModifierGroup value] : GROUP BY {$value=new SparqlSolutionModifierGroup(q);} 
 ( groupCondition  { $value.Add($groupCondition.value);})+;
 groupCondition returns [SparqlGroupConstraint value] : builtInCall {$value=new SparqlGroupConstraint($builtInCall.value);}
  | functionCall   {$value=new SparqlGroupConstraint($functionCall.value);}
  | '(' expression {$value=new SparqlGroupConstraint($expression.value);} ( AS var  {$value=new SparqlGroupConstraint(q.CreateExpressionAsVariable($var.value, $expression.value));})? ')' 
  | var {$value=new SparqlGroupConstraint($var.value);};
 havingClause returns [SparqlSolutionModifierHaving value] : HAVING {$value=new SparqlSolutionModifierHaving();} (havingCondition {$value.Add($havingCondition.value);} )+;
 havingCondition returns [SparqlExpression value] : constraint { $value=$constraint.value;};
 orderClause returns [SparqlSolutionModifierOrder value]: ORDER BY {$value=new SparqlSolutionModifierOrder();} (orderCondition {$value.Add($orderCondition.value); } )+;
 orderCondition returns [SparqlOrderCondition value]: ( ( dir= ASC | dir= DESC ) brackettedExpression {$value = new SparqlOrderCondition($brackettedExpression.value, $dir.text, q);} )
| ( brackettedExpression {$value=new SparqlOrderCondition($brackettedExpression.value, q);}
 | builtInCall {$value=new SparqlOrderCondition($builtInCall.value,q);}
 | functionCall {$value=new SparqlOrderCondition($functionCall.value,q);} 
 | var {$value=new SparqlOrderCondition($var.value,q);}  );
 limitOffsetClauses returns [SparqlSolutionModifierLimit value] : {$value=new SparqlSolutionModifierLimit();} (limitClause {$value.CreateLimit($limitClause.value); } ( offsetClause {$value.CreateOffset($offsetClause.value); } )? | offsetClause {$value.CreateOffset($offsetClause.value); }  ( limitClause  {$value.CreateLimit($limitClause.value); } )?);
 limitClause returns [int value]: LIMIT integer {$value=$integer.value;};
 offsetClause returns [int value]: OFFSET integer {$value=$integer.value;};
 integer returns [int value] : INTEGER {$value=int.Parse($INTEGER.text);};
 valuesClause returns [ISparqlGraphPattern value] : ( VALUES dataBlock { $value=$dataBlock.value; } )?;

 update returns [SparqlUpdateQuery value]: prologue {q.prolog.StringRepresentationOfProlog=$prologue.text;} {$value=new SparqlUpdateQuery(q);} ( update1 {$value.Create($update1.value);} ( ';'r= update {$value.Add($r.value);} )? )?;
 update1 returns [ISparqlUpdate value]: load {$value=$load.value;}
 | clear {$value=$clear.value;}
 | drop {$value=$drop.value;}
 | add {$value=$add.value;}
 | move {$value=$move.value;}
 | copy {$value=$copy.value;}
 | create {$value=$create.value;}
 | insertData {$value=$insertData.value;}
 | deleteData {$value=$deleteData.value;}
 | deleteWhere {$value=$deleteWhere.value;}
 | modify {$value=$modify.value;};
 load returns [SparqlUpdateLoad value]: LOAD {$value=new SparqlUpdateLoad();} (SILENT {$value.Silent();} )? iri {$value.SetIri($iri.value);} ( INTO graphRef {$value.Into($graphRef.value);} )?;
 clear returns [SparqlUpdateClear value]: CLEAR {$value=new SparqlUpdateClear();} (SILENT{$value.Silent();} )? graphRefAll {$value.Graph =$graphRefAll.value;};
 drop returns [SparqlUpdateDrop value]: DROP {$value=new SparqlUpdateDrop();} (SILENT{$value.Silent();} )? graphRefAll {$value.Graph =$graphRefAll.value;};
 create returns [SparqlUpdateCreate value] : CREATE  {$value=new SparqlUpdateCreate();} (SILENT{$value.Silent();} )? graphRef {$value.Graph=$graphRef.value;};
 add returns [SparqlUpdateAdd value]: ADD {$value=new SparqlUpdateAdd();} (SILENT{$value.Silent();} )? g1 = graphOrDefault TO g2 = graphOrDefault {$value.From =$g1.value; $value.To =$g2.value;};
 move returns [SparqlUpdateMove value]: MOVE {$value=new SparqlUpdateMove();} (SILENT{$value.Silent();} )? g1 = graphOrDefault TO g2 = graphOrDefault {$value.From =$g1.value; $value.To =$g2.value;};
 copy returns [SparqlUpdateCopy value]: COPY {$value=new SparqlUpdateCopy();} (SILENT{$value.Silent();} )? g1 = graphOrDefault TO g2 = graphOrDefault {$value.From =$g1.value; $value.To =$g2.value;};
 insertData returns [SparqlUpdateInsertData value]: INSERT DATA quadData { $value=new SparqlUpdateInsertData($quadData.value);};
 deleteData returns [SparqlUpdateDeleteData value]: DELETE DATA quadData { $value=new SparqlUpdateDeleteData($quadData.value);};
 deleteWhere returns [SparqlUpdateModify value]: DELETE WHERE quadPattern {$value=new SparqlUpdateModify($quadPattern.value);};
 modify returns [SparqlUpdateModify value]: {$value=new SparqlUpdateModify(q);} 
 ( WITH iri { q.ActiveGraphs.Add($iri.value); $value.SetWith($iri.value);}  )? 
 ( deleteClause { $value.SetDelete($deleteClause.value); } 
	(insertClause {$value.SetInsert($insertClause.value);} )? 
| insertClause {$value.SetInsert($insertClause.value);} ) 
	({q.ActiveGraphs.Clear();} usingClause+)? 
WHERE groupGraphPattern { $value.SetWhere($groupGraphPattern.value); };
 deleteClause returns [SparqlQuadsPattern value]: DELETE quadPattern { $value=$quadPattern.value;};
 insertClause returns [SparqlQuadsPattern value]: INSERT quadPattern { $value=$quadPattern.value;};
 usingClause: USING ( iri {q.ActiveGraphs.Add($iri.value);} | NAMED iri  {q.NamedGraphs.Add($iri.value);} );
 graphOrDefault returns [string value] : DEFAULT | GRAPH? iriString {$value=$iriString.value; } ;
 graphRef returns [string value] : GRAPH iriString {$value=$iriString.value;};
 graphRefAll returns [UpdateGraph value]  : graphRef {$value=new UpdateGraph($graphRef.value); } 
 | g = DEFAULT  {$value=new UpdateGraph(SparqlGrpahRefTypeEnum.Default); }
 | g = NAMED  {$value=new UpdateGraph( SparqlGrpahRefTypeEnum.Named); }
 | g = ALL {$value=new UpdateGraph(SparqlGrpahRefTypeEnum.All); };
 quadPattern returns [SparqlQuadsPattern value]: '{' quads '}' {$value=$quads.value;};
 quadData returns [SparqlQuadsPattern value]: '{' quads '}'{$value=$quads.value;};
 quads returns [SparqlQuadsPattern value]:  {$value=new SparqlQuadsPattern();} (triplesTemplate {$value.AddRange($triplesTemplate.value);} )? ( quadsNotTriples {$value.Add($quadsNotTriples.value);} '.'? (triplesTemplate {$value.AddRange($triplesTemplate.value);} )? )*;
 quadsNotTriples returns [SparqlGraphGraph value] : GRAPH varOrIri {$value=new SparqlGraphGraph($varOrIri.value);} '{' ( triplesTemplate { $value.AddTriples($triplesTemplate.value);} )? '}';
 triplesTemplate returns [SparqlGraphPattern value] : triplesSameSubject {$value=$triplesSameSubject.value;} ( '.' ( tt= triplesTemplate {$value.AddRange($tt.value); } )? )?;

 groupGraphPattern  returns [ISparqlGraphPattern value] : '{' ( subSelect {$value=$subSelect.value;} | groupGraphPatternSub  {$value=$groupGraphPatternSub.value;} ) '}';
 groupGraphPatternSub  returns [SparqlGraphPattern value] : {$value=new SparqlGraphPattern();} 
	(triplesBlock  {$value.AddRange($triplesBlock.value); })? 
	( graphPatternNotTriples  {$value.Add($graphPatternNotTriples.value); } '.'? ( triplesBlock  {$value.AddRange($triplesBlock.value); } )? )*;
 triplesBlock  returns [SparqlGraphPattern value]: triplesSameSubjectPath  {$value=$triplesSameSubjectPath.value;} ( '.' (added=triplesBlock  {$value.AddRange($added.value);})? )?;
 graphPatternNotTriples  returns [ISparqlGraphPattern value]
 : groupOrUnionGraphPattern {$value=$groupOrUnionGraphPattern.value;}
  | optionalGraphPattern  {$value=$optionalGraphPattern.value;} 
  | minusGraphPattern  {$value=$minusGraphPattern.value;}
  | graphGraphPattern {$value=$graphGraphPattern.value;}
  | serviceGraphPattern  { $value=$serviceGraphPattern.value;}
  | filter  {$value=$filter.value;}
  | bind {$value=$bind.value;}
  | inlineData{$value=$inlineData.value;};
 optionalGraphPattern  returns [SparqlOptionalGraphPattern value] : OPTIONAL  groupGraphPattern  {$value=new SparqlOptionalGraphPattern($groupGraphPattern.value);};
 graphGraphPattern returns [ISparqlGraphPattern value]: GRAPH varOrIri{var temp=q.ActiveGraphs;  q.ActiveGraphs=q.SetNamedGraphOrVariable($varOrIri.value, q.NamedGraphs); } groupGraphPattern {$value=$groupGraphPattern.value; q.ActiveGraphs=temp;};
 serviceGraphPattern  returns [SparqlServicePattern value] : {$value=new SparqlServicePattern();} SERVICE (SILENT {$value.IsSilent();})? varOrIri groupGraphPattern  {$value.Create($varOrIri.value, $groupGraphPattern.text, q.prolog.StringRepresentationOfProlog, q);};
 bind returns [SparqlExpressionAsVariable value] : BIND '(' expression AS var ')' {$value=q.CreateExpressionAsVariable($var.value, $expression.value);};
 inlineData returns [ISparqlGraphPattern value] : VALUES dataBlock { $value=$dataBlock.value;};
 dataBlock returns [ISparqlGraphPattern value] : inlineDataOneVar {$value=$inlineDataOneVar.value;} | inlineDataFull {$value=$inlineDataFull.value;};
 inlineDataOneVar returns [SparqlInlineVariable value] : var { $value=new SparqlInlineVariable($var.value);} '{' (dataBlockValue { $value.Add($dataBlockValue.value);})* '}';
 inlineDataFull  returns [SparqlInline value] : {$value=new SparqlInline();} ( NIL | '(' (var {$value.AddVar($var.value);} )* ')' ) '{' ( '(' (dataBlockValue {$value.AddValue($dataBlockValue.value);})* {$value.NextListOfVarBindings();} ')' | NIL  )* '}';
 dataBlockValue returns [ObjectVariants value] : iri {$value=$iri.value;} | blankNode {$value=$blankNode.value;} |	rDFLiteral {$value=$rDFLiteral.value;} |	numericLiteral {$value=$numericLiteral.value;} |	booleanLiteral {$value=$booleanLiteral.value;} |	UNDEF {$value=new SparqlUnDefinedNode();};
 minusGraphPattern  returns [SparqlMinusGraphPattern value]: MINUS groupGraphPattern  {$value=new SparqlMinusGraphPattern($groupGraphPattern.value, q);};
 groupOrUnionGraphPattern returns [SparqlUnionGraphPattern value ] : groupGraphPattern  {$value = new SparqlUnionGraphPattern($groupGraphPattern.value);} ( UNION groupGraphPattern  {$value.Add($groupGraphPattern.value);} )*;
 filter returns [SparqlFilter value]: FILTER constraint {$value=new SparqlFilter($constraint.value);} ;
 constraint returns [SparqlExpression value]: brackettedExpression {$value=$brackettedExpression.value;}
 | builtInCall {$value=$builtInCall.value;}
 | functionCall {$value=$functionCall.value;};
 functionCall returns [SparqlFunctionCall value] : iriString argList { $value=new SparqlFunctionCall($iriString.value, $argList.value); };
 argList returns [SparqlArgs value] : NIL | {$value = new SparqlArgs();} '('  (DISTINCT { $value.IsDistinct(); } )? expression { $value.Add($expression.value); } ( ',' expression { $value.Add($expression.value); } )* ')';
 expressionList returns [List<SparqlExpression> value] : NIL | '(' expression {$value=new List<SparqlExpression>(){$expression.value};} ( ',' expression { $value.Add($expression.value);} )* ')';
 constructTemplate returns [SparqlGraphPattern value] : '{' {$value=new SparqlGraphPattern();} ( constructTriples {$value.AddRange($constructTriples.value);} )? '}';
 constructTriples returns [SparqlGraphPattern value] : triplesSameSubject {$value=$triplesSameSubject.value;} ( '.' ( c=constructTriples {$value.AddRange($c.value);} )? )?;

 triplesSameSubject returns [SparqlGraphPattern value ] : {$value=new SparqlGraphPattern();} 
 subjectNode propertyListNotEmpty [$value, $subjectNode.value]	
 | {$value=new SparqlGraphPattern();}	triplesNode [$value] (propertyListNotEmpty [$value, $triplesNode.value] )?;  
 //propertyList [SparqlGraphPattern triples] : (propertyListNotEmpty [$triples] {$value=$propertyListNotEmpty.value;})?;
 propertyListNotEmpty [SparqlGraphPattern triples, ObjectVariants subject] : verb objectList [$triples, $subject, $verb.value] ( ';' ( verb objectList [$triples, $subject, $verb.value])? )*;
 verb returns [ObjectVariants value]
 : varOrIri  {$value = (ObjectVariants)$varOrIri.value;}
 | 'a' {$value = q.Store.NodeGenerator.SpecialTypes.type;};
 objectList [SparqlGraphPattern triples, ObjectVariants subject, ObjectVariants predicate] : object [$triples, $subject, $predicate] ( ',' object [$triples, $subject, $predicate] )*;
 object [SparqlGraphPattern triples, ObjectVariants subject, ObjectVariants predicate] : graphNode [$triples] { $triples.Add(new SparqlTriple($subject, $predicate, $graphNode.value, q)); };
 graphNode [SparqlGraphPattern triples] returns [ObjectVariants value] 	
 :  varOrTerm { $value=$varOrTerm.value; } 
 |	triplesNode [$triples] { $value=$triplesNode.value; };	   
 collection [SparqlGraphPattern triples] returns [SparqlRdfCollection value] : '(' {$value=new SparqlRdfCollection();} (graphNode [$triples] {  $value.nodes.Add($graphNode.value);})+ ')';
 triplesNode [SparqlGraphPattern triples] returns [ObjectVariants value] : collection [$triples] { $value = $collection.value.GetNode((Subject, Predicate,Object)=> $triples.Add(new SparqlTriple(Subject, Predicate,Object,q)), q.Store.NodeGenerator); } 
 |	{ $value=q.CreateBlankNode();} blankNodePropertyList [$triples,$value] ;
 blankNodePropertyList [SparqlGraphPattern triples, ObjectVariants subject] : '['propertyListNotEmpty [$triples, $subject] ']';

 subjectNode returns [ObjectVariants value] : varOrTerm { $value=$varOrTerm.value; }  ;

 triplesSameSubjectPath   returns [SparqlGraphPattern value] : {$value=new SparqlGraphPattern();} subjectNode propertyListPathNotEmpty [$value, $subjectNode.value] 
  |	{$value=new SparqlGraphPattern();} triplesNodePath [$value] (propertyListPathNotEmpty [$value,$triplesNodePath.value])?;
// propertyListPath [SparqlGraphPattern triples] returns [PropertyWithObjects value] : (propertyListPathNotEmpty [$triples] {$value=$propertyListPathNotEmpty.value;} )?;
 propertyListPathNotEmpty [SparqlGraphPattern triples, ObjectVariants subject] :  
  ( path objectListPath [$triples, $subject, $path.value]  | var objectListPath [$triples,$subject, $var.value] )  
  ( ';' ( ( path objectListPath [$triples, $subject, $path.value]  | var objectListPath [$triples, $subject, $var.value]  ) )? )*;
 //verbPath returns [INode value] :  path { $value = $path.value.GetNode(); } ;
 //verbSimple returns [INode value] : var  { $value = $var.value;};
 objectListPath [SparqlGraphPattern triples, ObjectVariants subject, ObjectVariants predicate] : objectPath [$triples, $subject, $predicate] ( ',' objectPath [$triples, $subject, $predicate] )*;
 objectPath [SparqlGraphPattern triples, ObjectVariants subject, ObjectVariants predicate] : graphNodePath [$triples] {
 //if(predicate is VariableNode)
 $triples.CreateTriple($subject, $predicate, $graphNodePath.value, q);
   };
 path returns [SparqlPathTranslator value] : pathSequence {$value=$pathSequence.value;} ( '|' pathSequence {$value=$value.AddAlt($pathSequence.value);} )*;
 pathSequence returns [SparqlPathTranslator value ] : pathEltOrInverse {$value = $pathEltOrInverse.value;} ( '/' pathEltOrInverse {$value=$value.AddSeq($pathEltOrInverse.value);} )*;
 pathEltOrInverse returns [SparqlPathTranslator value] : pathElt {$value=$pathElt.value;} | '^' pathElt {$value=$pathElt.value.Inverse();} ;
 pathElt returns [SparqlPathTranslator value] : pathPrimary {$value=$pathPrimary.value;}  ( '?' {$value=new SparqlPathMaybeOne($value);} | '*' {$value=new SparqlPathZeroOrMany($value);} | '+' {$value=new SparqlPathOneOrMany($value);})?;
 pathPrimary returns [SparqlPathTranslator value] : iri { $value=new SparqlPathTranslator($iri.value); } 
 | 'a' {$value=new SparqlPathTranslator(q.Store.NodeGenerator.SpecialTypes.type);}
  | '!' pathNegatedPropertySet {$value=$pathNegatedPropertySet.value;} | '(' path { $value=$path.value; } ')';
 pathNegatedPropertySet  returns [SparqlPathNotTranslator value] : pathOneInPropertySet { $value=new SparqlPathNotTranslator($pathOneInPropertySet.value); } | '(' ( pathOneInPropertySet { $value=new SparqlPathNotTranslator($pathOneInPropertySet.value); } ( '|' pathOneInPropertySet { $value.alt.Add($pathOneInPropertySet.value); } )* )? ')';
 pathOneInPropertySet returns [SparqlPathTranslator value] : iri {$value = new SparqlPathTranslator($iri.value);} 
 | 'a' {$value = new SparqlPathTranslator(q.Store.NodeGenerator.SpecialTypes.type);} 
 | '^' ( iri {$value=new SparqlPathTranslator($iri.value).Inverse();} 
		| 'a' {$value=new SparqlPathTranslator(q.Store.NodeGenerator.SpecialTypes.type).Inverse();}  );

 triplesNodePath [SparqlGraphPattern triples] returns [ObjectVariants  value] : collectionPath [$triples] { $value = $collectionPath.value.GetNode((s, p, o)=> _localctx.triples.Add(new SparqlTriple(s, p, o, q)), q.Store.NodeGenerator); } | { $value=q.CreateBlankNode();}	blankNodePropertyListPath [$triples, $value];
 blankNodePropertyListPath  [SparqlGraphPattern triples, ObjectVariants subject]  : '[' propertyListPathNotEmpty [$triples, $subject] ']';
 collectionPath [SparqlGraphPattern triples] returns [SparqlRdfCollection value] : '(' {$value=new SparqlRdfCollection();} (graphNodePath [$triples] {  $value.nodes.Add($graphNodePath.value);})+ ')';
 graphNodePath [SparqlGraphPattern triples] returns [ObjectVariants value] : varOrTerm { $value = $varOrTerm.value; } | triplesNodePath [$triples] { $value=$triplesNodePath.value; };
 varOrTerm returns [ObjectVariants value] : var { $value=$var.value; } | graphTerm {  $value=$graphTerm.value; };
 varOrIri returns [ObjectVariants value] : var {$value=$var.value;} | iri {$value=$iri.value;};
 var returns [VariableNode value]: varString {$value =q.GetVariable($varString.text);};
 varString : VAR1 | VAR2 ;
 graphTerm returns [ObjectVariants value]
 : iri {$value=$iri.value;}
 |	rDFLiteral {$value=$rDFLiteral.value;}
 |	numericLiteral {$value=$numericLiteral.value;}
 |	booleanLiteral {$value=$booleanLiteral.value;}
 |	blankNode {$value=$blankNode.value;}
 |	NIL {$value = q.Store.NodeGenerator.SpecialTypes.nil;};
 //expression returns [SparqlExpression value] : conditionalOrExpression {$value=};
 //conditionalOrExpression returns [SparqlExpression value] : conditionalAndExpression { $value=$conditionalAndExpression.value; } ( '||' conditionalAndExpression {$value.or.Add($conditionalAndExpression.value);} )*;
 expression returns [SparqlExpression value] : conditionalAndExpression { $value=$conditionalAndExpression.value; } ( '||' r= conditionalAndExpression {$value=new SparqlOrExpression($value, $r.value);} )*;

 conditionalAndExpression returns [SparqlExpression value] :  relationalExpression { $value=$relationalExpression.value; } ( '&&' r= relationalExpression {$value=new SparqlAndExpression($value, $r.value);} )*;
 relationalExpression returns [SparqlExpression value] :  numericExpression { $value=$numericExpression.value; }  
 ( '=' r=numericExpression {$value=new SparqlEqualsExpression($value, $r.value, q.Store.NodeGenerator);}
  | '!=' r=numericExpression {$value= new SparqlNotEqualsExpression($value, $r.value, q.Store.NodeGenerator);}
  | '<' r=numericExpression {$value=SparqlExpression.Smaller($value, $r.value);}
  | '>' r=numericExpression {$value=SparqlExpression.Greather($value, $r.value);}
  | '<=' r=numericExpression {$value=SparqlExpression.SmallerOrEquals($value, $r.value);}
  | '>=' r=numericExpression {$value=SparqlExpression.GreatherOrEquals($value, $r.value);}
  | IN expressionList {$value=$value.InCollection($expressionList.value);}
  | NOT IN expressionList {$value=$value.NotInCollection($expressionList.value);} )?;
 numericExpression returns [SparqlExpression value] : multiplicativeExpression { $value=$multiplicativeExpression.value; } 
 ( '+' r= multiplicativeExpression { $value += $r.value; }
 | '-'r= multiplicativeExpression { $value -= $r.value; }
 | { SparqlExpression multExp;} ( numericLiteralPositive    {multExp = new SparqlLiteralExpression($numericLiteralPositive.value); } | numericLiteralNegative  { multExp = new SparqlLiteralExpression($numericLiteralNegative.value); } ) 
	( ( '*' unaryExpression {multExp *= $unaryExpression.value; }	) 
	| ( '/' unaryExpression { multExp /= $unaryExpression.value; }) )*
	{ $value += multExp; } )*;
 multiplicativeExpression returns [SparqlExpression value] : unaryExpression { $value=$unaryExpression.value; } 
 ( '*'r= unaryExpression {$value=$value*$r.value;} | '/'r= unaryExpression {$value=$value/$r.value;}  )*;
 unaryExpression returns [SparqlExpression value] :   '!' primaryExpression    { $value=!$primaryExpression.value; }
|	'+' primaryExpression 	{ $value=$primaryExpression.value; }
|	'-' primaryExpression 	 { $value=-$primaryExpression.value; }
|	primaryExpression { $value=$primaryExpression.value; };
 primaryExpression returns [SparqlExpression value] : brackettedExpression 	 {$value=$brackettedExpression.value;} 
 | builtInCall {$value=$builtInCall.value;} 
 | (iriString {$value=new SparqlIriExpression($iriString.value, q.Store.NodeGenerator);} ( argList {$value=new SparqlFunctionCall($iriString.value,  $argList.value);} )?) 
 | rDFLiteral {$value=new SparqlLiteralExpression($rDFLiteral.value);} 
 | numericLiteral {$value=new SparqlNumLiteralExpression($numericLiteral.value);} 
 | booleanLiteral {$value=new SparqlBoolLiteralExpression($booleanLiteral.value);} 
 | var {$value=new SparqlVarExpression($var.value);};
 brackettedExpression returns [SparqlExpression value] : '(' expression ')' { $value=$expression.value; };
 builtInCall returns [SparqlExpression value] :   aggregate { $value=$aggregate.value; }
|	STR '(' expression ')'  { $value=new SparqlToString($expression.value); }
|	LANG '(' expression ')' { $value=new SparqlLang($expression.value); }
|	LANGMATCHES '(' lit = expression ',' lang = expression ')' { $value=new SparqlLangMatches($lit.value, $lang.value); }
|	DATATYPE '(' expression ')' 	 { $value=new SparqlDataType($expression.value); }
|	BOUND  '(' var ')'    { $value=new SparqlBound($var.value); }
|	IRI '(' expression ')' { $value=new SparqlUri($expression.value, q); }
|	URI '(' expression ')'  { $value=new SparqlUri($expression.value,q); }
|	BNODE ( '(' expression ')' { $value=new SparqlBnode($expression.value,q); } | NIL { $value=new SparqlBnode(q); }) 
|	RAND NIL 	{ $value=new SparqlRand(); }
|	ABS '(' expression ')' { $value=new SparqlAbs($expression.value); }
|	CEIL '(' expression ')' { $value=new SparqlCeil($expression.value); }
|	FLOOR '(' expression ')' { $value=new SparqlFloor($expression.value); }
|	ROUND '(' expression ')' 	{ $value=new SparqlRound($expression.value); }
|	CONCAT expressionList   { $value=new SparqlConcat($expressionList.value,q.Store.NodeGenerator); }
|	substringExpression  { $value=$substringExpression.value; }
|	STRLEN '(' expression ')'  { $value=new SparqlStrLength($expression.value); }
|	strReplaceExpression { $value=$strReplaceExpression.value; }
|	UCASE '(' expression ')' 	{ $value=new SparqlUcase($expression.value); }
|	LCASE '(' expression ')' 	 { $value=new SparqlLCase($expression.value); }
|	ENCODE_FOR_URI '(' expression ')' { $value=new SparqlEncodeForUri($expression.value, q); }
|	CONTAINS '(' lit = expression ',' pattern = expression ')'   { $value=new SparqlContains($lit.value, $pattern.value); }
|	STRSTARTS '(' lit = expression ',' pattern = expression ')' { $value=new SparqlStrStarts($lit.value, $pattern.value); } 
|	STRENDS '(' lit = expression ',' pattern = expression ')' { $value=new SparqlStrEnds($lit.value, $pattern.value); } 
|	STRBEFORE '('  lit = expression ',' pattern =  expression ')' { $value=new SparqlStrBefore($lit.value, $pattern.value); } 
|	STRAFTER '('  lit = expression ',' pattern = expression ')' { $value=new SparqlStrAfter($lit.value, $pattern.value); } 
|	YEAR '(' expression ')'  { $value=new SparqlYear($expression.value); }
|	MONTH '(' expression ')' { $value=new SparqlMonth($expression.value); }
|	DAY '(' expression ')'{ $value=new SparqlDay($expression.value); } 
|	HOURS '(' expression ')' { $value=new SparqlHours($expression.value); }
|	MINUTES '(' expression ')'{ $value=new SparqlMinutes($expression.value); } 
|	SECONDS '(' expression ')'  { $value=new SparqlSeconds($expression.value); }
|	TIMEZONE '(' expression ')' { $value=new SparqlTimeZone($expression.value); }
|	TZ '(' expression ')' { $value=new SparqlTz($expression.value); }
|	NOW NIL  { $value=new SparqlNow(); }
|	UUID NIL 	{ $value=new SparqlUuid(); }
|	STRUUID NIL 	 { $value=new SparqlStrUuid(); }
|	MD5 '(' expression ')' { $value=new SparqlMD5($expression.value); }
|	SHA1 '(' expression ')' { $value=new SparqlSHA1($expression.value); }
|	SHA256 '(' expression ')'  { $value=new SparqlSHA256($expression.value); }
|	SHA384 '(' expression ')' { $value=new SparqlSHA384($expression.value); }
|	SHA512 '(' expression ')' { $value=new SparqlSHA512($expression.value); }
|	COALESCE expressionList 	 { $value=new SparqlCoalesce($expressionList.value); }
|	IF '(' condit=expression ',' ifTrue=expression ',' ifFalse= expression ')'  { $value=new SparqlIf($condit.value, $ifTrue.value, $ifFalse.value); } 
|	STRLANG '(' lit=expression ',' lang=expression ')'   { $value=new SparqlStringLang($lit.value, $lang.value); } 
|	STRDT '(' lit= expression ',' type = expression ')'  { $value=new SparqlStrDataType($lit.value, $type.value,q.Store.NodeGenerator); } 
|	SAMETERM '(' t1=expression ',' t2=expression ')'  { $value=new SparqlSameTerm($t1.value, $t2.value); } 
|	ISIRI '(' expression ')' { $value=new SparqlIsIri($expression.value); }
|	ISURI '(' expression ')' 	{ $value=new SparqlIsIri($expression.value); }
|	ISBLANK '(' expression ')'  { $value=new SparqlIsBlank($expression.value); }
|	ISLITERAL '(' expression ')' { $value=new SparqlIsLiteral($expression.value); }
|	ISNUMERIC '(' expression ')' 	{ $value=new SparqlIsNum($expression.value); }
|	regexExpression { $value=$regexExpression.value; }
|	existsFunc 	 { $value=$existsFunc.value; }
|	notExistsFunc { $value=$notExistsFunc.value; };
 regexExpression returns [SparqlRegexExpression value] : REGEX  {$value=new SparqlRegexExpression();}  '(' v = expression {$value.SetVariableExpression($v.value, q.Store);} ',' regex = expression ( ',' parameters = expression {$value.SetParameters($parameters.value);} )? ')' {$value.SetRegex($regex.value);};
 substringExpression returns [SparqlSubstringExpression value] :  SUBSTR {$value=new SparqlSubstringExpression();} '(' lit= expression {$value.SetString($lit.value);} ',' startExp = expression {$value.SetStartPosition($startExp.value);} ( ',' length= expression {$value.SetLength($length.value);} )? ')';
 strReplaceExpression returns [SparqlReplaceStrExpression value] : REPLACE {$value=new SparqlReplaceStrExpression();} '(' lit = expression  {$value.SetString($lit.value);} ',' pattern = expression  {$value.SetPattern($lit.value);}',' replacement = expression {$value.SetReplacement($replacement.value);} ( ',' parameters= expression {$value.SetParameters($parameters.value);} )? {$value.Create();} ')';
 existsFunc returns [SparqlExistsExpression value] : EXISTS groupGraphPattern {$value=new SparqlExistsExpression($groupGraphPattern.value);};
 notExistsFunc  returns [SparqlNotExistsExpression value] : NOT EXISTS groupGraphPattern  {$value=new SparqlNotExistsExpression($groupGraphPattern.value);};
 aggregate returns [SparqlAggregateExpression value] :   
 COUNT {$value=new SparqlCountExpression();} '(' (DISTINCT {$value.IsDistinct=true;})? ('*' {$value.IsAll();} | expression {$value.Expression=$expression.value;} ) ')' 
| SUM {$value=new SparqlSumExpression();}  '(' (DISTINCT{$value.IsDistinct=true;})? expression {$value.Expression=$expression.value;} ')' 
| MIN  {$value=new SparqlMinExpression();}  '(' (DISTINCT{$value.IsDistinct=true;})? expression {$value.Expression=$expression.value;} ')' 
| MAX  {$value=new SparqlMaxExpression();}  '(' (DISTINCT{$value.IsDistinct=true;})? expression {$value.Expression=$expression.value;} ')' 
| AVG  {$value=new SparqlAvgExpression();}  '(' (DISTINCT{$value.IsDistinct=true;})? expression {$value.Expression=$expression.value;} ')' 
| SAMPLE  {$value=new SparqlSampleExpression();}  '(' (DISTINCT{$value.IsDistinct=true;})? expression {$value.Expression=$expression.value;}')' 
| GROUP_CONCAT  {$value=new SparqlGroupConcatExpression();}  '(' (DISTINCT{$value.IsDistinct=true;})? expression {$value.Expression=$expression.value;} ( ';' SEPARATOR '=' string {$value.Separator=$string.text;} )? ')';
 //iriOrFunction returns [INode ] : iri argList?;
 rDFLiteral returns [ObjectVariants value] : string { $value=new OV_string($string.text.Trim('"', '\'')); }  
 | (string LANGTAG { $value=new OV_langstring($string.text.Trim('"', '\''), $LANGTAG.text.Substring(1,2));} )
 | ( string '^^' iriString { $value=q.Store.NodeGenerator.CreateLiteralNode($string.text, $iriString.value);} ) ;
 numericLiteral returns [ObjectVariants value] : numericLiteralUnsigned {$value=$numericLiteralUnsigned.value;}
 | numericLiteralPositive  {$value=$numericLiteralPositive.value;}
 | numericLiteralNegative {$value=$numericLiteralNegative.value;};
 numericLiteralUnsigned returns [ObjectVariants value] 
 : INTEGER { $value=new OV_int(int.Parse($INTEGER.text)); } 
 |	DECIMAL { $value=new OV_decimal(decimal.Parse($DECIMAL.text.Replace(".", ","))); }
 |	DOUBLE { $value=new OV_double(double.Parse($DOUBLE.text.Replace(".", ","))); };
 numericLiteralPositive returns [ObjectVariants value] 
 : INTEGER_POSITIVE { $value=new OV_int(int.Parse($INTEGER_POSITIVE.text)); }
 |	DECIMAL_POSITIVE  { $value=new OV_decimal(decimal.Parse($DECIMAL_POSITIVE.text.Replace(".", ","))); }
 |	DOUBLE_POSITIVE { $value=new OV_double(double.Parse($DOUBLE_POSITIVE.text.Replace(".", ","))); };
 numericLiteralNegative returns [ObjectVariants value] 
 : INTEGER_NEGATIVE { $value=new OV_int(int.Parse($INTEGER_NEGATIVE.text)); }
 |	DECIMAL_NEGATIVE { $value=new OV_decimal(decimal.Parse($DECIMAL_NEGATIVE.text.Replace(".", ","))); }
 |	DOUBLE_NEGATIVE{ $value=new OV_double(double.Parse($DOUBLE_NEGATIVE.text.Replace(".", ","))); };
  booleanLiteral returns [ObjectVariants value]
 :  boolean { $value=new OV_bool($boolean.value); } ;
 boolean returns [bool value]
 :  TRUE { $value=true; } 
 |	FALSE { $value=false; } ;
 string : STRING_LITERAL1 | STRING_LITERAL2 | STRING_LITERAL_LONG1 | STRING_LITERAL_LONG2;
 iri returns [ObjectVariants value] : iriString { $value=new OV_iri($iriString.value);};
 iriString returns [string value] : IRIREF {$value=$IRIREF.text.Substring(1, $IRIREF.text.Length-2);} 
 |	PNAME_LN {$value=q.prolog.GetUriFromPrefixed($PNAME_LN.text); } 
 | PNAME_NS {$value=q.prolog.GetUriFromPrefixedNamespace($PNAME_NS.text); };
 blankNode returns [ObjectVariants value] : BLANK_NODE_LABEL { $value = q.CreateBlankNode($BLANK_NODE_LABEL.text); } |	ANON {  $value = q.CreateBlankNode(); };

ADD : [Aa][Dd][Dd];
ALL : [Aa][Ll][Ll];
AS : [Aa][Ss];
ASC : [Aa][Ss][Cc];
ASK : [Aa][Ss][Kk];
BIND : [Bb][Ii][Nn][Dd];
BY : [Bb][Yy];
CREATE : [Cc][Rr][Ee][Aa][Tt][Ee];
CONSTRUCT : [Cc][Oo][Nn][Ss][Tt][Rr][Uu][Cc][Tt];
COPY : [Cc][Oo][Pp][Yy];
CLEAR : [Cc][Ll][Ee][Aa][Rr];
DATA : [Dd][Aa][Tt][Aa];
DROP : [Dd][Rr][Oo][Pp];
DESCRIBE : [Dd][Ee][Ss][Cc][Rr][Ii][Bb][Ee];
DELETE : [Dd][Ee][Ll][Ee][Tt][Ee];
DESC : [Dd][Ee][Ss][Cc];
DEFAULT : [Dd][Ee][Ff][Aa][Uu][Ll][Tt];
SELECT : [Ss][Ee][Ll][Ee][Cc][Tt];
DISTINCT : [Dd][Ii][Ss][Tt][Ii][Nn][Cc][Tt];
FROM : [Ff][Rr][Oo][Mm];
FILTER :[Ff][Ii][Ll][Tt][Ee][Rr];
GRAPH : [Gg][Rr][Aa][Pp][Hh];
HAVING : [Hh][Aa][Vv][Ii][Nn][Gg];
IN : [Ii][Nn];
INSERT : [Ii][Nn][Ss][Ee][Rr][Tt];
INTO : [Ii][Nn][Tt][Oo];
GROUP : [Gg][Rr][Oo][Uu][Pp];
LOAD : [Ll][Oo][Aa][Dd];
LIMIT : [Ll][Ii][Mm][Ii][Tt];
MINUS : [Mm][Ii][Nn][Uu][Ss];
MOVE : [Mm][Oo][Vv][Ee];
NAMED : [Nn][Aa][Mm][Ee][Dd];
NOT : [Nn][Oo][Tt];
OFFSET : [Oo][Ff][Ff][Ss][Ee][Tt];
ORDER : [Oo][Rr][Dd][Ee][Rr];
OPTIONAL : [Oo][Pp][Tt][Ii][Oo][Nn][Aa][Ll];
PREFIX : [Pp][Rr][Ee][Ff][Ii][Xx];
REDUCED : [Rr][Ee][Dd][Uu][Cc][Ee][Dd];
SILENT : [Ss][Ii][Ll][Ee][Nn][Tt];
SERVICE : [Ss][Ee][Rr][Vv][Ii][Cc][Ee];
TO : [Tt][Oo];
UNDEF : [Uu][Nn][Dd][Ee][Ff];
UNION : [Uu][Nn][Ii][Oo][Nn];
VALUES : [Vv][Aa][Ll][Uu][Ee][Ss];
WITH : [Ww][Ii][Tt][Hh];
WHERE : [Ww][Hh][Ee][Rr][Ee];
STR : [Ss][Tt][Rr];
LANG : [Ll][Aa][Nn][Gg];
LANGMATCHES : [Ll][Aa][Nn][Gg][Mm][Aa][Tt][Cc][Hh][Ee][Ss];
DATATYPE : [Dd][Aa][Tt][Aa][Tt][Yy][Pp][Ee];
BOUND : [Bb][Oo][Uu][Nn][Dd];
IRI : [Ii][Rr][Ii];
URI : [Uu][Rr][Ii];
BNODE : [Bb][Nn][Oo][Dd][Ee];
RAND : [Rr][Aa][Nn][Dd];
ABS : [Aa][Bb][Ss];
CEIL : [Cc][Ee][Ii][Ll];
FLOOR : [Ff][Ll][Oo][Oo][Rr];
ROUND : [Rr][Oo][Uu][Nn][Dd];
CONCAT : [Cc][Oo][Nn][Cc][Aa][Tt];
STRLEN : [Ss][Tt][Rr][Ll][Ee][Nn];
UCASE : [Uu][Cc][Aa][Ss][Ee];
LCASE : [Ll][Cc][Aa][Ss][Ee];
ENCODE_FOR_URI : [Ee][Nn][Cc][Oo][Dd][Ee][__][Ff][Oo][Rr][__][Uu][Rr][Ii];
CONTAINS : [Cc][Oo][Nn][Tt][Aa][Ii][Nn][Ss];
STRSTARTS : [Ss][Tt][Rr][Ss][Tt][Aa][Rr][Tt][Ss];
STRENDS : [Ss][Tt][Rr][Ee][Nn][Dd][Ss];
STRBEFORE : [Ss][Tt][Rr][Bb][Ee][Ff][Oo][Rr][Ee];
STRAFTER : [Ss][Tt][Rr][Aa][Ff][Tt][Ee][Rr];
YEAR : [Yy][Ee][Aa][Rr];
MONTH : [Mm][Oo][Nn][Tt][Hh];
DAY : [Dd][Aa][Yy];
HOURS : [Hh][Oo][Uu][Rr][Ss];
MINUTES : [Mm][Ii][Nn][Uu][Tt][Ee][Ss];
SECONDS : [Ss][Ee][Cc][Oo][Nn][Dd][Ss];
TIMEZONE : [Tt][Ii][Mm][Ee][Zz][Oo][Nn][Ee];
TZ : [Tt][Zz];
NOW : [Nn][Oo][Ww];
UUID : [Uu][Uu][Ii][Dd];
STRUUID : [Ss][Tt][Rr][Uu][Uu][Ii][Dd];
MD5 : [Mm][Dd][55];
SHA1 : [Ss][Hh][Aa][11];
SHA256 : [Ss][Hh][Aa][22][55][66];
SHA384 : [Ss][Hh][Aa][33][88][44];
SHA512 : [Ss][Hh][Aa][55][11][22];
COALESCE : [Cc][Oo][Aa][Ll][Ee][Ss][Cc][Ee];
IF : [Ii][Ff];
STRLANG : [Ss][Tt][Rr][Ll][Aa][Nn][Gg];
STRDT : [Ss][Tt][Rr][Dd][Tt];
SAMETERM : [Ss][Aa][Mm][Ee][Tt][Ee][Rr][Mm];
ISIRI : [Ii][Ss][Ii][Rr][Ii];
ISURI : [Ii][Ss][Uu][Rr][Ii];
ISBLANK : [Ii][Ss][Bb][Ll][Aa][Nn][Kk];
ISLITERAL : [Ii][Ss][Ll][Ii][Tt][Ee][Rr][Aa][Ll];
ISNUMERIC : [Ii][Ss][Nn][Uu][Mm][Ee][Rr][Ii][Cc];
REGEX : [Rr][Ee][Gg][Ee][Xx];
SUBSTR : [Ss][Uu][Bb][Ss][Tt][Rr];
REPLACE : [Rr][Ee][Pp][Ll][Aa][Cc][Ee];
EXISTS : [Ee][Xx][Ii][Ss][Tt][Ss];
COUNT : [Cc][Oo][Uu][Nn][Tt];
SUM : [Ss][Uu][Mm];
MIN : [Mm][Ii][Nn];
MAX : [Mm][Aa][Xx];
AVG : [Aa][Vv][Gg];
SAMPLE : [Ss][Aa][Mm][Pp][Ll][Ee];
GROUP_CONCAT : [Gg][Rr][Oo][Uu][Pp]'_'[Cc][Oo][Nn][Cc][Aa][Tt];
TRUE : [Tt][Rr][Uu][Ee];
FALSE : [Ff][Aa][Ll][Ss][Ee];
IRIREF	: '<'([a-zA-Zà-ÿÀ-ß0-9:/\\#.%-@_~])*'>';
//IRIREF : '<' ([^<>"{}|^`\]-[\x00-\x20])* '>';
PNAME_LN : PNAME_NS PN_LOCAL;
PNAME_NS : PN_PREFIX? ':';
BLANK_NODE_LABEL : '_:' ( PN_CHARS_U | [0-9] ) ((PN_CHARS|'.')* PN_CHARS)?;
VAR1 : '?' VARNAME;
VAR2 : '$' VARNAME;
LANGTAG : '@' [a-zA-Z]+ ('-' [a-zA-Z0-9]+)*;
 INTEGER : [0-9]+;
DECIMAL : [0-9]* '.' [0-9]+;
DOUBLE : [0-9]+ '.' [0-9]* EXPONENT | '.' ([0-9])+ EXPONENT | ([0-9])+ EXPONENT;
 INTEGER_POSITIVE : '+' INTEGER;
DECIMAL_POSITIVE : '+' DECIMAL;
DOUBLE_POSITIVE : '+' DOUBLE;
 INTEGER_NEGATIVE : '-' INTEGER;
DECIMAL_NEGATIVE : '-' DECIMAL;
DOUBLE_NEGATIVE : '-' DOUBLE;
EXPONENT : [eE] [+-]? [0-9]+;																
STRING_LITERAL1 : '\''(~(['\\\n\r]) |  ECHAR  )*'\''; /* #x27=' #x5C=\ #xA=new line #xD=carriage return */ 
STRING_LITERAL2 : '"' ( (~(["\\\n\r])) | ECHAR )* '"';  /* #x22=" #x5C=\ #xA=new line #xD=carriage return */
STRING_LITERAL_LONG1	 :'\'\'\'' ( ( '\'' | '\'\'\'' )? ( ~[\'\\] | ECHAR ) )* '\'\'\'' ;
STRING_LITERAL_LONG2	 :'"""' ( ( '"' | '""' )? ( ~[\"\\] | ECHAR ) )* '"""'  ;
ECHAR : '\\' [tbnrf\"\'];
NIL : BracketOpen BracketClose;
BracketOpen : '(';
BracketClose : ')';
WS : (' ' | '\t' | '\n' | '\r')+  -> channel(HIDDEN);//->skip	;
ANON : SquareBracketOpen SquareBracketClose;
SquareBracketOpen :  '[';
SquareBracketClose :  ']';
PN_CHARS_BASE : [A-Z] | [a-z] | [\x00C0-\x00D6] | [\x00D8-\x00F6] | [\x00F8-\x02FF] | [\x0370-\x037D] | [\x037F-\x1FFF] | [\x200C-\x200D] | [\x2070-\xw218F] | [\x2C00- \x2FEF] | [\x3001-\xD7FF] | [\xF900-\xFDCF] | [\xFDF0-\xFFFD] | [\x10000-\xEFFFF];
PN_CHARS_U : PN_CHARS_BASE | '_';
VARNAME : ( PN_CHARS_U | [0-9] ) ( PN_CHARS_U | [0-9] | [\x00B7] | [\x0300-\x036F] | [\x203F-\x2040] )*;
PN_CHARS : PN_CHARS_U | '-' | [0-9] | [\x00B7] | [\x0300-\x036F] | [\x203F-\x2040];
PN_PREFIX : PN_CHARS_BASE ((PN_CHARS|'.')* PN_CHARS)?;
PN_LOCAL : (PN_CHARS_U | ':' | [0-9] | PLX ) ((PN_CHARS | '.' | ':' | PLX)* (PN_CHARS | ':' | PLX) )?;
PLX : PERCENT | PN_LOCAL_ESC;
PERCENT : '%' HEX HEX;
HEX : [0-9] | [A-F] | [a-f];
PN_LOCAL_ESC : '\\' ( '_' | '~' | '.' | '-' | '!' | '$' | '&' | '\'' | '(' | ')' | '*' | '+' | ',' | ';' | '=' | '/' | '?' | '#' | '@' | '%' );
LineComment
    :   '#' ~('\n'|'\r')* //NEWLINE
       ->skip;