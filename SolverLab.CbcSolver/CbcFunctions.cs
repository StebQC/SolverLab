using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SolverLab.CbcSolver
{
    public static class CbcFunctions
    {
        private const string _dllName = "cbcCInterfaceDll";
        [DllImport(_dllName)]
        public static extern unsafe IntPtr Cbc_newModel();
        [DllImport(_dllName)]
        public static extern unsafe IntPtr Cbc_getVersion();
        [DllImport(_dllName)]
        public static extern unsafe double Cbc_getObjValue(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe double Cbc_getBestPossibleObjValue(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe void Cbc_loadProblem(IntPtr model, int numcols, int numrows, int[] start, int[] index, double[] value, double[] collb, double[] colub, double[] obj, double[] rowlb, double[] rowub);
        [DllImport(_dllName)]
        public static extern unsafe void Cbc_setInteger(IntPtr model, int iColumn);
        [DllImport(_dllName)]
        public static extern unsafe void Cbc_solve(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe void Cbc_setObjSense(IntPtr model, double sense);
        [DllImport(_dllName)]
        public static extern unsafe double Cbc_getObjSense(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe int Cbc_getNumCols(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe int Cbc_getNumRows(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe bool Cbc_isInteger(IntPtr model, int i);
        [DllImport(_dllName)]
        public static extern unsafe bool Cbc_isProvenOptimal(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe IntPtr Cbc_getColSolution(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe void Cbc_readMps(IntPtr model, string filename);
        [DllImport(_dllName)]
        public static extern unsafe void Cbc_writeMps(IntPtr model, string filename);
        [DllImport(_dllName)]
        public static extern unsafe void Cbc_setParameter(IntPtr model, string name, string value);
        [DllImport(_dllName)]
        public static extern unsafe int Cbc_status(IntPtr model);
        [DllImport(_dllName)]
        public static extern unsafe int Cbc_secondaryStatus(IntPtr model);

        
    }       
}
