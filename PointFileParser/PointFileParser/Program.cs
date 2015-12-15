using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointFileParser
{
    class Point
    {
        public int x;
        public int y;
        public uint time;
        public int pressure;

        public Point(string X, string Y, string t, string p)
        {
            x = Int32.Parse(X);
            y = Int32.Parse(Y);
            time = UInt32.Parse(t);
            pressure = Int32.Parse(p);
        }
    }

    class Program
    {
        static int X = 1;
        static int Y = 2;
        static int STROKE = 3;
        static int TIME = 4;
        static int PRESSURE = 5;
        static int SET = 6;
        static int SEQ = 7;
        static int SHAPE = 8;

        static double PointDist(Point a, Point b)
        {
            return Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        }

        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Error! Need a file to try and parse.");
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
                return;
            }

            string fileName = args[0];
            if(!System.IO.File.Exists(fileName))
            {
                Console.WriteLine("Cannot find file {0}!",fileName);
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
                return;
            }

            using (System.IO.StreamReader input = new System.IO.StreamReader(fileName,true))
            {
                if(input.ReadLine() == null)
                {
                    Console.WriteLine("Warning! File appears to be empty!");
                    return;
                }

                string fileNum = fileName.Split('-')[1].Split('.')[0];
                string outFile = String.Format("../../../../Logs/Subject/InterpretedLog-{0}.txt", fileNum);
                using (System.IO.StreamWriter output = new System.IO.StreamWriter(outFile, false))
                {
                    output.WriteLine("Set,Seq,Shape,startCos,startSin,DiagLen,DiagAngle,EndDist,EndCos,EndSin,PathLen,TotalRotSum,AbsRotSum,RotSquaredSum,AvgSpeed,TotalTime");
                }

                while (true)
                {
                    string shapeLine = input.ReadLine();
                    if (shapeLine == null)
                        break;
                    string shapeName = shapeLine.Split(' ')[2];
                    string success = input.ReadLine();
                    int numPoints = Int32.Parse(input.ReadLine());

                    if (numPoints < 10)
                    {
                        Console.WriteLine("Warning! A {0} Shape has less than 10 points (has {1}). Discarding shape.", shapeName, numPoints);
                        continue;
                    }

                    List<Point> points = new List<Point>();

                    int set = 0;
                    int seq = 0;
                    int shape = 0;


                    for (int i = 0; i < numPoints; i++)
                    {
                        string[] parts = input.ReadLine().Split(',');
                        set = Int32.Parse(parts[SET]);
                        seq = Int32.Parse(parts[SEQ]);
                        shape = Int32.Parse(parts[SHAPE]);
                        points.Add(new Point(parts[X], parts[Y], parts[TIME], parts[PRESSURE]));
                    }

                    //calculate rubine features

                    //find first point that's not on top of first point (otherwise does odd stuff)
                    int notSame = 0;
                    while (points[notSame].x == points[0].x && points[notSame].y == points[0].y)
                        notSame++;


                    double denominator = PointDist(points[notSame], points[0]);
                    //Console.WriteLine(denominator);
                    double startCos = (points[notSame].x - points[0].x) / denominator;
                    double startSin = (points[notSame].y - points[0].y) / denominator;

                    int xMax, xMin, yMax, yMin;
                    xMax = yMax = Int32.MinValue;
                    yMin = xMin = Int32.MaxValue;

                    double pathLen = 0;
                    double totalRot = 0;
                    double absRot = 0;
                    double rotSquared = 0;

                    for (int i = 0; i < points.Count; i++)
                    {
                        xMin = System.Math.Min(xMin, points[i].x);
                        yMin = System.Math.Min(yMin, points[i].y);
                        xMax = System.Math.Max(xMax, points[i].x);
                        yMax = System.Math.Max(yMax, points[i].y);
                        if (i > 0)
                        {
                            if (points[i].x == points[i - 1].x && points[i].y == points[i - 1].y)
                                continue;
                            pathLen += PointDist(points[i], points[i - 1]);
                            int xDist = points[i].x - points[i - 1].x;
                            int yDist = points[i].y - points[i - 1].y;
                            if (i > 1)
                            {
                                int lastY = points[i - 1].y - points[i - 2].y;
                                int lastX = points[i - 1].x - points[i - 2].x;

                                double rot = Math.Atan((xDist * lastY - yDist * lastX) / (double)(xDist * lastX + yDist * lastY));
                                totalRot += rot;
                                absRot += Math.Abs(rot);
                                rotSquared += rot * rot;
                            }
                        }
                    }


                    double diagLen = Math.Sqrt((xMax - xMin) * (xMax - xMin) + (yMax - yMin) * (yMax - yMin));
                    double diagAngle = Math.Atan(((double)(yMax - yMin)) / (xMax - xMin));
                    double endDist = PointDist(points[numPoints - 1], points[0]);
                    double endCos = (points[numPoints - 1].x - points[0].x) / endDist;
                    double endSin = (points[numPoints - 1].y - points[0].y) / endDist;

                    uint totalTime = points[numPoints - 1].time - points[0].time;
                    double avgSpeed = pathLen / totalTime;
                    if (totalTime == 0)
                        avgSpeed = 0;
                    Console.WriteLine("{0}:{1}:{2}:{16} :: {3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}", set + 1, seq + 1, shape + 1, startCos, startSin, diagLen, diagAngle, endDist, endCos, endSin, pathLen, totalRot, absRot, rotSquared, avgSpeed, totalTime, shapeName);

                    using (System.IO.StreamWriter output = new System.IO.StreamWriter(outFile, true))
                    {
                        output.WriteLine("{0},{1},{2},{16},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}", set + 1, seq + 1, shape + 1, startCos, startSin, diagLen, diagAngle, endDist, endCos, endSin, pathLen, totalRot, absRot, rotSquared, avgSpeed, totalTime, shapeName);
                    }
                }
            }


            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
