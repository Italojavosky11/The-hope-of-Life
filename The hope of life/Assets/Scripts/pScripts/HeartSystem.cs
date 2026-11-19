using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class HeartSystem : MonoBehaviour
{
    public float vida;
    public float vidaMaxima;

    public Image[] coracao;
    public Sprite cheio;
    public Sprite meio;
    public Sprite vazio;

    public Player player;

    private bool morreu = false;
    private EfeitoDano efeitoDano;
    private Animation animacaoMorte;
    private Animator animator;
    private Player controladorPlayer;
    private Rigidbody2D corpo;
    private Collider2D colisao;
    private static readonly int TriggerMorte = Animator.StringToHash("Morrer");
    private static readonly int EstadoMorte = Animator.StringToHash("Base Layer.Morte");
    [SerializeField] private AnimationClip clipMorte;
    [SerializeField] private GameObject painelMorte;

    private bool aguardandoReinicio;

    void Awake()
    {
        efeitoDano = GetComponent<EfeitoDano>();
        animacaoMorte = GetComponent<Animation>();
        animator = GetComponent<Animator>();
        controladorPlayer = player != null ? player : GetComponent<Player>();
        corpo = GetComponent<Rigidbody2D>();
        colisao = GetComponent<Collider2D>();

        if (painelMorte == null)
            painelMorte = EncontrarPainelMorte();

        if (painelMorte != null)
            painelMorte.SetActive(false);
    }

    void Start()
    {
        LogicaCoracao();
    }

    public void TomarDano(float dano)
    {
        if (morreu)
            return;

        vida -= dano;

        if (dano > 0f)
            efeitoDano?.AtivarEfeitoDano();

        if (vida < 0)
            vida = 0;

        LogicaCoracao();
        DeadStage();
    }

    void LogicaCoracao()
    {
        if (vida > vidaMaxima)
        {
            vida = vidaMaxima;
        }

        for (int i = 0; i < coracao.Length; i++)
        {
            if (vida >= i + 1)
            {
                coracao[i].sprite = cheio;
            }
            else if (vida >= i + 0.5f)
            {
                coracao[i].sprite = meio;
            }
            else
            {
                coracao[i].sprite = vazio;
            }

            coracao[i].enabled = (i < vidaMaxima);
        }
    }

    void DeadStage()
    {
        if (vida <= 0 && !morreu)
        {
            morreu = true;

            if (controladorPlayer != null)
            {
                controladorPlayer.enabled = false;
                controladorPlayer.podeMover = false;
            }
            if (corpo != null)
            {
                corpo.linearVelocity = Vector2.zero;
                corpo.simulated = false;
            }
            if (colisao != null) colisao.enabled = false;
            if (animator != null)
            {
                animator.ResetTrigger(TriggerMorte);
                animator.SetTrigger(TriggerMorte);
            }
            StartCoroutine(AguardarFimAnimacaoMorte());
        }
    }

    private IEnumerator AguardarFimAnimacaoMorte()
    {
        float duracaoMaxima = (clipMorte != null ? clipMorte.length : 1.04f) + 1f;
        float tempo = 0f;
        while (tempo < duracaoMaxima)
        {
            yield return null;
            tempo += Time.deltaTime;

            if (animator == null || !animator.enabled)
                continue;

            AnimatorStateInfo estado = animator.GetCurrentAnimatorStateInfo(0);
            if (estado.fullPathHash != EstadoMorte)
                continue;

            if (estado.normalizedTime >= 1f)
                break;
        }

        bool exibirPainel = CheckpointManager.Instance == null ||
            CheckpointManager.Instance.RegistrarMorte();

        if (exibirPainel)
        {
            aguardandoReinicio = true;
            if (painelMorte != null)
                painelMorte.SetActive(true);
            else
                Debug.LogError("Painel de morte não configurado no HeartSystem. Arraste o painel do Level1 para o campo Painel Morte.", this);

            Time.timeScale = 0f;
            yield break;
        }

        Respawn();
    }

    private GameObject EncontrarPainelMorte()
    {
        string[] nomesPossiveis = { "PainelDeMorte", "PanelDeMorte", "PainelMorte", "DeathPanel" };
        GameObject[] objetos = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (string nome in nomesPossiveis)
        {
            foreach (GameObject objeto in objetos)
            {
                if (objeto.name == nome && objeto.scene.IsValid())
                    return objeto;
            }
        }

        return null;
    }

    // Conecte este método ao botão Back do painel de morte no Inspector.
    public void VoltarAoInicioDaFase()
    {
        if (!aguardandoReinicio)
            return;

        Time.timeScale = 1f;
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.RestaurarDesbloqueiosIniciaisDaFase();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Respawn()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.Respawn(gameObject);
        }

        vida = vidaMaxima;
        if (animacaoMorte != null) animacaoMorte.Stop();
        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
            animator.ResetTrigger(TriggerMorte);
            animator.SetBool("Atacando", false);
            animator.SetFloat("speed", 0f);
            animator.SetInteger("Direcao", 0);
        }
        if (corpo != null)
        {
            corpo.simulated = true;
            corpo.linearVelocity = Vector2.zero;
        }
        if (colisao != null) colisao.enabled = true;
        if (controladorPlayer != null)
        {
            controladorPlayer.RestaurarEstadoAposRespawn();
            controladorPlayer.podeMover = true;
            controladorPlayer.enabled = true;
        }

        morreu = false;

        LogicaCoracao();
    }

    public void RestaurarVida(float vidaSalva)
    {
        vida = Mathf.Clamp(vidaSalva, 0f, vidaMaxima);
        morreu = false;
        LogicaCoracao();
    }
}
