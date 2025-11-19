using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Code largely taken from the example source code on GitHub, but I've made my own modifications
[ExecuteInEditMode]
public class SceneNode : MonoBehaviour {

    protected Matrix4x4 mCombinedParentXform;
    
    public Vector3 NodeOrigin = Vector3.zero;

    private List<NodePrimitive> primitiveList = new();
    private List<SceneNode> childrenList = new();
    private bool isRoot = false;

	// Use this for initialization
	protected void Start ()
    {
        mCombinedParentXform = Matrix4x4.identity;

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
            var childNode = child.GetComponent<SceneNode>();
            if (childNode != null)
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
            Matrix4x4 identity = Matrix4x4.identity;
            CompositeXform(ref identity);
        }
    }

    // This must be called _BEFORE_ each draw!!  @
    public void CompositeXform(ref Matrix4x4 parentXform)
    {
        Matrix4x4 orgT = Matrix4x4.Translate(NodeOrigin);
        Matrix4x4 trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        
        mCombinedParentXform = parentXform * orgT * trs;

        // propagate to all children
        foreach (SceneNode child in childrenList)
            child.CompositeXform(ref mCombinedParentXform);
        
        // disseminate to primitives
        foreach (NodePrimitive p in primitiveList)
            p.LoadShaderMatrix(ref mCombinedParentXform);

    }
}