
using UnityEngine;
using System.Collections;

public class EfeitoDano : MonoBehaviour
{
    [SerializeField] private float duracao = 3f;
    [SerializeField] private float intervalo = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private Coroutine efeitoAtual;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        corOriginal = spriteRenderer.color;
    }

    public void AtivarEfeitoDano()
    {
        if (efeitoAtual != null)
            StopCoroutine(efeitoAtual);

        efeitoAtual = StartCoroutine(PiscarVermelho());
    }

    IEnumerator PiscarVermelho()
    {
        float tempo = 0f;

        while (tempo < duracao)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(intervalo);

            spriteRenderer.color = corOriginal;
            yield return new WaitForSeconds(intervalo);

            tempo += intervalo * 2;
        }

        spriteRenderer.color = corOriginal;
        efeitoAtual = null;
    }
}
