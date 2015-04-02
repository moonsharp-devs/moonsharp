using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tutorials
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				List<Tuple<string, Type>> tutorialTypes = new List<Tuple<string, Type>>();

				foreach (Type t in Assembly.GetExecutingAssembly()
					.GetTypes()
					.Where(t => t.GetCustomAttributes(typeof(TutorialAttribute)).Any())
					.OrderBy(t => t.Name))
				{
					tutorialTypes.Add(new Tuple<string, Type>(t.Name, t));
				}

				Type chosenType = DoMenu("Choose chapter", tutorialTypes);

				if (chosenType == null)
					return;

				List<Tuple<string, MethodInfo>> tutorialMethods = new List<Tuple<string, MethodInfo>>();

				foreach (MethodInfo mi in chosenType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Where(mi => mi.GetCustomAttributes(typeof(TutorialAttribute)).Any()))
				{
					tutorialMethods.Add(new Tuple<string, MethodInfo>(mi.Name, mi));
				}

				MethodInfo chosenMethod = DoMenu(string.Format("{0} - Choose tutorial", chosenType), tutorialMethods);

				if (chosenMethod == null)
					continue;

				Console.Clear();
				Banner(string.Format("{0} . {1}", chosenType.Name, chosenMethod.Name));

				Object o = chosenMethod.Invoke(null, new object[0]);

				if (chosenMethod.ReturnType != typeof(void))
					Console.WriteLine(o);

				Console.WriteLine();
				Console.WriteLine("press any key...");

				Console.ReadKey();
			}
		}

		private static T DoMenu<T>(string title, List<Tuple<string, T>> choices)
		{
			while (true)
			{
				Console.Clear();
				Banner(title);

				for (int i = 0; i < choices.Count; i++)
				{
					Console.WriteLine("{0,2} - {1}", i + 1, choices[i].Item1);
				}

				Console.WriteLine("99 - CANCEL / EXIT");

				Console.Write(" >");
				string choice = Console.ReadLine();
				int chosen;
				if (int.TryParse(choice, out chosen))
				{
					if (chosen == 99) return default(T);
					if (chosen >= 1 && chosen <= choices.Count) return choices[chosen-1].Item2;
					Console.WriteLine("Invalid choice");
				}
			}
		}

		private static void Banner(string title)
		{
			Console.WriteLine("=====================================================================");
			Console.WriteLine("  " + title.ToUpper());
			Console.WriteLine("=====================================================================");
		}
	}
}
