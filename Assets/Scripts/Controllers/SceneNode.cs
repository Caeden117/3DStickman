using UnityEngine;
using System.Collections.Generic;

// Code largely taken from the example source code on GitHub, but I've made my own modifications
[ExecuteInEditMode]
public class SceneNode : MonoBehaviour {

    protected Matrix4x4 CombinedTransform;
    
    public Vector3 NodeOrigin = Vector3.zero;

    private readonly List<NodePrimitive> primitiveList = new();
    private readonly List<SceneNode> childrenList = new();
    private bool isRoot = false;

	// Use this for initialization
	private void Start ()
    {
        CombinedTransform = Matrix4x4.identity;

        // determine if this is a root node
        isRoot = transform.parent == null || !GetComponentInChildren<SceneNode>();

        // gather primitives attached to this node
        foreach (var primitive in GetComponents<NodePrimitive>())
        {
            primitiveList.Add(primitive);
        }

        foreach (Transform child in transform)
        {
            // gather scene nodes from children and assign them as such
            if (child.TryGetComponent<SceneNode>(out var childNode))
            {
                childrenList.Add(childNode);
                continue;
            }

            // gather any primitives attached to children without scene nodes
            var childPrimitives = child.GetComponents<NodePrimitive>();
            if (childPrimitives.Length > 0)
            {
                foreach (var primitive in childPrimitives)
                {
                    primitiveList.Add(primitive);
                }
            }
        }
    }

    // start the composite process from the root node
    // this is done on LateUpdate so scripts that modify transforms in Update() are accounted for
    private void LateUpdate()
    {
        if (isRoot)
        {
            var identity = Matrix4x4.identity;
            CompositeXform(ref identity);
        }
    }

    // This must be called _BEFORE_ each draw!!  @
    public void CompositeXform(ref Matrix4x4 parentXform)
    {
        var orgT = Matrix4x4.Translate(NodeOrigin);
        var trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        
        CombinedTransform = parentXform * orgT * trs;

        // propagate to all children
        foreach (var child in childrenList)
            child.CompositeXform(ref CombinedTransform);
        
        // disseminate to primitives
        foreach (var primitive in primitiveList)
            primitive.LoadShaderMatrix(ref CombinedTransform);
    }
}
