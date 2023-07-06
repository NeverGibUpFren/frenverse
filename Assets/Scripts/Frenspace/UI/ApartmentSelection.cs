using UnityEngine;
using UnityEngine.EventSystems;

namespace Frenspace.UI
{

  public class ApartmentSelection : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler
  {

    public Transform scene;

    private bool held = false;

    private Transform xP;
    private Transform yP;

    public void OnPointerDown(PointerEventData eventData)
    {
      if (eventData.button == PointerEventData.InputButton.Left)
      {
        held = true;
        xP = scene.transform;
        yP = xP.GetChild(0).transform;
      }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      if (eventData.button == PointerEventData.InputButton.Left)
      {
        held = false;
      }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
      if (held)
      {
        xP.transform.Rotate(new Vector3(eventData.delta.y, 0, 0) * 0.6f);
        yP.transform.Rotate(new Vector3(0, -eventData.delta.x, 0) * 0.6f);
      }
    }
  }

}