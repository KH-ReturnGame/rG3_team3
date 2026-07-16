using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // 플레이어의 입력을 다루는 스크립트.

    private PlayerManager plrManager; // 귀찮아서 여기서 StunList, 쿨타임List, 좌우 이동 움직임을 총괄할 예정. 
    private PlayerCombat plrCombat;

    // movementVariables
    public float xInput, yInput;

    void Awake()
    {
        plrManager = GetComponent<PlayerManager>();
        plrCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        // 좌우 입력값 / 상하 입력값 (데메크 위로 올리기 기능)

        if (plrManager.CanMove == true) // CanMove는 Combat스크립트에서 isStun,isMovementDisabled, 등등을 포괄하여 움직임을 뜻하는 변수.
            // 이게 근데 의미가 있나?
        {
            // 점프
            if (Input.GetButtonDown("Jump"))
            {
                plrManager.Jump();
            }
            // 평타, ||는 OR이란 뜻.
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.L))
            {
                plrCombat.RequestAttack();// yInput에 따라 달라짐 -> Aerial Cleaves
            }
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.R))
            {
                plrCombat.RequestStrongAttack(); // yInput에 따라 달라짐 -> HighTime / Downslam
            }

            /*
            if (Input.GetKeyDown(KeyCode.Alpha1)) plrCombat.Skill1(); // 스킬 슬롯마다 체크
            if (Input.GetKeyDown(KeyCode.Alpha2)) plrCombat.Skill2();
            if (Input.GetKeyDown(KeyCode.Alpha3)) plrCombat.Skill3();
            if (Input.GetKeyDown(KeyCode.Alpha4)) plrCombat.Skill4();

            if (Input.GetKeyDown(KeyCode.C)) plrCombat.Dash();
            if (Input.GetKeyDown(KeyCode.F)) plrCombat.Block();

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.R)) plrCombat.Parry();
            */

        }
       

    }

    void FixedUpdate()
    {
        
    }
}
