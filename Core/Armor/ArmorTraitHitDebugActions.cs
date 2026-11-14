


using LudeonTK;
using RimWorld;
using System;
using System.IO;
using System.Text;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitHitDebugActions
    {
        [DebugAction("UET", "Show hit report B", allowedGameStates = AllowedGameStates.Playing)]
        public static void ShowReportB()
        {
            Log.Message("[UET HIT B] Recorded sequences: " +
                ArmorTraitHitSequenceRecorder.SequenceCount);
            ArmorTraitHitSequenceRecorder.PrintReport();
        }

        [DebugAction("UET", "Export hit report B", allowedGameStates = AllowedGameStates.Playing)]
        public static void ExportReportB()
        {
            try
            {
                string folder = Path.Combine(GenFilePaths.SaveDataFolderPath, "UET_Diagnostics");
                Directory.CreateDirectory(folder);
                string filename = "HitReport_B_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt";
                string path = Path.Combine(folder, filename);
                File.WriteAllText(path, ArmorTraitHitSequenceRecorder.BuildReport(),
                    new UTF8Encoding(false));
                Log.Message("[UET HIT B] Exported " +
                    ArmorTraitHitSequenceRecorder.SequenceCount +
                    " sequences to: " + path);
                Messages.Message("UET: report B exported (" +
                    ArmorTraitHitSequenceRecorder.SequenceCount + " sequences).",
                    MessageTypeDefOf.TaskCompletion, false);
            }
            catch (Exception ex)
            {
                Log.Error("[UET HIT B] Export failed: " + ex);
            }
        }

        [DebugAction("UET", "Reset hit statistics B", allowedGameStates = AllowedGameStates.Playing)]
        public static void ResetReportB()
        {
            ArmorTraitHitSequenceRecorder.Reset();
            Messages.Message("UET: hit statistics B reset.",
                MessageTypeDefOf.NeutralEvent, false);
        }
    }
}

