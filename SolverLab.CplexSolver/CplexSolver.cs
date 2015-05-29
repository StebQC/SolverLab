using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolverLab;
using ILOG.CPLEX;
using ILOG.Concert;

namespace SolverLab.CplexSolver
{
    public class CplexSolver : GenericSolver
    {
        private Cplex _model = null;
        private IObjective _objective = null;
        private Dictionary<int, INumVar> _vars = new Dictionary<int, INumVar>();
        private Dictionary<int, IRange> _consts = new Dictionary<int, IRange>();
        private Dictionary<VarType, NumVarType> _varTypes = new Dictionary<VarType, NumVarType>() { { VarType.Binary, NumVarType.Bool }, { VarType.Integer, NumVarType.Int }, { VarType.Linear, NumVarType.Float } };

        public CplexSolver(bool launchDebugger = false)
        {
            if (launchDebugger)
            {
                System.Diagnostics.Debugger.Launch();
            }
            _model = new Cplex();
        }

        protected override string InternalVersion()
        {
            return _model.Version;
        }

        protected override void InternalChangeObjectives(int[] columns, double[] values)
        {
            throw new NotImplementedException();
        }

        protected override void InternalCreateModel(bool maximize)
        {
            // Add variables
            for (var i = 0; i < vars.Count; ++i)
            {
                var currentVar = vars[i];
                if (!string.IsNullOrEmpty(vars[i].name))
                {
                    _vars.Add(i, _model.NumVar(currentVar.lb, currentVar.ub, _varTypes[currentVar.type], currentVar.name));
                }
                else
                {
                    _vars.Add(i, _model.NumVar(currentVar.lb, currentVar.ub, _varTypes[currentVar.type]));
                }
            }

            // Add constraints
            var groupedNzByConst = nzs.GroupBy(n => n.row).ToDictionary(g => g.Key, g => g.ToList());
            foreach(var currentNzs in groupedNzByConst)
            {
                var currentConts = consts[currentNzs.Key];
                var expr = _model.ScalProd(currentNzs.Value.Select(n => n.val).ToArray(), currentNzs.Value.Select(n => _vars[n.col]).ToArray());

                switch (currentConts.sense)
                {
                    case ConstSense.Equal:
                        _consts.Add(currentNzs.Key, _model.AddEq(expr, currentConts.rhs));
                        break;
                    case ConstSense.GreaterOrEqual:
                        _consts.Add(currentNzs.Key, _model.AddGe(expr, currentConts.rhs));
                        break;
                    case ConstSense.LesserOrEqual:
                        _consts.Add(currentNzs.Key, _model.AddLe(expr, currentConts.rhs));
                        break;
                }
            }

            // Add objective
            var objValIndexes = vars.Where(v => v.Value.objval != 0).Select(v => v.Key).ToList();
            _objective = _model.AddObjective(maximize ? ObjectiveSense.Maximize : ObjectiveSense.Minimize, _model.ScalProd(objValIndexes.Select(v => vars[v].objval).ToArray(), objValIndexes.Select(v => _vars[v]).ToArray()));
        }

        protected override void InternalSetParam(Param param, string value)
        {
            switch (param)
            {
                case Param.IsMip:
                    if (!bool.Parse(value))
                    {
                        _varTypes[VarType.Binary] = NumVarType.Float;
                        _varTypes[VarType.Integer] = NumVarType.Float;
                    }
                    break;
                case Param.TimeLimit:
                    _model.SetParam(Cplex.IntParam.TimeLimit, int.Parse(value));
                    break;
                case Param.LogOutput:
                    if (!bool.Parse(value))
                    {
                        _model.SetOut(null);
                    }
                    break;
                case Param.Gap:
                    _model.SetParam(Cplex.DoubleParam.EpGap, double.Parse(value));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void InternalSolve()
        {
            _model.Solve();
        }

        public override OptimizationStatus GetStatus()
        {
            var status = _model.GetStatus();
            if (status == Cplex.Status.Optimal)
            {
                return OptimizationStatus.Optimal;
            }
            else if (status == Cplex.Status.Feasible)
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
            return _model.GetObjValue();
        }

        public override double GetMipGap()
        {
            return _model.GetMIPRelativeGap();
        }

        public override double[] GetSolution()
        {
            return _model.GetValues(_vars.Values.ToArray());
        }

        public override double[] GetDualPrices()
        {
            throw new NotImplementedException();
        }

        public override void DeleteModel()
        {
            _model.EndModel(); // Necessary????
        }

        protected override string GetSolverName()
        {
            return "Cplex";
        }

        public override void ExportConflicts(string filename)
        {
            _model.WriteConflict(filename);
        }

        public override void ExportModel(string filename)
        {
            _model.ExportModel(filename);
        }
    }
}
