using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;

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
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                    LogILStuff(il, c);
                }
                else
                {
                    MakeDurationAndAnimatorUseAttackSpeed(c);
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
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 1");
                    LogILStuff(il, c);
                }
                else
                {
                    MakeDurationAndAnimatorUseAttackSpeed(c);
                }



                if (!c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<EntityStates.EntityState>("outer")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 2");
                    LogILStuff(il, c);
                    return;
                }

                ResetAnimatorSpeed(c);
            }

            private static void ClapState_OnExit(ILContext il)
            {
                ILCursor c = new(il);

                // don't need to go to any specific line, we can just throw our own il at the start and it's fine
                ResetAnimatorSpeed(c);
            }



            private static void MakeDurationAndAnimatorUseAttackSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.GolemMonster.ClapState, float>>((clapStateDuration, currentClapState) =>
                {
                    currentClapState.modelAnimator.speed = 1 * currentClapState.attackSpeedStat;
                    return clapStateDuration / currentClapState.attackSpeedStat;
                });
            }

            private static void MakeDurationUseAttackSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.GolemMonster.ClapState, float>>((clapStateDuration, currentClapState) =>
                {
                    return clapStateDuration / currentClapState.attackSpeedStat;
                });
            }

            private static void ResetAnimatorSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<EntityStates.GolemMonster.ClapState>>((clapState) =>
                {
                    clapState.modelAnimator.speed = 1;
                });
            }
        }


        internal static class StoneTitan
        {
            internal static void SetupILHooks()
            {
                IL.EntityStates.TitanMonster.FireFist.OnEnter += FireFist_OnEnter;
                IL.EntityStates.TitanMonster.FireFist.FixedUpdate += FireFist_FixedUpdate;
                IL.EntityStates.TitanMonster.FireFist.PlaceSingleDelayBlast += FireFist_PlaceSingleDelayBlast;
                IL.EntityStates.TitanMonster.FireFist.OnExit += FireFist_OnExit;

                IL.RoR2.TitanRockController.Start += TitanRockController_Start;
                IL.RoR2.TitanRockController.FixedUpdate += TitanRockController_FixedUpdate;
                IL.RoR2.TitanRockController.FixedUpdateServer += TitanRockController_FixedUpdateServer;

                IL.EntityStates.TitanMonster.FireMegaLaser.FixedUpdate += FireMegaLaser_FixedUpdate;
                IL.EntityStates.TitanMonster.FireMegaLaser.FireBullet += FireMegaLaser_FireBullet;
            }



            private static void FireFist_OnEnter(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdstr("PrepFist"),
                    x => x.MatchLdstr("PrepFist.playbackRate"),
                    x => x.MatchLdsfld<EntityStates.TitanMonster.FireFist>("entryDuration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                    LogILStuff(il, c);
                    return;
                }
                
                MakeFireFistDurationUseAttackSpeed(c);
            }

            private static void FireFist_FixedUpdate(ILContext il)
            {
                ILCursor c = new(il);
                int partCounter = 1;



                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<EntityStates.TitanMonster.FireFist>("stopwatch"),
                    x => x.MatchLdsfld<EntityStates.TitanMonster.FireFist>("trackingDuration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                    LogILStuff(il, c);
                    return;
                }

                MakeFireFistDurationUseAttackSpeed(c);
                partCounter++;



                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<EntityStates.TitanMonster.FireFist>("stopwatch"),
                    x => x.MatchLdsfld<EntityStates.TitanMonster.FireFist>("entryDuration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART {partCounter}");
                    LogILStuff(il, c);
                    return;
                }

                MakeFireFistDurationUseAttackSpeed(c);
                partCounter++;



                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<EntityStates.TitanMonster.FireFist>("stopwatch"),
                    x => x.MatchLdsfld<EntityStates.TitanMonster.FireFist>("fireDuration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART {partCounter}");
                    LogILStuff(il, c);
                    return;
                }

                MakeFireFistDurationUseAttackSpeed(c);
                partCounter++;



                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdstr("ExitFist"),
                    x => x.MatchLdstr("ExitFist.playbackRate"),
                    x => x.MatchLdsfld<EntityStates.TitanMonster.FireFist>("exitDuration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART {partCounter}");
                    LogILStuff(il, c);
                    return;
                }

                // same as MakeFireFistDurationUseAttackSpeed but with some numbers tweaked to make the exit animation play nicely
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.TitanMonster.FireFist, float>>((fireFirstDuration, currentFireFistState) =>
                {
                    currentFireFistState.GetModelAnimator().speed = 1;
                    return fireFirstDuration / (currentFireFistState.attackSpeedStat * 1.4f);
                });
                partCounter++;



                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<EntityStates.TitanMonster.FireFist>("stopwatch"),
                    x => x.MatchLdsfld<EntityStates.TitanMonster.FireFist>("exitDuration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART {partCounter}");
                    LogILStuff(il, c);
                    return;
                }

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.TitanMonster.FireFist, float>>((fireFirstDuration, currentFireFistState) =>
                {
                    currentFireFistState.GetModelAnimator().speed = 1;
                    return fireFirstDuration / (currentFireFistState.attackSpeedStat * 1.4f);
                });
                partCounter++;



                if (!c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<EntityStates.EntityState>("outer")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART {partCounter}");
                    LogILStuff(il, c);
                    return;
                }

                ResetAnimatorSpeed(c);
            }

            private static void FireFist_PlaceSingleDelayBlast(ILContext il)
            {
                ILCursor c = new(il);



                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchCall<ProjectileManager>("get_instance")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 1");
                    LogILStuff(il, c);
                    return;
                }

                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<FireProjectileInfo, FireProjectileInfo>>((fireProjectileInfo) =>
                {
                    // why isn't this line used in vanilla code?
                    fireProjectileInfo.useFuseOverride = true;
                    return fireProjectileInfo;
                });
                c.Emit(OpCodes.Stloc_0);
                c.Index = 0;



                if (!c.TryGotoNext(MoveType.Before,
                    x => x.MatchCall<FireProjectileInfo>("set_fuseOverride")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 2");
                    LogILStuff(il, c);
                    return;
                }

                MakeFireFistDurationUseAttackSpeed(c);
            }

            private static void FireFist_OnExit(ILContext il)
            {
                ILCursor c = new(il);

                ResetAnimatorSpeed(c);
            }



            private static void TitanRockController_Start(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<TitanRockController>("startDelay")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                    LogILStuff(il, c);
                    return;
                }

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, TitanRockController, float>>((startDelay, titanRockController) =>
                {
                    if (titanRockController.ownerCharacterBody)
                    {
                        return startDelay / titanRockController.ownerCharacterBody.attackSpeed;
                    }
                    return startDelay;
                });
            }

            private static void TitanRockController_FixedUpdate(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdflda<TitanRockController>("velocity"),
                    x => x.MatchLdcR4(1)
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                    LogILStuff(il, c);
                    return;
                }

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, TitanRockController, float>>((moveTime, titanRockController) =>
                {
                    if (titanRockController.ownerCharacterBody)
                    {
                        // halving the actual attack speed difference
                        return moveTime / (1 + ((titanRockController.ownerCharacterBody.attackSpeed - 1) / 2));
                    }
                    return moveTime;
                });
            }

            private static void TitanRockController_FixedUpdateServer(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<TitanRockController>("fireInterval")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                    LogILStuff(il, c);
                    return;
                }

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, TitanRockController, float>>((fireInterval, titanRockController) =>
                {
                    if (titanRockController.ownerCharacterBody)
                    {
                        return fireInterval / titanRockController.ownerCharacterBody.attackSpeed;
                    }
                    return fireInterval;
                });
            }



            private static void FireMegaLaser_FixedUpdate(ILContext il)
            {
                ILCursor c = new(il);

                MakeMegaLaserFireFrequencyUseAttackSpeed(il, c, 1);
                // need to do it twice
                MakeMegaLaserFireFrequencyUseAttackSpeed(il, c, 2);
            }

            private static void FireMegaLaser_FireBullet(ILContext il)
            {
                ILCursor c = new(il);

                MakeMegaLaserFireFrequencyUseAttackSpeed(il, c, 1);
            }



            private static void MakeFireFistDurationUseAttackSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.TitanMonster.FireFist, float>>((fireFirstDuration, currentFireFistState) =>
                {
                    currentFireFistState.GetModelAnimator().speed = 1 * currentFireFistState.attackSpeedStat;
                    return fireFirstDuration / currentFireFistState.attackSpeedStat;
                });
            }

            private static void ResetAnimatorSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<EntityStates.TitanMonster.FireFist>>((fireFistState) =>
                {
                    fireFistState.GetModelAnimator().speed = 1;
                });
            }

            private static void MakeMegaLaserFireFrequencyUseAttackSpeed(ILContext il, ILCursor c, int partNumber)
            {
                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdsfld<EntityStates.TitanMonster.FireMegaLaser>("fireFrequency")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART {partNumber}");
                    LogILStuff(il, c);
                    return;
                }

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.TitanMonster.FireMegaLaser, float>>((fireFrequency, megaLaserState) =>
                {
                    return fireFrequency * megaLaserState.attackSpeedStat;
                });
            }
        }


        internal static class Parent
        {
            internal static void SetupILHooks()
            {
                IL.EntityStates.ParentMonster.GroundSlam.OnEnter += GroundSlam_OnEnter;
                IL.EntityStates.ParentMonster.GroundSlam.FixedUpdate += GroundSlam_FixedUpdate;
                IL.EntityStates.ParentMonster.GroundSlam.OnExit += GroundSlam_OnExit;
            }



            private static void GroundSlam_OnEnter(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdstr("Slam"),
                    x => x.MatchLdstr("Slam.playbackRate"),
                    x => x.MatchLdsfld<EntityStates.ParentMonster.GroundSlam>("duration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                    LogILStuff(il, c);
                }
                else
                {
                    MakeDurationAndAnimatorUseAttackSpeed(c);
                }
            }

            private static void GroundSlam_FixedUpdate(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall(out _),
                    x => x.MatchLdsfld<EntityStates.ParentMonster.GroundSlam>("duration")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 1");
                    LogILStuff(il, c);
                }
                else
                {
                    MakeDurationUseAttackSpeed(c);
                }



                if (!c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<EntityStates.EntityState>("outer")
                ))
                {
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 2");
                    LogILStuff(il, c);
                    return;
                }

                ResetAnimatorSpeed(c);
            }

            private static void GroundSlam_OnExit(ILContext il)
            {
                ILCursor c = new(il);

                ResetAnimatorSpeed(c);
            }



            private static void MakeDurationAndAnimatorUseAttackSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.ParentMonster.GroundSlam, float>>((slamStateDuration, currentSlamState) =>
                {
                    currentSlamState.modelAnimator.speed = 1 * currentSlamState.attackSpeedStat;
                    return slamStateDuration / currentSlamState.attackSpeedStat;
                });
            }

            private static void MakeDurationUseAttackSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.ParentMonster.GroundSlam, float>>((slamStateDuration, currentSlamState) =>
                {
                    return slamStateDuration / currentSlamState.attackSpeedStat;
                });
            }

            private static void ResetAnimatorSpeed(ILCursor c)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<EntityStates.ParentMonster.GroundSlam>>((slamState) =>
                {
                    slamState.modelAnimator.speed = 1;
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
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
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
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 1");
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
                    Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 2");
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
                    return slamStateDuration / currentSlamState.attackSpeedStat;
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