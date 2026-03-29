using Artemis.Core.Modules;

namespace Artemis.Plugins.Payday2GSI.DataModels;

public class Payday2DataModel : DataModel
{
	public Payday2DataModel()
	{
		Level = new LevelInfo();
		Player = new PlayerInfo();
	}

	[DataModelProperty(Name = "Level")]
	public LevelInfo Level { get; set; }

	[DataModelProperty(Name = "Player")]
	public PlayerInfo Player { get; set; }

	public class LevelInfo
	{
		[DataModelProperty(Name = "Phase")]
		public string Phase { get; set; } = string.Empty;
	}

	public class PlayerInfo
	{
		[DataModelProperty(Name = "Health Total")]
		public double HealthTotal { get; set; }

		[DataModelProperty(Name = "Health Current")]
		public double HealthCurrent { get; set; }

		[DataModelProperty(Name = "Armor Total")]
		public double ArmorTotal { get; set; }

		[DataModelProperty(Name = "Armor Current")]
		public double ArmorCurrent { get; set; }

		[DataModelProperty(Name = "Is Swansong")]
		public bool IsSwansong { get; set; }

		[DataModelProperty(Name = "State")]
		public string State { get; set; } = string.Empty;

		[DataModelProperty(Name = "Suspicion")]
		public double Suspicion { get; set; }

		[DataModelProperty(Name = "Weapon Primary Left")]
		public int WeaponPrimaryLeft { get; set; }

		[DataModelProperty(Name = "Weapon Secondary Left")]
		public int WeaponSecondaryLeft { get; set; }

		[DataModelProperty(Name = "Weapon Selected")]
		public int WeaponSelected { get; set; }
	}
}
