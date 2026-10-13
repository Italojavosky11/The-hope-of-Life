
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animações básicas")]
    public AnimationClip idleFrente;
    public AnimationClip idleCosta;
    public AnimationClip walkLado;
    public AnimationClip walkFrente;
    public AnimationClip walkCosta;

    [Header("Revolver")]
    public AnimationClip walkRevolverLado;
    public AnimationClip walkRevolverFrente;
    public AnimationClip walkRevolverCosta;

    [Header("Colt")]
    public AnimationClip walkColtLado;
    public AnimationClip walkColtFrente;
    public AnimationClip walkColtCosta;

    [Header("Springfield")]
    public AnimationClip walkSpringfieldLado;
    public AnimationClip walkSpringfieldFrente;
    public AnimationClip walkSpringfieldCosta;

    private Animation animacao;
    private AnimationClip animacaoAtual;

    private void Awake()
    {
        animacao = GetComponent<Animation>();
        animacao.playAutomatically = false;
        animacao.Stop();
    }

    public void AtualizarAnimacao(
        Vector2 movimento,
        int direcao,
        int armaAtual,
        bool atacando)
    {
        AnimationClip clip = EscolherAnimacao(
            movimento,
            direcao,
            armaAtual,
            atacando
        );

        Reproduzir(clip);
    }

    private AnimationClip EscolherAnimacao(
        Vector2 movimento,
        int direcao,
        int armaAtual,
        bool atacando)
    {
        bool andando = movimento.sqrMagnitude > 0.01f;

        if (!andando && !atacando)
        {
            if (direcao == 1)
                return idleCosta;

            return idleFrente;
        }

        if (armaAtual == 1)
            return EscolherArma(
                andando, direcao,
                walkRevolverLado,
                walkRevolverFrente,
                walkRevolverCosta
            );

        if (armaAtual == 2)
            return EscolherArma(
                andando, direcao,
                walkColtLado,
                walkColtFrente,
                walkColtCosta
            );

        if (armaAtual == 3)
            return EscolherArma(
                andando, direcao,
                walkSpringfieldLado,
                walkSpringfieldFrente,
                walkSpringfieldCosta
            );

        if (andando)
        {
            if (direcao == 1)
                return walkCosta;

            if (direcao == 2)
                return walkFrente;

            return walkLado;
        }

        return direcao == 1 ? idleCosta : idleFrente;
    }

    private AnimationClip EscolherArma(
        bool andando,
        int direcao,
        AnimationClip lado,
        AnimationClip frente,
        AnimationClip costa)
    {
        if (andando)
        {
            if (direcao == 1)
                return costa;

            if (direcao == 2)
                return frente;

            return lado;
        }

        if (direcao == 1 && costa != null)
            return costa;

        if (direcao == 2 && frente != null)
            return frente;

        return lado;
    }

    private void Reproduzir(AnimationClip clip)
    {
        if (clip == null || animacao == null)
            return;

        if (animacaoAtual == clip && animacao.isPlaying)
            return;

        animacao.Stop();
        animacao.RemoveClip(clip);
        animacao.AddClip(clip, clip.name);
        animacao.Play(clip.name);

        animacaoAtual = clip;
    }
}
