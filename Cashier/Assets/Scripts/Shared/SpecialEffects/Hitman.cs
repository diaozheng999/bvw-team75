using System;
using Team75.Client;
using UnityEngine;


namespace Team75.Shared.SpecialEffects {

    public class Hitman : SpecialEffect {
        
        Animator _anim = null;
        Animator anim {
            get {
                if(_anim == null) _anim = GetComponent<Animator>();
                return _anim;
            }
        }

        public Hitman() : base() {
            executionFlag = ExecutionFlag.AFTER_ITEMS | ExecutionFlag.BEFORE_LEAVE;
        }

        protected override void AfterItems(Action cont){
            cont();
            anim.SetTrigger("turnback");
        }

    }

}