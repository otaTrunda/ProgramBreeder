using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter;

namespace ProgramBreeder
{
	class SearchMethodsSupport
	{
		protected static Random r = new Random();

		/// <summary>
		/// Creates a new node that is "default" for given class. For "directive" the default value is TerminalDirective, for "boolean" the default is BoolConstant with value "false", and for numeric the default is "NumericConstant" with value 0.
		/// If NodeCreationConstrains are given, then other result might be returned. E.g. if the constrains forbid constants, it will return input-reading-node instead of a constant one, etc.
		/// </summary>
		/// <param name="cl"></param>
		/// <returns></returns>
		public static Node createDefaultNode(NodeClass cl, NodeCreationConstrains constrains = null)
		{
			switch (cl)
			{
				case NodeClass.directive:
					return new directiveTerminal();
				case NodeClass.numeric:
					if (constrains?.Contains(NodeCreationContrainsOption.cantBeConstant) == true)
						return new InputNode(0);
					return new NumericConstant(0);
				case NodeClass.boolean:
					if (constrains?.Contains(NodeCreationContrainsOption.cantBeConstant) == true)
						return new BoolEqualsOperator(new InputNode(0), new NumericConstant(0));
					return new BoolConstant(false);
				default:
					throw new Exception();
			}
		}

		/// <summary>
		/// Creates new node of given type. The returned node is a prototype with default value. 
		/// E.g. for numeric constant it will always have a value zero, in case of node that requires successors, these seuccessors will be set to null. (It is necessarry to set them later.)
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Node createPrototypeNode(NodeType type)
		{
			return NodeFactory.createNode(type);
		}

		/// <summary>
		/// Creates a fully initialized node of given type. If the node requires successors, these successors will also be created and assigned to the node. Successors will be default-value-nodes of required types.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Node createRandomNode(NodeType type, NodeCreationConstrains constrains = null)
		{
			Node n;
			if (type == NodeType.BoolConstant)
			{
				n = new BoolConstant(r.NextDouble() < 0.5);
				return n;
			}
			if (type == NodeType.NumConst)
			{
				n = new NumericConstant(r.Next(10));
				return n;
			}
			if (type == NodeType.NumInput)
			{
				n = new InputNode(r.Next(10));
				return n;
			}
			n = createPrototypeNode(type);

			for (int i = 0; i < n.successors.count; i++)
			{
				n.setSuccessor(createDefaultNode(n.successors.GetSlot(i).argumentClass, constrains), i);
			}
			return n;
		}

		/// <summary>
		/// Returns all posible node of given type. If the node requires successors, these successors will also be created and assigned to the node. Successors will be default-value-nodes of required types.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<Node> enumerateAllNodes(NodeType type, NodeCreationConstrains constrains = null)
		{
			Node n;
			int constantsLimit = 2;
			switch (type)
			{
				case NodeType.BoolConstant:
					yield return new BoolConstant(false);
					yield return new BoolConstant(true);
					break;

				case NodeType.NumConst:
					for (int i = 0; i < constantsLimit; i++)
					{
						yield return new NumericConstant(i);
					}
					break;

				case NodeType.NumInput:
					for (int i = 0; i < constantsLimit; i++)
					{
						yield return new InputNode(i);
					}
					break;

				default:		
					n = createPrototypeNode(type);
					yield return n;
					break;
			}
		}

		/// <summary>
		/// Creates a new fully initialized node whose type is selected randomly based on distribution in given NodeTypeFrequencyProfile object. Node that require value to be set (like numeric constants) will receive a random number between 0 and 10 as their value.
		/// If the node requires successors to be specified, such successors will be created and default-value-node will be used.
		/// </summary>
		/// <param name="cl"></param>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static Node createRandomNode(NodeClass cl, NodeTypeFrequencyProfile profile, NodeCreationConstrains constrains = null)
		{
			NodeType t = profile.getRandomNodeType(cl, constrains);
			return createRandomNode(t, constrains);
		}

		/// <summary>
		/// Creates a new fully initialized node whose type is selected randomly based on distribution in given NodeTypeFrequencyProfile object. Node that require value to be set (like numeric constants) will receive a random number between 0 and 10 as their value.
		/// If the node requires successors to be specified, such successors will be created and default-value-node will be used.
		/// </summary>
		/// <param name="cl"></param>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static Node createRandomNode(NodeClass cl, NodeType? parrentType, NodeTypeRelativizedFrequencyProfile profile, NodeCreationConstrains constrains = null)
		{
			NodeType t = profile.getRandomNodeType(cl, parrentType, constrains);
			return createRandomNode(t, constrains);
		}

		/// <summary>
		/// Creates a tree of fully instantiated nodes with given depth. Nodes at the leaf level will be default-value-nodes.
		/// </summary>
		/// <param name="cl"></param>
		/// <param name="profile"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		public static Node createRandomTree(NodeClass cl, NodeTypeFrequencyProfile profile, int depth, NodeCreationConstrains contrains = null)
		{
			if (depth == 0)
				return createDefaultNode(cl, contrains);
			Node root = createRandomNode(cl, profile, contrains);
			foreach (var slot in root.successors.getSlots())
			{
				slot.setNodeConnectedToSlot(createRandomTree(slot.argumentClass, profile, depth - 1, contrains));
			}
			return root;
		}

		/// <summary>
		/// Creates a tree of fully instantiated nodes with given depth. Nodes at the leaf level will be default-value-nodes.
		/// </summary>
		/// <param name="cl"></param>
		/// <param name="profile"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		public static Node createRandomTree(NodeClass cl, NodeTypeRelativizedFrequencyProfile profile, int depth, NodeType? parrentType = null, NodeCreationConstrains contrains = null)
		{
			if (depth == 0)
				return createDefaultNode(cl, contrains);
			Node root = createRandomNode(cl, parrentType, profile, contrains);
			
			foreach (var slot in root.successors.getSlots())
			{
				slot.setNodeConnectedToSlot(createRandomTree(slot.argumentClass, profile, depth - 1, root?.type, contrains));
			}
			return root;
		}

		public static IEnumerable<Node> enumerateAllTrees(NodeClass rootNodeClass, int depth, NodeCreationConstrains constrains = null)
		{
			if (depth <= 0)	//it should not get here anyway. Unless it was already called with depth = 0;
				yield return createDefaultNode(rootNodeClass, constrains);

			if (depth == 1)
			{
				foreach (var type in EnumUtils.getTerminalTypes(rootNodeClass))
				{
					foreach (var node in enumerateAllNodes(type, constrains))
					{
						yield return node;
					}
				}
			}

			else
			{
				foreach (var type in EnumUtils.getTerminalTypes(rootNodeClass))
				{
					foreach (var node in enumerateAllNodes(type, constrains))
					{
						yield return node;
					}
				}
				foreach (var type in EnumUtils.getNonterminalTypes(rootNodeClass))
				{
					foreach (var node in enumerateAllNodes(type, constrains))
					{
						foreach (var tree in fillSlotsWithAllPossibilities(node, depth-1, 0, constrains))
						{
							yield return tree;							
						}
					}
				}
			}
		}

		private static IEnumerable<Node> fillSlotsWithAllPossibilities(Node n, int depth, int slotIndex, NodeCreationConstrains constrains = null)
		{
			if (slotIndex >= n.successors.count)
				yield return n.createDeepCopy();
			else
			{
				var slot = n.successors.GetSlot(slotIndex);
				foreach (var subtree in enumerateAllTrees(slot.argumentClass, depth, constrains))
				{
					slot.setNodeConnectedToSlot(subtree.createDeepCopy());
					foreach (var rest in fillSlotsWithAllPossibilities(n, depth, slotIndex+1, constrains))
					{
						yield return rest.createDeepCopy();
					}
				}
			}
		}

	}

	/// <summary>
	/// Stores information about relative frequencies of occurence of various types of nodes in structure of typical programs. This can then be used when randomly generating new nodes during the search.
	/// Profile can be automatically computed from given samples of typical programs.
	/// </summary>
	class NodeTypeFrequencyProfile
	{
		protected static Random r = new Random();
		Dictionary<NodeType, int> frequencies;
		int sumOfFrequencies;
		Dictionary<NodeClass, List<NodeType>> typesByClasses;
		Dictionary<NodeClass, int> sumsOfFrequenciesByClasses;

		/// <summary>
		/// Adds one occurence of every type of node into the frequency profile
		/// </summary>
		private void addOneOfEach()
		{
			foreach (var item in EnumUtils.getAllTypes())
			{
				increaseCount(item, 1);
			}
		}

		/// <summary>
		/// Creates a frequency profile of occurence of nodes of specific types in given set of programs. If atLeastOneOfEach is set to true, frequency of all nodes will initially be set to one, which leads to non-zero probability of generating every type of node, 
		/// even if it is never encountered in any of given programs. If it is set to false, initial frequencies will be set to zero and nodetypes that were never encountered in samples will never be generated.
		/// </summary>
		/// <param name="programs"></param>
		/// <param name="atLeastOneOfEach"></param>
		/// <returns></returns>
		public static NodeTypeFrequencyProfile createProfile(List<SolutionProgram> programs, bool atLeastOneOfEach)
		{
			NodeTypeFrequencyProfile result = new NodeTypeFrequencyProfile();
			foreach (var program in programs)
			{
				foreach (var node in program.getAllNodes())
				{
					result.increaseCount(node.type, 1);
				}
			}
			if (atLeastOneOfEach)
				result.addOneOfEach();

			result.sortTypeByFrequencies();
			return result;
		}

		public static NodeTypeFrequencyProfile createProfile(IEnumerable<Node> allNodes, bool atLeastOneOfEach)
		{
			NodeTypeFrequencyProfile result = new NodeTypeFrequencyProfile();
			foreach (var node in allNodes)
			{
				result.increaseCount(node.type, 1);
			}
			
			if (atLeastOneOfEach)
				result.addOneOfEach();

			result.sortTypeByFrequencies();
			return result;
		}

		public NodeTypeFrequencyProfile()
		{
			this.frequencies = new Dictionary<NodeType, int>();
			sumOfFrequencies = 0;
			typesByClasses = new Dictionary<NodeClass, List<NodeType>>();
			sumsOfFrequenciesByClasses = new Dictionary<NodeClass, int>();
			foreach (var item in EnumUtils.getAllNodeClasses())
			{
				typesByClasses.Add(item, EnumUtils.getTypesByClass(item).ToList());
				sumsOfFrequenciesByClasses.Add(item, 0);
			}
		}

		private void increaseCount(NodeType t, int additionalAmount)
		{
			 if (t == NodeType.dirEntryPoint)
			     return; //dont want entry points in the result

			if (!frequencies.ContainsKey(t))
				frequencies.Add(t, 0);
			frequencies[t] += additionalAmount;
			sumOfFrequencies += additionalAmount;
			sumsOfFrequenciesByClasses[EnumUtils.getClassFromType(t)] += additionalAmount;
		}

		protected double getRelativeFrequency(NodeType t)
		{
			if (!frequencies.ContainsKey(t))
				return 0;
			return (double)frequencies[t] / sumOfFrequencies;
		}

		protected int getFrequency(NodeType t)
		{
			if (!frequencies.ContainsKey(t))
				return 0;
			return frequencies[t];
		}

		/// <summary>
		/// Generates a random node type based on a distribution computed from current frequencies.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public virtual NodeType getRandomNodeType(NodeClass c, NodeCreationConstrains constrains = null)
		{
			int rouletteWheelIndex = 0;
			int sampledNumber = r.Next(sumsOfFrequenciesByClasses[c]);
			foreach (var cummulativeSum in getCummulativeFrequencies(c))
			{
				if (sampledNumber < cummulativeSum)
					return typesByClasses[c][rouletteWheelIndex];

				rouletteWheelIndex++;
			}
			throw new Exception();
		}

		private IEnumerable<int> getCummulativeFrequencies(NodeClass c)
		{
			int sum = 0;
			foreach (var item in typesByClasses[c])
			{
				sum += getFrequency(item);
				yield return sum;
			}
		}

		/// <summary>
		/// Sorts types by frequencies from largest to smallest. This improves performance during random generation.
		/// </summary>
		private void sortTypeByFrequencies()
		{
			foreach (var item in typesByClasses.Keys)
			{
				typesByClasses[item].Sort((a, b) => getFrequency(b) - getFrequency(a));
			}
		}

		public string printFrequencies()
		{
			StringBuilder sb = new StringBuilder();
			List<NodeType> sortedTypes = new List<NodeType>(EnumUtils.getAllTypes());
			sortedTypes.Sort((a, b) => getFrequency(b) - getFrequency(a));
			foreach (var item in sortedTypes)
			{
				sb.AppendLine(item.ToString() + "\t" + getFrequency(item));
			}
			return sb.ToString();
		}

		public string printRelativeFrequencies()
		{
			StringBuilder sb = new StringBuilder();
			List<NodeType> sortedTypes = new List<NodeType>(EnumUtils.getAllTypes());
			sortedTypes.Sort((a, b) => getFrequency(b) - getFrequency(a));
			foreach (var item in sortedTypes)
			{
				sb.AppendLine(item.ToString() + "\t" + (getRelativeFrequency(item) * 100).ToString("0.##") + "%");
			}
			return sb.ToString();
		}
	}

	/// <summary>
	/// Stores the frequencies separatelly for each type of node. I.e. for every nodetype K it stores frequencies of nodes that occur as direct successors of K.
	/// </summary>
	class NodeTypeRelativizedFrequencyProfile : NodeTypeFrequencyProfile
	{
		protected Dictionary<NodeType, NodeTypeFrequencyProfile> relativizedProfiles;
		protected NodeTypeFrequencyProfile simpleProfile;

		public static NodeTypeRelativizedFrequencyProfile createProfile(List<SolutionProgram> programs)
		{
			int minPrograms = 100;
			int repeat = programs.Count < minPrograms ? minPrograms - programs.Count : 1;
			NodeTypeRelativizedFrequencyProfile result = new NodeTypeRelativizedFrequencyProfile();
			result.simpleProfile = createProfile(programs, atLeastOneOfEach: true);

			foreach (var program in programs)
			{
				foreach (var node in program.getAllNodes())
				{
					if (!result.relativizedProfiles.ContainsKey(node.type))
					{
						NodeTypeFrequencyProfile p = NodeTypeFrequencyProfile.createProfile(enumarateAllNodesThatAreSuccessorsOfType(node.type, programs, repeat), atLeastOneOfEach: true);
						result.relativizedProfiles.Add(node.type, p);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Enumerates all nodes from given programs that occur as a direct successor of some node that has type of <paramref name="type"/>
		/// </summary>
		/// <param name="type"></param>
		/// <param name="programs"></param>
		/// <returns></returns>
		protected static IEnumerable<Node> enumarateAllNodesThatAreSuccessorsOfType(NodeType type, List<SolutionProgram> programs, int repeats)
		{
			foreach (var program in programs)
				foreach (var node in program.getAllNodes())
					if (node.predecessor?.type == type)
						for (int i = 0; i < repeats; i++)
							yield return node;
		}

		public NodeTypeRelativizedFrequencyProfile()
		{
			this.relativizedProfiles = new Dictionary<NodeType, NodeTypeFrequencyProfile>();
		}

		/// <summary>
		/// Generates a random node type based on a distribution computed from current frequencies.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public virtual NodeType getRandomNodeType(NodeClass c, NodeType? parrentType, NodeCreationConstrains constrains = null)
		{
			if (parrentType == null || !relativizedProfiles.ContainsKey(parrentType.Value))
				return simpleProfile.getRandomNodeType(c, constrains);
			
			return relativizedProfiles[parrentType.Value].getRandomNodeType(c, constrains);
		}

		public override NodeType getRandomNodeType(NodeClass c, NodeCreationConstrains constrains = null)
		{
			return simpleProfile.getRandomNodeType(c, constrains);
		}
	}

	class EvaluatedEntity<T>
	{
		public T item;
		public double value;

		public EvaluatedEntity(T item, double v)
		{
			this.item = item;
			this.value = v;
		}
	}

}
