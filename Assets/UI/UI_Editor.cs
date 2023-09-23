using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Editor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/UI_Editor")]
    public static void ShowExample()
    {
        UI_Editor wnd = GetWindow<UI_Editor>();
        wnd.titleContent = new GUIContent("UI_Editor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
