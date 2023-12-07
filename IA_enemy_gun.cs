using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.VFX;

public class IA_enemy_gun : MonoBehaviour
{
    [SerializeField]private Transform player;
    [SerializeField] private Rigidbody rb;

    public bool danoRecebido;

    public float YD;
    
    public float minDistance = 5f;
    public float maxDistance = 10f;

    public float tooCloseDistance = 2f;

    public float moveSpeed = 3f;

    public float rotationSpeed = 90.0f; // Velocidade de rotação em graus por segundo
    [SerializeField] private bool PodeAtirar = true;

    [SerializeField] private bool jogadorDetectado = false;
    [SerializeField] public bool emCombate = false;
    [SerializeField] private static bool algumInimigoEmCombate = false;

    [SerializeField] private Animator anim;


    //--- TIROS DA ARMA ---//
    private int tirosDados = 0;
    private float[] probabilidades = { 0.1f, 0.3f, 1.0f };
    //Visual Efeitos
    public VisualEffect VFXEffect; // Arraste o efeito VFX Graph aqui
    // ---

    public Transform coldreEnemy;
    public Transform armaEneemy;
    public Transform MaoEnemy;

    private bool MorreuEn;

    public Transform MiraLazer;

    public TrailRenderer BulletTrail; // Prefab do rastro da bala
    private TrailRenderer currentTrail; // Referência ao rastro atual da bala

    //ESTATUS DO INIMIGO 
    [SerializeField]private EnemyState currentState = EnemyState.Patrol;

    private RaycastHit hit;

    //Walk Patrol
    private Vector3 ultimaPosicao; // Variável para armazenar a última posição do inimigo
    public float avoidanceRadius = 2.0f;
    public float avoidanceForce = 0.5f;
    public LayerMask enemyLayer;

    public enum EnemyState
    {
        Patrol,
        Combat,
        Waiting
    }


    private void ExitCombatState()
    {
        currentState = EnemyState.Waiting; // ou EnemyState.Patrol, dependendo da lógica
    }

    private void WaitingState()
    {
        // Adicione lógica de espera aqui
        // Por exemplo, aguarde alguns segundos antes de voltar à patrulha
        StartCoroutine(WaitAndReturnToPatrol());
    }

    private IEnumerator WaitAndReturnToPatrol()
    {
        yield return new WaitForSeconds(3f); // Aguarda 3 segundos
        currentState = EnemyState.Patrol; // Volta ao estado de patrulha
        jogadorDetectado = false;
        emCombate = false;
    }
    //FIM DOS ESTATUS

      
    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();

        PodeAtirar = true;
        VFXEffect.Stop(); // Certifique-se de que o efeito esteja inicialmente parado


        ultimaPosicao = transform.position; // Define a posição inicial como a última posição
        
    }

    private void FixedUpdate()
    {

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Combat:
                FollowPlayerCombat();
                CombatMode();
                if (!jogadorDetectado) // Condição para voltar ao estado de espera ou patrulha
                {
                    ExitCombatState();
                }
                break;

            case EnemyState.Waiting:
                WaitingState();
                break;
            default:
                break;
        }


    }

    private void Update()
    {
        // Encontrar o jogador usando a tag "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        Controle_player PlayerComponent = playerObject.GetComponent<Controle_player>();

        if (PlayerComponent.HP_vida > 0)
        {
            VisonEny(); // Certifique-se de chamar VisonEny() para verificar a visão do jogador.


            anim.SetBool("Morreu", MorreuEn);

            if (jogadorDetectado) emCombate = true;
        }
        else if(PlayerComponent.HP_vida == 0)
        {
            emCombate = false;
            currentState = EnemyState.Patrol;
        }


        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, avoidanceRadius, enemyLayer);

        foreach (Collider enemy in nearbyEnemies)
        {
            if (enemy.gameObject != gameObject) // Evita comparar o inimigo consigo mesmo
            {
                Vector3 avoidVector = transform.position - enemy.transform.position;
                transform.position += avoidVector.normalized * avoidanceForce * Time.deltaTime;
            }
        }

    }


    //MORENDO DESGRAÇA
    public void ReceberDano()
    {
       
        danoRecebido = true;
        emCombate = false;

        StartCoroutine(MorrendoAnim());
    }

    private IEnumerator MorrendoAnim()
    {

        Debug.Log("FOI MOREENDO AAA");
        MorreuEn = true;
        yield return new WaitForSeconds(0.1f);
        MorreuEn = false;

        algumInimigoEmCombate = false;
        danoRecebido = true;
        emCombate = false;

        yield return new WaitForSeconds(2f);

        DestroyEnemy();
    }


    private void DestroyEnemy()
    {
        // Adicione aqui qualquer lógica adicional que você precise antes de destruir o inimigo.

        // Libere a variável algumInimigoEmCombate quando o inimigo é destruído.
        algumInimigoEmCombate = false;
        danoRecebido = true;

        // Destrua o inimigo.
        Destroy(gameObject);
    }

    //FIM DA MORTE


    //
    private void FollowPlayerCombat()
    {

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > maxDistance)
        {
            // Mova o inimigo em direção ao jogador ao longo do eixo Z.
            Vector3 moveDirection = new Vector3(0, 0, directionToPlayer.normalized.z);
            rb.velocity = moveDirection * moveSpeed;

            // Rotação para enfrentar o jogador
            RotateTowardsPlayer(directionToPlayer);

            if (jogadorDetectado)
            {
                anim.SetBool("walkFront", true);
            }
            else
            {
                anim.SetBool("walkFront", false);
            }
        }
        else if (distanceToPlayer < minDistance)
        {
            // Mova o inimigo para longe do jogador ao longo do eixo Z.
            Vector3 moveDirection = new Vector3(0, 0, -directionToPlayer.normalized.z);
            rb.velocity = moveDirection * moveSpeed;

            // Rotação para enfrentar o jogador
            RotateTowardsPlayer(directionToPlayer);

            if (jogadorDetectado)
            {
                anim.SetBool("walkBack", true);
            }
            else
            {
                anim.SetBool("walkBack", false);
            }
        }
        else
        {
            // O jogador está dentro do intervalo desejado, pare de se mover.
            rb.velocity = Vector3.zero;
            anim.SetBool("walkBack", false);
            anim.SetBool("walkFront", false);
        }

        if (distanceToPlayer < tooCloseDistance)
        {
            // O jogador está muito perto, ajuste a rotação sem restrições no eixo Y
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            float step = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

        }
    }

   
    void RotateTowardsPlayer(Vector3 directionToPlayer)
    { 
        // Calcula a direção da rotação apenas no eixo Y
        Vector3 targetDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);

        // Calcula a rotação necessária para enfrentar o jogador
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // Mantém a rotação apenas no eixo Y
        transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
    }


    private void walkAnim()
    {

        // Calcula a diferença na posição atual e na última posição
        Vector3 diferencaPosicao = transform.position - ultimaPosicao;

        // Verifica se houve movimento desde a última atualização
        if (diferencaPosicao.magnitude > 0.001f)
        {
            
            anim.SetBool("walkE", true);
        }
        else
        {
            anim.SetBool("walkE", false);
        }
    }


    
    private void Patrol()
    {

        armaEneemy.transform.position = coldreEnemy.position;
        armaEneemy.transform.rotation = coldreEnemy.rotation;

        // Implemente a lógica de patrulha aqui.
        // Certifique-se de atualizar a posição do inimigo de acordo com a lógica de patrulha.
        // Armazena a posição atual antes de mover o inimigo

        // Movimento para frente (usando a direção forward do transform do inimigo)
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        walkAnim();
    }
  


    // ------------  Visao e COmabete -------------
    private void VisonEny()
    {
        Vector3 rayStartPos = transform.position + new Vector3(0, YD, 0);

        if (Physics.Raycast(rayStartPos, transform.forward, out hit, maxDistance))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            if (hit.collider.tag == "Player")
            {
                //Debug.Log("thcauI");
                jogadorDetectado = true;
                EnterCombatState(); // Chama a transição para o estado de combate
            }
        }
    }

    private void EnterCombatState()
    {
        currentState = EnemyState.Combat;


        if (jogadorDetectado && !emCombate && !algumInimigoEmCombate)
        {
            EnterCombat();
        }
    }


    //UM por vez
    public bool IsPlayerDetected()
    {
        return jogadorDetectado;
    }

    public bool IsInCombat()
    {
        return emCombate;
    }

    private void EnterCombat()
    {
        emCombate = true;
        algumInimigoEmCombate = true;

        // Adicione lógica adicional para notificar outros inimigos ou realizar ações ao entrar em combate

        // Exemplo: StartCoroutine(WaitAndExitCombat()); // Para sair do combate após alguns segundos
    }
    //FIm

    //Modo de comabete
    private void CombatMode()
    {

        armaEneemy.transform.position = MaoEnemy.position;
        armaEneemy.transform.rotation = MaoEnemy.rotation;

        anim.SetBool("ECombate", true);

        // Implemente a lógica de combate aqui.
        // Você pode usar a função FollowPlayerCombat() como base.
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("gunHand") || anim.GetCurrentAnimatorStateInfo(0).IsName("shott"))
        {
            StartCoroutine(armaConter());
        }
    }

    private IEnumerator armaConter()
    {

        if (emCombate)
        {
            yield return new WaitForSeconds(3f); // Espera antes do primeiro tiro

            if (PodeAtirar)
            {
                Debug.Log("Começo");
                // Executa o ataque
                AtirarNoPlayer();

                VFXEffect.Play(); // Inicie o efeito
                anim.SetBool("shott", true);

                // Bloqueia os tiros por um período de tempo
                PodeAtirar = false;

                // Aguarda o término da animação de tiro
                yield return new WaitForSeconds(0.1f);

                anim.SetBool("shott", false);

                // Aguarda o tempo de cooldown após o término da animação
                yield return new WaitForSeconds(10f);

                // Permite atirar novamente após o tempo de cooldown
                PodeAtirar = true;
            }
        }
    }

    public void AtirarNoPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        Controle_player playerHP = playerObject.GetComponent<Controle_player>();

        if (tirosDados < probabilidades.Length)
        {
            float chanceDeMorte = probabilidades[tirosDados];
            float resultado = UnityEngine.Random.value; // Gere um número aleatório entre 0 e 1.

            if (resultado <= chanceDeMorte)
            {
                // O player morreu.
                Debug.Log("Fim1");
                //Debug.Log(playerHP.HP_vida);
                playerHP.HP_vida -= 1;
                //playerHP.RecebeDano();
                tirosDados = 0;
                CreateBulletTrail(hit);
            }
            else
            {
                CreateBulletTrail();
                Debug.Log("ERROU DESGRAÇA");
                // sobreviveu. Você pode adicionar efeitos visuais ou comportamentos extras aqui.
            }

            tirosDados++;
        }


        VFXEffect.Stop(); // Certifique-se de que o efeito esteja inicialmente parado
        anim.SetBool("shott", false);
    }


    void CreateBulletTrail(RaycastHit hit = new RaycastHit())
    {
        currentTrail = Instantiate(BulletTrail, MiraLazer.position, Quaternion.identity);
        currentTrail.transform.position = MiraLazer.position;

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            StartCoroutine(SpawnTrailToTarget(currentTrail, hit.point));
        }
        else
        {
            StartCoroutine(SpawnTrail(currentTrail));
        }
    }

    private IEnumerator SpawnTrailToTarget(TrailRenderer trail, Vector3 endPoint)
    {
        Vector3 startPoint = MiraLazer.position;

        float elapsedTime = 0f;
        float trailDuration = 0.1f; // Duração do rastro

        while (elapsedTime < trailDuration)
        {
            trail.transform.position = Vector3.Lerp(startPoint, endPoint, elapsedTime / trailDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(trail.gameObject);
    }

    private IEnumerator SpawnTrail(TrailRenderer trail)
    {
        Vector3 startPoint = MiraLazer.position;
        Vector3 endPoint = MiraLazer.position + MiraLazer.forward * 12f; // Define um ponto além do alcance para continuar reto

        float elapsedTime = 0f;
        float trailDuration = 0.1f; // Duração do rastro

        bool trailDestroyed = false;

        while (elapsedTime < trailDuration && !trailDestroyed)
        {
            try
            {
                trail.transform.position = Vector3.Lerp(startPoint, endPoint, elapsedTime / trailDuration);
                elapsedTime += Time.deltaTime;
               
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exceção durante a exibição do rastro de bala: " + e.Message);
                trailDestroyed = true;
            }
            yield return null;
        }

        if (!trailDestroyed)
        {
            Destroy(trail.gameObject); // Destroi o rastro de bala se não houve exceção
        }
        else
        {
            Destroy(trail.gameObject); // Destroi o rastro de bala mesmo em caso de exceção
        }
    }

}

