//本文件为自动生成，请不要手动修改
using System;
using System.Collections.Generic;
namespace ConfigAuto
{
	public partial class Config_Player
	{
		private static Config_Player _Player;
		public static Config_Player Player
		{
			get
			{
				if (_Player == null)
					_Player = new()
					{
						data = new()
						{
							shootRadius = 30f,
						},
					};
				return _Player;
			}
		}
		public Rootdata data {get;set;}
		public partial class Rootdata
		{
			public Single shootRadius {get;set;}
		}

    }
}