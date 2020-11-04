using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GamePiece))]
public class ClearablePiece : MonoBehaviour
{
    public AnimationClip clearAnimation;
    protected GamePiece piece;

    void Awake()
    {
        piece = GetComponent<GamePiece>();
    }

    public virtual void Clear()
    {
        piece.GridRef.PieceCleared(piece);
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play(clearAnimation.name);

            yield return new WaitForSeconds(clearAnimation.length);

            Destroy(gameObject);
        }
    }
}
