using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace dependancyAnalyzer.View
{
    internal class LineRenderer
    {
        private double _radius;

        public double Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public LineRenderer(double radius)
        {
            _radius = radius;   
        }

        public void Render(Graphics g, double x1, double y1, double x2, double y2)
        {
            g.DrawLine(new Pen(Brushes.Black, 1)
             , (int)Math.Round(x1)
             , (int)Math.Round(y1)
             , (int)Math.Round(x2)
             , (int)Math.Round(y2)
            );

            double aDir = Math.Atan2(x2 - x1, y2 - y1);
            double arrowX = x2 - Geometry.XComponent((int)Math.Round(_radius), aDir);
            double arrowY = y2 - Geometry.YComponent((int)Math.Round(_radius), aDir); ;

            g.DrawPolygon(new Pen(Brushes.Black, 1),new Point[]
            {
                new Point((int)Math.Round(arrowX),(int)Math.Round(arrowY)),
                new Point((int)Math.Round(arrowX - Geometry.XComponent(15, aDir + .5)),(int)Math.Round(arrowY - Geometry.YComponent(15, aDir + .5))),
                new Point((int)Math.Round(arrowX - Geometry.XComponent(15, aDir - .5)),(int)Math.Round(arrowY - Geometry.YComponent(15, aDir - .5)))
            });
        }
    }
}
