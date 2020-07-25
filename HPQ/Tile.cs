using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace NotGCalc
{
    partial class Window
    {
        protected class Tile : Control
        {
            //public formula def;
            //public tensor sub;
            public Panel Canvas;
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
                        var formae = (from p in PIECES where fo(p) select new[] { PIECES.IndexOf(p), next(p, PIECES, fo) }).ToList();
                        var equati = (from f in formae where eq(f) select new[] { formae.IndexOf(f), next(f, formae, eq) }).ToList();

                        //Add partitioning points at the start so there are an equal number of partitions & partitioning point indices
                        formae.Insert(0, new[] { 0, next(PIECES[0], PIECES, fo) });
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

                Root = new Group(null, 'n');
                //Roots = new List<Group> { new Group(null) };
                //CurrentGroup = Roots[0];
                cursor = new ScalarPiece("init", Root);
                //CurrentIContainer = equation;
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
                string err = ""; bool _err;
                try { Root.draw(0, CanvasHeight / 2, e.Graphics, cursor); }
                catch (Error error) { err = error.Message; _err = true; }
                //e.Graphics.DrawLine(new Pen(Color.Crimson), 0, Canvas.Height / 2, Canvas.Width, Canvas.Height / 2);
                
                e.Graphics.DrawLine(new Pen(SystemColors.GrayText), 4, CanvasHeight, Canvas.Width - 8, CanvasHeight);//pretty border line

                Equation interp = new Equation(); IVariable variable = new Tensor();
                try
                {
                    interp = Interpret();
                    if (interp.EqRelations[0] != relation.None)
                        variable = interp.SolveFor(((interp.parts[0] as IContainer).parts[0] as Term).numerator[1] as IVariable, Globals.values);
                    else
                        throw new SyntaxError("SyntaxError: No equation to solve.");
                    _err = false;
                }
                catch (Error error) { if(err == "") err = error.Message; _err = true; }
                if (_err)
                    e.Graphics.DrawString(err, new Font(Globals.MathsFont.FontFamily, 8), Globals.MathsBrush, 0, CanvasHeight + 2);
                else
                {
                    var answer = new Group(null);
                    var rels = new[] { " ", "=", ">", "<", ">=", "<=", "=" };//last element is the Piece.val for relation.Maps, and is the reason you see '=' displayed for function identities
                    Piece value = new NullPiece(answer);
                    if (variable is Function)
                        value = DisplayFormula((variable as Function).Process, answer);
                    else switch((variable as Tensor).DimDim)
                    {
                        case 0 when interp.EqRelations[0] != relation.Maps:
                            value = new ScalarPiece(((variable as Tensor).parts[0][0] as Scalar).output.Value + "", answer);
                            break;
                        case 0 when interp.EqRelations[0] == relation.Maps:
                            break;
                        case 1:
                        case 2:
                            value = new TensorPiece(answer, new Size((variable as Tensor).cols, (variable as Tensor).parts.Count));
                            break;
                        default:
                            break;
                    }
                    answer.pieces = new List<Piece>
                    {
                        new ScalarPiece("Key" + variable.name.Last() + "", answer),
                        new OperatorPiece(rels[(int)interp.EqRelations[0]], answer), 
                        value
                    };
                    answer.draw(0, Canvas.Height + 4 - answer.height, e.Graphics, cursor);
                }
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
                    case Keys.Oemplus when !shift:
                        //Roots.Add(new Group(null, 'n'));
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece("=");
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.Oemcomma when shift:
                        //Roots.Add(new Group(null, 'n'));
                        next_piece_group = piece_group.none;
                        next_piece_type = piece_type.operator_sign;
                        AddPiece("<");
                        next_piece_type = piece_type.ambiguous;
                        break;
                    case Keys.OemPeriod when shift:
                        //Roots.Add(new Group(null, 'n'));
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
            
            public Equation Interpret()
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
                return cout.ElementAt(0);
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
                                addend[0].pieces.Add(new ScalarPiece(tensor.name == null ? (tensor.parts[0][0] as Scalar).output.Value + "" : tensor.name,
                                    group, tensor.Index == null ? null : DisplayFormula(tensor.Index).inside.pieces));
                                if (tensor.Index != null)
                                    addend[0].pieces.Last().super.pieces.ForEach(s => s.group = addend[0].pieces.Last().super);
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
}
