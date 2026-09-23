using System.Collections;
using _KIT;
using TMPro;
using UnityEngine;

namespace _KIT
{
    public class KitTooltip : MonoBehaviour
    {
        public GameObject container;
        public TextMeshProUGUI txtTooltip;

        public void Show(string content, float duration)
        {
            StartCoroutine(ShowIE(content, duration));
        }

        private IEnumerator ShowIE(string content, float duration)
        {
            txtTooltip.text = content;
            
            yield return new WaitForSeconds(duration);
            
            KitPool.Destroy(gameObject);
        }
    }
}