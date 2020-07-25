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

namespace GCalc_UI
{
    public partial class Window : Engine
    {
        /// <summary>
        /// All the global variables and methods
        /// </summary>
        protected static class Globals
        {
            public static Font MathsFont = new Font("XITS", 12, FontStyle.Italic);
            public static Font DataFont = new Font("Arial", 9);

            public static double rotation_speed = 0.03;
            public static double panning_speed = 0.05;
        }

        public Window()
        {
            InitializeComponent();
            InitialiseTilePane();
            InitialiseInputPane();
            InitialiseGraphPanel();

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
            The_Events.KeyUp += OnKeyUp;
        }

        private void Unsubscribe()
        {
            if (The_Events == null) return;

            The_Events.MouseUp -= OnMouseUp;
            The_Events.MouseWheel -= OnMouseWheel;
            The_Events.KeyDown -= OnKeyDown;
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

        class Tile : Control
        {
            //public formula def;
            //public tensor sub;
            public Panel Canvas;
            public TextBox TextInput;
            public new static Size Size { get { return new Size(183, 50); } }

            public Tile(Control v, Point Location/*, formula L = null, formula R = null*/)
            {
                //def = f;
                //sub = t;

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
            }
            
            private void OnPaint(object sender, PaintEventArgs e)
            {
                e.Graphics.DrawString(TextInput.Text, Globals.MathsFont, new SolidBrush(SystemColors.ControlText), 0, 10);
            }

            private void OnClick(object sender, EventArgs e)
            {
                (Parent.Parent as Window).IsTyping = true; ;
                TextInput.Focus();
            }

            private void OnChange(object sender, EventArgs e)
            {
                //if (def == null) { def = new formula(new List<term>()); }
                /*if (TextInput.Text.Last() == '+')
                {
                    Debug.Write("+");
                    var novus = new term(new List<coefficient>(), new List<coefficient>());
                    for()
                }*/
                Canvas.Invalidate();

                if (((Tile)Parent.Controls[Parent.Controls.Count - 1]).TextInput.Text != "")
                {
                    Parent.Controls.Add(new Tile(Parent, new Point(Canvas.Location.X, Canvas.Location.Y + Canvas.Height)));
                }
                else
                {
                    if (Parent.Controls.Count > 6 && ((Tile)Parent.Controls[Parent.Controls.Count - 4]).TextInput.Text == "")
                    {
                        Parent.Controls.RemoveAt(Parent.Controls.Count - 1); //the TextInput
                        Parent.Controls.RemoveAt(Parent.Controls.Count - 1); //the Tile
                        Parent.Controls.RemoveAt(Parent.Controls.Count - 1); //the Canvas (all of these were added to the controls)
                    }
                }
                /*
                if((Parent.Controls.Count - 1) / 3 > 5)
                {
                    ((VScrollBar)Parent.Controls[0]).Maximum = 10;
                }
                else if((Parent.Controls.Count - 1) * Size.Height / (3 * Parent.Height) > 1)
                    */
                ((VScrollBar)Parent.Controls[0]).Maximum = (int)((Parent.Controls.Count - 4.0) * Size.Height * 10 / (3 * Parent.Height));
                //-1 because of scroller but then + 2 for extra scroll while cancelling
                //out the 3 in the denominator, / 3 because adding a tile will add 3 controls, * Size.Height / Parent.Height to get height of TilePane percentage 
                //Debug.Write("[" + ((VScrollBar)Parent.Controls[0]).Maximum + "'" + (int)((Parent.Controls.Count + 2.0) * Size.Height * 10 / (3 * Parent.Height)) + "]");
            }
        }

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
                for (int j = 1; j < 10; j++)
                {
                    var InputBtnFunc = new Button();

                    InputBtnFunc.BackColor = SystemColors.ControlLight;
                    InputBtnFunc.FlatStyle = FlatStyle.Popup;
                    InputBtnFunc.Location = new Point(50 * (9 - j), 0);
                    InputBtnFunc.Name = "InputBtnFunc";
                    InputBtnFunc.Size = new Size(50, 58);
                    InputBtnFunc.TabIndex = 1;
                    InputBtnFunc.Text = j + " : " + j;
                    InputBtnFunc.UseVisualStyleBackColor = false;
                    InputBtnFunc.Click += OnInputBtnFunc;

                    FuncGroups[i].Controls.Add(InputBtnFunc);
                    RankGroups[i].Add(InputBtnFunc, new[] { j, j });
                }
            }
        }

        void OnInputBtnFunc(object sender, EventArgs e)
        {
            var InputBtnFunc = sender as Button;

            if (InputBtnFunc.Left == 0) { return; }

            InputBtnFunc.Text = (int.Parse(InputBtnFunc.Text.Substring(0, InputBtnFunc.Text.IndexOf(' '))) + 1) +
                InputBtnFunc.Text.Substring(InputBtnFunc.Text.IndexOf(' '));

            int new_rank;
            KeyValuePair<Button, int[]> overtaken_button;

            for (int i = 0; i < RankGroups.Count(); i++)
            {
                try
                {
                    //increase rank of the clicked InputBtnFunc
                    RankGroups[i][InputBtnFunc][1]++;
                    new_rank = RankGroups[i][InputBtnFunc][1];
                    if (new_rank > RankGroups[i].Count - 9)
                        InputBtnFunc.Left = 50 * (9 - new_rank);

                    //find the previous title holder of InputBtnFunc's new rank
                    overtaken_button = RankGroups[i].Where(r => r.Value[1] == new_rank && r.Key != InputBtnFunc).ElementAt(0);

                    //demote the previous title holder
                    overtaken_button.Value[1]--;
                    overtaken_button.Key.Text = (int.Parse(overtaken_button.Key.Text.Substring(0, overtaken_button.Key.Text.IndexOf(' '))) - 1) +
                    overtaken_button.Key.Text.Substring(overtaken_button.Key.Text.IndexOf(' '));
                    if (overtaken_button.Value[1] > RankGroups[i].Count - 9)
                        overtaken_button.Key.Left = 50 * (10 - new_rank);
                    else if (overtaken_button.Value[1] == RankGroups[i].Count - 10)
                        overtaken_button.Key.Visible = false;
                }
                catch (Exception x) { Debug.Write("{" + x + "|" + i + "}"); }
            }
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

        double x_rotation, y_rotation, z_rotation = 0;
        vector camera_pan = vector.o;

        Keys key_being_pressed;
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            key_being_pressed = e.KeyCode;
            KeyPressDownTimer.Start();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            KeyPressDownTimer.Stop();
            GraphPanel.Invalidate();
        }

        private void KeyPressDownTick(object sender, EventArgs e)
        {
            if (key_being_pressed == Keys.Escape)
            {
                IsTyping = false;
                GraphPanel.Focus();
            }

            if (!IsTyping)
            {
                switch (key_being_pressed)
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
    }
}
