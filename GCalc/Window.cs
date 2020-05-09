using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;

using Liber;

namespace GCalc
{
    public partial class Window : Engine
    {
        class Tile
        {
            public formula def;
            public tensor sub;
            public bool foc;
            public Panel pan;
            public TextBox text;

            public Tile(Window v, Rectangle bnds, tensor t = null, formula f = null)
            {
                def = f;
                sub = t;
                foc = false;

                pan = new Panel();
                pan.Location = bnds.Location;
                pan.Name = "pan";
                pan.Size = bnds.Size;
                pan.TabIndex = 2;
                pan.Paint += ppaint;
                pan.Click += pclick;
                v.Controls.Add(pan);

                text = new TextBox();
                text.Location = bnds.Location;
                text.Name = "text";
                text.Size = bnds.Size;
                text.TabIndex = 1;
                text.TextChanged += tchange;
                //text.Visible = false;
                text.Click += pclick;
                v.Controls.Add(pan);
            }

            private void ppaint(object sender, PaintEventArgs e)
            {

            }

            private void pclick(object sender, EventArgs e)
            {
                text.Focus();
                foc = true;
            }

            private void tchange(object sender, EventArgs e)
            {
                Debug.Write(".");
                if(def == null) { def = new formula(new List<term>()); }
                /*if (text.Text.Last() == '+')
                {
                    Debug.Write("+");
                    var novus = new term(new List<coefficient>(), new List<coefficient>());
                    for()
                }*/
            }
        }

        interface coefficient { tensor Value(params tensor[] vars); }
        class tensor : coefficient
        {
            public List<List<double>> parts;
            public int rows { get { return parts.Count; } }
            public int cols { get { return parts[0].Count; } }
            public bool param;
            public string name;

            public tensor() { }
            public tensor(double[,] bits, string name = null, bool param = false)
            {
                parts = new List<List<double>>();
                for (int i = 0; i < bits.GetLength(0); i++)
                { parts.Add(new List<double>()); for (int j = 0; j < bits.GetLength(1); j++) { parts[i].Add(bits[i, j]); } }

                this.param = param;
                this.name = name;
            }
            public tensor(List<List<double>> bits, string name = null, bool param = false)
            {
                this.param = param;
                this.name = name;
                parts = bits;
            }
            public tensor(List<double> bits, string name = null, bool param = false)
            {
                this.param = param;
                this.name = name;
                parts = new List<List<double>>() { bits };
            }
            public tensor(double part, string name = null, bool param = false)
            {
                this.param = param;
                this.name = name;
                parts = new List<List<double>>() { new List<double> { part } };
            }

            public tensor Value(params tensor[] vars) { return this; }

            public tensor add(tensor b)
            {
                if (rows == b.rows && cols == b.cols)
                {
                    var bits = new List<List<double>>();
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                            bits[i][j] = parts[i][j] + b.parts[i][j];
                    }
                    return new tensor(bits);
                }
                else { throw new Exception("Error: Tensors have different number of dimensions."); }
            }
            public tensor dot(tensor b)
            {
                if (rows == b.rows && cols == b.cols)
                {
                    var bits = new List<List<double>>();
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                            bits[i][j] = parts[i][j] + b.parts[i][j];
                    }
                    return new tensor(bits);
                }
                else { throw new Exception("Error: Tensors have different number of dimensions."); }
            }

            public tensor inv()
            {
                var cout = dcpy(this);
                if(rows == 1 && cols == 1) { cout.parts[0][0] = 1 / parts[0][0]; return cout; }
                else { throw new Exception("can only invert scalars atm"); }
            }
        }
        class term : coefficient
        {
            public List<coefficient> num;
            public List<coefficient> den;
            public List<coefficient> tensors { get { return (List<coefficient>)num.Concat(den); } }

            public term(List<coefficient> numerator, List<coefficient> denomenator) { num = numerator; den = denomenator; }

            /// <summary>
            /// value of the term given the parameters
            /// </summary>
            /// <param name="prms"></param>
            /// <returns></returns>
            public tensor Value(params tensor[] vars)
            {
                return producti.arrprd<coefficient, tensor>(num, (s, p) => 
                {
                    if (p is tensor && ((tensor)p).name != null) { p = vars.ToList().Find(t => t.name == ((tensor)p).name); }
                    return s.Value().dot(p.Value());
                })
                .dot(producti.arrprd<coefficient, tensor>(num, (s, p) =>
                {
                    if (p is tensor && ((tensor)p).name != null) { p = vars.ToList().Find(t => t.name == ((tensor)p).name); }
                    return s.Value().dot(p.Value());
                }));
            }
            public List<tensor> getParams() { return tensors.Where(t => t is tensor && ((tensor)t).param).Cast<tensor>().ToList(); }

        }
        class formula : coefficient
        {
            public List<term> terms;

            public formula(List<term> bits) { terms = bits; }
            /*public static formula fromString(string line)
            {
                for(int i = 0; i < )
            }*/

            public tensor Value(params tensor[] vars) { return producti.arrprd<term, tensor>(terms, (s, p) => s.add(p.Value(vars))); }
        }

        List<formula> formulae;

        public Window()
        {
            InitializeComponent();

            formulae = new List<formula>();
            textBox1.Width = 0;
        }
        
        List<Keys> keys = new List<Keys>();
        private void kd(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            { //formulae.Add(formula.fromString(input.Text)); 

            }
            //if(e.KeyCode == Keys.Left)
            //if (e.KeyCode == Keys.Add) { keys.Add(e.KeyCode); newterm(); }
            //if (e.KeyCode == Keys.Subtract) { keys.Add(e.KeyCode); newterm(); }


        }
        void newterm() { }
        void newcoef() { }
        private void change(object sender, EventArgs e)
        {
            Debug.Write(".");
            if (textBox1.TextLength > 0)
            {
                if (textBox1.Text.Last() == '+') { Debug.Write("plus"); }
            }
            casus = textBox1.Text;
            panel1.Invalidate();
        }

        string casus = "";
        private void ppaint(object sender, PaintEventArgs e)
        {
            
            e.Graphics.DrawString(casus, new Font("Arial", 8), new SolidBrush(SystemColors.ControlText), 0, 0);
        }

        private void pmd(object sender, MouseEventArgs e)
        {
            textBox1.Focus();
        }
    }
}
