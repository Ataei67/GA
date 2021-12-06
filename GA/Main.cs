using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GA
{
    public partial class Main : Form
    {
        const double MaxRMembers = 1;
        const int NGeneration = 2000;
        const int MaximumMembers = 30;
        int StopCounter = 0;
        int Generation = 0;
        DateTime startTime;        
        //circle SHC;
        //RectangleF SHR;
        //Polygon SHP;
        public static Random rand;
        individual bestind;
        bool objchanged = false;
        List<individual> ind = new List<individual>();

        public Main()
        {
            InitializeComponent();                        
        }

        private void Main_Load(object sender, EventArgs e)
        {
            startTime = DateTime.Now;
            rand = new Random(DateTime.Now.GetHashCode());
            individual.ObjectSh = new circle(0, 0, 10);
            individual.ShType = (int)ObjType.Circle;
            bestind = new individual();

            for (int i = 0; i < NGeneration; i++)
            {
                individual ind0 = new individual();
                circle c = new circle(rand.NextDouble() * 20 - 10, rand.NextDouble() * 20 - 10, rand.NextDouble() * MaxRMembers);
                if (c.isValid()) ind0.Add(c);
                if (ind0.members.Count > 0) ind.Add(ind0);
            }
            ind = ind.OrderBy(x => -x.fitness).ToList();
            if (ind[0].filling > bestind.filling) bestind = new individual(ind[0]);
            Draw();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            plot.Height = this.Height - 60;
            plot.Width = this.Width - 150;
            plot.Size = new Size(Math.Min(plot.Height, plot.Width), Math.Min(plot.Height, plot.Width));
        }

        private void btnNextGeneration_Click(object sender, EventArgs e)
        {
            StopCounter = 0;
            NextGeneration();                        
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            btnNextGeneration.Enabled = false;
            timer1.Enabled = true;
            startTime = DateTime.Now;
        }
        private void btnRandom_Click(object sender, EventArgs e)
        {
            Generation = 0;
            StopCounter = 0;
            startTime = DateTime.Now;
            rand = new Random(DateTime.Now.GetHashCode());
            ChangeObject(rand.Next((int)ObjType.Count));
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            StopCounter = 0;
            ind.RemoveAll(i => i.members.Count >= 0);
            bestind = new individual();
            //Random rand = new Random(DateTime.Now.GetHashCode());
            for (int i = 0; i < NGeneration; i++)
            {
                individual ind0 = new individual();
                circle c = new circle(rand.NextDouble() * 20 - 10, rand.NextDouble() * 20 - 10, rand.NextDouble() * MaxRMembers);
                if (c.isValid()) ind0.Add(c);
                if (ind0.members.Count > 0) ind.Add(ind0);
            }
            ind = ind.OrderBy(x => -x.fitness).ToList();
            bestind = new individual(ind[0]);
            Generation = 0;
            startTime = DateTime.Now;
            Draw();
        }

        public void NextGeneration()
        {
            Generation++;            
            int NParrent = (int)(NGeneration * 0.02);
            int intdad = rand.Next(NParrent);
            int intmom = rand.Next(NParrent);
            int intmem = 0;
            // Delete individuals with lowest fitness, keep good ones
            while ((NParrent) < ind.Count)
            {
                ind.RemoveAt(ind.Count-1);
            }
            // creat population
            creatPopulation();
            // Calculate the Fitness
            calculateFitness();
            //sort fitness
            ind = ind.OrderBy(x => -x.fitness).ToList();
            if (ind[0].filling > bestind.filling)
            {
                StopCounter = 0;
                bestind = new individual(ind[0]);
            }
            else
            {
                StopCounter++;
            }
            // remove individuals ith same fitness       
            for (int i = ind.Count - 1; i > 0; i--)
            {
                if (ind[i].fitness == ind[i - 1].fitness)
                {
                    if (ind.Count > NParrent)
                        ind.RemoveAt(i);
                }
                else
                {
                    if(double.IsNaN( ind[i].fitness) || double.IsNaN(ind[i].filling) )
                        ind.RemoveAt(i);
                }
                
            }
            ind = ind.OrderBy(x => -x.filling).ToList();
            individual.BestFitness = ind[0].filling;
            ind = ind.OrderBy(x => -x.fitness).ToList();
            for (int i = ind.Count - 1; i > 0; i--)
            {
                int n = ind[i].members.Count;
                for (int j = 0; j < n;j++)
                {
                    for(int k = j+1;k< n;k++)
                    {
                        if(ind[i].members[j].hit(ind[i].members[k]))
                        {
                            n = ind[i].members.Count;
                            ind[i].join_colides();
                        }
                    }
                }
            }
            Draw();
            if(StopCounter>50)
            {
                btnStop_Click(new object(), new EventArgs());
            }
        }
        private void Draw()
        {
            Bitmap b = new Bitmap(plot.Width, plot.Height);
            Graphics g = Graphics.FromImage(b) ;
                        
            g.Clear(Color.White);
            float width = plot.Width / 25;
            float heigh = plot.Height / 25;
            float x, y, r;
            List<Brush> p = new List<Brush>();
            p.Add(new SolidBrush(Color.FromArgb(100 , Color.Blue)));
            p.Add(new SolidBrush(Color.FromArgb(100 , Color.Red)));
            p.Add(new SolidBrush(Color.FromArgb(100 , Color.Purple)));
            p.Add(new SolidBrush(Color.FromArgb(100 , Color.Green)));
            p.Add(new SolidBrush(Color.FromArgb(100, Color.HotPink)));
            
            for (int i = 4; i >= 0; i--)
            {
                foreach (circle c in ind[i].members)
                {
                    x = (float) (plot.Width / 2 + c.GetX() * width );
                    y = (float)(plot.Height / 2 + c.GetY() * heigh );
                    r = (float)(c.GetR() ) * width;
                    g.FillEllipse(p[i], x - r, y - r, 2 * r, 2 * r);
                    
                }
            }
            foreach (circle c in bestind.members)
            {
                x = (float)(plot.Width / 2 + c.GetX() * width);
                y = (float)(plot.Height / 2 + c.GetY() * heigh);
                r = (float)(c.GetR()) * width;
                g.DrawEllipse(Pens.Red, x - r, y - r, 2 * r, 2 * r);
            }

            if (individual.ShType == (int)ObjType.Circle)
            {
                circle SHC = (circle)individual.ObjectSh;
                x = (float)((plot.Width / 2) + SHC.GetX() * width);
                y = (float)((plot.Height / 2) + SHC.GetY() * heigh);
                float r1 = (float) (SHC.GetR() * width);
                float r2 = (float) (SHC.GetR() * heigh);
                //g.DrawRectangle(Pens.Black, x - r, y - r, 2 * r, 2 * r);
                g.DrawEllipse(Pens.Black, x - r1, y - r2, 2 * r1, 2 * r2);            
            }
            if(individual.ShType == (int)ObjType.Rectangle)
            {
                RectangleF SHR = (RectangleF)individual.ObjectSh;
                x = (float)((plot.Width / 2) + SHR.X * width);
                y = (float)((plot.Height / 2) + SHR.Y * heigh);
                float w = (float)(SHR.Width * width);
                float h = (float)(SHR.Height * width);
                g.DrawRectangle(Pens.Black, x, y, w, h);                
            }
            if(individual.ShType == (int)ObjType.Polygon)
            {
                Polygon SHP = (Polygon)individual.ObjectSh;
                PointF[] pl = new PointF[SHP.pointCount() + 1];
                for (int i = 0; i < SHP.pointCount(); i++)
                {
                    pl[i] = TransformPoint(SHP.getPoint(i));
                }
                pl[pl.Length - 1] = new PointF(pl[0].X, pl[0].Y);
                //g.FillPolygon(Brushes.Aqua, pl);
                g.DrawPolygon(Pens.Red, pl);
            }
            plot.Image = b;
            if(ind.Count >0)
            {
                if(!btnPlay.Enabled)                
                    g.DrawString("Gen:" + Generation.ToString() + "  BestFill:" + (ind[0].filling * 100).ToString("0.0") + "%  Elapsed:" + (DateTime.Now - startTime).TotalSeconds.ToString("0.00"), new Font("Arial", 12), Brushes.Navy, 10, 20);
                else
                    g.DrawString("Gen:" + Generation.ToString() + "  BestFill:" + (ind[0].filling * 100).ToString("0.0") + "%", new Font("Arial", 12), Brushes.Navy, 10, 20);

            }
        }
        

        private void timer1_Tick(object sender, EventArgs e)
        {            
            timer1.Enabled = false;
            NextGeneration();
            Draw();
            Application.DoEvents();
            if(!btnPlay.Enabled) timer1.Enabled = true;
            if (objchanged)
            {
                bestind = new individual();
                objchanged = false;
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopCounter = 0;
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
            timer1.Enabled = false;
            btnNextGeneration.Enabled = true;
        }
        
        
        private PointF TransformPoint(PointF p)
        {
            PointF p0 = new PointF(0, 0);
            float width = plot.Width / 25;
            float heigh = plot.Height / 25;
            p0.X = (float)plot.Width / 2 + p.X * width;
            p0.Y = (float)plot.Height / 2 + p.Y * heigh;
            return p0;
        }
        private void ChangeObject(int randtype)
        {
            if (randtype >= (int)ObjType.Count) return;
            rand = new Random(DateTime.Now.GetHashCode());            
            if (randtype == (int)ObjType.Circle)
            {
                double R = rand.NextDouble() * 8 + 2;
                double X = rand.NextDouble() * 2 * (10 - R) - 10 + R;
                double Y = rand.NextDouble() * 2 * (10 - R) - 10 + R;
                individual.ObjectSh = new circle(X, Y, R);                
            }
            if (randtype == (int)ObjType.Rectangle)
            {
                float W = (float)(rand.NextDouble() * 15 + 5);
                float H = (float)(rand.NextDouble() * 15 + 5);
                float X = (float)(rand.NextDouble() * (20 - W) - 10);
                float Y = (float)(rand.NextDouble() * (20 - H) - 10);
                individual.ObjectSh = new RectangleF(X, Y, W, H);                
            }
            if (randtype == (int)ObjType.Polygon)
            {
                Polygon SHP = new Polygon();
                int n = rand.Next(3) + 3;
                for (int i = 0; i < n; i++)
                {
                    float X = (float)(rand.NextDouble() * 20 - 10);
                    float Y = (float)(rand.NextDouble() * 20 - 10);
                    SHP.Add(new PointF(X, Y));
                }
                individual.ObjectSh = SHP;
            }
            individual.ShType = randtype;
            bestind = new individual();
            objchanged = true;
            Draw();
        }
        
        
        
        
        private void creatPopulation()
        {
            int NParrent = (int)(NGeneration * 0.02);
            int intdad = rand.Next(NParrent);
            int intmom = rand.Next(NParrent);
            int intmem = 0;
            while (NGeneration > ind.Count)
            {
                individual ind0 = new individual();
                double chance = individual.BestFitness;
                if (chance > 0.9) chance = 0.9;
                if (chance < 0.1) chance = 0.1;
                double sel = rand.NextDouble(); // Copy or Crossover
                if (sel < 0.2) // crossover
                {
                    intdad = (int)(Math.Pow(rand.NextDouble(), 2) * NParrent);
                    intmom = rand.Next(NParrent);
                    while (intmom == intdad) intmom = rand.Next(NParrent);

                    individual dad = ind[intdad];
                    individual mom = ind[intmom];

                    foreach (circle c in dad.members)
                    {
                        if (rand.NextDouble() < 0.3)
                        {
                            circle A = c.Clone();
                            if (A.isValid()) ind0.Add(A);
                        }
                    }
                    foreach (circle c in mom.members)
                    {
                        if (rand.NextDouble() < 0.3)
                        {
                            circle A = c.Clone();
                            if (A.isValid()) ind0.Add(A);
                        }
                    }
                }
                else //copy
                {
                    //intdad = rand.Next(NParrent);
                    intdad = (int)(Math.Pow(rand.NextDouble(), 2) * NParrent);
                    //ind0 = new individual(ind[intdad]);
                    individual dad = ind[intdad];
                    foreach (circle c in dad.members)
                    {
                        circle A = c.Clone();
                        if (A.isValid()) ind0.Add(A);
                    }
                }
                sel = rand.NextDouble(); // Add members
                while (sel < 0.2) // add a member
                {
                    circle c1 = new circle(rand.NextDouble() * 20 - 10, rand.NextDouble() * 20 - 10, (rand.NextDouble() * 0.99 + 0.01) * MaxRMembers);
                    if (c1.isValid()) ind0.Add(c1);
                    sel = rand.NextDouble(); // Add members
                }
                sel = rand.NextDouble(); // Del members
                while (sel < 0.05 && ind0.members.Count > 2) // del a member
                {
                    intmem = rand.Next(ind0.members.Count);
                    ind0.Del(intmem);
                    sel = rand.NextDouble(); // Del members
                }
                //Mutate
                ind0.Mutates(1 - chance);

                while (ind0.members.Count > MaximumMembers) // Get rid of maximum extera circles
                {
                    intmem = rand.Next(ind0.members.Count);
                    ind0.Del(intmem);
                }
                if (ind0.members.Count > 1) // add child to poplation
                {
                    ind0.join_colides();
                    //ind0.remove_insides();
                    ind.Add(ind0);
                }

            }
        }
        private void calculateFitness()
        {
            for (int i = 0; i < ind.Count; i++)
            {
                ind[i].calculate_fitness();
            }
        }

    }
}
