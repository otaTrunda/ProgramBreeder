using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	public static class Printing
	{
		public static void PrintMsg(string msg, bool quiet)
		{
			if (!quiet)
				Console.WriteLine(msg);
		}

	}
}
