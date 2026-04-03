
using System.Reflection;
using UnityEngine;

// Script responsável por gerenciar recursos (água/comida), aplicar buffs e aplicar dano ao player
// Regras implementadas conforme especificado pelo usuário:
// - Velocidade depende APENAS de água cheia
// - Força depende APENAS de comida cheia
// - Dano: (sem água && sem comida) -> -1; (sem água || sem comida) -> -0.5
// - Dano aplicado a cada damageInterval segundos (padrão 2s)
public class Buffs : MonoBehaviour
{
    // Vida em corações (1 = 1 coração)
    public float vida = 3f;

    // Recursos
    public float aguaAtual = 0f;
    public float aguaMax = 100f;
    public float comidaAtual = 0f;
    public float comidaMax = 100f;

    // Velocidade
    public float velocidadeBase = 5f;
    public float bonusVelocidade = 2f;

    // Força
    public float forcaBase = 1f;
    public float bonusForca = 1f;

    // Intervalo entre danos (segundos)
    public float damageInterval = 2f;

    // Referência ao script do player (assign no Inspector ou por GetComponent)
    public Player player;

    // Controle de tempo para dano
    private float lastDamageTime = -999f;

    // Reflexão para aplicar força se o Player expuser um campo/propriedade pública
    private FieldInfo playerForcaField = null;
    private PropertyInfo playerForcaProp = null;

    void Start()
    {
        // Se a referência ao Player estiver definida, inicializa valores base de velocidade/força
        if (player != null)
        {
            // captura velocidadeBase do player existente
            velocidadeBase = player.speed;

            // tenta localizar um campo ou propriedade pública relacionado à força
            var pType = player.GetType();
            // procurar nomes comuns (forca, force, strength)
            playerForcaField = pType.GetField("forca", BindingFlags.Public | BindingFlags.Instance)
                              ?? pType.GetField("force", BindingFlags.Public | BindingFlags.Instance)
                              ?? pType.GetField("strength", BindingFlags.Public | BindingFlags.Instance);

            if (playerForcaField != null)
            {
                object val = playerForcaField.GetValue(player);
                if (val is float f) forcaBase = f;
            }
            else
            {
                playerForcaProp = pType.GetProperty("forca", BindingFlags.Public | BindingFlags.Instance)
                                   ?? pType.GetProperty("force", BindingFlags.Public | BindingFlags.Instance)
                                   ?? pType.GetProperty("strength", BindingFlags.Public | BindingFlags.Instance);

                if (playerForcaProp != null)
                {
                    object val = playerForcaProp.GetValue(player, null);
                    if (val is float f) forcaBase = f;
                }
            }
        }
    }

    void Update()
    {
        UpdateBuffs();
        CheckAndApplyDamage();
    }

    // Método obrigatório: atualiza os buffs continuamente
    public void UpdateBuffs()
    {
        bool aguaCheia = aguaAtual >= aguaMax;
        bool comidaCheia = comidaAtual >= comidaMax;

        // Velocidade: depende APENAS da água cheia
        if (player != null)
        {
            if (aguaCheia)
            {
                player.speed = velocidadeBase + bonusVelocidade;
            }
            else
            {
                player.speed = velocidadeBase;
            }
        }

        // Força: depende APENAS da comida cheia
        if (playerForcaField != null || playerForcaProp != null)
        {
            if (comidaCheia)
            {
                SetPlayerForca(forcaBase + bonusForca);
            }
            else
            {
                SetPlayerForca(forcaBase);
            }
        }
        else
        {
            // se o Player não expuser força, mantemos forcaBase internamente (sem aplicar externamente)
            // nada a fazer além de manter as variáveis para inspector
        }
    }

    // Método obrigatório: verifica e aplica dano com intervalo
    public void CheckAndApplyDamage()
    {
        if (Time.time - lastDamageTime < damageInterval) return;

        bool semAgua = aguaAtual <= 0f;
        bool semComida = comidaAtual <= 0f;

        float dano = 0f;
        if (semAgua && semComida)
        {
            dano = 1f;
        }
        else if (semAgua || semComida)
        {
            dano = 0.5f;
        }

        if (dano > 0f)
        {
            vida -= dano;
            lastDamageTime = Time.time;
            // não adicionamos efeitos visuais/UI; apenas mantemos vida interna
        }
    }

    // Tenta setar a força no objeto Player via reflexão
    private void SetPlayerForca(float value)
    {
        if (playerForcaField != null)
        {
            playerForcaField.SetValue(player, value);
        }
        else if (playerForcaProp != null && playerForcaProp.CanWrite)
        {
            playerForcaProp.SetValue(player, value, null);
        }
    }
}
