using System;
using PowerMeter.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PowerMeter.Plugin.UI
{
    /// <summary>
    /// ゲーム内 uGUI 上に常駐する電力表示ウィジット。
    /// ゲーム UI (UIGame) の配下にぶら下げるので、メインメニューでは自動的に隠れる。
    /// </summary>
    public class PowerMeterWidget
    {
        private const int RowCount = 4;      // ヘッダ + 惑星 + 星系 + 全星系
        private const int ColumnCount = 10;

        private const int ColLabel = 0;
        private const int ColGen = 1;
        private const int ColSep = 2;
        private const int ColCon = 3;
        private const int ColCap = 4;
        private const int ColUtil = 5;
        private const int ColSat = 6;
        private const int ColCharge = 7;
        private const int ColDischarge = 8;
        private const int ColAcc = 9;

        private const int RowHeader = 0;
        private const int RowPlanet = 1;
        private const int RowStar = 2;
        private const int RowGlobal = 3;

        private static readonly Color BackgroundColor = new Color(0.04f, 0.08f, 0.12f, 1f);
        private static readonly Color HeaderColor = new Color(0.55f, 0.72f, 0.85f, 1f);
        private static readonly Color LabelColor = new Color(0.72f, 0.82f, 0.90f, 1f);
        private static readonly Color ValueColor = new Color(0.94f, 0.96f, 0.98f, 1f);
        private static readonly Color WarningColor = new Color(1.00f, 0.48f, 0.36f, 1f);

        // ゲーム内パネルの配色に合わせる（充電はオレンジ、放電はシアン）。
        private static readonly Color ChargeColor = new Color(1.00f, 0.72f, 0.35f, 1f);
        private static readonly Color DischargeColor = new Color(0.40f, 0.85f, 1.00f, 1f);

        private GameObject _root;
        private RectTransform _rootRect;
        private Image _background;
        private Text[,] _cells;
        private WidgetLabels _labels;

        public bool Exists
        {
            get { return _root != null; }
        }

        /// <summary>ゲーム UI 配下にウィジットを構築する。UI がまだ無ければ false。</summary>
        public bool TryCreate(PowerMeterConfig config)
        {
            if (_root != null)
            {
                return true;
            }

            var parent = FindParent();
            if (parent == null)
            {
                return false;
            }

            _root = new GameObject("PowerMeterWidget", typeof(RectTransform), typeof(Image));
            _rootRect = _root.GetComponent<RectTransform>();
            _rootRect.SetParent(parent, false);
            _rootRect.localScale = Vector3.one;

            _background = _root.GetComponent<Image>();
            _background.raycastTarget = false;

            var font = FindGameFont(parent);
            _cells = new Text[RowCount, ColumnCount];
            for (var row = 0; row < RowCount; row++)
            {
                for (var col = 0; col < ColumnCount; col++)
                {
                    _cells[row, col] = CreateText(_rootRect, "R" + row + "C" + col, font);
                }
            }

            ApplyConfig(config);
            return true;
        }

        /// <summary>設定に沿って寸法・配置・色・ラベルを反映する。</summary>
        public void ApplyConfig(PowerMeterConfig config)
        {
            if (_root == null)
            {
                return;
            }

            _labels = WidgetLabels.For(config.UseJapanese);

            var fontSize = config.FontSize.Value;
            var chargeMode = config.ChargeColumn.Value;

            var pad = Mathf.Round(fontSize * 0.6f);
            var gap = Mathf.Round(fontSize * 0.5f);
            var rowHeight = Mathf.Round(fontSize * 1.55f);
            var labelWidth = Mathf.Round(fontSize * 4.6f);
            var valueWidth = Mathf.Round(fontSize * 5.4f);
            var sepWidth = Mathf.Round(fontSize * 0.9f);
            var pctWidth = Mathf.Round(fontSize * 3.4f);

            // 発電 / 需要 は「a / b」と読ませたいので、この 3 列だけは隙間を空けない。
            var x = pad;
            PlaceColumn(ColLabel, x, labelWidth, rowHeight, pad, fontSize, TextAnchor.MiddleLeft, true);
            x += labelWidth + gap;
            PlaceColumn(ColGen, x, valueWidth, rowHeight, pad, fontSize, TextAnchor.MiddleRight, true);
            x += valueWidth;
            PlaceColumn(ColSep, x, sepWidth, rowHeight, pad, fontSize, TextAnchor.MiddleCenter, true);
            x += sepWidth;
            PlaceColumn(ColCon, x, valueWidth, rowHeight, pad, fontSize, TextAnchor.MiddleRight, true);
            x += valueWidth;

            x = PlaceOptionalColumn(
                ColCap, x, valueWidth, rowHeight, pad, gap, fontSize, config.ShowCapacity.Value);
            x = PlaceOptionalColumn(
                ColUtil, x, pctWidth, rowHeight, pad, gap, fontSize, config.ShowUtilization.Value);
            x = PlaceOptionalColumn(
                ColSat, x, pctWidth, rowHeight, pad, gap, fontSize, config.ShowSatisfaction.Value);
            x = PlaceOptionalColumn(
                ColCharge, x, valueWidth, rowHeight, pad, gap, fontSize, chargeMode != ChargeColumnMode.Off);
            x = PlaceOptionalColumn(
                ColDischarge, x, valueWidth, rowHeight, pad, gap, fontSize, chargeMode == ChargeColumnMode.Split);
            x = PlaceOptionalColumn(
                ColAcc, x, valueWidth, rowHeight, pad, gap, fontSize, config.ShowAccumulated.Value);

            _rootRect.sizeDelta = new Vector2(x + pad, pad * 2f + rowHeight * RowCount);
            ApplyCorner(config);

            var background = BackgroundColor;
            background.a = config.BackgroundOpacity.Value;
            _background.color = background;
            _background.enabled = config.BackgroundOpacity.Value > 0.001f;

            ApplyHeaderTexts(chargeMode);
        }

        /// <summary>集計結果を反映する。</summary>
        public void UpdateValues(
            PowerSnapshot planet, PowerSnapshot star, PowerSnapshot global, PowerMeterConfig config)
        {
            if (_root == null)
            {
                return;
            }

            SetRow(RowPlanet, _labels.Planet, planet, config);
            SetRow(RowStar, _labels.Star, star, config);
            SetRow(RowGlobal, _labels.Global, global, config);
        }

        public void SetVisible(bool visible)
        {
            if (_root != null && _root.activeSelf != visible)
            {
                _root.SetActive(visible);
            }
        }

        public void Destroy()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }

            _root = null;
            _rootRect = null;
            _background = null;
            _cells = null;
        }

        private void SetRow(int row, string label, PowerSnapshot snapshot, PowerMeterConfig config)
        {
            _cells[row, ColLabel].text = label;
            _cells[row, ColSep].text = "/";

            if (!snapshot.IsValid)
            {
                for (var col = ColGen; col < ColumnCount; col++)
                {
                    if (col == ColSep)
                    {
                        continue;
                    }

                    _cells[row, col].text = _labels.NoData;
                    _cells[row, col].color = ValueColor;
                }

                return;
            }

            _cells[row, ColGen].text = PowerFormatter.FormatWatt(snapshot.GenerationWatt);
            _cells[row, ColCon].text = PowerFormatter.FormatWatt(snapshot.ConsumptionWatt);
            _cells[row, ColCap].text = PowerFormatter.FormatWatt(snapshot.CapacityWatt);
            _cells[row, ColAcc].text = PowerFormatter.FormatJoule(snapshot.AccumulatedJoule);

            _cells[row, ColUtil].text = PowerFormatter.FormatPercent(snapshot.UtilizationRatio);
            _cells[row, ColUtil].color =
                snapshot.UtilizationRatio >= config.UtilizationWarningPercent.Value / 100.0
                    ? WarningColor
                    : ValueColor;

            _cells[row, ColSat].text = PowerFormatter.FormatPercent(snapshot.SatisfactionRatio);
            _cells[row, ColSat].color =
                snapshot.SatisfactionRatio < config.SatisfactionWarningPercent.Value / 100.0
                    ? WarningColor
                    : ValueColor;

            if (config.ChargeColumn.Value == ChargeColumnMode.Split)
            {
                _cells[row, ColCharge].text = PowerFormatter.FormatWatt(snapshot.ChargeWatt);
                _cells[row, ColCharge].color = ChargeColor;
                _cells[row, ColDischarge].text = PowerFormatter.FormatWatt(snapshot.DischargeWatt);
                _cells[row, ColDischarge].color = DischargeColor;
            }
            else
            {
                var net = snapshot.NetChargeWatt;
                _cells[row, ColCharge].text = PowerFormatter.FormatSignedWatt(net);
                _cells[row, ColCharge].color = net < 0.0 ? DischargeColor : ChargeColor;
            }
        }

        private void ApplyHeaderTexts(ChargeColumnMode chargeMode)
        {
            _cells[RowHeader, ColLabel].text = _labels.Title;
            _cells[RowHeader, ColGen].text = _labels.Generation;
            _cells[RowHeader, ColSep].text = string.Empty;
            _cells[RowHeader, ColCon].text = _labels.Demand;
            _cells[RowHeader, ColCap].text = _labels.Capacity;
            _cells[RowHeader, ColUtil].text = _labels.Utilization;
            _cells[RowHeader, ColSat].text = _labels.Satisfaction;
            _cells[RowHeader, ColCharge].text =
                chargeMode == ChargeColumnMode.Split ? _labels.Charge : _labels.NetCharge;
            _cells[RowHeader, ColDischarge].text = _labels.Discharge;
            _cells[RowHeader, ColAcc].text = _labels.Stored;

            for (var col = 0; col < ColumnCount; col++)
            {
                _cells[RowHeader, col].color = HeaderColor;
            }

            for (var row = RowPlanet; row <= RowGlobal; row++)
            {
                _cells[row, ColLabel].color = LabelColor;
                _cells[row, ColSep].color = LabelColor;
                _cells[row, ColGen].color = ValueColor;
                _cells[row, ColCon].color = ValueColor;
                _cells[row, ColCap].color = ValueColor;
                _cells[row, ColAcc].color = ValueColor;
            }
        }

        /// <summary>任意表示の列を配置し、次の列の開始位置を返す。</summary>
        private float PlaceOptionalColumn(
            int col, float x, float width, float rowHeight, float pad, float gap, int fontSize, bool visible)
        {
            if (!visible)
            {
                PlaceColumn(col, x, width, rowHeight, pad, fontSize, TextAnchor.MiddleRight, false);
                return x;
            }

            PlaceColumn(col, x + gap, width, rowHeight, pad, fontSize, TextAnchor.MiddleRight, true);
            return x + gap + width;
        }

        private void PlaceColumn(
            int col, float x, float width, float rowHeight, float pad, int fontSize,
            TextAnchor alignment, bool visible)
        {
            for (var row = 0; row < RowCount; row++)
            {
                var text = _cells[row, col];
                text.gameObject.SetActive(visible);
                text.fontSize = fontSize;
                text.alignment = alignment;

                var rect = text.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(width, rowHeight);
                rect.anchoredPosition = new Vector2(x, -(pad + rowHeight * row));
            }
        }

        private void ApplyCorner(PowerMeterConfig config)
        {
            var offsetX = config.OffsetX.Value;
            var offsetY = config.OffsetY.Value;

            Vector2 anchor;
            Vector2 position;
            switch (config.Corner.Value)
            {
                case WidgetCorner.TopLeft:
                    anchor = new Vector2(0f, 1f);
                    position = new Vector2(offsetX, -offsetY);
                    break;
                case WidgetCorner.BottomLeft:
                    anchor = new Vector2(0f, 0f);
                    position = new Vector2(offsetX, offsetY);
                    break;
                case WidgetCorner.BottomRight:
                    anchor = new Vector2(1f, 0f);
                    position = new Vector2(-offsetX, offsetY);
                    break;
                default:
                    anchor = new Vector2(1f, 1f);
                    position = new Vector2(-offsetX, -offsetY);
                    break;
            }

            _rootRect.anchorMin = anchor;
            _rootRect.anchorMax = anchor;
            _rootRect.pivot = anchor;
            _rootRect.anchoredPosition = position;
        }

        private static Text CreateText(RectTransform parent, string name, Font font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var text = go.GetComponent<Text>();
            text.rectTransform.SetParent(parent, false);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.color = ValueColor;
            if (font != null)
            {
                text.font = font;
            }

            return text;
        }

        /// <summary>ゲーム UI から親にできる RectTransform を探す。</summary>
        private static Transform FindParent()
        {
            var root = UIRoot.instance;
            if (root == null)
            {
                return null;
            }

            var game = root.uiGame;
            if (game != null)
            {
                // lowGroup はウィンドウ類より下のレイヤー。HUD 的な常駐表示に向く。
                if (game.lowGroup != null)
                {
                    return game.lowGroup;
                }

                if (game.transform != null)
                {
                    return game.transform;
                }
            }

            return root.overlayCanvas != null ? root.overlayCanvas.transform : null;
        }

        /// <summary>ゲーム UI で実際に使われているフォントを流用する。</summary>
        private static Font FindGameFont(Transform parent)
        {
            try
            {
                var sample = parent.GetComponentInChildren<Text>(true);
                if (sample != null && sample.font != null)
                {
                    return sample.font;
                }
            }
            catch (Exception)
            {
                // フォント探索に失敗しても組み込みフォントで動かす。
            }

            // Text.font が null のままだと何も描画されないため、組み込みフォントへ退避する。
            // Unity 2022.3 では Arial.ttf が LegacyRuntime.ttf に置き換わっている。
            return LoadBuiltinFont("LegacyRuntime.ttf") ?? LoadBuiltinFont("Arial.ttf");
        }

        private static Font LoadBuiltinFont(string name)
        {
            try
            {
                return Resources.GetBuiltinResource<Font>(name);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
