using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SolverLab.CbcSolver
{
    public class CbcSolver : GenericSolver
    {
        private unsafe IntPtr _model = IntPtr.Zero;
        private bool _isMip = true;

        public CbcSolver(bool launchDebugger = false)
        {
            if (launchDebugger)
            {
                System.Diagnostics.Debugger.Launch();
            }
            _model = CbcFunctions.Cbc_newModel();
        }

        protected override string InternalVersion()
        {
            return Marshal.PtrToStringAnsi(CbcFunctions.Cbc_getVersion());
        }

        protected override void InternalChangeObjectives(int[] columns, double[] values)
        {
            throw new NotImplementedException();
        }

        protected override void InternalCreateModel(bool maximize)
        {
            var orderedNzs = nzs.OrderBy(nz => nz.col).ToList();
            int[] start = new int[vars.Count + 1];
            int[] rowindex = new int[nzs.Count];
            double[] values = new double[nzs.Count];
            int lastStart = -1;
            for (var i = 0; i < nzs.Count; ++i)
            {
                if (lastStart != orderedNzs[i].col)
                {
                    lastStart = orderedNzs[i].col;
                    start[lastStart] = i;
                }
                rowindex[i] = orderedNzs[i].row;
                values[i] = orderedNzs[i].val;
            }
            start[start.Length - 1] = nzs.Count;
            double[] collb = vars.Values.Select(v => v.lb).ToArray();
            double[] colub = vars.Values.Select(v => v.ub).ToArray();
            double[] obj = vars.Values.Select(v => v.objval).ToArray();
            double[] rowlb = consts.Values.Select(c => (c.sense == ConstSense.Equal || c.sense == ConstSense.GreaterOrEqual) ? c.rhs : double.NegativeInfinity).ToArray();
            double[] rowub = consts.Values.Select(c => (c.sense == ConstSense.Equal || c.sense == ConstSense.LesserOrEqual) ? c.rhs : double.PositiveInfinity).ToArray();

            CbcFunctions.Cbc_loadProblem(_model, vars.Count, consts.Count, start, rowindex, values, collb, colub, obj, rowlb, rowub);

            if (_isMip)
            {
                for (int i = 0; i < vars.Count; ++i)
                {
                    if (vars[i].type != VarType.Linear)
                    {
                        CbcFunctions.Cbc_setInteger(_model, i);
                    }
                }
            }

            CbcFunctions.Cbc_setObjSense(_model, maximize ? -1 : 1);
        }

        protected override void InternalSetParam(Param param, string value)
        {
            switch (param)
            {
                case Param.IsMip:
                    if (!bool.Parse(value))
                    {
                        _isMip = false;
                    }
                    break;
                case Param.LogOutput:
                    if (bool.Parse(value))
                    {
                        CbcFunctions.Cbc_setParameter(_model, "message", "on");
                    }
                    else
                    {
                        CbcFunctions.Cbc_setParameter(_model, "message", "off");
                    }
                    break;
                case Param.TimeLimit:
                    CbcFunctions.Cbc_setParameter(_model, "seconds", value);
                    break;
                case Param.Gap:
                    CbcFunctions.Cbc_setParameter(_model, "ratioGap", value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void InternalSolve()
        {
            CbcFunctions.Cbc_solve(_model);
        }

        public override OptimizationStatus GetStatus()
        {
            switch (CbcFunctions.Cbc_status(_model))
            {
                case 0:
                case 1:
                    switch (CbcFunctions.Cbc_secondaryStatus(_model))
                    {
                        case 0:
                            return OptimizationStatus.Optimal;
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 8:
                            return OptimizationStatus.SubOptimal;
                        default:
                            return OptimizationStatus.Error;
                    }
                case 2:
                    return OptimizationStatus.Error;
                default:
                    return OptimizationStatus.Error;
            }
        }

        public override double GetObjVal()
        {

            if (CbcFunctions.Cbc_isProvenOptimal(_model))
            {
                return CbcFunctions.Cbc_getObjValue(_model);
            }
            return 0;
        }

        public override double GetMipGap()
        {
            if (CbcFunctions.Cbc_getObjValue(_model) > 0)
            {
                return Math.Abs((CbcFunctions.Cbc_getBestPossibleObjValue(_model) - CbcFunctions.Cbc_getObjValue(_model)) / (CbcFunctions.Cbc_getObjValue(_model)));
            }
            return -1;
        }

        public override double[] GetSolution()
        {
            double[] values = new double[vars.Count];
            Marshal.Copy(CbcFunctions.Cbc_getColSolution(_model), values, 0, vars.Count);
            return values;
        }

        public override double[] GetDualPrices()
        {
            throw new NotImplementedException();
        }

        public override void DeleteModel()
        {
            CbcFunctions.Cbc_deleteModel(_model);
            _model = IntPtr.Zero;
        }

        protected override string GetSolverName()
        {
            return "Cbc";
        }

        public override void ExportConflicts(string filename)
        {
            throw new NotImplementedException();
        }

        public override void ExportModel(string filename)
        {
            CbcFunctions.Cbc_writeMps(_model, filename);
        }

    }
}
