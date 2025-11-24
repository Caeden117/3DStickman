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
    private SceneNode parentNode = null;

	// Use this for initialization
	private void Start ()
    {
        CombinedTransform = Matrix4x4.identity;

        // gather primitives beneath this node
        foreach (Transform child in transform)
        {
            // Ignore children that are also scene nodes (they will gather their own primitives)
            if (child.TryGetComponent<SceneNode>(out var _))
            {
                continue;
            }

            // gather any primitives attached to children without scene nodes
            var childPrimitives = child.GetComponents<NodePrimitive>();
            if (childPrimitives.Length > 0)
            {
                primitiveList.AddRange(childPrimitives);
            }
        }

        // determine if this is a root node
        if (transform.parent == null)
        {
            isRoot = true;
            return;
        }

        parentNode = transform.parent.GetComponentInParent<SceneNode>();
        if (parentNode != null)
        {
            parentNode.RegisterChild(this);
        }
        else
        {
            isRoot = true;
        }
    }

    /// <summary>
    /// Returns the path of this SceneNode relative to the given root SceneNode.
    /// </summary>
    public string GetScenePathRelativeTo(SceneNode sceneTreeRoot)
    {
        if (sceneTreeRoot == null || sceneTreeRoot == this)
        {
            return gameObject.name;
        }

        // Use a stack to build the path from this node up to the root
        // then join the stack into a string
        var pathStack = new Stack<string>();
        var currentNode = this;
        while (currentNode != null && currentNode != sceneTreeRoot)
        {
            pathStack.Push(currentNode.gameObject.name);
            currentNode = currentNode.parentNode;
        }

        return currentNode != sceneTreeRoot
            ? null
            : string.Join("/", pathStack);
    }

    // Removes a child SceneNode from this node's list of children, should be called by the child node itself on destruction
    protected void RemoveChild(SceneNode sceneNode) => childrenList.Remove(sceneNode);

    // Adds a child SceneNode to this node's list of children, should be called by the child node itself on initialization
    protected void RegisterChild(SceneNode sceneNode) => childrenList.Add(sceneNode);

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

    private void OnDestroy()
    {
        if (parentNode != null)
        {
            parentNode.RemoveChild(this);
        }
    }
}
