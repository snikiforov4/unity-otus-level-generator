using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DOTweenDemo : MonoBehaviour
{
    public float variable;
    public AnimationCurve curve;
    Tween tween = default;

    // Start is called before the first frame update
    void Start()
    {
        /*
        DOTween.To(
            () => variable,
            (v) => variable = v,
            100.0f,
            20.0f).From(200.0f);
        */

        /*
        tween = transform.DOMoveX(10.0f, 5.0f)
            .SetEase(curve)
            .SetLoops(2) // -1 не останавливаться
            .OnComplete(() => Debug.Log("Complete"))
            ;

        transform.DORotate(new Vector3(0, 0, 90), 5.0f);

        transform.DOPause();
        transform.DOKill(true); // false - останется, где был; true - телепорт на последний кадр

        //tween.IsPlaying;
        //tween.IsComplete;

        tween.Kill(true);
        tween.Pause();
        */

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveX(10, 3));
        seq.Append(transform.DORotate(new Vector3(0, 0, 90), 1));
        seq.Insert(3.0f, transform.DOShakePosition(1.0f));
        seq.Append(transform.DOMoveY(10, 3));
        seq.AppendInterval(1);
        seq.Append(transform.DOMoveY(0, 3));
    }

    IEnumerator Coroutine()
    {
        yield return tween.WaitForCompletion();
    }
}
