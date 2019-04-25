using System;
using System.Collections.Generic;
using System.Text;

namespace MapboxNetCore
{
    public class Point2D
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Point2D()
        {
        }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return "Point2D( " + X + ", " + Y + " )";
        }
    }
}
