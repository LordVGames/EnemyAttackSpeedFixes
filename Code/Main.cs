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
        internal static class StoneGolem
        {
            internal static void SetupILHooks()
            {
                IL.EntityStates.GolemMonster.ClapState.OnEnter += ClapState_OnEnter;
                IL.EntityStates.GolemMonster.ClapState.FixedUpdate += ClapState_FixedUpdate;
                IL.EntityStates.GolemMonster.ClapState.OnExit += ClapState_OnExit;
            }



            private static void ClapState_OnEnter(ILContext il)
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

            private static void ClapState_FixedUpdate(ILContext il)
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

            private static void ClapState_OnExit(ILContext il)
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
        }


        internal static class Mithrix
        {
            internal static void SetupILHooks()
            {
                IL.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlam_OnEnter;
                IL.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += WeaponSlam_FixedUpdate;
                IL.EntityStates.BrotherMonster.WeaponSlam.OnExit += WeaponSlam_OnExit;
            }

            private static void WeaponSlam_OnEnter(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdstr("WeaponSlam"),
                    x => x.MatchLdstr("WeaponSlam.playbackRate"),
                    x => x.MatchLdsfld<EntityStates.BrotherMonster.WeaponSlam>("duration")
                ))
                {
                    Log.Error("COULD NOT IL HOOK EntityStates.BrotherMonster.WeaponSlam.OnEnter");
                    LogILStuff(il, c);
                }
                else
                {
                    MakeDurationUseAttackSpeed(c);
                    c.Next.Operand = 0.05f;
                }
            }

            private static void WeaponSlam_FixedUpdate(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall(out _),
                    x => x.MatchLdsfld<EntityStates.BrotherMonster.WeaponSlam>("duration")
                ))
                {
                    Log.Error("COULD NOT IL HOOK EntityStates.BrotherMonster.WeaponSlam.OnEnter PART 1");
                    LogILStuff(il, c);
                }
                else
                {
                    MakeDurationUseAttackSpeed(c);
                }

                if (!c.TryGotoNext(MoveType.AfterLabel,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<EntityStates.EntityState>("outer"),
                    x => x.MatchCallvirt(out _)
                ))
                {
                    Log.Error("COULD NOT IL HOOK EntityStates.BrotherMonster.WeaponSlam.OnEnter PART 2");
                    LogILStuff(il, c);
                }
                else
                {
                    // maybe helps the attack not coming out at high attack speeds?
                    // idk anymore so im leaving it in
                    ResetEntityStateAnimatorSpeed(c);
                }
            }

            private static void WeaponSlam_OnExit(ILContext il)
            {
                ILCursor c = new(il);

                // don't need to go to any specific line, we can just throw our own il at the start and it's fine
                c.Index = 0;
                ResetEntityStateAnimatorSpeed(c);
            }



            private static void MakeDurationUseAttackSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.BrotherMonster.WeaponSlam, float>>((slamStateDuration, currentSlamState) =>
                {                    

                    //Log.Warning($"currentSlamState.attackSpeedStat is {currentSlamState.attackSpeedStat}");
                    //Log.Warning($"slamStateDuration is {slamStateDuration}");


                    float newDuration = (slamStateDuration / currentSlamState.attackSpeedStat);
                    //Log.Debug(currentSlamState.modelAnimator.GetFloat("WeaponSlam.playbackRate"));
                    //Log.Warning($"newDuration is {newDuration}");
                    //Log.Warning($"currentSlamState.fixedAge is {currentSlamState.fixedAge}");
                    return newDuration;
                });
            }

            private static void ResetEntityStateAnimatorSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<EntityStates.BrotherMonster.WeaponSlam>>((slamState) =>
                {
                    slamState.modelAnimator.speed = 1;
                });
            }
        }


        private static void LogILStuff(ILContext il, ILCursor c)
        {
            Log.Warning($"cursor is {c}");
            Log.Warning($"il is {il}");
        }
    }
}