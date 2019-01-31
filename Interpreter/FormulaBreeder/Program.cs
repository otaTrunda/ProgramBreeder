using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter;

namespace FormulaBreeder
{
    class Program
    {
        static void Main(string[] args)
        {
            NodeModifier modif = new NodeModifier(DataSetSchema.prototype);

            List<Node> replacements = new List<Node>();

            OutputNode n = new OutputNode();

            for (int i = 0; i < 100000; i++)
            {
                Console.WriteLine(n);
                modif.mutate(n);
            }
        }
    }
}
