using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter;

namespace ProgramBreeder
{
    abstract class SearchOperator
    {
		protected static Random r => Program.r;
    }

    abstract class UnarySearchOperator : SearchOperator
	{
		public abstract void modify(SolutionProgram p);
    }

    abstract class BinarySearchOperator
    {

    }

	/// <summary>
	/// Picks a random node from given program and replaces one of its successors by a randomly created tree.
	/// </summary>
	class PointMutation : UnarySearchOperator
	{
		private NodeTypeFrequencyProfile profile;
		private int treeDepth;

		public override void modify(SolutionProgram p)
		{
			List<Node> allnodes = p.getAllNodes().ToList();
			if (allnodes.Count == 0)
				return;

			Node selectedNode = allnodes[r.Next(allnodes.Count)];
			if (selectedNode.successors.count == 0)
				return;

			int successorIndex = r.Next(selectedNode.successors.count);
			if (profile is NodeTypeRelativizedFrequencyProfile)
				selectedNode.setSuccessor(SearchMethodsSupport.createRandomTree(selectedNode.successors.GetSlot(successorIndex).argumentClass, (NodeTypeRelativizedFrequencyProfile)profile, treeDepth, selectedNode.type), successorIndex);
			else selectedNode.setSuccessor(SearchMethodsSupport.createRandomTree(selectedNode.successors.GetSlot(successorIndex).argumentClass, profile, treeDepth), successorIndex);
		}

		public PointMutation(NodeTypeFrequencyProfile profile, int treeDepth)
		{
			this.profile = profile;
			this.treeDepth = treeDepth;
		}
	}

	abstract class Selector<T>
	{
		protected static Random r => Program.r;
		public abstract EvaluatedEntity<T> select(List<EvaluatedEntity<T>> candidates);
	}

	class RouletteSelector<T> : Selector<T>
	{
		public override EvaluatedEntity<T> select(List<EvaluatedEntity<T>> candidates)
		{
			double totalSum = candidates.Sum(c => c.value);
            if (totalSum == 0)
            {
                return candidates[r.Next(candidates.Count)];
            }

			double randomVal = r.NextDouble() * totalSum;

			int rouletteIndex = 0;
			foreach (var cummulativeSum in getCummulativeSums(candidates))
			{
				if (randomVal < cummulativeSum)
					return candidates[rouletteIndex];
				rouletteIndex++;
			}
			throw new Exception();
		}

		protected static IEnumerable<double> getCummulativeSums(List<EvaluatedEntity<T>> candidates)
		{
			double sum = 0;
			foreach (var item in candidates)
			{
				sum += item.value;
				yield return sum;
			}
		}
	}

	class TournamentSelector<T> : Selector<T>
	{
		int tournamentSize = 5;
		public override EvaluatedEntity<T> select(List<EvaluatedEntity<T>> candidates)
		{
			EvaluatedEntity<T> best = candidates[r.Next(candidates.Count)];
			for (int i = 1; i < tournamentSize; i++)
			{
				EvaluatedEntity<T> candidate = candidates[r.Next(candidates.Count)];
				if (candidate.value > best.value)
					best = candidate;
			}
			return best;
		}

		public TournamentSelector(int tournamentSize = 5)
		{
			this.tournamentSize = tournamentSize;
		}
	}

    class TreeIterator
    {
        public static IEnumerable<Node> enumerateAllTrees(NodeClass cl, int depth, NodeTypeFrequencyProfile profile)
        {
            if (depth <= 0)
                yield break;
            if (depth == 1)
                foreach (var item in generateLeafNodes(cl, profile))
                {
                    yield return item;
                }
            else
            {
                foreach (var root in generateRootNodes(cl, profile))
                {
                    foreach (var tree in generateSubtrees(root, 0, depth - 1, profile))
                    {
                        yield return tree;
                    }
                }
            }
        }

        protected static IEnumerable<Node> generateSubtrees(Node root, int succIndex, int maxDepth, NodeTypeFrequencyProfile profile)
        {
            if (succIndex >= root.successors.count)
                yield return root.createDeepCopy();
            else
            {
                Slot s = root.successors.GetSlot(succIndex);
                if (maxDepth == 1)
                    foreach (var succ in generateLeafNodes(s.argumentClass, profile))
                    {
                        s.setNodeConnectedToSlot(succ);
                        foreach (var result in generateSubtrees(root, succIndex + 1, maxDepth, profile))
                            yield return result;
                    }
                
                else
                    foreach (var succ in generateRootNodes(s.argumentClass, profile))
                        for (int subtreeDepth = 1; subtreeDepth < maxDepth; subtreeDepth++)
                            foreach (var subtree in generateSubtrees(succ, 0, subtreeDepth, profile))
                            {
                                s.setNodeConnectedToSlot(succ);
                                foreach (var result in generateSubtrees(root, succIndex + 1, maxDepth, profile))
                                    yield return result;
                            } 
            }
        }

        protected static IEnumerable<Node> generateLeafNodes(NodeClass c, NodeTypeFrequencyProfile profile)
        {
            Node n = null;
            switch (c)
            {
                case NodeClass.directive:
                    n = new directiveTerminal();
                    yield return n;
                    break;
                case NodeClass.numeric:
                    for (int i = 0; i < 2; i++)
                    {
                        n = new NumericConstant(i);
                        yield return n;
                    }
                    break;
                case NodeClass.boolean:
                    yield return new BoolConstant(false);
                    yield return new BoolConstant(true);
                    break;
                default:
                    break;
            }
        }

        protected static IEnumerable<Node> generateRootNodes(NodeClass c, NodeTypeFrequencyProfile profile)
        {
            foreach (var item in EnumUtils.getTypesByClass(c))
            {
                Node n = SearchMethodsSupport.createPrototypeNode(item);
                if (n.successors.count <= 0)
                    continue;
                yield return n;
            }
        }
    }

}
