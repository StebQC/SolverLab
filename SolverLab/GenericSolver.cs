using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLab
{
    public enum ConstSense
    {
        Equal,
        GreaterOrEqual,
        LesserOrEqual
    }

    public enum VarType
    {
        Linear,
        Binary,
        Integer
    }

    public enum Param
    {
        TimeLimit,
        Gap,
        IsMip,
        LogOutput
    }

    public enum OptimizationStatus
    {
        Optimal,
        SubOptimal,
        Error
    }

    public abstract class GenericSolver
    {
        protected Dictionary<int, Var> vars = new Dictionary<int, Var>();
        protected Dictionary<int, Const> consts = new Dictionary<int, Const>();
        protected List<Nz> nzs = new List<Nz>();
        private bool _modelCreated = false;

        public string Version
        {
            get
            {
                return InternalVersion();
            }
        }

        protected abstract string InternalVersion();

        public int AddConst(string name, double rhs, ConstSense sense)
        {
            consts.Add(consts.Count, new Const(name, rhs, sense));
            return consts.Count - 1;
        }

        public int[] AddConsts(string[] name, double[] rhs, ConstSense[] sense)
        {
            int[] indexes = new int[name.Length];
            for (int i = 0; i < name.Length; ++i)
            {
                indexes[i] = AddConst(name[i], rhs[i], sense[i]);
            }
            return indexes;
        }

        public int AddVar(string name, double objval, double lb, double ub, VarType type)
        {
            vars.Add(vars.Count, new Var(name, objval, lb, ub, type));
            return vars.Count - 1;
        }

        public int[] AddVars(string[] name, double[] objval, double[] lb, double[] ub, VarType[] type)
        {
            int[] indexes = new int[name.Length];
            for (int i = 0; i < name.Length; ++i)
            {
                indexes[i] = AddVar(name[i], objval[i], lb[i], ub[i], type[i]);
            }
            return indexes;
        }

        public void AddNz(int row, int col, double val)
        {
            nzs.Add(new Nz(row, col, val));
        }

        public void ChangeObjectives(int[] columns, double[] values)
        {
            if (_modelCreated)
            {
                InternalChangeObjectives(columns, values);
            }
            else
            {
                for (var i = 0; i < columns.Length; ++i)
                {
                    vars[columns[i]].objval = values[i];
                }
            }
        }

        protected abstract void InternalChangeObjectives(int[] columns, double[] values);

        public void ChangeRHS(int row, double value)
        {
            if (_modelCreated)
            {
                throw new NotSupportedException("The model was already created!");
            }

            consts[row].rhs = value;
        }

        public void ChangeRHSs(int[] rows, double[] values)
        {
            if (_modelCreated)
            {
                throw new NotSupportedException("The model was already created!");
            }

            for (int i = 0; i < rows.Length; ++i)
            {
                consts[rows[i]].rhs = values[i];
            }
        }

        public void ChangeNzs(int[] rows, int[] columns, double[] values)
        {
            if (_modelCreated)
            {
                throw new NotSupportedException("The model was already created!");
            }

            var nzsDict = nzs.ToDictionary(nz => new Tuple<int, int>(nz.row, nz.col));
            for (int i = 0; i < rows.Length; ++i)
            {
                var key = new Tuple<int, int>(rows[i], columns[i]);
                if (nzsDict.ContainsKey(key))
                {
                    nzsDict[key].val = values[i];
                }
                else
                {
                    nzsDict.Add(key, new Nz(rows[i], columns[i], values[i]));
                    nzs.Add(nzsDict[key]);
                }
            }
        }

        public void ChangeBounds(int[] columns, double[] lb, double[] ub)
        {
            if (_modelCreated)
            {
                throw new NotSupportedException("The model was already created!");
            }

            for (var i = 0; i < columns.Length; ++i)
            {
                vars[i].lb = lb[i];
                vars[i].ub = ub[i];
            }
        }

        public void CreateModel(bool maximize)
        {
            if (_modelCreated)
            {
                throw new NotSupportedException("The model was already created!");
            }
            InternalCreateModel(maximize);
            _modelCreated = true;
        }

        protected abstract void InternalCreateModel(bool maximize);

        public void SetParam(Param param, string value)
        {
            if (_modelCreated)
            {
                throw new NotSupportedException("The model was already created. Could not change the parameter " + param.ToString());
            }

            try
            {
                InternalSetParam(param, value);
            }
            catch (NotImplementedException)
            {
                throw new NotImplementedException(string.Format("The parameter {0} is not implemented for solver {1}", param, GetSolverName()));
            }
        }

        protected abstract void InternalSetParam(Param param, string value);

        public void Solve(bool maximize)
        {
            var memoryUsed = Process.GetCurrentProcess().PrivateMemorySize64;
            Console.WriteLine("Memory used before solve(): {0} MB, {1} B ", ((double)memoryUsed / 1024.0 / 1024.0).ToString("N2"), memoryUsed);
            CreateModel(maximize);
            InternalSolve();
            memoryUsed = Process.GetCurrentProcess().PrivateMemorySize64;
            Console.WriteLine("Memory used before solve(): {0} MB, {1} B ", ((double)memoryUsed / 1024.0 / 1024.0).ToString("N2"), memoryUsed);
        }

        protected abstract void InternalSolve();

        public abstract OptimizationStatus GetStatus();

        public abstract double GetObjVal();

        public abstract double GetMipGap();

        public abstract double[] GetSolution();

        public double[] GetObjCoef()
        {
            return vars.OrderBy(v => v.Key).Select(v => v.Value.objval).ToArray();
        }

        public void GetBounds(out double[] lb, out double[] ub)
        {
            lb = vars.Values.Select(v => v.lb).ToArray();
            ub = vars.Values.Select(v => v.ub).ToArray();
        }

        public ConstSense[] GetSenses()
        {
            return consts.Values.Select(c => c.sense).ToArray();
        }

        public double[] GetRHSs()
        {
            return consts.Values.Select(c => c.rhs).ToArray();
        }

        public double[] GetRowValues(int row)
        {
            double[] rowValues = Enumerable.Repeat(0.0, vars.Count).ToArray();
            foreach (var nz in nzs.Where(nz => nz.row == row))
            {
                rowValues[nz.col] = nz.val;
            }
            return rowValues;
        }

        public double[] GetLowerBounds()
        {
            return vars.Values.Select(v => v.lb).ToArray();
        }

        public double[] GetUpperBounds()
        {
            return vars.Values.Select(v => v.ub).ToArray();
        }

        public abstract double[] GetReducedCosts();

        public abstract double[] GetShadowPrices();

        public abstract void DeleteModel();

        public string SolverName
        {
            get
            {
                return GetSolverName();
            }
        }

        protected abstract string GetSolverName();

        public abstract void ExportConflicts(string filename);

        public abstract void ExportModel(string filename);
    }

    public class Var
    {
        public Var(string name, double objval, double lb, double ub, VarType type)
        {
            this.name = name;
            this.objval = objval;
            this.lb = lb;
            this.ub = ub;
            this.type = type;
        }

        public string name { get; set; }
        public double objval { get; set; }
        public double lb { get; set; }
        public double ub { get; set; }
        public VarType type { get; set; }
    }

    public class Const
    {
        public Const(string name, double rhs, ConstSense sense)
        {
            this.name = name;
            this.rhs = rhs;
            this.sense = sense;
        }

        public string name { get; set; }
        public double rhs { get; set; }
        public ConstSense sense { get; set; }
    }

    public class Nz
    {
        public Nz(int row, int col, double val)
        {
            this.row = row;
            this.col = col;
            this.val = val;
        }

        public int row { get; set; }
        public int col { get; set; }
        public double val;

        public override string ToString()
        {
            return string.Format("R: {0} C: {1} V: {2}", row, col, val);
        }
    }
}