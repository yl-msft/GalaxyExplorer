using UnityEngine;

public class PlacementConfirmationButton : MonoBehaviour
{
    private Animator _animator;
    private static readonly int Visible = Animator.StringToHash("Visible");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Show()
    {
        _animator.SetBool(Visible, true);
    }

    public void Hide()
    {
        _animator.SetBool(Visible, false);
    }
}
