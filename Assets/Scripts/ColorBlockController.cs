using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ColorBlockController
    {
        Color whiteColor;

        Color blackColor;

        Color currentColor;

        Color whiteHighlightedColor;

        Color blackHighlightedColor;

        Color currentHighlightedColor;

        Color whitePressedColor;

        Color blackPressedColor;

        Color currentPressedColor;

        public void SetColors()
        {
            ColorUtility.TryParseHtmlString("#A19984", out whiteColor);
            ColorUtility.TryParseHtmlString("#323232", out blackColor);
            ColorUtility
                .TryParseHtmlString("#9C9191", out whiteHighlightedColor);
            ColorUtility
                .TryParseHtmlString("#B09B9B", out blackHighlightedColor);
            ColorUtility.TryParseHtmlString("#666666", out whitePressedColor);
            ColorUtility.TryParseHtmlString("#202020", out blackPressedColor);
        }

        public Color GetCurrentColor()
        {
            return currentColor;
        }

        public void SetPromotionPiecesWantedColors(
            Team team,
            ColorBlock colorBlock
        )
        {
            if (team == Team.White)
            {
                SetPromotionPiecesCurrentColors (
                    whiteColor,
                    whiteHighlightedColor,
                    whitePressedColor
                );
            }
            else if (team == Team.Black)
            {
                SetPromotionPiecesCurrentColors (
                    blackColor,
                    blackHighlightedColor,
                    blackPressedColor
                );
            }
            colorBlock.highlightedColor = currentHighlightedColor;
            colorBlock.pressedColor = currentPressedColor;
        }

        private void SetPromotionPiecesCurrentColors(
            Color wantedColor,
            Color wantedHighlightedColor,
            Color wantedPressedColor
        )
        {
            currentColor = wantedColor;
            currentHighlightedColor = wantedHighlightedColor;
            currentPressedColor = wantedPressedColor;
        }
    }
}
