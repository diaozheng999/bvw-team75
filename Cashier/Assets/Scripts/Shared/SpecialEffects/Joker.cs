using System;
using System.Collections;
using UnityEngine;


namespace Team75.Shared.SpecialEffects {

    public class Joker : SpecialEffect {
        Animator _anim = null;

        Joker() : base() {
            executionFlag = ExecutionFlag.AFTER_DEQUEUE | ExecutionFlag.BEFORE_LEAVE;
        }

        Animator anim {
            get {
                if(_anim == null) _anim = GetComponent<Animator>();
                return _anim;
            }
        }

        protected override void AfterDequeue(Action cont){
            Debug.Log("hi!");
            anim.SetTrigger("Aim");
            StartCoroutine(_afterDequeue(cont));
        }

        IEnumerator _afterDequeue(Action cont){
            yield return new WaitForSeconds(0.5f);
            cont();
        }

        protected override void BeforeLeave(Action cont){
            anim.SetTrigger("Fire");
            StartCoroutine(_beforeLeave(cont));
        }
        
        IEnumerator _beforeLeave(Action cont){
            yield return new WaitForSeconds(0.5f);
            cont();
        }
    }

}