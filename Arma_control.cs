using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;


public class Arma_control : MonoBehaviour
{
    [Header("Animaçao")]
    //ANIMACAO
    private Animator ani; //animaçao

    public bool ArmaNaMao;

    public bool TakeAGun;

    //NAO ESQUECER DE COLOCAR ETIQUETA
    public Transform mao;
    public Transform coldre;

    public Transform arma;


    //Interagir com a arma / PEGAR
    public bool Inter;


    //COLISOR NA ARMA
    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Arma") && Inter)
        {
            //quando colidir ele dessativa o box collider
            collision.gameObject.GetComponent<BoxCollider>().enabled = false;
            
            //fixa na possicao
            collision.transform.position = coldre.position;
            collision.transform.rotation = coldre.rotation;

            //move o objeto para o pararentesco
            collision.transform.SetParent(coldre);

            //Ta com a arma no inventario
            TakeAGun = true;

            ArmaNaMao = false;

            BoxCollider boxCollider = collision.gameObject.GetComponent<BoxCollider>();
            Destroy(boxCollider);
            //BoxCol = GetComponent<BoxCollider>();

        }

    }

    
    // Start is called before the first frame update
    void Start()
    {
      ani = GetComponent<Animator>();

        ArmaNaMao = false;

        if (TakeAGun == true)
        {
            arma.GetComponent<BoxCollider>().enabled = false;

            //fixa na possicao
            arma.transform.position = coldre.position;
            arma.transform.rotation = coldre.rotation;

            //move o objeto para o pararentesco
            arma.transform.SetParent(coldre);

            //Ta com a arma no inventario
            TakeAGun = true;

            ArmaNaMao = false;

        }
    }

    // Update is called once per frame
    void Update()
    {
        //Caso ele nao tenha arma no inventario ele fica rodando esse linha de codigo até ter a arma
        //TakeAGun = GetComponent<Interagir_Iteam_player>().TakeAGun;
        if(TakeAGun == false) Inter = GetComponent<Interagir_Iteam_player>().AnInter;

        Upanime();

        if (ani.GetCurrentAnimatorStateInfo(0).IsName("Pegar arma") || ani.GetCurrentAnimatorStateInfo(0).IsName("Guardando arma"))
        {
            StartCoroutine(PlayAnim());

            //Debug.Log("Inter ");
        }

    }

    private void Upanime()
    {
        ani.SetBool("ArmaMao", ArmaNaMao);
    }

    private IEnumerator PlayAnim()
    {
        yield return new WaitForSeconds(3.0f);


    }

    public void PegaArma()
    {
        //Coloca arma na mao
        if (ArmaNaMao == false && TakeAGun == true)
        {

            //fixa na possicao
            arma.position = mao.position;
            arma.rotation = mao.rotation;


            //move o objeto para o pararentesco
            arma.SetParent(mao);

            //Inversao da animaçao
            //ani.SetFloat("PegaArma", 2.5f);

            ArmaNaMao = true;
            //Debug.Log("Arma na mao");
        } 
        //Tira arma Da mao
        else if (ArmaNaMao == true && TakeAGun == true){


           
            //fixa na possicao
            arma.position = coldre.position;
            arma.rotation = coldre.rotation;


            //move o objeto para o pararentesco
            arma.SetParent(coldre);

            //Inversao da animaçao
            //ani.SetFloat("PegaArma", -1.5f);

            ArmaNaMao = false;
            //Debug.Log("gardar arma");
        }
    }


    public void HaveGun(InputAction.CallbackContext value)
    {
        PegaArma();

        //Debug.Log("PEGANDO NA PIXTOLA");
    }

}
