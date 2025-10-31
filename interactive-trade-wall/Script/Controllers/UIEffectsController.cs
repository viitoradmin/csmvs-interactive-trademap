using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIEffectsController : MonoBehaviour
{
    [Header("Effects")]
    //public List<UIEffectTweener> uiEffects;
    public UIEffectTweener[] uiEffects;
    private float duration = 1;
    public float buffer_duration = 1;

    public Coroutine effect_coroutine;

    //void Start()
    //{

    //}

    //public void SetupUIElements()
    //{
    //    uiEffects = GetComponentsInChildren<UIEffectTweener>(false);
    //}

    //public void PlayUIEffects(bool _forward,UnityAction onComplete = null)
    //{
    //    if (effect_coroutine != null)
    //    {
    //        return;
    //    }
    //    effect_coroutine = StartCoroutine(PlayUIEffectsCoroutine(_forward,onComplete));
    //}
    public IEnumerator PlayUIEffectsCoroutine(bool _forward, UnityAction onComplete = null)
    {
        uiEffects = GetComponentsInChildren<UIEffectTweener>(false);
        duration = uiEffects[0].duration;
        foreach (var e in uiEffects)
        {
            if (_forward)
                e.PlayForward();
            else
                e.PlayReverse();
        }
        yield return new WaitForSecondsRealtime(duration + buffer_duration);
        onComplete?.Invoke();
        //effect_coroutine = null;
    }


}
