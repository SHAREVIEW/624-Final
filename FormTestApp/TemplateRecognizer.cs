using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace FormTestApp
{
    class TemplateRecognizer
    {
        private List<DrawPoint> points;
        //private double TaniThreshold = .8;
        //private double HausdorffThreshold = 20;
        private double PDollarThreshold = 1.25;
        string fileName;

        public TemplateRecognizer()
        {
            points = new List<DrawPoint>();
            int iter = 0;
            do
            {
                fileName = String.Format(@"C:\Users\Admin\Documents\GitHub\624-Final\Logs\PointLog-{0}.txt", iter);
                iter++;
            } while (System.IO.File.Exists(fileName));

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                file.WriteLine("#,(X,Y),stoke#,time,pressure");
            }
        }

        public void AddStartPoint(DrawPoint p)
        {
            points.Add(p);
        }

        //Adds the point to the current list of points
        public void AddPoint(DrawPoint p)
        {
            points.Add(p);
        }

        private double PointDist(DrawPoint a, DrawPoint b)
        {
            double delX = a.X - b.X;
            double delY = a.Y - b.Y;
            return Math.Sqrt(delX * delX + delY * delY);
        }

        private double pathLength(List<DrawPoint> p)
        {
            double dist = 0;
            for(int i=1;i<p.Count;i++)
            {
                if (p[i].stroke == p[i - 1].stroke)
                {
                    double delX = p[i].X - p[i - 1].X;
                    double delY = p[i].Y - p[i - 1].Y;
                    dist += Math.Sqrt(delX * delX + delY * delY);
                }
            }
            return dist;
        }

        //Returns a resampled list of points to fit the template format.
        private List<DrawPoint> ResamplePoints(List<DrawPoint> givenPoints, int numPoints)
        {
            double incrementLength = pathLength(givenPoints) / (numPoints);
            //Console.WriteLine("pathLen: " + pathLength(givenPoints) + " incrementLen: " + incrementLength);
            double distSum = 0;
            List<DrawPoint> newPoints = new List<DrawPoint>();
            newPoints.Add(new DrawPoint(givenPoints[0].X,givenPoints[0].Y));
            DrawPoint lastPoint = new DrawPoint(0, 0);
            for (int i=1;i< givenPoints.Count();i++)
            {
                //Console.WriteLine("i is " + i+", lastPoint.X="+lastPoint.X+" size is "+newPoints.Count);
                if (lastPoint.X != 0 || lastPoint.Y != 0)
                {
                    double lastDist = PointDist(lastPoint, givenPoints[i]);
                    if (lastPoint.stroke == givenPoints[i].stroke)
                    {
                        if (distSum + lastDist >= incrementLength)
                        {
                            double perc = (incrementLength - distSum) / lastDist;
                            double newX = lastPoint.X;
                            newX += (perc * (givenPoints[i].X - lastPoint.X));
                            double newY = lastPoint.Y;
                            newY += (perc * (givenPoints[i].Y - lastPoint.Y));
                            newPoints.Add(new DrawPoint(newX, newY));
                            lastPoint.X = newX;
                            lastPoint.Y = newY;
                            lastPoint.stroke = givenPoints[i].stroke;
                            distSum = 0;
                            i--;
                        }
                        else
                        {
                            distSum += lastDist;
                            lastPoint.X = 0;
                            lastPoint.Y = 0;
                        }
                    }
                    else
                    {
                        lastPoint.X = 0;
                        lastPoint.Y = 0;
                    }
                }
                else
                {
                    double dist = PointDist(givenPoints[i - 1], givenPoints[i]);
                    //Console.WriteLine("dist from (" + givenPoints[i - 1].X + "," + givenPoints[i - 1].Y + ") to (" + givenPoints[i].X + "," + givenPoints[i].Y + ") is " + dist + ". distSum is "+distSum);
                    //exclusion is meant to ignore multistroke jumps
                    if (givenPoints[i - 1].stroke == givenPoints[i].stroke)
                    {
                        if (distSum + dist >= incrementLength)
                        {

                            double perc = (incrementLength - distSum) / dist;
                            double newX = givenPoints[i - 1].X;
                            newX += (perc * (givenPoints[i].X - givenPoints[i - 1].X));
                            double newY = givenPoints[i - 1].Y;
                            newY += (perc * (givenPoints[i].Y - givenPoints[i - 1].Y));
                            newPoints.Add(new DrawPoint(newX, newY));
                            //givenPoints.Insert(i,new Point(newX, newY)); //the new point should be the NEXT selected point
                            //Console.WriteLine("dump new point ("+newX+","+newY+")");
                            lastPoint.X = newX;
                            lastPoint.Y = newY;
                            lastPoint.stroke = givenPoints[i].stroke;
                            distSum = 0;
                            i--;
                        }
                        else
                        {
                            distSum += dist;
                            //Console.WriteLine("new distSum is " + distSum);
                        }
                    }
                    else
                    {
                        Console.WriteLine("skip\n");
                    }
                }
            }
            //Console.WriteLine("thing has " + newPoints.Count);
            return newPoints;
        }

        //Scales the given list of points to percentages of the greater of width/height
        private void Scale(List<DrawPoint> givenPoints)
        {
            double xmin, ymin,xmax,ymax,scale;
            xmin = ymin = Int32.MaxValue;
            xmax = ymax = 0;
            foreach (DrawPoint p in givenPoints)
            {
                xmin = Math.Min(xmin, p.X);
                ymin = Math.Min(ymin, p.Y);
                xmax = Math.Max(xmax, p.X);
                ymax = Math.Max(ymax, p.Y);
            }
            scale = Math.Max(xmax - xmin, ymax - ymin);
            for(int i = 0;i< givenPoints.Count;i++)
            {
                //Console.WriteLine("before: ("+givenPoints[i].X+","+ givenPoints[i].Y+")");
                DrawPoint p = givenPoints[i];
                p.X = (givenPoints[i].X - xmin) / scale;
                p.Y = (givenPoints[i].Y - ymin) / scale;
                givenPoints[i] = p;
                
                //Console.WriteLine("after: (" + givenPoints[i].X + "," + givenPoints[i].Y + ")");
            }
        }

        //Translates the centroid of the shape to the origin
        private void TranslatePointsToOrigin(List<DrawPoint> givenPoints, int numPoints)
        {
            DrawPoint cent = new DrawPoint(0,0);
            foreach(DrawPoint p in givenPoints)
            {
                cent.X += p.X;
                cent.Y += p.Y;
            }
            //Console.WriteLine("cent before div (" + cent.X + "," + cent.Y + ")");
            cent.X /= numPoints;
            cent.Y /= numPoints;
            //Console.WriteLine("cent before (" + cent.X + "," + cent.Y + ")");
            for(int i=0;i<givenPoints.Count;i++)
            {
                DrawPoint p = givenPoints[i];
                p.X -= cent.X;
                p.Y -= cent.Y;
                givenPoints[i] = p;
            }
            
            /*
            cent.X = 0;
            cent.Y = 0;
            foreach (DrawPoint p in givenPoints)
            {
                cent.X += p.X;
                cent.Y += p.Y;
            }
            cent.X /= numPoints;
            cent.Y /= numPoints;
            Console.WriteLine("cent after (" + cent.X + "," + cent.Y + ")");
            */
            
        }

        //Rescale all points to be in a squareWidth by squareWidth box
        //Can support uniform or nonuniform scaling.
        private List<DrawPoint> RescalePoints(List<DrawPoint> p, int squareWidth, bool uniformScale = false)
        {
            List<DrawPoint> rescaled = new List<DrawPoint>();

            if(uniformScale)
            {
                Trace.WriteLine("WARNING: ASKED FOR UNIFORM SCALING, WHICH IS NOT IMPLEMENTED.");
                
            }
            else
            {

                double minX = p[0].X;
                double minY = p[0].Y;
                double maxX = p[0].X;
                double maxY = p[0].Y;

                for (int i = 1; i < p.Count; i++)
                {
                    double x = p[i].X;
                    double y = p[i].Y;

                    if (x < minX)
                        minX = x;
                    if (y < minY)
                        minY = y;
                    if (x > maxX)
                        maxX = x;
                    if (y > maxY)
                        maxY = y;
                }
                double xBound = maxX - minX;
                double yBound = maxY - minY;
                
                for(int i=0;i<p.Count;i++)
                {

                    double newX = (squareWidth * (points[i].X - minX) / xBound);
                    double newY = (squareWidth * (points[i].Y - minY) / yBound);

                    rescaled.Add(new DrawPoint(newX, newY));
                }



            }

            return rescaled;
        }

        //Uses the OneDollarRecognizer to analyze the shape
        private bool OneDollarRecognizer(Shape s)
        {
            //Resample
            

            //Rotate based on "Indicative Angle"


            //Scale and translate


            //Find optimal angle for best score

            return false;
        }

        private double OneWayHaus(List<DrawPoint> first, List<DrawPoint> second)
        {
            return 0;
        }

        //Returns the Hausdorff distance
        private double Hausdorff(Shape s)
        {
            List<DrawPoint> sketchResampled = RescalePoints(ResamplePoints(points,128),40);

            double bestHaus = Double.MaxValue;

            for(int i=0;i<s.numTemplates;i++)
            {
                List<DrawPoint> tempResampled = RescalePoints(ResamplePoints(points, 128), 40);

                double thisHaus = Math.Max(OneWayHaus(sketchResampled,tempResampled),OneWayHaus(tempResampled,sketchResampled));

                if (thisHaus < bestHaus)
                    bestHaus = thisHaus;
            }

            return bestHaus;
        }

        private List<List<bool>> CalculateTaniGrid(List<DrawPoint> p,int gridLen)
        {
            List<List<bool>> grid = new List<List<bool>>();

            for (int i = 0; i < gridLen; i++)
            {
                grid.Add(new List<bool>());
                for (int j = 0; j < gridLen; j++)
                    grid[i].Add(false);
            }



            double minX = p[0].X;
            double minY = p[0].Y;
            double maxX = p[0].X;
            double maxY = p[0].Y;

            

            for (int i = 1; i < p.Count(); i++)
            {
                double x = p[i].X;
                double y = p[i].Y;

                if (x < minX)
                    minX = x;
                if (y < minY)
                    minY = y;

                if (x > maxX)
                    maxX = x;
                if (y > maxY)
                    maxY = y;
            }

            //Trace.WriteLine("minX: " + minX + " maxX: " + maxX + " minY: " + minY + " maxY: " + maxY);

            double xBound = maxX - minX;
            double yBound = maxY - minY;

            

            double xInc = xBound / gridLen;
            double yInc = yBound / gridLen;

            //Trace.WriteLine("xBound: " + xBound + " yBound: " + yBound + " xInc: " + xInc + " yInc: " + yInc);

            for (int i = 0; i < p.Count(); i++)
            {
                double xOff = p[i].X - minX;
                double yOff = p[i].Y - minY;


                int xGrid = (int)(xOff / xInc);
                int yGrid = (int)(yOff / yInc);

                if (xGrid == gridLen)
                    xGrid--;
                if (yGrid == gridLen)
                    yGrid--;
                if(!grid[xGrid][yGrid])
                    Trace.WriteLine("Grid[" + xGrid + "][" + yGrid + "] is true");
                grid[xGrid][yGrid] = true;
            }

            Trace.WriteLine("Finished grid.");

            return grid;
        }

        //Returns the Tanimoto coefficient
        private double Tanimoto(Shape s)
        {
            Trace.WriteLine("Starting Tanimoto");
            int gridLen = 10;

            Trace.WriteLine("Sketch grid:");
            List<List<bool>> sketchGrid = CalculateTaniGrid(points, gridLen);



            double maxScore = 0;

            for(int i=0;i<s.numTemplates;i++)
            {
                Trace.WriteLine("Starting template " + i);
                List<List<bool>> templateGrid = CalculateTaniGrid(s.templates[i], gridLen);
                Trace.WriteLine("Finished grid " + i);
                int union = 0;

                int sketchTrue = 0;
                int tempTrue = 0;

                for(int j=0;j< gridLen;j++)
                    for(int k=0;k< gridLen;k++)
                    {
                        if (sketchGrid[j][k] && templateGrid[j][k])
                        {
                            union++;
                            Trace.WriteLine("sketchGrid[" + j + "][" + k + "] == templateGrid of same.");
                        }
                        if (sketchGrid[j][k])
                            sketchTrue++;
                        if (templateGrid[j][k])
                            tempTrue++;
                    }
                double score = (double)union / (sketchTrue + tempTrue - union);
                Trace.WriteLine("Union is " + union);
                Trace.WriteLine("Bottom is " + (sketchTrue + tempTrue - union));
                Trace.WriteLine("Template "+i+" has a score of "+score);
                if (score > maxScore)
                    maxScore = score;
            }

            return maxScore;
        }

        private List<DrawPoint> normalize(List<DrawPoint> p, int numPoints)
        {
            List<DrawPoint> newPoints = ResamplePoints(p, numPoints);
            //Console.WriteLine("newPoints has " + newPoints.Count);
            Scale(newPoints);
            TranslatePointsToOrigin(newPoints, numPoints);
            return newPoints;
        }

        private double CloudDistance(List<DrawPoint> p1, List<DrawPoint> p2, int numPoints, int start)
        {
            List<bool> matched = new List<bool>();
            for (int ttt = 0; ttt < numPoints; ttt++)
                matched.Add(false);
            double sum = 0;
            int i = start;
            int index = 0;
            //Console.WriteLine("size1: " + p1.Count + " size2: " + p2.Count);
            do
            {
                //Console.WriteLine("i is " + i);
                double minDist = Double.MaxValue;
                //find closest point to i that is not matched already
                for(int j=0;j< numPoints;j++)
                {
                    //Console.WriteLine("j is " + j);
                    if (!matched[j])
                    {
                        //Console.WriteLine("inside loop ("+i+","+j+")");
                        double d = PointDist(p1[i], p2[j]);
                        //Console.WriteLine("dist is " + d);
                        if (d < minDist)
                        {
                            minDist = d;
                            index = j;
                        }
                    }
                }
                //Console.WriteLine("index is " + index);
                matched[index] = true;
                double weight = 1.0 - (double)((i - start + numPoints) % numPoints) / numPoints;
                sum += weight * minDist;
                i = (i + 1) % numPoints;
            } while (i != start);

            return sum;
        }


        private double GreedyCloudMatch(List<DrawPoint> givenPoints,List<DrawPoint> template, int numPoints)
        {
            double epsilon = 0.5;
            int step = (int)Math.Floor(Math.Pow(numPoints, 1 - epsilon));
            double min = Double.MaxValue;

            for(int i = 0;i < (numPoints - 1); i += step)
            {
                //Console.WriteLine("i is "+i);
                double d1 = CloudDistance(givenPoints, template, numPoints, i);
                //Console.WriteLine("finished first cloud");
                double d2 = CloudDistance(template, givenPoints, numPoints, i);
                //Console.WriteLine("finished second cloud");
                //Console.WriteLine("min: "+min+", d1: " + d1 + ", d2: " + d2);
                min = Math.Min(min, Math.Min(d1, d2));
            }

            return min;
        }

        private double PDollar(List<DrawPoint> givenPoints, Shape s)
        {
            double score = Double.MaxValue;
            int n = 32;
            Console.WriteLine("norm points");
            List<DrawPoint> normPoints = normalize(givenPoints, n);
            Console.WriteLine("normPoints has " + normPoints.Count);

            //Console.WriteLine("attempt template 1");
            for (int i=0;i<s.numTemplates;i++)
            {
                Console.WriteLine("norm template "+i);
                List<DrawPoint> normTemplate = normalize(s.getTemplate(i),n);
                Console.WriteLine("normTemplate{0} has {1}",i, normTemplate.Count);
                
                //Console.WriteLine("start cloudmatch");
                double d = GreedyCloudMatch(normPoints, normTemplate, n);
                if(score > d)
                    score = d;
            }
            

            return score;
        }


        //Classifies the current list of points as either the given shape template or not.
        public bool TryRecognizeShape(Shape s)
        {

            if (points.Count == 0)
                return false;

            //bool success = OneDollarRecognizer(s);
            //Console.WriteLine("attempt $p");
            double val = PDollar(points, s);
            Console.WriteLine("p$ val is "+val);
            bool success = val < PDollarThreshold;
            
            return success;
        }

        public List<DrawPoint> getPoints()
        {
            return points;
        }

        public void logPoints(Shape currentShape, bool success)
        {
            // Append new text to an existing file.
            // The using statement automatically flushes AND CLOSES the stream and calls 
            // IDisposable.Dispose on the stream object.
            Console.WriteLine("Logging points...");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                file.WriteLine("Intended Shape: {0}", currentShape.name);
                if (success)
                {
                    file.WriteLine("Success");
                }
                else
                {
                    file.WriteLine("Failure");
                }
                for (int i = 0; i < points.Count; i++)
                {
                    file.WriteLine("{0},{1},{2},{3},{4},{5}", i, points[i].X, points[i].Y, points[i].stroke, points[i].time, points[i].pressure);
                }
                file.WriteLine("!!");
            }
            Console.WriteLine("Done logging points.");
        }
        

        public void ResetPointsNoLog()
        {
            points.Clear();
        }

        public void ResetPoints(Shape currentShape, bool success)
        {
            logPoints(currentShape, success);
            points.Clear();
        }


    }
}
