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

using Gma.System.MouseKeyHook;
using Liber;

namespace NotGCalc
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
            
            public static double rotation_speed = 0.03;
            public static double panning_speed = 0.05;
            
            public static readonly ComponentResourceManager resources = new ComponentResourceManager(typeof(Window));
            public static Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
            public static Dictionary<string, Func<List<Formula>, object, Tensor>> functions = new Dictionary<string, Func<List<Formula>, object, Tensor>>();
            
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
                var n = eq.Item2[0].SolveFor((eq.Item2[0].parts[0] as IVariableContainer).Variables.Where(v => v != null && !v.param && v.name != null).First(), Globals.values) as Tensor;
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
                var n = eq.Item2[0].SolveFor((eq.Item2[0].parts[0] as IVariableContainer).Variables.Where(v => v != null && !v.param && v.name != null).First(), Globals.values) as Tensor;
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
        
        List<figura> GraphContents;
        double x_rotation, y_rotation, z_rotation = 0;
        vector camera_pan = vector.o;

        void InitialiseGraphPanel()
        {
            GraphContents = new List<figura>
            {
                new lin(pontus.o, new pontus(vector.X.partes), Color.Red, GraphPanel, false, null),
                new lin(pontus.o, new pontus(vector.Y.partes), Color.Green, GraphPanel, false, null),
                new lin(pontus.o, new pontus(vector.Z.partes), Color.Blue, GraphPanel, false, null)
            };

            cam.g = 100;

            ParamSldr_min = 0;
            ParamSldr_max = 10;

            on_slider = false;
            ParamSldr.Hide();
            ParamBtn.Hide();

            AddToParams("x");
            AddToParams("y");
            AddToParams("z");
            AddToParams("t");
        }

        private void GraphPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString("θ: " + Math.Round(x_rotation * 1000 * 180 / Math.PI) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 0));
            e.Graphics.DrawString("ϕ: " + Math.Round(y_rotation * 1000 * 180 / Math.PI) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 10));
            e.Graphics.DrawString("ψ: " + Math.Round(z_rotation * 1000 * 180 / Math.PI) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 20));

            e.Graphics.DrawString("x: " + Math.Round(camera_pan.x * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(65, 0));
            e.Graphics.DrawString("y: " + Math.Round(camera_pan.y * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(65, 10));
            e.Graphics.DrawString("z: " + Math.Round(camera_pan.z * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(65, 20));
            
            e.Graphics.DrawString("zoom: " + Math.Round(cam.g * 1000) / 1000.0, Globals.DataFont, new SolidBrush(SystemColors.WindowText), new PointF(0, 35));

            var to_be_plotted = GraphContents.ConvertAll
                (f => f.rmat(matrix.rotm(x_rotation, y_rotation, z_rotation), camera_pan));

            to_be_plotted.ForEach(f => f.plot(MouseDragDelta, this, e));
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
                if (e.KeyCode == Keys.F2)
                { Equation t1;
                    try
                    {
                        t1 = Globals.FocusedTile.Interpret();
                        //IVariable t2;

                        //t2 = t1.SolveFor(((t1.parts[0] as IContainer).parts[0] as Term).numerator[1] as IVariable, Globals.values);
                        //t1.Remember(t2);
                        var t3 = new List<Formula>();
                        foreach (var f in t1.parts)
                            t3.Add(t1.CollectLikeTerms(f as Formula));
                    }
                    catch (Error error) { Debug.WriteLine(error.Message); }
                }
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

            if (IsTyping)
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
