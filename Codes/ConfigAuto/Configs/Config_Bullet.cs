//本文件为自动生成，请不要手动修改
using System;
using System.Collections.Generic;
namespace ConfigAuto
{
	public partial class Config_Bullet
	{
		private static Config_Bullet _Bullet;
		public static Config_Bullet Bullet
		{
			get
			{
				if (_Bullet == null)
					_Bullet = new()
					{
						data = new()
						{
							moveSpeed = 15f,
							bullet = new Dictionary<Int32, bullet>
							{
								{1, new bullet {
									id = 1,
									Prefab = @"Prefab/Bullet Case",
									numerics = new Dictionary<String, Int32>
									{
										{@"speed", 60},
										{@"damage", 5},
										{@"range", 100},
									},
								}},
								{2, new bullet {
									id = 2,
									Prefab = @"Prefab/Bullet Case",
									numerics = new Dictionary<String, Int32>
									{
										{@"speed", 65},
										{@"damage", 10},
										{@"range", 150},
									},
								}},
								{3, new bullet {
									id = 3,
									Prefab = @"Prefab/Bullet Case",
									numerics = new Dictionary<String, Int32>
									{
										{@"speed", 70},
										{@"damage", 15},
										{@"range", 200},
									},
								}},
								{4, new bullet {
									id = 4,
									Prefab = @"Prefab/Bullet Case",
									numerics = new Dictionary<String, Int32>
									{
										{@"speed", 75},
										{@"damage", 20},
										{@"range", 250},
									},
								}},
							},
						},
					};
				return _Bullet;
			}
		}
		public Rootdata data {get;set;}
		public partial class Rootdata
		{
			public Single moveSpeed {get;set;}
			public Dictionary<Int32, bullet> bullet {get;set;}
		}
		public partial class bullet
		{
			public Int32 id {get;set;}
			public String Prefab {get;set;}
			public Dictionary<String, Int32> numerics {get;set;}
		}

    }
}