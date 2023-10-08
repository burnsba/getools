using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// <see cref="AiCommandBuilder"/> main methods.
    /// </summary>
    public partial class AiCommandBuilder
    {
        /// <summary>
        /// Parses a block of bytes as ai commands.
        /// </summary>
        /// <param name="bytes">Bytes to read.</param>
        /// <returns>Command block.</returns>
        /// <exception cref="NotSupportedException">If the command description can't be resolved to a known implementation.</exception>
        public static AiCommandBlock ParseBytes(byte[] bytes)
        {
            int position = 0;
            byte b;
            var results = new AiCommandBlock();

            while (true)
            {
                b = bytes[position++];
                var commandDescription = AiCommandById[b];
                var commandParameters = new List<IAiParameter>();

                IAiConcreteCommand? aic = null;

                if (commandDescription is IAiVariableCommandDescription vcommand)
                {
                    var len = 0;
                    while (bytes[position + len] > 0)
                    {
                        len++;
                    }

                    len++; // include trailing '\0'
                    var commandData = new byte[len];

                    Array.Copy(bytes, position, commandData, 0, len);

                    aic = new AiVariableCommand(vcommand, commandData);

                    // don't increment position until after done with it!
                    position += len;
                }
                else if (commandDescription is IAiFixedCommandDescription fcommand)
                {
                    for (int i = 0; i < fcommand.NumberParameters; i++)
                    {
                        var len = fcommand.CommandParameters[i].ByteLength;

                        var byteValue = new byte[len];
                        Array.Copy(bytes, position, byteValue, 0, len);
                        position += len;

                        if (string.IsNullOrEmpty(fcommand.CommandParameters[i].ParameterName))
                        {
                            throw new NullReferenceException();
                        }

                        commandParameters.Add(new AiParameter(fcommand.CommandParameters[i].ParameterName!, len, byteValue, Architecture.ByteOrder.BigEndien));
                    }

                    aic = new AiFixedCommand(fcommand, commandParameters);
                }
                else
                {
                    throw new NotSupportedException();
                }

                results.Commands.Add(aic);

                if (commandDescription.CommandId == AiListEnd.CommandId
                    || position >= bytes.Length)
                {
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// Lookup of all known AI Commands.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "<Justification>")]
        public static Dictionary<int, IAiCommandDescription> AiCommandById = new Dictionary<int, IAiCommandDescription>()
        {
            { GotoNext.CommandId, GotoNext },
            { GotoFirst.CommandId, GotoFirst },
            { Label.CommandId, Label },
            { AiSleep.CommandId, AiSleep },
            { AiListEnd.CommandId, AiListEnd },
            { JumpToAiList.CommandId, JumpToAiList },
            { SetReturnAiList.CommandId, SetReturnAiList },
            { JumpToReturnAiList.CommandId, JumpToReturnAiList },
            { GuardAnimationStop.CommandId, GuardAnimationStop },
            { GuardKneel.CommandId, GuardKneel },
            { GuardPlayAnimation.CommandId, GuardPlayAnimation },
            { IfGuardPlayingAnimation.CommandId, IfGuardPlayingAnimation },
            { GuardPointsAtBond.CommandId, GuardPointsAtBond },
            { GuardLooksAroundSelf.CommandId, GuardLooksAroundSelf },
            { GuardTrySidestepping.CommandId, GuardTrySidestepping },
            { GuardTryHoppingSideways.CommandId, GuardTryHoppingSideways },
            { GuardTryRunningToSide.CommandId, GuardTryRunningToSide },
            { GuardTryFiringWalk.CommandId, GuardTryFiringWalk },
            { GuardTryFiringRun.CommandId, GuardTryFiringRun },
            { GuardTryFiringRoll.CommandId, GuardTryFiringRoll },
            { GuardTryFireOrAimAtTarget.CommandId, GuardTryFireOrAimAtTarget },
            { GuardTryFireOrAimAtTargetKneel.CommandId, GuardTryFireOrAimAtTargetKneel },
            { GuardTryFireOrAimAtTargetUpdate.CommandId, GuardTryFireOrAimAtTargetUpdate },
            { GuardTryFacingTarget.CommandId, GuardTryFacingTarget },
            { ChrHitBodyPartWithItemDamage.CommandId, ChrHitBodyPartWithItemDamage },
            { ChrHitChrBodyPartWithHeldItem.CommandId, ChrHitChrBodyPartWithHeldItem },
            { GuardTryThrowingGrenade.CommandId, GuardTryThrowingGrenade },
            { GuardTryDroppingItem.CommandId, GuardTryDroppingItem },
            { GuardRunsToPad.CommandId, GuardRunsToPad },
            { GuardRunsToPadPreset.CommandId, GuardRunsToPadPreset },
            { GuardWalksToPad.CommandId, GuardWalksToPad },
            { GuardSprintsToPad.CommandId, GuardSprintsToPad },
            { GuardStartPatrol.CommandId, GuardStartPatrol },
            { GuardSurrenders.CommandId, GuardSurrenders },
            { GuardRemoveFade.CommandId, GuardRemoveFade },
            { ChrRemoveInstant.CommandId, ChrRemoveInstant },
            { GuardTryTriggeringAlarmAtPad.CommandId, GuardTryTriggeringAlarmAtPad },
            { AlarmOn.CommandId, AlarmOn },
            { AlarmOff.CommandId, AlarmOff },
            { RemovedCommand27.CommandId, RemovedCommand27 },
            { GuardTryRunningToBondPosition.CommandId, GuardTryRunningToBondPosition },
            { GuardTryWalkingToBondPosition.CommandId, GuardTryWalkingToBondPosition },
            { GuardTrySprintingToBondPosition.CommandId, GuardTrySprintingToBondPosition },
            { RemovedCommand2B.CommandId, RemovedCommand2B },
            { GuardTryRunningToChrPosition.CommandId, GuardTryRunningToChrPosition },
            { GuardTryWalkingToChrPosition.CommandId, GuardTryWalkingToChrPosition },
            { GuardTrySprintingToChrPosition.CommandId, GuardTrySprintingToChrPosition },
            { IfGuardHasStoppedMoving.CommandId, IfGuardHasStoppedMoving },
            { IfChrDyingOrDead.CommandId, IfChrDyingOrDead },
            { IfChrDoesNotExist.CommandId, IfChrDoesNotExist },
            { IfGuardSeesBond.CommandId, IfGuardSeesBond },
            { RandomGenerateSeed.CommandId, RandomGenerateSeed },
            { IfRandomSeedLessThan.CommandId, IfRandomSeedLessThan },
            { IfRandomSeedGreaterThan.CommandId, IfRandomSeedGreaterThan },
            { IfAlarmIsOnUnused.CommandId, IfAlarmIsOnUnused },
            { IfAlarmIsOn.CommandId, IfAlarmIsOn },
            { IfGasIsLeaking.CommandId, IfGasIsLeaking },
            { IfGuardHeardBond.CommandId, IfGuardHeardBond },
            { IfGuardSeeAnotherGuardShot.CommandId, IfGuardSeeAnotherGuardShot },
            { IfGuardSeeAnotherGuardDie.CommandId, IfGuardSeeAnotherGuardDie },
            { IfGuardAndBondWithinLineOfSight.CommandId, IfGuardAndBondWithinLineOfSight },
            { IfGuardAndBondWithinPartialLineOfSight.CommandId, IfGuardAndBondWithinPartialLineOfSight },
            { IfGuardWasShotOrSeenBondWithinLast10Secs.CommandId, IfGuardWasShotOrSeenBondWithinLast10Secs },
            { IfGuardHeardBondWithinLast10Secs.CommandId, IfGuardHeardBondWithinLast10Secs },
            { IfGuardInRoomWithChr.CommandId, IfGuardInRoomWithChr },
            { IfGuardHasNotBeenSeen.CommandId, IfGuardHasNotBeenSeen },
            { IfGuardIsOnScreen.CommandId, IfGuardIsOnScreen },
            { IfGuardRoomContainingSelfIsOnScreen.CommandId, IfGuardRoomContainingSelfIsOnScreen },
            { IfRoomContainingPadIsOnScreen.CommandId, IfRoomContainingPadIsOnScreen },
            { IfGuardIsTargetedByBond.CommandId, IfGuardIsTargetedByBond },
            { IfGuardShotFromBondMissed.CommandId, IfGuardShotFromBondMissed },
            { IfGuardCounterClockwiseDirectionToBondLessThan.CommandId, IfGuardCounterClockwiseDirectionToBondLessThan },
            { IfGuardCounterClockwiseDirectionToBondGreaterThan.CommandId, IfGuardCounterClockwiseDirectionToBondGreaterThan },
            { IfGuardCounterClockwiseDirectionFromBondLessThan.CommandId, IfGuardCounterClockwiseDirectionFromBondLessThan },
            { IfGuardCounterClockwiseDirectionFromBondGreaterThan.CommandId, IfGuardCounterClockwiseDirectionFromBondGreaterThan },
            { IfGuardDistanceToBondLessThan.CommandId, IfGuardDistanceToBondLessThan },
            { IfGuardDistanceToBondGreaterThan.CommandId, IfGuardDistanceToBondGreaterThan },
            { IfChrDistanceToPadLessThan.CommandId, IfChrDistanceToPadLessThan },
            { IfChrDistanceToPadGreaterThan.CommandId, IfChrDistanceToPadGreaterThan },
            { IfGuardDistanceToChrLessThan.CommandId, IfGuardDistanceToChrLessThan },
            { IfGuardDistanceToChrGreaterThan.CommandId, IfGuardDistanceToChrGreaterThan },
            { GuardTrySettingChrPresetToGuardWithinDistance.CommandId, GuardTrySettingChrPresetToGuardWithinDistance },
            { IfBondDistanceToPadLessThan.CommandId, IfBondDistanceToPadLessThan },
            { IfBondDistanceToPadGreaterThan.CommandId, IfBondDistanceToPadGreaterThan },
            { IfChrInRoomWithPad.CommandId, IfChrInRoomWithPad },
            { IfBondInRoomWithPad.CommandId, IfBondInRoomWithPad },
            { IfBondCollectedObject.CommandId, IfBondCollectedObject },
            { IfItemIsStationaryWithinLevel.CommandId, IfItemIsStationaryWithinLevel },
            { IfItemIsAttachedToObject.CommandId, IfItemIsAttachedToObject },
            { IfBondHasItemEquipped.CommandId, IfBondHasItemEquipped },
            { IfObjectExists.CommandId, IfObjectExists },
            { IfObjectNotDestroyed.CommandId, IfObjectNotDestroyed },
            { IfObjectWasActivated.CommandId, IfObjectWasActivated },
            { IfBondUsedGadgetOnObject.CommandId, IfBondUsedGadgetOnObject },
            { ObjectActivate.CommandId, ObjectActivate },
            { ObjectDestroy.CommandId, ObjectDestroy },
            { ObjectDropFromChr.CommandId, ObjectDropFromChr },
            { ChrDropAllConcealedItems.CommandId, ChrDropAllConcealedItems },
            { ChrDropAllHeldItems.CommandId, ChrDropAllHeldItems },
            { BondCollectObject.CommandId, BondCollectObject },
            { ChrEquipObject.CommandId, ChrEquipObject },
            { ObjectMoveToPad.CommandId, ObjectMoveToPad },
            { DoorOpen.CommandId, DoorOpen },
            { DoorClose.CommandId, DoorClose },
            { IfDoorStateEqual.CommandId, IfDoorStateEqual },
            { IfDoorHasBeenOpenedBefore.CommandId, IfDoorHasBeenOpenedBefore },
            { DoorSetLock.CommandId, DoorSetLock },
            { DoorUnsetLock.CommandId, DoorUnsetLock },
            { IfDoorLockEqual.CommandId, IfDoorLockEqual },
            { IfObjectiveNumComplete.CommandId, IfObjectiveNumComplete },
            { GuardTryUnknown6E.CommandId, GuardTryUnknown6E },
            { GuardTryUnknown6F.CommandId, GuardTryUnknown6F },
            { IfGameDifficultyLessThan.CommandId, IfGameDifficultyLessThan },
            { IfGameDifficultyGreaterThan.CommandId, IfGameDifficultyGreaterThan },
            { IfMissionTimeLessThan.CommandId, IfMissionTimeLessThan },
            { IfMissionTimeGreaterThan.CommandId, IfMissionTimeGreaterThan },
            { IfSystemPowerTimeLessThan.CommandId, IfSystemPowerTimeLessThan },
            { IfSystemPowerTimeGreaterThan.CommandId, IfSystemPowerTimeGreaterThan },
            { IfLevelIdLessThan.CommandId, IfLevelIdLessThan },
            { IfLevelIdGreaterThan.CommandId, IfLevelIdGreaterThan },
            { IfGuardHitsLessThan.CommandId, IfGuardHitsLessThan },
            { IfGuardHitsGreaterThan.CommandId, IfGuardHitsGreaterThan },
            { IfGuardHitsMissedLessThan.CommandId, IfGuardHitsMissedLessThan },
            { IfGuardHitsMissedGreaterThan.CommandId, IfGuardHitsMissedGreaterThan },
            { IfChrHealthLessThan.CommandId, IfChrHealthLessThan },
            { IfChrHealthGreaterThan.CommandId, IfChrHealthGreaterThan },
            { IfChrWasDamagedSinceLastCheck.CommandId, IfChrWasDamagedSinceLastCheck },
            { IfBondHealthLessThan.CommandId, IfBondHealthLessThan },
            { IfBondHealthGreaterThan.CommandId, IfBondHealthGreaterThan },
            { LocalByte1Set.CommandId, LocalByte1Set },
            { LocalByte1Add.CommandId, LocalByte1Add },
            { LocalByte1Subtract.CommandId, LocalByte1Subtract },
            { IfLocalByte1LessThan.CommandId, IfLocalByte1LessThan },
            { IfLocalByte1LessThanRandomSeed.CommandId, IfLocalByte1LessThanRandomSeed },
            { LocalByte2Set.CommandId, LocalByte2Set },
            { LocalByte2Add.CommandId, LocalByte2Add },
            { LocalByte2Subtract.CommandId, LocalByte2Subtract },
            { IfLocalByte2LessThan.CommandId, IfLocalByte2LessThan },
            { IfLocalByte2LessThanRandomSeed.CommandId, IfLocalByte2LessThanRandomSeed },
            { GuardSetHearingScale.CommandId, GuardSetHearingScale },
            { GuardSetVisionRange.CommandId, GuardSetVisionRange },
            { GuardSetGrenadeProbability.CommandId, GuardSetGrenadeProbability },
            { GuardSetChrNum.CommandId, GuardSetChrNum },
            { GuardSetHealthTotal.CommandId, GuardSetHealthTotal },
            { GuardSetArmour.CommandId, GuardSetArmour },
            { GuardSetSpeedRating.CommandId, GuardSetSpeedRating },
            { GuardSetArghRating.CommandId, GuardSetArghRating },
            { GuardSetAccuracyRating.CommandId, GuardSetAccuracyRating },
            { GuardBitfieldSetOn.CommandId, GuardBitfieldSetOn },
            { GuardBitfieldSetOff.CommandId, GuardBitfieldSetOff },
            { IfGuardBitfieldIsSetOn.CommandId, IfGuardBitfieldIsSetOn },
            { ChrBitfieldSetOn.CommandId, ChrBitfieldSetOn },
            { ChrBitfieldSetOff.CommandId, ChrBitfieldSetOff },
            { IfChrBitfieldIsSetOn.CommandId, IfChrBitfieldIsSetOn },
            { ObjectiveBitfieldSetOn.CommandId, ObjectiveBitfieldSetOn },
            { ObjectiveBitfieldSetOff.CommandId, ObjectiveBitfieldSetOff },
            { IfObjectiveBitfieldIsSetOn.CommandId, IfObjectiveBitfieldIsSetOn },
            { GuardFlagsSetOn.CommandId, GuardFlagsSetOn },
            { GuardFlagsSetOff.CommandId, GuardFlagsSetOff },
            { IfGuardFlagsIsSetOn.CommandId, IfGuardFlagsIsSetOn },
            { ChrFlagsSetOn.CommandId, ChrFlagsSetOn },
            { ChrFlagsSetOff.CommandId, ChrFlagsSetOff },
            { IfChrFlagsIsSetOn.CommandId, IfChrFlagsIsSetOn },
            { ObjectFlags1SetOn.CommandId, ObjectFlags1SetOn },
            { ObjectFlags1SetOff.CommandId, ObjectFlags1SetOff },
            { IfObjectFlags1IsSetOn.CommandId, IfObjectFlags1IsSetOn },
            { ObjectFlags2SetOn.CommandId, ObjectFlags2SetOn },
            { ObjectFlags2SetOff.CommandId, ObjectFlags2SetOff },
            { IfObjectFlags2IsSetOn.CommandId, IfObjectFlags2IsSetOn },
            { GuardSetChrPreset.CommandId, GuardSetChrPreset },
            { ChrSetChrPreset.CommandId, ChrSetChrPreset },
            { GuardSetPadPreset.CommandId, GuardSetPadPreset },
            { ChrSetPadPreset.CommandId, ChrSetPadPreset },
            { DebugLog.CommandId, DebugLog },
            { LocalTimerResetStart.CommandId, LocalTimerResetStart },
            { LocalTimerReset.CommandId, LocalTimerReset },
            { LocalTimerStop.CommandId, LocalTimerStop },
            { LocalTimerStart.CommandId, LocalTimerStart },
            { IfLocalTimerHasStopped.CommandId, IfLocalTimerHasStopped },
            { IfLocalTimerLessThan.CommandId, IfLocalTimerLessThan },
            { IfLocalTimerGreaterThan.CommandId, IfLocalTimerGreaterThan },
            { HudCountdownShow.CommandId, HudCountdownShow },
            { HudCountdownHide.CommandId, HudCountdownHide },
            { HudCountdownSet.CommandId, HudCountdownSet },
            { HudCountdownStop.CommandId, HudCountdownStop },
            { HudCountdownStart.CommandId, HudCountdownStart },
            { IfHudCountdownHasStopped.CommandId, IfHudCountdownHasStopped },
            { IfHudCountdownLessThan.CommandId, IfHudCountdownLessThan },
            { IfHudCountdownGreaterThan.CommandId, IfHudCountdownGreaterThan },
            { ChrTrySpawningAtPad.CommandId, ChrTrySpawningAtPad },
            { ChrTrySpawningNextToUnseenChr.CommandId, ChrTrySpawningNextToUnseenChr },
            { GuardTrySpawningItem.CommandId, GuardTrySpawningItem },
            { GuardTrySpawningHat.CommandId, GuardTrySpawningHat },
            { ChrTrySpawningClone.CommandId, ChrTrySpawningClone },
            { TextPrintBottom.CommandId, TextPrintBottom },
            { TextPrintTop.CommandId, TextPrintTop },
            { SfxPlay.CommandId, SfxPlay },
            { SfxEmitFromObject.CommandId, SfxEmitFromObject },
            { SfxEmitFromPad.CommandId, SfxEmitFromPad },
            { SfxSetChannelVolume.CommandId, SfxSetChannelVolume },
            { SfxFadeChannelVolume.CommandId, SfxFadeChannelVolume },
            { SfxStopChannel.CommandId, SfxStopChannel },
            { IfSfxChannelVolumeLessThan.CommandId, IfSfxChannelVolumeLessThan },
            { VehicleStartPath.CommandId, VehicleStartPath },
            { VehicleSpeed.CommandId, VehicleSpeed },
            { AircraftRotorSpeed.CommandId, AircraftRotorSpeed },
            { IfCameraIsInIntro.CommandId, IfCameraIsInIntro },
            { IfCameraIsInBondSwirl.CommandId, IfCameraIsInBondSwirl },
            { TvChangeScreenBank.CommandId, TvChangeScreenBank },
            { IfBondInTank.CommandId, IfBondInTank },
            { ExitLevel.CommandId, ExitLevel },
            { CameraReturnToBond.CommandId, CameraReturnToBond },
            { CameraLookAtBondFromPad.CommandId, CameraLookAtBondFromPad },
            { CameraSwitch.CommandId, CameraSwitch },
            { IfBondYPosLessThan.CommandId, IfBondYPosLessThan },
            { HudHideAndLockControlsAndPauseMissionTime.CommandId, HudHideAndLockControlsAndPauseMissionTime },
            { HudShowAllAndUnlockControlsAndResumeMissionTime.CommandId, HudShowAllAndUnlockControlsAndResumeMissionTime },
            { ChrTryTeleportingToPad.CommandId, ChrTryTeleportingToPad },
            { ScreenFadeToBlack.CommandId, ScreenFadeToBlack },
            { ScreenFadeFromBlack.CommandId, ScreenFadeFromBlack },
            { IfScreenFadeCompleted.CommandId, IfScreenFadeCompleted },
            { ChrHideAll.CommandId, ChrHideAll },
            { ChrShowAll.CommandId, ChrShowAll },
            { DoorOpenInstant.CommandId, DoorOpenInstant },
            { ChrRemoveItemInHand.CommandId, ChrRemoveItemInHand },
            { IfNumberOfActivePlayersLessThan.CommandId, IfNumberOfActivePlayersLessThan },
            { IfBondItemTotalAmmoLessThan.CommandId, IfBondItemTotalAmmoLessThan },
            { BondEquipItem.CommandId, BondEquipItem },
            { BondEquipItemCinema.CommandId, BondEquipItemCinema },
            { BondSetLockedVelocity.CommandId, BondSetLockedVelocity },
            { IfObjectInRoomWithPad.CommandId, IfObjectInRoomWithPad },
            { IfGuardIsFiringAndUsing180RangeFlag.CommandId, IfGuardIsFiringAndUsing180RangeFlag },
            { IfGuardIsFiring.CommandId, IfGuardIsFiring },
            { SwitchFogInstantly.CommandId, SwitchFogInstantly },
            { TriggerFadeAndExitLevelOnButtonPress.CommandId, TriggerFadeAndExitLevelOnButtonPress },
            { IfBondIsDead.CommandId, IfBondIsDead },
            { BondDisableDamageAndPickups.CommandId, BondDisableDamageAndPickups },
            { BondHideWeapons.CommandId, BondHideWeapons },
            { CameraOrbitPad.CommandId, CameraOrbitPad },
            { CreditsRoll.CommandId, CreditsRoll },
            { IfCreditsHasCompleted.CommandId, IfCreditsHasCompleted },
            { IfObjectiveAllCompleted.CommandId, IfObjectiveAllCompleted },
            { IfFolderActorIsEqual.CommandId, IfFolderActorIsEqual },
            { IfBondDamageAndPickupsDisabled.CommandId, IfBondDamageAndPickupsDisabled },
            { MusicXtrackPlay.CommandId, MusicXtrackPlay },
            { MusicXtrackStop.CommandId, MusicXtrackStop },
            { TriggerExplosionsAroundBond.CommandId, TriggerExplosionsAroundBond },
            { IfKilledCiviliansGreaterThan.CommandId, IfKilledCiviliansGreaterThan },
            { IfChrWasShotSinceLastCheck.CommandId, IfChrWasShotSinceLastCheck },
            { BondKilledInAction.CommandId, BondKilledInAction },
            { GuardRaisesArms.CommandId, GuardRaisesArms },
            { GasLeakAndFadeFog.CommandId, GasLeakAndFadeFog },
            { ObjectRocketLaunch.CommandId, ObjectRocketLaunch },
        };
    }
}
