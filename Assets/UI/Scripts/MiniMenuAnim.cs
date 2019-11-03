using System.Collections;
using UnityEngine;

public class MiniMenuAnim : MonoBehaviour
{
	[SerializeField] private float timeToDeactivateAfterMouseExit = 1f;

	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void OnMouseEnter()
	{
		StopAllCoroutines();
		animator.SetTrigger("Activated");
	}

	public void OnMouseExit()
	{
		StopAllCoroutines();
		StartCoroutine(DeactivateMenu(timeToDeactivateAfterMouseExit));
	}

	IEnumerator DeactivateMenu(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		animator.SetTrigger("Idle");
	}
}