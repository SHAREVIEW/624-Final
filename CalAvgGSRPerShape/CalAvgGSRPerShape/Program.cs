using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CalAvgGSRPerShape
{
    class ShapeTime
    {
        public uint startTime;
        public uint endTime;
        public int numGsrTimestamps = 0;
        public double avgGsr = 0; 
    }

    class Program
    { 

        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Error! Need a point log and a biometric log to read from as arguments!\nPress enter to exit.");
                var dummy = Console.ReadLine();
                return;
            }

            Dictionary<string, int> shapeCounts = new Dictionary<string, int>();

            string pointFileName = args[0];
            string bioFileName = args[1];

            List<ShapeTime> shapes = new List<ShapeTime>();

            if(!System.IO.File.Exists(pointFileName))
            {
                Console.WriteLine("Error! File {0} does not exist!\nPress enter to exit.",pointFileName);
                var dummy = Console.ReadLine();
                return;
            }


            if (!System.IO.File.Exists(bioFileName))
            {
                Console.WriteLine("Error! File {0} does not exist!\nPress enter to exit.", bioFileName);
                var dummy = Console.ReadLine();
                return;
            }

            using (System.IO.StreamReader input = new System.IO.StreamReader(pointFileName, true))
            {
                string firstline = input.ReadLine();
                while(true)
                {
                    string shapeLine = input.ReadLine();
                    if (shapeLine == null)
                        break;
                    string shape = shapeLine.Split(' ')[2];
                    if (shapeCounts.ContainsKey(shape))
                        shapeCounts[shape]++;
                    else
                        shapeCounts.Add(shape, 1);
                    //Console.WriteLine(shape);
                    string outcome = input.ReadLine();
                    int points = Int32.Parse(input.ReadLine());

                    ShapeTime st = new ShapeTime();
                    
                    for(uint i=0;i< points;i++)
                    {
                        string[] splitStuff = input.ReadLine().Split(',');
                        if(i==0)
                        {
                            st.startTime =UInt32.Parse(splitStuff[4]); //extract unix time
                        }
                        if(i==points-1)
                        {
                            st.endTime = UInt32.Parse(splitStuff[4]);
                        }
                    }
                    shapes.Add(st);
                }
            }

            int numPreGsrTimestamps = 0;
            double avgPreGsr = 0;
            bool preShapes = true;
            
            using (System.IO.StreamReader input = new System.IO.StreamReader(bioFileName, true))
            {
                input.ReadLine();
                int shapeIndex = 0;
                
                while (true)
                {
                    string bioline = input.ReadLine();
                    if (bioline == null)
                        break;
                    uint timestamp = UInt32.Parse(bioline.Split(',')[0]);
                    //correct to utc time
                    timestamp += 6 * 60 * 60;
                    if (shapeIndex < shapes.Count)
                    {
                        Console.WriteLine("GSR Timestamp: {0} \t Shape {1} Start Time: {2} \t Shape {1} End Time: {3}", timestamp, shapeIndex, shapes[shapeIndex].startTime, shapes[shapeIndex].endTime);
                    }
                    else
                    {
                        Console.WriteLine("GSR Timestamp after last timestamp of shapes");
                    }
                    string gsrString = bioline.Split(',')[8];
                    //Console.WriteLine(gsrString);
                    double gsr = Double.Parse(gsrString);
                    if (shapeIndex < shapes.Count)
                    {
                        if (timestamp < shapes[shapeIndex].startTime)
                        {
                            //this should only come up once - before the first shape is drawn
                            //find baseline?
                            avgPreGsr += gsr;
                            numPreGsrTimestamps++;
                        }
                        else if (timestamp >= shapes[shapeIndex].startTime && timestamp <= shapes[shapeIndex].endTime)
                        {
                            if (preShapes)
                            {
                                preShapes = false;
                                if (numPreGsrTimestamps > 0)
                                {
                                    avgPreGsr = avgPreGsr / numPreGsrTimestamps;
                                }
                                else
                                {
                                    avgPreGsr = Double.MinValue;
                                }
                            }
                            //timestamp of gsr reading takes place during the shape we are watching
                            shapes[shapeIndex].avgGsr += gsr;
                            shapes[shapeIndex].numGsrTimestamps++;

                        }
                        else if (timestamp > shapes[shapeIndex].endTime)
                        {
                            //calculate the average of the gsr values for the current shape
                            if (shapes[shapeIndex].numGsrTimestamps > 0)
                            {
                                if (shapes[shapeIndex].numGsrTimestamps > 0)
                                {
                                    shapes[shapeIndex].avgGsr = shapes[shapeIndex].avgGsr / shapes[shapeIndex].numGsrTimestamps;
                                }
                                else
                                {
                                    shapes[shapeIndex].avgGsr = Double.MinValue;
                                }
                                Console.WriteLine("Avg gsr for shape {0} = {1}", shapeIndex, shapes[shapeIndex].avgGsr);
                            }
                            shapeIndex++;
                            //increment through shape indices until we find one the gsr timestamp belongs to
                            while (shapeIndex < shapes.Count && timestamp > shapes[shapeIndex].endTime)
                            {

                                if (timestamp <= shapes[shapeIndex].endTime)
                                {
                                    shapes[shapeIndex].avgGsr += gsr;
                                    shapes[shapeIndex].numGsrTimestamps++;
                                }
                                shapeIndex++;
                            }
                        }
                    }
                }
            }
            for(int i = 0; i < shapes.Count; i++)
            {
                Console.WriteLine("Avg gsr in shape {0} = {1}", i, shapes[i].avgGsr);
            }

            using (System.IO.StreamWriter output = new System.IO.StreamWriter("../../../../Logs/Subject/avgerage-GSR-per-Shape-11.csv", true))
            {
                output.WriteLine("Average Gsr Per Shape, Average Gsr Before Shapes");
                foreach (ShapeTime s in shapes)
                {
                    output.WriteLine("{0},{1}", s.avgGsr, avgPreGsr);
                }
            }

            foreach (KeyValuePair<string, int> kvp in shapeCounts)
            {
                Console.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
            }

            Console.WriteLine("Press enter to continue");
            var tmp = Console.ReadLine();

        }
    }
}
