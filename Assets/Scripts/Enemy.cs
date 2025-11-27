using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float destroyDelay = 3f;       // 달라붙은 후 사라지는 시간
    public float shakeIntensity = 0.1f;   // 흔들림 강도
    public float shakeSpeed = 20f;        // 흔들림 속도
    public GameObject deathEffect;        // 사라질 때 나올 파티클
    public AudioClip deathSound;          // 사라질 때 나올 소리

    private bool isAttached = false;
    private Vector3 originalPos;
    private float shakeTimer = 0f;

    private AudioSource audioSource;
    private Animator animator;

    void Start()
    {
        originalPos = transform.position;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isAttached)
        {
            // 흔들림 연출
            shakeTimer += Time.deltaTime * shakeSpeed;
            float offsetX = Mathf.Sin(shakeTimer) * shakeIntensity;
            float offsetZ = Mathf.Cos(shakeTimer) * shakeIntensity;
            transform.position = originalPos + new Vector3(offsetX, 0, offsetZ);

            // 소멸 카운트
            destroyDelay -= Time.deltaTime;
            if (destroyDelay <= 0)
            {
                Die();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isAttached && collision.gameObject.CompareTag("Player"))
        {
            isAttached = true;
            originalPos = transform.position;

           

            // 필요시 사운드 재생
            if (audioSource && deathSound) audioSource.PlayOneShot(deathSound);
        }
    }

    private void Die()
    {
        // 파티클 효과 생성
        if (deathEffect)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
