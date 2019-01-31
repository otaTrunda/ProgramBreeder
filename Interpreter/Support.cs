using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
	public static class Support
	{
		public static List<Node> emptyNodeList = new List<Node>();
		public static List<SubtreeConstrain> emptyConstrainList = new List<SubtreeConstrain>();
		public static List<(Node, SubtreeConstrain)> emptyNodeConstrainList = new List<(Node, SubtreeConstrain)>();
	}
}
