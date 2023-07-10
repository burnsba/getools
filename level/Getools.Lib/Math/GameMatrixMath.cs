using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Math
{
    public static class GameMatrixMath
    {
        public static void matrix_4x4_7F059708(Mtxf arg0, Single arg1, Single arg2, Single arg3, Single arg4, Single arg5, Single arg6, Single arg7, Single arg8, Single arg9)
        {
            Single temp_f26;
            Single temp_f28;
            Single temp_f2_2;
            Single temp_f2_3;
            Single temp_f30;
            Single temp_f2 = -1.0f / (Single)System.Math.Sqrt((arg4 * arg4) + (arg5 * arg5) + (arg6 * arg6));
            arg4 *= temp_f2;
            arg5 *= temp_f2;
            arg6 *= temp_f2;
            temp_f26 = (arg8 * arg6) - (arg9 * arg5);
            temp_f28 = (arg9 * arg4) - (arg7 * arg6);
            temp_f30 = (arg7 * arg5) - (arg8 * arg4);
            temp_f2_2 = 1.0f / (Single)System.Math.Sqrt((temp_f26 * temp_f26) + (temp_f28 * temp_f28) + (temp_f30 * temp_f30));
            temp_f26 *= temp_f2_2;
            temp_f28 *= temp_f2_2;
            temp_f30 *= temp_f2_2;
            arg7 = (arg5 * temp_f30) - (arg6 * temp_f28);
            arg8 = (arg6 * temp_f26) - (arg4 * temp_f30);
            arg9 = (arg4 * temp_f28) - (arg5 * temp_f26);
            temp_f2_3 = 1.0f / (Single)System.Math.Sqrt((arg7 * arg7) + (arg8 * arg8) + (arg9 * arg9));
            arg7 *= temp_f2_3;
            arg8 *= temp_f2_3;
            arg9 *= temp_f2_3;
            arg0.M[0][0] = temp_f26;
            arg0.M[1][0] = arg7;
            arg0.M[2][0] = arg4;
            arg0.M[3][0] = arg1;
            arg0.M[0][1] = temp_f28;
            arg0.M[1][1] = arg8;
            arg0.M[2][1] = arg5;
            arg0.M[3][1] = arg2;
            arg0.M[0][2] = temp_f30;
            arg0.M[1][2] = arg9;
            arg0.M[2][2] = arg6;
            arg0.M[3][2] = arg3;
            arg0.M[0][3] = 0.0f;
            arg0.M[1][3] = 0.0f;
            arg0.M[2][3] = 0.0f;
            arg0.M[3][3] = 1.0f;
        }

        public static void matrix_4x4_7F059708(Mtxd arg0, double arg1, double arg2, double arg3, double arg4, double arg5, double arg6, double arg7, double arg8, double arg9)
        {
            double temp_f26;
            double temp_f28;
            double temp_f2_2;
            double temp_f2_3;
            double temp_f30;
            double temp_f2 = -1.0f / System.Math.Sqrt((arg4 * arg4) + (arg5 * arg5) + (arg6 * arg6));
            arg4 *= temp_f2;
            arg5 *= temp_f2;
            arg6 *= temp_f2;
            temp_f26 = (arg8 * arg6) - (arg9 * arg5);
            temp_f28 = (arg9 * arg4) - (arg7 * arg6);
            temp_f30 = (arg7 * arg5) - (arg8 * arg4);
            temp_f2_2 = 1.0f / System.Math.Sqrt((temp_f26 * temp_f26) + (temp_f28 * temp_f28) + (temp_f30 * temp_f30));
            temp_f26 *= temp_f2_2;
            temp_f28 *= temp_f2_2;
            temp_f30 *= temp_f2_2;
            arg7 = (arg5 * temp_f30) - (arg6 * temp_f28);
            arg8 = (arg6 * temp_f26) - (arg4 * temp_f30);
            arg9 = (arg4 * temp_f28) - (arg5 * temp_f26);
            temp_f2_3 = 1.0f / System.Math.Sqrt((arg7 * arg7) + (arg8 * arg8) + (arg9 * arg9));
            arg7 *= temp_f2_3;
            arg8 *= temp_f2_3;
            arg9 *= temp_f2_3;
            arg0.M[0][0] = temp_f26;
            arg0.M[1][0] = arg7;
            arg0.M[2][0] = arg4;
            arg0.M[3][0] = arg1;
            arg0.M[0][1] = temp_f28;
            arg0.M[1][1] = arg8;
            arg0.M[2][1] = arg5;
            arg0.M[3][1] = arg2;
            arg0.M[0][2] = temp_f30;
            arg0.M[1][2] = arg9;
            arg0.M[2][2] = arg6;
            arg0.M[3][2] = arg3;
            arg0.M[0][3] = 0.0f;
            arg0.M[1][3] = 0.0f;
            arg0.M[2][3] = 0.0f;
            arg0.M[3][3] = 1.0f;
        }

        public static void matrix_4x4_7F059908(Mtxf arg0, Single arg1, Single arg2, Single arg3, Single arg4, Single arg5, Single arg6, Single arg7, Single arg8, Single arg9)
        {
            matrix_4x4_7F059708(arg0, arg1, arg2, arg3, arg4 - arg1, arg5 - arg2, arg6 - arg3, arg7, arg8, arg9);
        }

        public static void matrix_4x4_7F059908(Mtxd arg0, double arg1, double arg2, double arg3, double arg4, double arg5, double arg6, double arg7, double arg8, double arg9)
        {
            matrix_4x4_7F059708(arg0, arg1, arg2, arg3, arg4 - arg1, arg5 - arg2, arg6 - arg3, arg7, arg8, arg9);
        }

        public static void matrix_4x4_set_rotation_around_x(Single angle, Mtxf matrix)
        {
            Single cosine = (Single)System.Math.Cos(angle);
            Single sine = (Single)System.Math.Sin(angle);
            matrix.M[0][0] = 1.0f;
            matrix.M[0][1] = 0.0f;
            matrix.M[0][2] = 0.0f;
            matrix.M[0][3] = 0.0f;
            matrix.M[1][0] = 0.0f;
            matrix.M[1][1] = cosine;
            matrix.M[1][2] = sine;
            matrix.M[1][3] = 0.0f;
            matrix.M[2][0] = 0.0f;
            matrix.M[2][1] = -sine;
            matrix.M[2][2] = cosine;
            matrix.M[2][3] = 0.0f;
            matrix.M[3][0] = 0.0f;
            matrix.M[3][1] = 0.0f;
            matrix.M[3][2] = 0.0f;
            matrix.M[3][3] = 1.0f;
        }

        public static void matrix_4x4_set_rotation_around_x(double angle, Mtxd matrix)
        {
            double cosine = System.Math.Cos(angle);
            double sine = System.Math.Sin(angle);
            matrix.M[0][0] = 1.0f;
            matrix.M[0][1] = 0.0f;
            matrix.M[0][2] = 0.0f;
            matrix.M[0][3] = 0.0f;
            matrix.M[1][0] = 0.0f;
            matrix.M[1][1] = cosine;
            matrix.M[1][2] = sine;
            matrix.M[1][3] = 0.0f;
            matrix.M[2][0] = 0.0f;
            matrix.M[2][1] = -sine;
            matrix.M[2][2] = cosine;
            matrix.M[2][3] = 0.0f;
            matrix.M[3][0] = 0.0f;
            matrix.M[3][1] = 0.0f;
            matrix.M[3][2] = 0.0f;
            matrix.M[3][3] = 1.0f;
        }

        public static void matrix_4x4_set_rotation_around_z(Single angle, Mtxf matrix)
        {
            Single cosine = (Single)System.Math.Cos(angle);
            Single sine = (Single)System.Math.Sin(angle);
            matrix.M[0][0] = cosine;
            matrix.M[0][1] = sine;
            matrix.M[0][2] = 0.0f;
            matrix.M[0][3] = 0.0f;
            matrix.M[1][0] = -sine;
            matrix.M[1][1] = cosine;
            matrix.M[1][2] = 0.0f;
            matrix.M[1][3] = 0.0f;
            matrix.M[2][0] = 0.0f;
            matrix.M[2][1] = 0.0f;
            matrix.M[2][2] = 1.0f;
            matrix.M[2][3] = 0.0f;
            matrix.M[3][0] = 0.0f;
            matrix.M[3][1] = 0.0f;
            matrix.M[3][2] = 0.0f;
            matrix.M[3][3] = 1.0f;
        }

        public static void matrix_4x4_set_rotation_around_z(double angle, Mtxd matrix)
        {
            double cosine = System.Math.Cos(angle);
            double sine = System.Math.Sin(angle);
            matrix.M[0][0] = cosine;
            matrix.M[0][1] = sine;
            matrix.M[0][2] = 0.0f;
            matrix.M[0][3] = 0.0f;
            matrix.M[1][0] = -sine;
            matrix.M[1][1] = cosine;
            matrix.M[1][2] = 0.0f;
            matrix.M[1][3] = 0.0f;
            matrix.M[2][0] = 0.0f;
            matrix.M[2][1] = 0.0f;
            matrix.M[2][2] = 1.0f;
            matrix.M[2][3] = 0.0f;
            matrix.M[3][0] = 0.0f;
            matrix.M[3][1] = 0.0f;
            matrix.M[3][2] = 0.0f;
            matrix.M[3][3] = 1.0f;
        }

        public static void matrix_4x4_multiply_in_place(Mtxf lhs, Mtxf rhs)
        {
            Mtxf result = new Mtxf();
            matrix_4x4_multiply(lhs, rhs, result);
            matrix_4x4_copy(result, rhs);
        }

        public static void matrix_4x4_multiply_in_place(Mtxd lhs, Mtxd rhs)
        {
            Mtxd result = new Mtxd();
            matrix_4x4_multiply(lhs, rhs, result);
            matrix_4x4_copy(result, rhs);
        }

        public static void matrix_4x4_multiply(Mtxf lhs, Mtxf rhs, Mtxf result)
        {
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    result.M[j][i] = (lhs.M[0][i] * rhs.M[j][0]) + (lhs.M[1][i] * rhs.M[j][1]) + (lhs.M[2][i] * rhs.M[j][2]) + (lhs.M[3][i] * rhs.M[j][3]);
                }
            }
        }

        public static void matrix_4x4_multiply(Mtxd lhs, Mtxd rhs, Mtxd result)
        {
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    result.M[j][i] = (lhs.M[0][i] * rhs.M[j][0]) + (lhs.M[1][i] * rhs.M[j][1]) + (lhs.M[2][i] * rhs.M[j][2]) + (lhs.M[3][i] * rhs.M[j][3]);
                }
            }
        }

        public static void matrix_4x4_copy(Mtxf src, Mtxf dst)
        {
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    dst.M[i][j] = src.M[i][j];
                }
            }
        }

        public static void matrix_4x4_copy(Mtxd src, Mtxd dst)
        {
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    dst.M[i][j] = src.M[i][j];
                }
            }
        }

        public static void matrix_column_1_scalar_multiply(Single scalar, Mtxf matrix)
        {
            matrix[0] *= scalar;
            matrix[1] *= scalar;
            matrix[2] *= scalar;
        }

        public static void matrix_column_1_scalar_multiply(double scalar, Mtxd matrix)
        {
            matrix[0] *= scalar;
            matrix[1] *= scalar;
            matrix[2] *= scalar;
        }

        public static void matrix_column_2_scalar_multiply(Single scalar, Mtxf matrix)
        {
            matrix[4] *= scalar;
            matrix[5] *= scalar;
            matrix[6] *= scalar;
        }

        public static void matrix_column_2_scalar_multiply(double scalar, Mtxd matrix)
        {
            matrix[4] *= scalar;
            matrix[5] *= scalar;
            matrix[6] *= scalar;
        }

        public static void matrix_column_3_scalar_multiply(Single scalar, Mtxf matrix)
        {
            matrix[8] *= scalar;
            matrix[9] *= scalar;
            matrix[10] *= scalar;
            matrix[11] *= scalar;
        }

        public static void matrix_column_3_scalar_multiply(double scalar, Mtxd matrix)
        {
            matrix[8] *= scalar;
            matrix[9] *= scalar;
            matrix[10] *= scalar;
            matrix[11] *= scalar;
        }

        public static void matrix_column_3_scalar_multiply_2(Single scalar, Mtxf matrix)
        {
            matrix[8] *= scalar;
            matrix[9] *= scalar;
            matrix[10] *= scalar;
        }

        public static void matrix_column_3_scalar_multiply_2(double scalar, Mtxd matrix)
        {
            matrix[8] *= scalar;
            matrix[9] *= scalar;
            matrix[10] *= scalar;
        }
    }
}
