using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GA
{
    class individual
    {
        public static double BestFitness = -1;
        public static int ShType = 0;
        public static Random rand;
        public static object ObjectSh;
        public List<circle> members = new List<circle>();        
        public double fitness;
        public double filling;
        public individual()
        {
            rand = new Random(DateTime.Now.GetHashCode());
            members = new List<circle>();
            fitness = -1;
        }
        public individual(individual ind)
        {
            filling = ind.filling;
            fitness = ind.fitness;
            members.Clear();
            foreach (circle c in ind.members)
            {
                members.Add(c.Clone());
            }            
        }

        public void Add(circle c)
        {
            if (c.isValid()) members.Add(c);            
        }
        public void Mutate(int i, double step)
        {
            circle A = members[i];
            double X = A.GetX();
            double Y = A.GetY();
            double R = A.GetR();

            X += ((rand.NextDouble() * step) - step / 2) * R;
            Y += ((rand.NextDouble() * step) - step / 2) * R;
            R += ((rand.NextDouble() * step) - step / 2) * R;
            
            if (!A.setCircle(X, Y, R)) Del(i);
            
        }

        public void Mutates(double chance)
        {            
            double step = 2;
            for (int i = 0; i < members.Count; i++)
            {
                if (rand.NextDouble() < chance)
                    Mutate(i, step * chance);
            }            
        }


        public bool Del(int i)
        {
            if (i >= members.Count) return false;
            members.RemoveAt(i);
            return true;
        }

        double interArea(circle A, circle B)
        {

            // https://www.xarg.org/2016/07/calculate-the-intersection-area-of-two-circles/
            double d = A.dist(B);
            double AR = A.GetR();
            double BR = B.GetR();
            if (d < (AR + BR))
            {
                double a = AR * AR;
                double b = BR * BR;
                if (d <= (Math.Abs(BR - AR) + 1e-5))
                {
                    return Math.PI * Math.Max(a, b);
                }
                double x = (a - b + d * d) / (2 * d);
                double z = x * x;
                if (a < z) return 0;
                double y = Math.Sqrt(a - z);
                double _x = Math.Sqrt(x);
                if ((y + 1e-6) > BR) y = BR;
                double l4 = b * Math.Asin(y / BR);
                if ((y + 1e-6) > AR) y = AR;
                double l3 = a * Math.Asin(y / AR);
                double l2 = 0;
                if (z + b - a >= 0) l2 = (x + Math.Sqrt(z + b - a));
                double l1 = l3 + l4 - y * l2;
                return l1;
            }
            return 0;
        }

        double interAreaA(circle A, circle B)
        {
            //https://www.xarg.org/2016/07/calculate-the-intersection-points-of-two-circles/
            double d = A.dist(B);
            double AR = A.GetR();
            double BR = B.GetR();

            if (d < (AR + BR))
            {

                double a = AR * AR;
                double b = BR * BR;
                double x = (a - b + d * d) / (2 * d);
                double z = x * x;
                if (d <= (Math.Abs(BR - AR) + 1e-5))
                {
                    if (AR > BR)
                        return Math.PI * a;
                    else
                        return 0;
                }
                if (a < z) return 0;
                double y = Math.Sqrt(a - z);
                return a * Math.Asin(y / AR) - y * x;
            }
            return 0;
        }
        public bool colide(int i, int j)
        {
            circle A = members[i];
            circle B = members[j];
            

            if (!A.hit(B)) return false;
            double AX = A.GetX();
            double AY = A.GetY();
            double AR = A.GetR();
            double BX = B.GetX();
            double BY = B.GetY();
            double BR = B.GetR();
            double areaC0 = Math.PI * (AR * AR);
            double areaC1 = Math.PI * (BR * BR);
            double intArea = interArea(A, B);
            double intAreaA = interAreaA(A, B);
            double intAreaB = interAreaA(B, A);
            double comArea = areaC0 + areaC1 - intArea;
            double X, Y, R;
            if (intArea <= 0) return false;

            R = Math.Sqrt(comArea / Math.PI);

            X = (AX * (areaC0 - intAreaB) + BX * (areaC1 - intAreaA)) / comArea;
            Y = (AY * (areaC0 - intAreaB) + BY * (areaC1 - intAreaA)) / comArea;
            if (double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(R))
            {
                return false;
            }
            string str = "Colide:[" + i.ToString() + "," + j.ToString() + "] ";
            if (!A.setCircle(X, Y, R))
            {
                return false;
            }
            
            
            //Console.WriteLine("Colide: R:" + (c1.R + c0.R).ToString() + " Dist:" + c0.dist(c1) + " , new R:" + c0.R.ToString());
            members[i] = A;
            Del(j);
            //calculate_fitness();
            return true;
        }

        public bool join_colides()
        {
            int i = 0;
            int j = members.Count - 1;
            bool res = false;
            bool collide = true;
            while (collide)
            {
                //members = members.OrderBy(x => -x.GetR()).ToList();
                collide = false;
                i = 0;
                j = members.Count - 1;
                while (i < members.Count)
                {
                    j = members.Count - 1;
                    while (j > i)
                    {
                        if (members[i].hit(members[j]))
                        {
                            if (colide(i, j))
                            {
                                res = true;
                                i = 0;
                                j = members.Count;
                                //collide = true;
                            }
                        }
                        j--;
                    }
                    i++;
                }
            }
            return res;
        }
        public bool inside(int i, int j)
        {
            circle A = members[i];
            circle B = members[j];

            if ((A.dist(B) + B.GetR()) >= (A.GetR() + 1e-5)) return false;
            Del(j);
            //calculate_fitness();
            return true;
        }
        public bool remove_insides()
        {
            bool res = false;
            members = members.OrderBy(x => -x.GetR()).ToList();
            int i = 0;
            int j = members.Count - 1;
            while (i < members.Count)
            {
                j = members.Count - 1;
                while (j > i)
                {
                    res |= inside(i, j);
                    j--;
                }
                i++;
            }

            return res;
        }
        
        public bool checktherectangle(circle c, RectangleF R)
        {
            //return (c.dist(0, 0) + c.R) < 10;

            double x = c.GetX(), y = c.GetY(), r = c.GetR();
            if (x + r > (R.X + R.Width)) return false;
            if (x - r < (R.X)) return false;
            if (y + r > (R.Y + R.Height)) return false;
            if (y - r < (R.Y)) return false;

            return true;
        }
        
        public void calculate_fitness()
        {
            double chance = individual.BestFitness;
            if (chance > 0.9) chance = 0.9;
            if (chance < 0.1) chance = 0.1;

            double res = 0;
            double area = 1;
            bool isInside = false;
            if (ShType == (int)ObjType.Circle) area = ((circle)ObjectSh).GetArea();
            if (ShType == (int)ObjType.Rectangle) area = ((RectangleF)ObjectSh).Width * ((RectangleF)ObjectSh).Height;
            if (ShType == (int)ObjType.Polygon) area = ((Polygon)ObjectSh).Area();
            foreach (circle c in members)
            {
                if (ShType == (int)ObjType.Circle) isInside = (c.dist((circle)ObjectSh) + c.GetR()) < ((circle)ObjectSh).GetR();
                if (ShType == (int)ObjType.Rectangle) isInside = checktherectangle(c, (RectangleF)ObjectSh);
                if (ShType == (int)ObjType.Polygon) isInside = ((Polygon)ObjectSh).CheckACircle(c);
                if (isInside)
                {
                    res += Math.Pow(c.GetArea(), 3);
                    filling += c.GetArea();
                }
                else
                {
                    res -= c.GetArea();
                }
            }

            fitness = res / area;                  
            filling = filling / area;
            fitness = fitness + filling * chance;
        }
    }
}
