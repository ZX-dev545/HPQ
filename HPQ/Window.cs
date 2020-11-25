using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

using Gma.System.MouseKeyHook;
using Liber;

namespace GCalc
{
    public partial class Window : Engine
    {
        /// <summary>
        /// All the global variables and methods
        /// </summary>
        protected static class Globals
        {
            public static Brush MathsBrush = new SolidBrush(SystemColors.ControlText);
            public static Font MathsFont = new Font("XITS", 12, FontStyle.Italic);
            public static Font DataFont = new Font("Arial", 9);
            public static Color GraphingColour = Color.Black;

            public static Color SpecialColour = Color.DarkBlue;
            public static string[] specials = new[] { "Keyd", "Keye", "KeyPi", "Keyi", "Keyj", "Keyk" };
            
            public static double rotation_speed = 0.03;
            public static double panning_speed = 0.05;
            
            public static readonly ComponentResourceManager resources = new ComponentResourceManager(typeof(Window));
            public static Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
            public static Dictionary<string, Func<List<Formula>, object, Tensor>> functions = new Dictionary<string, Func<List<Formula>, object, Tensor>>();
            public static Tensor[] Parameters = new Tensor[4];

            public static string input_pane_key = "";
            public static int KeyPress_count = 0;
            public static int KeyDown_count = 0;
            public static bool shift = false;

            public static Tile FocusedTile;

            public static Dictionary<string, IVariable> values = new Dictionary<string, IVariable>();
        }
        protected struct Symbol
        {
            /// <summary>Image of the Symbol</summary>
            public Image i;
            /// <summary>Distance of the 'floor' of the Symbol from the top.</summary>
            public int f;

            public int offset { get { return i.Height - f; } }
            public int height { get { return f > i.Height ? f : 2 * i.Height - f; } }

            public Symbol(int f, Image i)
            {
                this.f = f;
                this.i = i;
            }
        }
        static void InitialiseGlobals()
        {
            #region Greek symbols
            Globals.symbols.Add("KeyAlpha",   new Symbol(8, (Image)Globals.resources.GetObject("KeyAlpha.Image")));
            Globals.symbols.Add("KeyBeta",    new Symbol(8, (Image)Globals.resources.GetObject("KeyBeta.Image")));
            Globals.symbols.Add("KeyGamma",   new Symbol(8, (Image)Globals.resources.GetObject("KeyGamma.Image")));
            Globals.symbols.Add("KeyDelta",   new Symbol(8, (Image)Globals.resources.GetObject("KeyDelta.Image")));
            Globals.symbols.Add("KeyEpsilon", new Symbol(8, (Image)Globals.resources.GetObject("KeyEpsilon.Image")));
            Globals.symbols.Add("KeyZeta",    new Symbol(8, (Image)Globals.resources.GetObject("KeyZeta.Image")));
            Globals.symbols.Add("KeyEta",     new Symbol(8, (Image)Globals.resources.GetObject("KeyEta.Image")));
            Globals.symbols.Add("KeyTheta1",  new Symbol(8, (Image)Globals.resources.GetObject("KeyTheta1.Image")));
            Globals.symbols.Add("KeyTheta2",  new Symbol(8, (Image)Globals.resources.GetObject("KeyTheta2.Image")));
            Globals.symbols.Add("KeyIota",    new Symbol(8, (Image)Globals.resources.GetObject("KeyIota.Image")));
            Globals.symbols.Add("KeyKappa",   new Symbol(8, (Image)Globals.resources.GetObject("KeyKappa.Image")));
            Globals.symbols.Add("KeyLambda",  new Symbol(8, (Image)Globals.resources.GetObject("KeyLambda.Image")));
            Globals.symbols.Add("KeyMu",      new Symbol(8, (Image)Globals.resources.GetObject("KeyMu.Image")));
            Globals.symbols.Add("KeyNu",      new Symbol(8, (Image)Globals.resources.GetObject("KeyNu.Image")));
            Globals.symbols.Add("KeyXi",      new Symbol(8, (Image)Globals.resources.GetObject("KeyXi.Image")));
            Globals.symbols.Add("KeyPi",      new Symbol(8, (Image)Globals.resources.GetObject("KeyPi.Image")));
            Globals.symbols.Add("KeyRho",     new Symbol(8, (Image)Globals.resources.GetObject("KeyRho.Image")));
            Globals.symbols.Add("KeySigma1",  new Symbol(8, (Image)Globals.resources.GetObject("KeySigma1.Image")));
            Globals.symbols.Add("KeySigma2",  new Symbol(8, (Image)Globals.resources.GetObject("KeySigma2.Image")));
            Globals.symbols.Add("KeyTau",     new Symbol(8, (Image)Globals.resources.GetObject("KeyTau.Image")));
            Globals.symbols.Add("KeyUpsilon", new Symbol(8, (Image)Globals.resources.GetObject("KeyUpsilon.Image")));
            Globals.symbols.Add("KeyPhi1",    new Symbol(8, (Image)Globals.resources.GetObject("KeyPhi1.Image")));
            Globals.symbols.Add("KeyPhi2",    new Symbol(8, (Image)Globals.resources.GetObject("KeyPhi2.Image")));
            Globals.symbols.Add("KeyChi",     new Symbol(8, (Image)Globals.resources.GetObject("KeyChi.Image")));
            Globals.symbols.Add("KeyPsi",     new Symbol(8, (Image)Globals.resources.GetObject("KeyPsi.Image")));
            Globals.symbols.Add("KeyOmega",   new Symbol(8, (Image)Globals.resources.GetObject("KeyOmega.Image")));
            #endregion
            #region Latin symbols
            Globals.symbols.Add("KeyA", new Symbol(12, (Image)Globals.resources.GetObject("KeyAC")));
            Globals.symbols.Add("KeyB", new Symbol(12, (Image)Globals.resources.GetObject("KeyBC")));
            Globals.symbols.Add("KeyC", new Symbol(12, (Image)Globals.resources.GetObject("KeyCC")));
            Globals.symbols.Add("KeyD", new Symbol(12, (Image)Globals.resources.GetObject("KeyDC")));
            Globals.symbols.Add("KeyE", new Symbol(12, (Image)Globals.resources.GetObject("KeyEC")));
            Globals.symbols.Add("KeyF", new Symbol(12, (Image)Globals.resources.GetObject("KeyFC")));
            Globals.symbols.Add("KeyG", new Symbol(12, (Image)Globals.resources.GetObject("KeyGC")));
            Globals.symbols.Add("KeyH", new Symbol(12, (Image)Globals.resources.GetObject("KeyHC")));
            Globals.symbols.Add("KeyI", new Symbol(12, (Image)Globals.resources.GetObject("KeyIC")));
            Globals.symbols.Add("KeyJ", new Symbol(12, (Image)Globals.resources.GetObject("KeyJC")));
            Globals.symbols.Add("KeyK", new Symbol(12, (Image)Globals.resources.GetObject("KeyKC")));
            Globals.symbols.Add("KeyL", new Symbol(12, (Image)Globals.resources.GetObject("KeyLC")));
            Globals.symbols.Add("KeyM", new Symbol(12, (Image)Globals.resources.GetObject("KeyMC")));
            Globals.symbols.Add("KeyN", new Symbol(12, (Image)Globals.resources.GetObject("KeyNC")));
            Globals.symbols.Add("KeyO", new Symbol(12, (Image)Globals.resources.GetObject("KeyOC")));
            Globals.symbols.Add("KeyP", new Symbol(12, (Image)Globals.resources.GetObject("KeyPC")));
            Globals.symbols.Add("KeyQ", new Symbol(12, (Image)Globals.resources.GetObject("KeyQC")));
            Globals.symbols.Add("KeyR", new Symbol(12, (Image)Globals.resources.GetObject("KeyRC")));
            Globals.symbols.Add("KeyS", new Symbol(12, (Image)Globals.resources.GetObject("KeySC")));
            Globals.symbols.Add("KeyT", new Symbol(12, (Image)Globals.resources.GetObject("KeyTC")));
            Globals.symbols.Add("KeyU", new Symbol(12, (Image)Globals.resources.GetObject("KeyUC")));
            Globals.symbols.Add("KeyV", new Symbol(12, (Image)Globals.resources.GetObject("KeyVC")));
            Globals.symbols.Add("KeyW", new Symbol(12, (Image)Globals.resources.GetObject("KeyWC")));
            Globals.symbols.Add("KeyX", new Symbol(12, (Image)Globals.resources.GetObject("KeyXC")));
            Globals.symbols.Add("KeyY", new Symbol(12, (Image)Globals.resources.GetObject("KeyYC")));
            Globals.symbols.Add("KeyZ", new Symbol(12, (Image)Globals.resources.GetObject("KeyZC")));

            Globals.symbols.Add("Keya", new Symbol(8,  (Image)Globals.resources.GetObject("Keya")));
            Globals.symbols.Add("Keyb", new Symbol(13, (Image)Globals.resources.GetObject("Keyb")));
            Globals.symbols.Add("Keyc", new Symbol(8,  (Image)Globals.resources.GetObject("Keyc")));
            Globals.symbols.Add("Keyd", new Symbol(13, (Image)Globals.resources.GetObject("Keyd")));
            Globals.symbols.Add("Keye", new Symbol(8,  (Image)Globals.resources.GetObject("Keye")));
            Globals.symbols.Add("Keyf", new Symbol(13, (Image)Globals.resources.GetObject("Keyf")));
            Globals.symbols.Add("Keyg", new Symbol(8,  (Image)Globals.resources.GetObject("Keyg")));
            Globals.symbols.Add("Keyh", new Symbol(8,  (Image)Globals.resources.GetObject("Keyh")));
            Globals.symbols.Add("Keyi", new Symbol(12,  (Image)Globals.resources.GetObject("Keyi")));
            Globals.symbols.Add("Keyj", new Symbol(8,  (Image)Globals.resources.GetObject("Keyj")));
            Globals.symbols.Add("Keyk", new Symbol(13, (Image)Globals.resources.GetObject("Keyk")));
            Globals.symbols.Add("Keyl", new Symbol(13, (Image)Globals.resources.GetObject("Keyl")));
            Globals.symbols.Add("Keym", new Symbol(8,  (Image)Globals.resources.GetObject("Keym")));
            Globals.symbols.Add("Keyn", new Symbol(8,  (Image)Globals.resources.GetObject("Keyn")));
            Globals.symbols.Add("Keyo", new Symbol(8,  (Image)Globals.resources.GetObject("Keyo")));
            Globals.symbols.Add("Keyp", new Symbol(8,  (Image)Globals.resources.GetObject("Keyp")));
            Globals.symbols.Add("Keyq", new Symbol(8,  (Image)Globals.resources.GetObject("Keyq")));
            Globals.symbols.Add("Keyr", new Symbol(8,  (Image)Globals.resources.GetObject("Keyr")));
            Globals.symbols.Add("Keys", new Symbol(8,  (Image)Globals.resources.GetObject("Keys")));
            Globals.symbols.Add("Keyt", new Symbol(8,  (Image)Globals.resources.GetObject("Keyt")));
            Globals.symbols.Add("Keyu", new Symbol(8,  (Image)Globals.resources.GetObject("Keyu")));
            Globals.symbols.Add("Keyv", new Symbol(8,  (Image)Globals.resources.GetObject("Keyv")));
            Globals.symbols.Add("Keyw", new Symbol(8,  (Image)Globals.resources.GetObject("Keyw")));
            Globals.symbols.Add("Keyx", new Symbol(8,  (Image)Globals.resources.GetObject("Keyx")));
            Globals.symbols.Add("Keyy", new Symbol(8,  (Image)Globals.resources.GetObject("Keyy")));
            Globals.symbols.Add("Keyz", new Symbol(8,  (Image)Globals.resources.GetObject("Keyz")));
            #endregion
            #region Mathematical symbols
            //BODMAS
            Globals.symbols.Add("+", new Symbol(12,  (Image)Globals.resources.GetObject("Plus")));
            Globals.symbols.Add("-", new Symbol(5,  (Image)Globals.resources.GetObject("Minus")));
            Globals.symbols.Add("*", new Symbol(6,  (Image)Globals.resources.GetObject("Cross")));
            Globals.symbols.Add("/", new Symbol(8,  (Image)Globals.resources.GetObject("Divide")));
            Globals.symbols.Add("(", new Symbol(14, (Image)Globals.resources.GetObject("LeftBracket")));
            Globals.symbols.Add(")", new Symbol(14, (Image)Globals.resources.GetObject("RightBracket")));
            Globals.symbols.Add("=", new Symbol(8, (Image)Globals.resources.GetObject("EqualSign")));
            Globals.symbols.Add(">", new Symbol(8, (Image)Globals.resources.GetObject("GreaterThan")));
            Globals.symbols.Add("<", new Symbol(8, (Image)Globals.resources.GetObject("LessThan")));
            Globals.symbols.Add(">=", new Symbol(8, (Image)Globals.resources.GetObject("GreaterThanOrEqualTo")));
            Globals.symbols.Add("<=", new Symbol(8, (Image)Globals.resources.GetObject("LessThanOrEqualTo")));
            Globals.symbols.Add(",", new Symbol(2, (Image)Globals.resources.GetObject("Comma")));
            //scalar, represented by the [1] tag
            Globals.symbols.Add("[1](big)#summation", new Symbol((Globals.resources.GetObject("Summation") as Image).Height, (Image)Globals.resources.GetObject("Summation")));
            Globals.symbols.Add("[1](big)#piproduct", new Symbol((Globals.resources.GetObject("PiProduct") as Image).Height, (Image)Globals.resources.GetObject("PiProduct")));
            //calculus, represented by the [2] tag
            Globals.symbols.Add("[2](big)integral",  new Symbol((Globals.resources.GetObject("Integral") as Image).Height,        (Image)Globals.resources.GetObject("Integral")));
            Globals.symbols.Add("[2](big)cintegral", new Symbol((Globals.resources.GetObject("ContourIntegral") as Image).Height, (Image)Globals.resources.GetObject("ContourIntegral")));
            Globals.symbols.Add("[2](non)del",       new Symbol((Globals.resources.GetObject("Nabla") as Image).Height,           (Image)Globals.resources.GetObject("Nabla")));
            //matrix/vector
            //numbers
            Globals.symbols.Add("1", new Symbol(12, (Image)Globals.resources.GetObject("1")));
            Globals.symbols.Add("2", new Symbol(12, (Image)Globals.resources.GetObject("2")));
            Globals.symbols.Add("3", new Symbol(12, (Image)Globals.resources.GetObject("3")));
            Globals.symbols.Add("4", new Symbol(12, (Image)Globals.resources.GetObject("4")));
            Globals.symbols.Add("5", new Symbol(12, (Image)Globals.resources.GetObject("5")));
            Globals.symbols.Add("6", new Symbol(12, (Image)Globals.resources.GetObject("6")));
            Globals.symbols.Add("7", new Symbol(12, (Image)Globals.resources.GetObject("7")));
            Globals.symbols.Add("8", new Symbol(12, (Image)Globals.resources.GetObject("8")));
            Globals.symbols.Add("9", new Symbol(12, (Image)Globals.resources.GetObject("9")));
            Globals.symbols.Add("0", new Symbol(12, (Image)Globals.resources.GetObject("0")));
            Globals.symbols.Add(".", new Symbol(2 , (Image)Globals.resources.GetObject(".")));
            #endregion

            #region Scalar Functions
            //does all of the tangly conversions so that adding a function becomes less effort
            Func<List<Formula>, Func<double[], double>, Tensor> scalar = (tensor, func) =>
            {
                var rpl = tensor[0].Container is FormulaNodeContainer.FormulaNode ?
                    ((tensor[0].Container as FormulaNodeContainer.FormulaNode).parent as IFormulaic).Reciprocal : false;
                if (tensor.All(t => t.Value(Globals.values).known))
                {
                    var t1 = tensor.Select(t => (t.Value(Globals.values).parts[0][0] as Scalar).output.Value).ToArray();
                    var t2 = func(t1);
                    return new Tensor(new Scalar(t2), rpl);
                }
                else
                    return new Tensor(new Scalar(), rpl);
            };

            //scalar, represented by the [1] tag
            Globals.functions.Add("[1](sml)sqrt", (args, prms) => scalar(args, x => Math.Sqrt(x[0])));
            Globals.functions.Add("[1](sml)sin", (args, prms) => scalar(args, x => Math.Sin(x[0])));
            Globals.functions.Add("[1](sml)cos", (args, prms) => scalar(args, x => Math.Cos(x[0])));
            Globals.functions.Add("[1](sml)tan", (args, prms) => scalar(args, x => Math.Tan(x[0])));
            Globals.functions.Add("[1](sml)cot", (args, prms) => scalar(args, x => 1 / Math.Tan(x[0])));
            Globals.functions.Add("[1](sml)sec", (args, prms) => scalar(args, x => 1 / Math.Cos(x[0])));
            Globals.functions.Add("[1](sml)csc", (args, prms) => scalar(args, x => 1 / Math.Sin(x[0])));
            Globals.functions.Add("[1](sml)log", (args, prms) => scalar(args, x => Math.Log(x[0]) / Math.Log(x[1])));
            Globals.functions.Add("[1](sml)ln", (args, prms) => scalar(args, x => Math.Log(x[0])));
            Globals.functions.Add("[1](big)#summation", (args, prms) => 
            {
                //The parameters are casted into their true datatype
                var eq = prms as Tuple<Formula, List<Equation>>;
                //The iterator function is just the operand, but it takes the index as a parameter, meaning I can substitute 
                //the index variable for the parameter that I enter into the iterator function in the for loop.
                var iterator = new Function(null, "iterator", new List<Formula>(), args[0]);
                //initial value of the index is the solution to the equation underneath the built-in iterating function, stored in eq.Item2.
                var n = eq.Item2[0].SolveFor((eq.Item2[0].parts[0] as IVariableContainer).DeepVariables.Where(v => v != null && !v.known && v.name != null).First(), Globals.values) as Tensor;
                //index_formula will be passed as the parameter of iterator.
                var index_formula = new Formula(null);
                index_formula.Add(n);

                var cout = iterator.Value(new List<Formula> { index_formula }, Globals.values);
                for (n = n.add(new Tensor(new Scalar(1), false)); (n.parts[0][0] as Scalar).output <= (eq.Item1.Value(Globals.values).parts[0][0] as Scalar).output.Value; n = n.add(new Tensor(new Scalar(1), false)))
                {
                    //update index_formula so that the next index is passed as the parameter of iterator
                    (index_formula.parts[0] as Term).numerator[0] = n;//new Tensor(new Scalar(n), false);
                    //the actual addition part
                    cout = cout.add(iterator.Value(new List<Formula> { index_formula }, Globals.values));
                }

                return cout;
            });//iterator functions have the # tag
            Globals.functions.Add("[1](big)#piproduct", (args, prms) => 
            {
                //The parameters are casted into their true datatype
                var eq = prms as Tuple<Formula, List<Equation>>;
                //The iterator function is just the operand, but it takes the index as a parameter, meaning I can substitute 
                //the index variable for the parameter that I enter into the iterator function in the for loop.
                var iterator = new Function(null, "iterator", new List<Formula>(), args[0]);
                //initial value of the index is the solution to the equation underneath the built-in iterating function, stored in eq.Item2.
                var n = eq.Item2[0].SolveFor((eq.Item2[0].parts[0] as IVariableContainer).DeepVariables.Where(v => v != null && !v.known && v.name != null).First(), Globals.values) as Tensor;
                //index_formula will be passed as the parameter of iterator.
                var index_formula = new Formula(null);
                index_formula.Add(n);

                var cout = iterator.Value(new List<Formula> { index_formula }, Globals.values);
                for (n = n.add(new Tensor(new Scalar(1), false)); (n.parts[0][0] as Scalar).output <= (eq.Item1.Value(Globals.values).parts[0][0] as Scalar).output.Value; n = n.add(new Tensor(new Scalar(1), false)))
                {
                    //update index_formula so that the next index is passed as the parameter of iterator
                    (index_formula.parts[0] as Term).numerator[1] = n;//new Tensor(new Scalar(n), false);
                    //the actual addition part
                    cout = cout.dot(iterator.Value(new List<Formula> { index_formula }, Globals.values));
                }

                return cout;
            });
            #endregion
            #region Calculus Functions

            //calculus, representeed by the [2] tag
            Globals.functions.Add("[2](big)integral", (args, prms) =>
            {
                var eq = prms as IFormulaic[];
                var variables = args[0].Variables.ToList();
                var couts = new[] { args[0].DeepCopy() as Formula, args[0].DeepCopy() as Formula };

                var indefinite = (eq[0].Value(Globals.values).parts[0][0] as Scalar).output == 0 && 
                (eq[1].Value(Globals.values).parts[0][0] as Scalar).output == 0;
                
                var variable_of_integration = eq[2] as Tensor;
                if (variable_of_integration == null)
                    throw new SyntaxError("SyntaxError: The variable of integration has to be a number");
                var s_val = variable_of_integration.Value(Globals.values);
                
                for (int i = 0; i < (indefinite ? 1 : 2); i++)
                {
                    var t3 = couts[i].parts[0].Container == couts[i];
                    var suband = eq[i].Value(Globals.values);
                    var exter = indefinite ? s_val.DeepCopy() as Tensor : suband;
                    couts[i].ConvertVariables(s =>//Bow down to the One true one-liner, for He is the LORD; the One to rule over all in Glory.
                    {
                        var t10 = s.Container.parts.Contains(s);
                        //sub the integrand parameter
                        if (variable_of_integration.name == s.name)
                        {
                            if (indefinite && !s_val.known)
                                return new Tuple<IVariable, Func<int, int>>(s, p => p++);
                            
                            exter.name = (indefinite ? "" : "i:") + s.name + "_integrated";//make sure that its value in the context of being in an integral doesn't default to its parametric value
                            exter.Container = s.Container;
                            //increment index
                            if (exter.Index == null)
                            {
                                exter.Index = new Formula(null);
                                exter.Index.Add(new Term(s.Index, false));//add a new term to the index
                                (exter.Index.parts.Last() as Term).numerator.Add(new Tensor(new Scalar(1), false));//make that term 1
                            }
                            var index = exter.Index;
                            exter.Index.Add(new Term(exter.Index, false));//add a new term to the index
                            (exter.Index.parts.Last() as Term).numerator.Add(new Tensor(new Scalar(1), false));//make that term 1

                            //scale denominator by index
                            var new_d = new Formula(s.Container);//brackets
                            new_d.Add(new Term(new_d, false)//add the old denominator into the brackets
                            {
                                numerator = (exter.Container as Term).denominator.ConvertAll(d =>
                                    { d.Reciprocal = false; return d; })
                            });
                            new_d.Add(exter.Index.Value(Globals.values));//scale it by the new index
                            (exter.Container as Term).denominator = new List<IFormulaic> { new Tensor(new_d, false, null, null, exter.Container) };//replace the old denominator with the new one

                            exter = exter.pow(exter.Index, Globals.values);
                            //'s' is a reference type (a pointer). Plainly reassigning to 's' will reassign the pointer to the 
                            //address of the output of 'eq[i].Value(Globals.values)', and then 's.Container.parts' will no longer
                            //contain a pointer to the same address that 's' would now point to (it still contains the old 
                            //address that 's' will use to have pointed to), resulting in s.Container denying that it contains 
                            //'s'. TL;DR Don't fix this next line.
                            if ((s.Container as Term).numerator.Contains(s))
                                (s.Container as Term).numerator[(s.Container as Term).numerator.IndexOf(s)] = exter;
                            if ((s.Container as Term).denominator.Contains(s))
                                (s.Container as Term).denominator[(s.Container as Term).denominator.IndexOf(s)] = exter;
                            s = exter;
                        }
                        //sub embedded functions
                        var inc = 1;
                        if (s is Operation)
                        {
                            var subops = (s as Operation).Operands.ConvertAll(o =>
                            {
                                o.substitute(variable_of_integration.name, exter);
                                o.ConvertAllVariables(v =>
                                {
                                    if (variable_of_integration.name == v.name)
                                        v.name = "written:" + v.name;
                                    return v;
                                });
                                return o;
                            });
                            switch (s.name)
                            {
                                #region trigonometric
                                case "[1](sml)sin":
                                    var op1 = new Operation(s.Container,
                                        "[1](sml)cos", subops, Globals.functions["[1](sml)cos"], (s as Operation).Parameters);
                                    var te1 = op1.Value(Globals.values);

                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te1;
                                        (s.Container as Term).denominator.Insert(s.Container.parts.IndexOf(te1), new Tensor(new Scalar(-1), true, null, null, s.Container));
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te1;
                                        (s.Container as Term).numerator.Insert(s.Container.parts.IndexOf(te1), new Tensor(new Scalar(-1), false, null, null, s.Container));
                                    }
                                    inc++;
                                    s = te1;
                                    break;
                                case "[1](sml)cos":
                                    var op2 = new Operation(s.Container,
                                        "[1](sml)sin", subops, Globals.functions["[1](sml)sin"], (s as Operation).Parameters);
                                    var te2 = op2.Value(Globals.values);
                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te2;
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te2;
                                    }
                                    s = te2;
                                    break;
                                case "[1](sml)tan":
                                    var opi3 = new Formula(null);
                                    opi3.Add(new Operation(null, "opi", subops, (a, p) =>
                                    {
                                        var cout = Globals.functions["[1](sml)sec"](a, p);
                                        cout.ConvertAllParts(b => new Scalar(Math.Abs((b as Scalar).output.Value)));
                                        return cout;
                                    }, (s as Operation).Parameters));
                                

                                    var op3 = new Operation(s.Container, "[1](sml)ln", new List<Formula> { opi3 }, Globals.functions["[1](sml)ln"], (s as Operation).Parameters);
                                    var te3 = op3.Value(Globals.values);
                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te3;
                                        (s.Container as Term).denominator.Insert(s.Container.parts.IndexOf(te3), new Tensor(new Scalar(-1), true, null, null, s.Container));
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te3;
                                        (s.Container as Term).numerator.Insert(s.Container.parts.IndexOf(te3), new Tensor(new Scalar(-1), false, null, null, s.Container));
                                    }
                                    inc++;
                                    s = te3;
                                    break;
                                case "[1](sml)cot":
                                    var opi4 = new Formula(null);
                                    opi4.Add(new Operation(null, "opi", subops, (a, p) =>
                                    {
                                        var cout = Globals.functions["[1](sml)sec"](a, p);
                                        cout.ConvertAllParts(b => new Scalar(Math.Abs((b as Scalar).output.Value)));
                                        return cout;
                                    }, (s as Operation).Parameters));

                                    var op4 = new Operation(s.Container, "[1](sml)ln", new List<Formula> { opi4 }, Globals.functions["[1](sml)ln"], (s as Operation).Parameters);
                                    var te4 = op4.Value(Globals.values);
                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te4;
                                        (s.Container as Term).denominator.Insert(s.Container.parts.IndexOf(te4), new Tensor(new Scalar(-1), true, null, null, s.Container));
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te4;
                                        (s.Container as Term).numerator.Insert(s.Container.parts.IndexOf(te4), new Tensor(new Scalar(-1), false, null, null, s.Container));
                                    }
                                    inc++;
                                    s = te4;
                                    break;
                                case "[1](sml)sec":
                                    var opi5 = new Formula(null);
                                    opi5.Add(new Operation(null, "opi", subops, (a, p) =>
                                    {
                                        var cout = Globals.functions["[1](sml)sec"](a, p).add(Globals.functions["[1](sml)tan"](a, p));
                                        cout.ConvertAllParts(b => new Scalar(Math.Abs((b as Scalar).output.Value)));
                                        return cout;
                                    }, (s as Operation).Parameters));

                                    var op5 = new Operation(s.Container, "[1](sml)ln", new List<Formula> { opi5 }, Globals.functions["[1](sml)ln"], (s as Operation).Parameters);
                                    var te5 = op5.Value(Globals.values);
                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te5;
                                        (s.Container as Term).denominator.Insert(s.Container.parts.IndexOf(te5), new Tensor(new Scalar(-1), true, null, null, s.Container));
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te5;
                                        (s.Container as Term).numerator.Insert(s.Container.parts.IndexOf(te5), new Tensor(new Scalar(-1), false, null, null, s.Container));
                                    }
                                    inc++;
                                    s = te5;
                                    break;
                                case "[1](sml)csc":
                                    var opi6 = new Formula(null);
                                    opi6.Add(new Operation(null, "opi", subops, (a, p) =>
                                    {
                                        var cout = Globals.functions["[1](sml)cot"](a, p).add(Globals.functions["[1](sml)csc"](a, p));
                                        cout.ConvertAllParts(b => new Scalar(Math.Abs((b as Scalar).output.Value)));
                                        return cout;
                                    }, (s as Operation).Parameters));
                                    
                                    var op6 = new Operation(s.Container, "[1](sml)ln", new List<Formula> { opi6 }, Globals.functions["[1](sml)ln"], (s as Operation).Parameters);
                                    var te6 = op6.Value(Globals.values);
                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te6;
                                        (s.Container as Term).denominator.Insert(s.Container.parts.IndexOf(te6), new Tensor(new Scalar(-1), true, null, null, s.Container));
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te6;
                                        (s.Container as Term).numerator.Insert(s.Container.parts.IndexOf(te6), new Tensor(new Scalar(-1), false, null, null, s.Container));
                                    }
                                    inc++;
                                    s = te6;
                                    break;
                                #endregion
                                #region logarithmic
                                case "[1](sml)log":
                                    var opi7 = new Formula(null);
                                    opi7.Add(new Operation(null, "opi", subops, (a, p) =>
                                    {
                                        var e_as_formula = new Formula(null);
                                        e_as_formula.Add(new Tensor(new Scalar(Math.E), false));
                                        var fs = new List<Formula> { a[0], e_as_formula };
                                        var cout = Globals.functions["[1](sml)log"](a, p).add(Globals.functions["[1](sml)log"](fs, p).VectorInv() as Tensor).dot(a[0].Value(Globals.values));
                                        cout.ConvertAllParts(b => new Scalar(Math.Abs((b as Scalar).output.Value)));
                                        return cout;
                                    }, (s as Operation).Parameters));

                                    var te7 = opi7.Value(Globals.values);
                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te7;
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te7;
                                    }
                                    s = te7;
                                    break;
                                case "[1](sml)ln":
                                    var opi8 = new Formula(null);
                                    opi8.Add(new Operation(null, "opi", subops, (a, p) =>
                                    {
                                        var e_as_formula = new Formula(null);
                                        e_as_formula.Add(new Tensor(new Scalar(Math.E), false));
                                        var fs = new List<Formula> { a[0], e_as_formula };
                                        var a_value = a[0].Value(Globals.values);
                                        var cout = Globals.functions["[1](sml)ln"](a, p).dot(a_value).add(a_value.VectorInv());
                                        cout.ConvertAllParts(b => new Scalar(Math.Abs((b as Scalar).output.Value)));
                                        return cout;
                                    }, (s as Operation).Parameters));

                                    var te8 = opi8.Value(Globals.values);
                                    if (s.Reciprocal)
                                    {
                                        (s.Container as Term).denominator[s.Container.parts.IndexOf(s)] = te8;
                                    }
                                    else
                                    {
                                        (s.Container as Term).numerator[s.Container.parts.IndexOf(s)] = te8;
                                    }
                                    s = te8;
                                    break;
                                    #endregion
                            }
                        }

                        return new Tuple<IVariable, Func<int, int>>(s, p => p += inc);
                    }); //He will return in Glory to judge the living and the dead, and the martyr shall enjoy the pleasure of 154 virgins in paradise
                }

                var t20 = couts[0].Value(Globals.values);
                var t21 = couts[1].Value(Globals.values);
                return indefinite ? couts[0].Value(Globals.values) : couts[0].Value(Globals.values).add(couts[1].Value(Globals.values).VectorInv());
                //cout.name = //The output of a function is just a plain old nameless tensor, regardless of the input. What about an integral of another function? How does the integral know?
            });

            #endregion
        }

        public Window()
        {
            InitialiseGlobals();
            InitializeComponent();
            InitialiseTilePane();
            InitialiseInputPane();
            InitialiseGraphPanel();
            InitialiseEventHandlers();

            SubscribeApplication();
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            Unsubscribe();
        }

        #region MouseKeyHook methods

        IKeyboardMouseEvents The_Events;

        private void SubscribeApplication()
        {
            Unsubscribe();
            Subscribe(Hook.AppEvents());
        }

        private void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }

        private void Subscribe(IKeyboardMouseEvents events)
        {
            The_Events = events;
            The_Events.MouseUp += OnMouseUp;
            The_Events.MouseWheel += OnMouseWheel;
            The_Events.KeyDown += OnKeyDown;
            The_Events.KeyPress += OnKeyPress;
            The_Events.KeyUp += OnKeyUp;
        }

        private void Unsubscribe()
        {
            if (The_Events == null) return;

            The_Events.MouseUp -= OnMouseUp;
            The_Events.MouseWheel -= OnMouseWheel;
            The_Events.KeyDown -= OnKeyDown;
            The_Events.KeyPress -= OnKeyPress;
            The_Events.KeyUp -= OnKeyUp;

            The_Events.Dispose();
            The_Events = null;
        }

        bool IsTyping;

        #endregion

        #region Form dragging event handlers

        Point MousePosInital;
        Point MousePosFinal;

        private void OnBannerMouseDown(object sender, MouseEventArgs e)
        {
            Unsubscribe();
            SubscribeGlobal();
            MousePosInital = Cursor.Position;
            FormMovingTimer.Start();
        }
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            FormMovingTimer.Stop();
            ParamSldrThmbMovingTimer.Stop();
            GraphPanTimer.Stop();

            Unsubscribe();
            SubscribeApplication();
        }
        private void OnFormMovingTick(object sender, EventArgs e)
        {
            Location = new Point(Location.X + Cursor.Position.X - MousePosInital.X, Location.Y + Cursor.Position.Y - MousePosInital.Y);
            MousePosInital = Cursor.Position;
        }
        #endregion

        #region Banner

        #region close button

        Brush closeBrush = new SolidBrush(Color.White);

        private void CloseButtonPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(closeBrush, 3), new Point(4, 4), new Point(20, 20));
            e.Graphics.DrawLine(new Pen(closeBrush, 3), new Point(20, 4), new Point(4, 20));
        }

        private void CloseMouseHover(object sender, EventArgs e)
        {
            closeBrush = new SolidBrush(Color.Red);
            CloseButton.Invalidate();
        }

        private void CloseMouseLeave(object sender, EventArgs e)
        {
            closeBrush = new SolidBrush(Color.White);
            CloseButton.Invalidate();
        }

        private void OnClose(object sender, EventArgs e)
        {
            closeBrush = new SolidBrush(Color.White);
            CloseButton.Invalidate();
            Close();
        }

        #endregion

        #region maximise button

        Brush maximiseBrush = new SolidBrush(Color.White);

        private void MaximisePaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(maximiseBrush, 3), new Point(4, 4), new Point(20, 4));
            e.Graphics.DrawLine(new Pen(maximiseBrush, 3), new Point(4, 20), new Point(20, 20));
            e.Graphics.DrawLine(new Pen(maximiseBrush, 1), new Point(4, 4), new Point(4, 20));
            e.Graphics.DrawLine(new Pen(maximiseBrush, 1), new Point(20, 4), new Point(20, 20));

            e.Graphics.DrawLine(new Pen(maximiseBrush, 3), new Point(20, 4), new Point(4, 4));
            e.Graphics.DrawLine(new Pen(maximiseBrush, 3), new Point(20, 20), new Point(4, 20));
        }

        private void MaximiseMouseHover(object sender, EventArgs e)
        {
            maximiseBrush = new SolidBrush(Color.Blue);
            MaximiseButton.Invalidate();
        }

        private void MaximiseMouseLeave(object sender, EventArgs e)
        {
            maximiseBrush = new SolidBrush(Color.White);
            MaximiseButton.Invalidate();
        }

        private void OnMaximise(object sender, EventArgs e)
        {
            maximiseBrush = new SolidBrush(Color.White);
            MaximiseButton.Invalidate();

            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else
                WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region minimise button

        Brush minimiseBrush = new SolidBrush(Color.White);

        private void MinimisePaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(minimiseBrush, 4), new Point(4, 11), new Point(20, 11));
        }

        private void MinimiseMouseHover(object sender, EventArgs e)
        {
            minimiseBrush = new SolidBrush(Color.Blue);
            MinimiseButton.Invalidate();
        }

        private void MinimiseMouseLeave(object sender, EventArgs e)
        {
            minimiseBrush = new SolidBrush(Color.White);
            MinimiseButton.Invalidate();
        }

        private void OnMinimise(object sender, EventArgs e)
        {
            minimiseBrush = new SolidBrush(Color.White);
            MinimiseButton.Invalidate();

            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            else
                WindowState = FormWindowState.Minimized;
        }

        #endregion

        private void OnSettingsBanner(object sender, EventArgs e)
        {
            //open the settings GUI
        }

        private void OnStyleBtnColour(object sender, EventArgs e)
        {
            StyleColorDialog.ShowDialog();
            StyleBtnColour.BackColor = StyleColorDialog.Color;
            Globals.GraphingColour = StyleColorDialog.Color;
        }

        #endregion

        #region Tile Pane
        
        void InitialiseTilePane()
        {
            IsTyping = false;

            TilePaneScroll.Size = new Size(17, TilePane.Height);
            TilePaneScroll.Location = new Point(TilePane.Width - TilePaneScroll.Width, 0);
            TilePaneScroll.Dock = DockStyle.Right;
            TilePaneScroll.Scroll += OnTilePaneScroll;
            TilePaneScroll.Maximum = 1;
            TilePane.Controls.Add(TilePaneScroll);
            
            TilePane.Controls.Add(new Tile(TilePane, new Point(0, 0)));
        }

        VScrollBar TilePaneScroll = new VScrollBar();

        void OnTilePaneScroll(object sender, ScrollEventArgs e)
        {
            for (int i = 1; i < TilePane.Controls.Count; i++)
            {
                TilePane.Controls[i].Top = ((i - 1) / 3) * Tile.Size.Height - (int)(((TilePane.Controls.Count - 1.0) * Tile.Size.Height / 3 - TilePane.Height) * 10 * e.NewValue / TilePaneScroll.Maximum);
            }
        }

        #endregion

        #region Input Pane

        Dictionary<Button, int[]>[] RankGroups =
        {
            new Dictionary<Button, int[]>(), //ranks of scalar function buttons
            new Dictionary<Button, int[]>(), //ranks of calculus function buttons
            new Dictionary<Button, int[]>()  //ranks of tensor function buttons
        };

        void InitialiseInputPane()
        {
            #region Greek Key Event Handlers
            KeyAlpha.Click += OnInputBtnKey;
            KeyBeta.Click += OnInputBtnKey;
            KeyGamma.Click += OnInputBtnKey;
            KeyDelta.Click += OnInputBtnKey;
            KeyEpsilon.Click += OnInputBtnKey;
            KeyZeta.Click += OnInputBtnKey;
            KeyEta.Click += OnInputBtnKey;
            KeyTheta1.Click += OnInputBtnKey;
            KeyTheta2.Click += OnInputBtnKey;
            KeyIota.Click += OnInputBtnKey;
            KeyKappa.Click += OnInputBtnKey;
            KeyLambda.Click += OnInputBtnKey;
            KeyMu.Click += OnInputBtnKey;
            KeyNu.Click += OnInputBtnKey;
            KeyPi.Click += OnInputBtnKey;
            KeyRho.Click += OnInputBtnKey;
            KeyChi.Click += OnInputBtnKey;
            KeySigma1.Click += OnInputBtnKey;
            KeySigma2.Click += OnInputBtnKey;
            KeyTau.Click += OnInputBtnKey;
            KeyUpsilon.Click += OnInputBtnKey;
            KeyPhi1.Click += OnInputBtnKey;
            KeyPhi2.Click += OnInputBtnKey;
            KeyXi.Click += OnInputBtnKey;
            KeyPsi.Click += OnInputBtnKey;
            KeyOmega.Click += OnInputBtnKey;
            #endregion

            Panel[] FuncGroups = { InputPanelScalar, InputPanelCalculus, InputPanelTensor };

            for (int i = 0; i < 3; i++)
            {
                //get all symbols with the current [i] tag
                var button_symbols = Globals.symbols.Where(s => 
                {
                    int index;
                    return s.Key.IndexOf('[') != -1 && int.TryParse(s.Key[s.Key.IndexOf('[') + 1] + "", out index) && index == i + 1;
                }).ToList();
                
                for (int j = button_symbols.Count; j > 0; j--)
                {
                    var InputBtnFunc = new Button();

                    InputBtnFunc.BackColor = SystemColors.ControlLight;
                    InputBtnFunc.FlatStyle = FlatStyle.Popup;
                    InputBtnFunc.Location = new Point(50 * (button_symbols.Count - j), 0);
                    InputBtnFunc.Name = button_symbols[j - 1].Key;//.Substring(3); //remove function type tag as it is now useless
                    InputBtnFunc.Image = button_symbols[j - 1].Value.i;
                    InputBtnFunc.Size = new Size(50, 58);
                    InputBtnFunc.TabIndex = 1;
                    InputBtnFunc.Text = j + " : " + j;
                    InputBtnFunc.UseVisualStyleBackColor = false;
                    InputBtnFunc.Click += OnInputBtnFunc;

                    FuncGroups[i].Controls.Add(InputBtnFunc);
                    RankGroups[i].Add(InputBtnFunc, new[] { j, j }); //[0] relative rank [1] popularity
                }
            }
        }

        void OnInputBtnKey(object sender, EventArgs e)
        {
            var InputBtnKey = sender as Button;

            Globals.input_pane_key = InputBtnKey.Name;

            foreach (Control c in TilePane.Controls)
            {
                if (c == Globals.FocusedTile)
                {
                    (c as Tile).FromInputBtnKey(InputBtnKey.Name);
                    (c as Tile).OnClick(c as Tile, e);
                }
            }
        }
        void OnInputBtnFunc(object sender, EventArgs e)
        {
            var InputBtnFunc = sender as Button;

            //if (InputBtnFunc.Left == 0) { return; }

            Globals.input_pane_key = InputBtnFunc.Name;

            foreach (Control c in TilePane.Controls)
            {
                if (c == Globals.FocusedTile)
                {
                    (c as Tile).FromInputBtnFunc(InputBtnFunc.Name);
                    (c as Tile).OnClick(c as Tile, e);
                }
            }

            InputBtnFunc.Text = (int.Parse(InputBtnFunc.Text.Substring(0, InputBtnFunc.Text.IndexOf(' '))) + 1) +
                InputBtnFunc.Text.Substring(InputBtnFunc.Text.IndexOf(' '));

            //int new_rank;
            //KeyValuePair<Button, int[]> overtaken_button;

            for (int i = 0; i < RankGroups.Count(); i++)
            {
                try
                {
                    RankGroups[i][InputBtnFunc][1]++;
                    var order = sort.qsort(RankGroups[i].ToList(), (a, b) => a.Value[1] < b.Value[1]);
                    for (int j = 0; j < RankGroups[i].Count; j++)
                        order[j].Key.Left = 50 * (RankGroups[i].Count - j - 1);
                }
                catch (Exception) { Debug.WriteLine("อย่าสนใจ^^"); }
            }
            

            //for (int i = 0; i < RankGroups.Count(); i++)
            //{
            //    try
            //    {
            //        //update rank of the clicked InputBtnFunc
            //        RankGroups[i][InputBtnFunc][1]++;
            //        var order = sort.qsort(RankGroups[i].ToList(), (a, b) => a.Value[1] > b.Value[1]);
            //        order.Reverse();
            //        new_rank = order.FindIndex(s => s.Key == InputBtnFunc) + 1;
            //        InputBtnFunc.Left = 50 * (RankGroups[i].Count - new_rank);

            //        //find the previous title holder of InputBtnFunc's new rank
            //        overtaken_button = RankGroups[i].Where(r => r.Value[1] == RankGroups[i][InputBtnFunc][1] && r.Key != InputBtnFunc).ElementAt(0);
            //        var overtaken_button_new_rank = order.FindIndex(s => s.Key == overtaken_button.Key) + 1;
            //        //update the previous title holder
            //        //overtaken_button.Value[1]--;
            //        //overtaken_button.Key.Text = (int.Parse(overtaken_button.Key.Text.Substring(0, overtaken_button.Key.Text.IndexOf(' ')))) +
            //        //overtaken_button.Key.Text.Substring(overtaken_button.Key.Text.IndexOf(' '));
            //        if (overtaken_button.Value[1] > 0)
            //            overtaken_button.Key.Left = 50 * (RankGroups[i].Count - overtaken_button_new_rank);
            //        else if (overtaken_button.Value[1] == 0)
            //            overtaken_button.Key.Visible = false;
            //    }
            //    catch (Exception x) { Debug.Write("{" + x + "|" + i + "}"); }
            //}
        }

        #endregion

        #region Graph Panel

        #region ParamPane

        Dictionary<string, Button> Params = new Dictionary<string, Button>();
        bool on_slider;

        void AddToParams(string name)
        {
            var width = ParamPane.Width / (Params.Count + 1);
            var partition = Params.Count * width; //which partition of the ParamPane the new button will occupy
            var padding = 4;

            Params.Add(name, new Button()
            {
                Height = ParamPane.Height,
                FlatStyle = FlatStyle.Flat,
                Text = name
            });
            Params[name].Click += (sender, e) => 
            {
                on_slider = false;
                foreach (var p in Params) { p.Value.Hide(); }
                ParamSldr.Show();
                ParamBtn.Show();
            };

            ParamPane.Controls.Add(Params[name]);

            var i = 0; foreach (var p in Params)
            {
                width = ParamPane.Width / (Params.Count + 1);
                partition = i * width;

                p.Value.Width = width - padding;
                p.Value.Location = new Point(partition + padding / 2, 0);

                if (on_slider) p.Value.Hide();
                else p.Value.Show();

                i++;
            }
        }
        void RemoveFromParams(string name)
        {
            ParamPane.Controls.Remove(Params[name]);
            Params.Remove(name);
        }
        
        private void OnParamBtn(object sender, EventArgs e)
        {
            on_slider = false;
            ParamSldr.Hide();
            ParamBtn.Hide();
            foreach (var p in Params) { p.Value.Show(); }
        }

        #endregion

        #region ParamPane Slider

        double ParamSldr_min = 0;
        double ParamSldr_max = 10;
        double ParamSldr_value;

        private void OnParamSldrThmbMouseDown(object sender, MouseEventArgs e)
        {
            ParamSldrThmbMovingTimer.Start();
        }

        private void OnParamSldrThmbMouseUp(object sender, MouseEventArgs e)
        {
            ParamSldrThmbMovingTimer.Stop();
        }

        private void OnParamSldrThmbMovingTick(object sender, EventArgs e)
        {
            if (Cursor.Position.X < ParamSldrMin.Width + ParamSldr.Left + ParamPane.Left + GraphPanel.Left + Left)
                ParamSldrThmb.Left = ParamSldrMin.Width;
            else if (Cursor.Position.X > ParamSldr.Right - ParamSldrMax.Width + ParamPane.Left + GraphPanel.Left + Left)
                ParamSldrThmb.Left = ParamSldr.Width - ParamSldrMax.Width;
            else
                ParamSldrThmb.Left = Cursor.Position.X - ParamSldr.Left - ParamPane.Left - GraphPanel.Left - Left;

            ParamSldr_value = ParamSldrThmb.Left * (ParamSldr_max - ParamSldr_min) /
                (ParamSldrMax.Left - ParamSldrThmb.Width - ParamSldrMin.Right);

            ParamSldrThmb.Text = ParamSldr_value + "";
        }

        private void OnParamSldrMinChanged(object sender, EventArgs e)
        {
            ParamSldr_min = double.Parse(((TextBox)sender).Text);
        }

        private void OnParamSldrMaxChanged(object sender, EventArgs e)
        {
            ParamSldr_max = double.Parse(((TextBox)sender).Text);
        }

        #endregion
        
        List<Dictionary<string, figura>> GraphContents;
        double x_rotation, y_rotation, z_rotation = 0;
        vector camera_pan = vector.o;

        static class Graph
        {
            public static Tensor X_Value;
            public static Tensor Y_Value;
            public static Tensor Z_Value;
            public static Tensor T_Value;
        }

        void InitialiseGraphPanel()
        {
            //Form.SetStyle is being snotty with subtended Controls so I have to invoke double buffering for the GraphPanel in this way
            typeof(Panel).InvokeMember
                (
                "DoubleBuffered", 
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, 
                null, 
                GraphPanel, 
                new object[] { true }
                ); 

            Globals.Parameters[0] = new Tensor(new Scalar(), false, null, "Keyx");
            Globals.Parameters[1] = new Tensor(new Scalar(), false, null, "Keyy");
            Globals.Parameters[2] = new Tensor(new Scalar(), false, null, "Keyz");
            Globals.Parameters[3] = new Tensor(new Scalar(), false, null, "Keyt");

            GuideLines.Add(new List<List<pontus>>[4]);
            for (int i = 0; i < 4; i++)//note to self, T[].Initialize calls default(T), which is absolutely useless if T a is reference type
                GuideLines[0][i] = new List<List<pontus>>();

            GraphContents = new List<Dictionary<string, figura>> { new Dictionary<string, figura>(), new Dictionary<string, figura>() };
            GraphContents[0].Add("x", new lin(pontus.o, new pontus(vector.X.nummul(5).pares), Color.Red, GraphPanel, false, null));
            GraphContents[0].Add("y", new lin(pontus.o, new pontus(vector.Y.nummul(5).pares), Color.Green, GraphPanel, false, null));
            GraphContents[0].Add("z", new lin(pontus.o, new pontus(vector.Z.nummul(5).pares), Color.Blue, GraphPanel, false, null));
            
            cam.g = 20;

            ParamSldr_min = 0;
            ParamSldr_max = 10;

            on_slider = false;
            ParamSldr.Hide();
            ParamBtn.Hide();

            //GraphPanel.MouseDown += OnGraphMouseDown;
            //GraphPanel.

            AddToParams("x");
            AddToParams("y");
            AddToParams("z");
            AddToParams("t");
        }
        /// <summary>
        /// each element in this list is an array with a list of lines (each line is a list of points) for each array element (dimension). Each element is for a corrosponding Tile.
        /// </summary>
        List<List<List<pontus>>[]> GuideLines = new List<List<List<pontus>>[]>(); 

        private void GraphPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString("θ: " + Math.Round(x_rotation * 1000 * 180 / Math.PI) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 0));
            e.Graphics.DrawString("ϕ: " + Math.Round(y_rotation * 1000 * 180 / Math.PI) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 10));
            e.Graphics.DrawString("ψ: " + Math.Round(z_rotation * 1000 * 180 / Math.PI) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 20));

            e.Graphics.DrawString("x: " + Math.Round(camera_pan.x * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(65, 0));
            e.Graphics.DrawString("y: " + Math.Round(camera_pan.y * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(65, 10));
            e.Graphics.DrawString("z: " + Math.Round(camera_pan.z * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(65, 20));
            
            e.Graphics.DrawString("zoom: " + Math.Round(cam.g * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 35));

            #region green
            //for(int i = 0; i < TilePane.Controls.Count; i++)//ControlCollection won't let me do queries so deal with it
            //{
            //    if(TilePane.Controls[i] is Tile)
            //    {
            //        try
            //        {
            //            var t1 = (TilePane.Controls[i] as Tile).Interpret().Equations.ToList();
            //            GraphPanel_Plot((TilePane.Controls[i] as Tile).Interpret().Equations.ToList());
            //        }
            //        catch (Error) { }
            //    }
            //}
            #endregion

            var camera_rotation = matrix.rotm(x_rotation, y_rotation, z_rotation);
            var to_be_plotted = GraphContents.ConvertAll(g => g.ToList().ConvertAll
                (f => new Tuple<string, figura>(f.Key, f.Value.rmat(camera_rotation, camera_pan))));

            var buffer = new Bitmap(GraphPanel.Width, GraphPanel.Height);
            using (var buffer_Graphics = Graphics.FromImage(buffer))
            {
                to_be_plotted.ForEach(g => g.ForEach(f =>
                {
                    float x, y;
                    f.Item2.plot(MouseDragDelta, this, new PaintEventArgs(buffer_Graphics, GraphPanel.Bounds), out x, out y);
                    if (f.Item1[0] != '$')
                        /*e.Graphics*/buffer_Graphics.DrawString(f.Item1, Globals.DataFont, Globals.MathsBrush, x + 2, y + 2);
                }));

                for(int k = 0; k < GuideLines.Count; k++)
                    for (int i = 0; i < 4; i++)
                        for (int j = 0; j < GuideLines[k][i].Count; j++)
                        {
                            var points = GuideLines[k][i][j].ConvertAll(v =>
                            {
                                var cout = v.rmat(camera_rotation, camera_pan) as pontus;
                                return new PointF((float)(cout.x * cam.g + GraphPanel.Width / 2 + MouseDragDelta.X),
                                    (float)(cout.y * cam.g + GraphPanel.Height / 2 + MouseDragDelta.Y));
                            });
                            if (points.Count > 1)
                                /*e.Graphics*/buffer_Graphics.DrawCurve(new Pen(GuideLines[k][i][j][0].col), points.ToArray());
                        }

                e.Graphics.DrawImageUnscaled(buffer, 0, 0);
            }
        }

        protected void GraphPanel_Plot(List<Equation> equations, Tile tile/*, List<Tensor> sliders*/)//TODO: this is for the animation feature
        {
            double constrain(double constrainee, relation relation, double bar)
            {
                switch (relation)
                {
                    case relation.Equal:
                        return bar;
                    case relation.Greater:
                        return constrainee > bar ? constrainee : bar + 1;
                    case relation.Less:
                        return constrainee < bar ? constrainee : bar - 1;
                    case relation.GreaterEqual:
                        return Math.Max(constrainee, bar);
                    case relation.LessEqual:
                        return Math.Min(constrainee, bar);
                    default:
                        return constrainee;
                }
            }
            
            Debug.Write("calculating...");
            var id = 0;
            var tile_index = TilePane.Controls.OfType<Tile>().ToList().IndexOf(tile);
            GuideLines[tile_index] = new List<List<pontus>>[4];
            for (int i = 0; i < 4; i++)//note to self, T[].Initialize calls default(T), which is absolutely useless if T a is reference type
                GuideLines[tile_index][i] = new List<List<pontus>>();

            #region green
            //if(equations.Count == 1 && equations[0].parts.Count == 2 && (equations[0].parts[0] as IVariableContainer).Va)
            /*foreach (var eq in equations)
            {
                foreach (var fs in eq.Partition)
                {
                    var subjects = (fs.parts[0] as IVariableContainer).Variables.Where(v => v.name != null);
                    var subject = fs.SolveFor(subjects.First() as Tensor, Globals.values).Value(Globals.values);
                    var name = subject.name.Length > 3 ? subject.name.Substring(0, 3) == "Key" ? subject.name.Substring(3) : subject.name : subject.name;
                    var is_entity = subject.DimDim > 0;//subjects.Count() == 1 && subjects.First() is Tensor && subject.DimDim > 0; _is_entity |= is_entity;
                    if (is_entity)
                    {
                        //if vector dimensions == 3 or 4 add vector to GraphContents //4 doesn't work yet
                        var tensor = subject as Tensor;
                        if (tensor.cols == 1 && tensor.parts.Count == 3)
                        {
                            var neweq = new Equation(from r in tensor.parts from c in r where c is Formula select c as IFormulaic, 
                                new List<relation> { relation.Equal, relation.Equal, relation.Equal });
                            if (neweq.parts.Count == 3)
                                GraphPanel_Plot(neweq.Equations.ToList(), tile);
                            //var test = (fs.SolveFor(tensor, Globals.values) as Tensor).ToMatrix().columns();
                            GraphContents.Add(name, new pontus
                                (
                                (fs.SolveFor(tensor, Globals.values) as Tensor).ToMatrix().columns()[0],
                                (fs.SolveFor(tensor, Globals.values) as Tensor).ToMatrix().columns()[0],
                                Globals.GraphingColour,
                                GraphPanel,
                                false, null
                                ));
                        }
                    }
                }
            }*/
            #endregion
            //substitute parameters
            double h = 0.5;
            int tmin = -5, tmax = 5;
            double mag(double grad) => (grad * h + tmin) * Math.Log(Math.Abs(grad * h + tmin) + 1/*, 10*/);
            double step = .5, mag_tmin = mag(tmin), mag_tmax = mag(tmax), mag_step = mag(step);
            int cnt(double prog) => (int)(prog / h);
            //int mag;//calculate the number of decimal places of tmin & tmax
            //for (mag = 1; ; mag *= 10)
            //    if (Math.Round(mag * Math.Max(-tmin, tmax)) == (int)(mag * Math.Max(-tmin, tmax)))
            //        break;
            var points = new quaternion[tmax - tmin + 1];
            var plots = new List<quaternion>();
            //{ x, y, z, t }
            var ps = new int[4];
            GuideLines[tile_index][3].Add(new List<pontus>());//t guideline for parametric funcitons
            var is_parametric = equations.Exists(fs => fs.DeepVariables.ToList().Exists(v => v.name == "Keyt"));

            for (ps[3] = 0; ps[3] <= (is_parametric ? tmax - tmin : 0) / h; ps[3]++)//parametric t (2nd dimension of time)
            {
                Equation.Remember(new Tensor(new Scalar(mag(ps[3])), false, null, "Keyt"));
                for(int i = 0; i < 3; i++)
                    Equation.Remember(new Tensor(new Scalar(mag(0)), false, null, Globals.Parameters[i].name));//apply free value
                var free_parameter_flags = new bool[4];
                free_parameter_flags[3] = true;//parametric t is always free and never defined by the user
                for (int i = 0; i < 3; i++)
                {
                    var inequality = false;
                    var bearer = equations.Find(e => e.DeepVariables.ToList().Exists(v => //detect inequality
                    {
                        inequality = v.name == Globals.Parameters[i].name && (v as IFormulaic).CRelation.inequality;
                        return v.name == Globals.Parameters[i].name;
                    }));
                    IVariable val;
                    if (bearer != null)
                    {
                        val = bearer.SolveFor(Globals.Parameters[i], Globals.values);
                        free_parameter_flags[i] = !val.known;
                    }
                    else val = null;
                    free_parameter_flags[i] |= bearer == null || inequality;
                    if (!free_parameter_flags[i])//if we can solve for 'z' given 't'
                        Equation.Remember(val);
                }
                //if (free_parameter_flags[2])
                //GuideLines[2].Add(new List<pontus>());

                var NaNz = 0;

                for (ps[2] = 0; ps[2] <= (free_parameter_flags[2] ? tmax - tmin : 0) / h; ps[2]++)
                {
                    if (free_parameter_flags[2])
                        Equation.Remember(new Tensor(new Scalar(mag(ps[2])), false, null, "Keyz"));
                    //if (free_parameter_flags[1])
                    //GuideLines[1].Add(new List<pontus>());

                    var NaNy = 0;

                    for (ps[1] = 0; ps[1] <= (free_parameter_flags[1] ? tmax - tmin : 0) / h; ps[1]++)
                    {
                        if (free_parameter_flags[1])
                        {
                            Equation.Remember(new Tensor(new Scalar(mag(ps[1])), false, null, "Keyy"));
                            //GuideLines[1].Add(new List<pontus>());
                        }
                        if(free_parameter_flags[0])
                            GuideLines[tile_index][0].Add(new List<pontus>());

                        var fact_NaN = false;
                        var NaNx = 0;

                        for (ps[0] = 0; ps[0] <= (free_parameter_flags[0] ? tmax - tmin : 0) / h; ps[0]++)
                        {
                            if (free_parameter_flags[0])
                                Equation.Remember(new Tensor(new Scalar(mag(ps[0])), false, null, "Keyx"));
                            
                            //if one of these remains null afterwards then I know that that parameter wasn't mentioned
                            var constraints = new double?[] { null, null, null, null };
                            foreach (var eq in equations)
                            {
                                foreach (var fs in eq.Partition)
                                {
                                    foreach (var v in from f in fs.Variables.Where(v => v.name != null) select 
                                                      fs.SolveFor(f, Globals.values))//solve for each IVariable V IS TENSOR INSIDE THIS LOOP
                                    {
                                        if (v.name == "Keyt")
                                        {
                                            constraints[3] = mag(ps[3]);
                                            var current_t = new Tensor(new Scalar(mag(ps[3])), false, null, "Keyt");
                                            Equation.Remember(current_t);
                                            continue;//don't solve/substitute for parametric t.
                                        }
                                        var dims = (v is Operation || v is Function) ? v.Value(Globals.values).DimDim : (v as Tensor).DimDim;
                                        for (int j = 0; j < 3; j++)//for each of the 3 dimensions
                                        {
                                            switch (dims)
                                            {
                                                case 0 when Globals.Parameters[j].name == v.name://if the jth parameter is mentioned

                                                    double cout = mag(ps[j] * 20 / cam.g);

                                                    var current_value = v as Tensor;//fs.SolveFor(v, Globals.values) as Tensor;
                                                    if (current_value.known)//if the jth parameter is defined
                                                    {
                                                        if (fs.EqRelations[0] != relation.Equal)
                                                            constraints[j] = constrain(cout, fs.EqRelations[0], (current_value.parts[0][0] as Scalar).output.Value);//assign the definitive value
                                                        else
                                                            constraints[j] = (current_value.parts[0][0] as Scalar).output.Value;
                                                    }
                                                    else//otherwise, make it equal to the free (undefined and therefore unconstrained) value of that parameter
                                                    {
                                                        Equation.Remember(new Tensor(new Scalar(cout), false, null, Globals.Parameters[j].name));
                                                        constraints[j] = cout;
                                                    }
                                                    break;

                                                case 1 when v is Tensor && (v as Tensor).parts.Count == 3 &&
                                                    eq.parts.Count == 2 && (eq.parts[0] as IContainer).parts.Count == 1 && ((eq.parts[0] as IContainer).parts[0] as IContainer).parts.Count == 2:

                                                    var V = v as Tensor;
                                                    var neweq = new Equation();

                                                    for (int i = 0; i < V.parts.Count; i++)
                                                    {
                                                        var param = new Formula(neweq);
                                                        neweq.Add(param, true);//Add the given parameter to the parameter definition equation
                                                        param.Add(new Tensor(new Scalar(), false, null, Globals.Parameters[i].name));
                                                        neweq.EqRelations.Add(relation.Equal);//separate parameter definition equations

                                                        switch (V.parts[i][0])
                                                        {
                                                            case Formula f1:
                                                                neweq.Add(f1, true);
                                                                break;
                                                            case Scalar f2:
                                                                var s = new Formula(neweq);
                                                                neweq.Add(s, true);
                                                                s.Add(new Tensor(f2, false));
                                                                break;
                                                            case IFormulaic f3:
                                                                var d = new Formula(neweq);
                                                                neweq.Add(d, true);
                                                                d.Add(f3);
                                                                neweq.Add(d, true);
                                                                break;
                                                            default:
                                                                break;

                                                        }
                                                        neweq.EqRelations.Add(relation.None);
                                                    }

                                                    if (neweq.Equations.Count() == 3)//we're only working in 3D atm
                                                    {
                                                        //GraphPanel_Plot(neweq.Equations.ToList(), tile);
                                                        for (int i = 0; i < 3; i++)
                                                        {
                                                            //constraints[i] = (neweq.Equations.ElementAt(i).SolveFor(Globals.Parameters[i], Globals.values)
                                                            //    .Value(Globals.values).parts[0][0] as Scalar).output;

                                                            free_parameter_flags = new bool[4];
                                                            free_parameter_flags[3] = true;//t is always free
                                                            //find the i'th parameter
                                                            var bearer = neweq.Equations.ToList().Find(e => e.DeepVariables.ToList().Exists(s => s.name == Globals.Parameters[i].name));
                                                            //something somewhere (in Equation.Equations I think) is swapping out this reference so I have to restore it here
                                                            foreach (var p in bearer.parts)
                                                                p.Container = bearer;
                                                            //determine whether or not the i'th parameter is constrained
                                                            free_parameter_flags[i] = bearer == null ? true : !bearer.SolveFor(Globals.Parameters[i], Globals.values).known;
                                                            //define the constrained i'th paramter
                                                            if (!free_parameter_flags[i])
                                                            {
                                                                Equation.Remember(bearer.SolveFor(Globals.Parameters[i], Globals.values));
                                                                ps[i] = cnt(tmax);//(Globals.Parameters[i].Value(Globals.values).parts[0][0] as Scalar).output.Value;
                                                            }
                                                            constraints[i] = (Globals.Parameters[i].Value(Globals.values).parts[0][0] as Scalar).output;
                                                        }
                                                        goto AssignToPlots;
                                                    }
                                                    break;

                                                case 2:
                                                    return;
                                            }
                                        }
                                    }
                                }
                            }
                        AssignToPlots:

                            if (constraints.All(c => c == null))//if this is just an arbitrary scalar equation
                                break;

                            for (int i = 0; i < 4; i++)//account for all the free parameters that weren't mentioned
                            {
                                if (constraints[i] == null)
                                {
                                    constraints[i] = ps[i];
                                    constraints[i] = mag(constraints[i].Value);//also zoom out, converting everything back to its original magnitude
                                }
                            }

                            //ALL CONSTRAINTS HAVE VALUE FROM HERE UNTIL '//x'
                            var not_NaN = constraints.All(c => !double.IsNaN(c.Value) && !double.IsInfinity(c.Value));
                            fact_NaN |= !not_NaN;
                            if (not_NaN)//exclude values that aren't numbers
                            {
                                plots.Add(new quaternion
                                        (
                                        constraints[3].Value,
                                        constraints[0].Value,
                                        constraints[1].Value,
                                        constraints[2].Value
                                        ));

                                Debug.WriteLine("plots:" + plots.Count);

                                //if (constraints[4].HasValue)
                                //    AddValues(constraints, plots, t, tmin, tmax);

                                //for (int i = 0; i < 4; i++)
                                //{
                                //    var addendum = new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                //            constraints[0].Value, constraints[1].Value, constraints[2].Value);
                                //    if (free_parameter_flags[i]/* && !GuideLines[i].Last().Exists(g => g.equals(addendum))*/)
                                //        GuideLines[i][i == 3 ? 0 : GuideLines[i].Count - 1].Add(addendum);
                                //}
                                if (free_parameter_flags[0])
                                    GuideLines[tile_index][0][GuideLines[tile_index][0].Count == 0 ? 0 : GuideLines[tile_index][0].Count - 1].Add
                                        (new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                            constraints[0].Value, constraints[1].Value, constraints[2].Value, Globals.GraphingColour, GraphPanel, false, null));
                                if (free_parameter_flags[1])
                                {
                                    if (GuideLines[tile_index][1].Count == ps[free_parameter_flags[0] ? 0 : 2] - NaNx)//if the number of y-lines is the same as the number of points in the current x-line
                                        GuideLines[tile_index][1].Add(new List<pontus>());//add the next y-line
                                    if (GuideLines[tile_index][1].Count > ps[free_parameter_flags[0] ? 0 : 2] - NaNx)//if the number of y-lines is now greater
                                        GuideLines[tile_index][1][ps[free_parameter_flags[0] ? 0 : 2] - NaNx].Add
                                            (new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                                constraints[0].Value, constraints[1].Value, constraints[2].Value, Globals.GraphingColour, GraphPanel, false, null));
                                    else
                                        throw new DevError("DevError: Index out of sync when drawing Y-guidelines.");
                                }
                                if (free_parameter_flags[2])
                                {
                                    if (GuideLines[tile_index][2].Count == (ps[1] - NaNy) * (free_parameter_flags[0] ? cnt(tmax - tmin) + 1 : 1) + ps[0] - NaNx)//if the number of z-lines is the number of points in each completed y-line combined plus the number of points in the current x-line
                                        GuideLines[tile_index][2].Add(new List<pontus>());
                                    if (GuideLines[tile_index][2].Count > (ps[1] - NaNy) * (free_parameter_flags[0] ? cnt(tmax - tmin) + 1 : 1) + ps[0] - NaNx)
                                        GuideLines[tile_index][2][(ps[1] - NaNy) * (free_parameter_flags[0] ? cnt(tmax - tmin) + 1 : 1) + ps[0] - NaNx].Add
                                            (new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                                constraints[0].Value, constraints[1].Value, constraints[2].Value, Globals.GraphingColour, GraphPanel, false, null));
                                    else
                                        throw new DevError("DevError: Index out of sync when drawing Z-guidelines.");
                                }
                                if (is_parametric)
                                    GuideLines[tile_index][3][0].Add(new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                                constraints[0].Value, constraints[1].Value, constraints[2].Value, Globals.GraphingColour, GraphPanel, false, null));
                            }
                            else
                            {
                                //GuideLines[tile_index][0].RemoveAt(GuideLines[tile_index][0].Count - 1);
                                NaNx++;//make NaNx count backwards against ps[0] to keep the x-line index still when no point is plotted, so it is in the ps[0] loop.
                                
                                NaNz++;
                            }
                        }//x

                        if(fact_NaN)//NaNy has to count against ps[1] in order to keep the y-line index still when no point is plotted, so it is only in the ps[1] for loop
                            NaNy++;
                        //if (GuideLines[tile_index][0].Count == 0)//if this x-line has no points and simply doesn't exist, just get rid of it.
                          //  GuideLines[tile_index][0].RemoveAt(GuideLines[tile_index][0].Count - 1);

                    }//y
                }//z
                //for(int i = 0; i < 4; i++)
                //{
                //    for(ps[i] = tmin; ps[i] < tmax; ps[i]++)
                //    {
                //        for(ps[(i + 1) % 4] = tmin; ps[(i + 1) % 4] < tmax; ps[(i + 1) % 4]++)
                //        {
                //            for(ps[(i + 2) % 4] = tmin; ps[(i + 2) % 4] < tmax; ps[(i + 2) % 4]++)
                //            {
                //                for(ps[(i + 3) % 4] = tmin; ps[(i + 3) % 4] < tmax; ps[(i + 3) % 4]++)
                //                {
                //                    GuideLines[(i + 3) % 4].Last().Add(new pontus())
                //                }
                //            }
                //        }
                //    }
                //}
                //TEMPORARY
                foreach (var p in plots)
                {
                    if (GraphContents[tile_index + 1].ContainsKey("$" + tile.Parent.Controls.IndexOf(tile) + ":" + id))
                        GraphContents[tile_index + 1].Remove("$" + tile.Parent.Controls.IndexOf(tile) + ":" + id);
                    GraphContents[tile_index + 1].Add
                        (
                        "$" + tile.Parent.Controls.IndexOf(tile) + ":" + id,
                        new pontus(p.tovect().pares, p.tovect().pares, Globals.GraphingColour, GraphPanel, false, null)
                        );
                    id++;
                }
            }//parametric t
            //turn points onto lines using lagrange interpolation
            //for(int k = 0; k < 3; k++)
            //    for (int i = 0; i < lines[k].Count; i++)
            //        lines[k][i] * 
            //add lines to GraphContents
            Globals.values.Remove("Keyt");//forget the values(s) of 't' that were remembered for this instance of plotting
            //Globals.values.Remove("KeyT");
            Globals.values.Remove("Keyz");
            Globals.values.Remove("Keyy");
            Globals.values.Remove("Keyx");
        }

        private void GraphPanel_FromKeyDown(Keys KeyCode)
        {
            switch (KeyCode)
            {
                case Keys.W:
                    x_rotation += Globals.rotation_speed;
                    break;
                case Keys.S:
                    x_rotation -= Globals.rotation_speed;
                    break;
                case Keys.A:
                    y_rotation += Globals.rotation_speed;
                    break;
                case Keys.D:
                    y_rotation -= Globals.rotation_speed;
                    break;
                case Keys.Q:
                    z_rotation += Globals.rotation_speed;
                    break;
                case Keys.E:
                    z_rotation -= Globals.rotation_speed;
                    break;

                case Keys.Right:
                    camera_pan.x += Globals.panning_speed;
                    break;
                case Keys.Left:
                    camera_pan.x -= Globals.panning_speed;
                    break;
                case Keys.Up:
                    camera_pan.y += Globals.panning_speed;
                    break;
                case Keys.Down:
                    camera_pan.y -= Globals.panning_speed;
                    break;
                case Keys.NumPad3:
                    camera_pan.z += Globals.panning_speed;
                    break;
                case Keys.NumPad1:
                    camera_pan.z -= Globals.panning_speed;
                    break;
            }

            GraphPanel.Invalidate();
        }

        Point MouseDragDelta = new Point(0, 0);

        private void OnGraphMouseDown(object sender, MouseEventArgs e)
        {
            MousePosFinal = MousePosition;
            (sender as Panel).Focus();
            GraphPanTimer.Start();
        }

        private void OnGraphMouseUp(object sender, MouseEventArgs e)
        {
            GraphPanTimer.Stop();
        }

        private void OnGraphPanTick(object sender, EventArgs e)
        {
            MousePosInital = MousePosFinal;
            MousePosFinal = MousePosition;

            var change = new Point(MousePosFinal.X - MousePosInital.X, MousePosFinal.Y - MousePosInital.Y);
            MouseDragDelta.Offset(change);

            GraphPanel.Invalidate();
        }

        void OnMouseWheel(object sender, MouseEventArgs e)
        {
            cam.g += e.Delta / 10.0;
            GraphPanel.Invalidate();
        }
        #endregion

        #region Keyboard event handlers

        protected struct Key_ValueTuple
        {
            public Keys KeyCode;
            public char KeyChar;
        } Key_ValueTuple key_being_pressed = new Key_ValueTuple() { KeyChar = 'อ' };

        protected class KeyPressDownEventArgs : EventArgs
        {
            public char KeyChar { get; }
            public Keys KeyCode { get; }

            public KeyPressDownEventArgs(char KeyChar, Keys KeyCode)
            {
                this.KeyChar = KeyChar;
                this.KeyCode = KeyCode;
            }
        }

        protected delegate void KeyPressDownHandler(object sender, EventArgs e);
        protected event KeyPressDownHandler KeyPressDown;

        private void InitialiseEventHandlers()
        {
            KeyPressDown += new KeyPressDownHandler((sender, e) => { /*KeyPressDownTimer.Start();*/ KeyPressDownTick(this, new EventArgs()); Debug.Write("down"); });
        }
        
        private void CheckKeyPressDown()
        {
            //check that the two events are in sync so that the data for each are data from the same instance of a button being pressed
            if (Globals.KeyPress_count == Globals.KeyDown_count && Globals.KeyPress_count > 0)
                KeyPressDown?.Invoke(this, new EventArgs()); //new EventArgs() --> new KeyPressDownEventArgs(), however the combined data contained in the latter are not necessary atm
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.ControlKey)
            {
                //if (e.KeyCode == Keys.F2)
                //{ Equation t1;
                //    try
                //    {
                //        t1 = Globals.FocusedTile.Interpret();
                //        //IVariable t2;

                //        //t2 = t1.SolveFor(((t1.parts[0] as IContainer).parts[0] as Term).numerator[1] as IVariable, Globals.values);
                //        //t1.Remember(t2);
                //        var t3 = new List<Formula>();
                //        foreach (var f in t1.parts)
                //            t3.Add(t1.CollectLikeTerms(f as Formula));
                //    }
                //    catch (Error error) { Debug.WriteLine(error.Message); }
                //}
                //Debug.Write("[" + Globals.KeyDown_count + ":");
                if (Globals.KeyDown_count == 0 || Globals.KeyDown_count > 5 || key_being_pressed.KeyCode != e.KeyCode)
                {
                    key_being_pressed.KeyCode = e.KeyCode;
                    KeyPressDownTick(this, new EventArgs());//CheckKeyPressDown();
                    //Globals.KeyDown_count = 0;
                }
                else key_being_pressed.KeyCode = e.KeyCode;
                Globals.KeyDown_count++;
            }
            Globals.shift |= e.KeyCode == Keys.ShiftKey;
        }
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            key_being_pressed.KeyChar = e.KeyChar;
            Globals.KeyPress_count++; //Debug.Write(Globals.KeyPress_count + "]");
            //CheckKeyPressDown();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //Debug.Write("up");
            Globals.KeyDown_count = 0;
            KeyPressDownTimer.Stop();
            GraphPanel.Invalidate();
            Globals.shift &= e.Shift;
        }

        private void KeyPressDownTick(object sender, EventArgs e)
        {
            //Debug.Write("A");
            if (key_being_pressed.KeyCode == Keys.Escape)
            {
                IsTyping = false;
                GraphPanel.Focus();
            }

            if (IsTyping && !GraphPanel.Focused)
            {
                foreach (Control c in TilePane.Controls)
                {
                    if (c is Tile && (c as Tile).TextInput.Focused)
                        (c as Tile).FromKeyDown(key_being_pressed.KeyCode, Globals.shift);
                }
            }
            else
            {
                GraphPanel_FromKeyDown(key_being_pressed.KeyCode);
            }
        }

        #endregion
    }
}
