using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaBreeder
{
    public enum DataType
    {
        logic,
        categorial,
        text,
        character,
        numerical
    }
    public class DataSet
    {
        public static string[] separators = new string[] { " ", "\t", ";", "; " };
        public List<DataPoint> records;
        public DataSetSchema schema;

        public void readFromFile(string fileName)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string line = reader.ReadLine();
                schema = new DataSetSchema(line);
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    DataPoint point = new DataPoint();
                    point.readFromString(line, schema);
                    this.records.Add(point);
                }
            }
        }
    }

    public class DataPoint
    {
        public DataSetSchema schema;

        public List<int> integerData;
        public List<string> stringData;
        public List<bool> logicData;
        public List<char> characterData;

        public void addIntegerValue(int val)
        {
            this.integerData.Add(val);
        }

        public void addCharValue(char val)
        {
            this.characterData.Add(val);
        }

        public void addLogicValue(bool val)
        {
            this.logicData.Add(val);
        }
        public void addStringValue(string val)
        {
            this.stringData.Add(val);
        }

        public DataPoint()
        {
            this.characterData = new List<char>();
            this.integerData = new List<int>();
            this.logicData = new List<bool>();
            this.stringData = new List<string>();
        }
    
        public void readFromString(string data, DataSetSchema schema)
        {
            this.schema = schema;
            var splitted = data.Split(DataSet.separators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < schema.numberIfInputs; i++)
            {
                switch(schema.inputsType[i])
                {
                    case DataType.logic:
                        addLogicValue(splitted[i] == "TRUE" || splitted[i] == "true" || splitted[i] == "True" || splitted[i] == "1");
                        break;
                    case DataType.numerical:
                        addIntegerValue(int.Parse(splitted[i]));
                        break;
                    case DataType.character:
                        addCharValue(splitted[i][0]);
                        break;
                    case DataType.text:
                        addStringValue(splitted[i]);
                            break;
                }
            }
            for (int i = 0; i < schema.numberOfOutputs; i++)
            {
                switch (schema.outputsType[i])
                {
                    case DataType.logic:
                        addLogicValue(splitted[schema.numberIfInputs + i] == "TRUE" || splitted[schema.numberIfInputs + i] == "true" || splitted[schema.numberIfInputs + i] == "True" || splitted[schema.numberIfInputs + i] == "1");
                        break;
                    case DataType.numerical:
                        addIntegerValue(int.Parse(splitted[schema.numberIfInputs + i]));
                        break;
                    case DataType.character:
                        addCharValue(splitted[schema.numberIfInputs + i][0]);
                        break;
                    case DataType.text:
                        addStringValue(splitted[schema.numberIfInputs + i]);
                        break;
                }
            }
        }
    }

    public class DataSetSchema
    {
        public int numberIfInputs, numberOfOutputs;
        public DataType[] inputsType, outputsType;

        public DataSetSchema(string schemaString)
        {
            var splitted = schemaString.Split(DataSet.separators, StringSplitOptions.RemoveEmptyEntries);
            List<DataType> inputs = new List<DataType>();
            List<DataType> outputs = new List<DataType>();
            foreach (var item in splitted)
            {
                switch(item)
                {
                    case "bool":
                        inputs.Add(DataType.logic);
                        break;
                    case "int":
                        inputs.Add(DataType.numerical);
                        break;
                    case "string":
                        inputs.Add(DataType.text);
                        break;
                    case "char":
                        inputs.Add(DataType.character);
                        break;
                    case "bool*":
                        outputs.Add(DataType.logic);
                        break;
                    case "int*":
                        outputs.Add(DataType.numerical);
                        break;
                    case "string*":
                        outputs.Add(DataType.text);
                        break;
                    case "char*":
                        outputs.Add(DataType.character);
                        break;
                    default:
                        throw new Exception();
                }
            }
            this.numberIfInputs = inputs.Count;
            this.numberOfOutputs = outputs.Count;
            this.inputsType = inputs.ToArray();
            this.outputsType = outputsType.ToArray();
        }

        public DataSetSchema()
        {

        }

        public static DataSetSchema prototype
        {
            get
            {
                DataSetSchema s = new DataSetSchema();
                s.inputsType = new DataType[2];
                s.inputsType[0] = DataType.logic;
                s.inputsType[1] = DataType.numerical;
                s.numberIfInputs = 2;
                s.numberOfOutputs = 1;
                s.outputsType = new DataType[1];
                s.outputsType[0] = DataType.logic;
                return s;
            }
        }
    
    }
}
