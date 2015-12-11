using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FormTestApp
{
    class ShapeTemplate
    {
        public int NUM_SHAPES;
        public static int SQUARE = 0;
        public static int CIRCLE = 1;
        public static int TRIANGLE = 2;
        public static int ARROW = 3;
        public static int STAR = 4;
        public static int LINE = 5;
        public static int PENTAGON = 6;
        public static int AT = 7;

        public List<Shape> shapes;

        public ShapeTemplate()
        {
            //initialize all shapes
            shapes = new List<Shape>();
            //Initialize the square shape
            shapes.Add(new Shape("Square"));
            shapes.Add(new Shape("Circle"));
            shapes.Add(new Shape("Triangle"));
            shapes.Add(new Shape("Arrow"));
            shapes.Add(new Shape("Star"));
            shapes.Add(new Shape("Line"));
            shapes.Add(new Shape("Pentagon"));
            shapes.Add(new Shape("@"));


            NUM_SHAPES = shapes.Count;
        }

        public Shape getShape(int n)
        {
            return shapes[n];
        }
    }



    class Shape
    {
        public List<List<DrawPoint>> templates;
        public int numTemplates;
        public string name;
        public int readTemplates;

        public Shape(string n)
        {
            templates = new List<List<DrawPoint>>();
            name = n;
            numTemplates = 0;
            readTemplates = 0;
            LoadTemplates();
        }

        public void LoadTemplates()
        {
            string fileName = String.Format(@"C:\Users\Admin\Documents\GitHub\624-Final\Templates\{0}.txt", name);
            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(fileName, true))
                {
                    while (true)
                    {
                        string pointsStr = file.ReadLine();
                        if (pointsStr == null)
                        {
                            break;
                        }
                        numTemplates++;
                        readTemplates++;
                        int numPoints = Int32.Parse(pointsStr);
                        List<DrawPoint> points = new List<DrawPoint>();

                        for(int i=0;i< numPoints;i++)
                        {
                            string line = file.ReadLine();
                            string[] words = line.Split(',');

                            string x = words[0];
                            string y = words[1];
                            string stroke = words[2];

                            //Console.WriteLine("Shape has {0} points, first is ({1},{2}) stroke {3}",numPoints,x,y,stroke);
                            points.Add(new DrawPoint(Double.Parse(x),Double.Parse(y),Int32.Parse(stroke)));
                        }

                        templates.Add(points);
                    }
                }
                Console.WriteLine(readTemplates+" templates found for shape " + name);
            }
            else
            {
                Console.WriteLine("Warning! No templates found for " + name);
                numTemplates = 0;
            }
        }

        public void setTemplate(int templateNum,List<DrawPoint> p)
        {
            Trace.WriteLine("Starting cloning.");
            templates[templateNum].Clear();
            for (int i=0;i<p.Count();i++)
            {
                templates[templateNum].Add(p[i]);
            }
            Trace.WriteLine("Finished cloning");
        }

        public void addTemplate(List<DrawPoint> p)
        {
            templates.Add(new List<DrawPoint>());
            for(int i=0;i<p.Count;i++)
            {
                templates[numTemplates].Add(p[i]);
            }
            numTemplates++;
        }

        public void clearTemplates()
        {
            templates.Clear();
            numTemplates = 0;
            readTemplates = 0;
        }

        public List<DrawPoint> getTemplate(int templateNum)
        {
            return templates[templateNum];
        }

        public void appendTemplatesToFile()
        {
            Console.WriteLine("Dumping {0} {1} templates to file",numTemplates-readTemplates,name);
            string fileName = String.Format(@"C:\Users\Admin\Documents\GitHub\624-Final\Templates\{0}.txt", name);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                for(int i=readTemplates; i < numTemplates;i++)
                {
                    file.WriteLine(templates[i].Count);
                    for (int j = 0; j < templates[i].Count; j++)
                    {
                        file.WriteLine("{0},{1},{2}",templates[i][j].X,templates[i][j].Y,templates[i][j].stroke);
                    }
                }
            }
            readTemplates = numTemplates;
            Console.WriteLine("done");
        }
    }
}
