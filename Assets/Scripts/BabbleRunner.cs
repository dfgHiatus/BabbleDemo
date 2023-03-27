using Babble;
using System;
using UnityEngine;
using static Babble.BabbleExpressions;

public class BabbleRunner : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer Renderer;

    [SerializeField]
    private int DefaultPort = 9000;

    private TwoKeyDictionary<int, string, float> _mouthShape;
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
        _mouthShape = new TwoKeyDictionary<int, string, float>();

        for (int i = 0; i < ExpressionToAddress.Length; i++)
        {
            _mouthShape.Add(i, ExpressionToAddress[i], 0f); // Create a mapping of ints to our BabbleExpression addresses
        }
    }

    void Update()
    {
        if (_receiver.message == null) // We haven't connected yet
            return;

        try
        {
            switch (_receiver.message.Value.GetType())
            {
                // case Type decimalType when decimalType == typeof(decimal):
                // case Type doubleType when doubleType == typeof(double):
                case Type floatType when floatType == typeof(float):
                {
                    if (_mouthShape.ContainsKey2(_receiver.message.Address))
                        _mouthShape.SetByKey2(_receiver.message.Address, BitConverter.ToSingle(_receiver.message.Data, 0));

                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return;
        }

        for (int i = 0; i < Renderer.sharedMesh.blendShapeCount; i++)
        {
            // Does our mesh have an index whose shapekey name is also in our _mouthShape dictionary?
            // If so, update the weight
            var name = Renderer.sharedMesh.GetBlendShapeName(i);
            if (_mouthShape.TryGetByKey2(name, out float value))
                Renderer.SetBlendShapeWeight(i, value);
        }
    }

    private void OnApplicationQuit()
    {
        _receiver.Teardown();
    }
}
