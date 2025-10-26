using System;

namespace qbook
{
    [Serializable]
    public class Bounds
    {
        public Bounds()
        {

        }
        public Bounds(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public double X = 0;
        public double Y = 0;
        public double W = 10;
        public double H = 10;



        public bool Contains(System.Drawing.PointF p)
        {
            return Contains(p.X, p.Y);
        }

        public bool Contains(double x, double y)
        {
            return ((x >= X) && (x <= (X + W)) && (y >= Y) && (y <= (Y + H)));
        }

        public bool NearHome(double x, double y)
        {
            if (x < (X - 3))
                return false;
            if (x > (X + 8))
                return false;
            if (y < (Y - 3))
                return false;
            if (y > (Y + 8))
                return false;

            //  if (Math.Abs(x - X) > 5)
            //    return false;
            //if (Math.Abs(y - Y) > 5)
            //  return false;

            return true;
        }

        public bool InSettingsBox(double x, double y)
        {
            if (x > (X + W + 3))
                return false;
            if (x < (X + W - 8))
                return false;
            if (y > (Y + H - 10))
                return false;
            if (y < (Y + H - 15))
                return false;
            return true;


        }
        public bool InEditBox(double x, double y)
        {
            if (x > (X + W + 3))
                return false;
            if (x < (X + W - 8))
                return false;
            if (y > (Y + H - 15))
                return false;
            if (y < (Y + H - 20))
                return false;
            return true;
        }

        public bool InMoveBox(double x, double y)
        {
            if (x > (X + W + 3))
                return false;
            if (x < (X + W - 8))
                return false;
            if (y > (Y + H - 5))
                return false;
            if (y < (Y + H - 10))
                return false;
            return true;
        }



        public bool InScaleBox(double x, double y)
        {
            if (x > (X + W + 3))
                return false;
            if (x < (X + W - 8))
                return false;
            if (y > (Y + H))
                return false;
            if (y < (Y + H - 5))
                return false;


            //            if (Math.Abs(x - X - W) > 6)
            //              return false;
            //        if (Math.Abs(y - Y - H) > 6)
            //          return false;

            return true;
        }

        public override string ToString()
        {
            return X + " " + Y + " " + W + " " + H;
        }

        public Bounds Clone()
        {
            return (Bounds)this.MemberwiseClone();
        }

    }
}
