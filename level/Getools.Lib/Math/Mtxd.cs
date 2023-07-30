using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Math
{
    public class Mtxd
    {
        public Mtxd()
        {
            M = new double[4][];
            for (int i = 0; i < 4; i++)
            {
                M[i] = new double[4];
            }
        }

        public double[][] M { get; set; }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index > 15)
                {
                    throw new IndexOutOfRangeException();
                }

                int col = 0;
                while (index > 3)
                {
                    col++;
                    index -= 4;
                }

                return M[col][index];
            }

            set
            {
                if (index < 0 || index > 15)
                {
                    throw new IndexOutOfRangeException();
                }

                int col = 0;
                while (index > 3)
                {
                    col++;
                    index -= 4;
                }

                M[col][index] = value;
            }
        }
    }
}
