# SolverLab
A generic interface in .NET to interact with different solvers:

0. IBM CPLEX Optimizer (http://www-01.ibm.com/software/commerce/optimization/cplex-optimizer/)
0. Gurobi Optimizer (http://www.gurobi.com/)
0. GNU Linear Programming Kit (GLPK) (http://www.gnu.org/software/glpk/)
0. CBC (http://www.coin-or.org/projects/Cbc.xml)


## Example

```
using SolverLab;
using SolverLab.GlpkSolver;

namespace Forac.LogiLab.CloudSolver
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var solver = new GlpkSolver.GlpkSolver();
			solver.AddVar("x1", 3, 0, double.PositiveInfinity, VarType.Linear);
			solver.AddConst("const1", -3, ConstSense.GreaterOrEqual);
			solver.AddNz(0, 0, -1);
			solver.Solve(false);
		}
	}
}
```
