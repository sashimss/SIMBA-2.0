using TMPro;
using UnityEngine;

public class ActionPrompt : MonoBehaviour
{
    public static ActionPrompt Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    public TMP_Text prompt;
    private void Start()
    {
        prompt = GetComponent<TMP_Text>();
    }

    public void SetActionPrompt(string s)
    {
        prompt.text = s;
    }

    public void ClearPrompt()
    {
        prompt.text = "";
    }
}
