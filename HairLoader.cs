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
using Terraria.GameContent.UI;

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
            try
            {
                if (args.Length <= 0
                    || args[0] == null
                    || args[0] is not string)
                {
                    return "HairLoader Call Error: First argument is null or not a string";
                }

                string message = args[0] as string;

                if (args.Length <= 1
                    || args[1] == null 
                    || args[1] is not int)
                { 
                    return "HairLoader Call Error: Second argument is null or not a int";
                }

                int id = (int)args[1];

                switch (message)
                {
                    case "AddEverything":
                        {
                            if (args.Length <= 11)
                            {
                                return "HairLoader Call Error: too few argument given for AddCustomHairName. Given: " + args.Length + " Need: 11";
                            }

                            return "Success";
                        }

                    case "AddCustomHairName":
                        {
                            if (args.Length <= 3)
                            {
                                return "HairLoader Call Error: too few argument given for AddCustomHairName. Given: " + args.Length + " Need: 4";
                            }

                            if (args[2] == null
                                || args[2] is not string)
                            {
                                return "HairLoader Call Error: Third argument is null or not a string";
                            }

                            if (args[3] == null
                                || args[3] is not bool)
                            {
                                return "HairLoader Call Error: Fourth argument is null or not a bool";
                            }

                            AddCustomHair(id, args[2] as string, (bool)args[3]);

                            return "Success";
                        }

                    case "AddCustomModName":
                        {
                            if (args.Length <= 3)
                            {
                                return "HairLoader Call Error: too few argument given for AddCustomHairName. Given: " + args.Length + " Need: 4";
                            }

                            if (args[2] == null
                                || args[2] is not string)
                            {
                                return "HairLoader Call Error: Third argument is null or not a string";
                            }

                            if (args[3] == null
                                || args[3] is not bool)
                            {
                                return "HairLoader Call Error: Fourth argument is null or not a bool";
                            }

                            AddCustomMod(id, args[2] as string, (bool)args[3]);

                            return "Success";
                        }

                    case "AddCustomPrice":
                        {
                            if (args.Length <= 5)
                            {
                                return "HairLoader Call Error: too few argument given for AddCustomHairName. Given: " + args.Length + " Need: 6";
                            }

                            if (args[2] == null
                                || args[2] is not int)
                            {
                                return "HairLoader Call Error: Third argument is null or not a int";
                            }

                            if (args[3] == null
                                || args[3] is not int)
                            {
                                return "HairLoader Call Error: Fourth argument is null or not a int";
                            }

                            if (args[4] == null
                                || args[4] is not int)
                            {
                                return "HairLoader Call Error: Fourth argument is null or not a int";
                            }

                            if (args[5] == null
                                || args[5] is not bool)
                            {
                                return "HairLoader Call Error: Fourth argument is null or not a bool";
                            }

                            if ((int)args[2] != -1 && !CustomCurrencyManager.TryGetCurrencySystem((int)args[2], out _))
                            {
                                return "HairLoader Call Error: Given currency ID does not exist in CustomCurrencyManager";
                            }

                            if ((int)args[3] < 0)
                            {
                                return "HairLoader Call Error: Hair price cannot be negative";
                            }

                            if ((int)args[4] < 0)
                            {
                                return "HairLoader Call Error: Color price cannot be negative";
                            }

                            AddCustomPrice(id, (int)args[2], (int)args[3], (int)args[4], (bool)args[5]);

                            return "Success";
                        }

                    case "AddCustomUnlockHint":
                        {
                            if (args.Length <= 3)
                            {
                                return "HairLoader Call Error: too few argument given for AddCustomHairName. Given: " + args.Length + " Need: 4";
                            }

                            if (args[2] == null
                                || args[2] is not string)
                            {
                                return "HairLoader Call Error: Third argument is null or not a string";
                            }

                            if (args[3] == null
                                || args[3] is not bool)
                            {
                                return "HairLoader Call Error: Fourth argument is null or not a bool";
                            }

                            AddCustomHint(id, args[2] as string, (bool)args[3]);

                            return "Success";
                        }
                }
            }
            catch (Exception e)
            {
                Logger.Warn("HairLoader Call Error: " + e.StackTrace + e.Message);
            }
            return "Failure";
        }

        private static void AddCustomPrice(int id, int currency = -1, int hairPrice = 100000, int colorPrice = 20000, bool priceAdjustment = false)
        {
            // Check the bounds of the given id
            if (id > Terraria.ModLoader.HairLoader.Count || id < 0)
            {
                return;
            }

            // Check HairTable
            HairTable ??= new();

            // Load the default hair entry
            LoadHair(id);

            // Assign the price
            HairTable[id].HasCustomPrice = true;
            HairTable[id].CustomCurrencyID = currency;
            HairTable[id].CustomHairPrice = hairPrice;
            HairTable[id].CustomColorPrice = colorPrice;
            HairTable[id].UseCustomPriceAdjustment = priceAdjustment;
        }

        private static void AddCustomHair(int id, string name = "", bool localized = false)
        {
            // Check the bounds of the given id
            if (id > Terraria.ModLoader.HairLoader.Count || id < 0)
            {
                return;
            }

            // Check HairTable
            HairTable ??= new();

            // Load the default hair entry
            LoadHair(id);

            // Assign the name
            HairTable[id].HasCustomHairName = true;
            HairTable[id].CustomHairName = name;
            HairTable[id].CustomHairNameIsLocalized = localized;
        }

        private static void AddCustomMod(int id, string name = "", bool localized = false)
        { 
            // Check the bounds of the given id
            if (id > Terraria.ModLoader.HairLoader.Count || id < 0)
            {
                return;
            }

            // Check HairTable
            HairTable ??= new();

            // Load the default hair entry
            LoadHair(id);

            // Assign the name
            HairTable[id].HasCustomModName = true;
            HairTable[id].CustomModName = name;
            HairTable[id].CustomModNameIsLocalized = localized;
        }

        private static void AddCustomHint(int id, string hint = "", bool localized = false)
        {
            // Check the bounds of the given id
            if (id > Terraria.ModLoader.HairLoader.Count || id < 0)
            {
                return;
            }

            // Check HairTable
            HairTable ??= new();

            // Load the default hair entry
            LoadHair(id);

            // Assign the hint
            HairTable[id].HasCustomUnlockHint = true;
            HairTable[id].CustomUnlockHint = hint;
            HairTable[id].UnlockHintIsLocalized = localized;
        }

        private static void LoadHairEntries()
        {
            HairTable ??= new();

            // Loop through all the loaded hairstyles
            for (int i = 0; i < Terraria.ModLoader.HairLoader.Count; i++)
            {
                LoadHair(i);
            }
        }

        private static void LoadHair(int index)
        {
            // Load the Hair textures
            Main.instance.LoadHair(index);

            // Add an entry to our dictionary
            if (!HairTable.ContainsKey(index))
            {
                HairTable.TryAdd(index, new HairEntry()
                {
                    // Price
                    HairPrice = 100000,
                    ColorPrice = 20000,
                    UsePriceAdjustment = true,

                    // Unlock hint
                    HasUnlockHint = index < Main.maxHairStyles,
                    UnlockHintIsLocalized = index < Main.maxHairStyles,
                    UnlockHint = GetUnlockHint(index),

                    // Hair name
                    HairNameIsLocalized = index < Main.maxHairStyles,
                    HairName = GetHairName(index),

                    // Mod name
                    ModNameIsLocalized = index < Main.maxHairStyles,
                    ModName = GetModName(index)
                });
            }
            // Entry exists but is null
            else if (HairTable.ContainsKey(index) && HairTable[index] == null)
            {
                HairTable[index] = new HairEntry()
                {
                    // Price
                    HairPrice = 100000,
                    ColorPrice = 20000,
                    UsePriceAdjustment = true,

                    // Unlock hint
                    HasUnlockHint = index < Main.maxHairStyles,
                    UnlockHintIsLocalized = index < Main.maxHairStyles,
                    UnlockHint = GetUnlockHint(index),

                    // Hair name
                    HairNameIsLocalized = index < Main.maxHairStyles,
                    HairName = GetHairName(index),

                    // Mod name
                    ModNameIsLocalized = index < Main.maxHairStyles,
                    ModName = GetModName(index)
                };
            }
            // Entry exists, set default not-mod-callable fields.
            else if (HairTable.ContainsKey(index) && HairTable[index] != null)
            {
                // Price
                HairTable[index].HairPrice = 100000;
                HairTable[index].ColorPrice = 20000;
                HairTable[index].UsePriceAdjustment = true;

                // Unlock hint
                HairTable[index].HasUnlockHint = index < Main.maxHairStyles;
                HairTable[index].UnlockHintIsLocalized = index < Main.maxHairStyles;
                HairTable[index].UnlockHint = GetUnlockHint(index);

                // Hair name
                HairTable[index].HairNameIsLocalized = index < Main.maxHairStyles;
                HairTable[index].HairName = GetHairName(index);

                // Mod name
                HairTable[index].ModNameIsLocalized = index < Main.maxHairStyles;
                HairTable[index].ModName = GetModName(index);
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