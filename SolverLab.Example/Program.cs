using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SolverLab;
//using SolverLab.GlpkSolver;

namespace SolverLab.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var solver = new CbcSolver.CbcSolver();
            //var solver = new CplexSolver.CplexSolver();
            //var solver = new GurobiSolver.GurobiSolver();
            //var solver = new GlpkSolver.GlpkSolver();

            //minimize    3 x1 - x2
            //subject to  -x1 + 6 x2 - x3 + x4   >= -3
            //                  7 x2      + 2 x4 =   5
            //            x1 +    x2 + x3        =   1
            //                         x3 +   x4 <=  2

            solver.AddVar("x1", 3, 0, double.PositiveInfinity, VarType.Linear);
            solver.AddVar("x2", -1, 0, double.PositiveInfinity, VarType.Linear);
            solver.AddVar("x3", 0, 0, double.PositiveInfinity, VarType.Linear);
            solver.AddVar("x4", 0, 0, double.PositiveInfinity, VarType.Linear);

            solver.AddConst("const1", -3, ConstSense.GreaterOrEqual);
            solver.AddConst("const2", 5, ConstSense.Equal);
            solver.AddConst("const3", 1, ConstSense.Equal);
            solver.AddConst("const4", 2, ConstSense.LesserOrEqual);

            solver.AddNz(0, 0, -1);
            solver.AddNz(0, 1, 6);
            solver.AddNz(0, 2, -1);
            solver.AddNz(0, 3, 1);
            solver.AddNz(1, 1, 7);
            solver.AddNz(1, 3, 2);
            solver.AddNz(2, 0, 1);
            solver.AddNz(2, 1, 1);
            solver.AddNz(2, 2, 1);
            solver.AddNz(3, 2, 1);
            solver.AddNz(3, 3, 1);

            solver.Solve(false);
            solver.ExportModel("D:\\Temp.mps");

            Console.WriteLine("The objective value is: {0} ({1}: {2})", solver.GetObjVal(), solver.SolverName, solver.Version);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit!");
                Console.ReadKey();
            }
        }
    }
}
