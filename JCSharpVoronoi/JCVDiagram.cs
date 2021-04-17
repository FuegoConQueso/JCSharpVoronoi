using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi
{
    public class JCVDiagram
    {
        // fixes rounding errors in determining if lines are parallel.
        private const float EDGE_INTERSECT_THRESHOLD = 1.0e-10F; 

        //properties
        public RectangleF Bounds { get; private set; } //might be superfluous- point to clipper.BoundingBox instead?
        public int SiteCount { get { return sites.Count; } }

        //fields
        public List<JCVSite> sites;
        public List<JCVEdge> edges;
        private JCVHalfEdge beachlineStart;
        private JCVHalfEdge beachlineEnd;
        private JCVHalfEdge lastInserted;
        private SortedSet<JCVHalfEdge> priorityQueue;
        private JCVClipper clipper;

        /// <summary>
        /// default constructor; does not call GenerateDiagram
        /// </summary>
        public JCVDiagram() { }

        /// <summary>
        /// calls GenerateDiagram
        /// </summary>
        /// <param name="points"></param>
        /// <param name="rect"></param>
        public JCVDiagram(ref List<PointF> points, RectangleF rect)
        {
            RectangleF defaultRect = new RectangleF(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            JCVClipper defaultClipper = new RectClipper(defaultRect);

            GenerateDiagram(ref points, rect, defaultClipper);
        }

        /// <summary>
        /// calls GenerateDiagram
        /// </summary>
        /// <param name="points"></param>
        /// <param name="rect"></param>
        /// <param name="clipper"></param>
        public JCVDiagram(ref List<PointF> points, RectangleF rect, JCVClipper clipper)
        {
            GenerateDiagram(ref points, rect, clipper);
        }

        /// <summary>
        /// Generate the voronoi diagram.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="rect"></param>
        /// <param name="clipper"></param>
        public void GenerateDiagram(ref List<PointF> points, RectangleF rect, JCVClipper userClipper)
        {
            beachlineStart = new JCVHalfEdge(null, false);
            beachlineEnd = new JCVHalfEdge(null, false);
            clipper = userClipper;
            priorityQueue = new SortedSet<JCVHalfEdge>();

            beachlineStart.right = beachlineEnd;
            beachlineEnd.left = beachlineStart;

            lastInserted = null; //maybe not needed; added for clarity
            edges = new List<JCVEdge>();

            //create sites for each distinct point, sorted by min(y),min(x)
            sites = points.Distinct().Select((p, index) => new JCVSite(p, index)).OrderBy(s => s.Y).ThenBy(s => s.X).ToList();

            bool noRect = (rect == default);
            if (noRect)
            {
                rect.X = sites.Min(s => s.X);
                rect.Y = sites.Min(s => s.Y);
                rect.Width = sites.Max(s => s.X) - rect.X;
                rect.Height = sites.Max(s => s.Y) - rect.Y;
            }

            //uses clipper to clip any points outside bounds.
            clipper.boundingBox = rect;
            sites = (List<JCVSite>)sites.Where(s => clipper.TestPoint(s.center)).ToList();

            if (noRect)
            {
                // JCash used ceil and floor to adjust the bounding box to int coords- necessary?
                // see: jcv_rect_round(&tmp_rect);
                // Will add later if needed

                //pad bounding box by 10 in all directions
                clipper.boundingBox.Inflate(10, 10);
                rect = clipper.boundingBox;
            }

            this.Bounds = rect;

            int siteIndex = 1; //sites[0] is saved for initial bottom site
            int sCount = sites.Count;

            bool finished = false;
            while (!finished)
            {
                bool pqEmpty = (priorityQueue.Count == 0);
                bool before = true;
                if (!pqEmpty && siteIndex < sCount)
                {
                    JCVHalfEdge he = priorityQueue.ElementAt(0);
                    PointF p = new PointF(he.vertex.X, he.Y);
                    before = IsPointBefore(sites[siteIndex].center, p);
                }


                if (siteIndex < sCount && ( pqEmpty || before))
                {
                    SiteEvent(sites[siteIndex++]);
                }
                else if (!pqEmpty) {
                    CircleEvent();
                }
                else
                {
                    finished = true;
                }
            } //end while

            for (JCVHalfEdge temphe = beachlineStart.right; temphe != beachlineEnd; temphe = temphe.right)
            {
                FinishLine(temphe.Edge);
            }

            FillGaps();
        }

        private void SiteEvent(JCVSite site)
        {
            JCVHalfEdge left = GetEdgeAboveX(site.center);
            JCVHalfEdge right = left.right;
            JCVSite bottom = (left.Edge is null) ? sites[0] : left.rSite;

            JCVEdge edge = new JCVEdge(bottom, site);
            edges.Add(edge);

            JCVHalfEdge he1 = new JCVHalfEdge(edge, false, left);
            JCVHalfEdge he2 = new JCVHalfEdge(edge, true, he1);

            lastInserted = right;

            PointF p;
            if(CheckCircleEvent(left, he1, out p)) {
                priorityQueue.Remove(left);
                left.vertex = p;
                left.Y = p.Y + PointDistance(site.center, p);
                priorityQueue.Add(left);
            }
            if (CheckCircleEvent(he2, right, out p))
            {
                he2.vertex = p;
                he2.Y = p.Y + PointDistance(site.center, p);
                priorityQueue.Add(he2);
            }
        }


        private static bool IsPointBefore(PointF p1, PointF p2)
        {
            return (p1.Y == p2.Y) ? (p1.X < p2.X) : (p1.Y < p2.Y);
        }

        private static float PointDistance(PointF p1, PointF p2)
        {
            float diffx = p1.X - p2.X;
            float diffy = p1.Y - p2.Y;
            return (float)Math.Sqrt(diffx * diffx + diffy * diffy);
        }

        /// <summary>
        /// Gets the arc on the beach line at the x coordinate (i.e. right above the new site event)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private JCVHalfEdge GetEdgeAboveX(PointF p)
        {

            //guess that it's close by to last insert
            JCVHalfEdge he = lastInserted;
            if (he == null)
            {
                //changed to add "+ Bounds.Right"; believe original expression was erroneous
                he = (p.X < Bounds.Width / 2 + Bounds.Right) ? beachlineStart : beachlineEnd;
            }

            //note: could be optimized with binary search
            //then search right or left (as appropriate) until found
            if (he == beachlineStart || (he != beachlineEnd && RightOf(he, p)))
            {
                do
                {
                    he = he.right;
                }
                while (he != beachlineEnd && RightOf(he, p));

                he = he.left;
            }
            else
            {
                do
                {
                    he = he.left;
                }
                while (he != beachlineStart && !RightOf(he, p));

            }

            return he;
        }

        //note to self: this is currently a black box. Figure out how it works at some point
        private bool RightOf(JCVHalfEdge he, PointF p)
        {
            JCVEdge edge = he.Edge;
            JCVSite topsite = edge.Sites[1];

            bool rightOfSite = (p.X > topsite.X);

            if (rightOfSite ^ he.directionIsRight) //direction and rightOfSite don't match
            {
                return rightOfSite;
            }

            float dxp, dyp, dxs, t1, t2, t3, yl;
            bool above;

            if (edge.A == 1)
            {
                dyp = p.Y - topsite.Y;
                dxp = p.X - topsite.X;
                bool fast = false;
                if ((!rightOfSite & (edge.B < 0)) || (rightOfSite & (edge.B >= 0)))
                {
                    above = dyp >= edge.B * dxp;
                    fast = above;
                }
                else
                {
                    above = (p.X + p.Y * edge.B) > edge.C;
                    if (edge.B < 0)
                        above = !above;
                    if (!above)
                        fast = true;
                }
                if (!fast)
                {
                    dxs = topsite.X - edge.Sites[0].X;
                    above = edge.B * (dxp * dxp - dyp * dyp)
                            < dxs * dyp * (1 + 2 * dxp / dxs + edge.B * edge.B);
                    if (edge.B < 0)
                        above = !above;
                }
            }
            else // edge.b == 1
            {
                yl = edge.C - edge.A * p.X;
                t1 = p.Y - yl;
                t2 = p.X - topsite.X;
                t3 = yl - topsite.Y;
                above = t1 * t1 > (t2 * t2 + t3 * t3);
            }
            return he.directionIsRight ^ above;
        }


        private bool CheckCircleEvent(JCVHalfEdge he1, JCVHalfEdge he2, out PointF vertex)
        {
            JCVEdge e1 = he1.Edge;
            JCVEdge e2 = he2.Edge;
            vertex = new PointF();

            if (e1 is null || e2 is null || e1.Sites[1] == e2.Sites[1])
            {
                return false;
            }

            return HalfEdgeIntersection(he1, he2, out vertex);
        }


        private bool HalfEdgeIntersection(JCVHalfEdge he1, JCVHalfEdge he2, out PointF intersect)
        {
            JCVEdge e1 = he1.Edge;
            JCVEdge e2 = he2.Edge;
            intersect = new PointF();

            float d = e1.A * e2.B - e1.B * e2.A; //determinant
            if (-EDGE_INTERSECT_THRESHOLD < d && d < EDGE_INTERSECT_THRESHOLD) // lines are parallel
            {
                return false;
            }

            float xint = (e1.C * e2.B - e1.B * e2.C) / d;
            float yint = (e1.A * e2.C - e1.C * e2.A) / d;

            intersect = new PointF(xint, yint);

            JCVEdge e;
            JCVHalfEdge he;

            if (IsPointBefore(e1.Sites[1].center, e2.Sites[1].center)) {
                he = he1;
                e = e1;
            }
            else
            {
                he = he2;
                e = e2;
            }

            bool rightOfSite = intersect.X >= e.Sites[1].X;
            if (rightOfSite ^ he.directionIsRight)
            {
                return false;
            }
            return true;

        }

        private void CircleEvent()
        {
            JCVHalfEdge left = priorityQueue.First();
            priorityQueue.Remove(left);

            JCVHalfEdge leftleft = left.left;
            JCVHalfEdge right = left.right;
            JCVHalfEdge rightright = right.right;
            JCVSite bottom = left.lSite;
            JCVSite top = right.rSite;

            PointF vertex = left.vertex;
            EndPoints(left.Edge, vertex, left.directionIsRight);
            EndPoints(right.Edge, vertex, right.directionIsRight);

            lastInserted = rightright;

            priorityQueue.Remove(right);
            left.Unlink();
            right.Unlink();

            bool dirIsRight = false;
            if (bottom.Y > top.Y)
            {
                JCVSite temp = bottom;
                bottom = top;
                top = temp;
                dirIsRight = true;
            }

            JCVEdge edge = new JCVEdge(bottom, top);
            edges.Add(edge);

            JCVHalfEdge he = new JCVHalfEdge(edge, dirIsRight, leftleft);
            EndPoints(edge, vertex, !dirIsRight);

            PointF p;
            if (CheckCircleEvent(leftleft, he, out p))
            {
                priorityQueue.Remove(leftleft);
                leftleft.vertex = p;
                leftleft.Y = p.Y + PointDistance(bottom.center, p);
                priorityQueue.Add(leftleft);
            }
            if (CheckCircleEvent(he, rightright, out p))
            {
                he.vertex = p;
                he.Y = p.Y + PointDistance(bottom.center, p);
                priorityQueue.Add(he);
            }
        }

        private void EndPoints(JCVEdge e, PointF p, bool DirectionIsRight)
        {
            int index = (DirectionIsRight) ? 1 : 0;
            e.Points[index] = p;

            if (!e.isValid(1 - index))
            {
                return;
            }
            FinishLine(e);
        }

        private void FinishLine(JCVEdge e)
        {
            if (!clipper.ClipEdge(e))
            {
                return;
            }

            // Make sure the graph edges are CCW
            int flip = TriIsCCW(e.Sites[0].center, e.Points[0], e.Points[1]) ? 0 : 1;

            for (int i = 0; i < 2; i++)
            {
                PointF[] tempPts = new PointF[2];
                JCVSite home = e.Sites[i];
                JCVSite neighbor = e.Sites[1 - i];
                tempPts[flip] = e.Points[i];
                tempPts[1 - flip] = e.Points[1 - i];

                JCVGraphEdge ge = new JCVGraphEdge(e, home, neighbor, tempPts);
                home.edges.Add(ge);
            }
            //        // check that we didn't accidentally add a duplicate (rare), then remove it
            //        if(ge->next && ge->angle == ge->next->angle )
            //        {
            //            if(jcv_point_eq( &ge->pos[0], &ge->next->pos[0] ) && jcv_point_eq( &ge->pos[1], &ge->next->pos[1] ) )
            //            {
            //                ge->next = ge->next->next; // Throw it away, they're so few anyways
            //            }
            //      }
            //    }
        }

        private void FillGaps()
        {
            int numSites = SiteCount;
            for (int i = 0; i < numSites; i++)
            {
                //should probably also eliminate duplicates
                sites[i].edges.Sort();
                clipper.FillGaps(sites[i], this);
            }
        }
        

        // https://cp-algorithms.com/geometry/oriented-triangle-area.html
        private static bool TriIsCCW(PointF a, PointF b, PointF c)
        {
            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0;
        }
    }
}
