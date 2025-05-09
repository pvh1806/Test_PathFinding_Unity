using UnityEngine;

namespace _0_Project.Scripts.MapControl
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        void OnValidate()
        {
            transform.localScale = Vector3.one;
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer .color = Color.white;
        }
    }
}