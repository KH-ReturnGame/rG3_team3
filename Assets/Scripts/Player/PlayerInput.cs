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
                Debug.Log("평타");
            }


        }
       

    }

    void FixedUpdate()
    {
        
    }
}
