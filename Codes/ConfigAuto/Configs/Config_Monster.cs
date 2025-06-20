//本文件为自动生成，请不要手动修改
using System;
using System.Collections.Generic;
namespace ConfigAuto
{
	public partial class Config_Monster
	{
		private static Config_Monster _Monster;
		public static Config_Monster Monster
		{
			get
			{
				if (_Monster == null)
					_Monster = new()
					{
						data = new()
						{
							moveSpeed = 15f,
							alertRound = 30f,
							monsters = new Dictionary<Int32, monsters>
							{
								{1, new monsters {
									id = 1,
									Prefab = @"Prefab/Enemy A",
									size = 3,
									numerics = new Dictionary<String, Int32>
									{
										{@"hp", 10},
										{@"att", 1},
										{@"def", 1},
									},
								}},
								{2, new monsters {
									id = 2,
									Prefab = @"Prefab/Enemy B",
									size = 3,
									numerics = new Dictionary<String, Int32>
									{
										{@"hp", 20},
										{@"att", 2},
										{@"def", 1},
									},
								}},
								{3, new monsters {
									id = 3,
									Prefab = @"Prefab/Enemy C",
									size = 3,
									numerics = new Dictionary<String, Int32>
									{
										{@"hp", 30},
										{@"att", 3},
										{@"def", 1},
									},
								}},
								{4, new monsters {
									id = 4,
									Prefab = @"Prefab/Enemy D",
									size = 3,
									numerics = new Dictionary<String, Int32>
									{
										{@"hp", 40},
										{@"att", 4},
										{@"def", 1},
									},
								}},
							},
						},
					};
				return _Monster;
			}
		}
		public Rootdata data {get;set;}
		public partial class Rootdata
		{
			public Single moveSpeed {get;set;}
			public Single alertRound {get;set;}
			public Dictionary<Int32, monsters> monsters {get;set;}
		}
		public partial class monsters
		{
			public Int32 id {get;set;}
			public String Prefab {get;set;}
			public Int32 size {get;set;}
			public Dictionary<String, Int32> numerics {get;set;}
		}

    }
}