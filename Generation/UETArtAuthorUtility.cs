using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Provides a utility for assigning randomly generated human author names
    /// to equipment with a compatible CompArt component.
    /// Preserves existing authors and skips items that cannot display art.
    /// Uses Harmony field access to set the internal author name.
    /// Can be reused by other equipment generation systems.
    /// </summary>
    public static class UETArtAuthorUtility
    {
        private static readonly AccessTools.FieldRef<CompArt, TaggedString>
            AuthorNameField =
                AccessTools.FieldRefAccess<CompArt, TaggedString>(
                    "authorNameInt");

        public static void EnsureRandomAuthor(ThingWithComps thing)
        {
            if (thing == null)
                return;

            CompArt art = thing.TryGetComp<CompArt>();

            if (art == null || !art.CanShowArt)
                return;

            // Do not overwrite an existing author.
            if (!AuthorNameField(art).NullOrEmpty())
                return;

            Name name =
            PawnBioAndNameGenerator.GenerateFullPawnName(
            ThingDefOf.Human,
            gender: Rand.Bool ? Gender.Male : Gender.Female,
            nameCategory: PawnNameCategory.HumanStandard,
            forceNoNick: true);

            if (name == null)
                return;

            AuthorNameField(art) = name.ToStringFull;
        }
    }
}