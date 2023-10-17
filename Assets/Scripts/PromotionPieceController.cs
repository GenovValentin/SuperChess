using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class PromotionPieceController
    {
        public ColorBlockController colorBlockController;

        private Dictionary<int, int>
            promotionLines =
                new Dictionary<int, int>()
                {
                    [7] = -200,
                    [6] = -120,
                    [5] = -30,
                    [4] = 50,
                    [3] = 140,
                    [2] = 220,
                    [1] = 310,
                    [0] = 400
                };

        private Texture whiteQueenImage;

        private Texture blackQueenImage;

        private Texture currentQueenImage;

        private Texture whiteRookImage;

        private Texture blackRookImage;

        private Texture currentRookImage;

        private Texture whiteBishopImage;

        private Texture blackBishopImage;

        private Texture currentBishopImage;

        private Texture whiteKnightImage;

        private Texture blackKnightImage;

        private Texture currentKnightImage;

        GameObject promotionPiecesObject;

        GameObject promotionQueen;

        GameObject promotionRook;

        GameObject promotionBishop;

        GameObject promotionKnight;

        GameObject promotionQueenImage;

        GameObject promotionRookImage;

        GameObject promotionBishopImage;

        GameObject promotionKnightImage;

        public PromotionPieceController()
        {
            colorBlockController = new ColorBlockController();
        }

        public void SetColors()
        {
            colorBlockController.SetColors();
        }

        public void SetPromotionPiecesImagesPaths()
        {
            whiteQueenImage = Resources.Load<Texture>("Queen");
            blackQueenImage = Resources.Load<Texture>("Queen_B");
            whiteRookImage = Resources.Load<Texture>("Rook");
            blackRookImage = Resources.Load<Texture>("Rook_B");
            whiteBishopImage = Resources.Load<Texture>("Bishop");
            blackBishopImage = Resources.Load<Texture>("Bishop_B");
            whiteKnightImage = Resources.Load<Texture>("Knight");
            blackKnightImage = Resources.Load<Texture>("Knight_B");
        }

        public void SetPromotionPiecesObjects(GameObject promotionPieces)
        {
            promotionPiecesObject =
                promotionPieces.transform.GetChild(0).gameObject;

            SetPromotionPiecesObject(promotionPieces,
            ref promotionQueen,
            ref promotionQueenImage,
            0);
            SetPromotionPiecesObject(promotionPieces,
            ref promotionRook,
            ref promotionRookImage,
            1);
            SetPromotionPiecesObject(promotionPieces,
            ref promotionBishop,
            ref promotionBishopImage,
            2);
            SetPromotionPiecesObject(promotionPieces,
            ref promotionKnight,
            ref promotionKnightImage,
            3);
        }

        public void SetPromotionPiecesWantedImages(Team team)
        {
            Texture queenImage = whiteQueenImage;
            Texture rookImage = whiteRookImage;
            Texture bishopImage = whiteBishopImage;
            Texture knightImage = whiteKnightImage;

            if (team == Team.Black)
            {
                queenImage = blackQueenImage;
                rookImage = blackRookImage;
                bishopImage = blackBishopImage;
                knightImage = blackKnightImage;
            }

            SetPromotionPiecesCurrentImages (
                queenImage,
                rookImage,
                bishopImage,
                knightImage
            );
        }

        public void SetPromotionPiecesImage(Team team)
        {
            SetPromotionPiecesWantedImages (team);

            SetPromotionPiecesImages (promotionQueenImage, currentQueenImage);
            SetPromotionPiecesImages (promotionRookImage, currentRookImage);
            SetPromotionPiecesImages (promotionBishopImage, currentBishopImage);
            SetPromotionPiecesImages (promotionKnightImage, currentKnightImage);
        }

        public void SetPromotionPiecesColor(Team team)
        {
            Button promotionQueenButton = promotionQueen.GetComponent<Button>();
            Button promotionRookButton = promotionRook.GetComponent<Button>();
            Button promotionBishopButton =
                promotionBishop.GetComponent<Button>();
            Button promotionKnightButton =
                promotionKnight.GetComponent<Button>();
            ColorBlock colorBlock = promotionQueenButton.colors;

            SetPromotionPiecesWantedColors (team, colorBlock);

            SetPromotionPiecesColors(ref promotionQueenButton,
            ref promotionQueen,
            colorBlock);
            SetPromotionPiecesColors(ref promotionRookButton,
            ref promotionRook,
            colorBlock);
            SetPromotionPiecesColors(ref promotionBishopButton,
            ref promotionBishop,
            colorBlock);
            SetPromotionPiecesColors(ref promotionKnightButton,
            ref promotionKnight,
            colorBlock);
        }

        public void SetPromotionPiecesObjectPosition(
            Team team,
            Vector2Int lastMove
        )
        {
            int x = GetPromotionPiecesObjectX(team, lastMove.x);

            Vector3 newPosition = new Vector3(x, 500f, 0f);
            SetNewPosition (newPosition);
        }

        public int GetPromotionPiecesObjectX(Team team, int moveX)
        {
            int line = team == Team.White ? moveX : (7 - moveX);

            return promotionLines[moveX];
        }

        private void SetPromotionPiecesObject(
            GameObject promotionPieces,
            ref GameObject promotionPiece,
            ref GameObject promotionPieceImage,
            int x = 0
        )
        {
            promotionPiece =
                promotionPieces.transform.GetChild(0).GetChild(x).gameObject;
            promotionPieceImage =
                promotionPiece.transform.GetChild(0).gameObject;
        }

        private void SetPromotionPiecesColors(
            ref Button promotionButton,
            ref GameObject promotionPiece,
            ColorBlock colorBlock
        )
        {
            promotionButton.colors = colorBlock;
            promotionPiece.GetComponent<Image>().color =
                colorBlockController.GetCurrentColor();
        }

        private void SetPromotionPiecesWantedColors(
            Team team,
            ColorBlock colorBlock
        )
        {
            colorBlockController.SetPromotionPiecesWantedColors (
                team,
                colorBlock
            );
        }

        private void SetPromotionPiecesImages(
            GameObject pieceImage,
            Texture currentPieceImage
        )
        {
            pieceImage.GetComponent<RawImage>().texture = currentPieceImage;
        }

        private void SetPromotionPiecesCurrentImages(
            Texture wantedQueenImage,
            Texture wantedRookImage,
            Texture wantedBishopImage,
            Texture wantedKnightImage
        )
        {
            currentQueenImage = wantedQueenImage;
            currentRookImage = wantedRookImage;
            currentBishopImage = wantedBishopImage;
            currentKnightImage = wantedKnightImage;
        }

        private void SetNewPosition(Vector3 newPosition)
        {
            promotionPiecesObject.transform.position =
                promotionPiecesObject
                    .transform
                    .parent
                    .TransformPoint(newPosition);
        }
    }
}
