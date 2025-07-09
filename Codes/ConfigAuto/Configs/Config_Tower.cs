//本文件为自动生成，请不要手动修改
using System;
using System.Collections.Generic;
namespace ConfigAuto
{
	public partial class Config_Tower
	{
		private static Config_Tower _Tower;
		public static Config_Tower Tower
		{
			get
			{
				if (_Tower == null)
					_Tower = new()
					{
						data = new()
						{
							towers = new()
							{

								new()
								{
									level = 1,
									Prefab = @"Prefab/Weapon Hammer",
									shootRadius = 40f,
									rotateSpeed = 60,
									shootCD = 0.2f,
								},
								new()
								{
									level = 2,
									Prefab = @"Weapon HandGun",
									shootRadius = 45f,
									rotateSpeed = 80,
									shootCD = 0.2f,
								},
								new()
								{
									level = 3,
									Prefab = @"Weapon SubMachineGun",
									shootRadius = 50f,
									rotateSpeed = 100,
									shootCD = 0.2f,
								},
							},
						},
					};
				return _Tower;
			}
		}
		public Rootdata data {get;set;}
		public partial class Rootdata
		{
			public List<towers> towers {get;set;}
		}
		public partial class towers
		{
			public Int32 level {get;set;}
			public String Prefab {get;set;}
			public Single shootRadius {get;set;}
			public Int32 rotateSpeed {get;set;}
			public Single shootCD {get;set;}
		}

    }
}