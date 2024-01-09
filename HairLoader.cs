using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ReLogic.Content;
using HairLoader.UI;
using Terraria.Localization;
using System.Text;
using System.Text.RegularExpressions;

namespace HairLoader
{    
    public class HairEntry
    {
        // Basic visibility and price
        public bool Visible { get; set; }
        public int HairPrice { get; set; }
        public int ColorPrice { get; set; }
        public bool UsePriceAdjustment { get; set; }

        // Custom price fields for hair and color
        public bool HasCustomPrice { get; set; }
        public int CustomCurrencyID { get; set; }
        public int CustomHairPrice { get; set; }
        public int CustomColorPrice { get; set; }
        public bool UseCustomPriceAdjustment { get; set; }
        
        // Unlock hint
        public bool HasUnlockHint { get; set; }
        public bool UnlockHintIsLocalized { get; set; }
        public string UnlockHint { get; set; }

        // Custom unlock hint fields
        public bool HasCustomUnlockHint { get; set; }
        public bool CustomUnlockHintIsLocalized { get; set; }
        public string CustomUnlockHint { get; set; }

        // Hair name
        public bool HairNameIsLocalized { get; set; }
        public string HairName { get; set; }

        // Custom hair name
        public bool HasCustomHairName { get; set; }
        public bool CustomHairNameIsLocalized { get; set; }
        public string CustomHairName { get; set; }

        // Mod name
        public bool ModNameIsLocalized { get; set; }
        public string ModName { get; set; }

        // Custom mod name
        public bool HasCustomModName { get; set; }
        public bool CustomModNameIsLocalized { get; set; }
        public string CustomModName { get; set; }
    }

    class HairLoader : Mod
    {
        internal static HairLoader Instance;

        // Dictionary that stores all the PlayerHairEntries
        public static Dictionary<int, HairEntry> HairTable = new Dictionary<int, HairEntry>();

        // Point to this Mod object Instance.
        public HairLoader()
        {
            Instance = this;
        }

        public override void Load()
        {
            // Code not ran on server
            if (!Main.dedServ)
            {
                HairTable ??= new Dictionary<int, HairEntry>();
            }
            base.Load();
        }

        public override void Unload()
        {
            // Code not ran on server
            if (!Main.dedServ)
            {
                // Clear the HairTable
                HairTable = null;
            }

            // Clear the mod instance
            Instance = null;

            base.Unload();
        }

        public override void PostSetupContent()
        {
            if (!Main.dedServ)
            {
                LoadHairEntries();
            }
        }

        public override object Call(params object[] args)
        {
            return "Failure";
        }

        public static void LoadHairEntries()
        {
            HairTable ??= new();

            // Loop through all the loaded hairstyles
            for (int i = 0; i < Terraria.ModLoader.HairLoader.Count; i++)
            {
                // Load the Hair textures
                Main.instance.LoadHair(i);

                // Add an entry to our dictionary
                if (!HairTable.ContainsKey(i))
                {
                    HairTable.TryAdd(i, new HairEntry()
                    {
                        // Price
                        HairPrice = 100000,
                        ColorPrice = 20000,
                        UsePriceAdjustment = true,

                        // Unlock hint
                        HasUnlockHint = i < Main.maxHairStyles,
                        UnlockHintIsLocalized = i < Main.maxHairStyles,
                        UnlockHint = GetUnlockHint(i),

                        // Hair name
                        HairNameIsLocalized = i < Main.maxHairStyles,
                        HairName = GetHairName(i),

                        // Mod name
                        ModNameIsLocalized = i < Main.maxHairStyles,
                        ModName = GetModName(i)
                    });
                }
                // Entry exists but is null
                else if (HairTable.ContainsKey(i) && HairTable[i] == null)
                {
                    HairTable[i] = new HairEntry()
                    {
                        // Price
                        HairPrice = 100000,
                        ColorPrice = 20000,
                        UsePriceAdjustment = true,

                        // Unlock hint
                        HasUnlockHint = i < Main.maxHairStyles,
                        UnlockHintIsLocalized = i < Main.maxHairStyles,
                        UnlockHint = GetUnlockHint(i),

                        // Hair name
                        HairNameIsLocalized = i < Main.maxHairStyles,
                        HairName = GetHairName(i),

                        // Mod name
                        ModNameIsLocalized = i < Main.maxHairStyles,
                        ModName = GetModName(i)
                    };
                }
                // Entry exists, set default not-mod-callable fields.
                else if (HairTable.ContainsKey(i) && HairTable[i] != null)
                {
                    // Price
                    HairTable[i].HairPrice = 100000;
                    HairTable[i].ColorPrice = 20000;
                    HairTable[i].UsePriceAdjustment = true;

                    // Unlock hint
                    HairTable[i].HasUnlockHint = i < Main.maxHairStyles;
                    HairTable[i].UnlockHintIsLocalized = i < Main.maxHairStyles;
                    HairTable[i].UnlockHint = GetUnlockHint(i);

                    // Hair name
                    HairTable[i].HairNameIsLocalized = i < Main.maxHairStyles;
                    HairTable[i].HairName = GetHairName(i);

                    // Mod name
                    HairTable[i].ModNameIsLocalized = i < Main.maxHairStyles;
                    HairTable[i].ModName = GetModName(i);
                }
            }
        }

        private static string GetUnlockHint(int index)
        {
            switch (index)
            {
                case 145 or 162 or 163 or 164:
                    {
                        return "Mods.HairLoader.HairStyles.UnlockHint2";
                    }
                case >= 123 and <= 132:
                    {
                        return "Mods.HairLoader.HairStyles.UnlockHint3";
                    }
                case 133:
                    {
                        return "Mods.HairLoader.HairStyles.UnlockHint4";
                    }
                case >= Main.maxHairStyles:
                    {
                        return "";
                    }

                default:
                    { 
                        return "Mods.HairLoader.HairStyles.UnlockHint1";
                    }
            }
        }

        private static string GetHairName(int index)
        {
            if (index < Main.maxHairStyles)
            {
                return "Mods.HairLoader.HairStyles.Names.HairStyle" + index.ToString();
            }

            if (index >= Main.maxHairStyles 
                && index < Terraria.ModLoader.HairLoader.Count
                && Terraria.ModLoader.HairLoader.GetHair(index) != null)
            {
                return Regex.Replace(
                    Terraria.ModLoader.HairLoader.GetHair(index).Name,
                    "([a-z?])[_ ]?([A-Z])", "$1 $2").TrimStart(' ');
            }

            return "";
        }

        private static string GetModName(int index)
        {
            if (index < Main.maxHairStyles)
            {
                return "Mods.HairLoader.HairStyles.TerrariaName";
            }

            if (index >= Main.maxHairStyles 
                && index < Terraria.ModLoader.HairLoader.Count
                && Terraria.ModLoader.HairLoader.GetHair(index) != null)
            {
                return Regex.Replace(
                    Terraria.ModLoader.HairLoader.GetHair(index).Mod.Name,
                    "([a-z?])[_ ]?([A-Z])", "$1 $2").TrimStart(' ');
            }

            return "";
        }
    }
}