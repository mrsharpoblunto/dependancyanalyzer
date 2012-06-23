using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace dependancyAnalyzer.View
{
    internal class NodeRenderer
    {
        private double _radius;
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public double Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public NodeRenderer(string name,double radius)
        {
            _name = name;
            _radius = radius;
        }

        public void Render(Graphics g, double x, double y)
        {
            g.FillEllipse(Brushes.Black, (int)Math.Round(x) - (int)Math.Round(_radius), (int)Math.Round(y) - (int)Math.Round(_radius), (int)Math.Round(_radius * 2), (int)Math.Round(_radius * 2));
            g.DrawString(_name, new Font("Arial", 10), Brushes.Black, (int)Math.Round(x) + (int)Math.Round(_radius * 1.2), (int)Math.Round(y) -(int)Math.Round(_radius));
        }
    }
}
