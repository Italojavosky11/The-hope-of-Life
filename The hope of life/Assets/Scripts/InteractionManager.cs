using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractionManager : MonoBehaviour
{
    [SerializeField] private KeyCode teclaInteracao = KeyCode.T;
    [SerializeField] private Sprite imagemTeclaT;

    private BauArma bau;
    private Collider2D areaInteracao;
    private GameObject indicadorInteracao;
    private bool jogadorPerto;

    private void Awake()
    {
        bau = GetComponent<BauArma>();
        areaInteracao = GetComponent<Collider2D>();
        areaInteracao.isTrigger = true;

        CriarIndicador();
        AtualizarIndicador(false);
    }

    private void Update()
    {
        if (!jogadorPerto || bau == null || !bau.PodeInteragir)
            return;

        if (Input.GetKeyDown(teclaInteracao))
        {
            if (bau.Interagir())
                AtualizarIndicador(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        jogadorPerto = true;
        AtualizarIndicador(bau != null && bau.PodeInteragir);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        jogadorPerto = false;
        AtualizarIndicador(false);
    }

    private void CriarIndicador()
    {
        if (imagemTeclaT == null)
            return;

        indicadorInteracao = new GameObject("Indicacao de interacao");
        indicadorInteracao.transform.SetParent(transform, true);

        Bounds limites = areaInteracao.bounds;

        indicadorInteracao.transform.position = new Vector3(
            limites.center.x,
            limites.max.y + 0.25f,
            limites.center.z
        );

        SpriteRenderer renderizador = indicadorInteracao.AddComponent<SpriteRenderer>();
        renderizador.sprite = imagemTeclaT;

        indicadorInteracao.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        SpriteRenderer renderizadorBau = GetComponent<SpriteRenderer>();

        if (renderizadorBau != null)
        {
            renderizador.sortingLayerID = renderizadorBau.sortingLayerID;
            renderizador.sortingOrder = renderizadorBau.sortingOrder + 1;
        }
    }

    private void AtualizarIndicador(bool visivel)
    {
        if (indicadorInteracao != null)
            indicadorInteracao.SetActive(visivel);
    }
}