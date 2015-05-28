//------------------------------------------------------------------------------
// $URL: https://forac-storage.forac.ulaval.ca/svn/Forac/Sources/features/LogiLab/201307021/Web/Silvilab/DataModels/LogiLab/ModelObjects/Optimization.cs $
// $Id: Optimization.cs 694 2014-05-19 18:19:34Z slemieux $
// 
// Copyright (c) 2009-2014, Universite Laval - FORAC
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;

namespace SolverLab.GurobiSolver
{
    public class GurobiSolver : GenericSolver
    {
        GRBEnv _env = null;
        GRBModel _model = null;
        Dictionary<int, GRBConstr> _consts = new Dictionary<int, GRBConstr>();
        Dictionary<int, GRBVar> _vars = new Dictionary<int, GRBVar>();
        Dictionary<VarType, char> _varTypes = new Dictionary<VarType, char>() { { VarType.Binary, GRB.BINARY }, { VarType.Integer, GRB.INTEGER }, { VarType.Linear, GRB.CONTINUOUS } };

        public GurobiSolver(bool launchDebugger = false)
        {
            if (launchDebugger)
            {
                System.Diagnostics.Debugger.Launch();
            }
            _env = new GRBEnv();
            _model = new GRBModel(_env);
            _model.Set(GRB.IntAttr.ModelSense, -1);
        }

        protected override void InternalChangeObjectives(int[] columns, double[] values)
        {
            throw new NotImplementedException();
        }

        protected override void InternalCreateModel()
        {
            // Add variables
            for (var i = 0; i < vars.Count; ++i)
            {
                var currentVar = vars[i];
                _vars.Add(i, _model.AddVar(currentVar.lb, currentVar.ub, currentVar.objval, _varTypes[currentVar.type], currentVar.name));
            }

            // Update model to integrate new variables
            _model.Update();

            // Add constraints
            var groupedNzByConst = nzs.GroupBy(n => n.row).ToDictionary(g => g.Key, g => g.ToList());
            foreach(var currentNzs in groupedNzByConst)
            //for (var i = 0; i < consts.Count; ++i)
            {
                GRBLinExpr expr = 0.0;
                foreach (Nz nz in currentNzs.Value)
                {
                    expr.AddTerm(nz.val, _vars[nz.col]);
                }
                switch (consts[currentNzs.Key].sense)
                {
                    case ConstSense.Equal:
                        _model.AddConstr(expr == consts[currentNzs.Key].rhs, consts[currentNzs.Key].name);
                        break;
                    case ConstSense.GreaterOrEqual:
                        _model.AddConstr(expr >= consts[currentNzs.Key].rhs, consts[currentNzs.Key].name);
                        break;
                    case ConstSense.LesserOrEqual:
                        _model.AddConstr(expr <= consts[currentNzs.Key].rhs, consts[currentNzs.Key].name);
                        break;
                }
            }

        }

        protected override void InternalSetParam(Param param, string value)
        {
            switch (param)
            {
                case Param.IsMip:
                    if (!bool.Parse(value))
                    {
                        _varTypes[VarType.Binary] = GRB.CONTINUOUS;
                        _varTypes[VarType.Integer] = GRB.CONTINUOUS;
                    }
                    break;
                case Param.TimeLimit:
                    _model.GetEnv().Set(GRB.DoubleParam.TimeLimit, double.Parse(value));
                    break;
                case Param.LogOutput:
                    _model.GetEnv().Set(GRB.IntParam.LogToConsole, bool.Parse(value) ? 1 : 0);
                    break;
                case Param.Gap:
                    _model.GetEnv().Set(GRB.DoubleParam.MIPGap, double.Parse(value));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void InternalSolve()
        {
            _model.Optimize();
        }

        public override OptimizationStatus GetStatus()
        {
            var status = _model.Get(GRB.IntAttr.Status);
            if (status == GRB.Status.OPTIMAL)
            {
                return OptimizationStatus.Optimal;
            }
            else if (status == GRB.Status.SUBOPTIMAL || status == GRB.Status.TIME_LIMIT || status == GRB.Status.ITERATION_LIMIT || status == GRB.Status.NODE_LIMIT || status == GRB.Status.SOLUTION_LIMIT)
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
            return  _model.Get(GRB.DoubleAttr.ObjVal);
        }

        public override double GetMipGap()
        {
            if (_varTypes[VarType.Binary] == GRB.CONTINUOUS)
            {
                return -1;
            }

            return _model.Get(GRB.DoubleAttr.MIPGap);
        }

        public override double[] GetSolution()
        {
            double[] values = new double[_vars.Count];
            foreach (var v in _vars)
            {
                values[v.Key] = v.Value.Get(GRB.DoubleAttr.X);
            }
            return values;
        }

        public override double[] GetDualPrices()
        {
            throw new NotImplementedException();
        }

        public override void DeleteModel()
        {
            _model.Dispose();
            _env.Dispose();
        }

        protected override string GetSolverName()
        {
            return "Gurobi";
        }
    }
}
 