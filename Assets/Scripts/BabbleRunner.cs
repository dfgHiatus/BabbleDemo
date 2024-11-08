using UnityEngine;


public class BabbleRunner : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer Renderer;

    [SerializeField]
    private int DefaultPort = 8888;

    
    private BabbleOSC _receiver;

    // Start is called before the first frame update
    void Start()
    {
        if (_receiver != null)
        {
            Debug.LogError("BabbleOSC connection already exists.");
            return;
        }

        _receiver = new BabbleOSC("127.0.0.1", DefaultPort);
    }

    void Update()
    {
        for (int i = 0; i < Renderer.sharedMesh.blendShapeCount; i++)
        {
            var name = Renderer.sharedMesh.GetBlendShapeName(i);
            if (_receiver.MouthShapes.TryGetValue(name, out float value))
                Renderer.SetBlendShapeWeight(i, value * 100); // Babble returns 0-1, scale this up
        }
    }

    private void OnApplicationQuit()
    {
        _receiver.Teardown();
    }
}
