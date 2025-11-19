using UnityEngine;
using System;
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

        // gather children and primitives
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
                primitiveList.AddRange(childPrimitives);
            }
        }
    }

    /// <summary>
    /// Clears the SceneNode by deleting all child SceneNodes and NodePrimitives.
    /// This operation is only valid on the root SceneNode.
    /// </summary>
    public void ClearChildren()
    {
        if (!isRoot)
        {
            throw new InvalidOperationException($"{nameof(ClearChildren)} can only be called on a root SceneNode.");
        }

        foreach (var child in childrenList)
        {
            DestroyImmediate(child.gameObject);
        }

        childrenList.Clear();
        primitiveList.Clear();
    }

    // start the composite process from the root node
    // this is done on LateUpdate so scripts that modify transforms in Update() are accounted for
    private void LateUpdate()
    {
        if (isRoot)
        {
            var identity = Matrix4x4.identity;
            CompositeTransform(ref identity);
        }
    }

    // Computes the transform matrix for this node, then propagates this matrix to all children and primitives.
    protected void CompositeTransform(ref Matrix4x4 parentXform)
    {
        var orgT = Matrix4x4.Translate(NodeOrigin);
        var trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        
        CombinedTransform = parentXform * orgT * trs;

        // propagate to all children
        foreach (var child in childrenList)
            child.CompositeTransform(ref CombinedTransform);
        
        // disseminate to primitives
        foreach (var primitive in primitiveList)
            primitive.LoadShaderMatrix(ref CombinedTransform);
    }
}
