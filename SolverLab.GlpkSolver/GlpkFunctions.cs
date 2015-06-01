using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SolverLab.GlpkSolver
{
    public static class GlpkFunctions
    {
        private const string _dllName = "glpk_4_54";
        [DllImport(_dllName)]
        public static extern unsafe IntPtr glp_create_prob();
        [DllImport(_dllName)]
        public static extern unsafe void glp_set_obj_dir(IntPtr P, int dir);        
        [DllImport(_dllName)]
        public static extern unsafe void glp_add_rows(IntPtr P, int nrs);
        [DllImport(_dllName)]
        public static extern unsafe void glp_add_cols(IntPtr P, int ncs);
        [DllImport(_dllName)]
        public static extern unsafe void glp_set_row_bnds(IntPtr P, int i, int type, double lb, double ub);
        [DllImport(_dllName)]
        public static extern unsafe void glp_set_col_bnds(IntPtr P, int j, int type, double lb, double ub);
        [DllImport(_dllName)]
        public static extern unsafe void glp_set_obj_coef(IntPtr P, int j, double coef);
        [DllImport(_dllName)]
        public static extern unsafe void glp_set_col_kind(IntPtr P, int j, int kind);
        [DllImport(_dllName)]
        public static extern unsafe void glp_load_matrix(IntPtr P, int ne, int[] ia, int[] ja, double[] ra);
        [DllImport(_dllName)]
        public static extern unsafe void glp_scale_prob(IntPtr P, int flags);
        [DllImport(_dllName)]
        public static extern unsafe void glp_init_smcp(IntPtr parm);
        [DllImport(_dllName)]
        public static extern unsafe void glp_init_iocp(IntPtr parm);
        [DllImport(_dllName)]
        public static extern unsafe void glp_simplex(IntPtr P, IntPtr parm);
        [DllImport(_dllName)]
        public static extern unsafe void glp_intopt(IntPtr P, IntPtr parm);
        [DllImport(_dllName)]
        public static extern unsafe double glp_get_obj_val(IntPtr P);
        [DllImport(_dllName)]
        public static extern unsafe double glp_mip_obj_val(IntPtr P);
        [DllImport(_dllName)]
        public static extern unsafe double glp_ios_mip_gap(IntPtr P);
        [DllImport(_dllName)]
        public static extern unsafe double glp_get_col_prim(IntPtr P, int j);
        [DllImport(_dllName)]
        public static extern unsafe double glp_mip_col_val(IntPtr P, int j);
        [DllImport(_dllName)]
        public static extern unsafe int glp_get_status(IntPtr P);
        [DllImport(_dllName)]
        public static extern unsafe int glp_mip_status(IntPtr P);
        [DllImport(_dllName)]
        public static extern unsafe int glp_delete_prob(IntPtr P);
        [DllImport(_dllName)]
        public static extern unsafe string glp_version();
        [DllImport(_dllName)]
        public static extern unsafe int glp_write_mps(IntPtr P, int fmt, object parm, string fname);
    }
}
