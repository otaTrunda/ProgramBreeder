using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter;

namespace ProgramBreeder
{
    class SolutionProgram : ICloneable
    {
        private List<NumericNode> numericNodes;
        private List<DirectiveNode> directiveNodes;
        private List<BoolNode> booleanNodes;
        //private List<VariableWrapper> variableNodes;
        private directiveEntryPoint mainEntryPoint;
		private List<DirectiveNode> directivesList;

		/// <summary>
		/// Evaluates the given sample. Runs the program on inputs given by the sample and stores outputs of the program in sample.RealOutputs.
		/// </summary>
		/// <param name="sample"></param>
		/// <param name="interpret"></param>
		/// <returns></returns>
		public void evaluate(TrainingSample sample, Interpret interpret)
		{
			interpret.reset();
			interpret.numericInputs = sample.inputs.ToList();
			this.execute(interpret);
			for (int i = 0; i < sample.outputsCount; i++)
				sample.realOutputs[i] = interpret.outputs[i];
		}

		/// <summary>
		/// Creates a program from a list of directives. That will be executed one by one in given order. These directives should NOT have the property nextDirective set as it will be set here. If it was already set, the original value will be lost. 
		/// The method assumes that all these directives are isolated, i.e. that their nextDirective is directiveTerminal or null.
		/// </summary>
		/// <param name="list"></param>
		public SolutionProgram(List<DirectiveNode> list)
		{
			this.directivesList = list;
			this.mainEntryPoint = new directiveEntryPoint(list[0]);
			for (int i = 0; i < list.Count-1; i++)
			{
				list[i].setSuccessor(list[i + 1], list[i].successors.count - 1);
			}
			if (list.Last().type != NodeType.dirTerminal)
			{
				list.Last().setSuccessor(new directiveTerminal(), list.Last().successors.count - 1);
			}
			removeDirSequence();
		}

		/// <summary>
		/// If the program contains some node of type directivesSequence, it will correctly remove the node from the code.
		/// </summary>
		protected void removeDirSequence()
		{
			while(getAllNodes().Any(n => n is directivesSequence))
			{
				Node sequenceNode = getAllNodes().First(n => n is directivesSequence);
				Node parrent = sequenceNode.predecessor;
				Node successor = sequenceNode.successors.GetSlot(0).nodeConnectedToSlot;    //directive sequences have only one successor.
				parrent.setSuccessor(successor, sequenceNode.indexInParrentsSlot);  //circumvents the sequence node
			}
		}

		/// <summary>
		/// Creates a program from given directive. The method assumes that given directive node contains the whole program in a tree form, i.e. that its successors are already set. Method will not change the node nor any of its successors.
		/// </summary>
		/// <param name="treeRoot"></param>
		public SolutionProgram(DirectiveNode treeRoot)
		{
			this.directivesList = new List<DirectiveNode>() { treeRoot };
			this.mainEntryPoint = new directiveEntryPoint(treeRoot);
		}

		public void execute(Interpret interpreter)
        {
            DirectiveNode currentDir = mainEntryPoint;
            while (currentDir != null)
            {
                currentDir.execute(interpreter);
                currentDir = currentDir.getNextDirective();
            }
        }

		public DirectiveNode getEntryPoint()
		{
			return mainEntryPoint;
		}

		/// <summary>
		/// Returns just the first directive as a string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return getEntryPoint().toSourceCode();
		}

		/// <summary>
		/// Returns "source code" for this program
		/// </summary>
		/// <returns></returns>
		public string toSourceString()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Node> getAllNodes()
		{
			return getNodesRecur(getEntryPoint());
		}

		private IEnumerable<Node> getNodesRecur(Node current)
		{
			if (current != null)
			{
				yield return current;
				for (int i = 0; i < current.successors.count; i++)
				{
					foreach (var item in getNodesRecur(current.successors.get(i)))
					{
						yield return item;
					}
				}
			}
		}

		public object Clone()
		{
			return new SolutionProgram((DirectiveNode)getEntryPoint().getNextDirective().createDeepCopy());
		}

		public void tryRepair(ConstrainMapping constrains)
		{
			var allNodes = getAllNodes().ToList();
			//TODO... jak to udelat, aby se ten list nemenil kdyz budu nahrazovat vrcholy..? (Pak se muze stat, ze prepisu neco, co uz bylo predtim vyhozene ze stromu, ale ono si to porad pamatuje sveho predka)

		}
	}
}
