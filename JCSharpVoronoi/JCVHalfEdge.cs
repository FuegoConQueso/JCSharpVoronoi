using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi
{
    public class JCVHalfEdge : IComparable<JCVHalfEdge>
    {
        public JCVSite rSite { 
            get {
                if (directionIsRight)
                    return Edge.Sites[0];
                else
                    return Edge.Sites[1];
            }
        }
        public JCVSite lSite
        {
            get
            {
                if (directionIsRight)
                    return Edge.Sites[1];
                else
                    return Edge.Sites[0];
            }
        }

        public JCVEdge Edge;
        public JCVHalfEdge left;
        public JCVHalfEdge right;
        public PointF vertex;
        public float Y;
        public bool directionIsRight;

        public JCVHalfEdge()
        {
        }
        public JCVHalfEdge(JCVEdge edge, bool directionIsRight, JCVHalfEdge rNeighbor) : this(edge, directionIsRight)
        {
            left = rNeighbor;
            right = rNeighbor.right;
            rNeighbor.right.left = this;
            rNeighbor.right = this;
        }

        public JCVHalfEdge(JCVEdge edge, bool directionIsRight)
        {
            this.Edge = edge;
            left = null;
            right = null;
            this.directionIsRight = directionIsRight;
        }

        public void Unlink()
        {
            left.right = right;
            right.left = left;
        }

        public int CompareTo(JCVHalfEdge other)
        {
            if (this.Y == other.Y && this.vertex.X == other.vertex.X)
                return 0;

            return ((this.Y == other.Y) ? (this.vertex.X < other.vertex.X) : (this.Y < other.Y)) ? -1 : 1;
        }
    }
}
