using System;

namespace Assets.Code
{
	public struct SimpleBuildingPattern
	{
		public string Name { get; set; }

		public char Character { get; set; }

		public SimpleBuildingPattern(string name, char character)
		{
			Name = name;
			Character = character;
		}
	}
}

