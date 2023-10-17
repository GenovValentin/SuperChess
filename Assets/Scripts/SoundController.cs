using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class SoundController
    {
        private AudioSource Board;

        private AudioSource Pieces;

        private AudioSource Castle;

        private AudioSource Capture;

        private AudioSource Promote;

        private AudioSource Swoosh1;

        private AudioSource Swoosh2;

        private AudioSource Swoosh3;

        public void PlaySpecialMoveSound(SpecialMove specialMove)
        {
            if (specialMove == SpecialMove.None)
            {
                Pieces.Play();
            }
            else if (specialMove == SpecialMove.Castling)
            {
                Castle.Play();
            }
            else if (specialMove == SpecialMove.EnPassant)
            {
                Capture.Play();
            }
            else if (specialMove == SpecialMove.Promotion)
            {
                Promote.Play();
            }
            else if (specialMove == SpecialMove.Capture)
            {
                Capture.Play();
            }
        }

        public void PlaySwooshSound()
        {
            int swooshSoundToBePlayed = Random.Range(1, 4);
            switch (swooshSoundToBePlayed)
            {
                case 1:
                    Swoosh1.Play();
                    break;
                case 2:
                    Swoosh2.Play();
                    break;
                case 3:
                    Swoosh3.Play();
                    break;
            }
        }

        public void SetSounds()
        {
            GameObject boardSound = GameObject.Find("BoardSound");
            Board = boardSound.GetComponent<AudioSource>();
            GameObject piecesSound = GameObject.Find("PiecesSound");
            Pieces = piecesSound.GetComponent<AudioSource>();
            GameObject castleSound = GameObject.Find("CastleSound");
            Castle = castleSound.GetComponent<AudioSource>();
            GameObject captureSound = GameObject.Find("CaptureSound");
            Capture = captureSound.GetComponent<AudioSource>();
            GameObject promoteSound = GameObject.Find("PromoteSound");
            Promote = promoteSound.GetComponent<AudioSource>();
            GameObject swooshSound1 = GameObject.Find("SwooshSound1");
            Swoosh1 = swooshSound1.GetComponent<AudioSource>();
            GameObject swooshSound2 = GameObject.Find("SwooshSound2");
            Swoosh2 = swooshSound2.GetComponent<AudioSource>();
            GameObject swooshSound3 = GameObject.Find("SwooshSound3");
            Swoosh3 = swooshSound3.GetComponent<AudioSource>();
        }

        public void PlayBoardSound(int delayed = 0)
        {
            PlaySound (Board, delayed);
        }

        public void PlayPiecesSound(int delayed = 0)
        {
            PlaySound (Pieces, delayed);
        }

        public void PlaySound(AudioSource source, int delayed = 0)
        {
            source.PlayDelayed (delayed);
        }
    }
}
