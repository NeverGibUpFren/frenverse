using UnityEngine;

public class ChatInput : MonoBehaviour
{
  public GameObject input;

  public bool active = false;

  private string gatheredText = "";

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Return))
    {
      if (active)
      {
        EndInput();
      }
      else
      {
        BeginInput();
      }
      return;
    }

    if (active)
    {
      if (Input.GetKey(KeyCode.Backspace) && gatheredText.Length > 0)
      {
        gatheredText = gatheredText.Remove(gatheredText.Length - 1);
        UpdateText();
      }
      else
      {
        if (Input.anyKeyDown && Input.inputString != "")
        {
          gatheredText += Input.inputString;
          UpdateText();
        }
      }
    }
  }

  void BeginInput()
  {
    gatheredText = "";
    UpdateText();
    active = true;
    input.SetActive(true);
  }

  void EndInput()
  {
    active = false;
    input.SetActive(false);
  }

  void UpdateText()
  {
    input.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = gatheredText;
  }

}