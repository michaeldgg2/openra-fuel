#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using System;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("Refuels units with the `Refuelable` trait and which are located on top of this actor.")]
	public class RefuelsUnitsInfo : TraitInfo, Requires<BuildingInfo>
	{
		[Desc("The amount of fuel transferred to the recipient per interval.")]
		public readonly int FuelPerTransfer = 1;

		[Desc("Refuel transfer interval (in ticks).")]
		public readonly int TransferInterval = 1;

		[Desc("Offset relative to the building's top-left where the actor needs to be to receive fuel.")]
		public readonly CVec RefuelOffset = CVec.Zero;

		[Desc("Retrieve fuel from the player's global fuel reserve instead of the actor's own FuelTank.")]
		public readonly bool UseFuelReserve = true;

		public override object Create(ActorInitializer init) { return new RefuelsUnits(init.Self, this); }
	}

	public class RefuelsUnits : ITick, IRefuelUnits
	{
		public readonly RefuelsUnitsInfo Info;
		public readonly FuelTank FuelTank;

		public Actor CurrentUnit { get; private set; }
		FuelTank otherFuelTank;
		WPos cachedPosition;
		int ticks;

		public RefuelsUnits(Actor self, RefuelsUnitsInfo info)
		{
			Info = info;

			var source = info.UseFuelReserve ? self.Owner.PlayerActor : self;
			FuelTank = source.Trait<FuelTank>();
		}

		bool IRefuelUnits.CanRefuel(Actor self, Refuelable refuelable)
		{
			return !FuelTank.IsEmpty;
		}

		void ITick.Tick(Actor self)
		{
			if (CurrentUnit == null)
				return;

			if (cachedPosition != CurrentUnit.CenterPosition)
			{
				CurrentUnit = null;
				return;
			}

			if (--ticks > 0)
				return;

			if (otherFuelTank.IsFull)
				return;

			var amount = Math.Min(FuelTank.AvailableFuel(Info.FuelPerTransfer), otherFuelTank.ReceivableFuel(Info.FuelPerTransfer));
			if (amount > 0)
			{
				FuelTank.TakeFuel(amount);
				otherFuelTank.ReceiveFuel(self, amount);
			}

			cachedPosition = CurrentUnit.CenterPosition;
			ticks = Info.TransferInterval;
		}

		public void RefuelUnit(Actor self, Actor unit)
		{
			if (CurrentUnit != null)
				return;

			var refuelable = unit.TraitOrDefault<Refuelable>();
			if (refuelable == null || !refuelable.CanRefuelAt(self, this))
				return;

			otherFuelTank = refuelable.FuelTank;

			CurrentUnit = unit;
			cachedPosition = CurrentUnit.CenterPosition;
			ticks = Info.TransferInterval;
		}
	}
}
