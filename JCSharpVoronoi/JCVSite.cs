using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi
{
    public class JCVSite : IComparable<JCVSite>
    {
        public float X { get { return center.X; } } //might not need X, Y
        public float Y { get { return center.Y; } }
        public PointF center;
        public List<JCVGraphEdge> edges;
        public int index;

        public JCVSite() { }

        public JCVSite(PointF center, int index)
        {
            edges = new List<JCVGraphEdge>();
            this.center = center;
            this.index = index;
        }

        public int CompareTo(JCVSite other)
        {
            return (this.Y != this.Y) ? (this.Y < this.Y ? -1 : 1) : (this.X < this.X ? -1 : 1);
        }
    }
}
