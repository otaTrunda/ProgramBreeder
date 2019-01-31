using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter;
using System.Threading.Tasks;

namespace Interpreter.NodeVisitors
{
	/// <summary>
	/// Returns true if the visited node is equal to pattern. Doesn't check the subtree, just the root node.
	/// </summary>
	class NodeEqualsVisitor
	{
		Node pattern;
		bool result;

		public bool visit(Node n)
		{
			return false;
		}

		public void accept(BoolConstant n)
		{

		}

	}
}
