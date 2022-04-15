using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace M2MqttUnity.Button
{
    public class SwitchButton : MonoBehaviour
    {
        // Start is called before the first frame update
        public bool switchState = false;
        public GameObject switchBtn;

        public void OnSwitchButtonClicked()
        {
            switchBtn.transform.DOLocalMoveX(-switchBtn.transform.localPosition.x, 0.2f);
            switchState = !switchState;
        }
        public void SetStatus(bool state)
        {
            if (state != switchState)
            {
                OnSwitchButtonClicked();
                switchState = state;
            }
        }
    }
}