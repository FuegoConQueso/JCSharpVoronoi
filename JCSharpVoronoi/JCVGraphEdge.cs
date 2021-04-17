using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi
{
    public class JCVGraphEdge : IComparable<JCVGraphEdge>
    {
        public JCVEdge Edge;
        public JCVSite Home;
        public JCVSite Neighbor;
        public PointF[] Points;
        public float Angle;

        public JCVGraphEdge(JCVEdge e, JCVSite home, JCVSite neighbor, PointF[] points)
        {

            Edge = e;
            Home = home;
            Neighbor = neighbor;
            Points = points;
            Angle = calcAngle(home);

        }


        private float calcAngle(JCVSite site)
        {
            // We take the average of the two points, since we can better distinguish between very small edges
            float x = (Points[0].X + Points[1].X) / 2;
            float y = (Points[0].Y + Points[1].Y) / 2;
            float diffy = y - site.Y;
            float angle = (float)Math.Atan2(diffy, x - site.X);
            if (diffy < 0)
                angle = (float)(angle + 2 * Math.PI);
            return angle;
        }

        public int CompareTo(JCVGraphEdge other)
        {
            return (this.Angle < other.Angle) ? -1 : 1;
        }

    }
}
