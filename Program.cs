using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using GoldBars;
using System.Linq;

/*
 * author Maria Grazia
 * version 20/12/2017
 * 
 * This project aims to create maps by reading from external file. A map contains information about nodes and relative 
 * connections as well as delivery details (gold bars to deliver from a starting point to the ending point).
 * Bars travels through either villages or town. 
 * 
*/

namespace GoldBars
{
	/*
	 * Class Location is a superclass for villages and towns. Fields are the name and the entry tax.
	 * Containing constructors, getters and setters and specific methods.
	 * The entry tax is set to 1 per each enter by default.
	 */
	class Location
	{
		protected string name;
		protected int entryTax;
		protected int bars;
		protected int defaultEntryTax = 1;

		/* Constructor. */
		public Location(string name)
		{
			this.name = name;
			this.entryTax = defaultEntryTax;
		}

		public override Boolean Equals(Object obj)
		{
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (GetType() != obj.GetType())
				return false;
			Location other = (Location)obj;
			if (name == null)
			{
				if (other.name != null)
					return false;
			}
			else if (!name.Equals(other.name))
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			int prime = 31;
			int result = 1;
			result = prime * result + ((name == null) ? 0 : name.GetHashCode());
			return result;
		}

		public string GetName()
		{
			return name;
		}

		public void SetName(string name)
		{
			this.name = name;
		}

		public int GetEntryTax()
		{
			return entryTax;
		}

		public void SetEntryTax(int entryTax)
		{
			this.entryTax = entryTax;
		}

		/*
		 * Method to return the tax amount for a given nuber of bars.
		 */
		public virtual int BarsForEntryTax(int finalBars)
		{
			return entryTax + finalBars;
		}

		public int GetBars()
		{
			return this.bars;
		}

		public void SetBars(int bars)
		{
			this.bars = bars;
		}
	}

	/*
	 * Village is a subclass of Location.
	 * It inherits all the features of the parent class.
	 */
	class Village : Location
	{
		public Village(string name)
			: base(name)
		{
		}
	}

	/*
	 * Town is subclass of Location.
	 * It inherits everything but the default value of entry tax. It changes according to number of bars carried:
	 * 1 bars every 20 carried. Method BarsForEntryTax() is ovedrridden for this reason.
	 */
	class Town : Location
	{
		public Town(string name)
			: base(name)
		{
		}

		/*
		 * Given the number of bars to deliver, the method computes how many additional bars were supposed to be held 
		 * before entering the town.
		 */
		public override int BarsForEntryTax(int finalBars)
		{
			entryTax = 0;
			for (int i = 1; i <= finalBars; i += 19)
			{
				entryTax++;
			}
			return finalBars + entryTax;
		}

	}

	/*
	 * Class creating the maps by reading from external file.
	 * MyMap has fields locations (a set of locations contained in the map, each matching a set of connections), 
	 * finalBars (the number of bars to be delivered after tax), route (staring and ending locations).
	 */
	class MyMap
	{
		private Dictionary<Location, ArrayList> locations;
		private int finalBars;
		private Location[] route;

		/*
		 * Constructor.
		 */
		public MyMap(Dictionary<Location, ArrayList> locations, int finalBars, Location[] route)
		{
			this.locations = locations;
			this.finalBars = finalBars;
			this.route = route;
		}

		// Getters and setters.

		public Dictionary<Location, ArrayList> GetLocations()
		{
			return locations;
		}

		public void SetLocations(Dictionary<Location, ArrayList> locations)
		{
			this.locations = locations;
		}

		public int GetFinalBars()
		{
			return finalBars;
		}

		public void SetFinalBars(int finalBars)
		{
			this.finalBars = finalBars;
		}

		public Location[] GetRoute()
		{
			return route;
		}

		public void SetRoute(Location[] route)
		{
			this.route = route;
		}

		/*
		 * Convertin a char value into a Location objects (used when reading from file).
		 * Lowercase instances will be Villages; uppercase Towns.
		 */
		public static Location CreateLocFromChar(char letter)
		{
			Location l;
			if (Char.IsLower(letter))
			{
				l = new Village(Char.ToString(letter));
			}
			else {
				l = new Town(Char.ToString(letter));
			}
			return l;
		}

		/*
		 * Generating output for the maps contains in a file.
		 * Format: "Map case 1: a [Z], Z [a]. Delivery info: 19 bars from a to Z."
		 */
		public void Print(MyMap m)
		{
			foreach (Location e in m.locations.Keys)
			{
				Console.Write(e.GetName() + " [");
				foreach (Location el in m.locations[e])
				{
					Console.Write(el.GetName());
				}
				Console.Write("] ");
			}
			Console.Write("Delivery info: " + m.finalBars + " bars from " + m.route[0].GetName() + " to " + m.route[1].GetName() + ".");
		}

		/*
		 * Creating MyMaps by reading from file. Parameter 'path' is the file to read from.
		 */
		public static ArrayList CreateMaps(string path)
		{
			ArrayList finalMaps = new ArrayList(); // To store all the maps contained in the file.
			try
			{
				using (StreamReader sr = File.OpenText(path))
				{
					char[] arr; // To store location in each line.
					string line = string.Empty;
					while ((line = sr.ReadLine()) != null)
					{
						/* 1) For the field 'locations' of MyMap. */

						int edges = Convert.ToInt32(line); // Number of connections per map.
						if (edges == -1) // End of file.
						{
							break;
						}
						Dictionary<Location, ArrayList> l = new Dictionary<Location, ArrayList>();
						int finalBars = 0;
						String[] info;
						Location[] route = new Location[2];
						MyMap m = new MyMap(l, finalBars, route);
						for (int i = 0; i < edges; i++) // Assign neighbours nodes to each location in the map.
						{
							line = sr.ReadLine();
							arr = line.ToCharArray();
							Location one = CreateLocFromChar(arr[0]);
							Location two = CreateLocFromChar(arr[2]);
							if (!l.ContainsKey(one) && !l.ContainsKey(two))
							{
								ArrayList alOne = new ArrayList();
								ArrayList alTwo = new ArrayList();
								alOne.Add(two);
								l[one] = alOne;
								alTwo.Add(one);
								l[two] = alTwo;
							}
							else {
								l[one].Add(two);
								if (!l.ContainsKey(two))
								{
									ArrayList al1 = new ArrayList();
									al1.Add(one);
									l[two] = al1;
								}
								else {
									l[two].Add(one);
								}
							}
						}

						/* 2) For the fields 'finalBars' and 'route' of MyMap. */

						line = sr.ReadLine();
						info = line.Split(' ');
						finalBars = Convert.ToInt32(info[0]);
						m.SetFinalBars(finalBars);
						Location start = CreateLocFromChar(info[1][0]);
						Location end = CreateLocFromChar(info[2][0]);
						route[0] = start;
						route[1] = end;
						m.SetRoute(route);

						finalMaps.Add(m); // Add MyMap to the list, before creating the next ones (if any).

					}
					sr.Close();
				}
			}
			catch (Exception)
			{
				Console.WriteLine("Error occurred.");

			}

			/* Console output all MyMap. */
			int counter = 1;
			foreach (MyMap m in finalMaps)
			{
				Console.Write("Map case " + counter++ + ": ");
				m.Print(m);
				Console.WriteLine();
			}
			return finalMaps;
		}

		/*
		 * Helper method for BuildPath(). Building a path for each of the neighbours of the starting location.
		 */
		public ArrayList BuildPathHelper(Dictionary<Location, ArrayList> locations, ArrayList path, Location end)
		{
			Location current = (Location)path[path.Count - 1];
			foreach (Location l in locations[current])
			{
				if (l.Equals(end))
				{
					path.Add(l);
					return path;
				}
				else if (path.Contains(l))
				{
				}
				else
				{
					path.Add(l);
					BuildPathHelper(locations, path, end);
					current = l;
				}
			}
			return path;
		}

		/*
		 * Building paths from the neighbours of the starting point.
		 */
		public ArrayList BuildPath(Dictionary<Location, ArrayList> locations, Location start, Location end)
		{
			ArrayList finalList = new ArrayList();
			{
				if (start.Equals(end))
				{
					Console.WriteLine("Not valid path.");
				}
				else
				{
					foreach (Location l in locations[start])
					{
						ArrayList path = new ArrayList();
						path.Add(start);
						path.Add(l);
						path = BuildPathHelper(locations, path, end);
						finalList.Add(path);
					}
				}
				return finalList;
			}
		}

		/*
		 * Halper method. Calculating the bars needed for each path, in order to deliver the right number of bars for one MyMap. 
		 */
		public int barsPerPathHelper(ArrayList path, int finalBars, Location start, Location end)
		{
			path.Reverse();
			path.RemoveAt(path.Count - 1);
			int singleEntry = 0;
			foreach (Location l in path)
			{
				singleEntry = l.BarsForEntryTax(finalBars);
				finalBars = singleEntry;
			}
			return finalBars;
		}

		/*
		 * Calculating the bars needed for each path for all MyMaps in the file and returning the number of bars needed
		 * at the beginning of each path.
		 */
		public ArrayList barsPerPath(ArrayList paths, int finalBars, Location start, Location end)
		{
			ArrayList taxPerPath = new ArrayList();
			foreach (ArrayList al in paths)
			{
				taxPerPath.Add(barsPerPathHelper(al, finalBars, start, end));

			}
			return taxPerPath;
		}

		/*
		 * Printing the number of bars needed at the beginning of the cheapest path for all MyMaps in the file.
		 * Format:	"Case 1: 20"
		 * 			"Case 2: 44".
		 */
		public static void PrintBarsPerPath(String path)
		{
			ArrayList m = CreateMaps(path);
			int[] array = new int[m.Capacity];
			ArrayList listOfTax = new ArrayList();
			int counter = 1;
			foreach (MyMap mm in m)
			{
				Console.Write("Case " + counter++ + ": ");
				foreach (Int32 tax in mm.barsPerPath(mm.BuildPath(mm.locations, mm.route[0], mm.route[1]), mm.finalBars, mm.route[0], mm.route[1]))
				{
					listOfTax.Add(tax);
				}
				array = listOfTax.ToArray(typeof(int)) as int[];
				int cheapestPath = array.Min();
				//int cheapestPath = listOfTax.IndexOf(Collections.min(listOfTax));
				Console.WriteLine(cheapestPath);
				listOfTax.Clear();
			}

		}
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			/* Please past in the following method your path file: */
			MyMap.PrintBarsPerPath("input.txt");

		}
	}
}
