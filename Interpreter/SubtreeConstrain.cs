using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
	/// <summary>
	/// Represent a constrain imposed in subtree of a node. The constrain may dictate that there must be some type of node present in the subtree or some other constrains.
	/// </summary>
	public abstract class SubtreeConstrain
	{
		/// <summary>
		/// True if the node meets this condition, false otherwise
		/// </summary>
		/// <param name="n"></param>
		/// <returns>True if the node <paramref name="n"/> meets this condition, false otherwise</returns>
		public abstract bool isMet(Node n);

		/// <summary>
		/// Checks whether the node meets this condition and if not, it will attempt to change the subtree such that the condition is met.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public abstract (bool repairAttempted, bool repairSuccesfull) repair(Node n);
	}

	/// <summary>
	/// Enforces presence of some specified subtree within the subtree of a node.
	/// </summary>
	public class MustContainSubtreeConstrain : SubtreeConstrain
	{
		private Random r;
		private Node rootOfSubtreeToContain;
		Lazy<List<Node>> placeHolder;

		public override bool isMet(Node n)
		{
			return n.containsSubtree(rootOfSubtreeToContain);
		}

		public override (bool repairAttempted, bool repairSuccesfull) repair(Node n)
		{
			if (isMet(n))
				return (false, false);

			placeHolder.Value.Clear();
			n.gatherAllNodesInSubtree(placeHolder.Value);

			var sameTypeNodes = placeHolder.Value.Where(q => q.type == rootOfSubtreeToContain.type);
			if (sameTypeNodes.Any())	//if it contains nodes of the same type, one of them is selected and replaced by copy of the required subtree
			{
				var list = sameTypeNodes.ToList();
				var selectedNode = list[r.Next(list.Count)];
				var predecessor = selectedNode.predecessor;
				predecessor.setSuccessor(rootOfSubtreeToContain.createDeepCopy(), selectedNode.indexInParrentsSlot);
				return (true, true);
			}

			var sameClassNodes = placeHolder.Value.Where(q => q.getNodeClass() == rootOfSubtreeToContain.getNodeClass());
			if (sameClassNodes.Any())    //if it contains nodes of the same class, one of them is selected and replaced by copy of the required subtree
			{
				var list = sameClassNodes.ToList();
				var selectedNode = list[r.Next(list.Count)];
				var predecessor = selectedNode.predecessor;
				predecessor.setSuccessor(rootOfSubtreeToContain.createDeepCopy(), selectedNode.indexInParrentsSlot);
				return (true, true);
			}
			return (true, false);
		}

		public MustContainSubtreeConstrain(Random r, Node rootOfSubtreeToContain)
		{
			this.r = r;
			this.rootOfSubtreeToContain = rootOfSubtreeToContain;
			this.placeHolder = new Lazy<List<Node>>(() => new List<Node>());
		}
	}

	/// <summary>
	/// Allows to assign constrains to nodes by some rule. It allows for example to state that "in every FOR loop, the loop variable must be accesed in the body".
	/// </summary>
	public class ConstrainMapping
	{
		/// <summary>
		/// Mapping that to every nodetype assigns a function that takes a node (with previously stated type) and produces a pair (node, constrain) such that the "constrain" must hold in the "node"
		/// </summary>
		Dictionary<NodeType, List<Func<Node, (Node, SubtreeConstrain)>>> mapping;

		public List<(Node, SubtreeConstrain)> getConstrains(Node n)
		{
			if (mapping.ContainsKey(n.type))
				return mapping[n.type].Select(t => t(n)).ToList();
			return Support.emptyNodeConstrainList;
		}

		public void addConstrain(NodeType type, Func<Node, (Node, SubtreeConstrain)> constrainCreator)
		{
			if (!mapping.ContainsKey(type))
				mapping.Add(type, new List<Func<Node, (Node, SubtreeConstrain)>>());
			mapping[type].Add(constrainCreator);
		}

		public ConstrainMapping()
		{
			this.mapping = new Dictionary<NodeType, List<Func<Node, (Node, SubtreeConstrain)>>>();
		}

		public static ConstrainMapping createStandardConstrains(Random r)
		{
			ConstrainMapping res = new ConstrainMapping();

			//states that the loop variable has to be accessed in body of the loop
			Func<Node, (Node, SubtreeConstrain)> forLoopConstrain = new Func<Node, (Node, SubtreeConstrain)>(n =>
			{
				var forDirective = n as directiveFOR;
				int indexOfSlotOfLoopVariable = forDirective.successors.getSlots().Where(s => s.argumentDescription == "iterator variable index").Single().index;
				Node requiredSubtree = new ValueGetter(new NumericConstant(indexOfSlotOfLoopVariable));
				int indexOfSlotWithLoopBody = forDirective.successors.getSlots().Where(s => s.argumentDescription == "body").Single().index;
				return (forDirective.successors.get(indexOfSlotWithLoopBody), new MustContainSubtreeConstrain(r, requiredSubtree));
			});
			res.addConstrain(NodeType.dirFor, forLoopConstrain);

			//states that the input to the program has to be accessed somewhere in the program
			Func<Node, (Node, SubtreeConstrain)> globalConstrain = new Func<Node, (Node, SubtreeConstrain)>(n =>
			{
				Node requiredSubtree = new InputNode(0);
				return (n, new MustContainSubtreeConstrain(r, requiredSubtree));
			});
			res.addConstrain(NodeType.dirEntryPoint, globalConstrain);




			return res;
		}

	}

	public static class ConstrainEvaluationSupport
	{
		public static IEnumerable<Node> GetAllNodesFromSubtree(Node root)
		{
			yield return root;
			foreach (var item in root.successors.getSlots().Select(s => s.nodeConnectedToSlot))
			{
				foreach (var succ in GetAllNodesFromSubtree(item))
				{
					yield return succ;
				}
			}
		}

	}

	/// <summary>
	/// Represents a required relation between newly added nodes and nodes already present
	/// </summary>
	class Constrain
	{
		private string description;

		public string getDescription()
			=> description;

		public Func<List<Node>, Node, bool> canBeAdded;
		public Func<List<Node>, bool> canBeSatisfied;

		public Constrain(string description, Func<List<Node>, Node, bool> canBeAdded = null, Func<List<Node>, bool> canBeSatisfied = null)
		{
			this.description = description;
			this.canBeAdded = canBeAdded == null ? new Func<List<Node>, Node, bool>((x, y) => true) : canBeAdded;
			this.canBeSatisfied = canBeSatisfied == null ? new Func<List<Node>, bool>(q => true) : canBeSatisfied;
		}

		public static List<Constrain> standardConstrains = new List<Constrain>()
		{
			/*
					new Constrain("body of every for loop must contain the loop variable",
						new Func<List<Node>, Node, bool>((x, y) => true),
						new Func<List<Node>, bool>(q =>
						{
							var forLoopNodes = q.Where(n => n.type == NodeType.dirFor).Select(n => (directiveFOR)n);
							return forLoopNodes.All(n =>
							{
								var loopVarIndex = 
							})
							return true;
						}));
			*/
			/*
			new Constrain("",
				canBeAdded: new Func<List<Node>, Node, bool>((x, n) =>
				{
					if (n.type == NodeType.ListGetFirst || n.type == NodeType.ListGetLast || n.type == NodeType.ListGetValue || n.type == NodeType.ListSizeGetter)
					{
						var listIndex = n.successors.GetSlot(0);

					}
				}
				)
			*/
		};


	}



}
