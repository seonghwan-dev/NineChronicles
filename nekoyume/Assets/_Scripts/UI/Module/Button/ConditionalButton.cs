using System;
using UnityEngine;
using UnityEngine.UI;
using Nekoyume.L10n;

namespace Nekoyume.UI.Module
{
    using System.Collections.Generic;
    using TMPro;
    using UniRx;

    public class ConditionalButton : MonoBehaviour
    {
        public enum State
        {
            Normal,
            Conditional,
            Disabled
        }

        [SerializeField]
        private GameObject normalObject = null;

        [SerializeField]
        private GameObject conditionalObject = null;

        [SerializeField]
        private GameObject disabledObject = null;

        [SerializeField]
        private string conditionInfoKey = null;

        [SerializeField]
        private List<TextMeshProUGUI> texts = null;

        public readonly Subject<State> OnClickSubject = new Subject<State>();

        public readonly Subject<Unit> OnSubmitSubject = new Subject<Unit>();

        public bool IsSubmittable => _interactable && CurrentState.Value == State.Normal;

        public string Text
        {
            get => texts[0].text;
            set
            {
                foreach (var text in texts)
                {
                    text.text = value;
                }
            }
        }

        public readonly ReactiveProperty<State> CurrentState = new ReactiveProperty<State>();

        private Button _button = null;
        private Func<bool> _conditionFunc = null;
        private bool _interactable = true;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                UpdateObjects();
            }
        }

        protected virtual void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClickButton);
        }

        protected virtual bool CheckCondition()
        {
            return _conditionFunc?.Invoke() ?? true;
        }

        public void SetCondition(Func<bool> conditionFunc)
        {
            _conditionFunc = conditionFunc;
        }

        public State UpdateObjects()
        {
            var condition = CheckCondition();
            SetSubmittable(condition);

            if (_interactable)
            {
                UpdateState(condition ? State.Normal : State.Conditional);
            }
            else
            {
                UpdateState(State.Disabled);
            }

            disabledObject.SetActive(!_interactable);
            return CurrentState.Value;
        }

        public void SetSubmittable(bool value)
        {
            if (_interactable)
            {
                UpdateState(value ? State.Normal : State.Conditional);
            }
            else
            {
                UpdateState(State.Disabled);
            }
        }

        public void UpdateState(State state)
        {
            switch (CurrentState.Value)
            {
                case State.Normal:
                    normalObject.SetActive(true);
                    conditionalObject.SetActive(false);
                    CurrentState.Value = State.Normal;
                    break;
                case State.Conditional:
                    normalObject.SetActive(false);
                    conditionalObject.SetActive(true);
                    CurrentState.Value = State.Conditional;
                    break;
                case State.Disabled:
                    normalObject.SetActive(false);
                    conditionalObject.SetActive(false);
                    CurrentState.Value = State.Disabled;
                    break;
            }
        }

        protected virtual void OnClickButton()
        {
            OnClickSubject.OnNext(CurrentState.Value);
            if (IsSubmittable)
            {
                OnSubmitSubject.OnNext(default);
            }

            switch (CurrentState.Value)
            {
                case State.Normal:
                    UpdateObjects();
                    break;
                case State.Conditional:
                    if (!string.IsNullOrEmpty(conditionInfoKey))
                        NotificationSystem.Push(Nekoyume.Model.Mail.MailType.System, L10nManager.Localize(conditionInfoKey));
                    break;
                case State.Disabled:
                    break;
            }
        }
    }
}
