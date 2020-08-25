using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

using System.IO.Pipes;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Threading;

using Gma.System.MouseKeyHook;

namespace Graphing_1
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
            Globals.symbols.Add("KeyAlpha", new Symbol(8, (Image)Globals.resources.GetObject("KeyAlpha.Image")));
            Globals.symbols.Add("KeyBeta", new Symbol(8, (Image)Globals.resources.GetObject("KeyBeta.Image")));
            Globals.symbols.Add("KeyGamma", new Symbol(8, (Image)Globals.resources.GetObject("KeyGamma.Image")));
            Globals.symbols.Add("KeyDelta", new Symbol(8, (Image)Globals.resources.GetObject("KeyDelta.Image")));
            Globals.symbols.Add("KeyEpsilon", new Symbol(8, (Image)Globals.resources.GetObject("KeyEpsilon.Image")));
            Globals.symbols.Add("KeyZeta", new Symbol(8, (Image)Globals.resources.GetObject("KeyZeta.Image")));
            Globals.symbols.Add("KeyEta", new Symbol(8, (Image)Globals.resources.GetObject("KeyEta.Image")));
            Globals.symbols.Add("KeyTheta1", new Symbol(8, (Image)Globals.resources.GetObject("KeyTheta1.Image")));
            Globals.symbols.Add("KeyTheta2", new Symbol(8, (Image)Globals.resources.GetObject("KeyTheta2.Image")));
            Globals.symbols.Add("KeyIota", new Symbol(8, (Image)Globals.resources.GetObject("KeyIota.Image")));
            Globals.symbols.Add("KeyKappa", new Symbol(8, (Image)Globals.resources.GetObject("KeyKappa.Image")));
            Globals.symbols.Add("KeyLambda", new Symbol(8, (Image)Globals.resources.GetObject("KeyLambda.Image")));
            Globals.symbols.Add("KeyMu", new Symbol(8, (Image)Globals.resources.GetObject("KeyMu.Image")));
            Globals.symbols.Add("KeyNu", new Symbol(8, (Image)Globals.resources.GetObject("KeyNu.Image")));
            Globals.symbols.Add("KeyXi", new Symbol(8, (Image)Globals.resources.GetObject("KeyXi.Image")));
            Globals.symbols.Add("KeyPi", new Symbol(8, (Image)Globals.resources.GetObject("KeyPi.Image")));
            Globals.symbols.Add("KeyRho", new Symbol(8, (Image)Globals.resources.GetObject("KeyRho.Image")));
            Globals.symbols.Add("KeySigma1", new Symbol(8, (Image)Globals.resources.GetObject("KeySigma1.Image")));
            Globals.symbols.Add("KeySigma2", new Symbol(8, (Image)Globals.resources.GetObject("KeySigma2.Image")));
            Globals.symbols.Add("KeyTau", new Symbol(8, (Image)Globals.resources.GetObject("KeyTau.Image")));
            Globals.symbols.Add("KeyUpsilon", new Symbol(8, (Image)Globals.resources.GetObject("KeyUpsilon.Image")));
            Globals.symbols.Add("KeyPhi1", new Symbol(8, (Image)Globals.resources.GetObject("KeyPhi1.Image")));
            Globals.symbols.Add("KeyPhi2", new Symbol(8, (Image)Globals.resources.GetObject("KeyPhi2.Image")));
            Globals.symbols.Add("KeyChi", new Symbol(8, (Image)Globals.resources.GetObject("KeyChi.Image")));
            Globals.symbols.Add("KeyPsi", new Symbol(8, (Image)Globals.resources.GetObject("KeyPsi.Image")));
            Globals.symbols.Add("KeyOmega", new Symbol(8, (Image)Globals.resources.GetObject("KeyOmega.Image")));
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

            Globals.symbols.Add("Keya", new Symbol(8, (Image)Globals.resources.GetObject("Keya")));
            Globals.symbols.Add("Keyb", new Symbol(13, (Image)Globals.resources.GetObject("Keyb")));
            Globals.symbols.Add("Keyc", new Symbol(8, (Image)Globals.resources.GetObject("Keyc")));
            Globals.symbols.Add("Keyd", new Symbol(13, (Image)Globals.resources.GetObject("Keyd")));
            Globals.symbols.Add("Keye", new Symbol(8, (Image)Globals.resources.GetObject("Keye")));
            Globals.symbols.Add("Keyf", new Symbol(13, (Image)Globals.resources.GetObject("Keyf")));
            Globals.symbols.Add("Keyg", new Symbol(8, (Image)Globals.resources.GetObject("Keyg")));
            Globals.symbols.Add("Keyh", new Symbol(8, (Image)Globals.resources.GetObject("Keyh")));
            Globals.symbols.Add("Keyi", new Symbol(12, (Image)Globals.resources.GetObject("Keyi")));
            Globals.symbols.Add("Keyj", new Symbol(8, (Image)Globals.resources.GetObject("Keyj")));
            Globals.symbols.Add("Keyk", new Symbol(13, (Image)Globals.resources.GetObject("Keyk")));
            Globals.symbols.Add("Keyl", new Symbol(13, (Image)Globals.resources.GetObject("Keyl")));
            Globals.symbols.Add("Keym", new Symbol(8, (Image)Globals.resources.GetObject("Keym")));
            Globals.symbols.Add("Keyn", new Symbol(8, (Image)Globals.resources.GetObject("Keyn")));
            Globals.symbols.Add("Keyo", new Symbol(8, (Image)Globals.resources.GetObject("Keyo")));
            Globals.symbols.Add("Keyp", new Symbol(8, (Image)Globals.resources.GetObject("Keyp")));
            Globals.symbols.Add("Keyq", new Symbol(8, (Image)Globals.resources.GetObject("Keyq")));
            Globals.symbols.Add("Keyr", new Symbol(8, (Image)Globals.resources.GetObject("Keyr")));
            Globals.symbols.Add("Keys", new Symbol(8, (Image)Globals.resources.GetObject("Keys")));
            Globals.symbols.Add("Keyt", new Symbol(8, (Image)Globals.resources.GetObject("Keyt")));
            Globals.symbols.Add("Keyu", new Symbol(8, (Image)Globals.resources.GetObject("Keyu")));
            Globals.symbols.Add("Keyv", new Symbol(8, (Image)Globals.resources.GetObject("Keyv")));
            Globals.symbols.Add("Keyw", new Symbol(8, (Image)Globals.resources.GetObject("Keyw")));
            Globals.symbols.Add("Keyx", new Symbol(8, (Image)Globals.resources.GetObject("Keyx")));
            Globals.symbols.Add("Keyy", new Symbol(8, (Image)Globals.resources.GetObject("Keyy")));
            Globals.symbols.Add("Keyz", new Symbol(8, (Image)Globals.resources.GetObject("Keyz")));
            #endregion
            #region Mathematical symbols
            //BODMAS
            Globals.symbols.Add("+", new Symbol(12, (Image)Globals.resources.GetObject("Plus")));
            Globals.symbols.Add("-", new Symbol(5, (Image)Globals.resources.GetObject("Minus")));
            Globals.symbols.Add("*", new Symbol(6, (Image)Globals.resources.GetObject("Cross")));
            Globals.symbols.Add("/", new Symbol(8, (Image)Globals.resources.GetObject("Divide")));
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
            Globals.symbols.Add("[2](big)integral", new Symbol((Globals.resources.GetObject("Integral") as Image).Height, (Image)Globals.resources.GetObject("Integral")));
            Globals.symbols.Add("[2](big)cintegral", new Symbol((Globals.resources.GetObject("ContourIntegral") as Image).Height, (Image)Globals.resources.GetObject("ContourIntegral")));
            Globals.symbols.Add("[2](non)del", new Symbol((Globals.resources.GetObject("Nabla") as Image).Height, (Image)Globals.resources.GetObject("Nabla")));
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
            Globals.symbols.Add(".", new Symbol(2, (Image)Globals.resources.GetObject(".")));
            #endregion

            #region Scalar Functions
            //does all of the tangly conversions so that adding a function becomes less effort
            Func<List<Formula>, Func<double[], double>, Tensor> scalar = (tensor, func) =>
                new Tensor(new Scalar(func((from t in tensor select (t.Value(Globals.values).parts[0][0] as Scalar).output.Value).ToArray())), false);

            //scalar, represented by the [1] tag
            Globals.functions.Add("[1](sml)sqrt", (args, prms) => scalar(args, x => Math.Sqrt(x[0])));
            Globals.functions.Add("[1](sml)sin", (args, prms) => scalar(args, x => Math.Sin(x[0])));
            Globals.functions.Add("[1](sml)cos", (args, prms) => scalar(args, x => Math.Cos(x[0])));
            Globals.functions.Add("[1](sml)tan", (args, prms) => scalar(args, x => Math.Tan(x[0])));
            Globals.functions.Add("[1](big)#summation", (args, prms) =>
            {
                //The parameters are casted into their true datatype
                var eq = prms as Tuple<Formula, List<Equation>>;
                //The iterator function is just the operand, but it takes the index as a parameter, meaning I can substitute 
                //the index variable for the parameter that I enter into the iterator function in the for loop.
                var iterator = new Function(null, "iterator", new List<Formula>(), args[0]);
                //initial value of the index is the solution to the equation underneath the built-in iterating function, stored in eq.Item2.
                var n = eq.Item2[0].SolveFor((eq.Item2[0].parts[0] as IVariableContainer).Variables.Where(v => v != null && !v.known && v.name != null).First(), Globals.values) as Tensor;
                //index_formula will be passed as the parameter of iterator.
                var index_formula = new Formula(null);
                index_formula.Add(n);

                var cout = iterator.Value(new List<Formula> { index_formula }, Globals.values);
                for (n = n.add(new Tensor(new Scalar(1), false)); (n.parts[0][0] as Scalar).output <= (eq.Item1.Value(Globals.values).parts[0][0] as Scalar).output.Value; n = n.add(new Tensor(new Scalar(1), false)))
                {
                    //update index_formula so that the next index is passed as the parameter of iterator
                    (index_formula.parts[0] as Term).numerator[1] = n;//new Tensor(new Scalar(n), false);
                    //the actual addition part
                    cout = cout.add(iterator.Value(new List<Formula> { index_formula }, Globals.values));
                }

                return cout;
            });
            Globals.functions.Add("[1](big)#piproduct", (args, prms) =>
            {
                //The parameters are casted into their true datatype
                var eq = prms as Tuple<Formula, List<Equation>>;
                //The iterator function is just the operand, but it takes the index as a parameter, meaning I can substitute 
                //the index variable for the parameter that I enter into the iterator function in the for loop.
                var iterator = new Function(null, "iterator", new List<Formula>(), args[0]);
                //initial value of the index is the solution to the equation underneath the built-in iterating function, stored in eq.Item2.
                var n = eq.Item2[0].SolveFor((eq.Item2[0].parts[0] as IVariableContainer).Variables.Where(v => v != null && !v.known && v.name != null).First(), Globals.values) as Tensor;
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
                TilePane.Controls[i].Top = ((i - 1) / 3) * 50 - (int)(((TilePane.Controls.Count - 1.0) * Tile.Size.Height / 3 - TilePane.Height) * 10 * e.NewValue / TilePaneScroll.Maximum);
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
                catch (Exception) { }
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

        Dictionary<string, figura> GraphContents;
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

            GraphContents = new Dictionary<string, figura>();
            GraphContents.Add("x", new lin(pontus.o, new pontus(vector.X.nummul(5).pares), Color.Red, GraphPanel, false, null));
            GraphContents.Add("y", new lin(pontus.o, new pontus(vector.Y.nummul(5).pares), Color.Green, GraphPanel, false, null));
            GraphContents.Add("z", new lin(pontus.o, new pontus(vector.Z.nummul(5).pares), Color.Blue, GraphPanel, false, null));

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
            var to_be_plotted = GraphContents.ToList().ConvertAll
                (f => new Tuple<string, figura>(f.Key, f.Value.rmat(camera_rotation, camera_pan)));

            var buffer = new Bitmap(GraphPanel.Width, GraphPanel.Height);
            using (var buffer_Graphics = Graphics.FromImage(buffer))
            {
                to_be_plotted.ForEach(f =>
                {
                    float x, y;
                    f.Item2.plot(MouseDragDelta, this, new PaintEventArgs(buffer_Graphics, GraphPanel.Bounds), out x, out y);
                    if (f.Item1[0] != '$')
                        /*e.Graphics*/
                        buffer_Graphics.DrawString(f.Item1, Globals.DataFont, Globals.MathsBrush, x + 2, y + 2);
                });

                for (int k = 0; k < GuideLines.Count; k++)
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
                                /*e.Graphics*/
                                buffer_Graphics.DrawCurve(new Pen(GuideLines[k][i][j][0].col), points.ToArray());
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
            int tmin = -5, tmax = 5;
            //int mag;//calculate the number of decimal places of tmin & tmax
            //for (mag = 1; ; mag *= 10)
            //    if (Math.Round(mag * Math.Max(-tmin, tmax)) == (int)(mag * Math.Max(-tmin, tmax)))
            //        break;
            var points = new quaternion[tmax - tmin + 1];
            var plots = new List<quaternion>();
            //{ x, y, z, t }
            var ps = new[] { 0, 0, 0, 0 };
            GuideLines[tile_index][3].Add(new List<pontus>());//t guideline for parametric funcitons
            var is_parametric = equations.Exists(fs => fs.Variables.ToList().Exists(v => v.name == "Keyt"));
            for (ps[3] = tmin; ps[3] <= (is_parametric ? tmax : tmin); ps[3]++)//parametric t (2nd dimension of time)
            {
                equations[0].Remember(new Tensor(new Scalar(ps[3]), false, null, "Keyt"));
                var free_parameter_flags = new bool[4];
                free_parameter_flags[3] = true;//parametric t is always free and never defined by the user
                for (int i = 0; i < 3; i++)
                {
                    var inequality = false;
                    var bearer = equations.Find(e => e.Variables.ToList().Exists(v =>
                    {
                        inequality = v.name == Globals.Parameters[i].name && (v as IFormulaic).CRelation.R != relation.Equal;
                        return v.name == Globals.Parameters[i].name;
                    }));
                    free_parameter_flags[i] = (bearer == null ? true : !bearer.SolveFor(Globals.Parameters[i], Globals.values).known) || inequality;
                    if (free_parameter_flags[i])//if we can solve for 'z' given 't'
                        equations[0].Remember(new Tensor(new Scalar(tmin), false, null, Globals.Parameters[i].name));//apply free value
                    else
                        equations[0].Remember(bearer.SolveFor(Globals.Parameters[i], Globals.values));
                }
                //if (free_parameter_flags[2])
                //GuideLines[2].Add(new List<pontus>());
                for (ps[2] = tmin; ps[2] <= (free_parameter_flags[2] ? tmax : tmin); ps[2]++)
                {
                    if (free_parameter_flags[2])
                        equations[0].Remember(new Tensor(new Scalar(ps[2]), false, null, "Keyz"));
                    //if (free_parameter_flags[1])
                    //GuideLines[1].Add(new List<pontus>());

                    for (ps[1] = tmin; ps[1] <= (free_parameter_flags[1] ? tmax : tmin); ps[1]++)
                    {
                        if (free_parameter_flags[1])
                        {
                            equations[0].Remember(new Tensor(new Scalar(ps[1]), false, null, "Keyy"));
                            //GuideLines[1].Add(new List<pontus>());
                        }
                        if (free_parameter_flags[0])
                            GuideLines[tile_index][0].Add(new List<pontus>());

                        for (ps[0] = tmin; ps[0] <= (free_parameter_flags[0] ? tmax : tmin); ps[0]++)
                        {
                            if (free_parameter_flags[0])
                                equations[0].Remember(new Tensor(new Scalar(ps[0]), false, null, "Keyx"));

                            //if one of these remains null afterwards then I know that that parameter wasn't mentioned
                            var constraints = new double?[] { null, null, null, null };
                            foreach (var eq in equations)
                            {
                                foreach (var fs in eq.Partition)
                                {
                                    foreach (var v in from f in fs.Variables.Where(v => v.name != null) select fs.SolveFor(f, Globals.values))//solve for each IVariable
                                    {
                                        if (v.name == "Keyt")
                                        {
                                            constraints[3] = ps[3];
                                            var current_t = new Tensor(new Scalar(ps[3]), false, null, "Keyt");
                                            fs.Remember(current_t);
                                            continue;//don't solve/substitute for parametric t.
                                        }
                                        for (int j = 0; j < 3; j++)//for each of the 3 dimensions
                                        {
                                            switch (v.Value(Globals.values).DimDim)
                                            {
                                                case 0 when Globals.Parameters[j].name == v.name://if the jth parameter is mentioned

                                                    double cout = ps[j] * 20 / cam.g;

                                                    var current_value = fs.SolveFor(v, Globals.values) as Tensor;
                                                    if (current_value.known)//if the jth parameter is defined
                                                    {
                                                        if (fs.EqRelations[0] != relation.Equal)
                                                            constraints[j] = constrain(cout, fs.EqRelations[0], (current_value.parts[0][0] as Scalar).output.Value);//assign the definitive value
                                                        else
                                                            constraints[j] = (current_value.parts[0][0] as Scalar).output.Value;
                                                    }
                                                    else//otherwise, make it equal to the free (undefined and therefore unconstrained) value of that parameter
                                                    {
                                                        fs.Remember(new Tensor(new Scalar(cout), false, null, Globals.Parameters[j].name));
                                                        constraints[j] = cout;
                                                    }
                                                    break;

                                                case 1 when v is Tensor && (v as Tensor).parts.Count == 3 &&
                                                    eq.parts.Count == 2 && (eq.parts[0] as IContainer).parts.Count == 1 && ((eq.parts[0] as IContainer).parts[0] as IContainer).parts.Count == 3:

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
                                                            var bearer = neweq.Equations.ToList().Find(e => e.Variables.ToList().Exists(s => s.name == Globals.Parameters[i].name));
                                                            //something somewhere (in Equation.Equations I think) is swapping out this reference so I have to restore it here
                                                            foreach (var p in bearer.parts)
                                                                p.Container = bearer;
                                                            //determine whether or not the i'th parameter is constrained
                                                            free_parameter_flags[i] = bearer == null ? true : !bearer.SolveFor(Globals.Parameters[i], Globals.values).known;
                                                            //define the constrained i'th paramter
                                                            if (!free_parameter_flags[i])
                                                            {
                                                                equations[0].Remember(bearer.SolveFor(Globals.Parameters[i], Globals.values));
                                                                ps[i] = tmax;//(Globals.Parameters[i].Value(Globals.values).parts[0][0] as Scalar).output.Value;
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
                                if (constraints[i] == null)
                                    constraints[i] = ps[i];

                            //ALL CONSTRAINTS HAVE VALUE FROM HERE UNTIL '//x'
                            plots.Add(new quaternion
                                    (
                                    constraints[3].Value,
                                    constraints[0].Value,
                                    constraints[1].Value,
                                    constraints[2].Value
                                    ));

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
                                if (GuideLines[tile_index][1].Count == ps[0] - tmin)
                                    GuideLines[tile_index][1].Add(new List<pontus>());
                                if (GuideLines[tile_index][1].Count > ps[0] - tmin)
                                    GuideLines[tile_index][1][ps[0] - tmin].Add
                                        (new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                            constraints[0].Value, constraints[1].Value, constraints[2].Value, Globals.GraphingColour, GraphPanel, false, null));
                                else
                                    throw new DevError("DevError: Index out of sync when drawing Y-guidelines.");
                            }
                            if (free_parameter_flags[2])
                            {
                                if (GuideLines[tile_index][2].Count == (ps[1] - tmin) * (tmax + 1 - tmin) + (ps[0] - tmin))
                                    GuideLines[tile_index][2].Add(new List<pontus>());
                                if (GuideLines[tile_index][2].Count > (ps[1] - tmin) * (tmax + 1 - tmin) + (ps[0] - tmin))
                                    GuideLines[tile_index][2][(ps[1] - tmin) * (tmax + 1 - tmin) + (ps[0] - tmin)].Add
                                        (new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                            constraints[0].Value, constraints[1].Value, constraints[2].Value, Globals.GraphingColour, GraphPanel, false, null));
                                else
                                    throw new DevError("DevError: Index out of sync when drawing Z-guidelines.");
                            }
                            if (is_parametric)
                                GuideLines[tile_index][3][0].Add(new pontus(constraints[0].Value, constraints[1].Value, constraints[2].Value,
                                            constraints[0].Value, constraints[1].Value, constraints[2].Value, Globals.GraphingColour, GraphPanel, false, null));
                        }//x
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
                    if (GraphContents.ContainsKey("$" + tile.Parent.Controls.IndexOf(tile) + ":" + id))
                        GraphContents.Remove("$" + tile.Parent.Controls.IndexOf(tile) + ":" + id);
                    GraphContents.Add
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
        }
        Key_ValueTuple key_being_pressed = new Key_ValueTuple() { KeyChar = 'อ' };

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
    partial class Window
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Window));
            this.Banner = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.jpegToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FormMovingTimer = new System.Windows.Forms.Timer(this.components);
            this.CloseButton = new System.Windows.Forms.Button();
            this.MinimiseButton = new System.Windows.Forms.Button();
            this.MaximiseButton = new System.Windows.Forms.Button();
            this.AxisPanelX = new System.Windows.Forms.Panel();
            this.AxisDropdX = new System.Windows.Forms.ComboBox();
            this.AxisLabelX = new System.Windows.Forms.Label();
            this.AxisPanelY = new System.Windows.Forms.Panel();
            this.AxisDropdY = new System.Windows.Forms.ComboBox();
            this.AxisLabelY = new System.Windows.Forms.Label();
            this.AxisPanelZ = new System.Windows.Forms.Panel();
            this.AxisDropdZ = new System.Windows.Forms.ComboBox();
            this.AxisLabelZ = new System.Windows.Forms.Label();
            this.AxisPanelT = new System.Windows.Forms.Panel();
            this.AxisDropdT = new System.Windows.Forms.ComboBox();
            this.AxisLabelT = new System.Windows.Forms.Label();
            this.StylePanelWidth = new System.Windows.Forms.Panel();
            this.StyleDropdWidth = new System.Windows.Forms.ComboBox();
            this.StyleLabelWidth = new System.Windows.Forms.Label();
            this.StylePanelColour = new System.Windows.Forms.Panel();
            this.StyleBtnColour = new System.Windows.Forms.Button();
            this.StyleLabelColour = new System.Windows.Forms.Label();
            this.StylePanelFill = new System.Windows.Forms.Panel();
            this.StyleDropdFill = new System.Windows.Forms.ComboBox();
            this.StyleLabelFill = new System.Windows.Forms.Label();
            this.StyleColorDialog = new System.Windows.Forms.ColorDialog();
            this.TilePane = new System.Windows.Forms.Panel();
            this.InputPane = new System.Windows.Forms.SplitContainer();
            this.KeyPhi2 = new System.Windows.Forms.Button();
            this.KeyIota = new System.Windows.Forms.Button();
            this.KeyKappa = new System.Windows.Forms.Button();
            this.KeyEta = new System.Windows.Forms.Button();
            this.KeyRho = new System.Windows.Forms.Button();
            this.KeyMu = new System.Windows.Forms.Button();
            this.KeySigma2 = new System.Windows.Forms.Button();
            this.KeyUpsilon = new System.Windows.Forms.Button();
            this.KeyTau = new System.Windows.Forms.Button();
            this.KeyOmega = new System.Windows.Forms.Button();
            this.KeyPhi1 = new System.Windows.Forms.Button();
            this.KeyPi = new System.Windows.Forms.Button();
            this.KeyE = new System.Windows.Forms.Button();
            this.InputBtnUnderl = new System.Windows.Forms.Button();
            this.InputBtnItalic = new System.Windows.Forms.Button();
            this.InputBtnBold = new System.Windows.Forms.Button();
            this.KeyChi = new System.Windows.Forms.Button();
            this.KeyPsi = new System.Windows.Forms.Button();
            this.KeyNu = new System.Windows.Forms.Button();
            this.KeyXi = new System.Windows.Forms.Button();
            this.KeySigma1 = new System.Windows.Forms.Button();
            this.KeyTheta1 = new System.Windows.Forms.Button();
            this.KeyTheta2 = new System.Windows.Forms.Button();
            this.KeyLambda = new System.Windows.Forms.Button();
            this.KeyZeta = new System.Windows.Forms.Button();
            this.KeyEpsilon = new System.Windows.Forms.Button();
            this.KeyDelta = new System.Windows.Forms.Button();
            this.KeyGamma = new System.Windows.Forms.Button();
            this.KeyBeta = new System.Windows.Forms.Button();
            this.KeyAlpha = new System.Windows.Forms.Button();
            this.InputPanelTensor = new System.Windows.Forms.Panel();
            this.BtnAllTensor = new System.Windows.Forms.Button();
            this.InputPanelCalculus = new System.Windows.Forms.Panel();
            this.BtnAllCalculus = new System.Windows.Forms.Button();
            this.InputPanelScalar = new System.Windows.Forms.Panel();
            this.BtnAllScalar = new System.Windows.Forms.Button();
            this.GraphPanel = new System.Windows.Forms.Panel();
            this.ParamPane = new System.Windows.Forms.Panel();
            this.ParamSldr = new System.Windows.Forms.Panel();
            this.ParamSldrThmb = new System.Windows.Forms.Button();
            this.ParamSldrMax = new System.Windows.Forms.TextBox();
            this.ParamSldrMin = new System.Windows.Forms.TextBox();
            this.ParamBtn = new System.Windows.Forms.Button();
            this.ParamSldrThmbMovingTimer = new System.Windows.Forms.Timer(this.components);
            this.GraphPanTimer = new System.Windows.Forms.Timer(this.components);
            this.KeyPressDownTimer = new System.Windows.Forms.Timer(this.components);
            this.Banner.SuspendLayout();
            this.AxisPanelX.SuspendLayout();
            this.AxisPanelY.SuspendLayout();
            this.AxisPanelZ.SuspendLayout();
            this.AxisPanelT.SuspendLayout();
            this.StylePanelWidth.SuspendLayout();
            this.StylePanelColour.SuspendLayout();
            this.StylePanelFill.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InputPane)).BeginInit();
            this.InputPane.Panel1.SuspendLayout();
            this.InputPane.Panel2.SuspendLayout();
            this.InputPane.SuspendLayout();
            this.InputPanelTensor.SuspendLayout();
            this.InputPanelCalculus.SuspendLayout();
            this.InputPanelScalar.SuspendLayout();
            this.GraphPanel.SuspendLayout();
            this.ParamPane.SuspendLayout();
            this.ParamSldr.SuspendLayout();
            this.SuspendLayout();
            // 
            // Banner
            // 
            this.Banner.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Banner.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.Banner.Location = new System.Drawing.Point(0, 0);
            this.Banner.Name = "Banner";
            this.Banner.Size = new System.Drawing.Size(800, 24);
            this.Banner.TabIndex = 0;
            this.Banner.Text = "banner";
            this.Banner.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnBannerMouseDown);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.newToolStripMenuItem,
            this.loadToolStripMenuItem1,
            this.loadToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // loadToolStripMenuItem1
            // 
            this.loadToolStripMenuItem1.Name = "loadToolStripMenuItem1";
            this.loadToolStripMenuItem1.Size = new System.Drawing.Size(114, 22);
            this.loadToolStripMenuItem1.Text = "Load";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pngToolStripMenuItem,
            this.bmpToolStripMenuItem,
            this.jpegToolStripMenuItem});
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.loadToolStripMenuItem.Text = "Export";
            // 
            // pngToolStripMenuItem
            // 
            this.pngToolStripMenuItem.Name = "pngToolStripMenuItem";
            this.pngToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.pngToolStripMenuItem.Text = ".png";
            // 
            // bmpToolStripMenuItem
            // 
            this.bmpToolStripMenuItem.Name = "bmpToolStripMenuItem";
            this.bmpToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.bmpToolStripMenuItem.Text = ".bmp";
            // 
            // jpegToolStripMenuItem
            // 
            this.jpegToolStripMenuItem.Name = "jpegToolStripMenuItem";
            this.jpegToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.jpegToolStripMenuItem.Text = ".jpeg";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.clearToolStripMenuItem,
            this.findToolStripMenuItem,
            this.queryToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.findToolStripMenuItem.Text = "Find";
            // 
            // queryToolStripMenuItem
            // 
            this.queryToolStripMenuItem.Name = "queryToolStripMenuItem";
            this.queryToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.queryToolStripMenuItem.Text = "Query";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.viewToolStripMenuItem.Text = "Settings";
            this.viewToolStripMenuItem.Click += new System.EventHandler(this.OnSettingsBanner);
            // 
            // FormMovingTimer
            // 
            this.FormMovingTimer.Interval = 1;
            this.FormMovingTimer.Tick += new System.EventHandler(this.OnFormMovingTick);
            // 
            // CloseButton
            // 
            this.CloseButton.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Location = new System.Drawing.Point(776, 0);
            this.CloseButton.Margin = new System.Windows.Forms.Padding(0);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(24, 24);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.UseVisualStyleBackColor = false;
            this.CloseButton.Click += new System.EventHandler(this.OnClose);
            this.CloseButton.Paint += new System.Windows.Forms.PaintEventHandler(this.CloseButtonPaint);
            this.CloseButton.MouseLeave += new System.EventHandler(this.CloseMouseLeave);
            this.CloseButton.MouseHover += new System.EventHandler(this.CloseMouseHover);
            // 
            // MinimiseButton
            // 
            this.MinimiseButton.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MinimiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MinimiseButton.Location = new System.Drawing.Point(728, 0);
            this.MinimiseButton.Margin = new System.Windows.Forms.Padding(0);
            this.MinimiseButton.Name = "MinimiseButton";
            this.MinimiseButton.Size = new System.Drawing.Size(24, 24);
            this.MinimiseButton.TabIndex = 2;
            this.MinimiseButton.UseVisualStyleBackColor = false;
            this.MinimiseButton.Click += new System.EventHandler(this.OnMinimise);
            this.MinimiseButton.Paint += new System.Windows.Forms.PaintEventHandler(this.MinimisePaint);
            this.MinimiseButton.MouseLeave += new System.EventHandler(this.MinimiseMouseLeave);
            this.MinimiseButton.MouseHover += new System.EventHandler(this.MinimiseMouseHover);
            // 
            // MaximiseButton
            // 
            this.MaximiseButton.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MaximiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MaximiseButton.Location = new System.Drawing.Point(752, 0);
            this.MaximiseButton.Margin = new System.Windows.Forms.Padding(0);
            this.MaximiseButton.Name = "MaximiseButton";
            this.MaximiseButton.Size = new System.Drawing.Size(24, 24);
            this.MaximiseButton.TabIndex = 3;
            this.MaximiseButton.UseVisualStyleBackColor = false;
            this.MaximiseButton.Click += new System.EventHandler(this.OnMaximise);
            this.MaximiseButton.Paint += new System.Windows.Forms.PaintEventHandler(this.MaximisePaint);
            this.MaximiseButton.MouseLeave += new System.EventHandler(this.MaximiseMouseLeave);
            this.MaximiseButton.MouseHover += new System.EventHandler(this.MaximiseMouseHover);
            // 
            // AxisPanelX
            // 
            this.AxisPanelX.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.AxisPanelX.Controls.Add(this.AxisDropdX);
            this.AxisPanelX.Controls.Add(this.AxisLabelX);
            this.AxisPanelX.Location = new System.Drawing.Point(172, 0);
            this.AxisPanelX.Name = "AxisPanelX";
            this.AxisPanelX.Size = new System.Drawing.Size(67, 24);
            this.AxisPanelX.TabIndex = 5;
            // 
            // AxisDropdX
            // 
            this.AxisDropdX.FormattingEnabled = true;
            this.AxisDropdX.Items.AddRange(new object[] {
            "x",
            "y",
            "z",
            "t"});
            this.AxisDropdX.Location = new System.Drawing.Point(25, 2);
            this.AxisDropdX.Name = "AxisDropdX";
            this.AxisDropdX.Size = new System.Drawing.Size(42, 21);
            this.AxisDropdX.TabIndex = 1;
            // 
            // AxisLabelX
            // 
            this.AxisLabelX.AutoSize = true;
            this.AxisLabelX.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AxisLabelX.Location = new System.Drawing.Point(3, 2);
            this.AxisLabelX.Name = "AxisLabelX";
            this.AxisLabelX.Size = new System.Drawing.Size(20, 20);
            this.AxisLabelX.TabIndex = 0;
            this.AxisLabelX.Text = "x:";
            // 
            // AxisPanelY
            // 
            this.AxisPanelY.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.AxisPanelY.Controls.Add(this.AxisDropdY);
            this.AxisPanelY.Controls.Add(this.AxisLabelY);
            this.AxisPanelY.Location = new System.Drawing.Point(245, 0);
            this.AxisPanelY.Name = "AxisPanelY";
            this.AxisPanelY.Size = new System.Drawing.Size(67, 24);
            this.AxisPanelY.TabIndex = 6;
            // 
            // AxisDropdY
            // 
            this.AxisDropdY.FormattingEnabled = true;
            this.AxisDropdY.Items.AddRange(new object[] {
            "x",
            "y",
            "z",
            "t"});
            this.AxisDropdY.Location = new System.Drawing.Point(25, 2);
            this.AxisDropdY.Name = "AxisDropdY";
            this.AxisDropdY.Size = new System.Drawing.Size(42, 21);
            this.AxisDropdY.TabIndex = 1;
            // 
            // AxisLabelY
            // 
            this.AxisLabelY.AutoSize = true;
            this.AxisLabelY.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AxisLabelY.Location = new System.Drawing.Point(3, 2);
            this.AxisLabelY.Name = "AxisLabelY";
            this.AxisLabelY.Size = new System.Drawing.Size(20, 20);
            this.AxisLabelY.TabIndex = 0;
            this.AxisLabelY.Text = "y:";
            // 
            // AxisPanelZ
            // 
            this.AxisPanelZ.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.AxisPanelZ.Controls.Add(this.AxisDropdZ);
            this.AxisPanelZ.Controls.Add(this.AxisLabelZ);
            this.AxisPanelZ.Location = new System.Drawing.Point(318, 0);
            this.AxisPanelZ.Name = "AxisPanelZ";
            this.AxisPanelZ.Size = new System.Drawing.Size(67, 24);
            this.AxisPanelZ.TabIndex = 7;
            // 
            // AxisDropdZ
            // 
            this.AxisDropdZ.FormattingEnabled = true;
            this.AxisDropdZ.Items.AddRange(new object[] {
            "x",
            "y",
            "z",
            "t"});
            this.AxisDropdZ.Location = new System.Drawing.Point(25, 2);
            this.AxisDropdZ.Name = "AxisDropdZ";
            this.AxisDropdZ.Size = new System.Drawing.Size(42, 21);
            this.AxisDropdZ.TabIndex = 1;
            // 
            // AxisLabelZ
            // 
            this.AxisLabelZ.AutoSize = true;
            this.AxisLabelZ.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AxisLabelZ.Location = new System.Drawing.Point(3, 2);
            this.AxisLabelZ.Name = "AxisLabelZ";
            this.AxisLabelZ.Size = new System.Drawing.Size(21, 20);
            this.AxisLabelZ.TabIndex = 0;
            this.AxisLabelZ.Text = "z:";
            // 
            // AxisPanelT
            // 
            this.AxisPanelT.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.AxisPanelT.Controls.Add(this.AxisDropdT);
            this.AxisPanelT.Controls.Add(this.AxisLabelT);
            this.AxisPanelT.Location = new System.Drawing.Point(391, 0);
            this.AxisPanelT.Name = "AxisPanelT";
            this.AxisPanelT.Size = new System.Drawing.Size(67, 24);
            this.AxisPanelT.TabIndex = 8;
            // 
            // AxisDropdT
            // 
            this.AxisDropdT.FormattingEnabled = true;
            this.AxisDropdT.Items.AddRange(new object[] {
            "x",
            "y",
            "z",
            "t"});
            this.AxisDropdT.Location = new System.Drawing.Point(25, 2);
            this.AxisDropdT.Name = "AxisDropdT";
            this.AxisDropdT.Size = new System.Drawing.Size(42, 21);
            this.AxisDropdT.TabIndex = 1;
            // 
            // AxisLabelT
            // 
            this.AxisLabelT.AutoSize = true;
            this.AxisLabelT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AxisLabelT.Location = new System.Drawing.Point(3, 2);
            this.AxisLabelT.Name = "AxisLabelT";
            this.AxisLabelT.Size = new System.Drawing.Size(18, 20);
            this.AxisLabelT.TabIndex = 0;
            this.AxisLabelT.Text = "t:";
            // 
            // StylePanelWidth
            // 
            this.StylePanelWidth.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.StylePanelWidth.Controls.Add(this.StyleDropdWidth);
            this.StylePanelWidth.Controls.Add(this.StyleLabelWidth);
            this.StylePanelWidth.Location = new System.Drawing.Point(500, 0);
            this.StylePanelWidth.Name = "StylePanelWidth";
            this.StylePanelWidth.Size = new System.Drawing.Size(85, 24);
            this.StylePanelWidth.TabIndex = 9;
            // 
            // StyleDropdWidth
            // 
            this.StyleDropdWidth.FormattingEnabled = true;
            this.StyleDropdWidth.Items.AddRange(new object[] {
            "1px",
            "3px",
            "5px",
            "7px",
            "11px"});
            this.StyleDropdWidth.Location = new System.Drawing.Point(40, 2);
            this.StyleDropdWidth.Name = "StyleDropdWidth";
            this.StyleDropdWidth.Size = new System.Drawing.Size(42, 21);
            this.StyleDropdWidth.TabIndex = 1;
            // 
            // StyleLabelWidth
            // 
            this.StyleLabelWidth.AutoSize = true;
            this.StyleLabelWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StyleLabelWidth.Location = new System.Drawing.Point(3, 4);
            this.StyleLabelWidth.Name = "StyleLabelWidth";
            this.StyleLabelWidth.Size = new System.Drawing.Size(39, 15);
            this.StyleLabelWidth.TabIndex = 0;
            this.StyleLabelWidth.Text = "width:";
            // 
            // StylePanelColour
            // 
            this.StylePanelColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.StylePanelColour.Controls.Add(this.StyleBtnColour);
            this.StylePanelColour.Controls.Add(this.StyleLabelColour);
            this.StylePanelColour.Location = new System.Drawing.Point(658, 0);
            this.StylePanelColour.Name = "StylePanelColour";
            this.StylePanelColour.Size = new System.Drawing.Size(67, 24);
            this.StylePanelColour.TabIndex = 6;
            // 
            // StyleBtnColour
            // 
            this.StyleBtnColour.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StyleBtnColour.Location = new System.Drawing.Point(27, 0);
            this.StyleBtnColour.Name = "StyleBtnColour";
            this.StyleBtnColour.Size = new System.Drawing.Size(40, 24);
            this.StyleBtnColour.TabIndex = 1;
            this.StyleBtnColour.UseVisualStyleBackColor = true;
            this.StyleBtnColour.Click += new System.EventHandler(this.OnStyleBtnColour);
            // 
            // StyleLabelColour
            // 
            this.StyleLabelColour.AutoSize = true;
            this.StyleLabelColour.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StyleLabelColour.Location = new System.Drawing.Point(3, 2);
            this.StyleLabelColour.Name = "StyleLabelColour";
            this.StyleLabelColour.Size = new System.Drawing.Size(26, 15);
            this.StyleLabelColour.TabIndex = 0;
            this.StyleLabelColour.Text = "col:";
            // 
            // StylePanelFill
            // 
            this.StylePanelFill.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.StylePanelFill.Controls.Add(this.StyleDropdFill);
            this.StylePanelFill.Controls.Add(this.StyleLabelFill);
            this.StylePanelFill.Location = new System.Drawing.Point(588, 0);
            this.StylePanelFill.Name = "StylePanelFill";
            this.StylePanelFill.Size = new System.Drawing.Size(67, 24);
            this.StylePanelFill.TabIndex = 10;
            // 
            // StyleDropdFill
            // 
            this.StyleDropdFill.FormattingEnabled = true;
            this.StyleDropdFill.Items.AddRange(new object[] {
            "x",
            "y",
            "z",
            "t"});
            this.StyleDropdFill.Location = new System.Drawing.Point(25, 2);
            this.StyleDropdFill.Name = "StyleDropdFill";
            this.StyleDropdFill.Size = new System.Drawing.Size(42, 21);
            this.StyleDropdFill.TabIndex = 1;
            // 
            // StyleLabelFill
            // 
            this.StyleLabelFill.AutoSize = true;
            this.StyleLabelFill.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StyleLabelFill.Location = new System.Drawing.Point(3, 2);
            this.StyleLabelFill.Name = "StyleLabelFill";
            this.StyleLabelFill.Size = new System.Drawing.Size(22, 15);
            this.StyleLabelFill.TabIndex = 0;
            this.StyleLabelFill.Text = "fill:";
            // 
            // StyleColorDialog
            // 
            this.StyleColorDialog.AnyColor = true;
            this.StyleColorDialog.SolidColorOnly = true;
            // 
            // TilePane
            // 
            this.TilePane.Location = new System.Drawing.Point(0, 25);
            this.TilePane.Name = "TilePane";
            this.TilePane.Size = new System.Drawing.Size(200, 400);
            this.TilePane.TabIndex = 11;
            // 
            // InputPane
            // 
            this.InputPane.Location = new System.Drawing.Point(0, 425);
            this.InputPane.Name = "InputPane";
            // 
            // InputPane.Panel1
            // 
            this.InputPane.Panel1.Controls.Add(this.KeyPhi2);
            this.InputPane.Panel1.Controls.Add(this.KeyIota);
            this.InputPane.Panel1.Controls.Add(this.KeyKappa);
            this.InputPane.Panel1.Controls.Add(this.KeyEta);
            this.InputPane.Panel1.Controls.Add(this.KeyRho);
            this.InputPane.Panel1.Controls.Add(this.KeyMu);
            this.InputPane.Panel1.Controls.Add(this.KeySigma2);
            this.InputPane.Panel1.Controls.Add(this.KeyUpsilon);
            this.InputPane.Panel1.Controls.Add(this.KeyTau);
            this.InputPane.Panel1.Controls.Add(this.KeyOmega);
            this.InputPane.Panel1.Controls.Add(this.KeyPhi1);
            this.InputPane.Panel1.Controls.Add(this.KeyPi);
            this.InputPane.Panel1.Controls.Add(this.KeyE);
            this.InputPane.Panel1.Controls.Add(this.InputBtnUnderl);
            this.InputPane.Panel1.Controls.Add(this.InputBtnItalic);
            this.InputPane.Panel1.Controls.Add(this.InputBtnBold);
            this.InputPane.Panel1.Controls.Add(this.KeyChi);
            this.InputPane.Panel1.Controls.Add(this.KeyPsi);
            this.InputPane.Panel1.Controls.Add(this.KeyNu);
            this.InputPane.Panel1.Controls.Add(this.KeyXi);
            this.InputPane.Panel1.Controls.Add(this.KeySigma1);
            this.InputPane.Panel1.Controls.Add(this.KeyTheta1);
            this.InputPane.Panel1.Controls.Add(this.KeyTheta2);
            this.InputPane.Panel1.Controls.Add(this.KeyLambda);
            this.InputPane.Panel1.Controls.Add(this.KeyZeta);
            this.InputPane.Panel1.Controls.Add(this.KeyEpsilon);
            this.InputPane.Panel1.Controls.Add(this.KeyDelta);
            this.InputPane.Panel1.Controls.Add(this.KeyGamma);
            this.InputPane.Panel1.Controls.Add(this.KeyBeta);
            this.InputPane.Panel1.Controls.Add(this.KeyAlpha);
            // 
            // InputPane.Panel2
            // 
            this.InputPane.Panel2.Controls.Add(this.InputPanelTensor);
            this.InputPane.Panel2.Controls.Add(this.InputPanelCalculus);
            this.InputPane.Panel2.Controls.Add(this.InputPanelScalar);
            this.InputPane.Size = new System.Drawing.Size(800, 175);
            this.InputPane.SplitterDistance = 300;
            this.InputPane.TabIndex = 12;
            // 
            // KeyPhi2
            // 
            this.KeyPhi2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyPhi2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyPhi2.Image = ((System.Drawing.Image)(resources.GetObject("KeyPhi2.Image")));
            this.KeyPhi2.Location = new System.Drawing.Point(100, 105);
            this.KeyPhi2.Name = "KeyPhi2";
            this.KeyPhi2.Size = new System.Drawing.Size(50, 35);
            this.KeyPhi2.TabIndex = 15;
            this.KeyPhi2.UseVisualStyleBackColor = false;
            // 
            // KeyIota
            // 
            this.KeyIota.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyIota.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyIota.Image = ((System.Drawing.Image)(resources.GetObject("KeyIota.Image")));
            this.KeyIota.Location = new System.Drawing.Point(150, 35);
            this.KeyIota.Name = "KeyIota";
            this.KeyIota.Size = new System.Drawing.Size(50, 35);
            this.KeyIota.TabIndex = 9;
            this.KeyIota.UseVisualStyleBackColor = false;
            // 
            // KeyKappa
            // 
            this.KeyKappa.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyKappa.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyKappa.Image = ((System.Drawing.Image)(resources.GetObject("KeyKappa.Image")));
            this.KeyKappa.Location = new System.Drawing.Point(200, 35);
            this.KeyKappa.Name = "KeyKappa";
            this.KeyKappa.Size = new System.Drawing.Size(50, 35);
            this.KeyKappa.TabIndex = 8;
            this.KeyKappa.UseVisualStyleBackColor = false;
            // 
            // KeyEta
            // 
            this.KeyEta.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyEta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyEta.Image = ((System.Drawing.Image)(resources.GetObject("KeyEta.Image")));
            this.KeyEta.Location = new System.Drawing.Point(0, 35);
            this.KeyEta.Name = "KeyEta";
            this.KeyEta.Size = new System.Drawing.Size(50, 35);
            this.KeyEta.TabIndex = 12;
            this.KeyEta.UseVisualStyleBackColor = false;
            // 
            // KeyRho
            // 
            this.KeyRho.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyRho.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyRho.Image = ((System.Drawing.Image)(resources.GetObject("KeyRho.Image")));
            this.KeyRho.Location = new System.Drawing.Point(150, 70);
            this.KeyRho.Name = "KeyRho";
            this.KeyRho.Size = new System.Drawing.Size(50, 35);
            this.KeyRho.TabIndex = 14;
            this.KeyRho.UseVisualStyleBackColor = false;
            // 
            // KeyMu
            // 
            this.KeyMu.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyMu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyMu.Image = ((System.Drawing.Image)(resources.GetObject("KeyMu.Image")));
            this.KeyMu.Location = new System.Drawing.Point(0, 70);
            this.KeyMu.Name = "KeyMu";
            this.KeyMu.Size = new System.Drawing.Size(50, 35);
            this.KeyMu.TabIndex = 18;
            this.KeyMu.UseVisualStyleBackColor = false;
            // 
            // KeySigma2
            // 
            this.KeySigma2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeySigma2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeySigma2.Image = ((System.Drawing.Image)(resources.GetObject("KeySigma2.Image")));
            this.KeySigma2.Location = new System.Drawing.Point(250, 70);
            this.KeySigma2.Name = "KeySigma2";
            this.KeySigma2.Size = new System.Drawing.Size(50, 35);
            this.KeySigma2.TabIndex = 24;
            this.KeySigma2.UseVisualStyleBackColor = false;
            // 
            // KeyUpsilon
            // 
            this.KeyUpsilon.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyUpsilon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyUpsilon.Image = ((System.Drawing.Image)(resources.GetObject("KeyUpsilon.Image")));
            this.KeyUpsilon.Location = new System.Drawing.Point(50, 105);
            this.KeyUpsilon.Name = "KeyUpsilon";
            this.KeyUpsilon.Size = new System.Drawing.Size(50, 35);
            this.KeyUpsilon.TabIndex = 22;
            this.KeyUpsilon.UseVisualStyleBackColor = false;
            // 
            // KeyTau
            // 
            this.KeyTau.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyTau.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyTau.Image = ((System.Drawing.Image)(resources.GetObject("KeyTau.Image")));
            this.KeyTau.Location = new System.Drawing.Point(0, 105);
            this.KeyTau.Name = "KeyTau";
            this.KeyTau.Size = new System.Drawing.Size(50, 35);
            this.KeyTau.TabIndex = 23;
            this.KeyTau.UseVisualStyleBackColor = false;
            // 
            // KeyOmega
            // 
            this.KeyOmega.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyOmega.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyOmega.Image = ((System.Drawing.Image)(resources.GetObject("KeyOmega.Image")));
            this.KeyOmega.Location = new System.Drawing.Point(250, 105);
            this.KeyOmega.Name = "KeyOmega";
            this.KeyOmega.Size = new System.Drawing.Size(50, 35);
            this.KeyOmega.TabIndex = 30;
            this.KeyOmega.UseVisualStyleBackColor = false;
            // 
            // KeyPhi1
            // 
            this.KeyPhi1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.KeyPhi1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyPhi1.Image = ((System.Drawing.Image)(resources.GetObject("KeyPhi1.Image")));
            this.KeyPhi1.Location = new System.Drawing.Point(250, 140);
            this.KeyPhi1.Name = "KeyPhi1";
            this.KeyPhi1.Size = new System.Drawing.Size(50, 35);
            this.KeyPhi1.TabIndex = 21;
            this.KeyPhi1.UseVisualStyleBackColor = false;
            // 
            // KeyPi
            // 
            this.KeyPi.BackColor = System.Drawing.SystemColors.ControlLight;
            this.KeyPi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyPi.Image = ((System.Drawing.Image)(resources.GetObject("KeyPi.Image")));
            this.KeyPi.Location = new System.Drawing.Point(200, 140);
            this.KeyPi.Name = "KeyPi";
            this.KeyPi.Size = new System.Drawing.Size(50, 35);
            this.KeyPi.TabIndex = 29;
            this.KeyPi.UseVisualStyleBackColor = false;
            // 
            // KeyE
            // 
            this.KeyE.BackColor = System.Drawing.SystemColors.ControlLight;
            this.KeyE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyE.Font = new System.Drawing.Font("Cambria", 14.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyE.Location = new System.Drawing.Point(150, 140);
            this.KeyE.Name = "KeyE";
            this.KeyE.Size = new System.Drawing.Size(50, 35);
            this.KeyE.TabIndex = 28;
            this.KeyE.Text = "e";
            this.KeyE.UseVisualStyleBackColor = false;
            // 
            // InputBtnUnderl
            // 
            this.InputBtnUnderl.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.InputBtnUnderl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InputBtnUnderl.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputBtnUnderl.Location = new System.Drawing.Point(100, 140);
            this.InputBtnUnderl.Name = "InputBtnUnderl";
            this.InputBtnUnderl.Size = new System.Drawing.Size(50, 35);
            this.InputBtnUnderl.TabIndex = 27;
            this.InputBtnUnderl.Text = "U";
            this.InputBtnUnderl.UseVisualStyleBackColor = false;
            // 
            // InputBtnItalic
            // 
            this.InputBtnItalic.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.InputBtnItalic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InputBtnItalic.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputBtnItalic.Location = new System.Drawing.Point(50, 140);
            this.InputBtnItalic.Name = "InputBtnItalic";
            this.InputBtnItalic.Size = new System.Drawing.Size(50, 35);
            this.InputBtnItalic.TabIndex = 26;
            this.InputBtnItalic.Text = "I";
            this.InputBtnItalic.UseVisualStyleBackColor = false;
            // 
            // InputBtnBold
            // 
            this.InputBtnBold.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.InputBtnBold.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InputBtnBold.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputBtnBold.Location = new System.Drawing.Point(0, 140);
            this.InputBtnBold.Name = "InputBtnBold";
            this.InputBtnBold.Size = new System.Drawing.Size(50, 35);
            this.InputBtnBold.TabIndex = 25;
            this.InputBtnBold.Text = "B";
            this.InputBtnBold.UseVisualStyleBackColor = false;
            // 
            // KeyChi
            // 
            this.KeyChi.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyChi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyChi.Image = ((System.Drawing.Image)(resources.GetObject("KeyChi.Image")));
            this.KeyChi.Location = new System.Drawing.Point(150, 105);
            this.KeyChi.Name = "KeyChi";
            this.KeyChi.Size = new System.Drawing.Size(50, 35);
            this.KeyChi.TabIndex = 20;
            this.KeyChi.UseVisualStyleBackColor = false;
            // 
            // KeyPsi
            // 
            this.KeyPsi.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyPsi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyPsi.Image = ((System.Drawing.Image)(resources.GetObject("KeyPsi.Image")));
            this.KeyPsi.Location = new System.Drawing.Point(200, 105);
            this.KeyPsi.Name = "KeyPsi";
            this.KeyPsi.Size = new System.Drawing.Size(50, 35);
            this.KeyPsi.TabIndex = 19;
            this.KeyPsi.UseVisualStyleBackColor = false;
            // 
            // KeyNu
            // 
            this.KeyNu.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyNu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyNu.Image = ((System.Drawing.Image)(resources.GetObject("KeyNu.Image")));
            this.KeyNu.Location = new System.Drawing.Point(50, 70);
            this.KeyNu.Name = "KeyNu";
            this.KeyNu.Size = new System.Drawing.Size(50, 35);
            this.KeyNu.TabIndex = 17;
            this.KeyNu.UseVisualStyleBackColor = false;
            // 
            // KeyXi
            // 
            this.KeyXi.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyXi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyXi.Image = ((System.Drawing.Image)(resources.GetObject("KeyXi.Image")));
            this.KeyXi.Location = new System.Drawing.Point(100, 70);
            this.KeyXi.Name = "KeyXi";
            this.KeyXi.Size = new System.Drawing.Size(50, 35);
            this.KeyXi.TabIndex = 16;
            this.KeyXi.UseVisualStyleBackColor = false;
            // 
            // KeySigma1
            // 
            this.KeySigma1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeySigma1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeySigma1.Image = ((System.Drawing.Image)(resources.GetObject("KeySigma1.Image")));
            this.KeySigma1.Location = new System.Drawing.Point(200, 70);
            this.KeySigma1.Name = "KeySigma1";
            this.KeySigma1.Size = new System.Drawing.Size(50, 35);
            this.KeySigma1.TabIndex = 13;
            this.KeySigma1.UseVisualStyleBackColor = false;
            // 
            // KeyTheta1
            // 
            this.KeyTheta1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyTheta1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyTheta1.Image = ((System.Drawing.Image)(resources.GetObject("KeyTheta1.Image")));
            this.KeyTheta1.Location = new System.Drawing.Point(50, 35);
            this.KeyTheta1.Name = "KeyTheta1";
            this.KeyTheta1.Size = new System.Drawing.Size(50, 35);
            this.KeyTheta1.TabIndex = 11;
            this.KeyTheta1.UseVisualStyleBackColor = false;
            // 
            // KeyTheta2
            // 
            this.KeyTheta2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyTheta2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyTheta2.Image = ((System.Drawing.Image)(resources.GetObject("KeyTheta2.Image")));
            this.KeyTheta2.Location = new System.Drawing.Point(100, 35);
            this.KeyTheta2.Name = "KeyTheta2";
            this.KeyTheta2.Size = new System.Drawing.Size(50, 35);
            this.KeyTheta2.TabIndex = 10;
            this.KeyTheta2.UseVisualStyleBackColor = false;
            // 
            // KeyLambda
            // 
            this.KeyLambda.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyLambda.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyLambda.Image = ((System.Drawing.Image)(resources.GetObject("KeyLambda.Image")));
            this.KeyLambda.Location = new System.Drawing.Point(250, 35);
            this.KeyLambda.Name = "KeyLambda";
            this.KeyLambda.Size = new System.Drawing.Size(50, 35);
            this.KeyLambda.TabIndex = 7;
            this.KeyLambda.UseVisualStyleBackColor = false;
            // 
            // KeyZeta
            // 
            this.KeyZeta.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyZeta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyZeta.Image = ((System.Drawing.Image)(resources.GetObject("KeyZeta.Image")));
            this.KeyZeta.Location = new System.Drawing.Point(250, 0);
            this.KeyZeta.Name = "KeyZeta";
            this.KeyZeta.Size = new System.Drawing.Size(50, 35);
            this.KeyZeta.TabIndex = 6;
            this.KeyZeta.UseVisualStyleBackColor = false;
            // 
            // KeyEpsilon
            // 
            this.KeyEpsilon.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyEpsilon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyEpsilon.Image = ((System.Drawing.Image)(resources.GetObject("KeyEpsilon.Image")));
            this.KeyEpsilon.Location = new System.Drawing.Point(200, 0);
            this.KeyEpsilon.Name = "KeyEpsilon";
            this.KeyEpsilon.Size = new System.Drawing.Size(50, 35);
            this.KeyEpsilon.TabIndex = 5;
            this.KeyEpsilon.UseVisualStyleBackColor = false;
            // 
            // KeyDelta
            // 
            this.KeyDelta.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyDelta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyDelta.Image = ((System.Drawing.Image)(resources.GetObject("KeyDelta.Image")));
            this.KeyDelta.Location = new System.Drawing.Point(150, 0);
            this.KeyDelta.Name = "KeyDelta";
            this.KeyDelta.Size = new System.Drawing.Size(50, 35);
            this.KeyDelta.TabIndex = 4;
            this.KeyDelta.UseVisualStyleBackColor = false;
            // 
            // KeyGamma
            // 
            this.KeyGamma.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyGamma.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyGamma.Image = ((System.Drawing.Image)(resources.GetObject("KeyGamma.Image")));
            this.KeyGamma.Location = new System.Drawing.Point(100, 0);
            this.KeyGamma.Name = "KeyGamma";
            this.KeyGamma.Size = new System.Drawing.Size(50, 35);
            this.KeyGamma.TabIndex = 3;
            this.KeyGamma.UseVisualStyleBackColor = false;
            // 
            // KeyBeta
            // 
            this.KeyBeta.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyBeta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyBeta.Image = ((System.Drawing.Image)(resources.GetObject("KeyBeta.Image")));
            this.KeyBeta.Location = new System.Drawing.Point(50, 0);
            this.KeyBeta.Name = "KeyBeta";
            this.KeyBeta.Size = new System.Drawing.Size(50, 35);
            this.KeyBeta.TabIndex = 2;
            this.KeyBeta.UseVisualStyleBackColor = false;
            // 
            // KeyAlpha
            // 
            this.KeyAlpha.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.KeyAlpha.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyAlpha.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyAlpha.Image = ((System.Drawing.Image)(resources.GetObject("KeyAlpha.Image")));
            this.KeyAlpha.Location = new System.Drawing.Point(0, 0);
            this.KeyAlpha.Name = "KeyAlpha";
            this.KeyAlpha.Size = new System.Drawing.Size(50, 35);
            this.KeyAlpha.TabIndex = 1;
            this.KeyAlpha.UseVisualStyleBackColor = false;
            // 
            // InputPanelTensor
            // 
            this.InputPanelTensor.BackColor = System.Drawing.SystemColors.ControlLight;
            this.InputPanelTensor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputPanelTensor.Controls.Add(this.BtnAllTensor);
            this.InputPanelTensor.Location = new System.Drawing.Point(0, 116);
            this.InputPanelTensor.Name = "InputPanelTensor";
            this.InputPanelTensor.Size = new System.Drawing.Size(496, 58);
            this.InputPanelTensor.TabIndex = 2;
            // 
            // BtnAllTensor
            // 
            this.BtnAllTensor.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.BtnAllTensor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnAllTensor.Location = new System.Drawing.Point(450, 0);
            this.BtnAllTensor.Name = "BtnAllTensor";
            this.BtnAllTensor.Size = new System.Drawing.Size(46, 58);
            this.BtnAllTensor.TabIndex = 0;
            this.BtnAllTensor.Text = ". . .";
            this.BtnAllTensor.UseVisualStyleBackColor = false;
            // 
            // InputPanelCalculus
            // 
            this.InputPanelCalculus.BackColor = System.Drawing.SystemColors.ControlLight;
            this.InputPanelCalculus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputPanelCalculus.Controls.Add(this.BtnAllCalculus);
            this.InputPanelCalculus.Location = new System.Drawing.Point(0, 58);
            this.InputPanelCalculus.Name = "InputPanelCalculus";
            this.InputPanelCalculus.Size = new System.Drawing.Size(496, 58);
            this.InputPanelCalculus.TabIndex = 1;
            // 
            // BtnAllCalculus
            // 
            this.BtnAllCalculus.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.BtnAllCalculus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnAllCalculus.Location = new System.Drawing.Point(450, 0);
            this.BtnAllCalculus.Name = "BtnAllCalculus";
            this.BtnAllCalculus.Size = new System.Drawing.Size(46, 58);
            this.BtnAllCalculus.TabIndex = 0;
            this.BtnAllCalculus.Text = ". . .";
            this.BtnAllCalculus.UseVisualStyleBackColor = false;
            // 
            // InputPanelScalar
            // 
            this.InputPanelScalar.BackColor = System.Drawing.SystemColors.ControlLight;
            this.InputPanelScalar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputPanelScalar.Controls.Add(this.BtnAllScalar);
            this.InputPanelScalar.Location = new System.Drawing.Point(0, 0);
            this.InputPanelScalar.Name = "InputPanelScalar";
            this.InputPanelScalar.Size = new System.Drawing.Size(496, 58);
            this.InputPanelScalar.TabIndex = 0;
            // 
            // BtnAllScalar
            // 
            this.BtnAllScalar.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.BtnAllScalar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnAllScalar.Location = new System.Drawing.Point(450, 0);
            this.BtnAllScalar.Name = "BtnAllScalar";
            this.BtnAllScalar.Size = new System.Drawing.Size(46, 58);
            this.BtnAllScalar.TabIndex = 0;
            this.BtnAllScalar.Text = ". . .";
            this.BtnAllScalar.UseVisualStyleBackColor = false;
            // 
            // GraphPanel
            // 
            this.GraphPanel.Controls.Add(this.ParamPane);
            this.GraphPanel.Location = new System.Drawing.Point(200, 25);
            this.GraphPanel.Name = "GraphPanel";
            this.GraphPanel.Size = new System.Drawing.Size(600, 400);
            this.GraphPanel.TabIndex = 13;
            this.GraphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphPanel_Paint);
            this.GraphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnGraphMouseDown);
            this.GraphPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnGraphMouseUp);
            // 
            // ParamPane
            // 
            this.ParamPane.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ParamPane.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParamPane.Controls.Add(this.ParamSldr);
            this.ParamPane.Controls.Add(this.ParamBtn);
            this.ParamPane.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ParamPane.Location = new System.Drawing.Point(0, 360);
            this.ParamPane.Name = "ParamPane";
            this.ParamPane.Size = new System.Drawing.Size(600, 40);
            this.ParamPane.TabIndex = 0;
            // 
            // ParamSldr
            // 
            this.ParamSldr.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ParamSldr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParamSldr.Controls.Add(this.ParamSldrThmb);
            this.ParamSldr.Controls.Add(this.ParamSldrMax);
            this.ParamSldr.Controls.Add(this.ParamSldrMin);
            this.ParamSldr.Location = new System.Drawing.Point(75, 10);
            this.ParamSldr.Name = "ParamSldr";
            this.ParamSldr.Size = new System.Drawing.Size(520, 20);
            this.ParamSldr.TabIndex = 1;
            // 
            // ParamSldrThmb
            // 
            this.ParamSldrThmb.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ParamSldrThmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ParamSldrThmb.Location = new System.Drawing.Point(47, -1);
            this.ParamSldrThmb.Name = "ParamSldrThmb";
            this.ParamSldrThmb.Size = new System.Drawing.Size(20, 20);
            this.ParamSldrThmb.TabIndex = 2;
            this.ParamSldrThmb.UseVisualStyleBackColor = false;
            this.ParamSldrThmb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnParamSldrThmbMouseDown);
            this.ParamSldrThmb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnParamSldrThmbMouseUp);
            // 
            // ParamSldrMax
            // 
            this.ParamSldrMax.Dock = System.Windows.Forms.DockStyle.Right;
            this.ParamSldrMax.Location = new System.Drawing.Point(473, 0);
            this.ParamSldrMax.Name = "ParamSldrMax";
            this.ParamSldrMax.Size = new System.Drawing.Size(45, 20);
            this.ParamSldrMax.TabIndex = 1;
            this.ParamSldrMax.Text = "10";
            this.ParamSldrMax.TextChanged += new System.EventHandler(this.OnParamSldrMaxChanged);
            // 
            // ParamSldrMin
            // 
            this.ParamSldrMin.Dock = System.Windows.Forms.DockStyle.Left;
            this.ParamSldrMin.Location = new System.Drawing.Point(0, 0);
            this.ParamSldrMin.Name = "ParamSldrMin";
            this.ParamSldrMin.Size = new System.Drawing.Size(45, 20);
            this.ParamSldrMin.TabIndex = 0;
            this.ParamSldrMin.Text = "0";
            this.ParamSldrMin.TextChanged += new System.EventHandler(this.OnParamSldrMinChanged);
            // 
            // ParamBtn
            // 
            this.ParamBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ParamBtn.Location = new System.Drawing.Point(-1, -1);
            this.ParamBtn.Name = "ParamBtn";
            this.ParamBtn.Size = new System.Drawing.Size(50, 40);
            this.ParamBtn.TabIndex = 0;
            this.ParamBtn.Text = "Back";
            this.ParamBtn.UseVisualStyleBackColor = true;
            this.ParamBtn.Click += new System.EventHandler(this.OnParamBtn);
            // 
            // ParamSldrThmbMovingTimer
            // 
            this.ParamSldrThmbMovingTimer.Interval = 1;
            this.ParamSldrThmbMovingTimer.Tick += new System.EventHandler(this.OnParamSldrThmbMovingTick);
            // 
            // GraphPanTimer
            // 
            this.GraphPanTimer.Interval = 1;
            this.GraphPanTimer.Tick += new System.EventHandler(this.OnGraphPanTick);
            // 
            // KeyPressDownTimer
            // 
            this.KeyPressDownTimer.Interval = 1;
            this.KeyPressDownTimer.Tick += new System.EventHandler(this.KeyPressDownTick);
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.GraphPanel);
            this.Controls.Add(this.InputPane);
            this.Controls.Add(this.TilePane);
            this.Controls.Add(this.StylePanelFill);
            this.Controls.Add(this.StylePanelColour);
            this.Controls.Add(this.StylePanelWidth);
            this.Controls.Add(this.AxisPanelT);
            this.Controls.Add(this.AxisPanelZ);
            this.Controls.Add(this.AxisPanelY);
            this.Controls.Add(this.AxisPanelX);
            this.Controls.Add(this.MaximiseButton);
            this.Controls.Add(this.MinimiseButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.Banner);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MainMenuStrip = this.Banner;
            this.Name = "Window";
            this.Text = "GCalc";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            this.Banner.ResumeLayout(false);
            this.Banner.PerformLayout();
            this.AxisPanelX.ResumeLayout(false);
            this.AxisPanelX.PerformLayout();
            this.AxisPanelY.ResumeLayout(false);
            this.AxisPanelY.PerformLayout();
            this.AxisPanelZ.ResumeLayout(false);
            this.AxisPanelZ.PerformLayout();
            this.AxisPanelT.ResumeLayout(false);
            this.AxisPanelT.PerformLayout();
            this.StylePanelWidth.ResumeLayout(false);
            this.StylePanelWidth.PerformLayout();
            this.StylePanelColour.ResumeLayout(false);
            this.StylePanelColour.PerformLayout();
            this.StylePanelFill.ResumeLayout(false);
            this.StylePanelFill.PerformLayout();
            this.InputPane.Panel1.ResumeLayout(false);
            this.InputPane.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.InputPane)).EndInit();
            this.InputPane.ResumeLayout(false);
            this.InputPanelTensor.ResumeLayout(false);
            this.InputPanelCalculus.ResumeLayout(false);
            this.InputPanelScalar.ResumeLayout(false);
            this.GraphPanel.ResumeLayout(false);
            this.ParamPane.ResumeLayout(false);
            this.ParamSldr.ResumeLayout(false);
            this.ParamSldr.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip Banner;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jpegToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem queryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.Timer FormMovingTimer;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button MinimiseButton;
        private System.Windows.Forms.Button MaximiseButton;
        private System.Windows.Forms.Panel AxisPanelX;
        private System.Windows.Forms.Label AxisLabelX;
        private System.Windows.Forms.ComboBox AxisDropdX;
        private System.Windows.Forms.Panel AxisPanelY;
        private System.Windows.Forms.ComboBox AxisDropdY;
        private System.Windows.Forms.Label AxisLabelY;
        private System.Windows.Forms.Panel AxisPanelZ;
        private System.Windows.Forms.ComboBox AxisDropdZ;
        private System.Windows.Forms.Label AxisLabelZ;
        private System.Windows.Forms.Panel AxisPanelT;
        private System.Windows.Forms.ComboBox AxisDropdT;
        private System.Windows.Forms.Label AxisLabelT;
        private System.Windows.Forms.Panel StylePanelWidth;
        private System.Windows.Forms.ComboBox StyleDropdWidth;
        private System.Windows.Forms.Label StyleLabelWidth;
        private System.Windows.Forms.Panel StylePanelColour;
        private System.Windows.Forms.Button StyleBtnColour;
        private System.Windows.Forms.Label StyleLabelColour;
        private System.Windows.Forms.Panel StylePanelFill;
        private System.Windows.Forms.ComboBox StyleDropdFill;
        private System.Windows.Forms.Label StyleLabelFill;
        private System.Windows.Forms.ColorDialog StyleColorDialog;
        private System.Windows.Forms.Panel TilePane;
        private System.Windows.Forms.SplitContainer InputPane;
        private System.Windows.Forms.Button KeyAlpha;
        private System.Windows.Forms.Button KeyOmega;
        private System.Windows.Forms.Button KeyPi;
        private System.Windows.Forms.Button KeyE;
        private System.Windows.Forms.Button InputBtnUnderl;
        private System.Windows.Forms.Button InputBtnItalic;
        private System.Windows.Forms.Button InputBtnBold;
        private System.Windows.Forms.Button KeySigma2;
        private System.Windows.Forms.Button KeyTau;
        private System.Windows.Forms.Button KeyUpsilon;
        private System.Windows.Forms.Button KeyPhi1;
        private System.Windows.Forms.Button KeyChi;
        private System.Windows.Forms.Button KeyPsi;
        private System.Windows.Forms.Button KeyMu;
        private System.Windows.Forms.Button KeyNu;
        private System.Windows.Forms.Button KeyXi;
        private System.Windows.Forms.Button KeyPhi2;
        private System.Windows.Forms.Button KeyRho;
        private System.Windows.Forms.Button KeySigma1;
        private System.Windows.Forms.Button KeyEta;
        private System.Windows.Forms.Button KeyTheta1;
        private System.Windows.Forms.Button KeyTheta2;
        private System.Windows.Forms.Button KeyIota;
        private System.Windows.Forms.Button KeyKappa;
        private System.Windows.Forms.Button KeyLambda;
        private System.Windows.Forms.Button KeyZeta;
        private System.Windows.Forms.Button KeyEpsilon;
        private System.Windows.Forms.Button KeyDelta;
        private System.Windows.Forms.Button KeyGamma;
        private System.Windows.Forms.Button KeyBeta;
        private System.Windows.Forms.Panel InputPanelScalar;
        private System.Windows.Forms.Button BtnAllScalar;
        private System.Windows.Forms.Panel InputPanelTensor;
        private System.Windows.Forms.Button BtnAllTensor;
        private System.Windows.Forms.Panel InputPanelCalculus;
        private System.Windows.Forms.Button BtnAllCalculus;
        private System.Windows.Forms.Panel ParamPane;
        private System.Windows.Forms.Panel GraphPanel;
        private System.Windows.Forms.Button ParamBtn;
        private System.Windows.Forms.Panel ParamSldr;
        private System.Windows.Forms.TextBox ParamSldrMin;
        private System.Windows.Forms.Button ParamSldrThmb;
        private System.Windows.Forms.TextBox ParamSldrMax;
        private System.Windows.Forms.Timer ParamSldrThmbMovingTimer;
        private System.Windows.Forms.Timer GraphPanTimer;
        private System.Windows.Forms.Timer KeyPressDownTimer;
    }
    
    partial class Window
    {
        protected class Tile : Control
        {
            //public formula def;
            //public tensor sub;
            public Panel Canvas;
            public Button PlotBtn;
            public TextBox TextInput;
            public new static Size Size { get { return new Size(183, 50 + 18); } }//+ 12 is the extra output height
            int CanvasHeight = 50;//without the extra output height
            /// <summary>
            /// The cursor that points to an InputStream within the InputStream coordinate system.
            /// </summary>
            public Point pointer = new Point(0, 0);
            Piece cursor;
            public Group Root;//public List<Group> Roots;
            
            //public Equation equation = new Equation() { parts = new List<IValuable>() };
            //IContainer CurrentIContainer = null;
            protected static List<relation> EquationType(IEnumerable<Group> groups)
            {
                var rels = new List<string> { " ", "=", ">", "<", ">=", "<=" };
                var cout = (from g in groups.Skip(1) select (relation)rels.IndexOf(g.pieces[0].val)).ToList();
                //Catch out bad equation syntax
                if (cout.Count() > 0 && !cout.All(s => cout.Contains(relation.Equal) ? s == relation.Equal : true))
                    throw new SyntaxError("SynatxError: Can't mix those comparison symbols together");
                //in the case of a lone formula
                if (cout.Count() == 0)
                    return new List<relation> { relation.None };

                var FunctionIdentity = groups.Count() == 2 && //There is 1 equals sign
                groups.ElementAt(1).pieces[0].val == "=" && //There is 1 effective Piece on the left
                groups.ElementAt(0).pieces[0] is FunctionPiece; //that effective Piece is a FunctionPiece

                var function = groups.ElementAt(0).pieces[0] as FunctionPiece;
                if (function != null)
                    FunctionIdentity &= producti.arrprd(function.built_in ?
                    (function.operand.variables[0] as BracketPiece).inside.pieces : //if the function is built-in, then the super and sub are also parameters/operands.
                    (function.operand.variables[0] as BracketPiece).inside.pieces.Concat(function.super.pieces).Concat(function.sub.pieces).ToList(),
                        (o, i) => o &= i is OperatorPiece || !Globals.values.ContainsKey(i.val), true); //The parameter of the function is unknown

                if (FunctionIdentity)
                    cout[0] = relation.Maps;

                return cout;
            }
            /// <summary>
            /// Interface for Pieces that don't use the 'floor' property to draw(), and therefore who's 
            /// "ground" (the y argument in Group.draw()) is in the middle rather than near the bottom.
            /// </summary>
            interface INoFloor { int mem_y { get; set; } }

            [Serializable]
            public abstract class Piece
            {
                /// <summary>
                /// Parent group of this piece
                /// </summary>
                public Group group;

                public Group super;
                public Group sub;

                public string val = "";
               
                /// <summary>
                /// The way that this Piece of user input will be interpreted by the
                /// software as part of an Equation
                /// </summary>
                public virtual IFormulaic Interpret() { return null; }
                public bool rcp { get => group.parent is OperatorPiece && group.parent.sub == group; }

                public int width { get { return GetWidth(); } }
                public int height { get { return GetHeight(); } }
                protected virtual int GetWidth() { return 0; }
                protected virtual int GetHeight() { return 0; }

                public int SmallWidth { get { return GetSmallWidth(); } }
                public int SmallHeight { get { return GetSmallHeight(); } }
                protected virtual int GetSmallWidth() { return 0; }
                protected virtual int GetSmallHeight() { return 0; }

                public int offset
                {
                    get
                    {
                        if (val != null && Globals.symbols.ContainsKey(val))
                            return Globals.symbols[val].offset;
                        else if (this is FunctionPiece && !(this as FunctionPiece).built_in)
                        {
                            var piece = this as FunctionPiece;
                            return (int)Math.Max(sort.qsort(piece.name.pieces, a => a.offset)[0].Item2, sort.qsort(piece.operand.pieces, a => a.offset)[0].Item2);
                        }
                        else return 0;
                    }
                }
                public int floor 
                {
                    get
                    {
                        if (this is ScalarPiece && (this as ScalarPiece).number) return s(Globals.symbols[val[0] + ""].f);
                        return val != null && Globals.symbols.ContainsKey(val) ? s(Globals.symbols[val].f) : (height + stdh) / 2;
                    } 
                }
                public int stdh { get => s(8);  }
                public int MidFloor(int y) { return group.parent is INoFloor ? group.parent.MidFloor(y) : this is INoFloor ? (this as INoFloor).mem_y : y - SmallHeight / 2; }
                public int underage { get => this is FunctionPiece ? /*the distance between the "floor" and the bottom of a FunctionPiece when draw is a special case*/ offset + sub.height : offset + sub.height; }

                ///<summary>
                ///Divides number by 2 / 3 if this is the superscript or subscript of another Piece but not of a fraction, used with Scale() function.
                ///</summary>
                public int s(int l)
                {
                    var condition = group.parent == null ? false : //can't be super or sub if parent is null
                        group.parent.super.pieces.Contains(this) || //super
                        group.parent.sub.pieces.Contains(this) || //sub
                        ((group.parent is TensorPiece || group.parent is FunctionPiece || group.parent is BracketPiece) && group.parent.group.parent != null && //in the case of being inside a TensorPiece/FunctionPiece/BracketPiece
                        (
                            group.parent.group.parent.super.pieces.Contains(group.parent) ||  //TensorPiece/FunctionPiece/BracketPiece parent is in super Group
                            group.parent.group.parent.sub.pieces.Contains(group.parent) //TensorPiece/FunctionPiece/BracketPiece parent is in sub Group
                        ));

                    var num = condition ? group.parent.val : "Nabla";//delocalise parameter variables left over from DisplayFormula;
                    if (num.Contains(':'))//Nabla is my "something's wrong" symbol
                        num = "Key" + new string(num.Skip(num.IndexOf(':') + 1).ToArray());

                    return condition && num != "/" ? l * 2 / 3 : l;
                }
                
                public virtual void draw(int x, int y, Graphics g, Piece v)
                {
                    if (Equals(v))
                        //g.DrawLine
                        //(
                        //    new Pen(Color.Red), 
                        //    x + GetSmallWidth(), 
                        //    y - SmallHeight / 2, 
                        //    x + GetSmallWidth(), 
                        //    y + SmallHeight / 2
                        //);
                        g.DrawRectangle(new Pen(Color.Red), x - 1, y - (this is ScalarPiece ? offset + 1 : 2) - SmallHeight / 2 - super.height, width + 2, height + 3);
                }
            }
            [Serializable]
            public class NullPiece : Piece
            {
                public NullPiece(Group parent)
                {
                    group = parent;
                    super = new Group(this);
                    sub = new Group(this);
                    val = "null";
                }
                protected override int GetWidth()
                {
                    return s(8);
                }
                protected override int GetHeight()
                {
                    return s(8);
                }
                protected override int GetSmallWidth()
                {
                    return s(8);
                }
                protected override int GetSmallHeight()
                {
                    return s(8);
                }
            }
            [Serializable]
            public class OperatorPiece : Piece
            {
                protected override int GetWidth()
                {
                    return GetSmallWidth();
                }
                protected override int GetHeight()
                {
                    return GetSmallHeight();
                }
                protected override int GetSmallWidth()
                {
                    if (val != "/")
                        return s(Globals.symbols[val].i.Width);
                    else
                        return s(Math.Max(Math.Max(super.width, sub.width), 10));
                }
                protected override int GetSmallHeight()
                {
                    if (val != "/")
                        return s(Globals.symbols[val].i.Height);
                    else
                        return s(super.height + 3 + sub.height);
                }

                int padding { get => s(3); }

                public OperatorPiece(string value, Group group)
                {
                    val = value;
                    this.group = group;

                    super = new Group(this);
                    sub = new Group(this);

                    if (val == "/")
                    {
                        super.pieces.Add(new NullPiece(super));
                        sub.pieces.Add(new NullPiece(sub));
                    }
                }

                public override void draw(int x, int y, Graphics g, Piece v)
                {
                    var image = Globals.symbols[val].i;
                    if (val == "/")
                    {
                        super.draw(x + (width - super.width) / 2, y - stdh, g, v);
                        g.DrawLine(new Pen(Globals.MathsBrush), x, y - s(2), x + width, y - s(2));
                        sub.draw(x + (width - sub.width) / 2, y + stdh, g, v);
                    }
                    else
                    {
                        var between = new Group(this);
                        if (Between(group, this, out between, out _))
                            g.DrawImage(Scale(image, s(image.Width), between.height), x, y - between.height / 2);
                        else
                            g.DrawImage(Scale(image, s(image.Width), s(image.Height)), x, y - height / 2);
                    }
                    
                    base.draw(x, y, g, v);
                }
            }
            [Serializable]
            public class ScalarPiece : Piece
            {
                public bool number
                {
                    get
                    {
                        var num = val;
                        if(num.Last() == '.') { num += '0'; }
                        return double.TryParse(num, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out _); }
                }
                
                protected override int GetWidth()
                {
                    return Math.Max(super.width, sub.width) + GetSmallWidth();
                }
                protected override int GetHeight()
                {
                    var num = val;
                    if (num.Contains(':'))
                        num = "Key" + new string(num.Skip(num.IndexOf(':') + 1).ToArray());
                    return Math.Max(super.height - s((number ? (int)sort.qsort(num.ToList(), a => Globals.symbols[a + ""].f)[0].Item2 : Globals.symbols[num].f) - stdh), 0) + /*(num == "(" || num == ")" ? group.underage : */Math.Max(offset, sub.height)/*)*/ + GetSmallHeight();
                }
                protected override int GetSmallWidth()
                {
                    var num = val;
                    if (num.Contains(':'))
                        num = "Key" + new string(num.Skip(num.IndexOf(':') + 1).ToArray());
                    return s(number ?
                        producti.arrprd(val.ToList(), (sum, part) => sum += Globals.symbols[part.ToString()].i.Width, 0) :
                        Globals.symbols[num].i.Width);
                }
                protected override int GetSmallHeight()
                {
                    var num = val;
                    if (num.Contains(':'))
                        num = "Key" + new string(num.Skip(num.IndexOf(':') + 1).ToArray());
                    return number ?
                        s(Globals.symbols[sort.qsort(val.ToList(), (a, b) => a > b)[0].ToString()].f) :
                        /*(val == "(" || val == ")" ? Math.Max(group.tallness.Item2, s(Globals.symbols[val].f)) : */s(Globals.symbols[num].f)/*)*/;
                }

                public ScalarPiece(string value, Group group, List<Piece> above = null, List<Piece> below = null)
                {
                    val = value;
                    super = new Group(this) { pieces = above == null ? new List<Piece>() : above };
                    sub = new Group(this) { pieces = below == null ? new List<Piece>() : below };
                    this.group = group;
                }

                public override IFormulaic Interpret()
                {
                    var scalar = number ? new Scalar(double.Parse(val)) : new Scalar();
                    return new Tensor(scalar, rcp, super.pieces.Count > 0 ? super.Interpret(new Equation()) : null, number ? null : val, /*group.parent?.Interpret() is IContainer ? (IContainer)group.parent?.Interpret() :*/ null);
                }
                public override void draw(int x, int y, Graphics g, Piece v)
                {
                    Image image;
                    int effective_floor;

                    if (number)
                    {
                        effective_floor = sort.qsort(val.ToList(), a => Globals.symbols[a + ""].f)[0].Item2;

                        var acc = x;
                        var highest = sort.qsort(Globals.symbols.Where(k => k.Key.Length == 1).ToList(), (a, b) => a.Value.i.Height > b.Value.i.Height)?[0].Value.i.Height;
                        for (int i = 0; i < val.Length; i++)
                        {
                            image = Globals.symbols[val[i] + ""].i;
                            acc += i == 0 ? 0 : s(Globals.symbols[val[i - 1].ToString()].i.Width);
                            g.DrawImage(Scale(image, s(image.Width), s(image.Height)), acc, y + stdh / 2 - (val[i] == '.' ? s(2) : floor)/* + s(highest.Value - Globals.symbols[val[i].ToString()].Height)*/);
                        }
                    }
                    else if (val == "Infinity") //very rare but idk some kind of easter egg
                    {
                        effective_floor = Globals.symbols["Keya"].f;
                        g.DrawString("∞", Globals.MathsFont, Globals.MathsBrush, x, y - stdh / 2);
                    }
                    else
                    {
                        var num = val;
                        if (num.Contains(':'))
                            num = "Key" + new string(num.Skip(num.IndexOf(':') + 1).ToArray());
                        effective_floor = Globals.symbols[num].f;
                        image = Globals.symbols[num].i;
                        g.DrawImage(Scale(image, s(image.Width), s(image.Height)), x, y - floor/* + Math.Max(super.height - s(Globals.symbols[val].f - stdh), 0)*/ + stdh / 2 /*s(Globals.symbols[val].offset)*/);
                    }

                    super.draw(x + GetSmallWidth(), y - SmallHeight / 2 + Math.Max(s(effective_floor - stdh) - super.height, 0), g, v);
                    sub.draw(x + GetSmallWidth(), y + (GetSmallHeight() + sub.height) / 2/* - Math.Min(s(Globals.symbols[val].f - stdh), super.height) + s(Globals.symbols[val].offset)*/, g, v);

                    base.draw(x, y, g, v);
                }
            }
            [Serializable]
            public class TensorPiece : Piece, INoFloor
            {
                public List<List<Group>> array;
                public Size size;

                protected override int GetWidth()
                {
                    return Math.Max(super.width, sub.width) + GetSmallWidth();
                }
                protected override int GetHeight()
                {
                    return super.height + sub.height + GetSmallHeight();
                }
                protected override int GetSmallWidth()
                {
                    //sum of widths of each column + sum of spaces between columns + width of brackets + tabbing space
                    return producti.arrprd(ColumnWidths.ToList(), (o, i) => o += i, 0) + 2 * (size.Width - 1) + BracketWidth * 2 + 2;
                }
                protected override int GetSmallHeight()
                {
                    //sum of heights of each row + sum of spaces between rows + height at which the brackets hang over and under the Tensor (thickness of the brackets)
                    return producti.arrprd(RowHighests.ToList(), (o, i) => o += i.height, 0) + 2 * (size.Height - 1) + BracketWidth * 2;
                }

                int BracketWidth
                {
                    get
                    {
                        return s(2);//(int)Graphics.FromImage(Globals.symbols["0"]).MeasureString
                            //("[", new Font(Globals.MathsFont.FontFamily, s(height))).Width;
                    }
                }
                Group[] RowHighests
                {
                    get
                    {
                        var cout = new Group[size.Height];
                        for (int i = 0; i < size.Height; i++)
                            cout[i] = sort.qsort(array[i], (a, b) => a.height > b.height)[0];
                        return cout;
                    }
                }
                int[] ColumnWidths
                {
                    get
                    {
                        var cout = new int[size.Width];
                        for (int i = 0; i < size.Width; i++)
                            cout[i] = sort.qsort(columns[i], (a, b) => a.width > b.width)[0].width;
                        return cout;
                    }
                }

                //public List<List<Group>> ToList()
                //{
                //    var cout = new List<List<Group>>();
                //    for (int r = 0; r < size.Height; r++)
                //    {
                //        cout.Add(new List<Group>());
                //        for (int c = 0; c < size.Width; c++)
                //        {
                //            cout[cout.Count - 1].Add(array[r, c]);
                //        }
                //    }
                //    return cout;
                //}
                public List<List<Group>> columns
                {
                    get
                    {
                        var cout = new List<List<Group>>();
                        for (int c = 0; c < size.Width/*sort.qsort(ToList(), (a, b) => a.Count > b.Count).Last().Count*/; c++)
                        {
                            cout.Add(new List<Group>());
                            for (int r = 0; r < size.Height; r++)
                            {
                                cout[cout.Count - 1].Add(array[r][c]);
                            }
                        }
                        return cout;
                    }
                }

                public TensorPiece(Group group, Size? dimensions = null)
                {
                    this.group = group;
                    super = new Group(this);
                    sub = new Group(this);
                    array = new List<List<Group>>();

                    if (dimensions.HasValue)
                    {
                        size = dimensions.Value;
                        //new Group[size.Height, size.Width];

                        for (int r = 0; r < size.Height; r++)
                        {
                            array.Add(new List<Group>());
                            for (int c = 0; c < size.Width; c++)
                            {
                                array[r].Add(new Group(this));
                                array[r][c].pieces =
                                    new List<Piece> { new NullPiece(array[r][c])/*new ScalarPiece("0", array[r, c])*/ };
                            }
                        }
                    }
                    else
                    {
                        size = new Size(1, 1);
                        //array = new Group[1, 1];
                        array.Add(new List<Group>() { new Group(this) });
                        array[0][0].pieces.Add(new NullPiece(array[0][0]));
                    }
                }

                public enum side { left, right, top, bottom }
                public void Magnate(side Side)
                {
                    //var cursor_index = new int[3];
                    //if (cursor != null)
                    //{
                    //    cursor_index[0] = (cursor.group.parent as TensorPiece).array.FindIndex(r => r.Contains(cursor.group));
                    //    cursor_index[1] = (cursor.group.parent as TensorPiece).array[cursor_index[0]].IndexOf(cursor.group);
                    //    cursor_index[2] = cursor.group.pieces.IndexOf(cursor);
                    //}
                    
                    var tallen = Side == side.top || Side == side.bottom ? 1 : 0;
                    var widen = Side == side.left || Side == side.right ? 1 : 0;
                    
                    switch(Side)
                    {
                        case side.left:
                            array.ForEach(r => r.Insert(0, new Group(this, 'n')));
                            break;
                        case side.right:
                            array.ForEach(r => r.Add(new Group(this, 'n')));
                            break;
                        case side.bottom:
                            array.Add(array.Last().ConvertAll(g => new Group(this, 'n')));
                            break;
                        case side.top:
                            array.Insert(0, array.Last().ConvertAll(g => new Group(this, 'n')));
                            break;

                    }
                    
                    //array = dcpy(bigger_piece.array);
                    //this_group.pieces.Add(this);
                    //cursor = cursor == null ? null : /*bigger_piece.*/array[cursor_index[0], cursor_index[1]].pieces[cursor_index[2]];

                    //return bigger_piece;
                    size.Width += widen;
                    size.Height += tallen;
                }
                public void Parvate(side Side)
                {
                    var tallen = Side == side.top || Side == side.bottom ? 1 : 0;
                    var widen = Side == side.left || Side == side.right ? 1 : 0;

                    switch (Side)
                    {
                        case side.left:
                            array.ForEach(r => r.RemoveAt(0));
                            break;
                        case side.right:
                            array.ForEach(r => r.Remove(r.Last()));
                            break;
                        case side.bottom:
                            array.Remove(array.Last());
                            break;
                        case side.top:
                            array.RemoveAt(0);
                            break;

                    }

                    size.Width -= widen;
                    size.Height -= tallen;
                }

                public bool CheckIfEmpty(side side)
                {
                    switch (side)
                    {
                        case side.left:
                            return producti.arrprd(columns[0], (o, i) => o &= i.pieces[0] is NullPiece, true);
                        case side.right:
                            return producti.arrprd(columns[size.Width - 1], (o, i) => o &= i.pieces[0] is NullPiece, true);
                        case side.top:
                            return producti.arrprd(array[0], (o, i) => o &= i.pieces[0] is NullPiece, true);
                        case side.bottom:
                            return producti.arrprd(array[size.Height - 1], (o, i) => o &= i.pieces[0] is NullPiece, true);
                        default:
                            throw new Exception("wtf");
                    }
                }

                public void ShedEmptySides()
                {
                    var empty_sides = 0;

                    if (size.Width == 1 && size.Height == 1) return;

                    for (int i = 0; i < 8; i++)
                        if (CheckIfEmpty((side)(i % 4)))
                        {
                            empty_sides++;
                            if (i < 5)
                                Parvate((side)i);
                        }

                    if (empty_sides > 4)
                        ShedEmptySides();
                }

                public override IFormulaic Interpret()
                {
                    var t1 = array.ConvertAll(r => r.ConvertAll(c => (c.pieces.Count == 1 ? c.pieces[0] is ScalarPiece ? (c.pieces[0] as ScalarPiece).number ?
                            (c.pieces[0].Interpret() as Tensor).parts[0][0] : c.Interpret(new Equation()) : c.pieces[0].Interpret() : c.Interpret(new Equation())) as IValuable))
                        ;
                    return new Tensor
                        (
                        array.ConvertAll(r => r.ConvertAll(c => (c.pieces.Count == 1 ? c.pieces[0] is ScalarPiece ? (c.pieces[0] as ScalarPiece).number ?
                            (c.pieces[0].Interpret() as Tensor).parts[0][0] : c.Interpret(new Equation()) : c.pieces[0].Interpret() : c.Interpret(new Equation())) as IValuable)),
                        false,//group.parent is OperatorPiece ? false : group.parent.sub == group,
                        super.Interpret(new Equation())
                        );
                }

                public int mem_y { get; set; }
                public override void draw(int x, int y, Graphics g, Piece v)
                {
                    var bracket_font = new Font(Globals.MathsFont.FontFamily, s(height));
                    var t1 = GetSmallHeight();
                    var t2 = GetSmallWidth();

                    g.DrawLine(new Pen(Globals.MathsBrush), x, y - SmallHeight / 2, x + BracketWidth, y - SmallHeight / 2);
                    g.DrawLine(new Pen(Globals.MathsBrush), x, y - SmallHeight / 2, x,                y + SmallHeight / 2);
                    g.DrawLine(new Pen(Globals.MathsBrush), x, y + SmallHeight / 2, x + BracketWidth, y + SmallHeight / 2);
                    //g.DrawString("[", bracket_font, Globals.MathsBrush, x, y);

                    int accr = y - SmallHeight / 2 + BracketWidth, accc = x + BracketWidth;
                    for(int r = 0; r < size.Height; r++)
                    {
                        for(int c = 0; c < size.Width; c++)
                        {
                            array[r][c].draw(accc, accr + RowHighests[r].tallness.Item2/*RowHighests[r].height - (array[r][c].height == RowHighests[r].height ? 0 : RowHighests[r].Grounding) - array[r][c].height*/, g, v);
                            accc += ColumnWidths[c] + 2;
                        }
                        accr += RowHighests[r].height + 2;
                        accc = x + BracketWidth;
                    }

                    g.DrawLine(new Pen(Globals.MathsBrush), x + GetSmallWidth() - BracketWidth - 2, y - SmallHeight / 2, x + GetSmallWidth() - 2, y - SmallHeight / 2);
                    g.DrawLine(new Pen(Globals.MathsBrush), x + GetSmallWidth()                - 2, y - SmallHeight / 2, x + GetSmallWidth() - 2, y + SmallHeight / 2);
                    g.DrawLine(new Pen(Globals.MathsBrush), x + GetSmallWidth() - BracketWidth - 2, y + SmallHeight / 2, x + GetSmallWidth() - 2, y + SmallHeight / 2);
                    //g.DrawString("]", bracket_font, Globals.MathsBrush, x + 2 * size.Width + GetSmallWidth() - BracketWidth, y);

                    super.draw(x + GetSmallWidth() + s(4), y, g, v);
                    sub.draw(x + GetSmallWidth() + s(4), y + super.height + GetSmallHeight(), g, v);

                    mem_y = y;
                    base.draw(x, y, g, v);
                }
            }
            [Serializable]
            public class FunctionPiece : Piece, INoFloor
            {
                public Group operand;
                public Group name;
                public Group AllParts
                { get { return new Group(this) { pieces = name.pieces.Concat(operand.pieces).ToList() }; } }

                public bool built_in { get { return Globals.symbols.ContainsKey(val); } }
                public enum super_sub_type { big, non, sml }
                public super_sub_type sst
                {
                    get
                    {
                        switch (val.Contains("(") ? val.Substring(val.IndexOf('(') + 1, 3) : "sml")
                        {
                            case "big":
                                return super_sub_type.big;
                            case "non":
                                return super_sub_type.non;
                            case "sml":
                                return super_sub_type.sml;
                            default:
                                return 0;
                        }
                    }
                }

                protected override int GetWidth()
                {
                    return Math.Max(Math.Max(super.width, sub.width), operand.width) + GetSmallWidth();
                }
                protected override int GetHeight()
                {
                    return (sst == super_sub_type.big && val.Substring(0, 3) != "[1]" ? 0 : super.height + sub.height) + GetSmallHeight();
                }
                protected override int GetSmallWidth()
                {
                    return s(built_in ? Globals.symbols[val].i.Width : name.width);
                }
                protected override int GetSmallHeight()
                {
                    return s(built_in ? Globals.symbols[val].i.Height : Math.Max(name.height, operand.height));
                }

                public FunctionPiece(string name, Group group)
                {
                    this.group = group;
                    val = name;
                    super = new Group(this);
                    sub = new Group(this);
                    operand = new Group(this);
                    this.name = new Group(this);
                    
                    if (sst == super_sub_type.big)
                    {
                        super.pieces.Add(new NullPiece(super));
                        sub.pieces.Add(new NullPiece(sub));
                    }

                    if (!built_in)
                    {
                        foreach (var l in val)
                            this.name.pieces.Add(new ScalarPiece("Key" + l, this.name));
                        operand.pieces.Add(new BracketPiece(operand));
                    }
                    else operand.pieces.Add(new NullPiece(operand));
                }

                object ParameterInterpretation
                {
                    get
                    {
                        switch (val)
                        {
                            case "[1](big)#summation":
                                var index_parameter = new Equation();
                                index_parameter.Add(sub.Interpret(index_parameter));
                                return new Tuple<Formula, Equation>(super.Interpret(new Equation()), index_parameter);
                            default:
                                return null;
                        }
                    }
                }
                public override IFormulaic Interpret()
                {
                    if (built_in)
                        return new Operation(null, val, operand.Partition.Count() <= 1 ? new List<Formula> { operand.Interpret(new Equation()) } : throw new SyntaxError("SyntaxError: built-in functions only take 1 argument."),
                            Globals.functions.ContainsKey(val) ? Globals.functions[val] : throw new DevError("DevError: Couldn't find the function '" + val + "' in Globals.functions"),
                            val.Contains('#') ? new Tuple<Formula, List<Equation>>(super.Interpret(new Equation()), sub.Interpret().ToList()) : null); //Only iterator functions supported atm
                    else
                    {
                        var t1 = (/*effective_*/operand.pieces[0] as BracketPiece).inside.Interpret();
                        return new Function(null, val, new List<Formula>() { (/*effective_*/operand.pieces[0] as BracketPiece).inside.Interpret().First().parts[0] as Formula },
                            null, super.Interpret(new Equation()));
                    }
                }

                public int mem_y { get; set; }

                public override void draw(int x, int y, Graphics g, Piece v)
                {
                    mem_y = y;

                    if (built_in)
                    {
                        if(sst != super_sub_type.non)
                        {
                            super.draw(x + (val.Substring(0, 3) == "[1]" ? (GetSmallWidth() - super.width) / 2 : GetSmallWidth()), y - SmallHeight / 2 - (sst == super_sub_type.big && val.Substring(0, 3) != "[1]" ? 0 : 3), g, v);
                            sub.draw(x + (val.Substring(0, 3) != "[3]" ? (GetSmallWidth() - sub.width * (2 - int.Parse(val[1] + ""))) / 2 : GetSmallWidth()), 
                                y + 1 + (sst == super_sub_type.big && val.Substring(0, 3) == "[1]" ? sub.tallness.Item2 : 0) + SmallHeight / 2, g, v);
                        }
                        g.DrawImage(Scale(Globals.symbols[val].i, SmallWidth, SmallHeight), x, y - SmallHeight / 2/* + (sst == super_sub_type.big && val.Substring(0, 3) != "[1]" ? 0 : super.height)*/);
                        operand.draw(x + GetSmallWidth(), y/* + s(offset) + group.height / 2*/, g, v);
                        //g.DrawLine(new Pen(Color.Blue), x, y, x + width, y);
                    }
                    else
                    {
                        super.draw(x + GetSmallWidth(), y - SmallHeight / 2 - s((int)sort.qsort(AllParts.pieces, a => a.floor)[0].Item2 - stdh), g, v);

                        AllParts.draw(x, y, g, v);
                        //name.draw(x, y + super.height + (Math.Max(s(Globals.symbols[operand.pieces[0].val].f), operand.height) > name.height ? 
                        //    (Math.Max(s(Globals.symbols[operand.pieces[0].val].f), operand.height) - name.height) / 2 : 0), g, v);

                        ////var font_height_ratio = GetSmallHeight() / g.MeasureString("(", new Font(Globals.MathsFont.FontFamily, GetSmallHeight())).Height;
                        ////var corrected_font = new Font(Globals.MathsFont.FontFamily, GetSmallHeight() * font_height_ratio);

                        ////g.DrawString("(", corrected_font, Globals.MathsBrush, x + name.width, y);
                        ////var t1 = g.MeasureString("(", new Font(Globals.MathsFont.FontFamily, GetSmallHeight())).Height;
                        //operand.draw(x + name.width/* + (int)g.MeasureString("(", corrected_font).Width*/, y + super.height + (Math.Max(s(Globals.symbols[operand.pieces[0].val].f), operand.height) < name.height ?
                        //    (name.height - Math.Max(s(Globals.symbols[operand.pieces[0].val].f), operand.height)) / 2 : 0)/* + (name.height - operand.height) / 2*/, g, v);
                        ////g.DrawString(")", corrected_font, Globals.MathsBrush, x + name.width + (int)g.MeasureString("(", corrected_font).Width, y);

                        try { sub.draw(x + GetSmallWidth(), y + Math.Max(name.height, Math.Max(s(Globals.symbols[(operand.pieces[0] as BracketPiece).inside.pieces[0].val].f), operand.height)), g, v); }
                        catch (Exception) { throw new SyntaxError("Where's the operand for '" + val + "' ?"); }
                    }
                    base.draw(x, y, g, v);
                }
            }
            [Serializable]
            public class BracketPiece : Piece, INoFloor
            {
                public Group inside;

                protected override int GetWidth()
                {
                    return Math.Max(super.width, sub.width) + GetSmallWidth();
                }
                protected override int GetHeight()
                {
                    return super.height + GetSmallHeight() + sub.height;
                }
                protected override int GetSmallWidth()
                {
                    return inside.width;
                }
                protected override int GetSmallHeight()
                {
                    return inside.height;
                }

                public BracketPiece(Group parent)
                {
                    group = parent;
                    super = new Group(this);
                    sub = new Group(this);
                    inside = new Group(this);

                    inside.pieces.Add(new OperatorPiece("(", inside));
                }

                public override IFormulaic Interpret()
                {
                    var cout = inside.Interpret(new Equation());
                    cout.Index = super.Interpret(new Equation());
                    return cout;
                }

                public int mem_y { get; set; }
                public override void draw(int x, int y, Graphics g, Piece v)
                {
                    super.draw(x + SmallWidth, y - s(Globals.symbols["("].offset) + inside.height / 2 - s(floor), g, v);
                    inside.draw(x, y, g, v);
                    sub.draw(x + GetSmallWidth(), y - s(Globals.symbols["("].offset) + inside.height - sub.height, g, v);

                    mem_y = y;

                    var r = new Random(); //TODO: okay look we need to fix the arrow key navigation these BracketPieces are inside of BracketPieces. I used an irresponsible way of storing the y position of the previous INoFloor and I know it's really bad but we need to fix this. All I need you to do is get a BracketPiece to allign properly with an OperatorPiece {val = ")"} and then  make sure we can properly move left and right in and out of these cuz this is taking too long.
                    //g.DrawLine(new Pen(Color.FromArgb(r.Next(Int32.MinValue, Int32.MaxValue))), x, y, x + width, y);

                    base.draw(x, y, g, v);
                }
            }
            [Serializable]
            public class Group
            {
                public List<Piece> pieces;
                public Piece parent;

                public int width { get { return producti.arrprd(pieces, (o, i) => o += i.width, 0); } }
                public int height
                {
                    get
                    {
                        //var no_brackets = pieces.Where(p => p.val != "(" && p.val != ")").ToList();
                        //if (no_brackets.Count == 0 && pieces.Count > 0)
                        //    return parent != null && (parent.sub == this || parent.super == this) ? Globals.symbols[")"].i.Height * 2 / 3 : Globals.symbols[")"].i.Height;

                        var highest = pieces.Count > 0 ? sort.qsort(pieces, (a, b) => a.height > b.height)[0].height : 0;
                        return highest;
                    }
                }

                public List<Piece> variables
                {
                    get
                    {
                        var cout = new List<Piece>();
                        DeepForEach(null, (p, _null_) => { if (p.val != null || p.val != "") cout.Add(p); });
                        return cout;
                    }
                }

                /// <summary>
                /// The distance in pixels (negative) needed to bring a Piece up to ground level from the lowest point in this Group, 
                /// relative to this Group.
                /// </summary>
                public int Grounding
                {
                    get
                    {
                        var t1 = sort.qsort(pieces.ConvertAll(p => p.val != null && Globals.symbols.ContainsKey(p.val) ? Globals.symbols[p.val].offset : 0), (a, b) => a > b)[0];
                        var t2 = sort.qsort(pieces, (a, b) => a.sub.height > b.sub.height)[0].sub.height;

                        return 
                            sort.qsort(pieces.ConvertAll(p => p.val != null && Globals.symbols.ContainsKey(p.val) ? Globals.symbols[p.val].offset : 0), (a, b) => a > b)[0] + 
                            sort.qsort(pieces, (a, b) => a.sub.height > b.sub.height)[0].sub.height;
                    }
                }
                public Piece tallest { get { return sort.qsort(pieces, (a, b) => a.SmallHeight > b.SmallHeight)[0]; } }

                public Tuple<Piece, int> tallness
                { get {
                        return sort.qsort(pieces, a => a.floor - ((a is ScalarPiece || a is NullPiece) ? a.stdh / 2 : 0) + Math.Max(a.super.height - a.s(a.floor - a.stdh), 0), (a, b) => a > b)[0]; } }

                public int floor { get { return (int)sort.qsort(pieces, a => a.floor)[0].Item2; } }

                public int underage
                { get { return Math.Max(sort.qsort(pieces, a => a.offset)[0].Item2, sort.qsort(pieces, a => 
                {
                    if (a is FunctionPiece)
                    {
                        var f = a as FunctionPiece;
                        var o = f.group.pieces[f.group.pieces.IndexOf(f) - 1];
                        if (f.sst == FunctionPiece.super_sub_type.big)
                            if (f.val.Substring(0, 3) == "[1]")
                            {
                                var func_under = (f == f.group.pieces[0] ? f.s(4) : o is ScalarPiece ? o.stdh : o.height);
                                return (f.height - func_under) / 2;
                            }
                            else return Math.Min(f.sub.height, 6); //6 = s(8), the stdh for any Piece in f.sub
                    }
                    return a.sub.height;
                })[0].Item2); } }

                public Group(Piece Parent)
                {
                    parent = Parent;
                    pieces = new List<Piece>();
                }

                public Group(Piece Parent, char _null_)
                {
                    parent = Parent;
                    pieces = new List<Piece>() { new NullPiece(this) };
                }

                public Group(Piece Parent, List<Piece> pieces)
                {
                    parent = Parent;
                    this.pieces = pieces;
                }

                public void draw(int x, int y, Graphics g, Piece v)
                {
                    var acc = x;
                    if (pieces.Count > 0)
                    {
                        for (int i = 0; i < pieces.Count; i++)
                        {
                            acc += i == 0 ? 0 : pieces[i - 1].width + pieces[i].s(2);
                            pieces[i].draw(acc, y, g, v);
                        }
                        //g.DrawLine(new Pen(Color.Chocolate), x, y, x + width, y);
                    }
                }

                public void DeepForEach(object args, Action<Piece, object> instruction)
                {
                    foreach(var p in pieces)
                    {
                        instruction.Invoke(p, args);

                        try { p.super.DeepForEach(args, instruction); } catch (Exception) { }
                        try { p.sub.DeepForEach(args, instruction); } catch (Exception) { }
                        try { (p as BracketPiece).inside.DeepForEach(args, instruction); } catch (Exception) { }
                        try { (p as FunctionPiece).operand.DeepForEach(args, instruction); } catch (Exception) { }
                    }
                }

                /// <summary>
                /// Deep copy algorithm because the deep-copy provided by the Liber Engine can't due to this class containing a List.
                /// </summary>
                /// <returns></returns>
                public Group DeepCopy()
                {
                    var cout = new Group(parent);

                    for (int i = 0; i < pieces.Count; i++)
                        cout.pieces.Add(dcpy(pieces[i]));

                    return cout;
                }
                
                public Group CheckLocalise(IEnumerable<Group> groups) => 
                    EquationType(groups)[0] == relation.Maps ? localise(groups.ElementAt(0).pieces[0] as FunctionPiece) : this;
                public Group localise(FunctionPiece f)
                {
                    var cout = dcpy(this); //localise all of the parameter variables of the function so that we don't get confusion between function parameter variables

                    cout.DeepForEach(null, (p, _null_) =>
                    { try { if (!(Globals.values.ContainsKey(p.val) || (p as ScalarPiece).number)) p.val = f.val + ":" + p.val.Substring(3); } catch (Exception) { } });

                    return cout;
                }
                
                public IEnumerable<IEnumerable<Group>> Partition
                {
                    get
                    {
                        //Find the next (T)element that statisfies comp, meant for finding the length of the partition.
                        int next<T>(T p, List<T> l, Func<T, bool> comp) => l.FindIndex(s => l.IndexOf(s) > l.IndexOf(p) && comp(s)) - l.IndexOf(p);
                        var PIECES = pieces.Concat(new[] { new OperatorPiece(",", this) }).ToList();

                        //conditions to be met before it is decided that the partition has reached its end
                        Func<Piece, bool> fo = p => p.val.Contains("=") || p.val == "<" || p.val == ">" || p.val == ",";
                        Func<int[], bool> eq = f => PIECES[f[0]].val == ",";

                        //List<int>'s containing all the indicies representing all the partitioning points (rather than the actual partitions themselves)
                        //Add partitioning points at the start so there are an equal number of partitions & partitioning point indices
                        var formae = (from p in PIECES where fo(p) select new[] { PIECES.IndexOf(p), next(p, PIECES, fo) }).ToList();
                        formae.Insert(0, new[] { 0, next(PIECES[0], PIECES, fo) });
                        var equati = (from f in formae where eq(f) select new[] { formae.IndexOf(f), next(f, formae, eq) }).ToList();
                        equati.Insert(0, new[] { 0, next(formae[0], formae, eq) });
                        
                        //return IEnumerable partitions of partitions according to the aforementioned List<int>'s,
                        //localising them to their Function's if they are part of a Function identity.
                        foreach (var e in equati.Take(equati.Count - 1))
                        {
                            var localend = from f in formae.GetRange(e[0], e[1]) select new Group(parent, PIECES.GetRange(f[0], f[1]));
                            yield return from l in localend select l.CheckLocalise(localend);
                        }
                    }
                }
                /// <summary>
                /// Create a mathematical interpretation of this Group as a Formula
                /// </summary>
                /// <param name="equation">the parent Equation of the output Formula</param>
                /// <returns>Formula representation of the Group</returns>
                public Formula Interpret(Equation equation) //How to interpret functions
                {
                    var interpretation = new Formula(equation);
                    interpretation.Add(new Term(interpretation, false));

                    var coefficients = 0;
                    var terms = 1;
                    
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        if (pieces[i] is OperatorPiece)
                        {
                            switch (pieces[i].val)
                            {
                                case "+":
                                    try { interpretation.Add(new Term(null, false)); coefficients = 0; terms++; }
                                    catch (Exception e)
                                    {
                                        if (e.Message.Substring(0, 24) == "SyntaxError: Empty field")
                                            throw new Exception(e.Message + " in Term:" + (terms + 1));
                                    }
                                    break;
                                case "-":
                                    try { interpretation.Add(new Term(null, false) { parts = new List<IFormulaic>() { new Tensor(new Scalar(-1), false) } }); coefficients = 0; terms++;  }
                                    catch (Exception e)
                                    {
                                        if (e.Message.Substring(0, 24) == "SyntaxError: Empty field")
                                            throw new Exception(e.Message + " in Term:" + (terms + 1));
                                    }
                                    break;
                                case "/":
                                    //if (interpretation.parts.Count == 0) interpretation.parts.Add(new Term(interpretation, false));
                                    try { (interpretation.parts.Last() as Term).numerator.Add(pieces[i].super.Interpret(equation)); }
                                    catch (Exception e)
                                    {
                                        if (e.Message.Substring(0, 24) == "SyntaxError: Empty field")
                                            throw new Exception(e.Message + " in the numerator of the fraction at:" + (coefficients + 1) + " in Term:" + terms);
                                    }

                                    try { (interpretation.parts.Last() as Term).denominator.Add(pieces[i].sub.Interpret(equation)); }
                                    catch (Exception e)
                                    {
                                        if (e.Message.Substring(0, 24) == "SyntaxError: Empty field")
                                            throw new Exception(e.Message + " in the denominator of the fraction at:" + (coefficients + 1) + " in Term:" + terms);
                                    }
                                    break;
                                case "*":
                                    break;
                                case "(":
                                case ")":
                                    break; //TODO: Test the EquationType method and add inequalities, laying infrastructure for Equation.type.Constraint's
                                case ",":
                                case "=":
                                case "<":
                                case ">":
                                case ">=":
                                case "<=":
                                    if (i > 0)
                                        throw new SyntaxError("SyntaxError: Cannot put '" + pieces[i].val + "' sign here.");
                                    break;
                                default:
                                    throw new DevError("DevError: OperatorPiece not recognised by interpretation algorithm.");
                            }
                        }
                        else
                        {
                            IFormulaic formulaic;
                            coefficients++;

                            try
                            {
                                formulaic = pieces[i].Interpret();//SCRAPPED: make sure negative indices get put in the denominator within Piece.Interpret()
                                try { formulaic.Reciprocal ^= parent.val == "/" && parent.sub == this; } catch (Exception) { } //communicate across the whether or not the Piece is in the denominator of its Term
                                interpretation.Add(formulaic);
                            }
                            catch (Exception e)
                            {
                                if (e.Message == "SyntaxError: Empty field")
                                    throw new Exception(e.Message + " in Coefficient:" + coefficients + " in Term:" + terms);
                            }
                        }
                    }

                    #region green
                    ////explication phase
                    //Pieces.DeepForEach(Pieces.pieces[0], (next, prev) => 
                    //{
                    //    if (!(prev is OperatorPiece))
                    //    {
                    //        cursor = (Piece)prev;
                    //        AddPiece("*"); //if two Pieces are sat next to eachother, the default operation is *.
                    //    }
                    //    prev = next;
                    //});

                    ////operand parametrefication phase
                    //Pieces.DeepForEach(new Piece[2], (next, _prevs) =>
                    //{
                    //    var prevs = _prevs as Piece[];

                    //    if (prevs[0] is OperatorPiece)
                    //    {
                    //        switch((prevs[0] as OperatorPiece).val)
                    //        {
                    //            case "+":
                    //                var NextIContainer1 = new Term(null);
                    //                CurrentIContainer.Add(NextIContainer1);
                    //                CurrentIContainer = NextIContainer1;

                    //                CurrentIContainer.Add()
                    //                break;
                    //            case "-":
                    //                var NextIContainer2 = new Term(null);
                    //                NextIContainer2.numerator.Add(new Scalar(null, -1));
                    //                CurrentIContainer.Add(NextIContainer2);
                    //                CurrentIContainer = NextIContainer2;
                    //                break;
                    //            case "*":
                    //                var NextIContainer3 = new Tensor(null);
                    //                CurrentIContainer.Add(NextIContainer3);
                    //                CurrentIContainer = NextIContainer3;
                    //                break;
                    //            case '/':
                    //                if (CurrentIContainer is Formula)
                    //                {
                    //                    CurrentIContainer.parts.Add(new Term(CurrentIContainer));
                    //                    CurrentIContainer = (Term)CurrentIContainer.parts.Last();
                    //                }
                    //        (CurrentIContainer as Term).denominator.Add(new Tensor(CurrentIContainer));

                    //                next_piece_group = piece_group.none;
                    //                next_piece_type = piece_type.operator_sign;
                    //                AddPiece("/");
                    //                next_piece_type = piece_type.ambiguous;
                    //                break;
                    //        }
                    //    }
                    //    prevs = next;
                    //});

                    ////hierarchy assignment phase
                    #endregion

                    return interpretation;
                }
                /// <summary>
                /// Partition this Group into what would be its equations and their formulae, before actually 
                /// creating a mathematical interpretation of each (Group)partition as a Formula
                /// </summary>
                /// <param name="equation">the parent Equation of the output Formula</param>
                /// <returns>Formula representation of the Group</returns>
                public IEnumerable<Equation> Interpret() =>
                    from eq in Partition select new Equation(from fo in eq select fo.Interpret(new Equation()), EquationType(eq)); 
            }

            public Tile(Control v, Point Location)
            {
                Canvas = new Panel();
                Canvas.Location = Location;
                Canvas.Name = "Canvas";
                Canvas.Size = Size;
                Canvas.TabIndex = 2;
                Canvas.Paint += OnPaint;
                Canvas.Click += OnClick;
                Canvas.BackColor = Color.Beige;
                Canvas.BorderStyle = BorderStyle.FixedSingle;
                v.Controls.Add(Canvas);

                TextInput = new TextBox();
                TextInput.Location = Location;
                TextInput.Name = "TextInput";
                TextInput.Size = Size;
                TextInput.Width = 0;
                TextInput.TabIndex = 1;
                TextInput.TextChanged += OnChange;
                TextInput.Click += OnClick;
                v.Controls.Add(TextInput);

                PlotBtn = new Button();
                PlotBtn.Size = new Size(25, Size.Height);
                PlotBtn.Location = new Point(Size.Width - PlotBtn.Width, 0);
                PlotBtn.Name = "PlotBtn";
                PlotBtn.Text = "PLOT";
                PlotBtn.TabIndex = 3;
                PlotBtn.BackColor = SystemColors.ControlDarkDark;
                PlotBtn.FlatStyle = FlatStyle.Flat;
                PlotBtn.Margin = new Padding(0);
                PlotBtn.Visible = true;
                PlotBtn.UseVisualStyleBackColor = false;
                PlotBtn.MouseLeave += (sender, e) => BackColor = SystemColors.ControlDark;
                PlotBtn.MouseHover += (sender, e) => BackColor = SystemColors.ControlDarkDark;
                PlotBtn.Click += OnPlotBtnClick;
                Canvas.Controls.Add(PlotBtn);

                Root = new Group(null, 'n');
                //Roots = new List<Group> { new Group(null) };
                //CurrentGroup = Roots[0];
                cursor = new ScalarPiece("init", Root);
                //CurrentIContainer = equation;
            }

            string err = "";
            Group answer = new Group(null);

            void OnPlotBtnClick(object sender, EventArgs e)
            {
                List<Equation> interp = new List<Equation>();
                IVariable variable = new Tensor();

                try
                {
                    //find the value of 'variable'
                    interp = Interpret();
                    foreach (var eq in interp)
                    {
                        eq.Evaluate();
                        if (eq.EqRelations[0] != relation.None)
                            variable = eq.SolveFor(((eq.parts[0] as IContainer).parts[0] as Term).numerator[1] as IVariable, Globals.values);
                        else
                            throw new SyntaxError("SyntaxError: No equation to solve.");

                        //turn that value into a displayable Group
                        var rels = new[] { " ", "=", ">", "<", ">=", "<=", "=" };//last element is the Piece.val for relation.Maps, and is the reason you see '=' displayed for function identities
                        Piece value = new NullPiece(answer);
                        var surrogate = new Formula(null);

                        surrogate.Add(variable.Value(Globals.values));

                        answer.pieces = new List<Piece>
                        {
                            new ScalarPiece("Key" + variable.name.Last() + "", answer),
                            new OperatorPiece(rels[(int)eq.EqRelations[0]], answer),
                        };
                        if (variable is Function || (variable is Tensor && (variable as Tensor).DimDim == 0))
                            answer.pieces.Add(DisplayFormula(surrogate, answer));//value);
                        answer.pieces.Add(new NullPiece(answer));
                    }
                    //signify the success of the operation by providing no error to write
                    err = "";

                    //update
                    Canvas.Invalidate();
                    (Parent.Parent as Window).GraphPanel_Plot(interp, this);
                }
                catch (Error error)
                {
                    err = error.Message;
                    Canvas.Invalidate();
                }
            }

            private void OnPaint(object sender, PaintEventArgs e)
            {
                if (TextInput.Focused) e.Graphics.DrawRectangle(new Pen(SystemColors.ActiveCaption, 2), Canvas.Bounds);

                //e.Graphics.DrawString(TextInput.Text, Globals.MathsFont, Globals.MathsBrush, 0, 10);

                //Roots.ForEach(r => r.draw(0, Canvas.Height / 2 - 1, e.Graphics, cursor));
                //var acc = 0; //this is okay because the first group is initialised before the first invalidation and can't be disposed unless the whole Tile is.
                //for (int i = 0; i < Roots.Count; i++)
                //{
                //    try { acc += Roots[i - 1].width; } catch (Exception) { }
                //    Roots[i].draw(acc, Canvas.Height / 2, e.Graphics, cursor);
                //}
                try { Root.draw(0, CanvasHeight / 2, e.Graphics, cursor); }
                catch (Error error) { err = error.Message; }
                //e.Graphics.DrawLine(new Pen(Color.Crimson), 0, Canvas.Height / 2, Canvas.Width, Canvas.Height / 2);
                
                e.Graphics.DrawLine(new Pen(SystemColors.GrayText), 4, CanvasHeight, Canvas.Width - PlotBtn.Width - 8, CanvasHeight);//pretty border line
                e.Graphics.DrawString(err, new Font(Globals.MathsFont.FontFamily, 8), Globals.MathsBrush, 0, CanvasHeight + 2);
                answer.draw(0, Canvas.Height + 4 - answer.height, e.Graphics, cursor);
            }

            /// <summary>
            /// Recursive function that is only ever used in FromKeyDown(Key_ValueTuple key) to
            /// handle the event of the up arrow key being pressed
            /// </summary>
            /// <param name="p">the Piece above which is the output</param>
            /// <returns></returns>
            Piece move_up(Piece p)
            {
                Piece cout;

                if (p.group.parent == null)
                    cout = p.super.pieces.Any() ? p.sub.pieces[0] : p;
                else if (p.group.Equals(p.group.parent.super))
                    cout = move_up(p.group.parent);
                else if (p.group.Equals(p.group.parent.sub))
                    cout = p.group.parent;
                else
                    cout = p.group.parent.super.pieces.Any() ? p.group.parent.sub.pieces[0] : p;

                return cout;
            }
            /// <summary>
            /// Recursive function that is only ever used in FromKeyDown(Key_ValueTuple key) to
            /// handle the event of the down arrow key being pressed
            /// </summary>
            /// <param name="p">the Piece below which is the output</param>
            /// <returns></returns>
            Piece move_down(Piece p)
            {
                Piece cout;
                
                if (p.group.parent == null)
                    cout = p.sub.pieces.Any() ? p.sub.pieces[0] : p;
                else if (p.group.Equals(p.group.parent.sub))
                    cout = move_down(p.group.parent);
                else if (p.group.Equals(p.group.parent.super))
                    cout = p.group.parent;
                else
                    cout = p.group.parent.sub.pieces.Any() ? p.group.parent.sub.pieces[0] : p;

                return cout;
            }
            /// <summary>
            /// Recursive function that is only ever used in FromKeyDown(Key_ValueTuple key) to
            /// handle the event of the right arrow key being pressed
            /// </summary>
            /// <param name="p">the Piece to the right of which is the output</param>
            /// <returns></returns>
            Piece move_right(Piece p)
            {
                Piece cout;
                try
                {
                    cout = p.group.pieces.Last().Equals(p) ?
                    p.group.parent == null ? p : move_right(p.group.parent) :
                    p.group.pieces[p.group.pieces.FindIndex(q => q.Equals(p)) + 1];
                }
                catch (Exception e) { cout = e is IndexOutOfRangeException ? p : p.group.parent; }
                return cout;
            }

            static bool Between(Group group, OperatorPiece bracket, out Group between, out int other)
            {
                between = new Group(bracket);
                var level = 1;
                var index = group.pieces.IndexOf(bracket);
                if (bracket.val == "(")
                {
                    for (int i = index + 1; ; i++)
                    {
                        if (i > group.pieces.Count - 1)
                        {
                            between.pieces.Add(sort.qsort(group.pieces.Skip(index).ToList(), a => a.height)[0].Item1);
                            other = index;
                            break;
                        }
                        if (group.pieces[i].val == "(")
                            level++;
                        if (group.pieces[i].val == ")")
                        {
                            level--;
                            if (level == 0)
                            {
                                between.pieces = group.pieces.GetRange(index + 1, i - index);
                                if (between.pieces.Count == 0) between.pieces.Add(bracket);
                                other = i;
                                break;
                            }
                        }
                    }
                }
                else if (bracket.val == ")")
                {
                    for (int i = index - 1; ; i--)
                    {
                        if (i < 0)
                        {
                            between.pieces.Add(sort.qsort(group.pieces.Skip(index).ToList(), a => a.height)[0].Item1);
                            other = index;
                            break;
                        }
                        if (group.pieces[i].val == ")")
                            level++;
                        if (group.pieces[i].val == "(")
                        {
                            level--;
                            if (level == 0)
                            {
                                between.pieces = group.pieces.GetRange(i + 1, index - i);
                                if (between.pieces.Count == 0) between.pieces.Add(bracket);
                                other = i;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    other = index;
                    return false;
                }

                return true;
            }

            Point tensor_coords;

            enum piece_group { super, sub, none, initial }
            piece_group next_piece_group = piece_group.initial;
            enum piece_type { tensor, scalar, function, operator_sign, ambiguous }
            piece_type next_piece_type = piece_type.ambiguous;
            /// <summary>
            /// Handles the KeyDown event when it happens while this InputText is Focused.
            /// </summary>
            /// <param name="key">Keycode of the key being pressed</param>
            public void FromKeyDown(Keys key, bool shift)
            {
                switch (key)
                {
                    case Keys.Up:
                        if(cursor.group.parent is TensorPiece && 
                            !(cursor.group.parent.super.pieces.Contains(cursor) || cursor.group.parent.sub.pieces.Contains(cursor)))
                        {
                            tensor_coords.Y--;
                            var tensor = cursor.group.parent as TensorPiece;

                            if (tensor_coords.Y < 0)
                            {
                                tensor_coords.Y = 0;
                                tensor.Magnate(TensorPiece.side.top);
                            }
                            cursor = tensor.array[tensor_coords.Y][tensor_coords.X].pieces[0];
                        }
                        else cursor = cursor.super.pieces.Any() ? cursor.super.pieces[0] : move_up(cursor);
                        //CurrentIContainer = cursor.Interpret().Container;
                        break;
                    case Keys.Down:
                        if (cursor.group.parent is TensorPiece && 
                            !(cursor.group.parent.super.pieces.Contains(cursor) || cursor.group.parent.sub.pieces.Contains(cursor)))
                        {
                            tensor_coords.Y++;
                            var tensor = cursor.group.parent as TensorPiece;

                            if (tensor.size.Height - 1 < tensor_coords.Y)
                                tensor.Magnate(TensorPiece.side.bottom);
                            cursor = tensor.array[tensor_coords.Y][tensor_coords.X].pieces[0];
                        }
                        else cursor = cursor.sub.pieces.Any() ? cursor.sub.pieces[0] : move_down(cursor);
                        
                        //CurrentIContainer = cursor.Interpret().Container;
                        break;
                    case Keys.Left:
                        if (cursor.group.parent is TensorPiece && cursor.group.pieces[0].Equals(cursor))
                        {
                            tensor_coords.X--;
                            var tensor = cursor.group.parent as TensorPiece;

                            if (tensor_coords.X < 0)
                            {
                                tensor_coords.X = 0;
                                tensor.Magnate(TensorPiece.side.left);
                            }
                            cursor = tensor.array[tensor_coords.Y][tensor_coords.X].pieces[0];
                        }
                        else if (cursor.group.parent is FunctionPiece && cursor.group.pieces[0].Equals(cursor))
                            cursor = cursor.group.parent;
                        else
                        {
                            try
                            {
                                cursor = cursor.group.pieces.FindIndex(q => q.Equals(cursor)) == 0 ?
                                cursor.group.parent == null ? cursor : cursor.group.parent :
                                cursor.group.pieces[cursor.group.pieces.FindIndex(q => q.Equals(cursor)) - 1]; 
                            }
                            catch (Exception) { }
                        }

                        //CurrentIContainer = cursor.Interpret().Container;
                        break;
                    case Keys.Right:
                        if (cursor.group.parent is TensorPiece && cursor.group.pieces.Last().Equals(cursor))
                        {
                            tensor_coords.X++;
                            var tensor = cursor.group.parent as TensorPiece;

                            if (tensor.size.Width - 1 < tensor_coords.X)
                                tensor.Magnate(TensorPiece.side.right);

                            cursor = tensor.array[tensor_coords.Y][tensor_coords.X].pieces[0];
                            //cursor.group.parent = (cursor.group.parent as TensorPiece).Magnate(TensorPiece.side.right, cursor);
                        }
                        else if (cursor is FunctionPiece)
                            cursor = (cursor as FunctionPiece).operand.pieces[0];
                        else
                        {
                            if (cursor.group.parent is FunctionPiece && cursor.group.pieces.Last().Equals(cursor))
                                cursor = cursor.group.parent;
                            cursor = move_right(cursor);
                        }

                        //if (cursor is BracketPiece)
                        //    cursor = (cursor as BracketPiece).inside.pieces[0];

                        //CurrentIContainer = cursor.Interpret().Container;
                        break;

                    case Keys.F2:
                        return;

                    case Keys.OemMinus when shift:
                        next_piece_group = piece_group.sub;
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.D6 when shift:
                        next_piece_group = piece_group.super;
                        next_piece_type = piece_type.ambiguous;
                        Canvas.Invalidate();
                        return;
                    case Keys.Oem4:
                        if (!shift)
                        {
                            //if the Tensor literal gets added to the current Group by default and can't form a new Group
                            //cursor.group.pieces.Add(new TensorPiece(cursor.group));
                            //cursor = cursor.group.pieces.Last();

                            tensor_coords = new Point(0, 0);
                            next_piece_type = piece_type.tensor;
                            AddPiece("");
                        }

                        Canvas.Invalidate();
                        return;
                    case Keys.Oem6:
                        if (!shift)
                        {
                            tensor_coords = default(Point);
                            next_piece_type = piece_type.ambiguous;
                            next_piece_group = piece_group.none;
                        }

                        Canvas.Invalidate();
                        return;
                    case Keys.Enter when !shift:
                        if (cursor.group.parent != null && cursor.group.parent is TensorPiece)
                        {
                            cursor = cursor.group.parent;
                            (cursor as TensorPiece).ShedEmptySides();
                        }
                        else if (cursor is TensorPiece)
                        {
                            tensor_coords = new Point(0, 0);
                            cursor = (cursor as TensorPiece).array[0][0].pieces[0];
                        }
                        break;
                    case Keys.Oem1:
                        if(shift && cursor is ScalarPiece)
                        {
                            next_piece_type = piece_type.function;
                            AddPiece(cursor.val.Substring(3));
                            next_piece_type = piece_type.ambiguous;
                        }
                        Canvas.Invalidate();
                        return;
                    case Keys.D9 when shift:
                        next_piece_type = piece_type.operator_sign;
                        AddPiece("(");
                        next_piece_type = piece_type.ambiguous;

                        Canvas.Invalidate();
                        return;
                    case Keys.D0 when shift:
                        if (cursor.group.pieces.Count > 1 && cursor.group.pieces[cursor.group.pieces.IndexOf(cursor) + 1].val == ")")
                        {
                            next_piece_type = piece_type.operator_sign;
                            AddPiece(")");
                            next_piece_type = piece_type.ambiguous;
                        }
                        else cursor = cursor.group.pieces[cursor.group.pieces.IndexOf(cursor) + 1];

                        Canvas.Invalidate();
                        return;
                    case Keys.OemMinus when !shift:
                        if (
                            cursor.group.parent is FunctionPiece &&
                            producti.arrprd
                            (
                            cursor.group.pieces.Skip(cursor.group.pieces.IndexOf(cursor)).ToList(),
                            (o, i) => { if (i.val == "(") o++; if (i.val == ")") o--; return o; },
                            0
                            ) == 0
                            )
                        { cursor = cursor.group.parent; }

                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece("-");
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.Oemplus when shift:
                        if (
                            cursor.group.parent is FunctionPiece &&
                            producti.arrprd
                            (
                            cursor.group.pieces.Skip(cursor.group.pieces.IndexOf(cursor)).ToList(),
                            (o, i) => { if (i.val == "(") o++; if (i.val == ")") o--; return o; },
                            0
                            ) == 0
                            )
                        { cursor = cursor.group.parent; }

                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece("+");
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.Oemcomma when shift:
                        //Roots.Add(new Group(null, 'n'));
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece("<");
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.Oemcomma when !shift:
                        //Roots.Add(new Group(null, 'n'));
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece(",");
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.OemPeriod when shift:
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece(">");
                        next_piece_type = piece_type.ambiguous;
                        Canvas.Invalidate();
                        return;
                    case Keys.Oemplus when !shift && cursor.val == ">":
                        var group1 = cursor.group;
                        var i1 = group1.pieces.IndexOf(cursor);
                        group1.pieces.Remove(cursor);
                        cursor = new OperatorPiece(">=", group1);
                        group1.pieces.Insert(i1, cursor);
                        break;
                    case Keys.Oemplus when !shift && cursor.val == "<":
                        var group2 = cursor.group;
                        var i2 = group2.pieces.IndexOf(cursor);
                        group2.pieces.Remove(cursor);
                        cursor = new OperatorPiece("<=", group2);
                        group2.pieces.Insert(i2, cursor);
                        break;
                    case Keys.Oemplus when !shift://this case must go after the other Keys.OemPlus cases because the conditions are ordered chronologically
                        //Roots.Add(new Group(null, 'n'));
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        if (cursor.val == ">")
                            AddPiece(">=");
                        if (cursor.val == "<")
                            AddPiece("<=");
                        AddPiece("=");
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.D8 when shift:
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece("*");
                        next_piece_type = piece_type.ambiguous;

                        Canvas.Invalidate();
                        return;
                    case Keys.Oem2:
                        if (!shift)
                        {
                            next_piece_group = piece_group.none;
                            next_piece_type = piece_type.operator_sign;
                            AddPiece("/");
                            //next_piece_group = piece_group.super;
                            next_piece_type = piece_type.ambiguous;
                        }

                        Canvas.Invalidate();
                        return;
                    case Keys.Back:
                        
                        if (cursor is ScalarPiece && (cursor as ScalarPiece).number && (cursor as ScalarPiece).val.Length > 1)
                            (cursor as ScalarPiece).val = (cursor as ScalarPiece).val.Remove((cursor as ScalarPiece).val.Length - 1);
                        else
                        {
                            Piece new_cursor;
                            if
                                (
                                cursor.group.parent is FunctionPiece && 
                                !(cursor.group.parent as FunctionPiece).built_in && 
                                cursor.group.pieces.Count == 2 && 
                                cursor.group.pieces[0].val == "(" && 
                                cursor.group.pieces[1].val == ")"
                                )
                            {
                                cursor = cursor.group.parent;
                            }
                            if (cursor.group.pieces.TakeWhile(p => p != cursor).Count() > 1)
                            {
                                new_cursor = cursor.group.pieces[cursor.group.pieces.IndexOf(cursor) - 1];
                                cursor.group.pieces.Remove(cursor);
                                cursor = new_cursor;
                            }
                            else
                            {
                                if (!(cursor is NullPiece && !(cursor.group.parent == null || cursor.group.parent is TensorPiece)))
                                {
                                    new_cursor = new NullPiece(cursor.group);
                                    cursor.group.pieces.Add(new_cursor);
                                }
                                else new_cursor = cursor.group.parent;
                                
                                cursor.group.pieces.Remove(cursor);
                                cursor = new_cursor;
                            }
                        }
                        next_piece_group = piece_group.none;
                        break;
                }
                
                //if (char.IsNumber(key.KeyChar) || key.KeyChar == '.')
                //{
                //    number_being_written += key.KeyChar;
                //}
                //else if (number_being_written != "")
                //{
                //    next_piece_type = piece_type.scalar;
                //    AddPiece(number_being_written);
                //}
                if (char.IsNumber(key.ToString().Last()) || key == Keys.OemPeriod) 
                {
                    if (cursor is ScalarPiece && (cursor as ScalarPiece).number && next_piece_group == piece_group.none)
                        (cursor as ScalarPiece).val += key == Keys.OemPeriod ? '.' : key.ToString().Last();
                    else
                    {
                        next_piece_type = piece_type.scalar;
                        AddPiece(key.ToString().Last() + "");
                    }
                }
                if (char.IsLetter(key.ToString()[0]) && key.ToString().Length == 1)
                {
                    var index = "Key" + (shift ? key.ToString()[0] : char.ToLower(key.ToString()[0]));

                    next_piece_type = piece_type.scalar;
                    AddPiece(index);
                }
                
                Canvas.Invalidate();
            }

            /// <summary>
            /// Handles the OnInputBtnFunc event in regards to how the focused Tile should be affected.
            /// </summary>
            /// <param name="func_name">name of the function being clicked on</param>
            public void FromInputBtnFunc(string func_name)
            {
                //if (CurrentIContainer is Formula)
                //{
                //    CurrentIContainer.parts.Add(new Term(CurrentIContainer));
                //    CurrentIContainer = (Term)CurrentIContainer.parts.Last();
                //}
                //(CurrentIContainer as Term).numerator.Add(new Function(CurrentIContainer, func_name, Globals.functions[func_name]));

                next_piece_type = piece_type.function;
                AddPiece(func_name);
                Canvas.Invalidate();
            }

            void AddPiece(string piece_name)
            {
                Func<string, Group, Piece> new_piece = (name, group) =>
                {
                    if (name == "(")
                        return new BracketPiece(group);

                    switch (next_piece_type)
                    {
                        case piece_type.function:
                            return new FunctionPiece(name, group);
                        case piece_type.scalar:
                            //var _name = new string(name.ToList().ConvertAll(c => c = c == 'd' ? '.' : c).ToArray());
                            return new ScalarPiece(/*name[0] == 'K' ? name : _*/name, group);
                        case piece_type.tensor:
                            return new TensorPiece(group);
                        case piece_type.operator_sign:
                            return new OperatorPiece(name, group); ;
                        default:
                            throw new DevError("DevError: ambiguous piece type");
                    }
                };
                
                switch (next_piece_group)
                {
                    case piece_group.super:
                        cursor.super.pieces.Add(new_piece(piece_name, cursor.super));
                        cursor = cursor.super.pieces.Last();
                        break;
                    case piece_group.sub:
                        cursor.sub.pieces.Add(new_piece(piece_name, cursor.sub));
                        cursor = cursor.sub.pieces.Last();
                        break;
                    case piece_group.none:
                        cursor.group.pieces.Insert(cursor.group.pieces.IndexOf(cursor) + 1, new_piece(piece_name, cursor.group));
                        cursor = cursor.group.pieces[cursor.group.pieces.IndexOf(cursor) + 1];
                        break;
                    default:
                        cursor.group.pieces.Add(new_piece(piece_name, cursor.group));
                        cursor = cursor.group.pieces.Last();
                        break;
                }

                cursor.group.pieces.RemoveAll(p => p.val == "null");

                if (cursor is TensorPiece)
                    cursor = (cursor as TensorPiece).array[0][0].pieces[0];
                if (cursor is FunctionPiece)
                {
                    if (!(cursor as FunctionPiece).built_in)
                        cursor.group.pieces.RemoveAt(cursor.group.pieces.Count - 2);
                    cursor = (cursor as FunctionPiece).operand.pieces[0];
                }
                if (cursor is BracketPiece)
                    cursor = (cursor as BracketPiece).inside.pieces[0];
                if (cursor.val == "(")
                {
                    next_piece_type = piece_type.operator_sign;
                    AddPiece(")");
                    next_piece_type = piece_type.ambiguous;
                    cursor = cursor.group.pieces[cursor.group.pieces.IndexOf(cursor) - 1];
                }

                next_piece_group = piece_group.none;
            }
            
            public List<Equation> Interpret()
            {
                var cout = Root.Interpret();

                #region green
                //localise all of the parameter variables of the function so that we don't get confusion between function parameter variables
                //if (IsFunctionIdentity) //TODO: move IsFunctionIdentity into Equation
                //{
                //    var equation = new Equation();
                //    var function = Root.pieces[0] as FunctionPiece;
                //    var identity = new Group(null) { pieces = Root.pieces.Skip(2).ToList() }.localise(function); 
                //    var parameter = (function.operand.pieces[0] as BracketPiece).inside.localise(function);

                //    equation.Add(new Function
                //        (
                //        equation, 
                //        function.val, 
                //        new List<Formula>() { parameter.Interpret(new Equation()) }, 
                //        identity.Interpret(new Equation()), 
                //        function.super.Interpret(new Equation())
                //        ));
                //}
                //else //for (int i = 0; i < Roots.Count; i++)
                //{
                //    try { equations.parts.Add(Roots[i].Interpret(equation) as IFormulaic); }
                //    catch (Exception e)
                //    {
                //        if (e.Message.Substring(0, 24) == "SyntaxError: Empty field")
                //        {
                //            var tiles = 0;
                //            for (int j = 0; j <= Parent.Controls.IndexOf(this); j++) if (Parent.Controls[j] is Tile) tiles++;
                //            throw new Exception(e.Message + " in Formula:" + (Root.Partition. + 1) + " of the Equation in Tile:" + tiles);//ERROR_TAG
                //        }
                //    }
                //}
                #endregion
                //only able to interpret the first equation atm
                return cout.ToList();
            }
            public BracketPiece DisplayFormula(Formula f, Group group = null)
            {
                if (f == null)//safety first
                    return null;

                var cout = new BracketPiece(group);
                cout.inside = new Group(cout);

                foreach(var p in f.parts)
                {
                    var t = p as Term;
                    var addend = new[] { cout.inside }; //the idea is that addend acts as a pointer (I can't make an actual pointer because Tile.Group is a managed type)
                    if (t.denominator.Count > 1)
                    {
                        cout.inside.pieces.Add(new OperatorPiece("/", cout.inside));
                        addend[0] = cout.inside.pieces[0].super;
                    }
                    foreach (var v in t.numerator.Skip(1))
                    {
                        switch (v)
                        {
                            case Tensor tensor when tensor.DimDim == 0://scalar
                                addend[0].pieces.Add(new ScalarPiece(tensor.known ? (tensor.parts[0][0] as Scalar).output.Value + "" : tensor.name,
                                    group, tensor.Index == null ? null : DisplayFormula(tensor.Index, group).inside.pieces));
                                if (tensor.Index != null)
                                    addend[0].pieces.Last().super.pieces.ForEach(s => s.group = addend[0].pieces.Last().super);
                                break;
                            case Tensor tensor when tensor.DimDim > 0://tensor/matrix
                                addend[0].pieces.Add(new TensorPiece(group, new Size(tensor.cols, tensor.parts.Count)));
                                (addend[0].pieces.Last() as TensorPiece).array = tensor.parts.ConvertAll(r => r.ConvertAll(c =>
                                { var tf = new Formula(null); tf.Add(c.Value(Globals.values)); return DisplayFormula(tf, group).inside; }));
                                break;
                            case Function function://function
                                addend[0].pieces.Add(new FunctionPiece(function.name, group));
                                (addend[0].pieces.Last() as FunctionPiece).operand =
                                    new Group(addend[0].pieces.Last(), new List<Piece> { DisplayFormula(function.Process, group) });
                                (addend[0].pieces.Last() as FunctionPiece).super = function.Index == null ? null : 
                                    DisplayFormula(function.Index, (addend[0].pieces.Last() as FunctionPiece).super).inside;
                                break;
                            case Operation operation://built-in function
                                addend[0].pieces.Add(new FunctionPiece(operation.name, group));
                                (addend[0].pieces.Last() as FunctionPiece).operand =
                                    new Group(addend[0].pieces.Last(), new List<Piece> { DisplayFormula(operation.Operands[0], group) });
                                break;
                            case Formula formula://brackets
                                addend[0].pieces.Add(DisplayFormula(formula, group));
                                (addend[0].pieces.Last() as BracketPiece).super = formula.Index == null ? null :
                                    DisplayFormula(formula.Index, (addend[0].pieces.Last() as BracketPiece).super).inside;
                                break;
                            default:
                                break;

                        }
                    }
                    if (t.denominator.Count > 1)
                    {
                        addend[0] = cout.inside.pieces[0].sub;
                        foreach (var v in t.denominator.Skip(1))
                        {
                            switch (v)
                            {
                                case Tensor tensor when tensor.DimDim == 0://scalar
                                    addend[0].pieces.Add(new ScalarPiece(tensor.name == null ? (tensor.parts[0][0] as Scalar).output.Value + "" : tensor.name,
                                        group, DisplayFormula(tensor.Index).inside.pieces));
                                    break;
                                case Tensor tensor when tensor.DimDim > 0://tensor/matrix
                                    addend[0].pieces.Add(new TensorPiece(group));
                                    (addend[0].pieces.Last() as TensorPiece).array = tensor.parts.ConvertAll(r => r.ConvertAll(c =>
                                    { var tf = new Formula(null); tf.Add(c.Value(Globals.values)); return DisplayFormula(tf).inside; }));
                                    break;
                                case Function function://function
                                    addend[0].pieces.Add(new FunctionPiece(function.name, group));
                                    (addend[0].pieces.Last() as FunctionPiece).operand =
                                        new Group(addend[0].pieces.Last(), new List<Piece> { DisplayFormula(function.Process, group) });
                                    break;
                                case Operation operation://built-in function
                                    addend[0].pieces.Add(new FunctionPiece(operation.name, group));
                                    (addend[0].pieces.Last() as FunctionPiece).operand =
                                        new Group(addend[0].pieces.Last(), new List<Piece> { DisplayFormula(operation.Operands[0], group) });
                                    break;
                                case Formula formula://brackets
                                    addend[0].pieces.Add(DisplayFormula(formula, group));
                                    break;
                                default:
                                    break;

                            }//same old
                        }
                    }
                }
                return cout;
            }
            public List<Equation> Output()
            {
                var cout = Interpret();
                cout.ForEach(c => c.Evaluate());
                return cout;
            }

            public void OnClick(object sender, EventArgs e)
            {
                (Parent.Parent as Window).IsTyping = true;
                Globals.FocusedTile = this;
                TextInput.Focus();
                //update each other Tile so that they look like they aren't selected [anymore]
                foreach (Control c in Parent.Controls) { if (c is Tile) (c as Tile).Canvas.Invalidate(); }
            }

            private void OnChange(object sender, EventArgs e)
            {   
                Canvas.Invalidate();
                //var equation = Interpret();

                if (((Tile)Parent.Controls[Parent.Controls.Count - 1]).TextInput.Text != "")
                {
                    Parent.Controls.Add(new Tile(Parent, new Point(Canvas.Location.X, Canvas.Location.Y + Canvas.Height)));
                    (Parent.Parent as Window).GuideLines.Add(new List<List<pontus>>[4]);
                    for (int i = 0; i < 4; i++)
                        (Parent.Parent as Window).GuideLines.Last()[i] = new List<List<pontus>>();
                }
                else
                {
                    if (Parent.Controls.Count > 6 && ((Tile)Parent.Controls[Parent.Controls.Count - 4]).TextInput.Text == "")
                    {
                        Parent.Controls.RemoveAt(Parent.Controls.Count - 1); //the TextInput
                        Parent.Controls.RemoveAt(Parent.Controls.Count - 1); //the Tile
                        Parent.Controls.RemoveAt(Parent.Controls.Count - 1); //the Canvas (all of these were added to the controls)

                        (Parent.Parent as Window).GuideLines.RemoveAt((Parent.Parent as Window).GuideLines.Count - 1);//the set of guidelines associateed with that tile
                    }
                }

                ((VScrollBar)Parent.Controls[0]).Maximum = (int)((Parent.Controls.Count - 4.0) * Size.Height * 10 / (3 * Parent.Height));
                //-1 because of scroller but then + 2 for extra scroll while cancelling
                //out the 3 in the denominator, / 3 because adding a tile will add 3 controls, * Size.Height / Parent.Height to get height of TilePane percentage 
                //Debug.Write("[" + ((VScrollBar)Parent.Controls[0]).Maximum + "'" + (int)((Parent.Controls.Count + 2.0) * Size.Height * 10 / (3 * Parent.Height)) + "]");
            }

            /// <summary>
            /// Resize the image to the specified width and height.
            /// </summary>
            /// <param name="image">The image to resize.</param>
            /// <param name="width">The width to resize to.</param>
            /// <param name="height">The height to resize to.</param>
            /// <returns>The resized image.</returns>
            static Image Scale(Image image, int width, int height)
            {
                var cout_rect = new Rectangle(0, 0, width, height);
                var cout = new Bitmap(width, height);

                cout.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(cout))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrap_mode = new System.Drawing.Imaging.ImageAttributes())
                    {
                        wrap_mode.SetWrapMode(WrapMode.Tile);
                        graphics.DrawImage(image, cout_rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrap_mode);
                    }
                }

                return cout as Image;
            }
        }
    }

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
        protected interface IVariable : IValuable
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
        }

        public enum relation { None, Equal, Greater, Less, GreaterEqual, LessEqual, Maps }
        [Serializable]
        public struct relpair//I could have made this a Tuple or an IEnumerable but I really really don't want this to cross-reference
        {
            public relation L;
            public relation R;

            public bool has(Func<relation, bool> cond) => cond(L) || cond(R);
            public bool has(relation r) => L == r || R == r;
        }

        [Serializable]
        protected abstract class IVariableContainer : IContainer
        {
            public virtual List<IFormulaic> parts { get; set; }
            public virtual void Add(IFormulaic part, bool new_equals_sign_flag = false) =>
                throw new DevError("DevError: IContainer.Add not supported here.");
            public virtual relpair TRelation { get => throw new DevError("DevError: IContainer.TRelation not supported here."); }

            /// <summary>
            /// Returns every IVariable inside the IVariableContainer. If there is anything in there that isn't IVariable,
            /// then it will come out as null.
            /// </summary>
            public IEnumerable<IVariable> Variables
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
                                            foreach (var v in (c as IVariableContainer).Variables)
                                                yield return v;
                                    }
                                yield return p2 as IVariable;//goto case Function; dammit C# 7 if only
                                break;
                            case Formula p3:
                                foreach (var v in (p3 as IVariableContainer).Variables)
                                    yield return v;
                                break;
                            case Operation p4:
                                foreach(var f in p4.Operands)
                                    foreach (var v in (f as IVariableContainer).Variables)
                                        yield return v;
                                switch(p4.Parameters)
                                {
                                    case List<Equation> l:
                                        foreach (var e in l)
                                            foreach (var v in (e as IVariableContainer).Variables)
                                                yield return v;
                                        break;
                                    case IEnumerable<Equation> i:
                                        foreach (var e in i)
                                            foreach (var v in (e as IVariableContainer).Variables)
                                                yield return v;
                                        break;
                                    case List<Formula> l1:
                                        foreach (var e in l1)
                                            foreach (var v in (e as IVariableContainer).Variables)
                                                yield return v;
                                        break;
                                    case IEnumerable<Formula> i1:
                                        foreach (var e in i1)
                                            foreach (var v in (e as IVariableContainer).Variables)
                                                yield return v;
                                        break;
                                    case Equation E:
                                        foreach (var v in (E as IVariableContainer).Variables)
                                            yield return v;
                                        break;
                                    case Formula F:
                                        foreach (var v in (F as IVariableContainer).Variables)
                                            yield return v;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case Term p5:
                                foreach (var v in (p5 as IVariableContainer).Variables)
                                    yield return v as IVariable;
                                break;
                            default:
                                break;
                        }
                        if (p is IFormulaic && (p as IFormulaic).Index != null)
                            foreach (var v in (p as IFormulaic).Index.Variables)
                                yield return v as IVariable;
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

            public void Remember(IVariable variable)
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
                var cout = dcpy(invendum) as IVariable;
                //make the Equation think that it doesn't already know the variable that it's trying to solve for, in case the solution is different from how it is remembered.
                var vars = (from v in _vars where v.Key != invendum.name select v.Value).ToDictionary(k => k.name);
                
                //var vars = values.ToList().ConvertAll(v => new Tensor
                //(v.Value.array.ConvertAll(r => r.ConvertAll(c => new Scalar(c) as IValuable)), false, v.Key) as IVariable);

                //TEMPORARY CODE
                var pp = (parts[0] as IContainer).parts[0] as Term; //first term of first formula
                var ppp = pp.parts[1] as IVariable; //first effective IVariable in the Term

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

                if (parts.Count == 2 && (parts[0] as IContainer).parts.Count == 1 && pp.parts.Count == 3 && cout.name == ppp.name)
                {
                    
                    if (vars.ContainsKey(ppp.name))
                    {
                        cout = vars[ppp.name] as Tensor;
                    }
                    else
                    {
                        if (cout is Function)
                        {
                            if (EqRelations[0] == relation.Maps)
                            {
                                var process = dcpy(parts[1] as Formula);
                                //process.Container = null;
                                (cout as Function).Process = process;
                            }
                            else cout = (cout as Function).Value(vars);
                        }
                        else cout = (parts[1] as Formula).Value(vars);
                        cout.name = ppp.name;
                        //vars.Add(cout.name, cout);
                    }
                }
                
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
                    foreach (var v in e.Variables.Where(s => s.name != null/* && !Globals.Parameters.ToList().Exists(p => p.name == s.name)*/))
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
                    if (Container == null)
                        return new relpair();
                    var t2 = Container.parts.IndexOf(this);
                    var t1 = new relpair();
                    if (Container.parts.Count > 0)
                    {
                        t1.L = this == Container.parts[0] ? relation.None : (Container as Equation).EqRelations[Container.parts.IndexOf(this) - 1];
                        t1.R = this == Container.parts.Last() ? relation.None :(Container as Equation).EqRelations[Container.parts.IndexOf(this)];
                    }
                    return t1;
                }
            }
            public override relpair TRelation => CRelation;

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
                        return (producti.arrprd<Tensor, IValuable>(parts.ConvertAll(p =>
                        (p as IFormulaic).Value(vars)), (s, p) => s.Value(vars).add(p, vars), 'c') as Tensor).pow(Index, vars);
                    }
                    catch (Error e)
                    {
                        //var ename = e.Message.Contains('\'') ? e.Message.SkipWhile(c => c != '\'').TakeWhile(c => c != '\'') : "";
                        //if (ename.Contains('y') || ename.Contains('x') || ename.Contains('z'))

                        //else
                        throw new SyntaxError(e.Message + " in Formula:" + Container.parts.IndexOf(this));
                    }
                }
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

            public Term(IContainer parent, bool reciprocal, Formula index = null)
            {
                Container = parent;
                numerator = new List<IFormulaic>() { new Tensor(new Scalar(1), false, null, null, this) };
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
                    try
                    {//look at this beautiful one-liner
                        return (producti.arrprd<Tensor, IFormulaic>(numerator.Where(n => n is Tensor ? //excuse numerator Tensors that are parameters from being unknown if they are unknown
                        n.Value(vars).known ? true : !Globals.Parameters.ToList().Exists(s => s.name == (n as Tensor).name) : true).ToList().ConvertAll(p => //pi product all of the numerator
                        (p as IFormulaic).Value(vars).pow((p as IFormulaic).Index, vars)), (s, p) => (s as IFormulaic).Value(vars).dot(p), 'c') as Tensor).dot
                        ((producti.arrprd<Tensor, IFormulaic>(denominator.Where(d => d is Tensor ? //excuse denominator Tensors that are parameters from being unknown if they are unknown
                        d.Value(vars).known ? true : !Globals.Parameters.ToList().Exists(s => s.name == (d as Tensor).name) : true).ToList().ConvertAll(p => //divide by pi product of all of the denominator
                        (p as IFormulaic).Value(vars).pow((p as IFormulaic).Index, vars)), (s, p) => (s as IFormulaic).Value(vars).dot(p), 'c') as Tensor).inv(vars)
                        as Tensor).pow(Index, vars); //raise the whole thing to whatever power it's supposed to be raised to
                    }
                    catch (Error e)
                    {
                        if (parts.Concat(from v in Variables where v is IFormulaic select v as IFormulaic).ToList()
                            .Exists(p => !Globals.Parameters.ToList().Exists(s => p is Tensor && s.name == (p as Tensor).name)))
                            return new Tensor(new Formula(null) { parts = new List<IFormulaic> { this } }, false, null, null, this);
                        throw new SyntaxError(e.Message + " in Term:" + Container.parts.IndexOf(this));
                    }
                }
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
        protected class Tensor : IFormulaic, IVariable
        {
            public List<List<IValuable>> parts;
            public int rows { get { return parts.Count; } }
            public int cols { get { return sort.qsort(parts, r => r.Count)[0].Item2; } }
            /// <summary>
            /// What type of number this is: Scalar => 0, vector => 1 & matrix => 2.
            /// </summary>
            public int DimDim { get { if (cols > 1) return 2; return (int)Math.Round((rows - 1) / (double)rows); } }
            /// <summary>
            /// whether or not the tensor has any unknown scalars in it.
            /// </summary>
            public bool known {
                get {
                    var unknown = false;
                    parts.ForEach(r => r.ForEach(c => unknown |= c is Scalar && !(c as Scalar).known));
                    return !unknown;
                } }
            public string name { get; set; }
            public IContainer Container { get; set; }
            public bool Reciprocal { get; set; }
            public Formula Index { get; set; }
            public relpair CRelation => Container == null ? new relpair() : Container.TRelation; //catch

            [Serializable]
            class FormulaNode : IContainer
            {
                public List<IFormulaic> parts { get; set; }
                public Tensor parent;
                public relpair CRelation { get; set; }
                public relpair TRelation => CRelation.L == default(relation) && CRelation.R == default(relation) ? new relpair() : CRelation;

                public FormulaNode(Tensor parent, relpair crelation)
                {
                    this.parent = parent;
                    CRelation = crelation;
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
            } FormulaNode formulaNode;

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
                if (name != null && vars.ContainsKey(name)) return vars[name] as Tensor;

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
                            throw new SyntaxError("Syntax Error: An element of a matrix or vector cannot be another matrix or vector"));
                    }
                }

                return cout;
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
                        if ((parts[0][0] as Scalar).output.Value == 0)
                            throw new MathsError("MathsError: Can't divide by zero.");
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
                        return new Tensor(new Scalar(Math.Pow((parts[0][0] as Scalar).output.Value, index)), false, null, name);
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
                    return new Tensor(new Scalar(/*name + "£dot£" + cin.name, */(parts[0][0] as Scalar).output * (cin.parts[0][0] as Scalar).output), false, null,  name == null ? cin.name : name);
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
                var new_parts = new List<List<Scalar>>();
                if (DimDim == 0 && cin.DimDim == 0)
                {
                    var t1 = new Tensor(new Scalar(/*name + "£add£" + cin.name, */(Value(vars).parts[0][0] as Scalar).output + (cin.Value(vars).parts[0][0] as Scalar).output), false, null, name == null ? cin.name : name);
                    return t1;
                }
                else if (DimDim == 0 ^ cin.DimDim == 0)
                {
                    throw new MathsError("Maths Error: You cannot add a scalar to a matrix or a vector"); //ERROR_TAG
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
                if (!cin.known || !known) throw new MathsError("MathsError: Cannot apply function to unknown variable"); //ERROR_TAG

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
        protected class Operation : IFormulaic
        {
            public IContainer Container { get; set; }
            public bool Reciprocal { get; set; }
            public List<Formula> Operands;
            public string name;
            public Formula Index { get; set; }
            public relpair CRelation => Container == null ? new relpair() : Container.TRelation; //catch

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
            }
            
            public Tensor Value(Dictionary<string, IVariable> vars)
            {
                try
                {
                    return Process.Invoke(Operands, Parameters);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e);//ERROR_TAG
                    throw new SyntaxError("SyntaxError: Invalid function parameters.");
                }
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

            public Function(IContainer container, string name, List<Formula> prms, Formula value, Formula index = null)
            {
                this.name = name;
                Container = container;
                Process = value;
                Parameters = prms;
                Index = index;

                ParameterNames = (from p in prms where p.AddedVariables.All(a => a.name != null) && p.AddedVariables.Count() > 0
                                 select p.AddedVariables.First().name).ToList();
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
                if (Globals.values.ContainsKey(name))
                {
                    var f = Globals.values[name] as Function;
                    List<Tensor> named_tensors;

                    try
                    {
                        //calculate the value of the parameters and then give them their localised names before passing them to the function
                        named_tensors = Parameters.ConvertAll(p => //get the value and name them
                        {
                            var t = p.Value(vars);
                            t.name = f.ParameterNames[Parameters.IndexOf(p)];
                            return t;
                        });
                        //var named_parameters = named_tensors.ConvertAll(t => //create new parameters that are just tensors with the localised names
                        //{
                        //    var f = new Formula(null);
                        //    f.Add(t);
                        //    return f;
                        //});

                        //var inputs = Parameters.ConvertAll(p => p.Value(vars) as IVariable);
                        var cout = f.Process.Value(vars.Values.Concat(named_tensors.ConvertAll(p => p as IVariable)).ToDictionary(a => a.name));
                        cout.name = name + "(" + producti.arrprd(named_tensors, (o, i) => o += ", " + i.name.Substring(2), "").Substring(2) + ")";
                        return cout;
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
    
    /// <summary>
    /// Inheriting this Form will give your class access to 3D maths and graphics algorithms.
    /// This abstract class contains no Form-related methods or objects, only the extras that come
    /// with my library (and all of Form's methods/fields).
    /// </summary>
    public class Engine : Form
    {

        #region variable declarations

        protected struct CameraVariables
        {
            /// <summary>
            /// camera zoom
            /// </summary>
            public double g;
            /// <summary>
            /// camera near-field
            /// </summary>
            public double nf;
            /// <summary>
            /// camera far-field
            /// </summary>
            public double ff;
            /// <summary>
            /// camera Frame of View
            /// </summary>
            public double fov;
            /// <summary>
            /// extension of camera nozzle
            /// </summary>
            public double cn;
            /// <summary>
            /// render distance
            /// </summary>
            public double rendd;

            /// <summary>
            /// x camera rotation
            /// </summary>
            public double xmr;
            /// <summary>
            /// y camera rotation
            /// </summary>
            public double ymr;
            /// <summary>
            /// z camera rotation
            /// </summary>
            public double zmr;
            /// <summary>
            /// camera rotation speed
            /// </summary>
            public double rspeed;
            /// <summary>
            /// camera panning speed
            /// </summary>
            public double mspeed;
        }
        /// <summary>
        /// Struct containing camera variables for this instance of Engine.
        /// </summary>
        protected CameraVariables cam;

        /// <summary>
        /// projection matrix
        /// </summary>
        protected matrix projm;

        Point mouse_drag_delta;

        List<figura> figurae = new List<figura>();

        #endregion

        #region object definitions

        #region interfacis
        //---------------------------------------------------------------------------------------------------------interfacis
        protected interface species
        {
            string elog();
        }

        protected interface figura : species
        {
            Color col { get; set; }
            bool sel { get; set; }
            bool stat { get; set; }
            int? id { get; set; }
            Control Parent { get; set; }

            figura real();

            figura rmat(matrix rotm, vector disv);
            figura pmat(matrix prjm, double cn);
            void plot(Engine v, PaintEventArgs e);
            void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e);
            void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e, out float X, out float Y);
        }

        protected interface mathematica
        {
            mathematica nummul(double b);
            mathematica numadd(double b);
            double numdot(mathematica b);
            mathematica matdot(mathematica b);
            mathematica matvec(mathematica b);
            mathematica matadd(mathematica b);
            mathematica matmat(matrix a);

            List<double> pares { get; }
        }
        #endregion

        #region figurae
        //---------------------------------------------------------------------------------------------------------figurae
        [Serializable]
        protected class form : figura
        {
            public Color col { get; set; }
            public bool sel { get; set; }
            public bool rend { get; set; }
            public bool stat { get; set; }
            public int? id { get; set; }
            public Control Parent { get; set; }
            public string file { get; set; }

            public effector part;

            public vector pos = vector.o;
            public vector rot = vector.Y;
            public bool coll;
            public double sphere;
            /// <summary> whether or not this entity interacts with physics </summary>
            public bool phys { get { return this is tetrahedron || gettetrahedra().Count > 0; } }
            public pontus mid;

            public List<pontus> ponti;
            public List<lin> lineae;
            public List<reg> regies;
            public List<form> formae;

            //indivens
            public form()
            {
                ponti = new List<pontus>();
                lineae = new List<lin>();
                regies = new List<reg>();
                formae = new List<form>();

                pos = vector.o;
                v = vector.o;
                omega = vector.o;
                stat = false;

                coll = false;
                F = vector.o;
                tau = vector.o;
                I = matrix.I(3, 0);
            }
            public form(int? hoc, vector p, bool st, params List<figura>[] figs)
            {
                for (int i = 1; i < figs.Count(); i++) { figs[0].AddRange(figs[i]); }

                formae = new List<form>();

                for (int i = 0; i < figs[0].Count; i++)
                {
                    if (figs[0][i] is form)
                    { formae.Add((form)(figs[0][i])); }

                }
                id = hoc;

                ponti = figs[0].FindAll(s => s is pontus).ConvertAll(delegate (figura f) { f.stat = st; f.id = id; return (pontus)f; });
                lineae = figs[0].FindAll(s => s is lin).ConvertAll(delegate (figura f) { f.stat = st; f.id = id; return (lin)f; });
                regies = figs[0].FindAll(s => s is reg).ConvertAll(delegate (figura f) { f.stat = st; f.id = id; return (reg)f; });

                coll = false;
                init();

                F = vector.o;
                tau = vector.o;
                v = vector.o;
                omega = vector.o;

                pos = p;
                stat = st;
            }
            public form(int? hoc, string path, vector p, bool st, params List<figura>[] figs)
            {
                for (int i = 1; i < figs.Length; i++) { figs[0].AddRange(figs[i]); }
                id = hoc;

                ponti = figs[0].FindAll(s => s is pontus).ConvertAll(delegate (figura f) { f.stat = st; f.id = id; return (pontus)f; });
                lineae = figs[0].FindAll(s => s is lin).ConvertAll(delegate (figura f) { f.stat = st; f.id = id; return (lin)f; });
                regies = figs[0].FindAll(s => s is reg).ConvertAll(delegate (figura f) { f.stat = st; f.id = id; return (reg)f; });
                formae = figs[0].FindAll(s => s is form).ConvertAll(delegate (figura f) { f.stat = st; f.id = id; return (form)f; });

                coll = false;
                init();
                F = vector.o;
                tau = vector.o;
                v = vector.o;
                omega = vector.o;

                pos = p;
                file = path;
                stat = st;
            }
            public form(List<form> tetrahedra, bool st, int? hoc)
            {
                ponti = new List<pontus>();
                lineae = new List<lin>();
                regies = new List<reg>();
                this.formae = tetrahedra;//.ConvertAll(delegate(form t) { t.id = hoc; return t; });

                coll = false;
                init();
                F = vector.o;
                tau = vector.o;
                v = vector.o;
                omega = vector.o;

                this.stat = st;
                id = hoc;
            }

            public List<figura> figs()
            {
                var cout = new List<figura>();

                if (ponti != null) { cout.AddRange(ponti.ConvertAll(delegate (pontus f) { return (figura)(new pontus(f.matadd(pos).pares, ((pontus)f.real()).pares, f.col, Parent, stat, id)); })); }
                if (lineae != null) { cout.AddRange(lineae.ConvertAll(delegate (lin f) { return (figura)(new lin((pontus)f.a.matadd(pos), (pontus)f.b.matadd(pos), f.col, Parent, stat, id)); })); }
                if (regies != null) { cout.AddRange(regies.ConvertAll(delegate (reg f) { return f == null ? null : (figura)(new reg(f.ponti.ConvertAll(delegate (pontus s) { return (pontus)s.matadd(pos); }), f.col, stat, id)); })); }
                if (formae != null) { cout.AddRange(formae.ConvertAll(delegate (form f) { return (figura)(new form(id, pos, stat, f.figs())); })); }
                cout.ForEach(delegate (figura f) { if (f != null) { f.sel = sel; } });

                return cout;
            }

            public virtual pontus getmid()
            {
                if (formae.Count > 0)
                {
                    var cout = pontus.o;
                    foreach (var s in formae) { cout = new pontus(cout.matadd(s.mid).pares); }
                    var t1 = cout.nummul(1 / formae.Count).pares;
                    var t2 = cout.nummul(1 / formae.Count);
                    var t3 = 1.0 / formae.Count;
                    return new pontus(cout.nummul(1.0 / formae.Count).pares);
                }
                else if (ponti.Count > 0)
                {
                    return (pontus)producti.arrprd<pontus>(ponti, (sum, part) => (pontus)sum.matadd(part), pontus.o).nummul(1.0 / ponti.Count);
                }
                else if (lineae.Count > 0)
                {
                    return (pontus)producti.arrprd(lineae, (sum, part) => (pontus)sum.matadd(part.a).matadd(part.b), pontus.o).nummul(0.5 / lineae.Count);
                }
                else if (regies.Count > 0)
                {
                    return (pontus)producti.arrprd(regies, (sum, part) => (pontus)sum.matadd(part.tmid()), pontus.o).nummul(1.0 / lineae.Count);
                }
                else
                {
                    throw new Exception("mate there's no middle to this");
                }
            }

            public void overlay(vector v)
            {
                ponti.ForEach(p => p.vector = (vector)v.matadd((pontus)p));
                lineae.ForEach(l => { l.a.vector = (vector)v.matadd((pontus)l.a.real()); l.a.vector = (vector)pos.matadd((pontus)l.a.real()); });
                foreach (var r in regies) { r.ponti.ForEach(p => p.vector = (vector)v.matadd((pontus)p.real())); }
                formae.ForEach(f => f.overlay(v));
            }
            public List<figura> overlay()
            {
                var cout = new List<figura>();
                cout = cout.Concat(ponti.ConvertAll(delegate (pontus p) { p.rx += pos.x; p.ry += pos.y; p.rz += pos.z; return (figura)p; })).ToList();
                cout = cout.Concat(lineae.ConvertAll(delegate (lin l) { l.a.rx += pos.x; l.a.ry += pos.y; l.a.rz += pos.z; l.b.rx += pos.x; l.b.ry += pos.y; l.b.rz += pos.z; return (figura)l; })).ToList();
                cout = cout.Concat(regies.ConvertAll(delegate (reg r) { foreach (var p in r.ponti) { p.rx += pos.x; p.ry += pos.y; p.rz += pos.z; } return (figura)r; })).ToList();

                return cout;
            }

            public List<pontus> getpoints() { var cout = dcpy(ponti); foreach (var f in formae) { cout.AddRange(f is tetrahedron ? f.ponti : f.getpoints()); } return cout; }
            public List<lin> getlins() { var cout = dcpy(lineae); foreach (var f in formae) { cout.AddRange(f is tetrahedron ? f.lineae : f.getlins()); } return cout; }
            public List<reg> getregs() { var cout = dcpy(regies); foreach (var f in formae) { cout.AddRange(f is tetrahedron ? f.regies : f.getregs()); } return cout; }
            public List<tetrahedron> gettetrahedra()
            {
                var cout = dcpy(formae.Where(f => f is tetrahedron).Cast<tetrahedron>().ToList());
                foreach (var f in formae) { if (f is tetrahedron) { cout.Add((tetrahedron)f); } else { cout.AddRange(f.gettetrahedra()); } }
                return cout;
            }
            public int getpointsno() { var cout = ponti.Count; foreach (var f in formae) { cout += f.getpointsno(); } return cout; }
            public int getlinsno() { var cout = lineae.Count; foreach (var f in formae) { cout += f.getlinsno(); } return cout; }
            public int getregsno() { var cout = regies.Count; foreach (var f in formae) { cout += f.getregsno(); } return cout; }
            public int gettetrahedrano()
            {
                var cout = 0;
                //var cper = formae.Where(f => f is tetrahedron).Cast<tetrahedron>().ToList().Count;
                foreach (var f in formae) { if (f is tetrahedron) { cout++; } else { cout += f.gettetrahedrano(); } }
                return cout;
            }
            public int getformaeno()
            {
                var cout = 0;
                foreach (var f in formae) { cout += f.getformaeno() + 1; }
                return cout;
            }

            /// <summary>Will pour all of the polygons, lines and points into each given list of polygons, 
            /// lines and points respectively.</summary>
            public void pouri(List<pontus> ps, List<lin> ls, List<reg> rs)
            {
                ps.AddRange(ponti);
                ls.AddRange(lineae);
                rs.AddRange(regies);
                formae.ForEach(s => s.pouri(ps, ls, rs));
            }
            public void pouri(List<figura> ps, List<figura> ls, List<reg> rs)
            {
                var testc = this;
                ps.AddRange(ponti);
                ls.AddRange(lineae);
                rs.AddRange(regies);
                formae.ForEach(s => s.pouri(ps, ls, rs));
            }
            public void pouri(List<figura> fs)
            {
                var testc = this;
                fs.AddRange(ponti);
                fs.AddRange(lineae);
                fs.AddRange(regies);
                formae.ForEach(s => s.pouri(fs));
            }

            public virtual bool touching(form f, Engine v)
            {
                var intersecting = false;

                foreach (tetrahedron h in formae.Where(s => s is tetrahedron))
                {
                    foreach (tetrahedron a in f.formae.Where(s => s is tetrahedron))
                    {
                        intersecting |= h.touching(a, v);
                    }
                }

                return intersecting;
            }
            public virtual bool touching(form o, out pontus I, out planus cp, out tetrahedron f, out tetrahedron t, Engine v)
            {
                var intersecting = false;

                I = null;
                cp = new planus(vector.o);
                f = null;
                t = null;

                foreach (tetrahedron h in formae.Where(s => s is tetrahedron))
                {
                    foreach (tetrahedron a in o.formae.Where(s => s is tetrahedron))
                    {
                        pontus i;
                        planus _cp;

                        intersecting |= h.touching(a, out i, out _cp, v);

                        I = i;
                        cp = _cp;
                        f = a;
                        t = h;
                    }
                }

                var test = mid;
                var test1 = o.mid;
                if (!(I == null || o is tetrahedron)) { cp = new planus(new via(mid, o.mid).pnt(0.5).vector, I); }

                return intersecting;
            }

            //reageret physicacum

            /// <summary>mass</summary>
            public double m
            {
                get
                {
                    return this is tetrahedron ?
                        ((tetrahedron)this).matter.density * ((tetrahedron)this).volume() :
                        producti.arrprd(formae.ConvertAll(f => f.m), (sum, part) => sum + part);
                }
            }
            public double elasticity;
            public double energy;
            public double friction;
            /// <summary>linear velocity</summary>
            public vector v;
            /// <summary>resultant linear force</summary>
            public vector F;
            /// <summary>angular velocity</summary>
            public vector omega;
            /// <summary>torque (rotational force)</summary>
            public vector tau;
            /// <summary>inertia tensor</summary>
            public matrix I;
            /// <summary>inverse of inertia tensor</summary>
            public matrix Iinv;

            public vector eF;
            public vector etau;

            public void init()
            {
                mid = getmid();

                if (phys)
                {
                    var ps = getpoints().ConvertAll(p => { var t = p.tenor; var cout = (pontus)p.matadd(pos.inv()); cout.tenor = t; return cout; });
                    Func<List<pontus>, int, int, double> princ = delegate (List<pontus> lps, int i, int j)
                    {
                        return producti.sum(0, ps.Count - 1,
                            new dynamic[] { ps }, (k, s) => (((pontus)s[0][k]).pares[i] * ((pontus)s[0][k]).pares[i] +
                                                            ((pontus)s[0][k]).pares[j] * ((pontus)s[0][k]).pares[j]) *
                                                            ((pontus)s[0][k]).tenor.m / 4);
                    };
                    Func<List<pontus>, int, int, double> prodi = delegate (List<pontus> lps, int i, int j)
                    {
                        return producti.sum(0, ps.Count - 1,
                            new dynamic[] { ps }, (k, s) => ((pontus)s[0][k]).pares[i] * ((pontus)s[0][k]).pares[j] *
                                                            ((pontus)s[0][k]).tenor.m / 4);
                    };

                    var arr = new double[,]
                    {
                        { princ(ps, 1, 2), prodi(ps, 0, 1), prodi(ps, 0, 2) },
                        { prodi(ps, 0, 1), princ(ps, 0, 2), prodi(ps, 1, 2) },
                        { prodi(ps, 0, 2), prodi(ps, 1, 2), princ(ps, 0, 1) },
                    };

                    I = new matrix(arr);
                    Iinv = I.inv();
                    var tr = id;
                    var test = getpoints();
                    var test1 = mid;
                    var sorted = sort.qsort(getpoints(), (a, b) => ((pontus)a.matadd(mid.vector.inv())).vector.length() >
                                                          ((pontus)a.matadd(mid.vector.inv())).vector.length());
                    sphere = sorted.Count > 0 ? ((pontus)mid.matadd(sorted[sorted.Count - 1].vector.inv())).vector.length() : 0;

                    part = null;
                }
            }

            public double getmass()
            {
                if (this is tetrahedron) { return m; }
                else { var mout = 0.0; foreach (var f in formae) { mout += f.m; } return mout; }
            }

            public void react()
            {
                if (phys)
                {
                    //rotation
                    omega = (vector)omega.matadd(tau.matmat(I.inv()));
                    var tspeed = omega.length();
                    var L = new via(omega, mid.vector);
                    var theta = omega.length();

                    if (part is motor) { part.stat += ((vector)omega.nummul(part.dir.numdot(omega))).length(); }

                    rot = vector.quatrot(L, rot, theta);
                    ponti = ponti.ConvertAll(p => p.withvec(vector.quatrot(L, p.vector, theta)));
                    lineae = lineae.ConvertAll(delegate (lin l) { l.a = l.a.withvec(vector.quatrot(L, l.a.vector, theta)); l.b = l.b.withvec(vector.quatrot(L, l.b.vector, theta)); return l; });
                    for (int r = 0; r < regies.Count; r++) { regies[r].ponti = regies[r].ponti.ConvertAll(p => p.withvec(vector.quatrot(L, p.vector, theta))); }

                    //movement
                    v = (vector)v.matadd(F.nummul(1 / m));
                    pos = (vector)pos.matadd(v);

                    if (part is distensor) { part.stat += ((vector)v.nummul(part.dir.numdot(v))).length(); }

                    formae.ForEach(f => f.react(tau, F, mid, m, I));
                    overlay(v);
                }
            }
            public void react(vector t, vector F, pontus mid, double mass, matrix It)
            {
                if (phys)
                {
                    //rotation
                    omega = (vector)omega.matadd(t.matmat(It.inv()));
                    var L = new via(omega, mid.vector);
                    var tspeed = omega.length();

                    ponti = ponti.ConvertAll(p => p.withvec(vector.quatrot(L, p.vector, omega.length())));
                    lineae = lineae.ConvertAll(delegate (lin l) { l.a = l.a.withvec(vector.quatrot(L, l.a.vector, omega.length())); l.b = l.b.withvec(vector.quatrot(L, l.b.vector, omega.length())); return l; });
                    for (int r = 0; r < regies.Count; r++) { regies[r].ponti = regies[r].ponti.ConvertAll(p => p.withvec(vector.quatrot(L, p.vector, omega.length()))); }

                    var tesc = this;
                    //movement
                    v = (vector)v.matadd(F.nummul(1 / mass));
                    pos = (vector)pos.matadd(v);

                    formae.ForEach(f => f.react(t, F, mid, mass, It));
                }
            }

            //figurae
            public string elog()
            {
                var cout = "{";

                ponti.ForEach(delegate (pontus s) { cout += s.elog(); }); cout += "#";
                lineae.ForEach(delegate (lin s) { cout += s.elog(); }); cout += "#";
                regies.ForEach(delegate (reg s) { cout += s.elog(); }); cout += "#";
                formae.ForEach(delegate (form s) { cout += s.elog(); }); cout += "#";

                return cout + "#" + new pontus(pos.pares, pos.pares, col, Parent, stat, id).elog() + "#" + stat + "}";
            }

            public figura real() { return new form(id, file, pos, stat, figs().ConvertAll(delegate (figura f) { return f.real(); })); }

            public figura rmat(matrix rotm, vector disv)
            {
                var cout = dcpy(this);
                if (stat) { disv = vector.o; }

                cout.ponti = ponti.ConvertAll(f => (pontus)f.rmat(rotm, f.stat ? vector.o : disv));
                cout.lineae = lineae.ConvertAll(f => (lin)f.rmat(rotm, f.stat ? vector.o : disv));
                cout.regies = regies.ConvertAll(f => (reg)f.rmat(rotm, f.stat ? vector.o : disv));
                cout.formae = formae.ConvertAll(f => (form)f.rmat(rotm, f.stat ? vector.o : disv));

                return cout;
            }
            public figura pmat(matrix prjm, double cn)
            {
                var cout = new form
                    (id, file, pos, stat,
                    ponti.ConvertAll(delegate (pontus f) { return f.pmat(prjm, cn); }),
                    lineae.ConvertAll(delegate (lin f) { return f.pmat(prjm, cn); }),
                    regies.ConvertAll(delegate (reg f) { return f.pmat(prjm, cn); }),
                    formae.ConvertAll(delegate (form f) { return f.pmat(prjm, cn); })
                    );
                cout.v = v;
                return cout;
            }
            public void plot(Engine v, PaintEventArgs e)
            {
                sel = figs().Exists(s => s.sel);
                ponti.ForEach(delegate (pontus s) { s.sel = sel; s.plot(v, e); });
                lineae.ForEach(delegate (lin s) { s.sel = sel; s.plot(v, e); });
                regies.ForEach(delegate (reg s)
                {
                    s.sel = sel;
                    s.plot(v, e);
                });
                formae.ForEach(delegate (form s) { s.sel = sel; s.plot(v, e); });
            }
            public void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e)
            {
                sel = figs().Exists(s => s.sel);
                ponti.ForEach(delegate (pontus s) { s.sel = sel; s.plot(mouse_drag_delta, v, e); });
                lineae.ForEach(delegate (lin s) { s.sel = sel; s.plot(mouse_drag_delta, v, e); });
                regies.ForEach(delegate (reg s)
                {
                    s.sel = sel;
                    s.plot(v, e);
                });
                formae.ForEach(delegate (form s) { s.sel = sel; s.plot(v, e); });
            }
            public void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e, out float X, out float Y)
            {
                sel = figs().Exists(s => s.sel);
                ponti.ForEach(delegate (pontus s) { s.sel = sel; s.plot(mouse_drag_delta, v, e); });
                lineae.ForEach(delegate (lin s) { s.sel = sel; s.plot(mouse_drag_delta, v, e); });
                regies.ForEach(delegate (reg s)
                {
                    s.sel = sel;
                    s.plot(v, e);
                });
                formae.ForEach(delegate (form s) { s.sel = sel; s.plot(v, e); });

                X = (float)pos.x;
                Y = (float)pos.y;
            }
            public void selc(Engine v) { v.figurae.FindAll(s => s.id == id).ForEach(delegate (figura s) { s.sel = sel; }); }
            public void selc(Engine v, bool selin)
            {
                sel = selin;

                ponti = ponti.ConvertAll(delegate (pontus f) { f.sel = sel; return f; });
                lineae = lineae.ConvertAll(delegate (lin f) { f.sel = sel; return f; });
                regies = regies.ConvertAll(delegate (reg f) { f.sel = sel; return f; });
                formae = formae.ConvertAll(delegate (form f) { f.sel = sel; return f; });
            }
        }
        [Serializable]
        protected class pontus : figura, mathematica
        {
            public double x;
            public double y;
            public double z;

            public double rx;
            public double ry;
            public double rz;

            public Color col { get; set; }
            public bool sel { get; set; }
            public bool rend { get; set; }
            public bool stat { get; set; }
            public int? id { get; set; }
            public Control Parent { get; set; }

            public tetrahedron tenor = null;

            //indivens
            public pontus(double _x, double _y, double _z) { x = _x; y = _y; z = _z; col = new Color(); sel = false; rend = true; }
            public pontus(IList<double> a) { x = a[0]; y = a[1]; z = a[2]; col = new Color(); sel = false; rend = true; }

            public pontus(double _x, double _y, double _z, double _rx, double _ry, double _rz)
            { x = _x; y = _y; z = _z; rx = _rx; ry = _ry; rz = _rz; }
            public pontus(IList<double> a, IList<double> ra)
            { x = a[0]; y = a[1]; z = a[2]; rx = ra[0]; ry = ra[1]; rz = ra[2]; }

            public pontus(double _x, double _y, double _z, double _rx, double _ry, double _rz, Color c, Control parent, bool st, int? hoc)
            { x = _x; y = _y; z = _z; rx = _rx; ry = _ry; rz = _rz; col = c; Parent = parent; stat = st; sel = false; rend = true; id = hoc; }
            public pontus(IList<double> a, IList<double> ra, Color c, Control parent, bool st, int? hoc)
            { x = a[0]; y = a[1]; z = a[2]; rx = ra[0]; ry = ra[1]; rz = ra[2]; col = c; Parent = parent; stat = st; sel = false; rend = true; id = hoc; }
            /// <summary> We're basically just deep-copying at this point </summary>
            public pontus(double x, double y, double z, double rx, double ry, double rz, Color c, Control parent, bool st, int? hoc, tetrahedron tenor, float d, bool render, bool selected)
            {
                this.x = x; this.y = y; this.z = z;
                this.rx = rx;this.ry = ry;this.rz = rz;
                col = c; this.Parent = parent; stat = st; id = hoc;
                this.tenor = tenor; diameter = d; rend = render; sel = selected;
            }
            /// <summary>fake coordinates are changed to be the same as the real coordinates</summary>
            public pontus vr() { var cout = dcpy(this); cout.x = rx; cout.y = ry; cout.z = rz; return cout; }
            /// <summary>real coordinates are changed to be the same as the fake coordinates</summary>
            public pontus rv() => new pontus(rx, ry, rz, rx, ry, rz, col, Parent, stat, id, tenor, diameter, rend, sel);
            public pontus r { get { return rv(); } set { rx = value.x; ry = value.y; rz = value.z; } }
            public void usevec(vector v) { rx = v.x; ry = v.y; rz = v.z; rv(); }
            public pontus withvec(vector v) { var cout = this; cout.usevec(v); return cout; }

            public vector vector { get { return new vector(pares); } set { x = value.x; y = value.y; z = value.z; } }
            public double[] ToArray() { return new[] { x, y, z }; }

            public static bool equals(pontus a, pontus b) { return a.equals(b); }
            public static bool fequals(pontus a, pontus b) { return a.fequals(b); }
            /// <summary>whether real components are equal</summary>
            public bool requals(mathematica m) { return m.pares[0] == rx && m.pares[1] == ry && m.pares[2] == rz; }
            /// <summary>whether every property except 'id' is equal</summary>
            public bool fequals(pontus p) { return equals(p) && requals(p) && p.sel == sel && p.stat == stat && p.rend == rend && p.col == col; }

            /// <summary>Returns the point at the origin of the world</summary>
            public static pontus o { get => new pontus(0, 0, 0); }

            //figurae
            public string elog() { return "[pnt'" + x + "'" + y + "'" + z + "'" + (rend ? "T" : "F") + "'" + (col.IsEmpty ? "NaN" : "<" + col.A + "," + col.R + "," + col.G + "," + col.B + ">") + "]"; }

            public figura real() { return new pontus(rx, ry, rz, rx, ry, rz, col, Parent, stat, id); }

            public figura rmat(matrix rotm, vector disv)
            { var cout = stat ? this : new pontus(matadd(disv).matmat(rotm).pares, ((pontus)real()).pares, col, Parent, stat, id); cout.sel = sel; return cout; }
            public figura pmat(matrix prjm, double cn)
            {
                quaternion _c;
                _c = new quaternion(1, pares);
                _c = _c.ijkw();
                _c = ((quaternion)(_c.matmat(prjm)));
                var c = _c.dehom();
                c.rx = rx; c.ry = ry; c.rz = rz; c.col = col; c.rend = z >= cn; c.sel = sel; c.id = id; return c;
            }
            public float diameter = 2;
            public void plot(Engine v, PaintEventArgs e)
            {
                if (rend)
                {
                    //marshal-by-reference class proxies
                    var md = v.mouse_drag_delta;
                    var c = v.cam;

                    e.Graphics.FillEllipse(new SolidBrush(col), (int)(x * c.g + Parent.Width / 2 + md.X), (int)(y * c.g + Parent.Height / 2 + md.Y), diameter, diameter);

                    //e.Graphics.DrawString("(" + id + "|" + rx + ", " + ry + ", " + rz + ")", new Font("Arial", 8), new SolidBrush(SystemColors.ControlText),
                    //       (int)(x * v.g + Parent.Width / 2 + md.X) + 10, (int)(y * v.g + Parent.Height / 2 + md.Y) + 10);

                    if (sel) { e.Graphics.DrawEllipse(new Pen(new SolidBrush(Color.Red), 2), (int)(x * c.g + Parent.Width / 2 + md.Y - 2), (int)(y * c.g + Parent.Height / 2 + md.Y - 2), 6, 6); }
                }
            }
            public void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e)
            {
                if (rend)
                {
                    //marshal-by-reference class proxies
                    var md = mouse_drag_delta;
                    var c = v.cam;

                    e.Graphics.FillEllipse(new SolidBrush(col), (int)(x * c.g + Parent.Width / 2 + md.X), (int)(y * c.g + Parent.Height / 2 + md.Y), diameter, diameter);

                    //e.Graphics.DrawString("(" + id + "|" + rx + ", " + ry + ", " + rz + ")", new Font("Arial", 8), new SolidBrush(SystemColors.ControlText),
                    //       (int)(x * v.g + Parent.Width / 2 + md.X) + 10, (int)(y * v.g + Parent.Height / 2 + md.Y) + 10);

                    if (sel) { e.Graphics.DrawEllipse(new Pen(new SolidBrush(Color.Red), 2), (int)(x * c.g + Parent.Width / 2 + md.Y - 2), (int)(y * c.g + Parent.Height / 2 + md.Y - 2), 6, 6); }
                }
            }
            public void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e, out float X, out float Y)
            {
                //marshal-by-reference class proxies
                var md = mouse_drag_delta;
                var c = v.cam;

                X = (float)(x * c.g + Parent.Width / 2 + md.X);
                Y = (float)(y * c.g + Parent.Height / 2 + md.Y);

                if (rend)
                {
                    e.Graphics.FillEllipse(new SolidBrush(col), X, Y, diameter, diameter);

                    //e.Graphics.DrawString("(" + id + "|" + rx + ", " + ry + ", " + rz + ")", new Font("Arial", 8), new SolidBrush(SystemColors.ControlText),
                    //       (int)(x * v.g + Parent.Width / 2 + md.X) + 10, (int)(y * v.g + Parent.Height / 2 + md.Y) + 10);

                    if (sel) { e.Graphics.DrawEllipse(new Pen(new SolidBrush(Color.Red), 2), (int)(x * c.g + Parent.Width / 2 + md.Y - 2), (int)(y * c.g + Parent.Height / 2 + md.Y - 2), 6, 6); }
                }
            }

            public PointF ScreenPos(Engine v)
            {
                //marshal-by-reference class proxies
                var md = v.mouse_drag_delta;
                var c = v.cam;

                return new PointF((float)x * (float)c.g + Parent.Width / 2 + md.X - diameter / 2, (float)y * (float)c.g + Parent.Height / 2 + md.Y - diameter / 2);
            }
            public PointF ScreenPos(Engine v, Point mouse_drag_delta)
            {
                //marshal-by-reference class proxies
                var md = mouse_drag_delta;
                var c = v.cam;

                return new PointF((float)x * (float)c.g + Parent.Width / 2 + md.X - diameter / 2, (float)y * (float)c.g + Parent.Height / 2 + md.Y - diameter / 2);
            }

            public bool IsTouchingMouse(Engine v)
            {
                //marshal-by-reference class proxies
                var md = v.mouse_drag_delta;
                var c = v.cam;

                return (((x * c.g + v.Width / 2 + md.X - diameter <= (MousePosition.X - v.Left - 8) && (MousePosition.X - v.Left - 8) <= x * c.g + v.Width / 2 + md.X + 2 * diameter) &
                        (y * c.g + v.Height / 2 + md.Y - diameter <= (MousePosition.Y - v.Top - 31) && (MousePosition.Y - v.Top - 31) <= y * c.g + v.Height / 2 + md.Y + 2 * diameter)));
            }

            //mathematicae
            public List<double> pares => new List<double> { x, y, z };
            public bool equals(mathematica a) { return a.pares.Count == 3 && a.pares[0] == x & a.pares[1] == y & a.pares[2] == z; }
            /// <summary> Multiply point vector by coefficient 'b' </summary>
            public mathematica nummul(double b)
            { return col.IsEmpty ? new pontus(pares[0] * b, pares[1] * b, pares[2] * b) : new pontus(pares[0] * b, pares[1] * b, pares[2] * b, rx, ry, rz, col, Parent, stat, id); }
            /// <summary> Adds 'b' to each component of point </summary>
            public mathematica numadd(double b)
            { return col.IsEmpty ? new pontus(pares[0] * b, pares[1] * b, pares[2] * b) : new pontus(pares[0] + b, pares[1] + b, pares[2] + b, rx, ry, rz, col, Parent, stat, id); }
            /// <summary> Dot-Product point with first 3 components of 'b' </summary>
            public double numdot(mathematica b)
            {
                var c = 0.0;
                for (int i = 0; i < b.pares.Count && i < pares.Count; i++)
                {
                    c += pares[i] * b.pares[i];
                }
                return c;
            }
            /// <summary> Multiply each component of point by corresponding component of 'b' </summary>
            public mathematica matdot(mathematica b)
            {
                var c = new List<double>();
                for (int i = 0; i < pares.Count; i++)
                {
                    c.Add(pares[i] * b.pares[i]);
                }
                return new pontus(c[0], c[1], c[2], rx, ry, rz, col, Parent, stat, id);
            }
            /// <summary> Cross-Product with 'b' </summary>
            public mathematica matvec(mathematica b)
            {
                return new pontus
                (
                pares[1] * b.pares[2] - pares[2] * b.pares[1],
                pares[2] * b.pares[0] - pares[0] * b.pares[2],
                pares[0] * b.pares[1] - pares[1] * b.pares[0],
                rx, ry, rz, col, Parent, stat, id
                );
            }
            /// <summary> Translate point by 'a' </summary>
            public mathematica matadd(mathematica a) { return new pontus(x + a.pares[0], y + a.pares[1], z + a.pares[2], rx, ry, rz, col, Parent, stat, id); }
            /// <summary> Transform point by matrix 'm' </summary>
            public mathematica matmat(matrix m)
            {
                var c = new[] { 0.0, 0, 0 };

                for (int i = 0; i < pares.Count; i++)
                {
                    for (int j = 0; j < m.array[i].Count; j++)
                    {
                        c[i] += pares[j] * m.array[i][j];
                    }
                }
                return new pontus(c, new[] { rx, ry, rz }, col, Parent, stat, id);
            }
        }
        [Serializable]
        protected class tetrahedron : form, figura, species
        {
            public bool phys { get; set; }

            public material matter;

            //individens
            protected tetrahedron() { coll = false; }
            public tetrahedron(pontus a, pontus b, pontus c, pontus d, reg aa, reg bb, reg cc, reg dd, int id)
            {
                ponti = new List<pontus> { a, b, c, d }.ConvertAll(delegate (pontus s) { s.id = id; return s; });
                regies = new List<reg> { aa, bb, cc, dd }.ConvertAll(delegate (reg s) { s.id = id; return s; });
                this.id = id;
                coll = false;
                phys = true;
                lineae = vlins().ConvertAll(delegate (lin s) { s.id = id; return s; });
                formae = new List<form>();

                ponti.ForEach(p => p.tenor = this);

                init();
            }
            public tetrahedron(pontus a, pontus b, pontus c, pontus d, reg aa, reg bb, reg cc, reg dd)
            {
                ponti = new List<pontus> { a, b, c, d };
                regies = new List<reg> { aa, bb, cc, dd };
                this.id = id;
                coll = false;
                phys = true;
                lineae = vlins().ConvertAll(delegate (lin s) { s.id = id; return s; });
                formae = new List<form>();

                ponti.ForEach(p => p.tenor = this);

                init();
            }
            public tetrahedron(pontus a, pontus b, pontus c, pontus d)
            {
                ponti = new List<pontus> { a, b, c, d };
                regies = new List<reg>
                {
                    new reg(surf(0), Color.Transparent, false, null),
                    new reg(surf(1), Color.Transparent, false, null),
                    new reg(surf(2), Color.Transparent, false, null),
                    new reg(surf(3), Color.Transparent, false, null)
                };
                id = null; phys = false;
                coll = false;
                lineae = vlins();
                formae = new List<form>();

                ponti.ForEach(p => p.tenor = this);

                init();
            }
            public tetrahedron(pontus a, pontus b, pontus c, pontus d, Color ca, Color cb, Color cc, Color cd, material mat, bool st, int? hoc)
            {
                ponti = new List<pontus> { a, b, c, d }.ConvertAll(delegate (pontus s) { s.id = hoc; return s; });
                regies = new List<reg>
                {
                    new reg(surf(0), ca, st, hoc),
                    new reg(surf(1), cb, st, hoc),
                    new reg(surf(2), cc, st, hoc),
                    new reg(surf(3), cd, st, hoc)
                };
                lineae = vlins().ConvertAll(delegate (lin s) { s.id = hoc; return s; });

                matter = mat;
                phys = true;

                coll = false;

                stat = st;
                id = hoc;
                formae = new List<form>();

                ponti.ForEach(p => p.tenor = this);

                init();
            }

            public void setid(int id) { this.id = id; }
            public static tetrahedron import(string line, int id)
            {
                var vertices = new pontus[4];
                var cols = new Color[4];

                int j = 3;
                var num = new List<char>();
                var com = new double[3];
                int alpha;

                for (int i = 0; i < 4; i++)
                {
                    for (; line[j] != ' '; j++) { num.Add(line[j]); }
                    j++;
                    com[0] = Double.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                    num.Clear();
                    for (; line[j] != ' '; j++) { num.Add(line[j]); }
                    j++;
                    com[1] = Double.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                    num.Clear();
                    for (; line[j] != ' '; j++) { num.Add(line[j]); }
                    j++;
                    com[2] = Double.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                    num.Clear();
                    vertices[i] = new pontus(com, com);

                    for (; line[j] != ' '; j++) { num.Add(line[j]); }
                    j++;
                    alpha = Int32.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                    num.Clear();
                    for (; line[j] != ' '; j++) { num.Add(line[j]); }
                    j++;
                    com[0] = Int32.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                    num.Clear();
                    for (; line[j] != ' '; j++) { num.Add(line[j]); }
                    j++;
                    com[1] = Int32.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                    num.Clear();
                    for (; line[j] != ' '; j++) { num.Add(line[j]); }
                    j++;
                    com[2] = Int32.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                    num.Clear();
                    cols[i] = alpha == -1 ? Color.Transparent : Color.FromArgb(alpha, (int)com[0], (int)com[1], (int)com[2]);
                }

                for (; line[j] != ' '; j++) { num.Add(line[j]); }
                j++;
                com[0] = Double.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                num.Clear();
                for (; line[j] != ' '; j++) { num.Add(line[j]); }
                j++;
                com[1] = Double.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                num.Clear();
                for (; line[j] != ' '; j++) { num.Add(line[j]); }
                j++;
                com[2] = Double.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                num.Clear();
                var mat = new material(com[0], com[1], com[2]);

                for (; line[j] != ' '; j++) { num.Add(line[j]); }
                j++;
                com[0] = Int32.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                num.Clear();
                var rl = com[0] == 1;

                for (; line[j] != ' '; j++) { num.Add(line[j]); }
                j++;
                com[1] = Int32.Parse(new string(num.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                var stat = com[1] == 1;

                var cout = new tetrahedron(vertices[0], vertices[1], vertices[2], vertices[3], cols[0], cols[1], cols[2], cols[3], mat, stat, id);


                //for (int i = 0; i < 4; i++) { cout.regies[i] = cols[i] == Color.Transparent ? null : new reg(cout.surf(i), cols[i], false, id); }
                //cout.id = id;


                cout.phys = rl;
                cout.ponti.ForEach(p => p.tenor = cout);

                return cout;
            }
            public string export()
            {
                string cout = "[t ";

                for (int i = 0; i < 4; i++)
                {
                    cout += ponti[i].rx; cout += " ";
                    cout += ponti[i].ry; cout += " ";
                    cout += ponti[i].rz; cout += " ";

                    cout += regies[i] == null ? "-1 " : regies[i].col.A + " ";
                    cout += regies[i] == null ? "-1 " : regies[i].col.R + " ";
                    cout += regies[i] == null ? "-1 " : regies[i].col.G + " ";
                    cout += regies[i] == null ? "-1 " : regies[i].col.B + " ";

                }

                cout += phys ? "1 " : "0 ";
                cout += "0]";
                //cout += id + "]";

                //cout += "]";
                return cout;
            }
            public double volume()
            {
                var bse = new planus(ponti[0].vector, ponti[1].vector, ponti[2].vector);
                var h = ((vector)new via(bse.norm(), ponti[3].vector).intersect(bse).vector.inv().matadd(ponti[3])).length();

                var s = (new vector(ponti[0], ponti[1]).length() + new vector(ponti[1], ponti[2]).length() + new vector(ponti[2], ponti[0]).length()) / 2;
                var a = Math.Sqrt(s * (s - new vector(ponti[0], ponti[1]).length()) * (s - new vector(ponti[1], ponti[2]).length()) * (s - new vector(ponti[2], ponti[0]).length()));

                return h * a / 3;
            }
            public List<lin> lins()
            {
                var cout = new lin[]
                {
                    new lin(ponti[0], ponti[1], new Color(), Parent, false, null),
                    new lin(ponti[0], ponti[2], new Color(), Parent, false, null),
                    new lin(ponti[0], ponti[3], new Color(), Parent, false, null),
                    new lin(ponti[1], ponti[2], new Color(), Parent, false, null),
                    new lin(ponti[1], ponti[3], new Color(), Parent, false, null),
                    new lin(ponti[2], ponti[3], new Color(), Parent, false, null),
                };
                return cout.ToList();
            }
            public List<lin> vlins()
            {
                var c = Color.Red;
                if (regies.Exists(s => s != null)) { c = regies.Where(s => s != null).ToList().Find(s => !s.col.IsEmpty).col; }
                var cout = new lin[]
                {
                    new lin(ponti[0], ponti[1], c, Parent, false, null),
                    new lin(ponti[0], ponti[2], c, Parent, false, null),
                    new lin(ponti[0], ponti[3], c, Parent, false, null),
                    new lin(ponti[1], ponti[2], c, Parent, false, null),
                    new lin(ponti[1], ponti[3], c, Parent, false, null),
                    new lin(ponti[2], ponti[3], c, Parent, false, null),
                };
                return cout.ToList();
            }
            public List<pontus> surf(int id)
            { var cout = new List<pontus>(); for (int i = 0; i < 4; i++) { if (i != id) { cout.Add(ponti[i]); } } return cout; }
            public void updreg() { for (int i = 0; i < 4; i++) { regies[i].ponti = surf(i); } }
            public void updlin()
            {
                for (int i = 0; i < 6; i++)
                {
                    if (i < 3) { lineae[i].a = ponti[0]; lineae[i].b = ponti[i + 1]; }
                    if (i > 2) { lineae[i].a = ponti[i == 5 ? 2 : 1]; lineae[i].b = ponti[i == 5 ? 3 : i - 1]; }
                }
            }
            public override pontus getmid()
            {
                var ms = new vector[4];
                vector m;
                for (int i = 0; i < 4; i++)
                {
                    vector[] ps = surf(i).ConvertAll(delegate (pontus p) { return new vector(p.ToArray()); }).ToArray();
                    var fac = new planus(ps[0], ps[1], ps[2]);

                    var t4 = (vector)ps[0].matadd(ps[1]);
                    var t1 = ((vector)ps[0].matadd(ps[1]).nummul(0.5));
                    var t2 = ((vector)ps[0].matadd(ps[1]).nummul(0.5)).pares;
                    var t3 = new pontus(((vector)ps[0].matadd(ps[1]).nummul(0.5)).pares);
                    var t5 = ps[2].pares;
                    var t6 = new pontus(ps[2].pares);
                    var t7 = new via(new pontus(((vector)ps[0].matadd(ps[1]).nummul(0.5)).pares), new pontus(ps[2].pares));

                    var m1 = new via(new pontus(((vector)ps[0].matadd(ps[1]).nummul(0.5)).pares), new pontus(ps[2].pares));
                    var m2 = new via(new pontus(((vector)ps[2].matadd(ps[1]).nummul(0.5)).pares), new pontus(ps[0].pares));

                    ms[i] = m1.intersect(m2);
                }
                var l1 = new via(new vector(ponti[0].matadd(ms[0].inv()).pares), ms[0]);
                var l2 = new via(new vector(ponti[1].matadd(ms[1].inv()).pares), ms[1]);
                m = l1.intersect(l2);

                var te1 = new pontus(m.pares);
                return new pontus(m.pares);
            }
            public bool pcontains(pontus p, Engine v)
            {
                var Is = 0;
                var m = new pontus(mid.pares);

                if (
                    new vector(p, m).length() < new vector(ponti[0], m).length() &&
                    lins().Exists(l => !Double.IsNaN(new via(p, m).intersect(l).x)) &&
                    !lcontains(new lin(m, p, new Color(), Parent, false, null), v)
                    )
                { Is = 4; }
                return Is == 4;
            }
            public bool lcontains(lin l, Engine v)
            {
                var Is = 0;
                var m = mid;
                var pas = new via(l.a, l.b);

                for (int i = 0; i < 4; i++)
                {
                    vector[] ps = surf(i).ConvertAll(delegate (pontus f) { return new vector(f.ToArray()); }).ToArray();
                    var nv = (vector)ps[0].matadd(ps[1].inv()).matvec(ps[0].matadd(ps[2].inv()));
                    var fac = new planus(nv.x, nv.y, nv.z, nv.numdot(ps[0]));

                    var t = 0.0;

                    var I = fac.norm();//new reg(ps.ToList().ConvertAll(s => new pontus(s.pares))).mid(v);
                    var testing_v = ps.ToList().ConvertAll(s => (vector)new reg(ps.ToList().ConvertAll(r => new pontus(r.pares))).tmid().inv().matadd(s));
                    var testing_a = Math.Acos(I.unit().z) * 180 / Math.PI;
                    var testing_va = ((vector)I.matvec(new vector(0, 0, 1))).unit();
                    var testing_q = testing_v.ToList().ConvertAll(s => vector.quatrot(testing_va, s, testing_a * Math.PI / 180));
                    var qs = ps.ToList().ConvertAll(s => vector.quatrot(((vector)I.matvec(new vector(0, 0, 1))).unit(), (vector)new reg(ps.ToList().ConvertAll(r => new pontus(r.pares))).tmid().inv().matadd(s), Math.Acos(I.unit().z)));//transform 's' by the inverse of the mid of the 'ps' reg
                                                                                                                                                                                                                                            //var P = v.quatrot(((vector)I.matvec(new vector(0, 0, 1))).unit(), new vector(fac.intersect(pas).pares), Math.Acos(I.unit().z));
                    var P = vector.quatrot(((vector)I.matvec(new vector(0, 0, 1))).unit(), (vector)new reg(ps.ToList().ConvertAll(r => new pontus(r.pares))).tmid().inv().matadd(new vector(fac.intersect(pas).pares)), Math.Acos(I.unit().z));

                    if (fac.intersect(pas, out t).rend && reg.pcontains(qs.ConvertAll(s => new PointF((float)s.x, (float)s.y)), (float)P.x, (float)P.y, v))
                    {
                        Is = 4;
                    }
                }
                return Is == 4;
            }

            public override bool touching(form f, Engine v)
            {
                var intersecting = false;
                if (f is tetrahedron)
                {
                    var that = (tetrahedron)f;

                    var ma = that.mid;
                    var mi = this.mid;

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            intersecting |= this.regies[i].lcontains(that.lineae[j]) || that.regies[i].lcontains(this.lineae[j]); //no edges are intersecting
                        }
                    }
                }

                return intersecting;
            }
            public bool ttouching(form f, Engine v)
            {
                var intersecting = false;
                if (f is tetrahedron)
                {
                    var that = (tetrahedron)f;

                    var ma = that.mid;
                    var mi = this.mid;

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            intersecting |= this.regies[i].lcontains(that.lineae[j]) || that.regies[i].lcontains(this.lineae[j]); //no edges are intersecting
                        }
                    }

                    intersecting &= !this.ponti.Exists(i => that.ponti.Exists(a => i.equals(a)));
                }

                return intersecting;
            }
            public override bool touching(form o, out pontus I, out planus cp, out tetrahedron f, out tetrahedron t, Engine v)
            {
                var intersecting = false;

                I = null;
                cp = new planus(vector.o);
                f = null;
                t = null;

                if (o is tetrahedron)
                {
                    var that = (tetrahedron)o;
                    var ps = new List<pontus>();

                    var ma = that.mid;
                    var mi = this.mid;


                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            pontus p;

                            if (this.regies[i].lcontains(that.lineae[j], out p) || that.regies[i].lcontains(this.lineae[j], out p))
                            { intersecting = true; cp = regies[i].planus(); t = this; f = (tetrahedron)o; }//no edges are intersecting

                            ps.Add(p);
                        }
                    }

                    foreach (var s in ps) { I = new pontus(I.matadd(s.vector).pares); }
                    I = new pontus(I.nummul(1 / ps.Count).pares);
                }

                return intersecting;
            }
            public bool touching(form o, out pontus I, out planus cp, Engine v)
            {
                var intersecting = false;

                I = null;
                cp = new planus(vector.o);

                if (o is tetrahedron)
                {
                    var that = (tetrahedron)o;
                    var ps = new List<pontus>();

                    var ma = that.mid;
                    var mi = this.mid;

                    var Is = new List<reg>();
                    var As = new List<reg>();

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            pontus p;

                            if (this.regies[i].lcontains(that.lineae[j], out p))
                            {
                                intersecting = true;
                                cp = regies[i].planus();
                                ps.Add(p);
                                Is.Add(this.regies[i]);
                            }//no edges are intersecting
                            if (that.regies[i].lcontains(this.lineae[j], out p))
                            {
                                intersecting = true;
                                cp = regies[i].planus();
                                ps.Add(p);
                                As.Add(that.regies[i]);
                            }
                        }
                    }

                    if (Is.Count == 2 && As.Count == 2)
                    {
                        var li = new List<pontus>();
                        var la = new List<pontus>();

                        foreach (var u in Is[0].ponti) { var com = Is[1].ponti.Find(d => d.equals(u)); if (com != null) { li.Add(com); } }
                        foreach (var u in As[0].ponti) { var com = As[1].ponti.Find(d => d.equals(u)); if (com != null) { la.Add(com); } }

                        var t1 = new planus((vector)new vector(li[0], li[1]).matvec(new vector(la[0], la[1])), (pontus)la[0].matadd(li[0]).nummul(1 / 2.0));
                        cp = new planus((vector)new vector(li[0], li[1]).matvec(new vector(la[0], la[1])), (pontus)la[0].matadd(li[0]).nummul(1 / 2.0));
                        //cp = new planus((vector)new vector(li[0], li[1]).matvec(new vector(ps[0], ps[1])), li[0]);
                    }
                    if (Is.Count > 0 || As.Count > 0)
                    {
                        I = producti.arrprd<pontus>(ps, (sum, part) => (pontus)sum.matadd(part), pontus.o);
                        I = new pontus(I.nummul(1 / ps.Count).pares);
                    }
                }

                return intersecting;
            }

            //species
            public string elog()
            {
                var cout = "[thn|";

                for (int i = 0; i < 4; i++)
                {
                    cout += ponti[i].elog() + "_" + regies[i].elog() + "|";
                }

                return cout + "]";
            }

            public override bool Equals(object obj)
            {
                var tetrahedron = obj as tetrahedron;
                return tetrahedron != null &&
                       phys == tetrahedron.phys;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return -438176243 + phys.GetHashCode();
                }
            }
        }

        [Serializable]
        protected class lin : figura
        {
            public pontus a;
            public pontus b;
            public Color col { get; set; }
            public bool sel { get; set; }
            public bool rend { get; set; }
            public bool stat { get; set; }
            public int? id { get; set; }
            public Control Parent { get; set; }

            //indivens
            public lin(pontus _a, pontus _b, Color colin, Control parent, bool st, int? hoc) { a = _a; b = _b; col = colin; Parent = parent; stat = st; sel = false; rend = true; id = hoc; }
            public static PointF? _2di(PointF p1, PointF p2, PointF p3, PointF p4)
            {
                try
                {
                    return new PointF(((p1.X * p2.Y - p1.Y * p2.X) * (p3.X - p4.X) - (p1.X - p2.X) * (p3.X * p4.Y - p3.Y * p4.X)) /
                                     ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X)),
                                     ((p1.X * p2.Y - p1.Y * p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X * p4.Y - p3.Y * p4.X)) /
                                     ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X)));
                }
                catch (DivideByZeroException) { return null; }
            }

            public vector vector() { return new vector(a, b); }

            //figurae
            public string elog() { return "[lin|" + a.x + "'" + a.y + "'" + a.z + "|" + b.x + "'" + b.y + "'" + b.z + "|" + (rend ? "T" : "F") + "<" + col.A + "," + col.R + "," + col.G + "," + col.B + ">" + "]"; }

            public figura real() { return new lin((pontus)a.real(), (pontus)b.real(), col, Parent, stat, id); }

            public figura rmat(matrix rotm, vector disv)
            { var cout = stat ? this : new lin((pontus)a.matadd(disv).matmat(rotm), (pontus)b.matadd(disv).matmat(rotm), col, Parent, stat, id); cout.sel = sel; return cout; }
            public figura pmat(matrix prjm, double cn)
            {
                var q = a;
                var p = b;

                if (q.z < cn && p.z >= cn) { q = new pontus(p.x + (cn - p.z) * (p.x - q.x) / (p.z - q.z), p.y + (cn - p.z) * (p.y - q.y) / (p.z - q.z), cn); }
                if (p.z < cn && q.z >= cn) { p = new pontus(p.x + (cn - p.z) * (p.x - q.x) / (p.z - q.z), p.y + (cn - p.z) * (p.y - q.y) / (p.z - q.z), cn); }

                var I = new lin(((quaternion)(new quaternion(1, q.pares).ijkw().matmat(prjm))).dehom(), ((quaternion)(new quaternion(1, p.pares).ijkw().matmat(prjm))).dehom(), col, Parent, stat, id);
                I.sel = sel; I.rend = p.z >= cn | q.z >= cn;

                return I;
            }

            public bool IsTouchingMouse(Engine v)
            {
                var x1 = ScreenPos(v).Item1.X;
                var y1 = ScreenPos(v).Item1.Y;
                var x2 = ScreenPos(v).Item2.X;
                var y2 = ScreenPos(v).Item2.Y;

                return (y1 - y2) / (x1 - x2) <= ((MousePosition.Y - v.Top - 31) - y2 + 4) / (MousePosition.X - v.Left - 8) + x2 * (y1 - y2) / ((x1 - x2) * (MousePosition.X - v.Left - 8)) &&
                       (y1 - y2) / (x1 - x2) >= ((MousePosition.Y - v.Top - 31) - y2 - 4) / (MousePosition.X - v.Left - 8) + x2 * (y1 - y2) / ((x1 - x2) * (MousePosition.X - v.Left - 8)) &&
                       (MousePosition.X - v.Left - 8) <= x1 && (MousePosition.X - v.Left - 8) >= x2 || ((MousePosition.X - v.Left - 8) >= x1 && (MousePosition.X - v.Left - 8) <= x2) ||
                       (x1 == x2 && x2 <= (MousePosition.X - v.Left - 8) + 4 && (MousePosition.X - v.Left - 8) - 4 <= x2 &&
                       ((y1 <= (MousePosition.Y - v.Top - 31) + 1 && (MousePosition.Y - v.Top - 31) - 1 <= y2) || (y2 <= (MousePosition.Y - v.Top - 31) + 1 && (MousePosition.Y - v.Top - 31) - 1 <= y1)));
            }

            /// <summary>
            /// Method used to determine the appearance of the line between (or beyond) the two PointFs.
            /// </summary>
            public Action<Graphics, PointF, PointF> Draw = null;

            public struct PointF_ValueTuple
            {
                public PointF Item1;
                public PointF Item2;
                public PointF_ValueTuple(PointF a, PointF b) { Item1 = a; Item2 = b; }
            }

            public PointF_ValueTuple ScreenPos(Engine v)
            {
                //marshal-by-reference class proxies
                var md = v.mouse_drag_delta;
                var c = v.cam;

                return
                new PointF_ValueTuple(
                new PointF((float)(a.x * c.g + Parent.Width / 2 + md.X),
                           (float)(a.y * c.g + Parent.Height / 2 + md.Y)),
                new PointF((float)(b.x * c.g + Parent.Width / 2 + md.X),
                           (float)(b.y * c.g + Parent.Height / 2 + md.Y))
                );
            }
            public PointF_ValueTuple ScreenPos(Engine v, Point mouse_drag_delta)
            {
                //marshal-by-reference class proxies
                var md = mouse_drag_delta;
                var c = v.cam;

                return
                new PointF_ValueTuple(
                new PointF((float)(a.x * c.g + Parent.Width / 2 + md.X),
                           (float)(a.y * c.g + Parent.Height / 2 + md.Y)),
                new PointF((float)(b.x * c.g + Parent.Width / 2 + md.X),
                           (float)(b.y * c.g + Parent.Height / 2 + md.Y))
                );
            }
            
            /// <summary>
            /// Plots the line segment onto its parent control.
            /// </summary>
            /// <param name="e"></param>
            /// <param name="E"></param>
            public void plot(Engine E, PaintEventArgs e)
            {
                var A = ScreenPos(E).Item1;
                var B = ScreenPos(E).Item2;

                if (rend)
                {
                    if (Draw == null)
                        e.Graphics.DrawLine(new Pen(new SolidBrush(col)), A, B);
                    else
                        Draw(e.Graphics, A, B);
                }
            }
            /// <summary>
            /// Plots the line segment onto its parent control.
            /// </summary>
            /// <param name="e"></param>
            /// <param name="E"></param>
            /// <param name="mouse_drag_delta"></param>
            public void plot(Point mouse_drag_delta, Engine E, PaintEventArgs e)
            {
                var A = ScreenPos(E, mouse_drag_delta).Item1;
                var B = ScreenPos(E, mouse_drag_delta).Item2;

                if (rend)
                {
                    if (Draw == null)
                        e.Graphics.DrawLine(new Pen(new SolidBrush(col)), A, B);
                    else
                        Draw(e.Graphics, A, B);
                }
            }
            public void plot(Point mouse_drag_delta, Engine E, PaintEventArgs e, out float X, out float Y)
            {
                var A = ScreenPos(E, mouse_drag_delta).Item1;
                var B = ScreenPos(E, mouse_drag_delta).Item2;

                X = B.X;
                Y = B.Y;

                if (rend)
                {
                    if (Draw == null)
                        e.Graphics.DrawLine(new Pen(new SolidBrush(col)), A, B);
                    else
                        Draw(e.Graphics, A, B);
                }
            }
        }
        [Serializable]
        protected class reg : figura
        {
            public List<pontus> ponti;
            public Color col { get; set; }
            public bool sel { get; set; }
            public bool rend { get; set; }
            public bool stat { get; set; }
            public int? id { get; set; }
            public Control Parent { get; set; }

            //indivens
            public reg(IList<pontus> ps, Color colin, bool st, int? hoc)
            {
                ponti = new List<pontus>();
                foreach (pontus p in ps) { ponti.Add(p); }
                col = colin;
                stat = st;
                sel = false;
                rend = true;
                id = hoc;
            }
            public reg(IList<pontus> ps) { ponti = ps.ToList(); rend = false; }
            public reg pure()
            {
                var cout = new List<pontus>();
                for (int j = 0; j < ponti.Count; j++)
                {
                    //if(j > 0){Debug.Write("{" + cout[cout.Count - 1].elog() + "|" + ponti[j].elog() + "}");}
                    if (!cout.Exists(s => s.equals(ponti[j]))) { cout.Add(ponti[j]); }
                }
                return new reg(cout, col, stat, id);
            }
            public List<lin> lins()
            {
                var cout = new List<lin>();

                //If polygon is triangle
                cout.Add(new lin(ponti[0], ponti[1], col, Parent, stat, id));
                cout.Add(new lin(ponti[1], ponti[2], col, Parent, stat, id));
                cout.Add(new lin(ponti[2], ponti[0], col, Parent, stat, id));

                return cout;
            }
            public List<PointF> pointfs()
            {
                var cout = new List<PointF>();
                foreach (var p
 in ponti) { cout.Add(new PointF((float)p.rx, (float)p.ry)); }
                return cout;
            }
            public vector tmid()
            {
                var l1 = new via(new pontus(ponti[0].matadd(ponti[1]).nummul(0.5).pares), new pontus(ponti[2].pares));
                var l2 = new via(new pontus(ponti[2].matadd(ponti[1]).nummul(0.5).pares), new pontus(ponti[0].pares));

                return l1.intersect(l2);
            }
            /// <summary>The plane on which the polygon lies.</summary>
            public planus planus() { return new planus(ponti[0].vector, ponti[1].vector, ponti[2].vector); }
            public static bool pcontains(List<PointF> l, float x, float y, Engine v)
            {
                var bx = l.Max(p => p.X); var by = l.Max(p => p.Y);
                var sx = l.Min(p => p.X); var sy = l.Min(p => p.Y);

                var m = lin._2di(l[0], new PointF((l[1].X + l[2].X) / 2, (l[1].Y + l[2].Y) / 2),
                                 l[1], new PointF((l[0].X + l[2].X) / 2, (l[0].Y + l[2].Y) / 2)).Value;

                var Is = 0;

                if (!double.IsNaN(m.X) & !double.IsNaN(m.Y))
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        var x1 = l[i].X; var x2 = l[(i + 1) % l.Count].X;
                        var y1 = l[i].Y; var y2 = l[(i + 1) % l.Count].Y;

                        var t = ((x1 - x2) * (x1 - x2) * m.X + (y1 - y2) * (y1 - y2) * x2 + (x1 - x2) * (y1 - y2) * (m.Y - y2)) /
                                ((y1 - y2) * (y1 - y2) + (x1 - x2) * (x1 - x2));

                        if ((y1 - y2) * (t - x2) / (x1 - x2) + y2 > m.Y)
                        {
                            if (y < (y1 - y2) * (x - x2) / (x1 - x2) + y2)
                            {
                                Is++;
                            }
                        }
                        else if ((y1 - y2) * (t - x2) / (x1 - x2) + y2 < m.Y)
                        {
                            if (y > (y1 - y2) * (x - x2) / (x1 - x2) + y2)
                            {
                                Is++;
                            }
                        }
                        else
                        {
                            if ((x1 < x & x1 < m.X) | (m.X < x1 & x < x1))
                            {
                                Is++;
                            }
                        }
                    }
                }
                return Is == l.Count;
            }
            /// <summary>Returns whether polygon contains the point 'p' - only works if polygon is a triangle.</summary>
            public bool pcontains(pontus p)
            {
                var ac = new vector(ponti[0], ponti[2]);
                var ab = new vector(ponti[0], ponti[1]);

                var bc = new vector(ponti[1], ponti[2]);
                var ba = new vector(ponti[1], ponti[0]);

                var ap = new vector(ponti[0], p);
                var bp = new vector(ponti[1], p);

                return
                    planus().intersects(p) &&
                    ((Math.Acos(ap.numdot(ac) / (ap.length() * ac.length())) < Math.Acos(ab.numdot(ac) / (ab.length() * ac.length())) && ap.length() < ab.length()) ||
                    (Math.Acos(bp.numdot(bc) / (bp.length() * bc.length())) < Math.Acos(ba.numdot(bc) / (ba.length() * bc.length())) && bp.length() < ba.length()));
            }
            public bool lcontains(lin l)
            {
                double t;
                var p = via.Fromlin(l).intersect(planus(), out t);

                if (double.IsNaN(p.x) || double.IsInfinity(p.x) || 1 <= t || t <= 0) { return false; }

                var cout = true;
                var lines = lins();

                for (int i = 0; i < lines.Count; i++)
                {
                    var e = via.Fromlin(lines[i]);

                    for (int j = 0; j < ponti.Count; j++)
                    {
                        double tl;
                        var I = e.intersect(new via(ponti[j], p), out tl);

                        if (!(double.IsNaN(I.x) || double.IsInfinity(I.x)) && 1 > tl && tl > 0) { cout = false; };
                    }
                }

                return cout;
            }
            public bool lcontains(lin l, out pontus I)
            {
                double t;
                var p = via.Fromlin(l).intersect(planus(), out t);

                if (double.IsNaN(p.x) || double.IsInfinity(p.x) || 1 <= t || t <= 0) { I = null; return false; }

                var cout = true;
                var lines = lins();

                for (int i = 0; i < lines.Count; i++)
                {
                    var e = via.Fromlin(lines[i]);

                    for (int j = 0; j < ponti.Count; j++)
                    {
                        double tl;
                        var q = e.intersect(new via(ponti[j], p), out tl);

                        if (!(double.IsNaN(q.x) || double.IsInfinity(q.x)) && 1 > tl && tl > 0) { cout = false; };
                    }
                }

                I = via.Fromlin(l).pnt(t);
                return cout;
            }
            public reg sort(ref int safety_count)
            {
                int faults = 0;

                var arr = lins();
                for (int i = 0; i < arr.Count; i++)
                {
                    var n = i == 0 ? arr.Count - 1 : i - 1;

                    if (
                       ((arr[i == 0 ? arr.Count - 1 : i - 1].b.x != arr[i].a.x || arr[i == 0 ? arr.Count - 1 : i - 1].b.y != arr[i].a.y || arr[i == 0 ? arr.Count - 1 : i - 1].b.z != arr[i].a.z) &&
                        (arr[i == 0 ? arr.Count - 1 : i - 1].a.x != arr[i].b.x || arr[i == 0 ? arr.Count - 1 : i - 1].a.y != arr[i].b.y || arr[i == 0 ? arr.Count - 1 : i - 1].a.z != arr[i].b.z) &&
                        (arr[i == 0 ? arr.Count - 1 : i - 1].b.x != arr[i].b.x || arr[i == 0 ? arr.Count - 1 : i - 1].b.y != arr[i].b.y || arr[i == 0 ? arr.Count - 1 : i - 1].b.z != arr[i].b.z) &&
                        (arr[i == 0 ? arr.Count - 1 : i - 1].a.x != arr[i].a.x || arr[i == 0 ? arr.Count - 1 : i - 1].a.y != arr[i].a.y || arr[i == 0 ? arr.Count - 1 : i - 1].a.z != arr[i].b.z)))
                    {
                        faults++;

                        var t = new[] {arr[(i + 1) % arr.Count].a,
                                        arr[(i + 1) % arr.Count].b};

                        arr[(i + 1) % arr.Count].a = arr[i].a;
                        arr[(i + 1) % arr.Count].b = arr[i].b;
                        arr[i].a = t[0];
                        arr[i].b = t[1];
                    }
                }

                var cout = new List<pontus>();
                for (int i = 0; i < arr.Count; i++)
                {
                    if ((arr[i == 0 ? arr.Count - 1 : i - 1].a.pares[0] == arr[i].a.pares[0] && arr[i == 0 ? arr.Count - 1 : i - 1].a.pares[1] == arr[i].a.pares[1] && arr[i == 0 ? arr.Count - 1 : i - 1].a.pares[2] == arr[i].a.pares[2]) ||
                        (arr[i == 0 ? arr.Count - 1 : i - 1].b.pares[0] == arr[i].b.pares[0] && arr[i == 0 ? arr.Count - 1 : i - 1].b.pares[1] == arr[i].b.pares[1] && arr[i == 0 ? arr.Count - 1 : i - 1].b.pares[2] == arr[i].b.pares[2]))
                    {
                        var t = arr[i].b;
                        var u = arr[i].a;
                        arr[i].b = u;
                        arr[i].a = t;
                    }
                }
                for (int i = 0; i < arr.Count; i++) { cout.Add(arr[i].a); cout.Add(arr[i].b); }
                return new reg(cout, col, stat, id);

            }
            public reg clip(out int pout, ref int mem, ref int Is, double cn)
            {
                var ppointds = new reg(ponti, col, stat, id); //ut ipsi se cliperet regio ille
                pout = 0;
                var W_plane = cn - 0.1;

                for (int j = 0; j < ppointds.ponti.Count && Is < 2; j++)
                {
                    var l = new lin(ppointds.ponti[(j + mem) % ppointds.ponti.Count], ppointds.ponti[(j + mem + 1) % ppointds.ponti.Count], ppointds.col, ppointds.Parent, ppointds.stat, ppointds.id);
                    if (l.a.z < W_plane && l.b.z >= W_plane)
                    {
                        var px = l.a.x; var qx = l.b.x;
                        var py = l.a.y; var qy = l.b.y;
                        var pz = l.a.z; var qz = l.b.z;

                        ppointds.ponti.Insert((j + mem + 1) % ppointds.ponti.Count, new pontus(px + (W_plane + 0.1 - pz) * (px - qx) / (pz - qz), py + (W_plane + 0.1 - pz) * (py - qy) / (pz - qz), W_plane + 0.1));
                        mem = (j + mem + 2) % ppointds.ponti.Count;
                        Is++;
                        if (Is > 2) { break; }
                        return clip(ppointds, out pout, ref mem, ref Is, cn);
                    }
                    if (l.b.z < W_plane && l.a.z >= W_plane)
                    {
                        var px = l.a.x; var qx = l.b.x;
                        var py = l.a.y; var qy = l.b.y;
                        var pz = l.a.z; var qz = l.b.z;

                        ppointds.ponti.Insert((j + mem + 1) % ppointds.ponti.Count, new pontus(px + (W_plane + 0.1 - pz) * (px - qx) / (pz - qz), py + (W_plane + 0.1 - pz) * (py - qy) / (pz - qz), W_plane + 0.1));
                        mem = (j + mem + 2) % ppointds.ponti.Count;
                        Is++;
                        if (Is > 2) { break; }
                        return clip(ppointds, out pout, ref mem, ref Is, cn);
                    }
                    if (l.b.z < W_plane && l.a.z < W_plane)
                    {
                        pout++;
                    }
                }
                var kb = new List<pontus>();
                for (int j = 0; j < ppointds.ponti.Count; j++) { if (ppointds.ponti[j].z < W_plane && pout != ppointds.ponti.Count) { kb.Add(ppointds.ponti[j]); } }
                for (int j = 0; j < kb.Count; j++) { ppointds.ponti.Remove(kb[j]); }
                return ppointds;
            }
            reg clip(reg ppointds, out int pout, ref int mem, ref int Is, double cn) //secundus vicis cliptionis ubi semicliptos (primus punctus intersecans a quibus invenitus est) pleneclipuntur
            {
                pout = 0;
                var W_plane = cn - 0.1;
                for (int j = 0; j < ppointds.ponti.Count && Is < 2; j++)
                {
                    var l = new lin(ppointds.ponti[(j + mem) % ppointds.ponti.Count], ppointds.ponti[(j + mem + 1) % ppointds.ponti.Count], ppointds.col, ppointds.Parent, ppointds.stat, ppointds.id);
                    if (l.a.z < W_plane && l.b.z >= W_plane)
                    {
                        var px = l.a.x; var qx = l.b.x;
                        var py = l.a.y; var qy = l.b.y;
                        var pz = l.a.z; var qz = l.b.z;

                        //insereret in indecem puncti sequentis decussationem lineae W-planique
                        ppointds.ponti.Insert((j + mem + 1) % ppointds.ponti.Count, new pontus(px + (W_plane + 0.1 - pz) * (px - qx) / (pz - qz), py + (W_plane + 0.1 - pz) * (py - qy) / (pz - qz), W_plane + 0.1));
                        mem = (j + mem + 2) % ppointds.ponti.Count; //mem index illius puncti sequentis est ut incrementum elementius qui sunt in ordone dispungeret
                        Is++; //quod alius punctus intersecans invenitus est
                        if (Is > 2) { break; } //si est unum linea intersecans et in regione est, est linea alia regionis qui intersecat et unum solus
                        return clip(ppointds, out pout, ref mem, ref Is, cn); //clip iterum ut illam lineam (punctocum) aliam inveniret
                    }
                    if (l.b.z < W_plane && l.a.z >= W_plane) //pro alius ordinatus lineae intersecantis
                    {
                        var px = l.a.x; var qx = l.b.x;
                        var py = l.a.y; var qy = l.b.y;
                        var pz = l.a.z; var qz = l.b.z;

                        ppointds.ponti.Insert((j + mem + 1) % ppointds.ponti.Count, new pontus(px + (W_plane + 0.1 - pz) * (px - qx) / (pz - qz), py + (W_plane + 0.1 - pz) * (py - qy) / (pz - qz), W_plane + 0.1));
                        mem = (j + mem + 2) % ppointds.ponti.Count;
                        Is++;
                        if (Is > 2) { break; }
                        return clip(ppointds, out pout, ref mem, ref Is, cn);
                    }
                    if (l.b.z < W_plane && l.a.z < W_plane)
                    {
                        pout++;//pout numerus linearum qui post planum sunt
                    }
                }
                var kb = new List<pontus>();
                for (int j = 0; j < ppointds.ponti.Count; j++) { if (ppointds.ponti[j].z < W_plane && pout != ppointds.ponti.Count) { kb.Add(ppointds.ponti[j]); } }
                for (int j = 0; j < kb.Count; j++) { ppointds.ponti.Remove(kb[j]); }
                return ppointds;
            }
            public reg rim()
            {
                var m = tmid();
                return new reg(ponti.ConvertAll(p => new via(new pontus(m.pares), p).pnt(0.99)), col, stat, id);
            }

            //figurae
            public string elog()
            {
                var แพท = "[reg|";
                for (int i = 0; i < ponti.Count; i++) { แพท += ponti[i].x + "'" + ponti[i].y + "'" + ponti[i].z + "|"; }
                แพท += col.A + "," + col.R + "," + col.G + "," + col.B + "]";
                return แพท;
            }

            public figura real() { return new reg(ponti.ConvertAll(delegate (pontus p) { return (pontus)p.real(); }), col, stat, id); }

            public figura rmat(matrix rotm, vector disv)
            {
                var นี่ = dcpy(this);
                if (!stat) { นี่.ponti = นี่.ponti.ConvertAll(delegate (pontus p) { return (pontus)p.matadd(disv).matmat(rotm); }); }

                นี่.sel = sel; return นี่;
            }
            public figura pmat(matrix prjm, double cn)
            {
                var pout = 0;
                var mem = 0;
                var Is = 0;
                var f = this;
                var c = clip(out pout, ref mem, ref Is, cn);
                c.rend = pout != c.ponti.Count; c.sel = sel;
                for (int j = 0; j < c.ponti.Count; j++)
                {
                    var r = (pontus)c.ponti[j].real();
                    c.ponti[j] = ((quaternion)new quaternion(1, c.ponti[j].pares).ijkw().matmat(prjm)).dehom();
                    c.ponti[j].rx = r.x; c.ponti[j].ry = r.y; c.ponti[j].rz = r.x;
                }

                return c;
            }
            public void plot(Engine v, PaintEventArgs e)
            {
                if (rend)
                {
                    //marshal-by-reference class proxies
                    var md = v.mouse_drag_delta;
                    var m = v.cam;

                    var c = ponti.ConvertAll(p => p.ScreenPos(v));

                    e.Graphics.FillPolygon(new SolidBrush(col), c.ToArray());

                    if (sel)
                    {
                        e.Graphics.FillPolygon(new SolidBrush(Color.FromArgb
                        (
                        col.A + 50 > 255 ? col.A - 50 : col.A + 50,
                        col.R + 50 > 255 ? col.R - 50 : col.R + 50,
                        col.G + 50 > 255 ? col.G - 50 : col.G + 50,
                        col.B + 50 > 255 ? col.B - 50 : col.B + 50)
                        ), c.ToArray());
                    }
                }
            }
            public void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e)
            {
                if (rend)
                {
                    //marshal-by-reference class proxies
                    var md = mouse_drag_delta;
                    var m = v.cam;

                    var c = ponti.ConvertAll(p => p.ScreenPos(v, md));

                    e.Graphics.FillPolygon(new SolidBrush(col), c.ToArray());

                    if (sel)
                    {
                        e.Graphics.FillPolygon(new SolidBrush(Color.FromArgb
                        (
                        col.A + 50 > 255 ? col.A - 50 : col.A + 50,
                        col.R + 50 > 255 ? col.R - 50 : col.R + 50,
                        col.G + 50 > 255 ? col.G - 50 : col.G + 50,
                        col.B + 50 > 255 ? col.B - 50 : col.B + 50)
                        ), c.ToArray());
                    }
                }
            }
            public void plot(Point mouse_drag_delta, Engine v, PaintEventArgs e, out float X, out float Y)
            {
                //marshal-by-reference class proxies
                var md = mouse_drag_delta;
                var m = v.cam;

                var c = ponti.ConvertAll(p => p.ScreenPos(v, md));

                X = c[0].X;
                Y = c[0].Y;

                if (rend)
                {
                    e.Graphics.FillPolygon(new SolidBrush(col), c.ToArray());

                    if (sel)
                    {
                        e.Graphics.FillPolygon(new SolidBrush(Color.FromArgb
                        (
                        col.A + 50 > 255 ? col.A - 50 : col.A + 50,
                        col.R + 50 > 255 ? col.R - 50 : col.R + 50,
                        col.G + 50 > 255 ? col.G - 50 : col.G + 50,
                        col.B + 50 > 255 ? col.B - 50 : col.B + 50)
                        ), c.ToArray());
                    }
                }
            }
        }
        #endregion

        #region species
        //---------------------------------------------------------------------------------------------------------species
        [Serializable]
        protected struct via : species
        {
            public vector v;
            public vector c;

            public via(vector _v, double _c) { v = _v; c = new vector(_c, _c, _c); }
            public via(vector _v, vector _c) { v = _v; c = _c; }
            /// <summary>New via travelling from p to q</summary>
            public via(pontus p, pontus q) { v = ((pontus)q.matadd(p.vector.inv())).vector; c = new vector(p.x, p.y, p.z); }

            public static via Fromlin(lin l) { var cout = new via(l.a, l.b); return cout; }

            public pontus pnt(double t) { return new pontus(v.nummul(t).matadd(c).pares); }
            /// <summary>
            /// Finds the intersection of this via with another via or line.
            /// If there is none, a vector with 'NaN' as its components is returned.
            /// </summary>
            public vector intersect(via u)
            {
                var t = ((vector)v.matvec(u.c.matadd(c.inv()))).length() / ((vector)v.matvec(u.v)).length();

                var t1 = ((vector)v.matvec(u.c.matadd(c.inv()))).length();
                var t3 = ((vector)v.matvec(u.c.matadd(c.inv())));
                var t2 = ((vector)v.matvec(u.v)).length();
                var t4 = ((vector)v.matvec(u.v));

                return double.IsNaN(t) || double.IsInfinity(t) ?
                    new vector(double.NaN, double.NaN, double.NaN) :
                    new vector(u.pnt(t).pares);
            }
            public vector intersect(via u, out double t)
            {
                t = ((vector)v.matvec(u.c.matadd(c.inv()))).length() / ((vector)v.matvec(u.v)).length();

                return double.IsNaN(t) || double.IsInfinity(t) ?
                    new vector(double.NaN, double.NaN, double.NaN) :
                    new vector(u.pnt(t).pares);
            }
            /// <summary>
            /// Finds the intersection of this via with a point.
            /// If there is none, a point with 'NaN' as its components is returned.
            /// </summary>
            public pontus intersect(pontus p)
            {
                var ts = p.matadd(c.inv()).matdot(v.rpl()).pares;
                var t = double.NaN;
                for (int i = 0; i < 3; i++) { if (!double.IsNaN(ts[i])) { t = ts[i]; } }
                var I = (vector)v.nummul(t).matadd(c);
                var cout = new pontus(I.pares);
                cout.rend = !Double.IsNaN(t);
                return cout;
            }
            public pontus intersect(pontus p, out double t)
            {
                var ts = p.matadd(c.inv()).matdot(v.rpl()).pares;
                t = double.NaN;
                for (int i = 0; i < 3; i++) { if (!double.IsNaN(ts[i])) { t = ts[i]; } }
                var I = (vector)v.nummul(t).matadd(c);
                var cout = new pontus(I.pares);
                cout.rend = !Double.IsNaN(t);
                return cout;
            }
            /// <summary>
            /// Finds the intersection of this via with another via or line.
            /// If there is none, a vector with 'NaN' as its components is returned.
            /// </summary>
            public pontus intersect(lin l) { var u = new via(l.a, l.b); return new pontus(intersect(u).pares); }
            public pontus intersect(lin l, out double t) { var u = new via(l.a, l.b); return new pontus(intersect(u, out t).pares); }
            /// <summary>
            /// Finds the intersection of this via with a plane.
            /// If there is none, a vector with 'NaN' as its components is returned.
            /// </summary>
            public pontus intersect(planus p)
            {
                var t = (p.w - c.numdot(p.norm())) / v.numdot(p.norm());
                var I = (vector)v.nummul(t).matadd(c);
                var cout = new pontus(I.pares);
                cout.rend = !Double.IsNaN(t);
                return cout;
            }
            public pontus intersect(planus p, out double t)
            {
                t = (p.w - c.numdot(p.norm())) / v.numdot(p.norm());
                var I = (vector)v.nummul(t).matadd(c);
                var cout = new pontus(I.pares);
                cout.rend = !Double.IsNaN(t);
                return cout;
            }
            /// <summary>
            /// Finds the intersection of this via with any figura except a form.
            /// If it recieves a form as its input, then a point with 'NaN' as its components is returned.
            /// If there is none, a point with 'NaN' as its components is returned.
            /// </summary>
            public pontus intersect(figura f)
            {
                var cout = new pontus(double.NaN, double.NaN, double.NaN);
                if (f is pontus) { cout = intersect((pontus)f); }
                if (f is lin) { cout = intersect((lin)f); }
                if (f is reg) { cout = intersect(new planus(((reg)f).ponti[0].vector, ((reg)f).ponti[1].vector, ((reg)f).ponti[2].vector)); }
                return cout;
            }
            public pontus intersect(figura f, out double t)
            {
                var cout = new pontus(double.NaN, double.NaN, double.NaN);
                t = double.NaN;
                if (f is pontus) { cout = intersect((pontus)f, out t); }
                if (f is lin) { cout = intersect((lin)f, out t); }
                if (f is reg) { cout = intersect((reg)f, out t); }
                return cout;
            }
            //species
            public string elog() { return "[via'" + v.x + "'" + v.y + "'" + v.z + "'" + c + "]"; }
        }
        [Serializable]
        protected struct matrix : species
        {
            public List<List<double>> array;
            /// <summary>number of columns</summary>
            public int xl { get => (int)sort.qsort(array, a => a.Count)[0].Item2; }
            /// <summary>number of rows</summary>
            public int yl { get => array.Count; }

            //indivens
            public matrix(IList<IList<double>> m)
            {
                array = new List<List<double>>();
                for (int i = 0; i < m.Count; i++)
                { array.Add(new List<double>()); for (int j = 0; j < m[i].Count; j++) { array[i].Add(m[i][j]); } }
            }
            public matrix(IEnumerable<IEnumerable<double>> m)
            {
                array = new List<List<double>>();
                for (int i = 0; i < m.Count(); i++)
                { array.Add(new List<double>()); for (int j = 0; j < m.ElementAt(i).Count(); j++) { array[i].Add(m.ElementAt(i).ElementAt(j)); } }
            }
            public matrix(double[,] m)
            {
                array = new List<List<double>>();
                for (int i = 0; i < m.GetLength(0); i++)
                { array.Add(new List<double>()); for (int j = 0; j < m.GetLength(1); j++) array[i].Add(m[i, j]); }
            }

            ///<summary>returns a new empty matrix. 'new matrix()' will return a matrix with a null array.</summary>
            public static matrix empty() => new matrix(new List<List<double>>());

            public double[,] todarray()
            {
                var cout = new double[yl, xl];
                for (int i = 0; i < yl; i++)
                    for (int j = 0; j < xl; j++)
                        cout[i, j] = array[i][j];

                return cout;
            }

            /// <summary>
            /// fills out all the zeros
            /// </summary>
            /// <returns>a list of the rows (each row is a list aswell)</returns>
            public List<List<double>> rows()
            {
                var cout = new List<List<double>>();
                for (int r = 0; r < yl; r++)
                {
                    cout.Add(new List<double>());
                    for (int c = 0; c < array[r].Count; c++)
                        cout[r].Add(array[r][c]);
                    for (int c = array[r].Count; c < xl; c++)
                        cout[r].Add(0);
                }

                return cout;
            }
            /// <summary>
            /// fills out all the zeros and flips the matrix sideways
            /// </summary>
            /// <returns>a list of the columns (each column is a list aswell)</returns>
            public List<List<double>> columns()
            {
                var cout = new List<List<double>>();
                for (int c = 0; c < xl; c++)
                {
                    cout.Add(new List<double>());
                    for (int r = 0; r < yl; r++)
                    {
                        cout[cout.Count - 1].Add(c >= array[r].Count ? 0 : array[r][c]);
                    }
                }
                return cout;
            }

            public static matrix I(int size, int mag)
            {
                return psy(size, size, null, (i, j, s) => i == j ? mag : 0);
            }

            public static matrix xrotm(double theta)
            {
                return new matrix(new[,]
                {
                    {1, 0, 0} ,
                    {0, Math.Cos(theta), -Math.Sin(theta)} ,
                    {0, Math.Sin(theta), Math.Cos(theta)}
                });
            }
            public static matrix yrotm(double theta)
            {
                return new matrix(new[,]
                {
                    {Math.Cos(theta), 0, Math.Sin(theta)} ,
                    {0, 1, 0} ,
                    {-Math.Sin(theta), 0, Math.Cos(theta)}
                });
            }
            public static matrix zrotm(double theta)
            {
                return new matrix(new[,]
                {
                    {Math.Cos(theta), -Math.Sin(theta), 0} ,
                    {Math.Sin(theta), Math.Cos(theta), 0} ,
                    {0, 0, 1}
                });
            }
            public static matrix rotm(double thx, double thy, double thz)
            {
                return xrotm(thx).matm(yrotm(thy)).matm(zrotm(thz));
            }

            public matrix matadd(matrix m)
            {
                if (m.xl == xl && m.yl == yl)
                { return psy(yl, xl, new dynamic[] { this, m }, (i, j, s) => s[0].array[i - 1][j - 1] + s[1].array[i - 1][j - 1]); }
                else { return psy(yl, xl, new dynamic[] { this, m }, (i, j, s) => double.NaN); }
            }
            public matrix nummul(double cin)
            {
                var cout = array;
                for (var r = 0; r < yl; r++) { cout[r] = cout[r].ConvertAll(c => c * cin); }
                return new matrix(cout);
            }

            public matrix matm(matrix _b)
            {
                var a = this.rows();
                var b = _b.columns();
                var c = new List<List<double>>();

                if (b.Count == a.Count)
                {
                    for (int y = 0; y < this.yl; y++)
                    {
                        c.Add(new List<double>());
                        for (int x = 0; x < _b.xl; x++)
                        {
                            c[y].Add(new vector(b[x]).numdot(new vector(a[y])));
                        }
                    }
                }
                else { throw new Exception("Number of columns in first matrix should be equal to number of rows in second."); }
                return new matrix(c);
            }
            public matrix matmat(matrix _b)
            {
                var a = this.rows();
                var b = _b.columns();
                var c = new List<List<double>>();

                if (_b.array.Count == a.Count)
                {
                    for (int y = 0; y < this.yl; y++)
                    {
                        c.Add(new List<double>());
                        for (int x = 0; x < _b.xl; x++)
                        {
                            c[y].Add(new vector(b[x]).numdot(new vector(a[y])));
                        }
                    }
                }
                else { throw new Exception("Number of columns in first matrix should be equal to number of rows in second."); }
                return new matrix(c);
            }
            /// <summary>multiplies every vaslue in the matrix by -1 </summary>
            public matrix neg() { return nummul(-1); }
            public double det()
            {
                var cout = 0.0;

                if (xl == 2 && yl == 2)
                {
                    cout += array[0][0] * array[1][1] - array[0][1] * array[1][0];
                }
                else
                {
                    for (int c = 0; c < xl; c++)
                    {
                        var str = 1.0;
                        for (int r = 0; r < yl; r++)
                        {
                            str *= array[r][(c + r) % xl];
                        }
                        cout += str;
                    }
                }

                return cout;
            }

            public double minor(int row, int column)
            {
                var cout = dcpy(array);

                foreach (var r in cout) { r.RemoveAt(column - 1); }
                cout.RemoveAt(row - 1);

                return new matrix(cout).det();
            }
            /// <summary>matrix of minors</summary>
            public matrix matmin()
            {
                return psy(yl, xl, new dynamic[] { this }, (r, c, s) => s[0].minor(r, c));
            }
            public matrix matcofact()
            {
                var cout = dcpy(array);
                var phi = 1;
                for (int c = 0; c < xl; c++)
                {
                    for (int r = 0; r < yl; r++)
                    {
                        cout[r][c] *= phi;
                        phi *= -1;
                    }
                }
                return new matrix(cout);
            }
            public matrix transpose()
            {
                var cout = dcpy(array);
                for (int d = 0; d + 1 < xl; d++)
                {
                    for (int c = 1; d + c < xl; c++)
                    {
                        cout[d + c][d] = array[d][d + c];
                    }
                }
                return new matrix(cout);
            }
            public matrix inv() //matrix inversion method not working (still)
            {
                var cout = dcpy(this);
                cout.array = new List<List<double>>();
                var i = 0; array.ForEach(r => { var t = new double[r.Count]; r.CopyTo(t); cout.array.Add(t.ToList()); i++; });
                var t1 = matmin();
                var t2 = t1.matcofact();
                var t3 = t2.transpose();
                var t4 = det();
                var t5 = t3.nummul(1 / t4);
                return cout.matmin().matcofact().transpose().nummul(1 / det());
            }

            /// <summary>A matrix constructed from the output of 'func' and 'cin' as an array containing that function's parameters</summary>
            public static matrix psy(int rows, int columns, dynamic[] cin, Func<int, int, dynamic[], double> func)
            {
                var vals = new List<List<double>>();

                for (int r = 1; r <= rows; r++)
                {
                    vals.Add(new List<double>());
                    for (int c = 1; c <= columns; c++)
                    {
                        vals[vals.Count - 1].Add(func(r, c, cin));
                    }
                }

                return new matrix(vals);
            }

            //species
            public string elog()
            {
                var cout = "[mtx|";
                for (int i = 0; i < xl; i++)
                {
                    for (int j = 0; j < yl; j++)
                    {
                        cout += array[i][j] + "'";
                    }
                    cout = cout.Substring(0, cout.Length - 1) + "|";
                }
                return cout.Substring(0, cout.Length - 1) + "]";
            }
        }
        #endregion

        #region physicae
        //-------------------------------------------------------------------------------------------------------------physicae
        [Serializable]
        protected struct material
        {
            public double density;
            public double cor;
            public double friction;

            public material(double r, double e, double f) { density = r; cor = e; friction = f; }
        }
        #endregion

        #region machinae
        //-------------------------------------------------------------------------------------------------------------machinae
        [Serializable]
        protected abstract class effector
        {
            public double min;
            public double max;
            public vector dir;
            public double stat;
            public int port;

            public List<form> formae;

            public effector() { }

            public virtual void open(double mag) { }
            public virtual void close(double mag) { }
            public virtual void visita() { }
            public string slog()
            {
                return "[" + port + " " + stat + "]";
            }
        }
        protected class motor : effector
        {
            quaternion rotor; //fake quaternion 

            public motor(form vir, form fem, vector axis)
            {
                formae.Add(vir);
                formae.Add(fem);

                vir.part = this;
                fem.part = this;

                max = 0;
                min = 0;
                stat = 0;
                dir = axis.unit();

                var nv = fem.rot.matvec(axis.unit());
                var theta = arccos(fem.rot.numdot(axis.unit()));
                rotor = new quaternion(theta, nv.pares);
            }

            public override void open(double mag)
            {
                formae[0].tau = (vector)formae[0].tau.matadd(dir.nummul(mag));
            }
            public override void close(double mag)
            {
                formae[0].tau = (vector)formae[0].tau.matadd(dir.nummul(-mag));
            }
            public override void visita()
            {
                dir = vector.quatrot(rotor.tovect(), formae[1].rot, rotor.w);
            }
        }
        protected class distensor : effector
        {
            quaternion rotor; //fake quaternion

            public distensor(form vir, form fem, vector axis, double min, double max)
            {
                formae.Add(vir);
                formae.Add(fem);

                vir.part = this;
                fem.part = this;

                this.max = max;
                this.min = min;
                stat = min;
                this.dir = axis.unit();

                var nv = fem.rot.matvec(axis.unit());
                var theta = arccos(fem.rot.numdot(axis.unit()));
                rotor = new quaternion(theta, nv.pares);
            }

            public override void open(double mag)
            {
                formae[0].F = (vector)formae[0].F.matadd(dir.nummul(mag));
            }
            public override void close(double mag)
            {
                formae[0].F = (vector)formae[0].F.matadd(dir.nummul(-mag));
            }
            public override void visita()
            {
                if (stat > max) { stat = max; }
                if (stat < min) { stat = min; }
                dir = vector.quatrot(rotor.tovect(), formae[1].rot, rotor.w);
            }
        }
        #endregion

        #region mathematicae
        //---------------------------------------------------------------------------------------------------------mathematicae
        protected static class producti
        {
            /// <summary>adds together the output from every iteration of the function 'func' from the current value of 'counter' to 'finish'.
            /// The function must take the counter as its first parameter, then all other parameters are to be cheld in the dynamic array parameter</summary>
            public static double sum(ref int counter, int finish, dynamic[] cin, Func<int, dynamic[], double> func)
            {
                var cout = 0.0;
                for (; counter <= finish; counter++)
                {
                    cout += func(counter, cin);
                }
                return cout;
            }
            public static double sum(int start, int finish, dynamic[] cin, Func<int, dynamic[], double> func)
            {
                var cout = 0.0;
                var counter = start;
                for (; counter <= finish; counter++)
                {
                    cout += func(counter, cin);
                }
                return cout;
            }
            /// <summary>adds together the output from every iteration of the function 'func' from the current value of 'counter' to 'finish'.
            /// All parameters of the function are to be cheld in the dynamic array parameter, the first element of which is the counter as an integer.</summary>
            public static double sum(ref dynamic counter, int finish, dynamic[] cin, Func<dynamic[], double> func)
            {
                var cout = 0.0;
                for (; counter <= finish; counter++)
                {
                    cin[0] = counter;
                    cout += func(cin);
                }
                return cout;
            }
            /// <summary>adds together the output from every iteration of the function 'func' from the current value of 'counter' to 'finish'.
            /// The function must take the counter as its first parameter, then all other parameters are to be cheld in the dynamic array parameter</summary>
            public static double pi(ref int counter, int finish, dynamic[] cin, Func<int, dynamic[], double> func)
            {
                var cout = 0.0;
                for (; counter <= finish; counter++)
                {
                    cout *= func(counter, cin);
                }
                return cout;
            }
            /// <summary>multiplies together the output from every iteration of the function 'func' from the current value of 'counter' to 'finish'.
            /// All parameters of the function are to be cheld in the dynamic array parameter, the first element of which is the counter as an integer.</summary>
            public static double pi(ref dynamic counter, int finish, dynamic[] cin, Func<dynamic[], double> func)
            {
                var cout = 0.0;
                for (; counter <= finish; counter++)
                {
                    cin[0] = counter;
                    cout *= func(cin);
                }
                return cout;
            }
            /// <summary>adds together the output from every iteration of the function 'func' from the current value of 'counter' to 'finish'.
            /// The function must take the counter as its first parameter, then all other parameters are to be cheld in the dynamic array parameter</summary>
            public static mathematica matprd<T>(ref int counter, int finish, dynamic[] cin, Func<int, dynamic[], T> func, Func<mathematica, T, mathematica> finc)
            {
                mathematica cout = null;
                for (; counter <= finish; counter++)
                {
                    cout = finc(cout, func(counter, cin));
                }
                return cout;
            }
            /// <summary>adds together the output from every iteration of the function 'func' from the current value of 'counter' to 'finish'.
            /// All parameters of the function are to be cheld in the dynamic array parameter, the first element of which is the counter as an integer.</summary>
            public static mathematica matprd<T>(ref dynamic counter, int finish, dynamic[] cin, Func<dynamic[], T> func, Func<mathematica, T, mathematica> finc)
            {
                mathematica cout = null;
                for (; counter <= finish; counter++)
                {
                    cin[0] = counter;
                    cout = finc(cout, func(cin));
                }
                return cout;
            }
            /// <summary>
            /// Apply an aggregate function 'finc' across the array 'arr', where the output type is different from the elements' types.
            /// </summary>
            /// <typeparam name="T">The type parameter of 'arr'</typeparam>
            /// <param name="arr">The List<> that you want the product of</param>
            /// <param name="finc">The query aggregate function 'T Func(T product, T factor);'.</param>
            /// <returns>Accumulated output of query aggregate function across whole of 'arr'.</returns>
            public static O arrprd<I, O>(List<I> arr, Func<I, I, I> finc, Func<I, O> conv)
            {
                try
                {
                    I cout = arr[0];
                    for (int i = 1; i < arr.Count; i++)
                    {
                        cout = finc(cout, arr[i]);
                    }
                    return conv(cout);
                }
                catch (Exception) { return default(O); }
            }
            /// <summary>
            /// Apply an aggregate function 'finc' across the array 'arr', where the output type is different from the elements' types.
            /// </summary>
            /// <typeparam name="T">The type parameter of 'arr'</typeparam>
            /// <param name="arr">The List<> that you want the product of</param>
            /// <param name="finc">The query aggregate function 'T Func(T product, T factor);'.</param>
            /// <returns>Accumulated output of query aggregate function across whole of 'arr'.</returns>
            public static O arrprd<I, O>(List<I> arr, Func<O, I, O> finc, char mode = 'n') where I : O
            {
                O cout;
                switch (mode)
                {
                    case 'c':
                        try { cout = arr[0]; }
                        catch (Exception) { cout = default(I); }
                        for (int i = 1; i < arr.Count; i++)
                             cout = finc(cout, arr[i]);
                        break;
                    default:
                        cout = default(O);
                        for (int i = 0; i < arr.Count; i++)
                            cout = finc(cout, arr[i]);
                        break;
                }
                return cout;
            }
            public static I arrprd<I>(List<I> arr, Func<I, I, I> finc)
            {
                try
                {
                    I cout = arr[0];
                    for (int i = 1; i <= arr.Count; i++)
                    {
                        cout = finc(cout, arr[i - 1]);
                    }
                    return cout;
                }
                catch (Exception) { return default(I); }
            }
            /// <summary>
            /// Apply an aggregate function 'finc' across the array 'arr', where the output type is different from the elements' types.
            /// </summary>
            /// <typeparam name="T">The type parameter of 'arr'</typeparam>
            /// <param name="arr">The List<> that you want the product of</param>
            /// <param name="finc">The query aggregate function 'T Func(T product, T factor);'.</param>
            /// <param name="zero"> The 'zeroth' element of the sequence that will be aggregated.</param>
            /// <returns>Accumulated output of query aggregate function across whole of 'arr'.</returns>
            public static O arrprd<I, O>(List<I> arr, Func<O, I, O> finc, O zero)
            {
                O cout = zero;
                try
                {
                    for (int i = 0; i < arr.Count; i++)
                        cout = finc(cout, arr[i]);
                }
                catch (NullReferenceException) { }
                return cout;
            }
            /// <summary>
            /// Apply an aggregate function 'finc' across the array 'arr'.
            /// </summary>
            /// <typeparam name="T">The type parameter of 'arr'</typeparam>
            /// <param name="arr">The List<> that you want the product of</param>
            /// <param name="finc">The query aggregate function 'T Func(T product, T factor);'.</param>
            /// <returns>Accumulated output of query aggregate function across whole of 'arr'.</returns>
            public static T arrprd<T>(List<T> arr, Func<T, T, dynamic> finc)
            {
                T cout = default(T);
                for (int i = 0; i < arr.Count; i++)
                {
                    cout = (T)finc(cout, arr[i]);
                }
                return cout;
            }
            /// <summary>
            /// Apply an aggregate function 'finc' across the array 'arr'.
            /// </summary>
            /// <typeparam name="T">The type parameter of 'arr'</typeparam>
            /// <param name="arr">The List<> that you want the product of</param>
            /// <param name="finc">The query aggregate function 'T Func(T product, T factor);'.</param>
            /// <param name="zero"> The 'zeroth' element of the sequence that will be aggregated.</param>
            /// <returns>Accumulated output of query aggregate function across whole of 'arr'.</returns>
            public static T arrprd<T>(List<T> arr, Func<T, T, dynamic> finc, T zero)
            {
                T cout = zero;
                for (int i = 0; i < arr.Count; i++)
                {
                    cout = (T)finc(cout, arr[i]);
                }
                return cout;
            }/*
            public static mathematica arrprd<T>(int start, int finish, List<T> arr, Func<mathematica, T, mathematica> finc)
            {
                mathematica cout = null;
                for (int i = start; i <= finish; i++)
                {
                    cout = finc(cout, arr[i]);
                }
                return cout;
            }
            public static mathematica arrprd<T>(int start, int finish, List<T> arr, dynamic[] cin, Func<mathematica, dynamic[], T, mathematica> finc)
            {
                mathematica cout = null;
                for (int i = start; i <= finish; i++)
                {
                    cout = finc(cout, cin, arr[i]);
                }
                return cout;
            }*/
        }
        [Serializable]
        protected struct vector : mathematica
        {
            public double x { get { return partes[0]; } set { partes[0] = value; } }
            public double y { get { return partes.Count > 1 ? partes[1] : 0; } set { if (partes.Count > 1) { partes[1] = value; } } }
            public double z { get { return partes.Count > 2 ? partes[2] : 0; } set { if (partes.Count > 2) { partes[2] = value; } } }

            public List<double> partes;

            public vector(params double[] parts) { partes = parts.ToList(); }
            public vector(pontus p, pontus q) { partes = p.matadd(q.vector.inv()).pares; }
            public vector(IList<double> a) { partes = a.ToList(); }

            /// <summary> Find length of vector </summary>
            public double length()
            {
                var test = this; var cout = Math.Sqrt(producti.arrprd(partes, (sum, part) => sum += part * part));
                return Math.Sqrt(producti.arrprd(partes, (sum, part) => sum += part * part));
            }
            /// <summary> Converts to unit vector </summary>
            public vector unit() { return (vector)nummul(1 / length()); }
            /// <summary> Multiplies length by 'mag' </summary>
            public vector vect(double mag) { return (vector)nummul(mag); }
            /// <summary> Convert to array of components </summary>
            public double[] ToArray() { return partes.ToArray(); }
            /// <summary> Convert to quaternion with _real value of 0 </summary>
            public quaternion toquat() { return new quaternion(0, x, y, z); }
            /// <summary> Multiply each component by -1 </summary>
            public vector inv() { return (vector)nummul(-1); }
            /// <summary> Reciprocal each component </summary>
            public vector rpl() { return new vector(partes.ConvertAll(p => 1 / p)); }
            /// <summary> Make components look like floats </summary>
            public vector f() { return new vector(partes.Cast<float>().Cast<double>().ToList()); }
            /// <summary>3D origin vector</summary>
            public static vector o { get { return new vector(0, 0, 0); } }
            /// <summary>
            /// Rotate around an axis pointing from the origin using a unit quaternion.
            /// </summary>
            /// <param name="axis">Axis around which vector is rotated</param>
            /// <param name="v">Vector to be rotated</param>
            /// <param name="theta">Angle by which vector is rotated in radians</param>
            /// <returns></returns>
            public static vector quatrot(vector axis, vector v, double theta)
            {
                var q = new quaternion(Math.Cos(theta / 2), axis.x * Math.Sin(theta / 2), axis.y * Math.Sin(theta / 2), axis.z * Math.Sin(theta / 2));
                var qrotm = new matrix(new double[,]
                {
                {1 - 2 * q.j * q.j - 2 * q.k * q.k, 2 * q.i * q.j - 2 * q.k * q.w, 2 * q.i * q.k + 2 * q.j * q.w},
                {2 * q.i * q.j + 2 * q.k * q.w, 1 - 2 * q.i * q.i - 2 * q.k * q.k, 2 * q.j * q.k - 2 * q.i * q.w},
                {2 * q.i * q.k - 2 * q.j * q.w, 2 * q.j * q.k + 2 * q.i * q.w, 1 - 2 * q.i * q.i - 2 * q.j * q.j},
                });

                var c = (vector)v.matmat(qrotm);
                if (double.IsNaN(c.x) || double.IsNaN(c.y) || double.IsNaN(c.z)) { c = v; }

                return c;
            }
            /// <summary>
            /// Rotate around an arbitrary axis in 3D space using a unit quaternion.
            /// </summary>
            /// <param name="axis">Axis around which vector is rotated</param>
            /// <param name="v">Vector to be rotated</param>
            /// <param name="theta">Angle by which vector is rotated in radians</param>
            /// <returns></returns>
            public static vector quatrot(via axis, vector v, double theta)
            {
                var q = new quaternion(Math.Cos(theta / 2), axis.v.x * Math.Sin(theta / 2), axis.v.y * Math.Sin(theta / 2), axis.v.z * Math.Sin(theta / 2));
                var qrotm = new matrix(new double[,]
                {
                {1 - 2 * q.j * q.j - 2 * q.k * q.k, 2 * q.i * q.j - 2 * q.k * q.w, 2 * q.i * q.k + 2 * q.j * q.w},
                {2 * q.i * q.j + 2 * q.k * q.w, 1 - 2 * q.i * q.i - 2 * q.k * q.k, 2 * q.j * q.k - 2 * q.i * q.w},
                {2 * q.i * q.k - 2 * q.j * q.w, 2 * q.j * q.k + 2 * q.i * q.w, 1 - 2 * q.i * q.i - 2 * q.j * q.j},
                });

                var c = (vector)v.matadd(axis.pnt(0).vector.inv()).matmat(qrotm).matadd(axis.pnt(0).vector);
                if (double.IsNaN(c.x) || double.IsNaN(c.y) || double.IsNaN(c.z)) { c = v; }

                return c;
            }
            /// <summary>
            /// Rotate around an arbitrary axis in 3D space using a unit quaternions.
            /// </summary>
            /// <param name="axis">Axis around which quaternion is rotated</param>
            /// <param name="v">Quaternion to be rotated</param>
            /// <param name="theta">Angle by which quaternion is rotated in radians</param>
            /// <returns></returns>
            public static quaternion quatrot(quaternion v, via axis, double theta)
            {
                var q = new quaternion(Math.Cos(theta / 2), axis.v.x * Math.Sin(theta / 2), axis.v.y * Math.Sin(theta / 2), axis.v.z * Math.Sin(theta / 2));

                var c = q.matquat(v).matquat(q.conj());
                if (double.IsNaN(c.i) || double.IsNaN(c.j) || double.IsNaN(c.k)) { c = v; }

                return c;
            }
            public static vector X { get { return new vector(1, 0, 0); } }
            public static vector Y { get { return new vector(0, 1, 0); } }
            public static vector Z { get { return new vector(0, 0, 1); } }

            /// <summary>transposed vector as a matrix</summary>
            public matrix transmat() { return new Engine.matrix(new List<List<double>>() { partes }); }
            /// <summary>vector as a matrix with one column</summary>
            public matrix mat() { return matrix.psy(pares.Count, 1, new dynamic[] { this }, (i, j, s) => j == 1 ? s[0].pares[i - 1] : 0); }

            //mathematicae
            public List<double> pares { get => partes; set => partes = value; }
            /// <summary> Multiply vector by coefficient 'b' </summary>
            public mathematica nummul(double b) { var cin = dcpy(partes); for (int i = 0; i < partes.Count; i++) { cin[i] *= b; } return new vector(cin); }
            /// <summary> Adds 'b' to each component of vector </summary>
            public mathematica numadd(double b) { var cin = dcpy(partes); for (int i = 0; i < partes.Count; i++) { cin[i] += b; } return new vector(cin); }
            /// <summary> Dot-Product vector with first 3 components of 'b' </summary>
            public double numdot(mathematica b)
            {
                var c = 0.0;
                for (int i = 0; i < b.pares.Count; i++)
                {
                    c += partes[i] * b.pares[i];
                }
                return c;
            }
            /// <summary> Multiply each component of vector by corresponding component of 'b' </summary>
            public mathematica matdot(mathematica b)
            {
                var c = new List<double>();
                for (int i = 0; i < partes.Count; i++)
                {
                    c.Add(partes[i] * b.pares[i]);
                }
                return new vector(c[0], c[1], c[2]);
            }
            /// <summary> Cross-Product with 'b' </summary>
            public mathematica matvec(mathematica b)
            {
                if (partes.Count != 3) { throw new Exception("Can't cross-product a vector that is not 3D."); }
                return new vector
                (
                pares[1] * b.pares[2] - pares[2] * b.pares[1],
                pares[2] * b.pares[0] - pares[0] * b.pares[2],
                pares[0] * b.pares[1] - pares[1] * b.pares[0]
                );
            }
            /// <summary> Translate vector by 'a' </summary>
            public mathematica matadd(mathematica a) { var cin = dcpy(partes); for (int i = 0; i < partes.Count; i++) { cin[i] += a.pares[i]; } return new vector(cin); }
            /// <summary> Transform vector by matrix 'm' </summary>
            public mathematica matmat(matrix m)
            {
                var c = new[] { 0.0, 0, 0 };

                for (int i = 0; i < partes.Count; i++)
                {
                    c[i] = numdot(new vector(m.columns()[i]));
                }
                return new vector(c);
            }
        }
        [Serializable]
        protected struct planus : mathematica
        {
            public double a;
            public double b;
            public double c;
            public double w;

            public planus(double _a, double _b, double _c, double _d) { a = _a; b = _b; c = _c; w = _d; }
            /// <summary>Takes the normal vector 'v' and any point on the plane 'P'.</summary>
            public planus(vector v, pontus P) { a = v.x; b = v.y; c = v.z; w = v.numdot(P); }
            public planus(vector a, vector b, vector c)
            {
                var v = (vector)a.matadd(b.inv()).matvec(a.matadd(c.inv()));
                this.a = v.x; this.b = v.y; this.c = v.z;
                w = v.numdot(c);
            }
            public planus(vector v) { a = v.x; b = v.y; c = v.z; w = 0; }

            public planus inv() { return new planus(-a, -b, -c, -w); }
            public vector p() { return new vector(0, 0, w / c); }
            public vector norm() { return new vector(a, b, c); }
            public bool intersects(pontus p) { return p.numdot(norm()) == w; }
            public pontus intersect(pontus P, vector v)
            {
                var t = norm().numdot(p()) / norm().numdot(v);
                var I = new via(v, new vector(P.pares)).pnt(t);

                if (I.numdot(norm()) != w) { I.rend = false; }
                return I;
            }
            public pontus intersect(via v)
            {
                var t = (w - v.c.numdot(norm())) / v.v.numdot(norm());
                var I = (vector)v.v.nummul(t).matadd(v.c);
                var cout = new pontus(I.pares);

                cout.rend = !Double.IsNaN(t);
                return cout;
            }
            public pontus intersect(via v, out double t)
            {
                t = (w - v.c.numdot(norm())) / v.v.numdot(norm());
                var I = (vector)v.v.nummul(t).matadd(v.c);
                var cout = new pontus(I.pares);

                cout.rend = !Double.IsNaN(t);
                return cout;
            }

            //mathematicae
            public List<double> pares { get => new List<double> { a, b, c, w }; }
            public mathematica nummul(double d) { return new planus(pares[0] * d, pares[1] * d, pares[2] * d, w); }
            public mathematica numadd(double d) { return new planus(pares[0] + d, pares[1] + d, pares[2] + d, w); }
            /// <summary>Returns dot product of normal vector with first 3 components of mathematica 'd'.</summary>
            public double numdot(mathematica d)
            {
                var c = 0.0;
                for (int i = 0; i < d.pares.Count; i++)
                {
                    c += pares[i] * d.pares[i];
                }
                return c;
            }
            public mathematica matdot(mathematica d)
            {
                var c = new List<double>();
                for (int i = 0; i < pares.Count; i++)
                {
                    c.Add(pares[i] * d.pares[i]);
                }
                return new vector(c[0], c[1], c[2]);
            }
            public mathematica matvec(mathematica d)
            {
                return new planus
                (
                pares[1] * d.pares[2] - pares[2] * d.pares[1],
                pares[2] * d.pares[0] - pares[0] * d.pares[2],
                pares[0] * d.pares[1] - pares[1] * d.pares[0],
                w
                );
            }
            public mathematica matadd(mathematica ain) { return new planus(a + ain.pares[0], b + ain.pares[1], c + ain.pares[2], ain is planus ? ain.pares[3] : 0); }
            public mathematica matmat(matrix m)
            {
                return this;
            }
        }
        protected struct quaternion : mathematica, species
        {
            public double w;
            public double i;
            public double j;
            public double k;

            public quaternion(double _w, double _i, double _j, double _k) { w = _w; i = _i; j = _j; k = _k; }
            public quaternion(double _w, IList<double> ijk) { w = _w; i = ijk[0]; j = ijk[1]; k = ijk[2]; }

            public double length() { return Math.Sqrt(w * w + i * i + j * j + k * k); }
            public quaternion unit() { return new quaternion(w / length(), i / length(), j / length(), k / length()); }
            public double[] ToArray() { return new[] { w, i, j, k }; }
            /// <summary> Returns the vector component (i, j, k) of the quaternion. </summary>
            public vector tovect() { return new vector(i, j, k); }
            public pontus dehom() { return new pontus(w / k, i / k, j / k); }
            public quaternion ijkw() { return new quaternion(i, j, k, w); }
            public quaternion wijk() => new quaternion(w, i, j, k);
            public quaternion conj() { return new quaternion(w, -i, -j, -k); }
            public quaternion inv() { return (quaternion)conj().nummul(1 / Math.Pow(length(), 2)); }

            //species
            public string elog() { return "[qtn'" + w + "'" + i + "'" + j + "'" + k + "]"; }

            //mathematicae
            public List<double> pares => new List<double> { w, i, j, k };
            public mathematica nummul(double b) { return new quaternion(pares[0] * b, pares[1] * b, pares[2] * b, pares[3] * b); }
            public mathematica numadd(double b) { return new quaternion(pares[0] + b, pares[1] + b, pares[2] + b, pares[3] + b); }
            public double numdot(mathematica b)
            {
                var c = 0.0;
                for (int i = 0; i < b.pares.Count; i++)
                {
                    c += pares[i] * b.pares[i];
                }
                return c;
            }
            public mathematica matdot(mathematica b)
            {
                var c = new List<double>();
                for (int i = 0; i < pares.Count; i++)
                {
                    c.Add(pares[i] * b.pares[i]);
                }
                return new quaternion(c[0], c[1], c[2], c[3]);
            }
            public mathematica matvec(mathematica b)
            {
                return new quaternion
                (
                0,
                pares[2] * b.pares[3] - pares[3] * b.pares[2],
                pares[3] * b.pares[2] - pares[2] * b.pares[3],
                pares[1] * b.pares[2] - pares[2] * b.pares[1]
                );
            }
            public quaternion matquat(quaternion that)
            {
                var vec = (vector)this.tovect().matvec(that.tovect()).matadd(that.tovect().nummul(this.w)).matadd(this.tovect().nummul(that.w));
                return new quaternion(this.tovect().numdot(that.tovect().inv()), vec.pares);
            }
            public mathematica matadd(mathematica a) { return new quaternion(w + a.pares[0], i + a.pares[1], j + a.pares[2], k + a.pares[3]); }
            public mathematica matmat(matrix m)
            {
                var c = new double[] { 0, 0, 0, 0 };
                var b = m.todarray();

                var c1 = new quaternion(
                    (w * b[0, 0] + i * b[0, 1] + j * b[0, 2] + k * b[0, 3]),
                    (w * b[1, 0] + i * b[1, 1] + j * b[1, 2] + k * b[1, 3]),
                    (w * b[2, 0] + i * b[2, 1] + j * b[2, 2] + k * b[2, 3]),
                    (w * b[3, 0] + i * b[3, 1] + j * b[3, 2] + k * b[3, 3])
                );

                for (int r = 0; r < b.GetLength(0); r++)
                {
                    for (int l = 0; l < b.GetLength(1); l++)
                    {
                        c[r] += pares[l] * b[r, l];
                    }
                }

                return new quaternion(c[0], c[1], c[2], c[3]);
            }
        }

        #endregion

        protected class portal
        {
            public NamedPipeClientStream dataex;
            public object key = new object();
            public byte[] data = new byte[4096];
            public string name;
            bool operans;
            public bool open { get { return _open; } set { operans = value; _open = value; } }
            bool _open;

            public portal(string name)
            {
                this.name = name.Substring(name.LastIndexOf('\\') + 1).Substring(0, name.Substring(name.LastIndexOf('\\') + 1).Length - 3);
                operans = false;
                dataex = new NamedPipeClientStream
                (
                    ".",
                    this.name + "c",
                    PipeDirection.Out,
                    PipeOptions.Asynchronous
                );
                operans = true;

                var alterth = new Thread(new ThreadStart(delegate
                {
                    while (!operans) { var _ = '\0'; }
                    Process.Start(name);
                }));
                var datainth = new Thread(new ThreadStart(delegate
                {
                    Debug.WriteLine("waiting for connection...");

                    var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    var rule = new PipeAccessRule(sid, PipeAccessRights.ReadWrite,
                                                  AccessControlType.Allow);
                    var sec = new PipeSecurity();
                    sec.AddAccessRule(rule);

                    using (NamedPipeServerStream datain = new NamedPipeServerStream
                          ("rcworld", PipeDirection.InOut, 100,
                           PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, sec))
                    {
                        lock (key)
                        {
                            operans = true;
                        }
                        datain.WaitForConnection();
                        Debug.Write("[Connection Established]");

                        while (!data.Contains<byte>(10))
                        {
                            var read = 0;
                            Debug.Write(".");
                            lock (key)
                            {
                                //Debug.Write(",");
                                Debug.WriteLine(datain.Read(data, 0, data.Length));
                                if (!operans) { Thread.CurrentThread.Abort(); }
                            }
                            Debug.Write("[" + (char)(data[0] + 0x30) + "]");
                            //foreach (var b in bytes) { Debug.Write("[" + (char)(b + 0x30) + "]"); }
                            read += data.Length;
                        }
                    }
                }));
                datainth.Start();
                alterth.Start();

                bool _bf = true;
                while (_bf)
                {
                    lock (key)
                    {
                        Debug.Write("'" + (char)(data[0] + 0x30));
                        _bf = data[0] != 1;
                    }
                    Thread.Sleep(1);
                }

                dataex.Connect();

                open = true;
            }

            public void referre(string stati)
            {
                if (operans)
                {
                    try { dataex.Write(stati.ToList().ConvertAll(s => (byte)s).ToArray(), 0, stati.Length); }
                    catch (IOException e) { Debug.Write("[Pipe broken at" + name + "]"); operans = false; }
                }
            }
            public void referre(byte[] stati)
            {
                if (operans)
                {
                    try { dataex.Write(stati, 0, stati.Length); }
                    catch (IOException e) { Debug.Write("[Pipe broken at" + name + "]"); operans = false; }
                }
            }
        }

        #endregion

        #region functioning methods

        public Engine()
        {
            this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer,
            true);
            this.SetStyle(ControlStyles.StandardDoubleClick, false);
            /*
            cam.g = 5;
            cam.nf = 1;
            cam.ff = 140;
            cam.fov = 1;
            cam.cn = 0.1;
            cam.rendd = 50;
            cam.rspeed = 2;
            cam.mspeed = 0.1;
            cam.xmr = Math.PI;
            
            projm = new matrix(new[,]
            {
                {1 / Math.Tan(cam.fov * Math.PI/ (360.0 * 1)), 0, 0, 0},
                {0, 1 / Math.Tan(cam.fov * Math.PI / (360.0 / 1)), 0, 0},
                {0, 0, (cam.ff + cam.nf) / (cam.ff - cam.nf), 2 * cam.ff * cam.nf / (cam.ff - cam.nf)},
                {0, 0, -1, 0}
            });*/
        }

        /// <summary>
        /// Deep copy
        /// </summary>
        /// <typeparam name="T">Type of object to be deep copied</typeparam>
        /// <param name="cin">the object being deep copied</param>
        /// <returns></returns>
        protected static T dcpy<T>(T cin)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, cin);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        protected static double sin(double a) { return Math.Sin(a); }
        protected static double cos(double a) { return Math.Cos(a); }
        protected static double tan(double a) { return Math.Tan(a); }
        protected static double arcsin(double a) { return Math.Asin(a); }
        protected static double arccos(double a) { return Math.Acos(a); }
        protected static double arctan(double a) { return Math.Atan(a); }

        /// <summary>
        /// Class containing a bunch of functions related to sorting.
        /// </summary>
        protected static class sort
        {
            /// <summary>
            /// Quicksort algorithm for lists/enumerables of specified type <paramref name="a"/>. 
            /// Sorts in whatever order the function <paramref name="comp"/> decides to discriminate by.
            /// If comp returns true, then the second parameter will have a greater index than the first parameter
            /// </summary>
            public static List<T> qsort<T>(List<T> a, Func<T, T, bool> comp)
            {
                var c = new List<T>();
                for (int i = 0; i < a.Count; i++) { c.Add(a[i]); }
                if (c.Count == 0) return null;
                qs(c, 0, c.Count - 1, comp);
                return c;
            }
            static void qs<T>(List<T> a, int l, int h, Func<T, T, bool> c)
            {
                if (l < h)
                {
                    var p = a[h];
                    var i = (l - 1);

                    for (int j = l; j <= h - 1; j++)
                    {
                        if (c(a[j], p))
                        {
                            i++;
                            var t = a[i];
                            a[i] = a[j];
                            a[j] = t;
                        }
                    }
                    var t1 = a[i + 1];
                    a[i + 1] = a[h];
                    a[h] = t1;

                    var pi = i + 1;

                    qs(a, l, pi - 1, c);
                    qs(a, pi + 1, h, c);
                }
            }
            /// <summary>
            /// Quicksort algorithm for lists/enumerables of specified type <paramref name="a"/>.
            /// Applies the function <paramref name="proc"/> across the whole list and sorts in 
            /// whatever order the function <paramref name="comp"/> decides to discriminate by.
            /// If comp returns true, then the second parameter will have a greater index than 
            /// the first parameter.
            /// </summary>
            /// <typeparam name="I">input datatype</typeparam>
            /// <typeparam name="T">processed datatype to be compared</typeparam>
            /// <param name="a">input list</param>
            /// <param name="proc">function to apply across whole list</param>
            /// <param name="comp"></param>
            /// <returns>A sorted version of the list which has also had the <paramref name="proc"/> function applied to it.</returns>
            public static List<Tuple<I, T>> qsort<I, T>(List<I> a, Func<I, T> proc, Func<T, T, bool> comp)
            {
                var c = new List<Tuple<I, T>>();
                for (int i = 0; i < a.Count; i++) { c.Add(new Tuple<I, T>(a[i], proc(a[i])));}
                if (c.Count == 0) return null;
                qs(c, 0, c.Count - 1, (A, B) => comp(A.Item2, B.Item2));
                return c;
            }
            /// <summary>
            /// Quicksort algorithm which sorts from highest to lowest, in order of respective output of <paramref name="proc"/>.
            /// Returns a list of Tuples containing the elememts and their respective outputs of <paramref name="proc"/>.
            /// </summary>
            /// <typeparam name="I">input</typeparam>
            /// <typeparam name="T">throughput</typeparam>
            /// <param name="a"></param>
            /// <param name="proc"></param>
            /// <returns></returns>
            public static List<Tuple<I, T>> qsort<I, T>(List<I> a, Func<I, T> proc) where T : IComparable<T>
            {
                var c = new List<Tuple<I, T>>();
                for (int i = 0; i < a.Count; i++) { c.Add(new Tuple<I, T>(a[i], proc(a[i]))); }
                if (c.Count == 0)
                    return new List<Tuple<I, T>>() { new Tuple<I, T>(default(I), default(T)) };
                qs(c, 0, c.Count - 1, (A, B) => A.Item2.CompareTo(B.Item2) > 0);
                return c;
            }

            public static bool relz(reg a, reg p)
            {
                var infront = false;

                var ร = new planus(a.ponti[0].vector, a.ponti[1].vector, a.ponti[2].vector); if (ร.w < 0) { ร = ร.inv(); }
                var ล = new planus(p.ponti[0].vector, p.ponti[1].vector, p.ponti[2].vector); if (ล.w < 0) { ล = ล.inv(); }

                for (int k = 0; k < 3; k++)
                {
                    infront |= (float)a.ponti[k].numdot(ล.norm()) < (float)ล.w || (float)p.ponti[k].numdot(ร.norm()) > (float)ร.w;
                }

                return infront;
            }
        }
        #endregion
    }
}
