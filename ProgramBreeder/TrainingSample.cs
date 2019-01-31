using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgramBreeder
{
	/// <summary>
	/// Represenets a single sample of inputs - outputs mapping that the program should implement. Can be used to train programs.
	/// </summary>
	class TrainingSample
	{
		public int[] inputs, desiredOutputs, realOutputs;

		public int inputsCount => inputs.Length;
		public int outputsCount => desiredOutputs.Length;

		public TrainingSample(int[] inputs, int[] desiredOutputs)
		{
			this.inputs = inputs;
			this.desiredOutputs = desiredOutputs;
			this.realOutputs = new int[outputsCount];
		}

		public TrainingSample()
		{

		}

		public bool isCorrectlyAnswered
		{
			get
			{
				for (int i = 0; i < outputsCount; i++)
				{
					if (desiredOutputs[i] != realOutputs[i])
						return false;
				}
				return true;
			}
		}

		public override string ToString()
		{
			return inputs2String + "->" + desiredOutputs2String;
		}

		public string inputs2String => arrayToString(inputs);
		public string desiredOutputs2String => arrayToString(desiredOutputs);
		public string realOutputs2String => arrayToString(realOutputs);

		private static string arrayToString(int[] array)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			foreach (var item in array)
				sb.Append(item + " ");

			sb.Remove(sb.Length - 1, 1);
			sb.Append("]");
			return sb.ToString();
		}
	}

	abstract class TrainingSamplesGenerator
	{
		protected static Random r = new Random(123);
		/// <summary>
		/// Returns a sequence of training samples. Never call Count or other dangerous extension methods on this as the resulting sequence might be potentially infinite!!! 
		/// If you only want a fixed number of samples, use the method with argument.
		/// The sequence might contain same samples multi-times.
		/// The method might be randomized, i.e. different calls will give different results.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<TrainingSample> generateSamples();

		/// <summary>
		/// Returns only a fixed number of samples. 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public virtual IEnumerable<TrainingSample> generateSamples(int count)
		{
			return generateSamples().Take(count);
		}
	}

	abstract class SingleInputSingleOutputSamplesGenerator : TrainingSamplesGenerator
	{
		protected virtual int getInput()
		{
			return r.Next(1000000);
		}

		protected abstract int getOutput(int input);

		public override IEnumerable<TrainingSample> generateSamples()
		{
			while (true)
			{
				int input = getInput();
				int output = getOutput(input);
				yield return new TrainingSample(new int[] { input }, new int[] { output });
			}
		}
	}

	class PrimalityTestingSamplesGenerator : SingleInputSingleOutputSamplesGenerator
	{
		protected override int getOutput(int input)
		{
			return isPrime(input) ? 1 : 0;
		}

		private bool isPrime(int number)
		{
			int n = number,
				i = 2;
			while (i <= n/i)
			{
				if (number % i == 0)
					return false;
				i++;
			}
			return true;
		}
	}

	/// <summary>
	/// The task is to predict the number of steps before the Erratic sequence reaches 1 from given starting point. Erratic sequence is given by formula: X[i+1] = (X[i] % 2 == 0) ? X[i] / 2 : X[i] * 3 + 1
	/// </summary>
	class ErraticSequenceLengthSamplesGenerator : SingleInputSingleOutputSamplesGenerator
	{
		protected override int getOutput(int input)
		{
			return getSteps(input);
		}

		private int getSteps(int seed)
		{
			int r = seed, steps = 0;
			while (r > 1)
			{
				if (r % 2 == 0)
					r = r / 2;
				else r = r * 3 + 1;
				steps++;
			}
			return steps;
		}
	}

	/// <summary>
	/// The task is simply to copy given input to output.
	/// </summary>
	class SimpleCopySamplesGenerator : SingleInputSingleOutputSamplesGenerator
	{
		public SimpleCopySamplesGenerator()
		{
		}

		protected override int getOutput(int input)
		{
			return input;
		}
	}

	class MaximumOfTwo_SamplesGenerator : TrainingSamplesGenerator
	{
		public override IEnumerable<TrainingSample> generateSamples()
		{
			int first = r.Next(1000000);
			int second = r.Next(1000000);
			int result = first > second ? first : second;
			yield return new TrainingSample(new int[] { first, second }, new int[] { result });
		}
	}

	class AlwaysOne_SamplesGenerator : SingleInputSingleOutputSamplesGenerator
	{
		protected override int getOutput(int input)
		{
			return 1;
		}
	}
}
