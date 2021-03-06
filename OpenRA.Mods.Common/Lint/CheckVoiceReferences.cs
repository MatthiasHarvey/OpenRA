﻿#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Linq;
using System.Reflection;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Lint
{
	public class CheckVoiceReferences : ILintPass
	{
		public void Run(Action<string> emitError, Action<string> emitWarning, Map map)
		{
			foreach (var actorInfo in map.Rules.Actors)
			{
				foreach (var traitInfo in actorInfo.Value.Traits.WithInterface<ITraitInfo>())
				{
					var fields = traitInfo.GetType().GetFields().Where(f => f.HasAttribute<VoiceSetReferenceAttribute>());
					foreach (var field in fields)
					{
						var voiceSets = LintExts.GetFieldValues(traitInfo, field, emitError);
						foreach (var voiceSet in voiceSets)
						{
							if (string.IsNullOrEmpty(voiceSet))
								continue;

							CheckVoices(actorInfo.Value, emitError, map, voiceSet);
						}
					}
				}
			}
		}

		void CheckVoices(ActorInfo actorInfo, Action<string> emitError, Map map, string voiceSet)
		{
			var soundInfo = map.Rules.Voices[voiceSet.ToLowerInvariant()];

			foreach (var traitInfo in actorInfo.Traits.WithInterface<ITraitInfo>())
			{
				var fields = traitInfo.GetType().GetFields().Where(f => f.HasAttribute<VoiceReferenceAttribute>());
				foreach (var field in fields)
				{
					var voices = LintExts.GetFieldValues(traitInfo, field, emitError);
					foreach (var voice in voices)
					{
						if (string.IsNullOrEmpty(voice))
							continue;

						if (!soundInfo.Voices.Keys.Contains(voice))
							emitError("Actor {0} using voice set {1} does not define {2} voice required by {3}.".F(actorInfo.Name, voiceSet, voice, traitInfo));
					}
				}
			}
		}
	}
}