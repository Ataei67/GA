using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GA
{
    public class Objects
    {
        
    }
    public enum ObjType
    {
        Circle,
        Rectangle,
        Polygon,
        Count
    }
    public class Line
    {
        public PointF Dir;
        public PointF P0;
        public PointF P1;
        public Line(PointF p0, PointF p1)
        {
            Dir = new PointF(p1.X - p0.X, p1.Y - p0.Y);
            P0 = p0;
            P1 = p1;
        }
        public float dist(PointF p0)
        {
            float dis = 0;
            if (Dir.X == 0)
            {
                dis = Math.Abs(p0.X - P0.X);
            }
            else if (Dir.Y == 0)
            {
                dis = Math.Abs(p0.Y - P0.Y);
            }
            else
            {
                float a = Dir.Y / Dir.X;
                float c = -P1.X * a + P1.Y;
                float b = -1f;
                float ab = (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
                dis = Math.Abs(a * p0.X + b * p0.Y + c) / ab;
            }
            return dis;
        }
        public PointF nearP(PointF p0)
        {
            PointF p = new PointF();
            if (Dir.X == 0) return new PointF(P0.X, p0.Y);
            if (Dir.Y == 0) return new PointF(p0.X, P0.Y);
            float a = Dir.Y / Dir.X;
            float c = -P1.X * a + P1.Y;
            float b = -1f;
            float ab = (float)(Math.Pow(a, 2) + Math.Pow(b, 2));
            p.X = (b * (b * p0.X - a * p0.Y) - a * c) / ab;
            p.Y = (a * (-b * p0.X + a * p0.Y) - b * c) / ab;
            return p;
        }
        public double dist(PointF p0, PointF p1)
        {
            return Math.Sqrt(Math.Pow(p0.X - p1.X, 2) + Math.Pow(p0.Y - p1.Y, 2));
        }
        public bool inpoint(PointF p0)
        {
            PointF p = nearP(p0);
            double d1 = dist(p, P0);
            double d2 = dist(p, P1);
            double d = dist(P0, P1);
            return (d1 + d2) < (d + 0.001);

        }
        public bool inferCircle(circle c)
        {
            if (c.dist(P0.X, P0.Y) <= c.GetR()) return true;
            if (c.dist(P1.X, P1.Y) <= c.GetR()) return true;
            return inpoint(new PointF((float)c.GetX(), (float)c.GetY()));
        }
    }

    public class Polygon
    {

        private List<PointF> list = new List<System.Drawing.PointF>();
        public Polygon()
        {
        }
        public int pointCount()
        {
            return list.Count;
        }
        public void Add(PointF r) // add a point to polygon
        {
            list.Add(r);
        }
        public PointF getPoint(int i)
        {
            if(i<list.Count)
            {
                return list[i];
            }
            return new PointF();
        }

        public bool CheckAPoint(float x, float y) //Is this point inside Polygon?
        {
            //public boolean insidePolygon(Vector2[] vertices, Vector2 p)

            int i, j;
            bool c = false;
            int nvert = list.Count;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((list[i].Y > y) != (list[j].Y > y)) &&
                    (x < (list[j].X - list[i].X) * (y - list[i].Y) / (list[j].Y - list[i].Y) + list[i].X))
                    c = !c;
            }
            return c;
        }
        public double Area() //Area of polygon
        {
            //https://www.mathwords.com/a/area_convex_polygon.htm
            double r1 = 0, r2 = 0;
            int j = 0;
            for (int i = 0; i < list.Count; i++)
            {
                j = (i + 1) % list.Count;
                r1 += list[i].X * list[j].Y;
                r2 += list[i].Y * list[j].X;
            }
            return Math.Abs(r1 - r2) / 2;
        }
        
        public bool CheckACircle(circle c) //Is this circle inside Polygon?
        {
            float x = (float)c.GetX(), y = (float)c.GetY(), r = (float)c.GetR();

            if (!CheckAPoint(x, y)) return false;
            for (int i = 0; i < list.Count; i++)
            {
                int j = (i + 1) % list.Count;

                PointF p0 = list[i], p1 = list[j];
                Line l = new Line(p0, p1);
                float dist = l.dist(new PointF(x, y));
                if (dist < r)
                {
                    // check points be outside
                    bool res = l.inferCircle(c);
                    if (res) return false;
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (c.dist(list[i].X, list[i].Y) < c.GetR())
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class circle
    {
        private double A;
        private double X;
        private double Y;
        private double R;
        private static int D = 5;

        public circle()
        {
            X = 0;
            Y = 0;
            R = 1;
            A = Math.Round(Math.PI * R * R, D);
        }

        public circle(double x, double y, double r = 1)
        {
            setCircle(x, y, r);
        }
        public circle Clone()
        {
            circle c = new circle(X, Y, R);
            return c;
        }
        
        public double dist(double X0, double Y0) // point to center distance
        {
            double dist = 0;
            dist = (X0 - X) * (X0 - X) + (Y0 - Y) * (Y0 - Y);
            dist = Math.Sqrt(dist);
            return dist;
        }
        public double dist(circle c)// center to center distance
        {
            double dist = 0;
            dist = (c.X - X) * (c.X - X) + (c.Y - Y) * (c.Y - Y);
            dist = Math.Sqrt(dist);
            return dist;
        }
        public bool hit(circle c) // two circle overlap?
        {
            return (R + c.R) > (dist(c) + 1e-5);
        }

        public bool setCircle(double x, double y, double r = 1)
        {
            X = Math.Round(x, D);
            Y = Math.Round(y, D);
            R = Math.Round(r, D);
            A = Math.Round(Math.PI * R * R, D);
            if (double.IsNaN(A))
            {

                A = 0;
            }
            return A > 0;
        }
        public double GetX()
        {
            return X;
        }
        public double GetY()
        {
            return Y;
        }
        public double GetR()
        {
            return R;
        }
        public double GetArea()
        {
            return A;
        }
        public bool isValid()
        {
            return A > 0;
        }
    }

    
}
