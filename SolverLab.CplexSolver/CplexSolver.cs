﻿//------------------------------------------------------------------------------
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
            //for (var i = 0; i < groupedNzByConst.Count; ++i)
            {
                var currentConts = consts[currentNzs.Key];
                //var expr = _model.ScalProd(groupedNzByConst[i].Select(n => n.val).ToArray(), groupedNzByConst[i].Select(n => _vars[n.col]).ToArray());
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
            _objective = _model.AddObjective(ObjectiveSense.Maximize, _model.ScalProd(objValIndexes.Select(v => vars[v].objval).ToArray(), objValIndexes.Select(v => _vars[v]).ToArray()));
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
    }
}