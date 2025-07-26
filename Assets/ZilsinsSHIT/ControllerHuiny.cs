using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerHuiny : MonoBehaviour
{
    public Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 5f;
   

    float horizontalMovement;
    float verticalMovement;
    void Start ()
    {

    }

    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, verticalMovement * moveSpeed);   
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        verticalMovement = context.ReadValue<Vector2>().y;
    }


}
