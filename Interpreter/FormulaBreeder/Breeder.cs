using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter;

namespace FormulaBreeder
{
    public class Breeder
    {
        List<Formula> population;


    }

    public class NodeModifier
    {
        DataSetSchema problemSchema;
        int constantsLowerLimit = -5, constantsUpperLimit = 5;
        List<Node> workingList = new List<Node>();
        Random r = new Random();

        public NodeModifier(DataSetSchema problemSchema)
        {
            createCategories();
            this.problemSchema = problemSchema;
        }

        //get replacement? get all replacements?

        private Dictionary<FunctionTypeEnum, Dictionary<NodeType, double>> categories;
        
        private void createCategories()
        {
            categories = new Dictionary<FunctionTypeEnum, Dictionary<NodeType, double>>();
            foreach (var item in (FunctionTypeEnum[])Enum.GetValues(typeof(FunctionTypeEnum)))
            {
                categories.Add(item, new Dictionary<NodeType, double>());
                foreach (var prototype in NodeFactory.prototypes)
                {
                    if (prototype.functionType.signature == item)
                        categories[item].Add(prototype.type, 0.5);
                }
            }
        }

        public NodeModifier()
        {
            createCategories();
        }

        public void getAllEquivalentReplacements(Node original, List<Node> result)
        {
            //TODO predelat. mozna pomoci visitoru? 
            //result.Clear();
            switch (original.functionType.signature)
            {
                case FunctionTypeEnum._00_10:
                    for (int i = constantsLowerLimit; i <= constantsUpperLimit; i++)
                        result.Add(new NumericConstant(i));
                    for (int i = 0; i < problemSchema.inputsType.Length; i++)
                        if (problemSchema.inputsType[i] == DataType.numerical)
                            result.Add(new NumericInput(i));
                    break;
                case FunctionTypeEnum._00_01:
                    result.Add(new BoolConstant(true));
                    result.Add(new BoolConstant(false));
                    for (int i = 0; i < problemSchema.inputsType.Length; i++)
                        if (problemSchema.inputsType[i] == DataType.logic)
                            result.Add(new BoolInput(i));
                    break;
                case FunctionTypeEnum._10_10:
                    NumericUnaryOperator originalNode = (NumericUnaryOperator)original;
                    NumericUnaryOperator newNode;
                    foreach (var item in categories[original.functionType.signature])
                    {
                        newNode = (NumericUnaryOperator)NodeFactory.createNode(item.Key);
                        newNode.experssion = (NumericNode)originalNode.experssion.Clone();
                        result.Add(newNode);
                    }
                    break;
                case FunctionTypeEnum._20_10:
                    NumericBinaryOperator originalNode1 = (NumericBinaryOperator)original;
                    NumericBinaryOperator newNode1;
                    foreach (var item in categories[original.functionType.signature])
                    {
                        newNode1 = (NumericBinaryOperator)NodeFactory.createNode(item.Key);
                        newNode1.first = (NumericNode)originalNode1.first.Clone();
                        newNode1.second = (NumericNode)originalNode1.second.Clone();
                        result.Add(newNode1);
                    }
                    break;
                case FunctionTypeEnum._20_01:
                    BoolRelationOperator originalNode2 = (BoolRelationOperator)original;
                    BoolRelationOperator newNode2;
                    foreach (var item in categories[original.functionType.signature])
                    {
                        newNode2 = (BoolRelationOperator)NodeFactory.createNode(item.Key);
                        newNode2.first = (NumericNode)originalNode2.first.Clone();
                        newNode2.second = (NumericNode)originalNode2.second.Clone();
                        result.Add(newNode2);
                    }
                    break;
                case FunctionTypeEnum._02_01:
                    BoolLogicBinaryOperator originalNode3 = (BoolLogicBinaryOperator)original;
                    BoolLogicBinaryOperator newNode3;
                    foreach (var item in categories[original.functionType.signature])
                    {
                        newNode3 = (BoolLogicBinaryOperator)NodeFactory.createNode(item.Key);
                        newNode3.first = (BoolNode)originalNode3.first.Clone();
                        newNode3.second = (BoolNode)originalNode3.second.Clone();
                        result.Add(newNode3);
                    }
                    break;
                case FunctionTypeEnum._01_01:
                    BoolUnaryLogicOperator originalNode4 = (BoolUnaryLogicOperator)original;
                    BoolUnaryLogicOperator newNode4;
                    foreach (var item in categories[original.functionType.signature])
                    {
                        newNode4 = (BoolUnaryLogicOperator)NodeFactory.createNode(item.Key);
                        newNode4.exp = (BoolNode)originalNode4.exp.Clone();
                        result.Add(newNode4);
                    }
                    break;
                case FunctionTypeEnum._01_10:
                    BoolToNumber originalNode5 = (BoolToNumber)original;
                    BoolToNumber newNode5 = (BoolToNumber)NodeFactory.createNode(original.type);
                    newNode5.expression = (BoolNode)originalNode5.expression.Clone();
                    result.Add(newNode5);
                    break;
                default:
                    break;
            }
        }

        public void getSimplifyingReplacements(Node original, List<Node> result)
        {
            //result.Clear();
            foreach (var item in NodeFactory.prototypes)
            {
                if (original.functionType.matches(item.functionType) && !original.functionType.Equals(item.functionType))
                {
                    result.Add(NodeFactory.createNode(item.type));
                }
            }
        }    
    
        public void getAllNodes(Node original, List<Node> result)
        {
            int currentStart = result.Count, currentEnd = currentStart+1;
            bool somethingAdded = true;
            result.Add(original);
            while (somethingAdded)
            {
                for (int i = currentStart; i < currentEnd; i++)
                {
                    result[i].getSuccessors(result);
                }
                somethingAdded = result.Count > currentEnd;
                currentStart = currentEnd;
                currentEnd = result.Count;
            }
        }
    
        public void mutate(Node original)
        {
            workingList.Clear();
            getAllNodes(original, workingList);
            Node parrent = workingList[r.Next(workingList.Count)];
            workingList.Clear();
            parrent.getSuccessors(workingList);
            if (workingList.Count > 0)
            {
                int index = r.Next(workingList.Count);
                Node n = workingList[index];
                workingList.Clear();
                getAllEquivalentReplacements(n, workingList);
                getSimplifyingReplacements(n, workingList);
                parrent.setSuccessor(workingList[r.Next(workingList.Count)], index);
            }
        }

    }
}
