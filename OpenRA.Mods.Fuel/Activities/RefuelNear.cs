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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Fuel.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Fuel.Activities
{
	public class RefuelNear : Activity
	{
		readonly IMove move;
		readonly Target target;
		readonly RefuelsUnitsNear refuelsNear;

		public RefuelNear(Actor self, Actor host)
		{
			move = self.TraitOrDefault<IMove>();
			target = Target.FromActor(host);
			refuelsNear = host.TraitOrDefault<RefuelsUnitsNear>();
		}

		public override bool Tick(Actor self)
		{
			if (move == null || refuelsNear == null)
				return true;

			QueueChild(move.MoveWithinRange(target, refuelsNear.Info.Range));

			return true;
		}

		public override IEnumerable<TargetLineNode> TargetLineNodes(Actor self)
		{
			yield return new TargetLineNode(target, Color.Green);
		}
	}
}
