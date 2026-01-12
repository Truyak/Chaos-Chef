using UnityEngine;
using System;

public class ActionProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private Action onHitCallback;
    private float speed = 15f;
    private bool isInitialized = false;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Setup(Sprite sprite, Vector3 target, Action onHit)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;

        targetPosition = target;
        onHitCallback = onHit;
        isInitialized = true;

        // Yönü hedefe çevir (opsiyonel, sprite'a göre değişir)
        // transform.up = (target - transform.position).normalized;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Hedefe git
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Dönme efekti (görsel şov)
        transform.Rotate(0, 0, -360f * Time.deltaTime);

        // Hedefe ulaştı mı?
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            onHitCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}
