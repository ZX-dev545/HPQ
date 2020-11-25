using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Diagnostics;

namespace GCalc
{
    partial class Window
    {
        /// <summary>
        /// Interface used to classify an object as capable of outputting a numeric value.
        /// </summary>
        protected interface IValuable
        {
            Tensor Value(Dictionary<string, IVariable> vars);
        }
        /// <summary>
        /// Interface used to classify an object as a variable 
        /// </summary>
        protected interface IVariable : IFormulaic
        {
            string name { get; set; }
            /// <summary>
            /// Whether or not this variable is unknown (is it the output of IValuable.Value(Dictionary<string, IVariable> vars)?)
            /// </summary>
            bool known { get; }
        }
        /// <summary>
        /// interface used to classify an object as a container of IValuables
        /// </summary>
        protected interface IContainer
        {
            List<IFormulaic> parts { get; set; }
            void Add(IFormulaic part, bool new_equals_sign_flag = false);
            /// <summary>
            /// Throw-Relation, meant to "throw" the value of its ultimate IContainer Equation.Relation, after catching it through the IFormulaic 
            /// interface with IFormulaic.CRelation.
            /// </summary>
            relpair TRelation { get; }
            IFormulaic this[int i, bool n = true] { get; set; }
        }
        /// <summary>
        /// interface used to classify an object that can exist inside of an equation
        /// </summary>
        protected interface IFormulaic : IValuable
        {
            IContainer Container { get; set; }
            bool Reciprocal { get; set; }
            Formula Index { get; set; }
            /// <summary>
            /// Catch-Relation, meant to "catch" and make accessible through the IFormulaic interface the ultimate IContainer Equation.Relation, 
            /// "thrown" to it from its IFormulaic.Container using IContainer.TRelation.
            /// </summary>
            relpair CRelation { get; }
            IFormulaic DeepCopy();
        }
        
        public enum relation { None, Equal, Greater, Less, GreaterEqual, LessEqual, Maps }
        [Serializable]
        public struct relpair//I could have made this a Tuple or an IEnumerable but I really really don't want this to cross-reference
        {
            public relation L;
            public relation R;

            public bool has(Func<relation, bool> cond) => cond(L) || cond(R);
            public bool has(relation r) => L == r || R == r;

            public bool inequality => has(r => (int)r > 1);
        }

        [Serializable]
        protected abstract class IVariableContainer : IContainer
        {
            public virtual List<IFormulaic> parts { get; set; }
            public virtual void Add(IFormulaic part, bool new_equals_sign_flag = false) =>
                throw new DevError("DevError: IContainer.Add not supported here.");
            public virtual relpair TRelation { get => throw new DevError("DevError: IContainer.TRelation not supported here."); }
            public virtual IFormulaic this[int i, bool n = true] { get => parts[i]; set => parts[i] = value; }

            /// <summary>
            /// Returns every IVariable inside the IVariableContainer. If there is anything in there that isn't IVariable,
            /// then it will come out as null.
            /// </summary>
            public IEnumerable<IVariable> DeepVariables
            {
                get
                {
                    foreach (var p in parts)
                    {
                        switch(p)
                        {
                            case Function p1:
                                yield return p1 as IVariable;
                                break;
                            case Tensor p2:
                                foreach (var r in p2.parts)
                                    foreach (var c in r)
                                    {
                                        if (c is IVariableContainer)
                                            foreach (var v in (c as IVariableContainer).DeepVariables)
                                                yield return v;
                                    }
                                yield return p2 as IVariable;//goto case Function; dammit C# 7 if only
                                break;
                            case Formula p3:
                                foreach (var v in (p3 as IVariableContainer).DeepVariables)
                                    yield return v;
                                break;
                            case Operation p4:
                                foreach(var f in p4.Operands)
                                    foreach (var v in (f as IVariableContainer).DeepVariables)
                                        yield return v;
                                switch(p4.Parameters)
                                {
                                    case List<Equation> l:
                                        foreach (var e in l)
                                            foreach (var v in (e as IVariableContainer).DeepVariables)
                                                yield return v;
                                        break;
                                    case IEnumerable<Equation> i:
                                        foreach (var e in i)
                                            foreach (var v in (e as IVariableContainer).DeepVariables)
                                                yield return v;
                                        break;
                                    case List<Formula> l1:
                                        foreach (var e in l1)
                                            foreach (var v in (e as IVariableContainer).DeepVariables)
                                                yield return v;
                                        break;
                                    case IEnumerable<Formula> i1:
                                        foreach (var e in i1)
                                            foreach (var v in (e as IVariableContainer).DeepVariables)
                                                yield return v;
                                        break;
                                    case Equation E:
                                        foreach (var v in (E as IVariableContainer).DeepVariables)
                                            yield return v;
                                        break;
                                    case Formula F:
                                        foreach (var v in (F as IVariableContainer).DeepVariables)
                                            yield return v;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case Term p5:
                                foreach (var v in (p5 as IVariableContainer).DeepVariables)
                                    yield return v as IVariable;
                                break;
                            default:
                                break;
                        }
                        if (p is IFormulaic && (p as IFormulaic).Index != null)
                            foreach (var v in (p as IFormulaic).Index.DeepVariables)
                                yield return v as IVariable;
                    }
                }
            }
            public IEnumerable<IVariable> Variables
            {
                get
                {
                    foreach(var p in parts)
                    {
                        if (p is IVariable)
                            yield return p as IVariable;
                        else if (p is IVariableContainer)
                            foreach (var v in (p as IVariableContainer).Variables)
                                yield return v;
                    }
                }
            }
            public void ConvertVariables(Func<IVariable, IVariable> func)
            {
                for(var p = 0; p < parts.Count; p++)
                {
                    if (parts[p] is IVariable)
                        parts[p] = func(parts[p] as IVariable);
                    else if (parts[p] is IVariableContainer)
                        (parts[p] as IVariableContainer).ConvertVariables(func);
                }
            }
            public void ConvertVariables(Func<IVariable, Tuple<IVariable, Func<int, int>>> func)
            {
                for (var p = 0; p < parts.Count; )
                {
                    if (parts[p] is IVariable)
                    {
                        var both = func(parts[p] as IVariable);
                        parts[p] = both.Item1;
                        p = both.Item2(p);
                    }
                    else if (parts[p] is IVariableContainer)
                    {
                        (parts[p] as IVariableContainer).ConvertVariables(func);
                        p++;
                    }
                }
            }
            /// <summary>
            /// Returns every IVariable inside the IVariableContainer except the ones that are created in their parents' 
            /// constructor methods. If there is anything in there that isn't IVariable, then it will come out as null.
            /// </summary>
            public IEnumerable<IVariable> AddedVariables
            {
                get
                {
                    if (parts.Exists(v => v is IVariable))
                        foreach (var p in parts)
                        {
                            var f = p as IFormulaic;
                            if (!(f.Container is Term && ((f.Container as Term).denominator[0] == p || (f.Container as Term).numerator[0] == p)))
                                yield return p as IVariable;
                        }
                    else foreach (var p in parts)
                        foreach (var v in (p as IVariableContainer).Variables)
                        {
                            var f = v as IFormulaic;
                            if (!(f.Container is Term && ((f.Container as Term).denominator[0] == v || (f.Container as Term).numerator[0] == v)))
                                yield return v as IVariable;
                        }
                }
            }

            public IEnumerable<IFormulaic> Formulaics
            {
                get
                {
                    foreach (var p in parts)
                    {
                        yield return p as IFormulaic;

                        if (p is IVariableContainer)
                        {
                            foreach (var f in (p as IVariableContainer).Formulaics)
                                yield return f as IFormulaic;
                        }
                        if (p is Tensor)
                        {
                            foreach (var r in (p as Tensor).parts)
                                foreach (var c in r)
                                    if (c is IVariableContainer)
                                        foreach (var f in (c as IVariableContainer).Formulaics)
                                            yield return f as IFormulaic;
                        }
                    }
                }
            }
            
            public void substitute(string substitutee, IVariable substituent)
            {
                parts = parts.ConvertAll(P =>
                {
                    IFormulaic p = dcpy(P);
                    switch (p)
                    {
                        case Function p1 when p1.name == substituent.name && substituent is Function:
                            p = substituent;
                            (p as Tensor).name = substitutee;
                            break;
                        case Tensor p2:
                            foreach (var r in p2.parts)
                                foreach (var c in r)
                                {
                                    if (c is IVariableContainer)
                                        (c as IVariableContainer).substitute(substitutee, substituent);
                                }
                            if (p2.name != null && p2.name == substitutee && substituent is Tensor)
                            {
                                p = dcpy(substituent as Tensor);
                                (p as Tensor).name = substitutee;
                            }
                            break;
                        case Formula p3:
                            (p as Formula).substitute(substitutee, substituent);
                            break;
                        case Operation p4:
                            foreach (var f in p4.Operands)
                                f.substitute(substitutee, substituent);

                            switch (p4.Parameters)
                            {
                                case List<Equation> l:
                                    p4.Parameters = l.ConvertAll(e =>
                                    {
                                        e.substitute(substitutee, substituent);
                                        return e;
                                    });
                                    break;
                                case IEnumerable<Equation> i:
                                    p4.Parameters = i.ToList().ConvertAll(e =>
                                    {
                                        e.substitute(substitutee, substituent);
                                        return e;
                                    });
                                    break;
                                case List<Formula> l1:
                                    p4.Parameters = l1.ConvertAll(e =>
                                    {
                                        e.substitute(substitutee, substituent);
                                        return e;
                                    });
                                    break;
                                case IEnumerable<Formula> i1:
                                    p4.Parameters = i1.ToList().ConvertAll(e =>
                                    {
                                        e.substitute(substitutee, substituent);
                                        return e;
                                    });
                                    break;
                                case Equation E:
                                    E.substitute(substitutee, substituent);
                                    p4.Parameters = E;
                                    break;
                                case Formula F:
                                    F.substitute(substitutee, substituent);
                                    p4.Parameters = F;
                                    break;
                                default:
                                    break;
                            }
                            p = p4;
                            break;
                        case Term p5:
                            p5.substitute(substitutee, substituent);
                            p = p5;
                            break;
                        default:
                            break;
                    }
                    if (p is IFormulaic && (p as IFormulaic).Index != null)
                        (p.Index as Formula).substitute(substitutee, substituent);
                    return p;
                });
                UpdateContainerReferences();
            }

            public void ConvertAllVariables(Func<IVariable, IVariable> func)
            {
                parts = parts.ConvertAll(p =>
                {
                    switch (p)
                    {
                        case Function p1:
                            p = func(p1);
                            break;
                        case Tensor p2:
                            foreach (var r in p2.parts)
                                foreach (var c in r)
                                {
                                    if (c is IVariableContainer)
                                        (c as IVariableContainer).ConvertAllVariables(func);
                                }
                            p = func(p2);
                            break;
                        case Formula p3:
                            (p as Formula).ConvertAllVariables(func); ;
                            break;
                        case Operation p4:
                            foreach (var f in p4.Operands)
                                f.ConvertAllVariables(func);

                            switch (p4.Parameters)
                            {
                                case List<Equation> l:
                                    p4.Parameters = l.ConvertAll(e =>
                                    {
                                        e.ConvertAllVariables(func);
                                        return e;
                                    });
                                    break;
                                case IEnumerable<Equation> i:
                                    p4.Parameters = i.ToList().ConvertAll(e =>
                                    {
                                        e.ConvertAllVariables(func);
                                        return e;
                                    });
                                    break;
                                case List<Formula> l1:
                                    p4.Parameters = l1.ConvertAll(e =>
                                    {
                                        e.ConvertAllVariables(func);
                                        return e;
                                    });
                                    break;
                                case IEnumerable<Formula> i1:
                                    p4.Parameters = i1.ToList().ConvertAll(e =>
                                    {
                                        e.ConvertAllVariables(func);
                                        return e;
                                    });
                                    break;
                                case Equation E:
                                    E.ConvertAllVariables(func);
                                    p4.Parameters = E;
                                    break;
                                case Formula F:
                                    F.ConvertAllVariables(func);
                                    p4.Parameters = F;
                                    break;
                                default:
                                    break;
                            }
                            p = p4;
                            break;
                        case Term p5:
                            p5.ConvertAllVariables(func);
                            p = p5;
                            break;
                        default:
                            break;
                    }
                    if (p is IFormulaic && (p as IFormulaic).Index != null)
                        (p.Index as Formula).ConvertAllVariables(func);
                    return p;
                });
            }

            /// <summary>
            /// Always do this after deserialising an IVariableContainer. The serialisation ruins the references to each of the 
            /// parts' Container so we have to fix them here.
            /// </summary>
            public void UpdateContainerReferences()
            {
                for(var p = 0; p < parts.Count; p++)
                {
                    var t2 = parts[p].Container == this;
                    parts[p].Container = this;
                    t2 = parts[p].Container == this;
                    if (parts[p] is IVariableContainer)
                        (parts[p] as IVariableContainer).UpdateContainerReferences();
                    var t1 = parts[p].Container == this;
                }
            }
        }
        [Serializable]
        protected abstract class FormulaNodeContainer
        {
            public virtual relpair CRelation => new relpair();//OVERRIDE THIS
            public virtual relpair TRelation => CRelation;
            [Serializable]
            public class FormulaNode : IContainer
            {
                public List<IFormulaic> parts { get; set; }
                public FormulaNodeContainer parent;
                public relpair CRelation => parent.TRelation;
                public relpair TRelation => CRelation.L == default(relation) && CRelation.R == default(relation) ? new relpair() : CRelation;
                public IFormulaic this[int i, bool n = true] { get => parts[i]; set => parts[i] = value; }

                public FormulaNode(FormulaNodeContainer parent, relpair crelation)
                {
                    this.parent = parent;
                    parts = new List<IFormulaic>();
                }

                public void Add(IFormulaic part, bool new_equals_sign_flag = false)
                {
                    if (part == null)
                        throw new SyntaxError("SyntaxError: Empty field in");
                    else if (part is Formula && new_equals_sign_flag)
                    {
                        part.Container = this;
                        parts.Add(part);
                    }
                    else
                    {
                        if (parts.Count == 0) parts.Add(new Formula(this));
                        (parts.Last() as IContainer).Add(part);
                    }
                }
            }
            protected FormulaNode formulaNode;
        }

        [Serializable]
        protected class Equation : IVariableContainer
        {
            //public bool IsFunctionIdentity
            //{ get =>
            //        parts.Count == 2 && //There are 2 sides
            //        ((parts[0] as IContainer).parts[0] as IContainer).parts[0] is  //There is 1 effective Piece on the left
            //        Root.pieces[0] is FunctionPiece && //that effective Piece is a FunctionPiece
            //        producti.arrprd(((Root.pieces[0] as FunctionPiece).operand.variables[0] as BracketPiece).inside.pieces,
            //            (o, i) => o &= i.val == "(" || i.val == ")" || !Globals.values.ContainsKey(i.val), true); //The parameter of the function is unknown
            //}
            //public bool IsFunctionIdentity { get; set; }
            public List<relation> EqRelations { get; set; } //The relationship that then left side of each relation symbol has with the right side
            
            public Equation()
            {
                parts = new List<IFormulaic>();
                EqRelations = new List<relation>();
            }
            public Equation(IEnumerable<IFormulaic> formulae, List<relation> idfunc)
            {
                parts = formulae.ToList();
                parts.ForEach(p => (p as Formula).Container = this);
                EqRelations = idfunc;
            }

            public override void Add(IFormulaic part, bool new_equals_sign_flag = false)
            {
                if (part == null)
                    throw new SyntaxError("SyntaxError: Empty field in");
                else if (part is Formula && new_equals_sign_flag)
                {
                    part.Container = this;
                    parts.Add(part);
                }
                else
                {
                    if (parts.Count == 0) parts.Add(new Formula(this));
                    (parts.Last() as IContainer).Add(part);
                }
            }

            public static void Remember(IVariable variable)
            {
                switch (variable)
                {
                    case Tensor t when t.known:
                        if (Globals.values.ContainsKey(variable.name))
                            Globals.values.Remove(variable.name);
                        Globals.values.Add(variable.name, variable);
                        break;
                    case Tensor ft when ft.name.Contains(':')://these are all them localised variables to discard
                        break;//this arrprd is will return false if there are any known variables
                    case Function f when producti.arrprd(f.Parameters, (o, i) => o &= !i.AddedVariables.ToList().Exists(v => v.known), true) && f.Process != null:
                        if (!Globals.values.ContainsKey(f.name))
                            Globals.values.Add(f.name, f);
                        break;
                    default:
                        break;
                        if (Globals.Parameters.ToList().Exists(p => p.name == variable.name))// || (variable is Tensor && (variable as Tensor)))
                            break;
                        //throw new DevError("DevError: Equation is trying to commit to the memory something that it doesn't know");
                       
                }
            }

            public IEnumerable<Equation> Partition
            {
                get
                {
                    for (int i = 0; i + 1 < parts.Count; i++)
                    {
                        //something here is fucking around with my Formula.Container references
                        yield return new Equation(parts.GetRange(i, 2), EqRelations.GetRange(i, 1));
                    }
                    //yield return new Equation(parts, EqRelations);
                }

            }
            public IEnumerable<Equation> Equations => from p in Partition where p.EqRelations[0] != relation.None select p;
            public Equation RemoveParameters()
            {
                Formula remove_parameters(Formula subject, IContainer container)
                {
                    var rout = new Formula(container);
                    foreach (Term p2 in rout.parts)
                    {
                        subject.Add(new Term(subject, false, remove_parameters(subject.Index, subject.Container)));
                        foreach (var p3 in p2.parts)
                        {
                            if (!(p3 is Tensor && Globals.Parameters.ToList().Exists(s => s.name == (p3 as Tensor).name)))//Don't expand these brackets. I need the compiler to stop reading the condition if 'p3' isn't a Tensor
                                subject.Add(p3);
                        }
                    }
                    return rout;
                }
                var cout = new Equation();
                foreach (Formula p1 in parts)
                {
                    cout.Add(new Formula(cout, remove_parameters(p1.Index, this)));
                    cout.parts[cout.parts.Count - 1] = remove_parameters(cout.parts[cout.parts.Count - 1] as Formula, cout);
                }
                return cout;
            }

            public Formula CollectLikeTerms(Formula cin)
            {
                bool IsLike<T>(T a, T b) where T : IFormulaic
                {
                    switch(a)
                    {
                        case IVariable i when b is IVariable:
                            if (i.name == null && (b as IVariable).name == null)
                            {
                                if ((i as Tensor).DimDim == 0 && (b as Tensor).DimDim == 0)//yet to add like-term collector for vectors & matrices
                                    if ((i as Tensor).parts[0][0] is Scalar && (b as Tensor).parts[0][0] is Scalar)
                                        return ((i as Tensor).parts[0][0] as Scalar).output == ((b as Tensor).parts[0][0] as Scalar).output;
                                    else
                                        return false;
                                else return false;//it looks bad but I want to leave it like this until I write code to support matrices/vectors here
                            }
                            else
                                return i.name == (b as IVariable).name;
                        case IFormulaic f when b is Formula && f is Formula: //C# 7.0 can't directly test if T is Formula in a switch statement (I think C# 7.1 can?)
                            return (b as Formula).Variables.Zip((f as Formula).Variables, (x, y) =>
                            {
                                if (x is Tensor && y is Tensor)
                                    return IsLike(x as Tensor, y as Tensor);
                                else if (x is Formula && y is Formula)
                                    return IsLike(x as Formula, y as Formula);
                                else
                                    return false;
                            }).All(s => s);
                        case IFormulaic t when b is Term && t is Term:
                            var t1 = (from A in (b as Term).SeparateCoefficient.Item2.Variables where A.name != null
                                      from B in (t as Term).SeparateCoefficient.Item2.Variables where B.name != null
                                      select A.name == B.name && IsLike((A as IFormulaic).Index, (B as IFormulaic).Index)).All(s => s);
                            return t1;
                        default:
                            return a == null && b == null;
                    }
                }
                //quicksort-mimmicking algorithm for finding like terms, because I really don't want to do a 2D for-loop 
                void Qcollect_formula(Formula t, int h, int l = 0, int i = 1)
                {
                    if (l < h)
                    {
                        for (int j = l; j < h; j++)
                        {
                            if (IsLike(t.parts[j], t.parts[h]))
                            {
                                //i++;

                                //var new_term = new Term(t, false);
                                //(t.parts[j] as Term).parts.ForEach(p => new_term.Add(p));//pour all the coefficients in from either Term
                                //(t.parts[h] as Term).parts.ForEach(p => new_term.Add(p));
                                var new_coefficient = (t.parts[h] as Term).SeparateCoefficient.Item1.add((t.parts[j] as Term).SeparateCoefficient.Item1);
                                t.parts[h] = (t.parts[h] as Term).SeparateCoefficient.Item2;
                                (t.parts[h] as Term).Add(new_coefficient);

                                Qcollect_trm(t.parts[h] as Term, (t.parts[h] as Term).numerator, (t.parts[h] as Term).numerator.Count - 1);//tidy them up
                                Qcollect_trm(t.parts[h] as Term, (t.parts[h] as Term).denominator, (t.parts[h] as Term).denominator.Count - 1);//tidy them up

                                //t.parts.Insert(h, new_term);//add the new_term
                                //t.parts.RemoveAt(h + 1);//remove the one that was in its place
                                t.parts.RemoveAt(j);//remove the other one that was poured in
                                j--; h--;
                            }
                        }
                        //var temp_n = t.parts[i + 1];
                        //t.parts[i + 1] = t.parts[h];
                        //t.parts[h] = temp_n;
                        //var next_h = i + 1;
                        
                        Qcollect_formula(t, h - 1, l);
                        //Qcollect_formula(t, h, next_h + 1);
                    }
                }
                void Qcollect_trm(Term T, List<IFormulaic> t, int h, int l = 0)
                {
                    if (l < h)
                    {
                        int i = l - 1;

                        for (int j = l; j < h; j++)
                        {
                            if (IsLike(t[j], t[h]))
                            {
                                //i++;

                                var index_increment = new Term(t[h].Index, false);
                                if (t[j].Index != null && t[j].Index.parts.Count > 0)
                                    index_increment.Add(t[j].Index);//Get the power to which t.numerator[j] was raised
                                if (t[h].Index != null)
                                    t[h].Index.Add(index_increment);//Add that index to the index of t.numerator[hn]
                                t.RemoveAt(j);//remove t.numerator[j] because it has already been multiplied into t.numerator[hn] as represented there
                                
                                j--; h--;//account for the decrease in list size
                            }
                        }
                        //var temp_n = t[i + 1];
                        //t[i + 1] = t[h];
                        //t[h] = temp_n;
                        //var next_h = i + 1;

                        Qcollect_trm(T, t, h - 1, l);
                        //Qcollect_trm(T, t, h, next_h + 1);
                    }
                }

                var cout = new Formula(this);
                var refactee = new IFormulaic[cin.parts.Count];
                cin.parts.CopyTo(refactee);
                cout.parts = refactee.ToList();//avoid shallow copying

                foreach (var p in cout.parts)
                {
                    var t = p as Term;
                    Qcollect_trm(t, t.numerator, t.numerator.Count - 1);//make sure all of the like coefficients are collected
                    Qcollect_trm(t, t.denominator, t.denominator.Count - 1);//make sure all of the like coefficients are collected
                }
                Qcollect_formula(cout, cout.parts.Count - 1);//collect like terms

                return cout;

                //var comparand = (parts[0] as IContainer).parts[0];
                //var like_terms = from f in (parts[0] as IContainer).parts.ConvertAll(p => p as Term) //behold the mightiest one-liner
                //                 from s in (parts[1] as IContainer).parts.ConvertAll(p => p as Term)
                //                 where f.numerator.Where(n => (n as IVariable).name != null).Union
                //                 (s.numerator.Where(n => (n as IVariable).name != null)).SequenceEqual
                //                 (f.numerator.Where(n => (n as IVariable).name != null)) &&
                //                 f.denominator.Where(n => (n as IVariable).name != null).Union
                //                 (s.denominator.Where(n => (n as IVariable).name != null)).SequenceEqual
                //                 (f.denominator.Where(n => (n as IVariable).name != null))
                //                 select f;
            }

            //public Equation CancelOut()
            //{
            //    var cout = new Equation(parts, EqRelations).Partition;
            //}
            //Equation cancelout()
            //{
            //    var cout = new Equation(parts, EqRelations);


            //}

            //I've used a generic type here because I want invendum to be of a class that inherits IVariable, not of an actual IVariable, so don't change it.
            //The compiler will compile the method only once before runtime, and therfore can't know the actual Type of invendum, so it will just create a new IVariable with only fields that IVariable necessitates.
            public IVariable SolveFor<T>(T invendum, Dictionary<string, IVariable> _vars) where T : IVariable
            {
                //var cout = dcpy(invendum) as IVariable;
                IVariable cout;
                //make the Equation think that it doesn't already know the variable that it's trying to solve for, in case the solution is different from how it is remembered.
                var vars = (from v in _vars where v.Key != invendum.name select v.Value).ToDictionary(k => k.name);
                
                //var vars = values.ToList().ConvertAll(v => new Tensor
                //(v.Value.array.ConvertAll(r => r.ConvertAll(c => new Scalar(c) as IValuable)), false, v.Key) as IVariable);

                //TEMPORARY CODE
                var pp = (parts[0] as IContainer).parts[0] as Term; //first term of first formula
                var ppp = pp.parts[0] as IVariable; //first effective IVariable in the Term

                //if (Variables.Where(v => Globals.Parameters.ToList().Exists(p => p.name == v.name)).Count() > 1 ||//multiple parameters OR
                //    (Variables.Where(v => Globals.Parameters.ToList().Exists(p => p.name == v.name)).Count() == 1 &&//(single parameter AND
                //    !(parts.Count == 2 && (parts[0] as IContainer).parts.Count == 1 && pp.parts.Count == 3)))//that parameter isn't the only thing on the left)
                //{
                //    return invendum;
                //}
                var variables = new List<IVariable>();//get rid of repeating unknown parameters
                foreach (var v in Variables.Where(v => Globals.Parameters.ToList().Exists(p => p.name == v.name && !v.Value(vars).known && v.name != invendum.name)))
                    if (!variables.Exists(s => s.name == v.name))
                        variables.Add(v);//account for whether or not 'invendum' is one of said parameters
                if (variables.Count() > 0)//(Globals.Parameters.ToList().Exists(p => p.name == invendum.name && !invendum.Value(vars).known) ? 0 : 1))
                    return invendum;

                if (parts.Count == 2 && (parts[0] as IContainer).parts.Count == 1 && pp.parts.Count == 2 && invendum.name == ppp.name)
                {

                    if (vars.ContainsKey(ppp.name))
                    {
                        cout = vars[ppp.name] as Tensor;
                    }
                    else
                    {
                        if (invendum is Function)
                        {
                            if (EqRelations[0] == relation.Maps)
                            {
                                var process = dcpy(parts[1] as Formula);
                                //process.Container = null;
                                cout = new Function(invendum.Container, ppp.name, (invendum as Function).Parameters, process, true, invendum.Index);
                                //(cout as Function).Process = process;
                            }
                            else cout = (invendum as Function).Value(vars);
                        }
                        else cout = (parts[1] as Formula).Value(vars);
                        cout.name = ppp.name;
                        //vars.Add(cout.name, cout);
                    }
                }
                else cout = invendum;
                
                return cout;
            }
            
            /// <summary>
            /// SolveFor all the Variables foreach of the Equations and Remember them
            /// </summary>
            public void Evaluate()
            {
                foreach(var e in Equations)
                {
                    //I want to include this greened out bit so that the SolveFor algorithm can cancel out parameter variables in its 
                    //search for other variables but for now I'm just not allowing them to even get here
                    foreach (var v in e.DeepVariables.Where(s => s.name != null/* && !Globals.Parameters.ToList().Exists(p => p.name == s.name)*/))
                        Remember(SolveFor(v, Globals.values));
                }
            }

            //public IEnumerable<Tensor> Plot(IEnumerable<Tensor> parameters) =>
            //    from i in Variables select i is IFormulaic ? 
            //    SolveFor((i as IValuable).Value(Globals.values), Globals.values.Values.Concat(parameters).ToDictionary(v => v.name)) : null;
           
        }
        [Serializable]
        protected class Formula : IVariableContainer, IFormulaic
        {
            public IContainer Container { get; set; }
            public bool Reciprocal { get; set; }
            public Formula Index { get; set; }
            public relpair CRelation
            {
                get
                {
                    if (Container == null || !Container.parts.Contains(this))
                        return new relpair();
                    var t2 = Container.parts.IndexOf(this);
                    var t1 = new relpair();
                    if (Container.parts.Count > 0)
                    {
                        t1.L = Container is Equation && this == Container.parts[0] ? relation.None : Container is Equation ? (Container as Equation).EqRelations[Container.parts.IndexOf(this) - 1]
                            : Container.TRelation.L;
                        t1.R = Container is Equation && this == Container.parts.Last() ? relation.None : Container is Equation ? (Container as Equation).EqRelations[Container.parts.IndexOf(this)]
                            : Container.TRelation.R;
                    }
                    return t1;
                }
            }
            public override relpair TRelation => CRelation;

            public IFormulaic this[int i, int j, bool? n = null]
            {
                get => n.HasValue ? n.Value ? (parts[i] as Term).numerator[j] : (parts[i] as Term).denominator[j] : (parts[i] as Term).parts[j];
                set
                {
                    if (n.HasValue)
                        if (n.Value) (parts[i] as Term).numerator[j] = value;
                        else (parts[i] as Term).denominator[j] = value;
                    else (parts[i] as Term).parts[j] = value;
                }
            }
            public override IFormulaic this[int i, bool n = true]
            {
                get => parts[i];
                set => parts[i] = value;
            }

            public Formula(IContainer parent, Formula index = null)
            {
                Container = parent;
                parts = new List<IFormulaic>();
                Index = index;
            }

            public Tensor Value(Dictionary<string, IVariable> vars)
            {
                if (Container != null && CRelation.R == relation.Maps)
                    if (Variables.Count() == 1)
                        return null;
                    else
                        throw new SyntaxError("SyntaxError: Can't (yet) put anything else on the left of a Function identity.");
                else
                {
                    try
                    {
                        var t1 = (producti.arrprd<Tensor, IValuable>(parts.ConvertAll(p =>
                        {
                            var x1 = (p as IFormulaic).Value(vars);
                            return x1;
                        }), (s, p) =>
                        {
                            var x1 = s as Tensor;//.Value(vars);
                            var x2 = x1.add(p);
                            return x2;
                        }, 'c') as Tensor);
                        var t2 = t1.pow(Index, vars);
                        return t2;
                    }
                    catch (Error e)
                    {
                        //var ename = e.Message.Contains('\'') ? e.Message.SkipWhile(c => c != '\'').TakeWhile(c => c != '\'') : "";
                        //if (ename.Contains('y') || ename.Contains('x') || ename.Contains('z'))

                        //else

                        //if (parts.Concat(from v in Variables where v is IFormulaic select v as IFormulaic).ToList()
                        //    .Exists(p => !Globals.Parameters.ToList().Exists(s => p is Tensor && s.name == (p as Tensor).name)))
                        //    return new Tensor(new Formula(null) { parts = new List<IFormulaic> { this } }, false, null, null, this)
                        if (Variables.Where(v => !Globals.Parameters.ToList().Exists(t => t.name == v.name) && !v.known/* && v is Tensor*/).Count() == 0)
                            return new Tensor(new Formula(null) { parts = new List<IFormulaic> { this } }, false, null, null, this); ;

                        //Debug.Write(e.Message + " in Formula:" + Container.parts.IndexOf(this));
                        throw new SyntaxError(e.Message + " in Formula:" + Container.parts.IndexOf(this));
                    }
                }
            }

            public IFormulaic DeepCopy()
            {
                var cout = new Formula(Container, Index == null ? null : Index.DeepCopy() as Formula);
                cout.Reciprocal = Reciprocal;
                cout.parts = new List<IFormulaic>();
                cout.parts.AddRange(from p in parts select p.DeepCopy());
                cout.UpdateContainerReferences();
                return cout;
            }

            public override void Add(IFormulaic part, bool new_equals_sign_flag = false)
            {
                if (part == null)
                    throw new SyntaxError("SyntaxError: Empty field in");
                else if (part is Formula && new_equals_sign_flag)
                    Container.Add(part);
                else if (part is Term)
                {
                    part.Container = this;
                    parts.Add(part);
                }
                else
                {
                    if (parts.Count == 0)
                        parts.Add(new Term(this, false));
                    (parts.Last() as Term).Add(part);
                }
            }
        }

        [Serializable]
        protected class Term : IVariableContainer, IFormulaic
        {
            public List<IFormulaic> numerator;
            public List<IFormulaic> denominator;
            public override List<IFormulaic> parts
            {
                get => numerator.Concat(denominator).ToList();
                set {
                    if (value.Last() == null) denominator = value.GetRange(0, value.Count - 2);
                    else numerator = value;
                }
            }
            public IContainer Container { get; set; }
            public bool Reciprocal { get; set; }
            public Formula Index { get; set; }
            public relpair CRelation => Container == null ? new relpair() : Container.TRelation; //catch
            public override relpair TRelation => CRelation; //throw

            public override IFormulaic this[int i, bool n = true]
            {
                get => n ? numerator[i] : denominator[i];
                set { if (n) numerator[i] = value; else denominator[i] = value; }
            }

            public Term(IContainer parent, bool reciprocal, Formula index = null)
            {
                Container = parent;
                numerator = new List<IFormulaic>(); //{ new Tensor(new Scalar(1), false, null, null, this) };
                denominator = new List<IFormulaic>() { new Tensor(new Scalar(1), true, null, null, this) };

                Reciprocal = reciprocal;
                Index = index;
            }
            
            public Tensor Value(Dictionary<string, IVariable> vars)
            {
                if ((Container as IFormulaic).Container != null && CRelation.R == relation.Maps)
                    if (Variables.Count() == 1)
                        return null;
                    else
                        throw new SyntaxError("SyntaxError: Can't (yet) put anything else on the left of a Function identity.");
                else
                {
                    if (numerator.Count == 0)
                        return new Tensor(new Scalar(0), false);
                    if (denominator.Count == 0)
                        throw new MathsError("MathsError: Dividing by zero in Term:" + Container.parts.IndexOf(this));
                    try
                    {//look at this beautiful one-liner
                        var t1 = numerator; ;
                        var t2 = t1.ToList().ConvertAll(p => //pi product all of the numerator
                        {
                            var x1 = (p as IFormulaic).Value(vars);
                            var x2 = x1.pow((p as IFormulaic).Index, vars);
                            return x2;
                        });
                        var t3 = producti.arrprd<Tensor, IFormulaic>(t2, (s, p) => 
                        {
                            var x1 = s as Tensor;//(s as IFormulaic).Value(vars);
                            var x2 = x1.dot(p);
                            return x2;
                        }, 'c');
                        var t4 = denominator; ;
                        var t5 = t4.ToList().ConvertAll(p => //pi product all of the numerator
                        {
                            var x1 = (p as IFormulaic).Value(vars);
                            var x2 = x1.pow((p as IFormulaic).Index, vars);
                            return x2;
                        });
                        var t6 = producti.arrprd<Tensor, IFormulaic>(t5, (s, p) =>
                        {
                            var x1 = s as Tensor;//(s as IFormulaic).Value(vars);
                            var x2 = x1.dot(p);
                            return x2;
                        }, 'c');
                        var t7 = (t6 as Tensor).inv(vars);
                        if (t3 == null)
                            throw new SyntaxError("SyntaxError: No numerator");
                        var t8 = (t3 as Tensor).dot(t7 as Tensor);
                        var t9 = t8.pow(Index, vars);
                        return t9;

                        //if (numerator.Where(n => n is Tensor ? //excuse numerator Tensors that are parameters from being unknown if they are unknown
                        //n.Value(vars).known ? true : 
                        //!Globals.Parameters.ToList().Exists(s => s.name == (n as Tensor).name) : true).Count() == 0)
                        //    return new Tensor(new Scalar(0), Reciprocal, null, null, Container).pow(Index, vars); //0^0 is 1 and negative powers have to be caught out, so I'm putting pow here

                        //return (producti.arrprd<Tensor, IFormulaic>(numerator.Where(n => n is Tensor ? //excuse numerator Tensors that are parameters from being unknown if they are unknown
                        //n.Value(vars).known ? true : !Globals.Parameters.ToList().Exists(s => s.name == (n as Tensor).name) : true).ToList().ConvertAll(p => //pi product all of the numerator
                        //(p as IFormulaic).Value(vars).pow((p as IFormulaic).Index, vars)), (s, p) => (s as IFormulaic).Value(vars).dot(p), 'c') as Tensor).dot
                        //((producti.arrprd<Tensor, IFormulaic>(denominator.Where(d => d is Tensor ? //excuse denominator Tensors that are parameters from being unknown if they are unknown
                        //d.Value(vars).known ? true : !Globals.Parameters.ToList().Exists(s => s.name == (d as Tensor).name) : true).ToList().ConvertAll(p => //divide by pi product of all of the denominator
                        //(p as IFormulaic).Value(vars).pow((p as IFormulaic).Index, vars)), (s, p) => (s as IFormulaic).Value(vars).dot(p), 'c') as Tensor).inv(vars)
                        //as Tensor).pow(Index, vars); //raise the whole thing to whatever power it's supposed to be raised to
                    }
                    catch (Error e)
                    {
                        //if (parts.Concat(from v in Variables where v is IFormulaic select v as IFormulaic).ToList()
                        //    .Exists(p => !Globals.Parameters.ToList().Exists(s => p is Tensor && s.name == (p as Tensor).name)))
                        //    return new Tensor(new Formula(null) { parts = new List<IFormulaic> { this } }, false, null, null, this);
                        if (Variables.Where(v => !Globals.Parameters.ToList().Exists(t => t.name == v.name) && !v.known).Count() == 0)
                            return new Tensor(new Formula(null) { parts = new List<IFormulaic> { this } }, false, null, null, this);
                        throw new SyntaxError(e.Message + " in Term:" + Container.parts.IndexOf(this));
                    }
                }
            }

            public IFormulaic DeepCopy()
            {
                var cout = new Term(Container, Reciprocal, Index == null ? null : Index.DeepCopy() as Formula);
                cout.numerator = new List<IFormulaic>();
                cout.numerator.AddRange(from p in numerator select p.DeepCopy());
                cout.denominator = new List<IFormulaic>();
                cout.denominator.AddRange(from p in denominator select p.DeepCopy());
                return cout;
            }

            public List<Tensor> getParams() { return parts.Where(t => t is Tensor && ((Tensor)t).known).Cast<Tensor>().ToList(); }
            
            public override void Add(IFormulaic part, bool new_equals_sign_flag = false)
            {
                if (part == null)
                    throw new SyntaxError("SyntaxError: Empty field");
                else if (part is Tensor || part is Operation || part is Function || (part is Formula && !new_equals_sign_flag))
                {
                    part.Container = this;
                    if (part.Reciprocal) denominator.Add(part); else numerator.Add(part);
                }
                else
                    Container.Add(part);
            }

            public Tuple<Tensor, Term> SeparateCoefficient
            {
                get
                {
                    var purified = new Term(Container, Reciprocal, Index);
                    var coefficient = new Tensor(new Scalar(1), false, Index);

                    foreach(var v in Variables)
                    {
                        if (v.name == null)
                            coefficient = coefficient.dot(v as Tensor);
                        else if (v is Function)
                            purified.Add(v as Function);
                        else
                            purified.Add(v as Tensor);
                    }

                    return new Tuple<Tensor, Term>(coefficient, purified);
                }
            }
        }

        [Serializable]
        protected class Tensor : FormulaNodeContainer, IFormulaic, IVariable
        {
            public List<List<IValuable>> parts;
            public int rows { get { return parts.Count; } }
            public int cols { get { return sort.qsort(parts, r => r.Count)[0].Item2; } }
            /// <summary>
            /// What type of number this is: Scalar => 0, vector => 1 & matrix => 2.
            /// </summary>
            public int DimDim
            {
                get
                {
                    if (cols > 1) return 2;
                    return (int)Math.Round((rows - 1) / (double)rows);
                }
            }
            /// <summary>
            /// whether or not the tensor has any unknown scalars in it.
            /// </summary>
            public bool known {
                get {
                    var unknown = false;
                    parts.ForEach(r => r.ForEach(c => unknown |= !(c is Scalar) || (c is Scalar && !(c as Scalar).known)));
                    return !unknown;
                } }
            public string name { get; set; }
            public IContainer Container { get; set; }
            public bool Reciprocal { get; set; }
            public Formula Index { get; set; }
            public override relpair CRelation => Container == null ? new relpair() : Container.TRelation; //catch
            
            public Tensor(IContainer parent = null, Formula index = null)
            {
                Container = parent;
                parts = new List<List<IValuable>>();
                formulaNode = new FormulaNode(this, CRelation);
            }
            public Tensor(IValuable[,] bits, bool reciprocal, Formula index = null, string name = null, IContainer parent = null)
            {
                parts = new List<List<IValuable>>();
                for (int i = 0; i < bits.GetLength(0); i++)
                { parts.Add(new List<IValuable>()); for (int j = 0; j < bits.GetLength(1); j++) { parts[i].Add(bits[i, j]); } }

                formulaNode = new FormulaNode(this, CRelation);
                this.name = name;
                Container = parent;
                Reciprocal = reciprocal;
                Index = index;
            }
            public Tensor(List<List<IValuable>> bits, bool reciprocal, Formula index = null, string name = null, IContainer parent = null)
            {
                this.name = name;
                parts = bits;
                Container = parent;
                formulaNode = new FormulaNode(this, CRelation);

                Reciprocal = reciprocal;
                Index = index;
            }
            public Tensor(List<IValuable> bits, bool reciprocal, Formula index = null, string name = null, IContainer parent = null)
            {
                this.name = name;
                parts = new List<List<IValuable>>() { bits };
                Container = parent;
                formulaNode = new FormulaNode(this, CRelation);

                Reciprocal = reciprocal;
                Index = index;
            }
            public Tensor(IValuable part, bool reciprocal, Formula index = null, string name = null, IContainer parent = null)
            {
                this.name = name;
                parts = new List<List<IValuable>>() { new List<IValuable> { part } };
                Container = parent;
                formulaNode = new FormulaNode(this, CRelation);

                Reciprocal = reciprocal;
                Index = index;
            }

            /// <summary>
            /// Substitutes all the variables for their temporary/constant values calculated elsewhere,
            /// which exist in the Parameters list.
            /// </summary>
            /// <param name="vars"></param>
            /// <returns></returns>
            public Tensor Value(Dictionary<string, IVariable> vars)
            {
                if (name != null && vars.ContainsKey(name))
                    return vars[name].DeepCopy() as Tensor;

                var cout = new Tensor();
                for(int r = 0; r < parts.Count; r++)
                {
                    cout.parts.Add(new List<IValuable>());
                    for(int c = 0; c < parts[r].Count; c++)
                    {
                        if (parts[r][c] == null)
                            throw new SyntaxError("SyntaxError: Empty field in row:" + r + " in column:" + c + " in " + (DimDim == 1 ? "vector" : "matrix"));
                        if (parts[r][c].Value(vars).known)
                            cout.parts[r].Add(parts[r][c].Value(vars).parts[0][0]);
                        else
                            cout.parts[r].Add(parts[r][c]);//preserve the name/other data so that it can be identified and solved in future

                        if (cout.parts[r][c] is Tensor && (cout.parts[r][c] as Tensor).DimDim > 0) //ERROR_TAG (These are so I can easily Ctrl+F code that gives me errors)
                            throw new SyntaxError("SyntaxError: An element of a matrix or vector cannot be another matrix or vector");
                        //if (!(cout.parts[r][c] as Scalar).output.HasValue)
                        //    throw new MathsError("MathsError: " + name + " is unknown");
                    }
                }
                
                cout.name = name;
                return cout;
            }

            public IFormulaic DeepCopy()
            {
                var cout = new Tensor(Container, Index == null ? null : Index.DeepCopy() as Formula);
                cout.Reciprocal = Reciprocal;
                cout.name = name;
                cout.parts = new List<List<IValuable>>();
                cout.parts.AddRange(from r in parts select 
                                    (from c in r select c is IFormulaic ? 
                                     (c as IFormulaic).DeepCopy() as IValuable : new Scalar((c as Scalar).output)).ToList());
                return cout;
            }

            //public void Equate(Tensor eq) => parts = dcpy(eq.parts);

            public static Tensor FromMatrix(matrix mat)
            {
                return new Tensor(mat.array.ConvertAll(r => r.ConvertAll(c => new Scalar(c) as IValuable)), false);
            }
            public matrix ToMatrix()
            {
                if (!known) return matrix.empty();

                var cout = matrix.empty();
                for (int r = 0; r < parts.Count; r++)
                {
                    cout.array.Add(new List<double>());
                    for (int c = 0; c < parts[r].Count; c++)
                    {
                        cout.array[r].Add(parts[r][c] is Scalar ? (parts[r][c] as Scalar).output.Value : //ERROR_TAG
                            throw new SyntaxError("SyntaxError: An element of a matrix or vector cannot be another matrix or vector"));
                    }
                }

                return cout;
            }

            public void ConvertAllParts(Func<IValuable, IValuable> func)
            {
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        parts[r][c] = func(parts[r][c]);
            }

            public Tensor MatrixInv()
            {
                var product = ToMatrix().inv().array;

                return new Tensor
                (
                product.ConvertAll(r => r.ConvertAll(c => (IValuable)new Scalar(/*name + "_inv", */c))),
                false,
                Index,
                name + "_matinv"
                );
            }
            /// <summary>
            /// Makes every dimension a negative version of itself
            /// </summary>
            public Tensor VectorInv()
            {
                return new Tensor
                (
                parts.ConvertAll(r => r.ConvertAll(c => 
                {
                    if (c is Scalar) return new Scalar(/*name + "_inv", */-(c as Scalar).output);
                    else { (c as IContainer).Add(new Tensor(new Scalar(-1), false)); return c; }
                })),
                false,
                Index,
                name + "_matinv"
                );
            }
            public IValuable inv(Dictionary<string, IVariable> vars)
            {
                List<List<double>> product;
                switch (DimDim)
                {
                    case 0:
                        //if ((parts[0][0] as Scalar).output.Value == 0)
                        //    throw new MathsError("MathsError: Can't divide by zero.");
                        product = new List<List<double>>() { new List<double> { 1 / (parts[0][0] as Scalar).output.Value } };
                        break;
                    case 1:
                        throw new MathsError("MathsError: Can't divide by or find reciprocal of a vector");//ERROR TAG
                    case 2:
                        product = ToMatrix().inv().array;
                        break;
                    default:
                        throw new DevError("DevError: Tensor.DimDim broke.");
                }

                return new Tensor
                (
                product.ConvertAll(r => r.ConvertAll(c => (IValuable)new Scalar(/*name + "_inv", */c))),
                false,
                Index, 
                name + "_inv"
                );
            }

            public Tensor pow(Formula cin, Dictionary<string, IVariable> vars)
            {
                if (cin == null)
                    return this;
                if (cin.Value(vars).DimDim > 0)//ERROR_TAG
                    throw new MathsError("MathsError: You can only raise something to a scalar power.");

                var index = (cin.Value(vars).parts[0][0] as Scalar).output.Value;

                switch (DimDim)
                {
                    case 0:
                        return new Tensor(new Scalar(Math.Pow((parts[0][0] as Scalar).output.Value, index)), Reciprocal, null, name, Container);
                    default:
                        if (Math.Round(index) != index && DimDim == 1)//ERROR_TAG
                            throw new MathsError("MathsError: Can't raise a vector to a non-integer power.");

                        var cout = new Tensor(new Scalar(1), false, null, name);
                        if (index < 0)
                        {
                            cout = inv(vars) as Tensor;
                            for (int i = -1; i > index; i--)
                                cout = dot(cout);
                        }
                        else for (int i = 0; i < index; i++)
                            cout = dot(cout);

                        return cout;
                }
            }
            public IValuable dot(Tensor cin, Dictionary<string, IVariable> vars)
            {
                if (!cin.known)
                    throw new MathsError("MathsError: Cannot evaluate unknown variable '" + cin.name + "'"); //ERROR_TAG
                if (!known)
                    throw new MathsError("MathsError: Cannot evaluate unknown variable '" + name + "'"); //ERROR_TAG

                var new_parts = new List<List<Scalar>>();
                if (DimDim == 0 && cin.DimDim == 0)
                {
                    var t1 = new Tensor(new Scalar(/*name + "£dot£" + cin.name, */(Value(vars).parts[0][0] as Scalar).output * (cin.Value(vars).parts[0][0] as Scalar).output), false, null, name == null ? cin.name : name);
                    return t1;
                }
                else if (DimDim == 0 ^ cin.DimDim == 0)
                {
                    var scalar = new List<Tensor> { this, cin }.Find(t => t.DimDim == 0).parts[0][0] as Scalar;
                    var tensor = new List<Tensor> { this, cin }.Find(t => t.DimDim > 0);
                    for (int i = 0; i < tensor.parts.Count; i++)
                    {
                        new_parts.Add(new List<Scalar>());
                        for (int j = 0; j < tensor.parts[i].Count; j++)
                        {
                            new_parts[i].Add(new Scalar(/*name + "£dot£" + cin.name, */(scalar.Value(vars).parts[0][0] as Scalar).output * (tensor.Value(vars).parts[i][j] as Scalar).output));
                        }
                    }
                }
                else if (DimDim > 0 && cin.DimDim > 0)
                {
                    var product = Value(vars).ToMatrix().matmat(cin.Value(vars).ToMatrix()).array;
                    return new Tensor
                    (
                    product.ConvertAll(r => r.ConvertAll(c => (IValuable)new Scalar(/*name + "£dot£" + cin.name, */c))),
                    false,
                    Index,
                    name + "£dot£" + cin.name
                    );

                }

                throw new SyntaxError("SyntaxError: Invalid Input in 'dot' Function"); //ERROR_TAG
            }
            public Tensor dot(Tensor cin)
            {
                if (!cin.known)
                    throw new MathsError("MathsError: Cannot evaluate unknown variable '" + cin.name + "'"); //ERROR_TAG
                if (!known)
                    throw new MathsError("MathsError: Cannot evaluate unknown variable '" + name + "'"); //ERROR_TAG

                var new_parts = new List<List<Scalar>>();
                if (DimDim == 0 && cin.DimDim == 0)
                {
                    if (parts[0][0] is Scalar && cin.parts[0][0] is Scalar)
                        return new Tensor(new Scalar(/*name + "£dot£" + cin.name, */(parts[0][0] as Scalar).output * (cin.parts[0][0] as Scalar).output), false, null, name == null ? cin.name : name);
                }
                else if (DimDim == 0 ^ cin.DimDim == 0)
                {
                    var scalar = new List<Tensor> { this, cin }.Find(t => t.DimDim == 0).parts[0][0] as Scalar;
                    var tensor = new List<Tensor> { this, cin }.Find(t => t.DimDim > 0);
                    for (int i = 0; i < tensor.parts.Count; i++)
                    {
                        new_parts.Add(new List<Scalar>());
                        for (int j = 0; j < tensor.parts[i].Count; j++)
                        {
                            new_parts[i].Add(new Scalar(/*name + "£dot£" + cin.name, */scalar.output * (tensor.parts[i][j] as Scalar).output));
                        }
                    }//this algorithm currently favours the tensor over the scalar until I establish a clear protocal of how to do this
                    return new Tensor(new_parts.ConvertAll(r => r.ConvertAll(c => c as IValuable)), Reciprocal && cin.Reciprocal,
                        tensor.Index, name == null ? cin.name : name, tensor.Container);
                }
                else if (DimDim > 0 && cin.DimDim > 0)
                {
                    try
                    {
                        var product = ToMatrix().matmat(cin.ToMatrix()).array;
                        return new Tensor
                        (
                        product.ConvertAll(r => r.ConvertAll(c => (IValuable)new Scalar(/*name + "£dot£" + cin.name, */c))),
                        false,
                        Index,
                        name + "£dot£" + cin.name
                        );
                    }
                    catch (Exception e) when (e.Message == "Number of columns in first matrix should be equal to number of rows in second.")
                    { throw new MathsError("MathsError: The number of columns in the first tensor should be equal to the number of rows in second."); }
                }

                throw new SyntaxError("SyntaxError: Invalid Input in 'dot' Function"); //ERROR_TAG
            }

            public IValuable add(Tensor cin, Dictionary<string, IVariable> vars)
            {
                if (!cin.known)
                    throw new MathsError("MathsError: Cannot evaluate unknown variable '" + cin.name + "'"); //ERROR_TAG
                if (!known)
                    throw new MathsError("MathsError: Cannot evaluate unknown variable '" + name + "'"); //ERROR_TAG

                var new_parts = new List<List<Scalar>>();
                if (DimDim == 0 && cin.DimDim == 0)
                {
                    var t1 = new Tensor(new Scalar(/*name + "£add£" + cin.name, */(Value(vars).parts[0][0] as Scalar).output + (cin.Value(vars).parts[0][0] as Scalar).output), false, null, name == null ? cin.name : name);
                    return t1;
                }
                else if (DimDim == 0 ^ cin.DimDim == 0)
                {
                    throw new MathsError("MathsError: You cannot add a scalar to a matrix or a vector"); //ERROR_TAG
                }
                else if (DimDim > 0 && cin.DimDim > 0)
                {
                    try
                    {
                        var product = ToMatrix().matadd(cin.ToMatrix()).array;
                        return new Tensor
                        (
                        product.ConvertAll(r => r.ConvertAll(c => (IValuable)new Scalar(/*name + "£add£" + cin.name,*/ c))),
                        false,
                        Index,
                        name + "£add£" + cin.name
                        );
                    }
                    catch (Exception)
                    { throw new MathsError("MathsError: The rows/columns for these two Tensors must be the same."); }
                }

                throw new SyntaxError("SyntaxError: Invalid Input in 'dot' Function"); //ERROR_TAG
            }
            public Tensor add(Tensor cin)
            {
                if (!cin.known || !known)
                    throw new MathsError("MathsError: Cannot apply function to unknown variable"); //ERROR_TAG

                var new_parts = new List<List<Scalar>>();
                if (DimDim == 0 && cin.DimDim == 0)
                    return new Tensor(new Scalar(/*name + "£add£" + cin.name, */(parts[0][0] as Scalar).output + (cin.parts[0][0] as Scalar).output), false, null, name == null ? cin.name : name);
                else if (DimDim == 0 ^ cin.DimDim == 0)
                {
                    throw new MathsError("MathsError: You cannot add a scalar to a matrix or a vector"); //ERROR_TAG
                }
                else if (DimDim > 0 && cin.DimDim > 0)
                {
                    try
                    {
                        var product = ToMatrix().matadd(cin.ToMatrix()).array;
                        return new Tensor
                        (
                        product.ConvertAll(r => r.ConvertAll(c => (IValuable)new Scalar(/*name + "£add£" + cin.name,*/ c))),
                        false,
                        Index,
                        name + "£add£" + cin.name
                        );
                    }
                    catch (Exception)
                    { throw new MathsError("MathsError: The rows/columns for these two Tensors must be the same."); }
                }

                throw new SyntaxError("SyntaxError: Invalid Input in 'dot' Function"); //ERROR_TAG
            }
        }

        [Serializable]
        protected class Scalar : IValuable//, ISerializable //TODO: Make this work.
        {
            public double? output;
            public bool known { get { return output.HasValue; } }
            //public string name { get; set; }

            //public Scalar(string name)
            //{
            //    output = null;
            //    this.name = name;
            //}
            public Scalar(double? value = null)
            {
                this.output = value;
                //this.name = name;
            }

            /// <summary>
            /// Substitutes all variables for their current/temporary values and calculates the output,
            /// storing it in an output tensor.
            /// </summary>
            /// <param name="vars"></param>
            /// <returns></returns>
            public Tensor Value(Dictionary<string, IVariable> vars)
            {
               // if (vars.Exists(s => s.name == name))
               //     return new Tensor((Scalar)vars.Find(s => s.name == name), false, name);

                return new Tensor(new List<List<IValuable>> { new List<IValuable> { this } }, false);
            }

            //public void GetObjectData(SerializationInfo info, StreamingContext context)
            //{
            //    info.AddValue("name", name);
            //    info.AddValue("GetValue", GetValue);
            //    info.AddValue("output", output);
            //}
        }

        [Serializable]
        protected class Operation : FormulaNodeContainer, IVariable
        {
            public IContainer Container { get; set; }
            public bool Reciprocal { get; set; }
            public List<Formula> Operands;
            public string name { get; set; }
            public bool known => Operands.All(o => 
            {
                try
                {
                    var t1 = o.Value(Globals.values);
                    return t1.known;
                }
                catch (Error err) when (err.Message.Contains("unknown variable") && name.Contains("#"))
                { return false; }
            });
            public Formula Index { get; set; }
            public override relpair CRelation => Container == null ? new relpair() : Container.TRelation; //catch

            [NonSerialized]
            Func<List<Formula>, object, Tensor> Process;
            public object Parameters;

            public Operation(IContainer container, string name, List<Formula> operands, Func<List<Formula>, object, Tensor> func = null, object prms = null, Formula index = null)
            {
                Container = container;
                Operands = operands;
                Process = func;
                Parameters = prms;
                this.name = name;
                Index = index;

                formulaNode = new FormulaNode(this, CRelation);
                for (int i = 0; i < Operands.Count; i++)
                    formulaNode.Add(Operands[i], true);
            }

            /// <summary>
            /// This class has NonSerialized component(s), so a normal dcpy won't do the full trick; this will.
            /// </summary>
            /// <returns></returns>
            public IFormulaic DeepCopy()
            {
                var cout = new Operation
                    (
                    Container, 
                    name, 
                    Operands.ConvertAll(o => o.DeepCopy() as Formula), 
                    Process, 
                    dcpy(Parameters), 
                    Index == null ? null : Index.DeepCopy() as Formula
                    );
                cout.Process = Process;
                return cout;
            }
            object dcpy(object cin)
            {
                switch(cin)
                {
                    case IEnumerable<dynamic> e:
                        return from i in e select dcpy(i);
                    case Tuple<dynamic, dynamic> t://build on this later (number of fields)
                        return new Tuple<dynamic, dynamic>(dcpy(t.Item1), dcpy(t.Item2));
                    case IFormulaic f:
                        return f.DeepCopy();
                    default:
                        return cin == null ? null : Liber.Engine.dcpy(cin);
                }
            }

            public Tensor Value(Dictionary<string, IVariable> vars)
            {
                if (Globals.functions.ContainsKey(name) || known)
                {
                    var Function = Globals.functions.ContainsKey(name) ? Globals.functions[name] : Process;
                    try
                    {
                        var t1 = Function.Invoke(Operands, Parameters);
                        return t1;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Write(e);//ERROR_TAG
                        throw new SyntaxError("SyntaxError: Invalid function parameters.");
                    }
                }
                else
                    throw new MathsError("MathsError: Could not evaluate function because it is undefined.");
            }
        }

        [Serializable]
        protected class Function : IFormulaic, IVariable
        {
            public IContainer Container { get; set; }
            public bool Reciprocal { get; set; }
            public Formula Index { get; set; }
            public string name { get; set; }
            public bool known { get => Process != null; }
            public relpair CRelation => Container == null ? new relpair() : Container.TRelation; //catch

            public Formula Process;
            public List<Formula> Parameters;
            List<string> ParameterNames;

            public Function(IContainer container, string name, List<Formula> prms, Formula value, bool identity = false, Formula index = null)
            {
                this.name = name;
                Container = container;
                Process = value;
                Parameters = prms;
                Index = index;

                if (identity)
                    ParameterNames = (from p in prms select (p[0, 0, true] as Tensor).name).ToList();
                    //(from p in prms //where p.AddedVariables.All(a => a.name != null) && p.AddedVariables.Count() > 0
                    //             select p.AddedVariables.First().name).ToList();
            }

            //public Func<List<Formula>, object, Tensor> function
            //{ get
            //    {
            //        if (Globals.values.ContainsKey(name))
            //            return Globals.values[name];
            //        if (Process == null) throw new MathsError("MathsError: Could not evaluate function because it is undefined.");
            //        return (args, prms) =>
            //            Process.Value(Globals.values.Values.Concat(Parameters.ConvertAll(p => p.Value(Globals.values) as IVariable)).ToDictionary(a => a.name));
            //    }
            //}
            public Tensor Value(Dictionary<string, IVariable> vars)
            {
                if (Globals.values.ContainsKey(name) || known)
                {
                    var f = Globals.values.ContainsKey(name) ? Globals.values[name] as Function : this;
                    List<Tensor> named_tensors;
                    var identity = false;

                    try
                    {
                        var fact_x = false;
                        //calculate the value of the parameters and then give them their localised names before passing them to the function
                        named_tensors = Parameters.ConvertAll(p => //get the value and name them
                        {//don't expand these brackets, I need the compiler to stop asking questions if the name is null
                            fact_x |= !(p.parts.Count == 1 && (p[0] as Term).numerator.Count == 1 &&  //p only has one coefficient in it
                            p[0, 0, true] is IVariable && ((p[0, 0, true] as IVariable).name == null ? false : (p[0, 0, true] as IVariable).name[1] == ':')); //that coefficient has been localised
                            identity = !fact_x;

                            var val = p.Value(Globals.values);
                            var local_name = f.ParameterNames[Parameters.IndexOf(p)];
                            var t = val.known ? val : Globals.values["Key" + local_name.Substring(2)].DeepCopy() as Tensor;
                            t.name = local_name;
                            return t;
                        });
                        //var named_parameters = named_tensors.ConvertAll(t => //create new parameters that are just tensors with the localised names
                        //{
                        //    var f = new Formula(null);
                        //    f.Add(t);
                        //    return f;
                        //});

                        //var inputs = Parameters.ConvertAll(p => p.Value(vars) as IVariable);
                        //var concatenatum = vars.Values.Concat(named_tensors.ConvertAll(p => p as IVariable)).ToDictionary(a => a.name);
                        named_tensors.ForEach(t => Equation.Remember(t)); vars = Globals.values;
                        var cout = f.Process.Value(vars);
                        cout.name = name + "(" + producti.arrprd(named_tensors, (o, i) => o += ", " + i.name.Substring(2), "").Substring(2) + ")";
                        named_tensors.ForEach(t => Globals.values.Remove(t.name));
                        return cout;
                    }
                    catch (Exception e)
                    {
                        if (identity)
                            return new Tensor(Process, Reciprocal);
                        Debug.Write(e);//ERROR_TAG
                        throw new SyntaxError("SyntaxError: Invalid function parameters.");
                    }
                }
                else
                    throw new MathsError("MathsError: Could not evaluate function because it is undefined.");
                
            }
            public Tensor Value(List<Formula> parameters, Dictionary<string, IVariable> vars)
            {
                try
                {
                    var inputs = parameters.ConvertAll(p => p.Value(vars) as IVariable);
                    return Process.Value(vars.Values.Concat(inputs).ToDictionary(a => a.name));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e);//ERROR_TAG
                    throw new SyntaxError("SyntaxError: Invalid function parameters.");
                }
            }

            public IFormulaic DeepCopy()
            {
                var cout = new Function
                    (
                    Container, 
                    name, 
                    Parameters.ConvertAll(p => p.DeepCopy() as Formula), 
                    Process.DeepCopy() as Formula,
                    false,
                    Index == null ? null : Index.DeepCopy() as Formula
                    );
                cout.Reciprocal = Reciprocal;
                return cout;
            }
        }

        [Serializable]
        public class Error : Exception
        {
            public Error() { }
            public Error(string message) : base(message) { }
            public Error(string message, Exception inner) : base(message, inner) { }
            protected Error(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class SyntaxError : Error
        {
            public SyntaxError() { }
            public SyntaxError(string message) : base(message) { }
            public SyntaxError(string message, Exception inner) : base(message, inner) { }
            protected SyntaxError(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class MathsError : Error
        {
            public MathsError() { }
            public MathsError(string message) : base(message) { }
            public MathsError(string message, Exception inner) : base(message, inner) { }
            protected MathsError(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class DevError : Error
        {
            public DevError() { }
            public DevError(string message) : base(message) { }
            public DevError(string message, Exception inner) : base(message, inner) { }
            protected DevError(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}
