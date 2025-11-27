#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Fuel.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Fuel.Activities
{
	public class Refuel : Activity
	{
		readonly IMove move;
		readonly Actor host;
		readonly Target target;
		readonly RefuelsUnits refuels;
		readonly FuelTank fuelTank;
		readonly RallyPoint rallyPoint;

		public Refuel(Actor self, Actor host)
		{
			move = self.TraitOrDefault<IMove>();
			this.host = host;
			target = Target.FromActor(host);
			refuels = host.TraitOrDefault<RefuelsUnits>();
			fuelTank = self.TraitOrDefault<FuelTank>();
			rallyPoint = host.TraitOrDefault<RallyPoint>();
		}

		public override bool Tick(Actor self)
		{
			if (move == null || refuels == null || fuelTank == null)
				return true;

			QueueChild(new MoveAdjacentTo(self, target));
			QueueChild(move.MoveTo(host.Location + refuels.Info.RefuelOffset, 0));
			QueueChild(new CallFunc(() => refuels.RefuelUnit(host, self)));
			QueueChild(new WaitFor(() => fuelTank.IsFull, true));

			if (rallyPoint?.Path.Count > 0)
				self.QueueActivity(move.MoveTo(rallyPoint.Path[0], ignoreActor: host));

			return true;
		}

		public override IEnumerable<TargetLineNode> TargetLineNodes(Actor self)
		{
			if (rallyPoint != null)
				yield return new TargetLineNode(Target.FromCell(self.World, rallyPoint.Path[0]), Color.Green);
			else
				yield return new TargetLineNode(target, Color.Green);
		}
	}
}
