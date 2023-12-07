using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using Cinemachine;
using TMPro;

public class Controle_player : MonoBehaviour
{

    //CharacterController cc;
    private Rigidbody rb; //rigidbody
    public PlayerInput playerInput;

    public int HP_vida;
    public bool morreu;
    private bool TocandoAniMorte = false;
    public TextMeshProUGUI HPText;

    private Vector2 moveInput;
    public Vector2 InfoInputMove;

    //chao logic
    public LayerMask ThisChao;

    private bool chao;




    //Velocidade CATCHAU
    public float moveVel;

    //correr
    public bool Correndo;

    [SerializeField] private float walkAcelMax;
    [SerializeField] private float walktime;


    //pulo
    public float forcaPulo;


    //ANIMACAO
    private Animator ani; //animaçao

    private bool direcaoEsq;
    public bool walk;
    private bool agachamento;
    public bool ARMA;
    private int estadoCorrer;
    private bool correr;
    private bool rolada;
    private bool tiro;

    //capsule coleder
    private CapsuleCollider caps;

    public Transform miraDaArma; // Atribua a mira da arma no Unity Inspector.


    //ROLADAS Variaveis
    //rolar
    public float ForcaDeRoll; //força

    public bool CanRoll = true; //dar o roll
    public bool IsRoll; //roll ativo

    private float RollTime = 0.2f; //Tempo de roll
    private float RollCoolDown = 2f; // Cool Down
    //Fim ROLL

    //CAMERA ROOLLLL
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private bool haveGun;

    // : ) HELP
    //Gravidade
    //private float Gravidade = 0.5f;
    //private Vector3 Vmove;

    //variavel que permite se moventar e tirar o controle do jogador
    public bool TakeControlP;

    private bool ColidEscadas;

    [SerializeField]private bool PodeAndar;

    [SerializeField]private GameObject DeathScreen;


    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        rb = GetComponent<Rigidbody>(); //pega o rigidbody
        //cc = GetComponent<CharacterController>();

        //pega o animator do personagem
        ani = GetComponent<Animator>();

        //colisor da capsula
        caps = GetComponent<CapsuleCollider>();
        caps.height = 2.3f;

        //arrumar um bug que existe que quando o personagem ta em outra cena ele pode bugar o pulo e tiro
        tiro = true;
        chao = true;
        morreu = false;
        PodeAndar = true;

        HP_vida = 3;
        playerInput.enabled = true;

        Time.timeScale = 1f;

    }

    private void Update()
    {
        InfoInputMove = moveInput;

        ARMA = GetComponent<Arma_control>().ArmaNaMao;
        TakeControlP = GetComponent<Interagir_Iteam_player>().AnInter;
        tiro = ani.GetBool("Tiro");
        haveGun = GetComponent<Arma_control>().TakeAGun;

        //if se ele tiver tocando a animaçao de pegar animçao ele nao faz ESSAS COISAS
        if (!TakeControlP && tiro)
        {
            
            MoveP();
            
            Upanime();

        }

        if (walktime >= walkAcelMax)
        {

            // Debug.Log("RUN");
        }

        HPText.text = "Vida: " + HP_vida.ToString();
        ani.SetInteger("Vida", HP_vida);

        // Verifica se a vida é menor que zero
        if (HP_vida < 0)
        {
            // Define a vida como zero para evitar valores negativos
            HP_vida = 0;
        }
        
        if (HP_vida <= 0)
        {
            TocandoAniMorte = true;
            if(TocandoAniMorte)RecebeDano();
        }

    }


    private void FixedUpdate()
    {
        //chao
        chao = Physics.CheckSphere(transform.position, 0.4f, ThisChao);
        

        //Debug.Log(chao);
        //Debug.Log(ThisChao);

        //Giro da camera
        if (direcaoEsq == true)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, -95f, 0f);
            virtualCamera.transform.rotation = Quaternion.RotateTowards(virtualCamera.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0f, -85f, 0f);
            virtualCamera.transform.rotation = Quaternion.RotateTowards(virtualCamera.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }

    }

    //MORETE
    public void RecebeDano()
    {
        // Verifica se a vida é menor que zero
        if (HP_vida < 0)
        {
            // Define a vida como zero para evitar valores negativos
            HP_vida = 0;
        }

        //HP_vida -= 1;
        Debug.Log(HP_vida);
        if (HP_vida <= 0)
        {
            Debug.Log("VAi morrendo");
            StartCoroutine(TimeToDie());
        }
    }

    private IEnumerator TimeToDie()
    {
      
        playerInput.enabled = false;
        morreu = true;

        Debug.Log("Morreu");

        yield return new WaitForSeconds(0.1f);
        morreu = false;


        yield return new WaitForSeconds(2f);
        if (HP_vida == 0)
        {
            TGmeOver_SC controlDead = DeathScreen.GetComponent<TGmeOver_SC>();
            controlDead.DeathScreem();
        }
        TocandoAniMorte = false;
        yield return new WaitForSeconds(2f);
        //Time.timeScale = 0f; // Pausa completamente o tempo
        
    }

   

    //pode apagar no futuro só serve para saber o ponto de origem do personagem
    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(transform.position, 0.4f);
    }


    //Atalizao update ANIMAÇAO
    private void Upanime()
    {
        ani.SetBool("IsWalk", walk);
        ani.SetBool("Chao", chao);
        ani.SetBool("Agachado", agachamento);
        ani.SetBool("Correr", correr);
        ani.SetInteger("SwitchCorrer", estadoCorrer);

        //ani.SetBool("ArmaMao", ARMA);
        ani.SetBool("Rolar", rolada);

        ani.SetBool("Morto", morreu);
    }


    // Mantenha a velocidade base em uma variável separada
    private float baseMoveVel = 3f;

    private void MoveP()
    {
        if (moveInput.x > 0 && direcaoEsq == true)
        {
            Flip();
        }
        else if (moveInput.x < 0 && direcaoEsq == false)
        {
            Flip();
        }

        // Ativar animação
        walk = moveInput.x != 0 ? true : false;
        //Se o imput X for difernete de = 0 entao, walk é true se nao é false

        if (PodeAndar)
        {
            // Física de movimentação 
            rb.velocity = new Vector3(0f, rb.velocity.y, moveInput.x * moveVel);
            //variavel total =        informaçao do X * a velociade de movimento
        }

        if (walk && !correr && !ARMA)
        {
            walktime += Time.deltaTime;
        }
        else
        {
            walktime = 0;
        }

        // Verifique se o jogador caminhou por muito tempo
        if (walktime >= walkAcelMax)
        {
            moveVel = 8;
            estadoCorrer = 2;
        }
        else
        {
            estadoCorrer = 0;
            moveVel = baseMoveVel;
        }

        // Finalmente VOCE PODE CORRER SAFE AMEM
        if (Correndo == true && walk == true)
        {
            moveVel = 10;
            correr = true;
        }
        else
        {
            correr = false;
        }
    }



    //Vira a direçao do persoanagem 
    private void Flip()
    {
        direcaoEsq = !direcaoEsq; // se ele for verade entao ele vira falso

        // Restaure a rotação do jogador para a direção correta
        transform.rotation = Quaternion.Euler(0f, direcaoEsq ? 180f : 0f, 0f);

        
        if (direcaoEsq == true)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        
    }


    void JumpP(bool isPressed)
    {
        if (!TakeControlP)
        {
            if (isPressed == true && chao == true && ARMA == false)
            {
                rb.AddForce(Vector3.up * forcaPulo, ForceMode.Impulse);
                Debug.Log("Chao");
            }
            if (!chao)
            {
                PodeAndar = false;
            }
        }
    }


    public void Agacha()
    {
        //Agachado
        if (agachamento == false)
        {

            caps.height = 1.33f;
            caps.center = new Vector3(0f, 0.7f, 0f);

            if (haveGun)
            {
                // Adicione uma rotação à mira da arma usando Quaternion.Euler.
                miraDaArma.localRotation = Quaternion.Euler(80f, 90f, 90f);
            }

            agachamento = true;
            PodeAndar = false;
        }

        //Levantado
        else if (agachamento == true)
        {

            caps.height = 2.3f;
            caps.center = new Vector3(0f, 1.27f, 0f);
            if (haveGun)
            {
                miraDaArma.localRotation = Quaternion.Euler(86f, 66f, 66f);
            }
            agachamento = false;
            PodeAndar = true;
        }

    }


    //ROLAR
    private IEnumerator Roll()
    {
        CanRoll = false;
        rolada = true;
        IsRoll = true;
        agachamento = true;

        // Configure a força de rolamento
        Vector3 rollDirection = transform.forward * 2;

        // Zere a velocidade atual do Rigidbody antes de aplicar o impulso
        rb.velocity = Vector3.zero;

        // Aplica a força instantaneamente como um impulso
        rb.AddForce(rollDirection, ForceMode.Impulse);

        yield return new WaitForSeconds(RollTime);
        rolada = false;
        IsRoll = false;
        ForcaDeRoll = 1f;

        yield return new WaitForSeconds(RollCoolDown);
        CanRoll = true;
        
    }


    public void OnRoll(InputAction.CallbackContext button)
    {

        if (button.started) if (CanRoll == true) StartCoroutine(Roll());
        //if (button.performed) rolada = false;
        //if (button.canceled) rolada = false;
    }
    //FIM DA ROLADA




    //Input Control .0.
    public void OnMove(InputAction.CallbackContext value)
    {
      
        moveInput = value.ReadValue<Vector2>();


        //Debug.Log("Caminhando");
        //ani.SetBool("Caminhando",true);
    }

    public void OnCorrer(InputAction.CallbackContext button)
    {
        if (button.started)
        {
            Correndo = true;

        }
        if (button.canceled)
        {
            Correndo = false;
        }
    }



    public void OnJump(InputAction.CallbackContext value)
    {
        if (value.started) JumpP(true);

        if (value.canceled) JumpP(false);



        //Debug.Log("TO PULANDO");
    }

    public void OnAgachar(InputAction.CallbackContext value)
    {
        if(!ColidEscadas)
        Agacha();

        //Debug.Log("agachar");
    }

    public void OnTriggerStay(Collider collision)
    {        
            if (collision.gameObject.CompareTag("UpStair") || collision.gameObject.CompareTag("DownStair") || collision.gameObject.CompareTag("UpDownStair"))
            {
            ColidEscadas = true;
            }
        else
        {
            ColidEscadas = false;
        }
    }
    

}   

    /*
     * 
     * 
     * 
     * //Gravidade
        if (cc.isGrounded)
        {
            Vmove.y = 0f;
        }
        else
        {
            Vmove.y -= Gravidade * Time.deltaTime;
        }
            //Fim da gravidade


            //KeyCode
              private float Direcao;
           * 
           * 
           * if(!pulo && chao){
          Move();


          if (Input.GetButtonDown("Jump"))
          {
              Jump();
          }
          //checando qual direcao deve ir
          // Direcao = Input.GetAxis("Horizontal");
          Direcao = 1f;

          Vmove = new Vector3(Direcao * MoveVel, Vmove.y, 0);
          cc.Move(Vmove);

           */
