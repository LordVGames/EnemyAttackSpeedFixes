using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace EnemyAttackSpeedFixes
{
    internal static class Main
    {
        internal static void ClapState_OnEnter(ILContext il)
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Clap"),
                x => x.MatchLdstr("Clap.playbackRate"),
                x => x.MatchLdsfld<EntityStates.GolemMonster.ClapState>("duration")
            ))
            {
                Log.Error("COULD NOT IL HOOK EntityStates.GolemMonster.ClapState.OnEnter");
                LogILStuff(il, c);
            }
            else
            {
                MakeDurationUseAttackSpeed(c);
            }
        }

        internal static void ClapState_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall(out _),
                x => x.MatchLdsfld<EntityStates.GolemMonster.ClapState>("duration")
            ))
            {
                Log.Error("COULD NOT IL HOOK EntityStates.GolemMonster.ClapState.FixedUpdate");
                LogILStuff(il, c);
            }
            else
            {
                MakeDurationUseAttackSpeed(c);
            }
        }

        internal static void ClapState_OnExit(ILContext il)
        {
            ILCursor c = new(il);

            // don't need to go to any specific line, we can just throw our own il at the start and it's fine
            c.Index = 0;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EntityStates.GolemMonster.ClapState>>((clapState) =>
            {
                clapState.modelAnimator.speed = 1;
            });
        }



        private static void MakeDurationUseAttackSpeed(ILCursor c)
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, EntityStates.GolemMonster.ClapState, float>>((clapStateDuration, currentClapState) =>
            {
                currentClapState.modelAnimator.speed = 1 * currentClapState.attackSpeedStat;
                return clapStateDuration / currentClapState.attackSpeedStat;
            });
        }

        private static void LogILStuff(ILContext il, ILCursor c)
        {
            Log.Warning($"cursor is {c}");
            Log.Warning($"il is {il}");
        }
    }
}