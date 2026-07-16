using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerManager plrManager;
    private PlayerInput plrInput;

    private Rigidbody2D rb2d;
    private SpriteRenderer sprdr;
    private Animator animtr;

    // 활성 변수
    public bool isDebounce = false;

    // Attack - 평타 변수 
    public int currentComboStack = 0;
    private float lastComboChangeTime;
    private float comboStackResetLimit = 1f;
    private int maxComboStack = 4;

    [Header("전투 물리 설정")]
    public float forwardThrustForce = 55f;
    public LayerMask enemyLayer; 

    void Awake()
    {
        plrInput = GetComponent<PlayerInput>();
        plrManager = GetComponent<PlayerManager>();

        rb2d = GetComponent<Rigidbody2D>();
        sprdr = GetComponent<SpriteRenderer>();
        animtr = GetComponent<Animator>();
    }

    void Update()
    {
        if (currentComboStack > 0 && Time.time - lastComboChangeTime >= comboStackResetLimit)
        {
            currentComboStack = 0;
            Debug.Log("시간 초과 (스택 변화 없음): 콤보 스택이 0으로 초기화되었습니다.");
        }
    }


    public void RequestAttack()
    {
        if (plrManager.Stunlist.Count > 0) return;
        if (!plrManager.Cooldownlist.Contains("Attack"))
        {
            
            WeaponData weapon = WeaponManager.Instance.GetWeaponData(plrManager.currentWeapon);
            if (weapon == null) return;

            
            currentComboStack++;
            lastComboChangeTime = Time.time;
            comboStackResetLimit = weapon.attackCooldown * 1.4f;
            Debug.Log("Combo : " + currentComboStack);

            Utility.DataManagement.ListManagement.AddData("Attack", plrManager.Cooldownlist, weapon.attackCooldown);

            
            if (!plrManager.isGrounded && plrInput.yInput > 0)
            {
                Debug.Log("Aerial Cleaves: 체공");
                
                rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0f);
                ExecuteHitbox(1.5f, 10f, 0f); 
            }
            
            else if (plrManager.isGrounded)
            {
                if (weapon.hasForwardMove && plrInput.yInput != 1)
                {
                    Debug.Log("Frontdash: 지상 전진 평타");
                    float facingDirection = sprdr.flipX ? -1f : 1f;
                    rb2d.AddForce(new Vector2(facingDirection * forwardThrustForce, 0f));
                }
                ExecuteHitbox(1.5f, 10f, 0f);
            }
          
            if (currentComboStack >= maxComboStack)
            {
                currentComboStack = 0;
                Debug.Log("초과로 인해 0으로 초기화 진행.");
                Utility.DataManagement.ListManagement.AddData("Attack", plrManager.Cooldownlist, weapon.attackCooldown * 2.3f);
            }
        }
    }

   
    public void RequestStrongAttack()
    {
        if (plrManager.Stunlist.Count > 0) return;
        if (!plrManager.Cooldownlist.Contains("StrongAttack"))
        {
           
            Utility.DataManagement.ListManagement.AddData("StrongAttack", plrManager.Cooldownlist, 0.8f);

            
            if (plrManager.isGrounded && plrInput.yInput > 0)
            {
                Debug.Log("High Time: 공중으로 띄우기");
                rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, plrManager.JumpPower);
                ExecuteHitbox(2f, 20f, 15f); // Y 넉백(15f)으로 적을 띄움
            }
            else if (!plrManager.isGrounded && plrInput.yInput < 0)
            {
                Debug.Log("Helm Breaker: 급강하 내려찍기");
                rb2d.linearVelocity = new Vector2(0f, -25f); // 플레이어 급강하
                ExecuteHitbox(2f, 30f, -20f); 
            }
        }
    }


    private void ExecuteHitbox(float radius, float damage, float yKnockback)
    {
        float facingDirection = sprdr.flipX ? -1f : 1f;
        Vector2 hitboxPos = new Vector2(transform.position.x + (facingDirection * 1f), transform.position.y);

        Collider2D[] hitEnemies = Utility.DataManagement.CombatManagement.CreateHitbox(hitboxPos, radius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // TODO: 상대방 스크립트로 damage와 yKnockback(띄우기/내려찍기용) 전달
            Debug.Log(enemy.name + " 적중!");
        }
    }

    void LateUpdate()
    {

    }
}