// copy from Empty4Raycast. https://blog.uwa4d.com/archives/fillrate.html
namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Rect Raycast 2D", 18)]
    public class RectRaycast2D : MaskableGraphic
    {
        protected RectRaycast2D()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
