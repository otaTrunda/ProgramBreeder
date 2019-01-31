using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramBreeder
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Returns index of maximum element in the sequence. If the sequence is empty, it returns -1.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static int IndexOfMax<T>(this IEnumerable<T> source)
		{
			IComparer<T> comparer = Comparer<T>.Default;
			using (var iterator = source.GetEnumerator())
			{
				if (!iterator.MoveNext())
				{
					return -1;
				}
				int maxIndex = 0;
				T maxElement = iterator.Current;
				int index = 0;
				while (iterator.MoveNext())
				{
					index++;
					T element = iterator.Current;
					if (comparer.Compare(element, maxElement) > 0)
					{
						maxElement = element;
						maxIndex = index;
					}
				}
				return maxIndex;
			}
		}

		/// <summary>
		/// Returns index of minimum element in the sequence. If the sequence is empty, it returns -1.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static int IndexOfMin<T>(this IEnumerable<T> source)
		{
			IComparer<T> comparer = Comparer<T>.Default;
			using (var iterator = source.GetEnumerator())
			{
				if (!iterator.MoveNext())
				{
					return -1;
				}
				int minIndex = 0;
				T minElement = iterator.Current;
				int index = 0;
				while (iterator.MoveNext())
				{
					index++;
					T element = iterator.Current;
					if (comparer.Compare(element, minElement) < 0)
					{
						minElement = element;
						minIndex = index;
					}
				}
				return minIndex;
			}
		}

		public static T RandomElement<T>(this List<T> source, Random r)
		{
			if (source.Count <= 0)
				throw new IndexOutOfRangeException();
			return source[r.Next(source.Count)];
		}

		[Obsolete]
		public static T RandomElement<T>(this HashSet<T> source, Random r)
		{
			if (source.Count <= 0)
				throw new IndexOutOfRangeException();
			int randomIndex = r.Next(source.Count);
			return source.Skip(randomIndex).First();
		}

		/// <summary>
		/// Iterates throught all elements in the list in random order
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		public static IEnumerable<T> Randomly<T>(this List<T> source, Random r)
		{
			List<int> remainingIndices = Enumerable.Range(0, source.Count).ToList();
			int indexOfLast = remainingIndices.Count;

			while (indexOfLast > 0)
			{
				int randomIndex = r.Next(indexOfLast);
				int selectedIndex = remainingIndices[randomIndex];
				T selectedElement = source[selectedIndex];
				remainingIndices[randomIndex] = remainingIndices[indexOfLast];
				indexOfLast--;
				yield return selectedElement;
			}
		}

		public static IEnumerable<T> Yield<T>(this T obj)
		{
			yield return obj;
		}
		public static IEnumerable<T> Forever<T>(this T obj)
		{
			while (true)
				yield return obj;
		}

		/// <summary>
		/// Returns random double value that is greater or equal to given <paramref name="lowerBound"/> and less than given <paramref name="upperBound"/>, uniformly distributed.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="lowerBound"></param>
		/// <param name="upperBound"></param>
		/// <returns></returns>
		public static double NextDouble(this Random r, double lowerBound, double upperBound)
		{
			return lowerBound + r.NextDouble() * (upperBound - lowerBound);
		}

	}
}
