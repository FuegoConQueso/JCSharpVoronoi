using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace JCSharpVoronoi
{
    class RectClipper : JCVClipper
    {
        internal RectClipper(RectangleF rect) : base(rect) { }
        
        public override bool TestPoint(PointF point)
        {
            return boundingBox.Contains(point);
        }

        public override bool ClipEdge(JCVEdge edge)
        {
            float pxMin = boundingBox.X;
            float pxMax = boundingBox.Right;
            float pyMin = boundingBox.Y;
            float pyMax = boundingBox.Bottom;

            float x1, y1, x2, y2;
            PointF s1, s2;
            bool s1Null, s2Null;
            s1Null = s2Null = false;
            if (edge.A == 1 && edge.B >= 0)
            {
                if (edge.isValid(1))
                    s1 = edge.Points[1];
                else
                    s1Null = true;
                if (edge.isValid(0))
                    s2 = edge.Points[0];
                else
                    s2Null = true;
            }
            else
            {
                if (edge.isValid(0))
                    s1 = edge.Points[0];
                else
                    s1Null = true;
                if (edge.isValid(1))
                    s2 = edge.Points[1];
                else
                    s2Null = true;
            }

            if (edge.A == 1) //delta x is larger
            {
                y1 = pyMin;
                if (!s1Null && (s1.Y > pyMin))
                {
                    y1 = s1.Y;
                }
                if (y1 > pyMax)
                {
                    y1 = pyMax;
                }
                x1 = edge.C - edge.B * y1;

                y2 = pyMax;
                if (!s2Null && (s2.Y < pyMax))
                {
                    y2 = s2.Y;
                }
                if (y2 < pyMin)
                {
                    y2 = pyMin;
                }
                x2 = edge.C - edge.B * y2;

                // JCash note: condition never occurs according to lcov
                //if ((x1 > pxMax && x2 > pxMax) || (x1 < pxMin && x2 < pxMin))
                //{
                //    return false;
                //}

                if (x1 > pxMax)
                {
                    x1 = pxMax;
                    y1 = (edge.C - x1) / edge.B;
                }
                else if (x1 < pxMin)
                {
                    x1 = pxMin;
                    y1 = (edge.C - x1) / edge.B;
                }
                if (x2 > pxMax)
                {
                    x2 = pxMax;
                    y2 = (edge.C - x2) / edge.B;
                }
                else if (x2 < pxMin)
                {
                    x2 = pxMin;
                    y2 = (edge.C - x2) / edge.B;
                }

            }

            else //delta y is larger
            {
                x1 = pxMin;
                if (!s1Null && (s1.X > pxMin))
                {
                    x1 = s1.X;
                }
                if (x1 > pxMax)
                {
                    x1 = pxMax;
                }
                y1 = edge.C - edge.A * x1;

                x2 = pxMax;
                if (!s2Null && (s2.X < pxMax))
                {
                    x2 = s2.X;
                }
                if (x2 < pxMin)
                {
                    x2 = pxMin;
                }
                y2 = edge.C - edge.A * x2;

                // JCash note: condition never occurs according to lcov
                //if ((y1 > pyMax && y2 > pyMax) || (y1 < pyMin && y2 < pyMin))
                //{
                //    return false;
                //}

                if (y1 > pyMax)
                {
                    y1 = pyMax;
                    x1 = (edge.C - y1) / edge.A;
                }
                else if (y1 < pyMin)
                {
                    y1 = pyMin;
                    x1 = (edge.C - y1) / edge.A;
                }
                if (y2 > pyMax)
                {
                    y2 = pyMax;
                    x2 = (edge.C - y2) / edge.A;
                }
                else if (y2 < pyMin)
                {
                    y2 = pyMin;
                    x2 = (edge.C - y2) / edge.A;
                }
            }

            edge.Points[0].X = x1;
            edge.Points[0].Y = y1;
            edge.Points[1].X = x2;
            edge.Points[1].Y = y2;
            return !(x1 == x2 && y1 == y2); //if points are same, invalid line
        }

        // They're sorted CCW, so if the current->pos[1] != next->pos[0], then we have a gap
        public override bool FillGaps(JCVSite site, JCVDiagram diagram)
        {
            if (site.edges.Count == 0) //no edges, must be single cell graph
            {
                if (diagram.sites.Count != 0)
                {
                    throw new Exception("Invalid graph: edgeless site in graph");
                }

                PointF[] points = new PointF[2];
                points[0] = new PointF(boundingBox.Left, boundingBox.Top);
                points[1] = new PointF(boundingBox.Right, boundingBox.Top);

                JCVEdge edge = new JCVEdge(site, points);
                diagram.edges.Add(edge);
                JCVGraphEdge gap = new JCVGraphEdge(edge, site, null, points);
                site.edges.Add(gap);
            }

            if (site.edges.Count == 1) // one edge, assume corner gap
            {
                JCVGraphEdge current = site.edges[0];

                PointF[] points = new PointF[2];
                points[0] = current.Points[1];
                if (current.Points[1].X < boundingBox.Right && current.Points[1].Y == boundingBox.Top)
                {
                    points[1] = new PointF(boundingBox.Right, boundingBox.Top);
                }
                else if (current.Points[1].X > boundingBox.Left && current.Points[1].Y == boundingBox.Bottom)
                {
                    points[1] = new PointF(boundingBox.Left, boundingBox.Bottom);
                }
                else if (current.Points[1].Y > boundingBox.Top && current.Points[1].X == boundingBox.Left)
                {
                    points[1] = new PointF(boundingBox.Left, boundingBox.Top);
                }
                else if (current.Points[1].Y < boundingBox.Bottom && current.Points[1].X == boundingBox.Right)
                {
                    points[1] = new PointF(boundingBox.Right, boundingBox.Bottom);
                }

                JCVEdge edge = new JCVEdge(site, points);
                diagram.edges.Add(edge);
                JCVGraphEdge gap = new JCVGraphEdge(edge, site, null, points);
                site.edges.Add(gap);
            }

            int eIndex = 0;
            while (eIndex < site.edges.Count)
            {
                JCVGraphEdge current = site.edges[eIndex];
                JCVGraphEdge next = site.edges[(eIndex + 1) % site.edges.Count];
                if (PointIsOnEdge(current.Points[1]) && current.Points[1] != next.Points[0]) {

                    //border gap
                    if (current.Points[1].X == next.Points[0].X || current.Points[1].Y == next.Points[0].Y)
                    {
                        PointF[] points = { current.Points[1], next.Points[0] };
                        JCVEdge edge = new JCVEdge(site, points);
                        diagram.edges.Add(edge);
                        JCVGraphEdge gap = new JCVGraphEdge(edge, site, null, points);

                            // note: performance of repeated insertions may justify switching to linked list or SortedSet structure
                            // for processing phase; List currently used due to ease of use, and fact that it will be as fast or 
                            // faster than other structures when the diagram is later used (post generation)
                        site.edges.Insert(eIndex + 1, gap);
                    }
                    else if (PointIsOnEdge(current.Points[1]) && PointIsOnEdge(next.Points[0]))
                    {
                        PointF[] points = new PointF[2];
                        points[0] = current.Points[1];
                        if(current.Points[1].X < boundingBox.Right && current.Points[1].Y == boundingBox.Top)
                        {
                            points[1] = new PointF(boundingBox.Right, boundingBox.Top);
                        }
                        else if (current.Points[1].X > boundingBox.Left && current.Points[1].Y == boundingBox.Bottom)
                        {
                            points[1] = new PointF(boundingBox.Left, boundingBox.Bottom);
                        }
                        else if(current.Points[1].Y > boundingBox.Top && current.Points[1].X == boundingBox.Left)
                        {
                            points[1] = new PointF(boundingBox.Left, boundingBox.Top);
                        }
                        else if(current.Points[1].Y < boundingBox.Bottom && current.Points[1].X == boundingBox.Right)
                        {
                            points[1] = new PointF(boundingBox.Right, boundingBox.Bottom);
                        }

                        JCVEdge edge = new JCVEdge(site, points);
                        diagram.edges.Add(edge);
                        JCVGraphEdge gap = new JCVGraphEdge(edge, site, null, points);
                        site.edges.Insert(eIndex + 1, gap);
                    }
                    else
                    {
                        //something went wrong; abort instead of looping indefinitely
                        throw new Exception("Invalid Gap state, site location " + site.center.ToString());
                    }
                }
                eIndex++;
            }

            //    current = current->next;
            //    if (current)
            //    {
            //        next = current->next;
            //        if (!next)
            //            next = site->edges;
            //    }
            //}

            ////*********** moved down from top ****************
            //jcv_graphedge* current = site->edges;
            //if (!current)
            //{
            //    // No edges, then it should be a single cell
            //    assert(allocator->numsites == 1);

            //    jcv_graphedge* gap = jcv_alloc_graphedge(allocator);
            //    gap->neighbor = 0;
            //    gap->pos[0] = clipper->min;
            //    gap->pos[1].x = clipper->max.x;
            //    gap->pos[1].y = clipper->min.y;
            //    gap->angle = jcv_calc_sort_metric(site, gap);
            //    gap->next = 0;
            //    gap->edge = jcv_create_gap_edge(allocator, site, gap);

            //    current = gap;
            //    site->edges = gap;
            //}

            //jcv_graphedge* next = current->next;
            //if (!next)
            //{
            //    // Only one edge, then we assume it's a corner gap
            //    jcv_graphedge* gap = jcv_alloc_graphedge(allocator);
            //    jcv_create_corner_edge(allocator, site, current, gap);
            //    gap->edge = jcv_create_gap_edge(allocator, site, gap);

            //    gap->next = current->next;
            //    current->next = gap;
            //    current = gap;
            //    next = site->edges;
            //}
            return false;
        }

        private bool PointIsOnEdge(PointF point)
        {
            return point.X == boundingBox.Left || point.X == boundingBox.Right || point.Y == boundingBox.Top || point.Y == boundingBox.Bottom;
        }
    }
}
