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
    }

    class Program
    {
        public StreamReader pointDataReader;
        public StreamReader bioDataReader;
        


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


            foreach (KeyValuePair<string, int> kvp in shapeCounts)
            {
                Console.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
            }

            Console.WriteLine("Press enter to continue");
            var tmp = Console.ReadLine();

        }
    }
}
