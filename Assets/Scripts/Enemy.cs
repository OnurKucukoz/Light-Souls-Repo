using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : MonoBehaviour, IHasHealth
{
    public int currentHealth;
    int IHasHealth.currentHealth
    {
        get { return currentHealth; }
    }

    private int maxHealth = 200;
    Enemy enemy;
    Animator enemyAnimator;
    EnemyWeapon enemyWeapon;
    Rigidbody enemyRigidbody;
    VictoryPanel victoryPanel;

    bool isLightAttacking;
    bool isHeavyAttacking;
    float rotationSpeed;
    float distanceToPlayer;
    public GameObject player;
    public float moveSpeed = 1f;
    public float followRange = 15000f;

    public AudioSource enemyWeaponAudioSource;
    public AudioClip enemyDyingSound;
    
    public float attackRange = 1.5f; 
    public float minAttackDistance = 1.2f;

    public float attackCooldown = 5f;
    public bool canAttack = true;
   
    public void TurnOnWeaponCollider()
    {
        enemyWeapon.GetComponent<BoxCollider>().enabled = true;
    }

    public void TurnOffWeaponCollider()
    {
        enemyWeapon.GetComponent<BoxCollider>().enabled = false;
    }

    public void TurnOnLightDamage()
    {
        enemyWeapon.isLightDamage = true;
    }

    public void TurnOffLightDamage()
    {
        enemyWeapon.isLightDamage = false;
    }
    public void TurnOnHeavyDamage()
    {
        enemyWeapon.isHeavyDamage = true;
    }

    public void TurnOffHeavyDamage()
    {
        enemyWeapon.isHeavyDamage = false;
    }
    public void TurnOnComboDamage()
    {
        enemyWeapon.isComboDamage = true;
    }

    public void TurnOffComboDamage()
    {
        enemyWeapon.isComboDamage = false;
    }


    void Start()
    {
        enemy = GameObject.Find("newBosss").GetComponent<Enemy>();
        enemyAnimator = GameObject.Find("newBosss").GetComponent<Animator>();
        enemyWeapon = GameObject.Find("EnemyWeaponCol").GetComponent<EnemyWeapon>();
        enemyRigidbody = GameObject.Find("newBosss").GetComponent<Rigidbody>();
        victoryPanel = GameObject.Find("Victory Panel").GetComponent<VictoryPanel>();
        
        currentHealth = maxHealth;
        victoryPanel.victoryPanel.SetActive(false);
        victoryPanel.victoryAchievedText.SetActive(false);
        TurnOffWeaponCollider();
        isDodging = false;
    }
    
    private void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
        distanceToPlayer = Vector3.Distance(player.transform.position, gameObject.transform.position);
        Vector3 directionToTarget = player.transform.position - transform.position;
        directionToTarget.y = 0;
        rotationSpeed = 5f;

        if (directionToTarget.magnitude > 1f) // Avoid rotating if too close
        {
            Quaternion toRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        if (distanceToPlayer <= followRange)
        {
            
            if (distanceToPlayer > attackRange && !isDodging )
            {
                FollowPlayer();
            }
            
            else if (distanceToPlayer <= attackRange && canAttack && !isDodging)
            {
                LookAtPlayer();
                AttackPlayer();
                StopMoving();
            }
           if(distanceToPlayer <= 2.4f)
            {
                StopMoving();
            }           
        }
        else
        {
            enemyAnimator.SetBool("isEnemyMidRunning", false);
        }

        
    }

    private void StopMoving()
    {     
        enemyAnimator.SetBool("isEnemyMidRunning", false);
    }

    public void Die()
    {
        moveSpeed = 0f;
        enemy.enabled = false;

        enemyAnimator.SetBool("isEnemyDead",true);
        enemyWeaponAudioSource.PlayOneShot(enemyDyingSound);
        StartCoroutine(ShowVictoryPanelAfterDelay());
        StartCoroutine(GoToCreditsScene());
    }

    IEnumerator GoToCreditsScene()
    {
        yield return new WaitForSeconds(9.5f);
        SceneManager.LoadScene("CreditsScene");
    }

    IEnumerator ShowVictoryPanelAfterDelay()
    {       
        yield return new WaitForSecondsRealtime(4f);

        victoryPanel.ShowVictoryPanel();
        
    }

    public void EndingEnemyLife()
    {
       enemyAnimator.enabled = false;
       enemyRigidbody.constraints = RigidbodyConstraints.FreezeAll;

    }
    void FollowPlayer()
    {

        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));


        transform.position += direction * moveSpeed * Time.deltaTime;


        enemyAnimator.SetBool("isEnemyMidRunning", true);
    }



    public float attackMoveDistance = 0.5f;
    bool isDodging;
    

    public void AttackPlayer()
    {
        
        int randomAttack = UnityEngine.Random.Range(0, 3);
    
        switch (randomAttack )
        {
            case 0:
               StartCoroutine(LightAttack());
                break;
            case 1:
                StartCoroutine(HeavyAttack());
                break;

            case 2:
                StartCoroutine(ComboAttack());
                break;

        }      
        canAttack = false;
    }

  
    IEnumerator ComboAttack()
    {
        isDodging = true;
        enemyAnimator.SetTrigger("isEnemyCombo");
        yield return new WaitForSecondsRealtime(6.24f);
        canAttack = true;
        RandomDodgeMoves();
        isDodging = false;
    }

    IEnumerator LightAttack()
    {
        isDodging = true;
        enemyAnimator.SetTrigger("isEnemyLightAttacking");
        yield return new WaitForSecondsRealtime(3.28f);
        canAttack = true;
        RandomDodgeMoves(); 
        isDodging = false;
        
    }
    IEnumerator HeavyAttack()
    {
        isDodging = true;
        enemyAnimator.SetTrigger("isEnemyHeavyAttacking");
        yield return new WaitForSecondsRealtime(4.20f);
        canAttack = true;
        RandomDodgeMoves();
        isDodging = false;
    }

    void RandomDodgeMoves()
    {
        int randomDodgeMoves = UnityEngine.Random.Range(0, 5);

        switch (randomDodgeMoves)
        {
            case 0:
                StartCoroutine(WalkBackward());
                break;
            case 1:
                StartCoroutine(DodgeRight());
                break;
            case 2:
                StartCoroutine(DodgeLeft());
                break;
            case 3:
                StartCoroutine(DodgeBackward());
                
                break;
        }
    }


    private void JumpAttack()
    {
        enemyAnimator.SetTrigger("isEnemyJumpAttacking");
    }

    IEnumerator DodgeBackward()
    {
        isDodging = true;

        enemyAnimator.SetTrigger("isEnemyDodgingBackward");
        moveSpeed = 0;
        yield return new WaitForSecondsRealtime(3f);
        moveSpeed = 2.3f;
        isDodging = false;



    }
 
    void LookAtPlayer()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    IEnumerator DodgeRight()
    {
        isDodging = true;

        moveSpeed = 0;
        enemyAnimator.SetTrigger("isEnemyDodgingRight");
        Vector3 directionToPlayer = player.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); 


        yield return new WaitForSecondsRealtime(2f);
        moveSpeed = 2.3f;
        isDodging = false;
    }

    IEnumerator DodgeLeft()
    {
        isDodging = true;

        moveSpeed = 0;
        enemyAnimator.SetTrigger("isEnemyDodgingLeft");
        yield return new WaitForSecondsRealtime(2f);
        moveSpeed = 2.3f;
        isDodging = false;
    }

    IEnumerator WalkBackward()
    {
        isDodging = true;

        enemyAnimator.SetBool("isEnemyWalkingBack",true);
        
        yield return new WaitForSecondsRealtime(1.20f);
        enemyAnimator.SetBool("isEnemyWalkingBack", false);
       
        isDodging = false;

    }
}
