using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;

using UnityEngine;
using TMPro;

using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(Embroidery_DOEMod.Embroidery), "Embroidery", "1.2.0", "HanaOcha")]
[assembly: MelonGame()]

namespace Embroidery_DOEMod
{
    [HarmonyPatch(typeof(AssetLibrary), "LoadMods")]
    internal class Embroidery : MelonMod
    {
        [HarmonyPostfix]
        internal static void Load(AssetLibrary __instance)
        {
            __instance.StartCoroutine(EmbroideryData.GetFlavorTexts());
        }

        [HarmonyPatch(typeof(TipCard), nameof(TipCard.Set))]
        [HarmonyPostfix]
        internal static void SetDeck(TipCard __instance, ItemData itemData)
        {
            Transform embroidery = __instance.cardInner.transform.Find("Embroidery");
            if (embroidery == null)
            {
                TextMeshProUGUI setup = new GameObject().AddComponent<TextMeshProUGUI>();
                embroidery = setup.transform;
                embroidery.SetParent(__instance.cardInner.transform, false);
                embroidery.name = "Embroidery";

                setup.alignment = TextAlignmentOptions.Left;
                setup.verticalAlignment = VerticalAlignmentOptions.Bottom;
                setup.fontStyle = FontStyles.Italic;
                setup.enableWordWrapping = true;
                setup.color = Color.white * 0.7f;
                setup.fontSize = __instance.cardInner.description.fontSize - 0.7f;
                setup.rectTransform.sizeDelta = new Vector2(36f, setup.rectTransform.sizeDelta.y);

                embroidery.localPosition += new Vector3(-9f, -16.5f, 0);
            }

            bool frameData = false;
            if (CharacterSelectController.I.deckbuilderController.showFrameData)
            {
                frameData = true;
            }

            TextMeshProUGUI flavor = embroidery.GetComponent<TextMeshProUGUI>();
            flavor.text = string.Empty;
            if (!frameData && EmbroideryData.flavors.ContainsKey(itemData.id))
            {
                flavor.text = EmbroideryData.flavors[itemData.id];
            }
        }
    }
    internal class EmbroideryData
    {
        public static readonly string link = "https://raw.githubusercontent.com/HanaOcha/DuelistsEmbroidery/main/Flavors/";
        internal static Dictionary<string, string> flavors = new Dictionary<string, string>();
        internal static IEnumerator GetFlavorTexts()
        {
            flavors.Clear();

            yield return GetFromFile("Spells.txt");
            yield return GetFromFile("Weapons.txt");
            yield return GetFromFile("Modded.txt");

            yield break;
        }
        internal static IEnumerator GetFromFile(string file)
        {
            Debug.Log("EMBROIDERY ; Requesting \"" + file + "\" from \"" + link + "\"");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link + file);
            request.UserAgent = "Duelists Embroidery Request - " + file;

            HttpWebResponse? response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                Debug.LogException(ex);
            }

            if (response != null)
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                Debug.Log("Reading " + file);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("//"))
                    {
                        string[] data = line.Split(";");
                        if (data.Length >= 2)
                        {
                            flavors.Add(data[0], data[1]);
                        }
                    }
                }
            }

            yield break;
        }
    }
}
