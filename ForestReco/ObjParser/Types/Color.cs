using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser.Types
{
    public class Color : IType
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }

	    public Color()
	    {
			this.r = 1;
		    this.g = 1;
		    this.b = 1;
		}

		public Color(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void LoadFromStringArray(string[] data)
        {
            if (data.Length != 4) return;
            r = float.Parse(data[1]);
            g = float.Parse(data[2]);
            b = float.Parse(data[3]);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", r, g, b);
        }
    }
}
