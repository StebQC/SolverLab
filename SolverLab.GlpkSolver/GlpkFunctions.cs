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
        [DllImport("glpk_4_54")]
        public static extern unsafe IntPtr glp_create_prob();
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_set_obj_dir(IntPtr P, int dir);        
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_add_rows(IntPtr P, int nrs);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_add_cols(IntPtr P, int ncs);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_set_row_bnds(IntPtr P, int i, int type, double lb, double ub);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_set_col_bnds(IntPtr P, int j, int type, double lb, double ub);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_set_obj_coef(IntPtr P, int j, double coef);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_set_col_kind(IntPtr P, int j, int kind);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_load_matrix(IntPtr P, int ne, int[] ia, int[] ja, double[] ra);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_scale_prob(IntPtr P, int flags);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_init_smcp(IntPtr parm);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_init_iocp(IntPtr parm);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_simplex(IntPtr P, IntPtr parm);
        [DllImport("glpk_4_54")]
        public static extern unsafe void glp_intopt(IntPtr P, IntPtr parm);
        [DllImport("glpk_4_54")]
        public static extern unsafe double glp_get_obj_val(IntPtr P);
        [DllImport("glpk_4_54")]
        public static extern unsafe double glp_mip_obj_val(IntPtr P);
        [DllImport("glpk_4_54")]
        public static extern unsafe double glp_ios_mip_gap(IntPtr P);
        [DllImport("glpk_4_54")]
        public static extern unsafe double glp_get_col_prim(IntPtr P, int j);
        [DllImport("glpk_4_54")]
        public static extern unsafe double glp_mip_col_val(IntPtr P, int j);
        [DllImport("glpk_4_54")]
        public static extern unsafe int glp_get_status(IntPtr P);
        [DllImport("glpk_4_54")]
        public static extern unsafe int glp_mip_status(IntPtr P);
        [DllImport("glpk_4_54")]
        public static extern unsafe int glp_delete_prob(IntPtr P);
    }
}
