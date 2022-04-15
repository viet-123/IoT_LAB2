using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Newtonsoft.Json;
using M2MqttUnity.Button;
using Newtonsoft.Json.Linq;
using System.Linq;
/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// </summary>
namespace M2MqttUnity.Examples
{
    public class Status_Data
    {
        public string temperature { get; set; }
        public string humidity { get; set; }
    }

    public class Status_Device
    {
        public string device { get; set; }
        public bool status { get; set; }

    }
    /// <summary>
    /// Script for testing M2MQTT with a Unity UI
    /// </summary>
    public class M2MqttUnityTest : M2MqttUnityClient
    {
        public InputField broker_URI;
        public InputField userName;
        public InputField password;
        [SerializeField]
        private CanvasGroup _layer1;
        [SerializeField]
        private CanvasGroup _layer2;
        [SerializeField]
        private GameObject btn_connect;    
        [SerializeField]
        public List<string> topics = new List<string>();
        [SerializeField]
        public Status_Data _status_data;
        public Text bugMsg;
        public Text[] displayValue = new Text[2];
        private List<string> eventMessages = new List<string>();
        private bool updateUI = false;
        public SwitchButton _led;
        public SwitchButton pump;
        private Tween twenFade;

        public void Fade(CanvasGroup _canvas, float endValue, float duration, TweenCallback onFinish)
        {
            if (twenFade != null)
            {
                twenFade.Kill(false);
            }

            twenFade = _canvas.DOFade(endValue, duration);
            twenFade.onComplete += onFinish;
        }

        public void FadeIn(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 1f, duration, () =>
            {
                _canvas.interactable = true;
                _canvas.blocksRaycasts = true;
            });
        }

        public void FadeOut(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 0f, duration, () =>
            {
                _canvas.interactable = false;
                _canvas.blocksRaycasts = false;
            });
        }

        IEnumerator _IESwitchLayer()
        {
            if (_layer1.interactable == true)
            {
                FadeOut(_layer1, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_layer2, 0.25f);
            }
            else
            {
                FadeOut(_layer2, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_layer1, 0.25f);
            }
        }

        public void SwitchLayer()
        {
            StartCoroutine(_IESwitchLayer());
        }

        public void UpdateBeforeConnect()
        {
            if (broker_URI.text == "" || userName.text == "")
            {
                OnConnectionFailed("CONNECTION FAILED!");
            }
            else
            {
                this.brokerAddress = broker_URI.text;
                this.mqttUserName = userName.text;
                this.mqttPassword = password.text;
                this.Connect();
            }
        }

        public void TestPublish()
        {
            string data = "{\"device\":\"PUMP\",\"status\":true}";
            client.Publish("/bkiot/1915916/pump", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
            Debug.Log("publish led config");
        }

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }

        public void AddUiMessage(string msg)
        {
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SwitchLayer();
            SubscribeTopics();
        }

        protected override void SubscribeTopics()
        {
            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

                }
            }
        }

        protected override void UnsubscribeTopics()
        {
            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Unsubscribe(new string[] { topic });
                }
            }
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            bugMsg.text = errorMessage;
        }

        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
        }

        private void UpdateUI()
        {
        }

        protected override void Start()
        {
            updateUI = true;
            base.Start();
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            bugMsg.text = "";
            string msg = System.Text.Encoding.UTF8.GetString(message);
            if (topic == topics[0])
                ProcessMessageStatus(msg);
            if (topic == topics[1])
                ProcessMessageLed(msg);
            if (topic == topics[2])
                ProcessMessagePump(msg);
        }

        private string Convert(string msg)
        {
            string a = "";
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == '.') break;
                a += msg[i];
            }
            return a;
        }

        private void ProcessMessageStatus(string msg)
        {
            Status_Data _status_data = JsonConvert.DeserializeObject<Status_Data>(msg);
            displayValue[0].text = Convert(_status_data.temperature) + "°C";
            displayValue[1].text = Convert(_status_data.humidity) + "%";
        }

        private void ProcessMessageLed(string msg)
        {
            Status_Device _status_data = JsonConvert.DeserializeObject<Status_Device>(msg);
            _led.SetStatus(_status_data.status);
        }

        private void ProcessMessagePump(string msg)
        {
            Status_Device _status_data = JsonConvert.DeserializeObject<Status_Device>(msg);
            pump.SetStatus(_status_data.status);
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            AddUiMessage("Received: " + msg);
        }

        protected override void Update()
        {
            base.Update();
            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
            if (updateUI)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
        }

        public void switch_led()
        {
            if (_led.switchState == false)
            {
                string data = "{\"device\":\"LED\",\"status\":true}";
                client.Publish("/bkiot/1915916/led", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("LED:ON");
            }
            else
            {
                string data = "{\"device\":\"LED\",\"status\":false}";
                client.Publish("/bkiot/1915916/led", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("LED:OFF");
            }
            _led.SetStatus(!_led.switchState);
        }

        public void switch_pump()
        {
            if (pump.switchState == false)
            {
                string data = "{\"device\":\"PUMP\",\"status\":true}";
                client.Publish("/bkiot/1915916/pump", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("PUMP:ON");
            }
            else
            {
                string data = "{\"device\":\"PUMP\",\"status\":false}";
                client.Publish("/bkiot/1915916/pump", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("PUMP:OFF");
            }
            pump.SetStatus(!pump.switchState);
        }

        //public void send_stt()
       // {
        //    string data = "{\"temperature\":\"31.0\",\"humidity\":70.0}";
         //   client.Publish("/bkiot/1915916/status", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
         //   Debug.Log("recived data");
       // }
    }
}
