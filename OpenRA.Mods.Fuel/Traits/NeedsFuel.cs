#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("This actor needs fuel to move.")]
	public class NeedsFuelInfo : TraitInfo, Requires<FuelTankInfo>
	{
		[Desc("Fuel consumption per cell-to-cell movement.")]
		public readonly int Consumption = 1;

		[Desc("The amount of fuel the actor consumes while stopped. Set to 0 to disable.")]
		public readonly int ConsumptionWhileStopped = 0;

		[Desc("The interval at which a stopped actor consumes fuel (in ticks).")]
		public readonly int ConsumptionWhileStoppedInterval = 0;

		[Desc("Adds range information to the actor's tooltip if set to true.")]
		public readonly bool ShowTooltip = true;

		[Desc("Kill the actor when it runs out of fuel.")]
		public readonly bool KillOnOutOfFuel = false;

		[Desc("Time (in ticks) for how long the actor can survive without fuel.")]
		public readonly int KillOnOutOfFuelDelay = 0;

		[GrantedConditionReference]
		[Desc("Condition to grant when the actor is out of fuel.")]
		public readonly string OutOfFuelCondition = null;

		public override object Create(ActorInitializer init) { return new NeedsFuel(init.Self, this); }
	}

	public class NeedsFuel : ISync, ITick, INotifyAddedToWorld, IProvideTooltipInfo
	{
		readonly Actor self;

		public readonly NeedsFuelInfo Info;
		public readonly FuelTank FuelTank;

		[Sync]
		CPos lastLocation;
		[Sync]
		bool wasEmpty;
		[Sync]
		int outOfFuelTicks;
		[Sync]
		int ticksSinceLastFuelIntake;

		int conditionToken;

		public NeedsFuel(Actor self, NeedsFuelInfo info)
		{
			this.self = self;
			Info = info;
			FuelTank = self.Trait<FuelTank>();

			wasEmpty = FuelTank.IsEmpty;
		}

		public WDist RangeFull
		{
			get { return new WDist(FuelTank.Capacity / Info.Consumption); }
		}

		public WDist Range
		{
			get { return new WDist(FuelTank.Amount / Info.Consumption); }
		}

		void ITick.Tick(Actor self)
		{
			if (!self.IsInWorld)
				return;

			if (self.Location != lastLocation)
			{
				FuelTank.TakeFuel(Info.Consumption);
				lastLocation = self.Location;
			}
			else if (Info.ConsumptionWhileStopped > 0 && --ticksSinceLastFuelIntake <= 0)
			{
				FuelTank.TakeFuel(Info.ConsumptionWhileStopped);
				ticksSinceLastFuelIntake = Info.ConsumptionWhileStoppedInterval;
			}

			if (!string.IsNullOrEmpty(Info.OutOfFuelCondition))
			{
				if (!wasEmpty && FuelTank.IsEmpty)
					conditionToken = self.GrantCondition(Info.OutOfFuelCondition);
				else if (wasEmpty && !FuelTank.IsEmpty)
					conditionToken = self.RevokeCondition(conditionToken);
			}

			if (FuelTank.IsEmpty && Info.KillOnOutOfFuel)
			{
				if (!wasEmpty)
					outOfFuelTicks = Info.KillOnOutOfFuelDelay;
				else
					--outOfFuelTicks;

				if (outOfFuelTicks <= 0)
					self.Kill(self);
			}

			wasEmpty = FuelTank.IsEmpty;
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			lastLocation = self.Location;
		}

		bool IProvideTooltipInfo.IsTooltipVisible(Player forPlayer)
		{
			return Info.ShowTooltip && self.Owner.IsAlliedWith(forPlayer);
		}

		string IProvideTooltipInfo.TooltipText { get { return $"Fuel range: {Range.Length}/{RangeFull.Length} cells"; } }
	}
}
