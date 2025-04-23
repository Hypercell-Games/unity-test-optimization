using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Unpuzzle
{
    public class TMPTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        private readonly TimeSpan _timerTick = new(0, 0, 1);

        private DateTime _endTime;

        private TimeSpan _timeSpan;

        private void OnApplicationPause(bool pause)
        {
            if (_endTime <= DateTime.Now)
            {
                _timeSpan = new TimeSpan(0, 0, 0);
                return;
            }

            _timeSpan = _endTime - DateTime.Now;
        }

        public void Init(DateTime endTime)
        {
            _endTime = endTime;
            _timeSpan = endTime - DateTime.Now;
            UpdateTextLabel();
        }

        private void UpdateTextLabel()
        {
            if (_timeSpan.TotalMinutes < 60)
            {
                _text.text = string.Format("{0:00}:{1:00}", _timeSpan.Minutes, _timeSpan.Seconds);
            }
            else
            {
                _text.text = TimerUtils.GetTimeStr(_timeSpan);
            }
        }

        public IEnumerator TimeRoutine()
        {
            while (_timeSpan.TotalSeconds > 0)
            {
                yield return new WaitForSeconds(1f);
                _timeSpan = _timeSpan.Subtract(_timerTick);
                UpdateTextLabel();
            }
        }
    }
}
