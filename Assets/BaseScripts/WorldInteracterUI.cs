using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldInteracterUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    public bool isLockInsteadOfSell = true;
    public bool isMouseOver = false;
    private RectTransform rect;

    public IEnumerator RectPop()
    {
        rect = GetComponent<RectTransform>();
        rect.localScale = new Vector2(1.12f, 1.12f);
        while (rect.localScale.x >= 1.05)
        {
            yield return new WaitForSeconds(0.01f);
            rect.localScale = Vector2.Lerp(rect.localScale, new Vector2(1, 1), 0.5f);
        }
        rect.localScale = new Vector2(1f, 1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isMouseOver)
        {
            if (!isLockInsteadOfSell && !PalPlacementSystem.canPlace && PalPlacementSystem.palThatWantsToMove != null)
            {
                
            }
        }
    }
}
