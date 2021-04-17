using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi
{
    public class JCVEdge
    {
        const float JCV_INVALID_VALUE = float.MinValue;

        public PointF[] Points;
        public JCVSite[] Sites;
        public JCVEdge Next;
        public float A, B, C; // line equation: ax + by + c = 0

        public JCVEdge(JCVSite site, PointF[] points)
        {
            Points = points;
            Sites = new JCVSite[2];
            Next = null;
            Sites[0] = site;
            Sites[1] = null;
        }

        public JCVEdge(JCVSite site1, JCVSite site2)
        {
            Points = new PointF[2];
            Sites = new JCVSite[2];
            Next = null;
            Sites[0] = site1;
            Sites[1] = site2;
            Points[0].X = JCV_INVALID_VALUE;
            Points[0].Y = JCV_INVALID_VALUE;
            Points[1].X = JCV_INVALID_VALUE;
            Points[1].Y = JCV_INVALID_VALUE;

            // Create line equation between S1 and S2:
            // float a = -1 * (site2.Y - site1.Y);
            // float b = site2.X - site1.X;
            // //float c = -1 * (site2.X - site1.X) * site1.Y + (site2.Y - site1.Y) * site1.X;
            //
            // // create perpendicular line
            // float pa = b;
            // float pb = -a;
            // //float pc = pa * site1.X + pb * site1.Y;
            //
            // // Move to the mid point
            // float mx = site1.X + dx * float(0.5);
            // float my = site1.Y + dy * float(0.5);
            // float pc = ( pa * mx + pb * my );

            float dx = site2.X - site1.X;
            float dy = site2.Y - site1.Y;
            bool dx_is_larger = (dx * dx) > (dy * dy); // instead of fabs

            // Simplify it, using dx and dy
            C = dx * (site1.X + dx * 0.5f) + dy * (site1.Y + dy * 0.5f);

            if (dx_is_larger)
            {
                A = (float)1;
                B = dy / dx;
                C /= dx;
            }
            else
            {
                A = dx / dy;
                B = (float)1;
                C /= dy;
            }
        }

        public bool isValid(int pointIndex)
        {
            if (Points[pointIndex].X != JCV_INVALID_VALUE || Points[pointIndex].Y != JCV_INVALID_VALUE)
                return true;
            else
                return false;
        }
    }
}
