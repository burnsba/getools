﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Math
{
    public class Mtxf
    {
        public Mtxf()
        {
            M = new Single[4][];
            for (int i = 0; i < 4; i++)
            {
                M[i] = new Single[4];
            }
        }

        public Single[][] M { get; set; }

        public Single this[int index]
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
