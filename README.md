# SolverLab
A generic interface in .NET to interact with different solvers

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