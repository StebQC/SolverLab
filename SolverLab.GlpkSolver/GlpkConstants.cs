using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLab.GlpkSolver
{
    public static class GlpkConstants
    {
        public enum OptimizationDirection
        {
            GLP_MIN = 1,
            GLP_MAX = 2
        }

        public enum VariableKind
        {
            /// <summary>
            /// continuous variable
            /// </summary>
            GLP_CV = 1,
            /// <summary>
            /// integer variable
            /// </summary>
            GLP_IV = 2,
            /// <summary>
            /// binary variable
            /// </summary>
            GLP_BV = 3
        }

        public enum VariableType
        {
            /// <summary>
            /// free (unbounded) variable
            /// </summary>
            GLP_FR = 1,
            /// <summary>
            /// variable with lower bound
            /// </summary>
            GLP_LO = 2,
            /// <summary>
            /// variable with upper bound
            /// </summary>
            GLP_UP = 3,
            /// <summary>
            /// double-bounded variable
            /// </summary>
            GLP_DB = 4,
            /// <summary>
            /// fixed variable
            /// </summary>
            GLP_FX = 5
        }

        public enum VariableStatus
        {
            /// <summary>
            /// basic variable
            /// </summary>
            GLP_BS = 1,
            /// <summary>
            /// non-basic variable on lower bound
            /// </summary>
            GLP_NL = 2,
            /// <summary>
            /// non-basic variable on upper bound
            /// </summary>
            GLP_NU = 3,
            /// <summary>
            /// non-basic free (unbounded) variable
            /// </summary>
            GLP_NF = 4,
            /// <summary>
            /// non-basic fixed variable
            /// </summary>
            GLP_NS = 5
        }

        public enum OptimizationStatus
        {
            /// <summary>
            /// solution is undefined
            /// </summary>
            GLP_UNDEF = 1,
            /// <summary>
            /// solution is feasible
            /// </summary>
            GLP_FEAS = 2,
            /// <summary>
            /// solution is infeasible
            /// </summary>
            GLP_INFEAS = 3,
            /// <summary>
            /// no feasible solution exists
            /// </summary>
            GLP_NOFEAS = 4,
            /// <summary>
            /// solution is optimal
            /// </summary>
            GLP_OPT = 5,
            /// <summary>
            /// solution is unbounded
            /// </summary>
            GLP_UNBND = 6
        }

        public enum ScalingOptions
        {
            /// <summary>
            /// perform geometric mean scaling
            /// </summary>
            GLP_SF_GM = 0x01,
            /// <summary>
            /// perform equilibration scaling
            /// </summary>
            GLP_SF_EQ = 0x10,
            /// <summary>
            /// round scale factors to power of two
            /// </summary>
            GLP_SF_2N = 0x20,
            /// <summary>
            /// skip if problem is well scaled
            /// </summary>
            GLP_SF_SKIP = 0x40,
            /// <summary>
            /// choose scaling options automatically
            /// </summary>
            GLP_SF_AUTO = 0x80
        }

        public enum MessageLevel
        {
            /// <summary>
            /// no output
            /// </summary>
            GLP_MSG_OFF = 1,
            /// <summary>
            /// warning and error messages only
            /// </summary>
            GLP_MSG_ERR = 2,
            /// <summary>
            /// normal output
            /// </summary>
            GLP_MSG_ON = 3,
            /// <summary>
            /// full output
            /// </summary>
            GLP_MSG_ALL = 4,
            /// <summary>
            /// debug output
            /// </summary>
            GLP_MSG_DBG = 5
        }

        public enum SimplexMethod
        {
            /// <summary>
            /// use primal simplex
            /// </summary>
            GLP_PRIMAL = 1,
            /// <summary>
            /// use dual; if it fails, use primal
            /// </summary>
            GLP_DUALP = 2,
            /// <summary>
            /// use dual simplex
            /// </summary>
            GLP_DUAL = 3
        }

        public enum Pricing
        {
            /// <summary>
            /// standard (Dantzig rule)
            /// </summary>
            GLP_PT_STD = 0x11,
            /// <summary>
            /// projected steepest edge
            /// </summary>
            GLP_PT_PSE = 0x22
        }

        public enum RatioTest
        {
            /// <summary>
            /// standard (textbook)
            /// </summary>
            GLP_RT_STD = 0x11,
            /// <summary>
            /// two-pass Harris' ratio test
            /// </summary>
            GLP_RT_HAR = 0x22
        }

        public enum EnableDisableFlag
        {
            /// <summary>
            /// enable something
            /// </summary>
            GLP_ON = 1,
            /// <summary>
            /// disable something
            /// </summary>
            GLP_OFF = 0
        }
    }
}

///* solution indicator: */
//#define GLP_SOL            1  /* basic solution */
//#define GLP_IPT            2  /* interior-point solution */
//#define GLP_MIP            3  /* mixed integer solution */

///* solution status: */
//#define GLP_UNDEF          1  /* solution is undefined */
//#define GLP_FEAS           2  /* solution is feasible */
//#define GLP_INFEAS         3  /* solution is infeasible */
//#define GLP_NOFEAS         4  /* no feasible solution exists */
//#define GLP_OPT            5  /* solution is optimal */
//#define GLP_UNBND          6  /* solution is unbounded */

//      int r_test;             /* ratio test technique: */
//#define GLP_RT_STD      0x11  /* standard (textbook) */
//#define GLP_RT_HAR      0x22  /* two-pass Harris' ratio test */