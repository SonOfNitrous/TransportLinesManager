using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.VehicleAIExt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    public class TLMUtils
    {
        public static readonly TransferManager.TransferReason[] defaultAllowedVehicleTypes = {
            TransferManager.TransferReason.Blimp ,
            TransferManager.TransferReason.CableCar ,
            TransferManager.TransferReason.Ferry ,
            TransferManager.TransferReason.MetroTrain ,
            TransferManager.TransferReason.Monorail ,
            TransferManager.TransferReason.PassengerTrain ,
            TransferManager.TransferReason.PassengerPlane ,
            TransferManager.TransferReason.PassengerShip ,
            TransferManager.TransferReason.Tram ,
            TransferManager.TransferReason.Bus
        };

        public static void doLog(string format, params object[] args)
        {
            if (TransportLinesManagerMod.instance != null) {
                if (TransportLinesManagerMod.debugMode) {
                    Debug.LogWarningFormat("TLMRv" + TransportLinesManagerMod.majorVersion + " " + format, args);
                }
            } else {
                Console.WriteLine("TLMRv" + TransportLinesManagerMod.majorVersion + " " + format, args);
            }
        }

        public static void doErrorLog(string format, params object[] args)
        {
            if (TransportLinesManagerMod.instance != null) {
                Debug.LogErrorFormat("TLMRv" + TransportLinesManagerMod.majorVersion + " " + format, args);
            } else {
                Console.WriteLine("TLMRv" + TransportLinesManagerMod.majorVersion + " " + format, args);
            }

        }
        public static void createUIElement<T>(ref T uiItem, Transform parent) where T : Component
        {
            GameObject container = new GameObject();
            container.transform.parent = parent;
            uiItem = container.AddComponent<T>();
        }



        public static void uiTextFieldDefaults(UITextField uiItem)
        {
            uiItem.selectionSprite = "EmptySprite";
            uiItem.useOutline = true;
            uiItem.hoveredBgSprite = "TextFieldPanelHovered";
            uiItem.focusedBgSprite = "TextFieldPanel";
            uiItem.builtinKeyNavigation = true;
            uiItem.submitOnFocusLost = true;
        }

        public static Color contrastColor(Color color)
        {
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double a = (0.299 * color.r + 0.587 * color.g + 0.114 * color.b);

            if (a > 0.5)
                d = 0; // bright colors - black font
            else
                d = 1; // dark colors - white font

            return new Color(d, d, d, 1);
        }

        public static float calcBezierLenght(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float precision)
        {

            Vector3 aa = (-a + 3 * (b - c) + d);
            Vector3 bb = 3 * (a + c) - 6 * b;
            Vector3 cc = 3 * (b - a);

            int len = (int) (1.0f / precision);
            float[] arcLengths = new float[len + 1];
            arcLengths[0] = 0;

            Vector3 ov = a;
            Vector3 v;
            float clen = 0.0f;
            for (int i = 1; i <= len; i++) {
                float t = (i * precision);
                v = ((aa * t + (bb)) * t + cc) * t + a;
                clen += (ov - v).magnitude;
                arcLengths[i] = clen;
                ov = v;
            }
            return clen;

        }

        public static UIDragHandle createDragHandle(UIComponent parent, UIComponent target)
        {
            return createDragHandle(parent, target, -1);
        }

        public static UIDragHandle createDragHandle(UIComponent parent, UIComponent target, float height)
        {
            UIDragHandle dh = null;
            createUIElement<UIDragHandle>(ref dh, parent.transform);
            dh.target = target;
            dh.relativePosition = new Vector3(0, 0);
            dh.width = parent.width;
            dh.height = height < 0 ? parent.height : height;
            dh.name = "DragHandle";
            dh.Start();
            return dh;
        }

        public static void initButton(UIButton button, bool isCheck, string baseSprite, bool allLower = false)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite + "Disabled";
            button.hoveredBgSprite = spriteHov;
            button.focusedBgSprite = spriteHov;
            button.pressedBgSprite = isCheck ? sprite + "Pressed" : spriteHov;

            if (allLower) {
                button.normalBgSprite = button.normalBgSprite.ToLower();
                button.disabledBgSprite = button.disabledBgSprite.ToLower();
                button.hoveredBgSprite = button.hoveredBgSprite.ToLower();
                button.focusedBgSprite = button.focusedBgSprite.ToLower();
                button.pressedBgSprite = button.pressedBgSprite.ToLower();
            }

            button.textColor = new Color32(255, 255, 255, 255);
        }

        public static void initButtonSameSprite(UIButton button, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite;
            button.hoveredBgSprite = sprite;
            button.focusedBgSprite = sprite;
            button.pressedBgSprite = sprite;
            button.textColor = new Color32(255, 255, 255, 255);
        }

        public static void initButtonFg(UIButton button, bool isCheck, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalFgSprite = sprite;
            button.disabledFgSprite = sprite;
            button.hoveredFgSprite = spriteHov;
            button.focusedFgSprite = spriteHov;
            button.pressedFgSprite = isCheck ? sprite + "Pressed" : spriteHov;
            button.textColor = new Color32(255, 255, 255, 255);
        }

        public static void copySpritesEvents(UIButton source, UIButton target)
        {
            target.disabledBgSprite = source.disabledBgSprite;
            target.focusedBgSprite = source.focusedBgSprite;
            target.hoveredBgSprite = source.hoveredBgSprite;
            target.normalBgSprite = source.normalBgSprite;
            target.pressedBgSprite = source.pressedBgSprite;

            target.disabledFgSprite = source.disabledFgSprite;
            target.focusedFgSprite = source.focusedFgSprite;
            target.hoveredFgSprite = source.hoveredFgSprite;
            target.normalFgSprite = source.normalFgSprite;
            target.pressedFgSprite = source.pressedFgSprite;

        }

        public static string[] getStringOptionsForPrefix(ModoNomenclatura m, bool showUnprefixed = false)
        {
            List<string> saida = new List<string>(new string[] { "" });
            if (showUnprefixed) {
                saida.Add(Locale.Get("TLM_UNPREFIXED"));
            }
            if (m == ModoNomenclatura.Nenhum) {
                return saida.ToArray();
            }
            switch (m) {
                case ModoNomenclatura.GregoMaiusculo:
                case ModoNomenclatura.GregoMaiusculoNumero:
                    saida.AddRange(gregoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.GregoMinusculo:
                case ModoNomenclatura.GregoMinusculoNumero:
                    saida.AddRange(gregoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                case ModoNomenclatura.CirilicoMaiusculoNumero:
                    saida.AddRange(cirilicoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                case ModoNomenclatura.CirilicoMinusculoNumero:
                    saida.AddRange(cirilicoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                case ModoNomenclatura.LatinoMaiusculoNumero:
                    saida.AddRange(latinoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                case ModoNomenclatura.LatinoMinusculoNumero:
                    saida.AddRange(latinoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.Numero:
                    for (int i = 1; i <= 64; i++) {
                        saida.Add(i.ToString());
                    }
                    break;
            }
            if (TLMUtils.nomenclaturasComNumeros.Contains(m)) {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida.ToArray();
        }


        public static string[] getFilterPrefixesOptions(TLMCW.ConfigIndex transportType)
        {
            transportType &= TLMConfigWarehouse.ConfigIndex.SYSTEM_PART;
            var m = (ModoNomenclatura) TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);
            List<string> saida = new List<string>(new string[] { Locale.Get("TLM_ALL"), Locale.Get("TLM_UNPREFIXED") });
            switch (m) {
                case ModoNomenclatura.GregoMaiusculo:
                case ModoNomenclatura.GregoMaiusculoNumero:
                    saida.AddRange(gregoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.GregoMinusculo:
                case ModoNomenclatura.GregoMinusculoNumero:
                    saida.AddRange(gregoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                case ModoNomenclatura.CirilicoMaiusculoNumero:
                    saida.AddRange(cirilicoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                case ModoNomenclatura.CirilicoMinusculoNumero:
                    saida.AddRange(cirilicoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                case ModoNomenclatura.LatinoMaiusculoNumero:
                    saida.AddRange(latinoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                case ModoNomenclatura.LatinoMinusculoNumero:
                    saida.AddRange(latinoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.Numero:
                    for (int i = 1; i <= 64; i++) {
                        saida.Add(i.ToString());
                    }
                    break;
            }
            if (TLMUtils.nomenclaturasComNumeros.Contains(m)) {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida.ToArray();
        }


        public static List<string> getDepotPrefixesOptions(TLMCW.ConfigIndex transportType)
        {
            transportType &= TLMConfigWarehouse.ConfigIndex.SYSTEM_PART;
            var m = (ModoNomenclatura) TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);
            List<string> saida = new List<string>(new string[] { Locale.Get("TLM_UNPREFIXED") });
            switch (m) {
                case ModoNomenclatura.GregoMaiusculo:
                case ModoNomenclatura.GregoMaiusculoNumero:
                    saida.AddRange(gregoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.GregoMinusculo:
                case ModoNomenclatura.GregoMinusculoNumero:
                    saida.AddRange(gregoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                case ModoNomenclatura.CirilicoMaiusculoNumero:
                    saida.AddRange(cirilicoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                case ModoNomenclatura.CirilicoMinusculoNumero:
                    saida.AddRange(cirilicoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                case ModoNomenclatura.LatinoMaiusculoNumero:
                    saida.AddRange(latinoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                case ModoNomenclatura.LatinoMinusculoNumero:
                    saida.AddRange(latinoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.Numero:
                    for (int i = 1; i <= 64; i++) {
                        saida.Add(i.ToString());
                    }
                    break;
            }
            if (TLMUtils.nomenclaturasComNumeros.Contains(m)) {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida;
        }

        public static string getString(ModoNomenclatura prefixo, Separador s, ModoNomenclatura sufixo, ModoNomenclatura naoPrefixado, int numero, bool leadingZeros, bool invertPrefixSuffix)
        {
            string prefixoSaida = "";
            string separadorSaida = "";
            string sufixoSaida = "";
            int prefixNum = 0;
            if (prefixo != ModoNomenclatura.Nenhum) {
                prefixNum = numero / 1000;
                prefixoSaida = getStringFromNumber(getStringOptionsForPrefix(prefixo), prefixNum + 1);
                numero = numero % 1000;
            }

            if (numero > 0) {
                if (prefixoSaida != "" && s != Separador.Nenhum) {
                    switch (s) {
                        case Separador.Barra:
                            separadorSaida = "/";
                            break;
                        case Separador.Espaco:
                            separadorSaida = " ";
                            break;
                        case Separador.Hifen:
                            separadorSaida = "-";
                            break;
                        case Separador.Ponto:
                            separadorSaida = ".";
                            break;
                        case Separador.QuebraLinha:
                            separadorSaida = "\n";
                            break;
                    }
                }
                switch (prefixo != ModoNomenclatura.Nenhum && prefixNum > 0 ? sufixo : naoPrefixado) {
                    case ModoNomenclatura.GregoMaiusculo:
                        sufixoSaida = getStringFromNumber(gregoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.GregoMinusculo:
                        sufixoSaida = getStringFromNumber(gregoMinusculo, numero);
                        break;
                    case ModoNomenclatura.CirilicoMaiusculo:
                        sufixoSaida = getStringFromNumber(cirilicoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.CirilicoMinusculo:
                        sufixoSaida = getStringFromNumber(cirilicoMinusculo, numero);
                        break;
                    case ModoNomenclatura.LatinoMaiusculo:
                        sufixoSaida = getStringFromNumber(latinoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.LatinoMinusculo:
                        sufixoSaida = getStringFromNumber(latinoMinusculo, numero);
                        break;
                    default:
                        if (leadingZeros && prefixoSaida != "") {
                            sufixoSaida = numero.ToString("D3");
                        } else {
                            sufixoSaida = numero.ToString();
                        }
                        break;
                }

                if (invertPrefixSuffix && sufixo == ModoNomenclatura.Numero && prefixo != ModoNomenclatura.Numero) {
                    return sufixoSaida + separadorSaida + prefixoSaida;
                } else {
                    return prefixoSaida + separadorSaida + sufixoSaida;
                }
            } else {
                return prefixoSaida;
            }
        }

        private static string getString(ModoNomenclatura m, int numero)
        {

            switch (m) {
                case ModoNomenclatura.GregoMaiusculo:
                    return getStringFromNumber(gregoMaiusculo, numero);
                case ModoNomenclatura.GregoMinusculo:
                    return getStringFromNumber(gregoMinusculo, numero);
                case ModoNomenclatura.CirilicoMaiusculo:
                    return getStringFromNumber(cirilicoMaiusculo, numero);
                case ModoNomenclatura.CirilicoMinusculo:
                    return getStringFromNumber(cirilicoMinusculo, numero);
                case ModoNomenclatura.LatinoMaiusculo:
                    return getStringFromNumber(latinoMaiusculo, numero);
                case ModoNomenclatura.LatinoMinusculo:
                    return getStringFromNumber(latinoMinusculo, numero);
                default:
                    return "" + numero;
            }
        }

        public static string getStringFromNumber(string[] array, int number)
        {
            int arraySize = array.Length;
            string saida = "";
            while (number > 0) {
                int idx = (number - 1) % arraySize;
                saida = "" + array[idx] + saida;
                if (number % arraySize == 0) {
                    number /= arraySize;
                    number--;
                } else {
                    number /= arraySize;
                }

            }
            return saida;
        }

        public static void setLineColor(ushort lineIdx, Color color)
        {

            Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineIdx].m_color = color;
            Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineIdx].m_flags |= TransportLine.Flags.CustomColor;
        }

        public static void setLineName(ushort lineIdx, string name)
        {
            InstanceID lineIdSelecionado = default(InstanceID);
            lineIdSelecionado.TransportLine = lineIdx;
            if (name.Length > 0) {
                Singleton<InstanceManager>.instance.SetName(lineIdSelecionado, name);
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineIdx].m_flags |= TransportLine.Flags.CustomName;
            } else {
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineIdx].m_flags &= ~TransportLine.Flags.CustomName;
            }
        }

        public static IEnumerator setBuildingName(ushort buildingID, string name, OnEndProcessingBuildingName function)
        {
            InstanceID buildingIdSelect = default(InstanceID);
            buildingIdSelect.Building = buildingID;
            yield return Singleton<SimulationManager>.instance.AddAction<bool>(Singleton<BuildingManager>.instance.SetBuildingName(buildingID, name));
            function();
        }

        public delegate void OnEndProcessingBuildingName();

        public static string calculateAutoName(ushort lineIdx)
        {
            TransportManager tm = Singleton<TransportManager>.instance;
            TransportLine t = tm.m_lines.m_buffer[(int) lineIdx];
            ItemClass.SubService ss = ItemClass.SubService.None;
            if (t.Info.m_transportType == TransportInfo.TransportType.Train) {
                ss = ItemClass.SubService.PublicTransportTrain;
            } else if (t.Info.m_transportType == TransportInfo.TransportType.Metro) {
                ss = ItemClass.SubService.PublicTransportMetro;
            }
            int stopsCount = t.CountStops(lineIdx);
            ushort[] stopBuildings = new ushort[stopsCount];
            MultiMap<ushort, Vector3> bufferToDraw = new MultiMap<ushort, Vector3>();
            int middle;
            if (t.Info.m_transportType != TransportInfo.TransportType.Bus && t.Info.m_transportType != TransportInfo.TransportType.Tram && CalculateSimmetry(ss, stopsCount, t, out middle)) {
                string station1Name = getStationName(t.GetStop(middle), lineIdx, ss);

                string station2Name = getStationName(t.GetStop(middle + stopsCount / 2), lineIdx, ss);

                return station1Name + " - " + station2Name;
            } else {
                //float autoNameSimmetryImprecision = 0.075f;
                DistrictManager dm = Singleton<DistrictManager>.instance;
                Dictionary<int, KeyValuePair<TLMCW.ConfigIndex, String>> stationsList = new Dictionary<int, KeyValuePair<TLMCW.ConfigIndex, String>>();
                NetManager nm = Singleton<NetManager>.instance;
                for (int j = 0; j < stopsCount; j++) {
                    ItemClass.Service service;
                    ItemClass.SubService subservice;
                    string prefix;
                    ushort buidingId;
                    String value = getStationName(t.GetStop(j), lineIdx, ss, out service, out subservice, out prefix, out buidingId, true);
                    var tsd = TransportSystemDefinition.from(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buidingId].Info.GetAI());
                    stationsList.Add(j, new KeyValuePair<TLMCW.ConfigIndex, string>(tsd != null ? tsd.toConfigIndex() : GameServiceExtensions.toConfigIndex(service, subservice), value));
                }
                uint mostImportantCategoryInLine = stationsList.Select(x => (x.Value.Key).getPriority()).Min();
                if (mostImportantCategoryInLine < int.MaxValue) {
                    var mostImportantPlaceIdx = stationsList.Where(x => x.Value.Key.getPriority() == mostImportantCategoryInLine).Min(x => x.Key);
                    var destiny = stationsList[mostImportantPlaceIdx];

                    var inverseIdxCenter = (mostImportantPlaceIdx + stopsCount / 2) % stopsCount;
                    int resultIdx = inverseIdxCenter;
                    //int simmetryMargin = (int)Math.Ceiling(stopsCount * autoNameSimmetryImprecision);
                    //int resultIdx = -1;
                    //var destBuilding = getStationBuilding((uint)mostImportantPlaceIdx, ss);
                    //BuildingManager bm = Singleton<BuildingManager>.instance;
                    //for (int i = 0; i <= simmetryMargin; i++)
                    //{
                    //    int currentI = (inverseIdxCenter + i + stopsCount) % stopsCount;


                    //    var iBuilding = getStationBuilding((uint)currentI, ss);
                    //    if ((resultIdx == -1 || stationsList[currentI].Key.getPriority() < stationsList[resultIdx].Key.getPriority()) && iBuilding != destBuilding)
                    //    {
                    //        resultIdx = currentI;
                    //    }
                    //    if (i == 0) continue;
                    //    currentI = (inverseIdxCenter - i + stopsCount) % stopsCount;
                    //    iBuilding = getStationBuilding((uint)currentI, ss);
                    //    if ((resultIdx == -1 || stationsList[currentI].Key.getPriority() < stationsList[resultIdx].Key.getPriority()) && iBuilding != destBuilding)
                    //    {
                    //        resultIdx = currentI;
                    //    }
                    //}
                    string originName = "";
                    //int districtOriginId = -1;
                    if (resultIdx >= 0 && stationsList[resultIdx].Key.isLineNamingEnabled()) {
                        var origin = stationsList[resultIdx];
                        var transportType = origin.Key;
                        var name = origin.Value;
                        originName = GetStationNameWithPrefix(transportType, name);
                        originName += " - ";
                    }
                    //else
                    //{
                    //    NetNode nn = nm.m_nodes.m_buffer[t.GetStop((int)resultIdx)];
                    //    Vector3 location = nn.m_position;
                    //    districtOriginId = dm.GetDistrict(location);
                    //    if (districtOriginId > 0)
                    //    {
                    //        District d = dm.m_districts.m_buffer[districtOriginId];
                    //        originName = dm.GetDistrictName(districtOriginId) + " - ";
                    //    }
                    //    else {
                    //        originName = "";
                    //    }
                    //}
                    if (!destiny.Key.isLineNamingEnabled()) {
                        NetNode nn = nm.m_nodes.m_buffer[t.GetStop((int) resultIdx)];
                        Vector3 location = nn.m_position;
                        int districtDestinyId = dm.GetDistrict(location);
                        //if (districtDestinyId == districtOriginId)
                        //{
                        //    District d = dm.m_districts.m_buffer[districtDestinyId];
                        //    return (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + dm.GetDistrictName(districtDestinyId);
                        //}
                        //else
                        if (districtDestinyId > 0) {
                            District d = dm.m_districts.m_buffer[districtDestinyId];
                            return originName + dm.GetDistrictName(districtDestinyId);
                        }
                    }
                    return originName + GetStationNameWithPrefix(destiny.Key, destiny.Value);
                } else {
                    return autoNameByDistrict(t, stopsCount, out middle);
                }
            }
        }

        public static string getBuildingName(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix)
        {

            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;

            Building b = bm.m_buildings.m_buffer[buildingId];
            while (b.m_parentBuilding > 0) {
                doLog("getStationNameWithPrefix(): building id {0} - parent = {1}", buildingId, b.m_parentBuilding);
                buildingId = b.m_parentBuilding;
                b = bm.m_buildings.m_buffer[buildingId];
            }
            InstanceID iid = default(InstanceID);
            iid.Building = buildingId;
            serviceFound = b.Info.GetService();
            subserviceFound = b.Info.GetSubService();
            TLMCW.ConfigIndex index = GameServiceExtensions.toConfigIndex(serviceFound, subserviceFound);
            if (index == TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG) {
                var tsd = TransportSystemDefinition.from(b.Info.GetAI());
                index = tsd.toConfigIndex();
            }
            prefix = index.getPrefixTextNaming().Trim();

            return bm.GetBuildingName(buildingId, iid);
        }

        private static string GetStationNameWithPrefix(TLMCW.ConfigIndex transportType, string name)
        {
            return transportType.getPrefixTextNaming().Trim() + (transportType.getPrefixTextNaming().Trim() != string.Empty ? " " : "") + name;
        }

        private static string autoNameByDistrict(TransportLine t, int stopsCount, out int middle)
        {

            DistrictManager dm = Singleton<DistrictManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            string result = "";
            byte lastDistrict = 0;
            Vector3 local;
            byte district;
            List<int> districtList = new List<int>();
            for (int j = 0; j < stopsCount; j++) {
                local = nm.m_nodes.m_buffer[(int) t.GetStop(j)].m_bounds.center;
                district = dm.GetDistrict(local);
                if ((district != lastDistrict) && district != 0) {
                    districtList.Add(district);
                }
                if (district != 0) {
                    lastDistrict = district;
                }
            }

            local = nm.m_nodes.m_buffer[(int) t.GetStop(0)].m_bounds.center;
            district = dm.GetDistrict(local);
            if ((district != lastDistrict) && district != 0) {
                districtList.Add(district);
            }
            middle = -1;
            int[] districtArray = districtList.ToArray();
            if (districtArray.Length == 1) {
                return (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + dm.GetDistrictName(districtArray[0]);
            } else if (findSimetry(districtArray, out middle)) {
                int firstIdx = middle;
                int lastIdx = middle + districtArray.Length / 2;

                result = dm.GetDistrictName(districtArray[firstIdx % districtArray.Length]) + " - " + dm.GetDistrictName(districtArray[lastIdx % districtArray.Length]);
                if (lastIdx - firstIdx > 1) {
                    result += ", via ";
                    for (int k = firstIdx + 1; k < lastIdx; k++) {
                        result += dm.GetDistrictName(districtArray[k % districtArray.Length]);
                        if (k + 1 != lastIdx) {
                            result += ", ";
                        }
                    }
                }
                return result;
            } else {
                bool inicio = true;
                foreach (int i in districtArray) {
                    result += (inicio ? "" : " - ") + dm.GetDistrictName(i);
                    inicio = false;
                }
                return result;
            }
        }

        public static bool CalculateSimmetry(ItemClass.SubService ss, int stopsCount, TransportLine t, out int middle)
        {
            int j;
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            middle = -1;
            //try to find the loop
            for (j = -1; j < stopsCount / 2; j++) {
                int offsetL = (j + stopsCount) % stopsCount;
                int offsetH = (j + 2) % stopsCount;
                NetNode nn1 = nm.m_nodes.m_buffer[(int) t.GetStop(offsetL)];
                NetNode nn2 = nm.m_nodes.m_buffer[(int) t.GetStop(offsetH)];
                ushort buildingId1 = TLMUtils.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                ushort buildingId2 = TLMUtils.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                //					TLMUtils.doLog("buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                //					TLMUtils.doLog("offsetL="+offsetL+"|offsetH="+offsetH);
                if (buildingId1 == buildingId2) {
                    middle = j + 1;
                    break;
                }
            }
            //				TLMUtils.doLog("middle="+middle);
            if (middle >= 0) {
                for (j = 1; j <= stopsCount / 2; j++) {
                    int offsetL = (-j + middle + stopsCount) % stopsCount;
                    int offsetH = (j + middle) % stopsCount;
                    //						TLMUtils.doLog("offsetL="+offsetL+"|offsetH="+offsetH);
                    //						TLMUtils.doLog("t.GetStop (offsetL)="+t.GetStop (offsetH)+"|t.GetStop (offsetH)="+t.GetStop (offsetH));
                    NetNode nn1 = nm.m_nodes.m_buffer[(int) t.GetStop(offsetL)];
                    NetNode nn2 = nm.m_nodes.m_buffer[(int) t.GetStop(offsetH)];
                    ushort buildingId1 = TLMUtils.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                    ushort buildingId2 = TLMUtils.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                    //						TLMUtils.doLog("buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                    //						TLMUtils.doLog("buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                    //						TLMUtils.doLog("offsetL="+offsetL+"|offsetH="+offsetH);
                    if (buildingId1 != buildingId2) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }

        }

        public static readonly ItemClass.Service[] seachOrder = new ItemClass.Service[]{
            ItemClass.Service.PublicTransport,
            ItemClass.Service.Monument,
            ItemClass.Service.Beautification,
            ItemClass.Service.Disaster,
            ItemClass.Service.HealthCare,
            ItemClass.Service.FireDepartment,
            ItemClass.Service.PoliceDepartment,
            ItemClass.Service.Tourism,
            ItemClass.Service.Education,
            ItemClass.Service.Garbage,
            ItemClass.Service.Office,
            ItemClass.Service.Commercial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Residential,
            ItemClass.Service.Electricity,
            ItemClass.Service.Water
        };

        public static Dictionary<T, string> getValueFromStringArray<T>(string x, string SEPARATOR, string SUBCOMMA, string SUBSEPARATOR)
        {
            string[] array = x.Split(SEPARATOR.ToCharArray());
            var saida = new Dictionary<T, string>();
            if (array.Length != 2) {
                return saida;
            }
            var value = array[1];
            foreach (string item in value.Split(SUBCOMMA.ToCharArray())) {
                var kv = item.Split(SUBSEPARATOR.ToCharArray());
                if (kv.Length != 2) {
                    continue;
                }
                try {
                    T subkey = (T) Enum.Parse(typeof(T), kv[0]);
                    saida[subkey] = kv[1];
                } catch (Exception e) {
                    continue;
                }

            }
            return saida;
        }

        public static void setStopName(string newName, uint stopId, ushort lineId, OnEndProcessingBuildingName callback)
        {
            doLog("setStopName! {0} - {1} - {2}", newName, stopId, lineId);
            ushort buildingId = getStationBuilding(stopId, toSubService(Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_transportType), true, true);
            if (buildingId == 0) {
                doLog("b=0");
                TLMStopsExtension.instance.setStopName(newName, stopId, lineId);
                callback();
            } else {
                doLog("b≠0 ({0})", buildingId);
                Singleton<BuildingManager>.instance.StartCoroutine(setBuildingName(buildingId, newName, callback));
            }
        }

        public static void cleanStopInfo(uint stopId, ushort lineId)
        {
            TLMStopsExtension.instance.cleanStopInfo(stopId, lineId);
        }

        private static ItemClass.SubService toSubService(TransportInfo.TransportType t)
        {
            switch (t) {
                case TransportInfo.TransportType.Airplane:
                    return ItemClass.SubService.PublicTransportPlane;
                case TransportInfo.TransportType.Bus:
                    return ItemClass.SubService.PublicTransportBus;
                case TransportInfo.TransportType.Metro:
                    return ItemClass.SubService.PublicTransportMetro;
                case TransportInfo.TransportType.Ship:
                    return ItemClass.SubService.PublicTransportShip;
                case TransportInfo.TransportType.Taxi:
                    return ItemClass.SubService.PublicTransportTaxi;
                case TransportInfo.TransportType.Train:
                    return ItemClass.SubService.PublicTransportTrain;
                case TransportInfo.TransportType.Tram:
                    return ItemClass.SubService.PublicTransportTram;
                default:
                    return ItemClass.SubService.None;
            }
        }

        public static string getStationName(uint stopId, ushort lineId, ItemClass.SubService ss, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, out ushort buildingID, bool excludeCargo = false)
        {
            string savedName = TLMStopsExtension.instance.getStopName(stopId, lineId);
            if (savedName != null) {
                serviceFound = ItemClass.Service.PublicTransport;
                subserviceFound = toSubService(Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_transportType);
                prefix = "";
                buildingID = 0;
                return savedName;
            }
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int) stopId];
            buildingID = getStationBuilding(stopId, ss, excludeCargo);

            Vector3 location = nn.m_position;
            if (buildingID > 0) {
                return getBuildingName(buildingID, out serviceFound, out subserviceFound, out prefix);
            }



            NetNode nextNode = nm.m_nodes.m_buffer[nn.m_nextGridNode];
            //return nm.GetSegmentName(segId);
            ushort segId = FindNearNamedRoad(nn.m_position);
            string segName = nm.GetSegmentName(segId);
            NetSegment seg = nm.m_segments.m_buffer[segId];
            ushort cross1nodeId = seg.m_startNode;
            ushort cross2nodeId = seg.m_endNode;

            string crossSegName = string.Empty;

            NetNode cross1node = nm.m_nodes.m_buffer[cross1nodeId];
            for (int i = 0; i < 8; i++) {
                var iSegId = cross1node.GetSegment(i);
                if (iSegId > 0 && iSegId != segId) {
                    string iSegName = nm.GetSegmentName(iSegId);
                    if (iSegName != string.Empty && segName != iSegName) {
                        crossSegName = iSegName;
                        break;
                    }
                }
            }
            if (crossSegName == string.Empty) {
                NetNode cross2node = nm.m_nodes.m_buffer[cross2nodeId];
                for (int i = 0; i < 8; i++) {
                    var iSegId = cross2node.GetSegment(i);
                    if (iSegId > 0 && iSegId != segId) {
                        string iSegName = nm.GetSegmentName(iSegId);
                        if (iSegName != string.Empty && segName != iSegName) {
                            crossSegName = iSegName;
                            break;
                        }
                    }
                }
            }
            prefix = "";
            if (segName != string.Empty) {
                serviceFound = ItemClass.Service.Road;
                subserviceFound = ItemClass.SubService.PublicTransportBus;
                if (crossSegName == string.Empty) {
                    return segName;
                } else {
                    prefix = segName + " x ";
                    return crossSegName;
                }

            } else {
                serviceFound = ItemClass.Service.None;
                subserviceFound = ItemClass.SubService.None;
                return "????????";
            }
            //}

            //}
        }


        public static ushort FindNearNamedRoad(Vector3 position)
        {
            PathUnit.Position pathPosA, pathPosB;
            float distanceSqrA, distanceSqrB;
            return FindNearNamedRoad(position, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                    VehicleInfo.VehicleType.Car, false, false, 256f,
                    out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB);
        }


        public static ushort FindNearNamedRoad(Vector3 position, ItemClass.Service service, NetInfo.LaneType laneType, VehicleInfo.VehicleType vehicleType,
            bool allowUnderground, bool requireConnect, float maxDistance,
            out PathUnit.Position pathPosA, out PathUnit.Position pathPosB, out float distanceSqrA, out float distanceSqrB)
        {
            return FindNearNamedRoad(position, service, service, laneType,
                vehicleType, VehicleInfo.VehicleType.None, allowUnderground,
                requireConnect, maxDistance,
                out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB);
        }


        public static ushort FindNearNamedRoad(Vector3 position, ItemClass.Service service, ItemClass.Service service2, NetInfo.LaneType laneType,
            VehicleInfo.VehicleType vehicleType, VehicleInfo.VehicleType stopType, bool allowUnderground,
            bool requireConnect, float maxDistance,
            out PathUnit.Position pathPosA, out PathUnit.Position pathPosB, out float distanceSqrA, out float distanceSqrB)
        {


            Bounds bounds = new Bounds(position, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int num = Mathf.Max((int) ((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int) ((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int) ((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int) ((bounds.max.z + 64f) / 64f + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            pathPosA.m_segment = 0;
            pathPosA.m_lane = 0;
            pathPosA.m_offset = 0;
            distanceSqrA = 1E+10f;
            pathPosB.m_segment = 0;
            pathPosB.m_lane = 0;
            pathPosB.m_offset = 0;
            distanceSqrB = 1E+10f;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort num6 = instance.m_segmentGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0) {
                        NetInfo info = instance.m_segments.m_buffer[(int) num6].Info;
                        if (info != null && (info.m_class.m_service == service || info.m_class.m_service == service2) && (instance.m_segments.m_buffer[(int) num6].m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) == NetSegment.Flags.None && (allowUnderground || !info.m_netAI.IsUnderground())) {
                            ushort startNode = instance.m_segments.m_buffer[(int) num6].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[(int) num6].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[(int) startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[(int) endNode].m_position;
                            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num9 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            Vector3 b;
                            int num10;
                            float num11;
                            Vector3 b2;
                            int num12;
                            float num13;
                            if ((num8 < 0f || num9 < 0f) && instance.m_segments.m_buffer[(int) num6].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[(int) num6].GetClosestLanePosition(position, laneType, vehicleType, stopType, requireConnect, out b, out num10, out num11, out b2, out num12, out num13)) {
                                float num14 = Vector3.SqrMagnitude(position - b);
                                if (num14 < num5) {
                                    num5 = num14;
                                    pathPosA.m_segment = num6;
                                    pathPosA.m_lane = (byte) num10;
                                    pathPosA.m_offset = (byte) Mathf.Clamp(Mathf.RoundToInt(num11 * 255f), 0, 255);
                                    distanceSqrA = num14;
                                    num14 = Vector3.SqrMagnitude(position - b2);
                                    if (num12 == -1 || num14 >= maxDistance * maxDistance) {
                                        pathPosB.m_segment = 0;
                                        pathPosB.m_lane = 0;
                                        pathPosB.m_offset = 0;
                                        distanceSqrB = 1E+10f;
                                    } else {
                                        pathPosB.m_segment = num6;
                                        pathPosB.m_lane = (byte) num12;
                                        pathPosB.m_offset = (byte) Mathf.Clamp(Mathf.RoundToInt(num13 * 255f), 0, 255);
                                        distanceSqrB = num14;
                                    }
                                }
                            }
                        }
                        num6 = instance.m_segments.m_buffer[(int) num6].m_nextGridSegment;
                        if (++num7 >= 36864) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return pathPosA.m_segment;
        }

        public static string getStationName(ushort stopId, ushort lineId, ItemClass.SubService ss)
        {
            ItemClass.SubService subServ;
            ItemClass.Service serv;
            string prefix;
            ushort buildingId;
            return getStationName(stopId, lineId, ss, out serv, out subServ, out prefix, out buildingId, true);
        }

        public static string getFullStationName(ushort stopId, ushort lineId, ItemClass.SubService ss)
        {
            ItemClass.SubService subServ;
            ItemClass.Service serv;
            string prefix;
            ushort buildingId;
            string result = getStationName(stopId, lineId, ss, out serv, out subServ, out prefix, out buildingId, true);
            return string.IsNullOrEmpty(prefix) ? result : prefix + " " + result;
        }


        public static Vector3 getStationBuildingPosition(uint stopId, ItemClass.SubService ss)
        {
            ushort buildingId = getStationBuilding(stopId, ss);


            if (buildingId > 0) {
                BuildingManager bm = Singleton<BuildingManager>.instance;
                Building b = bm.m_buildings.m_buffer[buildingId];
                InstanceID iid = default(InstanceID);
                iid.Building = buildingId;
                return b.m_position;
            } else {
                NetManager nm = Singleton<NetManager>.instance;
                NetNode nn = nm.m_nodes.m_buffer[(int) stopId];
                return nn.m_position;
            }
        }

        public static ushort getStationBuilding(uint stopId, ItemClass.SubService ss, bool excludeCargo = false, bool restrictToTransportType = false)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int) stopId];
            ushort buildingId = 0, tempBuildingId;

            if (ss != ItemClass.SubService.None) {
                tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                if (!excludeCargo || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is TransportStationAI) {
                    buildingId = tempBuildingId;
                }
            }
            if (buildingId == 0 && !restrictToTransportType) {
                tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, defaultAllowedVehicleTypes, Building.Flags.Active, Building.Flags.Untouchable);
                if (!excludeCargo || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is TransportStationAI) {
                    buildingId = tempBuildingId;
                }
                if (buildingId == 0) {
                    tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                    if (!excludeCargo || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is TransportStationAI) {
                        buildingId = tempBuildingId;
                    }
                    if (buildingId == 0) {
                        int iterator = 1;
                        while (buildingId == 0 && iterator < seachOrder.Count()) {
                            buildingId = TLMUtils.FindBuilding(nn.m_position, 100f, seachOrder[iterator], ItemClass.SubService.None, defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                            iterator++;
                        }
                    }
                }
            }
            return buildingId;

        }

        public static bool findSimetry(int[] array, out int middle)
        {
            middle = -1;
            int size = array.Length;
            if (size == 0)
                return false;
            for (int j = -1; j < size / 2; j++) {
                int offsetL = (j + size) % size;
                int offsetH = (j + 2) % size;
                if (array[offsetL] == array[offsetH]) {
                    middle = j + 1;
                    break;
                }
            }
            //			TLMUtils.doLog("middle="+middle);
            if (middle >= 0) {
                for (int k = 1; k <= size / 2; k++) {
                    int offsetL = (-k + middle + size) % size;
                    int offsetH = (k + middle) % size;
                    if (array[offsetL] != array[offsetH]) {
                        return false;
                    }
                }
            } else {
                return false;
            }
            return true;
        }

        public static void clearAllVisibilityEvents(UIComponent u)
        {
            u.eventVisibilityChanged += null;
            for (int i = 0; i < u.components.Count; i++) {
                clearAllVisibilityEvents(u.components[i]);
            }
        }

        public static T GetPrivateField<T>(object o, string fieldName)
        {
            var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field = null;

            foreach (var f in fields) {
                if (f.Name == fieldName) {
                    field = f;
                    break;
                }
            }
            if (field != null) {
                return (T) field.GetValue(o);
            } else {
                return default(T);
            }
        }

        public static bool HasField(object o, string fieldName)
        {
            var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields) {
                if (f.Name == fieldName) {
                    return true;
                }
            }
            return false;
        }

        public static string getPrefixesServedAbstract(ushort m_buildingID, bool secondary)
        {
            Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingID];
            DepotAI ai = b.Info.GetAI() as DepotAI;
            if (ai == null)
                return "";
            List<string> options = TLMUtils.getDepotPrefixesOptions(TLMCW.getConfigIndexForTransportInfo(secondary ? ai.m_secondaryTransportInfo : ai.m_transportInfo));
            var prefixes = TLMDepotAI.getPrefixesServedByDepot(m_buildingID, secondary);
            if (prefixes == null) {
                TLMUtils.doErrorLog("DEPOT AI WITH WRONG TYPE!!! id:{0} ({1})", m_buildingID, BuildingManager.instance.GetBuildingName(m_buildingID, default(InstanceID)));
                return null;
            }
            List<string> saida = new List<string>();
            if (prefixes.Contains(0))
                saida.Add(Locale.Get("TLM_UNPREFIXED_SHORT"));
            uint sequenceInit = 0;
            bool isInSequence = false;
            for (uint i = 1; i < options.Count; i++) {
                if (prefixes.Contains(i)) {
                    if (sequenceInit == 0 || !isInSequence) {
                        sequenceInit = i;
                        isInSequence = true;
                    }
                } else if (sequenceInit != 0 && isInSequence) {
                    if (i - 1 == sequenceInit) {
                        saida.Add(options[(int) sequenceInit]);
                    } else {
                        saida.Add(options[(int) sequenceInit] + "-" + options[(int) (i - 1)]);
                    }
                    isInSequence = false;
                }
            }
            if (sequenceInit != 0 && isInSequence) {
                if (sequenceInit == options.Count - 1) {
                    saida.Add(options[(int) sequenceInit]);
                } else {
                    saida.Add(options[(int) sequenceInit] + "-" + options[(int) (options.Count - 1)]);
                }
                isInSequence = false;
            }
            if (prefixes.Contains(65))
                saida.Add(Locale.Get("TLM_REGIONAL_SHORT"));
            return string.Join(" ", saida.ToArray());
        }


        public static ushort FindBuilding(Vector3 pos, float maxDistance, ItemClass.Service service, ItemClass.SubService subService, TransferManager.TransferReason[] allowedTypes, Building.Flags flagsRequired, Building.Flags flagsForbidden)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            if (allowedTypes == null || allowedTypes.Length == 0) {
                return bm.FindBuilding(pos, maxDistance, service, subService, flagsRequired, flagsForbidden);
            }
            int num = Mathf.Max((int) ((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int) ((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int) ((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int) ((pos.z + maxDistance) / 64f + 135f), 269);
            ushort result = 0;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort num6 = bm.m_buildingGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0) {
                        BuildingInfo info = bm.m_buildings.m_buffer[(int) num6].Info;
                        if ((info.m_class.m_service == service || service == ItemClass.Service.None) && (info.m_class.m_subService == subService || subService == ItemClass.SubService.None)) {
                            Building.Flags flags = bm.m_buildings.m_buffer[(int) num6].m_flags;
                            if ((flags & (flagsRequired | flagsForbidden)) == flagsRequired) {
                                DepotAI depotAI = info.GetAI() as DepotAI;
                                if (depotAI != null && allowedTypes.Contains(depotAI.m_transportInfo.m_vehicleReason)) {
                                    float num8 = Vector3.SqrMagnitude(pos - bm.m_buildings.m_buffer[(int) num6].m_position);
                                    if (num8 < num5) {
                                        result = num6;
                                        num5 = num8;
                                    }
                                }
                            }
                        }
                        num6 = bm.m_buildings.m_buffer[(int) num6].m_nextGridBuilding;
                        if (++num7 >= 49152) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return result;
        }
        public static void doLocaleDump()
        {
            string localeDump = "LOCALE DUMP:\r\n";
            try {
                var locale = TLMUtils.GetPrivateField<Dictionary<Locale.Key, string>>(TLMUtils.GetPrivateField<Locale>(LocaleManager.instance, "m_Locale"), "m_LocalizedStrings");
                foreach (Locale.Key k in locale.Keys) {
                    localeDump += string.Format("{0}  =>  {1}\n", k.ToString(), locale[k]);
                }
            } catch (Exception e) {

                TLMUtils.doErrorLog("LOCALE DUMP FAIL: {0}", e.ToString());
            }
            Debug.LogWarning(localeDump);
        }

        public UISlider GenerateSliderField(UIHelperExtension uiHelper, OnValueChanged action, out UILabel label, out UIPanel container)
        {
            UISlider budgetMultiplier = (UISlider) uiHelper.AddSlider("", 0f, 5, 0.05f, 1, action);
            label = budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>();
            label.autoSize = true;
            label.wordWrap = false;
            label.text = string.Format(" x{0:0.00}", 0);
            container = budgetMultiplier.GetComponentInParent<UIPanel>();
            container.width = 300;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            container.wrapLayout = true;
            return budgetMultiplier;
        }
        public static string getTransportSystemPrefixName(TLMConfigWarehouse.ConfigIndex index, uint prefix, bool global = false)
        {
            return getExtensionFromConfigIndex(index).getPrefixName(prefix, global);
        }

        public static BasicTransportExtension getExtensionFromConfigIndex(TLMConfigWarehouse.ConfigIndex index)
        {
            return BasicTransportExtensionSingleton.instance(TLMConfigWarehouse.getTransportSystemDefinitionForConfigTransport(index));
        }

        public static BasicTransportExtension getExtensionFromTransportSystemDefinition(TransportSystemDefinition def)
        {
            return BasicTransportExtensionSingleton.instance(def);
        }

        private static string[] latinoMaiusculo = {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"
        };
        private static string[] latinoMinusculo = {
            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z"
        };
        private static string[] gregoMaiusculo = {
            "Α",
            "Β",
            "Γ",
            "Δ",
            "Ε",
            "Ζ",
            "Η",
            "Θ",
            "Ι",
            "Κ",
            "Λ",
            "Μ",
            "Ν",
            "Ξ",
            "Ο",
            "Π",
            "Ρ",
            "Σ",
            "Τ",
            "Υ",
            "Φ",
            "Χ",
            "Ψ",
            "Ω"
        };
        private static string[] gregoMinusculo = {
            "α",
            "β",
            "γ",
            "δ",
            "ε",
            "ζ",
            "η",
            "θ",
            "ι",
            "κ",
            "λ",
            "μ",
            "ν",
            "ξ",
            "ο",
            "π",
            "ρ",
            "σ",
            "τ",
            "υ",
            "φ",
            "χ",
            "ψ",
            "ω"
        };
        private static string[] cirilicoMaiusculo = {
            "А",
            "Б",
            "В",
            "Г",
            "Д",
            "Е",
            "Ё",
            "Ж",
            "З",
            "И",
            "Й",
            "К",
            "Л",
            "М",
            "Н",
            "О",
            "П",
            "Р",
            "С",
            "Т",
            "У",
            "Ф",
            "Х",
            "Ц",
            "Ч",
            "Ш",
            "Щ",
            "Ъ",
            "Ы",
            "Ь",
            "Э",
            "Ю",
            "Я"
        };
        private static string[] cirilicoMinusculo = {
            "а",
            "б",
            "в",
            "г",
            "д",
            "е",
            "ё",
            "ж",
            "з",
            "и",
            "й",
            "к",
            "л",
            "м",
            "н",
            "о",
            "п",
            "р",
            "с",
            "т",
            "у",
            "ф",
            "х",
            "ц",
            "ч",
            "ш",
            "щ",
            "ъ",
            "ы",
            "ь",
            "э",
            "ю",
            "я"
        };

        private static string[] numeros = {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"
        };


        public static readonly ModoNomenclatura[] nomenclaturasComNumeros = new ModoNomenclatura[]
        {
        ModoNomenclatura. LatinoMinusculoNumero ,
        ModoNomenclatura. LatinoMaiusculoNumero ,
        ModoNomenclatura. GregoMinusculoNumero,
        ModoNomenclatura. GregoMaiusculoNumero,
        ModoNomenclatura. CirilicoMinusculoNumero,
        ModoNomenclatura. CirilicoMaiusculoNumero
        };

    }


    public enum ModoNomenclatura
    {
        Numero = 0,
        LatinoMinusculo = 1,
        LatinoMaiusculo = 2,
        GregoMinusculo = 3,
        GregoMaiusculo = 4,
        CirilicoMinusculo = 5,
        CirilicoMaiusculo = 6,
        Nenhum = 7,
        LatinoMinusculoNumero = 8,
        LatinoMaiusculoNumero = 9,
        GregoMinusculoNumero = 10,
        GregoMaiusculoNumero = 11,
        CirilicoMinusculoNumero = 12,
        CirilicoMaiusculoNumero = 13,
    }

    public enum Separador
    {
        Nenhum = 0,
        Hifen = 1,
        Ponto = 2,
        Barra = 3,
        Espaco = 4,
        QuebraLinha = 5
    }

    public class Range<T> where T : IComparable<T>
    {
        /// <summary>
        /// Minimum value of the range
        /// </summary>
        public T Minimum { get; set; }

        /// <summary>
        /// Maximum value of the range
        /// </summary>
        public T Maximum { get; set; }

        public Range(T min, T max)
        {
            if (min.CompareTo(max) >= 0) {
                var temp = min;
                min = max;
                max = temp;
            }
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Presents the Range in readable format
        /// </summary>
        /// <returns>String representation of the Range</returns>
        public override string ToString() { return String.Format("[{0} - {1}]", Minimum, Maximum); }

        /// <summary>
        /// Determines if the range is valid
        /// </summary>
        /// <returns>True if range is valid, else false</returns>
        public Boolean IsValid() { return Minimum.CompareTo(Maximum) <= 0; }

        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean ContainsValue(T value)
        {
            return (Minimum.CompareTo(value) <= 0) && (value.CompareTo(Maximum) <= 0);
        }


        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean IsBetweenLimits(T value)
        {
            return (Minimum.CompareTo(value) < 0) && (value.CompareTo(Maximum) < 0);
        }

        /// <summary>
        /// Determines if this Range is inside the bounds of another range
        /// </summary>
        /// <param name="Range">The parent range to test on</param>
        /// <returns>True if range is inclusive, else false</returns>
        public Boolean IsInsideRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && Range.ContainsValue(this.Minimum) && Range.ContainsValue(this.Maximum);
        }



        /// <summary>
        /// Determines if another range is inside the bounds of this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean ContainsRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && this.ContainsValue(Range.Minimum) && this.ContainsValue(Range.Maximum);
        }

        /// <summary>
        /// Determines if another range intersect this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean IntersectRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && (this.ContainsValue(Range.Minimum) || this.ContainsValue(Range.Maximum) || Range.ContainsValue(this.Maximum) || Range.ContainsValue(this.Maximum));
        }

        public Boolean IsBorderSequence(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && (this.Maximum.Equals(Range.Minimum) || this.Minimum.Equals(Range.Maximum));
        }
    }

    public static class Vector2Extensions
    {
        public static float GetAngleToPoint(this Vector2 from, Vector2 to)
        {
            float ca = to.x - from.x;
            float co = to.y - from.y;
            if (co == 0) {
                return ca > 0 ? 0 : 180;
            } else if (ca < 0) {
                return Mathf.Atan(co / ca) * Mathf.Rad2Deg + 180;
            } else {
                return Mathf.Atan(co / ca) * Mathf.Rad2Deg;
            }
        }
    }

    public static class Int32Extensions
    {
        public static int ParseOrDefault(string val, int defaultVal)
        {
            try {
                return int.Parse(val);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return defaultVal;
            }
        }
    }
}

