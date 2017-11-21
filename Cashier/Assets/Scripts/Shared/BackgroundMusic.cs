using UnityEngine;
using PGT.Core;
using System.Collections;
using System;


namespace Team75.Shared {

    public class BackgroundMusic : Singleton<BackgroundMusic> {

        [SerializeField] AudioClip menu;
        [SerializeField] AudioClip inGame;
        [SerializeField] AudioClip frenzy;

        [SerializeField] AudioClip bossStart;
        [SerializeField] AudioClip bossEnd;
        [SerializeField] AudioClip bossHint;

        AudioSource asrc;

        void Start() {
            DontDestroyOnLoad(this);
            asrc = GetComponent<AudioSource>();
        }

        public void StartGame(Action onComplete=null) {
            asrc.clip = bossStart;
            asrc.Play();
            StartCoroutine(DelayBGM(onComplete));
        }

        IEnumerator DelayBGM(Action onComplete = null) {
            yield return new WaitForSeconds(bossStart.length+0.2f);
            onComplete?.Invoke();
            asrc.clip = inGame;
            asrc.Play();
        }

        public void StartFrenzy() {
            asrc.clip = frenzy;
            asrc.Play();
        }

        public void StopGame() {
            asrc.clip = menu;
            asrc.Play();
            PlayBossEnd();
        }

        public void PlayBossEnd() {
            asrc.PlayOneShot(bossEnd);
        }

        public void PlayBossHint() {
            asrc.PlayOneShot(bossHint);
        }

    }

}