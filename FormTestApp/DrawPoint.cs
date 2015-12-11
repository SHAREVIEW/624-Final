using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FormTestApp
{
    class DrawPoint
    {
        public double X;
        public double Y;
        public int stroke;
        public uint time;
        public uint pressure;
        public DrawPoint()
        {
            X = 0;
            Y = 0;
            stroke = 0;
            time = 0;
            pressure = 0;
        }

        public DrawPoint(double x, double y)
        {
            X = x;
            Y = y;
            stroke = 0;
        }

        public DrawPoint(double x, double y, int s)
        {
            X = x;
            Y = y;
            stroke = s;
        }

        public DrawPoint(double x, double y, int s, uint t, uint p)
        {
            X = x;
            Y = y;
            stroke = s;
            time = t;
            pressure = p;
        }

        public Point ToPoint()
        {
            return new Point((int)X, (int)Y);
        }
    }
}
