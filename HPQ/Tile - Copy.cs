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

namespace NotGCalc
{
    partial class Window
    {
        class Tile : Control
        {
            //public formula def;
            //public tensor sub;
            public Panel Canvas;
            public TextBox TextInput;
            public new static Size Size { get { return new Size(183, 50); } }
            /// <summary>
            /// The cursor that points to an InputStream within the InputStream coordinate system.
            /// </summary>
            public Point pointer = new Point(0, 0);
            ScalarPiece cursor;
            //Dictionary<Point, InputStream> Input_Streams;
            Group Pieces = new Group(null);

            public Equation equation = new Equation() { L = new Formula(), R = new Formula() };
            IContainer CurrentIContainer;

            class Piece
            {
                /// <summary>
                /// Parent group of this piece
                /// </summary>
                public Group group;

                public Group super;
                public Group sub;

                public int width;
                public int height;

                public virtual void draw(int x, int y, Graphics g) { }
            }
            class ScalarPiece : Piece
            {
                public string val;
                public Scalar scalar;

                public ScalarPiece(string value, Group group, List<ScalarPiece> above = null, List<ScalarPiece> below = null)
                {
                    val = value;
                    super = new Group(this) { pieces = above };
                    sub = new Group(this) { pieces = below };
                    this.group = group;
                }

                public override void draw(int x, int y, Graphics g)
                {
                    g.DrawImage(Globals.symbols[val], x, y);
                }
            }
            class TensorPiece : Piece
            {
                public Tensor tensor;

                public List<Group> rows;
            }
            class FunctionPiece : Piece
            {
                public Piece operand;

                public override void draw(int x, int y, Graphics g)
                {
                    g.DrawImage(Globals.symbols[Globals.input_pane_key], x, y);
                }
            }
            class DivPiece : Piece
            {
                public Term term;

                public override void draw(int x, int y, Graphics g)
                {
                    g.DrawLine(new Pen(Globals.MathsBrush), x, y, x + 10, y);
                }
            }

            class Group
            {
                public List<ScalarPiece> pieces;
                public ScalarPiece parent;

                public Group(ScalarPiece Parent)
                {
                    parent = Parent;
                    pieces = new List<ScalarPiece>();
                }

                public void draw(int x, int y, Graphics g)
                {
                    var acc = new Point(x, y);
                    for(int i = 0; i < pieces.Count; i++)
                    {
                        acc.Offset(pieces[i].width, pieces[i].height);
                        pieces[i].draw(acc.X, acc.Y, g);
                    }
                }
            }

            /*/// <summary>
            /// Would have been a value tuple containing the value and id of the input stream, but
            /// Nuget was being annoying so here we are.
            /// </summary>
            struct Input_Stream
            {
                public string val;
                public int id;
                public Input_Stream(string value, int id)
                {
                    val = value;
                    this.id = id;
                }
                public Input_Stream(int id)
                {
                    val = "";
                    this.id = id;
                }
                public void update(string new_value)
                { val = new_value; }
            }*/

            /*class InputStream
            {
                public string value;
                public List<Point> pos;
                public int id;

                public InputStream(int id)
                {
                    value = "";
                    pos = new List<Point>();
                    this.id = id;
                }
            }*/

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

                //Input_Streams = new Dictionary<Point, Input_Stream>();
                //Input_Streams.Add(new Point(0, 0), new Input_Stream(0));
            }

            private void OnPaint(object sender, PaintEventArgs e)
            {
                if (TextInput.Focused) e.Graphics.DrawRectangle(new Pen(SystemColors.ActiveCaption, 2), Canvas.Bounds);

                e.Graphics.DrawString(TextInput.Text, Globals.MathsFont, Globals.MathsBrush, 0, 10);

                Pieces.draw(0, 0, e.Graphics);
                
                //var InputStreams = new List<InputStream>();
                //InputStream active_stream;
                
                //for (int i = 0; i < TextInput.Text.Length; i++)
                //{
                //    /*if (TextInput.Text[i] == '_')
                //    {
                //        InputStreams.Add(new InputStream(InputStreams.Count));
                //        InputStreams[InputStreams.Count - 2].pos.ForEach(p =>
                //        {
                //            InputStreams[InputStreams.Count - 1].pos.Add(new Point(p.X, p.Y - 1));
                //            InputStreams[InputStreams.Count - 1].pos.Add(new Point(p.X + 1, p.Y));
                //        });

                //        active_stream = InputStreams[InputStreams.Count - 1];
                //    }
                //    else if (TextInput.Text[i] == '^')
                //    {
                //        pointer.Y++;
                //        InputStreams.Add(new InputStream(InputStreams.Count));
                //        InputStreams[InputStreams.Count - 2].pos.ForEach(p =>
                //        {
                //            InputStreams[InputStreams.Count - 1].pos.Add(new Point(p.X, p.Y + 1));
                //            InputStreams[InputStreams.Count - 1].pos.Add(new Point(p.X + 1, p.Y));
                //        });
                        
                //        active_stream = InputStreams[InputStreams.Count - 1];
                //    }
                //    else */
                    
                //    #region user input drawers

                //    switch(TextInput.)
                //    {

                //    }

                //    #endregion
                //}
            }

            enum piece_group { super, sub, none, initial }
            piece_group next_piece_group = piece_group.initial;
            enum piece_type { tensor, scalar, function, ambiguous }
            piece_type next_piece_type = piece_type.ambiguous;
            /// <summary>
            /// Handles the KeyDown event when it happens while this.InputText is Focused.
            /// </summary>
            /// <param name="key">Keycode of the key being pressed</param>
            public void KeyDown(Key_ValueTuple key)
            {
                switch(key.KeyCode)
                {
                    //case Keys.Left:
                    //    cursor--;
                    //    if (cursor < 0) pointer.X++;
                    //    if (!Input_Streams.ToList().Exists(k => k.Key == pointer))
                    //    {
                    //        pointer.X--;
                    //        cursor++;
                    //    }
                    //    else cursor = Input_Streams[pointer].val.Length - 1;
                    //    break;
                    //case Keys.Right:
                    //    cursor++;
                    //    if (cursor >= Input_Streams[pointer].val.Length) pointer.X++;
                    //    if (!Input_Streams.ToList().Exists(k => k.Key == pointer))
                    //    {
                    //        pointer.X--;
                    //        cursor--;
                    //    }
                    //    else cursor = 0;
                    //    break;
                    //case Keys.Up:
                    //    pointer.Y++;
                    //    if (!Input_Streams.ToList().Exists(k => k.Key == pointer)) pointer.Y--;
                    //    break;
                    //case Keys.Down:
                    //    pointer.Y--;
                    //    if (!Input_Streams.ToList().Exists(k => k.Key == pointer)) pointer.Y++;
                    //    break;
                }
                
                switch (key.KeyCode)
                {
                    case Keys.Up:
                        cursor = cursor.super.pieces.Any() ? cursor.super.pieces[0] : cursor;
                        CurrentIContainer = (Term)CurrentIContainer.parts[0];
                        break;
                }

                switch(key.KeyChar)
                {
                    case '_':
                        next_piece_group = piece_group.sub;
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case '^':
                        next_piece_group = piece_group.super;
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case '[':
                        if (CurrentIContainer is Formula)
                        {
                            CurrentIContainer.parts.Add(new Term());
                            CurrentIContainer = (Term)CurrentIContainer.parts.Last();
                        }
                        (CurrentIContainer as Term).numerator.Add(new Tensor());

                        next_piece_type = piece_type.tensor;
                        next_piece_group = piece_group.none;
                        break;
                    case '+':
                        CurrentIContainer.parts.Add(new Term());
                        CurrentIContainer = (Term)CurrentIContainer.parts.Last();
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case '-':
                        var negative_term = new Term();
                        negative_term.numerator.Add(new Scalar(null, -1));
                        CurrentIContainer.parts.Add(negative_term);
                        CurrentIContainer = (Term)CurrentIContainer.parts.Last();
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case '*':
                        if (CurrentIContainer is Formula)
                        {
                            CurrentIContainer.parts.Add(new Term());
                            CurrentIContainer = (Term)CurrentIContainer.parts.Last();
                        }
                        (CurrentIContainer as Term).numerator.Add(new Tensor());
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case '/':
                        if (CurrentIContainer is Formula)
                        {
                            CurrentIContainer.parts.Add(new Term());
                            CurrentIContainer = (Term)CurrentIContainer.parts.Last();
                        }
                        (CurrentIContainer as Term).denominator.Add(new Tensor());
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.ambiguous;
                        break;
                }

                if(char.IsLetterOrDigit(key.KeyChar))
                {
                    var index = "Key" + key.KeyChar;

                    switch(next_piece_group)
                    {
                        case piece_group.super:
                            cursor.super.pieces.Add(new ScalarPiece(index, cursor.super));
                            cursor = cursor.super.pieces.Last();
                            break;
                        case piece_group.sub:
                            cursor.super.pieces.Add(new ScalarPiece(index, cursor.sub));
                            cursor = cursor.sub.pieces.Last();
                            break;
                        case piece_group.none:
                            cursor.group.pieces.Add(new ScalarPiece(index, cursor.group));
                            cursor = cursor.group.pieces.Last();
                            break;
                        default:
                            Pieces.pieces.Add(new ScalarPiece(index, Pieces));
                            cursor = Pieces.pieces.Last();
                            break;
                    }
                }
            }

            private void OnClick(object sender, EventArgs e)
            {
                (Parent.Parent as Window).IsTyping = true;
                TextInput.Focus();
                //update each other Tile so that they look like they aren't selected [anymore]
                foreach (Control c in Parent.Controls) { if (c is Tile) (c as Tile).Canvas.Invalidate(); }
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

                ((VScrollBar)Parent.Controls[0]).Maximum = (int)((Parent.Controls.Count - 4.0) * Size.Height * 10 / (3 * Parent.Height));
                //-1 because of scroller but then + 2 for extra scroll while cancelling
                //out the 3 in the denominator, / 3 because adding a tile will add 3 controls, * Size.Height / Parent.Height to get height of TilePane percentage 
                //Debug.Write("[" + ((VScrollBar)Parent.Controls[0]).Maximum + "'" + (int)((Parent.Controls.Count + 2.0) * Size.Height * 10 / (3 * Parent.Height)) + "]");
            }
        }
    }
}
