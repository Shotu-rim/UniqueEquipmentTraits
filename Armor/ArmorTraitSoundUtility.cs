
using RimWorld;
using Verse;
using Verse.Sound;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSoundUtility
    {
        public static void PlayActivationSound(Pawn caster)
        {
            if (caster == null ||
                !caster.Spawned ||
                caster.Map == null)
            {
                return;
            }

            SoundDef sound =
                DefDatabase<SoundDef>.GetNamedSilentFail(
                    "Psycast_Skip_Entry");

            if (sound != null)
            {
                sound.PlayOneShot(
                    new TargetInfo(caster.Position, caster.Map));
            }
        }
    }
}
