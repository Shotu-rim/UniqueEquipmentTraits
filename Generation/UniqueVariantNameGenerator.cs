
using System;
using System.Security.Cryptography;
using System.Text;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Generates stable, unique defNames for UET weapon and armor variants.
    /// Uses the source mod's package ID and the original ThingDef name
    /// to prevent naming conflicts between different mods.
    /// Other mods can call GetVariantName(originalDef, "Weapon" or "Armor")
    /// to generate compatible variant identifiers.
    /// </summary>
    public static class UniqueVariantNameGenerator
    {
        public static string GetVariantName(
            ThingDef original,
            string type)
        {
            if (original == null ||
                string.IsNullOrEmpty(original.defName))
            {
                throw new ArgumentException(
                    "Invalid original ThingDef.");
            }

            if (type != "Weapon" && type != "Armor")
            {
                throw new ArgumentException(
                    "Unsupported UET variant type: " + type);
            }

            string packageId =
                original.modContentPack != null
                    ? original.modContentPack.PackageId
                    : "unknown";

            string identity =
                packageId + ":" + original.defName;

            byte[] bytes =
                Encoding.UTF8.GetBytes(identity);

            byte[] hash;

            using (SHA256 sha = SHA256.Create())
            {
                hash = sha.ComputeHash(bytes);
            }

            StringBuilder suffix = new StringBuilder();

            for (int i = 0; i < 12; i++)
            {
                suffix.Append(hash[i].ToString("x2"));
            }

            return "UET_Unique_" +
                   suffix + "_" + type;
        }
    }
}
