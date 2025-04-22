using UnityEngine;

public class CameraRayCast : MonoBehaviour
{
    public RaycastHit hittedGameObject;
    public bool cursorOnLegalObject = false;

    private void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if(Physics.Raycast(ray, out hit)) {
            //Debug.Log("Current object --> " + hit.transform.name + " - " + hit.transform.tag);
            hittedGameObject = hit;
            cursorOnLegalObject = true;
        } else {
            cursorOnLegalObject = false;
        }
    }
}
