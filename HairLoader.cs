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
        public bool Visible { get; set; }
        public bool OverridePrice { get; set; }         // Determines if a custom price or currency is defined for this hairstyle, false means vanilla logic
        public int SpecialCurrencyID { get; set; }      // Determines the currency that will be used to buy this HairStyle. -1 = coins, 0 = defender medals
        public int HairPrice { get; set; }              // The price of this Hairstyle in copper coins or special currency 'value'
        public int ColorPrice { get; set; }             // The price to change the color of this Hairstyle in copper coins or special currency 'value'
        public bool HasUnlockHint { get; set; }         // Whether an unlock condition needs to be displayed for this hairstyle
        public bool UnlockHintIsLocalized { get; set; }
        public string UnlockHint { get; set; }          // Hint for the player how to unlock this hairstyle
        public bool HasCustomUnlockHint { get; set; }
        public bool CustomUnlockHintIsLocalized { get; set; }
        public string CustomUnlockHint { get; set; }
        public bool HairNameIsLocalized { get; set; }
        public string HairName { get; set; }            // The name of the hairstyle in the hairWindow
        public bool HasCustomHairName { get; set; }
        public bool CustomHairNameIsLocalized { get; set; }
        public string CustomHairName { get; set; }
        public bool ModNameIsLocalized { get; set; }
        public string ModName { get; set; }             // The internal name of the Mod that added this hairstyle
        public bool HasCustomModName { get; set; }
        public bool CustomModNameIsLocalized { get; set; }
        public string CustomModName { get; set; }       // The modified mod name that added this hairstyle, this line will overwrite the Mod's name in the categories window
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
                    HairTable.Add(i, new HairEntry()
                    {
                        OverridePrice = false,
                        SpecialCurrencyID = -1,
                        HairPrice = 0,
                        ColorPrice = 0,
                        HasUnlockHint = i < Main.maxHairStyles,
                        UnlockHintIsLocalized = i < Main.maxHairStyles,
                        UnlockHint = GetUnlockHint(i),
                        HairNameIsLocalized = i < Main.maxHairStyles,
                        HairName = GetHairName(i),
                        ModNameIsLocalized = i < Main.maxHairStyles,
                        ModName = GetModName(i)
                    });
                }
                // Entry exists but is null
                else if (HairTable.ContainsKey(i) && HairTable[i] == null)
                {
                    HairTable[i] = new HairEntry()
                    {
                        OverridePrice = false,
                        SpecialCurrencyID = -1,
                        HairPrice = 0,
                        ColorPrice = 0,
                        HasUnlockHint = i < Main.maxHairStyles,
                        UnlockHintIsLocalized = i < Main.maxHairStyles,
                        UnlockHint = GetUnlockHint(i),
                        HairNameIsLocalized = i < Main.maxHairStyles,
                        HairName = GetHairName(i),
                        ModNameIsLocalized = i < Main.maxHairStyles,
                        ModName = GetModName(i)
                    };
                }
                // Entry exists, set default not-mod-callable fields.
                else if (HairTable.ContainsKey(i) && HairTable[i] != null)
                {
                    HairTable[i].HasUnlockHint = i < Main.maxHairStyles;
                    HairTable[i].UnlockHintIsLocalized = i < Main.maxHairStyles;
                    HairTable[i].UnlockHint = GetUnlockHint(i);
                    HairTable[i].HairNameIsLocalized = i < Main.maxHairStyles;
                    HairTable[i].HairName = GetHairName(i);
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