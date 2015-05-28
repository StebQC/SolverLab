using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SolverLab.GlpkSolver
{
    public class GlpkSolver : GenericSolver
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct glp_smcp
        {
            public int msg_lev;            /* message level: */
            public int meth;               /* simplex method option: */
            public int pricing;            /* pricing technique: */
            public int r_test;             /* ratio test technique: */
            public double tol_bnd;         /* spx.tol_bnd */
            public double tol_dj;          /* spx.tol_dj */
            public double tol_piv;         /* spx.tol_piv */
            public double obj_ll;          /* spx.obj_ll */
            public double obj_ul;          /* spx.obj_ul */
            public int it_lim;             /* spx.it_lim */
            public int tm_lim;             /* spx.tm_lim (milliseconds) */
            public int out_frq;            /* spx.out_frq */
            public int out_dly;            /* spx.out_dly (milliseconds) */
            public int presolve;           /* enable/disable using LP presolver */
            public double[] foo_bar;     /* (reserved) */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct glp_iocp
        {
            public int msg_lev;            /* message level (see glp_smcp) */
            public int br_tech;            /* branching technique: */
            //#define GLP_BR_FFV         1  /* first fractional variable */
            //#define GLP_BR_LFV         2  /* last fractional variable */
            //#define GLP_BR_MFV         3  /* most fractional variable */
            //#define GLP_BR_DTH         4  /* heuristic by Driebeck and Tomlin */
            //#define GLP_BR_PCH         5  /* hybrid pseudocost heuristic */
            public int bt_tech;            /* backtracking technique: */
            //#define GLP_BT_DFS         1  /* depth first search */
            //#define GLP_BT_BFS         2  /* breadth first search */
            //#define GLP_BT_BLB         3  /* best local bound */
            //#define GLP_BT_BPH         4  /* best projection heuristic */
            public double tol_int;         /* mip.tol_int */
            public double tol_obj;         /* mip.tol_obj */
            public int tm_lim;             /* mip.tm_lim (milliseconds) */
            public int out_frq;            /* mip.out_frq (milliseconds) */
            public int out_dly;            /* mip.out_dly (milliseconds) */
            public unsafe void *cb_func; //(glp_tree *T, void *info); /* mip.cb_func */
            public unsafe void *cb_info;          /* mip.cb_info */
            public int cb_size;            /* mip.cb_size */
            public int pp_tech;            /* preprocessing technique: */
            //#define GLP_PP_NONE        0  /* disable preprocessing */
            //#define GLP_PP_ROOT        1  /* preprocessing only on root level */
            //#define GLP_PP_ALL         2  /* preprocessing on all levels */
            public double mip_gap;         /* relative MIP gap tolerance */
            public int mir_cuts;           /* MIR cuts       (GLP_ON/GLP_OFF) */
            public int gmi_cuts;           /* Gomory's cuts  (GLP_ON/GLP_OFF) */
            public int cov_cuts;           /* cover cuts     (GLP_ON/GLP_OFF) */
            public int clq_cuts;           /* clique cuts    (GLP_ON/GLP_OFF) */
            public int presolve;           /* enable/disable using MIP presolver */
            public int binarize;           /* try to binarize integer variables */
            public int fp_heur;            /* feasibility pump heuristic */
            public int ps_heur;            /* proximity search heuristic */
            public int ps_tm_lim;          /* proxy time limit, milliseconds */
            public int use_sol;            /* use existing solution */
            public unsafe char *save_sol;   /* filename to save every new solution */
            public int alien;              /* use alien solver */
            public double[] foo_bar;     /* (reserved) */
        }


        private unsafe IntPtr _model = IntPtr.Zero;
        private unsafe IntPtr _simplexParm = IntPtr.Zero;
        private unsafe IntPtr _mipParm = IntPtr.Zero;
        private bool _isMip = true;
        Dictionary<VarType, GlpkConstants.VariableKind> _varTypes = new Dictionary<VarType, GlpkConstants.VariableKind>() {
        { VarType.Binary, GlpkConstants.VariableKind.GLP_BV }, 
        { VarType.Integer, GlpkConstants.VariableKind.GLP_IV },
        { VarType.Linear, GlpkConstants.VariableKind.GLP_CV } };

        public GlpkSolver(bool launchDebugger = false)
        {
            if (launchDebugger)
            {
                System.Diagnostics.Debugger.Launch();
            }
            _model = GlpkFunctions.glp_create_prob();

            // Create the simplex parameters
            glp_smcp simplexOptions = new glp_smcp();
            _simplexParm = Marshal.AllocHGlobal(Marshal.SizeOf(simplexOptions));
            Marshal.StructureToPtr(simplexOptions, _simplexParm, false);
            GlpkFunctions.glp_init_smcp(_simplexParm);
            //simplexOptions = (glp_smcp)Marshal.PtrToStructure(_simplexParm, typeof(glp_smcp));
            //options.pricing = (int)GlpkConstants.Pricing.GLP_PT_STD;
            //simplexOptions.meth = (int)GlpkConstants.SimplexMethod.GLP_PRIMAL;
            //options.r_test = (int)GlpkConstants.RatioTest.GLP_RT_STD;
            //Marshal.StructureToPtr(simplexOptions, _simplexParm, false);

            glp_iocp mipOptions = new glp_iocp();
            _mipParm = Marshal.AllocHGlobal(Marshal.SizeOf(mipOptions));
            Marshal.StructureToPtr(mipOptions, _mipParm, false);
            GlpkFunctions.glp_init_iocp(_mipParm);
            mipOptions = (glp_iocp)Marshal.PtrToStructure(_mipParm, typeof(glp_iocp));
            mipOptions.presolve = (int)GlpkConstants.EnableDisableFlag.GLP_ON;
            Marshal.StructureToPtr(mipOptions, _mipParm, false);
        }

        protected override void InternalChangeObjectives(int[] columns, double[] values)
        {
            throw new NotImplementedException();
        }

        protected override void InternalCreateModel(bool maximize)
        {
            GlpkFunctions.glp_set_obj_dir(_model, maximize ? (int)GlpkConstants.OptimizationDirection.GLP_MAX : (int)GlpkConstants.OptimizationDirection.GLP_MIN);

            // Add constraints
            GlpkFunctions.glp_add_rows(_model, consts.Count);
            foreach (var c in consts)
            {
                switch (c.Value.sense)
                {
                    case ConstSense.Equal:
                        GlpkFunctions.glp_set_row_bnds(_model, c.Key + 1, (int)GlpkConstants.VariableType.GLP_FX, c.Value.rhs, c.Value.rhs);
                        break;
                    case ConstSense.GreaterOrEqual:
                        GlpkFunctions.glp_set_row_bnds(_model, c.Key + 1, (int)GlpkConstants.VariableType.GLP_LO, c.Value.rhs, System.Double.PositiveInfinity);
                        break;
                    case ConstSense.LesserOrEqual:
                        GlpkFunctions.glp_set_row_bnds(_model, c.Key + 1, (int)GlpkConstants.VariableType.GLP_UP, 0, c.Value.rhs);
                        break;
                }
            }

            // Add variables
            GlpkFunctions.glp_add_cols(_model, vars.Count);
            foreach (var v in vars)
            {
                if (v.Value.lb == System.Double.NegativeInfinity && v.Value.ub == System.Double.PositiveInfinity)
                {
                    GlpkFunctions.glp_set_col_bnds(_model, v.Key + 1, (int)GlpkConstants.VariableType.GLP_FR, v.Value.lb, v.Value.ub);
                }
                else if (v.Value.lb == System.Double.NegativeInfinity)
                {
                    GlpkFunctions.glp_set_col_bnds(_model, v.Key + 1, (int)GlpkConstants.VariableType.GLP_UP, v.Value.lb, v.Value.ub);
                }
                else if (v.Value.ub == System.Double.PositiveInfinity)
                {
                    GlpkFunctions.glp_set_col_bnds(_model, v.Key + 1, (int)GlpkConstants.VariableType.GLP_LO, v.Value.lb, v.Value.ub);
                }
                else if (v.Value.lb == v.Value.ub)
                {
                    GlpkFunctions.glp_set_col_bnds(_model, v.Key + 1, (int)GlpkConstants.VariableType.GLP_FX, v.Value.lb, v.Value.ub);
                }
                else
                {
                    GlpkFunctions.glp_set_col_bnds(_model, v.Key + 1, (int)GlpkConstants.VariableType.GLP_DB, v.Value.lb, v.Value.ub);
                }
                GlpkFunctions.glp_set_obj_coef(_model, v.Key + 1, v.Value.objval);
                GlpkFunctions.glp_set_col_kind(_model, v.Key + 1, (int)_varTypes[v.Value.type]);
            }

            // Add nz
            var ia = new int[nzs.Count + 1];
            var ja = new int[nzs.Count + 1];
            var ra = new double[nzs.Count + 1];
            for (var i = 0; i < nzs.Count; ++i)
            {
                ia[i + 1] = nzs[i].row + 1;
                ja[i + 1] = nzs[i].col + 1;
                ra[i + 1] = nzs[i].val;
            }

            GlpkFunctions.glp_load_matrix(_model, nzs.Count, ia, ja, ra);
        }

        protected override void InternalSetParam(Param param, string value)
        {
            var simplexOptions = (glp_smcp)Marshal.PtrToStructure(_simplexParm, typeof(glp_smcp));
            var mipOptions = (glp_iocp)Marshal.PtrToStructure(_mipParm, typeof(glp_iocp));
            switch (param)
            {
                case Param.IsMip:
                    if (!bool.Parse(value))
                    {
                        _varTypes[VarType.Binary] = GlpkConstants.VariableKind.GLP_CV;
                        _varTypes[VarType.Integer] = GlpkConstants.VariableKind.GLP_CV;
                        _isMip = false; 
                    }
                    break;
                case Param.LogOutput:
                    if (bool.Parse(value))
                    {
                        simplexOptions.msg_lev = (int)GlpkConstants.MessageLevel.GLP_MSG_ON;
                        mipOptions.msg_lev = (int)GlpkConstants.MessageLevel.GLP_MSG_ON;
                    }
                    else
                    {
                        simplexOptions.msg_lev = (int)GlpkConstants.MessageLevel.GLP_MSG_OFF;
                        mipOptions.msg_lev = (int)GlpkConstants.MessageLevel.GLP_MSG_OFF;
                    }
                    break;
                case Param.TimeLimit :
                    simplexOptions.tm_lim = Convert.ToInt32(double.Parse(value) * 1000);
                    mipOptions.tm_lim = Convert.ToInt32(double.Parse(value) * 1000);
                    break;
                case Param.Gap:
                    mipOptions.mip_gap = double.Parse(value);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Marshal.StructureToPtr(simplexOptions, _simplexParm, false);
            Marshal.StructureToPtr(mipOptions, _mipParm, false);
        }

        protected override void InternalSolve()
        {
            // Scale the problem
            GlpkFunctions.glp_scale_prob(_model, (int)(GlpkConstants.ScalingOptions.GLP_SF_SKIP | GlpkConstants.ScalingOptions.GLP_SF_GM | GlpkConstants.ScalingOptions.GLP_SF_EQ));

            // Solve
            if (!_isMip)
            {
                // Solve it using the simple
                GlpkFunctions.glp_simplex(_model, _simplexParm);
            }
            else
            {
                // Solve the mip using interior points
                GlpkFunctions.glp_intopt(_model, _mipParm);
            }
        }

        public override OptimizationStatus GetStatus()
        {
            int status = GetSolutionStatus();
            
            if (status == (int)GlpkConstants.OptimizationStatus.GLP_OPT)
            {
                return OptimizationStatus.Optimal;
            }
            else if (status == (int)GlpkConstants.OptimizationStatus.GLP_FEAS)
            {
                return OptimizationStatus.SubOptimal;
            }
            else
            {
                return OptimizationStatus.Error;
            }
        }

        public override double GetObjVal()
        {
            if (!_isMip)
            {
                return GlpkFunctions.glp_get_obj_val(_model);
            }
            else
            {
                return GlpkFunctions.glp_mip_obj_val(_model);
            }
        }

        public override double GetMipGap()
        {
            if (_isMip)
            {
                return GlpkFunctions.glp_ios_mip_gap(_model);    
            }
            else
            {
                return -1;
            }
        }

        public override double[] GetSolution()
        {
            double[] values = new double[vars.Count];
            for (var i = 0; i < vars.Count; ++i)
            {
                values[i] = GetColumnValue(i);
            }
            return values;
        }

        public override double[] GetDualPrices()
        {
            throw new NotImplementedException();
        }

        public override void DeleteModel()
        {
            GlpkFunctions.glp_delete_prob(_model);
        }

        protected override string GetSolverName()
        {
            return "Glpk";
        }
        
        private int GetSolutionStatus()
        {
            if (!_isMip)
            {
                return GlpkFunctions.glp_get_status(_model);
            }
            else
            {
                return GlpkFunctions.glp_mip_status(_model);
            }
        }

        private double GetColumnValue(int i)
        {
            if (!_isMip)
            {
                return GlpkFunctions.glp_get_col_prim(_model, i + 1);
            }
            else
            {
                return GlpkFunctions.glp_mip_col_val(_model, i + 1);
            }
        }
    }
}
