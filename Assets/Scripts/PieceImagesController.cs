using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class PieceImagesController
    {
        public Texture whiteQueenImage;

        public Texture blackQueenImage;

        public Texture currentQueenImage;

        public Texture whiteRookImage;

        public Texture blackRookImage;

        public Texture currentRookImage;

        public Texture whiteBishopImage;

        public Texture blackBishopImage;

        public Texture currentBishopImage;

        public Texture whiteKnightImage;

        public Texture blackKnightImage;

        public Texture currentKnightImage;

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

        public void SetPromotionPiecesImage(
            Team team,
            GameObject promotionQueenImage,
            GameObject promotionRookImage,
            GameObject promotionBishopImage,
            GameObject promotionKnightImage
        )
        {
            SetPromotionPiecesWantedImages (team);

            SetPromotionPiecesImages (promotionQueenImage, currentQueenImage);
            SetPromotionPiecesImages (promotionRookImage, currentRookImage);
            SetPromotionPiecesImages (promotionBishopImage, currentBishopImage);
            SetPromotionPiecesImages (promotionKnightImage, currentKnightImage);
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

        private void SetPromotionPiecesImages(
            GameObject pieceImage,
            Texture currentPieceImage
        )
        {
            pieceImage.GetComponent<RawImage>().texture = currentPieceImage;
        }
    }
}
