using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DestroyZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Appearance")]
    public Color activeColor = Color.red;
    public Color cooldownColor = Color.gray;
    [SerializeField] private AudioClip numSound;
    [SerializeField] private float volume = 1f;
    [SerializeField] private Animator animator;
    private bool anim;
    private bool some = true;
    
    private Image zoneImage;
    private bool isPointerOver;
    private ConveyorBeltSystem conveyorSystem;

    void Start()
    {
        zoneImage = GetComponent<Image>();
        conveyorSystem = FindObjectOfType<ConveyorBeltSystem>();
        UpdateAppearance();
    }

    void Update()
    {
        UpdateAppearance();
        
    }

    private void UpdateAppearance()
    {
        if (conveyorSystem != null)
        {
            //animator.SetBool("Close", true);
            anim = conveyorSystem.CanDestroyCard() ? false : true;
            if (anim && some)
            {
                SoundFXManager.Instance.PlaySoundFXClip(numSound, transform, volume);
                some = false;
            }
            else if (!anim && !some)
            {
                some = true;
                
            }
            animator.SetBool("Close", anim);
        }
        //else
        //{
        //    animator.SetBool("Close", false);
        //}
    }

    public bool IsPointInside(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            screenPoint
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }
}