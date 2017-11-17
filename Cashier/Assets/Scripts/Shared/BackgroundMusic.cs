using UnityEngine;
using PGT.Core;


namespace Team75.Shared {

    public class BackgroundMusic : Singleton<BackgroundMusic> {

        [SerializeField] AudioClip menu;
        [SerializeField] AudioClip inGame;

        AudioSource asrc;

        void Start() {
            asrc = GetComponent<AudioSource>();
        }

        public void StartGame() {
            asrc.clip = inGame;
            asrc.Play();
        }

        public void StopGame() {
            asrc.clip = menu;
            asrc.Play();
        }

    }

}