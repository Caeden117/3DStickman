using UnityEngine;

// NodePrimitive copied from examples but tweaked
[ExecuteInEditMode]
public class NodePrimitive: MonoBehaviour {
    public Color MyColor = new(0.1f, 0.1f, 0.2f, 1.0f);
    public Vector3 Pivot;

    // We use a material property block to avoid duplicating materials
    // for each primitive, which dodges memory leak issues in Unity Editor.
    // Theoretically more perfomrant too but the shader isnt set up for this.
    private MaterialPropertyBlock propertyBlock;
    private new Renderer renderer;

#if UNITY_EDITOR
    private void OnValidate() => Start();
#endif

    private void Start ()
    {
        propertyBlock = new MaterialPropertyBlock();
        renderer = GetComponent<Renderer>();
    }
  
	public void LoadShaderMatrix(ref Matrix4x4 nodeMatrix)
    {
        var p = Matrix4x4.TRS(Pivot, Quaternion.identity, Vector3.one);
        var invp = Matrix4x4.TRS(-Pivot, Quaternion.identity, Vector3.one);
        var trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        var m = nodeMatrix * p * trs * invp;

        propertyBlock.SetMatrix("MyTRSMatrix", m);
        propertyBlock.SetColor("MyColor", MyColor);
        renderer.SetPropertyBlock(propertyBlock);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Don't draw gizmos in play mode / editor mode with game running
        if (renderer != null) return;

        var oldColor = Gizmos.color;
        Gizmos.color = MyColor;
        Gizmos.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh, transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = oldColor;
    }
#endif
}