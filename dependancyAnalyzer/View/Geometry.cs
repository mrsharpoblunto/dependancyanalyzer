using System;
using System.Collections.Generic;
using System.Text;

namespace dependancyAnalyzer.View
{
    class Geometry
    {
	    public static int YComponent(int len,double dir) {
		    return (int) (len * Math.Cos(dir));
	    }


	    public static int XComponent(int len,double dir) {
		    return (int) (len * Math.Sin(dir));
	    }
    }
}
