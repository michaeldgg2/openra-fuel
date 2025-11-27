#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Fuel.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.Scripting.Properties
{
	[ScriptPropertyGroup("Fuel")]
	public class NeedsFuelProperties : ScriptActorProperties, Requires<NeedsFuelInfo>
	{
		readonly NeedsFuel needsFuel;

		public NeedsFuelProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			needsFuel = self.Trait<NeedsFuel>();
		}

		[Desc("Returns the total fuel capacity.")]
		public int FuelCapacity { get { return needsFuel.FuelTank.Capacity; } }

		[Desc("Returns the current fuel level.")]
		public int FuelLevel { get { return needsFuel.FuelTank.Amount; } }

		[Desc("Returns true if the actor is out of fuel.")]
		public bool IsOutOfFuel { get { return needsFuel.FuelTank.IsEmpty; } }

		[Desc("Returns true if the actor is low on fuel.")]
		public bool IsLowOnFuel { get { return needsFuel.FuelTank.IsLowOnFuel; } }

		[Desc("Returns true if the actor has full fuel.")]
		public bool HasFullFuel { get { return needsFuel.FuelTank.IsFull; } }

		[Desc("Returns the remaining range.")]
		public WDist FuelRange { get { return needsFuel.Range; } }

		[Desc("Returns the maximum range on full fuel.")]
		public WDist FullFuelRange { get { return needsFuel.RangeFull; } }
	}

	[ScriptPropertyGroup("Fuel")]
	public class RefuelableProperties : ScriptActorProperties, Requires<RefuelableInfo>
	{
		readonly Refuelable refuelable;

		public RefuelableProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			refuelable = self.Trait<Refuelable>();
		}

		[Desc("Refuel at the target actor")]
		public void Refuel(Actor host)
		{
			if (refuelable.CanRefuelAt(host))
				Self.QueueActivity(refuelable.Refuel(Self, host));
		}
	}
}
