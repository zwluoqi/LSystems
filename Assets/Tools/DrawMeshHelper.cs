using UnityEngine;

namespace Tools
{
    public class DrawMeshHelper : MonoBehaviour
    {
        public bool gizmos;
        private void OnDrawGizmos()
        {
            if (!gizmos)
            {
                return;
            }
            var meshFilter = GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;
            var v = mesh.vertices;
            var n = mesh.normals;
            var t = mesh.tangents;
            for (int i = 0; i < v.Length; i++)
            {
                Gizmos.color = Color.blue;
                var position = this.transform.position;
                Gizmos.DrawLine(position+v[i],position+ v[i] + n[i]*0.3f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(position+v[i], position+v[i] + new Vector3(t[i].x,t[i].y,t[i].z)*0.3f);
            }
        }
    }
}